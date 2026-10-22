namespace Tamiron.Domain;

/// <summary>Kaydın satırı (VERI-MODELI §3). Parça ve işçilik aynı listede, işaretle ayrılır.</summary>
public class ServiceLine : TenantEntity
{
    public Guid ServiceRecordId { get; set; }
    /// <summary>Ustanın yazdığı sıra; geri alınan satır yerine döner (B3).</summary>
    public int Position { get; set; }
    public LineKind Kind { get; private set; }
    public string Name { get; private set; } = "";
    /// <summary>Müşteriye söylenen. Müşteri parçasında yazılı kalır, toplama girmez (K-037). 0 geçerli: fiyatı gelmemiş parça.</summary>
    public decimal Price { get; private set; }
    /// <summary>Alış. Null = bilinmiyor (kâra girmez), 0 = bedava (K-057).</summary>
    public decimal? Cost { get; private set; }
    /// <summary>Yalnız parçada dolu.</summary>
    public PartSupplier? SuppliedBy { get; private set; }
    public string? PartBrand { get; set; }
    public string? PartCode { get; set; }
    public MatchStatus MatchStatus { get; set; }

    private ServiceLine() { }   // EF

    public static ServiceLine Part(string name, decimal price, PartSupplier suppliedBy = PartSupplier.Shop, decimal? cost = null)
    {
        var l = new ServiceLine();
        l.Update(LineKind.Part, name, price, cost, suppliedBy);
        return l;
    }

    public static ServiceLine Labor(string name, decimal price)
    {
        var l = new ServiceLine();
        l.Update(LineKind.Labor, name, price, null, null);
        return l;
    }

    /// <summary>
    /// Satırın tamamını yazar (tür çevirme dahil). Tür çevrilince ad ve tutar KORUNUR;
    /// işçiliğe dönen satırın parçaya özel alanları temizlenir — kalsaydı işçilik
    /// satırında "alış 900 ₺" yazardı (prototip data-satir-tip). Parçaya dönen satır
    /// "tamirci aldı" ile başlar.
    /// </summary>
    public void Update(LineKind kind, string name, decimal price, decimal? cost, PartSupplier? suppliedBy)
    {
        name = (name ?? "").Trim();
        if (name.Length == 0)
            throw new DomainException("line.name_required", "Ne yapıldığını yaz — ör. balata.");
        if (price < 0 || cost < 0)
            throw new DomainException("line.negative_amount", "Tutar eksi olamaz.");
        Kind = kind;
        Name = name;
        Price = price;
        if (kind == LineKind.Labor)
        {
            SuppliedBy = null;
            Cost = null;
        }
        else
        {
            SuppliedBy = suppliedBy ?? PartSupplier.Shop;
            Cost = cost;   // müşteri getirse de silinmez: "ne kadara alabileceği" bilgisi (03.09)
        }
    }

    /// <summary>
    /// Dükkândan çıkan parça parası mı (prototip parcaAlislari): tamircinin aldığı parça,
    /// alışı yazılı ve sıfırdan büyük. Müşteri getirdiyse para tamirciden çıkmamıştır (K-005).
    /// </summary>
    public bool IsShopPurchase => Kind == LineKind.Part && SuppliedBy == PartSupplier.Shop && Cost > 0;

    /// <summary>Toplama girer mi: müşterinin getirdiği parça girmez (K-037).</summary>
    public bool CountsInTotal => !(Kind == LineKind.Part && SuppliedBy == PartSupplier.Customer);
}
