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
    [InlineData("34ABCD12345", "34 ABC 1234")]     // harf 3'te, rakam 4'te kesilir
    [InlineData("", "")]
    public void Normalize(string raw, string expected) => Assert.Equal(expected, Plate.Normalize(raw));

    [Theory]
    [InlineData("34 ABC 123", true)]
    [InlineData("06 TMR 07", true)]
    [InlineData("34 A 12", true)]
    [InlineData("34 ABC 1", false)]                // rakam en az 2
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
