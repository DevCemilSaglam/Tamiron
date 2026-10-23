using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Tamiron.Application;
using Tamiron.Infrastructure;

namespace Tamiron.Api.Tests;

/// <summary>
/// E2 · Gemini ruhsat okuyucusu (K-077.5). AĞA ÇIKILMAZ — sahte HTTP işleyici. İstek, ölçümde
/// kullanılanla (tools/degerlendirme/foto-kos.js + ruhsat-seti.json: şase 46/46, K-067.2b) AYNI
/// olmalı: talimat ya da şema değişirse ölçülen doğruluk artık bu kodu anlatmaz.
/// </summary>
public class GeminiReaderTests
{
    sealed class Fake(HttpStatusCode status, string reply) : HttpMessageHandler
    {
        public HttpRequestMessage? Req;
        public string? Body;
        public int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
        {
            Calls++; Req = r; Body = await r.Content!.ReadAsStringAsync(ct);
            return new HttpResponseMessage(status) { Content = new StringContent(reply, Encoding.UTF8, "application/json") };
        }
    }

    static readonly GeminiOptions Opt = new("anahtar-123", "gemini-3.8-flash");

    static string Reply(object fields) => JsonSerializer.Serialize(new
    {
        candidates = new[] { new { content = new { parts = new object[] { new { text = "düşünce", thought = true }, new { text = JsonSerializer.Serialize(fields) } } } } }
    });

    static string Root()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d is not null && !File.Exists(Path.Combine(d.FullName, "Tamiron.sln"))) d = d.Parent;
        return d!.FullName;
    }

    [Fact]
    public async Task Request_is_the_measured_one()
    {
        var h = new Fake(HttpStatusCode.OK, Reply(new { plaka = "34 ABC 123" }));
        await new GeminiRegistrationReader(new HttpClient(h), Opt).ReadAsync([1, 2, 3], "image/jpeg", default);

        Assert.Equal("https://generativelanguage.googleapis.com/v1beta/models/gemini-3.8-flash:generateContent", h.Req!.RequestUri!.ToString());
        Assert.Equal("anahtar-123", h.Req.Headers.GetValues("x-goog-api-key").Single());   // anahtar URL'de DEĞİL, başlıkta
        var b = JsonDocument.Parse(h.Body!).RootElement;

        // Talimat: ölçüm aracındakiyle birebir.
        var tool = File.ReadAllText(Path.Combine(Root(), "tools", "degerlendirme", "foto-kos.js"));
        var measured = Regex.Match(tool, @"ruhsat: \{[\s\S]*?sistem: `([\s\S]*?)`").Groups[1].Value;
        Assert.Equal(measured.Replace("\r\n", "\n"), b.GetProperty("systemInstruction").GetProperty("parts")[0].GetProperty("text").GetString());

        var parts = b.GetProperty("contents")[0].GetProperty("parts");
        Assert.Equal(("image/jpeg", "AQID"), (parts[0].GetProperty("inlineData").GetProperty("mimeType").GetString(), parts[0].GetProperty("inlineData").GetProperty("data").GetString()));
        Assert.Equal("Bu ruhsatın alanlarını çıkar. Okuyamadığın alanı boş metin (\"\") bırak; uydurma.", parts[1].GetProperty("text").GetString());

        // Şema: ölçüt setinin alanları, açıklamaları ("— kişisel…" modele gitmez), hepsi zorunlu.
        var gc = b.GetProperty("generationConfig");
        Assert.Equal(("application/json", 0), (gc.GetProperty("responseMimeType").GetString(), gc.GetProperty("temperature").GetInt32()));
        var set = JsonDocument.Parse(File.ReadAllText(Path.Combine(Root(), "tools", "degerlendirme", "ruhsat-seti.json"))).RootElement.GetProperty("alanlar");
        var props = gc.GetProperty("responseSchema").GetProperty("properties");
        Assert.Equal(set.EnumerateObject().Select(p => p.Name), props.EnumerateObject().Select(p => p.Name));
        foreach (var p in set.EnumerateObject())
            Assert.Equal(p.Value.GetString()!.Split(" — kişisel")[0], props.GetProperty(p.Name).GetProperty("description").GetString());
        Assert.Equal(set.EnumerateObject().Select(p => p.Name), gc.GetProperty("responseSchema").GetProperty("required").EnumerateArray().Select(x => x.GetString()));
        Assert.False(gc.GetProperty("responseSchema").TryGetProperty("additionalProperties", out _));   // Gemini'de yok
    }

    [Fact]
    public async Task Reply_maps_to_the_registration_fields_and_empty_becomes_null()
    {
        var h = new Fake(HttpStatusCode.OK, Reply(new
        {
            plaka = "34 ABC 123", ilkTescil = "02.03.2021", marka = "RENAULT", tip = "BFB", ticariAd = "MEGANE", modelYili = "2018",
            cinsi = "OTOMOBİL", renk = "GRİ (METALİK)", motorNo = "K9KA636", sase = "VF1RFB00858123456", silindir = "1461", guc = "81",
            yakit = "DİZEL", koltuk = "5", tcVergiNo = "12345678901", unvanSoyad = "KAYA", ad = "MEHMET", adres = ""
        }));
        var r = await new GeminiRegistrationReader(new HttpClient(h), Opt).ReadAsync([1], "image/png", default);
        Assert.Equal(("34 ABC 123", "RENAULT", "MEGANE", "2018", "GRİ (METALİK)", "DİZEL"), (r.Plate, r.Brand, r.Model, r.Year, r.Color, r.FuelType));
        Assert.Equal(("VF1RFB00858123456", "K9KA636", "1461", "81", "5", "12345678901"), (r.Vin, r.EngineNo, r.Displacement, r.PowerKw, r.Seats, r.TaxOrNationalId));
        Assert.Equal(new DateOnly(2021, 3, 2), r.FirstRegistrationDate);
        Assert.Null(r.Address);                // "" → okunamadı
        Assert.Null(r.Engine);                 // ölçülen şemada motor alanı yok — usta seçer
    }

    [Fact]
    public async Task Unreadable_date_is_dropped_not_guessed()
    {
        var h = new Fake(HttpStatusCode.OK, Reply(new { plaka = "34 ABC 123", ilkTescil = "2021?" }));
        Assert.Null((await new GeminiRegistrationReader(new HttpClient(h), Opt).ReadAsync([1], "image/png", default)).FirstRegistrationDate);
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests, "{\"error\":{\"message\":\"Your prepayment credits are depleted\"}}", "registration.reader_credit")]
    [InlineData(HttpStatusCode.Forbidden, "{\"error\":{\"message\":\"API key not valid\"}}", "registration.reader_auth")]
    [InlineData(HttpStatusCode.InternalServerError, "{}", "registration.reader_failed")]
    [InlineData(HttpStatusCode.OK, "{\"candidates\":[]}", "registration.reader_failed")]
    public async Task Failures_are_named_and_never_retried_in_a_loop(HttpStatusCode s, string body, string code)
    {
        var h = new Fake(s, body);
        var e = await Assert.ThrowsAsync<ReaderFailedException>(() => new GeminiRegistrationReader(new HttpClient(h), Opt).ReadAsync([1], "image/png", default));
        Assert.Equal(code, e.Code);
        Assert.Equal(1, h.Calls);   // her deneme para: istek içinde döngü YOK, usta yeniden basar
    }

    static IConfiguration Conf(params (string k, string v)[] kv)
        => new ConfigurationBuilder().AddInMemoryCollection(kv.Select(x => new KeyValuePair<string, string?>(x.k, x.v))).Build();

    [Fact]
    public void Needs_both_the_key_and_an_explicit_switch()
    {
        Assert.Null(GeminiOptions.From(Conf(("Gemini:ApiKey", "k"))));                                // anahtar tek başına para harcatmaz
        Assert.Null(GeminiOptions.From(Conf(("Gemini:Enabled", "true"))));
        Assert.Equal(new GeminiOptions("k", "gemini-3.8-flash"), GeminiOptions.From(Conf(("Gemini:ApiKey", "k"), ("Gemini:Enabled", "true"))));
        Assert.Equal("gemini-x", GeminiOptions.From(Conf(("Gemini:ApiKey", "k"), ("Gemini:Enabled", "true"), ("Gemini:Model", "gemini-x")))!.Model);
        Assert.DoesNotContain("anahtar-123", Opt.ToString());
    }
}
