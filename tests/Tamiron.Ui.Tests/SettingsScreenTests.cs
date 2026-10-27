using System.Net;
using System.Text.Json;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Tamirhane.Ui.Pages;
using Tamiron.Ui.Core;

namespace Tamiron.Ui.Tests;

/// <summary>
/// G11a · Ayarlar (prototip 99-ayarlar, K-055.2): dört sakin — bakım periyodu (3 · 6 · 12 ay), parça alış
/// fiyatı (Sorulsun / Sorulmasın — anahtar değil, iki düğme: T-18), tema (cihazda), gizlilik. Dükkânın adı,
/// adresi, telefonu OKUNUR (düzenlemek prototipte yok — C5). Sınırlar API'de.
/// </summary>
public class SettingsScreenTests : TestContext
{
    readonly FakeApi api = new();
    readonly MemoryTokens tokens = new("t-1");

    public SettingsScreenTests()
    {
        Services.AddSingleton<ITokenStore>(tokens);
        Services.AddSingleton(new TamironApi(new HttpClient(api) { BaseAddress = new Uri("http://api.test/") }, tokens));
        api.Routes["GET /api/shop"] = (HttpStatusCode.OK, Dukkan(6, false));
        JSInterop.Setup<string>("tamironTema.al").SetResult("");
        JSInterop.SetupVoid("tamironTema.yaz", _ => true).SetVoidResult();
    }

    NavigationManager Nav => Services.GetRequiredService<NavigationManager>();

    static string Dukkan(int ay, bool alis) =>
        $"{{\"name\":\"Deneme Oto\",\"address\":\"Sanayi Sitesi 4. Blok\",\"phone\":\"0532 555 00 01\",\"maintenanceMonths\":{ay},\"maintenanceKm\":10000,\"askPartCost\":{(alis ? "true" : "false")},\"privacyNoticeShownAt\":null}}";

    IRenderedComponent<Ayarlar> Ac()
    {
        var c = RenderComponent<Ayarlar>();
        c.WaitForElement("#periyotGrup [data-periyot]");
        return c;
    }

    [Fact]
    public void Without_session_goes_to_login()
    {
        tokens.Token = null;
        RenderComponent<Ayarlar>();
        Assert.Equal("http://localhost/giris", Nav.Uri);
    }

    [Fact]
    public void Shop_details_are_read_not_edited()
    {
        var c = Ac();
        Assert.Contains("Deneme Oto", c.Find("#ayarDukkan").TextContent);
        Assert.Contains("Sanayi Sitesi 4. Blok", c.Find("#ayarDukkan").TextContent);
        Assert.Contains("0532 555 00 01", c.Find("#ayarDukkan").TextContent);
        Assert.Empty(c.FindAll("#ayarDukkan input"));
    }

    [Fact]
    public void Period_shows_the_current_and_changing_it_patches_only_that()
    {
        var c = Ac();
        Assert.Equal(new[] { "3 ay", "6 ay", "12 ay" }, c.FindAll("#periyotGrup [data-periyot]").Select(x => x.TextContent.Trim()));
        Assert.Equal("true", c.Find("[data-periyot='6']").GetAttribute("aria-pressed"));
        Assert.Contains("10.000 km", c.Find("#ayarPeriyot").TextContent);   // km okunur
        api.Routes["PATCH /api/shop"] = (HttpStatusCode.OK, Dukkan(12, false));
        c.Find("[data-periyot='12']").Click();
        c.WaitForAssertion(() => Assert.Equal("true", c.Find("[data-periyot='12']").GetAttribute("aria-pressed")));
        using var b = JsonDocument.Parse(api.Calls.Single(x => x.Path == "/api/shop" && x.Body is not null).Body!);
        Assert.Equal(12, b.RootElement.GetProperty("maintenanceMonths").GetInt32());
        Assert.False(b.RootElement.TryGetProperty("askPartCost", out var a) && a.ValueKind != JsonValueKind.Null);
        Assert.Contains("Bakım periyodu 12 ay oldu.", c.Find("#ayarSonuc").TextContent);
    }

    [Fact]
    public void Part_cost_question_is_two_plain_buttons()
    {
        var c = Ac();
        Assert.Equal("true", c.Find("[data-ayar-alis='0']").GetAttribute("aria-pressed"));
        Assert.Equal("Sorulmasın", c.Find("[data-ayar-alis='0']").TextContent.Trim());
        api.Routes["PATCH /api/shop"] = (HttpStatusCode.OK, Dukkan(6, true));
        c.Find("[data-ayar-alis='1']").Click();
        c.WaitForAssertion(() => Assert.Equal("true", c.Find("[data-ayar-alis='1']").GetAttribute("aria-pressed")));
        using var b = JsonDocument.Parse(api.Calls.Single(x => x.Path == "/api/shop" && x.Body is not null).Body!);
        Assert.True(b.RootElement.GetProperty("askPartCost").GetBoolean());
        Assert.Contains("alış", c.Find("#ayarSonuc").TextContent);
    }

    [Fact]
    public void Theme_is_kept_on_the_device()
    {
        var c = Ac();
        Assert.Equal("true", c.Find("[data-tema='']").GetAttribute("aria-pressed"));   // seçilmemiş = telefona göre
        c.Find("[data-tema='dark']").Click();
        var yaz = JSInterop.Invocations.Single(i => i.Identifier == "tamironTema.yaz");
        Assert.Equal("dark", yaz.Arguments[0]);
        Assert.Equal("true", c.Find("[data-tema='dark']").GetAttribute("aria-pressed"));
        Assert.DoesNotContain(api.Calls, x => x.Path == "/api/shop" && x.Body is not null);   // sunucuya gitmez
    }

    [Fact]
    public void Privacy_says_only_what_is_true_today()
    {
        var c = Ac();
        Assert.Contains("ruhsat fotoğrafı", c.Find("#ayarGizlilik").TextContent);
        Assert.DoesNotContain("90 gün", c.Find("#ayarGizlilik").TextContent);   // silme betiği zamanlanmadı — söz verilmez
    }
}
