using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Domain;

namespace Tamiron.Application.Accounting;

// B9 · Alacak listesi ve muhasebe CSV'si (prototip 95-muhasebe, muhasebeCsv).

public sealed record ReceivableItem(Guid RecordId, string PlateNo, string? OwnerName, DateOnly? DeliveredAt, decimal Total);
public sealed record ReceivableList(decimal Total, IReadOnlyList<ReceivableItem> Items);

/// <summary>
/// Alacak bir BAKİYE, hareket değil: döneme tabi DEĞİL (Cemil, 05.09) — geçen aydan
/// kalan alacak bu ay da alacaktır. Ustanın sorusu "KİMDEN alacağım": sahip adı önde.
/// </summary>
public sealed record ListReceivables : IRequest<ReceivableList>;

public sealed record CsvFile(string FileName, string Content);

/// <summary>
/// TEK DÜZ TABLO: "Tür" sütunuyla Excel'de süzülür ve toplanır (bölümlü dosyada
/// başlıklar ara satır olur, süzgeç bozulur). Tahsilat ve gider seçili döneme,
/// alacak BUGÜNE (ekranla aynı kural).
/// </summary>
public sealed record ExportCashCsv(PeriodKind Period) : IRequest<CsvFile>;

internal sealed class AccountingHandlers(ITamironDb db, IClock clock) :
    IRequestHandler<ListReceivables, ReceivableList>,
    IRequestHandler<ExportCashCsv, CsvFile>
{
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
        var items = delivered.Where(r => r.PaymentMethod == PaymentMethod.Unpaid)
            .OrderByDescending(r => r.DeliveredAt).ThenByDescending(r => r.DateTimeCreated)
            .Select(r =>
            {
                vehicles.TryGetValue(r.VehicleId, out var v);
                return new ReceivableItem(r.Id, v?.PlateNo ?? "", v?.OwnerName, r.DeliveredAt, r.Total);
            }).ToList();
        return new ReceivableList(items.Sum(i => i.Total), items);
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
