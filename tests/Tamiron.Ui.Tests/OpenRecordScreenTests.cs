using System.Net;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Tamirhane.Ui.Pages;
using Tamiron.Ui.Core;

namespace Tamiron.Ui.Tests;

/// <summary>
/// G2 · Servis kaydı ekle (EKRANLAR §2, prototip 20-ruhsat). Plaka kapının kendisi: her yazışta API'ye
/// sorulur (geçerli mi, kayıtlı mı, açık kaydı var mı) — ekran yalnız cevabı gösterir. Düğme plaka
/// geçerli olmadan YOK (sönük değil, yok — prototip rEylemCiz); varsa her zaman basılır ve eksiği söyler.
/// </summary>
public class OpenRecordScreenTests : TestContext
{
    readonly FakeApi api = new();
    readonly MemoryTokens tokens = new("t-1");

    public OpenRecordScreenTests()
    {
        Services.AddSingleton<ITokenStore>(tokens);
        Services.AddSingleton(new TamironApi(new HttpClient(api) { BaseAddress = new Uri("http://api.test/") }, tokens));
    }

    NavigationManager Nav => Services.GetRequiredService<NavigationManager>();

    const string Gecersiz = "{\"plate\":\"34 AB\",\"valid\":false,\"hint\":\"Plaka eksik — ör. 34 ABC 123\",\"vehicle\":null,\"openRecordId\":null}";
    const string Kayitsiz = "{\"plate\":\"06 XYZ 77\",\"valid\":true,\"hint\":null,\"vehicle\":null,\"openRecordId\":null}";
    const string KayitliAcik = "{\"plate\":\"06 TMR 07\",\"valid\":true,\"hint\":null,\"openRecordId\":\"11111111-1111-1111-1111-111111111111\","
                             + "\"vehicle\":{\"id\":\"22222222-2222-2222-2222-222222222222\",\"plateNo\":\"06 TMR 07\",\"brand\":\"Fiat\",\"model\":\"Egea\",\"ownerName\":\"Veli Kaya\",\"hasPartIdentity\":true}}";
    const string KayitliSasesiz = "{\"plate\":\"34 VR 123\",\"valid\":true,\"hint\":null,\"openRecordId\":null,"
                                + "\"vehicle\":{\"id\":\"33333333-3333-3333-3333-333333333333\",\"plateNo\":\"34 VR 123\",\"brand\":\"Renault\",\"model\":\"Clio\",\"ownerName\":null,\"hasPartIdentity\":false}}";
    const string Acildi = "{\"id\":\"44444444-4444-4444-4444-444444444444\",\"vehicleId\":\"55555555-5555-5555-5555-555555555555\",\"plateNo\":\"06 XYZ 77\"}";

    IRenderedComponent<KayitAc> Yaz(string lookup, string plaka = "06xyz77")
    {
        api.Routes["GET /api/vehicles/lookup"] = (HttpStatusCode.OK, lookup);
        var c = RenderComponent<KayitAc>();
        c.Find("#rPlaka").Input(plaka);
        return c;
    }

    [Fact]
    public void Without_session_goes_to_login()
    {
        tokens.Token = null;
        RenderComponent<KayitAc>();
        Assert.Equal("http://localhost/giris", Nav.Uri);
    }

    [Fact]
    public void Nothing_to_press_before_a_plate()
    {
        var c = RenderComponent<KayitAc>();
        Assert.Empty(c.FindAll("#kaydiAc"));
        Assert.Empty(api.Calls);
    }

    [Fact]
    public void Each_keystroke_asks_the_api_and_shows_its_hint()
    {
        var c = Yaz(Gecersiz, "34ab");
        c.WaitForAssertion(() => Assert.Equal("Plaka eksik — ör. 34 ABC 123", c.Find("#rPlakaIpucu").TextContent));
        Assert.Contains(api.Calls, x => x.Path == "/api/vehicles/lookup?plate=34ab");
        Assert.Empty(c.FindAll("#kaydiAc"));
    }

    [Fact]
    public void Unregistered_vehicle_asks_for_its_details_and_offers_to_skip_the_registration()
    {
        var c = Yaz(Kayitsiz);
        c.WaitForAssertion(() => Assert.Contains("Bu araç kayıtlı değil.", c.Find("#rTanima").TextContent));
        Assert.Contains("Ruhsatı çekebilirsin ya da bu aşamayı atlayabilirsin.", c.Find("#rTanima").TextContent);
        Assert.NotNull(c.Find("#rAracKart"));
        Assert.Equal("Ruhsat adımını geç ve kaydı aç", c.Find("#kaydiAc").TextContent.Trim());
    }

    [Fact]
    public void Registered_vehicle_in_shop_leads_to_its_open_record()
    {
        var c = Yaz(KayitliAcik, "06tmr07");
        c.WaitForAssertion(() => Assert.Contains("Bu araç kayıtlı", c.Find("#rTanima").TextContent));
        Assert.Contains("Fiat Egea · Veli Kaya", c.Find("#rTanima").TextContent);
        Assert.Contains("Zaten serviste ve açık bir kaydı var.", c.Find("#rTanima").TextContent);
        Assert.Empty(c.FindAll("#rAracKart"));   // kayıtlı araç ezilmez
        Assert.Equal("Kaydı aç", c.Find("#kaydiAc").TextContent.Trim());
        c.Find("#acikKayit").Click();
        Assert.Equal("http://localhost/kayit/11111111-1111-1111-1111-111111111111", Nav.Uri);
    }

    [Fact]
    public void Registered_vehicle_without_vin_is_told_not_blocked()
    {
        var c = Yaz(KayitliSasesiz, "34vr123");
        c.WaitForAssertion(() => Assert.Contains("Şase numarası yok — parça sorarken ruhsat ya da araç seçimi gerekir.", c.Find("#rTanima").TextContent));
        Assert.Empty(c.FindAll("#acikKayit"));
    }

    [Fact]
    public void Opening_unregistered_sends_the_typed_details_in_the_same_request_and_goes_to_the_record()
    {
        api.Routes["POST /api/records"] = (HttpStatusCode.Created, Acildi);
        var c = Yaz(Kayitsiz);
        c.WaitForElement("#rMarka").Input("Fiat");
        c.Find("#rTel").Input("0532 111 22 33");
        c.Find("#kaydiAc").Click();
        c.WaitForAssertion(() => Assert.Equal("http://localhost/kayit/44444444-4444-4444-4444-444444444444", Nav.Uri));
        var body = api.Calls.Single(x => x.Path == "/api/records").Body!;
        Assert.Contains("\"plate\":\"06xyz77\"", body);
        Assert.Contains("\"brand\":\"Fiat\"", body);
        Assert.Contains("\"ownerPhone\":\"0532 111 22 33\"", body);
    }

    [Fact]
    public void Opening_registered_sends_no_vehicle_details()
    {
        api.Routes["POST /api/records"] = (HttpStatusCode.Created, Acildi);
        var c = Yaz(KayitliSasesiz, "34vr123");
        c.WaitForElement("#kaydiAc").Click();
        c.WaitForAssertion(() => Assert.StartsWith("http://localhost/kayit/", Nav.Uri));
        Assert.Contains("\"vehicle\":null", api.Calls.Single(x => x.Path == "/api/records").Body!);
    }

    [Fact]
    public void Details_typed_for_another_plate_do_not_ride_on_a_registered_vehicle()
    {
        // Mutasyonla bulundu: kartta yazılanlar plaka kayıtlı araca dönünce de gidiyordu (ezilmezdi ama
        // sunucuya güvenmek yerine ekran da göndermemeli).
        api.Routes["POST /api/records"] = (HttpStatusCode.Created, Acildi);
        var c = Yaz(Kayitsiz);
        c.WaitForElement("#rMarka").Input("Opel");
        api.Routes["GET /api/vehicles/lookup"] = (HttpStatusCode.OK, KayitliSasesiz);
        c.Find("#rPlaka").Input("34vr123");
        c.WaitForAssertion(() => Assert.Empty(c.FindAll("#rAracKart")));
        c.Find("#kaydiAc").Click();
        c.WaitForAssertion(() => Assert.StartsWith("http://localhost/kayit/", Nav.Uri));
        Assert.Contains("\"vehicle\":null", api.Calls.Single(x => x.Path == "/api/records").Body!);
    }

    [Fact]
    public void Api_rejection_is_shown_and_stays()
    {
        api.Routes["POST /api/records"] = (HttpStatusCode.UnprocessableEntity, "{\"code\":\"vehicle.year_invalid\",\"title\":\"Model yılı 1950 ile 2027 arasında olmalı.\"}");
        var c = Yaz(Kayitsiz);
        c.WaitForElement("#rYil").Input("1890");
        c.Find("#kaydiAc").Click();
        c.WaitForAssertion(() => Assert.Equal("Model yılı 1950 ile 2027 arasında olmalı.", c.Find("#rHata").TextContent.Trim()));
        Assert.Equal("http://localhost/", Nav.Uri);
        Assert.Equal("1890", c.Find("#rYil").GetAttribute("value"));   // yazılan silinmez
    }
}
