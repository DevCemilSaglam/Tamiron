using System.Net;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Tamirhane.Ui.Pages;
using Tamiron.Ui.Core;

namespace Tamiron.Ui.Tests;

/// <summary>
/// G5 · Kayıtta ödeme (prototip 30-kayit kTeslim + kTahsil, K-078, K-077.2). Teslimli kayıtta alınanlar
/// listesi ve kalan; kalanı olan işte "Ödeme alındı" (tutar kalanla dolu + yol), geri alınabilir;
/// ödeme yolunu düzeltme ön planda değil — tek "Düzenle" girişinin altında. Kurallar API'de.
/// </summary>
public class RecordPaymentScreenTests : TestContext
{
    readonly FakeApi api = new();
    readonly MemoryTokens tokens = new("t-1");
    const string Id = "11111111-1111-1111-1111-111111111111";
    const string Vid = "22222222-2222-2222-2222-222222222222";

    public RecordPaymentScreenTests()
    {
        Services.AddSingleton<ITokenStore>(tokens);
        Services.AddSingleton(new TamironApi(new HttpClient(api) { BaseAddress = new Uri("http://api.test/") }, tokens));
        api.Routes[$"GET /api/vehicles/{Vid}"] = (HttpStatusCode.OK, $"{{\"id\":\"{Vid}\",\"plateNo\":\"06 TMR 07\",\"ownerPhone\":\"0532 111 22 33\"}}");
    }

    static string Odeme(string id, decimal amount, string method, string on, bool atDelivery)
        => $"{{\"id\":\"{id}\",\"amount\":{amount},\"method\":\"{method}\",\"paidOn\":\"{on}\",\"atDelivery\":{(atDelivery ? "true" : "false")}}}";

    static string Kayit(decimal paid, decimal remaining, string method, string payments)
        => $"{{\"id\":\"{Id}\",\"vehicleId\":\"{Vid}\",\"plateNo\":\"06 TMR 07\",\"type\":\"Repair\",\"complaint\":\"\",\"deliveredAt\":\"2026-10-20\","
         + $"\"paymentMethod\":\"{method}\",\"waitingForPart\":false,\"showWaitingToCustomer\":false,\"total\":3450,"
         + $"\"lines\":[{{\"id\":\"aaaaaaaa-0000-0000-0000-000000000001\",\"kind\":\"Labor\",\"name\":\"Fren\",\"price\":3450,\"countsInTotal\":true}}],"
         + $"\"warnings\":[],\"paid\":{paid},\"remaining\":{remaining},\"payments\":{payments}}}";

    static readonly string Veresiye = Kayit(0, 3450, "Unpaid", "[]");
    static readonly string Kismi = Kayit(1000, 2450, "Unpaid", "[" + Odeme("bbbbbbbb-0000-0000-0000-000000000001", 1000, "Cash", "2026-10-20", true) + "]");
    static readonly string KismiIkinci = Kayit(1500, 1950, "Unpaid", "[" + Odeme("bbbbbbbb-0000-0000-0000-000000000001", 1000, "Cash", "2026-10-20", true)
                                                              + "," + Odeme("bbbbbbbb-0000-0000-0000-000000000002", 500, "Card", "2026-10-27", false) + "]");
    static readonly string Odenmis = Kayit(3450, 0, "Cash", "[" + Odeme("bbbbbbbb-0000-0000-0000-000000000001", 3450, "Cash", "2026-10-20", true) + "]");

    IRenderedComponent<Kayit> Ac(string kayit)
    {
        api.Routes[$"GET /api/records/{Id}"] = (HttpStatusCode.OK, kayit);
        return RenderComponent<Kayit>(p => p.Add(x => x.Id, Guid.Parse(Id)));
    }

    [Fact]
    public void Delivered_record_lists_what_was_taken_and_the_rest()
    {
        var c = Ac(Kismi);
        c.WaitForAssertion(() => Assert.Contains("Teslim edildi · 20.10.2026 · 1.000 ₺ alındı · kalan 2.450 ₺", c.Find("#kTeslim").TextContent));
        Assert.Equal(["20.10.2026 · 1.000 ₺ · Nakit · teslimde"], c.FindAll("#kOdemeler li").Select(x => x.TextContent.Trim()));
    }

    [Fact]
    public void Remaining_is_offered_prefilled_and_taken_with_a_way()
    {
        api.Routes[$"POST /api/records/{Id}/collect"] = (HttpStatusCode.OK, KismiIkinci);
        var c = Ac(Kismi);
        c.WaitForAssertion(() => Assert.Equal("2.450", c.Find("#kTahsilTutar").GetAttribute("value")));
        c.Find("#kTahsilTutar").Input("500");
        c.Find("[data-tahsil='Card']").Click();
        c.WaitForAssertion(() => Assert.Contains("500 ₺ alındı · Kart · kalan 1.950 ₺.", c.Find("#kTahsilNot").TextContent));
        var body = api.Calls.Single(x => x.Path.EndsWith("/collect")).Body!;
        Assert.Contains("\"method\":\"Card\"", body);
        Assert.Contains("\"amount\":500", body);
        Assert.Equal(2, c.FindAll("#kOdemeler li").Count);
    }

    [Fact]
    public void Taking_payment_can_be_undone()
    {
        api.Routes[$"POST /api/records/{Id}/collect"] = (HttpStatusCode.OK, KismiIkinci);
        api.Routes[$"POST /api/records/{Id}/collect/undo"] = (HttpStatusCode.OK, Kismi);
        var c = Ac(Kismi);
        c.WaitForElement("#kTahsilTutar").Input("500");
        c.Find("[data-tahsil='Card']").Click();
        c.WaitForElement("#kTahsilGeriAl").Click();
        c.WaitForAssertion(() => Assert.Single(c.FindAll("#kOdemeler li")));
        Assert.Empty(c.FindAll("#kTahsilNot"));
        Assert.Contains(api.Calls, x => x.Path.EndsWith("/collect/undo"));
    }

    [Fact]
    public void Over_payment_is_the_apis_sentence()
    {
        api.Routes[$"POST /api/records/{Id}/collect"] = (HttpStatusCode.UnprocessableEntity,
            "{\"code\":\"payment.over_remaining\",\"title\":\"Kalan 3.450 ₺ — fazlası girilemez.\"}");
        var c = Ac(Veresiye);
        c.WaitForElement("#kTahsilTutar").Input("5.000");
        c.Find("[data-tahsil='Cash']").Click();
        c.WaitForAssertion(() => Assert.Equal("Kalan 3.450 ₺ — fazlası girilemez.", c.Find("#kHata").TextContent.Trim()));
        Assert.Equal("5.000", c.Find("#kTahsilTutar").GetAttribute("value"));
    }

    [Fact]
    public void Paid_record_offers_no_collection()
    {
        var c = Ac(Odenmis);
        c.WaitForAssertion(() => Assert.Contains("Teslim edildi · 20.10.2026 · Nakit", c.Find("#kTeslim").TextContent));
        Assert.Empty(c.FindAll("#kTahsilTutar"));
    }

    [Fact]
    public void Way_correction_is_behind_edit_and_goes_to_the_api()
    {
        api.Routes[$"POST /api/records/{Id}/payment"] = (HttpStatusCode.OK, Odenmis.Replace("\"method\":\"Cash\"", "\"method\":\"Card\"").Replace("\"paymentMethod\":\"Cash\"", "\"paymentMethod\":\"Card\""));
        var c = Ac(Odenmis);
        c.WaitForElement("#kDuzenle");
        Assert.Empty(c.FindAll("[data-odeme-duzelt]"));   // ön planda değil
        c.Find("#kDuzenle").Click();
        Assert.Equal("true", c.Find("[data-odeme-duzelt='Cash']").GetAttribute("aria-pressed"));
        c.Find("[data-odeme-duzelt='Card']").Click();
        c.WaitForAssertion(() => Assert.Contains("Teslim edildi · 20.10.2026 · Kart", c.Find("#kTeslim").TextContent));
        Assert.Contains("\"method\":\"Card\"", api.Calls.Single(x => x.Path.EndsWith("/payment")).Body);
    }

    [Fact]
    public void Correcting_the_way_drops_the_stale_taken_note()
    {
        // Tarayıcıda bulundu: havale ile alınıp karta düzeltilince bildirim hâlâ "Havale" diyordu.
        api.Routes[$"POST /api/records/{Id}/collect"] = (HttpStatusCode.OK, KismiIkinci);
        api.Routes[$"POST /api/records/{Id}/payment"] = (HttpStatusCode.OK, KismiIkinci.Replace("\"method\":\"Card\"", "\"method\":\"Transfer\""));
        var c = Ac(Kismi);
        c.WaitForElement("#kTahsilTutar").Input("500");
        c.Find("[data-tahsil='Card']").Click();
        c.WaitForElement("#kTahsilNot");
        c.Find("#kDuzenle").Click();
        c.Find("[data-odeme-duzelt='Transfer']").Click();
        c.WaitForAssertion(() => Assert.Empty(c.FindAll("#kTahsilNot")));
        Assert.Contains("500 ₺ · Havale", c.Find("#kOdemeler").TextContent);
    }

    [Fact]
    public void Unpaid_record_has_no_way_to_correct()
    {
        // K-077.2: veresiye düzeltilmez — "havaleye çevir" geriye dönük kasaya para yazardı; o tahsilattır.
        var c = Ac(Veresiye);
        c.WaitForElement("#kTahsilTutar");
        Assert.Empty(c.FindAll("#kDuzenle"));
    }
}
