using System.Net;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Tamirhane.Ui.Pages;
using Tamiron.Ui.Core;

namespace Tamiron.Ui.Tests;

/// <summary>
/// G1 · Ana menü (EKRANLAR §0, prototip 10-home). Satır altındaki sayılar API'nin `GET /api/home`
/// özetinden; ekran yalnız yazar. Bakım hatırlatması ana ekranda YOK (K-019.1) — yalnız rozet.
/// </summary>
public class HomeScreenTests : TestContext
{
    readonly FakeApi api = new();
    readonly MemoryTokens tokens = new("t-1");

    public HomeScreenTests()
    {
        Services.AddSingleton<ITokenStore>(tokens);
        Services.AddSingleton(new TamironApi(new HttpClient(api) { BaseAddress = new Uri("http://api.test/") }, tokens));
    }

    NavigationManager Nav => Services.GetRequiredService<NavigationManager>();

    const string Ozet = "{\"inShopCount\":2,\"recordCount\":8,\"vehicleCount\":4,\"customerCount\":3,"
                      + "\"collectedToday\":1250,\"receivable\":3450,\"dueReminderCount\":1}";

    [Fact]
    public void Without_session_goes_to_login()
    {
        tokens.Token = null;
        RenderComponent<AnaMenu>();
        Assert.Equal("http://localhost/giris", Nav.Uri);
        Assert.Empty(api.Calls);
    }

    [Fact]
    public void Menu_rows_show_the_counts_the_api_gives_in_prototype_wording()
    {
        api.Routes["GET /api/home"] = (HttpStatusCode.OK, Ozet);
        var c = RenderComponent<AnaMenu>();
        c.WaitForAssertion(() => Assert.Equal("2 araç serviste · 8 kayıt", c.Find("#menuKayitlarAlt").TextContent));
        Assert.Equal("4 araç kayıtlı", c.Find("#menuAraclarAlt").TextContent);
        Assert.Equal("3 müşteri", c.Find("#menuMusterilerAlt").TextContent);
        Assert.Equal("Bugün 1.250 ₺ · 3.450 ₺ veresiye", c.Find("#menuMuhasebeAlt").TextContent);
        Assert.Equal("Bearer t-1", api.Calls[0].Auth);
    }

    [Fact]
    public void No_vehicle_in_shop_is_said_plainly()
    {
        api.Routes["GET /api/home"] = (HttpStatusCode.OK, Ozet.Replace("\"inShopCount\":2", "\"inShopCount\":0"));
        var c = RenderComponent<AnaMenu>();
        c.WaitForAssertion(() => Assert.Equal("8 kayıt · serviste araç yok", c.Find("#menuKayitlarAlt").TextContent));
    }

    [Fact]
    public void Greeting_follows_the_hour_and_addresses_the_usta()
    {
        // Prototip SELAMLAR havuzu: saatsizler her zaman, saatliler kendi penceresinde, cuma yalnız cuma.
        var sabah = Selam.Uygunlar(new DateTime(2026, 10, 26, 8, 0, 0));
        Assert.Contains("Günaydın", sabah);
        Assert.Contains("Kolay gelsin", sabah);
        Assert.DoesNotContain("İyi akşamlar", sabah);
        Assert.DoesNotContain("Hayırlı cumalar", sabah);          // pazartesi
        Assert.Contains("Hayırlı geceler", Selam.Uygunlar(new DateTime(2026, 10, 26, 2, 0, 0)));   // 23→5 gece sarar
        Assert.Contains("Hayırlı cumalar", Selam.Uygunlar(new DateTime(2026, 10, 30, 13, 0, 0)));
        var m = Selam.Metni(new DateTime(2026, 10, 26, 19, 0, 0));
        Assert.EndsWith(", usta", m);
        Assert.Contains(m[..^", usta".Length], Selam.Uygunlar(new DateTime(2026, 10, 26, 19, 0, 0)));
    }

    [Fact]
    public void Rows_go_to_their_screens()
    {
        api.Routes["GET /api/home"] = (HttpStatusCode.OK, Ozet);
        var c = RenderComponent<AnaMenu>();
        c.WaitForElement("#menuKayitlar").Click();
        Assert.Equal("http://localhost/kayitlar", Nav.Uri);
    }

    [Fact]
    public void Expired_session_returns_to_login()
    {
        api.Routes["GET /api/home"] = (HttpStatusCode.Unauthorized, "{\"title\":\"Oturum yok.\"}");
        RenderComponent<AnaMenu>();
        Assert.Equal("http://localhost/giris", Nav.Uri);
        Assert.Null(tokens.Token);
    }

    [Fact]
    public void Logout_ends_the_session_here_and_goes_to_login()
    {
        api.Routes["GET /api/home"] = (HttpStatusCode.OK, Ozet);
        api.Routes["POST /api/auth/logout"] = (HttpStatusCode.NoContent, "");
        var c = RenderComponent<AnaMenu>();
        c.WaitForElement("#cikis").Click();
        c.WaitForAssertion(() => Assert.Equal("http://localhost/giris", Nav.Uri));
        Assert.Null(tokens.Token);
        Assert.Contains(api.Calls, x => x.Path == "/api/auth/logout" && x.Auth == "Bearer t-1");
    }
}

/// <summary>G1 · Yeni dükkân (D6, K-076.6): kayıtlı olmayan numara girişte kaybolmaz, kayda yönlenir.</summary>
public class SignupScreenTests : TestContext
{
    readonly FakeApi api = new();
    readonly MemoryTokens tokens = new();

    public SignupScreenTests()
    {
        Services.AddSingleton<ITokenStore>(tokens);
        Services.AddSingleton(new TamironApi(new HttpClient(api) { BaseAddress = new Uri("http://api.test/") }, tokens));
    }

    NavigationManager Nav => Services.GetRequiredService<NavigationManager>();

    [Fact]
    public void Unknown_phone_offers_shop_signup_and_signs_in()
    {
        api.Routes["POST /api/auth/code"] = (HttpStatusCode.UnprocessableEntity, "{\"code\":\"auth.unknown_phone\",\"title\":\"Bu numarayla kayıtlı dükkân yok.\"}");
        api.Routes["POST /api/auth/signup/code"] = (HttpStatusCode.Accepted, "");
        api.Routes["POST /api/auth/signup"] = (HttpStatusCode.OK, "{\"token\":\"t-9\",\"shopName\":\"Kaya Oto\",\"userName\":null}");
        var c = RenderComponent<Giris>();
        c.Find("#grTel").Input("0532 111 22 33");
        c.Find("#grKodIste").Click();
        c.WaitForElement("#grKaydol").Click();
        c.Find("#grDukkan").Input("Kaya Oto");
        c.Find("#grKayitKodIste").Click();
        c.WaitForElement("#grKod").Input("654321");
        c.Find("#grGiris").Click();
        c.WaitForAssertion(() => Assert.Equal("http://localhost/", Nav.Uri));
        Assert.Equal("t-9", tokens.Token);
        var signup = api.Calls.Single(x => x.Path == "/api/auth/signup");
        Assert.Contains("\"shopName\":\"Kaya Oto\"", signup.Body);
        Assert.Contains("\"code\":\"654321\"", signup.Body);
    }

    [Fact]
    public void Signup_says_what_is_missing_and_sends_nothing()
    {
        api.Routes["POST /api/auth/code"] = (HttpStatusCode.UnprocessableEntity, "{\"code\":\"auth.unknown_phone\",\"title\":\"Bu numarayla kayıtlı dükkân yok.\"}");
        var c = RenderComponent<Giris>();
        c.Find("#grTel").Input("0532 111 22 33");
        c.Find("#grKodIste").Click();
        c.WaitForElement("#grKaydol").Click();
        c.Find("#grKayitKodIste").Click();
        Assert.Equal("Dükkânının adını yaz.", c.Find("#grHata").TextContent.Trim());
        Assert.DoesNotContain(api.Calls, x => x.Path.StartsWith("/api/auth/signup"));
    }
}
