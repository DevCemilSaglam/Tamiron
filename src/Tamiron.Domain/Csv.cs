using System.Globalization;
using System.Text;

namespace Tamiron.Domain;

/// <summary>
/// Muhasebeciye giden CSV — prototipin <c>csvHucre</c>'si (K-071.2):
///  · BOM: yoksa Excel Türkçe harfleri bozuk açar;
///  · ";" ayraç: Türkçe Excel'de virgül ondalık işaretidir;
///  · HAM rakam: "3.450" Excel'de metin olur ve toplanamaz;
///  · "=", "+", "-", "@" ile başlayan metnin başına ': gider açıklamasını usta
///    yazıyor; Excel onu formül diye ÇALIŞTIRIR.
/// </summary>
public static class Csv
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static string Cell(object? v) => v switch
    {
        null => "",
        decimal d => d.ToString("0.############", Inv).Replace('.', ','),
        int i => i.ToString(Inv),
        DateOnly t => t.ToString("dd.MM.yyyy", Inv),
        _ => Text(v.ToString() ?? "")
    };

    private static string Text(string s)
    {
        if (s.Length > 0 && "=+-@\t\r".Contains(s[0])) s = "'" + s;
        return s.IndexOfAny([';', '"', '\r', '\n']) >= 0 ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
    }

    public static string Document(IEnumerable<IEnumerable<object?>> rows)
    {
        var sb = new StringBuilder("\uFEFF");
        foreach (var r in rows) sb.Append(string.Join(';', r.Select(Cell))).Append("\r\n");
        return sb.ToString();
    }

    /// <summary>Ödeme yolunun ekrandaki adı (prototip ODEME_ETIKET).</summary>
    public static string PaymentLabel(PaymentMethod? m) => m switch
    {
        PaymentMethod.Cash => "Nakit",
        PaymentMethod.Card => "Kart",
        PaymentMethod.Transfer => "Havale",
        PaymentMethod.Unpaid => "Alınmadı",
        _ => ""
    };
}
