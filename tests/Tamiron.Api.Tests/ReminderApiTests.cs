using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.Reminders;
using Tamiron.Application.ServiceRecords;

namespace Tamiron.Api.Tests;

/// <summary>B12 · Hatırlatması gelenler; "hatırlatma" mesajı (arama dahil) döngüyü kapatır.</summary>
public class ReminderApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    async Task<T> Read<T>(HttpResponseMessage res)
    {
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    async Task<ServiceRecordDto> DeliveredMaintenance(HttpClient api, string plate, DateOnly on)
    {
        f.Clock.Today = on;
        var r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate, complaint = "periyodik bakım" }, Json));
        await Read<Tamiron.Application.Vehicles.VehicleDto>(await api.PatchAsJsonAsync($"/api/vehicles/{r.VehicleId}",
            new { ownerName = "Ali " + plate, ownerPhone = "05320000099" }, Json));
        return await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/deliver", new { method = "Cash" }, Json));
    }

    [Fact]
    public async Task Due_list_oldest_first_and_call_closes_the_loop()
    {
        var api = f.For(ApiFactory.ShopB);
        var eski = await DeliveredMaintenance(api, "34 HTR 01", new DateOnly(2026, 2, 1));   // hatırlatma 01.08
        var yeni = await DeliveredMaintenance(api, "34 HTR 02", new DateOnly(2026, 4, 1));   // hatırlatma 01.10
        await DeliveredMaintenance(api, "34 HTR 03", new DateOnly(2026, 6, 1));              // hatırlatma 01.12 — henüz değil
        f.Clock.Today = new DateOnly(2026, 10, 22);

        var due = await Read<List<DueReminder>>(await api.GetAsync("/api/reminders/due"));
        Assert.Equal(new[] { "34 HTR 01", "34 HTR 02" }, due.Select(d => d.PlateNo));       // en çok bekleyen üstte
        Assert.Equal(new DateOnly(2026, 8, 1), due[0].ReminderDate);
        Assert.Equal(82, due[0].DaysOverdue);
        Assert.Equal("0532 000 00 99", due[0].Phone);

        // Usta aradı: arama da hatırlatma sayılır.
        await Read<Tamiron.Application.Messages.MessageDto>(await api.PostAsJsonAsync("/api/messages",
            new { vehicleId = eski.VehicleId, channel = "Call", template = "hatirlatma", body = "Telefonla söylendi." }, Json));
        due = await Read<List<DueReminder>>(await api.GetAsync("/api/reminders/due"));
        Assert.Equal("34 HTR 02", Assert.Single(due).PlateNo);
        Assert.True((await Read<ServiceRecordDto>(await api.GetAsync($"/api/records/{eski.Id}"))).ReminderSent);

        // Başka şablonlu mesaj hatırlatmayı kapatmaz.
        await Read<Tamiron.Application.Messages.MessageDto>(await api.PostAsJsonAsync("/api/messages",
            new { vehicleId = yeni.VehicleId, channel = "WhatsApp", template = "serbest", body = "Merhaba" }, Json));
        Assert.Single(await Read<List<DueReminder>>(await api.GetAsync("/api/reminders/due")));
    }
}
