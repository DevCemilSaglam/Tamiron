using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.ServiceRecords;
using Tamiron.Application.Vehicles;

namespace Tamiron.Api.Tests;

/// <summary>
/// G2 · Kayıt açma ekranının iki sorusu (EKRANLAR §2, prototip 20-ruhsat ruhsatDurum / kaydiAc):
/// (1) plaka kapının kendisi — geçerli mi, araç kayıtlı mı, açık kaydı var mı (karar API'de, K-079.2);
/// (2) kayıtlı olmayan aracın elle yazılan bilgileri kayıtla AYNI istekte yazılır — yarım kalmaz,
/// ve kayıtlı aracın bilgisi buradan EZİLMEZ (aynı araç iki kez yaratılmaz).
/// </summary>
public class OpenRecordFlowApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    static async Task<T> Read<T>(HttpResponseMessage res)
    {
        Assert.True(res.IsSuccessStatusCode, await res.Content.ReadAsStringAsync());
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    [Fact]
    public async Task Lookup_says_invalid_plate_without_searching()
    {
        var l = await Read<PlateLookup>(await f.For(ApiFactory.ShopB).GetAsync("/api/vehicles/lookup?plate=34ab"));
        Assert.False(l.Valid);
        Assert.Null(l.Vehicle);
        Assert.Equal("Plaka eksik — ör. 34 ABC 123", l.Hint);
    }

    [Fact]
    public async Task Lookup_unknown_vehicle_is_valid_and_normalized()
    {
        var l = await Read<PlateLookup>(await f.For(ApiFactory.ShopB).GetAsync("/api/vehicles/lookup?plate=06xyz77"));
        Assert.True(l.Valid);
        Assert.Equal("06 XYZ 77", l.Plate);
        Assert.Null(l.Vehicle);
        Assert.Null(l.OpenRecordId);
        Assert.Null(l.Hint);
    }

    [Fact]
    public async Task Lookup_registered_vehicle_points_to_its_open_record_only_while_in_shop()
    {
        var api = f.For(ApiFactory.ShopB);
        var r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "06 LUK 01", complaint = "Fren" }, Json));
        var l = await Read<PlateLookup>(await api.GetAsync("/api/vehicles/lookup?plate=06LUK01"));
        Assert.Equal(r.VehicleId, l.Vehicle!.Id);
        Assert.Equal(r.Id, l.OpenRecordId);

        await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/deliver", new { method = "Unpaid" }, Json));
        l = await Read<PlateLookup>(await api.GetAsync("/api/vehicles/lookup?plate=06LUK01"));
        Assert.NotNull(l.Vehicle);
        Assert.Null(l.OpenRecordId);
    }

    [Fact]
    public async Task Lookup_is_tenant_isolated()
    {
        await Read<ServiceRecordDto>(await f.For(ApiFactory.ShopB).PostAsJsonAsync("/api/records", new { plate = "06 KRC 02", complaint = "" }, Json));
        var l = await Read<PlateLookup>(await f.For(ApiFactory.ShopA).GetAsync("/api/vehicles/lookup?plate=06KRC02"));
        Assert.Null(l.Vehicle);
    }

    [Fact]
    public async Task New_vehicle_info_is_written_with_the_record_in_one_request()
    {
        var api = f.For(ApiFactory.ShopB);
        var r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new
        {
            plate = "06 YNI 03", complaint = "Yağ değişimi",
            vehicle = new { brand = "fiat", model = "Egea", year = "2019", vin = "zfa35600006h12345", ownerName = "Veli Kaya", ownerPhone = "05321112233" },
        }, Json));
        var v = await Read<VehicleDto>(await api.GetAsync($"/api/vehicles/{r.VehicleId}"));
        Assert.Equal(("fiat", "Egea", "2019", "Veli Kaya"), (v.Brand, v.Model, v.Year, v.OwnerName));
        Assert.Equal("0532 111 22 33", v.OwnerPhone);   // biçim domain'de (VehicleFields)
        Assert.Equal("ZFA35600006H12345", v.Vin);
    }

    [Fact]
    public async Task Bad_vehicle_info_rejects_the_whole_request_and_opens_nothing()
    {
        var api = f.For(ApiFactory.ShopB);
        var res = await api.PostAsJsonAsync("/api/records", new { plate = "06 YRM 04", complaint = "", vehicle = new { year = "1890" } }, Json);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, res.StatusCode);
        var l = await Read<PlateLookup>(await api.GetAsync("/api/vehicles/lookup?plate=06YRM04"));
        Assert.Null(l.Vehicle);   // araç da yaratılmadı
    }

    [Fact]
    public async Task Registered_vehicle_is_not_overwritten_from_the_open_screen()
    {
        var api = f.For(ApiFactory.ShopB);
        var r1 = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new
        { plate = "06 ESK 05", complaint = "", vehicle = new { brand = "Renault", ownerName = "Ayşe" } }, Json));
        await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r1.Id}/deliver", new { method = "Unpaid" }, Json));
        var r2 = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new
        { plate = "06ESK05", complaint = "", vehicle = new { brand = "Opel", ownerName = "Başkası" } }, Json));
        Assert.Equal(r1.VehicleId, r2.VehicleId);
        var v = await Read<VehicleDto>(await api.GetAsync($"/api/vehicles/{r2.VehicleId}"));
        Assert.Equal(("Renault", "Ayşe"), (v.Brand, v.OwnerName));
    }
}
