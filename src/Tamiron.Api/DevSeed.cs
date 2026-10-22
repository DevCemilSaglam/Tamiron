using Microsoft.EntityFrameworkCore;
using Tamiron.Application;
using Tamiron.Domain;
using Tamiron.Infrastructure;

namespace Tamiron.Api;

/// <summary>
/// YALNIZ GELİŞTİRMEDE: göçleri uygular, veritabanı boşsa prototipin tohum
/// verisinden bir dilim yazar (02-veri.js — aynı plakalar, aynı tutarlar),
/// böylece API ile prototip yan yana karşılaştırılabilir. Giriş: dükkânın
/// telefonu (DevPhone) ile kod istenir, kod günlüğe yazılır (DevLogSmsSender).
/// </summary>
public static class DevSeed
{
    public static readonly Guid ShopId = Guid.Parse("7a3e0c1e-0000-4000-8000-000000000001");
    public const string DevPhone = "0532 000 00 00";

    public static async Task RunAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        scope.ServiceProvider.GetRequiredService<RequestContext>().TenantId = ShopId;
        var db = scope.ServiceProvider.GetRequiredService<TamironDbContext>();
        var today = scope.ServiceProvider.GetRequiredService<IClock>().Today;
        await db.Database.MigrateAsync();
        if (!await db.Shops.IgnoreQueryFilters().AnyAsync()) await SeedAsync(db, ShopId, today);
        // Giriş tablosu sonradan geldi (K-074): eski geliştirme veritabanında kullanıcı yoksa eklenir.
        if (!await db.Users.AnyAsync(u => u.Phone == DevPhone))
        {
            db.Users.Add(User.Create(ShopId, DevPhone, "Ahmet Usta"));
            await db.SaveChangesAsync();
        }
    }

    public static async Task SeedAsync(TamironDbContext db, Guid shopId, DateOnly today)
    {
        db.Shops.Add(new Shop(shopId) { Name = "Ahmet Usta Oto Servis", Address = "Ostim OSB, Ankara", Phone = "0532 000 00 00" });

        var mehmet = new Vehicle { TenantId = shopId, PlateNo = "34 ABC 123", Brand = "Renault", Model = "Mégane", Year = "2018",
            Engine = "1.5 dCi", Vin = "VF1RFB00858123456", OwnerName = "Mehmet Kaya", OwnerPhone = "0532 215 30 00",
            MarketingConsentAt = new DateOnly(2026, 6, 14) };
        var demir = new Vehicle { TenantId = shopId, PlateNo = "06 TMR 07", Brand = "Fiat", Model = "Egea", Year = "2021",
            Engine = "1.4 Fire", OwnerName = "Demir Nakliyat Ltd. Şti.", OwnerPhone = "0312 395 12 00",
            ContactName = "Hasan Demir", ContactPhone = "0542 331 90 07" };
        var ayse = new Vehicle { TenantId = shopId, PlateNo = "35 KLM 45", Brand = "Peugeot", Model = "301", Year = "2019",
            Engine = "1.6 BlueHDi", OwnerName = "Ayşe Kaya" };
        db.Vehicles.AddRange(mehmet, demir, ayse);

        // Serviste, parça bekliyor (prototip id 1).
        var r1 = new ServiceRecord(shopId, mehmet.Id, "Triger seti değişecek");
        r1.AddLine(ServiceLine.Part("Triger seti", 0));
        r1.SetWaitingForPart(true, today);

        // Havale, haziranda (prototip id 6).
        var r6 = new ServiceRecord(shopId, mehmet.Id, "Ara bakım");
        r6.SetKm("151.200");
        r6.AddLine(ServiceLine.Part("Motor yağı", 1950, cost: 1500));
        r6.AddLine(ServiceLine.Labor("Bakım işçiliği", 900));
        r6.Deliver(PaymentMethod.Transfer, new DateOnly(today.Year, 6, 20), MaintenancePolicy.Default, []);

        // Bugün nakit; parçanın alışı BİLİNMİYOR (prototip id 7, K-055.2).
        var r7 = new ServiceRecord(shopId, ayse.Id, "Ön cam silecek motoru");
        r7.AddLine(ServiceLine.Part("Silecek motoru", 2100));
        r7.AddLine(ServiceLine.Labor("Söküm takım", 700));
        r7.Deliver(PaymentMethod.Cash, today, MaintenancePolicy.Default, []);

        // Veresiye (prototip id 8) — "Ödeme alındı"nın denendiği kayıt.
        var r8 = new ServiceRecord(shopId, demir.Id, "Akü değişimi");
        r8.AddLine(ServiceLine.Part("Akü 60 Ah", 3200, cost: 2450));
        r8.AddLine(ServiceLine.Labor("Takma", 250));
        r8.Deliver(PaymentMethod.Unpaid, today.AddDays(-55), MaintenancePolicy.Default, []);

        db.ServiceRecords.AddRange(r1, r6, r7, r8);
        await db.SaveChangesAsync();
    }
}
