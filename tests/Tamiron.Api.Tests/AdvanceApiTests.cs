using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.Accounting;
using Tamiron.Application.Advances;
using Tamiron.Application.ServiceRecords;
using Tamiron.Application.Vehicles;

namespace Tamiron.Api.Tests;

/// <summary>
/// G7 · Müşteri avansı uçtan uca (K-079.1, TASARIM-G-AVANS): bakiye sahibe göre; avans alındığı gün
/// kasaya girer, iade çıkar; avanstan düşülen ödeme kalanı kapatır ama kasaya girmez; bakiye sınırı.
/// Her test kendi dükkânında (yalıtım: B dükkânının kasası başka testlerle paylaşılır).
/// </summary>
public class AdvanceApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    static async Task<T> Read<T>(HttpResponseMessage res)
    {
        Assert.True(res.IsSuccessStatusCode, await res.Content.ReadAsStringAsync());
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    static async Task<string> Code(HttpResponseMessage res)
    {
        Assert.Equal(HttpStatusCode.UnprocessableEntity, res.StatusCode);
        return (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString()!;
    }

    async Task<(HttpClient Api, ServiceRecordDto Rec)> Is(string plate, decimal price, string? owner = null, string? phone = null)
    {
        var api = f.For(await f.NewShopAsync());
        var r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new
        {
            plate, complaint = "Fren", vehicle = owner is null ? null : new { ownerName = owner, ownerPhone = phone },
        }, Json));
        r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Labor", name = "İşçilik", price }, Json));
        return (api, r);
    }

    [Fact]
    public async Task Received_advance_is_cash_today_and_a_balance()
    {
        var (api, r) = await Is("06 AVA 01", 1000);
        var v = await Read<AdvanceView>(await api.PostAsJsonAsync($"/api/vehicles/{r.VehicleId}/advance", new { amount = 1500, method = "Cash" }, Json));
        Assert.Equal(1500m, v.Balance);
        var cash = await Read<CashSummary>(await api.GetAsync("/api/cash?period=Today"));
        Assert.Equal((1500m, 1500m), (cash.Cash, cash.Collected));
    }

    [Fact]
    public async Task Paying_from_advance_closes_the_job_without_counting_twice()
    {
        var (api, r) = await Is("06 AVA 02", 1000);
        await Read<AdvanceView>(await api.PostAsJsonAsync($"/api/vehicles/{r.VehicleId}/advance", new { amount = 1500, method = "Card" }, Json));
        var d = await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/deliver", new { method = "Advance" }, Json));
        Assert.Equal(0m, d.Remaining);
        Assert.Equal(500m, (await Read<AdvanceView>(await api.GetAsync($"/api/vehicles/{r.VehicleId}/advance"))).Balance);
        var cash = await Read<CashSummary>(await api.GetAsync("/api/cash?period=Today"));
        Assert.Equal((1500m, 1500m), (cash.Card, cash.Collected));   // yalnız avansın kendisi
        var home = await Read<Tamiron.Application.Home.HomeSummary>(await api.GetAsync("/api/home"));
        Assert.Equal(1500m, home.CollectedToday);
    }

    [Fact]
    public async Task Advance_cannot_cover_more_than_its_balance()
    {
        var (api, r) = await Is("06 AVA 03", 1000);
        await Read<AdvanceView>(await api.PostAsJsonAsync($"/api/vehicles/{r.VehicleId}/advance", new { amount = 400, method = "Cash" }, Json));
        Assert.Equal("advance.over_balance", await Code(await api.PostAsJsonAsync($"/api/records/{r.Id}/deliver", new { method = "Advance" }, Json)));
        Assert.Null((await Read<ServiceRecordDto>(await api.GetAsync($"/api/records/{r.Id}"))).DeliveredAt);   // teslim edilmedi
        var d = await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/deliver", new { method = "Advance", amount = 400 }, Json));
        Assert.Equal(600m, d.Remaining);
        Assert.Equal("advance.over_balance", await Code(await api.PostAsJsonAsync($"/api/records/{r.Id}/collect", new { method = "Advance", amount = 1 }, Json)));
    }

    [Fact]
    public async Task Balance_belongs_to_the_owner_across_their_vehicles()
    {
        var (api, r) = await Is("06 AVA 04", 1000, "Veli Kaya", "0532 111 22 33");
        var ikinci = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new
        { plate = "06 AVA 05", complaint = "", vehicle = new { ownerName = "veli kaya", ownerPhone = "05321112233" } }, Json));
        var baska = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new
        { plate = "06 AVA 06", complaint = "", vehicle = new { ownerName = "Veli Kaya", ownerPhone = "0532 999 99 99" } }, Json));
        await Read<AdvanceView>(await api.PostAsJsonAsync($"/api/vehicles/{r.VehicleId}/advance", new { amount = 700, method = "Cash" }, Json));
        Assert.Equal(700m, (await Read<AdvanceView>(await api.GetAsync($"/api/vehicles/{ikinci.VehicleId}/advance"))).Balance);
        Assert.Equal(0m, (await Read<AdvanceView>(await api.GetAsync($"/api/vehicles/{baska.VehicleId}/advance"))).Balance);
    }

    [Fact]
    public async Task Refund_leaves_the_till_and_is_limited_by_the_balance()
    {
        var (api, r) = await Is("06 AVA 07", 1000);
        await Read<AdvanceView>(await api.PostAsJsonAsync($"/api/vehicles/{r.VehicleId}/advance", new { amount = 1000, method = "Cash" }, Json));
        Assert.Equal("advance.over_balance", await Code(await api.PostAsJsonAsync($"/api/vehicles/{r.VehicleId}/advance/refund", new { amount = 1200, method = "Cash" }, Json)));
        var v = await Read<AdvanceView>(await api.PostAsJsonAsync($"/api/vehicles/{r.VehicleId}/advance/refund", new { amount = 300, method = "Cash" }, Json));
        Assert.Equal(700m, v.Balance);
        var cash = await Read<CashSummary>(await api.GetAsync("/api/cash?period=Today"));
        Assert.Equal(700m, cash.Cash);
    }

    [Fact]
    public async Task Undo_restores_and_refuses_to_go_negative()
    {
        var (api, r) = await Is("06 AVA 08", 1000);
        var v = await Read<AdvanceView>(await api.PostAsJsonAsync($"/api/vehicles/{r.VehicleId}/advance", new { amount = 1000, method = "Cash" }, Json));
        await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/deliver", new { method = "Advance", amount = 600 }, Json));
        var alis = v.Items.Single().Id;
        Assert.Equal("advance.undo_would_go_negative", await Code(await api.PostAsync($"/api/advances/{alis}/undo", null)));
        await Read<ServiceRecordDto>(await api.PostAsync($"/api/records/{r.Id}/deliver/undo", null));
        v = await Read<AdvanceView>(await api.PostAsync($"/api/advances/{alis}/undo", null));
        Assert.Equal(0m, v.Balance);
        Assert.Empty(v.Items);
    }

    [Fact]
    public async Task Csv_lists_advances_and_does_not_count_the_advance_payment_as_takings()
    {
        var (api, r) = await Is("06 AVA 09", 1000);
        await Read<AdvanceView>(await api.PostAsJsonAsync($"/api/vehicles/{r.VehicleId}/advance", new { amount = 1500, method = "Cash" }, Json));
        await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/deliver", new { method = "Advance" }, Json));
        await Read<AdvanceView>(await api.PostAsJsonAsync($"/api/vehicles/{r.VehicleId}/advance/refund", new { amount = 500, method = "Cash" }, Json));
        var csv = await api.GetStringAsync("/api/export/cash.csv?period=Today");
        Assert.Contains("Avans;", csv);
        Assert.Contains("Avans iadesi;", csv);
        Assert.Contains(";-500", csv);
        Assert.Contains("Avanstan düşüldü;", csv);
        Assert.DoesNotContain("Tahsilat;", csv);
    }

    [Fact]
    public async Task Advance_is_tenant_isolated()
    {
        var (api, r) = await Is("06 AVA 10", 1000);
        await Read<AdvanceView>(await api.PostAsJsonAsync($"/api/vehicles/{r.VehicleId}/advance", new { amount = 100, method = "Cash" }, Json));
        Assert.Equal(HttpStatusCode.NotFound, (await f.For(ApiFactory.ShopA).GetAsync($"/api/vehicles/{r.VehicleId}/advance")).StatusCode);
    }
}
