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

---

# İSTİSNA KOLLARI

> **Kural:** Bir kol ana yola ancak **var olan bir adımın içinden** bağlanabilir. Yeni adım ekleyemez, yeni ekran açmak zorunda değildir. Ana yol yedi adım olarak kalır.
>
> Tamircinin *"karmaşık geldi"* dediği şey buydu: v1'de istisnalar ana yola **adım olarak** girmişti.

---

## Kol A · Teşhis — müşteri ne istediğini bilmiyor (%20)

**Nereden bağlanıyor:** Adım 2 · *"Ne istiyor"*
**Nerede bitiyor:** Adım 2 — aynı yerde, aynı satırla

| | |
|---|---|
| Ana yolda | Müşteri söyler → usta yazar |
| Bu kolda | Müşteri anlatır → **usta bakar** → usta yazar |

**Ekran açmıyor.** Adım 2'nin alanı serbest metin: ana yolda müşterinin dediği yazılıyor, bu kolda ustanın gördüğü. Çıktı **aynı**: kayda bir satır.

> **Şikâyet ve tespit ayrı alanlar DEĞİL.** v1'de iki ayrı akordiyon vardı (*"müşteri ne diyor"* / *"usta ilk izlenimi"*). Tamirci böyle bir ayrım istemedi. Tek alan; usta ne yazacağını bilir.

**Farkı:** Araç lifte kalkar, zaman geçer. Bu **süre farkıdır, adım farkı değildir** — sistemde karşılığı yok.

---

## Kol B · Büyük iş — birden fazla firmadan fiyat (T-11)

**Nereden bağlanıyor:** Adım 3 · *"Parçacıya sor"*
**Nerede bitiyor:** Adım 4 · *"Fiyat geldi"*

Tamirci: *"Büyük bir iş ise genelde parçalar farklı firmalardan araştırılabiliyor."* → **istisna**, ana yol değil.

| | |
|---|---|
| Ana yolda | Bir parçacı seçilir |
| Bu kolda | **Birden fazla** parçacı seçilir |
| Adım 4'te | Cevaplar **yan yana** düşer, usta birini seçer |

**Ekran açmıyor** — aynı ekranda çoklu seçim. Cevaplar aynı satırın altında biriktiği için ayrı bir "karşılaştırma ekranı" gerekmiyor.

> v1'de bu bir **ekran ailesiydi** (fiyat karşılaştırma tablosu, en uygun seçili, teklif detayı). Tamirci bunu ana iş sanmamızı düzeltti.

---

## Kol C · Ek iş — bu bir kol bile değil (T-10)

**Nereden bağlanıyor:** Adım 6 · *"İş yapıldı"*
**Nerede bitiyor:** Aynı yerde

> *"Trigger değiştirilirken, örnek olarak balata da eskidi ise balata değişim teklifi sunuyor tamirci müşteriye. **Fakat bunlar akış olarak değil.** Yani serviste güncelleme yapılabiliyor."*

**Ana yolun zaten var olan davranışı:** kayıt teslim edilene kadar açıktır, satır eklenir (K-003). Ek iş için özel bir şey yapılmıyor — **yapılmaması** gerekiyor.

Parça gerekiyorsa Adım 3–4'e geri dönülür. Bu bir geri dönüş, yeni bir yol değil.

---

## Kol D · Parçayı müşteri getirdi (T-7, T-8)

**Nereden bağlanıyor:** Adım 5 · *"Parça geldi — kim getirdi?"*
**Nerede bitiyor:** Adım 6

İki giriş noktası var:

1. **Baştan belli** — müşteri *"parçayı ben alırım"* der → Adım 3–4 hiç olmaz, doğrudan Adım 5
2. **Sonradan** — fiyat söylenir, müşteri *"ucuza buldum, kendim alayım"* der → Adım 4'ten Adım 5'e geçilir, gelen fiyat **kayıtta kalır** (ne kadara alabileceği bilgisi değerli)

| | |
|---|---|
| Satır | Fiyatsız ya da 0 ₺ (S2) |
| `kimGetirdi` | `musteri` (K-005) |
| Sonucu | Sorun çıkarsa **tekrar işçilik alınır** — araç ikinci kez geldiğinde bu bilgi karşıya çıkar |

**Ekran açmıyor** — Adım 5'teki iki düğmeden biri.

---

## Kol E · Onay isteme (K-003)

**Nereden bağlanıyor:** Kayıt açıkken **her an**
**Nerede bitiyor:** Hiçbir yerde — **akış onayı beklemez**

> *"Müşteri onay vermek zorunda değil. Tamirciye sözlü olarak söylemiş olabilir. Onay iste diye ayrıca bir alan olabilir."*

Tek düğme: **"Onay iste"** → müşteriye özet gider. Cevap gelirse kayda düşer, gelmezse hiçbir şey olmaz. **Kayıt beklemede kalmaz, durum değişmez** (K-004: tek durum var, o da teslim).

> Bu, kolların en önemlisi — çünkü **kol olmayan** bir şey. v1'de onay bir akıştı ve ekranların dörtte birini tutuyordu.

---

## Kolların ana yola maliyeti

| Kol | Yeni ekran | Yeni adım | Ana yola eklediği |
|---|---|---|---|
| A · Teşhis | — | — | Yok (aynı alan) |
| B · Büyük iş | — | — | Adım 3'te çoklu seçim |
| C · Ek iş | — | — | Yok (zaten var) |
| D · Müşteri parçası | — | — | Adım 5'te ikinci düğme |
| E · Onay isteme | — | — | Bir düğme |

**Beş kolun toplam maliyeti: sıfır yeni ekran, sıfır yeni adım.**

Ana yol yedi adım, 10–13 dokunuş olarak duruyor.

---

## Kollarla ilgili AÇIK

| Kod | Karar |
|---|---|
| **A-11** | **Müşteri vazgeçerse ne olur?** Tamirci bundan hiç bahsetmedi, K-003 red panelini kaldırdı. Kayıt yalnız işçilikle mi teslim edilir, yoksa "yapılmadı" diye mi kapanır? Öneri: **teslim edilir** — araç çıkıyorsa iş bitmiştir, satırlar ne ise odur |
| **A-12** | Kol B'de **cevaplar ne kadar bekletilir?** Bir parçacı hiç dönmezse kayıt takılı mı kalır? Öneri: takılmaz — usta gelen cevapla ilerler |
| **A-13** | Kol D'deki *"sonradan"* hâlinde gelen fiyat kayıtta kalıyor. Bu **rakam müşteriye görünmeli mi**? (Ustanın alabileceği fiyat) Öneri: hayır |

---

# TAMİRAT / BAKIM — ayrım ve hatırlatma

> Dayanak: **K-006** (iki ayrı tür) · **K-002** (km kaydedilir, hatırlatma ay üzerinden)
> Tamirci: *"Tamirat ve bakım iki ayrı durum var. Bakımda km önemli. Tekrar bakım geldiğinde bilgi sms olarak göndermeliyiz."* (T-15, T-16, T-17)

---

## 1 · Ayrım nerede tutuluyor

Servis kaydında **tek alan**: `tur` = `tamirat` | `bakim`.

Adım 1'de, kayıt açılırken **iki büyük düğmeyle** seçilir. Ayrı kayıt türü, ayrı tablo, alt kırılım **yok**.

---

## 2 · Ne farkediyor

| | Tamirat | Bakım |
|---|---|---|
| **km alanı** | Yok | **Var ve istenir** |
| **Hatırlatma** | Kurulmaz | **Teslimde kurulur** |
| **Teslim ekranında** | Özet gider | Özet + *"6 ay sonra hatırlatılacak"* |
| **Araç kartında** | Geçmiş satırı | Geçmiş satırı + **son bakım km'si** |

**Farkın tamamı bu.** Ekran değişmiyor, akış değişmiyor — bir alan çıkıyor, bir hatırlatma kuruluyor.

---

## 3 · km — neden var, ne işe yarıyor

Km **bir kez yazılır** ve **hiç güncellenmez**. Araç dükkânda değilken kimse bize km göndermiyor; takip edilebilir bir şey değil (K-002).

Ama yazılınca üç yerde işe yarıyor:

1. **Araç kartında geçmiş:** *"Geçen bakım 15.03.2026'da 142.000 km'de yapılmış."* Usta bir sonraki gelişte buna bakar.
2. **Hatırlatma mesajında:** *"Son bakımınız 142.000 km'de yapılmıştı."* — mesaja **inandırıcılık** katar, maliyeti sıfırdır.
3. **İki bakım arasındaki km farkı:** aracın ne kadar kullanıldığını söyler. Usta *"bu araç çok gidiyor, 4 ayda çağıralım"* diyebilir.

> Üçü de **yazılmış bir sayının** ürünü. Hiçbiri km *takibi* gerektirmiyor.

---

## 4 · Hatırlatma — ne zaman, nasıl

### Kurulma

**Teslim anında** (Adım 7), kayıt `bakim` ise:

```
hatirlatmaTarihi  =  teslim tarihi + periyot
hatirlatmaGonderildi = false
```

**Ayrı bir "hatırlatma" varlığı yok** — servis kaydında iki alan. En az veri kuralı (7. iş).

### Periyot nereden geliyor

| Kaynak | Ne zaman |
|---|---|
| Dükkân ayarı — **varsayılan 6 ay** | Her bakımda otomatik |
| Kayıtta elle değiştirilebilir | Usta *"bu araç çok gidiyor, 4 ay"* derse |

**Cemil onayına açık (A-3):** varsayılan 6 ay mı?

### Tetiklenme

Günde bir kez çalışan bir tarama:

```
bakım kayıtları
  · hatirlatmaTarihi <= bugün
  · hatirlatmaGonderildi = false
  · aracın telefonu var
  · araçta DAHA YENİ bir bakım kaydı yok
        ↓
   SMS gönder → hatirlatmaGonderildi = true
```

**Mesai saatinde gönderilir.** Sabahın üçünde SMS atmak bu kullanıcı kitlesinde iyi karşılanmaz.

### ⚠️ Eskiyen hatırlatma — kolay kaçırılan kural

Araç **hatırlatma tarihinden önce** bakıma gelirse eski hatırlatma **anlamsızlaşır.** Gönderilirse müşteri *"daha geçen ay geldim"* der.

> **Kural:** Aynı araca yeni bir **bakım** kaydı açılınca, o aracın **gönderilmemiş bakım hatırlatmaları düşer.** Tarama zaten *"daha yeni bakım kaydı yok"* koşuluyla bunu sağlıyor — ama kural açıkça yazılı olmalı, yoksa ilk düzenlemede kaybolur.

### Gönderilmeme hâlleri

Hatırlatma **sessizce atlanır**, hata üretmez:

| Hâl | Ne olur |
|---|---|
| Telefon yok | Atlanır |
| Araç yeniden bakıma gelmiş | Düşer |
| İYS izni yok | **Gönderilemez** — yasal kısıt (10. iş) |

---

## 5 · Mesajın içeriği

Kısa, sade, tek iş. Tamircinin müşterisi teknolojiden uzak olabilir (T-18) — mesaj da öyle olmalı.

> **Ahmet Usta Oto Servis**
> 34 ABC 123 plakalı aracınızın bakım zamanı geldi.
> Son bakım: 15.03.2026 · 142.000 km
> Randevu için: 0532 000 00 00
> Ret için B İPTAL yaz 4045'e gönder

Ne yok: link tıklama zorunluluğu, kampanya, indirim kuponu, uzun metin.

> ⚠️ **Son satır yasal zorunluluk, süs değil.** 6563 sayılı Kanun m.8: alıcı
> her zaman ve gerekçesiz reddedebilmeli, ret **kolay ve ücretsiz** olmalı,
> **3 iş günü** içinde durdurulmalı. Bakım hatırlatması için *onay* muafiyeti
> var (`arastirma/SMS-VE-IYS.md` §0) ama **ret hakkı muafiyet dışı.**
> Bu satır 10. iş araştırmasında eklendi — ilk taslakta yoktu.

---

## 6 · AÇIK

| Kod | Karar |
|---|---|
| **A-3** | Bakım periyodu varsayılanı **6 ay** mı? |
| **A-17** | Aynı müşterinin **birden fazla aracı** varsa ayrı ayrı mı SMS gider? Öneri: **evet, ayrı** — plaka mesajın konusudur |
| **A-18** | Müşteri hatırlatmaya **cevap yazarsa** ne olur? SMS tek yönlü; öneri: **kapsam dışı**, telefonla arar |
| **A-19** | Hatırlatma **kaç gün önceden** gitsin — tam gününde mi, birkaç gün önce mi? Öneri: **tam gününde**, sadeliği bozmasın |
