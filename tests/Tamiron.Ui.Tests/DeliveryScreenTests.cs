using System.Net;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Tamirhane.Ui.Pages;
using Tamiron.Ui.Core;

namespace Tamiron.Ui.Tests;

/// <summary>
/// G4 · Teslim et (EKRANLAR §5, prototip 50-teslim). Ayrı ekran çünkü ayrı an. Ödeme yolu seçilmeden
/// düğme KONUŞUR (kapalı değil); "Bir kısmını aldım" tutar + yol ister (K-078). Fazla ödeme kuralı
/// API'de — ekran API'nin cümlesini gösterir, kendi hesabını yapmaz.
/// </summary>
public class DeliveryScreenTests : TestContext
{
    readonly FakeApi api = new();
    readonly MemoryTokens tokens = new("t-1");
    const string Id = "11111111-1111-1111-1111-111111111111";
    const string Vid = "22222222-2222-2222-2222-222222222222";

    public DeliveryScreenTests()
    {
        Services.AddSingleton<ITokenStore>(tokens);
        Services.AddSingleton(new TamironApi(new HttpClient(api) { BaseAddress = new Uri("http://api.test/") }, tokens));
        api.Routes[$"GET /api/vehicles/{Vid}/advance"] = (HttpStatusCode.OK, "{\"balance\":0,\"items\":[]}");   // G8: avans yok
        api.Routes[$"GET /api/vehicles/{Vid}"] = (HttpStatusCode.OK,
            $"{{\"id\":\"{Vid}\",\"plateNo\":\"06 TMR 07\",\"brand\":\"Fiat\",\"model\":\"Egea\",\"year\":\"2019\",\"engine\":\"1.3 Multijet\",\"ownerPhone\":\"0532 111 22 33\"}}");
        api.Routes["GET /api/shop"] = (HttpStatusCode.OK, "{\"name\":\"Usta\",\"maintenanceMonths\":6,\"maintenanceKm\":10000,\"askPartCost\":false}");
        api.Routes[$"GET /api/records/{Id}/delivery-preview"] = (HttpStatusCode.OK, $"{{\"recordId\":\"{Id}\",\"reminder\":null}}");
    }

    NavigationManager Nav => Services.GetRequiredService<NavigationManager>();

    static string Kayit(string type = "Repair", bool waiting = false, string lines = "[{\"id\":\"aaaaaaaa-0000-0000-0000-000000000001\",\"kind\":\"Labor\",\"name\":\"Fren\",\"price\":3450,\"countsInTotal\":true}]",
                        string delivered = "null", string method = "null", decimal paid = 0, decimal remaining = 0, string reminder = "null")
        => $"{{\"id\":\"{Id}\",\"vehicleId\":\"{Vid}\",\"plateNo\":\"06 TMR 07\",\"type\":\"{type}\",\"complaint\":\"\",\"deliveredAt\":{delivered},"
         + $"\"paymentMethod\":{method},\"waitingForPart\":{(waiting ? "true" : "false")},\"showWaitingToCustomer\":false,\"total\":3450,"
         + $"\"lines\":{lines},\"warnings\":[],\"paid\":{paid},\"remaining\":{remaining},\"reminderDate\":{reminder}}}";

    IRenderedComponent<Teslim> Ac(string kayit)
    {
        api.Routes[$"GET /api/records/{Id}"] = (HttpStatusCode.OK, kayit);
        return RenderComponent<Teslim>(p => p.Add(x => x.Id, Guid.Parse(Id)));
    }

    [Fact]
    public void Shows_what_is_collected_big_and_the_five_ways()
    {
        var c = Ac(Kayit());
        c.WaitForAssertion(() => Assert.Equal("3.450 ₺", c.Find("#tsTutar .tv").TextContent.Trim()));
        Assert.Contains("Fiat Egea", c.Find("#tsKunye").TextContent);
        Assert.Contains("2019", c.Find("#tsKunye").TextContent);
        Assert.Equal(["Tamirat", "1.3 Multijet"], c.FindAll("#tsKunye .spec-val").Select(x => x.TextContent));
        Assert.Equal(["Nakit", "Kart", "Havale", "Almadım", "Bir kısmını aldım"], c.FindAll("#tsOdeme .cip").Select(b => b.TextContent.Trim()));
    }

    [Fact]
    public void Waiting_flag_and_priceless_lines_are_told_not_blocking()
    {
        var c = Ac(Kayit(waiting: true, lines: "[{\"id\":\"aaaaaaaa-0000-0000-0000-000000000001\",\"kind\":\"Part\",\"name\":\"Balata\",\"price\":0,\"countsInTotal\":true}]"));
        c.WaitForAssertion(() => Assert.Contains("parça bekliyor olarak işaretli", c.Find("#tsTutar").TextContent));
        Assert.Contains("fiyatı girilmemiş satır var", c.Find("#tsTutar").TextContent);
        Assert.NotNull(c.Find("#tsTeslim"));
    }

    [Fact]
    public void Button_speaks_when_no_way_is_chosen_and_sends_nothing()
    {
        var c = Ac(Kayit());
        c.WaitForElement("#tsTeslim").Click();
        Assert.Equal("Ödemeyi nasıl aldığını seç — almadıysan \"Almadım\" de.", c.Find("#tsHata").TextContent.Trim());
        Assert.DoesNotContain(api.Calls, x => x.Path.EndsWith("/deliver"));
    }

    [Fact]
    public void Full_payment_delivers_with_that_way_and_says_what_happened()
    {
        api.Routes[$"POST /api/records/{Id}/deliver"] = (HttpStatusCode.OK,
            Kayit(delivered: "\"2026-10-26\"", method: "\"Card\"", paid: 3450, reminder: "\"2027-04-26\""));
        var c = Ac(Kayit());
        c.WaitForElement("[data-odeme='Card']").Click();
        Assert.Equal("true", c.Find("[data-odeme='Card']").GetAttribute("aria-pressed"));
        c.Find("#tsTeslim").Click();
        c.WaitForAssertion(() => Assert.Contains("06 TMR 07 teslim edildi.", c.Find("#tsNot").TextContent));
        Assert.Contains("Ödeme: Kart.", c.Find("#tsNot").TextContent);
        Assert.Contains("Bakım hatırlatması 26.04.2027 için kuruldu.", c.Find("#tsNot").TextContent);
        Assert.Contains("\"method\":\"Card\"", api.Calls.Single(x => x.Path.EndsWith("/deliver")).Body);
        Assert.Contains("\"amount\":null", api.Calls.Single(x => x.Path.EndsWith("/deliver")).Body);
        Assert.Empty(c.FindAll("#tsTeslim"));          // ikinci kez teslim edilmez
        c.Find("#tsKayda").Click();
        Assert.Equal($"http://localhost/kayit/{Id}", Nav.Uri);
    }

    [Fact]
    public void Unpaid_is_a_way_not_a_block()
    {
        api.Routes[$"POST /api/records/{Id}/deliver"] = (HttpStatusCode.OK, Kayit(delivered: "\"2026-10-26\"", method: "\"Unpaid\"", remaining: 3450));
        var c = Ac(Kayit());
        c.WaitForElement("[data-odeme='Unpaid']").Click();
        c.Find("#tsTeslim").Click();
        c.WaitForAssertion(() => Assert.Contains("Ödeme: Almadım.", c.Find("#tsNot").TextContent));
    }

    [Fact]
    public void Part_payment_asks_amount_then_way_then_delivers_and_shows_the_rest()
    {
        api.Routes[$"POST /api/records/{Id}/deliver"] = (HttpStatusCode.OK,
            Kayit(delivered: "\"2026-10-26\"", method: "\"Unpaid\"", paid: 1000, remaining: 2450));
        var c = Ac(Kayit());
        c.WaitForElement("[data-odeme='kismi']").Click();
        c.Find("#tsTeslim").Click();
        Assert.Equal("Ne kadar aldığını yaz.", c.Find("#tsHata").TextContent.Trim());
        c.Find("#tsKismiTutar").Input("1.000");
        c.Find("#tsTeslim").Click();
        Assert.Equal("Nasıl aldığını seç: nakit, kart ya da havale.", c.Find("#tsHata").TextContent.Trim());
        c.Find("[data-kismi-yol='Cash']").Click();
        c.Find("#tsTeslim").Click();
        c.WaitForAssertion(() => Assert.Contains("Ödeme: 1.000 ₺ alındı · kalan 2.450 ₺.", c.Find("#tsNot").TextContent));
        var body = api.Calls.Single(x => x.Path.EndsWith("/deliver")).Body!;
        Assert.Contains("\"method\":\"Cash\"", body);
        Assert.Contains("\"amount\":1000", body);
    }

    [Fact]
    public void Over_payment_is_the_apis_sentence_and_nothing_is_lost()
    {
        api.Routes[$"POST /api/records/{Id}/deliver"] = (HttpStatusCode.UnprocessableEntity,
            "{\"code\":\"payment.over_remaining\",\"title\":\"Toplam 3.450 ₺ — fazlası girilemez.\"}");
        var c = Ac(Kayit());
        c.WaitForElement("[data-odeme='kismi']").Click();
        c.Find("#tsKismiTutar").Input("5.000");
        c.Find("[data-kismi-yol='Card']").Click();
        c.Find("#tsTeslim").Click();
        c.WaitForAssertion(() => Assert.Equal("Toplam 3.450 ₺ — fazlası girilemez.", c.Find("#tsHata").TextContent.Trim()));
        Assert.Equal("5.000", c.Find("#tsKismiTutar").GetAttribute("value"));   // yazılan silinmez
        Assert.NotNull(c.Find("#tsTeslim"));
    }

    [Fact]
    public void Maintenance_shows_the_reminder_that_will_be_set()
    {
        api.Routes[$"GET /api/records/{Id}/delivery-preview"] = (HttpStatusCode.OK,
            $"{{\"recordId\":\"{Id}\",\"reminder\":{{\"date\":\"2027-04-26\",\"basis\":\"Month\",\"monthDate\":\"2027-04-26\",\"kmTarget\":null,\"kmDate\":null}}}}");
        var c = Ac(Kayit(type: "Maintenance"));
        c.WaitForAssertion(() => Assert.Contains("Teslimden sonra 26.04.2027 için bakım hatırlatması kurulacak (6 ay).", c.Find("#tsNot").TextContent));
    }

    [Fact]
    public void Km_reminder_says_the_km_comes_first()
    {
        api.Routes[$"GET /api/records/{Id}/delivery-preview"] = (HttpStatusCode.OK,
            $"{{\"recordId\":\"{Id}\",\"reminder\":{{\"date\":\"2027-01-10\",\"basis\":\"Km\",\"monthDate\":\"2027-04-26\",\"kmTarget\":60000,\"kmDate\":\"2027-01-10\"}}}}");
        var c = Ac(Kayit(type: "Maintenance"));
        c.WaitForAssertion(() => Assert.Contains("10.000 km aydan önce doluyor", c.Find("#tsNot").TextContent));
        Assert.Contains("10.01.2027", c.Find("#tsNot").TextContent);
    }

    [Fact]
    public void Already_delivered_record_is_not_delivered_again()
    {
        var c = Ac(Kayit(delivered: "\"2026-10-20\"", method: "\"Cash\"", paid: 3450));
        c.WaitForAssertion(() => Assert.Contains("Bu iş 20.10.2026'da teslim edildi.", c.Find("#tsNot").TextContent));
        Assert.Empty(c.FindAll("#tsTeslim"));
        Assert.Empty(c.FindAll("#tsOdeme .cip"));
    }
}
