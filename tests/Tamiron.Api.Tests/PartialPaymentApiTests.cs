using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.Accounting;
using Tamiron.Application.ServiceRecords;
using Tamiron.Domain;

namespace Tamiron.Api.Tests;

/// <summary>
/// F5 · Elden taksit uçları (K-078): teslimde "Bir kısmını aldım", kayıtta "x lira aldım", fazlası 422,
/// geri al son taksiti siler. Kasa ve alacak aynı istekten okunur — iki rakam birbirini yalanlamasın.
/// </summary>
public class PartialPaymentApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    static async Task<T> Read<T>(HttpResponseMessage res)
    {
        if (!res.IsSuccessStatusCode) throw new Exception($"{(int)res.StatusCode}: {await res.Content.ReadAsStringAsync()}");
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    async Task<(HttpClient api, ServiceRecordDto r)> Job(string plate)
    {
        var api = f.For(ApiFactory.ShopB);
        var r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate, complaint = "Akü" }, Json));
        r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Part", name = "Akü", price = 3200 }, Json));
        r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Labor", name = "Takma", price = 250 }, Json));
        return (api, r);
    }

    static async Task<decimal> ReceivableOf(HttpClient api, Guid vehicleId)
        => (await api.GetFromJsonAsync<ReceivableList>($"/api/receivables?vehicleId={vehicleId}", Json))!.Total;

    [Fact]
    public async Task Partial_at_delivery_then_installments_until_closed()
    {
        var (api, r) = await Job("34 KSM 001");
        var cashBefore = (await api.GetFromJsonAsync<CashSummary>("/api/cash?period=Today", Json))!;

        // Teslimde 1.000 kart — kalan veresiye.
        var d = await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/deliver", new { method = "Card", amount = 1000 }, Json));
        Assert.Equal((PaymentMethod.Unpaid, 1000m, 2450m), (d.PaymentMethod!.Value, d.Paid, d.Remaining));
        var p = Assert.Single(d.Payments);
        Assert.Equal((1000m, PaymentMethod.Card, f.Clock.Today, true), (p.Amount, p.Method, p.PaidOn, p.AtDelivery));
        Assert.Equal(2450m, await ReceivableOf(api, r.VehicleId));

        // Kalandan fazlası: 422, ustanın okuyacağı Türkçe başlık; hiçbir şey yazılmaz.
        var over = await api.PostAsJsonAsync($"/api/records/{r.Id}/collect", new { method = "Cash", amount = 5000 }, Json);
        Assert.Equal((HttpStatusCode)422, over.StatusCode);
        var problem = await over.Content.ReadFromJsonAsync<JsonElement>(Json);
        Assert.Equal(("payment.over_remaining", "Kalan 2.450 ₺ — fazlası girilemez."),
                     (problem.GetProperty("code").GetString(), problem.GetProperty("title").GetString()));

        // 450 nakit taksit, sonra tutarsız "Ödeme alındı" = KALANIN TAMAMI.
        d = await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/collect", new { method = "Cash", amount = 450 }, Json));
        Assert.Equal((1450m, 2000m), (d.Paid, d.Remaining));
        d = await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/collect", new { method = "Transfer" }, Json));
        Assert.Equal((PaymentMethod.Transfer, 0m, 3), (d.PaymentMethod!.Value, d.Remaining, d.Payments.Count));
        Assert.Equal(0m, await ReceivableOf(api, r.VehicleId));

        // Kasa: bugün üç ödeme üç yolda — toplam işin toplamı.
        var cash = (await api.GetFromJsonAsync<CashSummary>("/api/cash?period=Today", Json))!;
        Assert.Equal((1000m, 450m, 2000m), (cash.Card - cashBefore.Card, cash.Cash - cashBefore.Cash, cash.Transfer - cashBefore.Transfer));

        // Geri al: SON taksit gider, iş yeniden veresiye.
        d = await Read<ServiceRecordDto>(await api.PostAsync($"/api/records/{r.Id}/collect/undo", null));
        Assert.Equal((PaymentMethod.Unpaid, 2000m, 2), (d.PaymentMethod!.Value, d.Remaining, d.Payments.Count));
        Assert.Equal(2000m, await ReceivableOf(api, r.VehicleId));
    }

    [Fact]
    public async Task Partial_at_delivery_more_than_total_does_not_deliver()
    {
        var (api, r) = await Job("34 KSM 002");
        var res = await api.PostAsJsonAsync($"/api/records/{r.Id}/deliver", new { method = "Card", amount = 9000 }, Json);
        Assert.Equal((HttpStatusCode)422, res.StatusCode);
        Assert.Null((await Read<ServiceRecordDto>(await api.GetAsync($"/api/records/{r.Id}"))).DeliveredAt);
    }

    [Fact]
    public async Task Receivable_item_shows_total_paid_and_remaining()
    {
        var (api, r) = await Job("34 KSM 003");
        await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/deliver", new { method = "Cash", amount = 450 }, Json));
        var item = Assert.Single((await api.GetFromJsonAsync<ReceivableList>($"/api/receivables?vehicleId={r.VehicleId}", Json))!.Items);
        Assert.Equal((3450m, 450m, 3000m), (item.Total, item.Paid, item.Remaining));
    }
}
