using System.Globalization;

namespace Tamiron.Domain;

/// <summary>
/// Bir mesaj metnini üretmek için gerekenler. <see cref="CurrentJob"/> = aracın şu anki işi
/// (açık kayıt varsa o, yoksa sonuncusu — prototip aracinIsi); <see cref="LastRecord"/> =
/// son kayıt (sonKayit); <see cref="LastMaintenance"/> = teslim edilmiş son bakım (sonBakim).
/// </summary>
public sealed record MessageContext(Vehicle Vehicle, MessageTarget? Target, Shop Shop,
                                    ServiceRecord? CurrentJob, ServiceRecord? LastRecord, ServiceRecord? LastMaintenance);

/// <summary>
/// Mesaj şablonları — kural ve metin TEK YERDE (prototip MESAJ_SABLONLARI, mesajMetniUret,
/// HAZIR_METIN). Aynı kuralı mesaj ekranı ve kaydın süreç şeridi soruyor (K-019.1).
/// </summary>
public static class MessageTemplates
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

    /// <summary>Önerilerin sırası — prototipteki dizinin sırası.</summary>
    public static readonly string[] Order = ["ozet", "bekliyor", "hazir", "teslim", "hatirlatma", "serbest"];

    /// <summary>
    /// Hazır metinler (prototip HAZIR_METIN). Bayramın ADI yazılmaz: hangi bayram olduğunu
    /// bilen takvim yok. "Selam" "Merhaba" ile başlamaz — hitap zaten başta.
    /// </summary>
    private static readonly Dictionary<string, (string Name, string Body)> ReadyMade = new()
    {
        ["bayram"] = ("Bayram mesajı", "Bayramınız mübarek olsun. Sağlıklı, huzurlu ve kazasız günler dileriz."),
        ["selam"] = ("Selam mesajı", "nasılsınız? Aracınızla ilgili bir ihtiyacınız olursa bekleriz; iyi günler dileriz."),
        ["tesekkur"] = ("Teşekkür mesajı", "Bizi tercih ettiğiniz için teşekkür ederiz. İyi günlerde kullanın."),
    };

    private static readonly Dictionary<string, string> Names = new()
    {
        ["ozet"] = "İşin özeti", ["bekliyor"] = "Parça bekliyoruz", ["hazir"] = "Aracınız hazır",
        ["teslim"] = "Teslim edildi", ["hatirlatma"] = "Bakım hatırlatması", ["serbest"] = "Serbest mesaj",
    };

    /// <summary>Şablonun adı tek yerde — çip, onay ekranı ve arşiv satırı aynı adı görsün.</summary>
    public static string Name(string key)
        => Names.TryGetValue(key, out var n) ? n : ReadyMade.TryGetValue(key, out var h) ? h.Name : key;

    public static bool IsKnown(string key) => Names.ContainsKey(key) || ReadyMade.ContainsKey(key);

    /// <summary>
    /// Bu İŞE uygun şablonlar (kayıt bazlı, araç bazlı DEĞİL — 14.09: aynı aracın başka
    /// açık kaydı teslim edilmiş işe "hazır" dedirtiyordu). <paramref name="sentForJob"/>:
    /// bu kayıt için gönderilmiş mesajların şablonları.
    /// </summary>
    public static IReadOnlyList<string> Applicable(ServiceRecord? k, IReadOnlyCollection<string> sentForJob)
        => Order.Where(key => key switch
        {
            "ozet" => k is not null && k.DeliveredAt is null && !sentForJob.Contains("ozet"),
            "bekliyor" => k is not null && k.DeliveredAt is null && k.WaitingForPart,
            // A-M17: teslim edilmiş kayıtta "hazır" yalandı; yalnız açık işte.
            "hazir" => k is not null && k.DeliveredAt is null,
            // Cemil: "Teslim edilmiş ve mesaj gönderilmemiş araca" — HERHANGİ bir mesaj.
            "teslim" => k is not null && k.DeliveredAt is not null && sentForJob.Count == 0,
            "hatirlatma" => k is not null && k.DeliveredAt is not null && k.IsMaintenance && k.ReminderDate is not null && !k.ReminderSent,
            _ => true,   // serbest: öneri değil, her zaman açık kapı
        }).ToList();

    /// <summary>
    /// Şablonun tam metni: hitap + gövde + imza. Özet ve teslim LİNK taşır (T-20); link
    /// istenince kaydın özet kodu doğar (K-076.3).
    /// </summary>
    public static string Text(string key, MessageContext c)
    {
        var head = MessageText.Greeting(c.Target);
        var sig = MessageText.Signature(c.Shop);
        var arac = Car(c.Vehicle);
        if (ReadyMade.TryGetValue(key, out var h)) return head + h.Body + sig;
        switch (key)
        {
            case "bekliyor":
                // SÖZ VERİLMİYOR: parçanın ne zaman geleceğini biz de bilmiyoruz.
                return head + arac + " için parça bekliyoruz. Parça gelir gelmez işe başlayıp size haber vereceğiz." + sig;
            case "hazir":
                return head + arac + " hazır, teslim alabilirsiniz." + sig;
            case "teslim":
            {
                var tk = c.LastRecord;
                var tl = tk is not null && tk.Total != 0 ? "Toplam: " + Money(tk.Total) + " ₺\n" : "";
                return head + arac + " teslim edildi.\n" + tl +
                       (tk is not null ? "Ayrıntılı özet: " + CustomerSummary.Link(tk.EnsureSummaryCode()) + "\n" : "") +
                       "\nBizi tercih ettiğiniz için teşekkür ederiz." + sig;
            }
            case "serbest":
                // Hitap ve imza biliniyor; ustanın yazacağı yalnız orta kısım.
                return head + sig;
            case "hatirlatma":
            {
                var sb = c.LastMaintenance;
                var ek = sb?.DeliveredAt is { } d
                    ? $"Son bakımı {d:dd.MM.yyyy}" + (string.IsNullOrEmpty(sb.Km) ? "" : " · " + sb.Km + " km") + " idi. "
                    : "";
                return head + arac + " için bakım zamanı geldi. " + ek + "Uygun olduğunuzda bekleriz." + sig;
            }
            case "ozet":
            {
                var k = c.CurrentJob;
                // Prototipte bu cümle imzasız — davranış korunur.
                if (k is null) return head + arac + " için henüz bir kayıt yok.";
                var adet = k.Lines.Count;
                return head + arac + " için " + (adet > 0 ? adet + " kalem " : "") + "işlem yapıldı.\nToplam: " +
                       Money(k.Total) + " ₺\n\nAyrıntılı özet: " + CustomerSummary.Link(k.EnsureSummaryCode()) + sig;
            }
            default:
                throw new DomainException("message.unknown_template", "Böyle bir hazır mesaj yok.");
        }
    }

    /// <summary>Prototipin aracı: "Marka Model (plaka)". Marka/model boşsa yalnız plaka (K-076.4).</summary>
    private static string Car(Vehicle v)
    {
        var name = string.Join(" ", new[] { v.Brand, v.Model }.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s!.Trim()));
        return name.Length > 0 ? name + " (" + v.PlateNo + ")" : v.PlateNo;
    }

    /// <summary>JS toLocaleString('tr-TR'): binlik nokta, en çok üç ondalık, gereksiz sıfır yok.</summary>
    public static string Money(decimal n) => n.ToString("#,0.###", Tr);
}
