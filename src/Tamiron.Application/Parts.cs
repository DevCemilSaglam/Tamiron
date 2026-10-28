using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Application.ServiceRecords;
using Tamiron.Domain;

namespace Tamiron.Application.Parts;

// H6 · Yedek Parça (K-091, TASARIM-H-YEDEK-PARCA). Kiracı süzgecinin dışına çıkan İKİ okuma burada:
//  1. Parçacı: tamirhanelerin KENDİ NUMARASINA sorduğu sorular (Supplier.Phone = parçacının giriş telefonu).
//  2. Tamirhane: KENDİ sorularına parçacının yazdığı cevaplar (cevabı yazan dükkânın numarası = sorulan numara).
// Süzgeç kapatıldığında kiracı da kapanır — her koşul AÇIKÇA yazılır (yalıtım testi). Yazma her zaman kendi
// kiracısına: altyapı başka dükkânın kiracısına yazmayı yasaklar.

public sealed record InboxAnswer(Guid Id, Guid LineId, decimal? Price, string? Brand, string? Code, string? Note, DateTime AnsweredAt);
public sealed record InboxLine(Guid LineId, string Name, IReadOnlyList<InboxAnswer> Answers);
/// <summary>Parçacının gördüğü soru: dükkân, araç kimliği (uyum — T-14), parçalar. Müşteri bilgisi YOK.</summary>
public sealed record InboxItem(Guid RequestId, string ShopName, string? ShopPhone, string PlateNo, string? Brand, string? Model,
                               string? Year, string? Engine, string? Vin, DateTime SentAt, IReadOnlyList<InboxLine> Lines);
public sealed record Inbox(IReadOnlyList<InboxItem> Items);

public sealed record GetInbox : IRequest<Inbox>;
public sealed record AnswerLine(Guid LineId, decimal? Price, string? Brand, string? Code, string? Note);
public sealed record AnswerBody(IReadOnlyList<AnswerLine> Lines);
public sealed record AnswerRequest(Guid RequestId, AnswerBody Body) : IRequest<Inbox>;

/// <summary>Tamirhanenin gördüğü parçacı cevabı; <paramref name="Used"/> = usta "Bunu kullan" dedi.</summary>
public sealed record SupplierAnswerDto(Guid Id, Guid LineId, decimal? Price, string? Brand, string? Code, string? Note, DateTime AnsweredAt, bool Used);
public sealed record UseBody(decimal? CustomerPrice, bool OnCredit = false);
public sealed record UseSupplierAnswer(Guid AnswerId, UseBody Body) : IRequest<ServiceRecordDto>;

internal static class CrossTenant
{
    /// <summary>Parçacıya (bu numaraya) sorulmuş, silinmemiş kayıtlara ait sorular.</summary>
    public static async Task<List<PriceRequest>> RequestsTo(ITamironDb db, string phone, Guid? only, CancellationToken ct)
    {
        var sups = await db.Suppliers.IgnoreQueryFilters()
            .Where(s => s.Phone == phone && s.DateTimeDeleted == null).Select(s => new { s.Id, s.TenantId }).ToListAsync(ct);
        var ids = sups.Select(s => s.Id).ToList();
        var reqs = await db.PriceRequests.IgnoreQueryFilters()
            .Where(r => ids.Contains(r.SupplierId) && r.DateTimeDeleted == null && (only == null || r.Id == only)
                     && db.ServiceRecords.IgnoreQueryFilters().Any(x => x.Id == r.ServiceRecordId && x.TenantId == r.TenantId && x.DateTimeDeleted == null))
            .ToListAsync(ct);
        // Sorunun kiracısı parçacı kaydının kiracısıyla AYNI olmalı (başka dükkânın kimliği karışmasın).
        return reqs.Where(r => sups.Any(s => s.Id == r.SupplierId && s.TenantId == r.TenantId)).ToList();
    }

    /// <summary>Kiracının (dükkânın) giriş telefonları — cevabı yazanın, sorunun sorulduğu numara olduğunu doğrulamak için.</summary>
    public static async Task<Dictionary<Guid, HashSet<string>>> PhonesOf(ITamironDb db, IEnumerable<Guid> tenants, CancellationToken ct)
    {
        var ids = tenants.Distinct().ToList();
        return (await db.Users.IgnoreQueryFilters().Where(u => ids.Contains(u.TenantId) && u.DateTimeDeleted == null)
                    .Select(u => new { u.TenantId, u.Phone }).ToListAsync(ct))
            .GroupBy(x => x.TenantId).ToDictionary(g => g.Key, g => g.Select(x => x.Phone).ToHashSet());
    }
}

internal sealed class PartsHandlers(ITamironDb db, ITenantContext tenant, ICurrentSession session, IClock clock) :
    IRequestHandler<GetInbox, Inbox>,
    IRequestHandler<AnswerRequest, Inbox>,
    IRequestHandler<UseSupplierAnswer, ServiceRecordDto>
{
    private Task<string> MyPhone(CancellationToken ct) => db.Users.Where(u => u.Id == session.UserId).Select(u => u.Phone).FirstAsync(ct);

    public async Task<Inbox> Handle(GetInbox q, CancellationToken ct)
    {
        var reqs = await CrossTenant.RequestsTo(db, await MyPhone(ct), null, ct);
        var recIds = reqs.Select(r => r.ServiceRecordId).ToList();
        var recs = await db.ServiceRecords.IgnoreQueryFilters().AsNoTracking().Where(r => recIds.Contains(r.Id))
            .Select(r => new { r.Id, r.VehicleId, r.TenantId }).ToDictionaryAsync(r => r.Id, ct);
        var vIds = recs.Values.Select(r => r.VehicleId).ToList();
        var vehicles = await db.Vehicles.IgnoreQueryFilters().AsNoTracking().Where(v => vIds.Contains(v.Id)).ToDictionaryAsync(v => v.Id, ct);
        var shopIds = reqs.Select(r => r.TenantId).Distinct().ToList();
        var shops = await db.Shops.IgnoreQueryFilters().AsNoTracking().Where(s => shopIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, ct);
        var lineIds = reqs.SelectMany(r => r.Lines.Select(l => l.ServiceLineId)).ToList();
        var lines = await db.ServiceLines.IgnoreQueryFilters().AsNoTracking()
            .Where(l => lineIds.Contains(l.Id) && l.DateTimeDeleted == null).ToDictionaryAsync(l => l.Id, ct);
        var reqIds = reqs.Select(r => r.Id).ToList();
        var mine = await db.SupplierAnswers.AsNoTracking().Where(a => reqIds.Contains(a.PriceRequestId)).OrderBy(a => a.AnsweredAt).ToListAsync(ct);

        var items = reqs.Select(r =>
        {
            var v = vehicles[recs[r.ServiceRecordId].VehicleId];
            var s = shops[r.TenantId];
            return new InboxItem(r.Id, s.Name, s.Phone, v.PlateNo, v.Brand, v.Model, v.Year, v.Engine, v.Vin, r.SentAt,
                r.Lines.Where(l => lines.ContainsKey(l.ServiceLineId)).Select(l => lines[l.ServiceLineId])
                    .OrderBy(l => l.Position).ThenBy(l => l.DateTimeCreated)
                    .Select(l => new InboxLine(l.Id, l.Name, mine.Where(a => a.PriceRequestId == r.Id && a.ServiceLineId == l.Id)
                        .Select(a => new InboxAnswer(a.Id, a.ServiceLineId, a.Price, a.Brand, a.Code, a.Note, a.AnsweredAt)).ToList())).ToList());
        })
        // Cevapsız sorular üstte, sonra en yeni.
        .OrderBy(i => i.Lines.All(l => l.Answers.Count > 0) ? 1 : 0).ThenByDescending(i => i.SentAt).ToList();
        return new Inbox(items);
    }

    public async Task<Inbox> Handle(AnswerRequest c, CancellationToken ct)
    {
        var req = (await CrossTenant.RequestsTo(db, await MyPhone(ct), c.RequestId, ct)).SingleOrDefault()
                  ?? throw new NotFoundException("Soru");
        var asked = req.Lines.Select(l => l.ServiceLineId).ToHashSet();
        var now = clock.UtcNow;
        foreach (var l in c.Body.Lines ?? [])
        {
            // Yanlış satıra fiyat düşmesin (D2): yalnız bu soruda sorulan satırlar.
            if (!asked.Contains(l.LineId))
                throw new DomainException("answer.line_not_asked", "Bu parça sana sorulmamış.");
            db.SupplierAnswers.Add(SupplierAnswer.Create(tenant.TenantId, req.Id, l.LineId, l.Price, l.Brand, l.Code, l.Note, now));
        }
        await db.SaveChangesAsync(ct);
        return await Handle(new GetInbox(), ct);
    }

    /// <summary>
    /// "Bunu kullan" (Kol B): parçacının cevabı tamirhanenin kiracısına PriceReply olarak kopyalanır ve satıra
    /// işlenir — bugünkü "Kayda yaz"ın yazmadan hâli. Soru bu dükkânın, cevabı yazan sorulan numara olmalı.
    /// </summary>
    public async Task<ServiceRecordDto> Handle(UseSupplierAnswer c, CancellationToken ct)
    {
        var a = await db.SupplierAnswers.IgnoreQueryFilters().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == c.AnswerId && x.DateTimeDeleted == null, ct) ?? throw new NotFoundException("Cevap");
        var req = await db.PriceRequests.FirstOrDefaultAsync(r => r.Id == a.PriceRequestId, ct) ?? throw new NotFoundException("Cevap");
        var supPhone = await db.Suppliers.IgnoreQueryFilters().Where(s => s.Id == req.SupplierId && s.TenantId == tenant.TenantId)
                           .Select(s => s.Phone).FirstAsync(ct);
        var phones = await CrossTenant.PhonesOf(db, [a.TenantId], ct);
        if (!phones.TryGetValue(a.TenantId, out var p) || !p.Contains(supPhone)) throw new NotFoundException("Cevap");
        var line = await db.ServiceLines.FirstOrDefaultAsync(l => l.Id == a.ServiceLineId && l.ServiceRecordId == req.ServiceRecordId, ct)
                   ?? throw new NotFoundException("Satır");

        var reply = PriceReply.FromAnswer(a, clock.UtcNow);
        line.ApplyReply(reply, c.Body.CustomerPrice, req.SupplierId, c.Body.OnCredit);
        db.PriceReplies.Add(reply);
        await db.SaveChangesAsync(ct);
        return await Map.LoadDto(db, req.ServiceRecordId, ct);
    }
}
