# A-11 · Ürün Yapısı Araştırması — bulgular ve öneri

> 30.08.2026 · **Bu bir ÖNERİDİR, karar değil.** K-003 ve K-021 buna bağlı; kararı Cemil verir (K-005).
> Kapsam: TecDoc (Avrupa) · ACES/PIES (Kuzey Amerika) · pazaryeri modeli · N-3 (oto elektrik vb.)

## Özet — tek cümle

İki büyük otomotiv veri standardı, birbirinden bağımsız geliştikleri hâlde **aynı dört katmana** varmış: **araç tipi · jenerik parça tipi · somut ürün · tipli öznitelik**. Tamiron'un ürün yapısını bu dörtlü üzerine kurmasını öneriyorum; kendi başımıza icat etmemiz gereken bir şey yok.

---

## 1 · TecDoc (TecAlliance) — Avrupa fiilî standardı

Kaynak: **TecDoc Data Format 2.6** resmî spesifikasyonu (179 sayfa, 13.12.2021). Tablo numaraları aşağıda spesifikasyondan birebir alınmıştır.

### 1.1 İki katmanlı ürün: jenerik tip ≠ somut ürün

| Katman | Tablo | Ne | Kim tanımlar |
|---|---|---|---|
| **Jenerik ürün (GenArt)** | `320` | "fren balatası", "yağ filtresi" — parçanın **ne olduğu** | TecDoc (ortak) |
| **Somut ürün (Article)** | `200` | Bosch 0986424815 — üreticinin gerçek ürünü | Tedarikçi |
| Bağ | `211` | Hangi somut ürün hangi jenerik tipe ait | Tedarikçi |

`320` tablosunda ayrıca **`OK_PC` / `OK-CV` / `OK-Eng` / `OK-Axle` / `OK-Universal`** bayrakları var: bir jenerik tipin **hangi bağlantı türlerinde kullanılabileceğini** kısıtlıyor. "Universal" bayrağı, araçtan bağımsız ürünleri (yağ, temizlik malzemesi) aynı yapıda tutmayı sağlıyor — **ayrı bir "araçsız ürün" mimarisi gerekmiyor.**

### 1.2 Parça araca DEĞİL, "bağlantı hedefi"ne bağlanır — K-003'ün cevabı

Tablo `400` (Article Linkage) birincil anahtarı:

    LnkTargetType · LnkTargetNo · GenArtNo · ArtNo · SeqNo

`LnkTargetType` = hedefin **türü**; `LnkTargetNo` = hedefin numarası. Spesifikasyonun kendi ifadesiyle hedef "PC, CV, engine, ..." olabilir — anahtar tablo 271'de: **2 = binek araç tipi (KType) · 16 = ticari araç tipi (NType) · 14 = motor · 19 = aks · 7 = üretici**.

Bunun anlamı büyük: **parça her zaman bir ARACA bağlanmaz.** Bir conta motora, bir balata aksa, bir ürün doğrudan üreticiye bağlanabilir. Tek bir "ürün↔araç" tablosu bu gerçeği taşıyamaz.

Tablo `115` bu hedefleri daha da ayrıştırıyor: *TecDoc PC · Motorcycle · **E-PC** · **E-LCV** · **E-Motorcycle** · CV · Bus · Tractor · **E-Bus** · LCV*. Elektrikli araçlar ayrı sınıf olarak duruyor — yapı elektrikli araca hazır.

### 1.3 Öznitelik sistemi — EAV değil, **tipli ve şablonlu**

Bu, "her ürüne uyabilecek esnek attribute" sorusunun asıl cevabı:

| Tablo | Ne yapar |
|---|---|
| `050` | **Kriter tanımı**: `Type` = `A` alfanümerik · `N` sayısal · `D` tarih · `K` anahtar (liste) · `V` değersiz; `MaxLen`; `TabNo` → `K` tipinde değer listesinin geldiği anahtar tablosu |
| `050` bayrakları | `OKArticle` / `OKPC` / `OKCV` / `OKEngine` / `OKDrivCab` — kriterin **nerede kullanılabileceği** |
| `328` / `329` / `331` | Jenerik tip başına **zorunlu** / **önerilen** / atanmış kriterler = **şablon** |
| `330` / `332` | O kriterin **izinli değer listesi** |
| `333` | **Kriterler arası kısıt** (birlikte tutarsız değerleri engeller) |
| `210` | Ürünün araçtan **bağımsız** sabit kriterleri |
| `410` | **Bağlantıya bağlı** kriterler — aynı parça farklı araçta farklı nitelik gösterebilir |

İki şey burada kritik:

1. **Öznitelik serbest metin değil, tipli**; `K` tipinde değer kapalı listeden gelir. Ham EAV'ın veri kalitesi sorunu böyle çözülüyor.
2. **`210` ile `410` ayrımı.** Bir balatanın kalınlığı üründe sabittir (`210`); ama "yalnız ABS'li modelde" bilgisi **ürünün değil, bağlantının** niteliğidir (`410`). Bu ayrım olmadan uyumluluk verisi doğru modellenemiyor.

### 1.4 Ticari gerçekliğin karşılıkları

`203` **OE / çapraz referans** (üretici + referans no + referans tipi + ülke dahil/hariç) · `204` **eskiyen ürünün yerine geçen** · `205` **set/kit içeriği** · `222` **aksesuar listesi** · `207` ticari numaralar · `201` fiyat.

---

## 2 · ACES / PIES (Auto Care Association) — Kuzey Amerika standardı

- **ACES** = uyumluluk (hangi parça hangi araca), **PIES** = ürün bilgisi (ağırlık, açıklama, menşe, görsel, fiyat).
- Beş veritabanı: **VCdb** (araç konfigürasyonu, 1896'dan beri) · **PCdb** (parça sınıflandırma) · **PAdb** (ürün öznitelikleri) · **Qdb** (qualifier — koşul ifadeleri) · Brand Table.

### Örtüşme — bulgunun asıl değeri

| Kavram | TecDoc | ACES/PIES |
|---|---|---|
| Araç tipi kataloğu | `120` / `532` (KType/NType) | **VCdb** |
| Jenerik parça tipi | `320` GenArt | **PCdb** |
| Öznitelik sözlüğü + şablon | `050` + `328/329/331` | **PAdb** |
| Uyumluluk koşulu | `410` linkage attributes | **Qdb** qualifier |
| Somut ürün bilgisi | `200` + `210` | **PIES** |

**İki standart, iki kıta, birbirinden bağımsız — aynı ayrışma.** Bu, yapının keyfî değil, problemin doğasından geldiğini gösteriyor. Tamiron farklı bir yapı seçerse bunu bilerek yapmalı.

---

## 3 · Pazaryeri modeli (Trendyol / Hepsiburada / N11) ve Sahibinden

- Pazaryerleri **kategori → öznitelik şeması** kullanır: her kategorinin zorunlu/opsiyonel öznitelikleri, bir kısmı **varyant belirleyici** (renk, beden). Kavramsal olarak TecDoc'un `331` şablonuyla aynı fikir, ama **araç uyumluluğu zayıftır** — genelde tek bir serbest "uyumlu araçlar" alanı.
- **Sahibinden ilginç bir karşı örnek:** orada **araç ilanın kendisidir** (satılık araç = ürün). Tamiron'da araç satılmıyor; bu yüzden Sahibinden'in modeli bize uymaz — ama K-003'ü doğru sormamızı sağlıyor (aşağıda).

**Sonuç:** pazaryeri modeli ürün kartı ve varyant için iyi bir referans; **uyumluluk için yetersiz**. Otomotivde uyumluluk asıl zorluk olduğu için omurgayı TecDoc/ACES tarafından almak gerekir.

---

## 4 · N-3'ün cevabı — oto elektrik, kablo vb.

**Ayrı bir ürün mimarisi gerekmiyor.** GenArt/PCdb yaklaşımında "kablo" da bir jenerik tiptir; kendi kriterleri (kesit mm², renk, uzunluk, damar sayısı) `331` şablonuyla tanımlanır. Araçtan bağımsız ürünler `OK-Universal` bayrağıyla aynı yapıda durur. Yani **tek ürün yapısı**, alan farkı **şablon farkı** olarak çözülür.

---

## 5 · Tamiron için kritik ayrım — ARAÇ ÖRNEĞİ ≠ ARAÇ TİPİ

Araştırmanın Tamiron'a en doğrudan katkısı bu:

| | Araç **örneği** | Araç **tipi** |
|---|---|---|
| Ne | Servise gelen **o** araba | "Mégane III 1.5 dCi 2012–2016" |
| Doğal anahtar | **Şasi no (VIN)** — D-1 | KType / VCdb benzeri tip numarası |
| Kim üretir | Dükkan (kendi müşterisi) | Ortak katalog |
| Neye bağlanır | Servis kaydı, müşteri | **Parça uyumluluğu** |

Prototipte topladığımız ruhsat alanları (marka / model / yıl / motor / yakıt) aslında **örneği tipe bağlayan** alanlar. Bu bağ kurulduğunda "bu araca uyan balatalar" sorusu cevaplanabilir hâle geliyor.

**D-1 ile tutarlı:** plaka değişse de örnek aynı kalır (VIN sabit), tip zaten hiç değişmez.

---

## 6 · Öneri — üç seçenek

### Seçenek A · Tam TecDoc uyumlu (ağır)

Tablo yapısını birebir taşı. **Artı:** TecDoc verisi satın alınırsa doğrudan içeri akar. **Eksi:** 179 sayfalık bir modelin karmaşıklığı; Tamiron'un ilk sürümünde kullanılmayacak onlarca tablo.

### Seçenek B · Dört katmanlı sadeleştirme — **önerilen**

Standartların ortak çekirdeğini al, TecDoc'a **göç edilebilir** kalacak şekilde adlandır:

1. `VehicleType` — araç tipi kataloğu (ortak; ileride KType eşlemesi için bir dış-kod alanı)
2. `GenericArticle` — jenerik parça tipi (ortak; hiyerarşik grup + kullanım amacı)
3. `Product` — somut ürün (K-002 gereği `TenantId` **nullable**: null = ortak, dolu = dükkan ürünü)
4. `Attribute` + `AttributeTemplate` — **tipli** öznitelik (A/N/D/K/V) + jenerik tip başına zorunlu/önerilen şablon + `K` için değer listesi
5. `Fitment` — **`TargetType` + `TargetId`** ile bağlantı (araç tipi / motor / aks / üretici) + bağlantıya özel nitelikler

**Artı:** problemin doğasına uygun, TecDoc/ACES'e göç edilebilir, K-002 hibrit modeliyle uyumlu.
**Eksi:** ilk sürüm için `TargetType` fazla gelebilir — ama sonradan eklemek şema kırar, şimdi koymak ucuz.

### Seçenek C · Basit ürün + serbest uyumluluk metni

Tek `Product` tablosu, uyumluluk serbest metin. **Artı:** en hızlı. **Eksi:** "bu araca uyan parçalar" sorusu **hiç** cevaplanamaz; sonradan dönüş çok pahalı. Yedek parçacı aktörü (K-028) için yetersiz.

---

## 7 · ⚠️ AÇIK — Cemil kararı gerekiyor

- **A-11.1 · Hangi seçenek?** (B öneriliyor.) K-021 buna bağlanacak.
- **A-11.2 · K-003'ün cevabı:** araç **ürün değildir**; ayrı `VehicleType` + `VehicleInstance` olarak öneriliyor. Onaylanırsa K-003 kapanır.
- **A-11.3 · TecDoc verisi satın alınacak mı?** Alınacaksa `VehicleType` ve `GenericArticle` en baştan dış-kod alanı taşımalı. Alınmayacaksa katalog elle/başka kaynakla dolacak — **maliyet ve zaman farkı büyük.** Bu ticari bir karar.
- **A-11.4 · İlk sürümde `TargetType` olacak mı?** (Öneri: evet — sonradan eklemek şema kırar.)
- **A-11.5 · Araç tipi kataloğunun kaynağı** — TecDoc mu, elle mi, yoksa ruhsat alanlarından **türetilen** hafif bir tip mi? Prototipte marka/model/yıl/motor zaten toplanıyor.

## Kaynaklar

- [TecDoc Data Format 2.6 — resmî spesifikasyon (PDF)](https://www.tec-doc-services.com/download/TecDoc-Data-Format_Version_2.6_EN.pdf)
- [TecAlliance — TecDoc kataloğu](https://www.tecalliance.net/tecdoc-catalogue/)
- [Auto Care Association — Data Standards (ACES/PIES)](https://www.autocare.org/data-standards)
- [ACES ve PIES verisi açıklaması](https://hedgescompany.com/blog/2024/07/product-data-questions-aces-pies-data/)
