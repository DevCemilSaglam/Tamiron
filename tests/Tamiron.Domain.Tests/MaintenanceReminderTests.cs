using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// B5 · Bakım hatırlatması — prototip <c>hatirlatmaHesapla</c> / <c>gunlukKm</c>:
/// ay VEYA km, hangisi önce gelirse (Cemil, 03.09). Km tahmini ancak İKİ ölçümle.
/// </summary>
public class MaintenanceReminderTests
{
    static readonly MaintenancePolicy Policy = new(Months: 6, Km: 10_000);

    [Fact]
    public void Prototype_example_km_comes_first()
    {
        // Prototip 34 ABC 123: 15.03'te 142.500, 20.06'da 151.200 → 97 günde 8.700 km.
        // 10.000 km / (8700/97) = 111,49 gün → 111 → 09.10.2026; ay tarihi 20.12.2026.
        var r = MaintenanceReminder.Compute(new DateOnly(2026, 6, 20), 151_200, Policy,
            [new(new DateOnly(2026, 3, 15), 142_500), new(new DateOnly(2026, 6, 20), 151_200)]);
        Assert.Equal(new DateOnly(2026, 12, 20), r.MonthDate);
        Assert.Equal(new DateOnly(2026, 10, 9), r.KmDate);
        Assert.Equal(161_200, r.KmTarget);
        Assert.Equal(ReminderBasis.Km, r.Basis);
        Assert.Equal(new DateOnly(2026, 10, 9), r.Date);
    }

    [Fact]
    public void Single_reading_never_guesses_km_month_alone_works()
    {
        var r = MaintenanceReminder.Compute(new DateOnly(2026, 6, 20), 151_200, Policy,
            [new(new DateOnly(2026, 6, 20), 151_200)]);
        Assert.Null(r.KmDate);
        Assert.Equal(161_200, r.KmTarget);         // hedef söylenir, tarihi uydurulmaz
        Assert.Equal(ReminderBasis.Month, r.Basis);
        Assert.Equal(new DateOnly(2026, 12, 20), r.Date);
    }

    [Fact]
    public void No_km_no_target()
    {
        var r = MaintenanceReminder.Compute(new DateOnly(2026, 6, 20), null, Policy, []);
        Assert.Null(r.KmTarget);
        Assert.Equal(new DateOnly(2026, 12, 20), r.Date);
    }

    [Fact]
    public void Month_end_is_clamped_not_overflowed()
    {
        // ⚠️ Prototipten BİLİNÇLİ sapma (K-073.9): JS setMonth 31.08 + 6 ayı 03.03'e taşırıyor.
        var r = MaintenanceReminder.Compute(new DateOnly(2026, 8, 31), null, Policy, []);
        Assert.Equal(new DateOnly(2027, 2, 28), r.Date);
    }

    [Fact]
    public void Slow_driver_month_comes_first()
    {
        // 100 günde 1.000 km → 10.000 km 1.000 gün sürer; 6 ay önce gelir.
        var r = MaintenanceReminder.Compute(new DateOnly(2026, 6, 20), 51_000, Policy,
            [new(new DateOnly(2026, 3, 12), 50_000), new(new DateOnly(2026, 6, 20), 51_000)]);
        Assert.Equal(ReminderBasis.Month, r.Basis);
        Assert.Equal(new DateOnly(2026, 12, 20), r.Date);
    }

    [Fact]
    public void Half_day_rounds_up_like_javascript_math_round()
    {
        // 1 günde 2 km, periyot 201 km → 100,5 gün. JS Math.round → 101; C#'ın
        // varsayılanı (çifte yuvarlama) 100 derdi ve prototiple bir gün ayrışırdı.
        var d = new DateOnly(2026, 6, 20);
        var r = MaintenanceReminder.Compute(d, 1_002, new MaintenancePolicy(Months: 12, Km: 201),
            [new(d.AddDays(-1), 1_000), new(d, 1_002)]);
        Assert.Equal(d.AddDays(101), r.KmDate);
    }

    [Fact]
    public void Same_day_readings_do_not_divide_by_zero()
    {
        var d = new DateOnly(2026, 6, 20);
        var r = MaintenanceReminder.Compute(d, 51_000, Policy, [new(d, 50_000), new(d, 51_000)]);
        Assert.Null(r.KmDate);
    }

    // ---- Teslim ----

    [Fact]
    public void Delivering_maintenance_sets_reminder_repair_does_not()
    {
        var on = new DateOnly(2026, 6, 20);
        var bakim = new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), "Ara bakım");
        bakim.SetKm("151.200");
        bakim.Deliver(PaymentMethod.Transfer, on, Policy, [new(new DateOnly(2026, 3, 15), 142_500)]);
        Assert.Equal(new DateOnly(2026, 10, 9), bakim.ReminderDate);   // bu kaydın ölçümü de geçmişe katılır
        Assert.False(bakim.ReminderSent);

        var tamir = new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), "Fren");
        tamir.Deliver(PaymentMethod.Cash, on, Policy, []);
        Assert.Null(tamir.ReminderDate);
    }

    [Fact]
    public void Delivering_twice_is_rejected()
    {
        var r = new ServiceRecord(Guid.NewGuid(), Guid.NewGuid(), "Fren");
        r.Deliver(PaymentMethod.Cash, new DateOnly(2026, 6, 20), Policy, []);
        var e = Assert.Throws<DomainException>(() => r.Deliver(PaymentMethod.Cash, new DateOnly(2026, 6, 21), Policy, []));
        Assert.Equal("record.already_delivered", e.Code);
    }
}
