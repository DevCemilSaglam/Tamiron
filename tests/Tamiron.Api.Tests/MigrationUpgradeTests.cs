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

    /// <summary>
    /// F4 · K-078: ödemeler tablosu gelince var olan kayıtlara ödeme satırı ÜRETİLİR — teslimde
    /// ödenmiş: (toplam, yol, teslim günü); sonradan: (toplam, yol, tahsil günü); veresiye: satır yok.
    /// Ölçü: kasa (gün × yol × tutar) göçten ÖNCE eski kuralla, SONRA ödemelerden — aynı olmalı.
    /// Silinmiş satır ve müşteri parçası toplama girmez; 0 ₺'lik işe ödeme satırı yazılmaz (ödeme > 0).
    /// </summary>
    [Fact]
    public async Task Existing_records_get_payment_rows_and_the_cash_stays_the_same()
    {
        var migrator = _ctx.GetService<IMigrator>();
        var all = _ctx.Database.GetMigrations().ToList();
        await migrator.MigrateAsync(all[all.IndexOf(all.Single(m => m.EndsWith("_Payments"))) - 1]);

        var shop = Guid.NewGuid();
        await _ctx.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO Shops (Id, Name, DefaultMaintenanceMonths, DefaultMaintenanceKm, AskPartCost, DefaultVatRate, DateTimeCreated) VALUES ({shop}, {"Eski"}, 6, 10000, 0, 20, SYSUTCDATETIME())");
        var veh = Guid.NewGuid();
        await _ctx.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO Vehicles (Id, TenantId, PlateNo, DateTimeCreated) VALUES ({veh}, {shop}, {"34 ESK 001"}, SYSUTCDATETIME())");

        DateOnly aug28 = new(2026, 8, 28), oct22 = new(2026, 10, 22);
        // (yol, teslim, sonradan tahsil, satırlar: fiyat · müşteri parçası mı · silinmiş mi)
        var cases = new (int? Method, DateOnly? Delivered, DateOnly? Collected, (decimal P, bool Cust, bool Del)[] Lines)[]
        {
            (1, aug28, null, [(3200m, false, false), (250m, false, false)]),   // teslimde nakit
            (2, oct22, null, [(1000m, false, false), (900m, true, false)]),    // kart; müşteri parçası toplamda yok
            (3, oct22, null, [(500m, false, false), (700m, false, true)]),     // havale; silinmiş satır yok sayılır
            (4, aug28, null, [(3450m, false, false)]),                          // veresiye — ödeme yok
            (2, aug28, oct22, [(3450m, false, false)]),                         // ağustosta teslim, ekimde kartla
            (1, oct22, null, []),                                               // 0 ₺ nakit — ödeme satırı yok
            (null, null, null, [(800m, false, false)]),                         // serviste
        };
        var n = 0;
        foreach (var c in cases)
        {
            var rec = Guid.NewGuid();
            await _ctx.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO ServiceRecords (Id, TenantId, VehicleId, Type, TypeSetManually, Complaint, DeliveredAt, PaymentMethod, PaymentCollectedAt, ReminderSent, WaitingForPart, ShowWaitingToCustomer, DateTimeCreated) VALUES ({rec}, {shop}, {veh}, 1, 0, {"iş " + n++}, {c.Delivered}, {c.Method}, {c.Collected}, 0, 0, 0, SYSUTCDATETIME())");
            var pos = 0;
            foreach (var l in c.Lines)
                await _ctx.Database.ExecuteSqlInterpolatedAsync(
                    $"INSERT INTO ServiceLines (Id, TenantId, ServiceRecordId, Position, Kind, Name, Price, SuppliedBy, MatchStatus, DateTimeCreated, DateTimeDeleted) VALUES ({Guid.NewGuid()}, {shop}, {rec}, {++pos}, 1, {"p"}, {l.P}, {(l.Cust ? 2 : 1)}, 0, SYSUTCDATETIME(), {(l.Del ? DateTime.UtcNow : (DateTime?)null)})");
        }

        // ÖNCE — eski kural (K-071.1): tahsil günü = PaymentCollectedAt ?? DeliveredAt, "almadım" hariç; tutar = toplam.
        var before = await Cash($@"SELECT CONCAT(CONVERT(char(10), COALESCE(r.PaymentCollectedAt, r.DeliveredAt), 23), '|', r.PaymentMethod, '|',
                CAST(SUM(l.Price) AS decimal(18,2))) AS Value
            FROM ServiceRecords r JOIN ServiceLines l ON l.ServiceRecordId = r.Id AND l.DateTimeDeleted IS NULL AND NOT (l.Kind = 1 AND l.SuppliedBy = 2)
            WHERE r.TenantId = '{shop}' AND r.DeliveredAt IS NOT NULL AND r.PaymentMethod <> 4
            GROUP BY r.Id, r.PaymentCollectedAt, r.DeliveredAt, r.PaymentMethod");

        await migrator.MigrateAsync();

        var after = await Cash($@"SELECT CONCAT(CONVERT(char(10), PaidOn, 23), '|', Method, '|', Amount) AS Value
            FROM Payments WHERE TenantId = '{shop}' AND DateTimeDeleted IS NULL");
        Assert.Equal(["2026-08-28|1|3450.00", "2026-10-22|2|1000.00", "2026-10-22|2|3450.00", "2026-10-22|3|500.00"], before);
        Assert.Equal(before, after);

        // Teslimde alınan teslimle gider, sonradan alınan "ödemeyi geri al"la — bayrak doğru kurulmalı.
        // Sıra (K-078.5): kayıt başına tek satır → 1.
        var flags = await Cash($"SELECT CONCAT(CONVERT(char(10), PaidOn, 23), '|', AtDelivery, '|', Sequence) AS Value FROM Payments WHERE TenantId = '{shop}'");
        Assert.Equal(["2026-08-28|1|1", "2026-10-22|0|1", "2026-10-22|1|1", "2026-10-22|1|1"], flags);
    }

    private async Task<List<string>> Cash(string sql)
        => (await _ctx.Database.SqlQueryRaw<string>(sql).ToListAsync()).Order(StringComparer.Ordinal).ToList();
}
