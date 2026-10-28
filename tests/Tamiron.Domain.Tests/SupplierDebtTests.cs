using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// H3 · Parçacıya borç (K-090, TASARIM-H-MUHASEBE §2). Borç yalnız ustanın "veresiye aldım" dediği parçadan
/// doğar; saklanmaz: veresiye alışlar − ödemeler. Veresiye yalnız tamircinin aldığı, alışı yazılı, parçacısı
/// belli parçada. Ödeme ≤ borç; ödeme gider değildir.
/// </summary>
public class SupplierDebtTests
{
    static readonly Guid Shop = Guid.NewGuid();
    static readonly Guid Oz = Guid.NewGuid();
    static readonly Guid Can = Guid.NewGuid();
    static readonly DateOnly Day = new(2026, 10, 28);

    static ServiceLine Veresiye(decimal alis, Guid parcaci)
    {
        var l = ServiceLine.Part("Balata", 1500, cost: alis);
        l.SetPurchase(parcaci, onCredit: true);
        return l;
    }

    [Fact]
    public void Debt_is_credit_purchases_minus_payments_per_supplier()
    {
        var satirlar = new[] { Veresiye(1200, Oz), Veresiye(800, Oz), Veresiye(500, Can) };
        var odemeler = new[] { SupplierPayment.Pay(Shop, Oz, 700, PaymentMethod.Cash, Day, debt: 2000) };
        Assert.Equal(1300m, SupplierDebt.Of(satirlar, odemeler, Oz));
        Assert.Equal(500m, SupplierDebt.Of(satirlar, odemeler, Can));
    }

    [Fact]
    public void Paid_on_the_spot_is_not_a_debt()
    {
        // Varsayılan "ödedim": usta işaretlemediyse borç doğmaz — eksik söyler, uydurmaz (K-013).
        var l = ServiceLine.Part("Balata", 1500, cost: 1200);
        l.SetPurchase(Oz, onCredit: false);
        Assert.Equal(0m, SupplierDebt.Of([l], [], Oz));
        Assert.Equal(Oz, l.SupplierId);
    }

    [Fact]
    public void Deleted_lines_and_payments_do_not_count()
    {
        var silinen = Veresiye(1200, Oz);
        silinen.DateTimeDeleted = DateTime.UtcNow;
        var odeme = SupplierPayment.Pay(Shop, Oz, 300, PaymentMethod.Cash, Day, debt: 800);
        odeme.Undo();
        Assert.Equal(800m, SupplierDebt.Of([silinen, Veresiye(800, Oz)], [odeme], Oz));
    }

    [Theory]
    [InlineData("no_cost")]
    [InlineData("customer")]
    [InlineData("labor")]
    [InlineData("no_supplier")]
    public void Credit_needs_a_shop_bought_part_with_cost_and_supplier(string durum)
    {
        var l = durum switch
        {
            "no_cost" => ServiceLine.Part("Balata", 1500),
            "customer" => ServiceLine.Part("Balata", 0, PartSupplier.Customer, cost: 900),
            "labor" => ServiceLine.Labor("İşçilik", 500),
            _ => ServiceLine.Part("Balata", 1500, cost: 1200),
        };
        var ex = Assert.Throws<DomainException>(() => l.SetPurchase(durum == "no_supplier" ? null : Oz, onCredit: true));
        Assert.Equal("line.credit_not_possible", ex.Code);
        Assert.False(l.OnCredit);
    }

    [Fact]
    public void Line_turning_into_customer_part_or_labor_drops_the_credit()
    {
        // Parçayı müşteri getirdiyse para tamirciden çıkmamıştır (K-005) — borç da yoktur.
        var l = Veresiye(1200, Oz);
        l.Update(LineKind.Part, "Balata", 0, 1200, PartSupplier.Customer);
        Assert.False(l.OnCredit);
        var m = Veresiye(1200, Oz);
        m.Update(LineKind.Labor, "Balata", 500, null, null);
        Assert.False(m.OnCredit);
        Assert.Null(m.SupplierId);
    }

    [Fact]
    public void Clearing_the_cost_drops_the_credit()
    {
        var l = Veresiye(1200, Oz);
        l.Update(LineKind.Part, "Balata", 1500, null, PartSupplier.Shop);
        Assert.False(l.OnCredit);
        Assert.Equal(Oz, l.SupplierId);   // kimden alındığı bilgisi kalır
    }

    [Fact]
    public void Reply_marks_where_the_part_came_from()
    {
        var l = ServiceLine.Part("Balata", 0);
        var cevap = PriceReply.Create(Guid.NewGuid(), "1200", 1200, null, null, DateTime.UtcNow);
        l.ApplyReply(cevap, customerPrice: 1500, supplierId: Can, onCredit: true);
        Assert.Equal(Can, l.SupplierId);
        Assert.True(l.OnCredit);
        Assert.Equal(1200m, l.Cost);
    }

    [Fact]
    public void Payment_cannot_exceed_the_debt_nor_be_empty_nor_unpaid()
    {
        Assert.Equal("supplier_payment.over_debt",
            Assert.Throws<DomainException>(() => SupplierPayment.Pay(Shop, Oz, 1001, PaymentMethod.Cash, Day, debt: 1000)).Code);
        Assert.Equal("supplier_payment.amount_required",
            Assert.Throws<DomainException>(() => SupplierPayment.Pay(Shop, Oz, 0, PaymentMethod.Cash, Day, debt: 1000)).Code);
        Assert.Equal("supplier_payment.method_required",
            Assert.Throws<DomainException>(() => SupplierPayment.Pay(Shop, Oz, 100, PaymentMethod.Unpaid, Day, debt: 1000)).Code);
        Assert.Equal("supplier_payment.method_required",
            Assert.Throws<DomainException>(() => SupplierPayment.Pay(Shop, Oz, 100, PaymentMethod.Advance, Day, debt: 1000)).Code);
    }
}
