using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Domain;
using Tamiron.Infrastructure;

namespace Tamiron.Api.Tests;

/// <summary>
/// C7 · Hata sözleşmesi. Her hata aynı biçimde döner: Problem Details + makine kodu +
/// TÜRKÇE başlık (UI %100 Türkçe — istemci başlığı doğrudan gösterebilsin).
///   400 request.invalid  — okunamayan gövde, yanlış tür, geçersiz seçenek
///   409 record.changed   — aynı kaydı başka cihaz değiştirdi (RowVersion)
///   422 &lt;alan.kural&gt;   — iş kuralı (zaten vardı)
/// </summary>
public class ErrorContractTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static async Task<(HttpStatusCode, string?, string?)> Problem(HttpResponseMessage res)
    {
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        return (res.StatusCode, body.TryGetProperty("code", out var c) ? c.GetString() : null,
                body.TryGetProperty("title", out var t) ? t.GetString() : null);
    }

    [Fact]
    public async Task Broken_json_is_400_in_turkish()
    {
        var res = await f.For(ApiFactory.ShopB).PostAsync("/api/records",
            new StringContent("{\"plate\": ", Encoding.UTF8, "application/json"));
        var (status, code, title) = await Problem(res);
        Assert.Equal((HttpStatusCode.BadRequest, "request.invalid"), (status, code));
        Assert.StartsWith("İstek okunamadı", title);
    }

    [Fact]
    public async Task Unknown_period_is_400()
    {
        var (status, code, _) = await Problem(await f.For(ApiFactory.ShopB).GetAsync("/api/cash?period=Hafta"));
        Assert.Equal((HttpStatusCode.BadRequest, "request.invalid"), (status, code));
    }

    [Fact]
    public async Task Unknown_payment_method_in_body_is_400()
    {
        var api = f.For(ApiFactory.ShopB);
        var r = await (await api.PostAsJsonAsync("/api/records", new { plate = "34 HAT 01", complaint = "" })).Content.ReadFromJsonAsync<JsonElement>();
        var res = await api.PostAsync($"/api/records/{r.GetProperty("id").GetGuid()}/deliver",
            new StringContent("{\"method\":\"Bozuk\"}", Encoding.UTF8, "application/json"));
        var (status, code, _) = await Problem(res);
        Assert.Equal((HttpStatusCode.BadRequest, "request.invalid"), (status, code));
    }

    [Fact]
    public async Task Rowversion_detects_a_lost_update()
    {
        // İki cihaz aynı kaydı okudu; biri yazdı. Öteki ESKİ hâlin üstüne yazamaz.
        Guid id;
        using (var s = f.Services.CreateScope())
        {
            s.ServiceProvider.GetRequiredService<RequestContext>().TenantId = ApiFactory.ShopB;
            id = await s.ServiceProvider.GetRequiredService<TamironDbContext>().ServiceRecords.Select(r => r.Id).FirstAsync();
        }
        using var a = f.Services.CreateScope();
        using var b = f.Services.CreateScope();
        a.ServiceProvider.GetRequiredService<RequestContext>().TenantId = ApiFactory.ShopB;
        b.ServiceProvider.GetRequiredService<RequestContext>().TenantId = ApiFactory.ShopB;
        var dbA = a.ServiceProvider.GetRequiredService<TamironDbContext>();
        var dbB = b.ServiceProvider.GetRequiredService<TamironDbContext>();
        var ra = await dbA.ServiceRecords.SingleAsync(r => r.Id == id);
        var rb = await dbB.ServiceRecords.SingleAsync(r => r.Id == id);
        ra.SetComplaint("telefondan");
        await dbA.SaveChangesAsync();
        rb.SetComplaint("tabletten");
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => dbB.SaveChangesAsync());
    }

    [Fact]
    public async Task Concurrency_exception_maps_to_409_in_turkish()
    {
        var ctx = new DefaultHttpContext { RequestServices = f.Services };
        ctx.Response.Body = new MemoryStream();
        await new ErrorMiddleware(_ => throw new DbUpdateConcurrencyException()).InvokeAsync(ctx);
        Assert.Equal(409, ctx.Response.StatusCode);
        ctx.Response.Body.Position = 0;
        var body = await JsonSerializer.DeserializeAsync<JsonElement>(ctx.Response.Body);
        Assert.Equal("record.changed", body.GetProperty("code").GetString());
        Assert.Contains("başka bir cihazda", body.GetProperty("title").GetString());
    }
}
