using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Application.Traces;
using Tamiron.Infrastructure;

namespace Tamiron.Api.Tests;

/// <summary>D9 · İz kaydı: toplu gönderim, sunucuda temizlik, dükkân ve oturum oturumdan; 90 günde silinir.</summary>
public class TraceApiTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    async Task<T> Db<T>(Func<TamironDbContext, Task<T>> q)
    {
        using var scope = f.Services.CreateScope();
        return await q(scope.ServiceProvider.GetRequiredService<TamironDbContext>());
    }

    [Fact]
    public async Task Batch_is_stored_cleaned_and_bound_to_the_session()
    {
        var api = f.For(ApiFactory.ShopB);
        var marker = "34 IZ " + Random.Shared.Next(100, 999);
        var res = await api.PostAsJsonAsync("/api/trace", new
        {
            events = new object[]
            {
                new { at = "2026-10-22T09:00:01Z", kind = "ekran", name = "ruhsat" },
                new { at = "2026-10-22T09:00:05Z", kind = "alan", name = "rPlaka", data = new { deger = marker, kez = 3, bos = "" } },
                new { kind = "dokunus", name = "kaydiAc", data = new { yazi = new string('x', 120) } },
                new { kind = "uydurma", name = "x" },
            }
        });
        var r = await res.Content.ReadFromJsonAsync<TraceResult>();
        Assert.Equal((3, 1), (r!.Accepted, r.Dropped));   // tanınmayan tür düşer, söylenir

        var rows = await Db(db => db.TraceEvents.IgnoreQueryFilters().Where(e => e.TenantId == ApiFactory.ShopB)
            .OrderBy(e => e.DateTimeCreated).ToListAsync());
        var alan = rows.Single(e => e.Data != null && e.Data.Contains(marker));
        Assert.Equal(("alan", "rPlaka", $"{{\"deger\":\"{marker}\",\"kez\":3}}"), (alan.Kind, alan.Name, alan.Data));
        Assert.Equal(new DateTime(2026, 10, 22, 9, 0, 5, DateTimeKind.Utc), alan.ClientAt);
        Assert.NotEqual(Guid.Empty, alan.SessionId);
        var dok = rows.Last(e => e.Kind == "dokunus");
        Assert.Equal(80 + 1, JsonDocument.Parse(dok.Data!).RootElement.GetProperty("yazi").GetString()!.Length);   // 80 + "…"
    }

    [Fact]
    public async Task Too_big_batch_is_rejected_and_trace_needs_a_session()
    {
        var many = Enumerable.Range(0, 301).Select(_ => new { kind = "ekran", name = "x" }).ToArray();
        var res = await f.For(ApiFactory.ShopB).PostAsJsonAsync("/api/trace", new { events = many });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, res.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await f.CreateClient().PostAsJsonAsync("/api/trace", new { events = Array.Empty<object>() })).StatusCode);
    }

    [Fact]
    public async Task Purge_removes_only_detail_older_than_ninety_days()
    {
        await f.For(ApiFactory.ShopA).PostAsJsonAsync("/api/trace", new { events = new[] { new { kind = "ekran", name = "eski-olacak" } } });
        var today = f.Clock.Today;
        int purged;
        try
        {
            f.Clock.Today = today.AddDays(89);
            using (var s = f.Services.CreateScope()) Assert.Equal(0, await s.ServiceProvider.GetRequiredService<ISender>().Send(new PurgeTrace()));
            f.Clock.Today = today.AddDays(91);
            using (var s = f.Services.CreateScope()) purged = await s.ServiceProvider.GetRequiredService<ISender>().Send(new PurgeTrace());
        }
        finally { f.Clock.Today = today; }
        Assert.True(purged >= 1);
        Assert.False(await Db(db => db.TraceEvents.IgnoreQueryFilters().AnyAsync(e => e.Name == "eski-olacak")));
    }
}
