using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tamiron.Application.Customers;

namespace Tamiron.Api.Tests;

/// <summary>B10 · Müşteri listesi (serviste üstte), detay (anahtarla), ileti izni.</summary>
public class CustomerApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    async Task<T> Read<T>(HttpResponseMessage res)
    {
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    [Fact]
    public async Task List_puts_in_shop_first_then_alphabet_and_searches_plate()
    {
        var api = f.For(ApiFactory.ShopA);
        var l = await Read<CustomerList>(await api.GetAsync("/api/customers"));
        // Tohum: Mehmet Kaya (serviste — triger), sonra Ayşe Kaya, Demir Nakliyat.
        Assert.Equal(new[] { "Mehmet Kaya", "Ayşe Kaya", "Demir Nakliyat Ltd. Şti." }, l.Items.Select(i => i.Name));
        Assert.True(l.Items[0].InShop);
        Assert.Equal("34 ABC 123", l.Items[0].DisplayPlate);
        Assert.NotNull(l.Items[0].ConsentAt);                       // tohumda izni var (14.06)

        var ara = await Read<CustomerList>(await api.GetAsync("/api/customers?q=tmr"));
        Assert.Equal("Demir Nakliyat Ltd. Şti.", Assert.Single(ara.Items).Name);
    }

    [Fact]
    public async Task Consent_grant_and_revoke_date_from_server()
    {
        var api = f.For(ApiFactory.ShopA);
        var ayse = (await Read<CustomerList>(await api.GetAsync("/api/customers?q=ayse"))).Items.Single();
        Assert.Null(ayse.ConsentAt);

        var c = await Read<CustomerDetail>(await api.PutAsJsonAsync("/api/customers/consent", new { key = ayse.Key, granted = true }, Json));
        Assert.Equal(f.Clock.Today, c.ConsentAt);
        var v = await Read<Tamiron.Application.Vehicles.VehicleDto>(await api.GetAsync($"/api/vehicles/{c.Vehicles[0].Id}"));
        Assert.Equal(f.Clock.Today, v.MarketingConsentAt);         // araca yazıldı

        c = await Read<CustomerDetail>(await api.PutAsJsonAsync("/api/customers/consent", new { key = ayse.Key, granted = false }, Json));
        Assert.Null(c.ConsentAt);
    }

    [Fact]
    public async Task Detail_by_key_and_unknown_key_is_404()
    {
        var api = f.For(ApiFactory.ShopA);
        var mehmet = (await Read<CustomerList>(await api.GetAsync("/api/customers?q=mehmet"))).Items.Single();
        var d = await Read<CustomerDetail>(await api.GetAsync($"/api/customers/detail?key={Uri.EscapeDataString(mehmet.Key)}"));
        Assert.Equal("34 ABC 123", Assert.Single(d.Vehicles).PlateNo);
        Assert.Equal(HttpStatusCode.NotFound, (await api.GetAsync("/api/customers/detail?key=yok%7C0")).StatusCode);
        // Başka dükkânın müşterisi görünmez.
        Assert.Equal(HttpStatusCode.NotFound,
            (await f.For(ApiFactory.ShopB).GetAsync($"/api/customers/detail?key={Uri.EscapeDataString(mehmet.Key)}")).StatusCode);
    }
}
