using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Domain;

namespace Tamiron.Application.Accounting;

// B9 · Alacak listesi ve muhasebe CSV'si (prototip 95-muhasebe, muhasebeCsv).

/// <param name="Description">Şikâyet; boşsa tür adı (prototip 60-arac: <c>k.sikayet || TUR_AD[k.tur]</c>).</param>
/// <param name="Total">İşin toplamı. <paramref name="Paid"/>: o güne kadar alınan (elden taksit, K-078);
/// <paramref name="Remaining"/>: alacak — listenin toplamı bunların toplamıdır (prototip kalanTutar).</param>
public sealed record ReceivableItem(Guid RecordId, Guid VehicleId, string PlateNo, string? OwnerName, string Description,
                                    DateOnly? DeliveredAt, decimal Total, decimal Paid, decimal Remaining);
/// <param name="Total">Alacak = kalanların toplamı (K-078).</param>
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
        var (delivered, vehicles) = await Load(DeliveredIn(p), ct);
        var items = delivered
            .OrderByDescending(r => r.DeliveredAt).ThenByDescending(r => r.DateTimeCreated)
            .SelectMany(r => r.Lines.Where(l => l.IsShopPurchase).OrderBy(l => l.Position)
                .Select(l => new PartPurchaseItem(r.Id, l.Id, vehicles.TryGetValue(r.VehicleId, out var v) ? v.PlateNo : "",
                                                  r.DeliveredAt!.Value, l.Name, l.Cost!.Value)))
            .ToList();
        return new PartPurchaseList(q.Period, p.Start, p.End, items.Sum(i => i.Cost), items);
    }

    /// <summary>
    /// C9 · Süzgeç SQL'de: her sorgu yalnız kendi kayıtlarını yükler (eskiden teslim edilmiş
    /// BÜTÜN kayıtlar belleğe geliyordu). Araçlar plaka/sahip için; sayısı kayıtlardan çok azdır.
    /// </summary>
    private async Task<(List<ServiceRecord> records, Dictionary<Guid, Vehicle> vehicles)> Load(
        System.Linq.Expressions.Expression<Func<ServiceRecord, bool>> where, CancellationToken ct, Guid? vehicleId = null)
    {
        var q = db.ServiceRecords.AsNoTracking().Where(where);
        if (vehicleId is { } vid) q = q.Where(r => r.VehicleId == vid);
        var records = await q.ToListAsync(ct);   // satırlar ve ödemeler AutoInclude (K-078.2)
        var vehicles = await db.Vehicles.AsNoTracking().ToDictionaryAsync(v => v.Id, ct);
        return (records, vehicles);
    }

    private static System.Linq.Expressions.Expression<Func<ServiceRecord, bool>> DeliveredIn(Period p)
        => r => r.DeliveredAt >= p.Start && r.DeliveredAt <= p.End;

    public async Task<ReceivableList> Handle(ListReceivables q, CancellationToken ct)
    {
        // K-078: alacak = KALANI OLAN iş ("almadım" durumu değil) — kısmen ödenmiş iş kalanıyla girer.
        var (delivered, vehicles) = await Load(ServiceRecord.HasRemaining, ct, q.VehicleId);
        var items = delivered
            .OrderByDescending(r => r.DeliveredAt).ThenByDescending(r => r.DateTimeCreated)
            .Select(r =>
            {
                vehicles.TryGetValue(r.VehicleId, out var v);
                return new ReceivableItem(r.Id, r.VehicleId, v?.PlateNo ?? "", v?.OwnerName,
                    string.IsNullOrWhiteSpace(r.Complaint) ? ServiceTypeGuess.Label(r.Type) : r.Complaint,
                    r.DeliveredAt, r.Total, r.Paid, r.Remaining);
            }).ToList();
        return new ReceivableList(items.Sum(i => i.Remaining), items);
    }

    public async Task<JobProfitList> Handle(ListJobProfits q, CancellationToken ct)
    {
        var p = Period.Of(q.Period, clock.Today);
        var (delivered, vehicles) = await Load(DeliveredIn(p), ct);
        var items = delivered
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
        var (unpaid, vehicles) = await Load(ServiceRecord.HasRemaining, ct);
        // K-078 · Kasa ÖDEMELERDEN: dönemde alınan her ödeme kendi satırı (Cemil: "Ayrı satır").
        var payments = await db.Payments.AsNoTracking().Where(x => x.PaidOn >= p.Start && x.PaidOn <= p.End).ToListAsync(ct);
        var recIds = payments.Select(x => x.ServiceRecordId).Distinct().ToList();
        var paidRecords = await db.ServiceRecords.AsNoTracking().IgnoreAutoIncludes()
            .Where(r => recIds.Contains(r.Id)).ToDictionaryAsync(r => r.Id, ct);
        string Plate(ServiceRecord r) => vehicles.TryGetValue(r.VehicleId, out var v) ? v.PlateNo : "";
        string Owner(ServiceRecord r) => vehicles.TryGetValue(r.VehicleId, out var v) ? v.OwnerName ?? "" : "";
        string What(ServiceRecord r) => string.IsNullOrWhiteSpace(r.Complaint) ? "Servis kaydı" : r.Complaint;

        var rows = new List<object?[]> { new object?[] { "Tür", "Tarih", "Açıklama", "Plaka", "Müşteri", "Ödeme", "Tutar" } };

        // Tahsilat: ödeme GÜNÜNE göre (K-071.1 · K-078), tarih sırasıyla — kayıt sırası açılış sırasıdır,
        // ödeme sırası değil; aynı gün: kaydın sırası, sonra ödemenin yazılış sırası (prototip donemOdemeleri).
        rows.AddRange(payments
            .OrderBy(x => x.PaidOn).ThenBy(x => paidRecords[x.ServiceRecordId].DateTimeCreated).ThenBy(x => x.Sequence)
            .Select(x => { var r = paidRecords[x.ServiceRecordId];
                           return new object?[] { "Tahsilat", x.PaidOn, What(r), Plate(r), Owner(r), Csv.PaymentLabel(x.Method), x.Amount }; }));

        rows.AddRange((await db.Expenses.AsNoTracking().Where(e => e.Date >= p.Start && e.Date <= p.End)
                .OrderBy(e => e.Date).ThenBy(e => e.DateTimeCreated).ToListAsync(ct))
            .Select(e => new object?[] { "Gider", e.Date, e.Description, "", "", "", e.Amount }));

        rows.AddRange(unpaid
            .OrderBy(r => r.DeliveredAt).ThenBy(r => r.DateTimeCreated)
            .Select(r => new object?[] { "Alacak", r.DeliveredAt, What(r), Plate(r), Owner(r), Csv.PaymentLabel(PaymentMethod.Unpaid), r.Remaining }));

        var name = $"tamiron-muhasebe-{q.Period.ToString().ToLowerInvariant()}-{clock.Today:dd-MM-yyyy}.csv";
        return new CsvFile(name, Csv.Document(rows));
    }
}
