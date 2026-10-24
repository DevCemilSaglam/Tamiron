using Microsoft.EntityFrameworkCore;
using Tamiron.Domain;

namespace Tamiron.Application;

/// <summary>İsteğin hangi dükkâna ait olduğu — OTURUMDAN gelir (K-074.4).</summary>
public interface ITenantContext
{
    Guid TenantId { get; }
}

/// <summary>İsteği yapan kişi ve cihazı (oturum). Giriş öncesi uçlarda boştur.</summary>
public interface ICurrentSession
{
    Guid UserId { get; }
    Guid SessionId { get; }
}

/// <summary>
/// SMS gönderimi (K-074). Sağlayıcı (Netgsm, İleti Merkezi …) Cemil'in kararı ve
/// paralı; bağlanmadıkça gönderim <see cref="SmsNotConfiguredException"/> atar —
/// "gönderildi" diye sessizce yalan söylenmez.
/// </summary>
public interface ISmsSender
{
    Task SendAsync(string phone, string text, CancellationToken ct);
}

public sealed class SmsNotConfiguredException() : Exception("SMS sağlayıcısı bağlanmadı.");

/// <summary>
/// Ruhsat fotoğrafını alanlara çevirir (D8). Okuyucu (K-067.2c Gemini, sonra kendi sunucu
/// K-070) Cemil'in kararı ve paralı; bağlanmadıkça <see cref="ReaderNotConfiguredException"/>.
/// Fotoğraf okuyucuya gider, SAKLANMAZ.
/// </summary>
public interface IRegistrationReader
{
    Task<Domain.RegistrationRead> ReadAsync(byte[] image, string mediaType, CancellationToken ct);
}

public sealed class ReaderNotConfiguredException() : Exception("Ruhsat okuyucusu bağlanmadı.");

/// <summary>Okuyucu bağlı ama okuma olmadı (kredi bitti, anahtar geçersiz, servis hatası). Usta elle yazar.</summary>
public sealed class ReaderFailedException(string code) : Exception("Ruhsat okunamadı: " + code)
{
    public string Code { get; } = code;
}

/// <summary>Sağlayıcı bağlı ama gönderim olmadı (hesap, başlık, ağ). Sessizce "gönderildi" denmez.</summary>
public sealed class SmsSendFailedException(string code) : Exception("SMS gönderilemedi: " + code)
{
    public string Code { get; } = code;
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
    DbSet<Payment> Payments { get; }
    DbSet<Expense> Expenses { get; }
    DbSet<Message> Messages { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<PriceRequest> PriceRequests { get; }
    DbSet<PriceReply> PriceReplies { get; }
    DbSet<ConsentEvent> ConsentEvents { get; }
    DbSet<TraceEvent> TraceEvents { get; }
    // Giriş tabloları kiracı süzgecine TABİ DEĞİL (giriş öncesi aranır) — sorgu kiracıyı açıkça yazar.
    DbSet<User> Users { get; }
    DbSet<LoginCode> LoginCodes { get; }
    DbSet<Session> Sessions { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    /// <summary>Süresi dolan izi GERÇEKTEN siler (soft-delete değil, bütün dükkânlarda) — K-053.4.</summary>
    Task<int> PurgeTraceBefore(DateTime utc, CancellationToken ct);
}

/// <summary>Bulunamadı — kayıt yok YA DA başka dükkânın (süzgeç ikisini ayırt ettirmez, bilerek).</summary>
public sealed class NotFoundException(string what) : Exception($"{what} bulunamadı.");
