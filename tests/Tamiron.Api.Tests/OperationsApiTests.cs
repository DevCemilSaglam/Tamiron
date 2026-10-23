using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace Tamiron.Api.Tests;

/// <summary>
/// D10 · Güvenlik/işletim (K-076.10): giriş ve özet uçlarında IP başına hız sınırı, CORS yalnız
/// yapılandırılan kökenlere, sağlık ucu. Sınırlar yapılandırmadan — ortak fabrikada yüksek,
/// burada düşük (test sunucusunda bütün istekler aynı "IP"den gelir).
/// </summary>
public class OperationsApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Auth_endpoints_are_rate_limited_per_ip_with_a_turkish_429()
    {
        using var low = f.WithWebHostBuilder(b => b.UseSetting("RateLimit:AuthPerMinute", "3"));
        var c = low.CreateClient();
        for (var i = 0; i < 3; i++)   // kayıtsız numara: 422, ama SAYILIR (deneme denemedir)
            Assert.Equal(HttpStatusCode.UnprocessableEntity, (await c.PostAsJsonAsync("/api/auth/code", new { phone = "05449990001" })).StatusCode);
        var res = await c.PostAsJsonAsync("/api/auth/verify", new { phone = "05449990001", code = "123456" });
        Assert.Equal(HttpStatusCode.TooManyRequests, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("request.too_many", body.GetProperty("code").GetString());
        Assert.Equal("Çok sık denendi. Bir dakika sonra tekrar dene.", body.GetProperty("title").GetString());

        // Oturumlu iş uçları bu sınıra GİRMEZ: usta gün boyu çalışırken kesilmesin.
        var api = low.CreateClient();
        api.DefaultRequestHeaders.Authorization = f.For(ApiFactory.ShopB).DefaultRequestHeaders.Authorization;
        for (var i = 0; i < 5; i++) Assert.Equal(HttpStatusCode.OK, (await api.GetAsync("/api/suppliers")).StatusCode);
    }

    [Fact]
    public async Task Public_summary_is_rate_limited_too()
    {
        using var low = f.WithWebHostBuilder(b => b.UseSetting("RateLimit:PublicPerMinute", "2"));
        var c = low.CreateClient();
        Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync("/api/o/abcdefghjkmnpqrs")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync("/api/o/abcdefghjkmnpqrt")).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await c.GetAsync("/api/o/abcdefghjkmnpqru")).StatusCode);   // kod denemek yavaşlar
    }

    [Fact]
    public async Task Cors_allows_only_configured_origins_and_preflight_needs_no_session()
    {
        using var app = f.WithWebHostBuilder(b => b.UseSetting("Cors:Origins", "https://app.tamiron.test"));
        var c = app.CreateClient();

        var pre = new HttpRequestMessage(HttpMethod.Options, "/api/records");
        pre.Headers.Add("Origin", "https://app.tamiron.test");
        pre.Headers.Add("Access-Control-Request-Method", "GET");
        pre.Headers.Add("Access-Control-Request-Headers", "authorization");
        var ok = await c.SendAsync(pre);
        Assert.Equal(HttpStatusCode.NoContent, ok.StatusCode);   // 401 DEĞİL: ön sorgu oturum taşımaz
        Assert.Equal("https://app.tamiron.test", ok.Headers.GetValues("Access-Control-Allow-Origin").Single());

        var bad = new HttpRequestMessage(HttpMethod.Options, "/api/records");
        bad.Headers.Add("Origin", "https://kotu.example");
        bad.Headers.Add("Access-Control-Request-Method", "GET");
        Assert.False((await c.SendAsync(bad)).Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task No_origin_configured_means_no_cross_origin()
    {
        var pre = new HttpRequestMessage(HttpMethod.Options, "/api/records");
        pre.Headers.Add("Origin", "https://app.tamiron.test");
        pre.Headers.Add("Access-Control-Request-Method", "GET");
        Assert.False((await f.CreateClient().SendAsync(pre)).Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Health_is_open_and_checks_the_database()
    {
        var res = await f.CreateClient().GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("ok", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("status").GetString());

        using var broken = f.WithWebHostBuilder(b => b.UseSetting("ConnectionStrings:Tamiron",
            @"Server=(localdb)\MSSQLLocalDB;Database=TamironYok_" + Guid.NewGuid().ToString("N")[..8] + ";Trusted_Connection=True;Connect Timeout=5"));
        var down = await broken.CreateClient().GetAsync("/health");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, down.StatusCode);
        Assert.DoesNotContain("Server=", await down.Content.ReadAsStringAsync());   // ayrıntı sızmaz
    }
}
