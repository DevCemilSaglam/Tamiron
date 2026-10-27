using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Application.Customers;
using Tamiron.Application.PriceRequests;
using Tamiron.Application.ServiceRecords;
using Tamiron.Application.Suppliers;
using Tamiron.Infrastructure;

namespace Tamiron.Api.Tests;

/// <summary>
/// Dükkânlar arası yalıtım — SİSTEMATİK (Cemil, 23.10: "Başka dükkânın herhangi bir kaydı vb var mı,
/// herhangi bir işlem yapılabiliyor. İyice kontrol ettin mi?").
///
/// Uç uç yazılmış testler yamalıydı: deliver, collect, satır ekle/düzelt, mesaj önerisi, teslimi geri
/// al, özet linki… için "başka dükkân" testi yoktu. Bu test:
///   1. B dükkânında her türden veri kurar (açık + teslimli kayıt, satır, araç, parçacı, gider,
///      parça sorusu, mesaj, müşteri izni) ve B'nin BÜTÜN satırlarının anlık görüntüsünü alır
///      (RowVersion: herhangi bir güncelleme onu değiştirir);
///   2. API'nin kimlikli HER ucunu A'nın oturumuyla, B'nin kimlikleriyle, geçerli bir gövdeyle çağırır;
///   3. hiçbir isteğin başarılı olmadığını, B'nin hiçbir satırının değişmediğini ve B'nin gizli
///      işaretinin A'nın hiçbir yanıtında (listeler dahil) görünmediğini ölçer.
/// Yeni bir kimlikli uç eklenip buraya yazılmazsa test KIRMIZI — kapsam kendiliğinden eksilmez.
/// </summary>
public class TenantIsolationTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };
    const string Mark = "GIZLIB";

    static async Task<T> Read<T>(HttpResponseMessage res)
    {
        if (!res.IsSuccessStatusCode) throw new Exception($"{(int)res.StatusCode}: {await res.Content.ReadAsStringAsync()}");
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    sealed record BData(Guid OpenRecord, Guid DeliveredRecord, Guid Line, Guid Vehicle, Guid Supplier, Guid Expense,
                        Guid PriceRequest, string CustomerKey, Guid Advance);

    async Task<BData> SeedB()
    {
        var b = f.For(ApiFactory.ShopB);
        var open = await Read<ServiceRecordDto>(await b.PostAsJsonAsync("/api/records", new { plate = "06 GZL 99", complaint = Mark + " açık iş" }, Json));
        await Read<JsonElement>(await b.PatchAsJsonAsync($"/api/vehicles/{open.VehicleId}", new
        {
            brand = "Fiat", model = "Egea", year = "2021", engine = "1.4", ownerName = Mark + " Sahip", ownerPhone = "05337001122", note = Mark
        }, Json));
        open = await Read<ServiceRecordDto>(await b.PostAsJsonAsync($"/api/records/{open.Id}/lines", new { kind = "Part", name = Mark + " balata", price = 0 }, Json));
        var sup = await Read<SupplierDto>(await b.PostAsJsonAsync("/api/suppliers", new { name = Mark + " Parçacı", phone = "05337001133" }, Json));
        var pr = await Read<List<PriceRequestDto>>(await b.PostAsJsonAsync($"/api/records/{open.Id}/price-requests", new { supplierIds = new[] { sup.Id } }, Json));
        var exp = await Read<JsonElement>(await b.PostAsJsonAsync("/api/expenses", new { description = Mark + " gider", amount = 777 }, Json));
        await Read<JsonElement>(await b.PostAsJsonAsync("/api/messages", new { vehicleId = open.VehicleId, serviceRecordId = open.Id, channel = "WhatsApp", template = "serbest", body = Mark + " mesaj" }, Json));

        var done = await Read<ServiceRecordDto>(await b.PostAsJsonAsync("/api/records", new { plate = "06 GZL 98", complaint = Mark + " teslimli" }, Json));
        done = await Read<ServiceRecordDto>(await b.PostAsJsonAsync($"/api/records/{done.Id}/lines", new { kind = "Labor", name = Mark, price = 900 }, Json));
        await Read<ServiceRecordDto>(await b.PostAsJsonAsync($"/api/records/{done.Id}/deliver", new { method = "Unpaid" }, Json));

        // G7 · avans: B'nin aracında bakiye 500 (A iade etse, geri alsa ya da avanstan düşse görüntü değişir).
        var adv = await Read<JsonElement>(await b.PostAsJsonAsync($"/api/vehicles/{open.VehicleId}/advance", new { amount = 500, method = "Cash" }, Json));
        var key = (await Read<CustomerList>(await b.GetAsync("/api/customers?q=" + Mark))).Items.Single().Key;
        await Read<CustomerDetail>(await b.PutAsJsonAsync("/api/customers/consent", new { key, granted = true }, Json));
        return new(open.Id, done.Id, open.Lines[0].Id, open.VehicleId, sup.Id, exp.GetProperty("id").GetGuid(),
                   pr[0].Id, key, adv.GetProperty("items")[0].GetProperty("id").GetGuid());
    }

    /// <summary>B'nin bütün satırları: kimlik + RowVersion + silinme. Bir alan bile değişirse görüntü değişir.</summary>
    async Task<string> Snapshot()
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TamironDbContext>();
        var b = ApiFactory.ShopB;
        var sb = new StringBuilder();
        async Task Add<T>(string name, IQueryable<T> q) where T : Tamiron.Domain.TenantEntity
        {
            var rows = await q.IgnoreQueryFilters().Where(x => x.TenantId == b).OrderBy(x => x.Id)
                .Select(x => new { x.Id, x.RowVersion, x.DateTimeDeleted }).ToListAsync();
            sb.Append(name).Append(':').Append(rows.Count).Append('\n');
            foreach (var r in rows) sb.Append(r.Id).Append(' ').Append(Convert.ToBase64String(r.RowVersion)).Append(' ').Append(r.DateTimeDeleted).Append('\n');
        }
        await Add("records", db.ServiceRecords);
        await Add("lines", db.ServiceLines);
        await Add("vehicles", db.Vehicles);
        await Add("expenses", db.Expenses);
        await Add("suppliers", db.Suppliers);
        await Add("price-requests", db.PriceRequests);
        await Add("price-replies", db.PriceReplies);
        await Add("messages", db.Messages);
        await Add("consent", db.ConsentEvents);
        await Add("advances", db.Advances);
        await Add("payments", db.Payments);
        return sb.ToString();
    }

    /// <summary>Kimlikli her uç: yol şablonu → (gövde). Gövde "kendi verisi olsaydı geçerli" olacak biçimde.</summary>
    static Dictionary<string, Func<BData, object?>> Bodies() => new()
    {
        ["GET /api/records/{id}"] = _ => null,
        ["DELETE /api/records/{id}"] = _ => null,
        ["POST /api/records/{id}/restore"] = _ => null,
        ["PATCH /api/records/{id}"] = _ => new { complaint = "A yazdı" },
        ["GET /api/records/{id}/delivery-preview"] = _ => null,
        ["POST /api/records/{id}/deliver"] = _ => new { method = "Cash" },
        ["POST /api/records/{id}/deliver/undo"] = _ => null,
        ["POST /api/records/{id}/payment"] = _ => new { method = "Card" },
        ["POST /api/records/{id}/collect"] = _ => new { method = "Cash" },
        ["POST /api/records/{id}/collect/undo"] = _ => null,
        ["POST /api/records/{id}/lines"] = _ => new { kind = "Labor", name = "A satırı", price = 1 },
        ["PUT /api/records/{id}/lines/{lineId}"] = _ => new { kind = "Labor", name = "A düzeltti", price = 1 },
        ["DELETE /api/records/{id}/lines/{lineId}"] = _ => null,
        ["POST /api/records/{id}/lines/{lineId}/restore"] = _ => null,
        ["POST /api/records/{id}/summary-link"] = _ => null,
        ["POST /api/records/{id}/price-requests"] = d => new { supplierIds = new[] { d.Supplier } },
        ["GET /api/records/{id}/price-requests"] = _ => null,
        ["POST /api/price-requests/{id}/replies"] = d => new { lineId = d.Line, rawText = "A", price = 5 },
        ["GET /api/vehicles/{id}"] = _ => null,
        ["PATCH /api/vehicles/{id}"] = _ => new { note = "A yazdı" },
        ["GET /api/vehicles/{id}/message-suggestions"] = _ => null,
        ["GET /api/vehicles/{id}/message-text"] = _ => null,
        ["PUT /api/suppliers/{id}"] = _ => new { name = "A", phone = "05337009999" },
        ["DELETE /api/suppliers/{id}"] = _ => null,
        ["GET /api/vehicles/{id}/advance"] = _ => null,
        ["POST /api/vehicles/{id}/advance"] = _ => new { amount = 1, method = "Cash" },
        ["POST /api/vehicles/{id}/advance/refund"] = _ => new { amount = 1, method = "Cash" },
        ["POST /api/advances/{id}/undo"] = _ => null,
        ["DELETE /api/expenses/{id}"] = _ => null,
        ["POST /api/expenses/{id}/restore"] = _ => null,
    };

    static IEnumerable<Guid> IdsFor(string route, BData d) => route switch
    {
        _ when route.Contains("/records/") => [d.OpenRecord, d.DeliveredRecord],
        _ when route.Contains("/vehicles/") => [d.Vehicle],
        _ when route.Contains("/suppliers/") => [d.Supplier],
        _ when route.Contains("/expenses/") => [d.Expense],
        _ when route.Contains("/price-requests/") => [d.PriceRequest],
        _ when route.Contains("/advances/") => [d.Advance],
        _ => throw new InvalidOperationException(route),
    };

    [Fact]
    public async Task Shop_A_can_neither_see_nor_touch_anything_of_shop_B()
    {
        var d = await SeedB();
        var before = await Snapshot();
        var a = f.For(ApiFactory.ShopA);
        var failures = new List<string>();

        async Task Call(string label, HttpMethod m, string url, object? body)
        {
            using var req = new HttpRequestMessage(m, url);
            if (body is not null) req.Content = JsonContent.Create(body, options: Json);
            var res = await a.SendAsync(req);
            var text = await res.Content.ReadAsStringAsync();
            if (res.IsSuccessStatusCode) failures.Add($"{label} → {(int)res.StatusCode} (başarılı olmamalıydı)");
            if (text.Contains(Mark)) failures.Add($"{label} → yanıtta B'nin verisi");
        }

        // 1 · Yolunda kimlik taşıyan HER uç.
        foreach (var (route, body) in Bodies())
        {
            var sp = route.IndexOf(' ');
            var method = new HttpMethod(route[..sp]);
            foreach (var id in IdsFor(route, d))
            {
                var url = route[(sp + 1)..].Replace("{id}", id.ToString()).Replace("{lineId}", d.Line.ToString());
                if (url.EndsWith("/message-text")) url += "?template=bayram";
                await Call($"{route} [{id}]", method, url, body(d));
            }
        }

        // 2 · Kimliği gövdede / sorguda taşıyanlar.
        var key = Uri.EscapeDataString(d.CustomerKey);
        await Call("POST /api/messages (B aracı)", HttpMethod.Post, "/api/messages",
            new { vehicleId = d.Vehicle, channel = "WhatsApp", template = "serbest", body = "A" });
        await Call("GET /api/customers/detail (B anahtarı)", HttpMethod.Get, $"/api/customers/detail?key={key}", null);
        await Call("PUT /api/customers/consent (B anahtarı)", HttpMethod.Put, "/api/customers/consent", new { key = d.CustomerKey, granted = false });
        await Call("POST /api/customers/consent/undo (B anahtarı)", HttpMethod.Post, "/api/customers/consent/undo", new { key = d.CustomerKey });
        await Call("GET /api/customers/consent-history (B anahtarı)", HttpMethod.Get, $"/api/customers/consent-history?key={key}", null);

        // 3 · A'nın listeleri B'den hiçbir şey göstermez (başarılı olmaları beklenir — yalnız sızıntıya bakılır).
        foreach (var url in new[]
        {
            "/api/records", "/api/records?inShop=true", $"/api/records?q={Mark}", "/api/vehicles/list", $"/api/vehicles/list?q={Mark}",
            "/api/customers", $"/api/customers?q={Mark}", "/api/messages", $"/api/messages?vehicleId={d.Vehicle}",
            "/api/expenses?period=Year", "/api/suppliers", "/api/receivables", $"/api/receivables?vehicleId={d.Vehicle}",
            "/api/home", "/api/accounting?period=Year", "/api/cash?period=Year", "/api/profit?period=Year",
            "/api/part-purchases?period=Year", "/api/export/cash.csv?period=Year", "/api/reminders/due",
            "/api/messages/bulk-preview", "/api/vehicles?plate=06%20GZL%2099", "/api/shop",
        })
        {
            var res = await a.GetAsync(url);
            var text = await res.Content.ReadAsStringAsync();
            if (text.Contains(Mark) || text.Contains("06 GZL 99")) failures.Add($"GET {url} → yanıtta B'nin verisi");
        }

        Assert.True(failures.Count == 0, string.Join("\n", failures));
        Assert.Equal(before, await Snapshot());   // B'nin hiçbir satırı değişmedi
    }

    [Fact]
    public void Every_route_with_an_id_is_covered_here()
    {
        var routes = f.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>()
            .Where(e => e.RoutePattern.RawText!.StartsWith("/api") && e.RoutePattern.RawText.Contains('{') && !e.RoutePattern.RawText.StartsWith("/api/o/"))
            .SelectMany(e => (e.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? [])
                .Select(m => m + " " + System.Text.RegularExpressions.Regex.Replace(e.RoutePattern.RawText!, @"\{(\w+)(:[^}]+)?\}", "{$1}")))
            .ToHashSet();
        var missing = routes.Except(Bodies().Keys).ToList();
        Assert.True(missing.Count == 0, "Yalıtım testine eklenmemiş kimlikli uç: " + string.Join(", ", missing));
    }
}
