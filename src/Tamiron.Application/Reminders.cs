using MediatR;
using Microsoft.EntityFrameworkCore;
using Tamiron.Domain;

namespace Tamiron.Application.Reminders;

// B12 · Hatırlatması gelenler — ana ekran, çekmece rozeti ve araç ekranı AYNI
// kaynaktan beslenir (prototip hatirlatmasiGelenler). Sıra: en çok bekleyen
// üstte (prototipte kayıt sırasıydı; gecikme sırası ustanın asıl sorusu).

public sealed record DueReminder(Guid RecordId, Guid VehicleId, string PlateNo, string? OwnerName, string? Phone,
                                 DateOnly ReminderDate, int DaysOverdue);

public sealed record ListDueReminders : IRequest<List<DueReminder>>;

internal sealed class ReminderHandlers(ITamironDb db, IClock clock) : IRequestHandler<ListDueReminders, List<DueReminder>>
{
    public async Task<List<DueReminder>> Handle(ListDueReminders q, CancellationToken ct)
    {
        var today = clock.Today;
        // SQL'de kaba süzgeç (tarihi gelmiş, gönderilmemiş); kural domain'de (IsReminderDue).
        var records = await db.ServiceRecords.AsNoTracking()
            .Where(r => r.ReminderDate != null && r.ReminderDate <= today && !r.ReminderSent).ToListAsync(ct);
        var vehicles = await db.Vehicles.AsNoTracking().ToDictionaryAsync(v => v.Id, ct);
        return records.Where(r => r.IsReminderDue(today))
            .OrderBy(r => r.ReminderDate)
            .Select(r =>
            {
                vehicles.TryGetValue(r.VehicleId, out var v);
                return new DueReminder(r.Id, r.VehicleId, v?.PlateNo ?? "", v?.OwnerName, v?.DefaultTarget()?.Phone,
                                       r.ReminderDate!.Value, today.DayNumber - r.ReminderDate!.Value.DayNumber);
            }).ToList();
    }
}
