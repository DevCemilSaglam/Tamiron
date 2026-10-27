namespace Tamiron.Domain;

public enum AdvanceKind { Received = 1, Refunded = 2 }

/// <summary>
/// Müşteri avansı hareketi (VERI-MODELI §2c, K-079.1, TASARIM-G-AVANS). Avans bir işe değil aracın
/// SAHİBİNE aittir; bakiye saklanmaz (<see cref="AdvanceBalance"/>). Alındığı gün kasaya girer, iade
/// edildiği gün kasadan çıkar; avanstan düşülen ödeme (PaymentMethod.Advance) kasaya ikinci kez girmez.
/// </summary>
public class Advance : TenantEntity
{
    /// <summary>Alındığı araç — bakiye sahibe göre toplanır (aynı ad + telefon).</summary>
    public Guid VehicleId { get; private set; }
    public AdvanceKind Kind { get; private set; }
    /// <summary>&gt; 0. Yön <see cref="Kind"/>'da.</summary>
    public decimal Amount { get; private set; }
    /// <summary>Nakit · kart · havale — kasanın hangi satırı.</summary>
    public PaymentMethod Method { get; private set; }
    /// <summary>Paranın geldiği / çıktığı gün.</summary>
    public DateOnly On { get; private set; }

    private Advance() { }   // EF

    private Advance(Guid tenantId, Guid vehicleId, AdvanceKind kind, decimal amount, PaymentMethod method, DateOnly on)
    {
        if (amount <= 0)
            throw new DomainException("advance.amount_required", "Avansın tutarını yaz.");
        if (method is not (PaymentMethod.Cash or PaymentMethod.Card or PaymentMethod.Transfer))
            throw new DomainException("advance.method_required", "Paranın nasıl verildiğini seç: nakit, kart ya da havale.");
        TenantId = tenantId; VehicleId = vehicleId; Kind = kind; Amount = amount; Method = method; On = on;
    }

    /// <summary>Müşteri önceden para verdi — o gün kasaya girer.</summary>
    public static Advance Receive(Guid tenantId, Guid vehicleId, decimal amount, PaymentMethod method, DateOnly on)
        => new(tenantId, vehicleId, AdvanceKind.Received, amount, method, on);

    /// <summary>Kullanılmayan avans geri verildi (K-079.1c) — o gün kasadan çıkar. Bakiyeden fazlası verilemez.</summary>
    public static Advance Refund(Guid tenantId, Guid vehicleId, decimal amount, PaymentMethod method, DateOnly on, decimal balance)
    {
        if (amount > balance)
            throw new DomainException("advance.over_balance", $"Avans {MessageTemplates.Money(balance)} ₺ — fazlası iade edilemez.");
        return new(tenantId, vehicleId, AdvanceKind.Refunded, amount, method, on);
    }

    /// <summary>Bakiyeye katkısı: alınan +, iade −.</summary>
    public decimal Signed => Kind == AdvanceKind.Received ? Amount : -Amount;

    /// <summary>
    /// Yanlış girişi geri al (soft-delete). Alınan avansı silmek bakiyeyi düşürür — avansın bir kısmı
    /// bir işe düşülmüşse önce o ödeme geri alınır (eksi bakiye = borç, o ayrı kavram).
    /// </summary>
    public void Undo(decimal balance)
    {
        if (Kind == AdvanceKind.Received && Amount > balance)
            throw new DomainException("advance.undo_would_go_negative",
                "Bu avansın bir kısmı bir işe düşülmüş — önce o ödemeyi geri al.");
        DateTimeDeleted = DateTime.UtcNow;
    }
}

/// <summary>Avans bakiyesi — tek hesap (saklanmaz, hareketlerle ayrışmasın).</summary>
public static class AdvanceBalance
{
    /// <param name="movements">Sahibin araçlarındaki hareketler (silinmişler sayılmaz).</param>
    /// <param name="usedInPayments">Sahibin işlerinde avanstan düşülen ödemelerin toplamı.</param>
    public static decimal Of(IEnumerable<Advance> movements, decimal usedInPayments)
        => movements.Where(a => a.DateTimeDeleted is null).Sum(a => a.Signed) - usedInPayments;

    /// <summary>Avanstan düşülecek tutar bakiyeyi aşamaz.</summary>
    public static void EnsureCovers(decimal balance, decimal amount)
    {
        if (amount > balance)
            throw new DomainException("advance.over_balance", $"Avans {MessageTemplates.Money(balance)} ₺ — fazlası düşülemez.");
    }
}
