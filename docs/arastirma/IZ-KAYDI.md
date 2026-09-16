# İZ KAYDI — uzaktan görmek, veriyi taşımadan

> **Cemil, 16.09:** *"Log kaydı, nereye tıklanmış, ne yapılmış adım
> adım, hatalar, başarısızlıklar. Kullanıcı bir hata aldığında, yanlış
> bir şey yaptığında uzaktan fark etmeliyiz. Mantığa aykırı işlemleri
> de görebilmeliyiz."*

---

## 0 · Neden gerekli — v1'in dersi tam burada

Tamirci *"çok fazla özellik var, karmaşık geldi"* dedi. Bunu
**sorduğumuz için** öğrendik. Sormasaydık öğrenemeyecektik, çünkü
**usta destek aramaz — sessizce bırakır.**

İz kaydının işi o sessizliği ölçüye çevirmek. Tek cümlelik gerekçesi
budur; geri kalan her şey bunun nasıl yapılacağı.

> ⚠️ **İz kaydı ustanın işi değil, bizim işimiz.** Yedi işten
> (`TAMIRCI-GERIBILDIRIM §7`) hiçbirine hizmet etmiyor. Bu yüzden
> *"yeni özellik varsayılan HAYIR"* kuralının istisnası olarak değil,
> **ürünün görünmez bir parçası** olarak giriyor: ekran yok, ayar yok,
> soru yok, ustaya maliyeti sıfır.

---

## 1 · Asıl tehlike: ölçen şeyin kendisi sızdırması

Bir iz kaydı iki yönden bozulur:

| | Ne olur | Nasıl anlaşılır |
|---|---|---|
| **Az yazar** | Uzaktan hiçbir şey görünmez | Kolay: hiç veri gelmez |
| **Çok yazar** | Plaka, ad, telefon, TC ize düşer | **Zor: hiç fark edilmez** |

İkincisi sinsi, çünkü *daha iyi çalışır* — ayrıntılı iz hata ayıklamayı
kolaylaştırır. Yanlışlığı ancak kimse bakmazken anlaşılır: her yedek,
her hata raporu, her destek gönderimi o veriyi taşır. K-050 ve K-051
ile TC/adres zaten **tutuluyor**; iz kaydı onların ikinci bir kopyasını
üretmemeli.

### Kural: **değer yazılmaz, şekil yazılır**

```
  YANLIŞ   plaka "34 ABC 123" -> "34 QBC 123" olarak düzeltildi
  DOĞRU    plaka alanı 3 kez düzeltildi
```

Ve bu bir temenni değil, **mekanik**: `Iz.guvenli()` yalnız sayı,
doğru/yanlış ve küçük harfli kısa kod geçirir. Plaka ("34 ABC 123"),
ad ("Ali Yılmaz"), telefon ("0532…") biçimce elenir — *unutmakla
değil, geçememekle.*

47. sağlık denetimi bunu iki yönden ölçüyor:
* tohum verideki **her plaka, sahip, telefon, TC, adres** izde aranır
  → bugün sızmıyor;
* eleğe **bilerek bir plaka verilir** → yarın da sızamaz.

Negatif doğrulama yapıldı: elek delindiğinde denetim
*"Iz.guvenli() plakayı geçiriyor — elek delik"* diyor.

---

## 2 · İki katman — dükkânda ayrıntılı, bizde kısır

```
   ┌── DÜKKÂNDA (cihazda) ──────────────┐   ┌── BİZDE ─────────────┐
   │ son N olay, AYRINTILI              │   │ yalnız ŞEKİL         │
   │ ustanın kendi verisi               │──▶│ ekran · dokunuş      │
   │ dükkândan çıkmaz                   │   │ hata sınıfı · süre   │
   │                                    │   │ şüphe kodu           │
   │ "sorun bildir" derse EK OLARAK     │   │ değer YOK            │
   │ gönderilir — onayıyla              │   └──────────────────────┘
   └────────────────────────────────────┘
```

Ayrımın sebebi: hata ayıklamak için bazen değere ihtiyaç var (*"hangi
plakayı yanlış duydu"*). O bilgi **dükkânda** durur ve ancak usta
*"sorun bildir"* derse, gördüğü bir özetle birlikte gelir. Varsayılan
akışta bize giden izde müşteri verisi yoktur.

---

## 3 · Ne yazılıyor

| tür | ne demek | örnek |
|---|---|---|
| `ekran` | ekran açıldı | `ekran: kayit` |
| `sure` | ekranda geçen süre | `sure: kayit, sn: 48.3` |
| `dokunus` | neye dokunuldu (**cinsi**, kimliği değil) | `dokunus: nav-ana` |
| `hata` | yakalanan ya da yakalanmayan hata | `hata: ruhsat, sinif: typeerror` |
| `suphe` | mantığa aykırı ya da zorlanma işareti | `suphe: kararsiz-gezinme` |

**Dokunuşta metin yazılmıyor.** Düğmenin metninde plaka olabilir
(*"34 ABC 123 · Ali Yılmaz"* kartı), id'sinde de olabilir. Yazılan şey
**sınıf adı** — sınıf sözlüğünden (K-008) gelir, yapıyı anlatır,
içeriği anlatmaz.

---

## 4 · "Mantığa aykırı işlemler" — liste

Cemil'in istediği asıl şey bu ve **hatadan ayrı bir şey**: program
çalışıyor, kimse hata almıyor, ama olan iş yanlış.

### 4.1 · İş mantığına aykırı (veri çelişiyor)

| kod | ne oldu | neden şüpheli |
|---|---|---|
| `teslimden-sonra-islem` | Teslim edilmiş kayda parça/işlem eklendi | Ya teslim erken işaretlendi ya yanlış kayıt açık |
| `ikinci-acik-kayit` | Aynı araca ikinci açık kayıt | Biri unutulmuş; fatura ikiye bölünür |
| `km-geri` | Yeni kayıtta kilometre öncekinden düşük | Yanlış araç ya da yanlış yazım |
| `parca-beklerken-teslim` | Parça bekleniyorken teslim edildi | Ya parça geldi ve işaretlenmedi ya araç eksik gitti |
| `fiyatsiz-teslim` | Fiyat girilmeden teslim | Para konuşulmadan araç çıkıyor |
| `ayni-musteriye-tekrar-mesaj` | 1 dakikada ikinci mesaj | Gönderdiğini görmedi — buton geri bildirimi zayıf |

### 4.2 · Zorlanma işareti (usta kayboldu)

| kod | ne oldu | neden şüpheli |
|---|---|---|
| `kararsiz-gezinme` | Aynı ekrana 20 sn içinde 3. kez | Aradığını bulamıyor |
| `tekrar-eden-hata` | Aynı yerde aynı hata 2. kez | Tesadüf değil, kalıp |
| `uzun-bekleme` | Bir ekranda alışılmadık süre | Ya takıldı ya vazgeçti |
| `yarim-birakma` | Kayıt açıldı, 10 sn içinde silindi | Yanlış açtı — akış yanıltıyor |
| `ses-ustuste-red` | Ses komutu 3 kez anlaşılmadı | Tanıma ya da sözlük yetmiyor (A-M39 ile kesişir) |
| `okuma-cogu-duzeltildi` | Ruhsat okundu, alanların çoğu elle düzeltildi | Model kötü okuyor (K-051 ile kesişir) |

**İlk ikisi prototipe girdi**, gerisi kural olarak burada duruyor:
kuralı *çağıran yerde* tanımlanır, `Iz.supheli(kod)` ile kaydedilir.

---

## 5 · Bugün prototipte ne var

* `betik/00-iz.js` — halka tampon (son 300 olay), elek, şüphe kuralları
* `goster()` tek geçiş noktasına bağlı — ekran ekran çağrı
  serpiştirilseydi biri unutulur ve iz **sessizce eksilirdi**
* `ac`/`ciz` hataları artık yalnız konsola değil ize de düşüyor
  *(konsolu kimse görmüyordu)*
* yakalanmamış hatalar ve sözler (`unhandledrejection`) de ize düşüyor
* 47. sağlık denetimi: yazıyor mu · sızdırıyor mu

---

## 6 · Cemil'in kararına açık

**A-M40 — İzin gönderilmesi.** Cihazdaki iz bize *ne zaman* gelir?
Seçenekler: (a) yalnız usta *"sorun bildir"* derse, (b) günde bir kez
kısır özet kendiliğinden, (c) ikisi birden. Önerim **(c)**: kısır özet
sessiz kalır ve zorlanmayı gösterir; ayrıntı yalnız istendiğinde gider.

**A-M41 — Saklama süresi.** İz ne kadar durur? Önerim: cihazda **son 7
gün**, bizde **90 gün**. A-M36'daki KVKK aydınlatmasıyla aynı cümlede
söylenmeli.
