# Dışa aktarma — PDF ve Excel, backend olmadan

> **Durum:** araştırma notu · 08.09.2026 · kuyruk 4/D3
> **Kaynak (Cemil, 07.09):** *"Giderler listele sayfasında ay bazında
> gider toplamları listelenebilir olmalı… Bunu pdf'e, excele vb.
> aktarabilelim."*
> **Akıştaki yeri:** muhasebe kümesi (B2) — **K-022'nin kapsam onayına
> bağlı**
>
> ⚠️ Rakamlar **Eylül 2026** ölçümüdür.

---

## 0 · Ana bulgu — kütüphane gerekmiyor, ikisi de tarayıcıda var

İlk refleks *"jsPDF + SheetJS ekleyelim"*. Ölçünce iki şey çıktı:

1. **PDF için tarayıcının kendi yazdırma özelliği yeterli** — ve
   Türkçe sorununu kendiliğinden çözüyor.
2. **Excel için `.xlsx` gerekmiyor; CSV yetiyor** — ama Türkiye'de iki
   tuzağı var ve ikisi de bilinmezse dosya bozuk açılıyor.

Kütüphane yolu **çalışır ama pahalıdır** (§3). Önce ucuz yolun neden
yettiğine bakalım.

---

## 1 · PDF — `window.print()`

Tarayıcının yazdırma penceresinde **"PDF olarak kaydet"** her yerde
var: Android Chrome, iOS Safari, masaüstü. Yaptığımız iş bir **yazdırma
stili** (`@media print`) yazmak:

```css
@media print {
  .topbar, .botbar, .drawer, .katman, .toast { display: none; }
  .screen { overflow: visible; }
}
```

| | Kazanç |
|---|---|
| **Türkçe karakter** | Tarayıcının kendi metin motoru — `ğışçöü` sorunu **yok** |
| **Boyut** | **0 KB** ek |
| **Bakım** | Tablo değişince PDF de değişir; ayrı bir çizim kodu yok |
| **Sayfalama** | Tarayıcı yapıyor (`page-break-inside: avoid` ile yönlendirilir) |

**Bedeli:** çıktının görünümü üzerinde tam denetim yok (üstbilgi/altbilgi
tarayıcıdan gelir, kullanıcı ayarlayabilir), ve dosya adı otomatik
gelmez.

> Bu bedel, tamircinin işine göre **küçük**: ona lazım olan şey
> *"muhasebeciye göndereceğim bir döküm"*, matbaa kalitesinde bir
> belge değil.

---

## 2 · Excel — CSV, ama Türkiye ayarıyla

CSV üretmek birkaç satır. **İki tuzağı var ve ikisi de sessiz:**

### Tuzak 1 — kodlama (BOM)

CSV dosyası kendi kodlamasını **söylemez**; okuyan uygulama tahmin
eder ve Excel (Windows) çift tıklamayla açıldığında sıklıkla eski ANSI
varsayar. Sonuç: `Dükkân kirası` → `DÃ¼kkÃ¢n kirasÄ±`.

**Çözüm:** dosyanın başına **UTF-8 BOM** (`﻿`) konur. Excel o
zaman UTF-8 olduğunu anlıyor.

### Tuzak 2 — ayraç

Excel virgül kullanmaz; **kullanıcının bölgesel ayarındaki liste
ayırıcısını** kullanır. Virgülün **ondalık ayırıcı** olduğu ülkelerde
— **Türkiye dâhil** — bu **noktalı virgüldür**.

Virgülle yazılan bir CSV, Türkçe Excel'de **tek sütuna** düşer.

**Çözüm:** ayraç olarak `;` kullanılır.

### Üçüncüsü, bizim kendi tuzağımız

Tutarları `binlikAyir()` ile **`1.900`** diye yazıyoruz. Bu metin
Excel'de Türkçe ayarla **1900** okunur (nokta = binlik) — tesadüfen
doğru. Ama `12.970` → doğru, `1.900,50` → sorun. **Dışa aktarmada
rakam HAM yazılmalı** (`1900`), biçimlendirme ekranın işi.

> Bu, K-037'nin kuralının aynısı: aynı veri, iki okuyucuya iki
> gösterim. Ekran ustaya, CSV Excel'e yazıyor.

### Sonuç

```
BOM + ";" ayraç + ham rakam  →  Türkçe Excel'de doğru açılır
```

---

## 3 · Kütüphane yolu — ne kadara mal olur

Bugünkü paket **848 KB** (`app/`):

| Dosya | Boyut |
|---|---|
| `index.html` | 392 KB |
| `fonts.css` | 411 KB (yazı tipleri gömülü) |
| ötekiler | ~45 KB |

**jsPDF'in Türkçe sorunu var:** varsayılan yazı tipleri UTF-8
desteklemiyor. Türkçe çıktı için **TTF gömmek** gerekiyor — font
base64'e çevrilip sanal dosya sistemine (VFS) ekleniyor. Alt küme
çıkarılsa bile bu **yüz KB'lar** demek, üstüne kütüphanenin kendisi.

`.xlsx` için SheetJS de ayrı bir yük.

**Karşılaştırma:**

| Yol | Ek boyut | Türkçe | Denetim |
|---|---|---|---|
| `window.print()` | **0 KB** | sorunsuz | düşük |
| CSV (BOM + `;`) | **0 KB** | sorunsuz | tam |
| jsPDF + gömülü font | **yüz KB'lar** | font gömülürse | tam |
| SheetJS `.xlsx` | önemli | sorunsuz | tam |

⚠️ **Paket çevrimdışı ve düz yapıda** (`pwa-build.js`): CDN yok, her
şey gömülü. Kütüphane eklemek doğrudan indirme boyutuna biniyor —
tamirhanede telefon bağlantısı iyi olmayabilir.

---

## 4 · İndirme çalışır mı

Tarayıcıda dosya indirme `Blob` + `<a download>` ile yapılıyor.

* **Android Chrome:** çalışıyor.
* **Masaüstü:** çalışıyor.
* **iOS Safari:** çalışıyor ama davranışı farklı (dosya "İndirilenler"e
  gidiyor, kullanıcı Dosyalar uygulamasından buluyor). **Prototipte
  denenmedi — doğrulanmadı.**
* **Ana ekrana eklenmiş PWA (standalone):** iOS'ta indirme
  davranışının tam olarak nasıl olduğu **doğrulanmadı**; bu, gerçek
  cihazda denenmesi gereken tek nokta.

> `window.print()` bu riski hiç almıyor — yazdırma her yerde aynı.
> İkinci bir gerekçe.

---

## 5 · Yedi işe hizmet ediyor mu?

Doğrudan hayır — yedi işin hiçbiri *"dışa aktar"* demiyor. **Ama
gerçek bir bitişiği var:** tamircinin muhasebecisi. Dükkânın kayıtları
sonunda birine gidiyor ve o kişi Excel istiyor.

Bu, K-022'nin *"kasa + gider"* kapsamının doğal ucu: **girilen veri
çıkabilmeli.** Veriyi içeri alıp dışarı vermeyen bir program, ustayı
kendi verisine kilitler.

---

## 6 · Öneri

**İkisi de yazılsın — ama kütüphanesiz, ve B kümesinden sonra.**

1. **PDF = yazdırma.** `@media print` stili + *"Yazdır / PDF olarak
   kaydet"* düğmesi. Sıfır bağımlılık, Türkçe sorunsuz, her cihazda
   aynı.
2. **Excel = CSV.** `﻿` BOM + `;` ayraç + **ham rakam**. Sıfır
   bağımlılık.
3. **`.xlsx` ve jsPDF şimdilik yazılmasın.** Gerekçe boyut ve Türkçe
   font yükü. Tamirci *"biçimli bir rapor"* isterse o zaman konuşulur —
   ve o an muhtemelen backend de olacaktır.
4. **Dışa aktarılan rakam biçimlenmez.** Ekranın `1.900`'ü CSV'de
   `1900` olur. Aksi hâlde Excel'de toplam alınamaz — dışa aktarmanın
   tek amacı da odur.
5. **iOS'ta indirme gerçek cihazda denensin.** Tek doğrulanmamış nokta;
   yazdırma yolu bu riski taşımıyor.
6. **Sıra:** B2 (ay bazında gider toplamı) yazılmadan dışa aktarılacak
   bir tablo yok. **K-022 onayı bekliyor.**

> **Cemil onayına açık (A-M22):** PDF için ayrı bir kütüphane yerine
> tarayıcının yazdırma penceresi öneriliyor. *"PDF olarak kaydet"*
> ustaya yeterse bu yol 0 KB; yetmezse yüz KB'lık bir font yükü var.

---

## Kaynaklar

- [Microsoft Support · Opening CSV UTF-8 files correctly in Excel](https://support.microsoft.com/en-us/office/opening-csv-utf-8-files-correctly-in-excel-8a935af5-3416-4edd-ba7e-3dfd2bc4a032)
- [ChangeThisFile · CSV encoding and delimiters — Excel uses the locale list separator](https://changethisfile.com/blog/csv-encoding-delimiters)
- [toolhq.io · CSV UTF-8 in Excel: fixing the encoding and BOM problem](https://www.toolhq.io/blog/csv-encoding-and-the-excel-utf8-bom)
- [Medium · jsPDF UTF-8 support (font embedding)](https://medium.com/@berkayyyulguel/jspdf-utf-8-support-b7df7a76e593)
- [jsPDF · Unicode / UTF-8 örneği](https://codepen.io/kuznetsovvn/pen/jOyWMgq)
- [parallax/jsPDF · issue 2968 — html'de unicode sorunu](https://github.com/parallax/jsPDF/issues/2968)

**Doğrulanamayanlar:** iOS Safari'de ve ana ekrana eklenmiş PWA'da
`<a download>` davranışı · Türkçe alt küme font dosyasının gerçek
boyutu · Excel'in Türkçe kurulumda `;` dışında bir ayırıcı beklediği
durumlar.
