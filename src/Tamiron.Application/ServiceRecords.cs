using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Domain;

namespace Tamiron.Application.ServiceRecords;

// ---- Okuma modeli ----

public sealed record ServiceLineDto(Guid Id, LineKind Kind, string Name, decimal Price, decimal? Cost,
                                    PartSupplier? SuppliedBy, bool CountsInTotal);

public sealed record ServiceRecordDto(Guid Id, Guid VehicleId, string PlateNo, ServiceType Type, bool TypeSetManually,
                                      string Complaint, string? Km, DateOnly? DeliveredAt, PaymentMethod? PaymentMethod,
                                      DateOnly? PaymentCollectedAt, DateOnly? CollectedOn,
                                      decimal Total, IReadOnlyList<ServiceLineDto> Lines);

internal static class Map
{
    public static ServiceRecordDto ToDto(this ServiceRecord r, string plateNo) => new(
        r.Id, r.VehicleId, plateNo, r.Type, r.TypeSetManually, r.Complaint, r.Km, r.DeliveredAt, r.PaymentMethod,
        r.PaymentCollectedAt, r.CollectedOn, r.Total,
        r.Lines.OrderBy(l => l.Position).ThenBy(l => l.DateTimeCreated).Select(l => new ServiceLineDto(l.Id, l.Kind, l.Name, l.Price, l.Cost, l.SuppliedBy, l.CountsInTotal)).ToList());

    public static async Task<ServiceRecordDto> LoadDto(ITamironDb db, Guid id, CancellationToken ct, bool tracking = false)
    {
        var q = db.ServiceRecords.Include(r => r.Lines).AsQueryable();
        if (!tracking) q = q.AsNoTracking();
        var r = await q.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new NotFoundException("Servis kaydı");
        var plate = await db.Vehicles.AsNoTracking().Where(v => v.Id == r.VehicleId).Select(v => v.PlateNo).FirstOrDefaultAsync(ct) ?? "";
        return r.ToDto(plate);
    }
}

// ---- Sorgu: tek kayıt ----

public sealed record GetServiceRecord(Guid Id) : IRequest<ServiceRecordDto>;

internal sealed class GetServiceRecordHandler(ITamironDb db) : IRequestHandler<GetServiceRecord, ServiceRecordDto>
{
    public Task<ServiceRecordDto> Handle(GetServiceRecord q, CancellationToken ct) => Map.LoadDto(db, q.Id, ct);
}

// ---- Komut: kayıt aç (B2 · ana yol) ----

/// <summary>
/// Plakayla araç bulunur; yoksa oluşturulur (prototip 20-ruhsat "Kaydı aç").
/// Kayıt açmak araç kimliği İSTEMEZ — kapı parça sormada (K-068).
/// Tür verilmediyse metinden TAHMİN edilir ve "elle" sayılmaz (sonra düzeltilebilir).
/// ⚠️ Aynı aracın açık kaydı varken ikinci kayıt ENGELLENMEZ — prototip de
/// engellemiyor; engellenmesi bir ürün kararı olur, sessizce eklenmez.
/// </summary>
public sealed record OpenServiceRecord(string Plate, string? Complaint, ServiceType? Type = null, string? Km = null)
    : IRequest<ServiceRecordDto>;

internal sealed class OpenServiceRecordHandler(ITamironDb db, ITenantContext tenant) : IRequestHandler<OpenServiceRecord, ServiceRecordDto>
{
    public async Task<ServiceRecordDto> Handle(OpenServiceRecord c, CancellationToken ct)
    {
        var plate = Domain.Plate.Normalize(c.Plate);
        if (!Domain.Plate.IsValid(plate))
            throw new DomainException("plate.invalid", "Önce plakayı yaz — ör. 34 ABC 123.");

        var vehicle = await db.Vehicles.FirstOrDefaultAsync(v => v.PlateNo == plate, ct);
        if (vehicle is null)
        {
            vehicle = new Vehicle { TenantId = tenant.TenantId, PlateNo = plate };
            db.Vehicles.Add(vehicle);
        }

        var complaint = (c.Complaint ?? "").Trim();
        var r = new ServiceRecord(tenant.TenantId, vehicle.Id, c.Type ?? ServiceTypeGuess.FromText(complaint), complaint)
        {
            TypeSetManually = c.Type is not null,
            Km = string.IsNullOrWhiteSpace(c.Km) ? null : c.Km.Trim()
        };
        db.ServiceRecords.Add(r);
        await db.SaveChangesAsync(ct);
        return r.ToDto(vehicle.PlateNo);
    }
}

// ---- Komut: veresiyenin parası geldi (K-071.1) ----

public sealed record CollectPayment(Guid RecordId, PaymentMethod Method) : IRequest<ServiceRecordDto>;

internal sealed class CollectPaymentHandler(ITamironDb db, IClock clock) : IRequestHandler<CollectPayment, ServiceRecordDto>
{
    public async Task<ServiceRecordDto> Handle(CollectPayment c, CancellationToken ct)
    {
        var r = await db.ServiceRecords.FirstOrDefaultAsync(x => x.Id == c.RecordId, ct)
                ?? throw new NotFoundException("Servis kaydı");
        r.CollectPayment(c.Method, clock.Today);   // kural domain'de; gün sunucudan, istemciden değil
        await db.SaveChangesAsync(ct);
        return await Map.LoadDto(db, r.Id, ct);
    }
}

public sealed record UndoCollection(Guid RecordId) : IRequest<ServiceRecordDto>;

internal sealed class UndoCollectionHandler(ITamironDb db) : IRequestHandler<UndoCollection, ServiceRecordDto>
{
    public async Task<ServiceRecordDto> Handle(UndoCollection c, CancellationToken ct)
    {
        var r = await db.ServiceRecords.FirstOrDefaultAsync(x => x.Id == c.RecordId, ct)
                ?? throw new NotFoundException("Servis kaydı");
        r.UndoCollection();
        await db.SaveChangesAsync(ct);
        return await Map.LoadDto(db, r.Id, ct);
    }
}

// ---- Sorgu: kasa özeti (muhasebe · Basit) ----

public sealed record CashSummary(PeriodKind Period, DateOnly Start, DateOnly End, int DeliveredCount,
                                 decimal Cash, decimal Card, decimal Transfer, decimal Collected, decimal Receivable);

public sealed record GetCashSummary(PeriodKind Period) : IRequest<CashSummary>;

internal sealed class GetCashSummaryHandler(ITamironDb db, IClock clock) : IRequestHandler<GetCashSummary, CashSummary>
{
    public async Task<CashSummary> Handle(GetCashSummary q, CancellationToken ct)
    {
        var p = Period.Of(q.Period, clock.Today);
        // Teslim edilmiş kayıtlar SQL'de süzülür; tahsil günü ve toplam DOMAIN'de
        // hesaplanır (CollectedOn, Total). Kuralı burada LINQ'e ikinci kez yazmak,
        // aynı kuralı iki yerde tutmak olurdu (K-019.1). Bir dükkânın teslim
        // sayısı için bellekte hesap ucuz; büyürse SQL'e ifade olarak taşınır.
        var delivered = await db.ServiceRecords.AsNoTracking().Include(r => r.Lines)
            .Where(r => r.DeliveredAt != null).ToListAsync(ct);

        decimal Sum(PaymentMethod m) => delivered
            .Where(r => r.PaymentMethod == m && r.CollectedOn is { } d && p.Contains(d)).Sum(r => r.Total);

        var cash = Sum(PaymentMethod.Cash);
        var card = Sum(PaymentMethod.Card);
        var transfer = Sum(PaymentMethod.Transfer);
        return new CashSummary(q.Period, p.Start, p.End,
            DeliveredCount: delivered.Count(r => p.Contains(r.DeliveredAt!.Value)),
            cash, card, transfer, cash + card + transfer,
            // Alacak bir BAKİYE, döneme tabi değil (05.09).
            Receivable: delivered.Where(r => r.PaymentMethod == PaymentMethod.Unpaid).Sum(r => r.Total));
    }
}
