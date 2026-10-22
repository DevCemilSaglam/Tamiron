using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.ServiceRecords;
using Tamiron.Domain;

namespace Tamiron.Api.Tests;

/// <summary>B5 · Teslim: ödeme yolu + hatırlatma; önizleme kaydetmez.</summary>
public class DeliveryApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    async Task<T> Read<T>(HttpResponseMessage res)
    {
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    [Fact]
    public async Task Maintenance_delivery_uses_shop_policy_and_vehicle_km_history()
    {
        var api = f.For(ApiFactory.ShopB);
        // Önce bir bakım: 20 gün önce teslim, 50.000 km.
        f.Clock.Today = new DateOnly(2026, 10, 2);
        var r1 = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "34 TES 01", complaint = "bakım", km = "50000" }, Json));
        await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r1.Id}/deliver", new { method = "Cash" }, Json));

        // Bugün ikinci bakım: 20 günde 2.000 km → günde 100 → 10.000 km 100 gün.
        f.Clock.Today = new DateOnly(2026, 10, 22);
        var r2 = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "34 TES 01", complaint = "bakım", km = "52000" }, Json));

        var preview = await Read<DeliveryPreview>(await api.GetAsync($"/api/records/{r2.Id}/delivery-preview"));
        Assert.Equal(new DateOnly(2027, 1, 30), preview.Reminder!.Date);   // 22.10 + 100 gün
        Assert.Equal(ReminderBasis.Km, preview.Reminder.Basis);
        Assert.Null((await Read<ServiceRecordDto>(await api.GetAsync($"/api/records/{r2.Id}"))).DeliveredAt);   // önizleme kaydetmez

        var done = await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r2.Id}/deliver", new { method = "Unpaid" }, Json));
        Assert.Equal(f.Clock.Today, done.DeliveredAt);
        Assert.Equal(PaymentMethod.Unpaid, done.PaymentMethod);
        Assert.Equal(new DateOnly(2027, 1, 30), done.ReminderDate);

        var again = await api.PostAsJsonAsync($"/api/records/{r2.Id}/deliver", new { method = "Cash" }, Json);
        Assert.Equal((HttpStatusCode)422, again.StatusCode);
    }

    [Fact]
    public async Task Repair_delivery_has_no_reminder_and_preview_says_so()
    {
        var api = f.For(ApiFactory.ShopB);
        var r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "34 TES 02", complaint = "fren" }, Json));
        Assert.Null((await Read<DeliveryPreview>(await api.GetAsync($"/api/records/{r.Id}/delivery-preview"))).Reminder);
        var done = await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/deliver", new { method = "Card" }, Json));
        Assert.Null(done.ReminderDate);
        Assert.Equal(f.Clock.Today, done.CollectedOn);
    }
}
