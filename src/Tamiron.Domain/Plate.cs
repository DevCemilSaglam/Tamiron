using System.Globalization;
using System.Text.RegularExpressions;

namespace Tamiron.Domain;

/// <summary>
/// Plaka biçimi — prototipin <c>plakaBicim</c> / <c>plakaGecerli</c>'si (K-059).
/// "34abc123" → "34 ABC 123": il (2 rakam) · harf (en çok 3) · rakam (en çok 4).
/// Türkçe büyük harf: "i" → "İ" (tr-TR), prototipte toLocaleUpperCase('tr').
/// </summary>
public static partial class Plate
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");
    private const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZÇĞİÖŞÜ";

    public static string Normalize(string? raw)
    {
        var t = new string((raw ?? "").ToUpper(Tr).Where(c => char.IsAsciiDigit(c) || Letters.Contains(c)).ToArray());
        var il = new string(t.Take(2).Where(char.IsAsciiDigit).ToArray());
        var rest = t[il.Length..];
        var harf = new string(rest.Where(c => Letters.Contains(c)).Take(3).ToArray());
        // Prototip: harfler kalan metnin BAŞINDAN değil, içinden toplanır; rakam,
        // harf sayısı kadar atlandıktan sonraki rakamlardır.
        var rakam = new string(rest[harf.Length..].Where(char.IsAsciiDigit).Take(4).ToArray());
        return string.Join(' ', new[] { il, harf, rakam }.Where(s => s.Length > 0));
    }

    public static bool IsValid(string? plate) => plate is not null && Pattern().IsMatch(plate);

    [GeneratedRegex(@"^\d{2} [A-ZÇĞİÖŞÜ]{1,3} \d{2,4}$")]
    private static partial Regex Pattern();
}

/// <summary>
/// İş türünü metinden tahmin (prototip <c>isTuruTahmin</c>, 06.09). BİR TAHMİN,
/// hüküm değil: kayıt ekranında düzeltilir. Varsayılan TAMİRAT — yanlış bakım
/// tahmini olmayan bir hatırlatmayı kurardı; hatırlatma kurmamak daha az zarar.
/// </summary>
public static class ServiceTypeGuess
{
    private static readonly string[] Maintenance = ["bakim", "yag", "filtre", "periyodik", "servis"];
    private static readonly string[] Repair = ["ariza", "degis", "kacir", "ses", "titre", "yanmi", "calismi", "kirik", "onar", "tamir"];

    public static ServiceType FromText(string? text)
    {
        var words = Fold(text).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                              .Select(w => w.Trim('.', ',', '!', '?', ';', ':')).Where(w => w.Length > 0).ToList();
        bool Starts(string[] roots) => words.Any(w => roots.Any(r => w.StartsWith(r, StringComparison.Ordinal)));
        var m = Starts(Maintenance);
        var r = Starts(Repair);
        return m && r ? ServiceType.Both : m ? ServiceType.Maintenance : ServiceType.Repair;
    }

    /// <summary>Ekranda görünen ad (prototip TUR_AD) — liste araması bu adda da arar.</summary>
    public static string Label(ServiceType t) => t switch
    {
        ServiceType.Maintenance => "Bakım",
        ServiceType.Both => "Tamirat + Bakım",
        _ => "Tamirat"
    };

    /// <summary>Prototipin <c>norm</c>'u: Türkçe küçük harf + ı/ü/ö/ş/ç/ğ katlama.</summary>
    public static string Fold(string? s) => (s ?? "").ToLower(CultureInfo.GetCultureInfo("tr-TR"))
        .Replace('ı', 'i').Replace('ü', 'u').Replace('ö', 'o').Replace('ş', 's').Replace('ç', 'c').Replace('ğ', 'g');
}
