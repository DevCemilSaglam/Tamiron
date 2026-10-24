namespace Tamiron.Domain;

/// <summary>
/// Alınan ödeme (VERI-MODELI §2b, K-078). Bir işin parası birden çok parçada gelebilir —
/// Cemil: "elden taksit de x miktar aldım şeklinde olur". Cari hesap YOK: ödeme müşteriye
/// değil işe aittir. Kurallar <see cref="ServiceRecord"/>'da; burası yalnız olgu.
/// </summary>
public class Payment : TenantEntity
{
    public Guid ServiceRecordId { get; set; }
    /// <summary>&gt; 0, alındığı an kalandan fazla değil.</summary>
    public decimal Amount { get; private set; }
    /// <summary>Nakit · kart · havale — "almadım" ödeme değildir.</summary>
    public PaymentMethod Method { get; private set; }
    /// <summary>Paranın geldiği gün: kasaya bu güne yazılır, CSV'de kendi satırı.</summary>
    public DateOnly PaidOn { get; private set; }
    /// <summary>
    /// Teslimde mi alındı. Teslimdeki para teslimle gider (teslimi geri al); sonradan gelen
    /// "ödemeyi geri al"la — yanlış teslimi geri alan usta sonraki tahsilatı farkında olmadan silmesin.
    /// </summary>
    public bool AtDelivery { get; private set; }

    private Payment() { }   // EF

    internal Payment(ServiceRecord r, decimal amount, PaymentMethod method, DateOnly paidOn, bool atDelivery)
    {
        ServiceRecordId = r.Id;
        TenantId = r.TenantId;
        Amount = amount;
        Method = method;
        PaidOn = paidOn;
        AtDelivery = atDelivery;
    }

    internal void Correct(PaymentMethod method) => Method = method;
}
