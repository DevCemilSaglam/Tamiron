namespace Tamiron.Domain;

/// <summary>
/// H3 · Parçacıya ödeme (VERI-MODELI §4b, K-090, TASARIM-H-MUHASEBE). Borç saklanmaz
/// (<see cref="SupplierDebt"/>). Gider DEĞİLDİR — alış iş başına farktan zaten düşülüyor; kasa
/// tahsilatına girmez; CSV'de kendi Türüyle ("Parçacıya ödeme").
/// </summary>
public class SupplierPayment : TenantEntity
{
    public Guid SupplierId { get; private set; }
    public DateOnly PaidOn { get; private set; }
    /// <summary>&gt; 0, ödendiği anki borcu aşamaz.</summary>
    public decimal Amount { get; private set; }
    /// <summary>Nakit · kart · havale.</summary>
    public PaymentMethod Method { get; private set; }

    private SupplierPayment() { }   // EF

    /// <summary>Borçtan fazlası ödenemez — fazlası parçacıdan alacak olurdu, o ayrı kavram (cari).</summary>
    public static SupplierPayment Pay(Guid tenantId, Guid supplierId, decimal amount, PaymentMethod method, DateOnly on, decimal debt)
    {
        if (amount <= 0)
            throw new DomainException("supplier_payment.amount_required", "Ödenen tutarı yaz.");
        if (method is not (PaymentMethod.Cash or PaymentMethod.Card or PaymentMethod.Transfer))
            throw new DomainException("supplier_payment.method_required", "Nasıl ödediğini seç: nakit, kart ya da havale.");
        if (amount > debt)
            throw new DomainException("supplier_payment.over_debt", $"Borç {MessageTemplates.Money(debt)} ₺ — fazlası ödenemez.");
        return new SupplierPayment { TenantId = tenantId, SupplierId = supplierId, Amount = amount, Method = method, PaidOn = on };
    }

    /// <summary>Yanlış girişi geri al (soft-delete) — borç geri artar.</summary>
    public void Undo() => DateTimeDeleted = DateTime.UtcNow;
}

/// <summary>Parçacıya borç — tek hesap (saklanmaz, hareketlerle ayrışmasın).</summary>
public static class SupplierDebt
{
    /// <param name="lines">Satırlar (silinmişler ve veresiye olmayanlar sayılmaz).</param>
    /// <param name="payments">Ödemeler (silinmişler sayılmaz).</param>
    public static decimal Of(IEnumerable<ServiceLine> lines, IEnumerable<SupplierPayment> payments, Guid supplierId)
        => lines.Where(l => l.DateTimeDeleted is null && l.OnCredit && l.SupplierId == supplierId).Sum(l => l.Cost ?? 0)
         - payments.Where(p => p.DateTimeDeleted is null && p.SupplierId == supplierId).Sum(p => p.Amount);
}
