using System.Net;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Tamirhane.Ui.Components;
using Tamiron.Ui.Core;

namespace Tamiron.Ui.Tests;

/// <summary>
/// G11b · Kabuk (prototip kabuk.html, K-009): üstte rozetli menü + logo; çekmecede BİLDİRİMLER = tarihi
/// gelmiş, gönderilmemiş bakım hatırlatmaları (yedinci iş) — uydurma bildirim yok, rozet boşken görünmez.
/// Kaynak tek: GET /api/reminders/due (B12). Bildirime dokunmak aracın mesajını "Bakım hatırlatması" seçili açar.
/// Çekmece: Servis kaydı ekle · Servis kayıtları · Parçacılar · Ayarlar (ana menüye satır eklenmedi — A-M8).
/// </summary>
public class ShellTests : TestContext
{
    readonly FakeApi api = new();
    readonly MemoryTokens tokens = new("t-1");
    const string Vid = "22222222-2222-2222-2222-222222222222";
    const string Rid = "11111111-1111-1111-1111-111111111111";

    public ShellTests()
    {
        Services.AddSingleton<ITokenStore>(tokens);
        Services.AddSingleton(new TamironApi(new HttpClient(api) { BaseAddress = new Uri("http://api.test/") }, tokens));
        api.Routes["GET /api/reminders/due"] = (HttpStatusCode.OK, Bir);
    }

    NavigationManager Nav => Services.GetRequiredService<NavigationManager>();

    const string Bir = $"[{{\"recordId\":\"{Rid}\",\"vehicleId\":\"{Vid}\",\"plateNo\":\"34 ABC 123\",\"ownerName\":\"Veli Kaya\",\"phone\":\"0532 111 22 33\",\"reminderDate\":\"2026-10-24\",\"daysOverdue\":3}}]";

    IRenderedComponent<Kabuk> Ac() => RenderComponent<Kabuk>(p => p.AddChildContent("<section id=\"ekran\">içerik</section>"));

    [Fact]
    public void Without_session_only_the_page_is_drawn()
    {
        tokens.Token = null;
        var c = Ac();
        Assert.NotNull(c.Find("#ekran"));
        Assert.Empty(c.FindAll("#menuBtn"));
        Assert.DoesNotContain(api.Calls, x => x.Path.StartsWith("/api/reminders"));
    }

    [Fact]
    public void Badge_counts_due_reminders_and_is_empty_without_any()
    {
        var c = Ac();
        c.WaitForAssertion(() => Assert.Equal("1", c.Find("#menuBadge").TextContent));
        Assert.NotNull(c.Find("#ekran"));

        api.Routes["GET /api/reminders/due"] = (HttpStatusCode.OK, "[]");
        var bos = Ac();
        bos.WaitForAssertion(() => Assert.Equal(2, api.Calls.Count(x => x.Path == "/api/reminders/due")));
        Assert.Equal("", bos.Find("#menuBadge").TextContent);   // .badge:empty ile gizlenir — sahte sayı yok
    }

    [Fact]
    public void Badge_refreshes_when_the_screen_changes()
    {
        var c = Ac();
        c.WaitForAssertion(() => Assert.Equal("1", c.Find("#menuBadge").TextContent));
        api.Routes["GET /api/reminders/due"] = (HttpStatusCode.OK, "[]");   // ör. hatırlatma mesajı gitti
        Nav.NavigateTo("/kayitlar");
        c.WaitForAssertion(() => Assert.Equal("", c.Find("#menuBadge").TextContent));
    }

    [Fact]
    public void Menu_opens_the_drawer_and_closes_it()
    {
        var c = Ac();
        Assert.DoesNotContain("open", c.Find("#drawer").ClassList);
        c.Find("#menuBtn").Click();
        Assert.Contains("open", c.Find("#drawer").ClassList);
        Assert.Equal("true", c.Find("#menuBtn").GetAttribute("aria-expanded"));
        c.Find("#drawerClose").Click();
        Assert.DoesNotContain("open", c.Find("#drawer").ClassList);
        c.Find("#menuBtn").Click();
        c.Find("#drawerOverlay").Click();
        Assert.DoesNotContain("open", c.Find("#drawer").ClassList);
    }

    [Fact]
    public void Notification_says_which_car_and_how_late_and_opens_its_reminder_message()
    {
        var c = Ac();
        c.Find("#menuBtn").Click();
        c.WaitForAssertion(() => Assert.Equal("1", c.Find("#notifCount").TextContent));
        var n = c.Find($"[data-hatirlatma='{Rid}']");
        Assert.Contains("Bakım zamanı geldi", n.TextContent);
        Assert.Contains("34 ABC 123 · Veli Kaya · 3 gün geçti", n.TextContent);
        n.Click();
        Assert.Equal($"http://localhost/mesaj?arac={Vid}&sablon=hatirlatma", Nav.Uri);
        Assert.DoesNotContain("open", c.Find("#drawer").ClassList);
    }

    [Fact]
    public void No_notifications_section_when_nothing_is_due()
    {
        api.Routes["GET /api/reminders/due"] = (HttpStatusCode.OK, "[]");
        var c = Ac();
        c.Find("#menuBtn").Click();
        Assert.Empty(c.FindAll("#notifSection"));
    }

    [Theory]
    [InlineData("#drawerEkle", "/kayit-ac")]
    [InlineData("#drawerKayitlar", "/kayitlar")]
    [InlineData("#drawerParcacilar", "/parcacilar")]
    [InlineData("#drawerAyarlar", "/ayarlar")]
    public void Drawer_items_go_and_close(string item, string path)
    {
        var c = Ac();
        c.Find("#menuBtn").Click();
        c.Find(item).Click();
        Assert.Equal("http://localhost" + path, Nav.Uri);
        Assert.DoesNotContain("open", c.Find("#drawer").ClassList);
    }

    [Fact]
    public void Logo_goes_home()
    {
        Nav.NavigateTo("/kayitlar");
        var c = Ac();
        c.Find("#brandHome").Click();
        Assert.Equal("http://localhost/", Nav.Uri);
    }

    [Fact]
    public void Reminder_failure_does_not_break_the_page()
    {
        api.Routes["GET /api/reminders/due"] = (HttpStatusCode.InternalServerError, "{\"title\":\"x\"}");
        var c = Ac();
        Assert.NotNull(c.Find("#ekran"));
        c.WaitForAssertion(() => Assert.Equal("", c.Find("#menuBadge").TextContent));
    }
}
