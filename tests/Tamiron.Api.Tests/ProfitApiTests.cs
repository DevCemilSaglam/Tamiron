using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.Accounting;

namespace Tamiron.Api.Tests;

/// <summary>C1 · İş başına fark (96-is-basina) — dönem TESLİM gününe göre, en yeni üstte, toplam.</summary>
public class ProfitApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    [Fact]
    public async Task Year_list_newest_first_with_totals_and_unknown_count()
    {
        // A tohumu yıl içinde teslim: ara bakım (20.06, fark 1.350), silecek (bugün, 700 + 1 bilinmeyen), akü (28.08, 3.450 − 2.450 = 1.000).
        var l = (await f.For(ApiFactory.ShopA).GetFromJsonAsync<JobProfitList>("/api/profit?period=Year", Json))!;
        Assert.Equal(new[] { "35 KLM 45", "06 TMR 07", "34 ABC 123" }, l.Items.Select(i => i.PlateNo));
        Assert.Equal(new[] { 700m, 1000m, 1350m }, l.Items.Select(i => i.Difference));
        Assert.Equal((3050m, 1, 2100m), (l.Difference, l.UnknownCount, l.Outside));
        Assert.Equal(2800m + 3450m + 2850m, l.Received);
        Assert.False(l.AskPartCost);   // ekran "alış fiyatı bugün sorulmuyor" der
    }

    [Fact]
    public async Task Today_counts_only_delivered_today()
    {
        var l = (await f.For(ApiFactory.ShopA).GetFromJsonAsync<JobProfitList>("/api/profit?period=Today", Json))!;
        Assert.Equal("35 KLM 45", Assert.Single(l.Items).PlateNo);
    }
}
