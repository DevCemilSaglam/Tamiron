using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// G7 · Müşteri avansı (K-079.1, TASARIM-G-AVANS §2). Bakiye saklanmaz: alınanlar − iadeler −
/// avanstan düşülenler. Eksiye düşmez. Avanstan düşülen ödemenin yolu düzeltilmez.
/// </summary>
public class AdvanceTests
{
    static readonly Guid Shop = Guid.NewGuid();
    static readonly Guid Car = Guid.NewGuid();
    static readonly DateOnly Day = new(2026, 10, 27);

    static Advance Al(decimal t, PaymentMethod m = PaymentMethod.Cash) => Advance.Receive(Shop, Car, t, m, Day);

    [Fact]
    public void Balance_is_received_minus_refunded_minus_used()
    {
        var hareketler = new[] { Al(1000), Al(500, PaymentMethod.Card), Advance.Refund(Shop, Car, 200, PaymentMethod.Cash, Day, balance: 1500) };
        Assert.Equal(1300m, AdvanceBalance.Of(hareketler, usedInPayments: 0));
        Assert.Equal(900m, AdvanceBalance.Of(hareketler, usedInPayments: 400));
    }

    [Fact]
    public void Deleted_movements_do_not_count()
    {
        var a = Al(1000);
        a.DateTimeDeleted = DateTime.UtcNow;
        Assert.Equal(0m, AdvanceBalance.Of([a, Al(300)], 0) - 300m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Amount_must_be_positive(decimal t)
    {
        var e = Assert.Throws<DomainException>(() => Al(t));
        Assert.Equal("advance.amount_required", e.Code);
    }

    [Theory]
    [InlineData(PaymentMethod.Unpaid)]
    [InlineData(PaymentMethod.Advance)]
    public void Only_real_money_ways(PaymentMethod m)
    {
        var e = Assert.Throws<DomainException>(() => Al(100, m));
        Assert.Equal("advance.method_required", e.Code);
    }

    [Fact]
    public void Refund_cannot_exceed_the_balance()
    {
        var e = Assert.Throws<DomainException>(() => Advance.Refund(Shop, Car, 600, PaymentMethod.Cash, Day, balance: 500));
        Assert.Equal("advance.over_balance", e.Code);
        Assert.Equal("Avans 500 ₺ — fazlası iade edilemez.", e.Message);
        Assert.Equal(AdvanceKind.Refunded, Advance.Refund(Shop, Car, 500, PaymentMethod.Cash, Day, balance: 500).Kind);
    }

    [Fact]
    public void Undoing_a_receipt_may_not_push_the_balance_below_zero()
    {
        var a = Al(1000);
        // Bakiye 1000 ama 700'ü bir işe düşülmüş → kalan bakiye 300: 1000'lik alışı silmek eksiye düşürür.
        var e = Assert.Throws<DomainException>(() => a.Undo(balance: 300));
        Assert.Equal("advance.undo_would_go_negative", e.Code);
        a.Undo(balance: 1000);
        Assert.NotNull(a.DateTimeDeleted);
    }

    [Fact]
    public void Undoing_a_refund_only_raises_the_balance()
    {
        var r = Advance.Refund(Shop, Car, 200, PaymentMethod.Cash, Day, balance: 500);
        r.Undo(balance: 0);
        Assert.NotNull(r.DateTimeDeleted);
    }

    [Fact]
    public void Use_from_balance_is_limited_by_the_balance()
    {
        var e = Assert.Throws<DomainException>(() => AdvanceBalance.EnsureCovers(balance: 400, amount: 500));
        Assert.Equal("advance.over_balance", e.Code);
        Assert.Equal("Avans 400 ₺ — fazlası düşülemez.", e.Message);
        AdvanceBalance.EnsureCovers(400, 400);
    }

    static ServiceRecord Teslimli(decimal toplam, PaymentMethod m)
    {
        var r = new ServiceRecord(Shop, Car, "İş");
        r.AddLine(ServiceLine.Labor("İşçilik", toplam));
        r.Deliver(m, Day, MaintenancePolicy.Default, []);
        return r;
    }

    [Fact]
    public void Payment_from_advance_closes_the_job_like_any_other_way()
    {
        var r = Teslimli(1000, PaymentMethod.Unpaid);
        r.CollectPayment(600, PaymentMethod.Advance, Day);
        Assert.Equal(400m, r.Remaining);
        r.CollectPayment(400, PaymentMethod.Cash, Day);
        Assert.Equal(0m, r.Remaining);
        Assert.Equal(600m, r.Payments.Where(p => p.Method == PaymentMethod.Advance).Sum(p => p.Amount));
    }

    [Theory]
    [InlineData(PaymentMethod.Advance, PaymentMethod.Cash)]
    [InlineData(PaymentMethod.Cash, PaymentMethod.Advance)]
    public void Way_from_or_to_advance_is_not_corrected(PaymentMethod was, PaymentMethod to)
    {
        var r = Teslimli(1000, was);
        var e = Assert.Throws<DomainException>(() => r.CorrectPaymentMethod(to));
        Assert.Equal("payment.advance_fixed", e.Code);
    }

    [Fact]
    public void Unpaid_job_is_not_corrected_into_advance()
    {
        var r = Teslimli(1000, PaymentMethod.Unpaid);
        var e = Assert.Throws<DomainException>(() => r.CorrectPaymentMethod(PaymentMethod.Advance));
        Assert.Equal("payment.advance_fixed", e.Code);
    }
}
