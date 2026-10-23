using System.Text.Encodings.Web;
using System.Text.Json;

namespace Tamiron.Domain;

/// <summary>
/// İz olayı (D9 · K-052 · K-053) — prototip 00-iz.js'in sunucudaki yeri. Ustaya GÖRÜNMEZ;
/// destek, vazgeçme ve zorlanma için bizim tarafa ait. Değerler YAZILIR (K-053).
/// </summary>
public class TraceEvent : TenantEntity
{
    public Guid UserId { get; private set; }
    public Guid SessionId { get; private set; }
    /// <summary>İstemcinin saati — olaylar arasındaki süre buradan okunur; geliş anı DateTimeCreated.</summary>
    public DateTime? ClientAt { get; private set; }
    public string Kind { get; private set; } = "";
    public string? Name { get; private set; }
    /// <summary>Ek bilgi, JSON — temizlenmiş (Trace.Data).</summary>
    public string? Data { get; private set; }

    private TraceEvent() { }   // EF

    public static TraceEvent Create(Guid userId, Guid sessionId, DateTime? clientAt, string kind, JsonElement? name, JsonElement? data) => new()
    {
        UserId = userId, SessionId = sessionId, ClientAt = clientAt, Kind = kind,
        Name = name is { } n ? Trace.Safe(n)?.ToString() : null,
        Data = Trace.Data(data),
    };
}

/// <summary>İz kuralları — prototipin guvenli / temizle'si ve K-053.4 saklama süresi.</summary>
public static class Trace
{
    /// <summary>Bir değerden yazılacak en fazla harf (prototip KIRP). Sınırın sebebi gizlilik değil ÖLÇÜ.</summary>
    public const int Cut = 80;
    /// <summary>Bir olayın ekinde en çok kaç anahtar — tek olay depoyu şişirmesin.</summary>
    public const int MaxKeys = 12;
    /// <summary>Bir gönderimde en çok kaç olay (prototip TAVAN: halka tampon 300).</summary>
    public const int MaxBatch = 300;
    /// <summary>Ayrıntı bizde 90 gün (K-053.4); istatistik süresiz (henüz yok).</summary>
    public const int RetentionDays = 90;

    private static readonly HashSet<string> Kinds = ["ekran", "sure", "dokunus", "alan", "okuma", "duzeltme", "hata", "suphe"];
    private static readonly JsonSerializerOptions Out = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public static bool IsKind(string? k) => k is not null && Kinds.Contains(k);

    public static DateTime PurgeBefore(DateTime nowUtc) => nowUtc.AddDays(-RetentionDays);

    /// <summary>Prototip guvenli: sayı iki basamağa, mantık olduğu gibi, metin 80 harfte "…" ile; boş = yok.</summary>
    public static object? Safe(JsonElement e)
    {
        switch (e.ValueKind)
        {
            case JsonValueKind.Null or JsonValueKind.Undefined: return null;
            case JsonValueKind.True: return true;
            case JsonValueKind.False: return false;
            case JsonValueKind.Number when e.TryGetDecimal(out var d): return Math.Round(d, 2, MidpointRounding.AwayFromZero);
        }
        var m = e.ValueKind == JsonValueKind.String ? e.GetString()! : e.GetRawText();
        if (m.Length == 0) return null;
        return m.Length > Cut ? m[..Cut] + "…" : m;
    }

    /// <summary>Prototip temizle: boş değerli anahtar düşer, hiçbir şey kalmazsa ek yok.</summary>
    public static string? Data(JsonElement? data)
    {
        if (data is not { ValueKind: JsonValueKind.Object } o) return null;
        var c = new Dictionary<string, object>();
        foreach (var p in o.EnumerateObject())
        {
            if (c.Count == MaxKeys) break;
            if (Safe(p.Value) is { } v) c[p.Name.Length > Cut ? p.Name[..Cut] : p.Name] = v;
        }
        return c.Count == 0 ? null : JsonSerializer.Serialize(c, Out);
    }
}
