using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Tamiron.Api.Tests;

/// <summary>
/// C8 · Muhasebe ekranı (95-muhasebe "Basit") tek istekte: kasa özeti + dönemin giderleri +
/// alacak. Yeni hesap YOK — her parça kendi ucunun cevabıyla BİREBİR aynı olmalı
/// (aynı soruyu iki yerde cevaplamak iki cevap demektir, K-019.1).
/// </summary>
public class AccountingOverviewApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    [Theory]
    [InlineData("Today")]
    [InlineData("Month")]
    [InlineData("Year")]
    public async Task Each_part_equals_its_own_endpoint(string period)
    {
        var api = f.For(ApiFactory.ShopA);
        await api.PostAsJsonAsync("/api/expenses", new { description = "Çay ocağı", amount = 350 });

        var all = JsonNode.Parse(await api.GetStringAsync($"/api/accounting?period={period}"))!;
        var cash = JsonNode.Parse(await api.GetStringAsync($"/api/cash?period={period}"))!;
        var exp = JsonNode.Parse(await api.GetStringAsync($"/api/expenses?period={period}"))!;
        var rec = JsonNode.Parse(await api.GetStringAsync("/api/receivables"))!;

        Assert.True(JsonNode.DeepEquals(cash, all["cash"]), "kasa özeti ayrışmış");
        Assert.True(JsonNode.DeepEquals(exp, all["expenses"]), "giderler ayrışmış");
        Assert.True(JsonNode.DeepEquals(rec, all["receivables"]), "alacak ayrışmış");
        Assert.Equal(3450m, all["receivables"]!["total"]!.GetValue<decimal>());   // boş cevaplar da "eşit" olurdu
    }
}
