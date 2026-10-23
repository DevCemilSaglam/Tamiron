using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tamiron.Application;
using Tamiron.Domain;

namespace Tamiron.Infrastructure;

/// <summary>
/// Tek veritabanı (MIMARI §3). İki süzgeç ALTYAPIDA ve zorunlu:
/// kiracı (yalnız kendi dükkânın satırları) ve soft-delete (silinmiş görünmez).
/// Uygulama katmanı bunları unutamasın diye orada değil burada.
/// </summary>
public class TamironDbContext(DbContextOptions<TamironDbContext> options, ITenantContext tenant, IClock clock)
    : DbContext(options), ITamironDb
{
    public DbSet<Shop> Shops => Set<Shop>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<ServiceRecord> ServiceRecords => Set<ServiceRecord>();
    public DbSet<ServiceLine> ServiceLines => Set<ServiceLine>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<User> Users => Set<User>();
    public DbSet<LoginCode> LoginCodes => Set<LoginCode>();
    public DbSet<Session> Sessions => Set<Session>();

    /// <summary>Süzgeçler bu özelliği her sorguda okur (EF bağlamın alanını parametre yapar).</summary>
    private Guid CurrentTenant => tenant.TenantId;

    protected override void OnModelCreating(ModelBuilder b)
    {
        Common(b.Entity<Shop>());
        b.Entity<Shop>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Address).HasMaxLength(500);
            e.Property(x => x.Phone).HasMaxLength(30);
            e.Property(x => x.DefaultVatRate).HasPrecision(5, 2);
            // Dükkân kiracının KENDİSİ: kendi satırından başkasını görmez.
            e.HasQueryFilter(x => x.Id == CurrentTenant && x.DateTimeDeleted == null);
        });

        Tenant(b.Entity<Vehicle>());
        b.Entity<Vehicle>(e =>
        {
            e.Property(x => x.PlateNo).HasMaxLength(16);
            // Plaka dükkân içinde tekil — ama silinmiş araç plakayı kilitlemesin.
            e.HasIndex(x => new { x.TenantId, x.PlateNo }).IsUnique().HasFilter("[DateTimeDeleted] IS NULL");
            foreach (var p in new[] { nameof(Vehicle.Brand), nameof(Vehicle.Model), nameof(Vehicle.Engine),
                                      nameof(Vehicle.OwnerName), nameof(Vehicle.ContactName), nameof(Vehicle.TaxOffice) })
                e.Property(p).HasMaxLength(100);
            foreach (var p in new[] { nameof(Vehicle.Year), nameof(Vehicle.Seats), nameof(Vehicle.Displacement),
                                      nameof(Vehicle.PowerKw), nameof(Vehicle.FuelTankLiters) })
                e.Property(p).HasMaxLength(10);
            foreach (var p in new[] { nameof(Vehicle.Vin), nameof(Vehicle.EngineNo), nameof(Vehicle.Color),
                                      nameof(Vehicle.FuelType), nameof(Vehicle.TaxOrNationalId) })
                e.Property(p).HasMaxLength(40);
            e.Property(x => x.OwnerPhone).HasMaxLength(30);
            e.Property(x => x.ContactPhone).HasMaxLength(30);
            e.Property(x => x.Address).HasMaxLength(500);
            e.Property(x => x.Note).HasMaxLength(2000);
        });

        Tenant(b.Entity<ServiceRecord>());
        b.Entity<ServiceRecord>(e =>
        {
            e.Property(x => x.Complaint).HasMaxLength(2000);
            e.Property(x => x.Km).HasMaxLength(20);
            e.HasOne<Vehicle>().WithMany().HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Lines).WithOne().HasForeignKey(l => l.ServiceRecordId).OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
            e.Property(x => x.WithholdingRate).HasPrecision(5, 4);   // 0,7000 = 7/10; yalnız YER (C6)
            e.Ignore(x => x.Total);
            e.Ignore(x => x.CollectedOn);
            e.Ignore(x => x.IsMaintenance);
        });

        Tenant(b.Entity<ServiceLine>());
        b.Entity<ServiceLine>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(300);
            e.Property(x => x.Price).HasPrecision(18, 2);
            e.Property(x => x.Cost).HasPrecision(18, 2);   // nullable: null ≠ 0 (K-057)
            e.Property(x => x.VatRate).HasPrecision(5, 2);  // yüzde; yalnız YER (C6)
            e.Property(x => x.PartBrand).HasMaxLength(100);
            e.Property(x => x.PartCode).HasMaxLength(100);
            e.Ignore(x => x.CountsInTotal);
            e.Ignore(x => x.IsShopPurchase);
        });

        ConfigureExpense(b);
        ConfigureLogin(b);

        Tenant(b.Entity<Supplier>());
        b.Entity<Supplier>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Phone).HasMaxLength(20);
            // Dükkân içinde numara tekil — silinen parçacı numarayı kilitlemez.
            e.HasIndex(x => new { x.TenantId, x.Phone }).IsUnique().HasFilter("[DateTimeDeleted] IS NULL");
        });

        Tenant(b.Entity<Message>());
        b.Entity<Message>(e =>
        {
            e.Property(x => x.RecipientName).HasMaxLength(200);
            e.Property(x => x.RecipientPhone).HasMaxLength(30);
            e.Property(x => x.Template).HasMaxLength(40);
            e.Property(x => x.Body).HasMaxLength(4000);
            e.HasOne<Vehicle>().WithMany().HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ServiceRecord>().WithMany().HasForeignKey(x => x.ServiceRecordId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.TenantId, x.VehicleId, x.DateTimeCreated });   // aracın arşivi
        });
    }

    /// <summary>
    /// Giriş tabloları (K-074). Kiracı SÜZGECİ YOK: giriş öncesi telefonla ve anahtar
    /// özetiyle aranırlar. Soft-delete de yok — oturum RevokedAt ile, kod ConsumedAt ile biter.
    /// </summary>
    private static void ConfigureLogin(ModelBuilder b)
    {
        Common(b.Entity<User>());
        b.Entity<User>(e =>
        {
            e.Property(x => x.Phone).HasMaxLength(20);
            e.HasIndex(x => x.Phone).IsUnique();   // telefon → kişi → dükkân
            e.Property(x => x.Name).HasMaxLength(100);
            e.HasOne<Shop>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        });
        Common(b.Entity<LoginCode>());
        b.Entity<LoginCode>(e =>
        {
            e.Property(x => x.Phone).HasMaxLength(20);
            e.Property(x => x.Salt).HasMaxLength(32);
            e.Property(x => x.CodeHash).HasMaxLength(64);
            e.HasIndex(x => new { x.Phone, x.DateTimeCreated });
        });
        Common(b.Entity<Session>());
        b.Entity<Session>(e =>
        {
            e.Property(x => x.TokenHash).HasMaxLength(64);
            e.HasIndex(x => x.TokenHash).IsUnique();
            e.HasIndex(x => x.UserId);
            e.Property(x => x.DeviceName).HasMaxLength(100);
            e.Ignore(x => x.IsActive);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Shop>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureExpense(ModelBuilder b)
    {
        Tenant(b.Entity<Expense>());
        b.Entity<Expense>(e =>
        {
            e.Property(x => x.Description).HasMaxLength(300);
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.HasIndex(x => new { x.TenantId, x.Date });   // dönem süzgeci
        });
    }

    private static void Common<T>(EntityTypeBuilder<T> e) where T : Entity
    {
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).ValueGeneratedNever();   // Guid istemcide doğar (K-073.7)
        e.Property(x => x.RowVersion).IsRowVersion();
    }

    private void Tenant<T>(EntityTypeBuilder<T> e) where T : TenantEntity
    {
        Common(e);
        e.HasIndex(x => x.TenantId);
        e.HasOne<Shop>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        e.HasQueryFilter(x => x.TenantId == CurrentTenant && x.DateTimeDeleted == null);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var now = clock.UtcNow;
        foreach (var en in ChangeTracker.Entries<Entity>())
        {
            switch (en.State)
            {
                case EntityState.Added:
                    en.Entity.DateTimeCreated = now;
                    // Kiracısız satır yazılamaz; başkasının kiracısına da yazılamaz.
                    if (en.Entity is TenantEntity t)
                    {
                        if (t.TenantId == Guid.Empty) t.TenantId = CurrentTenant;
                        else if (t.TenantId != CurrentTenant)
                            throw new InvalidOperationException("Başka dükkânın kiracısına satır yazılamaz.");
                    }
                    break;
                case EntityState.Modified:
                    en.Entity.DateTimeUpdated = now;
                    break;
                case EntityState.Deleted:
                    // SİLME = SOFT-DELETE (MIMARI §4.6): satır yerinde kalır, geri alma = boşaltmak.
                    en.State = EntityState.Modified;
                    en.Entity.DateTimeDeleted = now;
                    break;
            }
        }
        return base.SaveChangesAsync(ct);
    }
}
