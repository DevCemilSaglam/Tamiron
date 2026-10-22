namespace Tamiron.Domain;

/// <summary>
/// Araç (VERI-MODELI §1). Müşteri ayrı tablo değil, buradaki sahip alanlarından
/// türetilir (A-M1). Plaka doğal anahtar: dükkân içinde tekil.
/// </summary>
public class Vehicle : TenantEntity
{
    public string PlateNo { get; set; } = "";
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? Year { get; set; }
    public string? Engine { get; set; }
    public string? Vin { get; set; }

    // Ruhsatın teknik alanları (K-033.2c) — hepsi boş olabilir.
    public string? Color { get; set; }
    public string? FuelType { get; set; }
    public string? EngineNo { get; set; }
    public string? Displacement { get; set; }
    public string? PowerKw { get; set; }
    public string? Seats { get; set; }
    public DateOnly? FirstRegistrationDate { get; set; }
    public string? FuelTankLiters { get; set; }

    public string? OwnerName { get; set; }
    public string? OwnerPhone { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? TaxOrNationalId { get; set; }
    public string? Address { get; set; }
    public string? TaxOffice { get; set; }
    public string? Note { get; set; }

    /// <summary>Ticari ileti izninin verildiği gün (K-071.3). Null = izin yok.</summary>
    public DateOnly? MarketingConsentAt { get; set; }

    /// <summary>
    /// Parça sormak için araç kimliği (K-068): 17 haneli şase YA DA marka + model + yıl + motor.
    /// Kayıt açmak için gerekmez.
    /// </summary>
    public bool HasPartIdentity =>
        // Prototipin saseGecerli'siyle aynı: büyük harfe çevir, yalnız A–Z ve 0–9 say.
        // char.IsLetterOrDigit "Ş" gibi harfleri de sayardı — şasede Türkçe harf yok.
        (Vin is not null && Vin.ToUpperInvariant().Count(c => c is >= 'A' and <= 'Z' or >= '0' and <= '9') == 17) ||
        (!string.IsNullOrWhiteSpace(Brand) && !string.IsNullOrWhiteSpace(Model) &&
         !string.IsNullOrWhiteSpace(Year) && !string.IsNullOrWhiteSpace(Engine));
}
