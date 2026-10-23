using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>D1 · Parçacı (VERI-MODELI §4): iki alan — ad ve telefon. Soru WhatsApp'la gider (T-12b).</summary>
public class SupplierTests
{
    [Fact]
    public void Name_and_phone_normalized()
    {
        var s = Supplier.Create(Guid.NewGuid(), "  Öz Yedek Parça ", "05321112233");
        Assert.Equal(("Öz Yedek Parça", "0532 111 22 33"), (s.Name, s.Phone));
    }

    [Theory]
    [InlineData("", "05321112233", "supplier.name_required")]
    [InlineData("Öz Yedek", "", "supplier.phone_required")]
    [InlineData("Öz Yedek", "532", "vehicle.phone_invalid")]   // aynı telefon kuralı (VehicleFields.Phone)
    public void Name_and_phone_required(string name, string phone, string code)
        => Assert.Equal(code, Assert.Throws<DomainException>(() => Supplier.Create(Guid.NewGuid(), name, phone)).Code);
}
