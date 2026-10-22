using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.Accounting;
using Tamiron.Application.ServiceRecords;

namespace Tamiron.Api.Tests;

/// <summary>
/// C2 · Parça alışları — prototip <c>parcaAlislari</c>: yalnız TESLİM edilmiş işlerde,
/// tamircinin aldığı ve alışı yazılı (sıfırdan büyük) parçalar. İçerideki araçta parça
/// henüz alınmamış olabilir; olmamış harcama sayılmaz.
/// </summary>
public class PartPurchaseApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    async Task<T> Read<T>(HttpResponseMessage res)
    {
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    [Fact]
    public async Task Only_delivered_shop_bought_known_cost_parts_in_period()
    {
        var api = f.For(ApiFactory.ShopB);
        f.Clock.Today = new DateOnly(2026, 10, 20);
        var r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "34 ALS 01", complaint = "fren" }, Json));
        await api.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Part", name = "Balata", price = 1200, cost = 800 }, Json);
        await api.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Part", name = "Disk", price = 2000 }, Json);                         // alış yok
        await api.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Part", name = "Conta", price = 50, cost = 0 }, Json);                // bedava
        await api.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Part", name = "Yağ", price = 900, cost = 700, suppliedBy = "Customer" }, Json);
        await api.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Labor", name = "İşçilik", price = 500 }, Json);
        await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/deliver", new { method = "Cash" }, Json));

        // İçerideki araç: alışı yazılı olsa da sayılmaz.
        var icerde = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "34 ALS 02", complaint = "x" }, Json));
        await api.PostAsJsonAsync($"/api/records/{icerde.Id}/lines", new { kind = "Part", name = "Akü", price = 3200, cost = 2450 }, Json);
        f.Clock.Today = new DateOnly(2026, 10, 22);

        var ay = await Read<PartPurchaseList>(await api.GetAsync("/api/part-purchases?period=Month"));
        var bal = Assert.Single(ay.Items);
        Assert.Equal(("Balata", 800m, "34 ALS 01", new DateOnly(2026, 10, 20)), (bal.Name, bal.Cost, bal.PlateNo, bal.DeliveredAt));
        Assert.Equal(800m, ay.Total);

        Assert.Empty((await Read<PartPurchaseList>(await api.GetAsync("/api/part-purchases?period=Today"))).Items);   // teslim 20.10
    }
}
