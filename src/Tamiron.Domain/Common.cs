namespace Tamiron.Domain;

/// <summary>
/// Her tabloda ortak alanlar (VERI-MODELI "Her tabloda ortak", MIMARI §4.6).
/// Id Guid: çevrimdışı çalışan istemci (PWA) kaydı sunucuya sormadan
/// oluşturabilsin; int kimlik sunucudan gelmeden bilinemez (K-073.7).
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime DateTimeCreated { get; set; }
    public DateTime? DateTimeUpdated { get; set; }
    /// <summary>Soft-delete: dolu = silinmiş; global süzgeç gizler, geri alma = boşaltmak.</summary>
    public DateTime? DateTimeDeleted { get; set; }
    /// <summary>Eşzamanlılık damgası (MSSQL rowversion).</summary>
    public byte[] RowVersion { get; set; } = [];
}

/// <summary>Dükkâna ait satır. TenantId = Shop.Id; global süzgeç yalnız kendi dükkânını gösterir (MIMARI §4.2).</summary>
public abstract class TenantEntity : Entity
{
    public Guid TenantId { get; set; }
}

/// <summary>
/// Kural ihlali. <see cref="Code"/> makine için (İngilizce, sabit),
/// <see cref="Message"/> ustanın okuyacağı metin (Türkçe, UI %100 Türkçe).
/// </summary>
public sealed class DomainException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
