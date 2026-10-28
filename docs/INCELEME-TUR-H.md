# Tur H — inceleme rehberi (H9 · 28.10.2026)

> Cemil, 28.10: *"Otomix'i örnek alacak muhasebe alanı tamam olabilir mesela. Yedek parça satıcısı için alan
> yapılabilir."* İki kol: **muhasebe** (OtoMix'in modül tahtası + Cari: müşteriden alacak, parçacıya borç) ve
> **yedek parça** (parçacının kendi uygulaması — soruyu görür, cevabı yazar; usta yazmadan kullanır).
> Kurallar tur G'ninki: önce tasarım (yeni aktör), önce test, mutasyon, tarayıcıda uçtan uca; para harcayan,
> dışarıya veri gönderen, makine ayarı değiştiren adım YOK.
>
> **Karar bekleyen her şey §1'de, tek listede.** Gerisi okumak için.

---

## 1 · Karar bekleyenler — TEK LİSTE

### 1a · Senin cevabın olmadan ilerlemeyen

| # | Soru | Benim önerim | Nerede |
|---|---|---|---|
| 1 | **Parçacılar bir uygulama kullanır mı?** Yedek parça tarafı (H5–H8) buna dayanıyor. Ustaya sor: *"parçacın fiyatı programa kendisi yazsa işine yarar mı?"* | Sor; "hayır"sa parçacı uygulaması kaldırılır (ustanın ekranında yalnız bir düğme var, geri almak kolay) | K-091 · TASARIM-H-YEDEK-PARCA §6 |
| 2 | **Sunucu ya da telefondan erişim.** Program bu bilgisayarda çalıştıkça **gerçek bir parçacı kendi telefonundan bağlanamaz**; ustanın telefonu da (K-080.2). Aynı Wi-Fi için güvenlik duvarı kuralı (makine ayarı) — parçacı başka yerdeyse sunucu şart | Önce ustanın telefonu: güvenlik duvarı kuralı (adımları yazarım). Parçacı için sunucu kararı Tamiron'un satışa çıkacağı güne | K-091 · K-080.2 · K-079.3 |
| 3 | **Parçacıya veresiye yaygın mı?** "Parçacılara borcum" buna dayanıyor (MUHASEBE-KAPSAM §4.4) | Ustaya sor; yaygın değilse "veresiye" düğmesi kalkar, Cari'de yalnız alacak kalır | K-090 |
| 4 | **Ses (G13)** — tur G'den | Şimdilik yok | K-080.3 |
| 5 | **90 günlük silme zamanlansın mı?** — tur G'den (görev zamanlayıcısı = makine ayarı) | Evet | K-086 |
| 6 | **Teslimi geri alma** (aynı gün) ekrana gelsin mi? — tur G'den | Evet, "Düzenle" altında | K-076.13 |
| 7 | **Toplu mesaj (bayram)** — tur G'den | Ustaya kaç kişiye attığını sor | K-084 |

### 1b · Verdiğim kararlar — itirazın yoksa kalır (Cemil onayına açık)

| K | Karar | Neden |
|---|---|---|
| **89** | Muhasebe **Detaylı** = OtoMix'in modül tahtası: Kasa · Raporlar · SMS · **Cari** çalışır; Fatura · Banka · Çek · POS · Stok · Teklif **soluk, "yazılmadı"** | Altısı ustadan sürekli yeni veri ister (T-1); açılır gibi duran ölü düğme en pahalı hata. Yerleri duruyor |
| 89 | **Yazdır/PDF ertelendi** | CSV muhasebeciye gidiyor; ustanın yazdırma ihtiyacı sorulmadı |
| **90** | **Borç yalnız "veresiye" işaretli parçadan** doğar; varsayılan "ödedim" | Girilmezse borç sıfır görünür — eksik söyler, uydurmaz (K-013) |
| 90 | **Parçacıya ödeme kasada görünmez** (gider de değil); CSV'de "Parçacıya ödeme" | Alış iş başına farktan zaten düşülüyor; kasa bugün tahsilat |
| **91** | Parçacı **kendi numarasıyla** girer; tamirhanenin o numaraya sorduğu sorular görünür — davet/eşleştirme yok | Kimlik zaten telefon (K-074) |
| 91 | Parçacı **müşteri bilgisi, başka parçacının cevabı, fiyat geçmişi görmez** | KVKK; rakip fiyatı ticari sır |
| 91 | Parçacının cevabı **kendiliğinden işlenmez**; usta "Bunu kullan" der | Kol B — birden çok parçacı |
| 91 | **Bir telefon iki türden dükkân açamaz** (hem tamir hem parça satan dükkân?) | Tek girişte iki uygulama karışır |
| 91 | Parçacıya **iki ekran** (giriş + gelen sorular); muhasebe/stok/katalog yok | En dar hâli; ihtiyaç görülünce |

### 1c · Bu turda bulunan, ÖNCEDEN var olan bir hata — düzeltildi, senin programında da var

- **Saat başı 409:** oturumun "son görülme"si saatte bir yazılıyor; saat dolunca aynı anda giden iki istek (menü
  rozeti + ana sayfa) çatışıyor, ana sayfa *"Bu kayıt başka bir cihazda değişti"* diyebiliyordu. Düzeltildi
  (K-092, testli). **5090'da açık duran programın eski — yeniden başlatınca** düzeltme ve tur H gelir (göçler
  açılışta kendiliğinden uygulanır; var olan dükkânın "tamirhane" olarak kalır).

---

## 2 · Yerelde nasıl denenir

```
powershell -ExecutionPolicy Bypass -File tools\yerel-baslat.ps1
```

- **Usta:** `http://localhost:5090` — Muhasebe → **Detaylı** (modül tahtası) → **Raporlar** (iş başına) ·
  **Cari** (alacak + parçacılara borç). Ayarlar'da *"Parça alış fiyatı: Sorulsun"* açıksa kayıtta parçanın
  altında **"veresiye"**.
- **Parçacı:** `http://localhost:5090/parca/` — kendi numarasıyla "Dükkânımı kaydet" (kod sunucu penceresinde).
  Ustanın parçacılar listesindeki numara buysa sorular görünür. Cevap yazınca ustanın **Parça fiyatı sor**
  ekranında "… yazdı" + **"Bunu kullan"**.
- Aynı tarayıcıda ikisi birden açık olabilir (oturumlar ayrı).

Denemeler ayrı bir kopyada yapıldı (`-Klasor .yerel-deneme -Port 5091`, aynı veritabanı) — senin 5090'daki
programına dokunulmadı. Veritabanında deneme verisi: **Deneme Oto** (deneme dükkânı) ve yeni **Deneme Parça**
parçacı hesabı (0533 000 00 02); 34 ABC 123'ün fren balatası parçacının cevabıyla yazıldı (1.150 ₺, Bosch).
Senin dükkânına (Saglamlar) yazılmadı.

---

## 3 · Ne yapıldı — 8 commit

| İş | Ne | Commit |
|---|---|---|
| H1 | Muhasebe Basit/Detaylı · modül tahtası · İş başına | 1877cbc |
| H2 | Parçacıya borç — tasarım (`TASARIM-H-MUHASEBE.md`) | 2bd571e |
| H3 | Parçacıya borç — sunucu (veresiye satır, ödeme, borç listesi, CSV) | df21b30 |
| H4 | Cari ekranı + veresiye çipleri (kayıt, parça sor) | 7f66172 |
| H5 | Yedek Parça — tasarım (`TASARIM-H-YEDEK-PARCA.md`) | ecf313a |
| H6 | Yedek Parça — sunucu (dükkân türü, gelen sorular, cevap, "Bunu kullan", tür sınırı) | 4538339 |
| H7 | Yedek Parça uygulaması `/parca/` · ortak giriş formu · saat başı 409 | a06a1f3 |
| H8 | Parça sorda parçacının cevabı + "Bunu kullan" | be52278 |

**Ölçüm:** 683 → **754 test yeşil** (domain 304 · arayüz 230 · API 220). Mutasyon: **67'nin 67'si** yakalandı;
ilk koşuda kaçan 3 boşluk önce testle kapatıldı, 1'i denk (aynı süzgeç sonra tekrar ediyor). Yalıtım testi yeni
uçları ve kiracılar arası iki okumayı (parçacı → soru, tamirhane → cevap) kapsıyor. *(Commit mesajlarındaki
ara test sayılarının bazıları yanlış yazıldı; doğrusu bu tablo.)*

**Tasarımda düzelttiğim bir şey (H6):** parçacının cevabını tamirhanenin verisine yazmayı planlamıştım; program
başka dükkânın verisine yazmayı kesin yasaklıyor — doğru kural, gevşetmedim. Cevap parçacının kendi verisi;
"Bunu kullan" tamirhaneye kopyalar.

**Tarayıcıda bulunanlar:** `/parca/` dosyaları yakalanıp 500 dönüyordu · ortak tasarım dosyaları `/parca/`
altında yoktu · saat başı 409 (§1c) · dükkân adına "'ya" eki sabitti ("Kaya Oto Servis'ya" olurdu).
