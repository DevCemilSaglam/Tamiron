using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Api;
using Tamiron.Application.ServiceRecords;
using Tamiron.Domain;
using Tamiron.Infrastructure;

namespace Tamiron.Api.Tests;

/// <summary>
/// İlk API kuralı: veresiyenin tahsili ve kasa özeti (K-071.1, K-073.5), kiracı
/// yalıtımı ve soft-delete. Prototipteki 59. denetim testinin (ALACAK-TAHSIL)
/// backend karşılığı.
/// </summary>
public class CollectionApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    async Task<Guid> UnpaidRecordOfA()
    {
        using var scope = f.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<HeaderTenantContext>().TenantId = ApiFactory.ShopA;
        var db = scope.ServiceProvider.GetRequiredService<TamironDbContext>();
        return await db.ServiceRecords.Where(r => r.Complaint == "Akü değişimi").Select(r => r.Id).SingleAsync();
    }

    [Fact]
    public async Task Collecting_unpaid_moves_money_from_receivable_into_todays_cash_and_undo_reverts()
    {
        var api = f.For(ApiFactory.ShopA);
        var id = await UnpaidRecordOfA();

        var before = await api.GetFromJsonAsync<CashSummary>("/api/cash?period=Today", Json);
        Assert.Equal(3450m, before!.Receivable);
        Assert.Equal(2800m, before.Collected);          // bugün nakit teslim (silecek motoru)

        var res = await api.PostAsJsonAsync($"/api/records/{id}/collect", new { method = "Card" }, Json);
        res.EnsureSuccessStatusCode();
        var rec = await res.Content.ReadFromJsonAsync<ServiceRecordDto>(Json);
        Assert.Equal(PaymentMethod.Card, rec!.PaymentMethod);
        Assert.Equal(f.Clock.Today, rec.CollectedOn);   // gün sunucudan

        var after = await api.GetFromJsonAsync<CashSummary>("/api/cash?period=Today", Json);
        Assert.Equal(0m, after!.Receivable);
        Assert.Equal(3450m, after.Card);
        Assert.Equal(2800m + 3450m, after.Collected);
        Assert.Equal(1, after.DeliveredCount);          // TESLİM bugün değil (55 gün önce): sayı değişmez

        (await api.PostAsync($"/api/records/{id}/collect/undo", null)).EnsureSuccessStatusCode();
        var undone = await api.GetFromJsonAsync<CashSummary>("/api/cash?period=Today", Json);
        Assert.Equal(3450m, undone!.Receivable);
        Assert.Equal(2800m, undone.Collected);
    }

    [Fact]
    public async Task Collecting_a_paid_record_is_rejected_with_code_and_turkish_message()
    {
        var api = f.For(ApiFactory.ShopA);
        var cash = await api.GetFromJsonAsync<CashSummary>("/api/cash?period=Year", Json);
        Assert.True(cash!.Collected > 0);

        using var scope = f.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<HeaderTenantContext>().TenantId = ApiFactory.ShopA;
        var paidId = await scope.ServiceProvider.GetRequiredService<TamironDbContext>()
            .ServiceRecords.Where(r => r.Complaint == "Ara bakım").Select(r => r.Id).SingleAsync();

        var res = await api.PostAsJsonAsync($"/api/records/{paidId}/collect", new { method = "Cash" }, Json);
        Assert.Equal((HttpStatusCode)422, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("payment.not_unpaid", body.GetProperty("code").GetString());
        Assert.Contains("ödeme", body.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Another_shops_record_is_invisible_not_forbidden()
    {
        // Başka dükkânın kaydı 404: 403 deseydik kaydın VAR olduğunu söylemiş olurduk.
        var api = f.For(ApiFactory.ShopA);
        Assert.Equal(HttpStatusCode.NotFound, (await api.GetAsync($"/api/records/{f.ShopBRecordId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await api.PostAsJsonAsync($"/api/records/{f.ShopBRecordId}/collect", new { method = "Cash" }, Json)).StatusCode);

        // B'nin 999 ₺ alacağı A'nın kasasına karışmaz; B kendi kaydını görür.
        var a = await api.GetFromJsonAsync<CashSummary>("/api/cash?period=Year", Json);
        Assert.DoesNotContain(999m, new[] { a!.Receivable, a.Receivable - 3450m });
        var b = await f.For(ApiFactory.ShopB).GetFromJsonAsync<CashSummary>("/api/cash?period=Year", Json);
        Assert.Equal(999m, b!.Receivable);
    }

    [Fact]
    public async Task Request_without_shop_is_rejected()
        => Assert.Equal(HttpStatusCode.Unauthorized, (await f.CreateClient().GetAsync("/api/cash?period=Today")).StatusCode);

    [Fact]
    public async Task Delete_is_soft_row_stays_but_disappears_from_queries()
    {
        Guid id;
        using (var scope = f.Services.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<HeaderTenantContext>().TenantId = ApiFactory.ShopB;
            var db = scope.ServiceProvider.GetRequiredService<TamironDbContext>();
            var v = new Vehicle { PlateNo = "34 SİL 01" };
            db.Vehicles.Add(v);
            await db.SaveChangesAsync();
            id = v.Id;
            db.Vehicles.Remove(v);
            await db.SaveChangesAsync();
        }
        using (var scope = f.Services.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<HeaderTenantContext>().TenantId = ApiFactory.ShopB;
            var db = scope.ServiceProvider.GetRequiredService<TamironDbContext>();
            Assert.False(await db.Vehicles.AnyAsync(v => v.Id == id));
            var row = await db.Vehicles.IgnoreQueryFilters().SingleAsync(v => v.Id == id);
            Assert.NotNull(row.DateTimeDeleted);
            // Silinen araç plakayı kilitlemez (süzgeçli tekil indeks).
            db.Vehicles.Add(new Vehicle { PlateNo = "34 SİL 01" });
            await db.SaveChangesAsync();
        }
    }
}
