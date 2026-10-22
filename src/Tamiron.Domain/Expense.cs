namespace Tamiron.Domain;

/// <summary>
/// Dükkânın gideri (VERI-MODELI §9): kira, elektrik, çay ocağı. Araca bağlı değil.
/// Yedek parça gider DEĞİLDİR — ServiceLine.Cost'ta durur ve iş başına farktan
/// düşülür (Cemil, 05.09); ikisine yazılırsa aynı para iki kez düşer.
/// </summary>
public class Expense : TenantEntity
{
    public DateOnly Date { get; private set; }
    public string Description { get; private set; } = "";
    public decimal Amount { get; private set; }

    private Expense() { }   // EF

    public static Expense Create(Guid tenantId, string description, decimal amount, DateOnly on)
    {
        description = (description ?? "").Trim();
        if (description.Length == 0 || amount <= 0)
            throw new DomainException("expense.incomplete", "Gider için hem açıklama hem tutar gerekiyor.");
        return new Expense { TenantId = tenantId, Description = description, Amount = amount, Date = on };
    }
}
