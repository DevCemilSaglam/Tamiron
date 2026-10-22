using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.Lists;
using Tamiron.Application.ServiceRecords;

namespace Tamiron.Api.Tests;

/// <summary>
/// B7 · Listeler. Her test sınıfının kendi veritabanı var (IClassFixture), öteki
/// sınıfların kayıtları sayıları bozmaz; A dükkânına bu sınıfta YAZILMAZ.
/// </summary>
public class ListApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    async Task<T> Read<T>(HttpResponseMessage res)
    {
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    [Fact]
    public async Task Records_in_shop_first_then_newest_search_and_filter()
    {
        // A dükkânının tohumu: serviste 1 (triger), teslim 3 (ara bakım, silecek, akü).
        var api = f.For(ApiFactory.ShopA);
        var all = await Read<RecordList>(await api.GetAsync("/api/records"));
        Assert.Equal(1, all.InShopCount);
        Assert.Null(all.Items[0].DeliveredAt);                           // serviste olan üstte
        Assert.Equal("Triger seti değişecek", all.Items[0].Complaint);
        Assert.All(all.Items.Skip(1), i => Assert.NotNull(i.DeliveredAt));

        var bakim = await Read<RecordList>(await api.GetAsync("/api/records?q=bakim"));       // tür adında ve şikâyette
        Assert.Contains(bakim.Items, i => i.Complaint == "Ara bakım");
        var sahip = await Read<RecordList>(await api.GetAsync("/api/records?q=ayse"));        // sahipte, katlanarak
        Assert.Equal("35 KLM 45", Assert.Single(sahip.Items).PlateNo);

        var serviste = await Read<RecordList>(await api.GetAsync("/api/records?inShop=true"));
        Assert.All(serviste.Items, i => Assert.Null(i.DeliveredAt));
    }

    [Fact]
    public async Task Newest_first_among_same_state()
    {
        var api = f.For(ApiFactory.ShopB);
        f.Clock.Today = new DateOnly(2026, 10, 20);
        await api.PostAsJsonAsync("/api/records", new { plate = "34 LST 01", complaint = "eski iş" }, Json);
        f.Clock.Today = new DateOnly(2026, 10, 22);
        await api.PostAsJsonAsync("/api/records", new { plate = "34 LST 02", complaint = "yeni iş" }, Json);
        var l = await Read<RecordList>(await api.GetAsync("/api/records?q=iş"));
        Assert.Equal(new[] { "yeni iş", "eski iş" },
            l.Items.Where(i => i.PlateNo.StartsWith("34 LST")).Select(i => i.Complaint));   // tohumdaki "Gizli iş" karışmasın
        Assert.Equal(new DateOnly(2026, 10, 20), l.Items.Single(i => i.Complaint == "eski iş").OpenedOn);
    }

    [Fact]
    public async Task Vehicles_in_shop_first_and_search_in_contact_name()
    {
        var api = f.For(ApiFactory.ShopA);
        var l = await Read<VehicleList>(await api.GetAsync("/api/vehicles/list"));
        Assert.Equal("34 ABC 123", l.Items[0].PlateNo);                 // servisteki aracın sahibi Mehmet Kaya
        Assert.True(l.Items[0].InShop);
        Assert.All(l.Items.Skip(1), v => Assert.False(v.InShop));

        var hasan = await Read<VehicleList>(await api.GetAsync("/api/vehicles/list?q=hasan"));   // ikinci iletişim
        Assert.Equal("06 TMR 07", Assert.Single(hasan.Items).PlateNo);
    }

    [Fact]
    public async Task Record_carries_delivery_warnings()
    {
        var api = f.For(ApiFactory.ShopA);
        var triger = (await Read<RecordList>(await api.GetAsync("/api/records?inShop=true"))).Items[0];
        var r = await Read<ServiceRecordDto>(await api.GetAsync($"/api/records/{triger.Id}"));
        Assert.Contains("1 satırın tutarı yazılmamış", r.Warnings);
        Assert.Contains("parça hâlâ \"bekliyor\" işaretli", r.Warnings);
        Assert.DoesNotContain(r.Warnings, w => w.StartsWith("telefon"));   // Mehmet Kaya'nın telefonu var
    }
}
