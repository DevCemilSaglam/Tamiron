using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Application.Accounting;
using Tamiron.Application.Home;
using Tamiron.Application.ServiceRecords;
using Tamiron.Domain;
using Tamiron.Infrastructure;

namespace Tamiron.Api.Tests;

/// <summary>
/// C9 · Sorgular SQL'e taşındıktan sonra rakamlar DEĞİŞMEZ. B dükkânına dönem DIŞINDA
/// 300 eski iş yazılır (geçen yıl: ödenmiş, veresiye ve sonradan bu ay ödenmiş karışık);
/// her uç beklenen rakamı verir. Sorgu dönemi süzmeyi unutursa rakam şişer.
/// </summary>
public class CashQueryEquivalenceTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    [Fact]
    public async Task Many_old_jobs_do_not_leak_into_this_month()
    {
        using (var scope = f.Services.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<RequestContext>().TenantId = ApiFactory.ShopB;
            var db = scope.ServiceProvider.GetRequiredService<TamironDbContext>();
            var v = new Vehicle { TenantId = ApiFactory.ShopB, PlateNo = "34 ESK 01" };
            db.Vehicles.Add(v);
            for (var i = 0; i < 300; i++)
            {
                var r = new ServiceRecord(ApiFactory.ShopB, v.Id, "eski iş " + i);
                r.AddLine(ServiceLine.Labor("İşçilik", 100));
                var on = new DateOnly(2025, 1, 1).AddDays(i);
                if (i % 3 == 0) r.Deliver(PaymentMethod.Cash, on, MaintenancePolicy.Default, []);
                else
                {
                    r.Deliver(PaymentMethod.Unpaid, on, MaintenancePolicy.Default, []);
                    if (i % 3 == 1) r.CollectPayment(PaymentMethod.Card, new DateOnly(2026, 10, 5));   // bu ay ödendi: 100 eski iş
                }
                db.ServiceRecords.Add(r);
            }
            await db.SaveChangesAsync();
        }

        var api = f.For(ApiFactory.ShopB);
        var month = (await api.GetFromJsonAsync<CashSummary>("/api/cash?period=Month", Json))!;
        Assert.Equal(100 * 100m, month.Card);                    // yalnız bu ay ödenen eski işler
        Assert.Equal(0m, month.Cash);
        Assert.Equal(1, month.DeliveredCount);                   // eskiler geçen yıl; yalnız tohumdaki "Gizli iş" bugün
        Assert.Equal(100 * 100m + 999m, month.Receivable);       // 100 veresiye + tohumdaki "Gizli iş" 999

        var home = (await api.GetFromJsonAsync<HomeSummary>("/api/home", Json))!;
        Assert.Equal(0m, home.CollectedToday);
        Assert.Equal(month.Receivable, home.Receivable);

        var profit = (await api.GetFromJsonAsync<JobProfitList>("/api/profit?period=Month", Json))!;
        Assert.DoesNotContain(profit.Items, i => i.PlateNo == "34 ESK 01");   // iş başına fark TESLİM gününe göre

        var csv = await api.GetStringAsync("/api/export/cash.csv?period=Month");
        Assert.Equal(100, csv.Split("\r\n").Count(l => l.StartsWith("Tahsilat;")));
        Assert.Equal(101, csv.Split("\r\n").Count(l => l.StartsWith("Alacak;")));
    }
}
