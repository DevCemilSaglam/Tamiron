namespace Tamiron.Domain;

/// <summary>
/// Parçacı (VERI-MODELI §4) — İKİ alan: ad ve telefon. Cari hesap, adres, vergi no,
/// vade YOK. Tamirci: "Yerel parçacının tel belli. Bunları eklememiz gerekecek."
/// Telefon zorunlu: fiyat sorusu WhatsApp'la ona gider (T-12b).
/// </summary>
public class Supplier : TenantEntity
{
    public string Name { get; private set; } = "";
    public string Phone { get; private set; } = "";

    private Supplier() { }   // EF

    public static Supplier Create(Guid tenantId, string name, string phone)
    {
        var s = new Supplier { TenantId = tenantId };
        s.Update(name, phone);
        return s;
    }

    public void Update(string name, string phone)
    {
        name = (name ?? "").Trim();
        if (name.Length == 0) throw new DomainException("supplier.name_required", "Parçacının adını yaz.");
        Phone = VehicleFields.Phone(phone) ?? throw new DomainException("supplier.phone_required", "Parçacının telefonunu yaz — soru WhatsApp'la ona gider.");
        Name = name;
    }
}
