using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.ServiceRecords;
using Tamiron.Application.Vehicles;

namespace Tamiron.Api.Tests;

/// <summary>B6 · Araç: plakayla bul, alanları yaz (null = dokunma, "" = temizle), plaka değiştir.</summary>
public class VehicleApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    async Task<T> Read<T>(HttpResponseMessage res)
    {
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    async Task<VehicleDto> NewVehicle(HttpClient api, string plate)
    {
        var r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate, complaint = "" }, Json));
        return await Read<VehicleDto>(await api.GetAsync($"/api/vehicles/{r.VehicleId}"));
    }

    [Fact]
    public async Task Find_by_plate_normalizes_input()
    {
        var api = f.For(ApiFactory.ShopB);
        var v = await NewVehicle(api, "34 ARC 01");
        var found = await Read<VehicleDto>(await api.GetAsync("/api/vehicles?plate=34arc01"));
        Assert.Equal(v.Id, found.Id);
        Assert.Equal(HttpStatusCode.NotFound, (await api.GetAsync("/api/vehicles?plate=34ZZZ999")).StatusCode);
    }

    [Fact]
    public async Task Patch_normalizes_touches_only_given_and_reports_part_identity()
    {
        var api = f.For(ApiFactory.ShopB);
        var v = await NewVehicle(api, "34 ARC 02");
        Assert.False(v.HasPartIdentity);

        v = await Read<VehicleDto>(await api.PatchAsJsonAsync($"/api/vehicles/{v.Id}",
            new { brand = "Fiat", model = "Egea", year = "2021", ownerName = "Ali Usta", ownerPhone = "05321112233" }, Json));
        Assert.Equal("0532 111 22 33", v.OwnerPhone);
        Assert.Equal(new[] { "motor" }, v.MissingForParts);

        v = await Read<VehicleDto>(await api.PatchAsJsonAsync($"/api/vehicles/{v.Id}", new { engine = "1.4 Fire", note = "" }, Json));
        Assert.True(v.HasPartIdentity);
        Assert.Equal("Ali Usta", v.OwnerName);     // dokunulmadı
        Assert.Null(v.Note);                        // "" = temizle

        v = await Read<VehicleDto>(await api.PatchAsJsonAsync($"/api/vehicles/{v.Id}", new { firstRegistrationDate = "2021-03-02" }, Json));
        Assert.Equal(new DateOnly(2021, 3, 2), v.FirstRegistrationDate);
    }

    [Fact]
    public async Task Plate_change_requires_valid_and_free_plate_and_records_follow()
    {
        var api = f.For(ApiFactory.ShopB);
        var a = await NewVehicle(api, "34 ARC 03");
        await NewVehicle(api, "34 ARC 04");

        var taken = await api.PatchAsJsonAsync($"/api/vehicles/{a.Id}", new { plateNo = "34arc04" }, Json);
        Assert.Equal((HttpStatusCode)422, taken.StatusCode);
        Assert.Equal("plate.taken", (await taken.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());

        Assert.Equal((HttpStatusCode)422, (await api.PatchAsJsonAsync($"/api/vehicles/{a.Id}", new { plateNo = "34 A" }, Json)).StatusCode);

        var moved = await Read<VehicleDto>(await api.PatchAsJsonAsync($"/api/vehicles/{a.Id}", new { plateNo = "34 arc 99" }, Json));
        Assert.Equal("34 ARC 99", moved.PlateNo);
        // Kayıt araca kimlikle bağlı: plaka değişince kaydın plakası da değişir.
        var rec = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "34ARC99", complaint = "" }, Json));
        Assert.Equal(a.Id, rec.VehicleId);
    }

    [Fact]
    public async Task Invalid_phone_and_year_are_rejected_with_codes()
    {
        var api = f.For(ApiFactory.ShopB);
        var v = await NewVehicle(api, "34 ARC 05");
        var res = await api.PatchAsJsonAsync($"/api/vehicles/{v.Id}", new { ownerPhone = "532" }, Json);
        Assert.Equal("vehicle.phone_invalid", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
        res = await api.PatchAsJsonAsync($"/api/vehicles/{v.Id}", new { year = "1899" }, Json);
        Assert.Equal("vehicle.year_invalid", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task Another_shops_vehicle_is_404()
    {
        var v = await NewVehicle(f.For(ApiFactory.ShopB), "34 ARC 06");   // testin kendi aracı: sıraya bağlı değil
        var a = f.For(ApiFactory.ShopA);
        Assert.Equal(HttpStatusCode.NotFound, (await a.GetAsync("/api/vehicles?plate=34ARC06")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await a.GetAsync($"/api/vehicles/{v.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await a.PatchAsJsonAsync($"/api/vehicles/{v.Id}", new { note = "x" }, Json)).StatusCode);
    }
}
