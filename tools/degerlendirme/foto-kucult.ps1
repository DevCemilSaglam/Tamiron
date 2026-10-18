# FOTOĞRAF KÜÇÜLTME (18.10.2026 · K-067 maliyet ölçümü)
# Cemil: "Ruhsat okuma tutarını düşürmenin yoluna ihtiyacımız var."
# Görüntü jetonu piksel sayısıyla büyür; uzun kenarı küçültmek girdiyi düşürür.
# Soru: şase gibi küçük yazı hâlâ okunuyor mu? Bunu foto-kos.js ölçer.
#
#   powershell -File tools\degerlendirme\foto-kucult.ps1 -Uzun 1100 -Cikti <klasör>
#
# ⚠️ EXIF YÖNÜ ELLE UYGULANIR: System.Drawing bunu kendiliğinden yapmaz;
#    yapılmazsa telefonda düz görünen ruhsat yan kaydedilir ve model
#    "yan çekilmiş belge" okur — ölçüm sessizce bozulur.
# ⚠️ ÇIKTI DEPOYA GİRMEZ: fotoğraflarda TC no ve adres var. Varsayılan
#    hedef yok; klasör açıkça verilir (scratchpad).
param(
  [int]$Uzun = 1100,
  [Parameter(Mandatory = $true)][string]$Cikti,
  [string]$Harita = ''
)
# PS 5.1'de param varsayılanında $PSScriptRoot BOŞ gelir — gövdede çözülür.
if (-not $Harita) { $Harita = Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) 'foto-harita.json' }
Add-Type -AssemblyName System.Drawing
$h = Get-Content $Harita -Raw -Encoding UTF8 | ConvertFrom-Json
New-Item -ItemType Directory -Force $Cikti | Out-Null
$kod = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object { $_.MimeType -eq 'image/jpeg' }
$par = New-Object System.Drawing.Imaging.EncoderParameters 1
$par.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter ([System.Drawing.Imaging.Encoder]::Quality), 85L
$n = 0
foreach ($p in $h.yollar.PSObject.Properties) {
  $img = [System.Drawing.Image]::FromFile($p.Value)
  if ($img.PropertyIdList -contains 0x0112) {
    $o = $img.GetPropertyItem(0x0112).Value[0]
    switch ($o) {
      3 { $img.RotateFlip('Rotate180FlipNone') }
      6 { $img.RotateFlip('Rotate90FlipNone') }
      8 { $img.RotateFlip('Rotate270FlipNone') }
    }
  }
  $oran = [Math]::Min(1.0, $Uzun / [Math]::Max($img.Width, $img.Height))
  $w = [int]($img.Width * $oran); $hh = [int]($img.Height * $oran)
  $bmp = New-Object System.Drawing.Bitmap $w, $hh
  $g = [System.Drawing.Graphics]::FromImage($bmp)
  $g.InterpolationMode = 'HighQualityBicubic'
  $g.DrawImage($img, 0, 0, $w, $hh)
  $bmp.Save((Join-Path $Cikti ($p.Name + '.jpg')), $kod, $par)
  $g.Dispose(); $bmp.Dispose(); $img.Dispose(); $n++
}
"$n fotoğraf → $Cikti (uzun kenar $Uzun px)"
