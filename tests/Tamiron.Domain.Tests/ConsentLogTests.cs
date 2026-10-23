using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// D5 · İzin geçmişi (K-076.5, K-073.10'u kapatır). Prototip 91-musteri: kaldırılan izin
/// "Geri al" ile ESKİ TARİHİYLE döner — tarih istemciden değil, kayıttan gelir.
/// </summary>
public class ConsentLogTests
{
    static readonly DateOnly Today = new(2026, 10, 22);
    static readonly DateOnly Old = new(2026, 6, 14);
    static readonly Guid User = Guid.NewGuid();

    static Customer Cust(DateOnly? consent = null)
    {
        var a = new Vehicle { PlateNo = "34 A 1", OwnerName = "Ayşe", OwnerPhone = "0533 111 11 11", MarketingConsentAt = consent };
        var b = new Vehicle { PlateNo = "34 B 2", OwnerName = "Ayşe", OwnerPhone = "0533 111 11 11", MarketingConsentAt = consent };
        return Customers.From([a, b]).Single();
    }

    [Fact]
    public void Every_change_is_logged_per_vehicle_with_before_and_after()
    {
        var c = Cust(Old);
        var log = c.SetConsent(false, Today, User);
        Assert.Equal(2, log.Count);
        Assert.All(log, e =>
        {
            Assert.False(e.Granted);
            Assert.Equal((Old, (DateOnly?)null), (e.PreviousDate!.Value, e.ConsentDate));
            Assert.Equal(("0533 111 11 11", User), (e.Phone, e.UserId));
            Assert.False(e.IsUndo);
        });
        Assert.Single(log.Select(e => e.ActionId).Distinct());   // tek dokunuş, tek eylem
    }

    [Fact]
    public void Second_yes_changes_nothing_and_logs_nothing()
        => Assert.Empty(Cust(Old).SetConsent(true, Today, User));

    [Fact]
    public void Undo_after_revoke_brings_back_the_old_date_from_the_log()
    {
        var c = Cust(Old);
        var log = c.SetConsent(false, Today, User).ToList();
        var undo = c.UndoConsent(log, Today, User);
        Assert.Equal(Old, c.ConsentAt);                               // bugün DEĞİL
        Assert.All(c.Vehicles, v => Assert.Equal(Old, v.MarketingConsentAt));
        Assert.All(undo, e => Assert.True(e.IsUndo && e.Granted));
    }

    [Fact]
    public void Undo_after_grant_removes_it()
    {
        var c = Cust();
        var log = c.SetConsent(true, Today, User).ToList();
        c.UndoConsent(log, Today, User);
        Assert.Null(c.ConsentAt);
    }

    [Fact]
    public void Only_the_last_action_is_undone_and_only_once()
    {
        var c = Cust(Old);
        var log = c.SetConsent(false, Today, User).ToList();
        log.AddRange(c.SetConsent(true, Today, User));       // yeniden verildi: bugünün tarihi
        log.AddRange(c.UndoConsent(log, Today, User));       // son eylem (verme) geri alındı → yok
        Assert.Null(c.ConsentAt);
        Assert.Equal("consent.nothing_to_undo", Assert.Throws<DomainException>(() => c.UndoConsent(log, Today, User)).Code);
    }

    [Fact]
    public void Yesterdays_action_cannot_be_undone()
    {
        var c = Cust(Old);
        var log = c.SetConsent(false, Today, User).ToList();
        Assert.Equal("consent.nothing_to_undo", Assert.Throws<DomainException>(() => c.UndoConsent(log, Today.AddDays(1), User)).Code);
    }

    [Fact]
    public void Nothing_logged_nothing_to_undo()
        => Assert.Equal("consent.nothing_to_undo", Assert.Throws<DomainException>(() => Cust(Old).UndoConsent([], Today, User)).Code);
}
