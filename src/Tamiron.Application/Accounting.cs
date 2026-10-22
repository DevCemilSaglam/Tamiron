using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Domain;

namespace Tamiron.Application.Accounting;

// B9 · Alacak listesi ve muhasebe CSV'si (prototip 95-muhasebe, muhasebeCsv).

/// <param name="Description">Şikâyet; boşsa tür adı (prototip 60-arac: <c>k.sikayet || TUR_AD[k.tur]</c>).</param>
public sealed record ReceivableItem(Guid RecordId, Guid VehicleId, string PlateNo, string? OwnerName, string Description,
                                    DateOnly? DeliveredAt, decimal Total);
public sealed record ReceivableList(decimal Total, IReadOnlyList<ReceivableItem> Items);

/// <summary>
/// Alacak bir BAKİYE, hareket değil: döneme tabi DEĞİL (Cemil, 05.09) — geçen aydan
/// kalan alacak bu ay da alacaktır. Ustanın sorusu "KİMDEN alacağım": sahip adı önde.
/// <paramref name="VehicleId"/> verilirse araç ekranının "Ödeme durumu" (C4): AYNI liste,
/// araca süzülmüş — iki ayrı hesap olsaydı biri diğerini yalanlardı (prototip notu).
/// </summary>
public sealed record ListReceivables(Guid? VehicleId = null) : IRequest<ReceivableList>;

public sealed record CsvFile(string FileName, string Content);

/// <summary>
/// TEK DÜZ TABLO: "Tür" sütunuyla Excel'de süzülür ve toplanır (bölümlü dosyada
/// başlıklar ara satır olur, süzgeç bozulur). Tahsilat ve gider seçili döneme,
/// alacak BUGÜNE (ekranla aynı kural).
/// </summary>
public sealed record ExportCashCsv(PeriodKind Period) : IRequest<CsvFile>;

public sealed record JobProfitItem(Guid RecordId, string PlateNo, ServiceType Type, string TypeLabel, string? Brand,
                                   DateOnly DeliveredAt, decimal Received, decimal PartsCost, decimal Outside,
                                   int UnknownCount, decimal Difference);

/// <summary>
/// İş başına fark (C1). Dönem TESLİM gününe göre (prototip donemKayitlari), en yeni üstte.
/// <see cref="AskPartCost"/>: ekran bilinmeyen alış uyarısının çözümünü buna göre söyler
/// (sorulmuyorsa ayarlara götürür — K-021).
/// </summary>
public sealed record JobProfitList(PeriodKind Period, DateOnly Start, DateOnly End, decimal Received, decimal PartsCost,
                                   decimal Outside, int UnknownCount, decimal Difference, bool AskPartCost,
                                   IReadOnlyList<JobProfitItem> Items);

public sealed record ListJobProfits(PeriodKind Period) : IRequest<JobProfitList>;

public sealed record PartPurchaseItem(Guid RecordId, Guid LineId, string PlateNo, DateOnly DeliveredAt, string Name, decimal Cost);
public sealed record PartPurchaseList(PeriodKind Period, DateOnly Start, DateOnly End, decimal Total, IReadOnlyList<PartPurchaseItem> Items);

/// <summary>
/// Parça alışları (C2) — dükkândan parçaya çıkan para. Yalnız TESLİM edilmiş işler:
/// içerideki araçta parça henüz alınmamış olabilir. Dönem teslim gününe göre, en yeni üstte.
/// </summary>
public sealed record ListPartPurchases(PeriodKind Period) : IRequest<PartPurchaseList>;

internal sealed class AccountingHandlers(ITamironDb db, IClock clock, ITenantContext tenant) :
    IRequestHandler<ListReceivables, ReceivableList>,
    IRequestHandler<ExportCashCsv, CsvFile>,
    IRequestHandler<ListJobProfits, JobProfitList>,
    IRequestHandler<ListPartPurchases, PartPurchaseList>
{
    public async Task<PartPurchaseList> Handle(ListPartPurchases q, CancellationToken ct)
    {
        var p = Period.Of(q.Period, clock.Today);
        var (delivered, vehicles) = await Load(ct);
        var items = delivered.Where(r => p.Contains(r.DeliveredAt!.Value))
            .OrderByDescending(r => r.DeliveredAt).ThenByDescending(r => r.DateTimeCreated)
            .SelectMany(r => r.Lines.Where(l => l.IsShopPurchase).OrderBy(l => l.Position)
                .Select(l => new PartPurchaseItem(r.Id, l.Id, vehicles.TryGetValue(r.VehicleId, out var v) ? v.PlateNo : "",
                                                  r.DeliveredAt!.Value, l.Name, l.Cost!.Value)))
            .ToList();
        return new PartPurchaseList(q.Period, p.Start, p.End, items.Sum(i => i.Cost), items);
    }

    private async Task<(List<ServiceRecord> delivered, Dictionary<Guid, Vehicle> vehicles)> Load(CancellationToken ct)
    {
        var delivered = await db.ServiceRecords.AsNoTracking().Include(r => r.Lines)
            .Where(r => r.DeliveredAt != null).ToListAsync(ct);
        var vehicles = await db.Vehicles.AsNoTracking().ToDictionaryAsync(v => v.Id, ct);
        return (delivered, vehicles);
    }

    public async Task<ReceivableList> Handle(ListReceivables q, CancellationToken ct)
    {
        var (delivered, vehicles) = await Load(ct);
        var items = delivered.Where(r => r.PaymentMethod == PaymentMethod.Unpaid && (q.VehicleId is null || r.VehicleId == q.VehicleId))
            .OrderByDescending(r => r.DeliveredAt).ThenByDescending(r => r.DateTimeCreated)
            .Select(r =>
            {
                vehicles.TryGetValue(r.VehicleId, out var v);
                return new ReceivableItem(r.Id, r.VehicleId, v?.PlateNo ?? "", v?.OwnerName,
                    string.IsNullOrWhiteSpace(r.Complaint) ? ServiceTypeGuess.Label(r.Type) : r.Complaint,
                    r.DeliveredAt, r.Total);
            }).ToList();
        return new ReceivableList(items.Sum(i => i.Total), items);
    }

    public async Task<JobProfitList> Handle(ListJobProfits q, CancellationToken ct)
    {
        var p = Period.Of(q.Period, clock.Today);
        var (delivered, vehicles) = await Load(ct);
        var items = delivered.Where(r => p.Contains(r.DeliveredAt!.Value))
            .OrderByDescending(r => r.DeliveredAt).ThenByDescending(r => r.DateTimeCreated)
            .Select(r =>
            {
                var jp = JobProfit.Of(r);
                vehicles.TryGetValue(r.VehicleId, out var v);
                return new JobProfitItem(r.Id, v?.PlateNo ?? "", r.Type, ServiceTypeGuess.Label(r.Type), v?.Brand,
                    r.DeliveredAt!.Value, jp.Received, jp.PartsCost, jp.Outside, jp.UnknownCount, jp.Difference);
            }).ToList();
        var ask = await db.Shops.AsNoTracking().Where(s => s.Id == tenant.TenantId).Select(s => s.AskPartCost).FirstOrDefaultAsync(ct);
        return new JobProfitList(q.Period, p.Start, p.End, items.Sum(i => i.Received), items.Sum(i => i.PartsCost),
            items.Sum(i => i.Outside), items.Sum(i => i.UnknownCount), items.Sum(i => i.Difference), ask, items);
    }

    public async Task<CsvFile> Handle(ExportCashCsv q, CancellationToken ct)
    {
        var p = Period.Of(q.Period, clock.Today);
        var (delivered, vehicles) = await Load(ct);
        string Plate(ServiceRecord r) => vehicles.TryGetValue(r.VehicleId, out var v) ? v.PlateNo : "";
        string Owner(ServiceRecord r) => vehicles.TryGetValue(r.VehicleId, out var v) ? v.OwnerName ?? "" : "";
        string What(ServiceRecord r) => string.IsNullOrWhiteSpace(r.Complaint) ? "Servis kaydı" : r.Complaint;

        var rows = new List<object?[]> { new object?[] { "Tür", "Tarih", "Açıklama", "Plaka", "Müşteri", "Ödeme", "Tutar" } };

        // Tahsilat: tahsil GÜNÜNE göre (K-071.1), tarih sırasıyla — kayıt sırası açılış sırasıdır, ödeme sırası değil.
        rows.AddRange(delivered.Where(r => r.CollectedOn is { } d && p.Contains(d))
            .OrderBy(r => r.CollectedOn).ThenBy(r => r.DateTimeCreated)
            .Select(r => new object?[] { "Tahsilat", r.CollectedOn, What(r), Plate(r), Owner(r), Csv.PaymentLabel(r.PaymentMethod), r.Total }));

        rows.AddRange((await db.Expenses.AsNoTracking().Where(e => e.Date >= p.Start && e.Date <= p.End)
                .OrderBy(e => e.Date).ThenBy(e => e.DateTimeCreated).ToListAsync(ct))
            .Select(e => new object?[] { "Gider", e.Date, e.Description, "", "", "", e.Amount }));

        rows.AddRange(delivered.Where(r => r.PaymentMethod == PaymentMethod.Unpaid)
            .OrderBy(r => r.DeliveredAt).ThenBy(r => r.DateTimeCreated)
            .Select(r => new object?[] { "Alacak", r.DeliveredAt, What(r), Plate(r), Owner(r), Csv.PaymentLabel(r.PaymentMethod), r.Total }));

        var name = $"tamiron-muhasebe-{q.Period.ToString().ToLowerInvariant()}-{clock.Today:dd-MM-yyyy}.csv";
        return new CsvFile(name, Csv.Document(rows));
    }
}
