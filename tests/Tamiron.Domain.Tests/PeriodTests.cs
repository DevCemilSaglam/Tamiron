using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// Muhasebenin dönemleri — prototipin <c>donemBasi</c> / <c>donemIcinde</c>:
/// bugün · bu ay (ayın 1'inden bugüne) · bu yıl (1 Ocak'tan bugüne).
/// "Hepsi" yok: dükkânın açılışından beri toplam ustanın sorduğu soru değil.
/// </summary>
public class PeriodTests
{
    static readonly DateOnly Today = new(2026, 10, 22);

    [Theory]
    [InlineData(PeriodKind.Today, 2026, 10, 22)]
    [InlineData(PeriodKind.Month, 2026, 10, 1)]
    [InlineData(PeriodKind.Year, 2026, 1, 1)]
    public void Start_of_period(PeriodKind kind, int y, int m, int d)
        => Assert.Equal(new DateOnly(y, m, d), Period.Of(kind, Today).Start);

    [Fact]
    public void Period_ends_today_future_is_not_included()
    {
        var p = Period.Of(PeriodKind.Month, Today);
        Assert.True(p.Contains(Today));
        Assert.False(p.Contains(Today.AddDays(1)));
        Assert.False(p.Contains(new DateOnly(2026, 9, 30)));
    }
}
