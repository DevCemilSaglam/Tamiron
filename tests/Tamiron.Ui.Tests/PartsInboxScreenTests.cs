using System.Net;
using System.Text.Json;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Ui.Core;
using Tamiron.YedekParca.Ui.Pages;

namespace Tamiron.Ui.Tests;

/// <summary>
/// H7 · Yedek Parça — gelen sorular (K-091, TASARIM-H-YEDEK-PARCA §4). Parçacı kendi numarasına sorulanları görür:
/// dükkân (aranabilir), araç kimliği, parçalar; satır başına fiyat · marka · OEM kodu · not. Gönderilen cevap
/// listede kalır. Müşteri bilgisi yok (API). Uygulama kendi kökünde çalışır (/parca/): yollar GÖRELİ.
/// </summary>
public class PartsInboxScreenTests : TestContext
{
    readonly FakeApi api = new();
    readonly MemoryTokens tokens = new("t-p");
    const string Q1 = "66666666-6666-6666-6666-666666666661";
    const string L1 = "33333333-3333-3333-3333-333333333331";
    const string L2 = "33333333-3333-3333-3333-333333333332";
    const string A1 = "77777777-7777-7777-7777-777777777771";

    public PartsInboxScreenTests()
    {
        Services.AddSingleton<ITokenStore>(tokens);
        Services.AddSingleton(new TamironApi(new HttpClient(api) { BaseAddress = new Uri("http://api.test/") }, tokens));
        api.Routes["GET /api/parts/inbox"] = (HttpStatusCode.OK, Kutu(cevapli: false));
    }

    NavigationManager Nav => Services.GetRequiredService<NavigationManager>();

    static string Kutu(bool cevapli) =>
        "{\"items\":[{\"requestId\":\"" + Q1 + "\",\"shopName\":\"Usta Oto\",\"shopPhone\":\"0532 000 00 01\",\"plateNo\":\"34 YP 101\","
      + "\"brand\":\"Fiat\",\"model\":\"Egea\",\"year\":\"2021\",\"engine\":\"1.4 Fire\",\"vin\":\"ZFA35600006H12345\",\"sentAt\":\"2026-10-28T10:00:00Z\",\"lines\":["
      + "{\"lineId\":\"" + L1 + "\",\"name\":\"Balata\",\"answers\":["
      + (cevapli ? "{\"id\":\"" + A1 + "\",\"lineId\":\"" + L1 + "\",\"price\":1200,\"brand\":\"Bosch\",\"code\":\"0986494\",\"note\":null,\"answeredAt\":\"2026-10-28T10:05:00Z\"}" : "")
      + "]},{\"lineId\":\"" + L2 + "\",\"name\":\"Disk\",\"answers\":[]}]}]}";

    IRenderedComponent<Sorular> Ac()
    {
        var c = RenderComponent<Sorular>();
        c.WaitForElement("#psoListe");
        return c;
    }

    [Fact]
    public void Without_session_goes_to_its_own_login()
    {
        tokens.Token = null;
        RenderComponent<Sorular>();
        Assert.Equal("http://localhost/giris", Nav.Uri);   // göreli yol: /parca/ altında /parca/giris olur
    }

    [Fact]
    public void Shows_who_asks_for_which_car_and_what()
    {
        var c = Ac();
        var k = c.Find($"[data-soru='{Q1}']");
        Assert.Contains("Usta Oto", k.TextContent);
        Assert.Equal("tel:05320000001", k.QuerySelector("a[href^='tel:']")!.GetAttribute("href"));
        Assert.Contains("34 YP 101", k.TextContent);
        Assert.Contains("Fiat Egea 2021 · 1.4 Fire", k.TextContent);
        Assert.Contains("Şase: ZFA35600006H12345", k.TextContent);
        Assert.Equal(new[] { "Balata", "Disk" }, k.QuerySelectorAll("[data-parca]").Select(x => x.TextContent.Trim()));
    }

    [Fact]
    public void Sends_only_the_lines_written_and_clears_them()
    {
        api.Routes[$"POST /api/parts/requests/{Q1}/answers"] = (HttpStatusCode.OK, Kutu(cevapli: true));
        var c = Ac();
        c.Find($"[data-fiyat='{L1}']").Input("1.200");
        c.Find($"[data-marka='{L1}']").Input("Bosch");
        c.Find($"[data-kod='{L1}']").Input("0986494");
        c.Find($"[data-gonder='{Q1}']").Click();
        c.WaitForAssertion(() => Assert.Contains("Cevabın iletildi: Usta Oto.", c.Find("#psoSonuc").TextContent));
        using var b = JsonDocument.Parse(api.Calls.Single(x => x.Path == $"/api/parts/requests/{Q1}/answers").Body!);
        var satir = Assert.Single(b.RootElement.GetProperty("lines").EnumerateArray());   // Disk boş — gitmez
        Assert.Equal(L1, satir.GetProperty("lineId").GetString());
        Assert.Equal(1200m, satir.GetProperty("price").GetDecimal());
        Assert.Equal("Bosch", satir.GetProperty("brand").GetString());
        Assert.Equal("0986494", satir.GetProperty("code").GetString());
        Assert.Equal("", c.Find($"[data-fiyat='{L1}']").GetAttribute("value") ?? "");
        Assert.Contains("1.200 ₺ · Bosch · 0986494", c.Find($"[data-cevap='{A1}']").TextContent);
    }

    [Fact]
    public void Not_available_can_be_said_with_a_note()
    {
        api.Routes[$"POST /api/parts/requests/{Q1}/answers"] = (HttpStatusCode.OK, Kutu(cevapli: false));
        var c = Ac();
        c.Find($"[data-not='{L2}']").Input("Yok");
        c.Find($"[data-gonder='{Q1}']").Click();
        c.WaitForAssertion(() => Assert.Contains(api.Calls, x => x.Path.EndsWith("/answers")));
        Assert.Contains("\"note\":\"Yok\"", api.Calls.Single(x => x.Path.EndsWith("/answers")).Body);
    }

    [Fact]
    public void Nothing_written_says_so_and_sends_nothing()
    {
        var c = Ac();
        c.Find($"[data-gonder='{Q1}']").Click();
        Assert.Contains("En az bir parçaya", c.Find("#psoHata").TextContent);
        Assert.DoesNotContain(api.Calls, x => x.Path.EndsWith("/answers"));
    }

    [Fact]
    public void Unreadable_price_is_not_sent()
    {
        var c = Ac();
        c.Find($"[data-fiyat='{L1}']").Input("bin iki");
        c.Find($"[data-gonder='{Q1}']").Click();
        Assert.Contains("Tutarı rakamla yaz", c.Find("#psoHata").TextContent);
        Assert.DoesNotContain(api.Calls, x => x.Path.EndsWith("/answers"));
    }

    [Fact]
    public void Empty_inbox_says_how_questions_come()
    {
        api.Routes["GET /api/parts/inbox"] = (HttpStatusCode.OK, "{\"items\":[]}");
        var c = Ac();
        Assert.Contains("Sana sorulmuş parça yok", c.Find("#psoListe").TextContent);
    }

    [Fact]
    public void Workshop_session_is_told_to_use_its_own_app()
    {
        api.Routes["GET /api/parts/inbox"] = (HttpStatusCode.Forbidden, "{\"code\":\"app.wrong_kind\",\"title\":\"Bu işlem senin dükkânının uygulamasında yok.\"}");
        var c = RenderComponent<Sorular>();
        c.WaitForAssertion(() => Assert.Contains("Bu işlem senin dükkânının uygulamasında yok.", c.Find("#psoHata").TextContent));
    }

    [Fact]
    public void Its_login_says_whose_app_it_is_and_signs_up_a_parts_shop()
    {
        api.Routes["POST /api/auth/code"] = (HttpStatusCode.UnprocessableEntity, "{\"code\":\"auth.unknown_phone\",\"title\":\"Bu numarayla kayıtlı dükkân yok.\"}");
        api.Routes["POST /api/auth/signup/code"] = (HttpStatusCode.Accepted, "");
        api.Routes["POST /api/auth/signup"] = (HttpStatusCode.OK, "{\"token\":\"t-y\",\"shopName\":\"Öz Parça\",\"userName\":null,\"kind\":\"YedekParca\"}");
        tokens.Token = null;
        var c = RenderComponent<Tamiron.YedekParca.Ui.Pages.Giris>();
        Assert.Equal("Yedek parça girişi", c.Find(".eyebrow").TextContent.Trim());
        c.Find("#grTel").Input("0533 100 00 01");
        c.Find("#grKodIste").Click();
        c.WaitForElement("#grKaydol").Click();
        c.Find("#grDukkan").Input("Öz Parça");
        c.Find("#grKayitKodIste").Click();
        c.WaitForElement("#grKod").Input("123456");
        c.Find("#grGiris").Click();
        c.WaitForAssertion(() => Assert.Equal("t-y", tokens.Token));
        Assert.Contains("\"kind\":\"YedekParca\"", api.Calls.Single(x => x.Path == "/api/auth/signup").Body);
        Assert.Equal("http://localhost/", Nav.Uri);   // kendi kökü (/parca/)
    }

    [Fact]
    public void Logout_returns_to_its_own_login()
    {
        api.Routes["POST /api/auth/logout"] = (HttpStatusCode.NoContent, "");
        var c = Ac();
        c.Find("#psoCikis").Click();
        c.WaitForAssertion(() => Assert.Equal("http://localhost/giris", Nav.Uri));
        Assert.Null(tokens.Token);
    }
}
