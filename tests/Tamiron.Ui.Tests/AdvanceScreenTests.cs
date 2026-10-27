using System.Net;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Tamirhane.Ui.Pages;
using Tamiron.Ui.Core;

namespace Tamiron.Ui.Tests;

/// <summary>
/// G8 · Avans ekranda (K-081, TASARIM-G-AVANS §5): yeni ekran YOK — kayıt künyesinde sahibin bakiyesi,
/// "Avans al" · "İade et" · "Geri al"; bakiye varsa teslimde ve tahsilatta "Avanstan" çipi (usta seçer,
/// kendiliğinden düşmez). Bakiye ve sınırlar API'de.
/// </summary>
public class AdvanceScreenTests : TestContext
{
    readonly FakeApi api = new();
    readonly MemoryTokens tokens = new("t-1");
    const string Id = "11111111-1111-1111-1111-111111111111";
    const string Vid = "22222222-2222-2222-2222-222222222222";
    const string A1 = "cccccccc-0000-0000-0000-000000000001";

    public AdvanceScreenTests()
    {
        Services.AddSingleton<ITokenStore>(tokens);
        Services.AddSingleton(new TamironApi(new HttpClient(api) { BaseAddress = new Uri("http://api.test/") }, tokens));
        api.Routes[$"GET /api/vehicles/{Vid}"] = (HttpStatusCode.OK, $"{{\"id\":\"{Vid}\",\"plateNo\":\"06 TMR 07\",\"ownerName\":\"Veli Kaya\",\"ownerPhone\":\"0532 111 22 33\"}}");
        api.Routes["GET /api/shop"] = (HttpStatusCode.OK, "{\"name\":\"Usta\",\"maintenanceMonths\":6,\"maintenanceKm\":10000,\"askPartCost\":false}");
        api.Routes[$"GET /api/records/{Id}/delivery-preview"] = (HttpStatusCode.OK, $"{{\"recordId\":\"{Id}\",\"reminder\":null}}");
    }

    static string Bakiye(decimal b, string items = "[]") => $"{{\"balance\":{b},\"items\":{items}}}";
    static readonly string BirAlis = $"[{{\"id\":\"{A1}\",\"kind\":\"Received\",\"amount\":1000,\"method\":\"Cash\",\"on\":\"2026-10-27\",\"plateNo\":\"06 TMR 07\"}}]";

    static string Kayit(string delivered = "null", decimal remaining = 0, decimal paid = 0, string method = "null")
        => $"{{\"id\":\"{Id}\",\"vehicleId\":\"{Vid}\",\"plateNo\":\"06 TMR 07\",\"type\":\"Repair\",\"complaint\":\"\",\"deliveredAt\":{delivered},"
         + $"\"paymentMethod\":{method},\"waitingForPart\":false,\"showWaitingToCustomer\":false,\"total\":3450,"
         + "\"lines\":[{\"id\":\"aaaaaaaa-0000-0000-0000-000000000001\",\"kind\":\"Labor\",\"name\":\"Fren\",\"price\":3450,\"countsInTotal\":true}],"
         + $"\"warnings\":[],\"paid\":{paid},\"remaining\":{remaining},\"payments\":[]}}";

    IRenderedComponent<Kayit> KayitAc(string kayit, string avans)
    {
        api.Routes[$"GET /api/records/{Id}"] = (HttpStatusCode.OK, kayit);
        api.Routes[$"GET /api/vehicles/{Vid}/advance"] = (HttpStatusCode.OK, avans);
        return RenderComponent<Kayit>(p => p.Add(x => x.Id, Guid.Parse(Id)));
    }

    [Fact]
    public void No_balance_shows_only_take_advance()
    {
        var c = KayitAc(Kayit(), Bakiye(0));
        c.WaitForElement("#kAvansAl");
        Assert.Empty(c.FindAll("#kAvansIade"));
        Assert.DoesNotContain("Avans:", c.Find("#kAvans").TextContent);
    }

    [Fact]
    public void Taking_an_advance_needs_amount_and_way_and_can_be_undone()
    {
        api.Routes[$"POST /api/vehicles/{Vid}/advance"] = (HttpStatusCode.OK, Bakiye(1000, BirAlis));
        api.Routes[$"POST /api/advances/{A1}/undo"] = (HttpStatusCode.OK, Bakiye(0));
        var c = KayitAc(Kayit(), Bakiye(0));
        c.WaitForElement("#kAvansAl").Click();
        c.Find("[data-avans-yol='Cash']").Click();
        Assert.Equal("Avansın tutarını yaz.", c.Find("#kHata").TextContent.Trim());
        c.Find("#kAvansTutar").Input("1.000");
        c.Find("[data-avans-yol='Cash']").Click();
        c.WaitForAssertion(() => Assert.Contains("Avans: 1.000 ₺", c.Find("#kAvans").TextContent));
        Assert.Contains("1.000 ₺ avans alındı · Nakit.", c.Find("#kAvansNot").TextContent);
        Assert.Contains("\"amount\":1000", api.Calls.Single(x => x.Path == $"/api/vehicles/{Vid}/advance" && x.Body is not null).Body);
        c.Find("#kAvansGeriAl").Click();
        c.WaitForAssertion(() => Assert.DoesNotContain("Avans:", c.Find("#kAvans").TextContent));
        Assert.Empty(c.FindAll("#kAvansNot"));
    }

    [Fact]
    public void Refund_is_offered_only_with_a_balance_and_goes_to_the_api()
    {
        api.Routes[$"POST /api/vehicles/{Vid}/advance/refund"] = (HttpStatusCode.OK, Bakiye(700));
        var c = KayitAc(Kayit(), Bakiye(1000, BirAlis));
        c.WaitForElement("#kAvansIade").Click();
        c.Find("#kAvansTutar").Input("300");
        c.Find("[data-avans-yol='Cash']").Click();
        c.WaitForAssertion(() => Assert.Contains("Avans: 700 ₺", c.Find("#kAvans").TextContent));
        Assert.Contains("300 ₺ avans iade edildi · Nakit.", c.Find("#kAvansNot").TextContent);
    }

    [Fact]
    public void Over_refund_is_the_apis_sentence()
    {
        api.Routes[$"POST /api/vehicles/{Vid}/advance/refund"] = (HttpStatusCode.UnprocessableEntity,
            "{\"code\":\"advance.over_balance\",\"title\":\"Avans 1.000 ₺ — fazlası iade edilemez.\"}");
        var c = KayitAc(Kayit(), Bakiye(1000, BirAlis));
        c.WaitForElement("#kAvansIade").Click();
        c.Find("#kAvansTutar").Input("5.000");
        c.Find("[data-avans-yol='Card']").Click();
        c.WaitForAssertion(() => Assert.Equal("Avans 1.000 ₺ — fazlası iade edilemez.", c.Find("#kHata").TextContent.Trim()));
    }

    [Fact]
    public void Collecting_offers_advance_when_there_is_a_balance()
    {
        api.Routes[$"POST /api/records/{Id}/collect"] = (HttpStatusCode.OK, Kayit("\"2026-10-20\"", 2450, 1000, "\"Unpaid\""));
        var c = KayitAc(Kayit("\"2026-10-20\"", 3450, 0, "\"Unpaid\""), Bakiye(1000, BirAlis));
        c.WaitForElement("[data-tahsil='Advance']");
        Assert.Equal("Avanstan (1.000 ₺)", c.Find("[data-tahsil='Advance']").TextContent.Trim());
        c.Find("#kTahsilTutar").Input("1.000");
        api.Routes[$"GET /api/vehicles/{Vid}/advance"] = (HttpStatusCode.OK, Bakiye(0));
        c.Find("[data-tahsil='Advance']").Click();
        c.WaitForAssertion(() => Assert.Contains("1.000 ₺ alındı · Avanstan · kalan 2.450 ₺.", c.Find("#kTahsilNot").TextContent));
        Assert.Contains("\"method\":\"Advance\"", api.Calls.Single(x => x.Path.EndsWith("/collect")).Body);
        c.WaitForAssertion(() => Assert.Empty(c.FindAll("[data-tahsil='Advance']")));   // bakiye bitti
    }

    [Fact]
    public void Advance_payment_offers_no_way_correction()
    {
        // Tarayıcıda bulundu: son ödemesi avanstan olan işte "Düzenle" çıkıyordu — API reddeder (K-081).
        var odenmis = Kayit("\"2026-10-20\"", 0, 3450, "\"Advance\"").Replace("\"payments\":[]",
            "\"payments\":[{\"id\":\"bbbbbbbb-0000-0000-0000-000000000001\",\"amount\":3450,\"method\":\"Advance\",\"paidOn\":\"2026-10-27\",\"atDelivery\":false}]");
        var c = KayitAc(odenmis, Bakiye(0));
        c.WaitForAssertion(() => Assert.Contains("Avanstan", c.Find("#kTeslim").TextContent));
        Assert.Empty(c.FindAll("#kDuzenle"));
    }

    [Fact]
    public void Using_the_advance_drops_the_stale_advance_note()
    {
        // Tarayıcıda bulundu: avans alındı bildirimi ("Geri al") avanstan düşüldükten sonra da duruyordu;
        // o geri alma artık reddedilir (bakiye eksiye düşerdi).
        api.Routes[$"POST /api/vehicles/{Vid}/advance"] = (HttpStatusCode.OK, Bakiye(1000, BirAlis));
        api.Routes[$"POST /api/records/{Id}/collect"] = (HttpStatusCode.OK, Kayit("\"2026-10-20\"", 2450, 1000, "\"Unpaid\""));
        var c = KayitAc(Kayit("\"2026-10-20\"", 3450, 0, "\"Unpaid\""), Bakiye(0));
        c.WaitForElement("#kAvansAl").Click();
        c.Find("#kAvansTutar").Input("1.000");
        c.Find("[data-avans-yol='Cash']").Click();
        c.WaitForElement("#kAvansNot");
        c.Find("#kTahsilTutar").Input("1.000");
        c.Find("[data-tahsil='Advance']").Click();
        c.WaitForAssertion(() => Assert.Empty(c.FindAll("#kAvansNot")));
    }

    [Fact]
    public void No_advance_chip_without_balance()
    {
        var c = KayitAc(Kayit("\"2026-10-20\"", 3450, 0, "\"Unpaid\""), Bakiye(0));
        c.WaitForElement("[data-tahsil='Cash']");
        Assert.Empty(c.FindAll("[data-tahsil='Advance']"));
    }

    [Fact]
    public void Delivery_offers_advance_as_a_way_and_in_the_part_ways()
    {
        api.Routes[$"GET /api/records/{Id}"] = (HttpStatusCode.OK, Kayit());
        api.Routes[$"GET /api/vehicles/{Vid}/advance"] = (HttpStatusCode.OK, Bakiye(1000, BirAlis));
        api.Routes[$"POST /api/records/{Id}/deliver"] = (HttpStatusCode.OK, Kayit("\"2026-10-27\"", 2450, 1000, "\"Unpaid\""));
        var c = RenderComponent<Teslim>(p => p.Add(x => x.Id, Guid.Parse(Id)));
        c.WaitForElement("[data-odeme='Advance']");
        Assert.Equal("Avanstan (1.000 ₺)", c.Find("[data-odeme='Advance']").TextContent.Trim());
        c.Find("[data-odeme='kismi']").Click();
        c.Find("#tsKismiTutar").Input("1.000");
        c.Find("[data-kismi-yol='Advance']").Click();
        c.Find("#tsTeslim").Click();
        c.WaitForAssertion(() => Assert.Contains("1.000 ₺ alındı · kalan 2.450 ₺", c.Find("#tsNot").TextContent));
        var body = api.Calls.Single(x => x.Path.EndsWith("/deliver")).Body!;
        Assert.Contains("\"method\":\"Advance\"", body);
        Assert.Contains("\"amount\":1000", body);
    }

    [Fact]
    public void Advance_way_has_a_turkish_name()
        => Assert.Equal("Avanstan", Format.PaymentLabel("Advance"));
}
