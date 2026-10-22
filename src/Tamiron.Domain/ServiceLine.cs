namespace Tamiron.Domain;

/// <summary>Kaydın satırı (VERI-MODELI §3). Parça ve işçilik aynı listede, işaretle ayrılır.</summary>
public class ServiceLine : TenantEntity
{
    public Guid ServiceRecordId { get; set; }
    public LineKind Kind { get; private set; }
    public string Name { get; set; } = "";
    /// <summary>Müşteriye söylenen. Müşteri parçasında yazılı kalır, toplama girmez (K-037).</summary>
    public decimal Price { get; set; }
    /// <summary>Alış. Null = bilinmiyor (kâra girmez), 0 = bedava (K-057).</summary>
    public decimal? Cost { get; set; }
    /// <summary>Yalnız parçada dolu.</summary>
    public PartSupplier? SuppliedBy { get; set; }
    public string? PartBrand { get; set; }
    public string? PartCode { get; set; }
    public MatchStatus MatchStatus { get; set; }

    private ServiceLine() { }   // EF

    public static ServiceLine Part(string name, decimal price, PartSupplier suppliedBy = PartSupplier.Shop, decimal? cost = null)
        => new() { Kind = LineKind.Part, Name = name, Price = price, SuppliedBy = suppliedBy, Cost = cost };

    public static ServiceLine Labor(string name, decimal price)
        => new() { Kind = LineKind.Labor, Name = name, Price = price };

    /// <summary>Toplama girer mi: müşterinin getirdiği parça girmez (K-037).</summary>
    public bool CountsInTotal => !(Kind == LineKind.Part && SuppliedBy == PartSupplier.Customer);
}
