# TAMIRON — KARARLAR (KARARLAR.md)

> Verilen kararların gerekçeleri. Aynı tartışma iki kez açılmasın. Yeni karar **en üste** eklenir.
>
> **Bu liste 03.09.2026'da SIFIRDAN başladı.** v1'in ürün/akış kararları bilinçli olarak devralınmadı — tamircinin geri bildirimi (`arastirma/TAMIRCI-GERIBILDIRIM.md`) akışı yeniden kuruyor ve eski kararların gündemde durması yeni tartışmayı kirletiyordu.
>
> **Devralınan mimari kararlar `MIMARI.md`'dedir** ve burada tekrarlanmaz: teknoloji yığını, üç uygulama ayrımı, tenant/yetki altyapısı, soft-delete, katalog hibrit yapısı, Türkçe UI / İngilizce kod, tokens tek kaynak, tasarım-önce. Bunlar akıştan bağımsızdır — değişecekse `MIMARI.md` açılır.
>
> v1 arşivi: `github.com/DevCemilSaglam/Tamiron-v1` (195 commit; her kararın gerekçesi commit mesajında).

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
