using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Tamiron.Application;
using Tamiron.Domain;

namespace Tamiron.Infrastructure;

/// <summary>
/// Gemini hesabı (K-077.5). ORTAM DEĞİŞKENİNDEN, depoya girmez: <c>Gemini__ApiKey</c>,
/// <c>Gemini__Enabled=true</c> (ikisi birden — anahtarın tanımlı olması tek başına para harcatmaz),
/// isteğe bağlı <c>Gemini__Model</c> (ölçülen: gemini-3.8-flash, K-067.2b).
/// </summary>
public sealed record GeminiOptions(string ApiKey, string Model)
{
    public const string MeasuredModel = "gemini-3.8-flash";

    public static GeminiOptions? From(IConfiguration c)
    {
        var s = c.GetSection("Gemini");
        var key = s["ApiKey"];
        if (string.IsNullOrWhiteSpace(key) || !string.Equals(s["Enabled"], "true", StringComparison.OrdinalIgnoreCase)) return null;
        return new(key.Trim(), string.IsNullOrWhiteSpace(s["Model"]) ? MeasuredModel : s["Model"]!.Trim());
    }

    /// <summary>Anahtar günlüğe/hata metnine düşmesin.</summary>
    public override string ToString() => $"GeminiOptions {{ Model = {Model}, ApiKey = *** }}";
}

/// <summary>
/// Ruhsat okuyucusu — Gemini ücretli katman (E2, K-077.5). İstek ÖLÇÜMDE KULLANILANLA AYNI
/// (tools/degerlendirme/foto-kos.js + ruhsat-seti.json; şase 46/46, ruhsat başına ~0,48 ¢ — K-067.2b);
/// bir test bunu sınar. Bu dosyanın talimat ve alan kısmı o ikisinden ÜRETİLDİ.
/// Fotoğraf yalnız bu istekte gider, saklanmaz.
/// ⚠️ Gerçek hesapla bu koddan HİÇ çağrılmadı — ilk çağrı Cemil'in onayıyla.
/// İstek içinde YENİDEN DENEME YOK: her deneme para; başarısızlıkta usta yeniden basar ya da elle yazar.
/// </summary>
public sealed class GeminiRegistrationReader(HttpClient http, GeminiOptions o) : IRegistrationReader
{
    /// <summary>Ölçülen sistem talimatı — foto-kos.js'teki ruhsat talimatıyla birebir (testli).</summary>
    public const string Instruction = """
Sen bir tamirhane programının ruhsat okuyucususun. Türk araç
tescil belgesinin (ruhsat) fotoğrafını alır, alanlarını çıkarırsın.

KURALLAR:
1. YALNIZ BELGEDE YAZANI OKU. Aracın ne olması gerektiğini bilmen
   önemli değil; belgede ne yazıyorsa o. Belgedeki rakam sana tuhaf
   gelse bile düzeltme.
2. OKUYAMADIĞIN ALANI BOŞ BIRAK (null). Parmak kapatıyorsa, silikse,
   kırpılmışsa: null. Uydurma. Yanlış bilgi, eksik bilgiden çok daha
   pahalıdır — usta onu bir daha kontrol etmez.
3. BELGEDE OLMAYAN ALANI BOŞ BIRAK. Eski tip ruhsatlarda yakıt cinsi,
   motor gücü, silindir hacmi ve koltuk sayısı YOKTUR.
4. TC KİMLİK NO / VERGİ NO, SOYADI-ÜNVAN, ADI ve ADRESİ DE OKU.
   Bunlar faturanın kesileceği bilgiler (K-050). Kişide 11 haneli TC,
   firmada 10 haneli vergi numarası olur.
5. VERGİ DAİRESİ ruhsatta YAZMAZ — böyle bir alan döndürme.
6. Plakayı boşluklu yaz: "33VAH34" değil "33 VAH 34".
7. Belge yan ya da ters çekilmiş olabilir; döndürüp oku.
""";

    public const string Ask = "Bu ruhsatın alanlarını çıkar. Okuyamadığın alanı boş metin (\"\") bırak; uydurma.";

    /// <summary>Ölçüt setinin alanları ve açıklamaları (ruhsat-seti.json; "— kişisel" notu modele gitmez).</summary>
    public static readonly (string Name, string Description)[] Fields =
    [
        ("plaka", "(A) PLAKA — boşluklu normal hâle getirilir: '33VAH34' -> '33 VAH 34'"),
        ("ilkTescil", "(B) İLK TESCİL TARİHİ"),
        ("marka", "(D.1) MARKASI"),
        ("tip", "(D.2) TİPİ"),
        ("ticariAd", "(D.3) TİCARİ ADI"),
        ("modelYili", "(D.4) MODEL YILI"),
        ("cinsi", "(D.5) CİNSİ"),
        ("renk", "(R) RENGİ — parantezli ton dahil, belgede nasılsa öyle"),
        ("motorNo", "(P.5) MOTOR NO"),
        ("sase", "(E) ŞASE NO"),
        ("silindir", "(P.1) SİLİNDİR HACMİ"),
        ("guc", "(P.2) MOTOR GÜCÜ (kW)"),
        ("yakit", "(P.3) YAKIT CİNSİ"),
        ("koltuk", "(S.1) KOLTUK SAYISI"),
        ("tcVergiNo", "(Y.4) TC KİMLİK NO / VERGİ NO"),
        ("unvanSoyad", "(C.1.1) SOYADI / TİCARİ ÜNVANI"),
        ("ad", "(C.1.2) ADI"),
        ("adres", "(C.1.3) ADRESİ"),
    ];

    public async Task<RegistrationRead> ReadAsync(byte[] image, string mediaType, CancellationToken ct)
    {
        var props = new JsonObject();
        foreach (var (n, d) in Fields) props[n] = new JsonObject { ["type"] = "string", ["description"] = d };
        var body = new JsonObject
        {
            // Depo CRLF açılsa da ölçülen metin (LF) gider.
            ["systemInstruction"] = new JsonObject { ["parts"] = new JsonArray(new JsonObject { ["text"] = Instruction.Replace("\r\n", "\n") }) },
            ["contents"] = new JsonArray(new JsonObject
            {
                ["role"] = "user",
                ["parts"] = new JsonArray(
                    new JsonObject { ["inlineData"] = new JsonObject { ["mimeType"] = mediaType, ["data"] = Convert.ToBase64String(image) } },
                    new JsonObject { ["text"] = Ask })
            }),
            ["generationConfig"] = new JsonObject
            {
                ["responseMimeType"] = "application/json",
                // Hepsi zorunlu: isteğe bağlı bırakılınca model temiz ruhsatta bile alan atlıyordu (18.10 ölçümü).
                ["responseSchema"] = new JsonObject
                {
                    ["type"] = "object", ["properties"] = props,
                    ["required"] = new JsonArray(Fields.Select(f => (JsonNode)f.Name).ToArray())
                },
                ["temperature"] = 0
            }
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, $"https://generativelanguage.googleapis.com/v1beta/models/{o.Model}:generateContent")
        {
            Content = new StringContent(body.ToJsonString(), System.Text.Encoding.UTF8, "application/json")
        };
        req.Headers.Add("x-goog-api-key", o.ApiKey);   // anahtar URL'de değil: sunucu günlüklerine düşmesin

        HttpResponseMessage res;
        try { res = await http.SendAsync(req, ct); }
        catch (HttpRequestException) { throw new ReaderFailedException("registration.reader_failed"); }
        using (res)
        {
            var text = await res.Content.ReadAsStringAsync(ct);
            if (!res.IsSuccessStatusCode)
            {
                // "Kredi bitti" 429 da olabilir (16.10 ses koşusu) — metinden ayırt edilir.
                if (Regex.IsMatch(text, "credit|prepay|billing|quota exceeded for", RegexOptions.IgnoreCase))
                    throw new ReaderFailedException("registration.reader_credit");
                if (res.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                    throw new ReaderFailedException("registration.reader_auth");
                throw new ReaderFailedException("registration.reader_failed");
            }
            return Map(text);
        }
    }

    /// <summary>Yanıt → alanlar. Düşünce parçaları atlanır; boş metin = okunamadı (null).</summary>
    public static RegistrationRead Map(string response)
    {
        JsonElement f;
        try
        {
            var parts = JsonDocument.Parse(response).RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts");
            var json = string.Concat(parts.EnumerateArray()
                .Where(p => !(p.TryGetProperty("thought", out var t) && t.ValueKind == JsonValueKind.True))
                .Select(p => p.TryGetProperty("text", out var x) ? x.GetString() : "")).Trim();
            f = JsonDocument.Parse(json).RootElement;
        }
        catch (Exception e) when (e is KeyNotFoundException or IndexOutOfRangeException or InvalidOperationException or JsonException)
        {
            throw new ReaderFailedException("registration.reader_failed");
        }
        string? S(string k) => f.TryGetProperty(k, out var v) && v.ValueKind == JsonValueKind.String && v.GetString()!.Trim().Length > 0 ? v.GetString()!.Trim() : null;
        DateOnly? D(string? s) => s is not null && DateOnly.TryParseExact(s, ["dd.MM.yyyy", "d.M.yyyy", "dd/MM/yyyy", "yyyy-MM-dd"],
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;
        // Eşleme prototipin ruhsat eşlemesiyle aynı (K-076.8): sahip adı/unvan doldurulmaz; ölçülen şemada motor alanı yok.
        return new RegistrationRead(Plate: S("plaka"), Brand: S("marka"), Model: S("ticariAd"), Year: S("modelYili"),
            Color: S("renk"), FuelType: S("yakit"), Vin: S("sase"), EngineNo: S("motorNo"), Displacement: S("silindir"),
            PowerKw: S("guc"), Seats: S("koltuk"), FirstRegistrationDate: D(S("ilkTescil")),
            TaxOrNationalId: S("tcVergiNo"), Address: S("adres"));
    }
}
