# G1 · Tamiron'u BU BİLGİSAYARDA açar (K-079.3, K-080.1) — sunucu yok, tek komut.
#
#   powershell -ExecutionPolicy Bypass -File tools\yerel-baslat.ps1
#
# Ne yapar: uygulamayı derler (.yerel\ klasörüne, depoya girmez), "Local" ortamında başlatır,
# hazır olunca tarayıcıda http://localhost:5090 açar. Veritabanı LocalDB'de TamironYerel —
# geliştirme veritabanından (TamironDev, prototip tohumu) AYRI ve ilk açılışta boş: dükkânı
# girişteki "Dükkânımı kaydet" ile usta açar.
#
# SMS paketi yok: giriş kodu BU PENCEREDE yazar ("SMS GÖNDERİLMEDİ (paket yok) — kod burada → ..."). Pencereyi
# kapatmak uygulamayı kapatır. Gizli bilgi yok: LocalDB Windows kimliğiyle açılır.
#
# -Derlemeden: kod değişmediyse derlemeyi atla (daha hızlı açılır).
# -TarayicisizAc: tarayıcıyı açma (deneme/önizleme için).

param([switch]$Derlemeden, [switch]$TarayicisizAc)

$ErrorActionPreference = 'Stop'
# Türkçe harfler pencerede bozulmasın (PowerShell 5.1 varsayılanı OEM kod sayfası).
[Console]::OutputEncoding = [Text.Encoding]::UTF8
$kok = Split-Path -Parent $PSScriptRoot
$cikti = Join-Path $kok '.yerel'
$adres = 'http://localhost:5090'

if (-not $Derlemeden -or -not (Test-Path (Join-Path $cikti 'Tamiron.Api.exe'))) {
    Write-Host 'Tamiron derleniyor...'
    dotnet publish (Join-Path $kok 'src\Tamiron.Api') -c Release -o $cikti --nologo -v q
    if ($LASTEXITCODE -ne 0) { throw 'Derleme başarısız.' }
}

$env:ASPNETCORE_ENVIRONMENT = 'Local'
$env:ASPNETCORE_URLS = $adres

# Hazır olunca tarayıcıyı aç (arka planda bekler; uygulama bu pencerede çalışır).
if (-not $TarayicisizAc) { Start-Job -ScriptBlock {
    param($a)
    for ($i = 0; $i -lt 120; $i++) {
        try { if ((Invoke-WebRequest "$a/health" -UseBasicParsing -TimeoutSec 2).StatusCode -eq 200) { Start-Process $a; return } } catch { }
        Start-Sleep -Milliseconds 500
    }
} -ArgumentList $adres | Out-Null }

Write-Host "Tamiron açılıyor: $adres  (kapatmak için bu pencereyi kapat)"
Write-Host 'Giriş kodu bu pencerede görünür.'
Push-Location $cikti
try { & (Join-Path $cikti 'Tamiron.Api.exe') } finally { Pop-Location }
