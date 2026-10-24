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

/// <summary>
/// B2 · Ana yol: kayıt aç (prototip 20-ruhsat "Kaydı aç", 60-arac "Yeni kayıt").
/// Kayıt açmak araç kimliği İSTEMEZ (K-068); plaka yeter.
/// </summary>
public class OpenRecordApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    async Task<int> VehicleCount(string plate)
    {
        using var scope = f.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<Tamiron.Api.RequestContext>().TenantId = ApiFactory.ShopB;
        return await scope.ServiceProvider.GetRequiredService<TamironDbContext>().Vehicles.CountAsync(v => v.PlateNo == plate);
    }

    [Fact]
    public async Task Opening_with_new_plate_creates_vehicle_once_and_guesses_type()
    {
        var api = f.For(ApiFactory.ShopB);
        var res = await api.PostAsJsonAsync("/api/records", new { plate = "16 rt 724", complaint = "Yağ bakımı" }, Json);
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        var r = await res.Content.ReadFromJsonAsync<ServiceRecordDto>(Json);
        Assert.Equal("16 RT 724", r!.PlateNo);
        Assert.Equal(ServiceType.Maintenance, r.Type);
        Assert.Null(r.DeliveredAt);                         // serviste
        Assert.Equal($"/api/records/{r.Id}", res.Headers.Location!.OriginalString);

        // Aynı plakayla ikinci kayıt: araç YENİDEN oluşmaz.
        (await api.PostAsJsonAsync("/api/records", new { plate = "16RT724", complaint = "" }, Json)).EnsureSuccessStatusCode();
        Assert.Equal(1, await VehicleCount("16 RT 724"));
    }

    [Fact]
    public async Task Explicit_type_wins_and_is_marked_manual()
    {
        var res = await f.For(ApiFactory.ShopB).PostAsJsonAsync("/api/records",
            new { plate = "35 ZZ 990", complaint = "Yağ bakımı", type = "Repair" }, Json);
        var r = await res.Content.ReadFromJsonAsync<ServiceRecordDto>(Json);
        Assert.Equal(ServiceType.Repair, r!.Type);
        Assert.True(r.TypeSetManually);
    }

    [Fact]
    public async Task Invalid_plate_is_rejected_with_turkish_hint()
    {
        var res = await f.For(ApiFactory.ShopB).PostAsJsonAsync("/api/records", new { plate = "34 AB", complaint = "x" }, Json);
        Assert.Equal((HttpStatusCode)422, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("plate.invalid", body.GetProperty("code").GetString());
        Assert.Contains("34 ABC 123", body.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Same_plate_in_another_shop_is_another_vehicle()
    {
        // B'de "06 TMR 07" var (ApiFactory); A'da da var (tohum). A'da açılan kayıt A'nın aracına gider.
        var res = await f.For(ApiFactory.ShopA).PostAsJsonAsync("/api/records", new { plate = "06 TMR 07", complaint = "Fren" }, Json);
        var r = await res.Content.ReadFromJsonAsync<ServiceRecordDto>(Json);
        using var scope = f.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<Tamiron.Api.RequestContext>().TenantId = ApiFactory.ShopA;
        var v = await scope.ServiceProvider.GetRequiredService<TamironDbContext>().Vehicles.SingleAsync(x => x.Id == r!.VehicleId);
        Assert.Equal("Demir Nakliyat Ltd. Şti.", v.OwnerName);
    }
}
