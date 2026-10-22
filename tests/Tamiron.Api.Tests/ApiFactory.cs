using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Api;
using Tamiron.Application;
using Tamiron.Domain;
using Tamiron.Infrastructure;

namespace Tamiron.Api.Tests;

public sealed class FixedClock : IClock
{
    public DateOnly Today { get; set; } = new(2026, 10, 22);
    public DateTime UtcNow => Today.ToDateTime(new TimeOnly(9, 0));
}

/// <summary>
/// Gerçek MSSQL (LocalDB) üzerinde, her koşu kendi veritabanında: kiracı süzgeci,
/// rowversion ve süzgeçli tekil indeks SQLite'ta farklı davranır — ölçtüğümüz
/// şey üretimde koşacak şey olsun. Göçler UYGULANIR (EnsureCreated değil):
/// göçün kendisi de sınanır.
/// İki dükkân: A (prototipin tohumu) ve B (tek araç) — kiracı yalıtımı için.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public static readonly Guid ShopA = Guid.Parse("aaaaaaaa-0000-4000-8000-000000000001");
    public static readonly Guid ShopB = Guid.Parse("bbbbbbbb-0000-4000-8000-000000000002");
    public readonly FixedClock Clock = new();
    private readonly string _db = "TamironTest_" + Guid.NewGuid().ToString("N")[..12];
    public Guid ShopBRecordId { get; private set; }

    private string Cs => $@"Server=(localdb)\MSSQLLocalDB;Database={_db};Trusted_Connection=True;TrustServerCertificate=True";

    protected override void ConfigureWebHost(IWebHostBuilder b)
    {
        b.UseEnvironment("Testing");   // DevSeed koşmaz
        // UseSetting, ConfigureAppConfiguration DEĞİL: minimal hosting'te Program.cs
        // bağlantı dizgesini Build()'den önce okuyor; geç eklenen yapılandırmayı görmezdi.
        b.UseSetting("ConnectionStrings:Tamiron", Cs);
        b.ConfigureServices(s =>
        {
            s.RemoveAll<IClock>();
            s.AddSingleton<IClock>(Clock);
        });
    }

    public HttpClient For(Guid shop)
    {
        var c = CreateClient();
        c.DefaultRequestHeaders.Add("X-Tenant-Id", shop.ToString());
        return c;
    }

    public async Task InitializeAsync()
    {
        using (var scope = Services.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<HeaderTenantContext>().TenantId = ShopA;
            var db = scope.ServiceProvider.GetRequiredService<TamironDbContext>();
            await db.Database.MigrateAsync();
            await DevSeed.SeedAsync(db, ShopA, Clock.Today);
        }
        using (var scope = Services.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<HeaderTenantContext>().TenantId = ShopB;
            var db = scope.ServiceProvider.GetRequiredService<TamironDbContext>();
            db.Shops.Add(new Shop(ShopB) { Name = "Öteki Dükkân" });
            var v = new Vehicle { TenantId = ShopB, PlateNo = "06 TMR 07" };   // aynı plaka, başka dükkân: serbest
            db.Vehicles.Add(v);
            var r = new ServiceRecord(ShopB, v.Id, "Gizli iş");
            r.AddLine(ServiceLine.Labor("İşçilik", 999));
            r.Deliver(PaymentMethod.Unpaid, Clock.Today);
            db.ServiceRecords.Add(r);
            await db.SaveChangesAsync();
            ShopBRecordId = r.Id;
        }
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        using (var scope = Services.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<HeaderTenantContext>().TenantId = ShopA;
            await scope.ServiceProvider.GetRequiredService<TamironDbContext>().Database.EnsureDeletedAsync();
        }
        await base.DisposeAsync();
    }
}

internal static class ServiceCollectionExt
{
    public static void RemoveAll<T>(this IServiceCollection s)
    {
        foreach (var d in s.Where(d => d.ServiceType == typeof(T)).ToList()) s.Remove(d);
    }
}
