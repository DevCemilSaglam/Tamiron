using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// Kayıt toplamı (K-037) ve tahsil günü (K-071.1). Beklenen değerler prototipin
/// <c>kayitToplami</c> ve <c>tahsilTarihi</c> fonksiyonlarının davranışıdır
/// (prototypes/kaynak/betik/02-veri.js) — backend o davranışı korur (K-073).
/// </summary>
public class ServiceRecordMoneyTests
{
    static readonly DateOnly Aug28 = new(2026, 8, 28);
    static readonly DateOnly Oct22 = new(2026, 10, 22);

    static ServiceRecord Record(params ServiceLine[] lines)
    {
        var r = new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), "Akü değişimi");
        foreach (var l in lines) r.AddLine(l);
        return r;
    }

    static ServiceLine Part(decimal price, PartSupplier by = PartSupplier.Shop, decimal? cost = null)
        => ServiceLine.Part("Parça", price, by, cost);

    static ServiceLine Labor(decimal price) => ServiceLine.Labor("İşçilik", price);

    // ---- Toplam (K-037) ----

    [Fact]
    public void Total_sums_parts_and_labor()
        => Assert.Equal(4200m, Record(Part(3200m), Labor(1000m)).Total);

    [Fact]
    public void Total_excludes_customer_supplied_part_but_keeps_its_labor()
        => Assert.Equal(1000m, Record(Part(3200m, PartSupplier.Customer), Labor(1000m)).Total);

    [Fact]
    public void Customer_part_keeps_its_price_written_it_is_only_excluded_from_total()
    {
        // Eleme okuma tarafında; veri silinmez (K-037 — "bir arayüz düğmesi veriyi silmez").
        var r = Record(Part(3200m, PartSupplier.Customer));
        Assert.Equal(3200m, r.Lines.Single().Price);
        Assert.Equal(0m, r.Total);
    }

    [Fact]
    public void Unknown_cost_is_null_not_zero()
        => Assert.Null(Part(3200m, cost: null).Cost);   // K-057: null = bilinmiyor, 0 = bedava

    // ---- Tahsil günü (K-071.1) ----

    [Fact]
    public void Not_delivered_has_no_collection_day()
        => Assert.Null(Record(Part(3200m)).CollectedOn);

    [Fact]
    public void Paid_at_delivery_is_collected_on_delivery_day()
    {
        var r = Record(Part(3200m));
        r.Deliver(PaymentMethod.Cash, Aug28);
        Assert.Equal(Aug28, r.CollectedOn);
        Assert.Null(r.PaymentCollectedAt);   // teslimde ödendiyse ayrı tarih tutulmaz
    }

    [Fact]
    public void Unpaid_has_no_collection_day()
    {
        var r = Record(Part(3200m));
        r.Deliver(PaymentMethod.Unpaid, Aug28);
        Assert.Null(r.CollectedOn);
    }

    [Fact]
    public void Unpaid_collected_later_counts_on_the_day_money_came_in()
    {
        // Ağustos'ta teslim, Ekim'de ödendi: Ekim'in kasası (prototipteki örnek, 06 TMR 07).
        var r = Record(Part(3450m));
        r.Deliver(PaymentMethod.Unpaid, Aug28);
        r.CollectPayment(PaymentMethod.Card, Oct22);
        Assert.Equal(PaymentMethod.Card, r.PaymentMethod);
        Assert.Equal(Oct22, r.CollectedOn);
    }

    [Fact]
    public void Undo_collection_returns_to_unpaid()
    {
        var r = Record(Part(3450m));
        r.Deliver(PaymentMethod.Unpaid, Aug28);
        r.CollectPayment(PaymentMethod.Cash, Oct22);
        r.UndoCollection();
        Assert.Equal(PaymentMethod.Unpaid, r.PaymentMethod);
        Assert.Null(r.PaymentCollectedAt);
        Assert.Null(r.CollectedOn);
    }

    [Fact]
    public void Cannot_collect_a_record_that_is_not_unpaid()
    {
        var r = Record(Part(3200m));
        r.Deliver(PaymentMethod.Cash, Aug28);
        var e = Assert.Throws<DomainException>(() => r.CollectPayment(PaymentMethod.Card, Oct22));
        Assert.Equal("payment.not_unpaid", e.Code);
    }

    [Fact]
    public void Cannot_collect_as_unpaid()
    {
        var r = Record(Part(3200m));
        r.Deliver(PaymentMethod.Unpaid, Aug28);
        Assert.Throws<DomainException>(() => r.CollectPayment(PaymentMethod.Unpaid, Oct22));
    }

    [Fact]
    public void Cannot_collect_before_delivery_day()
    {
        var r = Record(Part(3200m));
        r.Deliver(PaymentMethod.Unpaid, Aug28);
        Assert.Throws<DomainException>(() => r.CollectPayment(PaymentMethod.Cash, Aug28.AddDays(-1)));
    }
}
