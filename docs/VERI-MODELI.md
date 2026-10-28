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
| `OwnerType` | **Sahip tipi — yalnız YER (C6, Faz 3).** `Individual` \| `Company`; boş = bilinmiyor. Tevkifat yalnız şirket/kurum alıcıda var, bireyselde YOK (KDV.md §2). Ekranda yok; ileride vergi no uzunluğundan (10/11 hane) önerilebilir |
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
| `PaymentCollectedAt` | **Veresiye sonradan ödendiyse ödendiği gün** (K-071.1). Teslimde ödendiyse BOŞ kalır — tahsil günü `DeliveredAt`'tır. Tahsilat bu güne sayılır: Ağustos'ta teslim edilip Ekim'de ödenen para Ekim'in kasasıdır. **K-078'den sonra:** kasa `Payment` satırlarından okunur; bu iki alan kaydın **ödeme durumunun özeti** olarak kalır (kalan varken `Unpaid`, kapanınca son ödemenin yolu/günü) — prototipteki `odemeYolu`/`tahsilTarihi`'nin aynısı |
| `ReminderDate` | Bakımsa teslimde hesaplanır |
| `ReminderSent` | Aynı hatırlatma iki kez gitmesin |
| `WaitingForPart` | **Parça yolda.** Usta işaretler — K-033.5, K-034. Türetilemez: veride bunu söyleyen hiçbir şey yok |
| `WithholdingRate` | **KDV tevkifat oranı — yalnız YER (C6, Faz 3).** Boş = belirlenmedi. Araç bakım-onarımında 7/10 (KDV.md §2) ama alıcıya ve tutara bağlı; hesap Faz 3'te, fatura kesme kararıyla. Ekranda yok |
| `SummaryCode` | **Müşteri özet linkinin kodu (D3, K-076.3).** 16 karakter, rastgele, okunurken karışan harf yok; bütün dükkânlarda tekil. İlk istendiğinde üretilir, sonra **değişmez** — gönderilmiş link ölmesin. Prototipte plaka + kayıt numarasıydı (tahmin edilebilir, A-M9) |
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

## 2b · `Payment` — Alınan ödeme (K-078 · 24.10)

Akıştaki yeri: **Adım 7** (teslim) ve sonrası (kayıtta "Ödeme alındı"). Cemil: *"elden
taksit de x miktar aldım şeklinde olur."* Bir işin parası birden çok parçada gelebilir.

| Alan | Neden var |
|---|---|
| `ServiceRecordId` | Ödeme bir işe aittir (müşteriye değil — cari hesap YOK, K-022) |
| `Amount` | Alınan tutar. **> 0, kalandan fazla olamaz** (Cemil: *"Kalandan fazla ödeme olmaz"*) |
| `Method` | `Cash` \| `Card` \| `Transfer` \| `Advance` (avanstan düşüldü, §2c) — `Unpaid` olamaz: alınmayan para ödeme değildir |
| `PaidOn` | Paranın geldiği gün — **kasaya bu güne yazılır**, CSV'de ayrı satır (Cemil: *"Ayrı satır"*) |
| `Sequence` | **Yazılış sırası** (kayıt içinde 1, 2, 3…) — K-078.5. "Son ödemeyi geri al", "son ödemenin yolunu düzelt" ve durum özeti SON ödemeyi bilmek zorunda; veritabanından gelen sıra yazılış sırası değildir (aynı gün iki taksit tarihten de ayrılmaz). `ServiceLine.Position`'ın aynısı |
| `AtDelivery` | **Teslimde mi alındı** (K-078.1). Teslimdeki para teslimle gider (teslimi geri al); sonradan gelen "ödemeyi geri al"la — yanlış teslimi geri alan usta sonraki tahsilatı farkında olmadan silmesin |

> **Kalan** saklanmaz: `ServiceRecord.Total − Σ Amount` (en az 0). Saklanan kalan, satır
> değişince yalan söylerdi (`Total` alanının olmamasıyla aynı gerekçe). Teslimden sonra satır
> değişirse kalan yeniden hesaplanır; **alınan para değişmez** — para bir olgudur.
>
> **Göç:** var olan teslimli kayıtlara satır üretilir — teslimde ödenmiş: (toplam, yol, teslim
> günü); sonradan: (toplam, yol, tahsil günü); veresiye: satır yok. Göçten önce ve sonra kasa aynı.
>
> **Prototipte iki biçim, backend'de bir:** prototip tam ödemeli kayıtta `odemeler` yazmaz,
> ödemeyi `odemeYolu`/`tahsilTarihi`'nden türetir (`kayitOdemeleri` — eski veri olduğu gibi
> okunsun); backend her alınan parayı `Payment` satırı olarak tutar. Hesaplar ikisinde de
> yalnız ödemeleri okur, bu yüzden rakamlar aynıdır (`PrototypeEquivalenceTests`).
>
> **Müşteri avansından düşülen ödeme** (G7, K-079.1): `Method = Advance`. İşin kalanını azaltır
> ama **kasaya yazılmaz** — para avansın alındığı gün girmişti (§2c).

---

## 2c · `Advance` — Müşteri avansı (G7 · 27.10 · K-079.1)

Cemil: *"Hesabına önceden para verebilir."* Avans bir işe değil **aracın sahibine** aittir;
sonraki işin ödemesinde usta **"avanstan düş"** der. Tasarım: `TASARIM-G-AVANS.md`.

| Alan | Neden var |
|---|---|
| `VehicleId` | Alındığı araç. Bakiye **sahibe göre** toplanır: aynı sahip anahtarı (ad + telefon, A-M1) olan araçlar; sahip yazılmamışsa yalnız bu araç |
| `Kind` | `Received` (alındı, kasaya giriş) \| `Refunded` (iade, kasadan çıkış — K-079.1c) |
| `Amount` | > 0. İade bakiyeden fazla olamaz |
| `Method` | `Cash` \| `Card` \| `Transfer` — kasanın hangi satırına |
| `On` | Paranın geldiği/çıktığı gün — kasaya bu güne |

> **Bakiye** saklanmaz: `Σ Received − Σ Refunded − Σ Payment(Method = Advance)` (sahibin
> araçlarında). Saklanan bakiye hareketlerle ayrışırdı (kalanın saklanmamasıyla aynı gerekçe).
> Eksiye düşemez: avanstan düşülen ve iade edilen tutar bakiyeyi aşamaz; bakiyeyi eksiye
> düşürecek "geri al" reddedilir.

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
| `VatRate` | **KDV oranı (yüzde) — yalnız YER (C6, Faz 3 için).** Satır eklenirken dükkânın varsayılanından (`Shop.DefaultVatRate`, 20) yazılır ve **donar**: oran devlet kararıyla değişir, geçmiş iş kendi oranıyla yeniden basılabilmeli (KDV.md §1, A-K1). Ekranda yok, hesapta yok |
| `Position` | **Satırın sırası** — ustanın yazdığı sıra (prototipte dizinin sırası). Silinip geri alınan satır **yerine** döner (prototip: `splice(i, 0, silinen)`). Kimlik Guid olduğu ve aynı anda eklenen satırların oluşturulma anı eşit olabildiği için sıra ayrı tutulur (B3, 22.10) |
| `SelectedReplyId` | **Kol B · hangi parçacının cevabı kullanıldı.** Birden fazla parçacıya sorulmuşsa usta birini seçiyor; seçmezse boş kalır ve akış durmaz (03.09). **Backend (D2):** cevap satıra işlenince dolar (`ServiceLine.ApplyReply`) |
| `SupplierId` | **Parça kimden alındı** (H3 · 28.10 · K-090). Null olabilir. Parça sorda cevap satıra işlenince o cevabın parçacısı yazılır; usta elle seçebilir. Parçacıya borcun kime olduğu buradan |
| `OnCredit` | **Veresiye aldım** (K-090). Varsayılan `false` = parayı ödedim. Yalnız tamircinin aldığı, alışı yazılı, parçacısı belli parçada `true` olabilir; borç yalnız bundan doğar |

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

## 4b · `SupplierPayment` — Parçacıya ödeme (H3 · 28.10 · K-090)

Akıştaki yeri: Muhasebe · Detaylı · Cari. Tasarım: `TASARIM-H-MUHASEBE.md`.

| Alan | Neden var |
|---|---|
| `SupplierId` | Kime ödendi — silinmiş parçacı da adıyla kalır |
| `PaidOn` | Hangi gün (dönem süzgeci, CSV) |
| `Amount` | > 0, ≤ o anki borç |
| `Method` | `Cash` · `Card` · `Transfer` — "Almadım"/"Avanstan" yok |

> **Borç saklanmaz:** parçacı başına = `OnCredit` satırların `Cost` toplamı − ödemeler. Silinen (soft) satır
> ve ödeme hesaba girmez. **Gider değildir** (§9 — alış iş başına farktan düşülüyor), **kasa tahsilatına girmez**;
> CSV'de kendi Türüyle: "Parçacıya ödeme".

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
>
> **Backend (D2, 23.10):** A-24 kesinleşti — `PriceRequestLines (PriceRequestId, ServiceLineId)`,
> kiracı taşımaz (soru üzerinden okunur). Mesajı API **göndermez**: WhatsApp ustanın
> telefonunda açılır; API metni, kime gittiğini ve hangi satırları sorduğunu saklar.

---

## 6 · `PriceReply` — Fiyat cevabı

Akıştaki yeri: **Adım 4** · Ekran 4

| Alan | Neden var |
|---|---|
| `PriceRequestId` | Hangi soruya cevap |
| `RawText` | **Parçacının yazdığı, olduğu gibi.** Ayrıştırma yanlışsa dönülecek tek yer |
| `ParsedPrice` · `ParsedBrand` · `ParsedCode` | Çıkarılabildiyse |
| `ReceivedAt` | Ne zaman geldi |
| `SupplierAnswerId` | **Parçacının kendi yazdığı cevaptan mı** (H6 · K-091). Null = usta yazdı. "Bunu kullan" parçacının cevabını (§6b, parçacının kiracısında) tamirhanenin kiracısına bu tabloya kopyalar |

> **`ParsedPrice` = parçacının verdiği fiyat = ustanın ALIŞI** (K-076.2). Cevap satıra
> işlenince `ServiceLine.Cost` olur; müşteriye söylenen fiyatı usta ayrıca yazar (T-9:
> *usta üstüne ekliyor*). Gelmeyen alan satırda olanı silmez. Ayrıştırma YOK: usta
> rakamı kendi girer (prototip balonu: *"Cevap gelince fiyatı sen yaz"*).

> **`RawText` en kritik alan.** v1 dersi: türetilmiş veriyi saklayıp kaynağı atmak, ayrıştırma hatasını **kalıcı** yapıyor.

---

## 6b · `SupplierAnswer` — Parçacının kendi cevabı (H6 · 28.10 · K-091)

**Parçacının kiracısında** (`Shop.Kind = YedekParca`). Tasarım: `TASARIM-H-YEDEK-PARCA.md`.

| Alan | Neden var |
|---|---|
| `PriceRequestId` · `ServiceLineId` | Hangi soru, hangi satır — **tamirhanenin** kayıtları (kiracılar arası, yabancı anahtar yok). Yazarken: soru bu parçacının numarasına sorulmuş, satır o soruda sorulmuş olmalı |
| `Price` · `Brand` · `Code` | Parçacının verdiği (T-13: ürün adı · marka · OEM kodu). Hepsi boş olabilir ama cevap boş olamaz |
| `Note` | *"Yok"*, *"yarın gelir"* |
| `AnsweredAt` | Ne zaman |

> Cevap **değişmez**; yenisi yazılır. Tamirhane yalnız kendi sorusuna gelen cevabı okur; satıra kendiliğinden
> işlenmez (Kol B) — usta "Bunu kullan" der, cevap `PriceReply` olarak tamirhaneye kopyalanır.

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
| `DefaultVatRate` | KDV varsayılanı, yüzde — **20** (KDV.md karar önerisi `AYAR.kdvVarsayilan`). Yeni satır bunu alır ve dondurur. Yalnız YER (C6); ayar ekranı yok |
| `AskPartCost` | Ayarlar: parça alış fiyatı sorulsun mu (A-M43). Varsayılan **kapalı** — *"Zorunlu ya da beklenen alan olmamalı."* |
| `Kind` | `Tamirhane` (varsayılan) · `YedekParca` (H6 · K-091). Kayıtta seçilir; hangi uygulamanın uçlarını kullanacağını belirler. Bir telefon iki türden dükkân açamaz |
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

## 11 · Giriş — `User` · `LoginCode` · `Session` (K-074, 22.10)

Cemil: *"SMS ile. Ve programdan oturumu düşmeyecek. Tekrar tekrar girmek zorunda
kalmasın çıkış yapmadığı sürece."* Parola YOK: unutulan parola teknolojiden uzak
ustada dükkânı kilitler (T-18).

**`User`** — dükkânda programı kullanan kişi.

| Alan | Neden var |
|---|---|
| `TenantId` | Hangi dükkânın — oturum açılınca istek bu dükkâna bağlanır |
| `Phone` | Giriş kimliği. **Bütün sistemde tekil:** telefon → kişi → dükkân. Biçim `0532 215 30 00` |
| `Name` | Boş olabilir; "oturumlarım" listesinde ve izde |

**`LoginCode`** — SMS ile giden tek kullanımlık kod. **Kiracısız** (giriş öncesi).

| Alan | Neden var |
|---|---|
| `Phone` | Kime gitti |
| `CodeHash` | Kodun kendisi SAKLANMAZ — veritabanı sızsa da kod okunmaz |
| `ExpiresAt` | 5 dakika |
| `Attempts` | Yanlış deneme sayısı — 5'te kod ölür (6 haneli kodu tahminle bulmak engellenir) |
| `ConsumedAt` | Kod bir kez kullanılır |

**`Session`** — açık oturum, cihaz başına bir tane.

| Alan | Neden var |
|---|---|
| `TenantId` · `UserId` | Oturum kimin, hangi dükkânın |
| `TokenHash` | Cihazdaki anahtarın özeti; anahtarın kendisi saklanmaz |
| `DeviceName` | "Ahmet'in telefonu" — "bütün cihazlardan çık" listesinde tanınsın |
| `LastSeenAt` | En son ne zaman kullanıldı |
| `RevokedAt` | Çıkış yapıldı. **Süre dolması YOK** — oturum yalnız çıkışla biter (Cemil) |

> **Bedeli:** telefon kaybolursa oturum açık kalır. Karşılığı "bütün cihazlardan çık"
> ve oturum listesi. JWT yerine veritabanında oturum seçildi: süresiz bir JWT geri
> alınamaz, veritabanındaki oturum tek satırla kapanır.

---

## 12 · `ConsentEvent` — Ticari ileti izninin izi (D5, 23.10)

Akıştaki yeri: müşteri künyesindeki izin çipi (91-musteri) — ekran yok, iz arkada birikir.

| Alan | Neden var |
|---|---|
| `VehicleId` | İzin müşterinin bütün araçlarına birlikte yazılır (K-071.3); iz **araç başına** — ispat numara başınadır |
| `ActionId` | Aynı dokunuşla yazılan satırlar **tek eylem**; "geri al" eylemi bütün olarak geri alır |
| `Granted` | Olaydan sonra izin var mı |
| `PreviousDate` · `ConsentDate` | Olaydan önceki ve sonraki izin tarihi. **"Geri al" eski tarihi buradan alır** — istemciden değil (K-073.10 kapandı) |
| `Phone` | İznin ait olduğu numara, **o anki hâliyle** — araçtaki telefon sonradan değişebilir |
| `UserId` | Kim işaretledi |
| `RecordedOn` | Ustanın günü. Geri alma yalnız aynı gün, yalnız son eylem, bir kez (prototipte balonun "Geri al"ı saniyeler sürer) |
| `IsUndo` | Bu satır bir geri almadır — geri alma geri alınmaz |

> **Yalnız eklenir.** İz ispat kaydıdır; düzeltme, silme yok. Değişmeyen araç (zaten
> izni olana ikinci "evet") iz bırakmaz. **İznin NASIL alındığı** (sözlü/yazılı/SMS onayı)
> alanı YOK: bugün tek yol "usta işaretledi"; İYS bağlanınca eklenir (K-071.3, K-076.5).

---

## 13 · `TraceEvent` — İz kaydı (D9, 23.10 · K-052 · K-053)

Akıştaki yeri: **yok** — ustaya görünmez, bizim tarafa ait (destek · vazgeçme · zorlanma).

| Alan | Neden var |
|---|---|
| `UserId` · `SessionId` | Kim, hangi cihazda — **oturumdan**, istemci söylemez |
| `ClientAt` | İstemcinin saati: olaylar arası süre buradan; geliş anı `DateTimeCreated` |
| `Kind` | Prototipin türleri: `ekran` · `sure` · `dokunus` · `alan` · `okuma` · `duzeltme` · `hata` · `suphe` |
| `Name` | Ekran adı, alan adı, şüphe kodu… (80 harf) |
| `Data` | Ek, JSON — **değerler yazılır** (K-053); her değer 80 harfte kırpılır, en çok 12 anahtar |

> **Saklama (K-053.4):** ayrıntı **90 gün**, sonra **gerçekten silinir** (soft-delete değil —
> süresi dolan ayrıntı geri alınmaz). İstatistik (süresiz) henüz yok; A-M42 özetiyle gelir.

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

**22.10 (C11):** "Backend'de" sütunu eklendi — kuralın taşındığı yer. Muhasebe kurallarının
prototiple **0 farkla** aynı sonucu verdiği `PrototypeEquivalenceTests` ile ölçülüyor (C10).

| Kural | Prototipte | Backend'de (`src/`) | Karar |
|---|---|---|---|
| Kayıt toplamı satırlardan; müşteri parçası toplama girmez | `kayitToplami` | `ServiceRecord.Total` · `ServiceLine.CountsInTotal` | K-037 |
| İş başına fark: bilinmeyen alış (null) satışıyla hesap dışı, sayılır; 0 = bedava | `isKari` · `alisBilinmiyor` | `JobProfit.Of` | K-057 · K-055.2 |
| Parça alışı: teslim edilmiş işte, tamircinin aldığı, alışı > 0 | `parcaAlislari` | `ServiceLine.IsShopPurchase` | 04.09 |
| Tahsil günü: `PaymentCollectedAt ?? DeliveredAt`, `Unpaid` hariç | `tahsilTarihi` · `donemTahsilatlari` | `ServiceRecord.CollectedOn` + SQL biçimi `CollectedBetween` (eşitliği testli) | K-071.1 |
| Kasa = o dönemde ALINAN ödemeler (yola göre); kısmi ödemenin her parçası kendi gününde; CSV'de her ödeme ayrı satır | `kayitOdemeleri` · `donemOdemeleri` · `tahsilatToplami` · `bugunTahsilat` · `muhasebeCsv` | `Map.CollectedBetween` (SQL'de gün + yol) · `ExportCashCsv` | K-078 |
| Alacak = kalan (toplam − alınanlar, en az 0); kısmi ödenmiş iş kalanıyla; CSV alacak satırları teslim günü sırasıyla | `kalanTutar` · `veresiyeMi` · `veresiyeToplami` | `ServiceRecord.Remaining` + SQL biçimi `HasRemaining` (eşitliği testli) · `ListReceivables` · `Map.UnpaidTotal` | K-078 · K-078.6 |
| Ödeme > 0 ve kalandan fazla değil; teslimsiz işe ödeme yok; teslimde kısmi toplamdan fazla değil | `odemeHatasi` · `odemeEkle` · teslim ekranı | `ServiceRecord.CollectPayment(amount, …)` · `DeliverPartly` | K-078 · Cemil 24.10 |
| Kalan varken durum `Unpaid`; kapanınca son ödemenin yolu/günü | `odemeDurumu` | `ServiceRecord.RefreshPaymentStatus` | K-078 |
| Son (sonradan alınan) ödeme geri alınır; teslimdeki teslimle gider | `sonOdemeyiGeriAl` | `ServiceRecord.UndoCollection` · `UndoDelivery` | K-078 · K-078.1 |
| Göç: var olan teslimli kayda ödeme satırı; kasa önce/sonra aynı | — | `Payments` göçü + `MigrationUpgradeTests` | K-078 |
| Sesle "x aldım": yalnız kalanı olan iş; fiil + tutar ya da "kalanını"; yol söylenmezse onayda sorulur | `sesOdemeAl` · `odeme-al` şeması | `SesCozumleyici` (`odeme-al`) — cihazda | K-078.7 |
| Plaka sorusu: geçerli mi (ipucu), kayıtlı araç, açık kaydı (en yeni teslimsiz) | `ruhsatDurum` | `LookupPlate` (`GET /api/vehicles/lookup`) | K-080.6 |
| Kayıt açarken kayıtsız aracın bilgileri kayıtla tek istekte; biri geçersizse hiçbiri; kayıtlı araç ezilmez | `kaydiAc` (iki ayrı yazım) | `OpenServiceRecord.Vehicle` (`NewVehicleInfo`) | K-080.6 |
| Avans bakiyesi = alınanlar − iadeler − avanstan düşülenler, sahibe göre; eksiye düşmez | — (prototipte yok) | `AdvanceBalance` · `Advances` uçları | K-079.1 · G7 |
| Kasa: avans alındığı gün girer, iade çıkar; avanstan düşülen ödeme kasaya girmez | — | `Map.CollectedBetween` | K-079.1 · G7 |
| Alacak bir bakiyedir, döneme tabi değil | muhasebe alacak tablosu | `ListReceivables` (araç süzgeçli: `?vehicleId=`) | 05.09 |
| Dönem: bugün · bu ay · bu yıl (1 Ocak'tan bugüne) | `donemBasi` · `donemIcinde` | `Period.Of` | 05.09 |
| Hatırlatma: ay **veya** km, hangisi önce; km ancak iki ölçümle | `hatirlatmaHesapla` · `gunlukKm` | `MaintenanceReminder.Compute` | 03.09 · K-073.9 (ay sonu sapması) |
| Hatırlatması gelen: bakım, tarihi geldi, gönderilmedi; "hatırlatma" mesajı (arama dahil) kapatır | `hatirlatmasiGelenler` | `ServiceRecord.IsReminderDue` · `MarkReminderSent` | 85-mesaj |
| Teslim eksikleri söylenir, teslimi engellemez | `kEksikler` | `ServiceRecord.DeliveryWarnings` | 13.10 |
| Parça sormak için araç kimliği: VIN ya da marka+model+yıl+motor | `aracKimligiTamam` | `Vehicle.HasPartIdentity` · `MissingForParts` · kapı `PartQuestion.EnsureCanAsk` | K-068 |
| Sorulacak parça: tamircinin aldığı, fiyatı belli olmayan; kapılar sırayla konuşur (parça · kimlik · parçacı) | `psParcaListesi` · `psGonder` | `PartQuestion.AskableLines` · `EnsureCanAsk` · `Compose` | K-033.3 · K-076.2 |
| Ruhsattan okunan plaka yazılanı ezmez; çatışmada usta seçer; okununca araç kaydedilir, kayıtlıda yalnız boş alan dolar | `ruhsatOku` · `ruhsatKaydet` | `RegistrationIntake.Decide` · `NewVehicle` · `FillEmpty` (okuyucu arayüzü bağlanmadı — K-076.8) | K-065 · K-069 |
| Toplu mesaj yalnız `MarketingConsentAt` dolu müşteriye | `topluAlicilar` | `SendBulk` · `GetBulkPreview` | K-071.3 |
| CSV: BOM + `;` + ham rakam + formül koruması | `muhasebeCsv` · `csvHucre` | `Csv.Cell` · `Csv.Document` · `ExportCashCsv` (bayt bayt eşit) | K-071.2 |
| Müşteri ayrı tablo değil, araçtan türetilir (ad + telefon) | `musteriler` | `Customers.From` | A-M1 |
| Mesaj önerisi: hangi şablon BU İŞE uygun (kayıt bazlı); metin = hitap + gövde + imza | `MESAJ_SABLONLARI.uygunKayit` · `mesajMetniUret` · `hazirMesajMetni` | `MessageTemplates.Applicable` · `Text` · `Name` | K-019.1 · A-M17 · K-076.4 |
| İzin değişikliği izlenir; geri alma eski tarihi izden getirir (son eylem, aynı gün, bir kez) | `iletiIzniYaz` · `balonGeriAl` (91-musteri) | `Customer.SetConsent` · `UndoConsent` · `ConsentEvent` | K-071.3 · K-073.10 · K-076.5 |
| Ödeme yolu düzeltilir (SON ödeme satırının yolu); tutar/gün korunur; sonradan tahsilde "almadım" = tahsili geri al | `odeme-duzelt` (ses, E5) | `ServiceRecord.CorrectPaymentMethod` | K-077.2 · K-078 |
| Müşteri özeti: tek durum cümlesi, müşteri parçasında rakam yok, alış yok, yarım satır yok | `ozetGovdeHtml` | `CustomerSummary.StatusText` · `Amount` · `GetCustomerSummary` | K-023 · K-037 · K-076.3 |
| KDV oranı satırda donar (yalnız yer) | — | `ServiceLine.FreezeVatRate` | C6 · KDV.md |

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
| ~~A-24~~ | **Kesinleşti (D2, K-076.2):** `PriceRequestLine` ara tablosu |

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
| `KAYITLAR.odemeler` | `Payment` (ServiceRecordId) — `tutar` · `yol` · `tarih` → `Amount` · `Method` · `PaidOn` | yalnız kısmi ödemeli kayıtta dolu (K-078) |
| `KAYITLAR.hatirlatmaTarihi` · `KAYITLAR.hatirlatmaGonderildi` | `ServiceRecord.ReminderDate` · `ReminderSent` | |
| `KAYITLAR.parcaBekliyor` · `KAYITLAR.bekliyorTarihi` · `KAYITLAR.bekliyorMusteriye` | `ServiceRecord.WaitingForPart` · `WaitingSince` · `ShowWaitingToCustomer` | |
| `KAYITLAR.satirlar` | `ServiceLine` (ServiceRecordId) | |
| `KAYITLAR.satirlar.tip` · `KAYITLAR.satirlar.ad` | `ServiceLine.Kind` · `Name` | |
| `KAYITLAR.satirlar.fiyat` · `KAYITLAR.satirlar.alis` | `ServiceLine.Price` · `Cost` | |
| `KAYITLAR.satirlar.kimGetirdi` | `ServiceLine.SuppliedBy` | |
| `KAYITLAR.satirlar.marka` · `KAYITLAR.satirlar.kod` · `KAYITLAR.satirlar.tanim` | `ServiceLine.PartBrand` · `PartCode` · `MatchStatus` | |
| `KAYITLAR.satirlar.sorulan` | `PriceRequestLine` → `PriceRequest.SupplierId` | A-24 |
| `KAYITLAR.satirlar.kaynak` | `ServiceLine.SelectedReplyId` → `PriceReply` → `PriceRequest.SupplierId` | Kol B · K-076.2 |
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
