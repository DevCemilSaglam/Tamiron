using Microsoft.EntityFrameworkCore;
using Tamiron.Domain;

namespace Tamiron.Application;

/// <summary>
/// İsteğin hangi dükkâna ait olduğu. Geliştirmede X-Tenant-Id başlığından
/// okunuyor — GEÇİCİ (K-073.6); oturum açma yöntemi Cemil onayına açık.
/// </summary>
public interface ITenantContext
{
    Guid TenantId { get; }
}

/// <summary>
/// "Bugün" — ustanın günü, Türkiye saatiyle. Testte sabitlenir (prototipte
/// BUGUN tek kaynaktı; aynı ders: tarih tek yerden gelir).
/// </summary>
public interface IClock
{
    DateTime UtcNow { get; }
    DateOnly Today { get; }
    /// <summary>Saklanan UTC anın ustanın günündeki karşılığı (ör. açılış günü = DateTimeCreated).</summary>
    DateOnly LocalDate(DateTime utc);
}

/// <summary>Uygulama katmanının gördüğü veritabanı. Kiracı ve soft-delete süzgeci altyapıda, zorunlu (MIMARI §4.2).</summary>
public interface ITamironDb
{
    DbSet<Shop> Shops { get; }
    DbSet<Vehicle> Vehicles { get; }
    DbSet<ServiceRecord> ServiceRecords { get; }
    DbSet<ServiceLine> ServiceLines { get; }
    DbSet<Expense> Expenses { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

/// <summary>Bulunamadı — kayıt yok YA DA başka dükkânın (süzgeç ikisini ayırt ettirmez, bilerek).</summary>
public sealed class NotFoundException(string what) : Exception($"{what} bulunamadı.");
