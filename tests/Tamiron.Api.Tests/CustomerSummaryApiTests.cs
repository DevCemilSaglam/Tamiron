using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Infrastructure;
using Tamiron.Application.Summaries;
using Tamiron.Application.ServiceRecords;

namespace Tamiron.Api.Tests;

/// <summary>
/// D3 · Müşteri özet linki (K-023, K-076.3): usta link alır; müşteri OTURUMSUZ açar,
/// salt okunur görür. Alış fiyatı hiçbir yerde yok; kod tahmin edilemez.
/// </summary>
public class CustomerSummaryApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    static async Task<T> Read<T>(HttpResponseMessage res)
    {
        if (!res.IsSuccessStatusCode) throw new Exception($"{(int)res.StatusCode}: {await res.Content.ReadAsStringAsync()}");
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    async Task<(HttpClient api, ServiceRecordDto r)> Record(string plate)
    {
        var api = f.For(ApiFactory.ShopB);
        var r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate, complaint = "Fren sesi\nSilecek" }, Json));
        r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Part", name = "Balata", price = 1200, cost = 800 }, Json));
        r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Part", name = "Yağ", price = 900, suppliedBy = "Customer" }, Json));
        r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync($"/api/records/{r.Id}/lines", new { kind = "Labor", name = "İşçilik", price = 500 }, Json));
        return (api, r);
    }

    [Fact]
    public async Task Usta_gets_a_stable_link_and_customer_opens_it_without_login()
    {
        var (api, r) = await Record("34 OZ 01");
        var link = await Read<SummaryLinkDto>(await api.PostAsync($"/api/records/{r.Id}/summary-link", null));
        Assert.Equal(link, await Read<SummaryLinkDto>(await api.PostAsync($"/api/records/{r.Id}/summary-link", null)));   // aynı link
        Assert.Equal("tamiron.app/o/" + link.Code, link.Url);

        var musteri = f.CreateClient();   // Authorization YOK
        var res = await musteri.GetAsync($"/api/o/{link.Code}");
        var raw = await res.Content.ReadAsStringAsync();
        Assert.True(res.IsSuccessStatusCode, raw);
        Assert.DoesNotContain("800", raw);              // alış müşteriye gitmez
        Assert.DoesNotContain("cost", raw, StringComparison.OrdinalIgnoreCase);

        var s = JsonSerializer.Deserialize<CustomerSummaryDto>(raw, Json)!;
        Assert.Equal(("Öteki Dükkân", "34 OZ 01", "Tamirat"), (s.ShopName, s.PlateNo, s.TypeLabel));
        Assert.Equal("Fren sesi\nSilecek", s.Complaint);   // satır sonu korunur (K-038.1)
        Assert.Equal(new[] { "Balata", "Yağ", "İşçilik" }, s.Lines.Select(l => l.Label));
        Assert.Equal(new decimal?[] { 1200, null, 500 }, s.Lines.Select(l => l.Amount));
        Assert.True(s.Lines[1].CustomerBrought);
        Assert.Equal(1700m, s.Total);
        Assert.Equal("Aracınız serviste. Hazır olduğunda haber vereceğiz.", s.StatusText);
    }

    [Fact]
    public async Task Summary_is_live_deleted_lines_vanish_and_delivery_shows()
    {
        var (api, r) = await Record("34 OZ 02");
        var link = await Read<SummaryLinkDto>(await api.PostAsync($"/api/records/{r.Id}/summary-link", null));
        await api.DeleteAsync($"/api/records/{r.Id}/lines/{r.Lines[0].Id}");
        (await api.PostAsJsonAsync($"/api/records/{r.Id}/deliver", new { method = "Card" }, Json)).EnsureSuccessStatusCode();

        var s = await Read<CustomerSummaryDto>(await f.CreateClient().GetAsync($"/api/o/{link.Code}"));
        Assert.Equal(new[] { "Yağ", "İşçilik" }, s.Lines.Select(l => l.Label));
        Assert.Equal("Aracınız 22.10.2026 tarihinde teslim edildi. Ödeme: Kart.", s.StatusText);
    }

    [Fact]
    public async Task Unknown_code_is_404_and_another_shop_cannot_make_a_link()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await f.CreateClient().GetAsync("/api/o/abcdefghjkmnpqrs")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await f.For(ApiFactory.ShopA).PostAsync($"/api/records/{f.ShopBRecordId}/summary-link", null)).StatusCode);
    }

    [Fact]
    public async Task Deleted_record_link_is_dead()
    {
        var (api, r) = await Record("34 OZ 03");
        var link = await Read<SummaryLinkDto>(await api.PostAsync($"/api/records/{r.Id}/summary-link", null));
        // Kayıt silme ucu D13'te; burada doğrudan soft-delete.
        using (var scope = f.Services.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<RequestContext>().TenantId = ApiFactory.ShopB;
            var db = scope.ServiceProvider.GetRequiredService<TamironDbContext>();
            db.ServiceRecords.Remove(db.ServiceRecords.Single(x => x.Id == r.Id));
            await db.SaveChangesAsync();
        }
        Assert.Equal(HttpStatusCode.NotFound, (await f.CreateClient().GetAsync($"/api/o/{link.Code}")).StatusCode);
    }

    [Fact]
    public async Task Other_endpoints_still_need_a_session()
        => Assert.Equal(HttpStatusCode.Unauthorized, (await f.CreateClient().GetAsync("/api/records")).StatusCode);
}
