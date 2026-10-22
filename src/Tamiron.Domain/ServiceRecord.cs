namespace Tamiron.Domain;

/// <summary>
/// Servis kaydı — ürünün merkezi (VERI-MODELI §2). Tek durum alanı
/// <see cref="DeliveredAt"/>: boşsa araç serviste (K-004).
/// Para kuralları burada, saf: DB'siz test edilir (MIMARI §4.4).
/// </summary>
public class ServiceRecord : TenantEntity
{
    public Guid VehicleId { get; private set; }
    public ServiceType Type { get; private set; }
    public bool TypeSetManually { get; private set; }
    /// <summary>Binlik noktalı ("86.400"); yalnız bakımda istenir (K-002).</summary>
    public string? Km { get; private set; }
    public string Complaint { get; private set; } = "";
    // Açılış günü AYRI ALAN DEĞİL (VERI-MODELI §2: "OpenedAt YOK"): ortak
    // DateTimeCreated'tır. Türkiye saati sabit UTC+3, gün oradan kesin türer.
    public DateOnly? DeliveredAt { get; private set; }
    public PaymentMethod? PaymentMethod { get; private set; }
    /// <summary>Veresiye sonradan ödendiyse ödendiği gün (K-071.1). Teslimde ödendiyse boş.</summary>
    public DateOnly? PaymentCollectedAt { get; private set; }
    public DateOnly? ReminderDate { get; set; }
    public bool ReminderSent { get; set; }
    public bool WaitingForPart { get; private set; }
    public DateOnly? WaitingSince { get; private set; }
    public bool ShowWaitingToCustomer { get; private set; }

    private readonly List<ServiceLine> _lines = [];
    public IReadOnlyList<ServiceLine> Lines => _lines;

    private ServiceRecord() { }   // EF

    /// <summary>
    /// Tür ekranda sorulmaz (Cemil, 06.09): verildiyse (sesin çözdüğü ya da
    /// ustanın seçtiği) "elle"dir; verilmediyse şikâyetten TAHMİN edilir.
    /// </summary>
    public ServiceRecord(Guid tenantId, Guid vehicleId, string complaint, ServiceType? manualType = null)
    {
        TenantId = tenantId;
        VehicleId = vehicleId;
        SetComplaint(complaint);
        if (manualType is { } t) SetType(t);
    }

    /// <summary>Şikâyet değişince tür onu TAKİP EDER — usta elle seçene kadar.</summary>
    public void SetComplaint(string? text)
    {
        Complaint = (text ?? "").Trim();
        if (!TypeSetManually) Type = ServiceTypeGuess.FromText(Complaint);
    }

    /// <summary>Elle seçim tahmini KALICI olarak yener.</summary>
    public void SetType(ServiceType type)
    {
        Type = type;
        TypeSetManually = true;
    }

    /// <summary>Km: rakam dışı atılır, en çok 7 hane, binlik noktalı (prototip GIRIS_TEMIZ.km). Boşsa yok.</summary>
    public void SetKm(string? raw)
    {
        var d = new string((raw ?? "").Where(char.IsAsciiDigit).ToArray()).TrimStart('0');
        if (d.Length > 7) d = d[..7];
        Km = d.Length == 0 ? null : long.Parse(d).ToString("#,0", System.Globalization.CultureInfo.GetCultureInfo("tr-TR"));
    }

    /// <summary>Parça beklendiği müşteriye söylensin mi — ustanın seçimi (A-M15); yalnız bayrak açıkken.</summary>
    public void SetShowWaitingToCustomer(bool show)
    {
        if (show && !WaitingForPart)
            throw new DomainException("record.not_waiting", "Önce \"parça bekliyor\" işaretle.");
        ShowWaitingToCustomer = show;
    }

    public void AddLine(ServiceLine line)
    {
        line.ServiceRecordId = Id;
        line.TenantId = TenantId;
        // Silinmiş satırlar yüklenmez; yeni satır onların sırasını alabilir. Geri
        // alınan satırla eşit sıra çıkarsa oluşturulma anı ayırır.
        line.Position = _lines.Count == 0 ? 1 : _lines.Max(l => l.Position) + 1;
        _lines.Add(line);
    }

    /// <summary>Kayıt toplamı — satırlardan hesaplanır, saklanmaz (K-037).</summary>
    public decimal Total => _lines.Where(l => l.CountsInTotal).Sum(l => l.Price);

    /// <summary>
    /// Tahsil günü (K-071.1): para kasaya hangi gün girdi. Ödenmediyse yok;
    /// sonradan ödendiyse ödendiği gün; teslimde ödendiyse teslim günü.
    /// Muhasebenin dönem tahsilatı bu güne göre sayar.
    /// </summary>
    public DateOnly? CollectedOn =>
        PaymentMethod is null or Domain.PaymentMethod.Unpaid ? null : PaymentCollectedAt ?? DeliveredAt;

    public bool IsMaintenance => Type is ServiceType.Maintenance or ServiceType.Both;

    public void Deliver(PaymentMethod method, DateOnly on)
    {
        if (DeliveredAt is not null)
            throw new DomainException("record.already_delivered", "Bu kayıt zaten teslim edildi.");
        DeliveredAt = on;
        PaymentMethod = method;
    }

    /// <summary>"Almadım" denmiş kaydın parası geldi (kayıt ekranındaki "Ödeme alındı").</summary>
    public void CollectPayment(PaymentMethod method, DateOnly on)
    {
        if (PaymentMethod != Domain.PaymentMethod.Unpaid || DeliveredAt is null)
            throw new DomainException("payment.not_unpaid", "Bu kaydın alınmamış ödemesi yok.");
        if (method == Domain.PaymentMethod.Unpaid)
            throw new DomainException("payment.method_required", "Paranın nasıl alındığını seç: nakit, kart ya da havale.");
        if (on < DeliveredAt.Value)
            throw new DomainException("payment.before_delivery", "Ödeme teslimden önceki bir güne yazılamaz.");
        PaymentMethod = method;
        PaymentCollectedAt = on;
    }

    /// <summary>"Geri al" — yanlış çipe dokunmak kasaya olmayan parayı yazmasın.</summary>
    public void UndoCollection()
    {
        if (PaymentCollectedAt is null)
            throw new DomainException("payment.nothing_to_undo", "Geri alınacak bir tahsilat yok.");
        PaymentMethod = Domain.PaymentMethod.Unpaid;
        PaymentCollectedAt = null;
    }

    public void SetWaitingForPart(bool waiting, DateOnly today)
    {
        // Bayrak zaten açıksa tarih DEĞİŞMEZ: ikinci "evet" beklemenin başladığı günü silmesin.
        if (waiting && WaitingForPart) return;
        WaitingForPart = waiting;
        WaitingSince = waiting ? today : null;
        // Bayrak kapanınca müşteriye gösterme seçimi de anlamını yitirir (prototip kBekliyor).
        if (!waiting) ShowWaitingToCustomer = false;
    }
}
