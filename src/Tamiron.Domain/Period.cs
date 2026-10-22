namespace Tamiron.Domain;

public enum PeriodKind { Today = 1, Month = 2, Year = 3 }

/// <summary>Muhasebe dönemi: başlangıçtan BUGÜNE (dahil). Prototipte <c>donemBasi</c>.</summary>
public readonly record struct Period(DateOnly Start, DateOnly End)
{
    public static Period Of(PeriodKind kind, DateOnly today) => kind switch
    {
        PeriodKind.Today => new(today, today),
        PeriodKind.Month => new(new DateOnly(today.Year, today.Month, 1), today),
        PeriodKind.Year => new(new DateOnly(today.Year, 1, 1), today),
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    public bool Contains(DateOnly day) => day >= Start && day <= End;
}
