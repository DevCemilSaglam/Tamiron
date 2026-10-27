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
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Advance> Advances => Set<Advance>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PriceRequest> PriceRequests => Set<PriceRequest>();
    public DbSet<PriceReply> PriceReplies => Set<PriceReply>();
    public DbSet<ConsentEvent> ConsentEvents => Set<ConsentEvent>();
    public DbSet<TraceEvent> TraceEvents => Set<TraceEvent>();
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
            // K-078 · Satırlar ve ödemeler kaydın parçası: HER YÜKLEMEDE gelir (AutoInclude). Toplam,
            // kalan, teslimdeki ödeme ve düzeltme kuralları ikisini de okur — bir sorguda Include
            // unutulsa kural boş listeyle çalışır ve SESSİZCE yanlış söylerdi. Bu oldu: tahsil,
            // düzeltme ve teslim uçları kaydı satırsız yüklüyordu (toplam 0 → "alınmamış ödemesi yok",
            // teslimde ödeme satırı yazılmaz); F3'te API testleri yakaladı.
            e.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();
            e.HasMany(x => x.Payments).WithOne().HasForeignKey(p => p.ServiceRecordId).OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.Payments).UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();
            e.Ignore(x => x.Paid);
            e.Ignore(x => x.Remaining);
            e.Property(x => x.WithholdingRate).HasPrecision(5, 4);   // 0,7000 = 7/10; yalnız YER (C6)
            e.Ignore(x => x.Total);
            e.Ignore(x => x.CollectedOn);
            e.Ignore(x => x.IsMaintenance);
            e.Property(x => x.SummaryCode).HasMaxLength(32);
            // Kod bütün dükkânlarda tekil: link kiracısız açılır, kod kaydı tek başına bulur.
            e.HasIndex(x => x.SummaryCode).IsUnique().HasFilter("[SummaryCode] IS NOT NULL");
            // D17 · Sıcak sorgular (K-076.16): kasa/teslim/alacak dönem süzgeci dükkân + teslim günü;
            // sonradan tahsil edilenler tahsil günü; hatırlatması gelenler yalnız GÖNDERİLMEMİŞLER.
            // Süzgeçli indeksler küçük kalır: tahsil ve hatırlatma kayıtların azında dolu.
            e.HasIndex(x => new { x.TenantId, x.DeliveredAt });
            e.HasIndex(x => new { x.TenantId, x.PaymentCollectedAt }).HasFilter("[PaymentCollectedAt] IS NOT NULL");
            e.HasIndex(x => new { x.TenantId, x.ReminderDate }).HasFilter("[ReminderDate] IS NOT NULL AND [ReminderSent] = 0");
        });

        Tenant(b.Entity<Payment>());
        b.Entity<Payment>(e =>
        {
            e.Property(x => x.Amount).HasPrecision(18, 2);
            // Kasa dönem süzgeci (F5): dükkân + ödeme günü.
            e.HasIndex(x => new { x.TenantId, x.PaidOn });
        });

        // G7 · Müşteri avansı (VERI-MODELI §2c).
        Tenant(b.Entity<Advance>());
        b.Entity<Advance>(e =>
        {
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.HasIndex(x => new { x.TenantId, x.VehicleId });
            e.HasIndex(x => new { x.TenantId, x.On });   // kasa dönem süzgeci
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
            e.HasOne<PriceReply>().WithMany().HasForeignKey(x => x.SelectedReplyId).OnDelete(DeleteBehavior.NoAction);
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

        ConfigurePriceRequest(b);

        // D9 · İz kaydı: bol satır, az okuma. Silme süreyle (90 gün), dükkân/oturum sorusu için iki indeks.
        Tenant(b.Entity<TraceEvent>());
        b.Entity<TraceEvent>(e =>
        {
            e.Property(x => x.Kind).HasMaxLength(16);
            e.Property(x => x.Name).HasMaxLength(100);
            e.Property(x => x.Data).HasMaxLength(4000);
            e.HasIndex(x => x.DateTimeCreated);
            e.HasIndex(x => new { x.SessionId, x.DateTimeCreated });
        });

        // D5 · İzin izi: yalnız eklenir. Araç silinse de iz kalır (ispat).
        Tenant(b.Entity<ConsentEvent>());
        b.Entity<ConsentEvent>(e =>
        {
            e.Property(x => x.Phone).HasMaxLength(30);
            e.HasIndex(x => new { x.VehicleId, x.DateTimeCreated });
            e.HasOne<Vehicle>().WithMany().HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Restrict);
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

    /// <summary>D2 · Fiyat sorusu, soru↔satır bağı (A-24) ve cevap (VERI-MODELI §5–6).</summary>
    private void ConfigurePriceRequest(ModelBuilder b)
    {
        Tenant(b.Entity<PriceRequest>());
        b.Entity<PriceRequest>(e =>
        {
            e.Property(x => x.SentText).HasMaxLength(4000);
            e.HasIndex(x => x.ServiceRecordId);
            e.HasOne<ServiceRecord>().WithMany().HasForeignKey(x => x.ServiceRecordId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Supplier>().WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            // Bağ tablosu kiracı taşımaz: sahibi olan soru taşır, soru üzerinden okunur.
            e.OwnsMany(x => x.Lines, l =>
            {
                l.ToTable("PriceRequestLines");
                l.WithOwner().HasForeignKey(x => x.PriceRequestId);
                l.HasKey(x => new { x.PriceRequestId, x.ServiceLineId });
                l.HasOne<ServiceLine>().WithMany().HasForeignKey(x => x.ServiceLineId).OnDelete(DeleteBehavior.NoAction);
            });
            e.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        Tenant(b.Entity<PriceReply>());
        b.Entity<PriceReply>(e =>
        {
            e.Property(x => x.RawText).HasMaxLength(4000);
            e.Property(x => x.ParsedPrice).HasPrecision(18, 2);
            e.Property(x => x.ParsedBrand).HasMaxLength(100);
            e.Property(x => x.ParsedCode).HasMaxLength(100);
            e.HasIndex(x => x.PriceRequestId);
            e.HasOne<PriceRequest>().WithMany().HasForeignKey(x => x.PriceRequestId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void Tenant<T>(EntityTypeBuilder<T> e) where T : TenantEntity
    {
        Common(e);
        e.HasIndex(x => x.TenantId);
        e.HasOne<Shop>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        e.HasQueryFilter(x => x.TenantId == CurrentTenant && x.DateTimeDeleted == null);
    }

    public Task<int> PurgeTraceBefore(DateTime utc, CancellationToken ct)
        => TraceEvents.IgnoreQueryFilters().Where(e => e.DateTimeCreated < utc).ExecuteDeleteAsync(ct);

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
