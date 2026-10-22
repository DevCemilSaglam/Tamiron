using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// B9 · CSV — prototip <c>csvHucre</c> / <c>muhasebeCsv</c> (K-071.2) ve onun
/// 60. denetim testi (DISA-AKTARMA): BOM, ";" ayraç, ham rakam, kaçış, formül koruması.
/// </summary>
public class CsvTests
{
    [Theory]
    [InlineData(3450, "3450")]          // biçimli "3.450" Excel'de METİN olur, toplanamaz
    [InlineData(12.5, "12,5")]          // Türkçe Excel'de ondalık virgül
    [InlineData(0, "0")]
    public void Numbers_are_raw(decimal v, string expected) => Assert.Equal(expected, Csv.Cell(v));

    [Theory]
    [InlineData("Elektrik", "Elektrik")]
    [InlineData("=HAT; \"deneme\"", "\"'=HAT; \"\"deneme\"\"\"")]   // prototip testinin birebir beklediği
    [InlineData("+90 532", "'+90 532")]
    [InlineData("-iade", "'-iade")]
    [InlineData("@x", "'@x")]
    [InlineData("iki\nsatır", "\"iki\nsatır\"")]
    [InlineData(null, "")]
    public void Text_is_escaped_and_formula_guarded(string? v, string expected) => Assert.Equal(expected, Csv.Cell(v));

    [Fact]
    public void Document_starts_with_bom_uses_crlf_and_ends_with_newline()
    {
        var s = Csv.Document([["Tür", "Tutar"], ["Gider", 2400m]]);
        Assert.Equal('\uFEFF', s[0]);
        Assert.Equal("\uFEFFTür;Tutar\r\nGider;2400\r\n", s);
    }

    [Fact]
    public void Dates_are_turkish()
        => Assert.Equal("01.09.2026", Csv.Cell(new DateOnly(2026, 9, 1)));
}
