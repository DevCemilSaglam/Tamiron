using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>B12 · Hatırlatması gelenler — prototip hatirlatmasiGelenler; gönderildi işareti (85-mesaj).</summary>
public class ReminderDueTests
{
    static readonly DateOnly Today = new(2026, 10, 22);

    static ServiceRecord Delivered(string complaint, DateOnly on)
    {
        var r = new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), complaint);
        r.Deliver(PaymentMethod.Cash, on, MaintenancePolicy.Default, []);
        return r;
    }

    [Fact]
    public void Due_when_date_reached_and_not_sent()
    {
        Assert.True(Delivered("bakım", new DateOnly(2026, 4, 22)).IsReminderDue(Today));      // tam bugün
        Assert.True(Delivered("bakım", new DateOnly(2026, 3, 1)).IsReminderDue(Today));       // geçmiş
        Assert.False(Delivered("bakım", new DateOnly(2026, 4, 23)).IsReminderDue(Today));     // yarın
        Assert.False(Delivered("fren", new DateOnly(2026, 3, 1)).IsReminderDue(Today));       // bakım değil
    }

    [Fact]
    public void Sent_reminder_is_no_longer_due()
    {
        var r = Delivered("bakım", new DateOnly(2026, 3, 1));
        r.MarkReminderSent();
        Assert.True(r.ReminderSent);
        Assert.False(r.IsReminderDue(Today));
    }
}
