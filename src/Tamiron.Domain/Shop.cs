namespace Tamiron.Domain;

/// <summary>
/// Dükkân = kiracı. <see cref="Entity.Id"/> öteki tablolardaki TenantId'dir;
/// kendisi TenantId taşımaz (VERI-MODELI §8).
/// </summary>
public class Shop : Entity
{
    public string Name { get; set; } = "";
    public string? Address { get; set; }
    public string? Phone { get; set; }
    /// <summary>Hatırlatma periyodu — ay VEYA km, hangisi önce (A-3, 03.09).</summary>
    public int DefaultMaintenanceMonths { get; set; } = 6;
    public int DefaultMaintenanceKm { get; set; } = 10_000;
    /// <summary>Parça alış fiyatı sorulsun mu (A-M43). Varsayılan kapalı.</summary>
    public bool AskPartCost { get; set; }
    /// <summary>Gizlilik notu (A-M33) gösterildiği an.</summary>
    public DateTime? PrivacyNoticeShownAt { get; set; }
}
