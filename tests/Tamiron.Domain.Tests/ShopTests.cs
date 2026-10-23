using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>C5 · Dükkân ayarları — prototip 99-ayarlar.</summary>
public class ShopTests
{
    [Theory]
    [InlineData(3)]
    [InlineData(6)]
    [InlineData(12)]
    public void Period_choices(int m)
    {
        var s = new Shop();
        s.SetMaintenanceMonths(m);
        Assert.Equal(m, s.DefaultMaintenanceMonths);
    }

    [Fact]
    public void Other_periods_are_rejected()
        => Assert.Equal("shop.period_invalid", Assert.Throws<DomainException>(() => new Shop().SetMaintenanceMonths(4)).Code);

    [Fact]
    public void Privacy_notice_keeps_first_time()
    {
        var s = new Shop();
        var t = new DateTime(2026, 10, 22, 9, 0, 0, DateTimeKind.Utc);
        s.MarkPrivacyNoticeShown(t);
        s.MarkPrivacyNoticeShown(t.AddDays(1));
        Assert.Equal(t, s.PrivacyNoticeShownAt);
    }

    [Fact]
    public void Register_trims_name_and_needs_one()
    {
        var s = Shop.Register("  Yıldız Oto ", "0544 123 00 01");
        Assert.Equal(("Yıldız Oto", "0544 123 00 01", 6), (s.Name, s.Phone, s.DefaultMaintenanceMonths));
        Assert.Equal("shop.name_required", Assert.Throws<DomainException>(() => Shop.Register("  ", "0544 123 00 01")).Code);
    }
}
