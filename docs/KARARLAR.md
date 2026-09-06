# TAMIRON — KARARLAR (KARARLAR.md)

> Verilen kararların gerekçeleri. Aynı tartışma iki kez açılmasın. Yeni karar **en üste** eklenir.
>
> **Bu liste 03.09.2026'da SIFIRDAN başladı.** v1'in ürün/akış kararları bilinçli olarak devralınmadı — tamircinin geri bildirimi (`arastirma/TAMIRCI-GERIBILDIRIM.md`) akışı yeniden kuruyor ve eski kararların gündemde durması yeni tartışmayı kirletiyordu.
>
> **Devralınan mimari kararlar `MIMARI.md`'dedir** ve burada tekrarlanmaz: teknoloji yığını, üç uygulama ayrımı, tenant/yetki altyapısı, soft-delete, katalog hibrit yapısı, Türkçe UI / İngilizce kod, tokens tek kaynak, tasarım-önce. Bunlar akıştan bağımsızdır — değişecekse `MIMARI.md` açılır.
>
> v1 arşivi: `github.com/DevCemilSaglam/Tamiron-v1` (195 commit; her kararın gerekçesi commit mesajında).

---

## K-031 · Görünmeyen kaydırma, kırpmanın kardeşidir (05.09.2026)

> *"Gider ve alacak tabloları telefonun ekranına sığmıyor, sağa kaydırmak
> gerekiyor. **Kullanıcı sağa kaydıracağını bilmez.**"*

### 31.1 · ⚠️ Bu, K-025'te verdiğim kararı DÜZELTİYOR

K-025'te *"kesilen metin, taşan metinden tehlikelidir"* deyip tabloya
`overflow-x: auto` koymuştum. Kırpmaya göre doğruydu — ama **üçüncü bir
seçenek** vardı ve o daha doğru: **sığdırmak.**

| | Kullanıcı ne görür | Eksik olduğunu bilir mi |
|---|---|---|
| Kırpma | yarım kelime | belki |
| **Görünmeyen kaydırma** | **hiçbir şey** | **hayır** |
| Sığdırma | hepsi | — |

**Görünmeyen kaydırma, kırpmanın kardeşidir.** İkisinde de kullanıcı bir
şeyin eksik olduğunu bilmez; fark, kırpmada yarım kelime görmesi,
kaydırmada **hiç görmemesi.**

### 31.2 · Nasıl sığdı: sütun sayısı azaldı

**İkincil bilgi kendi sütununu istemiyor** — ana metnin altında küçük yazıyor
(`.t-alt`).

| Tablo | Önce | Sonra |
|---|---|---|
| Gider | Tarih · Açıklama · Tutar · sil | **Açıklama** *(altında tarih)* · Tutar · sil |
| Alacak | Kim · Araç · Teslim · Tutar | **Müşteri** *(altında plaka · tarih)* · Tutar |
| İş başına | Araç · İş · Teslim · Alınan · Parça · Fark | **İş** *(altında tür · marka · tarih · alınan · parça)* · **Fark** |

**Ölçüldü (375 px):** üçü de **305 = 305** — taşma sıfır.
Negatif test eski davranışı geri getirdiğinde: gider **36 px**, alacak
**125 px**, iş başına **317 px** kayıyordu.

`table-layout: fixed` + `overflow-wrap: anywhere` metni sarıyor; sayı sütunu
`nowrap` ve sabit 96 px — rakam sarmaz, *"2.4"* alt satıra *"00"* olmaz.

### 31.3 · 36. test: GIZLI-KAYDIRMA

Ekran içinde yatay kaydırılabilen **ve gerçekten taşan** bir kutu varsa bulgu.
Dikey kaydırma kapsam dışı: **telefonda aşağı kaydırmak öğrenilmiş
davranıştır, sağa kaydırmak değil.**

### 31.4 · Öteki dört madde

| | |
|---|---|
| Gider başlığı | *"Ne için"* → **Açıklama** |
| Alacak başlığı | *"Kim"* → **Müşteri** — ustanın kelimesi |
| *"dönemden bağımsız"* şerhi | kalktı *(ve CSS'te ölü kalan `.lbl-not` denetimin `OLU-SINIF` testiyle yakalanıp silindi)* |
| Gider ekle | mikrofonun **solunda, aynı yuvarlak içinde `+`** |

**`.mic-round` → `.daire-btn`.** Aynı biçim artık iki iş yapıyor; sınıf adının
*"mic"* demesi yanlış olurdu. Bileşen **yuvarlak eylem düğmesi**, içindeki
ikon ne iş yaptığını söylüyor.

**Neden aynı biçim:** elle giriş ve sesle giriş aynı işin iki yolu. Biri düz
düğme, öteki yuvarlak dururken biri *"asıl"*, öteki *"yedek"* gibi okunuyordu
— oysa elle giriş yedek değil: mikrofonsuz tarayıcıda **tek yol** o (K-021.1).

### 31.5 · Giderlere araç işi yazılmaz

> *"Giderlerde yedek parça olmamalı. Yedek parçalar işten alınarak düşülüyor.
> Tabi kullanıcı isterse ekleyebilir."*

Parçanın alış fiyatı **zaten servis satırında** duruyor ve iş başına farkta
düşülüyor. Buraya elle yazmak **aynı parayı iki kez saymaktır.**

**Tohum veriden çıkarıldı.** *"Öz Yedek Parça — triger seti 4.200 ₺"* örnek
veride duruyordu ve ekran onu gider sayıyordu: **örneğin kendisi yanlış
alışkanlık öğretiyordu.** Yerine *"Dükkân kirası"* kondu.

**Yasak değil, uyarı** (Cemil: *"kullanıcı isterse ekleyebilir"*): ekran
*"buraya araç işleri dışındaki giderler yazılır… ayrıca yazarsan iki kez
sayılır"* diyor.

---

## K-030 · `inputmode` bir kısıt değil, bir öneridir (05.09.2026)

> *"Tutar alanına harf giremeyiz. Inputların veri doğruluğundan emin ol."*

### 30.1 · Ölçüldü: üç tutar alanı da harf kabul ediyordu

`inputmode="numeric"` yalnız **telefonun hangi klavyeyi açacağını söyler.**
Masaüstünde, **yapıştırmada** ve sesle girişte harf rahatça giriyordu.

| Alan | Yazılan | Kalan *(önce)* |
|---|---|---|
| Servis satırı tutarı | `abc12x3,4.5 TL` | `abc12x3,4.5 TL` |
| Gider tutarı | `abc12x3,4.5 TL` | `abc12x3,4.5 TL` |

**Neden sessiz ve pahalı:** okuma tarafı **zaten temizdi**
(`replace(/[^0-9]/g,'')`), yani **veri bozulmuyordu** — ama ustaya
**gösterilen** şey bozuluyordu. `abc12x3` yazan usta ekranda onu görüyor,
kayda **12.345 ₺** düşüyordu. Yanlış rakam göstermek, rakam göstermemekten
kötüdür (K-013'ün aynı kuralı, başka bir yerde).

### 30.2 · Tek delegasyon, `data-giris` sözleşmesi

Her alan kendi temizleyicisini yazsaydı biri unutulurdu — **nitekim üçünde de
yoktu.** `data-giris` bir sözleşme: alanı işaretlemek yeter, davranış
çekirdekten gelir.

| Tip | Kural | Örnek |
|---|---|---|
| `tutar` | tam sayı, binlik nokta. **Kuruş yok** — tamirci *"1900"* der | `abc12x3,4.5 TL` → **12.345** |
| `km` | tam sayı, binlik nokta | `142500xy` → **142.500** |
| `yil` | dört hane | `20a18999` → **2018** |
| `tel` | 11 hane, gruplanır. Ülke kodu **kabul edilmiyor** — tek biçim tutmak, iki biçimi karşılaştırmaktan kolay | `0532abc2153000999` → **0532 215 30 00** |
| `plaka` | v1'in biçimleyicisi, **çekirdeğe taşındı** | `34abc123` → **34 ABC 123** |

**Sıfır geçerli:** müşteri parçası 0 ₺ durur (S2, K-005).

**İmleç korunuyor.** Değeri yeniden yazmak imleci sona atar; usta ortadan
düzeltmek isteyince her tuşta sona fırlıyordu. Solunda kaç **rakam** varsa o
kadar rakamdan sonraya konuluyor — binlik noktası eklense bile aynı yerde.

**Yakalama evresinde dinleniyor** (`addEventListener(..., true)`): ekranların
kendi `input` dinleyicileri **temizlenmiş** değeri görüyor, hamı değil.

### 30.3 · Yazarken azarlanmaz, yazıldıktan sonra söylenir

Yıl ve telefon **aralığı** yazarken denetlenmiyor: *"20"* yazan biri henüz
2018'e gidiyordur. Alan **dolduğunda** söyleniyor ve *"Kaydı aç"* kapanıyor.

**Telefon eksik olabilir ama yanlış olamaz** — yanlış numaraya giden bir
hatırlatma, gitmeyen hatırlatmadan kötüdür.

### 30.4 · 35. test ve iki kez düzeltilen ölçüt

Test **iki şeyi** ölçüyor:
- **(a)** işaretli alana biçim dışı karakter girip kalıyor mu
- **(b)** biçimli görünen *(sayısal klavye ya da `.mono`)* ama **işaretsiz**
  alan var mı — yani koruması **unutulmuş** alan

(b) olmadan test yalnız bugünkü alanları korurdu; yarın eklenen alan yine
korumasız girer ve kimse görmezdi.

**Ölçüt iki kez yanlıştı, ikisini de negatif test buldu:**

1. *"Harf var mı"* diye soruyordu — **plaka harf içerir** (34 ABC 123) ve test
   onu bulgu sandı. Her tipin kendi **kabul deseni** var; ölçülen şey harf
   değil, **tipin dışına çıkan karakter**.
2. `if (!el.offsetHeight) return;` vardı — ruhsattaki yıl/km/telefon **ancak
   kayıtsız plaka yazılınca** açılıyor, o yüzden test onları **hiç
   görmüyordu**: korumaları silinse bile *"0 bulgu"* derdi. **Bir alanın
   koruması, o an görünür olup olmamasından bağımsızdır.**

**Negatif doğrulama:** bütün `data-giris` öznitelikleri silindi → **4 bulgu**
(üçü gizli alanlardan), geri konunca **0**.

---

## K-029 · Boşluk kuralının açığı: SARMALAYICI KAP (05.09.2026)

> *"Elementleri neden birbirine yapıştırıyorsun? Bunu genel olarak düzelt."*
> — Cemil, **üçüncü kez** ve bu sefer sebep başkaydı.

### 29.1 · Sebep: kabın gap'i torununa işlemez

K-026.6'da `.screen`, K-028.3'te `.card` flex + gap oldu ve kural yazıldı:
**boşluğu kap verir.** Ama aynı turlarda ekran gövdesine **düz `<div>`
sarmalayıcılar** eklendi — sekmeler, kipler, gövdeler. Onların **kendi gap'i
yoktu.**

`.screen`'in gap'i sarmalayıcıya uygulanıyor, **içindeki kardeşlere
uygulanmıyordu.** Ölçüldü:

```
muhBasitGovde: cip-grup -> card           = 0 px
muhBasitGovde: card -> section-label      = 0 px
muhBasitGovde: btn-row -> DIV             = 0 px
ruhsatGovde:   section-label -> cip-grup  = 0 px
sekmePlaka:    card -> notice             = 0 px
```

**Sekme eklemek ritmi sessizce kırdı.** Kural doğruydu, ama kuralın
uygulanacağı yer bir turda **çoğaldı** ve yeni kaplar kuralın dışında kaldı.

**Çözüm bir sınıf: `.yigin`** (flex sütun, gap 18). Sarmalayıcıya sınıf
vermek, kuralı görünür ve **denetlenebilir** yapıyor — anonim bir `<div>`
hiçbir araca kendini göstermiyordu.

### 29.2 · 33. testin kapsamı yanlıştı — iki kez

**İlk hâli** yalnız `.screen` ve `.card`'a bakıyordu: sarmalayıcıların içini
**hiç görmüyordu**. Cemil'in gördüğü sıfırlar tam oradaydı.

**Bütün kaplara bakmayı denedim: 165 "bulgu"** çıktı ve çoğu **meşruydu.** Bir
bileşenin **içi** kasıtlı olarak sıkıdır:

| Yer | Boşluk | Neden kasıtlı |
|---|---|---|
| `.tl-body`: başlık → alt satır | 8 px | **Birbirine ait** — az boşluk bunu söyler |
| `.arac-bas`: plaka → başlık | 12 px | Aynı künyenin parçaları |
| `.ozet-bas`: ad → adres | 2 px | Tek bir blok |

165 bulgulu bir test **okunmaz** olur ve okunmayan test, olmayan testtir.

**Doğru kapsam: blok kapları** — `.screen`, `.card`, `.yigin`. Bunlar **bölüm
listeler**; içleri birbirine ait değildir.

> **Ders:** bir testin eşiği kadar **kapsamı** da yanlış olabilir. İkisi de
> "0 bulgu" der; biri gevşek olduğu için, öteki bakmadığı için.

### 29.3 · Aynı turda açılan öteki boşluklar

| Yer | Önce | Sonra | Neden |
|---|---|---|---|
| `.tutar-serit` satırları | 6 | **10** | *Teslim edilen / Nakit / Kart / Havale* tek blok gibi okunuyordu — Cemil'in ekran görüntüsü |
| `.menu-list` | 9 | **12** | Menü satırları yapışıktı |
| `.tl-item` arası | 10 | **12** | |
| `.arac-kart` arası | 12 | **14** | |
| `.notif-list` | 8 | **10** | |
| `.kalem-list` | 10 | **12** | |

`.tutar-serit`'in `margin-top`'u da **kaldırıldı** — kart içinde kabın gap'i
zaten veriyordu, ikisi toplanıyordu.

**Negatif doğrulama:** `.yigin { gap: 0 }` geri konunca test **12 bulgu**
verdi ve bulguların ilk dördü **tam olarak Cemil'in gördüğü sıfırlardı**;
kural açılınca 0.

---

## K-028 · Modül tahtası OtoMix dilinde; boşluğu KAP verir; gizli olan gizli kalır (05.09.2026)

### 28.1 · "Otomix gibi" — tahmin edilmedi, bakıldı

> *"Detaylı muhasebe alanı Otomix gibi olacak. Buttonlar buna göre olmalı."*

OtoMixSQL'in modül dili çıkarıldı (`arastirma/OTOMIX-HARITA.md`):
**Cari · Fatura · Kasa · Banka · Çek · POS · Stok · Teklif · e-Fatura/e-Arşiv
· SMS.** Paraşüt ve LucaNet'te olduğu gibi **ölçülmüş bir referans**, tahmin
değil.

**Alınan şey düzen ve dil, kapsam değil.** Masaüstü programların klasik modül
tahtası: **ikonlu kare kutular, ızgara hâlinde.**

| | Ana menü | Modül tahtası |
|---|---|---|
| Biçim | `.menu-row` — **liste** | `.modul-kutu` — **ızgara** |
| Neden | Yukarıdan aşağı okunur, **sırası anlam taşır** | Gözle **taranır**, hepsi aynı ağırlıkta |

**Sıra OtoMix'in sırası değil, Tamiron'un sırası: çalışanlar önce.** OtoMix'te
modüller klasik sırada duruyor çünkü hepsi yazılmış; burada yarısı yazılmadı
ve çalışanı aşağı atmak, ustayı **çalışmayanların arasında aratmak** olurdu.

| Çalışan | Yazılmadı *(solgun, dokunulamaz)* |
|---|---|
| **Kasa** *(basit kipin kendisi)* · **Raporlar** *(iş başına)* · **SMS** *(mesaj)* | Cari · Fatura · Banka · Çek · POS · Stok · Teklif |

> **OtoMix, Tamiron'un en yakın rakibi:** tek program hem servis hem muhasebe
> yapıyor. Paraşüt ve LucaNet'in gövdesi fatura; ikisinin gövdesi **iş emri**.
> Fark, OtoMix'in muhasebeyi **altı modüle** açması — tamirci tam bu yığına
> *"karmaşık geldi"* demişti (T-1).

### 28.2 · Gezinme hedefi kodda BİRLEŞTİRİLMEZ

Modül düğmeleri `' data-git="' + m.git + '"'` diye yazılıyordu. Sonuç:
derlemedeki *"ölü data-git hedefi"* denetimi ve sağlık denetiminin **YETİM**
testi bu hedefi **göremiyordu** — ekran açılıyordu ama hiçbir araç bunu
bilmiyordu. `git` alanına bir yazım hatası girseydi düğme **sessizce** hiçbir
şey yapmazdı.

**Aynı sınıf renk kutucuğunda yaşanmıştı** (K-019: sınıf adı kodda
birleştirilmez, haritadan okunur). Hedef artık bir **etiket**; ekran adı
işleyicide **literal** yazılıyor ve bilinmeyen etiket **balon** veriyor —
menüye modül ekleyip bağlamayı unutmak sessiz kalmıyor.

### 28.3 · Boşluğu KAP verir — kart içi ritim de tek yerden

> *"Elementler yine birbirine yakın. Kontrol et."*

Birinci turda (K-026.6) `.screen` gap'i 18'e çıkarılmıştı ama **kart içi hâlâ
margin'lerle** yürüyordu ve her yer **14 px**'te kalmıştı. Cemil haklıydı;
ölçüm onu doğruladı: **on bir ekranda 14 px.**

İki sessiz sebep vardı:
- `.screen-head { margin-bottom: -4px }` — ritmi 18'den **14'e geri çekiyordu**
- `.section-label { margin-bottom: 14px }` — kart içinde ikinci bir kaynak

Artık `.card` da **flex sütun + gap: 16**. Form alanlarının `margin-top`
kuralları **kaldırıldı**: boşluğu artık kabın gap'i veriyor. İkisi birden
olsaydı **toplanır** ve alanlar arası 34 px'e çıkardı — *"çok yakın"*ın tersi
kadar yanlış.

> **Kural: BOŞLUĞU KAP VERİR.** Bileşen kendi dışındaki boşluğu yazmaz.

**33. testin eşiği 8 → 16.** İlk eşik *"0 bulgu"* veriyordu ama hedefin
altındaki her şeyi geçiriyordu. **Bir test ölçmüyorsa değil, YANLIŞ EŞİKLE
ölçüyorsa da yanlış güven verir.** Eşik artık tasarım hedefiyle aynı.

### 28.4 · ⚠️ GİZLİ OLAN GÖRÜNÜYORDU — ve bunu ekran görüntüsü buldu

`.card`'ı flex yapmak **gider formunu kapalıyken ekranda bıraktı.** `hidden`
özniteliği tarayıcının kendi stilinde `display: none` demektir — ve
**yazdığımız her `display` kuralına yenilir.**

Öznitelik duruyordu, form görünüyordu. **Hiçbir test görmedi**, derleme de
görmedi; ekran görüntüsüne bakarken yakalandı. Sessiz bozulmanın ders kitabı
hâli: **kod doğru, öznitelik doğru, görünüş yanlış.**

Çözüm tek satır ve bundan sonra hangi bileşene `display` verilirse verilsin
geçerli: `[hidden] { display: none !important; }`

**34. test (GIZLI-AMA-GORUNUR)** eklendi ve negatif testle doğrulandı
(kural kapatılınca 13 bulgu, açılınca 0).

### 28.5 · Basit kipte varsayılan BUGÜN

> *"Basit modda bugün seçilmeli."*

Ustanın günlük sorusu *"bugün ne kaldı"*; ay ve yıl **ikinci** sorudur. Ekran
açıldığında birinci soruyu cevaplar — servis kayıtlarındaki *"içeride açık
gelir"* kuralının aynısı.

### 28.6 · Alacak DÖNEME TABİ DEĞİL

> *"Alacak olmalı."*

Alacak bir **hareket** değil, bir **bakiye**: geçen aydan kalan alacak bu ay da
alacaktır. Dönem süzgecine sokunca *"Bugün"* seçiliyken alacak **gizleniyordu**
— ustanın en çok sorduğu şey ekrandan kayboluyordu. Tahsilat ve gider döneme
tabi, **alacak değil**; başlığın yanında bu yazıyor.

**Ölçüldü:** *Bugün* seçiliyken alacak görünüyor — Demir Nakliyat, 06 TMR 07,
28.08.2026, **3.450 ₺**.

---

## K-027 · Muhasebe: dönem her şeyi süzer, rakamlar tabloda, derinlik menüde (05.09.2026)

Cemil'in muhasebe turu. Dokuz düzeltme, üçü **kural** değiştirdi.

### 27.1 · Dönem düğmeleri ADINI söylüyor — ve her şeyi süzüyor

> *"Basit tabının altına Bugün, bulunduğumuz ay hangisi ise, sonra da
> bulunduğumuz yıl hangisi ise butonları olacak."*
> *"Sayfadaki alanları yukarı gün, ay filtresine göre yap."*

**"Bu ay" değil `Eylül`, "Hepsi" değil `2026`.** Fark küçük görünüyor ama
*"bu ay"* ustaya **hangi** ay olduğunu söylemiyor; ekran görüntüsü alınıp bir
hafta sonra bakıldığında ya da ay dönüldüğünde rakamın hangi pencereye ait
olduğu kayboluyordu. Ad yazınca **rakam kendi tarihini taşıyor**.

**Süzgeç artık ekranın tamamına işliyor.** Önce yalnız *"bugün"* kartı sabitti
ve ödeme dağılımı **bütün** kayıtları topluyordu: iki rakam aynı ekranda **iki
farklı pencereye** bakıyordu ve hangisinin ne olduğu yazmıyordu.

*"Hepsi"* penceresi kalktı — dükkânın açılışından beri toplam, ustanın sorduğu
bir soru değil.

### 27.2 · "Kâr değil" açıklaması kalktı — RAKAM DA KALKTI

> *"Bu rakam kâr değil diye açıklama var. Onu kaldır."*

Açıklamayı kaldırıp **rakamı bırakmak, uyarısız bir yanlış rakam bırakmak**
olurdu. K-014 hâlâ geçerli: *eksik giderle çıkan kâr, kârdan kötüdür — çünkü
usta ona inanır.* O yüzden *"Kalan"* satırının kendisi kaldırıldı.

**Yerine geçen şey daha iyi:** iş başına fark. O rakam **her zaman doğru**,
çünkü iki bileşeni de zaten kayıtlı (satır fiyatı ve `alis`, T-9). Girilmemiş
bir şeye bağlı değil, dolayısıyla yalan söyleyemez.

### 27.3 · Rakamlar tabloya geçti

> *"Gider alanı excel gibi tablo şeklinde olsun."*
> *"Alınamayan (Veresiye) değil Alacak olsun başlık. Yine tablo gibi
> okunabilir olabilir."*

Gerçek `<table>` kullanıldı: rakam sütunu **hizalı** olmalı. `.tutar-serit`
iki sütunluk bir şerit; üç–dört sütun gerekince satırlar birbirini tutmuyordu.
Tablo ayrıca ekran okuyucuya *"bu bir tablo"* diyor.

**Dar ekranda kırpılmaz, kaydırılır** — K-025'in dersi doğrudan uygulandı:
sarmalayıcı `overflow-x: auto`.

**"Alacak" ustanın kelimesi.** *"Alınamayan"* bir **kayıp** gibi okunuyordu;
oysa para duruyor, yalnız henüz alınmadı. İlk sütun **KİM** — ustanın sorusu
*"ne kadar alacağım"* değil, **"kimden alacağım"** (K-013).

### 27.4 · İş başına kendi sayfasına çıktı

> *"Hatta iş başına bu ay başka sayfa olabilir, tamamını içerecek,
> filtrelenebilecek."*

Muhasebenin basit kipi bir **bakış** ekranı: açılır, okunur, kapanır. İş başına
liste ise **taranan** bir şey — usta hangi işin ne bıraktığını arar,
karşılaştırır, dönemi değiştirir. İkisi aynı ekranda dururken muhasebe **iki
farklı işi** birden yapıyordu.

Ekran 13 → **14**. *"Toplam parça alışı"* bölümü basit kipten kalktı; parça
alışı artık her işin **kendi satırında** duruyor. İki yerde iki liste, iki ayrı
bakım ve zamanla **iki ayrı rakam** demektir.

### 27.5 · Detaylı sekmesi bir MENÜ — ve yazılmamış modül açılır gibi durmaz

> *"Detaylı alanda ise, muhasebe modüllerine gidebilsin. Çek, senet, fatura vb.
> Menü şeklinde olsun."*

| Modül | Durum |
|---|---|
| İş başına · Parça alışı | **açılır** |
| Fatura · KDV · Çek · Senet · Cari hesap | **devre dışı**, *"yazılmadı"* rozetiyle |

**Menüde görünüp dokununca hiçbir şey yapmayan bir madde**, tam olarak 27.
testin kovaladığı şeydir (ölü uç) — ve en pahalı hâli budur, çünkü usta *"bu
program bunu yapıyor"* sanır. Devre dışı düğme ölü uç değildir: dokunulamaz ve
nedenini söyler.

**Kapsam genişlemedi, YERİ AÇILDI.** `hazir: false` olanların hepsi ustadan
**yeni veri** ister ve K-022'nin gerekçesi duruyor. Cemil kapsamı genişletirse
menü zaten yerinde.

> **Cemil onayına açık (A-M11):** bu beş modül **kapsama girdi mi**, yoksa
> yalnız *"ileride buraya gelecek"* mi? Varsayım: **yeri açıldı, kapsam
> açılmadı** — K-022 duruyor.

### 27.6 · Dönem çipleri iki ekranda; delegasyon çekirdekte

`[data-donem]` dinleyicisi muhasebe ekranında yazılmıştı ve başında
`if (Ekran.adi() !== 'muhasebe') return;` vardı. Sonuç: **iş başına
sayfasındaki dönem düğmeleri ölüydü** — basıyorsun, çip seçili görünmüyor,
tablo değişmiyor, hata da yok.

**Bu, K-020.2'nin AYNI hatası:** iki ekranın kullandığı bir denetimin davranışı
tek bir ekranın dosyasında duruyordu. Ortak nesnenin **davranışı da ortaktır**
— çekirdeğe taşındı. Aynı sebeple `muhDonemSecili` de çekirdekte: iki ekranda
iki ayrı dönem tutulsaydı usta muhasebede *Eylül* seçip iş başına sayfasında
*Bugün* bulurdu ve rakamların neden tutmadığını anlamazdı.

**Ölçüldü:** Eylül → 1 iş / 2.800 ₺ · 2026 → 5 iş / 12.970 alınan · 7.160 parça
· **5.810 fark**. Dönem iki ekran arasında taşınıyor.

### 27.7 · Kapsam notu ekrandan kalktı

> *"En alttaki karar bilgisini kaldır. Sende var zaten karar."*

Haklı: ekran, kendi kapsam tartışmasını **kullanıcıya** anlatıyordu. O tartışma
`KARARLAR.md`'de ve `arastirma/MUHASEBE-KAPSAMI.md`'de duruyor — ustanın
ekranında değil. Ekranın *"burası yarım"* demesi (K-013) hâlâ doğru bir ilke,
ama **kime** dediği önemli: yarımlık artık *"yazılmadı"* rozetiyle **modül
menüsünde** söyleniyor.

---

## K-026 · Plaka kapının kendisi; ses bir kapı değil; muhasebe iki kipli (05.09.2026)

Cemil dokuz madde verdi. Hepsi uygulandı; üçü bir **kararı** değiştirdi.

### 26.1 · "Sesli işlem" menüden kalktı — ses kalktı DEĞİL

> *"En üstten sesli işlemi kaldıralım. Çünkü karışıklık olacak orada."*

Menüde altı satır var ve **her birinin sağında kendi mikrofonu** duruyor —
*"o satırın işini söyleyerek yap."* Yedinci satır olarak duran *"Sesli işlem"*
ise hangi işi yapacağını söylemiyordu; usta için iki ayrı şey gibi görünüyordu,
oysa aynı şey.

**Kalkan bir KAPI, yetenek değil.** Altı mikrofon duruyor, arama kutularının
mikrofonları duruyor (K-020), ses ekranı bağlamıyla açılmaya devam ediyor
(K-017). K-016 hâlâ tutuyor: sesin yaptığı her işin elle yolu var.

### 26.2 · PLAKA, "kayıtlı mı" sorusunun kendisi

> *"Kayıtlı araç mı geldi kısmını kaldır… Eğer daha önce gelmiş araç ise
> ruhsata gerek yok… Değilse ruhsatı çekmesini isteriz. Plakayı yaz alanı ve
> tab olabilir. Ses açık tab olur."*
> *"Ruhsatı çek ve plakayı yaz tab olsun."*

Ekranın tepesinde *"Kayıtlı araç mı geldi?"* diye bir çip listesi vardı ve
kayıtlı araçları **tek tek sayıyordu**. Dört araçta çalışır, kırk araçta
çalışmaz. **Plakanın kendisi zaten bu sorunun cevabı.**

| Plaka yazıldı | Ne oluyor |
|---|---|
| **Kayıtlı** | *"Bu araç kayıtlı — Renault Mégane · Mehmet Kaya"* · **ruhsat istenmez**, tür seçilir; açık kaydı varsa oraya götüren düğme çıkar |
| **Kayıtsız** | *"Bu araç kayıtlı değil"* · ruhsat istenir; ruhsat yoksa araç alanları açılır ve **elle ya da sesle** doldurulur |

**İki sekme:** *Plakayı yaz* (varsayılan, **mikrofonlu** — Cemil: *"ses açık
tab olur"*) ve *Ruhsatı çek*. Ruhsat okununca ekran **plaka sekmesine düşer**:
okunanı görmeden düzeltilemez.

**Sesle ne söylenebileceği YAZIYOR** (*"Sesle girişte beklediğimiz bilgileri
gösterelim"*). Boş bir mikrofon, ustaya *"ne diyeceğim"* diye düşündürüyordu.

### 26.3 · Üçüncü tür: "Tamirat + Bakım" — K-006 genişledi

> *"Sonra altına Tamirat, Bakım veya Tamirat + Bakım yazabilir."*

K-006 iki tür tanımlamıştı ve gerekçesi sağlamdı (km ve hatırlatma yalnız
bakımda). Ama **gerçek ziyaret ikisi birden olabiliyor** — araç triger için
geliyor, madem açıldı bakımı da yapılıyor. İki ayrı kayıt açmak ustayı aynı
aracı iki kez yazmaya zorluyordu.

**Kural tek cümle:** *bakım içeriyorsa bakım kuralları işler* — `bakimIceriyor()`.
Bu fonksiyon olmadan `tur === 'bakim'` kontrolü **sekiz yerde** duruyordu; üçüncü
tür eklenince sekizinin de değişmesi gerekirdi ve biri unutulsa **hatırlatma
sessizce kurulmazdı.**

> **Cemil onayına açık (A-M10):** *"Tamirat + Bakım"* tek kayıt mı olmalıydı,
> yoksa liste satırında iki kaydın **özeti** mi? Tek kayıt varsayıldı.

### 26.4 · Servis kayıtları araç kartı diline geçti

> *"Servis kayıtları araç listelemesi gibi olsun. Araç bilgileri belirgin
> olacak."* · *"Üstte yeni servis kaydı buttonunu kaldır. İçeride teslim
> edilmiş filtresini kaldır."*

**Düğme neden gereksiz:** menüde zaten *"Servis kaydı ekle"* var ve bu ekrana da
oradan geliniyor. Aynı işi iki yerde göstermek *"hangisinden gireceğim"*
sorusunu doğuruyordu — 03.09'da *"Araç kaydı"* menü maddesi tam bu sebeple
kalkmıştı.

**Süzgeç neden gereksiz:** liste artık araç kartı dilini kullanıyor ve
**içeridekiler üstte, çerçevesi farklı** duruyor. Süzgeç, çerçevenin zaten
söylediğini ikinci kez soruyordu. **K-004 duruyor:** *"içeride"* bir durum
değil, `teslimTarihi`'nden türetilen bir görünüm.

**Arama genişledi:** liste artık marka ve sahibi **gösterdiği** için onlarda da
aranıyor. Gösterilen bir şeyin aranamaması, ustaya *"yok"* dedirtir.

### 26.5 · Muhasebe iki kipli — basit varsayılan, detaylı opt-in

> *"Basit muhasebe olarak düşünelim. Fakat daha fazlasına ihtiyaç duyanlar için
> de bir geliştirme yapmalıyız."*

| Kip | İçinde ne var | Ustadan istediği karar |
|---|---|---|
| **Basit** *(varsayılan)* | bugün tahsilat · ödeme dağılımı · giderler · parça alışı · veresiye | **sıfır** |
| **Detaylı** *(opt-in)* | dönem (bugün/bu ay/hepsi) · para nereye gitti · **iş başına fark** | 1 (dönem) |

**Karar bütçesi (K-022.4) böyle korunuyor:** ustanın çoğunluğu için ekran hâlâ
sıfır kararlı; isteyen bir dokunuşla derinleşiyor.

**Detaylı kip HİÇBİR YENİ ALAN İSTEMİYOR.** *"Daha fazlası"* demek **daha fazla
veri girmek** değil — var olan veriye başka bir pencereden bakmak. Teslim
tarihi, satır fiyatı ve alış fiyatı zaten kayıtlı.

**"Kâr" denmedi, "Kalan" dendi.** Kira, elektrik, maaş girilmemiş olabilir;
*kâr* deseydik usta ona **inanırdı** (K-014). Ekran bunu açıkça yazıyor.
**İş başına fark** ise her zaman doğru: müşteriden alınan eksi o iş için
parçaya ödenen — ikisi de kayıtlı.

### 26.6 · Dikey ritim 14 → 18 px

> *"Elementler birbirine çok yakın. Bunu genel olarak kontrol et."*

Ölçüldü: altı ekranda kart ve alan araları **12–14 px**, bir yerde **0 px**
(kalem listesi ile *"+ Parça"* düğmeleri bitişikti ve düğmeler listenin bir
parçası gibi okunuyordu).

Tek tek yamamak yerine **ritim tek yerden büyütüldü** — v1 dersi: boyutlar
bileşene gömülüyse *"hepsini büyüt"* demek yüzlerce satıra dokunmak demek.

**33. test (SIKISIK)** eklendi: iki kardeş blok arası **8 px'ten az** ise bulgu.
`.field` içindeki etiket–kutu 6 px'i **kasıtlı** ve ölçüme girmiyor — birbirine
ait olduklarını söylüyor. Negatif testle doğrulandı (0 px geri konunca 1 bulgu).

### 26.7 · Denetim ölçtüğü şeyi YİNE bozuyordu — ikinci yüz

K-024.1 veri kirliliğini kapatmıştı. Bu turda aynı hatanın **ikinci yüzü**
çıktı: 30. test (SAHTE-EYLEM) her düğmeye basarken **sekmelere de** basıyordu ve
sekme, ekranın kipini **ekranın kendi değişkeninde** değiştiriyor — denetimin
veri yedeği oraya ulaşamaz. Sonuç: denetimden sonra muhasebe **detaylı kipte**
açılıyordu.

İki düzeltme:
- `role="tab"` sahte-eylem testinden **elendi** — sekme hiçbir eylem *iddia*
  etmiyor, dolayısıyla sahte eylem olamaz. **Test etmediği bir şeye dokunmasın.**
- Muhasebe ekranı **kipini her çizimde uyguluyor**. Önce gizleme yalnız
  tıklamanın yan etkisiydi; ekran başka bir yoldan çizilince değişken *"basit"*
  diyor ama ekranda detay duruyordu. **Çizim, değişkenin dediğini göstermeli.**

### 26.8 · "Müşteri getirdi" bir bakışta okunuyor

Bilgi zaten çiplerde vardı ama **çip bir seçim aracı**: hangisinin basılı
olduğunu anlamak için bakmak gerekiyordu. Oysa bu K-005'in **sorumluluk
anahtarı** — sorun çıktığında işçilik alınıp alınmayacağını belirler. Rozet
yalnız **müşteri** getirdiyse çıkıyor; tamirci getirmesi normal hâl, her satıra
rozet koymak gürültü olurdu.

---

## K-025 · Ölçüm üç genişlikte yapılır; kırpılan metin taşma sayılmaz (04.09.2026)

**Karar:** Sağlık denetimi bundan sonra **320 · 375 · 430 px**'te koşulur.
Tek genişlikte ölçmek, o genişlikte doğru olan bir tasarımı *"doğru"* sanmaktır.

| Genişlik | Ne temsil ediyor | Sonuç (04.09) |
|---|---|---|
| **320 px** | Küçük/eski telefon — en dar gerçek cihaz | 0 bulgu *(düzeltmeden sonra)* |
| **375 px** | Referans ölçü, bugüne kadarki tek ölçüm | 0 bulgu |
| **430 px** | Büyük telefon | 0 bulgu |

### 25.1 · Bulunan hata: 320 px'te düğme metni SESSİZCE kırpılıyordu

Araç ekranının üç düğmesi (K-019.4) **375 px'e göre** ölçülmüştü. 320 px'te
*"Açık kayıt"* **102 px** istiyor, kutu **89 px**.

**Hiçbir test görmedi ve göz de görmedi.** Sebebi kendi yazdığım
`overflow: hidden`: kutu taşmıyordu, **kesiyordu.** 20. test (IC-TASMA)
`overflowX !== 'visible'` olan kutuları **kasten atlıyor** — doğru, çünkü
onlar taşmıyor. Taşmamalarının sebebi içeriği kesmeleri.

Düğme *"Açık kay"* yazıyordu.

**Düzeltme — dar ekranda İKON kalkar, YAZI kalır.** Anlamı yazı taşır; ikon
süstür. 24 px ikon + 5 px boşluk = 29 px, sorunu fazlasıyla çözüyor.
`overflow: hidden` de **kaldırıldı** — bir daha olursa taşma testi görsün.

**Ölçüldü:** 320 px ikonlu 102 > 89 (kırpık) → ikonsuz **89 = 89** ·
375 px **107 = 107** · 430 px **125 = 125**.

### 25.2 · 32. test: KIRPILAN-METIN

> **Kesilen metin, taşan metinden tehlikelidir:** taşan görünür, kesilen
> görünmez. Kullanıcı eksik bir kelime okur ve eksik olduğunu bilmez.

Ölçüm: `overflow` görünür olmayan bir kutuda `scrollWidth > clientWidth` ve
kutunun **metni varsa** bulgu. İki şey **kasten** dışarıda:
- `overflow: auto/scroll` — kullanıcı kaydırabiliyorsa kırpılmış sayılmaz
- `text-overflow: ellipsis` — tasarımcı bilerek kısaltmış ve **üç nokta
  kullanıcıya bunu söylüyor**

Negatif doğrulama gerekmedi: test **yazıldığı anda gerçek bir bulgu verdi**
(`btn "Açık kayıt" 102>89`), düzeltmeden sonra 0'a düştü. Bir testin
doğruluğunun en iyi kanıtı, yazılırken var olan bir hatayı bulmasıdır.

---

## K-024 · K-016 tutuyor — ama denetimin kendisi bozuktu (04.09.2026)

**Karar:** *"Ses var olan işi kısaltır, yeni yetenek açmaz"* (K-016) **ölçüldü
ve doğrulandı.** Sesin yaptığı dokuz işin **dokuzunun da** elle yolu var.

| Sesin yaptığı | Elle karşılığı |
|---|---|
| `servis-ac` | araç ekranı → **Kayıt aç** → tür seç |
| `yeni-arac` | menü → Servis kaydı ekle → ruhsat |
| `teslim` (+ödeme) | kayıt → Teslim et → ödeme çipi |
| `satir-ekle` | kayıt → **+ Parça / + İşçilik** |
| `gider-ekle` | muhasebe → **Gider ekle** ← *bugün eklendi (K-021.1)* |
| `mesaj` | mesaj ekranı → müşteri + şablon |
| `ara` (+süzgeç) | her listenin arama kutusu + kayıtlardaki çipler |
| `bilinmeyen-arac` | ruhsat → elle gir |
| `teslim-edilecek-kayit-yok` | araç ekranı |

**⚠️ K-016 bu sabah ÇİĞNENMİŞ DURUMDAYDI.** `gider-ekle` yalnız sesin
yapabildiği bir işti; elle yolu **yoktu** ve Web Speech Firefox'ta çalışmıyor
(K-015). Ölü uç turunda bulundu (K-021.1). Kural yazılıydı, denetlenmiyordu.

### 24.1 · ⚠️ DENETİM ÖLÇTÜĞÜ ŞEYİ BOZUYORDU

Bu turun en pahalı bulgusu.

**30. test (SAHTE-EYLEM) her düğmeye basıyor**, 27. test `ACIK_KAYIT` ve
`ACIK_PLAKA` atıyor. Denetim `load` anında **kendiliğinden koştuğu** için
prototip **her açılışta kirli başlıyordu**: taze yüklenmiş bir sayfada
`SESLI_NIYET = 'kayit-icinde'`, `ACIK_KAYIT = 1` duruyordu.

Sonuç: sesli işlem çözümleyicisi **her cümleyi *"satır ekle"* diye okuyordu.**
Dokuz farklı cümle denendi, dokuzu da aynı cevabı verdi.

> **Kendi ölçtüğü şeyi değiştiren bir araç, ölçüm aleti değildir.**

Denetim artık başta **derin kopya** alıyor, sonda **geri yüklüyor**.

**Ve düzeltir düzeltmez GERÇEK bir hata ortaya çıktı:** özet ekranı boş
durumda **ölü uçtu** — hiçbir dokunulabilir öge yoktu. 27. test bunu
göremiyordu, çünkü `OZET_KAYIT` denetimin **kendi tıklamalarıyla** doluyordu
ve ekran hep dolu çiziliyordu. Test *"geçiyor"* değildi, **yanlış şeye
bakıyordu.**

### 24.2 · K-018 ÜÇÜNCÜ KEZ — kısa kelime, başka kelimenin içinde

`` yasaklandı (K-018), yerine `indexOf` kondu ve **aynı hata geri geldi.**
Ölçüldü:

| Usta ne dedi | Program ne anladı | Neden |
|---|---|---|
| *"41 KV 5588 yeni **araç** bakım"* | **arama** | `"arac"` içinde `" ara"` var |
| *"34 ABC 123 **beyaz** araba geldi"* | **mesaj** | `"beyaz"` içinde `"yaz"` var |
| *"34 ABC 123 **gösterge** paneli yanıyor"* | **arama** | `"gösterge"` içinde `"göster"` var |

Üçü de ustanın **en sıradan** cümleleri. Kural bir kez daha keskinleşti:

> **Kısa komut kelimeleri İÇİNDE aranmaz** — boşluktan bölünüp **eşitlikle**
> karşılaştırılır (`kelimeVarMi`). Uzun ve ayırt edici kökler (`mesaj`,
> `whatsapp`, `hatırlatma`, `içeridek`) için içinde aramak hâlâ doğru; onlar
> başka kelimenin içine düşmüyor.

**31. test (SES-KELIME-TUZAGI)** bu üç cümleyi ölçüyor. Neden kural değil
**davranış** ölçülüyor: kodda `indexOf` aramak yanlış pozitif yağdırır —
uzun kökler için `indexOf` doğrudur. Ölçülecek şey, tuzak cümlenin doğru
okunup okunmadığı. Negatif testle doğrulandı (eski davranış → 3 bulgu).

### 24.3 · Eski derleme ölçülmez — araca koruma kondu

*"derle → geri → göm"* sırasıyla koşuldu; `geri` kendi yedeğini yazıyor ve
**taze derlemeyi ezdi.** Sonra o eski dosyaya denetim gömüldü. Tarayıcıda
ölçülen şey kaynağın hâli **değildi** ve bu ancak *"eklediklerim neden
görünmüyor"* diye bakınca fark edildi.

> ***"0 bulgu"* raporlamanın en sinsi yolu bu:** denetim doğru koşuyor, doğru
> ölçüyor, ama **yanlış dosyayı** ölçüyor.

`denetim-kos.py gom` artık kaynak `omurga.html`'den yeniyse **reddediyor**.
Negatif testle doğrulandı.

---

## K-023 · Müşteriye PDF değil LİNK gider; özet ustanın ekranı değildir (04.09.2026)

**Karar:** Yedi işten altıncısı (*müşteriye özet göndermek*) yazıldı.
Müşteri bir **sayfa** görüyor, bir dosya değil.

### 23.1 · PDF neden değil

Tamircinin bugünkü işi (S8): *"müşteriye iş listesinin yani tutarın iş,
parça açıklaması ve Toplam tutar olarak pdf gönderiyorlarmış. Bunu WhatsApp
üzerinden uygulama linki gönderme fikri mantıklı geldi."*

| | PDF | Link |
|---|---|---|
| Üretmek | **sunucu ister** | ister istemez |
| Gönderildikten sonra | **ölü** — o anki hâli taşır | canlı, iş sürerken güncellenir |
| Müşteri bir hafta sonra | dosyayı bulamaz | linke yine dokunur |
| T-21 (*"müşteride de program olsun"*) | karşılamaz | **en ucuz hâli** — kurulum yok |

**Mesaj yine de bir işe yarıyor:** WhatsApp'ta linke dokunmayan da olur, o
yüzden **kalem sayısı ve toplam mesajın içinde** duruyor. Tam liste sayfada.

### 23.2 · Bu ekran ustanın ekranı DEĞİL

Ekran sayısı 12 → **13**, ama ekran bütçesi (A-2) **ustanın** ekranlarını
sayıyor; bu sayfayı müşteri görüyor. Prototipte ustanın **önizlemesi** olarak
duruyor ve **tepesinde bunu yazıyor** — yazmasaydı bir sonraki oturumda
*"13. ekran nereden çıktı"* diye sorulurdu.

Ustanın erişimi mesaj ekranındaki **"Müşteri ne görecek?"** düğmesi. Yalnız
**özet şablonunda** çıkıyor; öteki iki şablon link taşımıyor, gösterilecek
sayfa da yok.

### 23.3 · Sayfanın üzerinde ne YOK ve neden

| Yok | Neden |
|---|---|
| Fiyat düzenleme, satır ekleme | Müşterinin sayfası — salt okunur |
| Onay / red düğmesi | **Onay mekanizması kurulmuyor** (K-003, S4) |
| Ödeme alanı | Para dükkânda alınıyor; kapsam dışı |
| Alış fiyatı (`alis`) | **Müşteriye gitmez** (`VERI-MODELI §3`) |
| Başka araç, geçmiş kayıt | Link tek kaydın |

**Müşteri parçası SÖYLENİYOR.** Fiyatsız bir satırı sessizce göstermek
müşteriye *"bu bedavaydı"* diye okunur; oysa parayı o ödemiş (S2, K-005).
Satırda *"parçayı siz getirdiniz"* yazıyor.

**Durum tek** (K-004): *"Aracınız serviste"* ya da *"… tarihinde teslim
edildi"*. Müşterinin ikinci sorusu bu.

> **Cemil onayına açık (A-M9):** sayfada **kimlik doğrulama yok** — linki olan
> görür. Link plaka + kayıt numarasından türüyor, yani **tahmin edilebilir**.
> Gerçek kullanımda tahmin edilemez bir kod gerekir. Prototipte kabul;
> backend fazında kapatılacak.

### 23.4 · Ekranlar arası devredilen değer çekirdekte durur

`ozetLinki()` iki ekranda gerekiyordu (mesaj metni ve özet sayfası). Ekran
betikleri derlemede **kendi IIFE'sine sarıldığı** için özet ekranında
tanımlansaydı mesaj ekranı onu **göremezdi** — ve hata, mesaj metninde sessizce
eksik bir link olarak çıkardı. `OZET_KAYIT` ile birlikte çekirdeğe alındı;
`MESAJ_HEDEF`, `ARAMA_TERIMI`, `RUHSAT_ONDOLGU` zaten orada.

---

## K-022 · Muhasebe kapsamı: kasa + gider. Cari hesap kapsam dışı (04.09.2026)

> **Karar taslağı — Cemil onayına açık (MUH).** Lider Cemil yokken
> hazırladı; ölçümler ve gerekçe `arastirma/MUHASEBE-KAPSAMI.md`'de.
> Kod tarafında yalnız **hiçbir yeni alan istemeyen** kısım yazıldı.

### 22.1 · Seçenek B — kasa + gider

| Seçenek | Yeni veri girişi | Yalan söyleyebilir mi | Yedi işten hangisi |
|---|---|---|---|
| A · günlük kasa | **sıfır** | hayır | **4** — teslimde ödemeyi bilmek |
| **B · kasa + gider** | her gider için bir giriş | evet, eksik girilirse | 4'ün komşusu |
| C · cari hesap | açılış + her ödeme + her kısmi ödeme | **evet, ve kalıcı** | **hiçbiri** |

**C neden kapsam dışı:** yedi işten hiçbirine hizmet etmiyor · **K-012'yi geri
alıyor** (müşteri tablosu yok, müşteri aracın üstünde iki alan) · bir kez kayan
bakiye bir daha tutmaz. Paraşüt'te cari hesap bir **ekran ailesi**: ekstre,
mutabakat, virman, ödeme hatırlatma. Bu, tamircinin *"karmaşık"* dediği yığının
tam ortası. *"İleride lazım olur"* gerekçe değildir.

**B'nin sınırı keskin:** fatura yok · KDV yok · tevkifat yok · e-belge yok ·
banka yok · çek/senet yok · stok yok · gider kategorisi yok · aylık özet yok.
Hepsi rakiplerde var; **hiçbiri yedi işten birine hizmet etmiyor.**

### 22.2 · Parça alışı bedavaya geldi — ve bu bir kural örneği

`ServiceLine.Cost` (`alis`) **zaten kayıtlı**, çünkü T-9 var: *"Usta kendi fiyat
aldıysa, parça fiyatlarına ekleme yapabiliyor."* Tamircinin parçaya ödediği para
veri modelinde duruyor ve ustadan **tek bir yeni giriş istemiyor.**

K-013'ün ölçüsü şuydu: *"girilmezse yalan söyleyen rakam gösterilmez."* Bu rakam
o gruba ait değil — **her zaman doğru.** Bu yüzden yazıldı.

**İki süzgeç, ikisi de gerekçeli:**
- **Yalnız tamircinin aldığı parçalar.** Müşteri getirdiyse para tamirciden
  çıkmamıştır (K-005); sayılsaydı ödenmemiş bir para gider gibi görünürdü.
- **Yalnız teslim edilmiş kayıtlar.** İçerideki araçta parça daha alınmamış
  olabilir; olmamış bir harcamayı gider saymak ustayı olduğundan yoksul gösterir.

**⚠️ Çift sayma tehlikesi ekranda YAZILI.** Usta parçayı hem satıra alış olarak
yazıp hem elle *"Öz Yedek Parça — triger seti"* diye gider ekleyebilir. Bu
yüzden parça alışı, elle giderlerden **ayrı** duruyor ve ekran uyarıyor.
Karıştırılsaydı çift sayma **sessizce** olurdu.

**Ölçüldü (04.09):** 5 satır · **7.160 ₺** · içerideki kayıtlar sayılmadı ·
müşteri parçası sayılmadı.

### 22.3 · Kâr yine hesaplanmıyor — ama sebebi inceldi

**İş başına kâr** (`fiyat − alış`) matematiksel olarak doğru çıkar; **dükkân
kârı** çıkmaz, çünkü kira/elektrik/maaş girilmemiş olabilir. İkisi aynı ekranda
dururken usta ikincisini birincisi sanar.

İş başına kâr **önerildi ama yazılmadı** — yeni bir rakam ve karıştırılma riski
var. Cemil onayına açık (A-M7).

### 22.4 · YENİ KISIT: KARAR BÜTÇESİ

İki rakip incelemesi de **bağımsız olarak** bunu istedi. Ekran bütçesi (A-2)
karmaşıklığın yalnız bir kaynağını kısıtlıyor; ölçüm **üç** kaynak olduğunu
gösterdi: **menü genişliği · formdaki karar sayısı · dil.**

| Kısıt | Değer | Bugünkü ölçüm |
|---|---|---|
| Üst menüde başlık | en fazla **8** | **7** |
| Ana yolda bir ekranda sabit karar | en fazla **3** | **3** (ruhsat) |
| Alan adında tamircinin ağzından çıkmayan kelime | **0** | 0 |

**Varsayılanı olan alan karar sayılmaz** — rakiplerle aynı ölçü. Paraşüt'ün
fatura formunda ~10 alan görünüyor ama gerçekten doldurulan **2**; LucaNet'te
~20 alan, hiçbiri katlanmamış, en az 4–5 karar.

**Tamiron'un ana yolu ölçüldü (04.09):** ruhsat fotoğrafı çekilince 5 alan
kendiliğinden doluyor; tek parçalı bir tamirat baştan sona **5–6 karar**.

Üçüncü kısıt LucaNet'in tersi: *"Senaryo Tipi"*, *"Muhasebeleşebilir mi?"*,
*"Muhtelif Müşteri"* — T-18'in tam karşıtı. `SOZLUK.md` bu testin yeri.

---

## K-021 · Ölü uç turu: iki sahte eylem ve bir yalan söyleyen düğme (04.09.2026)

**Karar:** On iki ekranın hepsi tek tek **işletildi** — çizildiğine
bakılmadı, dokunuldu. Üç bulgu çıktı ve üçü de düzeltildi.

### 21.1 · Muhasebede gider yalnız SESLE eklenebiliyordu

Gider bölümünün tek girişi mikrofondu; boş durumu bile *"Mikrofona
basıp '2400 TL elektrik faturası ekle' demen yeter"* diyordu.

**Neden ölü uç:** K-015 — Web Speech **Firefox'ta yok** ve **güvenli
bağlam** ister. Mikrofonu açılmayan bir tarayıcıda gider bölümü
**tamamen kullanılamıyordu** ve ekran bunu söylemiyordu.

K-014 zaten *"sesle **ve elle** giriş"* diyordu; elle olan hiç
yazılmamıştı. **Ses bir kısayoldur, tek kapı değil** (K-016).

Elle giriş eklendi: iki alan (ne için · tutar) + Ekle. Tutar alanı
rakam dışını atıyor — usta *"2.400 TL"* yazınca `parseInt` tek başına
**2** okuyordu.

### 21.2 · "Onay iste" düğmesi yalan söylüyordu

Düğme *"Onay iste"* diyordu, basınca balon *"Özet gönderildi —
0532…"* diyordu ve **hiçbir şey gönderilmiyordu.** Mesaj ekranı zaten
varken oraya bile gitmiyordu.

**Üç ayrı sorun tek düğmede:** düğme ile balon farklı şey söylüyor ·
sahte bir gönderim rapor ediliyor · gerçek yol varken kullanılmıyor.

Düğme **"Özet gönder"** oldu ve mesaj ekranına özet şablonuyla
gidiyor. Ad neden değişti: tamirci onay için *"işin içine
katmayalım"* dedi (S4, K-003); düğmenin gerçekte yaptığı iş ise yedi
işten **altıncısı** — müşteriye özet göndermek (T-19).
**Cemil onayına açık (A-M6).**

> **Sessiz bir ölü uçtan pahalı:** bir kez *"gönderildi"* yazısına
> kanan usta, bir daha **hiçbir** onay yazısına inanmaz.

**Denetime 30. test (SAHTE-EYLEM) eklendi.** Ölçüm: balon tamamlanmış
bir eylem bildiriyorsa (*gönderildi · eklendi · kaydedildi · silindi ·
güncellendi*), ya **veri** ya **ekran** değişmiş olmalı; ikisi de aynı
kaldıysa balon yalan söylüyor. Negatif testle doğrulandı (eski davranış
geri kondu → 1 bulgu; kaldırılınca → 0).

### 21.3 · A-M2 uygulandı — mesaj ikinci iletişime gidiyor

**Varsayım (Cemil onayına açık):** ikinci iletişim varsa özet ve
hatırlatma **ona** gider, yoksa ruhsattaki numaraya. Usta mesaj
ekranından hedefi **değiştirebiliyor** — varsayım bir kilit değil,
varsayılan. Kime gittiği **göndermeden önce** yazıyor.

**Test bir hata daha buldurdu:** firma ruhsatlı araçta mesaj
*"Demir Nakliyat Ltd. Şti. **Bey/Hanım**,"* diye başlıyordu — müşteriye
giden **ilk cümle** bu. Ad firma işaretleri taşıyorsa (`Ltd · Şti ·
A.Ş. · San · Tic …`) *"Bey/Hanım"* yazılmıyor. Ayrı bir "tüzel kişi"
alanı **açılmadı**; ada bakılıyor.

⚠️ Firma araması **boşluktan bölüp eşitlikle** yapılıyor (K-018):
desenle arasaydık *"Astan"* içindeki *"as"* firma sanılırdı. Ölçüldü —
`firmaMi('Astan Aslan')` = false.

### 21.4 · `Ekran.ac()` zaten açık ekranda sessizce hiçbir şey yapmıyordu

Aynı ekrana ikinci kez gidilince düz `return` ediliyordu. v2'de
ekranlara **kuyrukta veri bırakılarak** gidiliyor (`MESAJ_HEDEF`,
`ARAMA_TERIMI`, `RUHSAT_ONDOLGU`, `SESLI_NIYET`): veri bırakılıyor,
ekran açıldı sanılıyor, `ciz()` koşmadığı için ekran **bir önceki
müşteriyi göstermeye devam ediyordu** — ve hata mesajı yok.

**Bu, K-017'nin hata sınıfının aynısı.** Orada çözüm sesli işleme özel
yazılmıştı (`sesleAc` tek giriş noktası) ve **kural taşınmadı**; aynı
tuzak mesaj ekranında yeniden kuruldu. Artık kökünden kapalı: zaten
açık ekrana gidilirse **tazelenir**, geçmişe ikinci kez yazılmaz.

> **Buna test yazılmadı, çünkü sınıf artık İMKÂNSIZ.** Yapısal olarak
> kapatılabilen bir hata için test yazmak, kapıyı kilitleyip yanına
> bekçi dikmektir. Ölçüldü: geri düğmesi aynı ekranı iki kez saymıyor.

---

## K-020 · Mikrofon, işin yanında durur — kendi yanında değil (04.09.2026)

**Karar (Cemil, üç madde):**

> *"Araçlar arama mic button olsun."*
> *"Sesli işlem buttonunun yanında mik button olmasın."*
> *"Bildirimler başlığı üstte kalıyor. Ss'te mevcut."*

### 20.1 · Arama kutusunun kendi mikrofonu var

Mikrofon **arama kutusunun sağ ucunda**, `.mic-round.ghost` olarak.
Bastığında sesli işlem **o listenin bağlamıyla** açılıyor (K-017):
araçlarda `araclar`, kayıtlarda `kayitlar`, müşterilerde `musteriler`,
mesajda `mesaj`.

> **Cemil onayına açık (A-M5):** Cemil yalnız **araçları** söyledi;
> mikrofon **dört arama kutusuna birden** kondu. Gerekçe: dördü de
> aynı iş (*"aradığımı söyleyerek bulayım"*) ve menüde bu iş için
> zaten dört ayrı mikrofon var; birine koyup ötekine koymamak,
> ustaya *"hangisinde çalışıyordu"* diye düşündürür. İstenmezse
> üç satır silinerek geri alınır.

### 20.2 · Sesli işlem satırının yanında mikrofon YOK

Öteki menü satırlarında mikrofonun bir işi var: *"bu satırın işini
söyleyerek yap."* **Sesli işlem satırının kendisi zaten o iş.** Yanına
ikinci bir mikrofon koymak, aynı düğmeyi iki kez göstermekti.

**Bu bir hatayı ortaya çıkardı:** `[data-sesle]` dinleyicisi **ana
ekranın betiğinde** duruyordu. Ana ekranın dışında mikrofon yokken
sorun değildi; arama kutularına mikrofon eklenince dört ekranın
davranışı, hiç ilgisi olmayan bir ekranın dosyasından gelmeye başladı.
Ana ekran yeniden yazılsa bütün mikrofonlar **sessizce** ölürdü —
tıklıyorsun, hiçbir şey olmuyor, hata da yok. Delegasyon çekirdeğe
taşındı (ortak nesne kuralı: davranış tek yerde).

### 20.3 · A-M4 kapandı — "Bildirimler" başlığı yapışmıyor

Ekran görüntüsü soruyu cevapladı: başlık `position: sticky` idi ve
çekmece kaydırılınca **altındaki bildirimin üstüne biniyordu**;
görüntüde bildirim kartının yalnız bir dilimi duruyor. Kural kalktı.

Yapışkan başlık **uzun** listeler içindir. Burada liste birkaç satır:
kazandırdığı şey yok, kaybettirdiği şey bildirimin kendisi.

**Denetime 29. test (ORTEN-YAPISKAN) eklendi** — ve ilk yazımı
**boştu**: negatif test bulgu vermedi, çünkü çekmece o anki veriyle
taşmıyordu. Kaydırma yoksa yapışkan öge hiç ayrılmaz. Test *"bu ekran
iyi"* demiyordu, *"bugünkü veri az"* diyordu. Doğru soru şu: **bu kap
kaydırılırsa başlık içeriği örter mi?** Test artık kabı 200 px'e
kısaltıp taşmayı **zorluyor**; ölçüm veriden bağımsız.

> **Ders (üçüncü kez):** bir testin yazılmış olması, ölçüyor olması
> demek değil. Negatif test olmasaydı bu delik *"29 test, 0 bulgu"*
> diye rapor edilecekti.

---

## K-019 · Araç ekranları Cemil'in tarifiyle yeniden kuruldu (04.09.2026)

**Karar:** Cemil araçlar listesinin satırını ve araç detayının sırasını
satır satır tarif etti. Tarif **olduğu gibi uygulandı**; bizim üç
gerekçemiz düştü. Aşağıda her biri ve neyi kaybettiğimiz yazıyor —
kaybı yazmayan bir karar, aynı tartışmayı altı ay sonra yeniden açar.

### 19.1 · Ana ekranda bakım hatırlatması YOK

> *"Anasayfada bir aracın bakımı geldi alanini istemiyorum. Sadece
> bildirimlerde gözüksün."*

**Düşen gerekçe:** K-011 ve T-17 *"ustanın 'bugün kimi arayacağım'
sorusu ilk ekranda cevaplanmalı"* diyordu ve şerit bu yüzden menüde
kalmıştı. **Ana ekran bir MENÜDÜR, pano değil** — Cemil haklı.

**Bilgi kaybolmadı:** aynı hatırlatmalar çekmecenin **Bildirimler**
bölümünde duruyor ve menü düğmesinin **rozeti** sayıyı ana ekrandan
gösteriyor. Soru hâlâ ilk ekranda cevaplanıyor, bir rakamla.

**Yan etkisi bir hatayı ortaya çıkardı:** aynı soru İKİ YERDE
süzülüyordu. Çekmece kendi `bekleyenHatirlatmalar()`'ını yazmıştı,
veri katmanındaki `hatirlatmasiGelenler()` ise ayrıca `tur === 'bakim'`
süzüyordu — hatırlatma tarihi olan bir **tamirat** kaydı çekmecede
*"Bakım zamanı geldi"* diye görünüyor, ana ekranda görünmüyordu.
Şerit kalkınca tek kaynak kaldı. **Aynı soruyu iki yerde cevaplamak,
iki ayrı cevap demektir.**

### 19.2 · Araçlar listesi: araç kartı, servis kaydı satırı değil

> *"Araç iconı olmayacak. Sol üstte plaka, altında marka model motor
> renk (renk yazmak yerine kutucuk içerisinde rengi olsun). Altında
> ruhsat sahibi. İçeride olanları üstte olsun. Div çerçeve farklı olsun
> fakat divin içerisinde text olarak içeride yazmasına gerek yok.
> Ruhsat sahibi telefon. Eğer ikinci bir iletişim varsa adı ve
> Telefonu. Ruhsat sahibi firma da olabilir hani."*
> *"Altında 1 kayıt, son bakım vb yazmasın."*
> *"Ayrıca liste telefondan okunmak için daha iyi gözüksün."*

**Düşen gerekçe:** 03.09'da Cemil *"araçlar listesi yine detaylı
olmalı"* demişti ve satıra model, motor, yakıt, son km, kayıt sayısı,
son bakım tarihi doldurulmuştu. **Detay ile kalabalık aynı şey
değilmiş.** Kalabalık listede plaka bile geç bulunuyor.

**Kaldırılanlar:** araç ikonu · *"içeride"* rozeti · kayıt sayısı ·
son bakım tarihi · son km · model yılı · yakıt.
**İçerideki araç** yalnız **çerçeveden** ve **sıradan** belli oluyor.

### 19.3 · Ruhsat sahibi firma olabilir → ikinci iletişim

`Vehicle`'a iki alan girdi: `ContactName` · `ContactPhone`
(`VERI-MODELI.md §1`). **Yeni tablo açılmadı** — K-012'nin kuralı
sürüyor: ekran gerekiyorsa tablo gerekmiyor olabilir.

**Adsız telefon yazılmaz.** Usta kimi aradığını bilmeden aramaz;
numara tek başına bir eylem değil, bir veri parçasıdır.

> **Cemil onayına açık (A-M2):** özet ve bakım hatırlatması hangi
> numaraya gidecek — ruhsattaki mi, ikinci iletişim mi? Varsayım:
> **ikinci iletişim varsa o**, çünkü aracı getiren o. Mesaj ekranı
> şu an hâlâ ruhsattaki numarayı kullanıyor.

### 19.4 · Araç detayının sırası: önce EYLEM, sonra BAŞVURU

> *"Araç detayda, işlem buttonlarının altında ruhsat bilgileri akordion
> olarak yer alsın. Altında km bilgileri yine akordion olarak olsun.
> Altına km bakım bilgileri diğer divler gibi başlıklı olsun."*
> *"Araç detayda işlem buttonları tek satırda yer alsın."*
> *"Buttonlarla ruhsat arasında not alanı olsun. v1'deki ile aynı."*

Yeni sıra: **künye → üç düğme (tek satır) → NOT → ruhsat (açılır) →
kilometre (açılır) → bakım bilgileri → geçmiş.**

Ruhsat ve kilometre **başvuru** bilgisidir — günde bir kez bakılır ama
ekranın yarısını kaplıyorlardı ve usta geçmişi görmek için iki ekran
boyu kaydırıyordu. Not ise aracın **en taze** bilgisi (*"arka cam kolu
kırık"*) ve ekranın dibinde kimse görmüyordu.

Üç düğme 375 px'e sığsın diye etiketler kısaldı (*Kayıt aç · Mesaj ·
Sesle*); 48 px dokunma hedefi korunuyor.

### 19.5 · Kilometre geçmişinde TAHMİN YOK

> *"Km geçmişinde tahmin için alanı olmasın. Tahmin de olmasın.
> Tarih ve km."*

Günlük ortalama satırı ve iki ölçüm arasındaki `+8.700` farkı **ekrandan
kalktı.**

**⚠️ HESAP KALKMADI.** K-002'nin *"ay veya km, hangisi önce gelirse"*
kuralı `gunlukKm()` ile çalışmaya devam ediyor; sonucu **Bakım
bilgileri** kartında **tek bir tarih** olarak duruyor. Kalkan şey,
ustanın önüne konan **ham tahmin**. Ara hesabı göstermek, ustadan onu
denetlemesini istemek demekti — o iş bizim.

### 19.6 · Araç geçmişi sadeleşti — ve K-005'in rozeti düştü

> *"Geçmiş altında listele: Başında icon yok. Bakım mı tamirat mı
> başlık. Marka model yazmasın. Ad soyad zaten belli. Başlık, açıklama,
> Tarih, teslim durumu. Basit olsun."*

**⚠️ BU BİR ÇELİŞKİ VE KAYBEDEN BİZİZ.** Araç ekranının var oluş
gerekçesi K-005'ti: *"bu parçayı geçen sefer müşteri getirmişti"*
bilgisi araç ikinci kez geldiğinde **karşıya çıkmalı** — çünkü
sorumluluğu o belirliyor (T-8: müşteri getirdiyse tekrar işçilik
alınır, tamirci aldıysa ücretsiz değişir). O bilgi geçmiş satırında
bir rozet olarak duruyordu; Cemil'in saydığı dört alanın içinde yok.

**Rozet kaldırıldı.** Tamircinin/Cemil'in sözü bizim akıl yürütmemizi
yener. Bilgi **kaybolmadı**: `SuppliedBy` satırın kendi verisi ve
kaydın kendi ekranında duruyor.

> **Cemil onayına açık (A-M3):** geçmiş satırında *"parçayı müşteri
> getirmişti"* uyarısı olmadan usta tartışmaya hazırlıksız yakalanır
> mı? Alternatif: liste sade kalır, **kaydı açınca** en üstte görünür.
> Şu anki hâl bu — kaydın kendi ekranında görünüyor.

---

## K-018 · Türkçe metinde `\b` kullanılmaz (04.09.2026)

**Karar:** Kelime atmak/aramak için JavaScript'in `\b` sınırı
**kullanılmaz.** Bunun yerine `kelimeleriAt()`: boşluktan böl,
eşitlikle karşılaştır.

**Neden — yaşanmış hata:** `/\bde\b/` deseni *"değişecek"* kelimesinin
başındaki **"de"yi tutuyordu.** JavaScript'in `\b`'si ASCII sınırına
bakar; `ğ`, `ş`, `ı`, `ç` kelime karakteri **sayılmaz**, o yüzden
"de" ile "ğişecek" arasında sınır var sanılıyor. Sesle *"balata da
değişecek"* denince kayda **"balata ğişecek"** düşüyordu.

Aynı tuzak `/\bara\b/` ile **"araç"** kelimesini **"ç"**ye indirirdi —
o hata henüz kimse söylemeden bulundu, çünkü aynı desen kullanılıyordu.

**Genel ders:** İngilizce için yazılmış metin araçları Türkçede sessizce
yanlış çalışır. Sessizce, çünkü hata mesajı yok — yalnız yanlış sonuç.

### ⚠️ 04.09 — AYNI TUZAĞA BU KARARDAN SONRA BİR KEZ DAHA DÜŞÜLDÜ

Sesle söylenen plakayı toparlamak için harf birleştirici yazdım ve
yine `\b` kullandım:
`/\b([A-Za-zÇĞİÖŞÜçğıöşü])(\s+[A-Za-zÇĞİÖŞÜçğıöşü])+\b/`

*"beş yüz"* içindeki **"ş y"** ikilisini "harf harf söylenmiş plaka"
sandı — çünkü `e`↔`ş` arasında da, `y`↔`ü` arasında da `\b` var.
Sonuç **"beşyüz"** oldu, *"kırk bir KV beş bin beş yüz seksen sekiz"*
plakası **41 KV 5000** olarak okundu.

**Kararı yazmak yetmiyor.** Kural artık şöyle keskin: *Türkçe metinde
kelime sınırı ARANMAZ, boşluktan BÖLÜNÜR.* İki fonksiyon da
(`kelimeleriAt`, `sesHarfleriBirlestir`) bu şekilde yazıldı.

---

## K-017 · Sesli işlem bağlamıyla açılır (03.09.2026)

**Karar:** Sesli işlem ekranı **hangi ekrandan/hangi mikrofondan**
açıldığını bilir; başlığında yazar (*"Sesli işlem · Araçlarda ara"*) ve
örneklerini ona göre gösterir. Bağlama geçiş **tek fonksiyondan**
yapılır: `sesleAc(niyet)`.

**Neden:** Ustanın *"ne diyebilirim"* sorusu geldiği yere göre farklı.
Muhasebeden gelen *"2400 TL elektrik faturası ekle"* der, açık servis
kaydından gelen *"balata da değişecek"*. Herkese aynı örnekleri
göstermek, ekranı **herkes için biraz yanlış** yapıyordu.

**Neden tek fonksiyon:** her mikrofon kendi başına `SESLI_NIYET = x;
Ekran.ac('ses')` yazıyordu ve ekran **zaten açıkken** `Ekran.ac()` erken
dönüyordu: bağlam değişiyor, başlık ve örnekler eski kalıyordu. Tek
giriş noktası olunca hata da tek yerde.

---

## K-016 · Ses, var olan işi kısaltır; yeni yetenek açmaz (03.09.2026)

**Karar:** Sesli işlemin yaptığı her şey **elle de yapılabilir.** Ses
yeni bir yol açmaz, var olanı kısaltır — yedi işin dışına çıkmaz.

**Dört hâl ve üçüncüsü:**
`dinliyor → duyuldu → NE YAPILACAĞI ANLAŞILDI → onay`
Üçüncü adım olmadan ses, bir arama kutusundan farksız kalır. Asıl iş
tanıma değil, **tanımadan sonrası**.

**⚠️ ONAYSIZ HİÇBİR ŞEY YAPILMAZ.** Yanlış duyulan bir cümlenin kaydı
kendiliğinden açması, ustayı sesten **tamamen** soğutur: bir kez yanlış
kayıt açan mikrofona ikinci kez basılmaz. Anlaşılan işlem yazıyla
gösterilir, usta onaylar.

---

## K-015 · Ses tanıma: tarayıcının kendi API'si (03.09.2026)

**Cemil:** *"Buna apiyi bağlayabilir miyiz? Ücretsiz bir api. Nasıl
çalışıyor, test etmiş oluruz."*

**Karar:** **Web Speech API** (`SpeechRecognition` /
`webkitSpeechRecognition`) kullanılıyor. Tarayıcının kendi motoru.

| | |
|---|---|
| Ücret | **Yok** |
| Anahtar / hesap | **Yok** — kayıt olmak gerekmiyor |
| Kurulum | **Yok** — `new webkitSpeechRecognition()`, `lang = 'tr-TR'` |
| Nerede var | Chrome, Edge · **Firefox'ta yok** |
| Şart | **Güvenli bağlam** (https ya da localhost) + mikrofon izni |

**⚠️ `file://` ile açılan dosyada çalışmaz.** Prototip şu an dosyadan
açıldığı için mikrofon açılmıyor; ekran bunu **yazıyor** ve örnek
cümleler her zaman duruyor. `app/` bir https adresine konursa mikrofon
çalışır.

**⚠️ GİZLİLİK — bu bir karardır, ayrıntı değil:** Chrome bu API'de sesi
**Google'ın sunucusuna gönderir.** Cihazda kalmaz. Tamirhanede
konuşulanlar (müşteri adı, telefon, plaka) bu yoldan dışarı çıkar.
Ücretsiz olmasının bedeli bu.

**Cemil onayına açık (A-S3):** kabul mü, yoksa cihazda çalışan bir
motor mu aranmalı? Alternatifler ücretli (Google STT, Azure) ya da
ağır (cihaz üstü Whisper). Öneri: **prototipte kabul**, gerçek
kullanıma geçerken yeniden konuşulur.

---

## K-014 · Gider girişi sesle mümkün; kâr yine hesaplanmıyor (03.09.2026)

**Karar:** Muhasebeye **gider girişi eklendi** — ama yalnız sesle ve
elle giriş; **kâr ve cari hesap hâlâ hesaplanmıyor.**

**K-013'ün itirazı neydi:** *"Gider için ustanın her gideri girmesi
gerekir; girilmezse rakam yalan söyler."*

**Neden zayıfladı:** Cemil *"x TL elektrik faturası ekle gibi
örnekler"* isteyince giriş maliyeti düştü. **Söylemek form doldurmak
değil.** Mikrofona basıp *"2400 TL elektrik faturası ekle"* demek,
ustanın zaten yaptığı bir şeyin yanına eklenebilir.

**Neden tamamen kalkmadı:** girilen gider görünür, **girilmeyen
görünmez.** Eksik giderle çıkan kâr, kâr olmamasından kötüdür — çünkü
usta ona *inanır*. Ekran bu yüzden giderlerin **"girildiği kadar
doğru"** olduğunu yazıyor.

---

## K-013 · Muhasebe, yeni alan istemeden başlar (03.09.2026)

**Karar:** Muhasebe ekranı yalnız **zaten kayıtlı olan** veriyi okur:
teslimdeki `PaymentMethod` ve satırlardaki `Price`. Gider, kâr, cari
hesap ve fatura **yazılmadı** — kapsam kararı verilene kadar da
yazılmayacak.

**Neden:** Bu dördü ustadan **yeni veri** ister. *"Her gideri gir"*
diyen bir ekran, girilmediğinde **yalan söyleyen** bir ekrandır. Yanlış
rakam gösteren muhasebe, muhasebe olmamasından kötüdür.

**Bedavaya gelen üç rakam:** bugün tahsil edilen · ödeme yoluna göre
dağılım · **veresiye listesi.** *"Almadım"* bir ödeme yolu olduğu için
(K-004 teslim akışı) alacak zaten kayıtlı. Liste olarak veriliyor,
tek rakam olarak değil: ustanın sorusu *"ne kadar alacağım"* değil,
**"kimden alacağım"**.

**Ekran kendi yarım olduğunu yazıyor.** Bir ekranın eksiğini söylemesi,
saklamasından iyidir.

**Cemil onayına açık:** kapsam. *"Sonra geçeceğiz"* dedin — seçenekler
günlük kasa · kasa+gider · cari hesap olarak duruyor.

---

## K-012 · Müşteri ekranı var, müşteri tablosu yok (03.09.2026)

**Karar:** *"Müşteriler"* ekranı açıldı ama `Customer` tablosu
**açılmadı**. Liste araçtan türetiliyor: aynı `sahipAd` + `sahipTel`
tek satırda toplanıyor, araçları altında sıralanıyor.

**Neden:** v1'de müşteri dünyası **üç tabloydu** (`Party`, `Contact`,
`VehicleContact`) ve tamircinin *"karmaşık"* dediği yığının içindeydi.
İhtiyaç duyulan şey **ekrandı** — *"Ahmet kimdi, hangi arabaydı"* —
tablo değil. Ekranı tablosuz vermek mümkün olduğu sürece tablo
açılmaz.

**Bedeli açık ve kabul edildi (A-M1):** aynı kişinin adı iki araçta
farklı yazılmışsa **iki müşteri görünür.** Birleştirme yok. Tamirci
bundan şikâyet ederse tablo o zaman konuşulur — şikâyet etmeden değil.

---

## K-011 · Ana ekran menüdür; her satırın kendi mikrofonu vardır (03.09.2026)

**Karar:** Ana ekran, *"içerideki araçlar"* listesi olmaktan çıkıp
**menü** oldu: en üstte Sesli işlem, altında altı satır (araç kaydı ·
araçlar · servis kaydı ekle · servis kayıtları · müşteriler ·
muhasebe), **her satırın sağ ucunda mikrofon.**

**Cemil:** *"Yine anasayfada buttonlar olmalı… Yine buttonların yanında
mikrofon buttonu olacak."*

**Mikrofon neden her satırda:** iş hem dokunarak hem söyleyerek
başlatılabilmeli. Mikrofonu tek bir ekrana hapsetmek, sesle başlamak
isteyen ustayı önce **gözle aramaya** zorluyordu — sesin bütün kazancı
orada gidiyor.

**Kaybolan bir şey yok:** içerideki araçlar listesi kendi ekranına
geçti ve orada *"içeride"* süzgeci **açık** geliyor. Bakım hatırlatma
şeridi ana ekranda **kaldı** — ustanın *"bugün kimi arayacağım"*
sorusu ilk ekranda cevaplanmalı (T-17).

**Ölçüldü:** ana yol yine **13 dokunuş.** Menü satırı, eski
*"Ruhsatı çek"* düğmesinin yerini aldı; bir tık eklemedi.

**T-18 (*"iç içe menü yok"*) ne oldu:** menü **tek katman**. Menüden
açılan hiçbir ekranın kendi menüsü yok; hepsi liste ya da tek iş.
Yasak olan iç içelik, menünün kendisi değil.

---

## K-010 · Tema üç hâllidir; "sistem" gerçek bir seçenektir (03.09.2026)

**Karar:** Tema üç hâl arasında döner: **sistem · açık · koyu.** Seçim
yapılmamışken kökte `data-theme` **bulunmaz** ve `prefers-color-scheme`
devreye girer. Seçim `localStorage`'da tutulur; erişilemediği yerde
(gizli sekme, `data:` URL) tema yine çalışır.

**Neden:** Kökte sabit `data-theme="light"` yazılıydı. Telefonunu akşam
koyuya alan usta, uygulamanın da geçmesini bekler; sabit değer onun
tercihini **sessizce eziyordu.** İki hâlli bir anahtar bu tercihi
ifade edemez — "sistem"i atmak, kullanıcının kararını atmaktır.

**Düğme şu anki hâli söyler**, bir sonrakini değil: v1'de *"Koyu tema"*
yazan düğme koyu temadayken de aynı şeyi yazıyordu ve hangi hâlde
olunduğu okunamıyordu.

---

## K-009 · Çekmece geri geldi (03.09.2026)

**Karar:** `TASARIM-SISTEMI.md` v2.0'daki *"çekmece yok"* kararı geri
alındı. Header v1'in düzenine döndü: menü (rozetli) · logo · ara ·
geri · ileri.

**Cemil:** *"Header aynı şekilde tutmamız gerekiyordu. Tema konusu da
devam edecek."*

**Kararın hatası:** çekmeceyi kaldırırken **içindekilerin nereye
gideceği** sorulmamıştı. Tema anahtarının evi çekmeceydi. Bir kabı
kaldırırken içindekiler de kalkmaz — kalkarsa sessizce kaybolurlar,
ki tam bu oldu.

**T-18 (*"iç içe menü yok"*) nasıl korunuyor:** ana yolun **hiçbir
adımı çekmeceden geçmiyor.** Ölçüldü — çekmece hiç açılmadan tam yol
yine 13 dokunuş. Çekmece bir **kestirme**, bir yol değil.

**Çekmecenin içi v2'nin:** v1'in on yedi maddesinden üçü kaldı (araç
girişi, içerideki araçlar, plaka ara) + bakım periyodu + tema.
Bildirimler uydurma değil, vadesi gelmiş bakım hatırlatmaları.

**Cemil onayına açık:** rozet yalnız bakım hatırlatmalarını sayıyor.
İleride başka bildirim türü girerse rozet ne sayacak?

---

## K-008 · Tasarım sistemi bağlayıcı bir sınıf sözlüğüdür (03.09.2026)

**Karar:** Bir ekran, `TASARIM-SISTEMI.md`'nin sonundaki sınıf sözlüğünde
olmayan bir bileşen sınıfı **açamaz**. Yeni bir görsel ihtiyaç varsa önce
sözlüğe girer, sonra kullanılır.

**Neden:** Tasarım sistemini v1'den **belge olarak** taşımıştık ve belge
doğruydu. Yine de altı ekranı yazarken bileşenleri sıfırdan uydurdum
(`.ana-btn`, `.kart`, `.plaka`, `.ekran-bas`, `.tutar-buyuk`). Prototip
yabancı çıktı. Cemil: *"Eski tasarımla hiç ilgisi yok. Tasarım eskisi gibi
devam etmeli. Yazdık bunları."*

*"Kalem listesi kullanılır"* yazması, ekranın `.kalem .k-tip .k-ad`
yazacağını garanti etmiyor. Bağlayıcı olan **sınıf adının kendisi.**

**Bekçisi denetim:** `TANIMSIZ-SINIF` sözlükte olmayanı, `OLU-SINIF`
sözlükte kalıp kullanılmayanı yakalar. Bu iş sırasında ikisi birlikte
**60 bulgu** çıkardı; içlerinde taşındığı sanılan ama taşınmamış dokuz v1
kuralı vardı (`.btn-primary`, `.btn-ghost`, `.btn-lg`, `.plate-sm`,
`.plate-lg`, `.arac-bas`, `.veh-title`, `.spec-strip`, `.k-katalog`) —
ana düğmeler **zeminsiz**, liste plakaları **büyük** çiziliyordu.

---

## K-007 · Kol B'de cevabın kaynağı satırda tutulur (03.09.2026)

**Karar:** Birden fazla parçacıya sorulduğunda hangi cevabın kullanıldığı
**satırın kendisinde** tutulur (`ServiceLine.SelectedReplyId`). Ayrı bir
fiyat karşılaştırma ekranı **açılmaz**; çipler yalnız birden fazla
parçacıya sorulmuşsa görünür.

**Neden:** v1'de bu bir **ekran ailesiydi** (karşılaştırma tablosu, en
uygun seçili, teklif detayı) ve tamirci *"karmaşık"* dediği yığının
içindeydi. Ama bilgi gerçek: `PriceRequest` modelde zaten her parçacı için
ayrı kayıt. Bilgiyi tutup ekranı açmamak mümkün — çünkü soru *"hangisi
daha ucuz"* değil, *"bu satırın fiyatı kimden geldi"*.

**Ana yola maliyeti sıfır:** tek parçacıya sorulduğunda çip çıkmıyor,
ekran ana yoldaki hâlinden farksız.

**Cemil onayına açık:** usta hiçbirini seçmezse alan boş kalıyor ve akış
duruyor mu? Öneri: **durmaz** — rakam satırda, kaynağı bilinmese de iş
yürür.

---

## K-006 · Tamirat ve bakım iki ayrı türdür (03.09.2026)

**Karar:** Servis kaydının bir **türü** vardır: `tamirat` | `bakim`. Kayıt açılırken seçilir.

**Gerekçe (T-15):** Tamirci açıkça *"tamirat ve bakım iki ayrı durum var"* dedi. İkisi farklı davranıyor:

| | Tamirat | Bakım |
|---|---|---|
| Neden geliyor | Bir şey bozuldu | Zamanı geldi |
| km | Bilgi | **Anlamlı** — bir sonrakinin dayanağı (K-002) |
| Tekrarı | Belirsiz | **Öngörülebilir** → hatırlatma (K-002) |
| Parça | Değişken | Çoğunlukla aynı küme (yağ, filtreler) |

**Kapsam dışı:** Alt türler (periyodik/ara bakım, ağır/hafif tamirat) yok. İki değer, o kadar.

**⚠️ Bu, v1'in K-029 kararının TERSİ.** Orada *"baştan tür seçilmez, durum akışta belirir"* denmişti. O karar bizim çıkarımımızdı; bu, kullanıcının sözü. Kullanıcı kazanır.

---

## K-005 · Parçayı kim getirdi — sorumluluk anahtarı (03.09.2026)

**Karar:** Her parça satırında **`kimGetirdi`** alanı: `tamirci` | `musteri`. Kayıtta ve araç geçmişinde **görünür** durur.

**Gerekçe (T-7, T-8):** Bu bir bilgi alanı değil, **ileride kimin ödeyeceğini belirleyen anahtar**:

- **Tamirci aldıysa** → parça sorun çıkarınca **ücretsiz** değiştirilir (parçanın sorumluluğu onda)
- **Müşteri getirdiyse** → **tekrar işçilik alınır** (parçayı seçen o)

Araç ikinci kez geldiğinde bu bilginin **karşına çıkması** gerekir; yoksa tartışma çıkar.

**Ek (S2):** Müşteri parçasının satırı **fiyatsız ya da 0 TL** durur. Ayrı bir mekanizma kurulmaz — sıradan bir satır, tutarı yok.

**Kapsam dışı — GARANTİ SÜRESİ TAKİBİ YOK (S3).** Cemil: *"Garanti süresi yok."* Ne bitiş tarihi, ne kalan gün, ne uyarı. v1'deki garanti sekmesi ve süre hesabı tamamen düşer. Tutulan tek şey: **kim getirdi.**

---

## K-004 · Tek durum: teslim edildi mi (03.09.2026)

**Karar:** Servis kaydının **tek durum alanı** vardır: **teslim edildi / edilmedi.**

**Gerekçe (T-6, S1):** Cemil: *"Sadece teslim durum olarak kalacak."*

v1'de yedi durum vardı — *fiyat bekliyor · parça bekliyor · onay bekliyor · işlemde · hazır · red · kapandı.* Her biri ekran, filtre, rozet ve geçiş kuralı üretiyordu. Tamircinin gerçekte sorduğu tek soru: **"bu araç çıktı mı, hâlâ bende mi?"**

**Sonuç:** Durum makinesi diye bir şey kalmıyor. Açık iş listesi = teslim edilmemiş kayıtlar.

**Kapsam dışı:** Durum filtreleri, durum rozetleri, duruma göre hızlı eylemler, durum geçiş kuralları.

**Cemil onayına açık:** *"Parça bekliyor"* bilgisi tamircinin işine yarıyor olabilir (bugün ne yapabileceğini söyler). Ama bu bir **durum** değil, kaydın kendi verisinden **türetilebilir** ("fiyatı sorulmuş ama parçası gelmemiş"). Önerim: durum olarak tutulmasın, listede gösterilsin. Akış çizilirken (3. iş) bakılacak.

---

## K-003 · Onay bir akış değil, isteğe bağlı bir eylemdir (03.09.2026)

**Karar:** Müşteri onayı **zorunlu adım değildir.** Varsayılan: usta işi ekler ve devam eder. İsteyen **"onay iste"** düğmesine basar; kayda o zaman bir onay isteği düşer.

**Gerekçe (T-10, S4):** Cemil, tamircinin anlattığını aktarıyor:

> *"Ruhsatı çekti, parçayı aldı, lifte kaldırdı. Bu sırada balataları gördü ve dedi ki balatalar nanay. Müşteri tamam derse, değişiyor. Burada müşteri onayı vb işin içine katmayalım."*

Onay **zaten alınıyor** — sözlü, yüz yüze, telefonda. Sisteme onay adımı koymak var olan bir şeyi ikinci kez yaptırmak olur.

> *"Onay iste diye ayrıca bir alan olabilir. O zaman müşteri onayı gerekebilir."*

Yani onay bir **eylem**, bir **kol** değil.

**Bizi ilgilendiren tek şey (S4):** *"Yapılan işe ek iş yapılabilir teslim etmeden önce."* Kayıt teslim edilene kadar **açıktır ve satır eklenebilir.** Hepsi bu.

**Kapsam dışı — v1'in bütün onay kolu:** onay ekranı, "müşteriye gönder · onay iste" akışı, red paneli, red sebebi çipleri, indirim teklifi, "yeniden gönder", "yaptırmıyor" çıkışı, `onay`/`red` durumları. v1'in ekranlarının yaklaşık dörtte biri buydu.

---

## K-002 · Bakım takibi: km kaydedilir, hatırlatma ay üzerinden (03.09.2026)

**Karar:**
- **km** bakım kaydında **bir kez yazılır** — araç geldiğinde usta göstergeden okur. Sürekli takip edilmez.
- **Hatırlatma** kayıt tarihinden **ay** sayılarak kurulur.

**Gerekçe (T-16, T-16b, S5):** Görünürde çelişki vardı:

- Tamirci: *"Bakımda km önemli."*
- Cemil: *"Km'yi takip edemeyiz sanırım, ay olarak takip edilebilir."*

Çelişki değil, **iki ayrı iş.** Km'yi *sürekli izlemek* imkânsız — araç dükkânda değil, kimse bize km göndermiyor. Ama *o anki km'yi yazmak* bedavadır ve geçmişte değerlidir: *"geçen bakım 142.000'de yapılmış"* ustanın işine yarar. **Hatırlatmanın dayanağı ise zamandır**, çünkü elimizde ölçebildiğimiz tek şey o.

**Cemil onayına açık:** Varsayılan periyot kaç ay? Öneri: **6 ay**, dükkân ayarından değiştirilebilir, kayıtta da elle değiştirilebilir.

**~~Kapsam dışı: km tahmini, km-bazlı bildirim.~~**

---

### ⚠️ 04.09.2026 — GÜNCELLENDİ: ay **veya** km, hangisi önce gelirse

Cemil sordu (*"Bakım zamanı hatırlatması neye göre olacak?"*) ve
**ay + km, hangisi önce gelirse** seçeneğini seçti. Yukarıdaki
*"hatırlatmanın dayanağı zamandır"* cümlesi artık **yarım doğru**.

**Yeni kural:**

| | |
|---|---|
| Ay hedefi | Teslim + `bakimPeriyoduAy` (varsayılan 6) |
| Km hedefi | Son bakımın km'si + `bakimPeriyoduKm` (varsayılan 10.000) |
| Hangisi kurulur | **Önce geleni** |

**Km hedefi tarihe nasıl çevriliyor:** aracın **kendi geçmişinden.**
İki bakım arasındaki km farkı ve gün farkı, günlük ortalamayı veriyor;
10.000 km'nin kaç günde dolacağı buradan çıkıyor.

> **⚠️ TEK ÖLÇÜMLE TAHMİN YAPILMAZ.** Bir aracın tek km kaydı varsa
> günlük ortalama **uydurulmaz** ve ay tek başına çalışır. Uydurma bir
> ortalama, ustayı yanlış güne çağırır — hatırlatmanın tek işi doğru
> günde çalmaktır.

**Eski gerekçe neden çürüdü:** *"km'yi izleyemiyoruz"* doğruydu ama
**izlemek gerekmiyormuş.** Araç her gelişinde km zaten yazılıyor; iki
nokta bir eğim veriyor. İzleme değil, **iki ölçüm** yetiyor.

**Ölçüldü (04.09):** 34 ABC 123 · 15.03'te 142.500, 20.06'da 151.200 →
97 günde 8.700 km → günde **89,7 km** → 10.000 km **09.10.2026**'da
dolar; 6 ay ise 20.12.2026. **Km kazanıyor**, hatırlatma 09.10'a
kuruluyor.

**Kapsam dışı kalmaya devam ediyor:** araçtan otomatik km okuma,
müşteriden km sorma.

---

## K-001 · Parça uyumunu parçacı çözer; TecDoc ana yoldan çıkar (03.09.2026)

**Karar:** Uyumlu parçayı bulmak **bizim işimiz değil, yedek parçacının işidir.** Ona ruhsatı ve istenen parçayı göndeririz, o uygun parçayı bulup fiyatıyla döner. **TecDoc / `Fitment` verisi ana yolun ön koşulu değildir.**

**Gerekçe (T-12, T-13, T-14, S7):**

Tamircinin akışı zaten böyle işliyor: *"Yedek parçacıdan fiyat alırken, ruhsatı ve gereken parçaları gönderiyor."* Cemil: *"Yedek parçacı kolayca bu işi çözüyor."* Uyum bilgisi **parçacının kataloğunda zaten var** ve bunu günde yüz kere yapıyor.

**⚠️ Bu, v1'in en pahalı kalemini kapsam dışına çıkarır.** v1'de A-11.3 maddesi *"ertelenemez"* diye işaretliydi: *"şasi no ve marka/model ile uyumsuz parçalardan kaçınmak `Fitment` verisi olmadan yapılamaz."* Doğruydu — **eğer uyumu biz bulacaksak.** Bulmuyoruz.

**Ayrım (Cemil'in asıl istediği):**

| | Uyumu bulmak | Ne satıldığını bilmek |
|---|---|---|
| Kim için | Tamirci — işi yapabilsin | **Biz** — veri biriksin |
| Ne zaman | Fiyat sorulmadan **önce** | Cevap geldikten **sonra** |
| Yanılırsa | **İş durur, yanlış parça takılır** | Bir satır eksik kalır |
| Maliyeti | Eksiksiz lisanslı veri | Kullandıkça biriken kendi verimiz |

Cemil'in istediği **sağ sütun**: *"hangi parçanın satıldığını tespit etmek için araya bir işlem sokmaya çalışıyorum."* Bu **akışı tıkamaz** — 2. işte (`PARCA-TANIMLAMA.md`) detaylandırılıyor.

**Kapsam dışı (şimdilik):** TecDoc lisansı, `Fitment` tablosu, VIN'den araç tipi çözümleme (NHTSA vPIC), araca göre parça süzme. Devralınan araştırma notları (`arastirma/A-11-*`) **arşiv olarak duruyor** — ihtiyaç doğarsa oradan devam edilir.

**Cemil onayına açık:** *"Bazı parçacılar kod vermiyor, sadece fiyat veriyor."* Bu durumda satır **"tanımlanamadı"** olarak kalır ve akış durmaz. Kabul mü?

---

## AÇIK — karara bağlanacak

| Kod | Konu | Ne zaman |
|---|---|---|
| **A-1** | **Ekran bütçesi** — ana yol için en fazla 6 önerildi. Onay? | Cemil |
| **A-2** | Müşteri tarafı (T-19/T-20/T-21) **bu fazda mı**? Öneri: özet linki evet, müşteri uygulaması sonra | Cemil |
| **A-3** | Bakım periyodu varsayılanı kaç ay? Öneri: 6 | Cemil |
| **A-4** | *"Parça bekliyor"* listede türetilerek gösterilsin mi (K-004 notu) | Akış çizilirken |
| **A-5** | Ruhsat okuma hangi teknolojiyle — OCR servisi, doğruluk, elle düzeltme | 9. iş |
| **A-6** | WhatsApp API sağlayıcısı ve maliyeti | 8. iş |
| **A-7** | SMS sağlayıcısı + **İYS kaydı** (yasal zorunluluk) | 10. iş |
| **A-M2** | Özet/hatırlatma hangi numaraya gitsin — ruhsattaki mi, ikinci iletişim mi? Varsayım: **ikinci iletişim varsa o** (K-019.3) | Cemil |
| **A-M3** | Araç geçmişinde *"parçayı müşteri getirmişti"* uyarısı gerçekten gereksiz mi (K-019.6) | Cemil |
| ~~A-M4~~ | **KAPANDI (K-020.3)** — ekran görüntüsü gösterdi ki başlık bildirimi örtüyordu; yapışkanlık kaldırıldı | — |
| **A-M5** | Arama mikrofonu Cemil yalnız **araçlar** için istedi; **dört listeye birden** kondu. Kalsın mı? | Cemil |
| ~~A-M2~~ | **UYGULANDI (K-021.3)** — ikinci iletişim varsa mesaj ona gider, usta hedefi değiştirebiliyor | Onay bekliyor |
| **A-M6** | *"Onay iste"* düğmesi **"Özet gönder"** oldu — ad doğru mu? | Cemil |
| **MUH** | **Muhasebe kapsamı** — K-022 taslağı hazır (öneri: B, kasa+gider). Onay? | Cemil |
| **A-M7** | **İş başına kâr** (`fiyat − alış`) kayıt ekranında gösterilsin mi? Önerildi, yazılmadı | Cemil |
| **A-M8** | **Karar bütçesi** kısıt olarak kabul mü (menü ≤8 · ekran başına karar ≤3 · jargon 0) | Cemil |
| **A-M11** | Detaylı menüdeki beş modül (fatura/KDV · çek · senet · cari) **kapsama girdi mi**, yoksa yeri mi açıldı? | Yeri açıldı; K-022 duruyor |
| **A-M10** | *"Tamirat + Bakım"* tek kayıt mı olmalı, yoksa liste satırında iki kaydın özeti mi? | Tek kayıt varsayıldı |
| **A-M9** | Özet sayfasında **kimlik doğrulama yok**, link tahmin edilebilir (plaka + kayıt no). Backend fazında kapatılacak — kabul mü | Cemil |
| ~~A-2~~ | **Özet linki YAZILDI (K-023).** Müşteri uygulaması hâlâ sonraya | Kısmen kapandı |
