using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.Messages;
using Tamiron.Application.ServiceRecords;
using Tamiron.Application.Summaries;

namespace Tamiron.Api.Tests;

/// <summary>D4 · Mesaj önerileri: işin hâline göre şablon listesi + metin; özet linki özet sayfasının linkiyle aynı.</summary>
public class MessageSuggestionApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    static async Task<T> Read<T>(HttpResponseMessage res)
    {
        if (!res.IsSuccessStatusCode) throw new Exception($"{(int)res.StatusCode}: {await res.Content.ReadAsStringAsync()}");
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    [Fact]
    public async Task Suggestions_follow_the_job_from_open_to_delivered()
    {
        var api = f.For(ApiFactory.ShopB);
        var r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "34 MS 01", complaint = "Fren" }, Json));
        await Read<JsonElement>(await api.PatchAsJsonAsync($"/api/vehicles/{r.VehicleId}",
            new { brand = "Fiat", model = "Egea", ownerName = "Ayşe Yılmaz", ownerPhone = "05337770011" }, Json));
        r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Labor", name = "İşçilik", price = 1500 }, Json));

        var acik = await Read<List<MessageSuggestion>>(await api.GetAsync($"/api/vehicles/{r.VehicleId}/message-suggestions"));
        Assert.Equal(new[] { "ozet", "hazir", "serbest" }, acik.Select(s => s.Template));
        var link = await Read<SummaryLinkDto>(await api.PostAsync($"/api/records/{r.Id}/summary-link", null));
        Assert.Equal("Ayşe Yılmaz Bey/Hanım, Fiat Egea (34 MS 01) için 1 kalem işlem yapıldı.\nToplam: 1.500 ₺\n\n" +
                     "Ayrıntılı özet: " + link.Url + "\n\nÖteki Dükkân\n", acik[0].Text);   // aynı kod: öneri kodu KAYDETTİ

        // Özet gönderildi → bir daha önerilmez.
        await Read<MessageDto>(await api.PostAsJsonAsync("/api/messages",
            new { vehicleId = r.VehicleId, serviceRecordId = r.Id, channel = "WhatsApp", template = "ozet", body = acik[0].Text }, Json));
        Assert.Equal(new[] { "hazir", "serbest" },
            (await Read<List<MessageSuggestion>>(await api.GetAsync($"/api/vehicles/{r.VehicleId}/message-suggestions"))).Select(s => s.Template));

        // Teslim: bu işe mesaj gitmişti → "teslim edildi" önerilmez (prototip: HERHANGİ bir mesaj).
        await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/deliver", new { method = "Cash" }, Json));
        Assert.Equal(new[] { "serbest" },
            (await Read<List<MessageSuggestion>>(await api.GetAsync($"/api/vehicles/{r.VehicleId}/message-suggestions"))).Select(s => s.Template));
    }

    [Fact]
    public async Task Delivered_job_without_message_suggests_delivered()
    {
        var api = f.For(ApiFactory.ShopB);
        var r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "34 MS 02", complaint = "Fren" }, Json));
        await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/deliver", new { method = "Cash" }, Json));
        var s = await Read<List<MessageSuggestion>>(await api.GetAsync($"/api/vehicles/{r.VehicleId}/message-suggestions"));
        Assert.Equal(new[] { "teslim", "serbest" }, s.Select(x => x.Template));
        Assert.Equal("Teslim edildi", s[0].Name);
    }

    /// <summary>Koşullar KAYIT bazlı (14.09): aynı aracın öteki kaydı bu işin önerisini bozmaz.</summary>
    [Fact]
    public async Task Another_record_of_the_same_vehicle_does_not_leak()
    {
        var api = f.For(ApiFactory.ShopB);
        // Eski iş: özeti gitti, teslim edildi. Yeni iş açık → özet YİNE önerilir.
        var a = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "34 MS 04", complaint = "Fren" }, Json));
        await Read<JsonElement>(await api.PatchAsJsonAsync($"/api/vehicles/{a.VehicleId}", new { ownerPhone = "05337770044" }, Json));
        await Read<MessageDto>(await api.PostAsJsonAsync("/api/messages",
            new { vehicleId = a.VehicleId, serviceRecordId = a.Id, channel = "WhatsApp", template = "ozet", body = "x" }, Json));
        await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{a.Id}/deliver", new { method = "Cash" }, Json));
        await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "34 MS 04", complaint = "Debriyaj" }, Json));
        Assert.Equal(new[] { "ozet", "hazir", "serbest" },
            (await Read<List<MessageSuggestion>>(await api.GetAsync($"/api/vehicles/{a.VehicleId}/message-suggestions"))).Select(s => s.Template));

        // Açık iş, sonradan açılıp teslim edilmiş bir işten ÖNCE gelir: şu anki iş açık olandır.
        var c = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "34 MS 05", complaint = "Fren" }, Json));
        var d = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "34 MS 05", complaint = "Lastik" }, Json));
        Assert.NotEqual(c.Id, d.Id);
        await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{d.Id}/deliver", new { method = "Cash" }, Json));
        Assert.Equal(new[] { "ozet", "hazir", "serbest" },
            (await Read<List<MessageSuggestion>>(await api.GetAsync($"/api/vehicles/{c.VehicleId}/message-suggestions"))).Select(s => s.Template));
    }

    [Fact]
    public async Task Ready_made_text_by_name_and_unknown_is_422()
    {
        var api = f.For(ApiFactory.ShopB);
        var r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "34 MS 03", complaint = "Fren" }, Json));
        var b = await Read<MessageSuggestion>(await api.GetAsync($"/api/vehicles/{r.VehicleId}/message-text?template=bayram"));
        Assert.Equal(("Bayram mesajı", "Merhaba, Bayramınız mübarek olsun. Sağlıklı, huzurlu ve kazasız günler dileriz.\n\nÖteki Dükkân\n"), (b.Name, b.Text));
        var res = await api.GetAsync($"/api/vehicles/{r.VehicleId}/message-text?template=yok");
        Assert.Equal("message.unknown_template", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
    }
}
