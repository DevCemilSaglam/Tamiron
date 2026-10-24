using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// F3 · Elden taksit = kısmi tahsilat (K-078, TASARIM-E6). Beklenen değerler prototipin
/// <c>kayitOdemeleri</c> · <c>kalanTutar</c> · <c>odemeEkle</c> · <c>odemeHatasi</c> ·
/// <c>odemeDurumu</c> · <c>sonOdemeyiGeriAl</c> davranışıdır (02-veri.js) — backend her alınan
/// parayı <see cref="Payment"/> satırı olarak tutar, prototip tam ödemede türetir; sonuç aynı.
/// </summary>
public class PartialPaymentTests
{
    static readonly DateOnly Aug28 = new(2026, 8, 28);
    static readonly DateOnly Sep3 = new(2026, 9, 3);
    static readonly DateOnly Oct22 = new(2026, 10, 22);

    /// <summary>Prototipin 8 numaralı kaydı: akü 3.200 + takma 250 = 3.450, "almadım".</summary>
    static ServiceRecord Veresiye()
    {
        var r = new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), "Akü değişimi");
        r.AddLine(ServiceLine.Part("Akü 60 Ah", 3200m, PartSupplier.Shop, 2450m));
        r.AddLine(ServiceLine.Labor("Takma", 250m));
        r.Deliver(PaymentMethod.Unpaid, Aug28, MaintenancePolicy.Default, []);
        return r;
    }

    static ServiceRecord Open3450()
    {
        var r = new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), "Akü değişimi");
        r.AddLine(ServiceLine.Part("Akü 60 Ah", 3200m, PartSupplier.Shop, 2450m));
        r.AddLine(ServiceLine.Labor("Takma", 250m));
        return r;
    }

    static string Code(Action a) => Assert.Throws<DomainException>(a).Code;

    // ---- Tam ödeme: bugünkü davranış, artık bir ödeme satırıyla ----

    [Fact]
    public void Full_payment_at_delivery_is_one_payment_of_the_total_on_delivery_day()
    {
        var r = Open3450();
        r.Deliver(PaymentMethod.Cash, Aug28, MaintenancePolicy.Default, []);
        var p = Assert.Single(r.Payments);
        Assert.Equal((3450m, PaymentMethod.Cash, Aug28, true), (p.Amount, p.Method, p.PaidOn, p.AtDelivery));
        Assert.Equal((0m, 3450m), (r.Remaining, r.Paid));
        Assert.Equal((PaymentMethod.Cash, (DateOnly?)null, Aug28), (r.PaymentMethod!.Value, r.PaymentCollectedAt, r.CollectedOn));
    }

    [Fact]
    public void Zero_total_paid_delivery_writes_no_payment_row()
    {
        // Ödeme > 0 (VERI-MODELI §2b): 0 ₺'lik iş "nakit" teslim edilir ama kasaya 0'lık satır yazılmaz.
        var r = new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), "Kontrol");
        r.Deliver(PaymentMethod.Cash, Aug28, MaintenancePolicy.Default, []);
        Assert.Empty(r.Payments);
        Assert.Equal((PaymentMethod.Cash, Aug28), (r.PaymentMethod!.Value, r.CollectedOn!.Value));
    }

    [Fact]
    public void Unpaid_delivery_has_no_payment_and_the_whole_total_remains()
    {
        var r = Veresiye();
        Assert.Empty(r.Payments);
        Assert.Equal(3450m, r.Remaining);
    }

    [Fact]
    public void Open_record_has_nothing_remaining()
        => Assert.Equal(0m, Open3450().Remaining);   // prototip kalanTutar: teslimsiz 0

    [Fact]
    public void Full_later_collection_is_the_remaining_on_its_day()
    {
        var r = Veresiye();
        r.CollectPayment(PaymentMethod.Card, Oct22);
        var p = Assert.Single(r.Payments);
        Assert.Equal((3450m, PaymentMethod.Card, Oct22, false), (p.Amount, p.Method, p.PaidOn, p.AtDelivery));
        Assert.Equal((PaymentMethod.Card, Oct22, 0m), (r.PaymentMethod!.Value, r.PaymentCollectedAt!.Value, r.Remaining));
    }

    // ---- Kısmi: "x lira aldım" ----

    [Fact]
    public void Partial_collection_leaves_the_rest_unpaid()
    {
        var r = Veresiye();
        r.CollectPayment(1000m, PaymentMethod.Cash, Sep3);
        Assert.Equal((1000m, 2450m), (r.Paid, r.Remaining));
        // Kalan varken durum "almadım" (prototip odemeDurumu) — veresiye listelerinde kalır.
        Assert.Equal((PaymentMethod.Unpaid, (DateOnly?)null, (DateOnly?)null), (r.PaymentMethod!.Value, r.PaymentCollectedAt, r.CollectedOn));
    }

    [Fact]
    public void Last_installment_closes_with_its_method_and_day()
    {
        var r = Veresiye();
        r.CollectPayment(1000m, PaymentMethod.Cash, Sep3);
        r.CollectPayment(2450m, PaymentMethod.Card, Oct22);
        Assert.Equal(2, r.Payments.Count);
        Assert.Equal((PaymentMethod.Card, Oct22, 0m), (r.PaymentMethod!.Value, r.PaymentCollectedAt!.Value, r.Remaining));
    }

    [Fact]
    public void Partial_at_delivery_is_paid_on_delivery_day_rest_unpaid()
    {
        var r = Open3450();
        r.DeliverPartly(1000m, PaymentMethod.Card, Sep3, MaintenancePolicy.Default, []);
        var p = Assert.Single(r.Payments);
        Assert.Equal((1000m, PaymentMethod.Card, Sep3, true), (p.Amount, p.Method, p.PaidOn, p.AtDelivery));
        Assert.Equal((Sep3, PaymentMethod.Unpaid, 2450m), (r.DeliveredAt!.Value, r.PaymentMethod!.Value, r.Remaining));
    }

    [Fact]
    public void Partial_at_delivery_equal_to_total_is_a_full_payment()
    {
        var r = Open3450();
        r.DeliverPartly(3450m, PaymentMethod.Transfer, Sep3, MaintenancePolicy.Default, []);
        Assert.Equal((PaymentMethod.Transfer, 0m, (DateOnly?)null), (r.PaymentMethod!.Value, r.Remaining, r.PaymentCollectedAt));
    }

    // ---- Kurallar: prototip odemeHatasi ----

    [Fact]
    public void More_than_remaining_is_refused_and_says_the_remaining()
    {
        var r = Veresiye();
        var e = Assert.Throws<DomainException>(() => r.CollectPayment(4000m, PaymentMethod.Cash, Sep3));
        Assert.Equal(("payment.over_remaining", "Kalan 3.450 ₺ — fazlası girilemez."), (e.Code, e.Message));
        Assert.Empty(r.Payments);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Zero_or_negative_is_refused(int amount)
        => Assert.Equal("payment.amount_required", Code(() => Veresiye().CollectPayment(amount, PaymentMethod.Cash, Sep3)));

    [Fact]
    public void Undelivered_record_takes_no_payment()
        => Assert.Equal("payment.not_unpaid", Code(() => Open3450().CollectPayment(10m, PaymentMethod.Cash, Sep3)));

    [Fact]
    public void Closed_record_takes_no_payment()
    {
        var r = Veresiye();
        r.CollectPayment(PaymentMethod.Cash, Sep3);
        Assert.Equal("payment.not_unpaid", Code(() => r.CollectPayment(1m, PaymentMethod.Cash, Sep3)));
    }

    [Fact]
    public void Unpaid_is_not_a_way_to_pay()
        => Assert.Equal("payment.method_required", Code(() => Veresiye().CollectPayment(10m, PaymentMethod.Unpaid, Sep3)));

    [Fact]
    public void Payment_cannot_be_before_delivery()
        => Assert.Equal("payment.before_delivery", Code(() => Veresiye().CollectPayment(10m, PaymentMethod.Cash, Aug28.AddDays(-1))));

    [Fact]
    public void Partial_at_delivery_more_than_total_is_refused_and_record_stays_open()
    {
        var r = Open3450();
        var e = Assert.Throws<DomainException>(() => r.DeliverPartly(5000m, PaymentMethod.Card, Sep3, MaintenancePolicy.Default, []));
        Assert.Equal(("payment.over_remaining", "Toplam 3.450 ₺ — fazlası girilemez."), (e.Code, e.Message));
        Assert.Null(r.DeliveredAt);   // prototip: yanlış girişte araç teslim edilmiş görünmez
    }

    [Fact]
    public void Partial_at_delivery_needs_an_amount_and_a_method()
    {
        Assert.Equal("payment.amount_required", Code(() => Open3450().DeliverPartly(0m, PaymentMethod.Card, Sep3, MaintenancePolicy.Default, [])));
        Assert.Equal("payment.method_required", Code(() => Open3450().DeliverPartly(10m, PaymentMethod.Unpaid, Sep3, MaintenancePolicy.Default, [])));
    }

    // ---- Geri alma: prototip sonOdemeyiGeriAl ----

    [Fact]
    public void Undo_removes_only_the_last_later_payment()
    {
        var r = Veresiye();
        r.CollectPayment(1000m, PaymentMethod.Cash, Sep3);
        r.CollectPayment(2450m, PaymentMethod.Card, Oct22);
        r.UndoCollection();
        Assert.Equal((1000m, 2450m, PaymentMethod.Unpaid), (r.Paid, r.Remaining, r.PaymentMethod!.Value));
        r.UndoCollection();
        Assert.Equal((0m, 3450m), (r.Paid, r.Remaining));
        Assert.Equal("payment.nothing_to_undo", Code(r.UndoCollection));
    }

    [Fact]
    public void Undo_does_not_touch_the_payment_taken_at_delivery()
    {
        var r = Open3450();
        r.DeliverPartly(1000m, PaymentMethod.Card, Sep3, MaintenancePolicy.Default, []);
        // Teslimdeki para teslimle gider (UndoDelivery) ya da yolu düzeltilir — "tahsili geri al" değil.
        Assert.Equal("payment.nothing_to_undo", Code(r.UndoCollection));
    }

    [Fact]
    public void Undo_delivery_takes_the_delivery_payment_with_it()
    {
        var r = Open3450();
        r.DeliverPartly(1000m, PaymentMethod.Card, Sep3, MaintenancePolicy.Default, []);
        r.UndoDelivery(Sep3);
        Assert.Empty(r.Payments);
        Assert.Equal(((DateOnly?)null, (PaymentMethod?)null), (r.DeliveredAt, r.PaymentMethod));
    }

    [Fact]
    public void Undo_delivery_after_a_later_installment_asks_to_undo_it_first()
    {
        var r = Open3450();
        r.DeliverPartly(1000m, PaymentMethod.Card, Sep3, MaintenancePolicy.Default, []);
        r.CollectPayment(500m, PaymentMethod.Cash, Sep3);   // aynı gün ikinci taksit — hâlâ kalan var
        Assert.Equal("delivery.undo_collection_first", Code(() => r.UndoDelivery(Sep3)));
    }

    // ---- Satır teslimden sonra değişirse: para bir olgudur ----

    [Fact]
    public void Line_change_after_delivery_changes_remaining_not_the_money_taken()
    {
        var r = Veresiye();
        r.CollectPayment(3000m, PaymentMethod.Transfer, Sep3);
        r.Lines[1].Update(LineKind.Labor, "Takma", 50m, null, null);   // 250 → 50: toplam 3.250
        Assert.Equal((3000m, 250m), (r.Paid, r.Remaining));
        r.Lines[0].Update(LineKind.Part, "Akü 60 Ah", 2000m, 2450m, PartSupplier.Shop);   // toplam 2.050 < alınan: kalan 0, eksi değil
        Assert.Equal((3000m, 0m), (r.Paid, r.Remaining));
    }

    // ---- Ödeme yolu düzeltme (E1) ödeme satırında ----

    [Fact]
    public void Correction_changes_the_last_payments_method_and_keeps_its_amount_and_day()
    {
        var r = Veresiye();
        r.CollectPayment(1000m, PaymentMethod.Cash, Sep3);
        r.CorrectPaymentMethod(PaymentMethod.Card);
        var p = Assert.Single(r.Payments);
        Assert.Equal((1000m, PaymentMethod.Card, Sep3), (p.Amount, p.Method, p.PaidOn));
        Assert.Equal(PaymentMethod.Unpaid, r.PaymentMethod);   // kalan hâlâ var
    }

    [Fact]
    public void Correcting_unpaid_delivery_to_paid_writes_the_total_on_delivery_day()
    {
        var r = Veresiye();
        r.CorrectPaymentMethod(PaymentMethod.Cash);   // "aslında teslimde almıştım"
        var p = Assert.Single(r.Payments);
        Assert.Equal((3450m, PaymentMethod.Cash, Aug28, true), (p.Amount, p.Method, p.PaidOn, p.AtDelivery));
        Assert.Equal((0m, (DateOnly?)null), (r.Remaining, r.PaymentCollectedAt));
    }

    [Fact]
    public void Correcting_paid_delivery_to_unpaid_removes_its_payment()
    {
        var r = Open3450();
        r.Deliver(PaymentMethod.Cash, Aug28, MaintenancePolicy.Default, []);
        r.CorrectPaymentMethod(PaymentMethod.Unpaid);   // "aslında almadım"
        Assert.Empty(r.Payments);
        Assert.Equal((PaymentMethod.Unpaid, 3450m), (r.PaymentMethod!.Value, r.Remaining));
    }
}
