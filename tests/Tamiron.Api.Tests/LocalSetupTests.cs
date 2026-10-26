using System.Net;
using Microsoft.Extensions.Configuration;
using Tamiron.Api;
using Tamiron.Infrastructure;

namespace Tamiron.Api.Tests;

/// <summary>
/// G1 · Yerel kurulum (K-079.3, K-080.1). SMS paketi yok: yerelde kod sunucu penceresine yazılır,
/// hesap tanımlı olsa bile gönderilmez (para). Arayüz API'yle aynı kökenden sunulur; tanınmayan
/// /api yolu arayüz sayfasına DÜŞMEZ, Türkçe 404 döner.
/// </summary>
public class LocalSetupTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static IConfiguration Netgsm() => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["Sms:Netgsm:UserCode"] = "u", ["Sms:Netgsm:Password"] = "p", ["Sms:Netgsm:Header"] = "TAMIRON",
    }).Build();

    [Theory]
    [InlineData("Development")]
    [InlineData("Local")]
    public void Development_and_local_write_the_code_to_the_server_window_even_with_an_account(string env)
        => Assert.Equal(typeof(DevLogSmsSender), Hosting.SmsSenderType(env, NetgsmOptions.From(Netgsm())));

    [Fact]
    public void Production_uses_the_account_only_when_it_is_complete()
    {
        Assert.Equal(typeof(NetgsmSmsSender), Hosting.SmsSenderType("Production", NetgsmOptions.From(Netgsm())));
        Assert.Equal(typeof(NotConfiguredSmsSender), Hosting.SmsSenderType("Production", null));
    }

    [Theory]
    [InlineData("Development", true)]
    [InlineData("Local", true)]
    [InlineData("Production", false)]
    [InlineData("Testing", false)]
    public void Only_development_and_local_migrate_on_start(string env, bool migrate)
        => Assert.Equal(migrate, Hosting.MigratesOnStart(env));

    [Fact]
    public void Only_development_writes_the_prototype_seed()
    {
        Assert.True(Hosting.WritesDevSeed("Development"));
        Assert.False(Hosting.WritesDevSeed("Local"));   // yerelde ustanın kendi dükkânı: boş başlar
    }

    [Fact]
    public async Task Unknown_api_path_is_a_turkish_404_not_the_app_page()
    {
        var r = await f.For(ApiFactory.ShopA).GetAsync("/api/boyle-bir-uc-yok");   // oturumsuz zaten 401
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
        Assert.DoesNotContain("text/html", r.Content.Headers.ContentType?.MediaType ?? "");
        Assert.Contains("Böyle bir istek yok.", await r.Content.ReadAsStringAsync());
    }
}
