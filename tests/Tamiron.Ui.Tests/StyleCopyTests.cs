namespace Tamiron.Ui.Tests;

/// <summary>
/// Tek renk/tip kaynağı docs/tokens.css; bileşen görünümü prototipin stilleri. Ui.Core'daki
/// kopyalar kaynaklarla BAYT BAYT eşit olmalı — biri değişip öteki kalırsa iki tasarım sistemi olur.
/// </summary>
public class StyleCopyTests
{
    static string Root()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d is not null && !File.Exists(Path.Combine(d.FullName, "Tamiron.sln"))) d = d.Parent;
        return d!.FullName;
    }

    [Theory]
    [InlineData("docs/tokens.css", "tokens.css")]
    [InlineData("prototypes/kaynak/stil/01-taban.css", "01-taban.css")]
    [InlineData("prototypes/kaynak/stil/02-kabuk.css", "02-kabuk.css")]
    [InlineData("prototypes/kaynak/stil/03-bilesen.css", "03-bilesen.css")]
    public void Copies_match_their_source(string source, string copy)
        => Assert.True(File.ReadAllBytes(Path.Combine(Root(), source)).SequenceEqual(File.ReadAllBytes(Path.Combine(Root(), "src", "Tamiron.Ui.Core", "wwwroot", copy))),
            $"{copy} kaynağından ({source}) farklı — kopyayı yenile: cp {source} src/Tamiron.Ui.Core/wwwroot/{copy}");

    /// <summary>
    /// G3 · Ekranların kendi stilleri (prototipte her ekranın &lt;style&gt;'ı) 04-ekranlar.css'te — tools/ekran-stil.js
    /// ile AYNI çıkarma. Prototipte stil değişip dosya yenilenmezse kırmızı (K-080.7).
    /// </summary>
    [Fact]
    public void Screen_styles_match_the_prototype_screens()
    {
        var dir = Path.Combine(Root(), "prototypes", "kaynak", "ekranlar");
        var parts = Directory.GetFiles(dir, "*.html").OrderBy(f => Path.GetFileName(f), StringComparer.Ordinal).SelectMany(f =>
        {
            var s = File.ReadAllText(f).Replace("\r\n", "\n");
            return System.Text.RegularExpressions.Regex.Matches(s, @"<style>([\s\S]*?)</style>")
                .Select(m => $"/* ---- {Path.GetFileName(f)} ---- */\n{m.Groups[1].Value.Trim()}\n");
        });
        var expected = "/* ÜRETİLDİ: node tools/ekran-stil.js — elle düzenleme. Kaynak: prototypes/kaynak/ekranlar/*.html <style> */\n" + string.Join("\n", parts);
        // Satır sonu git'in elinde (autocrlf) — içerik karşılaştırılır.
        Assert.True(expected == File.ReadAllText(Path.Combine(Root(), "src", "Tamiron.Ui.Core", "wwwroot", "04-ekranlar.css")).Replace("\r\n", "\n"),
            "04-ekranlar.css prototipin ekran stillerinden farklı — yenile: node tools/ekran-stil.js");
    }
}
