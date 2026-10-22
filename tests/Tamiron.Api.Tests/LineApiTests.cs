using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.ServiceRecords;
using Tamiron.Domain;

namespace Tamiron.Api.Tests;

/// <summary>B3 · Satır uçları: ekle · güncelle · sil (soft) · geri al.</summary>
public class LineApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    async Task<ServiceRecordDto> NewRecord(HttpClient api, string plate)
    {
        var res = await api.PostAsJsonAsync("/api/records", new { plate, complaint = "Fren" }, Json);
        return (await res.Content.ReadFromJsonAsync<ServiceRecordDto>(Json))!;
    }

    async Task<ServiceRecordDto> Send(Task<HttpResponseMessage> t)
    {
        var res = await t;
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<ServiceRecordDto>(Json))!;
    }

    [Fact]
    public async Task Add_update_delete_restore_keeps_order_and_total()
    {
        var api = f.For(ApiFactory.ShopB);
        var r = await NewRecord(api, "34 SAT 01");
        r = await Send(api.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Part", name = "Balata", price = 1200 }, Json));
        r = await Send(api.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Labor", name = "İşçilik", price = 500 }, Json));
        r = await Send(api.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Part", name = "Disk", price = 2000, cost = 0 }, Json));
        Assert.Equal(3700m, r.Total);
        Assert.Equal(new[] { "Balata", "İşçilik", "Disk" }, r.Lines.Select(l => l.Name));
        Assert.Equal(0m, r.Lines[2].Cost);        // 0 = bedava, null değil
        Assert.Null(r.Lines[0].Cost);             // yazılmadı = bilinmiyor

        var balata = r.Lines[0].Id;
        r = await Send(api.DeleteAsync($"/api/records/{r.Id}/lines/{balata}"));
        Assert.Equal(new[] { "İşçilik", "Disk" }, r.Lines.Select(l => l.Name));
        Assert.Equal(2500m, r.Total);

        r = await Send(api.PostAsync($"/api/records/{r.Id}/lines/{balata}/restore", null));
        Assert.Equal(new[] { "Balata", "İşçilik", "Disk" }, r.Lines.Select(l => l.Name));   // YERİNE döner

        // Müşteri getirdi: fiyat yazılı kalır, toplamdan düşer.
        r = await Send(api.PutAsJsonAsync($"/api/records/{r.Id}/lines/{balata}",
            new { kind = "Part", name = "Balata", price = 1200, suppliedBy = "Customer" }, Json));
        Assert.Equal(1200m, r.Lines[0].Price);
        Assert.False(r.Lines[0].CountsInTotal);
        Assert.Equal(2500m, r.Total);
    }

    [Fact]
    public async Task Nameless_line_is_rejected()
    {
        var api = f.For(ApiFactory.ShopB);
        var r = await NewRecord(api, "34 SAT 02");
        var res = await api.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Part", name = "", price = 10 }, Json);
        Assert.Equal((HttpStatusCode)422, res.StatusCode);
    }

    [Fact]
    public async Task Cannot_restore_or_delete_another_shops_line()
    {
        var b = f.For(ApiFactory.ShopB);
        var r = await NewRecord(b, "34 SAT 03");
        r = await Send(b.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Labor", name = "Gizli", price = 1 }, Json));
        var line = r.Lines[0].Id;
        await Send(b.DeleteAsync($"/api/records/{r.Id}/lines/{line}"));

        // Geri alma süzgeçsiz sorgu kullanıyor — kiracı denetimi orada da olmalı.
        var a = f.For(ApiFactory.ShopA);
        Assert.Equal(HttpStatusCode.NotFound, (await a.PostAsync($"/api/records/{r.Id}/lines/{line}/restore", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await a.DeleteAsync($"/api/records/{r.Id}/lines/{line}")).StatusCode);
    }
}
