using System.Net;
using System.Text.Json;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Tamirhane.Ui.Pages;
using Tamiron.Ui.Core;

namespace Tamiron.Ui.Tests;

/// <summary>
/// G9c · Müşteriye mesaj (prototip 85-mesaj). İki adım: KİME (arama + liste + gönderilenler arşivi) →
/// YAZ (kime gidiyor + değiştir, son mesajlar kapalı, öneri çipleri, düzenlenebilir metin, üç kanal).
/// GÖNDERİM YOK: API ne gittiğini kaydeder, ekran ustanın kendi uygulamasını metin hazır açar (WHATSAPP.md
/// A yolu) ve "gönderildi" DEMEZ — "açılıyor" der. Kime/varsayılan/hangi iş API'den (message-setup).
/// </summary>
public class MessageScreenTests : TestContext
{
    readonly FakeApi api = new();
    readonly MemoryTokens tokens = new("t-1");
    const string Vid = "22222222-2222-2222-2222-222222222222";
    const string Rid = "11111111-1111-1111-1111-111111111111";

    public MessageScreenTests()
    {
        Services.AddSingleton<ITokenStore>(tokens);
        Services.AddSingleton(new TamironApi(new HttpClient(api) { BaseAddress = new Uri("http://api.test/") }, tokens));
        JSInterop.SetupVoid("open", _ => true).SetVoidResult();
        api.Routes["GET /api/vehicles/list"] = (HttpStatusCode.OK, "{\"count\":1,\"items\":["
            + $"{{\"id\":\"{Vid}\",\"plateNo\":\"06 TMR 07\",\"brand\":\"Fiat\",\"model\":\"Egea\",\"ownerName\":\"Veli Kaya\",\"contactName\":null,\"inShop\":true}}]}}");
        api.Routes["GET /api/messages"] = (HttpStatusCode.OK, "[" + Mesaj("Fren balatası değişti.") + "]");
        api.Routes[$"GET /api/vehicles/{Vid}"] = (HttpStatusCode.OK,
            $"{{\"id\":\"{Vid}\",\"plateNo\":\"06 TMR 07\",\"brand\":\"Fiat\",\"model\":\"Egea\",\"ownerName\":\"Veli Kaya\",\"ownerPhone\":\"0532 111 22 33\",\"contactName\":\"Ali\",\"contactPhone\":\"0533 222 33 44\"}}");
        api.Routes[$"GET /api/vehicles/{Vid}/message-setup"] = (HttpStatusCode.OK, Kurulum(aliTel: "0533 222 33 44"));
        api.Routes[$"GET /api/vehicles/{Vid}/message-suggestions"] = (HttpStatusCode.OK,
            "[{\"template\":\"hazir\",\"name\":\"Aracınız hazır\",\"text\":\"Ali Bey, aracınız hazır.\"},{\"template\":\"serbest\",\"name\":\"Serbest\",\"text\":\"\"}]");
        api.Routes["POST /api/messages"] = (HttpStatusCode.OK, Mesaj("x"));
    }

    NavigationManager Nav => Services.GetRequiredService<NavigationManager>();

    static string Kurulum(string? aliTel) => "{\"targets\":["
        + $"{{\"kind\":\"Contact\",\"name\":\"Ali\",\"phone\":{(aliTel is null ? "null" : $"\"{aliTel}\"")},\"isDefault\":{(aliTel is null ? "false" : "true")}}},"
        + $"{{\"kind\":\"Owner\",\"name\":\"Veli Kaya\",\"phone\":\"0532 111 22 33\",\"isDefault\":{(aliTel is null ? "true" : "false")}}}],"
        + $"\"currentRecordId\":\"{Rid}\"}}";

    static string Mesaj(string metin) => $"{{\"id\":\"99999999-9999-9999-9999-999999999999\",\"vehicleId\":\"{Vid}\",\"serviceRecordId\":null,"
        + $"\"sentAt\":\"2026-10-20T10:00:00\",\"recipientName\":\"Veli Kaya\",\"recipientPhone\":\"0532 111 22 33\",\"channel\":\"WhatsApp\",\"template\":\"serbest\",\"isBulk\":false,\"body\":\"{metin}\"}}";

    IRenderedComponent<Mesaj> Yaz()
    {
        Nav.NavigateTo($"/mesaj?arac={Vid}");
        var c = RenderComponent<Mesaj>();
        c.WaitForElement("#msjMetin");
        return c;
    }

    [Fact]
    public void Without_session_goes_to_login()
    {
        tokens.Token = null;
        RenderComponent<Mesaj>();
        Assert.Equal("http://localhost/giris", Nav.Uri);
    }

    [Fact]
    public void First_step_picks_whom_and_shows_the_archive()
    {
        var c = RenderComponent<Mesaj>();
        c.WaitForAssertion(() => Assert.Single(c.FindAll("#msjListe [data-mesaj-arac]")));
        Assert.Contains("Veli Kaya", c.Find("#msjListe").TextContent);
        Assert.Contains("Gönderilenler (1)", c.Find("#msjArsiv").TextContent);
        Assert.Contains("Fren balatası değişti.", c.Find("#msjArsiv").TextContent);
        c.Find("#msjQ").Input("veli");
        c.WaitForAssertion(() => Assert.Contains(api.Calls, x => x.Path == "/api/vehicles/list?q=veli"));
        c.Find("#msjListe [data-mesaj-arac]").Click();
        Assert.Equal($"http://localhost/mesaj?arac={Vid}", Nav.Uri);
    }

    [Fact]
    public void Writing_step_shows_the_default_target_and_offers_the_other()
    {
        var c = Yaz();
        Assert.Contains("Ali", c.Find("#msjKunye").TextContent);
        Assert.Contains("0533 222 33 44", c.Find("#msjKunye").TextContent);
        Assert.Equal("true", c.Find("[data-hedef='Contact']").GetAttribute("aria-pressed"));
        Assert.Equal("false", c.Find("[data-hedef='Owner']").GetAttribute("aria-pressed"));
        c.Find("#msjBaskasi").Click();
        Assert.Equal("http://localhost/mesaj", Nav.Uri);
    }

    [Fact]
    public void Suggestion_fills_the_text_which_stays_editable()
    {
        var c = Yaz();
        c.Find("[data-sablon='hazir']").Click();
        Assert.Equal("Ali Bey, aracınız hazır.", c.Find("#msjMetin").GetAttribute("value") ?? c.Find("#msjMetin").TextContent);
        c.Find("#msjMetin").Input("Ali Bey, aracınız 17:00'de hazır.");
        c.Find("#msjGonder").Click();
        c.WaitForAssertion(() => Assert.Contains(api.Calls, x => x.Path == "/api/messages" && x.Body is not null));
        using var b = JsonDocument.Parse(api.Calls.Single(x => x.Path == "/api/messages" && x.Body is not null).Body!);
        Assert.Equal("Ali Bey, aracınız 17:00'de hazır.", b.RootElement.GetProperty("body").GetString());   // gönderilen = ustanın son hâli
        Assert.Equal("hazir", b.RootElement.GetProperty("template").GetString());
        Assert.Equal("WhatsApp", b.RootElement.GetProperty("channel").GetString());
        Assert.Equal(Rid, b.RootElement.GetProperty("serviceRecordId").GetString());   // iş API'den
        Assert.Equal("Contact", b.RootElement.GetProperty("target").GetString());
    }

    [Fact]
    public void WhatsApp_opens_the_masters_app_and_never_says_sent()
    {
        var c = Yaz();
        c.Find("[data-sablon='hazir']").Click();
        c.Find("#msjGonder").Click();
        c.WaitForAssertion(() => Assert.Contains("WhatsApp açılıyor — Ali · 0533 222 33 44, mesaj hazır.", c.Find("#msjSonuc").TextContent));
        Assert.DoesNotContain("gönderildi", c.Markup, StringComparison.OrdinalIgnoreCase);
        var open = Assert.Single(JSInterop.Invocations, i => i.Identifier == "open");
        Assert.Equal(Format.WhatsAppLink("0533 222 33 44", "Ali Bey, aracınız hazır."), open.Arguments[0]);
    }

    [Fact]
    public void Sms_and_call_use_their_own_apps()
    {
        var c = Yaz();
        c.Find("[data-sablon='hazir']").Click();
        c.Find("#msjSms").Click();
        c.WaitForAssertion(() => Assert.Contains(JSInterop.Invocations, i => i.Identifier == "open" && (string)i.Arguments[0]! == Format.SmsLink("0533 222 33 44", "Ali Bey, aracınız hazır.")));
        c.Find("#msjAra").Click();
        c.WaitForAssertion(() => Assert.Contains(JSInterop.Invocations, i => i.Identifier == "open" && (string)i.Arguments[0]! == "tel:05332223344"));
        var kanallar = api.Calls.Where(x => x.Path == "/api/messages" && x.Body is not null)
            .Select(x => JsonDocument.Parse(x.Body!).RootElement.GetProperty("channel").GetString()).ToList();
        Assert.Equal(new[] { "Sms", "Call" }, kanallar);
    }

    [Fact]
    public void Changing_target_asks_suggestions_for_that_person()
    {
        var c = Yaz();
        c.Find("[data-hedef='Owner']").Click();
        c.WaitForAssertion(() => Assert.Contains(api.Calls, x => x.Path == $"/api/vehicles/{Vid}/message-suggestions?target=Owner"));
        Assert.Contains("Veli Kaya", c.Find("#msjKunye").TextContent);
        Assert.Equal("true", c.Find("[data-hedef='Owner']").GetAttribute("aria-pressed"));
    }

    [Fact]
    public void Template_from_the_link_is_already_chosen()
    {
        // G11b · Çekmecedeki "Bakım zamanı geldi" aracın mesajını hatırlatma seçili açar — usta yalnız gönderir.
        Nav.NavigateTo($"/mesaj?arac={Vid}&sablon=hazir");
        var c = RenderComponent<Mesaj>();
        c.WaitForAssertion(() => Assert.Equal("Ali Bey, aracınız hazır.", c.Find("#msjMetin").GetAttribute("value")));
        Assert.Equal("true", c.Find("[data-sablon='hazir']").GetAttribute("aria-pressed"));
    }

    [Fact]
    public void Template_not_suggested_for_this_job_is_not_forced()
    {
        Nav.NavigateTo($"/mesaj?arac={Vid}&sablon=hatirlatma");   // öneride yok (API kuralı) → seçilmez
        var c = RenderComponent<Mesaj>();
        c.WaitForElement("#msjMetin");
        Assert.Equal("", c.Find("#msjMetin").GetAttribute("value") ?? "");
        Assert.Empty(c.FindAll("[data-sablon][aria-pressed='true']"));
    }

    [Fact]
    public void Empty_text_cannot_be_sent_and_says_why()
    {
        var c = Yaz();
        Assert.True(c.Find("#msjGonder").HasAttribute("disabled"));
        Assert.Contains("Gidecek mesajı yaz.", c.Find("#msjUyari").TextContent);
    }

    [Fact]
    public void Target_without_phone_cannot_be_sent_and_says_why()
    {
        api.Routes[$"GET /api/vehicles/{Vid}/message-setup"] = (HttpStatusCode.OK,
            "{\"targets\":[{\"kind\":\"Owner\",\"name\":\"Veli Kaya\",\"phone\":null,\"isDefault\":true}],\"currentRecordId\":null}");
        var c = Yaz();
        c.Find("[data-sablon='hazir']").Click();
        Assert.True(c.Find("#msjGonder").HasAttribute("disabled"));
        Assert.True(c.Find("#msjAra").HasAttribute("disabled"));
        Assert.Contains("Telefon yok — mesaj gönderilemez.", c.Find("#msjUyari").TextContent);
        Assert.Empty(c.FindAll("[data-hedef]"));   // tek kişi: seçecek bir şey yok
    }

    [Fact]
    public void Past_messages_of_this_vehicle_are_listed_closed()
    {
        var c = Yaz();
        Assert.Contains(api.Calls, x => x.Path == $"/api/messages?vehicleId={Vid}");
        var g = c.Find("#msjGecmis details");
        Assert.False(g.HasAttribute("open"));
        Assert.Contains("Son mesajlar (1)", g.TextContent);
    }
}
