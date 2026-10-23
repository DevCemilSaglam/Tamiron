using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.Customers;
using Tamiron.Application.ServiceRecords;

namespace Tamiron.Api.Tests;

/// <summary>D5 · İzin geçmişi: her değişiklik izde; "geri al" eski tarihi İZDEN getirir (K-073.10 kapandı).</summary>
public class ConsentHistoryApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    static async Task<T> Read<T>(HttpResponseMessage res)
    {
        if (!res.IsSuccessStatusCode) throw new Exception($"{(int)res.StatusCode}: {await res.Content.ReadAsStringAsync()}");
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    async Task<(HttpClient api, string key)> Customer(string plate, string name, string phone)
    {
        var api = f.For(ApiFactory.ShopB);
        var r = await Read<ServiceRecordDto>(await api.PostAsJsonAsync("/api/records", new { plate, complaint = "Fren" }, Json));
        await Read<JsonElement>(await api.PatchAsJsonAsync($"/api/vehicles/{r.VehicleId}", new { ownerName = name, ownerPhone = phone }, Json));
        var list = await Read<CustomerList>(await api.GetAsync($"/api/customers?q={Uri.EscapeDataString(name)}"));
        return (api, list.Items.Single().Key);
    }

    [Fact]
    public async Task Revoke_then_undo_brings_back_the_original_date()
    {
        var (api, key) = await Customer("34 IZ 01", "Zeynep Ak", "05339991101");
        var today = f.Clock.Today;
        f.Clock.Today = new DateOnly(2026, 6, 14);   // izin haziranda verildi
        try { await Read<CustomerDetail>(await api.PutAsJsonAsync("/api/customers/consent", new { key, granted = true }, Json)); }
        finally { f.Clock.Today = today; }

        var c = await Read<CustomerDetail>(await api.PutAsJsonAsync("/api/customers/consent", new { key, granted = false }, Json));
        Assert.Null(c.ConsentAt);
        c = await Read<CustomerDetail>(await api.PostAsJsonAsync("/api/customers/consent/undo", new { key }, Json));
        Assert.Equal(new DateOnly(2026, 6, 14), c.ConsentAt);   // bugün DEĞİL

        var h = await Read<List<ConsentEventDto>>(await api.GetAsync($"/api/customers/consent-history?key={Uri.EscapeDataString(key)}"));
        Assert.Equal(new[] { (true, true), (false, false), (true, false) }, h.Select(e => (e.Granted, e.IsUndo)));   // en yeni üstte
        Assert.All(h, e => Assert.Equal(("34 IZ 01", "0533 999 11 01"), (e.PlateNo, e.Phone)));
        Assert.All(h, e => Assert.Equal("0532 000 00 02", e.By));   // kim yaptı (adı yok: telefonu)
    }

    [Fact]
    public async Task Undo_twice_is_a_speaking_error()
    {
        var (api, key) = await Customer("34 IZ 02", "Mert Su", "05339991102");
        await Read<CustomerDetail>(await api.PutAsJsonAsync("/api/customers/consent", new { key, granted = true }, Json));
        await Read<CustomerDetail>(await api.PostAsJsonAsync("/api/customers/consent/undo", new { key }, Json));
        var res = await api.PostAsJsonAsync("/api/customers/consent/undo", new { key }, Json);
        Assert.Equal("consent.nothing_to_undo", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
    }
}
