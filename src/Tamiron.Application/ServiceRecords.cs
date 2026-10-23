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
                                      bool WaitingForPart, DateOnly? WaitingSince, bool ShowWaitingToCustomer,
                                      DateOnly? ReminderDate, bool ReminderSent,
                                      decimal Total, IReadOnlyList<ServiceLineDto> Lines,
                                      IReadOnlyList<string> Warnings);

internal static class Map
{
    /// <summary>Açık veresiyelerin toplamı — kasa özeti ve ana ekran AYNI yerden okur (C9).</summary>
    public static async Task<decimal> UnpaidTotal(ITamironDb db, CancellationToken ct)
        => (await db.ServiceRecords.AsNoTracking().Include(r => r.Lines)
                .Where(r => r.DeliveredAt != null && r.PaymentMethod == PaymentMethod.Unpaid).ToListAsync(ct))
           .Sum(r => r.Total);

    public static ServiceRecordDto ToDto(this ServiceRecord r, Vehicle? v) => new(
        r.Id, r.VehicleId, v?.PlateNo ?? "", r.Type, r.TypeSetManually, r.Complaint, r.Km, r.DeliveredAt, r.PaymentMethod,
        r.PaymentCollectedAt, r.CollectedOn, r.WaitingForPart, r.WaitingSince, r.ShowWaitingToCustomer,
        r.ReminderDate, r.ReminderSent, r.Total,
        r.Lines.OrderBy(l => l.Position).ThenBy(l => l.DateTimeCreated).Select(l => new ServiceLineDto(l.Id, l.Kind, l.Name, l.Price, l.Cost, l.SuppliedBy, l.CountsInTotal)).ToList(),
        // Eksikler DOMAIN'de (kEksikler); arayüz yalnız gösterir.
        r.DeliveryWarnings(v?.HasPhone ?? false));

    public static async Task<ServiceRecordDto> LoadDto(ITamironDb db, Guid id, CancellationToken ct, bool tracking = false)
    {
        var q = db.ServiceRecords.Include(r => r.Lines).AsQueryable();
        if (!tracking) q = q.AsNoTracking();
        var r = await q.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new NotFoundException("Servis kaydı");
        var v = await db.Vehicles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == r.VehicleId, ct);
        return r.ToDto(v);
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

        // Tür verilmediyse tahmin DOMAIN'de (kurucu); km biçimi de orada.
        var r = new ServiceRecord(tenant.TenantId, vehicle.Id, c.Complaint ?? "", c.Type);
        r.SetKm(c.Km);
        db.ServiceRecords.Add(r);
        await db.SaveChangesAsync(ct);
        return r.ToDto(vehicle);
    }
}

// ---- Komut: kaydın bayrakları (B4) ----

/// <summary>
/// Kısmi güncelleme: null = DOKUNMA. Sıra önemli — önce şikâyet (tür onu takip
/// eder), sonra tür (elle seçim tahmini yener); önce bekleme bayrağı, sonra
/// müşteriye gösterme (bayrak kapalıyken gösterilemez).
/// Bekleme günü SUNUCUDAN: istemci tarih göndermez.
/// </summary>
public sealed record UpdateRecord(Guid Id, string? Complaint = null, ServiceType? Type = null, string? Km = null,
                                  bool? WaitingForPart = null, bool? ShowWaitingToCustomer = null) : IRequest<ServiceRecordDto>;

internal sealed class UpdateRecordHandler(ITamironDb db, IClock clock) : IRequestHandler<UpdateRecord, ServiceRecordDto>
{
    public async Task<ServiceRecordDto> Handle(UpdateRecord c, CancellationToken ct)
    {
        var r = await db.ServiceRecords.FirstOrDefaultAsync(x => x.Id == c.Id, ct) ?? throw new NotFoundException("Servis kaydı");
        if (c.Complaint is not null) r.SetComplaint(c.Complaint);
        if (c.Type is { } t) r.SetType(t);
        if (c.Km is not null) r.SetKm(c.Km);
        if (c.WaitingForPart is { } w) r.SetWaitingForPart(w, clock.Today);
        if (c.ShowWaitingToCustomer is { } s) r.SetShowWaitingToCustomer(s);
        await db.SaveChangesAsync(ct);
        return await Map.LoadDto(db, r.Id, ct);
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
        // C9 · Süzgeç SQL'de: yalnız dönemde tahsil edilenler ve açık veresiyeler yüklenir.
        // Tahsil günü kuralının SQL biçimi ServiceRecord.CollectedBetween — bellekteki
        // CollectedOn ile eşitliği testli (CollectedExpressionTests). Toplam DOMAIN'de
        // (Total: müşteri parçası hariç) — o kural SQL'e İKİNCİ kez yazılmadı.
        var collected = await db.ServiceRecords.AsNoTracking().Include(r => r.Lines)
            .Where(ServiceRecord.CollectedBetween(p.Start, p.End)).ToListAsync(ct);

        decimal Sum(PaymentMethod m) => collected.Where(r => r.PaymentMethod == m).Sum(r => r.Total);

        var cash = Sum(PaymentMethod.Cash);
        var card = Sum(PaymentMethod.Card);
        var transfer = Sum(PaymentMethod.Transfer);
        return new CashSummary(q.Period, p.Start, p.End,
            DeliveredCount: await db.ServiceRecords.CountAsync(r => r.DeliveredAt >= p.Start && r.DeliveredAt <= p.End, ct),
            cash, card, transfer, cash + card + transfer,
            // Alacak bir BAKİYE, döneme tabi değil (05.09).
            Receivable: await Map.UnpaidTotal(db, ct));
    }
}

// ---- D13 · Yanlış açılan kaydı sil / geri al (K-076.12) ----

public sealed record DeleteServiceRecord(Guid Id) : IRequest;
public sealed record RestoreServiceRecord(Guid Id) : IRequest<ServiceRecordDto>;

internal sealed class DeleteServiceRecordHandlers(ITamironDb db, ITenantContext tenant) :
    IRequestHandler<DeleteServiceRecord>,
    IRequestHandler<RestoreServiceRecord, ServiceRecordDto>
{
    /// <summary>Silme = soft-delete; satırlar yerinde kalır, kayıt geri gelince onlarla döner.</summary>
    public async Task Handle(DeleteServiceRecord c, CancellationToken ct)
    {
        var r = await db.ServiceRecords.FirstOrDefaultAsync(x => x.Id == c.Id, ct) ?? throw new NotFoundException("Servis kaydı");
        r.EnsureDeletable();
        db.ServiceRecords.Remove(r);
        await db.SaveChangesAsync(ct);
    }

    public async Task<ServiceRecordDto> Handle(RestoreServiceRecord c, CancellationToken ct)
    {
        // Silinmişi görmek için süzgeç kapanır — kiracı AÇIKÇA yeniden yazılır (RestoreLine'daki ders).
        var r = await db.ServiceRecords.IgnoreQueryFilters().FirstOrDefaultAsync(x =>
                    x.Id == c.Id && x.TenantId == tenant.TenantId && x.DateTimeDeleted != null, ct)
                ?? throw new NotFoundException("Silinmiş kayıt");
        r.DateTimeDeleted = null;
        await db.SaveChangesAsync(ct);
        return await Map.LoadDto(db, r.Id, ct);
    }
}
