using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// B3 · Satırlar — prototip 30-kayit: satır ekleme (kYeniEkle), tür çevirme
/// (data-satir-tip), kim getirdi (kGetireniAyarla), alış null ≠ 0 (K-057).
/// </summary>
public class ServiceLineTests
{
    static ServiceRecord NewRecord() => new(Guid.NewGuid(), Guid.NewGuid(), ServiceType.Repair, "");

    [Fact]
    public void Name_is_required_with_turkish_hint()
    {
        var e = Assert.Throws<DomainException>(() => ServiceLine.Part("  ", 100));
        Assert.Equal("line.name_required", e.Code);
        Assert.Contains("balata", e.Message);
    }

    [Fact]
    public void Zero_price_is_valid_negative_is_not()
    {
        Assert.Equal(0m, ServiceLine.Part("Triger seti", 0).Price);   // fiyatı henüz gelmemiş parça
        Assert.Throws<DomainException>(() => ServiceLine.Labor("İşçilik", -1));
    }

    [Fact]
    public void Positions_follow_insertion_order()
    {
        var r = NewRecord();
        r.AddLine(ServiceLine.Part("A", 1));
        r.AddLine(ServiceLine.Labor("B", 2));
        r.AddLine(ServiceLine.Part("C", 3));
        Assert.Equal(new[] { 1, 2, 3 }, r.Lines.Select(l => l.Position));
    }

    [Fact]
    public void Turning_part_into_labor_keeps_name_and_price_clears_part_fields()
    {
        var l = ServiceLine.Part("Balata", 1200, PartSupplier.Customer, cost: 900);
        l.Update(LineKind.Labor, "Balata", 1200, cost: 900, suppliedBy: PartSupplier.Customer);
        Assert.Equal(LineKind.Labor, l.Kind);
        Assert.Equal(("Balata", 1200m), (l.Name, l.Price));
        Assert.Null(l.SuppliedBy);   // işçiliğin parçacısı olmaz
        Assert.Null(l.Cost);
    }

    [Fact]
    public void Turning_labor_into_part_defaults_supplier_to_shop()
    {
        var l = ServiceLine.Labor("Takma", 250);
        l.Update(LineKind.Part, "Takma", 250, cost: null, suppliedBy: null);
        Assert.Equal(PartSupplier.Shop, l.SuppliedBy);
        Assert.Null(l.Cost);   // bilinmiyor, 0 değil
    }

    [Fact]
    public void Customer_supplied_keeps_price_and_cost_only_total_excludes_it()
    {
        var r = NewRecord();
        var l = ServiceLine.Part("Akü", 3200, cost: 2450);
        r.AddLine(l);
        l.Update(LineKind.Part, "Akü", 3200, cost: 2450, suppliedBy: PartSupplier.Customer);
        Assert.Equal((3200m, (decimal?)2450m), (l.Price, l.Cost));   // K-037: veri silinmez
        Assert.Equal(0m, r.Total);
    }
}
