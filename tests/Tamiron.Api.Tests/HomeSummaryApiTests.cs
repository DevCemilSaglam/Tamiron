using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.Home;

namespace Tamiron.Api.Tests;

/// <summary>
/// C3 · Ana ekran özeti — prototip 10-home menü alt yazıları + çekmece rozeti.
/// Rakamlar ötekilerle AYNI kaynaktan: bugünkü tahsilat tahsil gününe göre (K-071.1),
/// veresiye bakiye, rozet hatırlatması gelenler.
/// </summary>
public class HomeSummaryApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    [Fact]
    public async Task Seed_numbers_then_collecting_moves_money_into_today()
    {
        var api = f.For(ApiFactory.ShopA);
        var s = (await api.GetFromJsonAsync<HomeSummary>("/api/home", Json))!;
        Assert.Equal((1, 4, 3, 3), (s.InShopCount, s.RecordCount, s.VehicleCount, s.CustomerCount));
        Assert.Equal((2800m, 3450m, 0), (s.CollectedToday, s.Receivable, s.DueReminderCount));

        // Veresiye bugün ödendi: bugünün kasasına girer, veresiyeden düşer (prototip bugunTahsilat).
        var veresiye = (await api.GetFromJsonAsync<Tamiron.Application.Accounting.ReceivableList>("/api/receivables", Json))!.Items.Single();
        (await api.PostAsJsonAsync($"/api/records/{veresiye.RecordId}/collect", new { method = "Cash" }, Json)).EnsureSuccessStatusCode();
        s = (await api.GetFromJsonAsync<HomeSummary>("/api/home", Json))!;
        Assert.Equal((6250m, 0m), (s.CollectedToday, s.Receivable));
    }

    [Fact]
    public async Task Customers_are_counted_not_vehicles()
    {
        // Tohumda araç ve müşteri sayısı tesadüfen eşit (3 = 3); ayırt eden durum: aynı sahibin
        // iki aracı ve sahipsiz araç. B dükkânı: tohumdaki sahipsiz "06 TMR 07" + iki araçlı Ali.
        var api = f.For(ApiFactory.ShopB);
        foreach (var plate in new[] { "34 MUS 01", "34 MUS 02" })
        {
            var r = await (await api.PostAsJsonAsync("/api/records", new { plate, complaint = "" }, Json))
                .Content.ReadFromJsonAsync<Tamiron.Application.ServiceRecords.ServiceRecordDto>(Json);
            (await api.PatchAsJsonAsync($"/api/vehicles/{r!.VehicleId}", new { ownerName = "Ali Usta", ownerPhone = "05321112233" }, Json)).EnsureSuccessStatusCode();
        }
        var s = (await api.GetFromJsonAsync<HomeSummary>("/api/home", Json))!;
        Assert.True(s.VehicleCount >= 3);
        Assert.Equal(1, s.CustomerCount);
    }

    [Fact]
    public async Task Badge_counts_due_reminders()
    {
        var api = f.For(ApiFactory.ShopB);
        f.Clock.Today = new DateOnly(2026, 2, 1);
        var r = await (await api.PostAsJsonAsync("/api/records", new { plate = "34 EV 01", complaint = "bakım" }, Json))
            .Content.ReadFromJsonAsync<Tamiron.Application.ServiceRecords.ServiceRecordDto>(Json);
        (await api.PostAsJsonAsync($"/api/records/{r!.Id}/deliver", new { method = "Cash" }, Json)).EnsureSuccessStatusCode();
        f.Clock.Today = new DateOnly(2026, 10, 22);
        Assert.Equal(1, (await api.GetFromJsonAsync<HomeSummary>("/api/home", Json))!.DueReminderCount);
    }
}
