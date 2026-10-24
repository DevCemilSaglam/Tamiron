using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// Plaka biçimi (prototip <c>plakaBicim</c> / <c>plakaGecerli</c>, K-059) ve iş türü
/// tahmini (<c>isTuruTahmin</c>, 06.09). Beklenen değerler prototipin davranışı.
/// </summary>
public class PlateAndTypeTests
{
    [Theory]
    [InlineData("34abc123", "34 ABC 123")]
    [InlineData(" 34 abc 123 ", "34 ABC 123")]
    [InlineData("06-tmr-07", "06 TMR 07")]
    [InlineData("34 şçi 12", "34 ŞÇİ 12")]        // Türkçe büyük harf: i → İ
    [InlineData("41rrh883", "41 RRH 883")]
    // K-077.9: harf sayısı rakam sayısını sınırlar — yazarken fazlası kesilir.
    [InlineData("34ABCD12345", "34 ABC 123")]      // 3 harf → en çok 3 rakam
    [InlineData("34a123456", "34 A 12345")]        // 1 harf → en çok 5 rakam
    [InlineData("34ab123456", "34 AB 1234")]       // 2 harf → en çok 4 rakam
    [InlineData("", "")]
    public void Normalize(string raw, string expected) => Assert.Equal(expected, Plate.Normalize(raw));

    [Theory]
    [InlineData("34 ABC 123", true)]
    [InlineData("06 TMR 07", true)]
    // K-077.9 · 1 harf → 4–5 rakam · 2 harf → 3–4 · 3 harf → 2–3 (ses çözümleyicisinin kuralı)
    [InlineData("34 A 12345", true)]               // eskiden reddediliyordu — gerçek plaka
    [InlineData("34 A 1234", true)]
    [InlineData("34 A 123", false)]
    [InlineData("34 A 12", false)]                 // eskiden geçiyordu
    [InlineData("34 AB 1234", true)]
    [InlineData("34 AB 123", true)]
    [InlineData("34 AB 12", false)]
    [InlineData("34 AB 12345", false)]
    [InlineData("34 ABC 1234", false)]             // eskiden geçiyordu — böyle plaka yok
    [InlineData("34 ABC 12", true)]
    [InlineData("34 ABC 1", false)]                // rakam en az 2
    [InlineData("34 AB 123\n", false)]             // .NET'te "$" son satır sonunu kabul eder — "\z" etmez
    [InlineData("3 ABC 123", false)]
    [InlineData("34 123", false)]
    public void IsValid(string plate, bool expected) => Assert.Equal(expected, Plate.IsValid(plate));

    [Theory]
    [InlineData("Periyodik bakım", ServiceType.Maintenance)]
    [InlineData("yağ ve filtre değişecek", ServiceType.Both)]   // "değiş" tamirat kökü, "yağ" bakım
    [InlineData("debriyaj kaçırıyor", ServiceType.Repair)]
    [InlineData("Akü", ServiceType.Repair)]                       // tanınmayan → tamirat (güvenli taraf)
    [InlineData("", ServiceType.Repair)]
    [InlineData("SERVİS", ServiceType.Maintenance)]
    public void Guess_type_from_text(string text, ServiceType expected)
        => Assert.Equal(expected, ServiceTypeGuess.FromText(text));
}
