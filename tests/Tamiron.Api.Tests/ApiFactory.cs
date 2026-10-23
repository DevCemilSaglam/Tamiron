using System.Collections.Concurrent;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tamiron.Api;
using Tamiron.Application;
using Tamiron.Domain;
using Tamiron.Infrastructure;

namespace Tamiron.Api.Tests;

public sealed class FixedClock : IClock
{
    public DateOnly Today { get; set; } = new(2026, 10, 22);
    // Gerçek saat İLERLER: her okuma bir milisaniye sonrası. Sabit saat, aynı istekte
    // ardışık yazılan kayıtları eşit anlı yapıyor ve "en yeni üstte" sınanamıyordu (B11).
    private long _tick;
    public DateTime UtcNow => Today.ToDateTime(new TimeOnly(9, 0)).AddMilliseconds(Interlocked.Increment(ref _tick));
    public DateOnly LocalDate(DateTime utc) => DateOnly.FromDateTime(utc.AddHours(3));   // Türkiye sabit UTC+3
}

/// <summary>SMS gitmez; testler kodu buradan okur (K-074: gerçek gönderim Cemil'in sağlayıcı kararında).</summary>
public sealed class FakeSms : ISmsSender
{
    public readonly ConcurrentQueue<(string Phone, string Text)> Queue = new();
    public IReadOnlyList<(string Phone, string Text)> Sent => Queue.ToList();
    /// <summary>Doluysa gönderim bu kodla BAŞARISIZ olur (sağlayıcı reddi, D7).</summary>
    public string? FailWith;
    public Task SendAsync(string phone, string text, CancellationToken ct)
    {
        if (FailWith is { } code) throw new SmsSendFailedException(code);
        Queue.Enqueue((phone, text));
        return Task.CompletedTask;
    }
}

/// <summary>
/// Gerçek MSSQL (LocalDB) üzerinde, her koşu kendi veritabanında: kiracı süzgeci,
/// rowversion ve süzgeçli tekil indeks SQLite'ta farklı davranır — ölçtüğümüz
/// şey üretimde koşacak şey olsun. Göçler UYGULANIR (EnsureCreated değil):
/// göçün kendisi de sınanır.
/// İki dükkân: A (prototipin tohumu) ve B (tek araç) — kiracı yalıtımı için.
/// K-074'ten beri dükkân OTURUMDAN gelir: For(dükkân) o dükkânın kullanıcısına
/// gerçek bir oturum açar ve anahtarıyla istek atar.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public static readonly Guid ShopA = Guid.Parse("aaaaaaaa-0000-4000-8000-000000000001");
    public static readonly Guid ShopB = Guid.Parse("bbbbbbbb-0000-4000-8000-000000000002");
    public readonly FixedClock Clock = new();
    public readonly FakeSms Sms = new();
    private readonly string _db = "TamironTest_" + Guid.NewGuid().ToString("N")[..12];
    private readonly Dictionary<Guid, string> _tokens = [];
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
            s.RemoveAll<ISmsSender>();
            s.AddSingleton<ISmsSender>(Sms);
        });
    }

    public HttpClient For(Guid shop)
    {
        var c = CreateClient();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokens[shop]);
        return c;
    }

    public async Task InitializeAsync()
    {
        using (var scope = Services.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<RequestContext>().TenantId = ShopA;
            var db = scope.ServiceProvider.GetRequiredService<TamironDbContext>();
            await db.Database.MigrateAsync();
            await DevSeed.SeedAsync(db, ShopA, Clock.Today);
        }
        using (var scope = Services.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<RequestContext>().TenantId = ShopB;
            var db = scope.ServiceProvider.GetRequiredService<TamironDbContext>();
            db.Shops.Add(new Shop(ShopB) { Name = "Öteki Dükkân" });
            var v = new Vehicle { TenantId = ShopB, PlateNo = "06 TMR 07" };   // aynı plaka, başka dükkân: serbest
            db.Vehicles.Add(v);
            var r = new ServiceRecord(ShopB, v.Id, "Gizli iş");
            r.AddLine(ServiceLine.Labor("İşçilik", 999));
            r.Deliver(PaymentMethod.Unpaid, Clock.Today, MaintenancePolicy.Default, []);
            db.ServiceRecords.Add(r);
            await db.SaveChangesAsync();
            ShopBRecordId = r.Id;
        }
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TamironDbContext>();
            // Giriş testlerinin numaraları: 01 ve 05 A'nın, 02–04 B'nin.
            var users = new[] { (ShopA, "05320000001"), (ShopB, "05320000002"), (ShopB, "05320000003"), (ShopB, "05320000004"), (ShopA, "05320000005") }
                .Select(x => User.Create(x.Item1, x.Item2)).ToList();
            db.Users.AddRange(users);
            foreach (var shop in new[] { ShopA, ShopB })
            {
                var (s, token) = Session.Open(shop, users.First(u => u.TenantId == shop).Id, "test", Clock.UtcNow);
                db.Sessions.Add(s);
                _tokens[shop] = token;
            }
            await db.SaveChangesAsync();
        }
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        using (var scope = Services.CreateScope())
            await scope.ServiceProvider.GetRequiredService<TamironDbContext>().Database.EnsureDeletedAsync();
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
