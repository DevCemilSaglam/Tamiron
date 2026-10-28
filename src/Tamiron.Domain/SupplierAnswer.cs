namespace Tamiron.Domain;

/// <summary>
/// H6 · Parçacının kendi cevabı (VERI-MODELI §6b, K-091). PARÇACININ kiracısında durur — altyapı başka dükkânın
/// kiracısına yazmayı yasaklar. Soru ve satır kimlikleri tamirhanenin kayıtlarıdır (kiracılar arası, yabancı
/// anahtar yok). Değişmez; yenisi yazılır. Satıra kendiliğinden işlenmez (Kol B) — usta "Bunu kullan" der.
/// </summary>
public class SupplierAnswer : TenantEntity
{
    public Guid PriceRequestId { get; private set; }
    public Guid ServiceLineId { get; private set; }
    /// <summary>Parçacının fiyatı = ustanın alışı (K-076.2).</summary>
    public decimal? Price { get; private set; }
    public string? Brand { get; private set; }
    /// <summary>OEM kodu (T-13b: tamirci bilmiyor, parçacı veriyor).</summary>
    public string? Code { get; private set; }
    /// <summary>"Yok", "yarın gelir".</summary>
    public string? Note { get; private set; }
    public DateTime AnsweredAt { get; private set; }

    private SupplierAnswer() { }   // EF

    public static SupplierAnswer Create(Guid supplierTenantId, Guid requestId, Guid lineId, decimal? price,
                                        string? brand, string? code, string? note, DateTime at)
    {
        static string? Blank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        if (price < 0) throw new DomainException("line.negative_amount", "Tutar eksi olamaz.");
        var a = new SupplierAnswer
        {
            TenantId = supplierTenantId, PriceRequestId = requestId, ServiceLineId = lineId,
            Price = price, Brand = Blank(brand), Code = Blank(code), Note = Blank(note), AnsweredAt = at,
        };
        if (a.Price is null && a.Brand is null && a.Code is null && a.Note is null)
            throw new DomainException("answer.empty", "Fiyat, marka, kod ya da bir not yaz — parça yoksa \"yok\" yaz.");
        return a;
    }

    /// <summary>Ustanın kaydına düşen tek satır: fiyat · marka · kod · not (T-13 sırası).</summary>
    public string Text => string.Join(" · ", new[]
    {
        Price is { } p ? MessageTemplates.Money(p) + " ₺" : null, Brand, Code, Note,
    }.Where(x => x is not null));
}
