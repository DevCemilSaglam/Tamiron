using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.ServiceRecords;

namespace Tamiron.Api.Tests;

/// <summary>E1 · Ödeme yolunu düzelt ucu: kasanın nakit/kart dağılımı düzelir, toplam aynı kalır.</summary>
public class PaymentCorrectionApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    static async Task<T> Read<T>(HttpResponseMessage res)
    {
        if (!res.IsSuccessStatusCode) throw new Exception($"{(int)res.StatusCode}: {await res.Content.ReadAsStringAsync()}");
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    static async Task<(decimal Cash, decimal Card, decimal Collected)> Today(HttpClient api)
    {
        var c = await Read<JsonElement>(await api.GetAsync("/api/cash?period=Today"));
        return (c.GetProperty("cash").GetDecimal(), c.GetProperty("card").GetDecimal(), c.GetProperty("collected").GetDecimal());
    }

    [Fact]
    public async Task Cash_corrected_to_card_moves_money_between_columns()
    {
        var api = f.For(ApiFactory.ShopB);
        var r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "34 OD 010", complaint = "Fren" }, Json));
        r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Labor", name = "İşçilik", price = 640 }, Json));
        await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/deliver", new { method = "Cash" }, Json));
        var before = await Today(api);

        var fixedR = await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/payment", new { method = "Card" }, Json));
        Assert.Equal("Card", fixedR.PaymentMethod.ToString());
        var after = await Today(api);
        Assert.Equal((before.Cash - 640, before.Card + 640, before.Collected), after);   // toplam aynı, sütun değişti
    }

    [Fact]
    public async Task Open_record_is_a_speaking_error()
    {
        var api = f.For(ApiFactory.ShopB);
        var r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "34 OD 020", complaint = "Fren" }, Json));
        var res = await api.PostAsJsonAsync($"/api/records/{r.Id}/payment", new { method = "Card" }, Json);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, res.StatusCode);
        Assert.Equal("payment.not_delivered", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
    }
}
