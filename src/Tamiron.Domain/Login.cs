using System.Security.Cryptography;
using System.Text;

namespace Tamiron.Domain;

/// <summary>
/// Programı kullanan kişi (VERI-MODELI §11, K-074). Telefon BÜTÜN SİSTEMDE tekil:
/// giriş telefonla yapılır, telefon kişiyi, kişi dükkânı bulur. Giriş öncesi
/// aranacağı için kiracı süzgecine TABİ DEĞİL (TenantEntity değil) — dükkânın
/// kullanıcılarını listeleyen sorgu kiracıyı AÇIKÇA yazmalı.
/// </summary>
public class User : Entity
{
    public Guid TenantId { get; private set; }
    public string Phone { get; private set; } = "";
    public string? Name { get; set; }

    private User() { }   // EF

    public static User Create(Guid tenantId, string phone, string? name = null)
        => new() { TenantId = tenantId, Phone = VehicleFields.Phone(phone) ?? throw new DomainException("auth.phone_required", "Telefonu yaz."), Name = name };
}

/// <summary>
/// SMS ile giden tek kullanımlık kod. Kodun kendisi saklanmaz, tuzlu özeti saklanır.
/// ⚠️ Dürüst not: 6 haneli kodun özeti bir milyon denemeyle çözülür; asıl koruma
/// 5 dakikalık ömür, 5 deneme hakkı ve tek kullanım — özet yalnız düz okunmayı önler.
/// </summary>
public class LoginCode : Entity
{
    public const int Digits = 6, MaxAttempts = 5, MaxPerWindow = 3;
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5), Window = TimeSpan.FromMinutes(10);

    public string Phone { get; private set; } = "";
    public string Salt { get; private set; } = "";
    public string CodeHash { get; private set; } = "";
    public DateTime ExpiresAt { get; private set; }
    public int Attempts { get; private set; }
    public DateTime? ConsumedAt { get; private set; }

    private LoginCode() { }   // EF

    public static (LoginCode Entity, string Code) Issue(string phone, DateTime nowUtc)
    {
        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var salt = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        return (new LoginCode { Phone = phone, Salt = salt, CodeHash = Hash(salt, code), ExpiresAt = nowUtc + Lifetime, DateTimeCreated = nowUtc }, code);
    }

    /// <summary>
    /// Bir telefona 10 dakikada en çok 3 kod. SMS paralı: birinin ustanın numarasına
    /// kod yağdırması hem fatura hem rahatsızlık.
    /// </summary>
    public static bool CanIssue(IEnumerable<DateTime> recentIssuedUtc, DateTime nowUtc)
        => recentIssuedUtc.Count(t => t > nowUtc - Window) < MaxPerWindow;

    /// <summary>Doğruysa kodu tüketir; yanlışsa deneme sayar. Sıra: kullanılmış → kilitli → süresi dolmuş → yanlış.</summary>
    public void Verify(string code, DateTime nowUtc)
    {
        if (ConsumedAt is not null) throw new DomainException("auth.code_used", "Bu kod kullanıldı. Yeni kod iste.");
        if (Attempts >= MaxAttempts) throw new DomainException("auth.code_locked", "Çok fazla yanlış deneme. Yeni kod iste.");
        if (nowUtc > ExpiresAt) throw new DomainException("auth.code_expired", "Kodun süresi doldu. Yeni kod iste.");
        var given = Encoding.ASCII.GetBytes(Hash(Salt, (code ?? "").Trim()));
        if (!CryptographicOperations.FixedTimeEquals(given, Encoding.ASCII.GetBytes(CodeHash)))
        {
            Attempts++;
            throw new DomainException("auth.code_wrong", "Kod yanlış.");
        }
        ConsumedAt = nowUtc;
    }

    private static string Hash(string salt, string code)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(salt + ":" + code)));
}

/// <summary>
/// Açık oturum, cihaz başına bir tane (K-074.2). SÜRESİZ: yalnız çıkışla biter
/// (Cemil: "tekrar tekrar girmek zorunda kalmasın"). Cihazda anahtar, burada özeti.
/// Giriş öncesi aranacağı için kiracı süzgecine tabi değil.
/// </summary>
public class Session : Entity
{
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = "";
    public string? DeviceName { get; private set; }
    public DateTime LastSeenAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    private Session() { }   // EF

    public bool IsActive => RevokedAt is null;

    public static (Session Session, string Token) Open(Guid tenantId, Guid userId, string? deviceName, DateTime nowUtc)
    {
        // 32 rastgele bayt: tahminle bulunamaz; yüksek entropili olduğu için düz SHA-256 özeti yeterli.
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return (new Session
        {
            TenantId = tenantId, UserId = userId, TokenHash = HashToken(token), LastSeenAt = nowUtc, DateTimeCreated = nowUtc,
            DeviceName = string.IsNullOrWhiteSpace(deviceName) ? null : deviceName.Trim()[..Math.Min(deviceName.Trim().Length, 100)]
        }, token);
    }

    public static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    /// <summary>Son görülme saatte bir yazılır — her istekte yazmak her okumayı yazmaya çevirirdi.</summary>
    public bool Touch(DateTime nowUtc)
    {
        if (nowUtc - LastSeenAt < TimeSpan.FromHours(1)) return false;
        LastSeenAt = nowUtc;
        return true;
    }

    public void Revoke(DateTime nowUtc) => RevokedAt ??= nowUtc;
}
