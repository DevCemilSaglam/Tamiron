# Toplu mesaj / kampanya — yapabilir miyiz?

> **Durum:** araştırma notu · 08.09.2026 · kuyruk 4/D2
> **Kaynak (Cemil, 07.09):** *"Tüm müşterilere mesaj/bildirim göndermek
> isterse bir yol bulmalıyız. Burada mesajı sesle okursa, kampanya vb.
> yapay zekâ ile düzeltme vereceğiz."*
> **Önceki not:** `SMS-VE-IYS.md` (03.09) — **bakım hatırlatması** için
> onay gerekmediğini bulmuştuk. **Bu belge o muafiyetin BİTTİĞİ yeri
> anlatıyor.**
>
> ⚠️ Bu bir **hukuki bilgi notu değildir.**
> ⚠️ Rakamlar **Eylül 2026** taramasıdır.

---

## 0 · Ana bulgu — kampanya, hatırlatmanın devamı DEĞİL

`SMS-VE-IYS.md`'de bulduğumuz muafiyet çok dardı ve tam olarak şunu
diyordu (6563 m.6):

> *"…**temin edilen mal veya hizmetlere ilişkin** değişiklik, kullanım
> ve **bakıma yönelik** ticari elektronik iletiler için ayrıca onay
> alınmaz."*

**Kampanya bu cümlenin dışında.** *"Bu ay yağ değişiminde %20
indirim"* bir bakım hatırlatması değil, **pazarlamadır**: temin edilmiş
bir hizmete ilişkin değil, **yeni bir satışa** yönelik.

Yani aynı ürün, aynı müşteri listesi, aynı kanal — **ama farklı hukuk.**

| | Bakım hatırlatması | Kampanya / duyuru |
|---|---|---|
| Onay | **gerekmiyor** (m.6 istisnası) | **ZORUNLU**, kanal bazında |
| İYS kaydı | ret hakkı için gerekli | **kayıt + onayların yüklenmesi zorunlu** |
| Kime | O hizmeti almış müşteriye | Yalnız **onay vermiş** olana |

**İkincisi birincisinin genişletilmiş hâli değil; ayrı bir iştir.**

---

## 1 · İYS — kayıt ve onay

| | |
|---|---|
| **Kim kaydolur** | Ticari elektronik ileti gönderen **tüm hizmet sağlayıcılar** |
| **Nasıl** | MERSİS / VERBİS bilgileriyle İYS'ye kayıt |
| **Onay** | **Kanal bazında ayrı** tutulur — SMS onayı e-postayı ya da aramayı kapsamaz |
| **İYS dışı onay** | Alınabilir ama **üç iş günü içinde İYS'ye yüklenmeli** |
| **İspat yükü** | **Gönderende** — *"onay vardı"* demek yetmez, gösterilmesi gerekir |

### Cezalar — Eylül 2026 taraması

| İhlal | İdari para cezası |
|---|---|
| Onaysız ticari ileti | **2.859 – 14.309 TL** · **toplu gönderimde 10 katına kadar** |
| Ret hakkının 3 iş günü içinde işlenmemesi | **3.799 – 28.544 TL** |

> ⚠️ `SMS-VE-IYS.md`'de *"1.000–5.000 TL"* yazıyordu ve orada **2014
> tutarları olduğu, güncel olmadığı** işaretlenmişti. Doğruymuş:
> güncel taban **üç kat** üstünde ve **toplu gönderimde on katına**
> çıkıyor. O not bugün doğrulandı.

**Toplu gönderimde çarpan var** — yani bu ihlalin cezası tam olarak
Cemil'in yapmak istediği işte en yüksek.

---

## 2 · WhatsApp — bizim modelimiz burada ÇÖKÜYOR

`WHATSAPP.md`'de **A yolunu** seçmiştik ve gerekçesi güçlüydü:

> *"Mesajı BİZ hazırlıyoruz, usta KENDİ WhatsApp'ından gönderiyor.
> Karşılığında hiçbir kurulum yok — Meta hesabı, işletme doğrulaması,
> şablon onayı gerekmiyor."*

**Bu yol tekil mesajda çalışıyor, toplu mesajda çalışmıyor:**

* Kişisel hesaptan ya da WhatsApp Business **uygulamasından** yüzlerce
  kişiye aynı mesajı atmak **numaranın engellenmesine** yol açıyor.
* Meta'nın hizmet şartları, otomasyon araçlarıyla toplu gönderimi
  **açıkça yasaklıyor**.
* Yasal yol **WhatsApp Business API**: Meta hesabı, **işletme
  doğrulaması**, ve her pazarlama mesajı için **önceden onaylanmış
  şablon** (Meta inceliyor).

Yani toplu mesaj, A yolunun kaçındığı **her şeyi** geri getiriyor:
Meta hesabı, doğrulama, şablon onayı, mesaj başına ücret.

> **Ustanın numarası banlanırsa** yalnız kampanya durmaz — **özet
> gönderme, hazır haber verme, parça bekliyor bildirimi** de durur.
> Yani kampanya özelliği, **çalışan altı işi riske atıyor.**
> Bu, tek başına en ağır bulgu.

---

## 3 · Sesle + yapay zekâ ile düzeltme

Cemil: *"mesajı sesle okursa… yapay zekâ ile düzeltme vereceğiz."*

Teknik olarak zor değil. **Sorun sorumluluk:**

* Gönderilen metnin hukuki sorumluluğu **dükkânındır**, yazanın değil.
* Yapay zekâ *"%20 indirim"* yazarsa ve dükkân onu okumadan
  gönderirse, **indirimi vermek zorunda kalır** (ticari taahhüt).
* Bu yüzden AI metni **her zaman ustanın onayından geçmeli** —
  K-016'nın aynı kuralı: *onaysız hiçbir şey gitmez.*

Ayrıca ileti içeriğinde **kimlik bilgisi ve ret yolu** zorunlu (m.8);
AI'nın ürettiği serbest metin bunları düşürebilir. **Şablon çerçevesi
sabit kalmalı, AI yalnız içini yazmalı.**

---

## 4 · Mesaj arşivi — C3'ün cevabı burada

Kuyruk 4/C3'te *"gönderilen mesajlar kaydedilsin mi, ne kadar süre"*
diye sormuştuk. **Cevap mevzuatta ve seçenek değil:**

> Hizmet sağlayıcı, **onay kayıtlarını** onayın geçerliliği bittikten,
> **ticari elektronik iletilere ilişkin diğer kayıtları** ise kayıt
> tarihinden itibaren **ÜÇ YIL** saklar.
> *(Süre 04.01.2020'de bir yıldan üç yıla çıkarıldı.)*

Ve **ispat yükü gönderende.** Yani arşiv bir kolaylık değil,
**savunma belgesi**: şikâyet gelirse *"onay vardı, şu tarihte şunu
gönderdik"* diyebilmenin tek yolu.

**Bu, C3'ün gerekçesini değiştiriyor:** arşivi *"spam kontrolü için"*
değil, **yasal zorunluluk olduğu için** yazacağız — ve o zaman
saklanacak alanlar da belli: kime, hangi kanaldan, ne zaman, hangi
metin, hangi onaya dayanarak.

⚠️ **Ama yalnız TİCARİ ileti için.** Bakım hatırlatması ve özet
gönderimi muafiyet kapsamında; onlar için üç yıllık saklama
zorunluluğunun aynen geçerli olup olmadığı **doğrulanmadı** —
yönetmelik *"ticari elektronik iletilere ilişkin kayıtlar"* diyor ve
muafiyetli ileti de ticari elektronik iletidir. **Hukukçuya
sorulmalı.**

---

## 5 · Yedi işe hizmet ediyor mu?

Tamircinin yedi işi arasında **kampanya yok.** Tamirci bundan hiç
bahsetmedi; istek Cemil'den geliyor ve meşru bir iş isteği — ama
**yedi işin dışında.**

Karşılığında getirdikleri:

| Getiri | Bedel |
|---|---|
| Müşteriye toplu duyuru | İYS kaydı + kanal bazında onay toplama |
| — | WhatsApp Business API (doğrulama, şablon onayı, mesaj ücreti) |
| — | Üç yıllık arşiv ve ispat yükü |
| — | **Ustanın numarasının banlanma riski** — altı işi birden durdurur |
| — | Onaysız gönderimde **10 katına çıkabilen** ceza |

---

## 6 · Öneri

**Kampanya bu fazda yazılmasın. Ama önündeki tek engel teknik değil,
ONAY TOPLAMA.**

1. **Kampanya ≠ hatırlatma.** İkisi aynı ekranda, aynı düğmeyle
   yapılmamalı — usta ayrımı bilmiyorsa yasayı ihlal ettiğini de
   bilmez. **Ayrı yol, ayrı dil.**
2. **Önce onay altyapısı.** Onay olmadan kampanya yazmak, kullanılamaz
   bir düğme yazmaktır. Onay şu an hiçbir yerde toplanmıyor: araç
   kaydında telefon var, **izin yok.**
3. **Sıradaki adım küçük ve ucuz:** araç/müşteri kaydına *"ticari ileti
   izni"* alanı + iznin **ne zaman, nasıl** alındığının kaydı. Bu tek
   alan, ileride kampanyanın önünü açar ve bugün **hiçbir şeyi
   bozmaz.** *(Yazılmadı — B/E kümesi kararı bekliyor.)*
4. **WhatsApp toplu için A yolu terk edilmeli.** Business API'siz toplu
   gönderim, ustanın numarasını yakar. Bu, kampanyanın **en pahalı**
   yanı ve tek başına erteleme gerekçesi.
5. **Mesaj arşivi (C3) yazılsın — ama gerekçesi değişti.** Spam
   kontrolü için değil, **üç yıllık yasal saklama ve ispat yükü** için.
   Yeri ve alanları belli olunca yazılabilir.
6. **AI metni onaysız gitmesin.** Şablonun çerçevesi (kimlik + ret
   yolu) sabit; AI yalnız içeriği yazar, usta onaylar.

> **Cemil onayına açık (A-M21):** kampanya ertelendi, yerine **izin
> alanı** önerildi. Kampanyayı bu fazda istiyorsan yol şudur: İYS
> kaydı → onay toplama → Business API → şablon onayı. Dördü de
> **dükkânın** yapması gereken işler; yazılım en son geliyor.

---

## Kaynaklar

- [Cenuta · 6563 ve İYS: yükümlülükler ve ceza rehberi (2026)](https://www.cenuta.com/blog/6563-sayili-kanun-ve-iys-nedir-ticari-elektronik-ileti-yukumlulukleri-ve-ceza-rehberi-2026/)
- [DSR · İleti Yönetim Sistemi (İYS) 2026 uygulama rehberi](https://dsrbelgelendirme.com.tr/ileti-yonetim-sistemi-iys-nedir-2026-guncel-uygulama-rehberi/)
- [BTS Legal · Hizmet sağlayıcılar için İYS kayıt yükümlülüğü — SSS](https://www.bts-legal.com/insights/publications/hizmet-saglayicilar-icin-ticari-elektronik-ileti-yonetim-sistemi-kayit-yukumlulugu-hakkinda-sikca-sorulan-sorular/)
- [Lexpera · Ticari İletişim ve Ticari Elektronik İletiler Hakkında Yönetmelik (konsolide)](https://www.lexpera.com.tr/mevzuat/yonetmelikler/ticari-iletisim-ve-ticari-elektronik-iletiler-hakkinda-yonetmelik)
- [Lexology · Yönetmelik değişikliği — saklama süresi 1 yıldan 3 yıla](https://www.lexology.com/library/detail.aspx?g=25b3f4bc-80dd-4c72-8e35-7af88c749b53)
- [whapi.cloud · WhatsApp toplu mesaj neden çalışmıyor (2026)](https://whapi.cloud/blog/tr/whatsapp-bulk-messaging-not-working-2026)
- [Yanıtly · WhatsApp Business API nedir, nasıl alınır (2026)](https://yanitly.com/blog/whatsapp-business-api-nedir-nasil-alinir)

**Doğrulanamayanlar:** İYS kaydında alıcı sayısına göre (ör. 150.000
altı) farklı bir yükümlülük olup olmadığı · muafiyetli iletilerin
(bakım hatırlatması) üç yıllık saklamaya tabi olup olmadığı ·
WhatsApp Business API'nin Türkiye'deki mesaj başına güncel ücreti.
