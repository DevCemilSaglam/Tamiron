using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// B4 · Kayıt bayrakları — prototip 30-kayit: şikâyet girişi (tür takibi),
/// data-kayit-tur (elle tür), kBekliyor / kBekliyorMusteriye (K-034, A-M15), km girişi.
/// </summary>
public class RecordFlagsTests
{
    static readonly DateOnly Today = new(2026, 10, 22);

    [Fact]
    public void New_record_guesses_type_from_complaint_and_is_not_manual()
    {
        var r = new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), "Periyodik bakım");
        Assert.Equal(ServiceType.Maintenance, r.Type);
        Assert.False(r.TypeSetManually);
    }

    [Fact]
    public void Type_follows_complaint_until_set_by_hand_then_hand_wins()
    {
        var r = new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), "");
        r.SetComplaint("debriyaj kaçırıyor");
        Assert.Equal(ServiceType.Repair, r.Type);
        r.SetComplaint("yağ bakımı");
        Assert.Equal(ServiceType.Maintenance, r.Type);   // takip ediyor

        r.SetType(ServiceType.Repair);
        Assert.True(r.TypeSetManually);
        r.SetComplaint("periyodik bakım");
        Assert.Equal(ServiceType.Repair, r.Type);         // elle seçim kalıcı olarak yener
    }

    [Fact]
    public void Manual_type_at_open_is_marked_manual()
    {
        var r = new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), "yağ bakımı", manualType: ServiceType.Repair);
        Assert.Equal(ServiceType.Repair, r.Type);
        Assert.True(r.TypeSetManually);
    }

    [Fact]
    public void Waiting_for_part_records_the_day_and_clearing_it_clears_customer_visibility()
    {
        var r = new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), "Triger");
        r.SetWaitingForPart(true, Today);
        r.SetShowWaitingToCustomer(true);
        Assert.Equal(Today, r.WaitingSince);

        r.SetWaitingForPart(false, Today.AddDays(2));
        Assert.Null(r.WaitingSince);
        Assert.False(r.ShowWaitingToCustomer);   // bayrak kalkınca seçim anlamını yitirir
    }

    [Fact]
    public void Cannot_show_waiting_to_customer_when_not_waiting()
    {
        var r = new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), "Triger");
        var e = Assert.Throws<DomainException>(() => r.SetShowWaitingToCustomer(true));
        Assert.Equal("record.not_waiting", e.Code);
    }

    [Theory]
    [InlineData("86400", "86.400")]
    [InlineData("151.200 km", "151.200")]
    [InlineData("0086400", "86.400")]
    [InlineData("123456789", "1.234.567")]   // 7 hane yeter
    [InlineData("", null)]
    [InlineData("km yok", null)]
    public void Km_is_normalized(string raw, string? expected)
    {
        var r = new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), "bakım");
        r.SetKm(raw);
        Assert.Equal(expected, r.Km);
    }
}
