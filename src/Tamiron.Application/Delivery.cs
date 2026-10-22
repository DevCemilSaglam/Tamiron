using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Domain;

namespace Tamiron.Application.ServiceRecords;

// B5 · Teslim (prototip 50-teslim). Hatırlatma hesabı DOMAIN'de
// (MaintenanceReminder); burada yalnız girdileri toplanır: dükkânın
// periyodu ve aynı aracın öteki kayıtlarının km ölçümleri.

public sealed record DeliveryPreview(Guid RecordId, ReminderPlan? Reminder);

public sealed record GetDeliveryPreview(Guid RecordId) : IRequest<DeliveryPreview>;
public sealed record DeliverRecord(Guid RecordId, PaymentMethod Method) : IRequest<ServiceRecordDto>;

internal sealed class DeliveryHandlers(ITamironDb db, IClock clock, ITenantContext tenant) :
    IRequestHandler<GetDeliveryPreview, DeliveryPreview>,
    IRequestHandler<DeliverRecord, ServiceRecordDto>
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
        r.Deliver(c.Method, clock.Today, policy, readings);   // gün sunucudan
        await db.SaveChangesAsync(ct);
        return await Map.LoadDto(db, r.Id, ct);
    }
}
