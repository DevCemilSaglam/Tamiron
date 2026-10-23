using System.Text;
using System.Text.RegularExpressions;

namespace Tamiron.Ui.Core.Ses;

/// <summary>
/// Ses çözümleyicisinin metin araçları — prototip 01-cekirdek / 02-veri / 04-islem'in karşılıkları
/// (E3, K-077.1). İKİ TUZAK ELLE ÇÖZÜLÜR:
/// 1. Türkçe büyük/küçük harf KÜLTÜR VERİSİNDEN OKUNMAZ: Blazor WASM tarayıcıya yalnız tarayıcı
///    dilinin verisini yükler — İngilizce tarayıcıda ToUpper(tr-TR) "i"yi "I" yapar (tutardaki
///    "3,450 ₺" hatasının ikizi). i↔İ, ı↔I eşlemesi burada yazılı.
/// 2. JS düzenli ifadesi .NET'inkiyle aynı DEĞİL: JS'te \b ve \w yalnız ASCII'dir (Türkçe harf sözcük
///    sayılmaz); /i bayrağı harfi toUpperCase ile karşılaştırır ("ı" ile "i" eşittir). İkisi de
///    burada JS'e göre taklit edilir — prototipin davranışı korunur.
/// </summary>
public static partial class SesMetin
{
    /// <summary>toLocaleUpperCase('tr') — kültür verisiz.</summary>
    public static string TrUpper(string? s)
    {
        if (string.IsNullOrEmpty(s)) return s ?? "";
        var b = new StringBuilder(s.Length);
        foreach (var c in s) b.Append(c == 'i' ? 'İ' : c == 'ı' ? 'I' : char.ToUpperInvariant(c));
        return b.ToString();
    }

    /// <summary>toLocaleLowerCase('tr') — kültür verisiz.</summary>
    public static string TrLower(string? s)
    {
        if (string.IsNullOrEmpty(s)) return s ?? "";
        var b = new StringBuilder(s.Length);
        foreach (var c in s) b.Append(c == 'I' ? 'ı' : c == 'İ' ? 'i' : char.ToLowerInvariant(c));
        return b.ToString();
    }

    /// <summary>Prototip <c>norm</c>: Türkçe küçük harf + ı/ü/ö/ş/ç/ğ katlama. UZUNLUK KORUNUR (kesme için).</summary>
    public static string Norm(string? s) => TrLower(s)
        .Replace('ı', 'i').Replace('ü', 'u').Replace('ö', 'o').Replace('ş', 's').Replace('ç', 'c').Replace('ğ', 'g');

    /// <summary>Aksan ve tireden arınmış karşılaştırma biçimi: "C-Elysée" → "c elysee" (prototip <c>sade</c>).</summary>
    public static string Sade(string? s) => Ws().Replace(Aksansiz(Norm(s)).Replace('-', ' '), " ").Trim();

    /// <summary>Aksanlar tek harfe — uzunluk korunur.</summary>
    public static string Aksansiz(string s)
    {
        var b = new StringBuilder(s.Length);
        foreach (var c in s)
            b.Append(c switch
            {
                'é' or 'è' or 'ë' => 'e', 'á' or 'â' or 'à' => 'a', 'î' or 'ï' => 'i', 'û' or 'ù' => 'u', _ => c
            });
        return b.ToString();
    }

    /// <summary>JS <c>split(/\s+/)</c>: baştaki boşluk boş bir parça üretir (JS'teki gibi).</summary>
    public static string[] SplitWs(string s) => Ws().Split(s);

    /// <summary>Prototip <c>kelimeVarMi</c>: kelime EŞİTLİKLE aranır (K-018: Türkçede \b yok).</summary>
    public static bool KelimeVarMi(string metin, params string[] kelimeler)
    {
        foreach (var p in SplitWs(Norm(metin)))
            if (Array.IndexOf(kelimeler, Noktalama5().Replace(p, "")) >= 0) return true;
        return false;
    }

    /// <summary>Prototip <c>kelimeleriAt</c>.</summary>
    public static string KelimeleriAt(string? metin, params string[] atilacaklar)
    {
        var kume = atilacaklar.Select(Norm).ToHashSet();
        return string.Join(' ', SplitWs(metin ?? "").Where(p => p.Length > 0 && !kume.Contains(Norm(Noktalama4().Replace(p, ""))))).Trim();
    }

    /// <summary>Prototip <c>kokleBasliyorMu</c>: bir kelime verilen köklerden biriyle başlıyor mu.</summary>
    public static bool KokleBasliyorMu(string metin, IReadOnlyList<string> kokler)
    {
        foreach (var p in SplitWs(Norm(metin)))
        {
            var k = Noktalama5().Replace(p, "");
            if (k.Length == 0) continue;
            foreach (var kok in kokler) if (k.StartsWith(kok, StringComparison.Ordinal)) return true;
        }
        return false;
    }

    /// <summary>Prototip <c>duzenlemeUzakligi</c> (ekle/sil/değiştir).</summary>
    public static int DuzenlemeUzakligi(string a, string b)
    {
        var o = new int[b.Length + 1];
        for (var j = 0; j <= b.Length; j++) o[j] = j;
        for (var i = 1; i <= a.Length; i++)
        {
            var sol = i - 1; o[0] = i;
            for (var j = 1; j <= b.Length; j++)
            {
                var ust = o[j];
                o[j] = Math.Min(Math.Min(o[j] + 1, o[j - 1] + 1), sol + (a[i - 1] == b[j - 1] ? 0 : 1));
                sol = ust;
            }
        }
        return o[b.Length];
    }

    /// <summary>JS'in sözcük karakteri — YALNIZ ASCII (Türkçe harf sözcük sayılmaz).</summary>
    public const string JsW = "A-Za-z0-9_";

    /// <summary>
    /// JS /i bayrağının bir sabit metin için karşılığı: her harf, JS'te büyük harfe çevrilince aynı
    /// olan harflerin sınıfına açılır ("i" → [iIı], "ş" → [şŞ]). Kalan karakterler kaçırılır.
    /// </summary>
    public static string JsCi(string literal)
    {
        var b = new StringBuilder();
        foreach (var c in literal)
        {
            if (!char.IsLetter(c)) { b.Append(Regex.Escape(c.ToString())); continue; }
            var up = JsUpper(c);
            var es = Alfabe.Where(x => JsUpper(x) == up).Append(c).Distinct().ToArray();
            b.Append(es.Length == 1 ? Regex.Escape(c.ToString()) : "[" + new string(es) + "]");
        }
        return b.ToString();
    }

    /// <summary>JS Canonicalize: toUpperCase tek karaktere iniyorsa o (ı → I, i → I, İ → İ).</summary>
    private static char JsUpper(char c) => c == 'ß' ? c : char.ToUpperInvariant(c);
    private const string Alfabe = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZçğıöşüÇĞİÖŞÜéèëáâàîïûù";

    [GeneratedRegex(@"\s+")] private static partial Regex Ws();
    [GeneratedRegex("[.,!?;:]")] private static partial Regex Noktalama5();
    [GeneratedRegex("[.,!?]")] private static partial Regex Noktalama4();
}
