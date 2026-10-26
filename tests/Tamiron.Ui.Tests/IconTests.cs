using System.Text.RegularExpressions;
using Tamiron.Ui.Core;

namespace Tamiron.Ui.Tests;

/// <summary>
/// G1 · Ekranlardaki her <c>&lt;use href="#i-…"&gt;</c> ikon kümesinde var. Prototipte yaşandı
/// (prototip-derle.js §5): olmayan ikon <c>use</c> ile sessizce HİÇBİR ŞEY çizmez, düğme boş kalır.
/// </summary>
public class IconTests
{
    [Fact]
    public void Every_icon_used_on_a_screen_exists()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (!File.Exists(Path.Combine(d!.FullName, "Tamiron.sln"))) d = d.Parent;
        var used = Directory.GetFiles(Path.Combine(d.FullName, "src"), "*.razor", SearchOption.AllDirectories)
            .SelectMany(f => Regex.Matches(File.ReadAllText(f), "\"#?(i-[a-z0-9-]+)\"").Select(m => (File: Path.GetFileName(f), Icon: m.Groups[1].Value)))
            .ToList();
        Assert.NotEmpty(used);
        Assert.Empty(used.Where(u => !Ikonlar.Adlar.Contains(u.Icon)).Select(u => $"{u.File}: {u.Icon}"));
    }

    /// <summary>
    /// Yalın <c>.btn</c> renksizdir (03-bilesen.css): prototipte her düğme bir tür taşır (btn-primary,
    /// btn-ghost …). Türsüz düğme koyu temada beyaz zemin + açık yazı oldu — G1'de tarayıcıda görüldü.
    /// </summary>
    [Fact]
    public void Every_button_has_a_colour_variant()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (!File.Exists(Path.Combine(d!.FullName, "Tamiron.sln"))) d = d.Parent;
        var bare = Directory.GetFiles(Path.Combine(d.FullName, "src"), "*.razor", SearchOption.AllDirectories)
            .SelectMany(f => Regex.Matches(File.ReadAllText(f), "class=\"(btn(?: [^\"]*)?)\"")
                .Select(m => (File: Path.GetFileName(f), Cls: m.Groups[1].Value)))
            .Where(x => !x.Cls.Split(' ').Any(c => c.StartsWith("btn-") && c is not "btn-lg" and not "btn-row"))
            .Select(x => $"{x.File}: class=\"{x.Cls}\"");
        Assert.Empty(bare);
    }
}
