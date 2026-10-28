namespace Tamiron.Domain;

/// <summary>
/// Fiyat sorusu (VERI-MODELI §5). Birden fazla parçacıya sorulursa her biri ayrı kayıt
/// (Kol B). Bir soru birden çok satırı sorar: bağ <see cref="PriceRequestLine"/> (A-24).
/// </summary>
public class PriceRequest : TenantEntity
{
    public Guid ServiceRecordId { get; private set; }
    public Guid SupplierId { get; private set; }
    /// <summary>Giden mesajın kendisi — sonradan "ne sormuşuz" diye bakılır.</summary>
    public string SentText { get; private set; } = "";
    public DateTime SentAt { get; private set; }

    private readonly List<PriceRequestLine> _lines = [];
    public IReadOnlyList<PriceRequestLine> Lines => _lines;

    private PriceRequest() { }   // EF

    public static PriceRequest Create(Guid tenantId, Guid recordId, Guid supplierId, string text, IEnumerable<ServiceLine> lines, DateTime at)
    {
        var r = new PriceRequest { TenantId = tenantId, ServiceRecordId = recordId, SupplierId = supplierId, SentText = text, SentAt = at };
        r._lines.AddRange(lines.Select(l => new PriceRequestLine(r.Id, l.Id)));
        return r;
    }
}

/// <summary>Soru ↔ satır bağı (A-24). Mesaj metni satır sayısı kadar çoğalmaz.</summary>
public class PriceRequestLine
{
    public Guid PriceRequestId { get; private set; }
    public Guid ServiceLineId { get; private set; }

    internal PriceRequestLine(Guid priceRequestId, Guid serviceLineId)
    {
        PriceRequestId = priceRequestId;
        ServiceLineId = serviceLineId;
    }
}

/// <summary>
/// Fiyat cevabı (VERI-MODELI §6). Prototipte cevap uygulamaya DÜŞMÜYOR — usta parçacının
/// WhatsApp'tan yazdığını kendi eliyle giriyor. <see cref="RawText"/> en kritik alan:
/// türetilmiş veriyi saklayıp kaynağı atmak ayrıştırma hatasını kalıcı yapar (v1 dersi).
/// </summary>
public class PriceReply : TenantEntity
{
    public Guid PriceRequestId { get; private set; }
    public string RawText { get; private set; } = "";
    /// <summary>Parçacının verdiği fiyat = ustanın ALIŞI (K-076.2). Müşteri fiyatı ayrı.</summary>
    public decimal? ParsedPrice { get; private set; }
    public string? ParsedBrand { get; private set; }
    public string? ParsedCode { get; private set; }
    public DateTime ReceivedAt { get; private set; }
    /// <summary>H6 · Parçacının kendi yazdığı cevaptan mı ("Bunu kullan" — K-091). Null = usta yazdı.</summary>
    public Guid? SupplierAnswerId { get; private set; }

    private PriceReply() { }   // EF

    /// <summary>H6 · Parçacının cevabını tamirhaneye kopyalar — ham metin cevabın kendisi (T-13 sırası).</summary>
    public static PriceReply FromAnswer(SupplierAnswer a, DateTime at)
    {
        var r = Create(a.PriceRequestId, a.Text, a.Price, a.Brand, a.Code, at);
        r.SupplierAnswerId = a.Id;
        return r;
    }

    public static PriceReply Create(Guid requestId, string? rawText, decimal? price, string? brand, string? code, DateTime at)
    {
        rawText = (rawText ?? "").Trim();
        if (rawText.Length == 0 && price is null)
            throw new DomainException("reply.empty", "Parçacının ne dediğini yaz ya da fiyatı gir.");
        if (price < 0)
            throw new DomainException("line.negative_amount", "Tutar eksi olamaz.");
        return new PriceReply
        {
            PriceRequestId = requestId, RawText = rawText, ParsedPrice = price,
            ParsedBrand = Blank(brand), ParsedCode = Blank(code), ReceivedAt = at,
        };
    }

    private static string? Blank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}

/// <summary>Parça sorma kuralları — prototip 40-parca-sor.html.</summary>
public static class PartQuestion
{
    /// <summary>
    /// Sorulacak satırlar (prototip psParcaListesi): yalnız TAMİRCİNİN alacağı ve fiyatı
    /// henüz belli olmayan parçalar. Müşterinin getireceği parçanın fiyatını sormanın anlamı yok.
    /// </summary>
    public static IReadOnlyList<ServiceLine> AskableLines(IEnumerable<ServiceLine> lines)
        => lines.Where(l => l.Kind == LineKind.Part && l.SuppliedBy == PartSupplier.Shop && l.Price == 0).ToList();

    /// <summary>
    /// Kapılar prototipteki SIRAYLA konuşur (K-033.3 — kapalı düğme sessizdir):
    /// sorulacak parça · araç kimliği (K-068) · kime soracağı.
    /// </summary>
    public static void EnsureCanAsk(Vehicle v, IReadOnlyList<ServiceLine> askable, int supplierCount)
    {
        if (askable.Count == 0)
            throw new DomainException("price_request.no_parts", "Sorulacak parça yok — önce servis kaydına parça ekle.");
        if (!v.HasPartIdentity)
            throw new DomainException("price_request.vehicle_identity",
                $"Şase yok — önce aracı seç: {string.Join(", ", v.MissingForParts)}. Ya da ruhsatı çek.");
        if (supplierCount == 0)
            throw new DomainException("price_request.no_supplier", "Önce kime soracağını seç.");
    }

    /// <summary>
    /// Mesajın gövdesi DÜZ METİN — parçacıya giden şey bu. Şase MESAJDA (K-068): parçanın
    /// araca uyumu şaseyle kurulur. Prototip araç satırını boş alanlarla da basıyordu
    /// ("  · "); burada boş parça atlanır (K-076.2). Kimlik tamamsa çıktı birebir aynı.
    /// Prototipin "📎 ruhsat fotoğrafı eklenecek" satırı ekrandaki hatırlatma, mesaja girmez.
    /// </summary>
    public static string Compose(Vehicle v, IEnumerable<ServiceLine> lines)
    {
        static bool Has(string? s) => !string.IsNullOrWhiteSpace(s);
        var head = string.Join(" ", new[] { v.Brand, v.Model, v.Year }.Where(Has).Select(s => s!.Trim()));
        if (Has(v.Engine)) head = head.Length > 0 ? head + " · " + v.Engine!.Trim() : v.Engine!.Trim();
        var sb = new System.Text.StringBuilder();
        if (head.Length > 0) sb.Append(head).Append('\n');
        if (v.HasValidVin) sb.Append("Şase: ").Append(v.Vin!.Trim()).Append('\n');
        var items = string.Join("\n", lines.Select(l => "· " + l.Name));
        sb.Append(v.PlateNo).Append("\n\n").Append(items.Length > 0 ? items : "—").Append("\n\nFiyat verebilir misiniz?");
        return sb.ToString();
    }
}
