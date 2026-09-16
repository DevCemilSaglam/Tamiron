# İZ KAYDI — uzaktan görmek, telefonda yardım edebilmek

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

## 1 · Ne yazılıyor — DEĞERLER DAHİL (K-053)

> **Cemil, 16.09:** *"Bu zaten müşteri verisi saklıyor olacağız
> mecburen… Amaç kullanıcı hangi veriyi girmiş ve ne sorun yaşamış.
> Kullanıcı problem yaşadığında arayacağı kişi olacağım ve ona neyi
> yanlış yaptığını direkt söyleyebilmeliyim."*

İlk sürümde değerleri biçimce eleyen bir kural yazmıştım (K-052.2).
**Geri alındı.** Eleğin koruduğunu sandığı şey zaten korunmuyordu —
plaka, ad, adres veritabanında duruyor. Elek yalnızca **desteği kör**
ediyordu: telefondaki ustaya *"şurada plakayı yanlış yazmışsın"*
diyebilmek için o plakayı görmek gerekiyor.

Kalan tek sınır **boy**: değer 80 harfte kırpılıyor. Sebebi gizlilik
değil ölçü — tek bir uzun not halka tamponu doldurup önceki adımları
siler ve iz yine kör kalır, bu kez fazlalıktan.

### İzin üç işi

| iş | ne cevaplar | ne gerekir |
|---|---|---|
| **Destek** | *"Ne yaptın da bozuldu?"* | son adımlar, girilen değerler |
| **Vazgeçme** | *"Neden bıraktı?"* | hangi adımdan sonra dönmedi |
| **Zorlanma** | *"Nerede takılıyor?"* | süre, geri-ileri, tekrar düzeltilen alan |

Birincisi tek kullanıcıya bakarak görülür; öteki ikisi **zaman
içinde** görünür. Saklama bu yüzden iki aşamalı (§2).

---

## 2 · Saklama — ayrıntı eskir, istatistik olgunlaşır

| aşama | süre | ne durur |
|---|---|---|
| **Ayrıntı** | cihazda 7 gün · bizde 90 gün | olay olay, değerler dahil |
| **İstatistik** | süresiz | ekran süresi, tamamlanma oranı, şüphe sayısı, vazgeçme noktası |

Ayrıntı desteğin işine yarar ve eskiyince değerini yitirir. İstatistik
tersine, **eskidikçe değer kazanır**: *"bu adımı altı ay önce %70
tamamlıyordu, şimdi %40"* ancak böyle görülür.

---

## 3 · Olay türleri

| tür | ne demek | örnek |
|---|---|---|
| `ekran` | ekran açıldı | `ekran: kayit` |
| `sure` | ekranda geçen süre | `sure: kayit, sn: 48.3` |
| `dokunus` | neye dokunuldu — cinsi **ve yazısı** | `dokunus: tus, yazi: "Teslim et"` |
| `alan` | **hangi alana ne girildi**, kaçıncı kez | `alan: plaka, deger: "34 QBC 123", kez: 2` |
| `hata` | yakalanan ya da yakalanmayan hata | `hata: ruhsat, sinif: typeerror` |
| `suphe` | mantığa aykırı ya da zorlanma işareti | `suphe: kararsiz-gezinme` |

`alan` olayı **tek bir dinleyiciden** çıkıyor (`change`, yakalama
evresinde). Ekran ekran çağrı serpiştirilseydi biri unutulurdu — ve
unutulan, tam da aranan alan olurdu.

`kez` sayacı kendi başına bir işaret: aynı alan üçüncü kez
değiştirilince `alan-tekrar-duzeltildi` şüphesi yazılıyor. *"Formu üç
kez düzeltmek"*, form yanlış soruyor demektir.

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
| `zararina-satis` | Fatura maliyeti, müşteriye söylenen fiyattan yüksek | Ya fiyat düşük yazıldı ya yanlış parça eşleşti (MALIYET-VE-KAR §5) |

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

* `betik/00-iz.js` — halka tampon (son 300 olay), 80 harflik kırpma,
  şüphe kuralları
* `goster()` tek geçiş noktasına bağlı — ekran ekran çağrı
  serpiştirilseydi biri unutulur ve iz **sessizce eksilirdi**
* girilen değerler tek `change` dinleyicisinden düşüyor
* `ac`/`ciz` hataları artık yalnız konsola değil ize de düşüyor
  *(konsolu kimse görmüyordu)*
* yakalanmamış hatalar ve sözler (`unhandledrejection`) de ize düşüyor
* **47. sağlık denetimi** üç şeyi ölçüyor ve üçü de *"iz var ama kör"*
  hata sınıfının yüzleri:
  1. girilen değer ize düşüyor mu *(boşluktan körlük)*
  2. uzun değer kırpılıyor mu *(fazlalıktan körlük — tek not tamponu
     doldurup önceki adımları siler)*
  3. halka tampon tavanı tutuyor mu

  Negatif doğrulama: değer yazımı kesildiğinde denetim *"girilen değer
  ize DÜŞMÜYOR — destek 'ne yazdın' diye sormak zorunda kalır"* deyip
  bulgu verdi; geri alınınca 0'a döndü.

---

## 6 · Cemil'in kararına açık

**A-M40 — İzin gönderilmesi.** Cihazdaki iz bize *ne zaman* gelir?
(a) yalnız usta *"sorun bildir"* derse, (b) günde bir kez özet
kendiliğinden, (c) ikisi birden. Önerim **(c)**.

**A-M42 — Çıkarım nerede görünecek?** *"İzden çıkarım yapabilmeliyiz"*
dedin. Önerim ayrı bir ekran değil, **tek sayfalık özet**: dükkân
listesi, yanında zorlanma işareti; ürüne değil bizim tarafa ait.

*A-M41 (saklama süresi) kapandı — §2.*
