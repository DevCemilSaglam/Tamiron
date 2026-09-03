# WhatsApp — parçacıya fiyat sorma

> **Durum:** araştırma notu · 03.09.2026 · iş 8
> **Akıştaki yeri:** Adım 3–4 · Ekran 4 (`IS-AKISI.md`, `EKRANLAR.md`)
> **Kaynak (Cemil):** *"Yerel parçacının tel belli. Bunları eklememiz gerekecek. Bunlar whatsapp api ile mesaj göndereceğiz."* (T-12b)

---

## 0 · Kararı belirleyen tek soru

> **Parçacının cevabını biz mi okuyacağız, usta mı girecek?**

Akışın 4. adımı *"fiyat geldi, kayda düştü"* diyor. Bu **ancak API ile** olur. Cevabı okumayacaksak adım *"usta gelen fiyatı yazar"* olur — akış aynı kalır, **maliyeti ve kurulumu tamamen değişir.**

Üç yol var ve aralarındaki fark teknik değil, **kurulum yükü** farkı.

---

## 1 · Üç yol

| | **A · Paylaş düğmesi** | **B · Cloud API (Meta)** | **C · BSP (Twilio vb.)** |
|---|---|---|---|
| Nasıl | Telefonun paylaşım penceresi WhatsApp'a açılır, mesaj hazır | Sunucumuz doğrudan Meta'ya | Aracı firma üzerinden |
| **Cevabı okuyabilir miyiz** | **HAYIR** — usta elle girer | **Evet** (webhook) | Evet |
| Mesaj maliyeti | **0** | Meta tarifesi | Meta tarifesi **+ aracı payı** |
| Kurulum | **Yok** | Meta app · işletme doğrulama · şablon onayı · numara kaydı | BSP hesabı |
| Ruhsat fotoğrafı | **Gider** (paylaşımda dosya taşınır) | Gider | Gider |
| Parçacının izni | **Gerekmez** — usta kendi gönderiyor | **Gerekir** (opt-in) | Gerekir |
| Hangi numaradan | **Ustanın kendi numarası** | Dükkânın iş numarası | Dükkânın iş numarası |

> **BSP zorunlu değil.** Piyasadaki Türkçe yazıların çoğu *"BSP gerekir"* diyor; Meta'nın **Cloud API**'si 2022'den beri doğrudan kullanılabiliyor ve Meta barındırıyor. BSP bir kolaylık, ön koşul değil.

---

## 2 · Meta tarifesi — bizi ne ilgilendiriyor

**1 Temmuz 2025'ten beri mesaj başına ücret** (öncesi konuşma başınaydı).

| Kategori | Ücret |
|---|---|
| **Service** — müşteri yazdıktan sonraki 24 saat içinde serbest yanıt | **ÜCRETSİZ** |
| **Utility** — işlem bildirimi | Ücretli; **24 saatlik pencere açıkken ücretsiz** |
| Marketing | Her zaman ücretli |
| Authentication | Ücretli |

**Bizim mesajımız `utility`** — ticari tanıtım değil, bir işlem talebi.

**Kritik ayrıntı:** Parçacı bir kez cevap yazınca **24 saatlik pencere açılıyor** ve o pencere içindeki her şey ücretsiz. Yani tipik maliyet: **sorgu başına bir utility şablonu.** Cevabı almak, okumak, sonra yazışmak — hepsi bedava.

> Türkiye tarifesi Meta'nın ülke bazlı rate card'ında yayınlanıyor ve **utility oranları düşürüldü** — ama **somut rakamı doğrulayamadım**, sözleşme öncesi Meta'nın güncel rate card'ından bakılmalı. **Uydurma rakam yazmıyorum.**

---

## 3 · Şablon zorunluluğu — ve bizim sorunumuz

Pencere kapalıyken **serbest metin gönderilemez.** Önceden **onaylı şablon** gerekir:

- Onay **24 saate kadar** sürüyor
- Şablon sabit metin + `{{degisken}}` taşır
- `APPROVED` olmadan gönderilemez

**Bizim mesajımız buna zor sığıyor.** Gitmesi gereken şey:

```
[ruhsat fotoğrafı]
Ürün adı · marka · OEM kodu (T-13)
— ama liste UZUNLUĞU değişken
```

Şablon değişkeni **tek değer** taşır, liste taşımaz. Bütün listeyi tek değişkene sığdırmak teknik olarak mümkün ama Meta'nın *"çoğu değişken olan şablon"* kuralına takılabilir. **Görsel başlık (image header) destekleniyor** — ruhsat fotoğrafı sorun değil.

**Bu bir risk ve şablon yazılmadan kesinleşmez.**

---

## 4 · Teknik sınırlar (Cloud API)

| | |
|---|---|
| Görsel | **JPEG/PNG, en fazla 5 MB** — ruhsat fotoğrafı için fazlasıyla yeterli |
| Belge (PDF) | 100 MB |
| Gelen mesaj | **Webhook** ile anında düşer — cevabı okumak mümkün |
| Medya saklama | Yüklenen 30 gün, webhook'tan gelen **7 gün** durur |

> **7 gün önemli:** parçacı cevabında fotoğraf gönderirse (kutu resmi, etiket) onu **kendi tarafımıza indirmemiz** gerekir, yoksa kaybolur.

---

## 5 · Önerim: **A ile başla, B'ye hazır dur**

**Gerekçe — kurulum yükü kimin üstünde:**

B ve C, dükkânın **Meta iş hesabı açmasını, işletme doğrulamasından geçmesini, numara kaydetmesini** gerektiriyor. Kullanıcımız *"ben anlamam"* diyen tamirci (T-18). Bu kurulumu her dükkân için ayrı ayrı yapmak, ürünün önüne **satıştan önce bir duvar** koyar.

**A yolunda hiçbiri yok:** usta düğmeye basar, kendi WhatsApp'ı açılır, mesaj ve ruhsat hazırdır, gönderir. **Bugün zaten yaptığı şeyin aynısı** — biz sadece yazmayı ve fotoğrafı hazırlıyoruz. Değer buradan geliyor.

**Kaybettiğimiz tek şey:** cevabı otomatik okumak. Usta gelen fiyatı elle giriyor — bir alan, bir dokunuş.

**Ne zaman B'ye geçilir:** Cevabı okumak, `PartMatch` birikimini (K-001) **kendiliğinden** besler. Elle girişte usta kodu yazmaz, sadece fiyatı yazar → kod birikmez. **Yani A, parça tanımlama stratejisini yavaşlatır.** Dükkân sayısı artınca ve API kurulumu bizim tarafımızdan yapılabilir hâle gelince B'ye geçilmeli.

### Bunun akışa ve modele etkisi

| | A ile |
|---|---|
| Adım 4 | *"Fiyat geldi"* → **"Gelen fiyatı yaz"** |
| `PriceReply.RawText` | Parçacının yazdığı değil, **ustanın yapıştırdığı/yazdığı** |
| `PriceRequest.SentText` | Aynen duruyor — ne sorduğumuzu biliyoruz |
| Ekran 4 | Aynı; *"Gönder"* düğmesi paylaşım penceresini açar |

**Ekran değişmiyor, adım sayısı değişmiyor.** Sadece 4. adımda veri elle giriyor.

---

## 6 · AÇIK

| Kod | Karar |
|---|---|
| **A-24** | **A mı B mi?** Öneri: **A ile başla.** Cemil'in kararı — API'yi baştan istiyorsa kurulum yükünü kimin taşıyacağını konuşmalıyız |
| **A-25** | A seçilirse `navigator.share()` ile **fotoğraf + metin** birlikte paylaşılabiliyor mu — prototipte gerçek telefonda denenmeli. Denenmeden söz verilmez |
| **A-26** | B seçilirse **Türkiye utility tarifesi** Meta rate card'ından doğrulanmalı |
| **A-27** | B seçilirse şablon, **değişken uzunluktaki parça listesini** taşıyabilecek mi? Onaya gönderilmeden bilinmez |
| **A-28** | B'de parçacının **opt-in**'i nasıl alınacak? İş ortağı ama Meta izin istiyor |

---

## Kaynaklar

- [Meta · WhatsApp Business Platform Pricing](https://developers.facebook.com/docs/whatsapp/pricing) — mesaj başına model (01.07.2025), kategoriler, 24 saatlik ücretsiz servis penceresi
- [Meta · Cloud API Overview](https://developers.facebook.com/docs/whatsapp/cloud-api/overview) — gelen mesajlar webhook ile
- [Meta · Cloud API Media](https://developers.facebook.com/docs/whatsapp/cloud-api/reference/media) — JPEG/PNG 5 MB, webhook medyası 7 gün
- [Meta · Message Template Guidelines](https://developers.facebook.com/docs/whatsapp/message-templates/guidelines) — onay 24 saate kadar, `APPROVED` şartı, değişken biçimleri
- [WhatsApp Business API Pricing 2026 — Blueticks](https://blueticks.co/blog/whatsapp-business-api-pricing-2026) · [EngageLab](https://www.engagelab.com/blog/whatsapp-business-api-pricing) — ülke bazlı oran aralıkları (**doğrulanmadı**, yalnız büyüklük fikri için)

> **Uyarı:** Meta tarifeleri ve şablon kuralları sık değişiyor. Bu belge **03.09.2026** tarihlidir; sözleşme öncesi güncel rate card'a bakılmalı.
