using System.Net;
using System.Text.Json;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Tamirhane.Ui.Pages;
using Tamiron.Ui.Core;

namespace Tamiron.Ui.Tests;

/// <summary>
/// G10b · Parça fiyatı sor (prototip 40-parca-sor). Ne sorulacak, araç kimliği tamam mı (K-068), gidecek
/// metin — hepsi API'nin önizlemesinden; ekran karar vermez, düğme her zaman basılır ve API eksiği söyler.
/// Her parçacıya AYRI soru (Kol B); WhatsApp ustanın telefonundan açılır (gönderim yok). Cevap gelince usta
/// fiyatı kendi yazar — uygulama "cevaplar kayda düşecek" diye vaat etmez; yazılan fiyat ALIŞ olur (K-076.2).
/// </summary>
public class PartQuestionScreenTests : TestContext
{
    readonly FakeApi api = new();
    readonly MemoryTokens tokens = new("t-1");
    const string Rid = "11111111-1111-1111-1111-111111111111";
    const string Vid = "22222222-2222-2222-2222-222222222222";
    const string L1 = "33333333-3333-3333-3333-333333333331";
    const string L2 = "33333333-3333-3333-3333-333333333332";
    const string S1 = "55555555-5555-5555-5555-555555555551";
    const string S2 = "55555555-5555-5555-5555-555555555552";
    const string Q1 = "66666666-6666-6666-6666-666666666661";
    const string Q2 = "66666666-6666-6666-6666-666666666662";
    const string Metin = "Fiat Egea 2021 · 1.4 Fire\n34 PS 010\n\n· Balata\n· Disk\n\nFiyat verebilir misiniz?";

    public PartQuestionScreenTests()
    {
        Services.AddSingleton<ITokenStore>(tokens);
        Services.AddSingleton(new TamironApi(new HttpClient(api) { BaseAddress = new Uri("http://api.test/") }, tokens));
        JSInterop.SetupVoid("open", _ => true).SetVoidResult();
        api.Routes[$"GET /api/records/{Rid}"] = (HttpStatusCode.OK,
            $"{{\"id\":\"{Rid}\",\"vehicleId\":\"{Vid}\",\"plateNo\":\"34 PS 010\",\"type\":\"Repair\",\"complaint\":\"Fren\",\"deliveredAt\":null,\"waitingForPart\":false,\"showWaitingToCustomer\":false,"
          + "\"total\":0,\"lines\":[],\"warnings\":[],\"paid\":0,\"remaining\":0}");
        api.Routes[$"GET /api/records/{Rid}/price-question"] = (HttpStatusCode.OK, Onizleme(kimlik: true));
        api.Routes["GET /api/suppliers"] = (HttpStatusCode.OK,
            $"[{{\"id\":\"{S1}\",\"name\":\"Öz Parça\",\"phone\":\"0533 100 00 01\"}},{{\"id\":\"{S2}\",\"name\":\"Can Oto\",\"phone\":\"0533 100 00 02\"}}]");
        api.Routes[$"GET /api/records/{Rid}/price-requests"] = (HttpStatusCode.OK, "[]");
    }

    NavigationManager Nav => Services.GetRequiredService<NavigationManager>();

    static string Onizleme(bool kimlik, bool parcaVar = true) =>
        "{\"lines\":[" + (parcaVar ? $"{{\"id\":\"{L1}\",\"name\":\"Balata\",\"selectedReplyId\":null}},{{\"id\":\"{L2}\",\"name\":\"Disk\",\"selectedReplyId\":null}}" : "")
        + $"],\"hasPartIdentity\":{(kimlik ? "true" : "false")},\"missing\":{(kimlik ? "[]" : "[\"yıl\",\"motor\"]")},\"text\":{JsonSerializer.Serialize(Metin)}}}";

    static string Soru(string id, string sid, string ad, string tel, string cevaplar = "") =>
        $"{{\"id\":\"{id}\",\"supplierId\":\"{sid}\",\"supplierName\":\"{ad}\",\"supplierPhone\":\"{tel}\",\"sentText\":{JsonSerializer.Serialize(Metin)},\"sentAt\":\"2026-10-27T10:00:00Z\","
        + $"\"lines\":[{{\"id\":\"{L1}\",\"name\":\"Balata\",\"selectedReplyId\":null}},{{\"id\":\"{L2}\",\"name\":\"Disk\",\"selectedReplyId\":null}}],\"replies\":[{cevaplar}]}}";

    IRenderedComponent<ParcaSor> Ac()
    {
        var c = RenderComponent<ParcaSor>(p => p.Add(x => x.Id, Guid.Parse(Rid)));
        c.WaitForElement("#psMesaj");
        return c;
    }

    [Fact]
    public void Without_session_goes_to_login()
    {
        tokens.Token = null;
        RenderComponent<ParcaSor>(p => p.Add(x => x.Id, Guid.Parse(Rid)));
        Assert.Equal("http://localhost/giris", Nav.Uri);
    }

    [Fact]
    public void Shows_what_will_be_asked_and_the_exact_text()
    {
        var c = Ac();
        Assert.Equal(new[] { "Balata", "Disk" }, c.FindAll("#psParcalar [data-parca]").Select(x => x.TextContent.Trim()));
        Assert.Equal(Metin, c.Find("#psMesaj").TextContent);
        Assert.Empty(c.FindAll("#psAracKart"));   // kimlik tamam
    }

    [Fact]
    public void Missing_identity_is_asked_on_the_screen_and_saved_to_the_vehicle()
    {
        api.Routes[$"GET /api/records/{Rid}/price-question"] = (HttpStatusCode.OK, Onizleme(kimlik: false));
        api.Routes[$"GET /api/vehicles/{Vid}"] = (HttpStatusCode.OK, $"{{\"id\":\"{Vid}\",\"plateNo\":\"34 PS 010\",\"brand\":\"Fiat\",\"model\":\"Egea\",\"year\":null,\"engine\":null}}");
        api.Routes[$"PATCH /api/vehicles/{Vid}"] = (HttpStatusCode.OK, $"{{\"id\":\"{Vid}\",\"plateNo\":\"34 PS 010\",\"brand\":\"Fiat\",\"model\":\"Egea\",\"year\":\"2021\",\"engine\":\"1.4 Fire\"}}");
        var c = Ac();
        Assert.Contains("Eksik: yıl, motor", c.Find("#psAracKart").TextContent);
        Assert.Equal("Fiat", c.Find("#psMarka").GetAttribute("value"));
        c.Find("#psYil").Input("2021");
        c.Find("#psMotor").Input("1.4 Fire");
        api.Routes[$"GET /api/records/{Rid}/price-question"] = (HttpStatusCode.OK, Onizleme(kimlik: true));
        c.Find("#psAracKaydet").Click();
        c.WaitForAssertion(() => Assert.Empty(c.FindAll("#psAracKart")));
        using var b = JsonDocument.Parse(api.Calls.Single(x => x.Path == $"/api/vehicles/{Vid}" && x.Body is not null).Body!);
        Assert.Equal("2021", b.RootElement.GetProperty("year").GetString());
        Assert.Equal("1.4 Fire", b.RootElement.GetProperty("engine").GetString());
    }

    [Fact]
    public void Nothing_to_ask_says_so_and_offers_the_record()
    {
        api.Routes[$"GET /api/records/{Rid}/price-question"] = (HttpStatusCode.OK, Onizleme(kimlik: true, parcaVar: false));
        var c = Ac();
        Assert.Contains("Sorulacak parça yok", c.Find("#psParcalar").TextContent);
        c.Find("#psKayda").Click();
        Assert.Equal($"http://localhost/kayit/{Rid}", Nav.Uri);
    }

    [Fact]
    public void No_suppliers_leads_to_adding_one_and_back()
    {
        api.Routes["GET /api/suppliers"] = (HttpStatusCode.OK, "[]");
        var c = Ac();
        c.Find("#psParcaciEkle").Click();
        Assert.Equal($"http://localhost/parcacilar?geri=%2Fkayit%2F{Rid}%2Fparca-sor", Nav.Uri);
    }

    [Fact]
    public void Pressing_without_a_supplier_shows_the_api_reason()
    {
        api.Routes[$"POST /api/records/{Rid}/price-requests"] = (HttpStatusCode.UnprocessableEntity, "{\"code\":\"price_request.no_supplier\",\"title\":\"Önce kime soracağını seç.\"}");
        var c = Ac();
        c.Find("#psGonder").Click();
        c.WaitForAssertion(() => Assert.Equal("Önce kime soracağını seç.", c.Find("#psHata").TextContent.Trim()));
        // Tarayıcıda bulundu: bildirim ekranın başındaydı, telefonda dipteki düğmeye basan görmüyordu.
        Assert.Equal("psHata", c.Find("#psGonder").PreviousElementSibling!.Id);
    }

    [Fact]
    public void Asking_one_sends_the_choice_and_opens_whatsapp_with_the_text()
    {
        api.Routes[$"POST /api/records/{Rid}/price-requests"] = (HttpStatusCode.OK, "[" + Soru(Q1, S1, "Öz Parça", "0533 100 00 01") + "]");
        var c = Ac();
        c.Find($"[data-parcaci='{S1}']").Click();
        Assert.Equal("true", c.Find($"[data-parcaci='{S1}']").GetAttribute("aria-pressed"));
        api.Routes[$"GET /api/records/{Rid}/price-requests"] = (HttpStatusCode.OK, "[" + Soru(Q1, S1, "Öz Parça", "0533 100 00 01") + "]");
        c.Find("#psGonder").Click();
        c.WaitForAssertion(() => Assert.Contains("Öz Parça — WhatsApp açılıyor, mesaj hazır.", c.Find("#psSonuc").TextContent));
        using var b = JsonDocument.Parse(api.Calls.Single(x => x.Path == $"/api/records/{Rid}/price-requests" && x.Body is not null).Body!);
        Assert.Equal(new[] { S1 }, b.RootElement.GetProperty("supplierIds").EnumerateArray().Select(e => e.GetString()));
        var open = Assert.Single(JSInterop.Invocations, i => i.Identifier == "open");
        Assert.Equal(Format.WhatsAppLink("0533 100 00 01", Metin), open.Arguments[0]);
    }

    [Fact]
    public void Asking_two_opens_the_first_and_offers_the_second()
    {
        // Tarayıcı tek dokunuşta yalnız BİR pencere açtırır; ikincisi kendi düğmesiyle (aynı soru, kendi kaydı).
        var iki = "[" + Soru(Q1, S1, "Öz Parça", "0533 100 00 01") + "," + Soru(Q2, S2, "Can Oto", "0533 100 00 02") + "]";
        api.Routes[$"POST /api/records/{Rid}/price-requests"] = (HttpStatusCode.OK, iki);
        var c = Ac();
        c.Find($"[data-parcaci='{S1}']").Click();
        c.Find($"[data-parcaci='{S2}']").Click();
        api.Routes[$"GET /api/records/{Rid}/price-requests"] = (HttpStatusCode.OK, iki);
        c.Find("#psGonder").Click();
        c.WaitForAssertion(() => Assert.Contains("2 parçacıya soruldu.", c.Find("#psSonuc").TextContent));
        Assert.Single(JSInterop.Invocations, i => i.Identifier == "open");
        c.Find($"[data-ac='{Q2}']").Click();
        Assert.Equal(Format.WhatsAppLink("0533 100 00 02", Metin), JSInterop.Invocations.Last(i => i.Identifier == "open").Arguments[0]);
        Assert.Equal(1, api.Calls.Count(x => x.Path == $"/api/records/{Rid}/price-requests" && x.Body is not null));   // yeniden sormaz
    }

    [Fact]
    public void Writing_a_reply_sends_the_price_as_cost_for_that_line()
    {
        api.Routes[$"GET /api/records/{Rid}/price-requests"] = (HttpStatusCode.OK, "[" + Soru(Q1, S1, "Öz Parça", "0533 100 00 01") + "]");
        api.Routes[$"POST /api/price-requests/{Q1}/replies"] = (HttpStatusCode.OK, api.Routes[$"GET /api/records/{Rid}"].Body);
        var c = Ac();
        var alis = c.Find($"[data-cevap-fiyat='{Q1}|{L1}']");
        Assert.Contains("Balata", c.Find($"label[for='{alis.Id}']").TextContent);   // etiket girdiye bağlı (tarayıcıda bulundu)
        alis.Input("1.200");
        c.Find($"[data-cevap-musteri='{Q1}|{L1}']").Input("1.500");
        c.Find($"[data-cevap-yaz='{Q1}|{L1}']").Click();
        c.WaitForAssertion(() => Assert.Contains(api.Calls, x => x.Path == $"/api/price-requests/{Q1}/replies"));
        using var b = JsonDocument.Parse(api.Calls.Single(x => x.Path == $"/api/price-requests/{Q1}/replies").Body!);
        Assert.Equal(L1, b.RootElement.GetProperty("lineId").GetString());
        Assert.Equal(1200m, b.RootElement.GetProperty("price").GetDecimal());
        Assert.Equal(1500m, b.RootElement.GetProperty("customerPrice").GetDecimal());
    }

    [Fact]
    public void Unreadable_amount_is_told_and_nothing_is_sent()
    {
        api.Routes[$"GET /api/records/{Rid}/price-requests"] = (HttpStatusCode.OK, "[" + Soru(Q1, S1, "Öz Parça", "0533 100 00 01") + "]");
        var c = Ac();
        c.Find($"[data-cevap-fiyat='{Q1}|{L1}']").Input("bin iki");
        c.Find($"[data-cevap-yaz='{Q1}|{L1}']").Click();
        Assert.Contains("Tutarı rakamla yaz", c.Find("#psHata").TextContent);
        Assert.DoesNotContain(api.Calls, x => x.Path.Contains("/replies"));
    }

    [Fact]
    public void Earlier_replies_show_and_the_used_one_is_marked()
    {
        var cevap = "{\"id\":\"77777777-7777-7777-7777-777777777771\",\"rawText\":\"1200\",\"parsedPrice\":1200,\"parsedBrand\":null,\"parsedCode\":null,\"receivedAt\":\"2026-10-27T11:00:00Z\"}";
        var soru = Soru(Q1, S1, "Öz Parça", "0533 100 00 01", cevap).Replace(
            $"{{\"id\":\"{L1}\",\"name\":\"Balata\",\"selectedReplyId\":null}}", $"{{\"id\":\"{L1}\",\"name\":\"Balata\",\"selectedReplyId\":\"77777777-7777-7777-7777-777777777771\"}}");
        api.Routes[$"GET /api/records/{Rid}/price-requests"] = (HttpStatusCode.OK, "[" + soru + "]");
        var c = Ac();
        Assert.Contains("1.200 ₺", c.Find($"[data-soru='{Q1}']").TextContent);
        Assert.Contains("kayıtta bu", c.Find($"[data-soru='{Q1}']").TextContent);
    }
}
