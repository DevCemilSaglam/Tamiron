using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Tamiron.Ui.Core.Ses;

namespace Tamiron.Ui.Tests;

/// <summary>
/// E3/E4 · Cihazdaki C# ses çözümleyicisi = prototip (K-077.1). Esdeger/prototip-ses.json'daki her
/// cümle × bağlam, korpusu üreten tohumun AYNISIYLA kurulan dünyada çözülür ve prototipin işlem
/// nesnesiyle BİREBİR karşılaştırılır. Ölçü: 0 fark.
/// Tek istisna mesaj-gonder'ın gövde metni: o çözümleyicinin değil sunucunun işi (MessageTemplates,
/// D4) — burada yalnız metnin DOĞRU plaka ve şablonla istendiği sınanır.
/// </summary>
public class SesEsdegerlikTests
{
    static string Root()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d is not null && !File.Exists(Path.Combine(d.FullName, "Tamiron.sln"))) d = d.Parent;
        return d!.FullName;
    }

    static JsonElement Json(params string[] yol) => JsonDocument.Parse(File.ReadAllText(Path.Combine([Root(), .. yol]))).RootElement;

    /// <summary>Korpusu üreten prototip tohumu: ARACLAR ve KAYITLAR, prototipteki sırayla.</summary>
    public static SesDunya Dunya()
    {
        var t = Json("src", "Tamiron.Api", "Seed", "prototip-tohum.json");
        static string? S(JsonElement o, string k) => o.TryGetProperty(k, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
        var araclar = t.GetProperty("araclar").EnumerateArray()
            .Select(a => new SesArac(S(a, "plaka")!, S(a, "marka"), S(a, "model"), S(a, "sahipAd"))).ToList();
        var kayitlar = t.GetProperty("kayitlar").EnumerateArray()
            .Select(k => new SesKayit(k.GetProperty("id").GetInt32().ToString(), S(k, "plaka")!, S(k, "teslimTarihi") is not null,
                k.GetProperty("satirlar").EnumerateArray().Select(s => S(s, "ad") ?? "").ToList(), S(k, "odemeYolu"))).ToList();
        return new SesDunya(araclar, kayitlar, (plaka, sablon) => $"<<{plaka}|{sablon}>>");
    }

    /// <summary>Karşılaştırma biçimi: anahtarlar sıralı; kayıt kimliği metin; mesaj-gonder gövdesi "istendi mi"ye indirgenir.</summary>
    static string Kanon(JsonNode? n)
    {
        switch (n)
        {
            case JsonObject o:
                var tur = o["tur"]?.ToString();
                var b = new StringBuilder("{");
                foreach (var (k, v) in o.OrderBy(p => p.Key, StringComparer.Ordinal))
                {
                    var deger = v;
                    if (k == "kayit" && v is JsonValue kv) deger = JsonValue.Create(kv.ToString());
                    if (k == "govde" && tur == "mesaj-gonder") deger = JsonValue.Create("<<" + o["plaka"] + "|" + o["sablon"] + ">>");
                    b.Append(JsonSerializer.Serialize(k)).Append(':').Append(Kanon(deger)).Append(',');
                }
                return b.Append('}').ToString();
            case null: return "null";
            default: return n.ToJsonString(new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        }
    }

    [Fact]
    public void Every_corpus_case_matches_the_prototype()
    {
        var cz = new SesCozumleyici(Dunya());
        var korpus = Json("tests", "Tamiron.Api.Tests", "Esdeger", "prototip-ses.json").GetProperty("korpus");
        var farklar = new List<string>();
        var n = 0;
        foreach (var d in korpus.EnumerateArray())
        {
            n++;
            var cumle = d.GetProperty("cumle").GetString()!;
            var baglam = d.GetProperty("baglam").GetString()!;
            var beklenen = Kanon(JsonNode.Parse(d.GetProperty("islem").GetRawText()));
            string gercek;
            var kayit = baglam != "kayit-icinde" ? null : d.TryGetProperty("kayit", out var kv) ? kv.GetInt32().ToString() : "1";
            try { gercek = Kanon(cz.Cozumle(cumle, baglam, kayit)); }
            catch (Exception e) { gercek = "HATA " + e.GetType().Name + ": " + e.Message; }
            if (gercek != beklenen) farklar.Add($"[{baglam}] \"{cumle}\"\n   beklenen {beklenen}\n   gerçek   {gercek}");
        }
        Assert.True(n > 700, $"korpus beklenenden küçük: {n}");
        Assert.True(farklar.Count == 0, $"{farklar.Count}/{n} fark:\n" + string.Join("\n", farklar.Take(25)));
    }
}
