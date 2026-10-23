using Microsoft.EntityFrameworkCore;
using Tamiron.Application;
using Tamiron.Domain;
using Tamiron.Infrastructure;

namespace Tamiron.Api;

/// <summary>
/// İsteğin bağlamı: hangi dükkân, hangi kişi, hangi cihaz. OTURUMDAN doldurulur
/// (K-074.4). Ayarlanabilir olması yalnız oturum ara katmanı, tohum ve testler için.
/// </summary>
public sealed class RequestContext : ITenantContext, ICurrentSession
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid SessionId { get; set; }
}

/// <summary>
/// "Authorization: Bearer &lt;anahtar&gt;" → oturum → dükkân. Giriş uçları dışında her
/// /api isteği oturum ister — bir de müşterinin özet sayfası (/api/o/{kod}, K-023). Eski X-Tenant-Id başlığı (K-073.6) KALDIRILDI: başlıkla
/// gelen dükkân, herkesin her dükkânı okuyabilmesi demekti.
/// </summary>
public sealed class SessionMiddleware(RequestDelegate next)
{
    private static readonly PathString[] Open = ["/api/auth/code", "/api/auth/verify", "/api/auth/signup/code", "/api/auth/signup"];

    public async Task InvokeAsync(HttpContext ctx, RequestContext rc, TamironDbContext db, IClock clock)
    {
        var path = ctx.Request.Path;
        if (!path.StartsWithSegments("/api") || Open.Any(p => path.Equals(p, StringComparison.OrdinalIgnoreCase))
            || path.StartsWithSegments("/api/o", StringComparison.OrdinalIgnoreCase))
        {
            await next(ctx);
            return;
        }
        var header = ctx.Request.Headers.Authorization.ToString();
        var token = header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? header[7..].Trim() : "";
        var session = token.Length == 0 ? null
            : await db.Sessions.FirstOrDefaultAsync(s => s.TokenHash == Session.HashToken(token) && s.RevokedAt == null);
        if (session is null)
        {
            await Results.Problem(statusCode: 401, title: "Oturum açık değil. Telefonunla giriş yap.").ExecuteAsync(ctx);
            return;
        }
        rc.TenantId = session.TenantId;
        rc.UserId = session.UserId;
        rc.SessionId = session.Id;
        if (session.Touch(clock.UtcNow)) await db.SaveChangesAsync();
        await next(ctx);
    }
}

/// <summary>
/// Kural ihlali 422 + makine kodu + Türkçe mesaj; bulunamadı 404.
/// Başka dükkânın kaydı da 404'tür: varlığı bile sızmasın. SMS sağlayıcısı yoksa 503.
/// </summary>
public sealed class ErrorMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try { await next(ctx); }
        catch (DomainException e)
        {
            await Results.Problem(statusCode: 422, title: e.Message,
                extensions: new Dictionary<string, object?> { ["code"] = e.Code }).ExecuteAsync(ctx);
        }
        catch (NotFoundException e)
        {
            await Results.Problem(statusCode: 404, title: e.Message).ExecuteAsync(ctx);
        }
        catch (SmsNotConfiguredException)
        {
            await Results.Problem(statusCode: 503, title: "SMS şu an gönderilemiyor. Dükkânın SMS hizmeti bağlanmadı.").ExecuteAsync(ctx);
        }
        // C7 · Okunamayan istek (bozuk JSON, yanlış tür, bilinmeyen seçenek). ASP.NET varsayılanı
        // GÖVDESİZ 400 dönüyordu — istemci ustaya bir şey gösteremezdi (ölçüldü, 22.10).
        catch (BadHttpRequestException)
        {
            await Results.Problem(statusCode: 400, title: "İstek okunamadı. Uygulamayı güncelleyip tekrar dene.",
                extensions: new Dictionary<string, object?> { ["code"] = "request.invalid" }).ExecuteAsync(ctx);
        }
        // C7 · Aynı kaydı iki cihaz değiştirdi (RowVersion). Telefon ve tablet artık aynı anda
        // açık (K-074); sessizce son yazan kazanırsa ötekinin yazdığı kaybolurdu. Önceden 500'dü.
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        {
            await Results.Problem(statusCode: 409, title: "Bu kayıt başka bir cihazda değişti. Sayfayı yenileyip tekrar dene.",
                extensions: new Dictionary<string, object?> { ["code"] = "record.changed" }).ExecuteAsync(ctx);
        }
    }
}
