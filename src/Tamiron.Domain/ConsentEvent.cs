namespace Tamiron.Domain;

/// <summary>
/// Ticari ileti izninin izi (D5, K-076.5): kim, ne zaman, hangi numara için, önce ne vardı,
/// sonra ne oldu. İspat kaydıdır — yalnız EKLENİR, değiştirilmez. Araç başına bir satır:
/// izin müşterinin bütün araçlarına birlikte yazılır (K-071.3), ispat numara başınadır.
/// </summary>
public class ConsentEvent : TenantEntity
{
    public Guid VehicleId { get; private set; }
    /// <summary>Aynı dokunuşla yazılan satırlar aynı eylemdir; geri alma eylemi bütün olarak geri alır.</summary>
    public Guid ActionId { get; private set; }
    public bool Granted { get; private set; }
    /// <summary>Olaydan ÖNCEKİ izin tarihi — geri almanın tek kaynağı.</summary>
    public DateOnly? PreviousDate { get; private set; }
    /// <summary>Olaydan SONRAKİ izin tarihi (null = izin yok).</summary>
    public DateOnly? ConsentDate { get; private set; }
    /// <summary>İznin ait olduğu numara, o anki hâliyle — araçtaki telefon sonradan değişebilir.</summary>
    public string Phone { get; private set; } = "";
    public Guid UserId { get; private set; }
    /// <summary>Ustanın günü (geri alma yalnız aynı gün).</summary>
    public DateOnly RecordedOn { get; private set; }
    public bool IsUndo { get; private set; }

    private ConsentEvent() { }   // EF

    internal static ConsentEvent Of(Vehicle v, Guid action, DateOnly? before, DateOnly today, Guid userId, bool isUndo) => new()
    {
        TenantId = v.TenantId, VehicleId = v.Id, ActionId = action, Granted = v.MarketingConsentAt is not null,
        PreviousDate = before, ConsentDate = v.MarketingConsentAt, Phone = v.OwnerPhone ?? "",
        UserId = userId, RecordedOn = today, IsUndo = isUndo,
    };
}
