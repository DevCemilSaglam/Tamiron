using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Domain;

namespace Tamiron.Application.Auth;

// K-074 · SMS koduyla giriş; oturum yalnız çıkışla biter.

public sealed record RequestLoginCode(string Phone) : IRequest;
public sealed record VerifyLoginCode(string Phone, string Code, string? DeviceName = null) : IRequest<LoginResult>;
public sealed record LoginResult(string Token, string ShopName, string? UserName);

public sealed record Logout : IRequest;
/// <summary>"Bütün cihazlardan çık" — kaybolan telefonun karşılığı (K-074.2).</summary>
public sealed record LogoutAll : IRequest;
public sealed record ListSessions : IRequest<List<SessionInfo>>;
public sealed record SessionInfo(Guid Id, string? DeviceName, DateTime CreatedAt, DateTime LastSeenAt, bool Current);

internal sealed class AuthHandlers(ITamironDb db, IClock clock, ISmsSender sms, ICurrentSession current) :
    IRequestHandler<RequestLoginCode>,
    IRequestHandler<VerifyLoginCode, LoginResult>,
    IRequestHandler<Logout>,
    IRequestHandler<LogoutAll>,
    IRequestHandler<ListSessions, List<SessionInfo>>
{
    private static string Phone(string raw)
        => VehicleFields.Phone(raw) ?? throw new DomainException("auth.phone_required", "Telefonunu yaz.");

    public async Task Handle(RequestLoginCode c, CancellationToken ct)
    {
        var phone = Phone(c.Phone);
        // Kayıtsız numaraya SMS gitmez (para). Yeni dükkânın kaydı henüz kararlaşmadı (K-074.5).
        if (!await db.Users.AnyAsync(u => u.Phone == phone, ct))
            throw new DomainException("auth.unknown_phone", "Bu numarayla kayıtlı dükkân yok.");

        var now = clock.UtcNow;
        var since = now - LoginCode.Window;
        var recent = await db.LoginCodes.Where(x => x.Phone == phone && x.DateTimeCreated > since)
                                        .Select(x => x.DateTimeCreated).ToListAsync(ct);
        if (!LoginCode.CanIssue(recent, now))
            throw new DomainException("auth.too_many_codes", "Çok sık kod istendi. Birkaç dakika sonra tekrar dene.");

        var (lc, code) = LoginCode.Issue(phone, now);
        db.LoginCodes.Add(lc);
        await db.SaveChangesAsync(ct);   // önce kayıt: gönderim başarısız olsa da istek sayılır (yağdırma sınırı aşılamasın)
        await sms.SendAsync(phone, $"Tamiron giriş kodun: {code}. Kimseyle paylaşma.", ct);
    }

    public async Task<LoginResult> Handle(VerifyLoginCode c, CancellationToken ct)
    {
        var phone = Phone(c.Phone);
        var now = clock.UtcNow;
        // Son verilen kod geçerlidir; eskileri yeni kodla anlamını yitirir.
        var lc = await db.LoginCodes.Where(x => x.Phone == phone).OrderByDescending(x => x.DateTimeCreated).FirstOrDefaultAsync(ct)
                 ?? throw new DomainException("auth.no_code", "Önce kod iste.");
        try
        {
            lc.Verify(c.Code, now);
        }
        finally
        {
            // Yanlış deneme de YAZILIR: yazılmasaydı 5 deneme sınırı her istekte sıfırlanırdı.
            await db.SaveChangesAsync(ct);
        }

        var user = await db.Users.FirstAsync(u => u.Phone == phone, ct);
        var (session, token) = Session.Open(user.TenantId, user.Id, c.DeviceName, now);
        db.Sessions.Add(session);
        await db.SaveChangesAsync(ct);
        var shop = await db.Shops.IgnoreQueryFilters().Where(s => s.Id == user.TenantId).Select(s => s.Name).FirstAsync(ct);
        return new LoginResult(token, shop, user.Name);
    }

    public async Task Handle(Logout c, CancellationToken ct)
    {
        var s = await db.Sessions.FirstAsync(x => x.Id == current.SessionId, ct);
        s.Revoke(clock.UtcNow);
        await db.SaveChangesAsync(ct);
    }

    public async Task Handle(LogoutAll c, CancellationToken ct)
    {
        var now = clock.UtcNow;
        foreach (var s in await db.Sessions.Where(x => x.UserId == current.UserId && x.RevokedAt == null).ToListAsync(ct))
            s.Revoke(now);
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<SessionInfo>> Handle(ListSessions q, CancellationToken ct)
        => (await db.Sessions.AsNoTracking().Where(x => x.UserId == current.UserId && x.RevokedAt == null)
                .OrderByDescending(x => x.LastSeenAt).ToListAsync(ct))
           .Select(s => new SessionInfo(s.Id, s.DeviceName, s.DateTimeCreated, s.LastSeenAt, s.Id == current.SessionId)).ToList();
}
