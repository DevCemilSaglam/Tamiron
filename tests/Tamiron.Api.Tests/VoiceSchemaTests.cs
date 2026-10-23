using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Tamiron.Domain;

namespace Tamiron.Api.Tests;

/// <summary>
/// D15 · Sesli işlem şemaları (K-076.14). Şemalar ve korpus prototipin kendi çözümleyicisinden
/// (node tools/ses-sema-dok.js). Doğrulayıcı: işlem nesnesini KİM doldurursa doldursun (cihazdaki
/// çözümleyici ya da dil modeli, K-054) API uygulamadan önce şemaya karşı sınar.
/// </summary>
public class VoiceSchemaTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly ActionSchemas Schemas = ActionSchemas.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Ses", "islem-semalari.json")));
    static readonly JsonElement Korpus = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Esdeger", "prototip-ses.json"))).RootElement;

    static JsonElement J(string json) => JsonDocument.Parse(json).RootElement.Clone();

    [Fact]
    public void Every_prototype_output_passes_its_own_schema()
    {
        int checkedCount = 0, knownPlateGap = 0;
        foreach (var d in Korpus.GetProperty("korpus").EnumerateArray())
        {
            var islem = d.GetProperty("islem");
            if (islem.GetProperty("tur").GetString() == "plakasiz") continue;   // "anlaşılmadı" — şema değil
            var errors = Schemas.Validate(islem);
            // ⚠️ BİLİNEN PROTOTİP TUTARSIZLIĞI (K-077.9, Cemil'e soru): çözümleyici tek harfli 5 rakamlı
            // plakayı ("34 M 49109") tanıyor, şemanın deseni en çok 4 rakam diyor. Yalnız BU durum
            // sayılır ve geçer; başka her sapma kırmızı.
            if (errors.SequenceEqual(["plaka:bicim"]) && System.Text.RegularExpressions.Regex.IsMatch(
                    islem.GetProperty("plaka").GetString()!, @"^\d{2} [A-ZÇĞİÖŞÜ] \d{5}$"))
            { knownPlateGap++; continue; }
            Assert.True(errors.Count == 0, $"{d.GetProperty("cumle")} [{d.GetProperty("baglam")}] → {string.Join(", ", errors)}");
            checkedCount++;
        }
        Assert.True(checkedCount > 500, $"korpus beklenenden küçük: {checkedCount}");
        Assert.True(knownPlateGap > 0, "5 rakamlı plaka korpusta görünmüyor — K-077.9 çözüldüyse bu istisna kaldırılmalı");
    }

    [Fact]
    public void Schema_list_matches_the_prototype()
        => Assert.Equal(Korpus.GetProperty("turler").EnumerateArray().Select(t => t.GetString()), Schemas.Types);

    [Theory]
    [InlineData("{\"tur\":\"uydurma\"}", "tur:bilinmeyen")]
    [InlineData("{\"tur\":\"gider-ekle\",\"tutar\":2400}", "aciklama:zorunlu")]
    [InlineData("{\"tur\":\"gider-ekle\",\"aciklama\":\"\",\"tutar\":2400}", "aciklama:zorunlu")]          // boş metin = söylenmedi
    [InlineData("{\"tur\":\"gider-ekle\",\"aciklama\":\"kira\",\"tutar\":0}", "tutar:zorunlu")]          // para: 0 = söylenmedi
    [InlineData("{\"tur\":\"gider-ekle\",\"aciklama\":\"kira\",\"tutar\":\"bin\"}", "tutar:tip")]
    [InlineData("{\"tur\":\"gider-ekle\",\"aciklama\":\"kira\",\"tutar\":12.5}", "tutar:tip")]         // kuruşsuz
    [InlineData("{\"tur\":\"mesaj-gonder\",\"plaka\":\"34 ABC 123\",\"kanal\":\"faks\"}", "kanal:secenek")]
    [InlineData("{\"tur\":\"bilinmeyen-arac\",\"plaka\":\"34ABC123\"}", "plaka:bicim")]
    [InlineData("{\"tur\":\"bilinmeyen-arac\",\"plaka\":34}", "plaka:tip")]
    [InlineData("{\"kanal\":\"sms\"}", "tur:bilinmeyen")]
    public void Bad_objects_are_named_precisely(string json, string expected)
        => Assert.Contains(expected, Schemas.Validate(J(json)));

    [Fact]
    public void Extra_derived_fields_are_allowed()
        // Prototip türetilmiş alanlar ekler (duyulanPlaka, baskaIslem, arac…) — şemada yok, yasak değil.
        => Assert.Empty(Schemas.Validate(J("{\"tur\":\"bilinmeyen-arac\",\"plaka\":\"34 ABC 123\",\"duyulanPlaka\":\"34 QBC 123\"}")));

    [Fact]
    public async Task Api_serves_schemas_and_checks_an_action()
    {
        var api = f.For(ApiFactory.ShopB);
        var s = await api.GetFromJsonAsync<JsonElement>("/api/voice/schemas");
        Assert.Equal(Schemas.Types.Count, s.GetArrayLength());
        Assert.Equal("ara", s[0].GetProperty("name").GetString());

        var ok = await (await api.PostAsJsonAsync("/api/voice/check", J("{\"tur\":\"gider-ekle\",\"aciklama\":\"kira\",\"tutar\":850}"))).Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(ok.GetProperty("ok").GetBoolean());
        var bad = await (await api.PostAsJsonAsync("/api/voice/check", J("{\"tur\":\"gider-ekle\",\"tutar\":850}"))).Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(bad.GetProperty("ok").GetBoolean());
        Assert.Equal("aciklama:zorunlu", bad.GetProperty("errors")[0].GetString());
        Assert.Equal(HttpStatusCode.Unauthorized, (await f.CreateClient().GetAsync("/api/voice/schemas")).StatusCode);
    }
}
