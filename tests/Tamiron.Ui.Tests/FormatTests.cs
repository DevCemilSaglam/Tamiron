using System.Globalization;
using Tamiron.Ui.Core;

namespace Tamiron.Ui.Tests;

/// <summary>
/// Para biçimi tarayıcının dilinden BAĞIMSIZ (K-076.19). Tarayıcıda ölçülerek bulundu: İngilizce
/// tarayıcıda Blazor WASM tr-TR verisini yüklemiyor, "3.450 ₺" → "3,450 ₺" oluyordu. Burada
/// iş parçacığının kültürü İngilizce yapılıp aynı durum taklit edilir.
/// </summary>
public class FormatTests
{
    [Theory]
    [InlineData(3450, "3.450")]
    [InlineData(1234.5, "1.234,5")]
    [InlineData(0, "0")]
    [InlineData(1000000, "1.000.000")]
    public void Money_is_turkish_whatever_the_browser_language(decimal n, string expected)
    {
        var old = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            Assert.Equal(expected, Format.Money(n));
        }
        finally { CultureInfo.CurrentCulture = old; }
    }

    [Fact]
    public void Money_does_not_read_culture_data()
    {
        // Sunucuda tr-TR verisi tam; WASM'daki eksikliği taklit etmenin dürüst yolu biçimin
        // kültür verisine HİÇ başvurmaması — kaynakta "tr-TR" adı geçmemeli.
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Tamiron.sln"))) dir = dir.Parent;
        var src = File.ReadAllText(Path.Combine(dir!.FullName, "src", "Tamiron.Ui.Core", "Api.cs"));
        Assert.DoesNotContain("GetCultureInfo(\"tr-TR\")", src);
    }
}
