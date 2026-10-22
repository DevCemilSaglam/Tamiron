namespace Tamiron.Domain;

/// <summary>Dükkânın bakım periyodu (Shop.DefaultMaintenanceMonths / Km).</summary>
public sealed record MaintenancePolicy(int Months, int Km)
{
    public static readonly MaintenancePolicy Default = new(6, 10_000);
    public static MaintenancePolicy Of(Shop s) => new(s.DefaultMaintenanceMonths, s.DefaultMaintenanceKm);
}

/// <summary>Bir km ölçümü. Ayrı tablo YOK: km servis kaydında durur, geçmiş kayıtlardan türer (prototip kmGecmisi).</summary>
public readonly record struct KmReading(DateOnly Date, int Km)
{
    /// <summary>"151.200" → 151200; rakam yoksa null.</summary>
    public static int? Parse(string? km)
    {
        var d = new string((km ?? "").Where(char.IsAsciiDigit).ToArray());
        return d.Length == 0 || !int.TryParse(d, out var v) || v <= 0 ? null : v;
    }
}

public enum ReminderBasis { Month = 1, Km = 2 }

public sealed record ReminderPlan(DateOnly Date, ReminderBasis Basis, DateOnly MonthDate, int? KmTarget, DateOnly? KmDate);

/// <summary>
/// Bakım hatırlatması: ay VEYA km, hangisi önce gelirse (Cemil, 03.09) —
/// prototipin <c>hatirlatmaHesapla</c>'sı. Km tarihi ANCAK iki ölçümle
/// tahmin edilir: tek ölçümle günlük km uydurmak ustayı yanlış güne çağırır.
/// </summary>
public static class MaintenanceReminder
{
    public static ReminderPlan Compute(DateOnly from, int? km, MaintenancePolicy policy, IEnumerable<KmReading> history)
    {
        // ⚠️ BİLİNÇLİ SAPMA (K-073.9): DateOnly.AddMonths ay sonunu SABİTLER
        // (31.08 + 6 ay = 28.02). Prototipteki JS setMonth taşırıyordu (03.03).
        var monthDate = from.AddMonths(policy.Months);
        if (km is not > 0) return new(monthDate, ReminderBasis.Month, monthDate, null, null);

        var target = km.Value + policy.Km;
        var perDay = DailyKm(history);
        if (perDay <= 0) return new(monthDate, ReminderBasis.Month, monthDate, target, null);

        // JS Math.round ile aynı: yarım yukarı.
        var days = (int)Math.Round(policy.Km / perDay, MidpointRounding.AwayFromZero);
        var kmDate = from.AddDays(days);
        return kmDate < monthDate
            ? new(kmDate, ReminderBasis.Km, monthDate, target, kmDate)
            : new(monthDate, ReminderBasis.Month, monthDate, target, kmDate);
    }

    /// <summary>İlk ve son ölçüm arasındaki günlük ortalama (prototip gunlukKm). İki ölçüm yoksa 0.</summary>
    public static double DailyKm(IEnumerable<KmReading> history)
    {
        var g = history.Where(h => h.Km > 0).OrderBy(h => h.Date).ToList();
        if (g.Count < 2) return 0;
        var days = g[^1].Date.DayNumber - g[0].Date.DayNumber;
        return days <= 0 ? 0 : (double)(g[^1].Km - g[0].Km) / days;
    }
}
