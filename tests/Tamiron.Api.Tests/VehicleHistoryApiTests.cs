using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.Lists;
using Tamiron.Application.ServiceRecords;

namespace Tamiron.Api.Tests;

/// <summary>
/// G9 · Araç ekranının geçmişi (EKRANLAR §6): kayıtlar aracın KİMLİĞİYLE süzülür (plaka metin
/// aramasıyla değil — "06 AB 123" araması "06 AB 1234"ü de bulurdu); satırda km ve hatırlatma günü var
/// (kilometre ve bakım bölümleri bunlardan okunur).
/// </summary>
public class VehicleHistoryApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    static async Task<T> Read<T>(HttpResponseMessage res)
    {
        Assert.True(res.IsSuccessStatusCode, await res.Content.ReadAsStringAsync());
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    [Fact]
    public async Task Records_of_one_vehicle_only_with_km_and_reminder()
    {
        var api = f.For(await f.NewShopAsync());
        var a = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "06 AB 123", complaint = "Yağ bakımı", km = "50000" }, Json));
        await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{a.Id}/lines", new { kind = "Labor", name = "Bakım", price = 100 }, Json));
        await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{a.Id}/deliver", new { method = "Cash" }, Json));
        await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "06 AB 1234", complaint = "Fren" }, Json));

        var l = await Read<RecordList>(await api.GetAsync($"/api/records?vehicleId={a.VehicleId}"));
        var only = Assert.Single(l.Items);
        Assert.Equal(a.Id, only.Id);
        Assert.Equal("50.000", only.Km);
        Assert.NotNull(only.ReminderDate);
    }
}
