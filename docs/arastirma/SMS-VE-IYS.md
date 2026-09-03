# SMS ve İYS — bakım hatırlatması yasal mı

> **Durum:** araştırma notu · 03.09.2026 · iş 10
> **Akıştaki yeri:** Adım 7 sonrası (`IS-AKISI.md` · tamirat/bakım bölümü) · **K-002**
> **Kaynak (Cemil):** *"Tekrar bakım geldiğinde bilgi sms olarak göndermeliyiz."* (T-17)
>
> ⚠️ Bu bir **hukuki bilgi notu değildir.** Mevzuat okunarak çıkarılmıştır; sözleşme öncesi mali müşavir/hukukçu teyidi alınmalıdır.

---

## 0 · Ana bulgu — **bakım hatırlatması için ayrıca onay GEREKMİYOR**

Korktuğumuz şey buydu: *"İYS izni olmayan müşteriye SMS atamayız, o zaman hatırlatma çalışmaz."*

**6563 sayılı Kanun, madde 6** bu hâli açıkça istisna tutuyor:

> *"Kendisiyle iletişime geçilmesi amacıyla alıcının iletişim bilgilerini vermesi hâlinde, **temin edilen mal veya hizmetlere ilişkin değişiklik, kullanım ve bakıma yönelik** ticari elektronik iletiler için ayrıca onay alınmaz."*

Bizim hâlimiz kelimesi kelimesine bu:

| Kanunun aradığı | Bizde |
|---|---|
| Alıcı iletişim bilgisini **kendisi vermiş** | Müşteri aracı bırakırken telefonunu veriyor |
| **Temin edilen** mal veya hizmet var | Bakım yapıldı, para alındı |
| İleti o hizmetin **bakımına yönelik** | *"Bakım zamanı geldi"* |

> **Sonuç: hatırlatma gönderilebilir.** Bu, K-002'yi ve tamircinin T-17 isteğini yasal zeminde ayakta tutuyor.

**Ama muafiyet onayla ilgili — ret hakkıyla değil.** İki şey hâlâ zorunlu (§2).

---

## 1 · Yasal çerçeve

| | |
|---|---|
| **Kanun** | 6563 · Elektronik Ticaretin Düzenlenmesi Hakkında Kanun |
| **Kural (m.6)** | Ticari elektronik ileti **ancak önceden onay alınarak** gönderilir |
| **Bizim istisnamız (m.6)** | Temin edilen hizmete ilişkin **bakıma yönelik** iletide onay aranmaz |
| **Ret hakkı (m.8)** | Alıcı **her zaman, gerekçesiz** reddedebilir. Ret **kolay ve ücretsiz** olmalı, **3 iş günü** içinde durdurulmalı |
| **İYS (m.11/5)** | Bakanlık, onayların alınması ve **ret hakkının kullanılması** için elektronik sistem kurmaya yetkili → İleti Yönetim Sistemi |

**Onay gerekmeyen diğer haller** (bizi ilgilendiren ikisi):
- Devam eden abonelik/üyelik, **tahsilat, borç hatırlatma, bilgi güncelleme, satın alma ve teslimat** bildirimleri
- **Tacir veya esnaf** olan alıcılara gönderilenler → **parçacıya gönderdiğimiz mesajlar bu kapsamda**

**İdari para cezası** kanunda m.6 için 1.000–5.000 TL, m.8 için 2.000–15.000 TL olarak yazılı. ⚠️ Bunlar **2014 tutarları** ve her yıl yeniden değerleme oranıyla artıyor — **güncel rakam bunlar değildir.**

---

## 2 · Muafiyete rağmen zorunlu olan iki şey

### a) Her mesajda **ret yolu** bulunmalı (m.8)

Onay muafiyeti ret hakkını kaldırmıyor. SMS'te bunun standart biçimi:

```
Ret için B İPTAL yaz 4045'e gönder
```

Sağlayıcılar bu mekanizmayı hazır veriyor.

> **⚠️ 6. işte yazdığım mesaj taslağında bu satır YOKTU.** Düzeltilecek.

### b) Ret bildirimleri **İYS'ye işlenmeli**

Müşteri reddederse bir daha gönderilmemeli ve bu **3 iş günü** içinde etkili olmalı. İYS bunun merkezî kaydı; sağlayıcıların çoğu **İYS entegrasyonunu hazır** sunuyor.

---

## 3 · Maliyet

Toplu SMS piyasası, paket büyüklüğüne göre:

| | Yaklaşık birim |
|---|---|
| Küçük/tanışma paketi | **~0,089 TL/SMS** |
| Standart paket (10.000) | **~0,165 TL/SMS** |
| Yıllık numara/başlık kullanım ücreti | ~349 TL + ~50 TL (sağlayıcıya göre) |

Yalnız **ulaşan** mesaj ücretlendiriliyor; ulaşmayan kredi iade ediliyor.

**Bizim hacmimiz:** Bir dükkân ayda kaç bakım yapıyorsa o kadar hatırlatma. Diyelim ayda 60 bakım → **6 ay sonra** ayda ~60 SMS → **~6–10 TL/ay.** İhmal edilebilir.

> BTK tarifesi 01.01.2026'da güncellendi; rakamlar **03.09.2026** tarihlidir ve sağlayıcıya göre değişir.

---

## 4 · Asıl mesele: **mesaj kimin adına gidiyor?**

Bu, WhatsApp'takiyle (`WHATSAPP.md` §5) **aynı gerilim** — ve burada **yasal boyutu da var.**

| | **A · Dükkân kendi hesabını açar** | **B · Biz tek hesaptan göndeririz** |
|---|---|---|
| Başlık | **AHMET OTO** | TAMIRON |
| Müşteri ne görür | Tanıdığı dükkân | *"Tamiron kim?"* |
| Kurulum | Her dükkân: sağlayıcı hesabı, başlık başvurusu, İYS kaydı | Yok |
| **Muafiyet geçerli mi** | ✅ Hizmeti veren = gönderen | ⚠️ **Şüpheli** — hizmeti dükkân verdi, ileti bizden gidiyor |

**B yasal olarak zayıf.** Muafiyetin dayanağı *"temin edilen hizmet"* — hizmeti veren dükkân. Gönderen biz olursak muafiyetin bize uzandığı tartışmalı hâle gelir.

### Öneri: **C · aracı model**

Sağlayıcılar **bayi/alt hesap** yapısı sunuyor. Biz sağlayıcıyla **bir kez** entegre oluruz; her dükkân **kendi başlığıyla** kendi alt hesabından gönderir.

| | |
|---|---|
| Müşteri gördüğü | **AHMET OTO** — inandırıcı |
| Gönderen (yasal) | **Dükkân** — muafiyet yerinde |
| Ustanın yapacağı | Bir kez başlık başvurusu; gerisini biz kurarız |
| Bizim yapacağımız | Tek entegrasyon, İYS bağlantısı |

Ustanın üstünde kalan tek yük **başlık başvurusu** — ve bu kaçınılmaz, çünkü başlık dükkânın markası.

---

## 5 · Parçacıya giden mesaj — ayrı hikâye

`WHATSAPP.md`'deki mesajlar **parçacıya**, yani **tacir/esnafa** gidiyor. Kanun bunları da onay dışında tutuyor (m.6). Yani WhatsApp tarafında **opt-in kaygısı yasal olarak hafif** — `WHATSAPP.md` A-28'i bu bilgiyle yeniden değerlendirilmeli.

> Not: Meta'nın **kendi** opt-in kuralı Türk kanunundan ayrıdır ve API kullanılırsa yine geçerlidir.

---

## 6 · AÇIK

| Kod | Karar |
|---|---|
| **A-34** | **C (aracı model) onaylanıyor mu?** Sağlayıcı seçimi buna bağlı |
| **A-35** | Hangi sağlayıcı? Karşılaştırma yapılmadı — İYS entegrasyonu ve **bayi yapısı** olanlar arasından seçilmeli |
| **A-36** | Ret bildirimi geldiğinde **bizim tarafımızda** ne olacak: aracın telefonu işaretlensin, hatırlatma bir daha kurulmasın. Modelde karşılığı yok — `Vehicle`'a bir alan gerekebilir |
| **A-37** | Hukukçu/mali müşavir teyidi — özellikle §4'teki gönderen kimliği |
| **A-38** | Müşteriye giden **özet** (T-19/T-20) de ticari elektronik ileti mi? *"Satın alma ve teslimat bildirimi"* muafiyetine giriyor gibi duruyor ama teyit edilmeli |

---

## 7 · Bu araştırmanın akışa etkisi

1. **Mesaj taslağına ret satırı eklenecek** (6. iş çıktısı düzeltilecek)
2. **`Vehicle`'a ret işareti** gerekebilir (A-36)
3. **Hatırlatma yasal olarak sağlam** — K-002 değişmiyor
4. **Parçacı mesajlarında onay kaygısı hafifledi** — `WHATSAPP.md` A-28 güncellenecek

---

## Kaynaklar

- [6563 sayılı Elektronik Ticaretin Düzenlenmesi Hakkında Kanun — Lexpera](https://www.lexpera.com.tr/mevzuat/kanunlar/elektronik-ticaretin-duzenlenmesi-hakkinda-kanun-6563) — m.6 onay ve istisnalar, m.8 ret hakkı, m.11/5 İYS
- [Mevzuat.gov.tr · Kanun metni (PDF)](https://mevzuat.gov.tr/MevzuatMetin/1.5.6563.pdf)
- [Mevzuat Kapsamında Ticari Elektronik İletiler — Ersan Şen Hukuk](https://sen.av.tr/tr/makale/mevzuat-kapsaminda-ticari-elektronik-iletiler) — onay gerekmeyen hâllerin dökümü
- [Netgsm · Toplu SMS](https://www.netgsm.com.tr/sms/toplu-sms) · [iletiMerkezi · Toplu SMS fiyatları](https://www.iletimerkezi.com/toplu-sms-fiyatlari) · [Mutlucell tarifeler](https://www.mutlucell.com.tr/toplu-sms-tarifeleri) — birim fiyat aralığı, İYS entegrasyonu, başlıklı SMS
