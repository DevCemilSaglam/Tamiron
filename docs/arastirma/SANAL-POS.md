# Sanal POS — kartla tahsilat alabilir miyiz?

> **Durum:** araştırma notu · 08.09.2026 · kuyruk 4/D1
> **Kaynak (Cemil, 07.09):** *"Sanal pos altyapısı sağlayabilir miyiz?
> Şartları nasıl olur, faydası olur mu? Nasıl bir işlem olabilir?"*
> **Akıştaki yeri:** Adım 7 (teslim + ödeme) · muhasebe kümesi (B7, B8)
>
> ⚠️ Bu bir **hukuki bilgi notu değildir.** Mevzuat okunarak
> çıkarılmıştır; sözleşme öncesi hukukçu teyidi alınmalıdır.
> ⚠️ Rakamlar **Eylül 2026** taramasıdır. **Fiyatlar eskir.**

---

## 0 · Ana bulgu — soru "yapabilir miyiz" değil, **"parayı kim tutuyor"**

Sanal POS iki bambaşka iş modeli demek ve ikisi arasındaki fark
teknik değil, **cezai**:

| | Model A — parayı BİZ topluyoruz | Model B — parayı DÜKKÂN topluyor |
|---|---|---|
| Para akışı | Müşteri → **Tamiron hesabı** → dükkân | Müşteri → **dükkânın kendi POS'u** |
| Hukuki sıfat | **Ödeme kuruluşu** | **Teknik hizmet sağlayıcı** |
| Lisans | **TCMB izni ZORUNLU** | **Gerekmiyor** |
| İzinsiz yapılırsa | **1–3 yıl hapis + 5.000 güne kadar adli para cezası** (6493 m.28) | — |

> **Model A bu projenin kapsamı dışında.** Ödeme kuruluşu lisansı
> sermaye, iç denetim, bağımsız denetim ve TCMB gözetimi demektir;
> tamirhane yazılımı yazan bir ekibin yan işi değildir.

**Model B yapılabilir ve yasaldır** — ama önce *"işe yarıyor mu"*
sorusunun cevaplanması gerekiyor (§4).

---

## 1 · Yasal çerçeve

| | |
|---|---|
| **Kanun** | 6493 · Ödeme ve Menkul Kıymet Mutabakat Sistemleri, Ödeme Hizmetleri ve Elektronik Para Kuruluşları Hakkında Kanun |
| **Düzenleyici** | **TCMB** (2020'de BDDK'dan devralındı) |
| **Ödeme hizmetleri** | m.12/1 — ödeme hesabı işletimi, para transferi, **ödeme aracının ihracı veya kabulü**, para havalesi, fatura ödemelerine aracılık |
| **Kapsam DIŞI haller** | m.12/2 |
| **İzinsiz faaliyet** | **m.28 — 1 yıldan 3 yıla kadar hapis ve 5.000 güne kadar adli para cezası** |

### Bizi kurtaran bent — m.12/2 (ğ)

> *"Teknik hizmet sağlayanların sunduğu, ödeme hizmetlerinde provizyon
> işlemlerinin yapılmasını destekleyen, **verinin işlenmesi,
> saklanması, güvenliğinin sağlanması, gizliliğinin korunması ve
> doğrulanması ile bilgi teknolojisi, iletişim ağı ve ödeme hizmetleri
> için kullanılan araçların tedarik ve bakımını** kapsayan
> faaliyetler."*

**Ayırt edici ölçü tek cümle:** teknik hizmet sağlayıcı, işlemin
**hiçbir anında aktarılan fonun sahibi olmaz.** Para bir an bile bizim
hesabımıza uğrarsa istisna düşer.

⚠️ **Bu, "muhasebe" değil "kasa" ayrımından da keskindir.** Muhasebede
yanlış rakam göstermek ustayı yanıltır; burada yanlış model **suçtur**.

---

## 2 · Model B nasıl işler

```
Dükkân  →  kendi adına sanal POS / ödeme kuruluşu hesabı açar
Tamiron →  o hesabın ödeme LİNKİNİ üretir ve müşteriye gönderir
Müşteri →  linke dokunur, kartıyla öder
Para    →  doğrudan DÜKKÂNIN hesabına geçer
Tamiron →  "ödendi" bilgisini okur, kaydı işaretler
```

Tamiron parayı görmez, tutmaz, aktarmaz. Yaptığı iş **link üretmek ve
sonucu okumak** — m.12/2/(ğ)'nin tarif ettiği şey.

### Dükkânın karşılaması gereken şartlar

| Şart | Not |
|---|---|
| Şirket türü | Şahıs şirketi **yeterli** (limited/anonim de olur) |
| Belge | Vergi levhası · ticaret sicil kaydı |
| Web sitesi | Banka sanal POS'unda **isteniyor**; ödeme kuruluşlarının link ürünlerinde **istenmiyor** |
| Onay | Banka/kuruluş sektöre ve ciroya göre değerlendiriyor |

> **Web sitesi şartı, banka POS'unu tamirhane için pratikte eliyor.**
> Tamircinin web sitesi yok ve olmayacak. Kalan yol: **ödeme
> kuruluşlarının link ürünü** (iyzico Link, PayTR link vb.), ki bunlar
> web sitesi istemiyor.

### Maliyet — Eylül 2026 taraması

| | Oran |
|---|---|
| Genel aralık (banka + kuruluş) | **%1,5 – %4** |
| PayTR | *"%1,99'dan başlıyor"* — kredi kartı %1,49–3,49 aralığı belirtiliyor |
| Kurulum/aylık ücret | Sağlayıcıya göre değişiyor — **doğrulanmadı** |

⚠️ **Banka standart fiyat listesi yayımlamıyor.** Oran ciroya,
sektöre ve pazarlığa göre belirleniyor. Tamirhane cirosunda pazarlık
gücü düşük olacaktır — **üst banda yakın beklenmeli.**

---

## 3 · Rakamın anlamı: %2 komisyon tamirciye ne yapar

Tohum verimizdeki iş başına farkla ölçelim (K-035'in rakamları):

| | Tutar |
|---|---|
| 2026 alınan | 12.970 ₺ |
| 2026 parça alışı | 7.160 ₺ |
| **İş başına fark** | **5.810 ₺** |

Tüm tahsilat kartla yapılsaydı, %2 komisyon **12.970 × 0,02 = 259 ₺**
eder — **farkın %4,5'i**. Küçük görünür; ama tamirci parayı bugün
**nakit** ya da kendi fiziksel POS'uyla alıyor ve o POS'un komisyonunu
zaten ödüyor. **Sanal POS ek bir maliyet değil, aynı maliyetin başka
kanalı** — yeter ki fiziksel POS'un yerine geçsin, üstüne binmesin.

---

## 4 · Yedi işe hizmet ediyor mu?

Tamircinin dördüncü isteği: *"teslim ettiğinde **ödemeyi nasıl
aldığını bilmek**."* Bu bir **kayıt** isteği, tahsilat isteği değil.
Sanal POS o isteğe cevap vermiyor — **yeni bir iş açıyor.**

**Ama gerçek bir bitişiği var: ALACAK.** Veresiye (*"almadım"*) bu
üründe zaten bir ödeme yolu (K-004) ve Cemil 07.09'da alacak
yönetimini genişletmek istedi (B3–B7). Alacağı olan müşteriye özet
sayfasıyla birlikte bir **ödeme linki** gitmesi, ustanın *"ne zaman
alacağım"* sorusuna gerçekten dokunuyor:

* usta müşteriyi arayıp para istemek zorunda kalmıyor
* müşteri dükkâna gelmeden ödeyebiliyor
* tahsil edildiği an kayda düşüyor — usta ayrıca işaretlemiyor

Bu, **var olan bir işin (alacak takibi) uzantısı**, yeni bir ürün
alanı değil.

---

## 5 · Öneri

**Model B, yalnız ALACAK için, ve B kümesinden SONRA.**

1. **Model A hiç konuşulmasın.** Parayı tutmak lisans, lisanssızı
   hapis. Tartışmaya değmez.
2. **Sanal POS bu fazda YAZILMASIN.** Ödeme kuruluşu hesabı, sözleşme,
   test kartları ve *"ödendi"* bilgisini okuyacak bir **backend**
   gerektiriyor. Bu fazda backend yok.
3. **Sıra: önce alacak, sonra link.** Alacak ekranı (B3–B7) olmadan
   ödeme linkinin gideceği bir yer yok. K-022'nin kapsam kararı
   verilmeden ikisi de başlamıyor.
4. **Sağlayıcı seçimi ertelensin.** Bugün alınacak bir karar,
   backend geldiğinde eskimiş olur. Karar anı: gerçek entegrasyon
   yazılırken.
5. **Tamirciye sorulacak (T-soru):** *"Müşteri aracını almadan,
   telefonundan kartla ödeyebilse işine yarar mıydı?"* Tamirci bundan
   hiç bahsetmedi — istemediğini değil, **aklına gelmediğini**
   düşünüyorum; ama varsayımla ürün yazılmaz.

> **Cemil onayına açık (A-M20):** sanal POS bu fazın dışında bırakıldı.
> Yanlış anladıysam ve amaç *"Tamiron müşterilerden para toplasın"*sa,
> o **başka bir şirket** kurmayı gerektirir; ürün kararı değil, şirket
> kararıdır.

---

## Kaynaklar

- [TCMB · 6493 Sayılı Kanun](https://www.tcmb.gov.tr/wps/wcm/connect/TR/TCMB+TR/Main+Menu/Banka+Hakkinda/Mevzuat/6493/)
- [TCMB · Ödemeler Alanında Sunulan İş Modellerine İlişkin Rehber (PDF)](https://tcmb.gov.tr/wps/wcm/connect/5b814310-a57b-4e64-82fe-76e520d0e0b1/%C3%96demeler+Alan%C4%B1nda+Sunulan+%C4%B0%C5%9F+Modellerine+%C4%B0li%C5%9Fkin+Rehber.pdf?MOD=AJPERES) — *PDF metni tam okunamadı; madde metinleri Lexpera'dan doğrulandı*
- [Lexpera · 6493 konsolide metin (m.12, m.28)](https://www.lexpera.com.tr/mevzuat/kanunlar/odeme-ve-menkul-kiymet-mutabakat-sistemleri-odeme-hizmetleri-ve-elektronik-para-kuruluslari-hakkinda)
- [TÖDEB · Ödeme hizmetlerine ilişkin genel bilgiler](https://todeb.org.tr/detay/odeme-hizmetlerine-iliskin-genel-bilgiler/115/)
- [Gürcan Partners · Sanal POS başvuru şartları](https://gurcanpartners.com/tr/blog/sanal-pos-basvuru-sartlari/)
- [iyzico · Link ile ödeme alma](https://www.iyzico.com/en/business/get-paid-by-link)
- [eticaretmerkezim · iyzico / PayTR / Param karşılaştırması (2026)](https://www.eticaretmerkezim.com/iyzico-vs-paytr-vs-param-e-ticaret-odeme-altyapisi-karsilastirmasi-2026/)

**Doğrulanamayanlar:** kurulum ve aylık sabit ücretler · tamirhane
sektörü için bankaların uyguladığı gerçek oran · ödeme kuruluşlarının
link ürünlerinde şahıs şirketi için istenen belgelerin tam listesi.
