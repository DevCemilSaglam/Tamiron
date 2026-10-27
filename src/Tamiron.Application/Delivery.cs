using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Domain;

namespace Tamiron.Application.ServiceRecords;

// B5 · Teslim (prototip 50-teslim). Hatırlatma hesabı DOMAIN'de
// (MaintenanceReminder); burada yalnız girdileri toplanır: dükkânın
// periyodu ve aynı aracın öteki kayıtlarının km ölçümleri.

public sealed record DeliveryPreview(Guid RecordId, ReminderPlan? Reminder);

public sealed record GetDeliveryPreview(Guid RecordId) : IRequest<DeliveryPreview>;
/// <param name="Amount">Dolu = "Bir kısmını aldım" (K-078): o kadarı teslimde, kalanı veresiye.</param>
public sealed record DeliverRecord(Guid RecordId, PaymentMethod Method, decimal? Amount = null) : IRequest<ServiceRecordDto>;
/// <summary>D14 · Yanlış teslimi geri al — aynı gün (K-076.13).</summary>
public sealed record UndoDelivery(Guid RecordId) : IRequest<ServiceRecordDto>;
/// <summary>E1 · Ödeme yolunu düzelt — teslim/tahsil günü korunur (K-077.2).</summary>
public sealed record CorrectPayment(Guid RecordId, PaymentMethod Method) : IRequest<ServiceRecordDto>;

internal sealed class DeliveryHandlers(ITamironDb db, IClock clock, ITenantContext tenant) :
    IRequestHandler<GetDeliveryPreview, DeliveryPreview>,
    IRequestHandler<DeliverRecord, ServiceRecordDto>,
    IRequestHandler<UndoDelivery, ServiceRecordDto>,
    IRequestHandler<CorrectPayment, ServiceRecordDto>
{
    private async Task<(ServiceRecord, MaintenancePolicy, List<KmReading>)> Inputs(Guid id, bool tracking, CancellationToken ct)
    {
        var q = tracking ? db.ServiceRecords : db.ServiceRecords.AsNoTracking();
        var r = await q.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new NotFoundException("Servis kaydı");
        var shop = await db.Shops.AsNoTracking().FirstOrDefaultAsync(s => s.Id == tenant.TenantId, ct);
        var policy = shop is null ? MaintenancePolicy.Default : MaintenancePolicy.Of(shop);

        // Km geçmişi: aynı aracın öteki kayıtları. Tarih: teslim günü, yoksa açılış
        // günü (prototip kmGecmisi: teslimTarihi || acilis).
        var others = await db.ServiceRecords.AsNoTracking()
            .Where(x => x.VehicleId == r.VehicleId && x.Id != r.Id && x.Km != null)
            .Select(x => new { x.Km, x.DeliveredAt, x.DateTimeCreated }).ToListAsync(ct);
        var readings = others
            .Select(x => (km: KmReading.Parse(x.Km), date: x.DeliveredAt ?? clock.LocalDate(x.DateTimeCreated)))
            .Where(x => x.km is not null)
            .Select(x => new KmReading(x.date, x.km!.Value)).ToList();
        return (r, policy, readings);
    }

    public async Task<DeliveryPreview> Handle(GetDeliveryPreview q, CancellationToken ct)
    {
        var (r, policy, readings) = await Inputs(q.RecordId, tracking: false, ct);
        return new DeliveryPreview(r.Id, r.PlanReminder(clock.Today, policy, readings));
    }

    public async Task<ServiceRecordDto> Handle(DeliverRecord c, CancellationToken ct)
    {
        var (r, policy, readings) = await Inputs(c.RecordId, tracking: true, ct);
        // G7: avanstan düşülecekse sahibin bakiyesi yetmeli — teslimden ÖNCE (araç teslim edilmiş görünmesin).
        if (c.Method == PaymentMethod.Advance)
            AdvanceBalance.EnsureCovers(await Advances.AdvanceQueries.BalanceFor(db, r.VehicleId, ct), c.Amount ?? r.Total);
        if (c.Amount is { } amount) r.DeliverPartly(amount, c.Method, clock.Today, policy, readings);   // gün sunucudan
        else r.Deliver(c.Method, clock.Today, policy, readings);
        await db.SaveChangesAsync(ct);
        return await Map.LoadDto(db, r.Id, ct);
    }

    public async Task<ServiceRecordDto> Handle(CorrectPayment c, CancellationToken ct)
    {
        var r = await db.ServiceRecords.FirstOrDefaultAsync(x => x.Id == c.RecordId, ct) ?? throw new NotFoundException("Servis kaydı");
        r.CorrectPaymentMethod(c.Method);
        await db.SaveChangesAsync(ct);
        return await Map.LoadDto(db, r.Id, ct);
    }

    public async Task<ServiceRecordDto> Handle(UndoDelivery c, CancellationToken ct)
    {
        var r = await db.ServiceRecords.FirstOrDefaultAsync(x => x.Id == c.RecordId, ct) ?? throw new NotFoundException("Servis kaydı");
        r.UndoDelivery(clock.Today);
        await db.SaveChangesAsync(ct);
        return await Map.LoadDto(db, r.Id, ct);
    }
}
