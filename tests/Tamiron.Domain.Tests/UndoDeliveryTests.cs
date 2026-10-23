using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// D14 · Yanlış teslimi geri al (K-076.13): yalnız AYNI GÜN; sonradan tahsil edildiyse önce tahsil
/// geri alınır; hatırlatma gönderildiyse geri alınmaz. Kayıt teslim öncesi hâline döner.
/// </summary>
public class UndoDeliveryTests
{
    static readonly DateOnly Today = new(2026, 10, 22);

    static ServiceRecord Delivered(PaymentMethod m, string complaint = "Periyodik bakım")
    {
        var r = new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), complaint);
        r.SetKm("86400");
        r.Deliver(m, Today, MaintenancePolicy.Default, []);
        return r;
    }

    [Fact]
    public void Same_day_undo_reopens_the_record_and_drops_payment_and_reminder()
    {
        var r = Delivered(PaymentMethod.Cash);
        Assert.NotNull(r.ReminderDate);
        r.UndoDelivery(Today);
        Assert.Equal((null, null, null, false), (r.DeliveredAt, r.PaymentMethod, r.ReminderDate, r.ReminderSent));
        r.Deliver(PaymentMethod.Card, Today, MaintenancePolicy.Default, []);   // yeniden teslim edilebilir
        Assert.Equal(PaymentMethod.Card, r.PaymentMethod);
    }

    [Fact]
    public void Next_day_it_is_too_late()
        => Assert.Equal("delivery.undo_too_late",
            Assert.Throws<DomainException>(() => Delivered(PaymentMethod.Cash).UndoDelivery(Today.AddDays(1))).Code);

    [Fact]
    public void Collected_later_must_undo_collection_first()
    {
        var r = Delivered(PaymentMethod.Unpaid);
        r.CollectPayment(PaymentMethod.Cash, Today);
        Assert.Equal("delivery.undo_collection_first", Assert.Throws<DomainException>(() => r.UndoDelivery(Today)).Code);
        r.UndoCollection();
        r.UndoDelivery(Today);
        Assert.Null(r.DeliveredAt);
    }

    [Fact]
    public void Sent_reminder_blocks_undo()
    {
        var r = Delivered(PaymentMethod.Cash);
        r.MarkReminderSent();
        Assert.Equal("delivery.undo_reminder_sent", Assert.Throws<DomainException>(() => r.UndoDelivery(Today)).Code);
    }

    [Fact]
    public void Open_record_has_nothing_to_undo()
        => Assert.Equal("delivery.nothing_to_undo",
            Assert.Throws<DomainException>(() => new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), "Fren").UndoDelivery(Today)).Code);
}
