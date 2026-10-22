namespace Tamiron.Domain;

/// <summary>
/// Dükkân = kiracı. <see cref="Entity.Id"/> öteki tablolardaki TenantId'dir;
/// kendisi TenantId taşımaz (VERI-MODELI §8).
/// </summary>
public class Shop : Entity
{
    public Shop() { }
    /// <summary>Kimliği belli dükkân (kiracı kimliği o olacağı için dışarıdan verilebilir).</summary>
    public Shop(Guid id) { Id = id; }

    public string Name { get; set; } = "";
    public string? Address { get; set; }
    public string? Phone { get; set; }
    /// <summary>Hatırlatma periyodu — ay VEYA km, hangisi önce (A-3, 03.09).</summary>
    public int DefaultMaintenanceMonths { get; private set; } = 6;

    /// <summary>Ayarlar ekranındaki seçenekler (prototip 99-ayarlar: 3 · 6 · 12 ay). Başka değer yok.</summary>
    public static readonly int[] MaintenanceMonthChoices = [3, 6, 12];

    public void SetMaintenanceMonths(int months)
    {
        if (!MaintenanceMonthChoices.Contains(months))
            throw new DomainException("shop.period_invalid", "Bakım periyodu 3, 6 ya da 12 ay olabilir.");
        DefaultMaintenanceMonths = months;
    }

    /// <summary>Gizlilik notu (A-M33) BİR KEZ gösterilir: ilk gösterim anı korunur.</summary>
    public void MarkPrivacyNoticeShown(DateTime nowUtc) => PrivacyNoticeShownAt ??= nowUtc;
    public int DefaultMaintenanceKm { get; set; } = 10_000;
    /// <summary>Parça alış fiyatı sorulsun mu (A-M43). Varsayılan kapalı.</summary>
    public bool AskPartCost { get; set; }
    /// <summary>Gizlilik notu (A-M33) gösterildiği an.</summary>
    public DateTime? PrivacyNoticeShownAt { get; set; }
}
