using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// C1 · İş başına fark — prototip <c>isKari</c> / <c>alisBilinmiyor</c> (K-055.2, K-057):
/// alışı bilinmeyen parça SATIŞIYLA BİRLİKTE hesap dışı kalır; eksik rakam yanlış rakamdan iyidir.
/// </summary>
public class JobProfitTests
{
    static ServiceRecord R(params ServiceLine[] lines)
    {
        var r = new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), "iş");
        foreach (var l in lines) r.AddLine(l);
        return r;
    }

    [Fact]
    public void Prototype_example_unknown_cost_leaves_with_its_sale()
    {
        // Prototip id 7: "eski hesap 2.800 ₺ fark yazardı, doğrusu 700 ₺ + bir parçanın alışı bilinmiyor".
        var p = JobProfit.Of(R(ServiceLine.Part("Silecek motoru", 2100), ServiceLine.Labor("Söküm takım", 700)));
        Assert.Equal((2800m, 0m, 2100m, 1, 700m), (p.Received, p.PartsCost, p.Outside, p.UnknownCount, p.Difference));
    }

    [Fact]
    public void Known_cost_is_subtracted()
    {
        // Prototip id 6: yağ 1.950 (alış 1.500) + işçilik 900 → 2.850 − 1.500 = 1.350.
        var p = JobProfit.Of(R(ServiceLine.Part("Motor yağı", 1950, cost: 1500), ServiceLine.Labor("Bakım işçiliği", 900)));
        Assert.Equal((2850m, 1500m, 0, 1350m), (p.Received, p.PartsCost, p.UnknownCount, p.Difference));
    }

    [Fact]
    public void Free_part_is_known_zero_not_unknown()
        => Assert.Equal(0, JobProfit.Of(R(ServiceLine.Part("Conta", 50, cost: 0))).UnknownCount);   // 0 = bedava (K-057)

    [Fact]
    public void Customer_part_and_unpriced_part_do_not_count()
    {
        var p = JobProfit.Of(R(ServiceLine.Part("Disk", 2000, PartSupplier.Customer), ServiceLine.Part("Triger", 0), ServiceLine.Labor("İşçilik", 500)));
        Assert.Equal((500m, 0m, 0m, 0, 500m), (p.Received, p.PartsCost, p.Outside, p.UnknownCount, p.Difference));
    }
}
