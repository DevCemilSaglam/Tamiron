using System.Net;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Tamirhane.Ui.Pages;
using Tamiron.Ui.Core;

namespace Tamiron.Ui.Tests;

/// <summary>
/// G3 · Servis kaydı (EKRANLAR §3 "kalp", prototip 30-kayit, K-058 tek sayfa). Her değişiklik API'ye
/// gider ve ekran API'nin döndürdüğü kaydı yeniden çizer — toplam, eksikler, durum sunucudan (karar API'de).
/// </summary>
public class RecordDetailScreenTests : TestContext
{
    readonly FakeApi api = new();
    readonly MemoryTokens tokens = new("t-1");
    const string Id = "11111111-1111-1111-1111-111111111111";
    const string Vid = "22222222-2222-2222-2222-222222222222";
    const string L1 = "aaaaaaaa-0000-0000-0000-000000000001";
    const string L2 = "aaaaaaaa-0000-0000-0000-000000000002";

    public RecordDetailScreenTests()
    {
        Services.AddSingleton<ITokenStore>(tokens);
        Services.AddSingleton(new TamironApi(new HttpClient(api) { BaseAddress = new Uri("http://api.test/") }, tokens));
        api.Routes[$"GET /api/vehicles/{Vid}/advance"] = (HttpStatusCode.OK, "{\"balance\":0,\"items\":[]}");   // G8: avans yok
        api.Routes[$"GET /api/vehicles/{Vid}"] = (HttpStatusCode.OK,
            $"{{\"id\":\"{Vid}\",\"plateNo\":\"06 TMR 07\",\"brand\":\"Fiat\",\"model\":\"Egea\",\"ownerName\":\"Veli Kaya\",\"ownerPhone\":\"0532 111 22 33\",\"hasPartIdentity\":true}}");
    }

    NavigationManager Nav => Services.GetRequiredService<NavigationManager>();

    /// <summary>Gövdede Türkçe harfler \u kaçışıyla gider ("İ" → İ) — karşılaştırmadan önce çözülür.</summary>
    static string Json(string body) => System.Text.RegularExpressions.Regex.Unescape(body);

    static string Kayit(string lines = "[]", decimal total = 0, string delivered = "null", bool waiting = false,
                        string type = "Repair", string warnings = "[]", string complaint = "Fren sesi")
        => $"{{\"id\":\"{Id}\",\"vehicleId\":\"{Vid}\",\"plateNo\":\"06 TMR 07\",\"type\":\"{type}\",\"complaint\":\"{complaint}\","
         + $"\"deliveredAt\":{delivered},\"waitingForPart\":{(waiting ? "true" : "false")},\"showWaitingToCustomer\":false,"
         + $"\"total\":{total},\"lines\":{lines},\"warnings\":{warnings},\"paid\":0,\"remaining\":0,\"payments\":[]}}";

    static string Satir(string id, string kind, string name, decimal price, string supplied = "null", string cost = "null")
        => $"{{\"id\":\"{id}\",\"kind\":\"{kind}\",\"name\":\"{name}\",\"price\":{price},\"cost\":{cost},\"suppliedBy\":{supplied},\"countsInTotal\":{(supplied == "\"Customer\"" ? "false" : "true")}}}";

    static readonly string IkiSatir = "[" + Satir(L1, "Part", "Balata", 1200, cost: "900") + "," + Satir(L2, "Labor", "İşçilik", 800) + "]";

    IRenderedComponent<Kayit> Ac(string kayit)
    {
        api.Routes[$"GET /api/records/{Id}"] = (HttpStatusCode.OK, kayit);
        return RenderComponent<Kayit>(p => p.Add(x => x.Id, Guid.Parse(Id)));
    }

    [Fact]
    public void Without_session_goes_to_login()
    {
        tokens.Token = null;
        RenderComponent<Kayit>(p => p.Add(x => x.Id, Guid.Parse(Id)));
        Assert.Equal("http://localhost/giris", Nav.Uri);
    }

    [Fact]
    public void Shows_vehicle_owner_with_callable_phone_and_the_lines_with_total()
    {
        var c = Ac(Kayit(IkiSatir, 2000));
        c.WaitForAssertion(() => Assert.Contains("Fiat Egea", c.Find("#kayitKunye").TextContent));
        Assert.Contains("Veli Kaya", c.Find("#kayitMusteri").TextContent);
        Assert.Equal("tel:05321112233", c.Find("#kayitMusteri a").GetAttribute("href"));
        Assert.Equal(2, c.FindAll("#kSatirlar tr[data-satir]").Count);
        Assert.Equal("2.000 ₺", c.Find("#kToplam").TextContent.Trim());
        Assert.Contains("parçacı 900 ₺", c.Find("#kSatirlar").TextContent);   // alış soluk, müşteriye değil
    }

    const string AlisAcik = "{\"name\":\"Deneme Oto\",\"maintenanceMonths\":6,\"maintenanceKm\":10000,\"askPartCost\":true}";

    // H4 · Veresiye (K-090): alış alanının yanında; parça kimden alındıysa adı yazılı. Varsayılan "ödedim".
    const string S1 = "55555555-5555-5555-5555-555555555551";
    const string Parcacilar = "[{\"id\":\"" + S1 + "\",\"name\":\"Öz Parça\",\"phone\":\"0533 100 00 01\"}]";

    static string ParcaciliSatir(bool veresiye, string? parcaci = S1)
    {
        var s = Satir(L1, "Part", "Balata", 1200, "\"Shop\"", cost: "900");
        return "[" + s[..^1] + $",\"supplierId\":{(parcaci is null ? "null" : $"\"{parcaci}\"")},\"onCredit\":{(veresiye ? "true" : "false")}}}" + "]";
    }

    [Fact]
    public void Credit_chip_marks_the_known_supplier_and_says_who()
    {
        api.Routes["GET /api/shop"] = (HttpStatusCode.OK, AlisAcik);
        api.Routes["GET /api/suppliers"] = (HttpStatusCode.OK, Parcacilar);
        api.Routes[$"PATCH /api/records/{Id}/lines/{L1}/purchase"] = (HttpStatusCode.OK, Kayit(ParcaciliSatir(true), 1200));
        var c = Ac(Kayit(ParcaciliSatir(false), 1200));
        c.WaitForAssertion(() => Assert.Contains("Öz Parça", c.Find($"[data-satir-parcaci='{L1}']").TextContent));
        Assert.Equal("false", c.Find($"[data-satir-veresiye='{L1}']").GetAttribute("aria-pressed"));
        c.Find($"[data-satir-veresiye='{L1}']").Click();
        c.WaitForAssertion(() => Assert.Equal("true", c.Find($"[data-satir-veresiye='{L1}']").GetAttribute("aria-pressed")));
        using var b = System.Text.Json.JsonDocument.Parse(api.Calls.Single(x => x.Path == $"/api/records/{Id}/lines/{L1}/purchase").Body!);
        Assert.Equal(S1, b.RootElement.GetProperty("supplierId").GetString());
        Assert.True(b.RootElement.GetProperty("onCredit").GetBoolean());
    }

    [Fact]
    public void Credit_without_a_known_supplier_asks_whom_first()
    {
        api.Routes["GET /api/shop"] = (HttpStatusCode.OK, AlisAcik);
        api.Routes["GET /api/suppliers"] = (HttpStatusCode.OK, Parcacilar);
        api.Routes[$"PATCH /api/records/{Id}/lines/{L1}/purchase"] = (HttpStatusCode.OK, Kayit(ParcaciliSatir(true), 1200));
        var c = Ac(Kayit(ParcaciliSatir(false, parcaci: null), 1200));
        c.WaitForElement($"[data-satir-veresiye='{L1}']").Click();
        Assert.DoesNotContain(api.Calls, x => x.Path.EndsWith("/purchase"));   // kime borçlu olduğu bilinmeden borç yazılmaz
        c.WaitForElement($"[data-satir-kimden='{S1}']").Click();
        c.WaitForAssertion(() => Assert.Contains(api.Calls, x => x.Path == $"/api/records/{Id}/lines/{L1}/purchase"));
        Assert.Contains($"\"supplierId\":\"{S1}\"", api.Calls.Single(x => x.Path.EndsWith("/purchase")).Body);
        Assert.Contains("\"onCredit\":true", api.Calls.Single(x => x.Path.EndsWith("/purchase")).Body);
    }

    [Fact]
    public void Credit_can_be_taken_back()
    {
        // Mutasyonla bulundu: ikinci dokunuş veresiyeyi kaldırmıyordu — yanlış dokunan usta borcu silemezdi.
        api.Routes["GET /api/shop"] = (HttpStatusCode.OK, AlisAcik);
        api.Routes["GET /api/suppliers"] = (HttpStatusCode.OK, Parcacilar);
        api.Routes[$"PATCH /api/records/{Id}/lines/{L1}/purchase"] = (HttpStatusCode.OK, Kayit(ParcaciliSatir(false), 1200));
        var c = Ac(Kayit(ParcaciliSatir(true), 1200));
        c.WaitForElement($"[data-satir-veresiye='{L1}']").Click();
        c.WaitForAssertion(() => Assert.Contains("\"onCredit\":false", api.Calls.Single(x => x.Path.EndsWith("/purchase")).Body));
    }

    [Fact]
    public void No_credit_chip_when_cost_is_not_asked()
    {
        api.Routes["GET /api/shop"] = (HttpStatusCode.OK, AlisAcik.Replace("true", "false"));
        var c = Ac(Kayit(ParcaciliSatir(false), 1200));
        c.WaitForElement($"[data-satir='{L1}']");
        Assert.Empty(c.FindAll("[data-satir-veresiye]"));
    }

    [Fact]
    public void Part_cost_is_asked_only_when_the_shop_wants_it()
    {
        // G11 · Ayarlardaki "Parça alış fiyatı: Sorulsun" (A-M43): yalnız tamircinin aldığı parçada, boş bırakılabilir.
        api.Routes["GET /api/shop"] = (HttpStatusCode.OK, AlisAcik);
        var lines = "[" + Satir(L1, "Part", "Balata", 1200, "\"Shop\"", cost: "900") + "," + Satir(L2, "Part", "Yağ", 0, "\"Customer\"") + "]";
        var c = Ac(Kayit(lines, 1200));
        c.WaitForAssertion(() => Assert.Equal("900", c.Find($"[data-satir-alis='{L1}']").GetAttribute("value")));
        Assert.Empty(c.FindAll($"[data-satir-alis='{L2}']"));   // müşterinin parçasının alışı sorulmaz
    }

    [Fact]
    public void Written_cost_is_sent_and_empty_clears_it()
    {
        api.Routes["GET /api/shop"] = (HttpStatusCode.OK, AlisAcik);
        api.Routes[$"PUT /api/records/{Id}/lines/{L1}"] = (HttpStatusCode.OK, Kayit(IkiSatir, 2000));
        var c = Ac(Kayit(IkiSatir, 2000));
        c.WaitForElement($"[data-satir-alis='{L1}']").Change("1.050");
        c.WaitForAssertion(() => Assert.Contains("\"cost\":1050", api.Calls.Last(x => x.Path == $"/api/records/{Id}/lines/{L1}").Body));
        c.Find($"[data-satir-alis='{L1}']").Change("");
        c.WaitForAssertion(() => Assert.Contains("\"cost\":null", api.Calls.Last(x => x.Path == $"/api/records/{Id}/lines/{L1}").Body));
    }

    [Fact]
    public void Without_the_setting_cost_is_only_read()
    {
        api.Routes["GET /api/shop"] = (HttpStatusCode.OK, AlisAcik.Replace("true", "false"));
        var c = Ac(Kayit(IkiSatir, 2000));
        c.WaitForAssertion(() => Assert.Contains("parçacı 900 ₺", c.Find($"[data-satir='{L1}']").TextContent));
        Assert.Empty(c.FindAll("[data-satir-alis]"));
    }

    [Fact]
    public void Part_question_is_offered_while_in_shop_only()
    {
        // G10 · Parça fiyatı sor kaydın içinden açılır; teslim edilmiş işe parça sorulmaz.
        var c = Ac(Kayit(IkiSatir, 2000));
        c.WaitForElement("#kParcaSor").Click();
        Assert.Equal($"http://localhost/kayit/{Id}/parca-sor", Nav.Uri);
        var teslim = Ac(Kayit(IkiSatir, 2000, delivered: "\"2026-10-20\""));
        teslim.WaitForElement("#kayitKunye");
        Assert.Empty(teslim.FindAll("#kParcaSor"));
    }

    [Fact]
    public void Empty_record_says_what_to_do()
    {
        var c = Ac(Kayit());
        c.WaitForAssertion(() => Assert.Contains("Henüz bir şey yazılmadı", c.Find("#kSatirlar").TextContent));
    }

    [Fact]
    public void Customer_supplied_part_shows_no_price_and_can_be_taken_back()
    {
        var lines = "[" + Satir(L1, "Part", "Yağ", 0, "\"Customer\"") + "]";
        api.Routes[$"PUT /api/records/{Id}/lines/{L1}"] = (HttpStatusCode.OK, Kayit("[" + Satir(L1, "Part", "Yağ", 0, "\"Shop\"") + "]"));
        var c = Ac(Kayit(lines));
        c.WaitForElement("[data-getiren]");
        Assert.Equal("—", c.Find("tr[data-satir] td.sayi").TextContent.Trim());
        c.Find("[data-getiren]").Click();
        c.WaitForAssertion(() => Assert.Contains("\"suppliedBy\":\"Shop\"", api.Calls.Last().Body));
    }

    [Fact]
    public void Adding_a_part_posts_it_and_redraws_from_the_answer()
    {
        api.Routes[$"POST /api/records/{Id}/lines"] = (HttpStatusCode.OK, Kayit("[" + Satir(L1, "Part", "Balata", 1200) + "]", 1200));
        var c = Ac(Kayit());
        c.WaitForElement("#kParcaEkle").Click();
        c.Find("#ekTurParca").Click();
        c.Find("#ekAd").Input("Balata");
        c.Find("#ekTutar").Input("1.200");
        c.Find("#ekKaydet").Click();
        c.WaitForAssertion(() => Assert.Equal("1.200 ₺", c.Find("#kToplam").TextContent.Trim()));
        var body = api.Calls.Single(x => x.Path == $"/api/records/{Id}/lines").Body!;
        Assert.Contains("\"kind\":\"Part\"", body);
        Assert.Contains("\"name\":\"Balata\"", body);
        Assert.Contains("\"price\":1200", body);
        Assert.Empty(c.FindAll("#ekPanel"));   // panel kapandı
    }

    [Fact]
    public void Add_panel_speaks_when_name_is_missing()
    {
        var c = Ac(Kayit());
        c.WaitForElement("#kParcaEkle").Click();
        c.Find("#ekKaydet").Click();
        Assert.Equal("Ne yapıldığını ya da parçanın adını yaz.", c.Find("#ekHata").TextContent.Trim());
        Assert.DoesNotContain(api.Calls, x => x.Path.EndsWith("/lines"));
    }

    [Fact]
    public void Editing_a_price_sends_the_whole_line_with_the_new_price()
    {
        api.Routes[$"PUT /api/records/{Id}/lines/{L2}"] = (HttpStatusCode.OK, Kayit(IkiSatir.Replace("\"price\":800", "\"price\":950"), 2150));
        var c = Ac(Kayit(IkiSatir, 2000));
        c.WaitForElement($"[data-satir-fiyat='{L2}']").Change("950");
        c.WaitForAssertion(() => Assert.Equal("2.150 ₺", c.Find("#kToplam").TextContent.Trim()));
        var body = Json(api.Calls.Single(x => x.Path == $"/api/records/{Id}/lines/{L2}").Body!);
        Assert.Contains("\"kind\":\"Labor\"", body);
        Assert.Contains("\"name\":\"İşçilik\"", body);
        Assert.Contains("\"price\":950", body);
    }

    [Fact]
    public void Kind_dot_flips_the_kind_and_keeps_name_and_price()
    {
        api.Routes[$"PUT /api/records/{Id}/lines/{L1}"] = (HttpStatusCode.OK, Kayit(IkiSatir, 2000));
        var c = Ac(Kayit(IkiSatir, 2000));
        c.WaitForElement($"[data-satir-tip='{L1}']").Click();
        c.WaitForAssertion(() => Assert.Contains(api.Calls, x => x.Path == $"/api/records/{Id}/lines/{L1}"));
        var body = api.Calls.Single(x => x.Path == $"/api/records/{Id}/lines/{L1}").Body!;
        Assert.Contains("\"kind\":\"Labor\"", body);
        Assert.Contains("\"name\":\"Balata\"", body);
        Assert.Contains("\"price\":1200", body);
    }

    [Fact]
    public void Deleting_a_line_can_be_undone()
    {
        var biri = "[" + Satir(L2, "Labor", "İşçilik", 800) + "]";
        api.Routes[$"DELETE /api/records/{Id}/lines/{L1}"] = (HttpStatusCode.OK, Kayit(biri, 800));
        api.Routes[$"POST /api/records/{Id}/lines/{L1}/restore"] = (HttpStatusCode.OK, Kayit(IkiSatir, 2000));
        var c = Ac(Kayit(IkiSatir, 2000));
        c.WaitForElement($"[data-satir-sil='{L1}']").Click();
        c.WaitForAssertion(() => Assert.Contains("Satır silindi", c.Find("#kGeriAl").TextContent));
        Assert.Single(c.FindAll("#kSatirlar tr[data-satir]"));
        c.Find("#kGeriAlBtn").Click();
        c.WaitForAssertion(() => Assert.Equal(2, c.FindAll("#kSatirlar tr[data-satir]").Count));
        Assert.Empty(c.FindAll("#kGeriAl"));
    }

    [Fact]
    public void Waiting_for_part_is_a_chip_and_offers_showing_it_to_the_customer()
    {
        api.Routes[$"PATCH /api/records/{Id}"] = (HttpStatusCode.OK, Kayit(waiting: true));
        var c = Ac(Kayit());
        c.WaitForElement("#kBekliyor").Click();
        c.WaitForAssertion(() => Assert.Equal("true", c.Find("#kBekliyor").GetAttribute("aria-pressed")));
        Assert.Contains("\"waitingForPart\":true", api.Calls.Last().Body);
        Assert.NotNull(c.Find("#kBekliyorMusteriye"));
    }

    [Fact]
    public void Complaint_and_type_are_saved_through_the_api()
    {
        api.Routes[$"PATCH /api/records/{Id}"] = (HttpStatusCode.OK, Kayit(type: "Maintenance", complaint: "Yağ"));
        var c = Ac(Kayit());
        c.WaitForElement("#kSikayet").Change("Yağ");
        c.WaitForAssertion(() => Assert.Contains("\"complaint\":\"Yağ\"", Json(api.Calls.Last().Body!)));
        c.Find("[data-kayit-tur='Maintenance']").Click();
        c.WaitForAssertion(() => Assert.Contains("\"type\":\"Maintenance\"", api.Calls.Last().Body));
        Assert.Equal("true", c.Find("[data-kayit-tur='Maintenance']").GetAttribute("aria-pressed"));
    }

    [Fact]
    public void Deliver_goes_to_the_delivery_screen_and_warnings_do_not_block()
    {
        var c = Ac(Kayit(IkiSatir, 2000, warnings: "[\"km\"]"));
        c.WaitForAssertion(() => Assert.Contains("Eksik görünen: km. Yine de teslim edebilirsin.", c.Find("#kTeslim").TextContent));
        c.Find("#kTeslimEt").Click();
        Assert.Equal($"http://localhost/teslim/{Id}", Nav.Uri);
    }

    [Fact]
    public void Delivered_record_says_so_instead_of_offering_delivery()
    {
        var c = Ac(Kayit(IkiSatir, 2000, delivered: "\"2026-10-20\""));
        c.WaitForAssertion(() => Assert.Contains("Teslim edildi · 20.10.2026", c.Find("#kTeslim").TextContent));
        Assert.Empty(c.FindAll("#kTeslimEt"));
    }

    [Fact]
    public void Missing_record_is_said_plainly()
    {
        api.Routes[$"GET /api/records/{Id}"] = (HttpStatusCode.NotFound, "{\"title\":\"Servis kaydı bulunamadı.\"}");
        var c = RenderComponent<Kayit>(p => p.Add(x => x.Id, Guid.Parse(Id)));
        c.WaitForAssertion(() => Assert.Equal("Servis kaydı bulunamadı.", c.Find("#kHata").TextContent.Trim()));
    }
}
