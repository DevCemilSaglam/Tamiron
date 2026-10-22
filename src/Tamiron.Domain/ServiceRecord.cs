namespace Tamiron.Domain;

/// <summary>
/// Servis kaydı — ürünün merkezi (VERI-MODELI §2). Tek durum alanı
/// <see cref="DeliveredAt"/>: boşsa araç serviste (K-004).
/// Para kuralları burada, saf: DB'siz test edilir (MIMARI §4.4).
/// </summary>
public class ServiceRecord : TenantEntity
{
    public Guid VehicleId { get; private set; }
    public ServiceType Type { get; set; }
    public bool TypeSetManually { get; set; }
    public string? Km { get; set; }
    public string Complaint { get; set; } = "";
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
    public bool ShowWaitingToCustomer { get; set; }

    private readonly List<ServiceLine> _lines = [];
    public IReadOnlyList<ServiceLine> Lines => _lines;

    private ServiceRecord() { }   // EF

    public ServiceRecord(Guid tenantId, Guid vehicleId, ServiceType type, string complaint)
    {
        TenantId = tenantId;
        VehicleId = vehicleId;
        Type = type;
        Complaint = complaint;
    }

    public void AddLine(ServiceLine line)
    {
        line.ServiceRecordId = Id;
        line.TenantId = TenantId;
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
        WaitingForPart = waiting;
        WaitingSince = waiting ? today : null;
    }
}
