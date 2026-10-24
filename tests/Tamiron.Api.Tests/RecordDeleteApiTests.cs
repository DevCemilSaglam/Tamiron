using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.Lists;
using Tamiron.Application.ServiceRecords;

namespace Tamiron.Api.Tests;

/// <summary>D13 · Kayıt sil (soft) + geri al. Teslim edilmiş kayıt silinmez; başka dükkânın kaydı 404.</summary>
public class RecordDeleteApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    static async Task<T> Read<T>(HttpResponseMessage res)
    {
        if (!res.IsSuccessStatusCode) throw new Exception($"{(int)res.StatusCode}: {await res.Content.ReadAsStringAsync()}");
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    [Fact]
    public async Task Delete_hides_the_record_everywhere_and_restore_brings_it_back_with_lines()
    {
        var api = f.For(ApiFactory.ShopB);
        var r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "34 SL 010", complaint = "Yanlış plaka" }, Json));
        r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Labor", name = "İşçilik", price = 500 }, Json));
        var before = await Read<RecordList>(await api.GetAsync("/api/records?inShop=true"));

        Assert.Equal(HttpStatusCode.NoContent, (await api.DeleteAsync($"/api/records/{r.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.GetAsync($"/api/records/{r.Id}")).StatusCode);
        var after = await Read<RecordList>(await api.GetAsync("/api/records?inShop=true"));
        Assert.Equal(before.InShopCount - 1, after.InShopCount);   // "serviste" sayısı yalan söylemez
        Assert.DoesNotContain(after.Items, i => i.Id == r.Id);

        var back = await Read<ServiceRecordDto>(await api.PostAsync($"/api/records/{r.Id}/restore", null));
        Assert.Equal(("34 SL 010", 500m), (back.PlateNo, back.Total));   // satırlarıyla döner
        Assert.Equal(before.InShopCount, (await Read<RecordList>(await api.GetAsync("/api/records?inShop=true"))).InShopCount);
    }

    [Fact]
    public async Task Delivered_record_is_not_deleted()
    {
        var api = f.For(ApiFactory.ShopB);
        var r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate = "34 SL 020", complaint = "Fren" }, Json));
        await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/deliver", new { method = "Cash" }, Json));
        var res = await api.DeleteAsync($"/api/records/{r.Id}");
        Assert.Equal(HttpStatusCode.UnprocessableEntity, res.StatusCode);
        Assert.Equal("record.delivered_cannot_delete", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
        Assert.Equal(HttpStatusCode.OK, (await api.GetAsync($"/api/records/{r.Id}")).StatusCode);
    }

    [Fact]
    public async Task Another_shops_record_cannot_be_deleted_or_restored()
    {
        var a = f.For(ApiFactory.ShopA);
        var b = f.For(ApiFactory.ShopB);
        var r = await Read<ServiceRecordDto>(await b.PostAsJsonAsync("/api/records", new { plate = "34 SL 030", complaint = "Fren" }, Json));
        Assert.Equal(HttpStatusCode.NotFound, (await a.DeleteAsync($"/api/records/{r.Id}")).StatusCode);
        await b.DeleteAsync($"/api/records/{r.Id}");
        Assert.Equal(HttpStatusCode.NotFound, (await a.PostAsync($"/api/records/{r.Id}/restore", null)).StatusCode);   // silinmişi de göremez
        // Yanıt 404 olsa da asıl soru: B'nin kaydı A'nın isteğiyle GERİ GELDİ Mİ? Gelmemeli.
        Assert.Equal(HttpStatusCode.NotFound, (await b.GetAsync($"/api/records/{r.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await b.PostAsync($"/api/records/{Guid.NewGuid()}/restore", null)).StatusCode);
    }
}
