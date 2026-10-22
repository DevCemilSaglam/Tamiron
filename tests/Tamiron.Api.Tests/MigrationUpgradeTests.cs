using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Tamiron.Api;
using Tamiron.Infrastructure;

namespace Tamiron.Api.Tests;

/// <summary>
/// Göç YÜKSELTME testi. Hata sınıfı (C6, 22.10): yeni zorunlu sütun VAR OLAN satırlara EF'in
/// ürettiği varsayılanla eklenir — DefaultVatRate için 0 üretmişti, her eski dükkânın KDV'si
/// %0 olurdu. Öteki testler sıfırdan kurulan veritabanında koştuğu için bunu GÖREMEZ.
/// Burada: önceki göçe kadar kur → eski şemayla dükkân yaz → son göçe yükselt → değere bak.
/// </summary>
public class MigrationUpgradeTests : IAsyncLifetime
{
    private readonly string _db = "TamironUpg_" + Guid.NewGuid().ToString("N")[..12];
    private TamironDbContext _ctx = null!;

    public Task InitializeAsync()
    {
        var o = new DbContextOptionsBuilder<TamironDbContext>()
            .UseSqlServer($@"Server=(localdb)\MSSQLLocalDB;Database={_db};Trusted_Connection=True;TrustServerCertificate=True").Options;
        _ctx = new TamironDbContext(o, new RequestContext(), new FixedClock());
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _ctx.Database.EnsureDeletedAsync();
        await _ctx.DisposeAsync();
    }

    [Fact]
    public async Task Existing_shop_gets_vat_20_not_0_after_upgrade()
    {
        var migrator = _ctx.GetService<IMigrator>();
        var all = _ctx.Database.GetMigrations().ToList();
        var vat = all.Single(m => m.EndsWith("_VatPlaceholders"));
        await migrator.MigrateAsync(all[all.IndexOf(vat) - 1]);                     // C6'dan önceki şema

        var id = Guid.NewGuid();
        await _ctx.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO Shops (Id, Name, DefaultMaintenanceMonths, DefaultMaintenanceKm, AskPartCost, DateTimeCreated) VALUES ({id}, {"Eski Dükkân"}, 6, 10000, 0, SYSUTCDATETIME())");

        await migrator.MigrateAsync();                                                // son şema
        var rate = await _ctx.Database.SqlQuery<decimal>($"SELECT DefaultVatRate AS Value FROM Shops WHERE Id = {id}").SingleAsync();
        Assert.Equal(20m, rate);
    }
}
