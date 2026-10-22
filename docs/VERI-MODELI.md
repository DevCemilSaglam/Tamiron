# TAMIRON — VERİ MODELİ

> **v2 · 22.10.2026** · Kod/DB **İngilizce**, UI **Türkçe** (`SOZLUK.md`)
> **Akıştan türetildi** (`IS-AKISI.md`), **prototiple uzlaştırıldı** (§Prototip
> eşlemesi). Kesin şema backend fazında kurulur; **başlangıç noktası bu belge.**
>
> **v2'de ne değişti (K-072):**
> - 15.09'dan sonra prototipe giren alanlar işlendi: araç (ruhsat alanları, ileti izni) ·
>   kayıt (tahsil tarihi, bekleme tarihi) · dükkân (km periyodu, alış sorusu).
> - İki tablo eklendi: `Expense` (gider) ve `Message` (gönderilen mesaj).
> - Prototipteki her alanın modeldeki karşılığı en altta tablo hâlinde. **Derleme
>   denetimi 9** bu tabloyu okuyor: prototipe bu tabloda olmayan bir alan girerse
>   derleme düşer.
>
> **KURAL: Her alanın akışta bir karşılığı vardır.** Karşılığı olmayan alan bu belgeye giremez. Bu kural v1'in `VERI-MODELI`'nde yoktu ve model, ekranlar sadeleştikçe şişmiş hâlde kaldı.

---

## Her tabloda ortak

Devralınan mimariden (`MIMARI.md`), tartışılmıyor:

`Id` · `TenantId` *(dükkân — global filter)* · `DateTimeCreated` · `DateTimeUpdated` · `DateTimeDeleted` *(soft-delete)* · `RowVersion`

---

## 1 · `Vehicle` — Araç

Akıştaki yeri: **Adım 1** · Ekran 2, 6

| Alan | Neden var |
|---|---|
| `PlateNo` | Doğal anahtar. Tamirci **plakayla düşünür, plakayla arar** |
| `Brand` · `Model` · `Year` | Ruhsattan okunur; parçacıya giden mesajda gerekir |
| `Engine` | Parçacının doğru parçayı bulması için (T-14) |
| `Vin` | Ruhsatta var, alınır. Zorunlu değil |
| `OwnerName` · `OwnerPhone` | **Telefon hatırlatmanın ön koşulu.** Ad özette geçer. **Ruhsat sahibi FİRMA da olabilir** (04.09) — ayrı bir "tüzel kişi" işareti tutulmaz, ad neyse o yazılır |
| `ContactName` · `ContactPhone` | **İkinci iletişim** (04.09, Cemil: *"Eğer ikinci bir iletişim varsa adı ve Telefonu"*). Ruhsat firmanınsa aranacak kişi başkasıdır; firmanın numarası santraldir. **Adsız telefon işe yaramaz** — usta kimi aradığını bilmeden aramaz. İkisi de boş olabilir |
| `TaxOrNationalId` · `Address` | **⚠️ 15.09'da EKLENDİ (K-050).** Cemil: *"Bu alanlar da gerekli. Çünkü bu alanlara fatura kesilecek vb."* Ruhsatta ikisi de var (Y.4 · C.1.3) ve **fatura tam olarak bunlara kesilir** — kişide 11 haneli TC, firmada 10 haneli vergi no. `RUHSAT-OKUMA.md` Bulgu 2 bunları *"bize gerekmiyor"* diye elemişti; **gerekçe doğruydu ama tek bir işe (araç kaydı) bakıyordu** |
| `TaxOffice` | **Vergi dairesi — RUHSATTA YOK.** Fatura için gerekiyor, belgede yazmıyor: okunamaz, **usta yazar**. Alan var, okuma yok. Bir alanı ruhsattan doldurulabilir sanmak, uydurmanın davetidir |
| `Note` | Serbest — *"arka cam kolu kırık"* |
| `Color` · `FuelType` · `EngineNo` · `Displacement` · `PowerKw` · `Seats` · `FirstRegistrationDate` · `FuelTankLiters` | **Ruhsatın teknik alanları** (K-033.2c). Araç ekranının "Ruhsat bilgileri" akordiyonunda gösterilir ve girilir. *"Gösterilen ama girilemeyen alan sonsuza kadar boş kalır."* Ruhsat okununca dolar (K-065). Hepsi boş olabilir |
| `MarketingConsentAt` | **Ticari ileti izni — verildiği gün** (K-071.3, A-M21). Null = izin yok, varsayılan. Toplu mesaj (bayram, kampanya) yalnız dolu olana gider. **Tarih tutulur, bayrak değil:** ispat yükü dükkânda. Müşteri araçtan türetildiği için izin sahibin **bütün araçlarına birlikte** yazılır |

> **Araç kimliği kuralı (K-068):** parça sormak için `Vin` (17 hane) **ya da**
> `Brand + Model + Year + Engine` dolu olmalı. Kayıt açmak için gerekmez.
> Kural API'de (§İş kuralları).

> **İkinci iletişim iki alandır, tablo değil.** v1'de müşteri dünyası
> üç tabloydu (`Party`, `Contact`, `VehicleContact`) ve tamircinin
> *"karmaşık"* dediği yığının içindeydi. İhtiyaç *"aracı kim getiriyor,
> kimi arayacağım"*; bu iki alanla cevaplanıyor. **Üçüncü bir iletişim
> gerekirse tablo o zaman konuşulur** — şimdi değil.
>
> **`Customer` diye ayrı tablo YOK.** Tamirci araçla düşünüyor; müşteri, aracın üstünde iki alan. Bir müşterinin iki aracı varsa ad iki kez yazılır — bunun tek maliyeti hatırlatmanın iki kez gitmesi (A-17: kabul edilmiş).
>
> **Kontrol tarihleri YOK** (muayene, sigorta, kasko). v1'de vardı; tamirci hiç bahsetmedi.

---

## 2 · `ServiceRecord` — Servis kaydı

Akıştaki yeri: **Adım 1–7** · Ekran 3, 5. **Ürünün merkezi.**

| Alan | Neden var |
|---|---|
| `VehicleId` | Kayıt bir araca aittir |
| `Type` | `Repair` \| `Maintenance` \| `Both` — **K-006, K-026.3'te genişledi.** `Both` bakım sayılır: km sorulur, hatırlatma kurulur (`bakimIceriyor`) |
| `Km` | Yalnız bakımda istenir — **K-002** |
| `Complaint` | *"Ne istiyor"* — Adım 2. Serbest metin, tek alan (şikâyet/tespit ayrımı yok) |
| `DeliveredAt` | **Tek durum alanı** — **K-004.** Null ise araç içeride |
| `PaymentMethod` | `Cash` \| `Card` \| `Transfer` \| `Unpaid` — Adım 7. `Unpaid` sonradan ödenirse ödeme yoluna döner |
| `PaymentCollectedAt` | **Veresiye sonradan ödendiyse ödendiği gün** (K-071.1). Teslimde ödendiyse BOŞ kalır — tahsil günü `DeliveredAt`'tır. Tahsilat bu güne sayılır: Ağustos'ta teslim edilip Ekim'de ödenen para Ekim'in kasasıdır |
| `ReminderDate` | Bakımsa teslimde hesaplanır |
| `ReminderSent` | Aynı hatırlatma iki kez gitmesin |
| `WaitingForPart` | **Parça yolda.** Usta işaretler — K-033.5, K-034. Türetilemez: veride bunu söyleyen hiçbir şey yok |
| `WaitingSince` | Bayrağın **ne zaman** konduğu — süreçte "Parça bekleniyor" adımının tarihi. Bayrak kalkınca boşalır |
| `ShowWaitingToCustomer` | **Parça beklendiği müşteriye söylensin mi.** Bayrak açık olsa bile söylemek ayrı bir karar — A-M15, Cemil: *"İsterse, evet."* K-034'ün ölçüsünden geçiyor: kullanıcının yapabilecekleri değişmiyor |
| `TypeSetManually` | **Türü usta elle seçti.**

> **`Status` YOK.** Yedi durum yerine `DeliveredAt` — dolu ya da boş (K-004).
>
> ⚠️ **06.09 — `WaitingForPart` bu kuralın istisnası değil, sınırın çizildiği yer (K-034).** Burada eskiden *"'parçası bekleniyor' gibi bilgiler türetilir, saklanmaz"* yazıyordu ve **yanlıştı**: türetilen `fiyatiBekleyen` **başka bir şeydir** — *fiyatı gelmemiş* demek. Parça elde olup fiyatı belirsiz olabilir; fiyatı belli olup parça yolda olabilir. İkisini tek şeye bindirmek, ikisini de yanlış söylemekti.
>
> **Bayrak ≠ durum.** `WaitingForPart` ve `TypeSetManually` akışı değiştirmez, hiçbir ekranı kilitlemez, teslimi engellemez. `DeliveredAt` **hâlâ tek durum alanıdır.** Ayrım şu soruyla yapılır: *bu alan dolduğunda kullanıcının yapabilecekleri değişiyor mu?* Değişiyorsa durumdur ve girmez; değişmiyorsa bayraktır.
>
> **`OpenedAt` YOK** — prototipteki `acilis`, her tabloda ortak olan `DateTimeCreated`'tır. Ayrı alan açmak aynı tarihi iki yerde tutmak olurdu.
>
> **`ApprovalStatus` YOK** (K-003). **`DiscountAmount` YOK** — toplam elle değiştirilebilir (A-15).
>
> `Total` alanı **yok**: satırlardan hesaplanır. Saklanan toplam, satır değişince yalan söyler.

---

## 3 · `ServiceLine` — Satır

Akıştaki yeri: **Adım 2, 4, 5, 6** · Ekran 3

| Alan | Neden var |
|---|---|
| `ServiceRecordId` | Satır bir kayda aittir |
| `Kind` | `Part` \| `Labor` — **satırın işareti, ayrı liste değil** |
| `Name` | Ustanın yazdığı/söylediği: *"triger seti"* |
| `Price` | Müşteriye söylenen. **Müşteri parçasında yazılı kalır ama TOPLAMA GİRMEZ** (S2, K-037) — eleme okuma tarafında; bir arayüz düğmesi veriyi silmez |
| `Cost` | Parçacıdan gelen — usta üstüne ekliyor (T-9). Müşteriye gitmez. **Kol D'de müşteri parçayı kendi getirse bile SİLİNMEZ** — *"ne kadara alabileceği"* bilgisi değerli (03.09). **Null ≠ 0** (K-057): null = bilinmiyor, kâra girmez; 0 = bedava. Nullable olmalı |
| `SuppliedBy` | `Shop` \| `Customer` — **K-005, sorumluluk anahtarı** |
| `PartBrand` · `PartCode` | Ayrıştırılabildiyse (K-001). **Boş olabilir** |
| `MatchStatus` | `Coded` \| `Matched` \| `Unidentified` — `PARCA-TANIMLAMA.md` §3 |
| `Position` | **Satırın sırası** — ustanın yazdığı sıra (prototipte dizinin sırası). Silinip geri alınan satır **yerine** döner (prototip: `splice(i, 0, silinen)`). Kimlik Guid olduğu ve aynı anda eklenen satırların oluşturulma anı eşit olabildiği için sıra ayrı tutulur (B3, 22.10) |
| `SelectedReplyId` | **Kol B · hangi parçacının cevabı kullanıldı.** Birden fazla parçacıya sorulmuşsa usta birini seçiyor; seçmezse boş kalır ve akış durmaz (03.09) |

> `SuppliedBy` yalnız `Kind = Part` için anlamlı. İşçilikte boş.
>
> **`Certainty` YOK** (kesin/olabilir/şartlı) — v1'in icadıydı, tamirci böyle konuşmadı.
> **`Quantity` YOK** — *"balata takımı"* zaten takım. Gerekirse ada yazılır. **Cemil onayına açık.**

---

## 4 · `Supplier` — Parçacı

Akıştaki yeri: **Adım 3** · Ekran 4, Ayarlar

| Alan | Neden var |
|---|---|
| `Name` | *"Öz Yedek Parça"* |
| `Phone` | **WhatsApp buraya gider** (T-12b) |

İki alan. Cari hesap, adres, vergi no, ödeme vadesi — **yok.** Tamircinin istediği: *"Yerel parçacının tel belli. Bunları eklememiz gerekecek."*

---

## 5 · `PriceRequest` — Fiyat sorusu

Akıştaki yeri: **Adım 3** · Ekran 4

| Alan | Neden var |
|---|---|
| `ServiceRecordId` | Hangi iş için soruldu |
| `SupplierId` | Kime soruldu |
| `SentText` | **Giden mesajın kendisi.** Sonradan *"ne sormuşuz"* diye bakılır |
| `SentAt` | Ne zaman soruldu — cevapsız kalanı görmek için |

Birden fazla parçacıya sorulursa **her biri ayrı kayıt** (Kol B · büyük iş).

> ⚠️ **Prototipte soru SATIRA bağlı** (`satirlar[].sorulan` — hangi parçacılara
> soruldu). Bir mesaj birden çok satırı sorar; satırla sorunun bağı için
> `PriceRequestLine (PriceRequestId, ServiceLineId)` ara tablosu öneriliyor
> (A-24). Ara tablo, mesaj metnini satır sayısı kadar çoğaltmaktan iyi.

---

## 6 · `PriceReply` — Fiyat cevabı

Akıştaki yeri: **Adım 4** · Ekran 4

| Alan | Neden var |
|---|---|
| `PriceRequestId` | Hangi soruya cevap |
| `RawText` | **Parçacının yazdığı, olduğu gibi.** Ayrıştırma yanlışsa dönülecek tek yer |
| `ParsedPrice` · `ParsedBrand` · `ParsedCode` | Çıkarılabildiyse |
| `ReceivedAt` | Ne zaman geldi |

> **`RawText` en kritik alan.** v1 dersi: türetilmiş veriyi saklayıp kaynağı atmak, ayrıştırma hatasını **kalıcı** yapıyor.

---

## 7 · `PartMatch` — Biriken eşleşme

Akıştaki yeri: akışta **yok** — arkada birikir. `PARCA-TANIMLAMA.md` §4

| Alan | Neden var |
|---|---|
| `VehicleBrand` · `VehicleModel` · `VehicleYear` · `VehicleEngine` | Eşleşmenin araç yarısı |
| `RequestedName` | *"triger seti"* — ustanın dili |
| `PartBrand` · `PartCode` | Parçacının verdiği |
| `SeenCount` | Kaç kez doğrulandı — güven göstergesi |

**Bu tablo `TenantId` taşımaz — ORTAK'tır** (`PartMatch` bilgisi dükkândan dükkâna değişmez). Devralınan katalog hibrit kararıyla uyumlu. **Fiyat burada tutulmaz** — o dükkâna özeldir ve `PriceReply`'de durur. (A-P4)

---

## 8 · `Shop` — Dükkân ayarları

| Alan | Neden var |
|---|---|
| `Name` · `Address` · `Phone` | Müşteriye giden özette (T-19) |
| `DefaultMaintenanceMonths` | Hatırlatma periyodu — varsayılan **6** (A-3) |
| `DefaultMaintenanceKm` | Aynı periyodun km yarısı — varsayılan **10.000**. Ay **veya** km, hangisi önce gelirse (Cemil, 03.09) |
| `AskPartCost` | Ayarlar: parça alış fiyatı sorulsun mu (A-M43). Varsayılan **kapalı** — *"Zorunlu ya da beklenen alan olmamalı."* |
| `PrivacyNoticeShownAt` | Gizlilik notu (A-M33) bir kez gösterildi mi — ilk ses ya da ruhsat çekiminde. Prototipte bayrak; backend'de tarih (kime, ne zaman gösterildi) |

> **Tema bu tabloda değil:** cihazın tercihi, cihazda kalır (K-010).

---

## 9 · `Expense` — Gider

Akıştaki yeri: Muhasebe · Basit. **K-022 / MUH-B: kasa + gider.**

| Alan | Neden var |
|---|---|
| `Date` | Hangi döneme sayılacağı — dönem süzgeci bununla çalışır |
| `Description` | *"Elektrik faturası"* — ustanın yazdığı ya da söylediği |
| `Amount` | Tutar. CSV'ye ham rakam gider (K-071.2) |

> **Yedek parça gider DEĞİLDİR** (Cemil, 05.09): parça alışı `ServiceLine.Cost`'ta
> durur ve iş başına farktan düşülür. İkisine birden yazılırsa aynı para iki kez düşer.
>
> **`Type` / `Category` YOK.** Prototipte `tur` vardı ama hiçbir ekran onu
> okumuyordu; 22.10'da silindi. Gider türüne göre rapor istenirse o gün eklenir.

---

## 10 · `Message` — Gönderilen mesaj

Akıştaki yeri: Mesaj ekranı · Mesajlar (arşiv) · servis kaydının süreci.
**K-048.2:** gönderilen her mesaj saklanır. **A-23 böylece cevaplandı.**

| Alan | Neden var |
|---|---|
| `VehicleId` | Hangi araç için — arşiv araca göre süzülür |
| `ServiceRecordId` | **Boş olabilir.** Kayıtla ilgiliyse süreçte adım olur ("Özet gönderildi"); bayram mesajının kaydı yok |
| `RecipientName` · `RecipientPhone` | **Gönderildiği AN kime gittiği.** Araçtaki telefon sonradan değişebilir; arşiv o gün kime gittiğini söylemeli |
| `Channel` | `WhatsApp` \| `Sms` \| `Call` — aramak da hatırlatma sayılır |
| `Template` | Hangi hazır metin (`ozet`, `hazir`, `hatirlatma`, `bayram` …) — süreç ve "bu mesaj gönderildi mi" önerileri buna bakar |
| `IsBulk` | Toplu gönderimin parçası mı. **Toplu da olsa kişi başına bir kayıt** (K-045.3): ispat kişi başına |
| `Body` | Giden metnin kendisi. *"Ne yazmıştık"* sorusunun tek cevabı |
| `SentAt` | Prototipte `tarih` + `saat`; ortak `DateTimeCreated` yeter |

> **Saklama süresi üç yıl** (TOPLU-MESAJ.md, K-045.3). KVKK silme kuralı A-M36'yla birlikte.

---

## Model haritası

```
Shop ──┐
       │
Vehicle ─────< ServiceRecord ─────< ServiceLine
   │                │                    │
   │                └──< PriceRequest >──┼── Supplier
   │                          │          │
   │                          ├──< PriceRequestLine (A-24)
   │                          └──< PriceReply
   │
   └──< Message >── (ServiceRecord, isteğe bağlı)

Expense    (dükkânın gideri — araca bağlı değil)
PartMatch  (ortak — dükkâna bağlı değil, arkada birikir)
```

**On tablo** (+ A-24 ara tablosu). İlk sekiz yedi işi yapıyor; `Expense` ve
`Message` muhasebe kararıyla (K-022) ve mesaj arşiviyle (K-048.2) geldi.

---

## İş kuralları — prototipte arayüzde, backend'de API'de

CLAUDE.md: *"İş mantığı/karar/yetki API'de — arayüz karar vermez."* Prototipte
backend olmadığı için bu kurallar tarayıcıda yazıldı; **hepsi API'ye taşınır**,
arayüz yalnız sonucu gösterir. Fonksiyon adları `prototypes/kaynak/betik/02-veri.js`.

| Kural | Prototipte | Karar |
|---|---|---|
| Kayıt toplamı satırlardan; müşteri parçası toplama girmez | `kayitToplami` | K-037 |
| Bilinmeyen alış (null) kâra girmez | iş başına fark | K-057 |
| Tahsil günü: `PaymentCollectedAt ?? DeliveredAt`, `Unpaid` hariç | `tahsilTarihi` · `donemTahsilatlari` | K-071.1 |
| Alacak bir bakiyedir, döneme tabi değil | muhasebe alacak tablosu | 05.09 |
| Dönem: bugün · bu ay · bu yıl (1 Ocak'tan bugüne) | `donemBasi` · `donemIcinde` | 05.09 |
| Hatırlatma: ay **veya** km, hangisi önce | teslim hesabı | 03.09 |
| Parça sormak için araç kimliği: VIN ya da marka+model+yıl+motor | `aracKimligiTamam` | K-068 |
| Ruhsattan okunan plaka yazılanı ezmez; çatışmada usta seçer | `ruhsatKaydet` | K-069 |
| Toplu mesaj yalnız `MarketingConsentAt` dolu müşteriye | `topluAlicilar` | K-071.3 |
| CSV: BOM + `;` + ham rakam + formül koruması | `muhasebeCsv` · `csvHucre` | K-071.2 |
| Müşteri ayrı tablo değil, araçtan türetilir (ad + telefon) | `musteriler` | A-M1 |

---

## v1'den düşenler

| Ne | Neden |
|---|---|
| `Customer` / `Party` ayrı tablo | Tamirci araçla düşünüyor |
| `Status` + durum geçişleri | K-004 — tek durum |
| `Approval` · `Rejection` · `Discount` | K-003 |
| `Warranty` + süre alanları | K-005 — garanti süresi yok |
| `Certainty` | v1'in icadıydı |
| `VehicleType` · `GenericArticle` · `Product` · `Fitment` | K-001 — uyumu parçacı çözüyor |
| Kontrol tarihleri, randevu, personel, rol, izin | Tamirci hiç bahsetmedi |

**Kıyas:** v1 veri modeli dört katmanlı ürün yapısı + tam katalog + yetki matrisi öngörüyordu.

---

## AÇIK

| Kod | Karar |
|---|---|
| **A-20** | `ServiceLine.Quantity` gerekli mi? Öneri: hayır, ada yazılır |
| **A-21** | Bir müşterinin iki aracı — ad iki kez yazılıyor. Kabul mü? (A-17 ile bağlı) |
| **A-22** | `PartMatch` ortak olsun dedik — **hangi dükkânın öğrettiği** tutulsun mu? Öneri: hayır, gereksiz |
| ~~A-23~~ | **Cevaplandı (K-048.2):** gönderilen her mesaj saklanır → `Message` tablosu |
| **A-24** | Parça sorusu ile satırın bağı: `PriceRequestLine` ara tablosu. Öneri: evet. Teknik bir karar; backend turunda kesinleşir |

---

## Prototip eşlemesi

**Derleme denetimi 9 bu tabloyu okur** (`tools/prototip-derle.js`).
Prototipin tohum verisindeki (`02-veri.js`) her alan burada `DİZİ.alan`
biçiminde yazılı olmalı. Yazılı değilse derleme düşer. Ekranlarda kayda ya da
araca yazılan alanın tohumda da bulunması gerekir; bunu da aynı denetim arar.
**Neden:** backend bu belgeden kurulacak. Prototipe girip buraya girmeyen alan,
taşınırken sessizce kaybolur.

| Prototip | Model | Not |
|---|---|---|
| `ARACLAR.plaka` | `Vehicle.PlateNo` | |
| `ARACLAR.marka` · `ARACLAR.model` · `ARACLAR.yil` | `Vehicle.Brand` · `Model` · `Year` | |
| `ARACLAR.motor` | `Vehicle.Engine` | |
| `ARACLAR.vin` | `Vehicle.Vin` | |
| `ARACLAR.renk` · `ARACLAR.yakit` | `Vehicle.Color` · `FuelType` | |
| `ARACLAR.motorNo` · `ARACLAR.silindir` · `ARACLAR.guc` | `Vehicle.EngineNo` · `Displacement` · `PowerKw` | |
| `ARACLAR.koltuk` · `ARACLAR.ilkTescil` · `ARACLAR.depo` | `Vehicle.Seats` · `FirstRegistrationDate` · `FuelTankLiters` | |
| `ARACLAR.sahipAd` · `ARACLAR.sahipTel` | `Vehicle.OwnerName` · `OwnerPhone` | |
| `ARACLAR.iletisimAd` · `ARACLAR.iletisimTel` | `Vehicle.ContactName` · `ContactPhone` | |
| `ARACLAR.tcVergiNo` · `ARACLAR.adres` · `ARACLAR.vergiDairesi` | `Vehicle.TaxOrNationalId` · `Address` · `TaxOffice` | |
| `ARACLAR.not` | `Vehicle.Note` | |
| `ARACLAR.iletiIzni` | `Vehicle.MarketingConsentAt` | prototipte `{ tarih }` |
| `KAYITLAR.id` | `ServiceRecord.Id` | |
| `KAYITLAR.plaka` | `ServiceRecord.VehicleId` | |
| `KAYITLAR.tur` · `KAYITLAR.turElle` | `ServiceRecord.Type` · `TypeSetManually` | |
| `KAYITLAR.km` · `KAYITLAR.sikayet` | `ServiceRecord.Km` · `Complaint` | |
| `KAYITLAR.acilis` | `ServiceRecord.DateTimeCreated` | ortak alan |
| `KAYITLAR.teslimTarihi` · `KAYITLAR.odemeYolu` · `KAYITLAR.tahsilTarihi` | `ServiceRecord.DeliveredAt` · `PaymentMethod` · `PaymentCollectedAt` | |
| `KAYITLAR.hatirlatmaTarihi` · `KAYITLAR.hatirlatmaGonderildi` | `ServiceRecord.ReminderDate` · `ReminderSent` | |
| `KAYITLAR.parcaBekliyor` · `KAYITLAR.bekliyorTarihi` · `KAYITLAR.bekliyorMusteriye` | `ServiceRecord.WaitingForPart` · `WaitingSince` · `ShowWaitingToCustomer` | |
| `KAYITLAR.satirlar` | `ServiceLine` (ServiceRecordId) | |
| `KAYITLAR.satirlar.tip` · `KAYITLAR.satirlar.ad` | `ServiceLine.Kind` · `Name` | |
| `KAYITLAR.satirlar.fiyat` · `KAYITLAR.satirlar.alis` | `ServiceLine.Price` · `Cost` | |
| `KAYITLAR.satirlar.kimGetirdi` | `ServiceLine.SuppliedBy` | |
| `KAYITLAR.satirlar.marka` · `KAYITLAR.satirlar.kod` · `KAYITLAR.satirlar.tanim` | `ServiceLine.PartBrand` · `PartCode` · `MatchStatus` | |
| `KAYITLAR.satirlar.sorulan` | `PriceRequestLine` → `PriceRequest.SupplierId` | A-24 |
| `MESAJLAR.id` | `Message.Id` | |
| `MESAJLAR.tarih` · `MESAJLAR.saat` | `Message.DateTimeCreated` | ortak alan |
| `MESAJLAR.plaka` · `MESAJLAR.kayit` | `Message.VehicleId` · `ServiceRecordId` | |
| `MESAJLAR.ad` · `MESAJLAR.tel` | `Message.RecipientName` · `RecipientPhone` | |
| `MESAJLAR.kanal` · `MESAJLAR.sablon` · `MESAJLAR.toplu` · `MESAJLAR.metin` | `Message.Channel` · `Template` · `IsBulk` · `Body` | |
| `PARCACILAR.ad` · `PARCACILAR.tel` | `Supplier.Name` · `Phone` | |
| `DUKKAN.ad` · `DUKKAN.adres` · `DUKKAN.tel` | `Shop.Name` · `Address` · `Phone` | |
| `DUKKAN.bakimPeriyoduAy` · `DUKKAN.bakimPeriyoduKm` | `Shop.DefaultMaintenanceMonths` · `DefaultMaintenanceKm` | |
| `DUKKAN.alisSor` · `DUKKAN.gizlilikGoruldu` | `Shop.AskPartCost` · `PrivacyNoticeShownAt` | |
| `GIDERLER.id` · `GIDERLER.tarih` · `GIDERLER.aciklama` · `GIDERLER.tutar` | `Expense.Id` · `Date` · `Description` · `Amount` | |

**Eşlemede olmayanlar — bilerek:**
- `HAZIR_METIN` · `MESAJ_SABLONLARI` · `SABLON_AD` · `KANAL_AD` · `AYLAR` ·
  `DONEMLER` · `*_KOKLERI` · `FIRMA_ISARETLERI`: **kod sabiti**, dükkânın verisi
  değil.
- Söyleyiş sözlüğü (`05-soyleyis.js`): kod sabiti. Onaylı giriş yoluyla büyür
  (`SOYLEYIS-BAKIM.md`).
- İz kaydı (`00-iz.js`): telemetri, iş verisi değil. Ayrı kanal; backend turunda.
