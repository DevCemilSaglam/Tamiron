using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// C6 · KDV/tevkifat için YER (MUHASEBE-KAPSAM Faz 1, KDV.md). Hesap YOK — yalnız alan ve
/// "oran satırda donar" kuralı: oran devlet kararıyla değişir, geçmiş iş kendi oranıyla kalır.
/// </summary>
public class VatPlaceholderTests
{
    [Fact]
    public void Shop_default_vat_is_20()
        => Assert.Equal(20m, new Shop().DefaultVatRate);

    [Fact]
    public void Line_vat_freezes_at_first_value()
    {
        var l = ServiceLine.Part("Balata", 1200);
        Assert.Null(l.VatRate);
        l.FreezeVatRate(20m);
        l.FreezeVatRate(10m);          // sonradan gelen oran eskisini EZMEZ
        Assert.Equal(20m, l.VatRate);
    }

    [Fact]
    public void Placeholders_start_empty()
    {
        Assert.Null(new Vehicle().OwnerType);
        Assert.Null(new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), "x").WithholdingRate);
    }
}
