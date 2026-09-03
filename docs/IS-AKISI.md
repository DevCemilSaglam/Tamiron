# TAMIRON — İŞ AKIŞI

> **v1 · 03.09.2026** · Kaynak: `arastirma/TAMIRCI-GERIBILDIRIM.md` · Kararlar: K-001…K-006
> **Yaşayan belge.** Ekran değişince burası da değişir; ekran akıştan türer, tersi değil.

---

## 0 · Ana yol nedir, ne değildir

Tamirci: **%80'i şu.** Müşteri **ne istediğini bilerek** geliyor (T-2).

> *"Araç geliyor, örnek olarak trigger kayışının değişmesini istiyor."*

Bu bir **sipariş**tir, teşhis değil. v1'in omurgası teşhis varsayıyordu ve %20'lik hâli %100'ün yoluna gömmüştü — tamircinin *"karmaşık geldi"* dediği şey büyük ölçüde buydu.

**Teşhis, büyük iş araştırması, ek iş, müşteri parçası — hepsi ana yola bağlanan kollardır** (4. iş). Ana yolu kalınlaştırmazlar.

---

## 1 · ANA YOL — yedi adım

Her adımda: **kim** yapıyor · **hangi veri** giriyor · **hangi karar** veriliyor.

### Adım 1 · Ruhsatı çek, kayıt açılsın

| | |
|---|---|
| **Kim** | Usta |
| **Ne yapıyor** | Telefonu ruhsata tutar |
| **Sistem** | Plaka · marka · model · yıl · motor · şasi okur. Araç kayıtlıysa **açar**, değilse **oluşturur** |
| **Karar** | **Tamirat mı, bakım mı?** (K-006) — iki büyük düğme |
| **Dokunuş** | 2 |

Tamircinin cümlesi: *"Ruhsatı çekti, kayıt açıldı."* (T-3) **Giriş noktası bu** — plaka yazmak değil.

> **Kaçış yolu her zaman açık:** ruhsat yoksa, okunmuyorsa, araç zaten kayıtlıysa → **plakayı yaz.** Okuma bir kolaylıktır, ön koşul değildir.

**Bakım seçildiyse:** km alanı çıkar (K-002). Usta göstergeden okur, bir kez yazar.

---

### Adım 2 · Ne istiyor

| | |
|---|---|
| **Kim** | Usta |
| **Ne yapıyor** | *"Triger seti"* — **söyler** ya da yazar (T-4) |
| **Sistem** | Kayda satır olarak düşer |
| **Karar** | Yok |
| **Dokunuş** | 1–2 |

Katalogdan seçtirmeye **çalışmıyoruz.** Usta ne diyorsa o yazılır. Katalog eşleşmesi varsa arkada olur (K-001, `PARCA-TANIMLAMA.md`) — ustaya sormaz, akışı durdurmaz.

---

### Adım 3 · Parçacıya fiyat sor

| | |
|---|---|
| **Kim** | Usta → parçacı |
| **Ne yapıyor** | Parçacıyı seçer, gönderir |
| **Sistem** | WhatsApp mesajı hazırlar: **ruhsat fotoğrafı + istenen parçalar** (T-12) |
| **Karar** | Hangi parçacı(lar)a soruluyor |
| **Dokunuş** | 2 |

> **⚠️ BU ADIM ATLANABİLİR.** Usta fiyatı biliyorsa doğrudan yazar ve 5. adıma geçer. Ana yolun zorunlu parçası değildir — *sorulduğunda* buradan geçilir.

Parçacı **kayıtlı bir firmadır** (ad + telefon, T-12b). Birden fazla parçacıya aynı anda sorulabilir — ama bu **büyük iş kolu**dur (T-11), varsayılan tek parçacıdır.

---

### Adım 4 · Fiyat geldi

| | |
|---|---|
| **Kim** | Parçacı (dışarıdan) |
| **Sistem** | Cevap kayda düşer, ham metin **saklanır**, ayrıştırılmaya çalışılır |
| **Usta** | Fiyatı görür, **üstüne ekleme yapar** (T-9) |
| **Karar** | Satış fiyatı ne olacak |
| **Dokunuş** | 1 |

Ayrıştırma başarısızsa satır *"tanımlanamadı"* kalır — **akış durmaz** (K-001).

---

### Adım 5 · Parça geldi

| | |
|---|---|
| **Kim** | Usta |
| **Karar** | **Parçayı kim getirdi?** tamirci / müşteri (K-005) |
| **Dokunuş** | 1 |

Bu tek dokunuş, ileride kimin ödeyeceğini belirliyor (T-8). **Müşteri getirdiyse** satır fiyatsız kalır (S2).

---

### Adım 6 · İş yapıldı

| | |
|---|---|
| **Kim** | Usta |
| **Ne yapıyor** | Yapılanı **söyler** (T-4) |
| **Sistem** | İşçilik satırı olarak düşer |
| **Karar** | Yok |
| **Dokunuş** | 1–2 |

> **Kayıt teslim edilene kadar AÇIKTIR ve satır eklenebilir** (K-003). Balata da eskimişse buraya eklenir — ayrı bir teklif/onay akışı yoktur. Cemil: *"Yapılan işe ek iş yapılabilir teslim etmeden önce. Bizi ilgilendiren durum bu."*

---

### Adım 7 · Teslim ve ödeme

| | |
|---|---|
| **Kim** | Usta |
| **Ne yapıyor** | Toplamı görür, ödeme yolunu işaretler, teslim eder |
| **Sistem** | Durum **teslim edildi** (K-004) · bakımsa **hatırlatma kurulur** (K-002) · müşteriye **özet gider** (T-19/T-20) |
| **Karar** | Ödeme nasıl alındı |
| **Dokunuş** | 2–3 |

Tamircinin dört isteğinden dördüncüsü buydu: *"teslim ettiğinde ödemeyi nasıl aldığını bilmek."*

---

## 2 · Sayım

| Yol | Dokunuş |
|---|---|
| **Tam yol** (parçacıya sorularak) | **10–13** |
| **Kısa yol** (usta fiyatı biliyor, 3–4. adım atlanıyor) | **7–9** |

**Kıyas:** v1'de aynı işin *yalnız fiyat çalışmasına kadarki* kısmı 11 dokunuştu; teslim ve ödeme onun üstüneydi. Burada **baştan sona** 10–13.

> Bu sayı **ölçülmedi, tasarımdan çıkarıldı.** 14. işte prototipte gerçekten işletilip doğrulanacak — v1'de senaryoyu işletmek üç kırık ortaya çıkarmıştı.

---

## 3 · Akışın şekli

```
                 ┌─────────────────────────────────────┐
                 │  1. RUHSATI ÇEK  →  kayıt açıldı    │
                 │     tamirat mı, bakım mı?           │
                 └──────────────┬──────────────────────┘
                                │
                 ┌──────────────▼──────────────────────┐
                 │  2. NE İSTİYOR   "triger seti"      │
                 └──────────────┬──────────────────────┘
                                │
              fiyatı biliyor?   │
        ┌───────── evet ────────┴───── hayır ──────────┐
        │                                              │
        │                          ┌───────────────────▼──────────┐
        │                          │  3. PARÇACIYA SOR            │
        │                          │     ruhsat + parça listesi   │
        │                          └───────────────────┬──────────┘
        │                                              │
        │                          ┌───────────────────▼──────────┐
        │                          │  4. FİYAT GELDİ  → üstüne ek │
        │                          └───────────────────┬──────────┘
        │                                              │
        └──────────────────────┬───────────────────────┘
                               │
                 ┌─────────────▼───────────────────────┐
                 │  5. PARÇA GELDİ                      │
                 │     kim getirdi? tamirci / müşteri   │
                 └─────────────┬───────────────────────┘
                               │
                 ┌─────────────▼───────────────────────┐
                 │  6. İŞ YAPILDI   (sesle söylenir)    │◄── ek iş
                 │     kayıt teslime kadar AÇIK         │    buraya
                 └─────────────┬───────────────────────┘
                               │
                 ┌─────────────▼───────────────────────┐
                 │  7. TESLİM + ÖDEME                   │
                 │     → özet gider · bakımsa hatırlat  │
                 └─────────────────────────────────────┘
```

**Tek dallanma var** ve o da bir adımı **atlamak** için (3–4). Onay kolu yok, durum makinesi yok, teşhis dalı ana yolda yok.

---

## 4 · Yedi işin akıştaki yeri

Ürünün tanımı yedi işti (geri bildirim §7). Hepsinin akışta bir karşılığı var — **karşılığı olmayan özellik yoktur:**

| # | İş | Adım |
|---|---|---|
| 1 | Aracın bilgilerini girmek | 1 |
| 2 | Yapılan işlemleri belirtmek | 2, 6 |
| 3 | Fiyatı belirtmek | 4 |
| 4 | Teslimde ödemeyi bilmek | 7 |
| 5 | Parçacıya fiyat sormak | 3 |
| 6 | Müşteriye özet göndermek | 7 |
| 7 | Bakım hatırlatması | 1 (km) → 7 (kurulur) |

---

## 5 · AÇIK

| Kod | Karar |
|---|---|
| **A-8** | **Tamirat/bakım seçimi 1. adımda mı sorulsun?** Bir dokunuş ekliyor ama hangi alanların çıkacağını belirliyor (km, hatırlatma). Alternatif: varsayılan tamirat, bakımsa değiştir. Öneri: **açıkça sorulsun** — belirsizlik bırakmaktan iyidir |
| **A-9** | 3. adım atlandığında usta fiyatı **nereye** yazıyor — satırın üstüne mi, ayrı alana mı? |
| **A-10** | Parçacı cevabı geldiğinde usta **nasıl haberdar oluyor** — bildirim mi, listede rozet mi? |
| **A-4** | *"Parça bekliyor"* kayıtlar listede türetilerek gösterilsin mi (K-004 notu) — **bu akışta 3–4. adımda takılı kayıtlar demek** |
