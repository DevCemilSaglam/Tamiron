<#
  D19 · Yerel yedek (K-076.18) — Tamiron veritabanının tam yedeği + doğrulama + eski yedeklerin
  temizliği + (isteğe bağlı) süresi dolan izin silinmesi (D9, K-053.4).

  KULLANIM (Windows PowerShell 5.1, ek kurulum yok):
    powershell -File tools\yedek.ps1                                   # TamironDev → D:\TamironYedek
    powershell -File tools\yedek.ps1 -Veritabani Tamiron -Klasor E:\Yedek -Sakla 30 -IzTemizle

  NE YAPAR:
    1. BACKUP DATABASE … WITH COPY_ONLY, CHECKSUM — COPY_ONLY: başka bir yedek zincirini bozmaz.
    2. RESTORE VERIFYONLY … WITH CHECKSUM — doğrulanamayan yedek "alındı" sayılmaz, çıkış kodu 1.
    3. Klasörde YALNIZ bu betiğin ürettiği "<Veritabani>_yyyyMMdd_HHmmss.bak" dosyalarından
       -Sakla günden eskileri siler; en yeni 3 yedek HER ZAMAN kalır (makine uzun kapalı kaldıysa
       hepsi silinmesin).
    4. -IzTemizle: 90 günden eski iz satırlarını siler (Trace.RetentionDays — IzGunu'nun ona eşit
       olduğunu bir test sınar).

  ⚠️ Yedek AYNI diskteyse disk ölünce yedek de ölür. Uzak kopya (K-070.2b "yedek uzak sunucuya")
  ayrı adım ve Cemil'in kararı: bu betik dışarıya hiçbir şey göndermez.
  Gizli bilgi yok: Windows kimliğiyle bağlanır (LocalDB / Windows kimlikli SQL Server).
#>
param(
  [string]$Sunucu = '(localdb)\MSSQLLocalDB',
  [string]$Veritabani = 'TamironDev',
  [string]$Klasor = 'D:\TamironYedek',
  [int]$Sakla = 14,
  [switch]$IzTemizle
)
$ErrorActionPreference = 'Stop'

# İz saklama süresi — Tamiron.Domain.Trace.RetentionDays ile AYNI olmalı (TraceRetentionScriptTests).
$IzGunu = 90
$EnAzKalan = 3

if ($Veritabani -notmatch '^[A-Za-z0-9_]+$') { throw "Veritabanı adı yalnız harf, rakam ve _ olabilir: $Veritabani" }

function Sql([string]$metin, [hashtable]$p = @{}, [int]$sure = 600) {
  $c = New-Object System.Data.SqlClient.SqlConnection "Server=$Sunucu;Database=master;Integrated Security=True;TrustServerCertificate=True"
  $c.Open()
  try {
    $k = $c.CreateCommand(); $k.CommandText = $metin; $k.CommandTimeout = $sure
    foreach ($a in $p.Keys) { [void]$k.Parameters.AddWithValue($a, $p[$a]) }
    return $k.ExecuteNonQuery()
  } finally { $c.Close() }
}

New-Item -ItemType Directory -Force -Path $Klasor | Out-Null
$dosya = Join-Path $Klasor ("{0}_{1}.bak" -f $Veritabani, (Get-Date -Format 'yyyyMMdd_HHmmss'))

Write-Host "Yedek: $Veritabani -> $dosya"
Sql "BACKUP DATABASE [$Veritabani] TO DISK = @d WITH COPY_ONLY, CHECKSUM, INIT, NAME = N'Tamiron yedek'" @{ '@d' = $dosya } | Out-Null
try {
  Sql "RESTORE VERIFYONLY FROM DISK = @d WITH CHECKSUM" @{ '@d' = $dosya } | Out-Null
} catch {
  Write-Error "Yedek DOĞRULANAMADI: $dosya — $($_.Exception.Message)"
  exit 1
}
$boy = [math]::Round((Get-Item $dosya).Length / 1MB, 1)
Write-Host "Doğrulandı ($boy MB)."

# Temizlik: yalnız bu betiğin adlandırdığı dosyalar; en yeni $EnAzKalan dokunulmaz.
$desen = '^' + [regex]::Escape($Veritabani) + '_\d{8}_\d{6}\.bak$'
$yedekler = Get-ChildItem -Path $Klasor -File | Where-Object { $_.Name -match $desen } | Sort-Object LastWriteTime -Descending
$sinir = (Get-Date).AddDays(-$Sakla)
$silinecek = $yedekler | Select-Object -Skip $EnAzKalan | Where-Object { $_.LastWriteTime -lt $sinir }
foreach ($s in $silinecek) { Remove-Item -LiteralPath $s.FullName; Write-Host "Eski yedek silindi: $($s.Name)" }
Write-Host ("Klasörde {0} yedek." -f (@($yedekler).Count - @($silinecek).Count))

if ($IzTemizle) {
  $n = Sql "DELETE FROM [$Veritabani].dbo.TraceEvents WHERE DateTimeCreated < DATEADD(day, -$IzGunu, SYSUTCDATETIME())"
  Write-Host "İz: $IzGunu günden eski $n satır silindi."
}
exit 0
