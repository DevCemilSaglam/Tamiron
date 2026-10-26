namespace Tamiron.Ui.Core;

/// <summary>
/// Ana menünün selamlaması — prototip SELAMLAR / selamMetni (01-cekirdek.js) birebir: saatsizler her
/// zaman, saatliler kendi penceresinde (23→5 gece sarar), cuma yalnız cuma. Havuzdan rastgele.
/// </summary>
public static class Selam
{
    const string Hitap = "usta";

    static readonly (string Metin, int? A, int? B, DayOfWeek? Gun)[] Havuz =
    [
        ("Hayırlı işler", null, null, null),
        ("Bol kazançlar", null, null, null),
        ("Kolay gelsin", null, null, null),
        ("Bereketli olsun", null, null, null),
        ("Günaydın", 5, 11, null),
        ("Hayırlı sabahlar", 5, 11, null),
        ("İyi günler", 11, 18, null),
        ("İyi akşamlar", 18, 23, null),
        ("Hayırlı akşamlar", 18, 23, null),
        ("Hayırlı geceler", 23, 5, null),
        ("Hayırlı cumalar", 5, 23, DayOfWeek.Friday),
    ];

    public static IReadOnlyList<string> Uygunlar(DateTime an)
    {
        var saat = an.Hour;
        var l = Havuz.Where(s => (s.Gun is null || s.Gun == an.DayOfWeek)
                              && (s.A is not { } a || s.B is not { } b || (a < b ? saat >= a && saat < b : saat >= a || saat < b)))
                     .Select(s => s.Metin).ToList();
        return l.Count > 0 ? l : [Havuz[0].Metin];
    }

    public static string Metni(DateTime an, Random? r = null)
    {
        var u = Uygunlar(an);
        return u[(r ?? Random.Shared).Next(u.Count)] + ", " + Hitap;
    }
}
