using System.Reflection;
using System.Text.Json;

namespace Tamiron.Ui.Core.Ses;

/// <summary>Çözümleyicinin tanıdığı araç — plaka bağlama, tarif ("Mehmet Kaya'nın arabası") ve öneri için.</summary>
public sealed record SesArac(string Plaka, string? Marka = null, string? Model = null, string? SahipAd = null);

/// <summary>
/// Çözümleyicinin tanıdığı kayıt — açık iş (teslim), satır adları (fiyat güncelleme), ödeme yolu
/// (düzeltme: "almadim" = veresiye, düzeltilmez — E5). Ödeme yolu prototipin adıyla: nakit · kart · havale · almadim.
/// <paramref name="Kalan"/>: alacak — "2 bin aldım" yalnız kalanı olan işi hedefler (K-078 · F7);
/// <paramref name="OdemeSayisi"/>: alınmış ödeme var mı — düzeltme ancak onu hedefler. İkisi de sunucudan
/// (kayıt cevabındaki Remaining ve Payments); çözümleyici hesaplamaz.
/// </summary>
public sealed record SesKayit(string Id, string Plaka, bool Teslim, IReadOnlyList<string> Satirlar, string? OdemeYolu = null,
                              decimal Kalan = 0, int OdemeSayisi = 0);

/// <summary>
/// Çözümleyicinin dünyası: dükkânın araçları ve kayıtları (prototipte ARACLAR / KAYITLAR), kayıtlar
/// prototipteki SIRAYLA (eskiden yeniye). Mesaj metni çözümleyicinin işi değil — sunucunun
/// (MessageTemplates, D4); çözümleyici onu buradan ister.
/// </summary>
public sealed record SesDunya(IReadOnlyList<SesArac> Araclar, IReadOnlyList<SesKayit> Kayitlar,
                              Func<string, string, string>? MesajMetni = null)
{
    public SesArac? AracBul(string plaka) => Araclar.FirstOrDefault(a => a.Plaka == plaka);
    public SesKayit? KayitBul(string? id) => id is null ? null : Kayitlar.FirstOrDefault(k => k.Id == id);
    public IEnumerable<SesKayit> Icerideki() => Kayitlar.Where(k => !k.Teslim);
}

/// <summary>
/// Söyleyiş sözlüğü ve hazır metinler — prototipten ÜRETİLİR (tools/ses-sema-dok.js → Ses/soyleyis.json,
/// gömülü kaynak). Kural değil veri.
/// </summary>
public sealed class SesSozluk
{
    public required IReadOnlyList<(IReadOnlyList<string> Bicimler, string Kanon)> Terim { get; init; }
    public required IReadOnlyDictionary<string, IReadOnlyList<string>> Marka { get; init; }
    public required IReadOnlyDictionary<string, IReadOnlyList<string>> Model { get; init; }
    public required IReadOnlyList<(string K, IReadOnlyList<string> Kokler, string Metin)> Hazir { get; init; }

    private static readonly Lazy<SesSozluk> Gomulu = new(() =>
    {
        using var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("Tamiron.Ui.Core.Ses.soyleyis.json")
                      ?? throw new InvalidOperationException("Ses/soyleyis.json gömülü değil");
        return Parse(JsonDocument.Parse(s).RootElement);
    });

    public static SesSozluk Varsayilan => Gomulu.Value;

    public static SesSozluk Parse(JsonElement r)
    {
        static IReadOnlyList<string> L(JsonElement a) => a.EnumerateArray().Select(x => x.GetString()!).ToList();
        static IReadOnlyDictionary<string, IReadOnlyList<string>> D(JsonElement o)
            => o.EnumerateObject().ToDictionary(p => p.Name, p => L(p.Value));
        return new SesSozluk
        {
            Terim = r.GetProperty("terim").EnumerateArray().Select(t => (L(t[0]), t[1].GetString()!)).ToList(),
            Marka = D(r.GetProperty("marka")),
            Model = D(r.GetProperty("model")),
            Hazir = r.GetProperty("hazir").EnumerateArray()
                .Select(h => (h.GetProperty("k").GetString()!, L(h.GetProperty("kokler")), h.GetProperty("metin").GetString()!)).ToList(),
        };
    }
}
