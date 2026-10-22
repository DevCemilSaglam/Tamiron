using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Domain;

namespace Tamiron.Application.Home;

// C3 · Ana ekran özeti (prototip 10-home menü alt yazıları + çekmece rozeti).
// Tek istek: ana ekran açılışta altı ayrı liste çekmesin. Her rakam ötekilerin
// KURALIYLA hesaplanır (aynı soruyu iki yerde cevaplamak iki cevap demektir, K-019.1):
// tahsil günü CollectedOn, alacak Unpaid bakiyesi, rozet IsReminderDue, müşteri Customers.From.

public sealed record HomeSummary(int InShopCount, int RecordCount, int VehicleCount, int CustomerCount,
                                 decimal CollectedToday, decimal Receivable, int DueReminderCount);

public sealed record GetHomeSummary : IRequest<HomeSummary>;

internal sealed class HomeHandler(ITamironDb db, IClock clock) : IRequestHandler<GetHomeSummary, HomeSummary>
{
    public async Task<HomeSummary> Handle(GetHomeSummary q, CancellationToken ct)
    {
        var today = clock.Today;
        var records = await db.ServiceRecords.AsNoTracking().Include(r => r.Lines).ToListAsync(ct);
        var vehicles = await db.Vehicles.AsNoTracking().ToListAsync(ct);
        return new HomeSummary(
            InShopCount: records.Count(r => r.DeliveredAt is null),
            RecordCount: records.Count,
            VehicleCount: vehicles.Count,
            CustomerCount: Domain.Customers.From(vehicles).Count,
            CollectedToday: records.Where(r => r.CollectedOn == today).Sum(r => r.Total),
            Receivable: records.Where(r => r.DeliveredAt is not null && r.PaymentMethod == PaymentMethod.Unpaid).Sum(r => r.Total),
            DueReminderCount: records.Count(r => r.IsReminderDue(today)));
    }
}
