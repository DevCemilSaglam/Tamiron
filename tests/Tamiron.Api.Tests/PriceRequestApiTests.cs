using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.PriceRequests;
using Tamiron.Application.ServiceRecords;
using Tamiron.Application.Suppliers;

namespace Tamiron.Api.Tests;

/// <summary>
/// D2 · Parça sorma: K-068 kapısı · parçacı başına ayrı soru (Kol B) · cevap ham metniyle
/// saklanır · cevap satıra işlenir ve "kimden geldi" işaretlenir.
/// </summary>
public class PriceRequestApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    static async Task<T> Read<T>(HttpResponseMessage res)
    {
        if (!res.IsSuccessStatusCode) throw new Exception($"{(int)res.StatusCode}: {await res.Content.ReadAsStringAsync()}");
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    static async Task<string?> Code(HttpResponseMessage res)
        => (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString();

    /// <summary>Kimliği tamam bir araçta, fiyatı belli olmayan iki parçalı kayıt.</summary>
    async Task<(HttpClient api, ServiceRecordDto r)> RecordWithParts(string plate, bool identity = true)
    {
        var api = f.For(ApiFactory.ShopB);
        var r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate, complaint = "Fren" }, Json));
        if (identity)
            await Read<JsonElement>(await api.PatchAsJsonAsync($"/api/vehicles/{r.VehicleId}",
                new { brand = "Fiat", model = "Egea", year = "2021", engine = "1.4 Fire" }, Json));
        r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Part", name = "Balata", price = 0 }, Json));
        r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Part", name = "Disk", price = 0 }, Json));
        r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Labor", name = "İşçilik", price = 500 }, Json));
        return (api, r);
    }

    async Task<SupplierDto> Supplier(HttpClient api, string name, string phone)
        => await Read<SupplierDto>(await api.PostAsJsonAsync("/api/suppliers", new { name, phone }, Json));

    [Fact]
    public async Task Asking_two_suppliers_makes_two_requests_with_the_same_text()
    {
        var (api, r) = await RecordWithParts("34 PS 01");
        var oz = await Supplier(api, "Öz Parça", "05331000001");
        var can = await Supplier(api, "Can Oto", "05331000002");

        var sent = await Read<List<PriceRequestDto>>(await api.PostAsJsonAsync($"/api/records/{r.Id}/price-requests",
            new { supplierIds = new[] { oz.Id, can.Id } }, Json));

        Assert.Equal(new[] { "Öz Parça", "Can Oto" }, sent.Select(s => s.SupplierName));
        Assert.Equal("0533 100 00 01", sent[0].SupplierPhone);   // WhatsApp bu numaraya açılır
        Assert.All(sent, s => Assert.Equal("Fiat Egea 2021 · 1.4 Fire\n34 PS 01\n\n· Balata\n· Disk\n\nFiyat verebilir misiniz?", s.SentText));
        Assert.All(sent, s => Assert.Equal(new[] { "Balata", "Disk" }, s.Lines.Select(l => l.Name)));   // işçilik sorulmaz

        var list = await Read<List<PriceRequestDto>>(await api.GetAsync($"/api/records/{r.Id}/price-requests"));
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task Without_vehicle_identity_nothing_is_sent()
    {
        var (api, r) = await RecordWithParts("34 PS 02", identity: false);
        var p = await Supplier(api, "Kimliksiz", "05331000003");
        var res = await api.PostAsJsonAsync($"/api/records/{r.Id}/price-requests", new { supplierIds = new[] { p.Id } }, Json);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, res.StatusCode);
        Assert.Equal("price_request.vehicle_identity", await Code(res));
        Assert.Empty(await Read<List<PriceRequestDto>>(await api.GetAsync($"/api/records/{r.Id}/price-requests")));
    }

    [Fact]
    public async Task No_supplier_chosen_is_a_speaking_error()
    {
        var (api, r) = await RecordWithParts("34 PS 03");
        var res = await api.PostAsJsonAsync($"/api/records/{r.Id}/price-requests", new { supplierIds = Array.Empty<Guid>() }, Json);
        Assert.Equal("price_request.no_supplier", await Code(res));
    }

    [Fact]
    public async Task Reply_is_kept_raw_and_applied_to_the_line()
    {
        var (api, r) = await RecordWithParts("34 PS 04");
        var oz = await Supplier(api, "Öz Cevap", "05331000004");
        var can = await Supplier(api, "Can Cevap", "05331000005");
        var sent = await Read<List<PriceRequestDto>>(await api.PostAsJsonAsync($"/api/records/{r.Id}/price-requests",
            new { supplierIds = new[] { oz.Id, can.Id } }, Json));
        var balata = r.Lines.First(l => l.Name == "Balata").Id;

        // Can'ın cevabı kullanılıyor: alış 850, müşteriye 1200.
        var rec = await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/price-requests/{sent[1].Id}/replies",
            new { lineId = balata, rawText = "balata bosch 850", price = 850, brand = "Bosch", customerPrice = 1200 }, Json));
        var line = rec.Lines.First(l => l.Id == balata);
        Assert.Equal((850m, 1200m), (line.Cost!.Value, line.Price));

        var list = await Read<List<PriceRequestDto>>(await api.GetAsync($"/api/records/{r.Id}/price-requests"));
        var reply = Assert.Single(list.First(x => x.Id == sent[1].Id).Replies);
        Assert.Equal(("balata bosch 850", 850m), (reply.RawText, reply.ParsedPrice!.Value));
        // "Cevap kimden geldi" satırda: seçili cevap Can'ınki.
        Assert.Equal(reply.Id, list.SelectMany(x => x.Lines).First(l => l.Id == balata).SelectedReplyId);
    }

    [Fact]
    public async Task Reply_to_a_line_that_was_not_asked_is_rejected()
    {
        var (api, r) = await RecordWithParts("34 PS 05");
        var p = await Supplier(api, "Sorulmayan", "05331000006");
        var sent = await Read<List<PriceRequestDto>>(await api.PostAsJsonAsync($"/api/records/{r.Id}/price-requests",
            new { supplierIds = new[] { p.Id } }, Json));
        var iscilik = r.Lines.First(l => l.Name == "İşçilik").Id;
        var res = await api.PostAsJsonAsync($"/api/price-requests/{sent[0].Id}/replies", new { lineId = iscilik, rawText = "500", price = 500 }, Json);
        Assert.Equal("reply.line_not_asked", await Code(res));
    }

    [Fact]
    public async Task Another_shops_supplier_and_request_are_invisible()
    {
        var (api, r) = await RecordWithParts("34 PS 06");
        var a = f.For(ApiFactory.ShopA);
        var yabanci = await Read<SupplierDto>(await a.PostAsJsonAsync("/api/suppliers", new { name = "A'nın parçacısı", phone = "05331000007" }, Json));
        var res = await api.PostAsJsonAsync($"/api/records/{r.Id}/price-requests", new { supplierIds = new[] { yabanci.Id } }, Json);
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);

        var p = await Supplier(api, "B'nin", "05331000008");
        var sent = await Read<List<PriceRequestDto>>(await api.PostAsJsonAsync($"/api/records/{r.Id}/price-requests", new { supplierIds = new[] { p.Id } }, Json));
        var res2 = await a.PostAsJsonAsync($"/api/price-requests/{sent[0].Id}/replies", new { lineId = r.Lines[0].Id, rawText = "x" }, Json);
        Assert.Equal(HttpStatusCode.NotFound, res2.StatusCode);
    }
}
