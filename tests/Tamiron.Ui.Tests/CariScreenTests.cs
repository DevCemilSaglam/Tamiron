using System.Net;
using System.Text.Json;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Tamirhane.Ui.Pages;
using Tamiron.Ui.Core;

namespace Tamiron.Ui.Tests;

/// <summary>
/// H4 · Cari (K-090, TASARIM-H-MUHASEBE §4): OtoMix'in Cari modülünün yedi işe hizmet eden iki parçası —
/// müşteriden alacağım (var olan alacak listesi) ve parçacılara borcum (veresiye alışlar − ödemeler, API).
/// Öde: tutar borçla dolu gelir (kısmi olabilir) + yol; ödeme geri alınır. Rakamlar API'den.
/// </summary>
public class CariScreenTests : TestContext
{
    readonly FakeApi api = new();
    readonly MemoryTokens tokens = new("t-1");
    const string S1 = "55555555-5555-5555-5555-555555555551";
    const string P1 = "88888888-8888-8888-8888-888888888881";
    const string R1 = "11111111-1111-1111-1111-111111111111";

    public CariScreenTests()
    {
        Services.AddSingleton<ITokenStore>(tokens);
        Services.AddSingleton(new TamironApi(new HttpClient(api) { BaseAddress = new Uri("http://api.test/") }, tokens));
        api.Routes["GET /api/receivables"] = (HttpStatusCode.OK,
            "{\"total\":600,\"items\":[{\"recordId\":\"" + R1 + "\",\"vehicleId\":\"22222222-2222-2222-2222-222222222222\",\"plateNo\":\"34 ABC 123\",\"ownerName\":\"Veli Kaya\",\"description\":\"Balata\",\"deliveredAt\":\"2026-10-26\",\"total\":1500,\"paid\":900,\"remaining\":600}]}");
        api.Routes["GET /api/supplier-debts"] = (HttpStatusCode.OK, Borc(1200, odemeli: false));
    }

    NavigationManager Nav => Services.GetRequiredService<NavigationManager>();

    static string Borc(decimal borc, bool odemeli) =>
        $"{{\"total\":{borc},\"items\":[{{\"supplierId\":\"{S1}\",\"name\":\"Öz Parça\",\"phone\":\"0533 100 00 01\",\"debt\":{borc},"
      + $"\"lines\":[{{\"recordId\":\"{R1}\",\"lineId\":\"33333333-3333-3333-3333-333333333331\",\"plateNo\":\"34 ABC 123\",\"name\":\"Balata\",\"cost\":1200}}],"
      + "\"payments\":[" + (odemeli ? $"{{\"id\":\"{P1}\",\"amount\":500,\"method\":\"Cash\",\"paidOn\":\"2026-10-28\"}}" : "") + "]}]}";

    IRenderedComponent<Cari> Ac()
    {
        var c = RenderComponent<Cari>();
        c.WaitForElement("#cariBorc");
        return c;
    }

    [Fact]
    public void Without_session_goes_to_login()
    {
        tokens.Token = null;
        RenderComponent<Cari>();
        Assert.Equal("http://localhost/giris", Nav.Uri);
    }

    [Fact]
    public void Shows_what_customers_owe_and_what_i_owe_suppliers()
    {
        var c = Ac();
        Assert.Contains("600 ₺", c.Find("#cariAlacakToplam").TextContent);
        Assert.Contains("34 ABC 123", c.Find("#cariAlacak").TextContent);
        Assert.Contains("1.200 ₺", c.Find("#cariBorcToplam").TextContent);
        var oz = c.Find($"[data-parcaci-borc='{S1}']");
        Assert.Contains("Öz Parça", oz.TextContent);
        Assert.Contains("34 ABC 123 · Balata · 1.200 ₺", oz.TextContent);
        c.Find($"#cariAlacak [data-kayit='{R1}']").Click();
        Assert.Equal($"http://localhost/kayit/{R1}", Nav.Uri);
    }

    [Fact]
    public void Paying_starts_with_the_whole_debt_and_sends_amount_and_method()
    {
        api.Routes[$"POST /api/suppliers/{S1}/payments"] = (HttpStatusCode.OK, Borc(700, odemeli: true));
        var c = Ac();
        c.Find($"[data-ode='{S1}']").Click();
        Assert.Equal("1.200", c.Find("#cariOdeTutar").GetAttribute("value"));
        c.Find("#cariOdeTutar").Input("500");
        c.Find("[data-ode-yol='Cash']").Click();
        c.WaitForAssertion(() => Assert.Contains("700 ₺", c.Find("#cariBorcToplam").TextContent));
        using var b = JsonDocument.Parse(api.Calls.Single(x => x.Path == $"/api/suppliers/{S1}/payments").Body!);
        Assert.Equal(500m, b.RootElement.GetProperty("amount").GetDecimal());
        Assert.Equal("Cash", b.RootElement.GetProperty("method").GetString());
        Assert.Empty(c.FindAll("#cariOdeTutar"));   // form kapandı
    }

    [Fact]
    public void Over_payment_is_explained_and_the_form_stays()
    {
        api.Routes[$"POST /api/suppliers/{S1}/payments"] = (HttpStatusCode.UnprocessableEntity, "{\"code\":\"supplier_payment.over_debt\",\"title\":\"Borç 1.200 ₺ — fazlası ödenemez.\"}");
        var c = Ac();
        c.Find($"[data-ode='{S1}']").Click();
        c.Find("#cariOdeTutar").Input("5000");
        c.Find("[data-ode-yol='Cash']").Click();
        c.WaitForAssertion(() => Assert.Equal("Borç 1.200 ₺ — fazlası ödenemez.", c.Find("#cariHata").TextContent.Trim()));
        Assert.Equal("5000", c.Find("#cariOdeTutar").GetAttribute("value"));
    }

    [Fact]
    public void Unreadable_amount_is_not_sent()
    {
        var c = Ac();
        c.Find($"[data-ode='{S1}']").Click();
        c.Find("#cariOdeTutar").Input("beş yüz");
        c.Find("[data-ode-yol='Cash']").Click();
        Assert.Contains("Tutarı rakamla yaz", c.Find("#cariHata").TextContent);
        Assert.DoesNotContain(api.Calls, x => x.Path.EndsWith("/payments"));
    }

    [Fact]
    public void A_payment_can_be_undone()
    {
        api.Routes["GET /api/supplier-debts"] = (HttpStatusCode.OK, Borc(700, odemeli: true));
        api.Routes[$"POST /api/supplier-payments/{P1}/undo"] = (HttpStatusCode.OK, Borc(1200, odemeli: false));
        var c = Ac();
        Assert.Contains("28.10.2026 · Nakit · 500 ₺", c.Find($"[data-odeme='{P1}']").TextContent);
        c.Find($"[data-odeme-geri='{P1}']").Click();
        c.WaitForAssertion(() => Assert.Contains("1.200 ₺", c.Find("#cariBorcToplam").TextContent));
        Assert.Empty(c.FindAll($"[data-odeme='{P1}']"));
    }

    [Fact]
    public void Nothing_owed_says_how_debts_come()
    {
        api.Routes["GET /api/supplier-debts"] = (HttpStatusCode.OK, "{\"total\":0,\"items\":[]}");
        var c = Ac();
        Assert.Contains("Parçacıya borcun yok", c.Find("#cariBorc").TextContent);
        Assert.Contains("veresiye", c.Find("#cariBorc").TextContent);
    }
}
