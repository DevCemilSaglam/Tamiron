using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// Parça sormak için araç kimliği (K-068). Beklenen davranış prototipin
/// <c>aracKimligiTamam</c> / <c>saseGecerli</c> fonksiyonları (02-veri.js).
/// </summary>
public class VehicleIdentityTests
{
    [Theory]
    [InlineData("VF1RFB00858123456", true)]      // 17 hane
    [InlineData("vf1-rfb0 0858123456", true)]    // küçük harf + ayraç: temizlenince 17
    [InlineData("VF1RFB0085812345", false)]      // 16 hane
    [InlineData("VF1RFB00858123ŞŞŞ", false)]     // Türkçe harf sayılmaz → 14
    public void Vin_alone_is_enough_when_it_has_17_characters(string vin, bool expected)
        => Assert.Equal(expected, new Vehicle { Vin = vin }.HasPartIdentity);

    [Fact]
    public void Without_vin_brand_model_year_engine_all_required()
    {
        var v = new Vehicle { Brand = "Fiat", Model = "Egea", Year = "2021", Engine = "1.4 Fire" };
        Assert.True(v.HasPartIdentity);
        v.Engine = " ";
        Assert.False(v.HasPartIdentity);
    }

    [Fact]
    public void Empty_vehicle_has_no_identity()
        => Assert.False(new Vehicle { PlateNo = "34 ABC 123" }.HasPartIdentity);
}
