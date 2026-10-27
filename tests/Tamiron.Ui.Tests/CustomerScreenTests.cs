using System.Net;
using System.Text.Json;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Tamirhane.Ui.Pages;
using Tamiron.Ui.Core;

namespace Tamiron.Ui.Tests;

/// <summary>
/// G9b · Müşteriler (prototip 90-musteriler): müşteri tablosu YOK, liste araçlardan türetilir (API).
/// Satır "hangi kişi?" sorusunu cevaplar — AD büyük, plaka ikinci satırda; serviste olanlar üstte
/// (API'nin sırası). Satıra dokunmak müşteri detayını açar.
/// </summary>
public class CustomerListScreenTests : TestContext
{
    readonly FakeApi api = new();
    readonly MemoryTokens tokens = new("t-1");

    public CustomerListScreenTests()
    {
        Services.AddSingleton<ITokenStore>(tokens);
        Services.AddSingleton(new TamironApi(new HttpClient(api) { BaseAddress = new Uri("http://api.test/") }, tokens));
    }

    NavigationManager Nav => Services.GetRequiredService<NavigationManager>();

    const string Liste = "{\"count\":2,\"items\":["
        + "{\"key\":\"veli kaya|05321112233\",\"name\":\"Veli Kaya\",\"phone\":\"0532 111 22 33\",\"displayPlate\":\"06 TMR 07\",\"vehicleCount\":2,\"inShop\":true,\"consentAt\":\"2026-10-01\"},"
        + "{\"key\":\"ayse demir|\",\"name\":\"Ayşe Demir\",\"phone\":null,\"displayPlate\":\"34 VR 123\",\"vehicleCount\":1,\"inShop\":false,\"consentAt\":null}]}";

    [Fact]
    public void Without_session_goes_to_login()
    {
        tokens.Token = null;
        RenderComponent<Musteriler>();
        Assert.Equal("http://localhost/giris", Nav.Uri);
    }

    [Fact]
    public void Lists_name_first_plate_second_in_api_order()
    {
        api.Routes["GET /api/customers"] = (HttpStatusCode.OK, Liste);
        var c = RenderComponent<Musteriler>();
        c.WaitForAssertion(() => Assert.Equal(2, c.FindAll("[data-musteri]").Count));
        Assert.Equal("2", c.Find("#msSay").TextContent);
        var ilk = c.FindAll("[data-musteri]")[0];
        Assert.Equal("Veli Kaya", ilk.QuerySelector(".mus-ad")!.TextContent.Trim());
        Assert.Contains("06 TMR 07", ilk.QuerySelector(".mus-arac")!.TextContent);
        Assert.Contains("2 araç", ilk.TextContent);
        Assert.Contains("serviste", ilk.ClassList);
        Assert.Contains("telefon yok", c.FindAll("[data-musteri]")[1].TextContent);
    }

    [Fact]
    public void Row_opens_the_customer_by_key()
    {
        api.Routes["GET /api/customers"] = (HttpStatusCode.OK, Liste);
        var c = RenderComponent<Musteriler>();
        c.WaitForAssertion(() => c.FindAll("[data-musteri]")[0].Click());
        Assert.Equal("http://localhost/musteri?key=veli%20kaya%7C05321112233", Nav.Uri);
    }

    [Fact]
    public void Search_asks_the_api()
    {
        api.Routes["GET /api/customers"] = (HttpStatusCode.OK, Liste);
        var c = RenderComponent<Musteriler>();
        c.WaitForElement("#msQ").Input("veli");
        c.WaitForAssertion(() => Assert.Contains(api.Calls, x => x.Path == "/api/customers?q=veli"));
    }

    [Fact]
    public void Empty_shop_says_where_customers_come_from()
    {
        api.Routes["GET /api/customers"] = (HttpStatusCode.OK, "{\"count\":0,\"items\":[]}");
        var c = RenderComponent<Musteriler>();
        c.WaitForAssertion(() => Assert.Contains("Henüz müşteri yok", c.Find("#msListe").TextContent));
    }
}

/// <summary>
/// G9b · Müşteri detayı (prototip 91-musteri): künye (ad, araç · kayıt sayısı, aranabilir telefon,
/// ileti izni çipi), araçları (araç ekranına gider), servis geçmişi (her aracın kendi kimliğiyle —
/// çok araçta plaka başlığıyla). İzin günü SUNUCUDAN; "Geri al" eski tarihi izden getirir (D5).
/// </summary>
public class CustomerScreenTests : TestContext
{
    readonly FakeApi api = new();
    readonly MemoryTokens tokens = new("t-1");
    const string Key = "veli kaya|05321112233";
    const string V1 = "22222222-2222-2222-2222-222222222222";
    const string V2 = "33333333-3333-3333-3333-333333333333";

    public CustomerScreenTests()
    {
        Services.AddSingleton<ITokenStore>(tokens);
        Services.AddSingleton(new TamironApi(new HttpClient(api) { BaseAddress = new Uri("http://api.test/") }, tokens));
        api.Routes["GET /api/customers/detail"] = (HttpStatusCode.OK, Detay(null, iki: false));
        api.Routes["GET /api/records"] = (HttpStatusCode.OK, Kayitlar);
    }

    NavigationManager Nav => Services.GetRequiredService<NavigationManager>();

    static string Detay(string? izin, bool iki, string? tel = "0532 111 22 33") =>
        $"{{\"key\":\"{Key}\",\"name\":\"Veli Kaya\",\"phone\":{(tel is null ? "null" : $"\"{tel}\"")},\"consentAt\":{(izin is null ? "null" : $"\"{izin}\"")},\"vehicles\":["
        + $"{{\"id\":\"{V1}\",\"plateNo\":\"06 TMR 07\",\"brand\":\"Fiat\",\"model\":\"Egea\",\"inShop\":true}}"
        + (iki ? $",{{\"id\":\"{V2}\",\"plateNo\":\"34 VR 123\",\"brand\":\"Renault\",\"model\":\"Clio\",\"inShop\":false}}" : "")
        + "]}";

    const string Kayitlar = "{\"inShopCount\":0,\"count\":1,\"items\":["
        + "{\"id\":\"11111111-1111-1111-1111-111111111111\",\"vehicleId\":\"22222222-2222-2222-2222-222222222222\",\"plateNo\":\"06 TMR 07\",\"type\":\"Maintenance\",\"typeLabel\":\"Bakım\",\"complaint\":\"Yağ bakımı\",\"openedOn\":\"2026-04-10\",\"deliveredAt\":\"2026-04-11\",\"total\":3450,\"km\":\"50.000\",\"reminderDate\":\"2026-10-11\"}]}";

    IRenderedComponent<Musteri> Ac()
    {
        Nav.NavigateTo("/musteri?key=" + Uri.EscapeDataString(Key));
        return RenderComponent<Musteri>();
    }

    [Fact]
    public void Asks_the_api_by_key_and_shows_the_card()
    {
        var c = Ac();
        c.WaitForAssertion(() => Assert.Contains("Veli Kaya", c.Find("#mstKunye").TextContent));
        Assert.Contains(api.Calls, x => x.Path == "/api/customers/detail?key=veli%20kaya%7C05321112233");
        Assert.Contains("1 araç · 1 kayıt", c.Find("#mstKunye").TextContent);
        Assert.Equal("tel:05321112233", c.Find("#mstKunye a[href^='tel:']").GetAttribute("href"));
    }

    [Fact]
    public void Key_with_an_ampersand_survives_the_query()
    {
        // Mutasyonla bulundu: boşluk ve "|" HttpClient'ta kendiliğinden kaçışlanıyor, "&" kaçışlanmaz —
        // "Kaya & Oğulları" adlı müşterinin anahtarı sorguyu ikiye bölerdi.
        Nav.NavigateTo("/musteri?key=" + Uri.EscapeDataString("kaya & ogullari|05321112233"));
        RenderComponent<Musteri>();
        Assert.Contains(api.Calls, x => x.Path == "/api/customers/detail?key=kaya%20%26%20ogullari%7C05321112233");
    }

    [Fact]
    public void Vehicles_open_the_vehicle_screen()
    {
        api.Routes["GET /api/customers/detail"] = (HttpStatusCode.OK, Detay(null, iki: true));
        var c = Ac();
        c.WaitForAssertion(() => Assert.Equal(2, c.FindAll("#mstAraclar [data-arac]").Count));
        c.FindAll("#mstAraclar [data-arac]")[1].Click();
        Assert.Equal($"http://localhost/arac/{V2}", Nav.Uri);
    }

    [Fact]
    public void History_is_asked_per_vehicle_and_grouped_only_when_many()
    {
        var tek = Ac();
        tek.WaitForAssertion(() => Assert.Single(tek.FindAll("#mstGecmis [data-kayit]")));
        Assert.Empty(tek.FindAll("#mstGecmis .mst-grup-bas"));   // tek araçta plaka başlığı tekrar olmaz

        api.Routes["GET /api/customers/detail"] = (HttpStatusCode.OK, Detay(null, iki: true));
        var cok = Ac();
        cok.WaitForAssertion(() => Assert.Contains(api.Calls, x => x.Path.EndsWith($"vehicleId={V2}")));
        Assert.Contains(api.Calls, x => x.Path.EndsWith($"vehicleId={V1}"));
        Assert.Equal(2, cok.FindAll("#mstGecmis .mst-grup-bas").Count);
    }

    [Fact]
    public void No_consent_by_default_and_granting_asks_the_server()
    {
        var c = Ac();
        c.WaitForAssertion(() => Assert.Equal("false", c.Find("#mstIzin").GetAttribute("aria-pressed")));
        Assert.Contains("İzin yok: toplu mesajlar bu müşteriye gitmez.", c.Find("#mstKunye").TextContent);

        api.Routes["PUT /api/customers/consent"] = (HttpStatusCode.OK, Detay("2026-10-27", iki: false));
        c.Find("#mstIzin").Click();
        c.WaitForAssertion(() => Assert.Equal("true", c.Find("#mstIzin").GetAttribute("aria-pressed")));
        Assert.Contains("İzin 27.10.2026 tarihinde kaydedildi.", c.Find("#mstKunye").TextContent);
        using var body = JsonDocument.Parse(api.Calls.Single(x => x.Path == "/api/customers/consent").Body!);
        Assert.Equal(Key, body.RootElement.GetProperty("key").GetString());
        Assert.True(body.RootElement.GetProperty("granted").GetBoolean());
        Assert.False(body.RootElement.TryGetProperty("date", out _));   // gün istemciden gitmez
    }

    [Fact]
    public void Removing_consent_can_be_undone_and_the_old_date_comes_back()
    {
        api.Routes["GET /api/customers/detail"] = (HttpStatusCode.OK, Detay("2026-10-01", iki: false));
        var c = Ac();
        c.WaitForAssertion(() => Assert.Equal("true", c.Find("#mstIzin").GetAttribute("aria-pressed")));

        api.Routes["PUT /api/customers/consent"] = (HttpStatusCode.OK, Detay(null, iki: false));
        c.Find("#mstIzin").Click();
        c.WaitForAssertion(() => Assert.Contains("İzin kaldırıldı.", c.Find("#mstGeriAlBildirim").TextContent));
        Assert.False(JsonDocument.Parse(api.Calls.Single(x => x.Path == "/api/customers/consent").Body!).RootElement.GetProperty("granted").GetBoolean());

        api.Routes["POST /api/customers/consent/undo"] = (HttpStatusCode.OK, Detay("2026-10-01", iki: false));
        c.Find("#mstGeriAl").Click();
        c.WaitForAssertion(() => Assert.Contains("İzin 01.10.2026 tarihinde kaydedildi.", c.Find("#mstKunye").TextContent));
        Assert.Empty(c.FindAll("#mstGeriAlBildirim"));
        Assert.Contains(Key, Regex(api.Calls.Single(x => x.Path == "/api/customers/consent/undo").Body!));
    }

    [Fact]
    public void Message_button_opens_the_message_screen_with_the_first_vehicle()
    {
        api.Routes["GET /api/customers/detail"] = (HttpStatusCode.OK, Detay(null, iki: true));
        var c = Ac();
        c.WaitForElement("#mstMesaj").Click();
        Assert.Equal($"http://localhost/mesaj?arac={V1}", Nav.Uri);
    }

    [Fact]
    public void Without_phone_no_consent_chip_and_says_why()
    {
        api.Routes["GET /api/customers/detail"] = (HttpStatusCode.OK, Detay(null, iki: false, tel: null));
        var c = Ac();
        c.WaitForAssertion(() => Assert.Contains("Telefon yok — mesaj gönderilemez.", c.Find("#mstKunye").TextContent));
        Assert.Empty(c.FindAll("#mstIzin"));
        Assert.Empty(c.FindAll("#mstMesaj"));
    }

    [Fact]
    public void Gone_customer_says_so_and_offers_the_way_back()
    {
        api.Routes["GET /api/customers/detail"] = (HttpStatusCode.NotFound, "{\"code\":\"not_found\",\"title\":\"Müşteri bulunamadı.\"}");
        var c = Ac();
        c.WaitForAssertion(() => Assert.Contains("Bu müşteri artık yok", c.Markup));
        c.Find("#mstListeyeDon").Click();
        Assert.Equal("http://localhost/musteriler", Nav.Uri);
    }

    static string Regex(string s) => System.Text.RegularExpressions.Regex.Unescape(s);
}
