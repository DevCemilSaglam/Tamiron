# TAMIRON — ÇALIŞMA KUYRUĞU (CALISMA-KUYRUGU.md)

> **Ne işe yarar:** Cemil bilgisayarda değilken lider tek başına
> çalışıyor. Bu dosya **sıradaki işi** ve **hangi işte kalındığını**
> tutar. HANDOFF *durumu* söyler, bu dosya *sırayı*.
>
> **Kural:** bir iş bitmeden bir sonrakine geçilmez. Her iş bitince
> · derleme denetimi · sağlık denetimi 320/375/430 px'te **0 bulgu**
> · commit (gerekçesiyle) · push · bu dosyada **BİTTİ** işareti + tek
> satır sonuç.
>
> **Karar gerektiren yerde durulmaz:** varsayım yazılır, *"Cemil
> onayına açık"* işaretlenir, iş bitirilir. Yalnız yanlış varsayım
> işi çöpe çıkaracaksa durulur ve buraya **BEKLİYOR** yazılır.
>
> **Yeni özellik yok.** Bu kuyruk hiçbir yeni özellik içermiyor;
> hepsi **doğrulama, uzlaştırma ve ölçüm**. Kuyruk sırasında yeni bir
> özellik ihtiyacı doğarsa yazılmaz — buraya *öneri* olarak düşülür.

---

## Tur 7 · 16.09.2026 · son commit `0a3b205`

> **Cemil:** *"Servis kayıt ekranı istediğim gibi değil. Orada parça
> ekle, işçilik ekle düzenle vb daha kolay olmalı. Sesle işlem yapılan
> ekranlarda sesten gelen veri yapılabilecek işler için json dosyası
> doldursun ve kullanıcı onayı ile işlemi yapsın. Kullanıcı farklı
> ekranda farklı bir işlem yapmak isteyebilir. Google aramada olduğu
> gibi 'bunu mu demek istediniz' diye yönlendirebilelim. Servis detayda
> yine sesle işlem yapmak istediğinde bu yaklaşımı ele almak lazım…
> Bittikten sonra bir kaynak hazırlaman gerek. Bu haliyle sunum yapmak
> istiyorum ve özelliklerimiz, baştan sona nasıl kullanılır göstermek
> istiyorum."*

| # | İş | Durum |
|---|---|---|
| 7.1 | **Servis kaydında satır düzenleme** | **BİTTİ** — asıl kusur çıktı: *ekle düğmesi adımların üstünde, satır tablosu 2. adımın içinde* → 1. adımda basınca satır **görünmeden** ekleniyordu. Artık satırın durduğu adıma geçiliyor + odak yeni satıra. Tip rozeti **düğme** oldu (parça↔işçilik, ad ve tutar korunuyor). 49. denetim testi koruyor |
| 7.2 | **Ses → işlem nesnesi → onay**, her ekranda aynı biçimde ve **görünür** | SIRADA |
| 7.3 | **"Bunu mu demek istediniz"** — yalnız plakada değil, **yanlış ekran/yanlış işlem** için de | SIRADA |
| 7.4 | **Servis detayında sesle işlem** — aynı yaklaşım kayıt içinde de | SIRADA |
| 7.5 | **Sunum kaynağı** — özellikler + baştan sona kullanım | SIRADA |

**Zemin (bugün ne var):** `sesCozumle()` zaten bir **işlem nesnesi**
üretiyor (`{tur, plaka, is, km, oneri…}`) ve onay ekranı var — yani
7.2'nin yarısı yazılı. *"Bunu mu demek istediniz"* yalnız **plaka**
için çalışıyor (A-M27, 44. test). `kayit-icinde` bağlamı var ama
kapsamı dar. Yani tur, sıfırdan yazmak değil, **var olanı bütünlemek.**

⚠️ **7.5 ötekiler bitmeden yazılamaz:** sunum, olmayan bir şeyi
anlatırsa ilk soruda çöker.

---

## Tur 6 · 16.09.2026 · son commit `bef7f2e`

Cemil sekiz not iletti (14–16.09, WhatsApp). Sekizi **beş iş**:
ikisi ikişer notun aynı konusu.

| # | İş | Hangi notlar | Durum |
|---|---|---|---|
| 6.1 | **Açılış logosunun arka planı** — uygulama açılırken logonun altında kutu görünüyor | *"Uygulama açılırken logonun arka planı var. Svg olmalı."* | **BİTTİ** — iki ayrı kusur çıktı: (1) açılış zemini ikon plakasından farklı tondaydı → kutu; (2) PNG ikonlar markanın TAKLİDİYDİ, artık `icon.svg`'den üretiliyor. Renk sapması derlemede yakalanıyor (negatif testle doğrulandı) |
| 6.2 | **Ses ölçümü: kendi test verimiz + kötü kaydı bulmak** | *"Kötü ses kayıtlarını nasıl bulabiliriz?"* · *"Test verisi kendimiz oluşturabilir miyiz ses için."* | **BİTTİ** — üç alet: `ses-olcu.js` (kötüyü bulur, tek cetvel, 8/8 sınav), `ses-kaydet.html` (telefonla dükkânda 40 cümle + ortam sesi, zip çıkarır), `ses-bozucu.js` (40 kayıttan kontrollü zor örnek). Kayıt Cemil'de: **A-M39** |
| 6.3 | **İz kaydı (log) — uzaktan görmek** | *"Log kaydı, nereye tıklanmış, ne yapılmış adım adım, hatalar, başarısızlıklar… mantığa aykırı işlemleri de görebilmeliyiz."* | **BİTTİ** — K-052 + `arastirma/IZ-KAYDI.md`; prototipe `00-iz.js` girdi (ekran · dokunuş · hata · şüphe), 47. denetim testi **sızıntıyı** ölçüyor, negatif doğrulama yapıldı. Kalan karar: A-M40, A-M41 |
| 6.4 | **Fatura → serviste kullanılan parça (kâr/zarar için)** | *"Fatura girişi olunca serviste kullanılan parçalara ekleyebilmeli… müşteri için değil kâr zarar muhasebe için."* · *"Faturasız çalışıyor da olabilirler."* | **BİTTİ** — `MALIYET-VE-KAR.md`; K-022 açılmıyor (fatura KESMEK ≠ GİRMEK). **Kusur bulundu:** parçacının fiyatı `fiyat`e yazılıyor, kâr şişik. 48. denetim testi *alış müşteri yüzeyinde görünüyor mu* diye ölçüyor. Kalan karar: A-M43/44/45 |
| 6.5 | **Taklit edilemezlik — alanı zapt etmek ve savunmak** | *"Kopyalanamaz bir özellik eklemek"* · *"Taklit edilemez olması için ne yapılabilir?…"* | **BİTTİ** — K-054 + `SAVUNMA.md`. Kopyalanamaz özellik yok; birikim var. **① düzeltme birikiminin mekanizması prototipe kondu** (`Iz.okuma` → `duzeltme`), 47. test koruyor. Kalan karar: A-M46, A-M47 |

**Sıra neden böyle:** 6.1 küçük ve somut, önden çıkar. 6.2 bir gün
önce açılan **A-M39**'u kapatır — ses tarafı bugün ölçülemiyor, ölçüm
aleti yarım. 6.3 mimariye dokunur ve 6.2 ile kesişir (kötü kaydı
bulmanın yolu izden geçiyor). 6.4 **bekleyen MUH kararına girdi**;
Cemil'in cevabını beklemeden hazırlanır. 6.5 strateji — ötekilerden
beslendiği için sona kondu.

⚠️ **6.4 ve 6.5 karar üretmez, karar HAZIRLAR.** MUH kapsamını lider
tek başına genişletmez (K-022); 6.4'ün çıktısı `MUHASEBE-KARARI.md`'ye
girdi olur. 6.5 bir ürün kararı değil, seçeneklerin bedeliyle birlikte
serilmesidir.


---

## Tur 5 · 14.09.2026 · son commit `cbb3302`

Cemil sırayı kendisi verdi: **(a) MUH kararı · (b) bekleyen açık
maddeler · (c) A-M27.** Üçü aynı cinsten değil — ilk ikisi **karar
hazırlığı** (kod yok), üçüncüsü kod.

> ⚠️ **İlk ikisi Cemil'in cevabını gerektiriyor.** Lider onları
> *cevaplanabilir* hâle getirir, cevaplamaz. Kuyruk buna rağmen
> durmaz: hazırlık biter, sıradaki işe geçilir.

| # | İş | Durum |
|---|---|---|
| 5.1 | **MUH kararı karara hazır** — `MUHASEBE-KARARI.md`: tek sayfa, üç seçenek, bedeli, açtığı işler, öneri | **BİTTİ** — B (kasa+gider) önerildi; üç seçenek "usta ne girer" diliyle yazıldı, onay sonrası sıra belirlendi |
| 5.2 | **Açık maddeler tazelensin** — kaynak KARARLAR.md'nin AÇIK tablosu; ikinci liste açılmaz (K-019.1). Kapananlar prototipte **doğrulanarak** işaretlenir | **BİTTİ** — 33 madde üç bloğa ayrıldı (kapsam · ekran · "böyle kalsın mı"); HANDOFF §5'teki **ikinci liste kaldırıldı**, oradan gelen A-14 ve A-M1 tabloya alındı |
| 5.3 | **A-M27** — ses plakayı yanlış duyunca *"bunu mu demek istedin"*; tek aday varsa öner, birden fazlaysa önerme. Yeni denetim testi + negatif test | **BİTTİ** — Cemil'in kaydettiği cümle (*"34 qbc 123…"*) artık öneri çıkarıyor; öneri bir SORU (dokunmadıkça hiçbir şey olmuyor), 44. test üç yönden ölçüyor |

**Tur 5 bitti — 14.09.2026.** Üç iş de kapandı; ikisi karar hazırlığıydı
ve **Cemil'in cevabını bekliyor** (MUH + AÇIK tablosunun üç bloğu).

---

## Tur 4 · 07.09.2026 · son commit `66c7870`

Cemil bir mesaj dizisinde **otuz beş madde** yazdı ve üç açık maddeyi
cevapladı. Aşağıda **triyaj** var: hepsi yapılacak, ama hepsi aynı
cinsten değil ve karıştırılırsa iş tıkanır.

### Açık maddelerin cevapları

| Kod | Cevap | Sonuç |
|---|---|---|
| **A-M13** | *"Evet, olabilir."* | Satır başına *"kim getirdi"* **geri gelecek** — K-033.5'te kaldırmıştım, gerekçem yanlışmış |
| **A-M14** | *"Anlamadım."* | **HÂLÂ AÇIK** — soru yeniden, sade dille soruldu (aşağıda) |
| **A-M15** | *"İsterse, evet."* | Parça beklendiğini müşteriye söylemek **ustanın seçimi** olacak; zorla değil |

---

### A · Hemen yapılabilir — tarifi net, karar gerekmiyor

| # | İş | Durum |
|---|---|---|
| A1 | *"İçeride"* → **"Serviste"** (her yerde, tek terim) | **BİTTİ** |
| A2 | Üstteki **sürüm/tanı şeridi** kalksın | **BİTTİ** |
| A3 | Footer'da gezinme tuşları — header'la **birlikte** çalışsın | **BİTTİ** |
| A4 | Gider ekle **+** düğmesi mikrofonla hizalı/yakın | **BİTTİ** |
| A5 | Servis kaydı ekle: **km ilk aşamada olmasın** | **BİTTİ** |
| A6 | Ses ekranı: *"şunları da diyebilirsin"* ve alttaki *"mikrofona dokun"* şeridi kalksın | **BİTTİ** |
| A7 | Kayıtsız araç metni yanlış → *"Araç kayıtlı değil. Ruhsatı çekebilirsin ya da bu aşamayı atlayabilirsin."* | **BİTTİ** |
| A8 | Kayıt detayı: sahibi ve iletişim **aynı font, büyük**; *"Sahibi"*den sonra boşluk | **BİTTİ** |
| A9 | *"Ne istiyor?"* → **"Açıklama"** | **BİTTİ** |
| A10 | *"Alt satıra ekle"* kalksın — textarea kendiliğinden kaydetsin | **BİTTİ** |
| A11 | **A-M13**: satır başına *"kim getirdi"* geri | **BİTTİ** |
| A12 | **A-M15**: parça beklendiğini müşteriye söylemek **seçenek** olsun | **BİTTİ** |
| A13 | Serviste **olmayan** araca *"işin özeti"* ve *"parça bekliyoruz"* şablonları çıkmasın | **BİTTİ** |
| A14 | *"Müşteri ne görecek"* → **popup**, yalnız mesaj görünsün | **BİTTİ** |
| A15 | Mesajda **telefon yoksa**: düğmeler pasif + *"telefon eklemelisiniz"* + telefonu yazıp/söyleyip ekleyebileceği **basit popup** | **BİTTİ** |

| A16 | Telefon katmanında **sesle giriş** — ses ekranına yeni bir niyet (`telefon`) ve çözümleyici işi gerektiriyor | AÇIK |

> ⚠️ **A15'i ilk triyajda ATLAMIŞIM.** Cemil'in listesinde vardı,
> kuyruğa geçmemiş. Otuz beş maddeyi elle ayırırken bir tanesi düştü —
> triyajın kendisi de gözden geçirilecek bir iş. A14 ile A15 **aynı
> bileşeni** istiyor (katman/popup), o yüzden birlikte yazılacaklar.

### B · Muhasebe — tarifi net ama BÜYÜK (kendi turunu hak ediyor)

| # | İş | Durum |
|---|---|---|
| B1 | Gider: son eklenen üstte · **en fazla 5** · toplam · *"Hepsini listele"* → ayrı sayfa | AÇIK |
| B2 | Gider listesi sayfası: **ay ve yıl bazında toplam** | AÇIK |
| B3 | Alacak: **elle eklenebilir** (servis kaydı olmadan) | AÇIK |
| B4 | Alacak: son 5 · tamamı ayrı sayfada · altta toplam | AÇIK |
| B5 | Alacak: son ay detaylı, **önceki aylar ay ay toplam** (detaysız) | AÇIK |
| B6 | Alacak **plakasız** olabilir — firmaya toplu alacak | AÇIK |
| B7 | Alacağa dokununca **tahsilat** yapılabilsin | AÇIK |
| B8 | Tahsilatta **hesap/POS** bilgisi (Akbank, Garanti, İş Bankası POS…) | AÇIK |

> ⚠️ **B kümesi muhasebeyi K-022'nin çizdiği sınırın dışına taşıyor.**
> K-022 kapsamı *"kasa + gider"* diye bağlamıştı; **cari hesap kapsam
> dışıydı.** *"Firmaya toplu alacak"* + *"ay ay bakiye"* + *"tahsilat
> kalemi"* = **cari hesabın kendisi.** Cemil kapsamı genişletebilir (tek
> o genişletebilir) ama **K-022 yeniden yazılmalı**, sessizce
> aşılmamalı.

### C · Karar bekleyen — varsayımla yazılır ama Cemil görmeli

| # | Soru | Varsayım |
|---|---|---|
| ~~C1~~ | **BİTTİ** — siler + *"Geri al"* (4,5 sn). Gerekçe değil çözüm değişti: onay **soru sorarak**, geri alma **cevabı geri vererek** korur. Kayıt tablosuna da taşındı (aynı jest iki anlama gelmesin — A-M19). Üç ölü sınıf denetimce yakalandı ve silindi (K-044.1) |
| ~~C2~~ | **KARAR VERİLDİ, KOD BEKLİYOR** — gösterilir ama **ayrı**: çek kasaya girmemiş paradır, tahsilata katılırsa muhasebe yalan söyler. Uygulaması `tahsilat kalemi`ni gerektiriyor → **B kümesi**, onay bekliyor (K-044.2) |
| ~~C3~~ | **KARAR VERİLDİ, BİLEREK YAZILMADI** — arşiv evet, kontrol kural tabanlı. Ama saklanacak yeri belli olmayan veri, gösterilmeyen alan kadar ölü; ayrıca **ne kadar süre saklanacağı** yasal bir soru → D2 araştırmasından sonra (K-044.3) |

### D · Araştırma — kod değil, belge

| # | Konu | Çıktı |
|---|---|---|
| ~~D1~~ | **BİTTİ** → `arastirma/SANAL-POS.md`. Soru *"yapabilir miyiz"* değil **"parayı kim tutuyor"**: parayı biz tutarsak ödeme kuruluşu oluruz, izinsizi **1–3 yıl hapis** (6493 m.28). Dükkânın kendi POS'una yönlendirme yasal (m.12/2-ğ) ama **backend ister**. Gerçek bitişiği **alacak** (K-045.1) |
| ~~D2~~ | **BİTTİ** → `arastirma/TOPLU-MESAJ.md`. Bakım hatırlatması muafiyeti **kampanyayı kapsamıyor**; üstelik toplu gönderim **WhatsApp A yolunu çökertiyor** — ustanın numarası banlanırsa çalışan **altı iş** birden durur. C3'ün saklama sorusu da cevaplandı: **üç yıl**, ispat yükü gönderende (K-045.2, K-045.3) |
| ~~D3~~ | **BİTTİ** → `arastirma/DISA-AKTARMA.md`. **Kütüphane gerekmiyor:** PDF = `window.print()` (0 KB, Türkçe sorunsuz — jsPDF'in varsayılan fontları UTF-8 desteklemiyor), Excel = CSV ama **BOM + `;` ayraç + ham rakam** olmadan Türkçe Excel'de bozuk açılıyor (K-045.4) |

### E · Faz dışı — yazılmaz, konuşulur

| # | İş | Neden burada |
|---|---|---|
| E1 | Servis kaydına **resim/video**, müşteri uygulamadan görsün | Depolama, boyut, gizlilik, müşteri uygulaması (T-21). **Backend gerektirir** |
| E2 | **Toplu mesaj + yapay zekâ ile düzeltme** | Yeni bir ürün alanı; D2 bitmeden yazılmaz |
| E3 | **Gelir-gider tablosu** | Cemil *"sonrasında"* dedi — B bitince |
| E4 | Sesli kaydı **AI ile düzeltme** (tek müşteri + toplu) | E2 ile aynı altyapı |
| E5 | Kayıt detayında **sağ üstte mikrofonla** ödeme/tahsilat/parça | Sesli işlem kapsamı genişliyor; K-016 (ses tek yol olamaz) korunmalı |

---

> **Sıra önerisi:** A → C → B → D. E, D bitmeden açılmaz.
> A kümesi tek turda biter ve prototipi Cemil'in gördüğü hâle getirir;
> B kendi turunu ister ve **önce K-022 yeniden yazılmalı**.

---

## Tur 3 · 06.09.2026 · son commit `906c553`

Önceki iki kuyruk (04.09 ve 05.09) **bitti**, aşağıda arşivde.
Bu kuyruk **K-033 turundan sonra açık kalan borçları** kapatıyor.
Sırası **riske göre**: en üstteki, yanlışsa en çok şeyi bozan.

| # | İş | Durum | Sonuç |
|---|---|---|---|
| 1 | **Veri modeli uzlaştırması** — `parcaBekliyor` ve `turElle` modelde YOK, model bunları açıkça yasaklıyor | **BİTTİ** | Belge yanlıştı, alan değil: türetilen `fiyatiBekleyen` *fiyatı gelmemiş* demek, *parça yolda* demek değil. **Bayrak ≠ durum** sınırı yazıldı (K-034) — ayrım: *bu alan dolduğunda kullanıcının yapabilecekleri değişiyor mu?* Ayrıca sözlükte üç uzlaşmazlık düzeldi (`Plate`/`PlateNo`, türün üçüncü değeri, eksik `ReminderSent`) |
| 2 | **Ölü uç turu** — bu turda değişen 8 ekran elle işletilecek | **BİTTİ** | **İki gerçek bulgu.** (a) **Kol B kopmuş**: parça-sor `sorulan`ı yazıyor, okuyan kalmamış, `SelectedReplyId` hiç dolmuyor — üstelik balon *"cevaplar kayda düşecek"* diye **olmayan bir şey vaat ediyordu**. Çipler geri kondu, balon dürüstleşti. (b) **Sesin çözdüğü tür çöpe gidiyordu**: `RUHSAT_ONDOLGU.tur` ölü veriydi, tür metinden yeniden tahmin ediliyordu. Kalan 40 düğmenin hepsi bir şey yapıyor |
| 3 | **Dokunuş sayımı** — ana yol değişti, HANDOFF'taki `12` artık yalan olabilir | **BİTTİ** | Yalanmış — ama beklediğim yönde değil: ana yol **12 → 13** (koca bir soru kalkmasına rağmen **arttı**). Tür −1, ruhsat sekmesi +1, teslim sonrası kapatma +1. Yeni **ruhsatsız yol 8**. Kayıtlı araç 8'de kaldı. **A-8 ölçümle kapandı**, A-M14 açıldı (K-035) |
| 4 | **İki "bekliyor" ayrımı** — `fiyatiBekleyen` ≠ `parcaBekliyor`, her ekranda doğru olan mı yazıyor | **BİTTİ** | Şüphem doğruydu: kart ikisini `\|\|` ile tek rozete bindiriyordu, **fiyatı gelmemiş satır *"parça bekliyor"* diye okunuyordu**. İki ayrı rozet oldu; teslim ekranı da bayraktan haberdar edildi. **Beklemediğim bulgu:** `kayitSatiriHtml`in tam hâli (60 satır) bir gündür **ulaşılamazdı** — silindi (K-036) |
| 5 | **Muhasebe hâlâ doğru mu** — "hepsi müşteriden" anahtarı fiyat→alış taşıyor, iş başına farkını bozuyor olabilir | **BİTTİ** | **Muhasebe doğruymuş** — müşteri parçası alış toplamına hiç girmiyordu (12.970−7.160=5.810 rakamla tutturuldu). Ama anahtar **satış fiyatını tek dokunuşta yok ediyordu**: 5.810→5.310→**3.710**. Kural artık silerek değil **toplamda eleyerek** işliyor; gidiş-dönüş tam (K-037) |
| 6 | **Müşteri özeti** — yeni yapıya uydu mu; "parça bekliyor" müşteriye söylenmeli mi | **BİTTİ** | Üç yerden geri kalmış: çok satırlı şikâyet **tek satıra yapışıyordu**, yarım satır müşteriye *"Parça —"* diye gidiyordu, ve sayfa parça beklendiğini söylemiyorken **mesaj şablonu söylüyordu** — müşteri iki ayrı hikâye görüyordu. Üçü de düzeldi; A-M15 açıldı (K-038) |
| 7 | **Açık tema turu** — bu turun bileşenleri çoğunlukla koyu temada görüldü | **BİTTİ** | **Varsayımım tersmiş**: `prefers-color-scheme` açık — bütün tur açık temadaydı, ölçülmemiş olan **koyu**. Ölçüldü: üç tema hâlinde de 0 bulgu, kontrastlar AA üstü (en düşük **5.00**). Kuyruğa yazdığım *"denetimin kontrast testi"* de yokmuş. Asıl bulgu **ölçüm aracımda**: `color(srgb …)` okunamıyordu ve tema geçişinin ara karesi okunuyordu (K-039) |
| 8 | **İki yeni denetim testi** — bugün hiçbir testin yakalamadığı iki hata sınıfı | **BİTTİ** | 36 → **38 test**. 38'in ilk tasarımı **boştu** — yazmadan önce dünkü iki hataya karşı denedim, ikisini de yakalamıyordu. Kök sebebe inince asıl mesele çıktı: `ciz` hem *"açıldım"* hem *"yeniden çiziliyorum"* demek. Çekirdeğe **`ac` kancası** eklendi, dört ekranın açılış işi taşındı — üçü latent hataydı. 37 iki kapalı düğme buldu, ikisi de **konuşan düğmeye** çevrildi (K-040) |
| 9 | **320 px elle turu** — denetim 0 diyor; K-025 sessiz kırpmayı da 0 demişti | **BİTTİ** | **Göz gördü, 38 test görmedi:** tabloda *"Motor yağ", "Bakım işç"* — `<input>` sessizce kesiyor (taşma 5-23 px). Dar ekranda yer açıldı (ad alanı 65 → **108 px**), **39. test** eklendi. Kovalarken iki denetim kusuru daha çıktı: DOM geri yüklenmiyordu (akordiyon) ve **`load` koşusuna hiç bakmamışım** — orada bir yanlış pozitif duruyordu (K-041) |
| 10 | **Belge tazeleme** — EKRANLAR · IS-AKISI · SOZLUK · HANDOFF sayıları | **BİTTİ** | En geride kalan **EKRANLAR.md**'ydi: ruhsat ekranını hâlâ *"iki büyük düğme: TAMİRAT · BAKIM"* diye, kayıt ekranını *"fotoğraftan liste okuma YOK"* diye anlatıyordu — ikisi de tersine dönmüştü. Ekran sayısı 13 yerine 12 yazıyordu (iş başına eksikti). IS-AKISI'nde Kol E hâlâ *"Onay iste"*ydi (04.09'da adı değişmişti). Dokunuş tablosu 03.09'dan kalmaydı |

---

## 1 · Veri modeli uzlaştırması

**Neden ilk sırada:** `VERI-MODELI.md §2` şunu yazıyor —

> *"`Status` YOK… **'Parçası bekleniyor' gibi bilgiler bu kaydın
> verisinden türetilir, saklanmaz.**"*

Dün (K-033.5) ben o kaydın üstüne **`parcaBekliyor` diye saklanan bir
alan** koydum. Ayrıca `turElle` diye ikinci bir saklanan alan var.
**CLAUDE.md açık:** *"Alanlar VERI-MODELI.md ile BİREBİR — modelde
olmayan alan buraya da girmez."* Şu anda prototip belgeyi çiğniyor.

İki yoldan biri seçilecek ve **gerekçesi yazılacak**:

* **(a) Alanlar kalır, model güncellenir.** Savunması: ikisi de
  türetilemez. *"Parça yolda"* ustanın **bildirdiği bir gerçek** — veride
  onu söyleyen hiçbir şey yok (fiyatın boş olması başka şey demek,
  K-033.5). `turElle` ise ustanın **kararı**; karar türetilemez, yoksa
  bir sonraki tahmin onu ezer (K-033.12).
* **(b) Alanlar kalkar.** O zaman K-033.5 ve K-033.12 geri alınır ve
  Cemil'in *"parça bekliyor alanı olmalı"* maddesi karşılıksız kalır.

**Önerim (a)** — ama karar `K-004`'ün sınırını genişlettiği için
gerekçesi KARARLAR'a yazılmalı: **bayrak ≠ durum.** `parcaBekliyor`
akışı değiştirmiyor, hiçbir ekranı kilitlemiyor, teslimi engellemiyor;
`DeliveredAt` hâlâ tek durum alanı.

**Bitti sayılır:** `VERI-MODELI.md` ve `SOZLUK.md` alanları içeriyor,
K-004'ün notu *"bayrak durum değildir"* diye genişletilmiş, karar
KARARLAR'a düşmüş, `02-veri.js`'in tohum verisi belgeyle birebir.

## 2 · Ölü uç turu — bu turda değişen sekiz ekran

**Neden:** 27. test *"ekranda hiç eylem yok"* der; söylemediği şey
**eylem VAR ama işi ilerletmiyor.** 04.09'da bu tur üç yalan buldu
(K-021), 06.09'da elle işletme dört hata daha buldu ki **hiçbirini
36 test yakalamamıştı.**

**Hangi ekranlar:** `ruhsat · kayit · teslim · arac · kayitlar ·
musteriler · mesaj · ozet`.

**Nasıl:** her düğmeye/çipe/alana **gerçekten dokun**, nereye
gittiğini ve **veride ne değiştiğini** yaz. Şu üç soruyu her düğmeye
sor:

1. Düğmenin **adı**, yaptığı işi söylüyor mu?
2. Balonu **olan** bir şeyi mi söylüyor, yoksa *"gönderildi"* gibi
   **olmayan** bir şeyi mi? (Bu turda bir tane daha çıktı — K-033.6.)
3. Basınca **hiçbir şey olmuyorsa**, ekran nedenini söylüyor mu?

**Özellikle bak:** `parca-sor` ekranı satır ekliyor mu, hâlâ eski
`.kalem` dilinde ve yeni tabloyla tutarlı mı; `sesUygula`'nın açtığı
kayıt yeni alanları taşıyor mu; `RUHSAT_ONDOLGU.tur` artık kullanılmıyor
(ruhsat ekranı türü metinden okuyor) — **ölü veri mi kaldı?**

**⚠️ ZATEN BİR BULGU VAR (1. iş sırasında çıktı):** `parca-sor` ekranı
hâlâ `s.sorulan` yazıyor ama **onu okuyan kod kalmadı** — dünkü kayıt
ekranı yeniden yazımında *"Cevap kimden geldi?"* çipleri düştü. Yani
`s.kaynak` (`SelectedReplyId`, **Kol B**) artık hiç doldurulmuyor: usta
iki parçacıya sorup iki cevap aldığında **hangisini kullandığını
yazacak yer yok.** Modelde duran, akışta karşılığı olan bir alan
sessizce koptu. Bu turda geri konacak.

**Bitti sayılır:** bulgular düzeltildi; yeni bir hata sınıfı çıktıysa
8. işe test önerisi olarak yazıldı.

## 3 · Dokunuş sayımı yeniden ölçülür

**Neden:** HANDOFF *"ana yol 12 dokunuş"* diyor ve o sayı **05.09'da**
ölçüldü. O gün ana yolda bir **tür seçimi** vardı; kalktı. Yerine
kayıtsız araçta bir **ruhsat teklifi** geldi. Sayı değişti ve
**değiştiğini bilmiyoruz.**

**Kural (v1'den):** sayı ölçülmeden yazılmaz. *"Herhalde azalmıştır"*
bir ölçüm değildir.

**Nasıl:** beş yol da baştan sona **yürütülecek** ve her dokunuş
sayılacak: ana yol (yeni araç) · kısa yol · kayıtlı araç yolu · sesli
yol · müşteriye özet. Yeni bir yol daha var: **ruhsatı çekerek** açılan
kayıt.

**Bitti sayılır:** HANDOFF tablosundaki sayılar yenilendi; ekran
bütçesi (A-2) aşıldıysa **söylendi**, sessizce geçilmedi.

## 4 · İki "bekliyor" birbirine karışıyor mu

**Neden:** artık iki ayrı şey var ve **ikisi de "bekliyor" diyor**:

| | Ne demek | Nereden gelir |
|---|---|---|
| `fiyatiBekleyen(k)` | **fiyatı** gelmemiş satır var | türetilir |
| `k.parcaBekliyor` | **parça** yolda | usta işaretler |

Bunlar ayrı gerçekler: parça elde olup fiyatı belirsiz olabilir, ya da
fiyatı belli olup parça yolda olabilir. **Ekranların hangisini
söylediği tek tek kontrol edilecek:** ana ekran · servis kayıtları
kartı · araç geçmişi · teslim ekranı · özet · muhasebe.

**Şüphelendiğim yer:** `kayitKartiHtml` şu an ikisini **tek rozete**
bindiriyor (`k.parcaBekliyor || fiyatiBekleyen(k)` → *"parça bekliyor"*).
Bu, fiyatı girilmemiş bir satırı *"parça yolda"* diye gösteriyor
olabilir — **yanlış rakam göstermek gibi, yanlış durum göstermek de
göstermemekten kötüdür** (K-013'ün kuralı).

**Bitti sayılır:** her ekranda hangisinin yazdığı belli; ikisi tek
rozete biniyorsa ya ayrıldı ya da **neden bindiği yazıldı**.

## 5 · Muhasebe hâlâ doğruyu mu söylüyor

**Neden:** *"Parçaları müşteri getirdi"* anahtarı, satırın `fiyat`ını
**`alis`e taşıyor** (K-005'in kuralı: rakam kaybolmasın). Ama
`isKari()` iş başına farkını `alinan − parça alışı` diye hesaplıyor ve
`parcaAlisiToplami()` bütün `alis` toplamını alıyor.

**Soru:** müşteri kendi parçasını getirdiğinde o parça için **biz
ödeme yapmadık** — ama `alis` dolu. Muhasebe o parayı **ödenmiş** gibi
mi sayıyor? Öyleyse iş başına farkı ve parça alışı toplamı **yanlış**.

**Nasıl:** rakamla ölç. Bugünkü tohum veride 2026 iş başına
`12.970 − 7.160 = 5.810` idi. *"Hepsi müşteriden"* anahtarını bir kayıtta
aç, rakamları yeniden oku, **elle hesapla ve karşılaştır.**

**Bitti sayılır:** ya rakam doğru çıktı (ve nedeni yazıldı), ya
`parcaAlislari()` *"müşteri getirdiyse alış sayılmaz"* diye düzeltildi
ve **eski/yeni rakam commit mesajına yazıldı.**

## 6 · Müşteri özeti yeni yapıya uydu mu

**Neden:** `ozet` ekranı müşterinin gördüğü sayfa (K-023) ve bu turda
kaydın yapısı değişti. Kontrol: *"müşteri getirdi"* rozeti hâlâ doğru
çiziliyor mu, boş satır ne gösteriyor, tür değişince başlık değişiyor mu.

**Karar gerektiren yer:** **parça beklediği müşteriye söylensin mi?**
Söylenirse müşteri aramaz — tamircinin şikâyet ettiği telefon trafiği
azalır. Söylenmezse *"neden bu kadar sürdü"* sorusu kalır.
**Varsayım: söylenir**, çünkü mesaj şablonu zaten söylüyor ve sayfanın
mesajla çelişmesi daha kötü. *Cemil onayına açık.*

**Bitti sayılır:** sayfa yeni yapıyla tutarlı; varsayım işaretlendi.

## 7 · Açık tema turu

**Neden:** bu turda yazılan bileşenlerin çoğunu **koyu temada** gördüm:
tablo içindeki çerçevesiz alanlar, `.ms-kart`, `.ak-tutar`, balonun yeni
hâli, `.k-alis`. 03.09'da tam bu sebeple bir hata çıkmıştı — *"iki
temayı da çizmek gerekiyor, birini çizip ötekine bakmamak değil."*

**Nasıl:** `Tema` üç hâlde de (sistem · açık · koyu) sekiz ekran
gezilecek; özellikle **kontrast** ve **çerçevesiz alanların görünürlüğü**.
Denetimin kontrast testi rakamı verir, ama *"bu alan buraya yazılır"*
duygusunu vermez — ekran görüntüsü alınacak.

**Bitti sayılır:** her iki temada ekran görüntüsü alındı, düzeltmeler
token'la yapıldı (**ham hex YOK**).

## 8 · İki yeni denetim testi

Bugün **dört hata** elle işletince çıktı ve 36 test hepsine *"0 bulgu"*
dedi. İkisi otomatikleştirilebilir:

**Test 37 — SESSIZ-KAPALI-DUGME.** Her ekranda `button[disabled]` ara.
Kapalı bir düğmenin **yakınında** nedenini söyleyen bir şey (bir
`.notice`, `.plate-hint` ya da `aria-describedby`) yoksa bulgu.
*Gerekçe:* K-033.3 — kapalı düğme sessizdir, Cemil bastı ve hiçbir şey
olmadı.

**Test 38 — TAZELEMEDE-KAYBOLAN-SECIM.** Her ekranı çiz, `aria-pressed="true"`
olan öğeleri say, `Ekran.tazele()` çağır, yeniden say. Sayı düşüyorsa
bulgu.
*Gerekçe:* K-033.12 — `ciz()` içinde sıfırlanan bir ekran değişkeni,
her tazelemede ustanın seçimini siliyordu. Aynı hatayı **bir turda iki
kez** yaptım.

**Test 40 — adayı (K-039.4'ten):** KONTRAST. Bu turda elle yapılan
WCAG hesabı otomatikleştirilebilir. İki şartı var ve ikisi de bugün elle
öğrenildi: `color(srgb …)` biçimini okumalı, ve **tema geçişini
beklemeli** (~700 ms). Denetim eşzamanlı koştuğu için ikincisi zor;
zorlanırsa yazılmaz ve sebebi buraya düşülür.

**Test 39 — adayı (K-036.3'ten):** bir ortak nesnenin hiç geçilmeyen
**seçeneği** ölüdür (`kayitSatiriHtml(k, {sade})` — `sade:false` bir
gündür hiç geçilmemişti ve 60 satır ulaşılamaz kalmıştı). Statik olarak
ölçmek prototipin tek dosyalık yapısında güvenilmez; **yazılabilirliği
8. işte tartılacak**, zorlanırsa yazılmaz ve sebebi buraya düşülür.

**Her ikisi de NEGATİF TESTLE doğrulanacak:** hata kasten geri konacak,
test bulgu verecek, sonra geri alınacak. *Delikli denetim, denetimsiz
olmaktan kötüdür.*

⚠️ **Denetimin kendisini kirletme (K-024.1):** test 38 `Ekran.tazele()`
çağırıyor; çağırdığı ekranın durumunu **bozmadan** bırakmalı.

**Bitti sayılır:** iki test eklendi, negatif testle doğrulandı, üç
genişlikte 0 bulgu.

## 9 · 320 px elle turu

**Neden:** denetim üç genişlikte de 0 diyor. K-025'te de 0 diyordu ve
düğme metni **sessizce kırpılıyordu** — kendi `overflow:hidden`'ım
taşma testinden gizliyordu.

**Nasıl:** 320 px'te sekiz ekranın **ekran görüntüsü** alınacak ve
**gözle** okunacak. Özellikle: kayıt tablosunun iki sütunu (rozet + ad
+ tutar aynı satıra sığıyor mu), `.ms-kart` başlığı (uzun firma adı +
sayaç), ruhsat akordiyonunun ikili alanları, teslim sonrası panel.

**Bitti sayılır:** her ekranın 320 px görüntüsü alındı; kırpılan ya da
üst üste binen hiçbir şey yok.

## 10 · Belge tazeleme

`EKRANLAR.md` (kayıt ve ruhsat ekranlarının tarifi değişti) ·
`IS-AKISI.md` (akıştan **tür seçimi adımı** kalktı) · `SOZLUK.md`
(yeni alanlar ve terimler) · `HANDOFF.md` (test sayısı, dokunuş
sayıları, karar aralığı).

**Bitti sayılır:** belgelerde prototiple çelişen tek cümle kalmadı.

---

## Kuyruk bitti — 06.09.2026

**On işin onu da BİTTİ.**

| | |
|---|---|
| Karar | **K-034 … K-041** (sekiz karar) |
| Denetim | 36 → **39 test** (üçü bu turda yaşanmış hatalardan doğdu, üçü de negatif testle doğrulandı) |
| Derleme denetimi | 5 → **8 kontrol** |
| Ölçüm | derleme temiz · 14 ekran · 39 test · `load` koşusu ve elle koşu **0 bulgu** · 320 / 375 / 430 px · açık ve koyu tema |

**Bu turun en pahalı dersi ölçüm aracıyla ilgili.** On işin altısında
ilk *"bulgu"* diye görünen şey **ölçüm hatası** çıktı: kopmuş DOM
düğümüne tıklamak · `innerHTML.length` imzası · paylaşılan
`muhDonemSecili` kirlenmesi · tema geçişinin ara karesi ·
`color(srgb …)` ayrıştırılamaması · `textContent`in gizli metni
vermesi. Hepsi `HANDOFF.md §4`'teki tabloya yazıldı.

İkinci ders: **belgede duran bir ders, uygulanmamışsa öğrenilmemiştir.**
K-041.3'te `.plate-lg` için yıllar önce yazılmış bir not, aynı hatayı
başka bir testte yapmamı engellemedi.

**Cemil'i bekleyen yeni açık maddeler:** A-M13 (tek parçayı ayrı
işaretleme), A-M14 (teslimden sonraki dokunuş), A-M15 (parça beklendiği
müşteriye söylensin mi). Hepsi `HANDOFF.md §5`'te, hepsinin yanında bir
öneri var ve hiçbiri işi durdurmadı.

---

## Arşiv

### Tur 2 · 05.09.2026 — yedi iş, yedisi de BİTTİ

Ölü uç turu (3 yalan) · A-M2 · muhasebe kapsamı (K-022) · müşteri
özeti (13. ekran) · sesli işlem kapsam denetimi (denetimin kendisi
bozuktu) · üç genişlikte ölçüm (320'de sessiz kırpma) · belge tazeleme
(ana yol 13 değil **12** çıktı). Sonuç: K-019…K-025, denetim 27 → 32 test.

### Tur 1 · 04.09.2026

`4a6dfe5` öncesi; ayrıntısı git geçmişinde.
