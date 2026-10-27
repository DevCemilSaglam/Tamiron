using System.Net;
using System.Text.Json;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Tamirhane.Ui.Pages;
using Tamiron.Ui.Core;

namespace Tamiron.Ui.Tests;

/// <summary>
/// G10a · Parçacılar (VERI-MODELI §4: ad + telefon, iki alan). Ekle · düzenle · sil (soft — geçmiş sorular
/// kime gittiğini kaybetmez). Numara dükkânda tekil: API reddeder, ekran nedenini gösterir ve yazılanı silmez.
/// Parça sor ekranından gelinirse ("geri") oraya dönüş düğmesi çıkar.
/// </summary>
public class SupplierScreenTests : TestContext
{
    readonly FakeApi api = new();
    readonly MemoryTokens tokens = new("t-1");
    const string S1 = "55555555-5555-5555-5555-555555555551";

    public SupplierScreenTests()
    {
        Services.AddSingleton<ITokenStore>(tokens);
        Services.AddSingleton(new TamironApi(new HttpClient(api) { BaseAddress = new Uri("http://api.test/") }, tokens));
        api.Routes["GET /api/suppliers"] = (HttpStatusCode.OK, $"[{{\"id\":\"{S1}\",\"name\":\"Öz Parça\",\"phone\":\"0533 100 00 01\"}}]");
    }

    NavigationManager Nav => Services.GetRequiredService<NavigationManager>();

    [Fact]
    public void Without_session_goes_to_login()
    {
        tokens.Token = null;
        RenderComponent<Parcacilar>();
        Assert.Equal("http://localhost/giris", Nav.Uri);
    }

    [Fact]
    public void Lists_with_a_callable_phone()
    {
        var c = RenderComponent<Parcacilar>();
        c.WaitForAssertion(() => Assert.Single(c.FindAll("#pcListe [data-parcaci]")));
        Assert.Contains("Öz Parça", c.Find("#pcListe").TextContent);
        Assert.Equal("tel:05331000001", c.Find("#pcListe a[href^='tel:']").GetAttribute("href"));
    }

    [Fact]
    public void Empty_says_what_suppliers_are_for()
    {
        api.Routes["GET /api/suppliers"] = (HttpStatusCode.OK, "[]");
        var c = RenderComponent<Parcacilar>();
        c.WaitForAssertion(() => Assert.Contains("Henüz parçacı yok", c.Find("#pcListe").TextContent));
    }

    [Fact]
    public void Adding_sends_name_and_phone_and_refreshes()
    {
        api.Routes["POST /api/suppliers"] = (HttpStatusCode.OK, "{\"id\":\"55555555-5555-5555-5555-555555555552\",\"name\":\"Can Oto\",\"phone\":\"0533 100 00 02\"}");
        var c = RenderComponent<Parcacilar>();
        c.WaitForElement("#pcAd").Input("Can Oto");
        c.Find("#pcTel").Input("05331000002");
        api.Routes["GET /api/suppliers"] = (HttpStatusCode.OK, $"[{{\"id\":\"{S1}\",\"name\":\"Öz Parça\",\"phone\":\"0533 100 00 01\"}},"
            + "{\"id\":\"55555555-5555-5555-5555-555555555552\",\"name\":\"Can Oto\",\"phone\":\"0533 100 00 02\"}]");
        c.Find("#pcEkle").Click();
        c.WaitForAssertion(() => Assert.Equal(2, c.FindAll("#pcListe [data-parcaci]").Count));
        using var b = JsonDocument.Parse(api.Calls.Single(x => x.Path == "/api/suppliers" && x.Body is not null).Body!);
        Assert.Equal("Can Oto", b.RootElement.GetProperty("name").GetString());
        Assert.Equal("05331000002", b.RootElement.GetProperty("phone").GetString());
        Assert.Equal("", c.Find("#pcAd").GetAttribute("value") ?? "");   // form boşaldı
    }

    [Fact]
    public void Taken_number_is_explained_and_the_typed_stays()
    {
        api.Routes["POST /api/suppliers"] = (HttpStatusCode.UnprocessableEntity, "{\"code\":\"supplier.phone_taken\",\"title\":\"0533 100 00 01 başka bir parçacıda kayıtlı.\"}");
        var c = RenderComponent<Parcacilar>();
        c.WaitForElement("#pcAd").Input("Öz Parça 2");
        c.Find("#pcTel").Input("05331000001");
        c.Find("#pcEkle").Click();
        c.WaitForAssertion(() => Assert.Equal("0533 100 00 01 başka bir parçacıda kayıtlı.", c.Find("#pcHata").TextContent.Trim()));
        Assert.Equal("Öz Parça 2", c.Find("#pcAd").GetAttribute("value"));
    }

    [Fact]
    public void Editing_puts_the_new_values()
    {
        api.Routes[$"PUT /api/suppliers/{S1}"] = (HttpStatusCode.OK, $"{{\"id\":\"{S1}\",\"name\":\"Öz Parça Ltd\",\"phone\":\"0533 100 00 01\"}}");
        var c = RenderComponent<Parcacilar>();
        c.WaitForElement("[data-duzenle]").Click();
        Assert.Equal("Öz Parça", c.Find("#pcDuzAd").GetAttribute("value"));
        c.Find("#pcDuzAd").Input("Öz Parça Ltd");
        c.Find("#pcKaydet").Click();
        c.WaitForAssertion(() => Assert.Contains(api.Calls, x => x.Path == $"/api/suppliers/{S1}" && x.Body is not null));
        using var b = JsonDocument.Parse(api.Calls.Single(x => x.Path == $"/api/suppliers/{S1}").Body!);
        Assert.Equal("Öz Parça Ltd", b.RootElement.GetProperty("name").GetString());
        Assert.Equal("0533 100 00 01", b.RootElement.GetProperty("phone").GetString());
    }

    [Fact]
    public void Delete_asks_once_then_deletes()
    {
        api.Routes[$"DELETE /api/suppliers/{S1}"] = (HttpStatusCode.NoContent, "");
        var c = RenderComponent<Parcacilar>();
        c.WaitForElement("[data-sil]").Click();
        Assert.DoesNotContain(api.Calls, x => x.Path == $"/api/suppliers/{S1}");   // ilk dokunuş sormak
        Assert.Contains("Geçmiş sorular adıyla kalır.", c.Find("#pcListe").TextContent);
        api.Routes["GET /api/suppliers"] = (HttpStatusCode.OK, "[]");
        c.Find("#pcSilEvet").Click();
        c.WaitForAssertion(() => Assert.Contains("Henüz parçacı yok", c.Find("#pcListe").TextContent));
        Assert.Contains(api.Calls, x => x.Path == $"/api/suppliers/{S1}");
    }

    [Fact]
    public void Coming_from_part_question_offers_the_way_back()
    {
        Nav.NavigateTo("/parcacilar?geri=%2Fkayit%2F11111111-1111-1111-1111-111111111111%2Fparca-sor");
        var c = RenderComponent<Parcacilar>();
        c.WaitForElement("#pcGeri").Click();
        Assert.Equal("http://localhost/kayit/11111111-1111-1111-1111-111111111111/parca-sor", Nav.Uri);
    }

    [Fact]
    public void Way_back_never_leaves_the_app()
    {
        Nav.NavigateTo("/parcacilar?geri=https%3A%2F%2Fkotu.example");
        var c = RenderComponent<Parcacilar>();
        c.WaitForElement("#pcListe");
        Assert.Empty(c.FindAll("#pcGeri"));
    }
}
