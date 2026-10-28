using System.Net;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Tamirhane.Ui.Pages;
using Tamiron.Ui.Core;

namespace Tamiron.Ui.Tests;

/// <summary>
/// H1 · İş başına (prototip 96-is-basina): teslim edilen her işin FARKI = alınan − parça alışı. Bilinmeyen alış
/// kâra girmez (K-057) ama sessizce kaybolmaz: "hesap dışı" yazılır ve nedeni söylenir. Rakamlar API'den.
/// </summary>
public class JobProfitScreenTests : TestContext
{
    readonly FakeApi api = new();
    readonly MemoryTokens tokens = new("t-1");
    const string R1 = "11111111-1111-1111-1111-111111111111";

    public JobProfitScreenTests()
    {
        Services.AddSingleton<ITokenStore>(tokens);
        Services.AddSingleton(new TamironApi(new HttpClient(api) { BaseAddress = new Uri("http://api.test/") }, tokens));
        api.Routes["GET /api/profit"] = (HttpStatusCode.OK, Liste(bilinmeyen: 1, alisSor: false));
    }

    NavigationManager Nav => Services.GetRequiredService<NavigationManager>();

    static string Liste(int bilinmeyen, bool alisSor, string period = "Month")
    {
        var disarida = bilinmeyen > 0 ? 800 : 0;
        var fark = bilinmeyen > 0 ? 3000 : 3800;
        var sor = alisSor ? "true" : "false";
        return $"{{\"period\":\"{period}\",\"start\":\"2026-10-01\",\"end\":\"2026-10-31\",\"received\":5000,\"partsCost\":1200,\"outside\":{disarida},"
             + $"\"unknownCount\":{bilinmeyen},\"difference\":{fark},\"askPartCost\":{sor},\"items\":["
             + $"{{\"recordId\":\"{R1}\",\"plateNo\":\"34 ABC 123\",\"type\":\"Repair\",\"typeLabel\":\"Tamirat\",\"brand\":\"Renault\",\"deliveredAt\":\"2026-10-26\","
             + $"\"received\":5000,\"partsCost\":1200,\"outside\":{disarida},\"unknownCount\":{bilinmeyen},\"difference\":{fark}}}]}}";
    }

    IRenderedComponent<IsBasina> Ac(string donem = "Month")
    {
        Nav.NavigateTo("/is-basina?donem=" + donem);
        var c = RenderComponent<IsBasina>();
        c.WaitForElement("#ibTablo");
        return c;
    }

    [Fact]
    public void Without_session_goes_to_login()
    {
        tokens.Token = null;
        RenderComponent<IsBasina>();
        Assert.Equal("http://localhost/giris", Nav.Uri);
    }

    [Fact]
    public void Asks_the_period_it_came_with_and_shows_each_job_and_the_total()
    {
        var c = Ac();
        Assert.Contains(api.Calls, x => x.Path == "/api/profit?period=Month");
        Assert.Equal("true", c.Find("[data-donem='Month']").GetAttribute("aria-pressed"));
        var satir = c.Find($"[data-kayit='{R1}']");
        Assert.Contains("34 ABC 123", satir.TextContent);
        Assert.Contains("Tamirat · Renault · 26.10.2026", satir.TextContent);
        Assert.Contains("alınan 5.000 · parça 1.200 · 800 hesap dışı", satir.TextContent);
        Assert.Contains("3.000 ₺", satir.QuerySelector(".sayi")!.TextContent);
        Assert.Contains("3.000 ₺", c.Find("#ibTablo tr.genel").TextContent);
        // Prototip: "1 teslim · ekim" — dönemin ADI, "bu ay" değil (rakam kendi tarihini taşır).
        Assert.Equal("1 teslim · " + Format.MonthName(DateTime.Today).ToLower(new System.Globalization.CultureInfo("tr-TR")), c.Find("#ibSay").TextContent);
    }

    [Fact]
    public void Unknown_cost_is_told_and_leads_to_settings_when_not_asked()
    {
        var c = Ac();
        Assert.Contains("1 parçanın alışı yazılmamış.", c.Find("#ibDisarida").TextContent);
        Assert.Contains("Satışı (800 ₺) farka katılmadı", c.Find("#ibDisarida").TextContent);
        c.Find("#ibAyarlar").Click();
        Assert.Equal("http://localhost/ayarlar", Nav.Uri);
    }

    [Fact]
    public void Unknown_cost_when_asked_says_where_to_write_it()
    {
        api.Routes["GET /api/profit"] = (HttpStatusCode.OK, Liste(bilinmeyen: 1, alisSor: true));
        var c = Ac();
        Assert.Contains("Alışı servis kaydında parçanın altına yazabilirsin.", c.Find("#ibDisarida").TextContent);
        Assert.Empty(c.FindAll("#ibAyarlar"));
    }

    [Fact]
    public void No_note_when_every_cost_is_known()
    {
        api.Routes["GET /api/profit"] = (HttpStatusCode.OK, Liste(bilinmeyen: 0, alisSor: false));
        var c = Ac();
        Assert.Empty(c.FindAll("#ibDisarida .notice"));
        Assert.DoesNotContain("hesap dışı", c.Find($"[data-kayit='{R1}']").TextContent);
    }

    [Fact]
    public void Period_change_asks_again_and_row_opens_the_record()
    {
        var c = Ac();
        api.Routes["GET /api/profit"] = (HttpStatusCode.OK, Liste(0, false, "Year"));
        c.Find("[data-donem='Year']").Click();
        c.WaitForAssertion(() => Assert.Contains(api.Calls, x => x.Path == "/api/profit?period=Year"));
        c.Find($"[data-kayit='{R1}']").Click();
        Assert.Equal($"http://localhost/kayit/{R1}", Nav.Uri);
    }

    [Fact]
    public void Empty_period_says_so()
    {
        api.Routes["GET /api/profit"] = (HttpStatusCode.OK,
            "{\"period\":\"Today\",\"start\":\"2026-10-28\",\"end\":\"2026-10-28\",\"received\":0,\"partsCost\":0,\"outside\":0,\"unknownCount\":0,\"difference\":0,\"askPartCost\":false,\"items\":[]}");
        Nav.NavigateTo("/is-basina?donem=Today");
        var c = RenderComponent<IsBasina>();
        c.WaitForAssertion(() => Assert.Contains("Bu dönemde teslim edilmiş iş yok", c.Markup));
        Assert.Empty(c.FindAll("#ibTablo"));
    }

    [Fact]
    public void Unknown_period_in_the_link_falls_back_to_this_month()
    {
        Nav.NavigateTo("/is-basina?donem=Hafta");
        RenderComponent<IsBasina>();
        Assert.Contains(api.Calls, x => x.Path == "/api/profit?period=Month");
    }
}
