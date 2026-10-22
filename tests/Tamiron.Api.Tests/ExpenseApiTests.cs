using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.Expenses;
using Tamiron.Domain;

namespace Tamiron.Api.Tests;

/// <summary>B8 · Gider: ekle (gün sunucudan), dönem listesi + toplam, sil / geri al, kiracı.</summary>
public class ExpenseApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    async Task<T> Read<T>(HttpResponseMessage res)
    {
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    [Fact]
    public async Task Period_list_newest_first_with_total_delete_and_restore()
    {
        var api = f.For(ApiFactory.ShopB);
        f.Clock.Today = new DateOnly(2026, 9, 30);
        await Read<ExpenseDto>(await api.PostAsJsonAsync("/api/expenses", new { description = "Eylül kirası", amount = 12000 }, Json));
        f.Clock.Today = new DateOnly(2026, 10, 1);
        var elektrik = await Read<ExpenseDto>(await api.PostAsJsonAsync("/api/expenses", new { description = "Elektrik", amount = 2400 }, Json));
        f.Clock.Today = new DateOnly(2026, 10, 22);
        await Read<ExpenseDto>(await api.PostAsJsonAsync("/api/expenses", new { description = "Çay ocağı", amount = 350 }, Json));
        Assert.Equal(new DateOnly(2026, 10, 1), elektrik.Date);   // gün sunucudan

        var ay = await Read<ExpenseList>(await api.GetAsync("/api/expenses?period=Month"));
        Assert.Equal(new[] { "Çay ocağı", "Elektrik" }, ay.Items.Select(i => i.Description));   // en yeni üstte, eylül yok
        Assert.Equal(2750m, ay.Total);

        var yil = await Read<ExpenseList>(await api.GetAsync("/api/expenses?period=Year"));
        Assert.Equal(14750m, yil.Total);

        var silindi = await Read<ExpenseList>(await api.DeleteAsync($"/api/expenses/{elektrik.Id}?period=Month"));
        Assert.Equal(350m, silindi.Total);
        Assert.Equal("Çay ocağı", Assert.Single(silindi.Items).Description);
        var geri = await Read<ExpenseList>(await api.PostAsync($"/api/expenses/{elektrik.Id}/restore?period=Month", null));
        Assert.Equal(2750m, geri.Total);
        Assert.Equal(new[] { "Çay ocağı", "Elektrik" }, geri.Items.Select(i => i.Description));   // yerine döner
    }

    [Fact]
    public async Task Incomplete_expense_is_rejected()
    {
        var res = await f.For(ApiFactory.ShopB).PostAsJsonAsync("/api/expenses", new { description = "Kira", amount = 0 }, Json);
        Assert.Equal((HttpStatusCode)422, res.StatusCode);
    }

    [Fact]
    public async Task Another_shop_cannot_delete_or_restore()
    {
        var b = f.For(ApiFactory.ShopB);
        var e = await Read<ExpenseDto>(await b.PostAsJsonAsync("/api/expenses", new { description = "Gizli gider", amount = 99 }, Json));
        var a = f.For(ApiFactory.ShopA);
        Assert.Equal(HttpStatusCode.NotFound, (await a.DeleteAsync($"/api/expenses/{e.Id}")).StatusCode);
        await Read<ExpenseList>(await b.DeleteAsync($"/api/expenses/{e.Id}"));
        // Geri alma süzgeçsiz sorgu kullanır — kiracı açıkça denetlenmeli.
        Assert.Equal(HttpStatusCode.NotFound, (await a.PostAsync($"/api/expenses/{e.Id}/restore", null)).StatusCode);
        Assert.DoesNotContain((await Read<ExpenseList>(await a.GetAsync("/api/expenses?period=Year"))).Items, i => i.Description == "Gizli gider");
    }
}
