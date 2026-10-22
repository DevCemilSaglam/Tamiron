using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>B8 · Gider — prototip 95-muhasebe "Gider ekle": açıklama VE tutar gerekir.</summary>
public class ExpenseTests
{
    static readonly DateOnly Today = new(2026, 10, 22);

    [Fact]
    public void Valid_expense_is_trimmed()
    {
        var e = Expense.Create(Guid.NewGuid(), "  Elektrik faturası ", 2400, Today);
        Assert.Equal(("Elektrik faturası", 2400m, Today), (e.Description, e.Amount, e.Date));
    }

    [Theory]
    [InlineData("", 100)]
    [InlineData("Kira", 0)]
    [InlineData("Kira", -5)]
    public void Description_and_positive_amount_required(string d, decimal a)
    {
        var e = Assert.Throws<DomainException>(() => Expense.Create(Guid.NewGuid(), d, a, Today));
        Assert.Equal("expense.incomplete", e.Code);
        Assert.Equal("Gider için hem açıklama hem tutar gerekiyor.", e.Message);
    }
}
