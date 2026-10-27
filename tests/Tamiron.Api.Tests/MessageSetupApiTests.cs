using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.Messages;
using Tamiron.Application.ServiceRecords;

namespace Tamiron.Api.Tests;

/// <summary>
/// G9c · Mesaj ekranının kurulumu: kime gidebilir (aracı getiren kişi + ruhsat sahibi), varsayılan kim
/// (telefonu OLAN ilk kişi — A-M2) ve mesaj hangi işe bağlanır (açık kayıt, yoksa son kayıt — önerilerin
/// baktığı iş). Kural API'de; ekran yalnız gösterir (arayüz karar vermez).
/// </summary>
public class MessageSetupApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    static async Task<T> Read<T>(HttpResponseMessage res)
    {
        Assert.True(res.IsSuccessStatusCode, await res.Content.ReadAsStringAsync());
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    [Fact]
    public async Task Targets_default_to_the_first_with_a_phone_and_the_job_is_the_open_one()
    {
        var api = f.For(await f.NewShopAsync());
        var eski = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "34 MK 101", complaint = "Yağ" }, Json));
        await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{eski.Id}/lines", new { kind = "Labor", name = "Bakım", price = 100 }, Json));
        await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{eski.Id}/deliver", new { method = "Cash" }, Json));
        // İletişim adı var ama telefonu yok → varsayılan sahibi olur (telefonsuz hedef "neden gönderemiyorum" demek).
        await Read<JsonElement>(await api.PatchAsJsonAsync($"/api/vehicles/{eski.VehicleId}",
            new { ownerName = "Ayşe Yılmaz", ownerPhone = "05337770011", contactName = "Mehmet" }, Json));

        var s = await Read<MessageSetup>(await api.GetAsync($"/api/vehicles/{eski.VehicleId}/message-setup"));
        Assert.Equal(new[] { "Contact", "Owner" }, s.Targets.Select(t => t.Kind.ToString()));
        Assert.Equal("Owner", s.Targets.Single(t => t.IsDefault).Kind.ToString());
        Assert.Null(s.Targets[0].Phone);
        Assert.Equal(eski.Id, s.CurrentRecordId);   // teslim edilmiş tek iş → son kayıt

        var yeni = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "34 MK 101", complaint = "Fren" }, Json));
        s = await Read<MessageSetup>(await api.GetAsync($"/api/vehicles/{eski.VehicleId}/message-setup"));
        Assert.Equal(yeni.Id, s.CurrentRecordId);    // içerideyse açık kayıt
    }

    [Fact]
    public async Task Contact_with_a_phone_is_the_default_and_the_open_job_wins_over_a_later_delivered_one()
    {
        // Mutasyonla bulundu: varsayılan "hep sahibi" ve iş "hep son kayıt" ikisi de yeşil kalıyordu.
        var api = f.For(await f.NewShopAsync());
        var acik = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "34 MK 103", complaint = "Fren" }, Json));
        var sonra = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "34 MK 103", complaint = "Lastik" }, Json));
        await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{sonra.Id}/lines", new { kind = "Labor", name = "Lastik", price = 100 }, Json));
        await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{sonra.Id}/deliver", new { method = "Cash" }, Json));
        await Read<JsonElement>(await api.PatchAsJsonAsync($"/api/vehicles/{acik.VehicleId}",
            new { ownerName = "Ayşe Yılmaz", ownerPhone = "05337770011", contactName = "Mehmet", contactPhone = "05338880022" }, Json));

        var s = await Read<MessageSetup>(await api.GetAsync($"/api/vehicles/{acik.VehicleId}/message-setup"));
        Assert.Equal("Contact", s.Targets.Single(t => t.IsDefault).Kind.ToString());   // aracı getiren (A-M2)
        Assert.Equal(acik.Id, s.CurrentRecordId);   // içerideki iş, sonradan teslim edilen değil
    }

    [Fact]
    public async Task Vehicle_without_records_has_no_job()
    {
        var api = f.For(await f.NewShopAsync());
        var r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "34 MK 102" }, Json));
        await api.DeleteAsync($"/api/records/{r.Id}");
        var s = await Read<MessageSetup>(await api.GetAsync($"/api/vehicles/{r.VehicleId}/message-setup"));
        Assert.Null(s.CurrentRecordId);
        Assert.Equal("Owner", Assert.Single(s.Targets).Kind.ToString());
        Assert.True(s.Targets[0].IsDefault);
    }
}
