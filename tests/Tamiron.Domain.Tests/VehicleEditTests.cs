using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// B6 · Araç alanları — prototip GIRIS_TEMIZ (tel · kod · yil) ve yilGecerli / telGecerli
/// (01-cekirdek.js), K-033.2c (ruhsat alanları girilebilir).
/// </summary>
public class VehicleEditTests
{
    [Theory]
    [InlineData("05322153000", "0532 215 30 00")]
    [InlineData("0532-215-30-00", "0532 215 30 00")]
    [InlineData("0532 215 30 00 99", "0532 215 30 00")]   // 11 haneden fazlası atılır
    [InlineData("", null)]
    public void Phone_is_grouped(string raw, string? expected) => Assert.Equal(expected, VehicleFields.Phone(raw));

    [Theory]
    [InlineData("+90 532 215 30 00")]   // ülke kodu kabul edilmiyor: tek biçim
    [InlineData("532 215 30 00")]       // 10 hane
    public void Invalid_phone_is_rejected(string raw)
        => Assert.Equal("vehicle.phone_invalid", Assert.Throws<DomainException>(() => VehicleFields.Phone(raw)).Code);

    [Theory]
    [InlineData("vf1-rfb 00858123456", "VF1RFB00858123456")]
    [InlineData("k9k872c123456", "K9K872C123456")]
    [InlineData("843a1000098765ŞĞÜ", "843A1000098765ŞĞÜ")]
    [InlineData("", null)]
    public void Code_is_upper_and_bare(string raw, string? expected) => Assert.Equal(expected, VehicleFields.Code(raw));

    [Fact]
    public void Code_is_capped_at_24_not_17()
        => Assert.Equal(24, VehicleFields.Code(new string('A', 30))!.Length);

    [Theory]
    [InlineData("2018", 2026, "2018")]
    [InlineData("1950", 2026, "1950")]
    [InlineData("2027", 2026, "2027")]   // gelecek yılın modeli satılıyor
    public void Year_in_range(string raw, int thisYear, string expected) => Assert.Equal(expected, VehicleFields.Year(raw, thisYear));

    [Theory]
    [InlineData("1899")]
    [InlineData("2028")]
    [InlineData("18")]
    public void Year_out_of_range(string raw)
        => Assert.Equal("vehicle.year_invalid", Assert.Throws<DomainException>(() => VehicleFields.Year(raw, 2026)).Code);

    [Fact]
    public void Missing_for_parts_lists_what_to_fill()
    {
        var v = new Vehicle { Brand = "Fiat", Year = "2021" };
        Assert.Equal(new[] { "model", "motor" }, v.MissingForParts);
        v.Vin = "NM4358004ZT129041";
        Assert.Empty(v.MissingForParts);   // şase tek başına yeter (K-068)
    }
}
