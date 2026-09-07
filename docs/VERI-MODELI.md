# TAMIRON — VERİ MODELİ (taslak)

> **v1 · 03.09.2026** · Kod/DB **İngilizce**, UI **Türkçe** (`SOZLUK.md`)
> **Akıştan türetildi** (`IS-AKISI.md`). Kesin şema backend fazında kurulur.
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
| `Note` | Serbest — *"arka cam kolu kırık"* |

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
| `PaymentMethod` | `Cash` \| `Card` \| `Transfer` \| `Unpaid` — Adım 7 |
| `ReminderDate` | Bakımsa teslimde hesaplanır |
| `ReminderSent` | Aynı hatırlatma iki kez gitmesin |
| `WaitingForPart` | **Parça yolda.** Usta işaretler — K-033.5, K-034. Türetilemez: veride bunu söyleyen hiçbir şey yok |
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
| `Cost` | Parçacıdan gelen — usta üstüne ekliyor (T-9). Müşteriye gitmez. **Kol D'de müşteri parçayı kendi getirse bile SİLİNMEZ** — *"ne kadara alabileceği"* bilgisi değerli (03.09) |
| `SuppliedBy` | `Shop` \| `Customer` — **K-005, sorumluluk anahtarı** |
| `PartBrand` · `PartCode` | Ayrıştırılabildiyse (K-001). **Boş olabilir** |
| `MatchStatus` | `Coded` \| `Matched` \| `Unidentified` — `PARCA-TANIMLAMA.md` §3 |
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

---

## Model haritası

```
Shop ──┐
       │
Vehicle ─────< ServiceRecord ─────< ServiceLine
                    │
                    └──< PriceRequest >── Supplier
                              │
                              └──< PriceReply

PartMatch   (ortak — dükkâna bağlı değil, arkada birikir)
```

**Sekiz tablo.** Yedi işin hepsi bunlarla yapılıyor.

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
| **A-23** | Müşteriye giden özet **saklanacak mı** (ne gönderdik)? Öneri: evet, ama A-16 kararından sonra |
