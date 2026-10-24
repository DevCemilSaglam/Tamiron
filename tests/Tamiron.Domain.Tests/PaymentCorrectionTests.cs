using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// E1 · Ödeme yolunu düzelt (K-077.2): "nakit aldım" dedi, kartla almıştı. Teslim ve tahsil günü
/// KORUNUR — yalnız yol değişir; kasa dağılımı kendiliğinden düzelir.
/// </summary>
public class PaymentCorrectionTests
{
    static readonly DateOnly Day = new(2026, 10, 20);

    static ServiceRecord Delivered(PaymentMethod m)
    {
        var r = new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), "Fren");
        r.AddLine(ServiceLine.Labor("İşçilik", 100m));   // 0 ₺'lik işte tahsil edilecek kalan yok (K-078)
        r.Deliver(m, Day, MaintenancePolicy.Default, []);
        return r;
    }

    [Theory]
    [InlineData(PaymentMethod.Cash, PaymentMethod.Card)]
    [InlineData(PaymentMethod.Card, PaymentMethod.Transfer)]
    [InlineData(PaymentMethod.Cash, PaymentMethod.Unpaid)]      // "aslında almadım"
    [InlineData(PaymentMethod.Unpaid, PaymentMethod.Cash)]      // "aslında teslimde almıştım"
    public void Paid_at_delivery_can_switch_freely_and_keeps_the_day(PaymentMethod from, PaymentMethod to)
    {
        var r = Delivered(from);
        r.CorrectPaymentMethod(to);
        Assert.Equal((to, Day, (DateOnly?)null), (r.PaymentMethod!.Value, r.DeliveredAt!.Value, r.PaymentCollectedAt));
        Assert.Equal(to == PaymentMethod.Unpaid ? null : Day, r.CollectedOn);   // kasa günü teslim günü
    }

    [Fact]
    public void Later_collection_keeps_its_day_and_only_its_method_changes()
    {
        var r = Delivered(PaymentMethod.Unpaid);
        r.CollectPayment(PaymentMethod.Cash, Day.AddDays(5));
        r.CorrectPaymentMethod(PaymentMethod.Card);
        Assert.Equal((PaymentMethod.Card, Day.AddDays(5)), (r.PaymentMethod!.Value, r.PaymentCollectedAt!.Value));
        // "Almadım"a çevirmek tahsili geri almaktır — o ayrı uç (UndoCollection).
        Assert.Equal("payment.use_undo_collection", Assert.Throws<DomainException>(() => r.CorrectPaymentMethod(PaymentMethod.Unpaid)).Code);
    }

    [Fact]
    public void Open_record_has_no_payment_to_correct()
        => Assert.Equal("payment.not_delivered",
            Assert.Throws<DomainException>(() => new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), "x").CorrectPaymentMethod(PaymentMethod.Card)).Code);

    [Fact]
    public void Same_method_is_said_not_silently_accepted()
        => Assert.Equal("payment.same_method",
            Assert.Throws<DomainException>(() => Delivered(PaymentMethod.Card).CorrectPaymentMethod(PaymentMethod.Card)).Code);
}
