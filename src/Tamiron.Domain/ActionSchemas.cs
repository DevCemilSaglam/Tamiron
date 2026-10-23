using System.Text.Json;
using System.Text.RegularExpressions;

namespace Tamiron.Domain;

/// <summary>
/// Sesli işlem şemaları (D15 · K-076.14) — prototipin <c>Islem.json()</c> çıktısı (araç tanımı
/// biçiminde: name · input_schema.properties · required). Tek kaynak prototip; dosyayı
/// <c>node tools/ses-sema-dok.js</c> üretir. Burada yalnız OKUNUR ve işlem nesnesi sınanır.
/// </summary>
public sealed class ActionSchemas
{
    private sealed record Field(string Type, IReadOnlyList<string>? Enum, Regex? Pattern);
    private sealed record Schema(IReadOnlyDictionary<string, Field> Fields, IReadOnlyList<string> Required);

    private readonly Dictionary<string, Schema> _all;
    /// <summary>Şema adları, prototipteki sırayla (alfabetik).</summary>
    public IReadOnlyList<string> Types { get; }

    private ActionSchemas(Dictionary<string, Schema> all, List<string> types) => (_all, Types) = (all, types);

    public static ActionSchemas Parse(string json)
    {
        var all = new Dictionary<string, Schema>();
        var types = new List<string>();
        foreach (var s in JsonDocument.Parse(json).RootElement.EnumerateArray())
        {
            var name = s.GetProperty("name").GetString()!;
            var input = s.GetProperty("input_schema");
            var fields = new Dictionary<string, Field>();
            foreach (var p in input.GetProperty("properties").EnumerateObject())
            {
                var v = p.Value;
                fields[p.Name] = new Field(
                    v.GetProperty("type").GetString()!,
                    v.TryGetProperty("enum", out var e) ? e.EnumerateArray().Select(x => x.GetString()!).ToList() : null,
                    v.TryGetProperty("pattern", out var pt) ? new Regex(pt.GetString()!, RegexOptions.CultureInvariant) : null);
            }
            all[name] = new Schema(fields, input.GetProperty("required").EnumerateArray().Select(x => x.GetString()!).ToList());
            types.Add(name);
        }
        return new ActionSchemas(all, types);
    }

    /// <summary>
    /// İşlem nesnesini şemasına karşı sınar; hatalar "alan:sorun" biçiminde (ör. "tutar:zorunlu").
    /// Kurallar prototipin alan tipleriyle aynı: para (integer) 0 = SÖYLENMEDİ, boş sayılır
    /// (TIP.para.bos). Şemada olmayan alan yasak DEĞİL — prototip türetilmiş alan ekler
    /// (duyulanPlaka, baskaIslem…).
    /// </summary>
    public IReadOnlyList<string> Validate(JsonElement action)
    {
        if (action.ValueKind != JsonValueKind.Object
            || !action.TryGetProperty("tur", out var t) || t.ValueKind != JsonValueKind.String
            || !_all.TryGetValue(t.GetString()!, out var s))
            return ["tur:bilinmeyen"];

        var errors = new List<string>();
        foreach (var (name, f) in s.Fields)
        {
            var present = action.TryGetProperty(name, out var v) && !IsEmpty(v, f);
            if (!present)
            {
                if (s.Required.Contains(name)) errors.Add(name + ":zorunlu");
                continue;
            }
            switch (f.Type)
            {
                case "integer" when v.ValueKind != JsonValueKind.Number || !v.TryGetInt64(out _):
                case "string" when v.ValueKind != JsonValueKind.String:
                    errors.Add(name + ":tip");
                    continue;
            }
            if (f.Enum is not null && !f.Enum.Contains(v.GetString()!)) errors.Add(name + ":secenek");
            else if (f.Pattern is not null && !f.Pattern.IsMatch(v.GetString()!)) errors.Add(name + ":bicim");
        }
        return errors;
    }

    private static bool IsEmpty(JsonElement v, Field f) => v.ValueKind switch
    {
        JsonValueKind.Null or JsonValueKind.Undefined => true,
        JsonValueKind.String => v.GetString()!.Length == 0,
        JsonValueKind.Number => f.Type == "integer" && v.TryGetDecimal(out var d) && d == 0,
        _ => false,
    };
}
