# TAMİRCİ GERİ BİLDİRİMİ — bu deponun 1 numaralı belgesi

> **Tarih:** 03.09.2026 · Cemil programı bir tamirciye gösterdi, notları aktardı.
> **Neden 1 numara:** Tamiron v1 baştan sona **Cemil + AI akıl yürütmesiyle** kuruldu. Bu, projenin aldığı **ilk gerçek kullanıcı sinyali**. Yeni akış buradan türer, tersinden değil.
>
> ⚠️ **§1 ham metindir — DEĞİŞTİRİLMEZ.** Süzülmedi, düzeltilmedi, sıraya konmadı. Sonradan *"biz bunu böyle mi anlamıştık"* sorusunun tek dayanağı budur. Yorum §3'te, ayrı.

---

## 1 · HAM NOT (Cemil'den, birebir)

> * Öncelikle çok fazla özellik var. Karmaşık geldi.
> * Sürecin işleyişi genelde (%80) araç geliyor, örnek olarak trigger kayışının değişmesini istiyor. Trigger seti için fiyat öğreniliyor, yerel yedek parçacıdan. Müşteri kendi gidip alıp gelebiliyor. Yada tamirci alıyor. Parçayı müşteri kendi aldı ise değiştirilen parçada sorun olduğunda tekrar işçilik alınıyor. Fakat parçayı tamirci satın aldı ise, sorun çıkaran ürünü ücretsiz değiştirebiliyor. Parçayı müşterinin getirmesi önemli. Trigger değiştirilirken, Örnek olarak balata da eskidi ise balata değişim teklifi sunuyor tamirci müşteriye. Fakat bunlar akış olarak değil. Yani serviste güncelleme yapılabiliyor. Usta kendi fiyat aldıysa, parça fiyatlarına ekleme yapabiliyor. Büyük bir iş ise genelde parçalar farklı firmalardan araştırılabiliyor. Araca uyumlu parça durumu geçerli. Yedek parçacıdan fiyat alırken, ruhsatı ve gereken parçaları gönderiyor. Buradan parçaların oem kodları ile whatsapptan göndermemiz gerekecek. Ürün adı, marka, oem kodu şeklinde. Ruhsatı çekti, kayıt açıldı. Yapılan işlemleri söyledim. Durum olarak, teslim edildiye Bakacağız. Tamirat ve bakım iki ayrı durum var. Bakımda km önemli. Tekrar bakım geldiğinde bilgi sms olarak göndermeliyiz. Mümkün olduğunca basit olacak şekilde tasarlamamız gerek. Bunu kullanacak kişiler teknolojiden uzak kişiler, ben anlamam diyecek kişiler.

---

## 2 · SORU–CEVAP (Cemil, 03.09.2026)

**S1 · "Durum olarak teslim edildiye bakacağız" ne demek?**
> Sadece teslim durum olarak kalacak.

**S2 · Müşteri parçayı getirdiğinde kayıtta parça satırı nasıl duruyor?**
> 0 TL olabilir, fiyatsız olabilir.

**S3 · Garanti süresi tutulacak mı?**
> Garanti süresi yok.

**S4 · Ek iş onayı nasıl alınıyor, kayıtta izi kalmalı mı?**
> Ruhsatı çekti, parçayı aldı, lifte kaldırdı. Bu sırada balataları gördü ve dedi ki balatalar nanay. Müşteri tamam derse, değişiyor. Burada müşteri onayı vb işin içine katmayalım. Yapılan işe ek iş yapılabilir teslim etmeden önce. Bizi ilgilendiren durum bu.

**S5 · Bakım periyodu neye göre?**
> Km'yi takip edemeyiz sanırım, ay olarak takip edilebilir.

**S6 · Yerel parçacı sistemde kayıtlı mı?**
> Yerel parçacının tel belli. Bunları eklememiz gerekecek. Bunlar whatsapp api ile mesaj göndereceğiz.

**S7 · OEM kodunu kim buluyor?**
> Tamirci bilmiyor, yedek parçacıya gönderiyor, o geri dönüş olarak listeyi veriyor.

**S8 · "Çok fazla özellik" derken neyi işaret etti?**
> Karmaşık dedi. İstediği; aracın bilgilerini girmek, yapılan işlemleri belirtmek, fiyatı belirtmek, teslim ettiğinde ödemeyi nasıl aldığını bilmek. Basitçe bu. Ayrıca müşteriye iş listesinin yani tutarın iş, parça açıklaması ve Toplam tutar olarak pdf gönderiyorlarmış. Bunu WhatsApp üzerinden uygulama linki gönderme fikri mantıklı geldi. Müşteride de program olması beğenildi.

---

## 3 · MADDELER (T-1 … T-21)

Konuşurken numarayla anılır: *"T-8'i şöyle kuralım."*

### Kapsam

| # | Madde |
|---|---|
| **T-1** | Çok fazla özellik var, karmaşık geldi |
| **T-18** | Kullanıcı **teknolojiden uzak**; *"ben anlamam"* diyecek kişi. Bu bir tasarım kısıtıdır, temenni değil |

**T-1'in cevabı (S8) ürünün tanımıdır — tamirci dört iş istiyor:**

1. Aracın bilgilerini girmek
2. Yapılan işlemleri belirtmek
3. Fiyatı belirtmek
4. Teslim ettiğinde **ödemeyi nasıl aldığını** bilmek

### Asıl akış (%80)

| # | Madde |
|---|---|
| **T-2** | Müşteri **ne istediğini bilerek** geliyor ("triger kayışı değişsin"). Teşhis %20 istisna |
| **T-3** | **Ruhsat fotoğrafı çekildi → kayıt açıldı.** Giriş noktası bu |
| **T-4** | Yapılan işlemler **sesle** söyleniyor |
| **T-5** | Fiyat **yerel yedek parçacıdan** öğreniliyor |
| **T-6** | **Tek durum: teslim edildi mi, edilmedi mi** (S1) |

### Parçayı kim aldı — sorumluluk anahtarı

| # | Madde |
|---|---|
| **T-7** | Parçayı müşteri getirebilir, ya da tamirci alır |
| **T-8** | **Müşteri getirdiyse** → sorun çıkınca **tekrar işçilik alınır**.<br>**Tamirci aldıysa** → **ücretsiz** değiştirir |
| **T-9** | Tamirci kendi aldıysa parça fiyatına **ekleme** yapabiliyor |
| **T-8b** | **Garanti SÜRESİ tutulmaz** (S3). Yalnız *"kim getirdi"* tutulur |
| **T-7b** | Müşteri parçasının satırı **0 TL / fiyatsız** durur (S2) |

### İş büyürken

| # | Madde |
|---|---|
| **T-10** | Ek iş (balata) **akış değil**: teslimden önce kayda satır eklenir. **Onay mekanizması kurulmaz** (S4) |
| **T-11** | **Büyük işte** farklı firmalardan araştırma — istisna |

### Parçacıyla iletişim

| # | Madde |
|---|---|
| **T-12** | Fiyat sorarken **ruhsat + gereken parçalar** gönderiliyor |
| **T-13** | Liste biçimi: **ürün adı · marka · OEM kodu** |
| **T-14** | **Araca uyumlu parça** önemli |
| **T-13b** | **OEM kodunu tamirci bilmiyor — parçacı geri dönüşte veriyor** (S7) |
| **T-12b** | Parçacı **kayıtlı firma** olacak (ad + telefon), **WhatsApp API** ile mesaj (S6) |

### Tamirat / Bakım

| # | Madde |
|---|---|
| **T-15** | **Tamirat ve bakım iki ayrı** |
| **T-16** | Bakımda **km** önemli |
| **T-17** | Sonraki bakım zamanı gelince **SMS** |
| **T-16b** | Takip **ay** üzerinden — *"km'yi takip edemeyiz sanırım"* (S5) |

### Müşteri tarafı — S8'den çıkan yeni başlık

| # | Madde |
|---|---|
| **T-19** | Bugün müşteriye **PDF** gönderiyorlar: iş · parça açıklaması · toplam tutar |
| **T-20** | **WhatsApp'tan uygulama linki** göndermek mantıklı bulundu |
| **T-21** | **Müşteride de program olması** beğenildi |

---

## 4 · ÇÖZÜLMESİ GEREKEN İKİ ÇELİŞKİ

Bunlar tamircinin sözüyle Cemil'in cevabı arasında; tasarımdan **önce** kapanmalı.

### Ç-1 · km mi, ay mı? (T-16 ↔ T-16b)

- Tamirci: *"Bakımda km önemli."*
- Cemil: *"Km'yi takip edemeyiz sanırım, ay olarak takip edilebilir."*

**Öneri — ikisi çelişmiyor, iki ayrı iş:**

| | Ne için | Nereden |
|---|---|---|
| **km** | Bakımın **kaydı** — hangi kilometrede yapıldı | Araç geldiğinde usta gösterge panelinden okur, bir kez yazar |
| **ay** | Bakımın **hatırlatması** — ne zaman SMS gidecek | Kayıt tarihinden sayılır |

Km'yi *sürekli takip etmek* imkânsız (araç dükkânda değil), ama *o anki km'yi yazmak* bedava ve geçmişte değerli. Hatırlatma zaman üzerinden kurulur. **Karara bağlanacak.**

### Ç-2 · OEM kodunu kim üretiyor? (T-13 ↔ T-13b)

- Tamirci: *"parçaların oem kodları ile whatsapptan göndermemiz gerekecek"*
- Cemil: *"Tamirci bilmiyor, parçacı geri dönüş olarak listeyi veriyor"*

**Yön belirsiz.** İki okuma var:

- **(a)** Biz gönderirken kod olacak → kodu **biz bulmalıyız** → TecDoc/`Fitment` verisi **ertelenemez**
- **(b)** Biz ruhsat + parça adı gönderiyoruz, parçacı **ad·marka·kod·fiyat** ile dönüyor → kodu **biz bulmuyoruz** → **TecDoc ertelenebilir**

Cevap **(b)** yönünde ama tamircinin cümlesi (a) gibi duruyor. **Bu ürünün en pahalı kaleminin kaderini belirliyor** — netleşmeden ilerlenmez.

**(b) doğruysa kazanç çok büyük:** v1'de A-11.3 *"ertelenemez"* diye işaretlenmişti, çünkü uyumlu parçayı bulmak `Fitment` verisi istiyordu. Eğer uyumu **parçacı** çözüyorsa (ruhsatı görüyor, kendi kataloğundan buluyor), TecDoc lisansı **ana yoldan çıkar**. T-14 *"araca uyumlu parça"* da o zaman *"parçacı garanti ediyor"* demek olur.

---

## 5 · v1 KARARLARINDAN DÜŞENLER

| Ne | Neden düştü |
|---|---|
| **Yedi durum** (fiyat · parça · onay · işlemde · hazır · red · kapandı) | T-6/S1: **tek durum** |
| **Onay ekranı ve red/indirim paneli** | S4: onay mekanizması kurulmuyor |
| **Kesinlik: kesin / olabilir / şartlı** | Bizim icadımızdı; tamirci böyle konuşmadı |
| **Garanti sekmesi / süre takibi** | S3: garanti süresi yok |
| **Fiyat karşılaştırma merkezî akış** | T-11: büyük iş istisnası |
| **Rozetli eksik parça tavsiyesi** | T-10: satır ekleniyor, mekanizma değil |
| **Rol/yetki (4 rol · 7 anahtar)** | Tamirci hiç bahsetmedi — mahalle tamircisi için fazla |

## 6 · v1'DE HİÇ OLMAYAN, ŞİMDİ GEREKENLER

1. **Parçayı kim getirdi** — ileride kimin ödeyeceğini belirleyen anahtar (T-8)
2. **Ruhsattan kayıt açma** (T-3)
3. **Parçacıya WhatsApp ile fiyat sorma** — kayıtlı firma + ruhsat + parça listesi (T-12, T-12b)
4. **Ay bazlı bakım hatırlatma SMS'i** (T-17, T-16b)
5. **Müşteriye özet + uygulama linki** (T-19, T-20, T-21)

---

## 7 · ÜRÜNÜN YENİ TANIMI

S8'deki dört iş + notlardan çıkan üç iş = **yedi iş.**

| # | İş |
|---|---|
| 1 | Aracın bilgilerini girmek *(ruhsattan)* |
| 2 | Yapılan işlemleri belirtmek *(sesle)* |
| 3 | Fiyatı belirtmek |
| 4 | Teslimde ödemeyi nasıl aldığını bilmek |
| 5 | Parçacıya fiyat sormak *(WhatsApp)* |
| 6 | Müşteriye özet göndermek *(link/PDF)* |
| 7 | Bakım hatırlatması *(SMS)* |

**Ölçüt:** v1'de 68 senaryo ve 38 ekran vardı. Yeni ürün bu yedi işi yapacak.
