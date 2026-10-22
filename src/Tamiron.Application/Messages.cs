using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Domain;

namespace Tamiron.Application.Messages;

// B11 · Mesaj arşivi ve toplu alıcı süzgeci (prototip 85-mesaj, 97-mesajlar).
// ⚠️ HİÇBİR ŞEY GÖNDERİLMEZ: WhatsApp/SMS dış servis ve para (BACKEND-KUYRUGU
// "dokunulmayacaklar"). Bu uçlar ne gönderildiğini KAYDEDER; gönderimi bugün
// usta kendi telefonundan yapıyor (WHATSAPP.md A yolu).

public sealed record MessageDto(Guid Id, Guid VehicleId, Guid? ServiceRecordId, DateTime SentAt, string RecipientName,
                                string RecipientPhone, MessageChannel Channel, string Template, bool IsBulk, string Body);

public sealed record RecordMessage(Guid VehicleId, MessageChannel Channel, string? Template, string Body,
                                   Guid? ServiceRecordId = null, MessageTargetKind? Target = null) : IRequest<MessageDto>;

public sealed record ListMessages(Guid? VehicleId = null) : IRequest<List<MessageDto>>;

public sealed record BulkRecipient(string Name, string Phone, string PlateNo);
public sealed record BulkPreview(IReadOnlyList<BulkRecipient> Recipients, IReadOnlyList<BulkRecipient> WithoutConsent);
public sealed record GetBulkPreview : IRequest<BulkPreview>;

/// <summary>
/// Toplu mesaj: YALNIZ ticari ileti izni olan müşterilere (K-071.3). Her alıcıya
/// kendi hitabı + metin + imza; her alıcı kendi kaydını alır.
/// </summary>
public sealed record SendBulk(MessageChannel Channel, string? Template, string Body) : IRequest<BulkResult>;
public sealed record BulkResult(int Recorded, int SkippedWithoutConsent);

internal sealed class MessageHandlers(ITamironDb db, ITenantContext tenant) :
    IRequestHandler<RecordMessage, MessageDto>,
    IRequestHandler<ListMessages, List<MessageDto>>,
    IRequestHandler<GetBulkPreview, BulkPreview>,
    IRequestHandler<SendBulk, BulkResult>
{
    private static MessageDto Dto(Message m) => new(m.Id, m.VehicleId, m.ServiceRecordId, m.DateTimeCreated, m.RecipientName,
                                                    m.RecipientPhone, m.Channel, m.Template, m.IsBulk, m.Body);

    public async Task<MessageDto> Handle(RecordMessage c, CancellationToken ct)
    {
        var v = await db.Vehicles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == c.VehicleId, ct) ?? throw new NotFoundException("Araç");
        if (c.ServiceRecordId is { } rid && !await db.ServiceRecords.AnyAsync(r => r.Id == rid && r.VehicleId == v.Id, ct))
            throw new NotFoundException("Servis kaydı");
        // Tek mesajın metni ustanın onayladığı hâliyle gelir (hitap dahil — mesaj ekranında düzenlenebilir).
        var m = Message.Record(tenant.TenantId, v, v.DefaultTarget(c.Target)!, c.Channel, c.Template, c.Body, isBulk: false, c.ServiceRecordId);
        db.Messages.Add(m);
        // "Hatırlatma" gittiyse (arama dahil) aracın açık hatırlatmaları kapanır (B12, prototip 85-mesaj).
        if (m.Template == "hatirlatma")
            foreach (var r in await db.ServiceRecords.Where(r => r.VehicleId == v.Id && r.ReminderDate != null && !r.ReminderSent).ToListAsync(ct))
                r.MarkReminderSent();
        await db.SaveChangesAsync(ct);
        return Dto(m);
    }

    /// <summary>En yeni üstte — liste "ne gönderdim" sorusunu cevaplıyor, o soru hep son gönderilenle başlar.</summary>
    public async Task<List<MessageDto>> Handle(ListMessages q, CancellationToken ct)
    {
        var list = await db.Messages.AsNoTracking()
            .Where(m => q.VehicleId == null || m.VehicleId == q.VehicleId)
            .OrderByDescending(m => m.DateTimeCreated).ToListAsync(ct);
        return list.Select(Dto).ToList();
    }

    private async Task<(List<(Customer c, Vehicle v, MessageTarget t)> candidates, Shop shop)> Candidates(CancellationToken ct)
    {
        var vehicles = await db.Vehicles.AsNoTracking().OrderBy(v => v.DateTimeCreated).ToListAsync(ct);
        var inShop = (await db.ServiceRecords.AsNoTracking().Where(r => r.DeliveredAt == null).Select(r => r.VehicleId).ToListAsync(ct)).ToHashSet();
        var shop = await db.Shops.AsNoTracking().FirstAsync(s => s.Id == tenant.TenantId, ct);
        // Telefonu olan her müşteri, bir kez; numara satırda görünen aracın varsayılan hedefi
        // (aracı getiren kişi, firma santrali değil · A-M2).
        var list = Domain.Customers.From(vehicles)
            .Select(c => { var v = c.DisplayVehicle(x => inShop.Contains(x.Id)); return (c, v, t: v.DefaultTarget()!); })
            .Where(x => !string.IsNullOrWhiteSpace(x.t.Phone)).ToList();
        return (list, shop);
    }

    public async Task<BulkPreview> Handle(GetBulkPreview q, CancellationToken ct)
    {
        var (cands, _) = await Candidates(ct);
        BulkRecipient R((Customer c, Vehicle v, MessageTarget t) x) => new(x.t.Name, x.t.Phone!, x.v.PlateNo);
        // Gitmeyenler de SAYILIR: usta "herkese" dedi; kimin atlandığını görmezse herkese gitti sanır.
        return new BulkPreview(cands.Where(x => x.c.ConsentAt is not null).Select(R).ToList(),
                               cands.Where(x => x.c.ConsentAt is null).Select(R).ToList());
    }

    public async Task<BulkResult> Handle(SendBulk c, CancellationToken ct)
    {
        var (cands, shop) = await Candidates(ct);
        var withConsent = cands.Where(x => x.c.ConsentAt is not null).ToList();
        foreach (var (_, v, t) in withConsent)
            db.Messages.Add(Message.Record(tenant.TenantId, v, t, c.Channel, c.Template, MessageText.Compose(t, c.Body, shop), isBulk: true));
        await db.SaveChangesAsync(ct);
        return new BulkResult(withConsent.Count, cands.Count - withConsent.Count);
    }
}
