using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Application.ServiceRecords;
using Tamiron.Domain;
using Tamiron.Infrastructure;

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

    /// <summary>
    /// K-078 · Teslimde alınan para bir ÖDEME SATIRI olur (toplam, yol, teslim günü). Hata sınıfı:
    /// teslim ucu kaydı satırsız yüklüyordu — domain toplamı 0 gördü, satır yazılmadı ve hiçbir test
    /// kırmızı olmadı (kasa o gün hâlâ eski alanlardan okunuyordu). Ödeme tablosunun KENDİSİNE bakılır.
    /// </summary>
    [Fact]
    public async Task Paid_delivery_writes_a_payment_row_of_the_total()
    {
        var api = f.For(ApiFactory.ShopB);
        var r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "34 TES 03", complaint = "fren" }, Json));
        await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Part", name = "Balata", price = 1200 }, Json));
        await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Labor", name = "İşçilik", price = 300 }, Json));
        await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/deliver", new { method = "Card" }, Json));

        using var scope = f.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<Tamiron.Api.RequestContext>().TenantId = ApiFactory.ShopB;
        var p = await scope.ServiceProvider.GetRequiredService<TamironDbContext>().Payments.AsNoTracking()
            .SingleAsync(x => x.ServiceRecordId == r.Id);
        Assert.Equal((1500m, PaymentMethod.Card, f.Clock.Today, true), (p.Amount, p.Method, p.PaidOn, p.AtDelivery));
    }
}
