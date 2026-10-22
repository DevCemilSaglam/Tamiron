using System.Globalization;

namespace Tamiron.Domain;

/// <summary>
/// Araç alanlarının biçimi ve geçerliliği — prototipin GIRIS_TEMIZ (tel · kod · yil)
/// ve yilGecerli / telGecerli'si. Boş girdi = alan boş (null); geçersiz dolu girdi
/// reddedilir. Prototip geçerliliği "alan dolunca" soruyordu; API'ye gelen değer
/// dolmuş değerdir.
/// </summary>
public static class VehicleFields
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");
    private const string TrLetters = "ÇĞİÖŞÜ";

    /// <summary>"05322153000" → "0532 215 30 00". 11 hane, 0 ile başlar; +90 kabul edilmez (tek biçim).</summary>
    public static string? Phone(string? raw)
    {
        var d = new string((raw ?? "").Where(char.IsAsciiDigit).ToArray());
        if (d.Length == 0) return null;
        if (d.Length > 11) d = d[..11];
        if (d.Length != 11 || d[0] != '0' || (raw ?? "").TrimStart().StartsWith('+'))
            throw new DomainException("vehicle.phone_invalid", "Telefonu 0 ile başlayan 11 hane olarak yaz — ör. 0532 215 30 00.");
        return $"{d[..4]} {d[4..7]} {d[7..9]} {d[9..11]}";
    }

    /// <summary>Şase / motor no: büyük harf, yalnız harf ve rakam, en çok 24. 17 sınırı YOK — motor no da bunu kullanır.</summary>
    public static string? Code(string? raw)
    {
        var s = new string((raw ?? "").ToUpper(Tr)
            .Where(c => c is >= 'A' and <= 'Z' or >= '0' and <= '9' || TrLetters.Contains(c)).Take(24).ToArray());
        return s.Length == 0 ? null : s;
    }

    /// <summary>Model yılı: 1950 ile gelecek yıl arası (gelecek yılın modeli satılıyor).</summary>
    public static string? Year(string? raw, int thisYear)
    {
        var d = new string((raw ?? "").Where(char.IsAsciiDigit).Take(4).ToArray());
        if (d.Length == 0) return null;
        if (d.Length != 4 || int.Parse(d) is var y && (y < 1950 || y > thisYear + 1))
            throw new DomainException("vehicle.year_invalid", $"Model yılı 1950 ile {thisYear + 1} arasında olmalı.");
        return d;
    }

    /// <summary>Serbest metin: kırpılır, boşsa yok.</summary>
    public static string? Text(string? raw) => string.IsNullOrWhiteSpace(raw) ? null : raw.Trim();
}
