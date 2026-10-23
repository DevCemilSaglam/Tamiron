using System.Net;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Tamirhane.Ui.Pages;
using Tamiron.Ui.Core;

namespace Tamiron.Ui.Tests;

/// <summary>D20 · Giriş ekranı (K-076.19). Ekran API'nin söylediğini gösterir; karar vermez.</summary>
public class LoginScreenTests : TestContext
{
    readonly FakeApi api = new();
    readonly MemoryTokens tokens = new();

    public LoginScreenTests()
    {
        Services.AddSingleton<ITokenStore>(tokens);
        Services.AddSingleton(new TamironApi(new HttpClient(api) { BaseAddress = new Uri("http://api.test/") }, tokens));
    }

    NavigationManager Nav => Services.GetRequiredService<NavigationManager>();

    [Fact]
    public void Login_speaks_when_phone_is_empty_and_sends_nothing()
    {
        var c = RenderComponent<Giris>();
        c.Find("#grKodIste").Click();
        Assert.Equal("Telefonunu yaz.", c.Find("#grHata").TextContent.Trim());
        Assert.Empty(api.Calls);
    }

    [Fact]
    public void Login_two_steps_store_the_session_and_go_to_records()
    {
        api.Routes["POST /api/auth/code"] = (HttpStatusCode.Accepted, "");
        api.Routes["POST /api/auth/verify"] = (HttpStatusCode.OK, "{\"token\":\"t-123\",\"shopName\":\"Usta Oto\",\"userName\":null}");
        var c = RenderComponent<Giris>();
        c.Find("#grTel").Input("0532 000 00 01");
        c.Find("#grKodIste").Click();
        c.WaitForElement("#grKod").Input("123456");
        c.Find("#grGiris").Click();
        c.WaitForAssertion(() => Assert.Equal("http://localhost/", Nav.Uri));
        Assert.Equal("t-123", tokens.Token);   // süresiz oturum cihazda (K-074)
        Assert.Contains("\"phone\":\"0532 000 00 01\"", api.Calls[0].Body);
        Assert.Null(api.Calls[0].Auth);          // giriş uçları anahtarsız
    }

    [Fact]
    public void Api_error_title_is_shown_as_is()
    {
        api.Routes["POST /api/auth/code"] = (HttpStatusCode.UnprocessableEntity, "{\"code\":\"auth.unknown_phone\",\"title\":\"Bu numarayla kayıtlı dükkân yok.\"}");
        var c = RenderComponent<Giris>();
        c.Find("#grTel").Input("0544 999 99 99");
        c.Find("#grKodIste").Click();
        c.WaitForAssertion(() => Assert.Equal("Bu numarayla kayıtlı dükkân yok.", c.Find("#grHata").TextContent.Trim()));
        Assert.Empty(c.FindAll("#grKod"));   // kod adımına GEÇİLMEDİ
    }

    [Fact]
    public void Wrong_code_length_is_said_before_asking_the_api()
    {
        api.Routes["POST /api/auth/code"] = (HttpStatusCode.Accepted, "");
        var c = RenderComponent<Giris>();
        c.Find("#grTel").Input("0532 000 00 01");
        c.Find("#grKodIste").Click();
        c.WaitForElement("#grKod").Input("123");
        c.Find("#grGiris").Click();
        Assert.Equal("Telefonuna gelen 6 haneli kodu yaz.", c.Find("#grHata").TextContent.Trim());
        Assert.Single(api.Calls);
    }

    [Fact]
    public void Records_without_session_go_to_login()
    {
        RenderComponent<Kayitlar>();
        Assert.Equal("http://localhost/giris", Nav.Uri);
        Assert.Empty(api.Calls);
    }
}

/// <summary>D20 · Servis kayıtları ekranı — prototip kayitKartiHtml'in yapısı.</summary>
public class RecordScreenTests : TestContext
{
    readonly FakeApi api = new();
    readonly MemoryTokens tokens = new("t-1");

    public RecordScreenTests()
    {
        Services.AddSingleton<ITokenStore>(tokens);
        Services.AddSingleton(new TamironApi(new HttpClient(api) { BaseAddress = new Uri("http://api.test/") }, tokens));
    }

    const string Liste = """
    {"inShopCount":1,"count":2,"items":[
      {"id":"00000000-0000-0000-0000-000000000001","vehicleId":"00000000-0000-0000-0000-0000000000a1","plateNo":"34 ABC 123","type":"Repair","typeLabel":"Tamirat",
       "complaint":"Triger seti değişecek","openedOn":"2026-10-22","deliveredAt":null,"paymentMethod":null,"waitingForPart":true,"total":0,
       "brand":"Renault","model":"Mégane","ownerName":"Mehmet Kaya"},
      {"id":"00000000-0000-0000-0000-000000000002","vehicleId":"00000000-0000-0000-0000-0000000000a2","plateNo":"35 KLM 45","type":"Maintenance","typeLabel":"Bakım",
       "complaint":"","openedOn":"2026-10-20","deliveredAt":"2026-10-21","paymentMethod":"Cash","waitingForPart":true,"total":2800,
       "brand":"Peugeot","model":"301","ownerName":null}]}
    """;

    [Fact]
    public void Cards_show_vehicle_then_job_like_the_prototype()
    {
        api.Routes["GET /api/records"] = (HttpStatusCode.OK, Liste);
        var c = RenderComponent<Kayitlar>();
        c.WaitForAssertion(() => Assert.Equal(2, c.FindAll(".arac-kart").Count));
        Assert.Equal("Bearer t-1", api.Calls[0].Auth);
        Assert.Equal("2", c.Find("#kytSay").TextContent);

        var acik = c.FindAll(".arac-kart")[0];
        Assert.Contains("icerde", acik.ClassList);
        Assert.Equal("34 ABC 123", acik.QuerySelector(".plate-no")!.TextContent);
        Assert.Equal("Renault Mégane", acik.QuerySelector(".ak-arac")!.TextContent.Trim());
        Assert.Equal("Mehmet Kaya", acik.QuerySelector(".ak-sahip")!.TextContent.Trim());
        Assert.Equal("Serviste", acik.QuerySelector(".ak-durum")!.TextContent.Trim());
        Assert.NotNull(acik.QuerySelector(".pill.warn"));                       // parça bekliyor
        Assert.NotNull(acik.QuerySelector(".pill.info"));                       // tamirat: info
        Assert.Equal(("22.10.2026", "—"), (acik.QuerySelector(".tl-date")!.TextContent, acik.QuerySelector(".ak-tutar")!.TextContent));

        var teslim = c.FindAll(".arac-kart")[1];
        Assert.DoesNotContain("icerde", teslim.ClassList);
        Assert.Equal("Teslim edildi", teslim.QuerySelector(".ak-durum")!.TextContent.Trim());
        Assert.Null(teslim.QuerySelector(".pill.warn"));                       // teslimde "parça bekliyor" YOK
        Assert.Null(teslim.QuerySelector(".ak-sahip"));
        Assert.NotNull(teslim.QuerySelector(".ak-is > .pill.accent"));         // bakım: accent
        Assert.Equal("ne yapılacağı yazılmamış", teslim.QuerySelector(".ak-is-ne")!.TextContent);
        Assert.Equal(("21.10.2026", "2.800 ₺"), (teslim.QuerySelector(".tl-date")!.TextContent, teslim.QuerySelector(".ak-tutar")!.TextContent));
    }

    [Fact]
    public void Dropped_session_clears_the_key_and_goes_to_login()
    {
        api.Routes["GET /api/records"] = (HttpStatusCode.Unauthorized, "{\"title\":\"Oturum açık değil.\"}");
        RenderComponent<Kayitlar>();
        Assert.Equal("http://localhost/giris", Services.GetRequiredService<NavigationManager>().Uri);
        Assert.Null(tokens.Token);
    }

    [Fact]
    public void Empty_list_says_what_to_do()
    {
        api.Routes["GET /api/records"] = (HttpStatusCode.OK, "{\"inShopCount\":0,\"count\":0,\"items\":[]}");
        var c = RenderComponent<Kayitlar>();
        c.WaitForAssertion(() => Assert.Contains("Henüz servis kaydı yok", c.Find(".bos-durum").TextContent));
    }
}
