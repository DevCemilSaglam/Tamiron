using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.ServiceRecords;

namespace Tamiron.Api.Tests;

/// <summary>D14 · Teslimi geri al ucu: kasa ve "serviste" bir daha doğru; ertesi gün 422.</summary>
public class UndoDeliveryApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    static async Task<T> Read<T>(HttpResponseMessage res)
    {
        if (!res.IsSuccessStatusCode) throw new Exception($"{(int)res.StatusCode}: {await res.Content.ReadAsStringAsync()}");
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    static async Task<decimal> CashToday(HttpClient api)
        => (await Read<JsonElement>(await api.GetAsync("/api/cash?period=Today"))).GetProperty("collected").GetDecimal();

    [Fact]
    public async Task Undo_takes_the_money_out_of_todays_cash_and_reopens()
    {
        var api = f.For(ApiFactory.ShopB);
        var r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "34 TG 01", complaint = "Fren" }, Json));
        r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Labor", name = "İşçilik", price = 750 }, Json));
        var before = await CashToday(api);
        await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/deliver", new { method = "Cash" }, Json));
        Assert.Equal(before + 750, await CashToday(api));

        var back = await Read<ServiceRecordDto>(await api.PostAsync($"/api/records/{r.Id}/deliver/undo", null));
        Assert.Equal((null, null), (back.DeliveredAt, back.PaymentMethod));
        Assert.Equal(before, await CashToday(api));   // kasaya olmayan para yazılı kalmaz
    }

    [Fact]
    public async Task Next_day_undo_is_refused()
    {
        var api = f.For(ApiFactory.ShopB);
        var r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "34 TG 02", complaint = "Fren" }, Json));
        await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/deliver", new { method = "Cash" }, Json));
        var today = f.Clock.Today;
        HttpResponseMessage res;
        try { f.Clock.Today = today.AddDays(1); res = await api.PostAsync($"/api/records/{r.Id}/deliver/undo", null); }
        finally { f.Clock.Today = today; }
        Assert.Equal(HttpStatusCode.UnprocessableEntity, res.StatusCode);
        Assert.Equal("delivery.undo_too_late", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
    }
}
