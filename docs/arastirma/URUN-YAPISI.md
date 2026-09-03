# Ürün yapısı — üst ürün ve varyant

> **Durum:** karar notu · 01.09.2026 · iş 7
> **Kaynak (Cemil):** *"Triger seti yazıyor. Bu ürünün adı. Seçili araca uygun triger setleri OES, orijinal vb olsun. Üstte 'bize bu triger seti lazım'. Bu şekilde gruplandırma gerek. Usta da zaten kodu yazmayacak, triger seti yazacak. Bunu fiyatlandırma vb.'de de böyle kullanmalıyız."*

---

## 1. Neden iki katman

Usta **kod bilmez ve yazmaz**. Kafasındaki cümle şudur:

> *"Bu Mégane'a bir **triger seti** lazım."*

Kod (`Gates K015607XS`), marka (`Gates`) ve sınıf (`Muadil`) **sonra** gelir — ve çoğu zaman ustanın değil, **fiyatın** kararıdır. Yani veri modelinde iki farklı şey var ve bunlar bugüne kadar tek listede karışıktı:

| | **ÜST ÜRÜN** | **VARYANT** |
|---|---|---|
| Ne | İhtiyacın adı | Satın alınabilir somut mal |
| Örnek | *Ön fren balatası (takım)* | *Bosch 0986424815 · OES · 1.850 ₺* |
| Kim söyler | **Usta** | Tedarikçi / fiyat çalışması |
| Kaç tane | Araç başına 1 | Aynı ihtiyaca 2–10 |
| Neye bağlı | **Araca** (uyum) | Üst ürüne |

Eski `KATALOG` düz bir listeydi: aynı ihtiyacın iki markası **iki ayrı ürün** gibi duruyordu. Sonucu:
- Parça seçicide *"Ön fren balatası (takım)"* iki kez alt alta görünüyordu — usta hangisini seçeceğini bilmeden önce **aynı satırı iki kez okuyordu**.
- Stokta *"2 kalem balata var"* deniyordu; oysa **bir ihtiyaç, iki marka**.
- Fiyatlandırma zaten grup mantığıyla çalışıyordu (`USTURUNLER`) — **iki ayrı doğruluk kaynağı** vardı.

---

## 2. Yapı

```
ÜST ÜRÜN  (ProductGroup)
  ad          "Ön fren balatası (takım)"     ← ustanın söylediği
  kategori    "Fren"                          ← gruplama/arama
  uyum        ['renault-megane']              ← hangi araçlara ( boş = her araca )
  varyantlar  [ ... ]

    VARYANT  (ProductVariant)
      kod         "Bosch 0986424815"          ← barkod/OEM eşleşmesi buradan
      marka       "Bosch"
      sinif       Orijinal | OES | Muadil     ← boş = sınıfsız (sarf)
      birim       1850                         ← liste/alış fiyatı
      stok        4                            ← dükkânda duran adet
      tedarikci   "Ankara Bosch Bayi"
```

**Uyum üst üründedir, varyantta değil.** Bir aracın "ön fren balatası" ihtiyacı vardır; o ihtiyaca uyan markalar zaten o grubun altındadır. (İstisna — aynı grupta bazı varyantlar sadece belirli motora uyuyorsa: **A-U1**, aşağıda.)

**Stok varyanttadır, üst üründe değil.** Rafta duran şey `Bosch 0986424815`'tir, "balata" değil.

---

## 3. Nerede nasıl görünür

| Ekran | Üst ürün | Varyant |
|---|---|---|
| **Parça seçici** (servis kaydı) | Aranan ve seçilen şey. *"Ön fren balatası · 2 seçenek · en uygun 1.240 ₺"* | Grup açılınca altında listelenir; biri seçilir ya da **fiyat sonra** bırakılır |
| **Fiyat çalışması** | Fiyatı sorulan ihtiyaç | Gelen teklifler — karşılaştırma tam da varyantlar arasında |
| **Mal kabul** | Barkod okununca **hangi ihtiyaca** karşılık geldiği | Barkod doğrudan varyanta düşer |
| **Stok** | Başlık: *"Ön fren balatası — 2 marka, 6 adet"* | Adet ve iade varyant düzeyinde |
| **Teklif / fatura** | Müşteri **ihtiyacı** okur | Takılan **varyant** yazılır (garanti/iade için şart) |

**Kural:** usta **üst ürünle** konuşur, dükkân **varyantla** çalışır. Ekran hangi tarafındaysa onu öne çıkarır.

---

## 4. Fiyatın hangi katmanda olduğu

- **Varyantın `birim` fiyatı** = dükkânın bildiği liste/alış fiyatı.
- **Üst ürünün fiyatı yoktur** — ancak *"en uygun 1.240 ₺"* diye **türetilir** (stoktakiler ve fiyatı olanlar arasından).
- Ustanın müşteriye söylediği **tahmini fiyat** ne birine ne ötekine aittir; **servis kaydı kalemine** aittir. (14. iş bu ayrımı ayrıca ele alıyor.)

---

## 5. AÇIK

| Kod | Karar |
|---|---|
| **A-U1** | Varyant düzeyinde uyum gerekir mi? (aynı balata grubunda 1.5 dCi'ye uyan / uymayan varyant). Şimdilik uyum yalnız üst üründe. |
| **A-U2** | Üst ürün adları nereden gelecek? Elle mi girilecek, TecDoc gibi bir kaynaktan mı (**A-11.3** ile bağlı)? Şimdilik elle. |
| **A-U3** | Aynı üst ürünün **takım / tek parça** ayrımı (balata takımı vs tek balata) ayrı üst ürün mü, varyant özelliği mi? Şimdilik adın içinde ("(takım)"). |
| **A-U4** | Sarf malzemesi (cam suyu, yağ) için sınıf boş bırakılıyor. Ayrı bir kategori mi olmalı? |
| **A-U5** | Bir varyant **iki üst ürüne** ait olabilir mi (ör. bazı setlerde devirdaim hem "triger seti" hem "devirdaim")? Şimdilik hayır — tek ebeveyn. |
