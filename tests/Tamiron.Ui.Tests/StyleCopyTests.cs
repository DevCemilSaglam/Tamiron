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
}
