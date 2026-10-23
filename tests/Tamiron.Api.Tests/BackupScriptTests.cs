using System.Text.RegularExpressions;
using Tamiron.Domain;

namespace Tamiron.Api.Tests;

/// <summary>
/// D19 · Yedek betiği (tools/yedek.ps1) iz saklama süresini kendi içinde yazar — kaynak kural
/// Trace.RetentionDays. İkisi ayrışırsa betik ya erken siler (destek kör kalır) ya hiç silmez
/// (K-053.4'ün 90 günü yalan olur). Bu test ikisini bağlar (K-076.18).
/// </summary>
public class BackupScriptTests
{
    static string Script()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Tamiron.sln"))) dir = dir.Parent;
        return File.ReadAllText(Path.Combine(dir!.FullName, "tools", "yedek.ps1"));
    }

    /// <summary>Yorumlar ayıklanmış KOD: açıklamada geçen bir söz, kodda olmayan bir adımı "var" göstermesin
    /// (mutasyonla bulundu — doğrulama satırı silindi, test baştaki açıklamadan buldu ve yeşil kaldı).</summary>
    static string Code()
    {
        var s = Regex.Replace(Script(), @"<#.*?#>", "", RegexOptions.Singleline);
        return string.Join("\n", s.Split('\n').Where(l => !l.TrimStart().StartsWith('#')));
    }

    [Fact]
    public void Trace_retention_in_the_script_matches_the_domain_rule()
    {
        var m = Regex.Match(Code(), @"^\$IzGunu\s*=\s*(\d+)\s*$", RegexOptions.Multiline);
        Assert.True(m.Success, "yedek.ps1'de $IzGunu satırı yok");
        Assert.Equal(Trace.RetentionDays, int.Parse(m.Groups[1].Value));
    }

    [Fact]
    public void Script_verifies_the_backup_and_never_sends_it_anywhere()
    {
        var s = Code();
        Assert.Matches(@"Sql ""RESTORE VERIFYONLY FROM DISK = @d WITH CHECKSUM""", s);
        Assert.Matches(@"Sql ""BACKUP DATABASE \[\$Veritabani\] TO DISK = @d WITH COPY_ONLY, CHECKSUM", s);
        Assert.DoesNotMatch(@"(?i)Invoke-WebRequest|Invoke-RestMethod|\bcurl\b|\bscp\b|Copy-Item\s+\S+\s+\\\\", s);   // dışarı gönderim yok
    }
}
