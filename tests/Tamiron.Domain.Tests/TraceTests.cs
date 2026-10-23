using System.Text.Json;
using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// D9 · İz kaydı (K-052 · K-053). Beklenen davranış prototipin <c>Iz.guvenli</c> / <c>temizle</c>'si
/// (00-iz.js): değer YAZILIR, yalnız boyu 80 harfte kırpılır; sayı iki basamağa yuvarlanır; boş atılır.
/// </summary>
public class TraceTests
{
    static JsonElement J(string json) => JsonDocument.Parse(json).RootElement.Clone();

    [Fact]
    public void Safe_keeps_values_and_only_trims_length()
    {
        Assert.Equal("34 ABC 123", Trace.Safe(J("\"34 ABC 123\"")));                 // değer yazılır (K-053)
        Assert.Equal(new string('a', 80) + "…", Trace.Safe(J("\"" + new string('a', 81) + "\"")));
        Assert.Equal(new string('a', 80), Trace.Safe(J("\"" + new string('a', 80) + "\"")));
        Assert.Equal(3.14m, Trace.Safe(J("3.14159")));
        Assert.Equal(true, Trace.Safe(J("true")));
        Assert.Null(Trace.Safe(J("\"\"")));
        Assert.Null(Trace.Safe(J("null")));
    }

    [Fact]
    public void Data_drops_empty_values_and_becomes_null_when_nothing_is_left()
    {
        Assert.Equal("{\"deger\":\"34 ABC 123\",\"kez\":2}", Trace.Data(J("{\"deger\":\"34 ABC 123\",\"bos\":\"\",\"kez\":2}")));
        Assert.Null(Trace.Data(J("{\"bos\":\"\",\"yok\":null}")));
        Assert.Null(Trace.Data(null));
    }

    [Fact]
    public void Nested_object_is_written_as_trimmed_text_not_expanded()
        => Assert.Equal("{\"x\":\"{\\\"a\\\":1}\"}", Trace.Data(J("{\"x\":{\"a\":1}}")));

    [Fact]
    public void Turkish_text_stays_readable_in_the_store()
        => Assert.Equal("{\"deger\":\"Şükrü Öztürk\"}", Trace.Data(J("{\"deger\":\"Şükrü Öztürk\"}")));

    [Fact]
    public void Too_many_keys_are_capped()
    {
        var many = "{" + string.Join(",", Enumerable.Range(0, 30).Select(i => $"\"k{i}\":{i + 1}")) + "}";
        Assert.Equal(Trace.MaxKeys, JsonDocument.Parse(Trace.Data(J(many))!).RootElement.EnumerateObject().Count());
    }

    [Theory]
    [InlineData("ekran", true)]
    [InlineData("sure", true)]
    [InlineData("dokunus", true)]
    [InlineData("alan", true)]
    [InlineData("okuma", true)]
    [InlineData("duzeltme", true)]
    [InlineData("hata", true)]
    [InlineData("suphe", true)]
    [InlineData("bilinmeyen", false)]
    [InlineData("", false)]
    public void Only_prototype_kinds_are_accepted(string kind, bool ok) => Assert.Equal(ok, Trace.IsKind(kind));

    [Fact]
    public void Old_detail_is_purged_after_ninety_days()
    {
        var now = new DateTime(2026, 10, 22, 9, 0, 0, DateTimeKind.Utc);
        Assert.Equal(new DateTime(2026, 7, 24, 9, 0, 0, DateTimeKind.Utc), Trace.PurgeBefore(now));
    }
}
