using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Tamiron.Application.Suppliers;

namespace Tamiron.Api.Tests;

/// <summary>D1 · Parçacılar: ekle · listele (Türkçe alfabe) · düzenle · sil (soft); aynı numara iki kez olmaz.</summary>
public class SupplierApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    async Task<T> Read<T>(HttpResponseMessage res)
    {
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    [Fact]
    public async Task Add_list_update_delete()
    {
        var api = f.For(ApiFactory.ShopB);
        var oz = await Read<SupplierDto>(await api.PostAsJsonAsync("/api/suppliers", new { name = "Öz Yedek Parça", phone = "05321112233" }, Json));
        await Read<SupplierDto>(await api.PostAsJsonAsync("/api/suppliers", new { name = "Ahmet Oto", phone = "05334445566" }, Json));
        await Read<SupplierDto>(await api.PostAsJsonAsync("/api/suppliers", new { name = "Çınar Parça", phone = "05330000001" }, Json));
        await Read<SupplierDto>(await api.PostAsJsonAsync("/api/suppliers", new { name = "Zafer Oto", phone = "05330000002" }, Json));

        var list = await Read<List<SupplierDto>>(await api.GetAsync("/api/suppliers"));
        // "Zafer": ordinal sıralamada Ç'den ÖNCE gelirdi (Z=90 < Ç=199) — Türkçe alfabe farkını ayırt eder.
        var mine = new[] { "Ahmet Oto", "Çınar Parça", "Öz Yedek Parça", "Zafer Oto" };   // aynı sınıfın öteki testleri de B'ye yazıyor
        Assert.Equal(mine, list.Select(s => s.Name).Where(mine.Contains));

        var guncel = await Read<SupplierDto>(await api.PutAsJsonAsync($"/api/suppliers/{oz.Id}", new { name = "Öz Yedek", phone = "05321112244" }, Json));
        Assert.Equal(("Öz Yedek", "0532 111 22 44"), (guncel.Name, guncel.Phone));

        Assert.Equal(HttpStatusCode.NoContent, (await api.DeleteAsync($"/api/suppliers/{oz.Id}")).StatusCode);
        Assert.DoesNotContain(await Read<List<SupplierDto>>(await api.GetAsync("/api/suppliers")), s => s.Id == oz.Id);
    }

    [Fact]
    public async Task Same_phone_twice_is_rejected_but_freed_after_delete()
    {
        var api = f.For(ApiFactory.ShopB);
        var a = await Read<SupplierDto>(await api.PostAsJsonAsync("/api/suppliers", new { name = "Bir", phone = "05339990011" }, Json));
        var res = await api.PostAsJsonAsync("/api/suppliers", new { name = "İki", phone = "0533 999 00 11" }, Json);
        Assert.Equal("supplier.phone_taken", (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
        await api.DeleteAsync($"/api/suppliers/{a.Id}");
        (await api.PostAsJsonAsync("/api/suppliers", new { name = "İki", phone = "05339990011" }, Json)).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Another_shops_suppliers_are_invisible()
    {
        var b = await Read<SupplierDto>(await f.For(ApiFactory.ShopB).PostAsJsonAsync("/api/suppliers", new { name = "Gizli Parçacı", phone = "05339990022" }, Json));
        var a = f.For(ApiFactory.ShopA);
        Assert.DoesNotContain(await Read<List<SupplierDto>>(await a.GetAsync("/api/suppliers")), s => s.Id == b.Id);
        Assert.Equal(HttpStatusCode.NotFound, (await a.DeleteAsync($"/api/suppliers/{b.Id}")).StatusCode);
        // Aynı numara BAŞKA dükkânda serbest.
        (await a.PostAsJsonAsync("/api/suppliers", new { name = "Gizli Parçacı", phone = "05339990022" }, Json)).EnsureSuccessStatusCode();
    }
}
