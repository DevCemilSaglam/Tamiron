# YAPAY ZEKA — dört iş, tek boru, bir şart

> **Tarih:** 14.09.2026 · **Soran:** Cemil —
> *"Ses, parça listesi okuma, ruhsat okuma, sesi modele çevirip iş
> yapılabilmesi (servis kaydı açmak vb) bunları da artık yapabiliyor
> olmalıyız. Bunu nasıl yaparız?"* + *"Mesaj gönderirken, mesajı yapay
> zeka düzenleyecek."*
>
> **Bu belge karar vermez, kararı hazırlar.** Model adları, fiyatlar ve
> API davranışı **14.09.2026 tarihli Claude API referansından** alındı;
> hafızadan yazılmadı.

---

## 1 · Dört iş, aynı boru

| # | İş | Girdi | Çıktı | Bugün prototipte |
|---|---|---|---|---|
| 1 | **Sesli komut anlama** | metin | **işlem nesnesi** (JSON) | elle yazılmış çözümleyici |
| 2 | **Ruhsat okuma** | fotoğraf | araç alanları (JSON) | sahte — düğme var, okuma yok |
| 3 | **Parça listesi okuma** | fotoğraf | satırlar (JSON) | sahte |
| 4 | **Mesaj düzeltme** | taslak metin | düzeltilmiş metin | yok |

Dördü de **aynı boruyu** kullanıyor: *girdi → model → **yapılandırılmış
çıktı** → ustanın onayı → uygulama.* Farklı olan yalnız girdinin cinsi
ve istenen şemanın şekli.

> ⚠️ **ONAY EKRANI YAPAY ZEKAYLA BİRLİKTE KALKMAZ, ÖNEMLİ HÂLE GELİR.**
> K-017 bugün sahte çözümleyici için yazılmıştı: *"onaysız hiçbir şey
> yapılmıyor."* Model geldiğinde bu kural gevşemez — çünkü model de
> yanılır ve yanıldığında **kendinden emin** yanılır. Prototipte kurulu
> olan onay ekranı, bu işin en pahalı parçasıydı ve **zaten yazılmış
> durumda.**

---

## 2 · Ses: model sesi DUYMUYOR — iki parçalı iş

**Claude API metin ve görüntü alır; ses almaz.** Yani *"sesi modele
çevirip"* cümlesi iki ayrı adım demek:

```
  usta konuşur
        |
        v
  [1] SES -> METİN        tarayıcının Web Speech API'si (bugün kullanılan, ücretsiz)
        |                  ya da paralı bir konuşma tanıma servisi
        v
  [2] METİN -> İŞLEM      Claude (bugün: elle yazılmış çözümleyici)
        |
        v
  onay ekranı -> uygulama
```

**Bugün ikinci adımı biz yazıyoruz** ve sınırı belli: `sesCozumle` bir
desen eşleştiricidir. *"34 ABC 123 triger seti değişecek"* anlıyor;
*"şu Mégane'ın trigerini değiştirdik, balatalara da baktık, yarın
alacak"* anlamıyor. Modelin kazandıracağı şey **bu ikinci cümle.**

**Birinci adım değişmiyor** — ve bu iyi haber: tanıma tarayıcıda,
bedava ve çevrimdışı kurulum istemiyor. Kötü haber de orada: tanıma
*"34 QBC 123"* duyarsa model de o plakayı görür. Bu yüzden 14.09'da
yazılan *"bunu mu demek istedin"* önerisi (A-M27) modelden **sonra da**
gerekli.

---

## 3 · Ruhsat ve parça listesi: fotoğraf doğrudan modele

`RUHSAT-OKUMA.md` üç yolu karşılaştırmış ve **C yolunu** önermişti:
*"fotoğrafı modele verip alan alan JSON istemek."* O belgenin
gerekçesi aynen duruyor:

* Cihazda OCR (A) ve bulut OCR (B) **ham metin** veriyor; hangi metnin
  hangi alan olduğunu **biz** ayrıştırıyoruz ve ruhsatın düzeni
  değişince ayrıştırıcı kırılıyor.
* Görüntü anlayan model **doğrudan alanları** veriyor ve düzen
  değişikliğine dayanıyor.

**Parça listesi aynı işin kardeşi:** parçacının WhatsApp'tan attığı
irsaliye ya da el yazısı liste. El yazısı, A ve B yollarının en zorlandığı
girdi; görüntü anlayan model burada açık ara önde.

**Hacim küçük:** her araç bir kez okunur (kayıtlı araç ikinci kez
okunmaz), ayda 150–300 ruhsat. Maliyet kararı vermiyor — **doğruluk
veriyor.**

> ⚠️ **RUHSAT FOTOĞRAFI SAKLANMAZ** (`RUHSAT-OKUMA.md` Bulgu 2, A-31):
> ruhsatta **TC kimlik no ve adres** var, bize gerekmiyor. Okunur,
> alanlar alınır, fotoğraf atılır. Modele giden fotoğraf da bu kuralın
> dışında değil — API'ye gönderilen görüntü, gönderdiğimiz veridir.

---

## 4 · ŞART: anahtar tarayıcıda duramaz

**Bu, bu belgedeki tek "olmazsa olmaz".**

API anahtarı bir şifredir. Tarayıcıya konan anahtar **herkese** konmuş
demektir: PWA'nın kaynağını açan, ağ sekmesine bakan ya da telefonunu
başkasına veren herkes onu alır ve **bizim hesabımızdan** harcar.

```
  BUGÜN (prototip)                YAPAY ZEKA İLE
  ----------------                --------------
  tarayıcı                        tarayıcı
     |  (hiçbir yere gitmiyor)       |  fotoğraf / metin
     v                               v
  omurga.html                     BİZİM SUNUCUMUZ   <- anahtar BURADA
                                     |
                                     v
                                  Claude API
```

`MIMARI.md` zaten bir backend öngörüyor (**ASP.NET Core Web API +
MSSQL**). Yapay zeka o backend'in ilk gerçek işi olur: üç uç nokta
yeter —

| Uç nokta | Ne alır | Ne döner |
|---|---|---|
| `POST /api/zeka/komut` | ustanın cümlesi + bağlam (hangi ekran, açık kayıt) | işlem nesnesi (JSON) |
| `POST /api/zeka/ruhsat` | fotoğraf | araç alanları (JSON) |
| `POST /api/zeka/parca-listesi` | fotoğraf | satır listesi (JSON) |
| `POST /api/zeka/mesaj` | taslak metin + kime | düzeltilmiş metin |

**Backend'in ikinci işi denetim:** kaç çağrı, ne kadar harcandı, hangi
dükkân. Anahtar tarayıcıdayken bu sorular cevaplanamaz.

---

## 5 · Model ve maliyet

Fiyatlar **1 milyon jeton** başına (14.09.2026 referansı):

| Model | Kimlik | Bağlam | Girdi | Çıktı |
|---|---|---|---|---|
| Claude Opus 5 | `claude-opus-5` | 1M | $5 | $25 |
| Claude Sonnet 5 | `claude-sonnet-5` | 1M | $2 | $10 |
| Claude Haiku 4.5 | `claude-haiku-4-5` | 200K | $1 | $5 |

### Bir dükkânın günü — kaba hesap

Günde 20 sesli komut · 5 ruhsat · 2 parça listesi · 10 mesaj düzeltmesi
= **37 çağrı**. Çağrı başına yaklaşık girdi/çıktı (fotoğraf ~1.500–2.500
jeton, cümle ~1.200 jeton):

| Kurulum | Günlük | Aylık (dükkân başına) |
|---|---|---|
| Hepsi **Opus 5** | ~$0,45 | **~$13** |
| Hepsi **Sonnet 5** | ~$0,18 | **~$5,5** |
| Ses+mesaj **Haiku**, fotoğraflar **Sonnet** | ~$0,10 | **~$3** |

> **Karar maliyetle verilmiyor, yine doğrulukla veriliyor.** Aylık 3 ile
> 13 dolar arasındaki fark bir dükkân için küçük; **yanlış okunan bir
> şasi numarası** ya da yanlış açılan bir servis kaydı büyük. Bu yüzden
> §7'deki ölçüm yapılmadan model seçilmez.

**İki ucuzlatma yolu, kalite düşürmeden:**
* **İstem önbelleği (prompt caching):** sistem istemi ve şema her
  çağrıda aynı — önbelleğe alınırsa girdi maliyetinin büyük kısmı düşer.
* **Toplu iş (Batch API, %50 indirim):** acelesi olmayan işler için —
  bizde acelesi olmayan bir iş **yok** (usta aracın başında bekliyor),
  o yüzden bu yol bize uymuyor. Yazıyorum ki bir daha araştırılmasın.

---

## 6 · Yapılandırılmış çıktı — sözleşme ZATEN YAZILI

Modelin serbest metin döndürmesi işimize yaramaz; ekranın beklediği
**nesneyi** döndürmeli. Claude API bunu iki yoldan garanti ediyor:
şemaya bağlı çıktı (`output_config.format`) ya da `strict: true` ile
araç çağrısı.

**Ve şema uydurulmayacak — zaten var.** Prototipin çözümleyicisi bugün
tam olarak bu nesneyi üretiyor:

```
  { tur: 'servis-ac',    plaka, isTuru, km, is }
  { tur: 'mesaj-gonder', plaka, sablon, kanal, govde }
  { tur: 'fiyat-guncelle', kayit, indeks, fiyat }
  { tur: 'teslim',       plaka, kayit, odeme }
  { tur: 'gider-ekle',   tutar, aciklama }
  { tur: 'ara',          hedef, terim, suzgec }
  ...
```

Yani model **prototipin yerine değil, çözümleyicinin yerine** geçiyor.
`sesUygula()`, onay ekranı, balonlar, geri alma — hepsi olduğu gibi
kalıyor. Bu, bu belgedeki en büyük kazanç: **değişecek yüzey bir
fonksiyon.**

---

## 7 · "Yapay zeka ile test etmek" — asıl soru buydu

Cemil'in sorusu *"nasıl yaparız"*dı; en zor parçası yazmak değil,
**çalıştığını bilmek.**

### 7.1 · Elimizde zaten bir ölçüm aleti var

`window.SES_COZUMLE(cumle)` prototipte açık duruyor ve 45. denetim
testi her örnek cümleyi işletiyor. Buna **beklenen çıktı listesi**
eklenince değerlendirme seti (eval) olur:

| Cümle | Beklenen |
|---|---|
| *"34 ABC 123 triger seti değişecek"* | `servis-ac` · plaka · iş |
| *"şu Mégane'ın trigerini değiştirdik"* | `servis-ac` · plaka **model bulacak** |
| *"balatalara da baktık"* | `satir-ekle` |
| *"tüm müşterilere bayram mesajı"* | `toplu-mesaj` |

**Aynı liste iki motora verilir** — bugünkü çözümleyici ve model — ve
sonuçlar karşılaştırılır. Kazanan cümle cümle belli olur; *"model daha
iyi"* demek zorunda kalmayız, **hangi cümlede** daha iyi olduğunu
gösteririz.

### 7.2 · Fotoğraf tarafı için elde veri yok

Ses için cümle uydurabiliriz; ruhsat ve parça listesi için **gerçek
fotoğraf** gerekiyor:

* 20–30 gerçek ruhsat fotoğrafı — **atölye ışığında, buruşuk, yağlı
  parmak izli** (RUHSAT-OKUMA §3'ün sorduğu iki sorudan biri buydu)
* 10–15 gerçek parça listesi/irsaliye — **el yazısı olanlar dahil**

Ölçülecek şey iki rakam: **alan başına doğruluk** ve **ustanın kaç alan
düzelttiği.** İkincisi asıl olan — %95 doğruluk, her ruhsatta bir alan
düzeltmek demektir ve usta bunu üç ruhsattan sonra "bu iş çalışmıyor"
diye okur.

> **Cemil'den istenen tek şey bu:** bir avuç gerçek ruhsat ve parça
> listesi fotoğrafı. Onlar olmadan doğruluk hakkında yazılan her cümle
> tahmindir.

### 7.3 · Ölçüm bir kez değil, sürekli

Model değişir, istem değişir, ruhsatın düzeni değişir. Değerlendirme
seti depoda durur ve sağlık denetimi gibi **her turda koşar**. Bugün
46 testimiz var ve hepsi *"ekran doğru mu"* diye soruyor; bu set
*"model doğru mu"* diye soracak.

---

## 8 · Sıra

Bağımlılığa göre — biri bitmeden öteki başlamaz:

| # | İş | Neden bu sırada |
|---|---|---|
| 1 | **Değerlendirme seti** (ses, 50 cümle, beklenen çıktılarla) | Ölçüm aleti olmadan yazılan hiçbir şeyin daha iyi olduğu bilinemez |
| 2 | **Backend + tek uç nokta** (`/api/zeka/komut`) | Anahtarın yeri; ve en küçük gerçek iş |
| 3 | **Ses komutu modele bağlanır**, iki motor karşılaştırılır | §7.1'deki set burada işe yarar |
| 4 | **Ruhsat okuma** (fotoğraflar toplandıktan sonra) | Cemil'in vereceği fotoğraflara bağlı |
| 5 | **Parça listesi okuma** | Ruhsatla aynı boru; ikincisi ucuz |
| 6 | **Mesaj düzeltme** | En az riskli: yanlışı ustanın gözünün önünde ve metin |

**Mesaj düzeltmenin arayüzü bugün yazıldı** (`85-mesaj.html`): *"Yapay
zeka düzeltsin"* düğmesi, önce/sonra karşılaştırması ve onay. Prototipte
düzeltme **sahte ve öyle olduğu yazılı** — gerçek model bağlandığında
değişecek tek şey, düzeltmeyi kimin yaptığı.

---

## 9 · Riskler — ve her birinin cevabı

| Risk | Cevap |
|---|---|
| **Anahtar sızması** | Anahtar yalnız backend'de; tarayıcı API'yi hiç görmez (§4) |
| **Gizlilik: ruhsatta TC kimlik ve adres** | Fotoğraf saklanmaz; okunur, alan alınır, atılır (Bulgu 2) |
| **Gizlilik: ses** | Bugün zaten Google'a gidiyor (K-015, A-S3). Model eklenince **metin** de bize ve Anthropic'e gider. Ustaya söylenecek bir cümle gerekiyor |
| **Model kendinden emin yanılır** | Onay ekranı kalkmaz (K-017). Model *"emin değilim"* diyemiyorsa, şema ona *"boş bırak"* seçeneği vermeli |
| **İnternet yokken** | PWA çevrimdışı çalışıyor; yapay zeka çalışmaz. Sesli komut motoru **düşer**, elle giriş yolu her ekranda duruyor — bu yüzden elle yol hiçbir zaman kaldırılmadı |
| **Maliyet kaçağı** | Dükkân başına günlük çağrı sınırı backend'de; §5'teki rakamlar tavan değil, başlangıç |

---

## 10 · Cemil'in kararına açık

| Kod | Soru | Öneri |
|---|---|---|
| **A-M30** | Hangi model? Aylık dükkân başına ~$3 (Haiku+Sonnet) ile ~$13 (hepsi Opus 5) arası | **Ölçmeden seçme.** Değerlendirme seti kurulunca üçü de aynı sette koşar; karar rakamla verilir |
| **A-M31** | Backend bu fazda mı açılıyor? Yapay zekanın önündeki tek yapısal engel bu | Evet ama **küçük**: tek uç noktayla başla, veri tabanı bekleyebilir |
| **A-M32** | Ruhsat/parça listesi fotoğrafları — 20–30 gerçek örnek toplanacak mı | Toplanmalı; yoksa doğruluk hakkında konuşulamaz |
| **A-M33** | Ustaya gizlilik cümlesi: *"söyledikleriniz ve çektiğiniz fotoğraf işlenmek üzere gönderilir"* — nerede yazsın | Ayarlarda tek satır; ilk kullanımda bir kez |
