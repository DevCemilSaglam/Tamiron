using System.Net;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Tamirhane.Ui.Pages;
using Tamiron.Ui.Core;

namespace Tamiron.Ui.Tests;

/// <summary>
/// G9 · Araç (EKRANLAR §6, prototip 60-arac; sıra K-019.4: künye → eylemler → not → ruhsat → km →
/// bakım → geçmiş) ve Araçlar listesi (70-araclar). Geçmiş aracın kimliğiyle gelir (API), alacak
/// muhasebeyle AYNI kaynaktan (receivables?vehicleId — iki hesap birbirini yalanlamasın).
/// </summary>
public class VehicleScreenTests : TestContext
{
    readonly FakeApi api = new();
    readonly MemoryTokens tokens = new("t-1");
    const string Vid = "22222222-2222-2222-2222-222222222222";
    const string R1 = "11111111-1111-1111-1111-111111111111";
    const string R2 = "11111111-1111-1111-1111-111111111112";

    public VehicleScreenTests()
    {
        Services.AddSingleton<ITokenStore>(tokens);
        Services.AddSingleton(new TamironApi(new HttpClient(api) { BaseAddress = new Uri("http://api.test/") }, tokens));
        api.Routes[$"GET /api/vehicles/{Vid}"] = (HttpStatusCode.OK,
            $"{{\"id\":\"{Vid}\",\"plateNo\":\"06 TMR 07\",\"brand\":\"Fiat\",\"model\":\"Egea\",\"year\":\"2019\",\"engine\":\"1.3 Multijet\",\"fuelType\":\"Dizel\",\"color\":\"Beyaz\","
          + "\"vin\":\"ZFA35600006H12345\",\"ownerName\":\"Veli Kaya\",\"ownerPhone\":\"0532 111 22 33\",\"contactName\":\"Ali\",\"contactPhone\":\"0533 222 33 44\",\"note\":\"Sol ön lastik yamalı\","
          + "\"taxOrNationalId\":null,\"address\":null}");
        api.Routes["GET /api/records"] = (HttpStatusCode.OK, Gecmis(acik: false));
        api.Routes["GET /api/receivables"] = (HttpStatusCode.OK, "{\"total\":0,\"items\":[]}");
    }

    NavigationManager Nav => Services.GetRequiredService<NavigationManager>();

    static string Gecmis(bool acik) => "{\"inShopCount\":" + (acik ? 1 : 0) + ",\"count\":2,\"items\":["
        + (acik
            ? $"{{\"id\":\"{R2}\",\"vehicleId\":\"{Vid}\",\"plateNo\":\"06 TMR 07\",\"type\":\"Repair\",\"typeLabel\":\"Tamirat\",\"complaint\":\"Fren sesi\",\"openedOn\":\"2026-10-27\",\"deliveredAt\":null,\"total\":0,\"km\":\"61.000\",\"reminderDate\":null}},"
            : "")
        + $"{{\"id\":\"{R1}\",\"vehicleId\":\"{Vid}\",\"plateNo\":\"06 TMR 07\",\"type\":\"Maintenance\",\"typeLabel\":\"Bakım\",\"complaint\":\"Yağ bakımı\",\"openedOn\":\"2026-04-10\",\"deliveredAt\":\"2026-04-11\",\"total\":3450,\"km\":\"50.000\",\"reminderDate\":\"2026-10-11\"}}"
        + "]}";

    IRenderedComponent<Arac> Ac() => RenderComponent<Arac>(p => p.Add(x => x.Id, Guid.Parse(Vid)));

    [Fact]
    public void Without_session_goes_to_login()
    {
        tokens.Token = null;
        Ac();
        Assert.Equal("http://localhost/giris", Nav.Uri);
    }

    [Fact]
    public void Card_shows_vehicle_owner_contact_and_callable_phones()
    {
        var c = Ac();
        c.WaitForAssertion(() => Assert.Contains("Fiat Egea", c.Find("#arKunye").TextContent));
        Assert.Equal(["1.3 Multijet", "Dizel", "Beyaz"], c.FindAll("#arKunye .spec-val").Select(x => x.TextContent));
        Assert.Contains("Veli Kaya", c.Find("#arIletisim").TextContent);
        Assert.Contains("İletişim: Ali", c.Find("#arIletisim").TextContent);
        Assert.Equal(["tel:05321112233", "tel:05332223344"], c.FindAll("#arIletisim a").Select(a => a.GetAttribute("href")));
        Assert.Contains("/api/records?inShop=false&vehicleId=" + Vid, api.Calls.Select(x => x.Path));
    }

    [Fact]
    public void Not_in_shop_offers_a_new_record_for_this_plate()
    {
        var c = Ac();
        c.WaitForElement("#arYeniKayit").Click();
        Assert.Equal("http://localhost/kayit-ac?plaka=06%20TMR%2007", Nav.Uri);
    }

    [Fact]
    public void In_shop_goes_to_the_open_record_instead()
    {
        api.Routes["GET /api/records"] = (HttpStatusCode.OK, Gecmis(acik: true));
        var c = Ac();
        c.WaitForElement("#arAcikKayit").Click();
        Assert.Equal($"http://localhost/kayit/{R2}", Nav.Uri);
        Assert.Empty(c.FindAll("#arYeniKayit"));
    }

    [Fact]
    public void Receivable_comes_from_the_same_source_as_accounting()
    {
        api.Routes["GET /api/receivables"] = (HttpStatusCode.OK,
            $"{{\"total\":600,\"items\":[{{\"recordId\":\"{R1}\",\"vehicleId\":\"{Vid}\",\"plateNo\":\"06 TMR 07\",\"description\":\"Yağ bakımı\",\"deliveredAt\":\"2026-04-11\",\"total\":3450,\"paid\":2850,\"remaining\":600}}]}}");
        var c = Ac();
        c.WaitForAssertion(() => Assert.Contains("600 ₺", c.Find("#arOdeme .genel").TextContent));
        Assert.Contains($"/api/receivables?vehicleId={Vid}", api.Calls.Select(x => x.Path));
    }

    [Fact]
    public void Note_is_saved_to_the_vehicle()
    {
        api.Routes[$"PATCH /api/vehicles/{Vid}"] = (HttpStatusCode.OK, $"{{\"id\":\"{Vid}\",\"plateNo\":\"06 TMR 07\",\"note\":\"Yeni not\"}}");
        var c = Ac();
        c.WaitForAssertion(() => Assert.Equal("Sol ön lastik yamalı", c.Find("#arNot").GetAttribute("value")));
        c.Find("#arNot").Change("Yeni not");
        c.WaitForAssertion(() => Assert.Contains("\"note\":\"Yeni not\"", api.Calls.Last().Body));
    }

    [Fact]
    public void Registration_details_open_and_can_be_corrected_through_the_api()
    {
        api.Routes[$"PATCH /api/vehicles/{Vid}"] = (HttpStatusCode.OK,
            $"{{\"id\":\"{Vid}\",\"plateNo\":\"06 TMR 07\",\"brand\":\"Fiat\",\"model\":\"Egea Cross\",\"ownerName\":\"Veli Kaya\"}}");
        var c = Ac();
        c.WaitForElement("#arRuhsatBas").Click();
        Assert.Contains("ZFA35600006H12345", c.Find("#arRuhsatGovde").TextContent);
        c.Find("#arDuzenle").Click();
        c.Find("#adModel").Input("Egea Cross");
        c.Find("#adKaydet").Click();
        c.WaitForAssertion(() => Assert.Contains("Egea Cross", c.Find("#arKunye").TextContent));
        var body = api.Calls.Last(x => x.Path == $"/api/vehicles/{Vid}").Body!;
        Assert.Contains("\"model\":\"Egea Cross\"", body);
        Assert.Contains("\"brand\":\"Fiat\"", body);   // dokunulmayan alan olduğu gibi gider
    }

    [Fact]
    public void Edit_rejection_is_the_apis_sentence_and_the_form_stays()
    {
        api.Routes[$"PATCH /api/vehicles/{Vid}"] = (HttpStatusCode.UnprocessableEntity,
            "{\"code\":\"vehicle.year_invalid\",\"title\":\"Model yılı 1950 ile 2027 arasında olmalı.\"}");
        var c = Ac();
        c.WaitForElement("#arRuhsatBas").Click();
        c.Find("#arDuzenle").Click();
        c.Find("#adYil").Input("1890");
        c.Find("#adKaydet").Click();
        c.WaitForAssertion(() => Assert.Equal("Model yılı 1950 ile 2027 arasında olmalı.", c.Find("#arHata").TextContent.Trim()));
        Assert.Equal("1890", c.Find("#adYil").GetAttribute("value"));
    }

    [Fact]
    public void Km_and_maintenance_come_from_the_history()
    {
        var c = Ac();
        c.WaitForElement("#arKmBas").Click();
        Assert.Contains("11.04.2026", c.Find("#arKmGovde").TextContent);
        Assert.Contains("50.000 km", c.Find("#arKmGovde").TextContent);
        Assert.Contains("Son bakım", c.Find("#arBakim").TextContent);
        Assert.Contains("11.04.2026 · 50.000 km", c.Find("#arBakim").TextContent);
        Assert.Contains("11.10.2026", c.Find("#arBakim").TextContent);   // sıradaki = hatırlatma günü
    }

    [Fact]
    public void Last_maintenance_skips_repairs_and_the_open_job()
    {
        // Mutasyonla bulundu: daha yeni bir tamirat ya da içerideki bakım "son bakım" sayılmamalı —
        // sayılırsa sıradaki bakım günü kayar.
        api.Routes["GET /api/records"] = (HttpStatusCode.OK, "{\"inShopCount\":1,\"count\":3,\"items\":["
            + $"{{\"id\":\"{R2}\",\"vehicleId\":\"{Vid}\",\"plateNo\":\"06 TMR 07\",\"type\":\"Maintenance\",\"typeLabel\":\"Bakım\",\"complaint\":null,\"openedOn\":\"2026-10-27\",\"deliveredAt\":null,\"total\":0,\"km\":\"61.000\",\"reminderDate\":null}},"
            + $"{{\"id\":\"11111111-1111-1111-1111-111111111113\",\"vehicleId\":\"{Vid}\",\"plateNo\":\"06 TMR 07\",\"type\":\"Repair\",\"typeLabel\":\"Tamirat\",\"complaint\":\"Fren\",\"openedOn\":\"2026-09-01\",\"deliveredAt\":\"2026-09-02\",\"total\":900,\"km\":\"58.000\",\"reminderDate\":null}},"
            + $"{{\"id\":\"{R1}\",\"vehicleId\":\"{Vid}\",\"plateNo\":\"06 TMR 07\",\"type\":\"Maintenance\",\"typeLabel\":\"Bakım\",\"complaint\":\"Yağ bakımı\",\"openedOn\":\"2026-04-10\",\"deliveredAt\":\"2026-04-11\",\"total\":3450,\"km\":\"50.000\",\"reminderDate\":\"2026-10-11\"}}"
            + "]}");
        var c = Ac();
        c.WaitForAssertion(() => Assert.Contains("11.04.2026 · 50.000 km", c.Find("#arBakim").TextContent));
        Assert.Contains("11.10.2026", c.Find("#arBakim").TextContent);
    }

    [Fact]
    public void First_maintenance_still_in_shop_is_not_a_last_maintenance()
    {
        api.Routes["GET /api/records"] = (HttpStatusCode.OK, "{\"inShopCount\":1,\"count\":1,\"items\":["
            + $"{{\"id\":\"{R2}\",\"vehicleId\":\"{Vid}\",\"plateNo\":\"06 TMR 07\",\"type\":\"Maintenance\",\"typeLabel\":\"Bakım\",\"complaint\":null,\"openedOn\":\"2026-10-27\",\"deliveredAt\":null,\"total\":0,\"km\":\"61.000\",\"reminderDate\":null}}"
            + "]}");
        var c = Ac();
        c.WaitForAssertion(() => Assert.Contains("Bu araca bakım yapılmamış.", c.Find("#arBakim").TextContent));
    }

    [Fact]
    public void History_lists_jobs_and_opens_them()
    {
        api.Routes["GET /api/records"] = (HttpStatusCode.OK, Gecmis(acik: true));
        var c = Ac();
        c.WaitForAssertion(() => Assert.Equal(2, c.FindAll("#arGecmis [data-kayit]").Count));
        Assert.Contains("Serviste", c.FindAll("#arGecmis [data-kayit]")[0].TextContent);
        Assert.Contains("Teslim edildi", c.FindAll("#arGecmis [data-kayit]")[1].TextContent);
        c.FindAll("#arGecmis [data-kayit]")[1].Click();
        Assert.Equal($"http://localhost/kayit/{R1}", Nav.Uri);
    }
}

/// <summary>G9 · Araçlar listesi (prototip 70-araclar): serviste olanlar üstte (API'nin sırası), arama API'de.</summary>
public class VehicleListScreenTests : TestContext
{
    readonly FakeApi api = new();
    readonly MemoryTokens tokens = new("t-1");

    public VehicleListScreenTests()
    {
        Services.AddSingleton<ITokenStore>(tokens);
        Services.AddSingleton(new TamironApi(new HttpClient(api) { BaseAddress = new Uri("http://api.test/") }, tokens));
    }

    NavigationManager Nav => Services.GetRequiredService<NavigationManager>();

    const string Liste = "{\"count\":2,\"items\":["
        + "{\"id\":\"22222222-2222-2222-2222-222222222222\",\"plateNo\":\"06 TMR 07\",\"brand\":\"Fiat\",\"model\":\"Egea\",\"ownerName\":\"Veli Kaya\",\"contactName\":null,\"inShop\":true},"
        + "{\"id\":\"33333333-3333-3333-3333-333333333333\",\"plateNo\":\"34 VR 123\",\"brand\":\"Renault\",\"model\":\"Clio\",\"ownerName\":null,\"contactName\":null,\"inShop\":false}]}";

    [Fact]
    public void Lists_in_api_order_marks_in_shop_and_opens_the_vehicle()
    {
        api.Routes["GET /api/vehicles/list"] = (HttpStatusCode.OK, Liste);
        var c = RenderComponent<Araclar>();
        c.WaitForAssertion(() => Assert.Equal(2, c.FindAll("[data-arac]").Count));
        Assert.Equal("2", c.Find("#arcSay").TextContent);
        Assert.Contains("Serviste", c.FindAll("[data-arac]")[0].TextContent);
        Assert.DoesNotContain("Serviste", c.FindAll("[data-arac]")[1].TextContent);
        c.FindAll("[data-arac]")[1].Click();
        Assert.Equal("http://localhost/arac/33333333-3333-3333-3333-333333333333", Nav.Uri);
    }

    [Fact]
    public void Search_asks_the_api()
    {
        api.Routes["GET /api/vehicles/list"] = (HttpStatusCode.OK, Liste);
        var c = RenderComponent<Araclar>();
        c.WaitForElement("#arcQ").Input("veli");
        c.WaitForAssertion(() => Assert.Contains(api.Calls, x => x.Path == "/api/vehicles/list?q=veli"));
    }

    [Fact]
    public void Empty_shop_says_how_vehicles_come()
    {
        api.Routes["GET /api/vehicles/list"] = (HttpStatusCode.OK, "{\"count\":0,\"items\":[]}");
        var c = RenderComponent<Araclar>();
        c.WaitForAssertion(() => Assert.Contains("Henüz araç yok", c.Find("#arcListe").TextContent));
    }
}
