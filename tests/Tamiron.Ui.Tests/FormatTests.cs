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

    /// <summary>G6 · Dönem çipinin adı (prototip AYLAR). Mutasyonla bulundu: ekran testi aynı fonksiyonla karşılaştırıyordu.</summary>
    [Theory]
    [InlineData(1, "Ocak")]
    [InlineData(10, "Ekim")]
    [InlineData(12, "Aralık")]
    public void Month_names_are_turkish(int month, string expected)
        => Assert.Equal(expected, Format.MonthName(new DateTime(2026, month, 15)));

    // G9c · Ustanın kendi uygulaması açılır (WHATSAPP.md A yolu): wa.me ülke koduyla ister, sms/tel rakam.
    [Theory]
    [InlineData("0532 111 22 33", "905321112233")]
    [InlineData("532 111 22 33", "905321112233")]
    [InlineData("+90 532 111 22 33", "905321112233")]
    [InlineData("90 532 111 22 33", "905321112233")]
    public void WhatsApp_number_carries_the_country_code(string tel, string expected)
        => Assert.Equal($"https://wa.me/{expected}?text=Merhaba%20%26%20ho%C5%9F%C3%A7a%20kal%0A", Format.WhatsAppLink(tel, "Merhaba & hoşça kal\n"));

    [Fact]
    public void Sms_and_call_links_keep_the_local_number()
    {
        Assert.Equal("sms:05321112233?body=Ara%C3%A7%20haz%C4%B1r", Format.SmsLink("0532 111 22 33", "Araç hazır"));
        Assert.Equal("tel:05321112233", Format.TelLink("0532 111 22 33"));
    }
}
