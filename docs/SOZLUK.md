# TAMIRON — TERİM SÖZLÜĞÜ (SOZLUK.md)

> **v2 · 03.09.2026'da yeniden yazıldı.** Kod/DB **İngilizce**, UI **Türkçe**.
> Bu tablo `VERI-MODELI.md`'deki **sekiz tablodan** türetildi — modelde
> karşılığı olmayan terim buraya giremez.
>
> Eski sözlük v1'in kavramlarını taşıyordu (`ServiceJob`, `Party`,
> `VisitType`, `PriceEstimate`, `GoodsReceipt`, `Account`…). O kavramların
> çoğu v2'de **yok**; sözlükte bırakılsalardı ekip var sanacaktı.
> Düşenler en altta, **neden düştükleriyle** duruyor.

---

## 1 · Veri terimleri

| Türkçe (UI) | İngilizce (kod/DB) | Not |
|---|---|---|
| **Araç** | **Vehicle** | ⭐ Ürünün merkezi. Müşteri **aracın üstünde** durur — ayrı müşteri tablosu yok |
| Plaka | `Vehicle.PlateNo` | Kimlik. Kayıt buna bağlanır; yanlış okunursa yanlış araca bağlanır. *(06.09: burada `Plate`, VERI-MODELI'nde `PlateNo` yazıyordu — model belgesi esas alındı)* |
| Sahip · Telefon | `OwnerName` · `OwnerPhone` | **Ruhsatta yazmaz** (`RUHSAT-OKUMA.md`), elle girilir, **zorunlu değil** (A-29). **Firma da olabilir** (K-019.3) |
| İkinci iletişim | `ContactName` · `ContactPhone` | Ruhsat firmanınsa aranacak kişi. **Adsız telefon yazılmaz** — K-019.3 |
| Renk | `Vehicle.Color` | Arayüzde **ad değil kutucuk** (`renkKutuHtml`). Ruhsatta tek kelime olmayabilir: *"Gri Metalik"* |
| **Servis kaydı** | **ServiceRecord** | ⭐ Araç her gelişinde açılan tek kayıt. **Teslim edilene kadar açıktır** (K-003) |
| İş türü | `ServiceRecord.Type` | `Repair` \| `Maintenance` \| `Both` — **K-006, K-026.3'te genişledi.** 06.09'dan beri **ekranda sorulmuyor**, iş metninden tahmin ediliyor (K-033.1) ve kayıt ekranında düzeltilebiliyor |
| Ne istiyor | `Complaint` | Adım 2. **Tek** serbest alan — şikâyet/tespit ayrımı yok (Kol A) |
| Kilometre | `Km` | Yalnız bakımda istenir, **bir kez** yazılır — K-002 |
| Teslim tarihi | `DeliveredAt` | ⭐ **Tek durum alanı** — K-004. Boşsa araç içeride |
| Ödeme yolu | `PaymentMethod` | `Cash` \| `Card` \| `Transfer` \| **`Unpaid`** — *"almadım"* bir ödeme yoludur |
| Hatırlatma tarihi | `ReminderDate` | Bakımsa teslimde hesaplanır — **ay VEYA km, hangisi önce gelirse** (K-002, 04.09) |
| Hatırlatma gönderildi | `ReminderSent` | Aynı hatırlatma iki kez gitmesin. **Arama da gönderim sayılır** (K-033.9) |
| Parça bekliyor | `WaitingForPart` | ⭐ Usta işaretler; *"parça yolda"*. **`fiyatiBekleyen` ile karıştırılmaz** — o *fiyatı gelmemiş* demek (K-034) |
| Tür elle seçildi | `TypeSetManually` | Ustanın kararı, tahmini yener. Karar türetilemez (K-033.12) |
| **Satır** | **ServiceLine** | ⭐ Parça **ve** işçilik aynı listede; ayrı liste değil, satırın işareti |
| Satır türü | `Kind` | `Part` \| `Labor` |
| Fiyat | `Price` | **Müşteriye söylenen.** Müşteri parçasında 0 |
| Alış | `Cost` | **Parçacıdan gelen.** Müşteriye gitmez. Kol D'de müşteri parçayı kendi getirse bile **saklanır** |
| Parçayı kim getirdi | `SuppliedBy` | ⭐ `Shop` \| `Customer` — **sorumluluk anahtarı**, K-005 |
| **Parçacı** | **Supplier** | ⭐ Fiyat veren ve mal gönderen taraf. v1'in `Distribütör`/`B2B` alt türleri **yok** |
| Fiyat sorusu | PriceRequest | Adım 3. Birden fazla parçacıya sorulursa **her biri ayrı kayıt** (Kol B) |
| Giden mesaj | `SentText` | *"Ne sormuşuz"* sorusunun tek cevabı |
| Fiyat cevabı | PriceReply | Adım 4 |
| Gelen metin | `RawText` | ⭐ **En kritik alan.** Parçacının yazdığı, olduğu gibi. Ayrıştırma yanlışsa dönülecek tek yer |
| Eşleşme | PartMatch | Akışta **yok** — arkada birikir. `TenantId` taşımaz, ortaktır |
| Dükkân | Shop | Ad, adres, telefon + bakım periyodu (varsayılan 6 ay) |

---

## 2 · Akış terimleri

| Terim | Anlamı |
|---|---|
| **Ana yol** | Yedi adım: ruhsat → ne istiyor → fiyat sor → fiyat geldi → parça geldi → iş yapıldı → teslim |
| **Kol** | İstisna. Ana yola **var olan bir adımın içinden** bağlanır; yeni adım/ekran açamaz |
| **İçerideki araç** | `DeliveredAt` boş olan kayıt. Ayrı bir "durum" değil, türetilmiş |
| **Onay isteme** | Bir **eylem**, akış değil (K-003). Kayıt beklemeye geçmez |
| **Hatırlatma** | Bakım teslimlerinde kurulan tek olay. **Ay veya km — hangisi önce gelirse** (K-002, 04.09 güncellemesi). Ana ekranda **görünmez**, yalnız çekmecedeki Bildirimler ve menü rozeti (K-019.1) |

---

## 3 · Arayüz sınıfları

Tasarım sistemi bir belge değil, bağlayıcı bir **sınıf sözlüğüdür** —
tam liste `TASARIM-SISTEMI.md`'nin sonunda. Yeni ekran oradaki sınıfları
kullanır; olmayan bir sınıf açacaksa önce sözlüğe girer.

Yedi nesneyi **yalnız kod üretir**, elle yazılmaz:
`plakaHtml()` → `.plate` · `kayitSatiriHtml()` → `.tl-item` ·
`bosDurumHtml()` → `.bos-durum` · `aracKartiHtml()` → `.arac-kart` ·
`renkKutuHtml()` → `.renk-kutu` · `akordiyonHtml()` → `.akordiyon` ·
`telLinkHtml()` → `.tel-link`. Balonu yalnız `balon()`, akordiyonun
açılıp kapanmasını yalnız `Akordiyon` modülü yönetir.

---

## 4 · v1'den düşen terimler

Bunlar **kaldırıldı**; kodda ya da konuşmada geçerse yanlıştır.

| Düşen terim | Neden |
|---|---|
| `ServiceJob` (servis işi) | Kayıt–iş–satır üç katmandı; ikiye indi. İş, satırın kendisi |
| `Party` / Müşteri tablosu | Müşteri **aracın üstünde**. Ayrı müşteri ekranı yok |
| `Contact` · `VehicleContact.IsPrimary` | Tek telefon alanı yeter |
| `VisitType` (dört ziyaret tipi) | İki tür kaldı: tamirat / bakım (**K-006**) |
| `Status` / durum makinesi | **Tek durum var**, o da teslim (**K-004**) |
| `Certainty` (kesin/olabilir/şartlı) | v1'in icadıydı; tamirci böyle konuşmadı |
| `PriceEstimate` (tahmini fiyat) | Usta fiyatı doğrudan satıra yazıyor |
| `GoodsReceipt` (mal kabul) | Depo/irsaliye akışı yedi işin dışında |
| `Account` (cari) · borç yaşlandırma | Ön muhasebe bu fazda yok |
| `Approval` / onay durumu | Onay bir eylem (**K-003**) |
| `Warranty*` (garanti süresi) | **K-005**: sorumluluğu `SuppliedBy` taşıyor, süre tutulmuyor |
| `Fitment` / TecDoc tabloları | Uyumu parçacı çözüyor (**K-001**) |
| `Quantity` | *"Balata takımı"* zaten takım. Gerekirse ada yazılır. **Cemil onayına açık** |
