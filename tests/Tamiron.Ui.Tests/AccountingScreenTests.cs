using System.Net;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Tamirhane.Ui.Pages;
using Tamiron.Ui.Core;

namespace Tamiron.Ui.Tests;

/// <summary>
/// G6 · Muhasebe (prototip 95-muhasebe "Basit"). Tek çağrı (C8): kasa özeti dönemin, gider dönemin, alacak
/// döneme tabi DEĞİL (bakiye — Cemil 05.09). Rakamlar API'den; ekran yalnız yazar. CSV API'den indirilir.
/// </summary>
public class AccountingScreenTests : TestContext
{
    readonly FakeApi api = new();
    readonly MemoryTokens tokens = new("t-1");

    public AccountingScreenTests()
    {
        Services.AddSingleton<ITokenStore>(tokens);
        Services.AddSingleton(new TamironApi(new HttpClient(api) { BaseAddress = new Uri("http://api.test/") }, tokens));
        JSInterop.SetupVoid("tamironIndir", _ => true);
    }

    NavigationManager Nav => Services.GetRequiredService<NavigationManager>();

    const string R1 = "11111111-1111-1111-1111-111111111111";
    const string E1 = "eeeeeeee-0000-0000-0000-000000000001";

    static string Ozet(string period = "Today", decimal cash = 500, decimal card = 0, decimal transfer = 400, int delivered = 1,
                       string expenses = "[]", decimal expTotal = 0, string receivables = "[]", decimal recTotal = 0)
        => $"{{\"cash\":{{\"period\":\"{period}\",\"start\":\"2026-10-27\",\"end\":\"2026-10-27\",\"deliveredCount\":{delivered},"
         + $"\"cash\":{cash},\"card\":{card},\"transfer\":{transfer},\"collected\":{cash + card + transfer},\"receivable\":{recTotal}}},"
         + $"\"expenses\":{{\"period\":\"{period}\",\"start\":\"2026-10-27\",\"end\":\"2026-10-27\",\"total\":{expTotal},\"items\":{expenses}}},"
         + $"\"receivables\":{{\"total\":{recTotal},\"items\":{receivables}}}}}";

    static readonly string Gider = $"[{{\"id\":\"{E1}\",\"date\":\"2026-10-27\",\"description\":\"Elektrik\",\"amount\":2400}}]";
    static readonly string Alacak = $"[{{\"recordId\":\"{R1}\",\"vehicleId\":\"22222222-2222-2222-2222-222222222222\",\"plateNo\":\"34 ABC 123\","
                                  + "\"ownerName\":null,\"description\":\"Balata\",\"deliveredAt\":\"2026-10-26\",\"total\":1500,\"paid\":900,\"remaining\":600}]";

    // H1 · OtoMix görünümü (prototip "Detaylı"): modül tahtası. Yazılmamış modül AÇILIR GİBİ DURMAZ — devre dışı
    // ve "yazılmadı" der (ölü uç, 27. test). Sıra Tamiron'un: çalışanlar önce.
    [Fact]
    public void Detailed_tab_shows_the_module_board_working_ones_first()
    {
        api.Routes["GET /api/accounting"] = (HttpStatusCode.OK, Ozet());
        var c = RenderComponent<Muhasebe>();
        c.WaitForElement("#muhOzet");
        Assert.Empty(c.FindAll("#muhModuller"));
        c.Find("#muhSekDetay").Click();
        Assert.Equal("true", c.Find("#muhSekDetay").GetAttribute("aria-selected"));
        Assert.Empty(c.FindAll("#muhOzet"));   // iki gövde aynı anda görünmez
        var kutular = c.FindAll("#muhModuller .modul-kutu");
        Assert.Equal(new[] { "Kasa", "Raporlar", "SMS" }, kutular.Take(3).Select(k => k.QuerySelector(".mk-ad")!.TextContent));
        Assert.All(kutular.Skip(3), k => Assert.True(k.HasAttribute("disabled")));
        Assert.All(kutular.Skip(3), k => Assert.Equal("yazılmadı", k.QuerySelector(".mk-alt")!.TextContent));
        Assert.All(kutular.Take(3), k => Assert.False(k.HasAttribute("disabled")));
    }

    [Fact]
    public void Kasa_module_is_the_simple_tab_itself()
    {
        api.Routes["GET /api/accounting"] = (HttpStatusCode.OK, Ozet());
        var c = RenderComponent<Muhasebe>();
        c.WaitForElement("#muhSekDetay").Click();
        c.Find("[data-muhmodul='kasa']").Click();
        Assert.Equal("true", c.Find("#muhSekBasit").GetAttribute("aria-selected"));
        Assert.NotNull(c.Find("#muhOzet"));
    }

    [Fact]
    public void Reports_open_per_job_with_the_same_period_and_sms_opens_messages()
    {
        // Dönem iki ekranda AYNI: muhasebede "Bu ay" seçip rapora gelen "Bugün" bulmasın (prototip muhDonemSecili).
        api.Routes["GET /api/accounting"] = (HttpStatusCode.OK, Ozet());
        var c = RenderComponent<Muhasebe>();
        c.WaitForElement("[data-donem='Month']").Click();
        c.Find("#muhSekDetay").Click();
        c.Find("[data-muhmodul='rapor']").Click();
        Assert.Equal("http://localhost/is-basina?donem=Month", Nav.Uri);
        var c2 = RenderComponent<Muhasebe>();
        c2.WaitForElement("#muhSekDetay").Click();
        c2.Find("[data-muhmodul='sms']").Click();
        Assert.Equal("http://localhost/mesaj", Nav.Uri);
    }

    [Fact]
    public void Without_session_goes_to_login()
    {
        tokens.Token = null;
        RenderComponent<Muhasebe>();
        Assert.Equal("http://localhost/giris", Nav.Uri);
    }

    [Fact]
    public void Today_summary_shows_each_way_and_the_collected_total()
    {
        api.Routes["GET /api/accounting"] = (HttpStatusCode.OK, Ozet());
        var c = RenderComponent<Muhasebe>();
        c.WaitForAssertion(() => Assert.Equal("900 ₺", c.Find("#muhOzet .genel .tv").TextContent.Trim()));
        var satirlar = c.FindAll("#muhOzet .tutar-sat").Select(x => x.TextContent.Replace(" ", "")).ToList();
        Assert.Equal(["Teslimedilen1araç", "Nakit500₺", "Kart0₺", "Havale400₺", "Tahsiledilen900₺"], satirlar);
        Assert.Equal("/api/accounting?period=Today", api.Calls[0].Path);
        Assert.Equal("true", c.Find("[data-donem='Today']").GetAttribute("aria-pressed"));
    }

    [Fact]
    public void Period_chip_asks_again_for_that_period()
    {
        api.Routes["GET /api/accounting"] = (HttpStatusCode.OK, Ozet());
        var c = RenderComponent<Muhasebe>();
        c.WaitForElement("[data-donem='Month']").Click();
        c.WaitForAssertion(() => Assert.Contains(api.Calls, x => x.Path == "/api/accounting?period=Month"));
        Assert.Equal("true", c.Find("[data-donem='Month']").GetAttribute("aria-pressed"));
        Assert.Equal(Format.MonthName(DateTime.Today), c.Find("[data-donem='Month']").TextContent.Trim());
    }

    [Fact]
    public void Receivables_say_who_and_how_much_is_left_and_open_the_record()
    {
        api.Routes["GET /api/accounting"] = (HttpStatusCode.OK, Ozet(receivables: Alacak, recTotal: 600));
        var c = RenderComponent<Muhasebe>();
        c.WaitForAssertion(() => Assert.Contains("sahip yazılmamış", c.Find("#muhAlacak").TextContent));
        Assert.Contains("600 ₺", c.Find("#muhAlacak tr[data-kayit]").TextContent);
        Assert.Contains("900 ₺ alındı", c.Find("#muhAlacak tr[data-kayit]").TextContent);
        Assert.Equal("600 ₺", c.Find("#muhAlacak .genel .sayi").TextContent.Trim());
        c.Find("#muhAlacak tr[data-kayit]").Click();
        Assert.Equal($"http://localhost/kayit/{R1}", Nav.Uri);
    }

    [Fact]
    public void No_receivable_is_said_plainly()
    {
        api.Routes["GET /api/accounting"] = (HttpStatusCode.OK, Ozet());
        var c = RenderComponent<Muhasebe>();
        c.WaitForAssertion(() => Assert.Contains("Alacak yok", c.Find("#muhAlacak").TextContent));
    }

    [Fact]
    public void Expense_needs_both_fields_then_is_added_and_the_view_refreshes()
    {
        api.Routes["GET /api/accounting"] = (HttpStatusCode.OK, Ozet());
        api.Routes["POST /api/expenses"] = (HttpStatusCode.OK, $"{{\"id\":\"{E1}\",\"date\":\"2026-10-27\",\"description\":\"Elektrik\",\"amount\":2400}}");
        var c = RenderComponent<Muhasebe>();
        c.WaitForElement("#muhGiderEkle").Click();
        c.Find("#muhGiderAd").Input("Elektrik");
        c.Find("#muhGiderKaydet").Click();
        Assert.Equal("Gider için hem açıklama hem tutar gerekiyor.", c.Find("#muhHata").TextContent.Trim());
        api.Routes["GET /api/accounting"] = (HttpStatusCode.OK, Ozet(expenses: Gider, expTotal: 2400));
        c.Find("#muhGiderTutar").Input("2.400");
        c.Find("#muhGiderKaydet").Click();
        c.WaitForAssertion(() => Assert.Contains("Elektrik", c.Find("#muhGiderler").TextContent));
        var body = api.Calls.Single(x => x.Path == "/api/expenses").Body!;
        Assert.Contains("\"amount\":2400", body);
        Assert.Empty(c.FindAll("#muhGiderForm"));
    }

    [Fact]
    public void Expense_delete_can_be_undone()
    {
        api.Routes["GET /api/accounting"] = (HttpStatusCode.OK, Ozet(expenses: Gider, expTotal: 2400));
        api.Routes[$"DELETE /api/expenses/{E1}"] = (HttpStatusCode.OK, "{\"period\":\"Today\",\"total\":0,\"items\":[]}");
        api.Routes[$"POST /api/expenses/{E1}/restore"] = (HttpStatusCode.OK, $"{{\"period\":\"Today\",\"total\":2400,\"items\":{Gider}}}");
        var c = RenderComponent<Muhasebe>();
        c.WaitForElement($"[data-gider-sil='{E1}']").Click();
        c.WaitForAssertion(() => Assert.Contains("Gider yazılmamış", c.Find("#muhGiderler").TextContent));
        Assert.Contains("period=Today", api.Calls.Single(x => x.Path.StartsWith($"/api/expenses/{E1}")).Path);
        c.Find("#muhGiderGeriAl").Click();
        c.WaitForAssertion(() => Assert.Contains("Elektrik", c.Find("#muhGiderler").TextContent));
    }

    [Fact]
    public void Csv_comes_from_the_api_for_the_chosen_period()
    {
        api.Routes["GET /api/accounting"] = (HttpStatusCode.OK, Ozet());
        api.Routes["GET /api/export/cash.csv"] = (HttpStatusCode.OK, "﻿Tarih;Tutar\n");
        var c = RenderComponent<Muhasebe>();
        c.WaitForElement("#muhCsv").Click();
        c.WaitForAssertion(() => Assert.Single(JSInterop.Invocations, i => i.Identifier == "tamironIndir"));
        Assert.Contains(api.Calls, x => x.Path == "/api/export/cash.csv?period=Today");
        Assert.Equal("Bearer t-1", api.Calls.Last().Auth);
    }
}
