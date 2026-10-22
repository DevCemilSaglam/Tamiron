using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Application.ServiceRecords;
using Tamiron.Infrastructure;

namespace Tamiron.Api.Tests;

/// <summary>
/// C6 · Yeni satır dükkânın KDV varsayılanını alır ve DONDURUR; dükkân varsayılanı sonradan
/// değişse de eski satır kendi oranında kalır. Ekranda yok — veritabanından okunur.
/// </summary>
public class VatPlaceholderApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    [Fact]
    public async Task New_line_takes_shop_default_and_keeps_it_when_default_changes()
    {
        var api = f.For(ApiFactory.ShopB);
        var r = await (await api.PostAsJsonAsync("/api/records", new { plate = "34 KDV 01", complaint = "fren" }, Json)).Content.ReadFromJsonAsync<ServiceRecordDto>(Json);
        r = await (await api.PostAsJsonAsync($"/api/records/{r!.Id}/lines", new { kind = "Part", name = "Balata", price = 1200 }, Json)).Content.ReadFromJsonAsync<ServiceRecordDto>(Json);
        var eski = r!.Lines.Single().Id;

        using (var scope = f.Services.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<RequestContext>().TenantId = ApiFactory.ShopB;
            var db = scope.ServiceProvider.GetRequiredService<TamironDbContext>();
            Assert.Equal(20m, (await db.ServiceLines.SingleAsync(l => l.Id == eski)).VatRate);
            (await db.Shops.SingleAsync()).DefaultVatRate = 18m;   // devlet oranı değiştirdi
            await db.SaveChangesAsync();
        }

        r = await (await api.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Labor", name = "İşçilik", price = 500 }, Json)).Content.ReadFromJsonAsync<ServiceRecordDto>(Json);
        var yeni = r!.Lines.Single(l => l.Name == "İşçilik").Id;
        using (var scope = f.Services.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<RequestContext>().TenantId = ApiFactory.ShopB;
            var db = scope.ServiceProvider.GetRequiredService<TamironDbContext>();
            Assert.Equal(20m, (await db.ServiceLines.SingleAsync(l => l.Id == eski)).VatRate);   // eski iş kendi oranında
            Assert.Equal(18m, (await db.ServiceLines.SingleAsync(l => l.Id == yeni)).VatRate);
        }
    }
}
