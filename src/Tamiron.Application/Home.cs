using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Application.ServiceRecords;
using Tamiron.Domain;

namespace Tamiron.Application.Home;

// C3 · Ana ekran özeti (prototip 10-home menü alt yazıları + çekmece rozeti).
// Tek istek: ana ekran açılışta altı ayrı liste çekmesin. Her rakam ötekilerin
// KURALIYLA hesaplanır (aynı soruyu iki yerde cevaplamak iki cevap demektir, K-019.1):
// kasa Map.CollectedBetween (ödemelerden, K-078), alacak Map.UnpaidTotal, rozet IsReminderDue,
// müşteri Customers.From.
// C9 · Sayımlar ve süzgeçler SQL'de; bütün kayıtlar belleğe yüklenmez.

public sealed record HomeSummary(int InShopCount, int RecordCount, int VehicleCount, int CustomerCount,
                                 decimal CollectedToday, decimal Receivable, int DueReminderCount);

public sealed record GetHomeSummary : IRequest<HomeSummary>;

internal sealed class HomeHandler(ITamironDb db, IClock clock) : IRequestHandler<GetHomeSummary, HomeSummary>
{
    public async Task<HomeSummary> Handle(GetHomeSummary q, CancellationToken ct)
    {
        var today = clock.Today;
        // K-078: bugün ALINAN para — kısmi ödemenin bugünkü parçası (prototip bugunTahsilat).
        var collectedToday = await Map.CollectedBetween(db, today, today, ct);
        // Rozet: SQL'de kaba süzgeç (tarihi gelmiş, gönderilmemiş), kural domain'de.
        var maybeDue = await db.ServiceRecords.AsNoTracking()
            .Where(r => r.ReminderDate != null && r.ReminderDate <= today && !r.ReminderSent).ToListAsync(ct);
        // Müşteri araçtan türer (A-M1): yalnız ad ve telefon gerekir.
        var owners = await db.Vehicles.AsNoTracking().Where(v => v.OwnerName != null).ToListAsync(ct);
        return new HomeSummary(
            InShopCount: await db.ServiceRecords.CountAsync(r => r.DeliveredAt == null, ct),
            RecordCount: await db.ServiceRecords.CountAsync(ct),
            VehicleCount: await db.Vehicles.CountAsync(ct),
            CustomerCount: Domain.Customers.From(owners).Count,
            CollectedToday: collectedToday.Values.Sum(),   // avanstan düşülen zaten yok (G7)
            Receivable: await Map.UnpaidTotal(db, ct),
            DueReminderCount: maybeDue.Count(r => r.IsReminderDue(today)));
    }
}
