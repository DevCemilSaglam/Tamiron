using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using static Tamiron.Ui.Core.Ses.SesMetin;

namespace Tamiron.Ui.Core.Ses;

/// <summary>Duyulan / bulunan plaka (prototip sesPlakaBul · sesPlakaBagla · sesAracTarif sonucu).</summary>
public sealed record SesPlaka(string Plaka, int Indeks, int Uzunluk, bool Gecersiz = false, bool Kisa = false, string? Duyulan = null);

/// <summary>
/// Sesli komut çözümleyicisi — prototip 04-islem.js (Islem.cozumle) ve ekranlardaki 12 işlem
/// şemasının tanıyıcıları, CİHAZDA çalışacak C# (E3/E4, K-077.1). Davranış prototiple AYNI:
/// ölçü Esdeger/prototip-ses.json'daki 705 durumda 0 fark (Tamiron.Ui.Tests).
/// Çıktı prototipin işlem nesnesi — düz alanlar: tur + alanlar (arac = plaka, kayit = kimlik).
/// Mesaj metni çözümleyicinin işi değil: SesDunya.MesajMetni'den istenir (sunucu, D4).
/// </summary>
public sealed partial class SesCozumleyici(SesDunya dunya, SesSozluk? sozluk = null)
{
    private readonly SesSozluk soz = sozluk ?? SesSozluk.Varsayilan;
    private static readonly NumberFormatInfo TrSayi = new() { NumberGroupSeparator = ".", NumberDecimalSeparator = "," };

    // ===================== PLAKA =====================

    private const string PlakaHarfleri = "ABCDEFGHIJKLMNOPRSTUVYZ";

    private static string TrHarfsiz(string h) => new(h.Select(c => c switch
    {
        'Ç' => 'C', 'Ğ' => 'G', 'İ' => 'I', 'Ö' => 'O', 'Ş' => 'S', 'Ü' => 'U', _ => c
    }).ToArray());

    /// <summary>Q W X plakada yok: böyle bir plaka KESİNLİKLE yanlış duyulmuştur.</summary>
    private static bool PlakaHarfiGecersizMi(string harf) => harf.Any(h => PlakaHarfleri.IndexOf(h) < 0);

    /// <summary>Prototip sesPlakaBul: biçime uyan ilk aday; hiçbiri uymazsa ilk (kısa) aday.</summary>
    public static SesPlaka? PlakaBul(string metin)
    {
        var buyuk = TrUpper(metin);
        SesPlaka? ilk = null;
        var start = 0;
        while (start <= buyuk.Length)
        {
            var m = PlakaAdayRe().Match(buyuk, start);
            if (!m.Success) break;
            start = m.Index + 1;
            var p = PlakaAday(metin, m);
            if (p is null) continue;
            if (!p.Kisa) return p;
            ilk ??= p;
        }
        return ilk;
    }

    /// <summary>Prototip sesPlakaAday: il 01–81; harf sayısı rakam sayısını sınırlar; bölünmüş rakam birleşir.</summary>
    private static SesPlaka? PlakaAday(string metin, Match m)
    {
        var harfHam = m.Groups[2].Value.Replace(" ", "");
        var il = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
        if (il < 1 || il > 81) return null;
        var harf = TrHarfsiz(harfHam);
        var rakam = m.Groups[3].Value;
        var uzunluk = m.Length;
        var (en, cok) = harfHam.Length switch { 1 => (4, 5), 2 => (3, 4), _ => (2, 3) };
        if (rakam.Length < en)
        {
            var ek = EkRakamRe().Match(TrUpper(metin)[(m.Index + uzunluk)..]);
            if (ek.Success && rakam.Length + ek.Groups[1].Length <= cok)
            {
                rakam += ek.Groups[1].Value;
                uzunluk += ek.Length;
            }
        }
        return new SesPlaka(m.Groups[1].Value + " " + harf + " " + rakam, m.Index, uzunluk,
            Gecersiz: PlakaHarfiGecersizMi(harf) || rakam.Length > cok, Kisa: rakam.Length < en);
    }

    private static readonly string[][] SesSiniflari =
        [["B", "P", "D", "T", "G", "K"], ["C", "Ç"], ["F", "V", "S", "Z", "Ş", "J", "H"], ["M", "N", "V"], ["L", "R"]];

    /// <summary>Prototip sesceYakinHarf: ünsüzü aynı biçimde çıkan harfler (te/de/pe, me/ne, gözlenen M~V).</summary>
    public static bool SesceYakinHarf(string a, string b)
    {
        if (a == b) return true;
        var x = Enumerable.Range(0, SesSiniflari.Length).Where(i => SesSiniflari[i].Contains(a));
        return x.Any(i => SesSiniflari[i].Contains(b));
    }

    private static string HarfOkunusu(string harf) => string.Concat(harf.Select(h => "AEIİOÖUÜ".IndexOf(h) >= 0 ? h.ToString() : h + "E"));
    private static string Iskelet(string s) => UnluRe().Replace(s, "");

    public sealed record Parca(string Il, string Harf, string Rakam);

    /// <summary>Prototip plakaParcala.</summary>
    public static Parca? PlakaParcala(string? plaka)
    {
        var p = SplitWs(TrUpper(plaka ?? "")).Where(x => x.Length > 0).ToArray();
        return p.Length == 3 ? new Parca(p[0], p[1], p[2]) : null;
    }

    /// <summary>Prototip harfFarki: değiştirme, ya da bir eksik/fazla harf.</summary>
    public static int HarfFarki(string a, string b)
    {
        if (a == b) return 0;
        if (a.Length == b.Length) return a.Where((c, i) => c != b[i]).Count();
        if (Math.Abs(a.Length - b.Length) != 1) return 99;
        var (uzun, kisa) = a.Length > b.Length ? (a, b) : (b, a);
        for (var j = 0; j < uzun.Length; j++) if (uzun[..j] + uzun[(j + 1)..] == kisa) return 1;
        return 99;
    }

    /// <summary>
    /// Prototip sesPlakaBagla: yalnız GERÇEK OLAMAYAN plaka (Q/W/X'li, harf yerine kelime) TEK kayıtlıya
    /// bağlanır (K-059); gerçek olabilecek plaka bağlanmaz — öneri olur, usta seçer.
    /// </summary>
    private SesPlaka? PlakaBagla(string metin, SesPlaka? p)
    {
        if (p is not null && dunya.AracBul(p.Plaka) is not null) return null;
        if (p is not null && !p.Gecersiz) return null;
        var adaylar = new List<string>();
        string duyulan; int indeks, uzunluk;
        if (p is not null)
        {
            var q = PlakaParcala(p.Plaka)!;
            duyulan = p.Plaka; indeks = p.Indeks; uzunluk = p.Uzunluk;
            foreach (var a in dunya.Araclar)
            {
                var r = PlakaParcala(a.Plaka);
                if (r is null || r.Il != q.Il || r.Rakam != q.Rakam) continue;
                if (HarfFarki(q.Harf, r.Harf) <= 1) adaylar.Add(a.Plaka);
            }
        }
        else
        {
            var b = KelimePlakaRe().Match(TrUpper(metin));
            if (!b.Success) return RakamlaBagla(metin, null);
            duyulan = b.Groups[2].Value + " " + b.Groups[3].Value + " " + b.Groups[4].Value;
            indeks = b.Index + b.Groups[1].Length; uzunluk = b.Length - b.Groups[1].Length;
            foreach (var a in dunya.Araclar)
            {
                var r = PlakaParcala(a.Plaka);
                if (r is null || r.Il != b.Groups[2].Value || r.Rakam != b.Groups[4].Value) continue;
                var ok = HarfOkunusu(r.Harf);
                if (Iskelet(ok) != Iskelet(b.Groups[3].Value)) continue;
                if (DuzenlemeUzakligi(ok, b.Groups[3].Value) > 2) continue;
                adaylar.Add(a.Plaka);
            }
            if (adaylar.Count == 0) return RakamlaBagla(metin, b.Groups[2].Value);
        }
        return adaylar.Count == 1 ? new SesPlaka(adaylar[0], indeks, uzunluk, Duyulan: duyulan) : null;
    }

    /// <summary>
    /// Prototip sesRakamlaBagla: plaka hiç duyulmadıysa rakam grubundan TEK kayıtlı plakaya (para/km
    /// sözü olan cümlede yapılmaz; adet/zaman sözünden önceki sayı aday değil).
    /// </summary>
    private SesPlaka? RakamlaBagla(string metin, string? il)
    {
        var d = Norm(metin);
        if (RakamYasakRe().IsMatch(d)) return null;
        if (TelefonBul(metin) is not null) return null;
        var t = RakamRe().Matches(metin).Select(m => (S: m.Value, I: m.Index, N: m.Length)).ToList();
        var adaylar = new Dictionary<string, (int I, int Son)>();
        var sira = new List<string>();

        bool BastaMi(int i)
        {
            var once = SplitWs(Norm(metin[..i]).Trim()).Where(w => w.Length > 0).ToList();
            return once.Count <= 3 && !once.Any(w => ZamanRe().IsMatch(w));
        }
        void Dene(string s, int i, int son)
        {
            if (s.Length < (il is not null || BastaMi(i) ? 3 : 4)) return;
            if (AdetRe().IsMatch(Norm(metin[son..]))) return;
            foreach (var a in dunya.Araclar)
            {
                var r = PlakaParcala(a.Plaka);
                if (r is not null && r.Rakam == s && (il is null || r.Il == il))
                {
                    if (!adaylar.ContainsKey(a.Plaka)) sira.Add(a.Plaka);
                    adaylar[a.Plaka] = (i, son);
                }
            }
        }
        for (var k = 0; k < t.Count; k++)
        {
            var x = t[k];
            Dene(x.S, x.I, x.I + x.N);
            if (k + 1 < t.Count)
            {
                var y = t[k + 1];
                if (BoslukRe().IsMatch(metin[(x.I + x.N)..y.I])) Dene(x.S + y.S, x.I, y.I + y.N);
            }
        }
        if (sira.Count != 1) return null;
        var (ai, son) = adaylar[sira[0]];
        return new SesPlaka(sira[0], ai, son - ai, Duyulan: metin[ai..son]);
    }

    /// <summary>Prototip benzerPlaka: kayıtlı OLMAYAN plakaya tek harf ya da sesçe yakın TEK aday (K-059: önerilir, sorulur).</summary>
    public string? BenzerPlaka(string plaka)
    {
        var p = PlakaParcala(plaka);
        if (p is null || dunya.AracBul(plaka) is not null) return null;
        var adaylar = new List<string>();
        foreach (var a in dunya.Araclar)
        {
            var q = PlakaParcala(a.Plaka);
            if (q is null || q.Il != p.Il || q.Rakam != p.Rakam) continue;
            var sesce = p.Harf.Length == q.Harf.Length && p.Harf.Select((h, i) => SesceYakinHarf(h.ToString(), q.Harf[i].ToString())).All(x => x);
            if (HarfFarki(p.Harf, q.Harf) > 1 && !sesce) continue;
            adaylar.Add(a.Plaka);
        }
        if (adaylar.Count > 0) return adaylar.Count == 1 ? adaylar[0] : null;
        return RakamlaBenzerPlaka(p);
    }

    /// <summary>Prototip rakamlaBenzerPlaka: aynı rakamlı kayıtlılar puanlanır; tek en yüksek ve ≥ 4 ise öneri.</summary>
    private string? RakamlaBenzerPlaka(Parca p)
    {
        const string Unlu = "AEIİOÖUÜ";
        var puanlar = new List<(string Plaka, int Puan)>();
        foreach (var a in dunya.Araclar)
        {
            var q = PlakaParcala(a.Plaka);
            if (q is null || q.Rakam != p.Rakam) continue;
            var puan = q.Il == p.Il ? 2 : 0;
            if (q.Harf.Length == p.Harf.Length)
            {
                for (var i = 0; i < q.Harf.Length; i++)
                {
                    char x = p.Harf[i], y = q.Harf[i];
                    if (x == y) puan += 2;
                    else if ((Unlu.IndexOf(x) >= 0 && Unlu.IndexOf(y) >= 0) || SesceYakinHarf(x.ToString(), y.ToString())) puan += 1;
                }
            }
            else if (HarfFarki(p.Harf, q.Harf) <= 1) puan += 2;
            puanlar.Add((a.Plaka, puan));
        }
        var sirali = puanlar.OrderByDescending(x => x.Puan).ToList();   // OrderBy kararlı — JS sort gibi
        if (sirali.Count == 0 || sirali[0].Puan < 4) return null;
        if (sirali.Count > 1 && sirali[1].Puan == sirali[0].Puan) return null;
        return sirali[0].Plaka;
    }

    /// <summary>Prototip sesAracTarif: "Mehmet Kaya'nın arabası", "Mégane'ın", "Ford Focus geldi" — TEK araçsa.</summary>
    private object? AracTarif(string metin, string baglam)
    {
        if (baglam != "genel" && baglam != "servis-ekle") return null;
        var d = Norm(metin);
        const string Iyelik = "'?n?[iu]n";
        var bulunan = new List<(SesArac A, int I, int N, string Tur)>();
        foreach (var a in dunya.Araclar)
        {
            var ad = Norm(a.SahipAd ?? "").Trim();
            if (!ad.Contains(' ')) continue;
            var m = Regex.Match(d, @"(^|\s)(" + Regex.Escape(ad) + Iyelik + @"\s+(?:araba|arac)\S*)");
            if (m.Success) bulunan.Add((a, m.Index + m.Groups[1].Length, m.Groups[2].Length, "sahip"));
        }
        if (bulunan.Count == 0)
        {
            var ds = Aksansiz(d).Replace('-', ' ');
            foreach (var a in dunya.Araclar)
            {
                var markalar = Soyleyisleri(soz.Marka, a.Marka ?? "");
                var modeller = Soyleyisleri(soz.Model, a.Model ?? "");
                Match? m = null;
                foreach (var mk in markalar)
                    foreach (var md in modeller)
                    {
                        if (m is { Success: true } || mk.Length == 0 || md.Length == 0) continue;
                        m = Regex.Match(ds, @"(^|\s)(" + Regex.Escape(mk) + @"\s+" + Regex.Escape(md.TrimStart('!').Length == md.Length ? md : md[1..]) + @")(?=\s|\z|')");
                    }
                foreach (var md in modeller)
                {
                    if (m is { Success: true } || md.Length == 0 || md[0] == '!' || SadeceRakamRe().IsMatch(md)) continue;
                    m = Regex.Match(ds, @"(^|\s)(" + Regex.Escape(md) + Iyelik + @")(?=\s|\z)");
                    if (!m.Success) m = Regex.Match(ds, @"(^|\s)(" + Regex.Escape(md) + @")(?=\s+geldi)");
                }
                if (m is { Success: true }) bulunan.Add((a, m.Index + m.Groups[1].Length, m.Groups[2].Length, "model"));
            }
        }
        if (bulunan.Count == 0) return null;
        if (bulunan.Count != 1) return "belirsiz";
        var b = bulunan[0];
        return new SesPlaka(b.A.Plaka, b.I, b.N);
    }

    private static IReadOnlyList<string> Soyleyisleri(IReadOnlyDictionary<string, IReadOnlyList<string>> sozluk, string ad)
    {
        var a = Sade(ad);
        return [a, .. sozluk.TryGetValue(a, out var l) ? l : []];
    }

    // ===================== TUTAR · TELEFON · KM =====================

    /// <summary>Prototip sesTelefonBul: boşluklar atılınca 0 ile başlayan 11 hane.</summary>
    public static string? TelefonBul(string metin)
    {
        var m = TelefonRe().Match(BoslukTumRe().Replace(metin, ""));
        if (!m.Success) return null;
        var t = m.Value;
        return t[..4] + " " + t[4..7] + " " + t[7..9] + " " + t[9..];
    }

    /// <summary>Prototip sesTutarBul: "2400 TL", "850 lira". SÖYLENMEYEN TUTAR 0 (null değil).</summary>
    public static long TutarBul(string d)
    {
        var m = TutarRe().Match(d);
        return m.Success ? long.Parse(m.Groups[1].Value.Replace(".", ""), CultureInfo.InvariantCulture) : 0;
    }

    public sealed record Ciplak(long Tutar, string Parca);

    private static Ciplak? CiplakSonuc(Match m, string d)
    {
        if (!m.Success || TelefonBul(d) is not null) return null;
        var rakam = m.Groups[2].Value.Replace(".", "");
        if (rakam.Length < 2 || rakam.Length >= 10) return null;
        return new Ciplak(long.Parse(rakam, CultureInfo.InvariantCulture), m.Groups[2].Value);
    }

    /// <summary>Prototip sesCiplakTutar: birimsiz sayı — yalnız tutardan başka olamayacak yerde okunur.</summary>
    public static Ciplak? CiplakTutar(string d) => CiplakSonuc(CiplakRe().Match(d), d);
    /// <summary>Prototip sesSonTutar: cümlenin SON kelimesi sayıysa.</summary>
    public static Ciplak? SonTutar(string d) => CiplakSonuc(SonTutarRe().Match(d), d);
    /// <summary>Prototip sesTutarsiz: metinden "x lira"yı söker.</summary>
    public static string Tutarsiz(string metin) => TutarsizRe().Replace(metin, "");

    // ===================== METİN HAZIRLAMA =====================

    /// <summary>Prototip sesDuzeltmeUygula: "33 yok 23" → "23" (iki SAYI arasında).</summary>
    public static string DuzeltmeUygula(string metin)
    {
        string onceki;
        do { onceki = metin; metin = DuzeltmeRe().Replace(metin, "$1$2", 1); } while (metin != onceki);
        return metin;
    }

    /// <summary>Prototip sesSoyleyisDuzelt: "eksoz" → "egzoz"; kayıttaki biçimin başı olan söyleyişe dokunulmaz.</summary>
    public string SoyleyisDuzelt(string metin)
    {
        foreach (var (bicimler, kanon) in soz.Terim)
        {
            var bicim = new List<string>();
            foreach (var v in bicimler.Where(v => !v.StartsWith('!')))
            {
                if (!bicim.Contains(v)) bicim.Add(v);
                if (!bicim.Contains(Norm(v))) bicim.Add(Norm(v));
            }
            foreach (var v in bicim.OrderByDescending(x => x.Length))   // kararlı sıralama — JS gibi
            {
                var re = new Regex("(^|[^A-Za-zÇĞİÖŞÜçğıöşü])(" + JsCi(v) + ")");
                var butun = metin;
                metin = re.Replace(butun, m =>
                {
                    var on = m.Groups[1].Value;
                    var sonra = butun[(m.Index + on.Length)..];
                    return TrLower(sonra).StartsWith(kanon, StringComparison.Ordinal) ? m.Value : on + kanon;
                });
            }
        }
        return metin;
    }

    /// <summary>Prototip Islem.cozumle'nin girişi: düzeltme, sözlük, saat/ondalık/nokta, "88 bin 900", harf-rakam ayırma.</summary>
    private string Hazirla(string metin)
    {
        metin = SoyleyisDuzelt(DuzeltmeUygula(metin));
        metin = IkiNoktaRe().Replace(metin, "$1 ");
        metin = VirgulRe().Replace(metin, "$1");
        metin = NoktaRe().Replace(metin, "$1");
        metin = NoktalamaRe().Replace(metin, " ");
        metin = BinRe().Replace(metin, m => m.Groups[1].Value + (int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture) * 1000
            + (m.Groups[3].Success ? int.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture) : 0)).ToString(CultureInfo.InvariantCulture));
        metin = NoktaRe().Replace(metin, "$1").Replace("°", "");
        metin = HarfRakamRe().Replace(metin, "$1 $2");
        metin = RakamHarfRe().Replace(metin, "$1 $2");
        return metin;
    }

    // ===================== BAĞLAM =====================

    private sealed class C
    {
        public required string Metin, D, Baglam;
        public SesPlaka? P;
        public string? KayitId;
        public SesKayit? Kayit;
        public string Kalan = "", Dk = "";
        public SesArac? Arac;
    }

    private C BaglamKur(string metin, string baglam, string? kayitId)
    {
        var p = PlakaBul(metin);
        var bag = PlakaBagla(metin, p);
        if (bag is not null) p = bag;
        if (p is null && AracTarif(metin, baglam) is SesPlaka tarif) p = tarif;
        var c = new C { Metin = metin, D = Norm(metin), P = p, Baglam = baglam, KayitId = kayitId, Kayit = dunya.KayitBul(kayitId) };
        if (p is not null)
        {
            c.Kalan = BoslukRe2().Replace(metin[..p.Indeks] + metin[(p.Indeks + p.Uzunluk)..], " ").Trim();
            c.Dk = Norm(c.Kalan);
            c.Arac = dunya.AracBul(p.Plaka);
        }
        return c;
    }

    // ===================== OKUYUCULAR (ekranların) =====================

    /// <summary>Prototip sesServisOku (30-kayit): iş türü, km, telefon, sahip, iş metni.</summary>
    private static (string IsTuru, string Km, string? Tel, string Sahip, string Is) ServisOku(C c)
    {
        var dk = c.Dk; var kalan = c.Kalan;
        var isTuru = dk.Contains("bakim") || dk.Contains("bakum") || dk.Contains("periyodik") || dk.Contains("yag degis") ? "bakim" : "tamirat";
        var km = "";
        var kmE = Km1Re().Match(dk);
        if (!kmE.Success) kmE = Km2Re().Match(dk);
        if (kmE.Success)
        {
            var sayi = long.Parse(kmE.Groups[1].Value.Replace(".", ""), CultureInfo.InvariantCulture);
            if (kmE.Groups[2].Value == "bin") sayi *= 1000;
            km = sayi.ToString("#,0", TrSayi);
        }
        var tel = TelefonBul(kalan);
        var adE = SahipRe().Match(kalan);
        var sahip = adE.Success ? adE.Groups[1].Value : "";
        var isMetni = Km1CiRe().Replace(kalan, "");
        isMetni = Km2CiRe().Replace(isMetni, "");
        isMetni = TelefonAtRe().Replace(isMetni, "");
        if (sahip.Length > 0 && isMetni.IndexOf(sahip, StringComparison.Ordinal) is var ix and >= 0) isMetni = isMetni.Remove(ix, sahip.Length);
        isMetni = BoslukRe2().Replace(isMetni, " ").Trim();
        return (isTuru, km, tel, sahip, isMetni);
    }

    /// <summary>Prototip sesSatirAdi (30-kayit): tutar ve komut kelimeleri atılmış satır adı.</summary>
    private static string SatirAdi(C c)
    {
        var metin = Tutarsiz(c.Metin);
        if (TutarBul(c.D) == 0 && c.P is null && SonTutar(metin) is { } son)
            metin = metin[..metin.LastIndexOf(son.Parca, StringComparison.Ordinal)];
        return KelimeleriAt(metin, "parça", "parca", "işçilik", "iscilik", "işçiliği", "isciligi", "ücreti", "ucreti",
            "yaz", "da", "de", "ekle", "yap", "oldu", "olsun", "fiyat", "fiyatı", "fiyati", "güncelle", "guncelle");
    }

    /// <summary>Prototip sesKayitTutari (30-kayit): "lira" ile ya da (plakasız cümlede) son kelime sayıysa.</summary>
    private static long KayitTutari(C c)
    {
        var t = TutarBul(c.D);
        if (t != 0 || c.P is not null) return t;
        return SonTutar(c.Metin)?.Tutar ?? 0;
    }

    private static string Kanali(string d) => d.Contains("sms") ? "sms" : "whatsapp";

    private static bool TopluMu(string d) => d.Contains("tum musteri") || d.Contains("butun musteri") || KelimeVarMi(d, "herkese", "hepsine");

    private static bool MesajKomutu(string d) =>
        d.Contains("mesaj") || d.Contains("whatsapp") || d.Contains("sms") ||
        KelimeVarMi(d, "yaz", "yazdir", "sahibine") || d.Contains("gonder") ||
        (KelimeVarMi(d, "sahibinden") && MesajMetniRe().IsMatch(d));

    /// <summary>Prototip sesOdemeYolu (50-teslim): söylenmeyen ödeme null — nakit VARSAYILMAZ.</summary>
    private static string? OdemeYolu(string d) =>
        d.Contains("nakit") || d.Contains("pesin") ? "nakit"
        : d.Contains("kart") || KelimeVarMi(d, "kk") ? "kart"
        : d.Contains("havale") || KelimeVarMi(d, "eft") || d.Contains("hesaba") ? "havale" : null;

    /// <summary>Prototip sesOdemeDuzeltmeMi (50-teslim, E5).</summary>
    private static bool OdemeDuzeltmeMi(string d) =>
        d.Contains("degil") || d.Contains("yanlis") || d.Contains("duzelt") || d.Contains("cevir") || d.Contains("degistir");

    /// <summary>
    /// Prototip sesDuzeltilenYol: "değil"den SONRA geçen, yoksa cümlede EN SON geçen yol — "nakit değil
    /// kartla" kart, "kartla değil nakit" nakit. Eşitlikte ilk sayılan (nakit · kart · havale) kalır.
    /// </summary>
    private static string? DuzeltilenYol(string d)
    {
        var i = d.IndexOf("degil", StringComparison.Ordinal);
        var s = i >= 0 ? d[(i + 5)..] : d;
        static int Son(string s, params string[] k) => k.Max(x => s.LastIndexOf(x, StringComparison.Ordinal));
        static int Ara(string s, Regex re) { var m = re.Match(s); return m.Success ? m.Index : -1; }
        (string Yol, int Yer)[] yer =
        [
            ("nakit", Son(s, "nakit", "pesin")),
            ("kart", Math.Max(Son(s, "kart"), Ara(s, KkRe()))),
            ("havale", Math.Max(Son(s, "havale", "hesaba"), Ara(s, EftRe()))),
        ];
        string? en = null; var enYer = -1;
        foreach (var (yol, y) in yer) if (y >= 0 && (en is null || y > enYer)) { en = yol; enYer = y; }
        return en;
    }

    /// <summary>Teslimde ödeme: "değil" geçen cümlede düzeltmenin kuralı ("nakit değil kartla" → kart).</summary>
    private static string? TeslimOdemesi(string d) => d.Contains("degil") ? DuzeltilenYol(d) : OdemeYolu(d);

    private (string K, IReadOnlyList<string> Kokler, string Metin)? HazirMetinBul(string cumle)
    {
        foreach (var h in soz.Hazir) if (KokleBasliyorMu(cumle, h.Kokler)) return h;
        return null;
    }

    // ===================== ŞEMALAR =====================

    private sealed record Sema(string Tur, string[] Baglam, int? Oncelik, bool Varsayilan = false, bool Guclu = true,
                               Func<C, JsonObject?>? Tani = null, Func<C, JsonObject?>? Baglamda = null);

    private static JsonObject O(params (string K, JsonNode? V)[] alanlar)
    {
        var o = new JsonObject();
        foreach (var (k, v) in alanlar) o[k] = v;
        return o;
    }

    private static JsonObject Anlasilmadi(string metin) => O(("tur", "plakasiz"), ("metin", metin));

    /// <summary>Kayıt sırası prototipteki yükleme sırasıdır (ekran dosyaları alfabetik, dosya içinde yazılış sırası).</summary>
    private IReadOnlyList<Sema> Semalar() =>
    [
        new("yeni-arac", ["servis-ekle"], 70, Tani: c =>
        {
            if (c.P is null || c.Arac is not null) return null;
            var s = ServisOku(c);
            return O(("tur", "yeni-arac"), ("plaka", c.P.Plaka), ("sahip", s.Sahip), ("tel", s.Tel), ("is", s.Is),
                     ("isTuru", s.IsTuru), ("km", s.Km), ("oneri", BenzerPlaka(c.P.Plaka)));
        }),
        new("bilinmeyen-arac", [], null),
        new("fiyat-guncelle", ["kayit-icinde"], 65, Baglamda: FiyatGuncelle),
        new("satir-ekle", ["kayit-icinde"], null, Varsayilan: true, Baglamda: c =>
        {
            if (c.KayitId is null) return null;
            var iscilikMi = c.D.Contains("iscili");
            var ad = SatirAdi(c);
            return O(("tur", "satir-ekle"), ("ad", ad.Length > 0 ? ad : iscilikMi ? "İşçilik" : "Parça"),
                     ("tip", iscilikMi ? "iscilik" : "parca"), ("fiyat", KayitTutari(c)));
        }),
        new("servis-ac", ["servis-ekle"], 80, Tani: c =>
        {
            if (c.P is null || c.Arac is null) return null;
            var s = ServisOku(c);
            return O(("tur", "servis-ac"), ("plaka", c.P.Plaka), ("arac", c.Arac.Plaka), ("isTuru", s.IsTuru), ("km", s.Km), ("is", s.Is));
        }),
        new("teslim", ["kayit-icinde"], 60,
            Tani: c =>
            {
                if (c.P is null || c.Arac is null || !c.Dk.Contains("teslim")) return null;
                var plaka = c.P.Plaka;
                var acik = dunya.Icerideki().FirstOrDefault(k => k.Plaka == plaka);
                if (acik is null) return O(("tur", "teslim-edilecek-kayit-yok"), ("plaka", plaka), ("arac", c.Arac.Plaka));
                return O(("tur", "teslim"), ("plaka", plaka), ("arac", c.Arac.Plaka), ("kayit", acik.Id), ("odeme", TeslimOdemesi(c.Dk)));
            },
            Baglamda: c =>
            {
                var k = c.Kayit;
                if (k is null || !c.D.Contains("teslim") || k.Teslim) return null;
                return O(("tur", "teslim"), ("plaka", k.Plaka), ("arac", dunya.AracBul(k.Plaka)?.Plaka), ("kayit", k.Id), ("odeme", TeslimOdemesi(c.D)));
            }),
        // E5 · teslim edilmiş işin ödeme yolu düzeltmesi. Veresiye ("almadim") hedef DEĞİL: oradaki
        // "havaleye çevir" geriye dönük kasaya para yazardı — o tahsilattır (E6).
        new("odeme-duzelt", ["kayit-icinde"], 55,
            Tani: c =>
            {
                if (c.P is null || c.Arac is null || !OdemeDuzeltmeMi(c.Dk) || DuzeltilenYol(c.Dk) is not { } yol) return null;
                var k = dunya.Kayitlar.LastOrDefault(k => k.Plaka == c.P.Plaka && k.Teslim && k.OdemeYolu != "almadim");
                if (k is null) return null;
                return O(("tur", "odeme-duzelt"), ("plaka", c.P.Plaka), ("arac", c.Arac.Plaka), ("kayit", k.Id), ("odeme", yol));
            },
            Baglamda: c =>
            {
                var k = c.Kayit;
                if (k is null || !k.Teslim || k.OdemeYolu == "almadim" || DuzeltilenYol(c.D) is not { } yol || !OdemeDuzeltmeMi(c.D)) return null;
                return O(("tur", "odeme-duzelt"), ("plaka", k.Plaka), ("arac", dunya.AracBul(k.Plaka)?.Plaka), ("kayit", k.Id), ("odeme", yol));
            }),
        new("teslim-edilecek-kayit-yok", [], null, Guclu: false),
        new("ara", ["araclar", "kayitlar", "musteriler"], 50, Tani: c =>
        {
            var d = c.D; var baglam = c.Baglam;
            if (!KelimeVarMi(d, "ara", "bul", "goster") && baglam is not ("araclar" or "kayitlar" or "musteriler")) return null;
            var terim = KelimeleriAt(c.Metin, "ara", "bul", "göster", "goster", "sahibini", "sahibi", "aracını", "aracini");
            var hedef = baglam == "musteriler" ? "musteriler" : baglam == "kayitlar" ? "kayitlar" : "araclar";
            if (c.P is not null && d.Contains("sahib"))
            {
                terim = !string.IsNullOrEmpty(c.Arac?.SahipAd) ? c.Arac!.SahipAd! : c.P.Plaka;
                hedef = "musteriler";
            }
            else if (c.P is not null && hedef == "araclar" && BoslukTumRe().Replace(c.Kalan, "").Length == 0)
                terim = c.P.Plaka;
            if (d.Contains("iceridek") || d.Contains("servistek") || d.Contains("serviste ne"))
                return O(("tur", "ara"), ("hedef", "kayitlar"), ("terim", ""), ("suzgec", "serviste"));
            if (terim.Length == 0) return null;
            return O(("tur", "ara"), ("hedef", hedef), ("terim", terim));
        }),
        new("toplu-mesaj", ["mesaj"], 30, Tani: c =>
        {
            if (!MesajKomutu(c.D) || !TopluMu(c.D)) return null;
            var hz = HazirMetinBul(c.Metin);
            var kalanT = KelimeleriAt(c.Metin, "tüm", "tum", "bütün", "butun", "herkese", "hepsine", "müşterilere", "musterilere",
                "müşteriye", "musteriye", "mesaj", "mesajı", "mesaji", "gönder", "gonder", "at", "yaz", "whatsapp", "sms",
                "üzerinden", "uzerinden", "ile");
            if (hz is null && kalanT.Length == 0) return Anlasilmadi(c.Metin);
            return O(("tur", "toplu-mesaj"), ("metinTuru", hz?.K ?? "serbest"), ("govde", hz?.Metin ?? kalanT), ("kanal", Kanali(c.D)));
        }),
        new("mesaj-gonder", ["mesaj"], 40, Tani: c =>
        {
            if (!MesajKomutu(c.D) || c.P is null) return null;
            if (c.Arac is null) return O(("tur", "bilinmeyen-arac"), ("plaka", c.P.Plaka), ("oneri", BenzerPlaka(c.P.Plaka)));
            var hz = HazirMetinBul(c.Metin); var d = c.D;
            var sablon = hz?.K ?? (d.Contains("hatirlatma") ? "hatirlatma" : d.Contains("bekliyor") ? "bekliyor"
                       : d.Contains("hazir") ? "hazir" : d.Contains("teslim") ? "teslim" : "ozet");
            return O(("tur", "mesaj-gonder"), ("plaka", c.P.Plaka), ("arac", c.Arac.Plaka), ("sablon", sablon),
                     ("kanal", Kanali(d)), ("govde", dunya.MesajMetni?.Invoke(c.P.Plaka, sablon)));
        }),
        new("tahsilat-sor", ["muhasebe"], 10, Tani: c =>
        {
            var d = c.D;
            if (TutarBul(d) != 0 || (!d.Contains("tahsil") && !d.Contains("kasa"))) return null;
            var donem = d.Contains("bugun") ? "bugun" : d.Contains("yil") || d.Contains("sene") ? "yil" : d.Contains("ay") ? "ay" : "bugun";
            return O(("tur", "tahsilat-sor"), ("donem", donem));
        }),
        new("gider-ekle", ["muhasebe"], 20, Tani: c =>
        {
            var d = c.D;
            if (!(d.Contains("gider") || d.Contains("fatura") || d.Contains("kira") || (c.Baglam == "muhasebe" && TutarBul(d) != 0))) return null;
            var t = TutarBul(d); var metin = Tutarsiz(c.Metin);
            if (t == 0 && CiplakTutar(c.P is not null ? c.Kalan : c.Metin) is { } ciplak)
            {
                t = ciplak.Tutar;
                var ix = metin.IndexOf(ciplak.Parca, StringComparison.Ordinal);
                if (ix >= 0) metin = metin.Remove(ix, ciplak.Parca.Length);
            }
            if (t == 0) return null;
            var aciklama = KelimeleriAt(metin, "ekle", "gider", "olarak", "için", "icin");
            return O(("tur", "gider-ekle"), ("tutar", t), ("aciklama", aciklama.Length > 0 ? aciklama : "Gider"));
        }),
    ];

    /// <summary>Prototip fiyat-guncelle.baglamda: söylenen ad TEK satıra uyuyorsa (A-M27); yakın söyleyiş %20.</summary>
    private JsonObject? FiyatGuncelle(C c)
    {
        var k = c.Kayit; var tutarR = KayitTutari(c); var ad = SatirAdi(c);
        if (k is null || tutarR == 0 || ad.Length == 0) return null;
        var adN = Norm(ad);
        var uyan = new List<int>();
        for (var i = 0; i < k.Satirlar.Count; i++)
        {
            var sN = Norm(k.Satirlar[i]);
            if (sN.Length < 3 || adN.Length < 3) continue;
            if (sN == adN || sN.Contains(adN) || adN.Contains(sN)) uyan.Add(i);
        }
        if (uyan.Count == 0)
        {
            var adB = BoslukTumRe().Replace(adN, "");
            for (var i = 0; i < k.Satirlar.Count; i++)
            {
                var sB = BoslukTumRe().Replace(Norm(k.Satirlar[i]), "");
                if (sB.Length < 5 || adB.Length == 0) continue;
                if (!SesceYakinHarf(TrUpper(adB[..1]), TrUpper(sB[..1]))) continue;
                if (DuzenlemeUzakligi(adB, sB) <= (int)Math.Floor(sB.Length * 0.2)) uyan.Add(i);
            }
        }
        if (uyan.Count != 1) return null;
        return O(("tur", "fiyat-guncelle"), ("kayit", k.Id), ("indeks", uyan[0]), ("satir", k.Satirlar[uyan[0]]), ("fiyat", tutarR));
    }

    // ===================== ÇÖZÜMLE =====================

    /// <summary>
    /// Prototip Islem.cozumle: (1) bağlamın kendi işleri, kendi sıralarıyla; (2) genel yol, öncelik
    /// sırasıyla; (3) bağlamın VARSAYILAN işi — genel yol güçlü bir iş bulduysa o asıl olur, varsayılan
    /// ÖNERİ (baskaIslem) olarak durur; bulamadıysa varsayılan asıl olur.
    /// </summary>
    public JsonObject Cozumle(string metin, string baglam = "genel", string? kayitId = null)
    {
        var c = BaglamKur(Hazirla(metin ?? ""), baglam, kayitId);
        var r = Doldur(c);
        if (c.P?.Duyulan is { } duyulan && r["plaka"]?.GetValue<string>() == c.P.Plaka) r["duyulanPlaka"] = duyulan;
        return r;
    }

    private JsonObject Doldur(C c)
    {
        var semalar = Semalar();
        var kendi = semalar.Where(s => s.Baglam.Contains(c.Baglam)).OrderBy(s => s.Oncelik ?? 0).ToList();   // kararlı
        Sema? varsayilan = null;
        foreach (var s in kendi)
        {
            if (s.Varsayilan) { varsayilan ??= s; continue; }
            if (s.Baglamda?.Invoke(c) is { } r) return r;
        }
        var yedek = varsayilan?.Baglamda?.Invoke(c);
        var genel = Genel(c, semalar);
        if (yedek is null) return genel;
        if (GucluMu(genel, semalar)) { genel["baskaIslem"] = yedek; return genel; }
        return yedek;
    }

    private static JsonObject Genel(C c, IReadOnlyList<Sema> semalar)
    {
        foreach (var s in semalar.Where(s => s.Tani is not null).OrderBy(s => s.Oncelik))
            if (s.Tani!(c) is { } r) return r;
        return Anlasilmadi(c.Metin);
    }

    private static bool GucluMu(JsonObject o, IReadOnlyList<Sema> semalar)
    {
        var tur = o["tur"]?.GetValue<string>();
        var s = semalar.FirstOrDefault(x => x.Tur == tur);
        return s is not null && s.Guclu;
    }

    // ===================== DÜZENLİ İFADELER (JS'ten; \b ve \w ASCII, $ = metnin sonu) =====================

    [GeneratedRegex(@"(^|\s)kk(\s|\z)")] private static partial Regex KkRe();
    [GeneratedRegex(@"(^|\s)eft(\s|\z)")] private static partial Regex EftRe();
    [GeneratedRegex(@"(?<![A-Za-z0-9_])([0-9]{2})\s*([A-ZÇĞİÖŞÜ](?: ?[A-ZÇĞİÖŞÜ]){0,2})\s*([0-9]{2,5})(?![A-Za-z0-9_])")] private static partial Regex PlakaAdayRe();
    [GeneratedRegex(@"^\s+([0-9]{1,3})(?![0-9])")] private static partial Regex EkRakamRe();
    [GeneratedRegex(@"(^|\s)([0-9]{2})\s+([A-ZÇĞİÖŞÜ]{4,9})\s+([0-9]{2,4})(?![0-9])")] private static partial Regex KelimePlakaRe();
    [GeneratedRegex("[AEIİOÖUÜ]")] private static partial Regex UnluRe();
    [GeneratedRegex(@"(lira|tl|gider|fatura|kira|tahsil|kasa|kilometre|km)(\s|\z)")] private static partial Regex RakamYasakRe();
    [GeneratedRegex("[0-9]+")] private static partial Regex RakamRe();
    [GeneratedRegex(@"^(yarin|bugun|dun|sonra|once|sabah|aksam|ogle|hafta|gun|ay|saat)")] private static partial Regex ZamanRe();
    [GeneratedRegex(@"^\s*(tane|adet|kere|kez|gun|saat|dakika|hafta|ay|yil|sene|parca)(\s|\z)")] private static partial Regex AdetRe();
    [GeneratedRegex(@"^\s+\z")] private static partial Regex BoslukRe();
    [GeneratedRegex(@"^[0-9]+\z")] private static partial Regex SadeceRakamRe();
    [GeneratedRegex(@"0[0-9]{10}")] private static partial Regex TelefonRe();
    [GeneratedRegex(@"\s")] private static partial Regex BoslukTumRe();
    [GeneratedRegex(@"\s+")] private static partial Regex BoslukRe2();
    [GeneratedRegex(@"([0-9][0-9.]*)\s*(tl|lira|₺)")] private static partial Regex TutarRe();
    [GeneratedRegex(@"([0-9][0-9.]*)\s*([tT][lL]|[lL][iIı][rR][aA]|₺)")] private static partial Regex TutarsizRe();
    [GeneratedRegex(@"(^|\s)([0-9]{1,3}(?:\.[0-9]{3})+|[0-9]+)(?=\s|\z)(?!\s+([tT][aA][nN][eE]|[aA][dD][eE][tT]))")] private static partial Regex CiplakRe();
    [GeneratedRegex(@"(^|\s)([0-9]{1,3}(?:\.[0-9]{3})+|[0-9]+)\s*\z")] private static partial Regex SonTutarRe();
    [GeneratedRegex(@"(^|\s)[0-9]+\s+(?:[yY][oO][kK]|[pP][aA][rR][dD][oO][nN]|[hH][aA][yY][ıI][rR]|[hH][aA][yY][iIı][rR]|[dD][eE][ğĞ][iIı][lL]|[dD][eE][gG][iIı][lL]|[yY][aA][nN][iIı])\s+([0-9]+)")] private static partial Regex DuzeltmeRe();
    [GeneratedRegex(@"([0-9]):(?=[0-9])")] private static partial Regex IkiNoktaRe();
    [GeneratedRegex(@"([0-9]),(?=[0-9])")] private static partial Regex VirgulRe();
    [GeneratedRegex(@"([0-9])\.(?=[0-9])")] private static partial Regex NoktaRe();
    [GeneratedRegex(@"[.!?;:,]+")] private static partial Regex NoktalamaRe();
    [GeneratedRegex(@"(^|\s)([0-9]{1,3})\s+[bB][iIı][nN](?:\s+([0-9]{1,3}))?(?=\s|\z)")] private static partial Regex BinRe();
    [GeneratedRegex(@"([A-Za-zÇĞİÖŞÜçğıöşü])([0-9])")] private static partial Regex HarfRakamRe();
    [GeneratedRegex(@"([0-9])([A-Za-zÇĞİÖŞÜçğıöşü])")] private static partial Regex RakamHarfRe();
    [GeneratedRegex(@"([0-9][0-9.]*)\s*(bin)?\s*(km|kilometre)")] private static partial Regex Km1Re();
    [GeneratedRegex(@"(?:km|kilometre)\s*([0-9][0-9.]*)\s*(bin)?")] private static partial Regex Km2Re();
    [GeneratedRegex(@"([0-9][0-9.]*)\s*([bB][iIı][nN])?\s*([kK][mM]|[kK][iIı][lL][oO][mM][eE][tT][rR][eE])")] private static partial Regex Km1CiRe();
    [GeneratedRegex(@"([kK][mM]|[kK][iIı][lL][oO][mM][eE][tT][rR][eE])\s*([0-9][0-9.]*)\s*([bB][iIı][nN])?")] private static partial Regex Km2CiRe();
    [GeneratedRegex(@"0\s*[0-9][0-9\s]{9,}")] private static partial Regex TelefonAtRe();
    [GeneratedRegex(@"(?:^|[^A-Za-zÇĞİÖŞÜçğıöşü])([A-ZÇĞİÖŞÜ][a-zçğıöşü]+\s+[A-ZÇĞİÖŞÜ][a-zçğıöşü]+)(?![A-Za-zçğıöşü])")] private static partial Regex SahipRe();
    [GeneratedRegex("bekliyor|hazir|hatirlat|selam|tesekkur|ozet")] private static partial Regex MesajMetniRe();
}
