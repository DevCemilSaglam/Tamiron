# SES ÖLÇÜMÜ — dükkân sesine erişmeden (A-M48 · Tur 8.4 · 12.10.2026)

> **Soru (Cemil, 12.10):** *"Bu sesi alabilecek imkânım henüz yok. Buna
> çözüm bulmalıyız."* Alet hazır (`ses-kaydet.html`), erişim yok. Ölçülmezse
> *"Web Speech yetiyor"* bir **tahmin** olarak kalır.
>
> **Kısa cevap:** dükkân **kaydı** şart değil. Asıl engel başkaydı: tarayıcının
> tanıyıcısı yalnız mikrofonu dinliyordu, kayıtlı dosyayı dinleyemiyordu. Bu
> tarayıcının bugünkü sürümü (Chrome 152) **dosyadan beslenebiliyor** — bu
> oturumda denendi. Böylece konuşma evden, gürültü açık arşivden gelebilir ve
> ölçüm **kontrollü** yapılabilir. Kalan tek ölçülemeyen şey dükkânın kendi
> akustiği; o risk olarak yazılır.

---

## 1 · Önce ayrım: sesin İKİ katmanı var, biri zaten ölçülüyor

| Katman | Ne yapar | Bugün ölçülüyor mu |
|---|---|---|
| **Tanıma** | ses → yazı (Web Speech, ileride belki başka motor) | **HAYIR** — hiç ölçülmedi |
| **Çözümleme** | yazı → işlem nesnesi (K-056 şemaları) | **EVET** — 40 cümle, **%93** (`ses-seti.json`) |

Çözümleme ölçümü **temiz yazıyla** yapılıyor. Asıl bilinmeyen: dükkânda
söylenen cümle tanıyıcıdan **ne kadar bozulmuş** çıkıyor ve bozulmuş
yazıda çözümleyici %93'ü tutabiliyor mu. A-M48 bu iki sorunun ilki.

## 2 · Bulgu: tanıyıcı dosyadan beslenebiliyor (bu oturumda denendi)

Web Speech API'nin taslağında `start(audioTrack)` diye bir imza var;
belgeler onu *"deneysel"* diye anıyor ve Chrome'da ne zaman geldiği
yazılı değil ([MDN](https://developer.mozilla.org/en-US/docs/Web/API/SpeechRecognition/start),
[2025 Google/Mozilla sunumu](https://docs.google.com/presentation/d/1zRSgSCJ32FtvwtxbVaStyxt6ZERlKL8obApqwBHRWGU/htmlpresent)).
Tahmin yerine bakıldı (uygulama bölmesindeki tarayıcı, **Chrome 152**):

| Deneme | Sonuç | Anlamı |
|---|---|---|
| `start({})` — yanlış tipte argüman | `TypeError: parameter 1 is not of type 'MediaStreamTrack'` | İmza **gerçekten var**; yok sayılan bir argüman değil |
| `start(ses_izi)` — mikrofon izni **engelliyken** | `start → audiostart → … → error:network` | Tanıyıcı izi **dinlemeye başladı**; mikrofona hiç dokunmadı |
| `available({langs:['tr-TR'], processLocally:false})` | `available` | Türkçe **bulutta** var |
| `available({langs:['tr-TR'], processLocally:true})` | `unavailable` | Türkçe **cihaz üstünde yok** — ses dışarı gidiyor (gizlilik cümlesi doğru, K-057) |
| `SpeechRecognitionPhrase`, `phrases` | var | Beklenen kelimeler önceden bildirilebilir (§6) |

⚠️ **Doğrulanmayan son adım:** `error:network` — bu bölme Google'ın tanıma
sunucusuna ulaşamıyor. **Gerçek masaüstü Chrome'da** bir dosyanın baştan
sona yazıya döndüğü **görülmedi.** İlk iş bu (§5, adım 1); tutmazsa plan
§4'teki B yoluna düşer.

Ses izi şöyle üretiliyor — dosya → `AudioContext` → `MediaStreamDestination`
→ `getAudioTracks()[0]` → `recognition.start(track)`. Mikrofon, hoparlör,
kablo yok.

## 3 · Konuşma nereden gelir — hazır Türkçe derlemler

| Derlem | Büyüklük | Lisans | Ne işe yarar | Eksiği |
|---|---|---|---|---|
| **Common Voice — Türkçe 25.0** ([Mozilla](https://datacollective.mozillafoundation.org/datasets/cmn2e7kbl01k2mm07gm5n1bc9)) | 135 saat · 1.816 konuşmacı | CC0 — ama yeniden dağıtmak ve konuşmacıyı tanımaya çalışmak **yasak** | Çok sesli, telefon kalitesine yakın; genel taban | Okunan cümleler; dükkân kelimesi yok |
| **MediaSpeech TR** ([OpenSLR 108](https://www.openslr.org/108)) | 10 saat · ~618 MB | CC BY 4.0 | Küçük ve elle yazılmış; hızlı taban | Haber/medya dili |
| **ISSAI TSC** ([Hugging Face](https://huggingface.co/datasets/issai/Turkish_Speech_Corpus)) | ~218 saat | ISSAI: CC BY 4.0 · HF kopyası: MIT — **çelişkili**, kaynağından bakılmalı | En büyük açık Türkçe derlem | Aynı |
| **FLEURS TR** ([Hugging Face](https://www.huggingface.co/datasets/google/fleurs/blame/main/fleurs.py)) | 2.000 cümle | kart üzerinde bakılmalı | Modeller arası karşılaştırma ölçütü | Aynı |

**Hiçbirinde "triger", "balata", "devirdaim" ya da plaka okunuşu yok.** Bu
derlemler tanıyıcının **genel** Türkçe başarısını verir; bizim cümlemizdeki
başarıyı vermez. Taban olarak değerli, cevap olarak yetersiz.

## 4 · Dört aday + bulunan beşinci yol

| Yol | Ne ölçer | Bedel | Güvenilirlik | Karar |
|---|---|---|---|---|
| **A · Hazır derlem** | genel Türkçe tanıma (kelime hata oranı) | indirme + koşucu | yüksek, ama **bizim cümlemiz değil** | taban olarak **evet** (MediaSpeech TR, küçük) |
| **B · Sentetik ses (TTS) + bozucu** | boru hattı çalışıyor mu | düşük | makine sesi insan sesi değil — **sayıya güvenilmez** | yalnız **duman testi** |
| **C · Tanıdık dükkânda tek seferlik kayıt** | gerçeğin kendisi | erişim yok | en yüksek | **sonraki faz**, ilk saha denemesinde |
| **D · Ertele, riski yaz** | hiçbir şey | sıfır | — | §7'de **her durumda** yazılıyor |
| **E · Konuşma evden, gürültü açık arşivden** *(yeni)* | **bizim 40 cümlemiz**, kontrollü gürültüde | ~20 dk okuma + koşucu | yüksek; akustik hariç | **ÖNERİ** |

### E yolu — nasıl

1. **Konuşma:** Cemil (ve isterse 2–4 tanıdık) `ses-kaydet.html` ile
   değerlendirme setinin **40 cümlesini** sessiz bir odada okur. Dükkân
   gerekmiyor; alet zaten telefonda çalışıyor ve zip çıkarıyor (6.2).
2. **Gürültü:** açık lisanslı **gerçek atölye kayıtları** — Freesound'da
   yalnız *"impact wrench"* için **41 CC0 kayıt** var
   ([arama](https://freesound.org/search/?q=impact+wrench&f=license%3A%22Creative+Commons+0%22));
   kompresör ve el aletleri de aranır. Her dosyanın lisansı kendi
   sayfasında doğrulanır, lisans ve kaynak dosyanın yanına yazılır.
3. **Bozma:** `ses-bozucu.js` aynen — SNR 18 / 12 / 6 + uzak + kırpık.
4. **Tanıma:** yeni bir koşucu (`ses-dosya-kos.html`, yazılacak): her
   dosyayı §2'deki yolla tanıyıcıya verir, yazıyı alır.
5. **Çözümleme + puan:** yazı `SES_COZUMLE`'ye, sonuç `puanla.js`'ye —
   **aynı cetvel** (K-049). Çıktı bir eğri: *"temizde %x, 12 dB'de %y,
   6 dB'de %z"*.

⚠️ **E yolu `ses-bozucu.js`'in bir kuralını esnetiyor:** *"GÜRÜLTÜ
UYDURULMAZ — karıştırılan ses dükkânın kendi 20 saniyesidir."* E'de
gürültü **uydurma değil, gerçek atölye sesi** — ama **bizim** dükkânın
değil. Beyaz gürültüden çok daha iyi, dükkânın kendisinden kötü.
**Cemil onayına açık.**

## 5 · Önerilen sıra

| # | İş | Kim | Çıktı |
|---|---|---|---|
| 1 | Dosyadan tanıma koşucusu + **gerçek Chrome'da tek dosyayla** baştan sona doğrulama | lider | §2'nin son adımı kapanır — ya da B yoluna düşülür |
| 2 | MediaSpeech TR'den 100 kısa kayıtla **genel taban** | lider (indirme izni Cemil'den) | *"Web Speech genel Türkçede %…"* |
| 3 | 40 cümleyi evde okumak | **Cemil** (~20 dk) | `kayitlar/*.wav` |
| 4 | CC0 atölye gürültüsü + `ses-bozucu.js` + koşucu + `puanla.js` | lider | SNR eğrisi — **ses tarafının ilk gerçek rakamı** |
| 5 | Kalan riski sunuma yazmak | lider | §7 |

1 numara tek başına değerli: tutarsa ses ölçümü **artık dükkâna bağlı değil.**

## 6 · Ölçümün açtığı bir fırsat — ölçülmeden açılmaz

Tanıyıcı `phrases` ile **beklenen kelimeleri** önceden alabiliyor (bağlama
göre yönlendirme). *"triger, balata, devirdaim"* ve plaka okunuşu buraya
verilebilir; Tamiron'un en sık yanlış duyulacak kelimeleri tam bunlar
(A-M27'deki *"34 QBC 123"* örneği gibi). Bu **yeni bir özellik değil**,
var olan tanımanın ayarı — ama **ölçmeden açılmaz:** E yolundaki eğri
`phrases` açık ve kapalı iki kez koşulur, fark rakamla görülür.
K-056'nın şemaları ekran başına beklenen kelimeleri zaten taşıyor; liste
oradan türetilebilir.

## 7 · Ne olursa olsun yazılacak risk

E yolu üç şeyi **ölçemez**:

- **Dükkânın akustiği:** yankılı kapalı alan, usta telefondan iki adım ötede.
  (`ses-bozucu.js`'in *"uzak"* hâli bunu taklit ediyor, ölçmüyor.)
- **Ustanın kendi ağzı:** şive, hız, yarım bırakılan cümle. Cemil'in okuduğu
  cümle, ustanın söylediği cümle değil.
- **Gerçek telefon mikrofonu ve çevrimiçi bağlantı:** dükkânda internet
  yavaşsa tanıma gecikir ya da düşer (`error:network`).

Sunumun *"henüz hazır olmayanlar"* bölümüne şu cümle girer:
*"Sesli işlem temiz yazıda %93 doğru. Gürültülü sesle [E yolu sonucu]. Gerçek
bir dükkânda henüz denenmedi."* — ölçülmemiş bir şey *"çalışıyor"* diye
anlatılmaz (K-055.5).

## 8 · Cemil'in onayları — **12.10'da verildi**

> *"Onaylıyorum."* (gürültü kuralı) · *"Okuyabilirim, fakat yarın sabah."*
> (40 cümle) · *"İzin veririm."* (indirme)

**Hazırlık yapıldı (12.10 akşamı):**
- MediaSpeech TR indi; sabit 100'lük alt küme ayrıldı (`sesler/derlem-100/`).
- Altı CC0 atölye gürültüsü indi, lisansları tek tek doğrulandı
  (`tools/degerlendirme/ses-kaynaklari.md`).
- Koşucu iki kip kazandı: **derlem kipi** (ses + aynı adlı `.txt` → yalnız
  kelime hatası) ve **gürültü** (MP3 seçilir, varyantlar tarayıcıda üretilir).
- Bozma hesabı **ortak modüle** çıktı (`ses-bozma.js`): Node'daki bozucu ve
  tarayıcıdaki koşucu aynı fonksiyonu çağırıyor; taşıma öncesi/sonrası 10
  varyant **bayt bayt aynı** (negatif test: kırpma kazancı değişti → 2 fark).
- Bu bölmede sınandı: FLAC ve MP3 çözülüyor, 1 temiz + 5 varyant üretiliyor,
  tanıma `network` verince karne **"— (tanınan yok)"** yazıyor — sahte %0
  yazmıyor (ilk yazışta yazıyordu, düzeltildi).

## 9 · 13.10 sabahı — sırayla

1. **Tanıyıcı gerçekten dosyadan yazıya döndürüyor mu** (§2'nin son adımı):
   bilgisayarda depo kökünde
   `python -m http.server 8123` → masaüstü Chrome'da
   `http://localhost:8123/tools/degerlendirme/ses-dosya-kos.html` →
   `sesler/derlem-100/`'den **bir** `.flac` ve aynı adlı `.txt` seç → Koş.
   Yazı çıkarsa yol açık; `network` çıkarsa Chrome'un tanıma sunucusuna
   ulaşılamıyor demektir ve plan §4'ün B yoluna düşer.
2. **Genel taban:** aynı sayfada `derlem-100`'ün tamamı (200 dosya) — ~25 dk.
3. **40 cümle (Cemil):** `ses-kaydet.html`. İki yol:
   - **Bilgisayarda** — hemen: `http://localhost:8123/tools/degerlendirme/ses-kaydet.html`
     (localhost güvenli sayılır, mikrofon açılır). Bilgisayar mikrofonu
     telefonunkinden farklıdır — not düşülür.
   - **Telefonda** — ürünün gerçek ortamı, ama https ister: kayıt aletinin
     Netlify'a **ayrı bir taslak** olarak yayınlanması gerekir. **Yayın
     onayı Cemil'in.**
4. **Eğri:** 40 kayıt + bir atölye gürültüsü seçilir → koşucu temiz · uzak ·
   kırpık · 18/12/6 dB karnesini verir. Her gürültü ayrı koşu (~20 dk).
5. Sonuç `SES-OLCUM.md`'ye ve sunumun *"henüz hazır olmayanlar"* bölümüne.

## 11 · İlk gerçek ölçümler (13.10, Cemil'in Chrome 154'ü)

**Yol açık:** Chrome kayıtlı dosyayı mikrofonsuz yazıya çeviriyor (§2'nin
son adımı kapandı). Dükkân kaydı şart değil.

| koşu | seçenek | sonuç |
|---|---|---|
| 40 cümle (Cemil, temiz) | kırpma | çözümleme **%28** (8 tam · 6 yarım · 26 yanlış) · kelime hatası %83 · 6 kısa cümlede **boş** |
| 40 cümle (Cemil, temiz) | kırpma + seviye eşitleme (−20 dB) | **%25** · kelime hatası %83 — eşitleme **işe yaramadı** |
| **Derlem 5 (kontrol)** | kırpma + eşitleme | kelime hatası **%13** — çoğu yazım farkı ("süleymaniye'si/süleymaniyesi") |

**Kontrol kararı verdi: düzenek sağlam** — akıcı, düz seviyeli konuşmada
Chrome neredeyse kusursuz duyuyor. Sorun Cemil'in kayıtlarıyla düzeneğin
**buluştuğu yerde**:

- Kayıtlarda cümle **yavaş ve öbek öbek** okunmuş: "34 ABC 123 triger seti
  değişecek" (~3 sn) → 11 sn kesintisiz ses, 7–8 öbek. Plaka harf harf.
- Öbekler arası seviye farkı **15–20 dB**.
- Tanıyıcı çoğunlukla İLK öbeği duyuyor, kısık öbekleri kaçırıyor:
  "35 KLM 45 teslim ettim parayı almadım" → "35 kalemle 45 parayı al".
- Hipotez: mikrofondan dinlerken Chrome **dinamik** kazanç uyguluyor (kısık
  kelimeyi o an yükseltir); dosyadan beslerken yok. Sabit eşitleme
  öbekler arasındaki farkı korur — o yüzden işe yaramadı.

**Ayıracak iki deney:**
1. **Dinamik kazanç** seçeneği (tarayıcının sıkıştırıcısı + 12 dB telafi)
   ile 40 cümle — kısık öbekler yükselince sonuç düzeliyor mu.
2. **Canlı mikrofon** — Cemil aynı cümleleri prototipin ses ekranında
   söyler. Mikrofon yolu iyiyse sorun DÜZENEĞİMİZDE (ürün sağlam); o da
   kötüyse sorun tanıyıcının bizim cümlelerimizde (ürün riski).

⚠️ **Ölçmeden yorum yok:** bu tablo "Web Speech dükkânda yetmez" demiyor —
iki deney bitmeden bu kayıtlardan ürün hakkında hüküm çıkmaz.

**13.10 akşam — iki deney de GEÇERSİZ:** dinamik kazanç koşusunun 40
kaydının 40'ı `network` (16:28 UTC); Cemil'in canlı mikrofon denemesi de
*"Tanıma sunucusuna ulaşılamadı"*. Bir önceki koşu (16:19) temizdi.
Sebep ya bağlantı ya da **ücretsiz hizmetin kotası** — o gün kısa sürede
~130 tanıma oturumu açılmıştı. Kota ise bu bir **ürün riskidir**: tek
dükkân bu sayıya ulaşmaz ama ücretsiz Web Speech'in sınırı bizim
kontrolümüzde değil ve **belgelenmiş değil** (A-M48'e eklenir).
Koşucu artık art arda 3 `network`ta duruyor ve sebebini söylüyor.
Yeniden deneme: önce **tek dosya**, tutarsa uzun koşu.

**16:52 UTC — sunucu geri geldi** (kesinti ~25 dk). Tek dosya: s01 yine
**"orospik diye"** — sabahki (eşitlemesiz) koşuyla BİREBİR aynı. Tanıyıcı
aynı dosyada her seferinde aynı şeyi duyuyor: rastgele değil, kaydın
içeriğiyle ilgili. Sıradaki en ucuz ayrım: Cemil s01.wav'ı DİNLESİN —
dosyada söylenen gerçekten o cümle mi.

**17:07 UTC — dinamik kazanç** (sunucu cevap verdi, 40/40): **%20**, kelime
hatası %91 — DAHA KÖTÜ. Seviye hipotezi (sabit de dinamik de) düştü.

**Cemil: "Kayıt çok yavaş."** Ölçüldü (aynı kod, tarayıcıda, derleme karşı):

| | s01 | derlem erkek | derlem kadın |
|---|---|---|---|
| perde (NCCF, 120 ms) | **110 Hz** | 124 Hz | 308 Hz |
| ardışık blok benzerliği | 0,15 | — | — |

→ **Teknik yavaşlama YOK** (3× olsaydı perde ~40 Hz olurdu; parça tekrarı da
yok). İlk perde ölçümüm normalize edilmemişti ve yüksek perdeye sapıyordu —
düzeltilmiş ölçüm aynı sonucu verdi.

Bant enerjisi (dB, en güçlü bant 0):

| | 3,5 kHz | 4 kHz | ≥ 4,5 kHz |
|---|---|---|---|
| derlem erkek | −20 | −21 | −29 → −81 doğal iniş |
| s01 / s20 | −37 / −34 | −42 / −39 | **−45…−49 DÜZ (gürültü zemini)** |

→ Kayıtlar **dar bantlı ve boğuk**: ıslıklı sesler (s ş ç z — Türkçede çok
sık) derlemden 15–20 dB zayıf, 4 kHz üstünde konuşma yok. Tipik iz:
Bluetooth kulaklık mikrofonu (telefon kipi) ya da uzak/iç mikrofon. Buna
yavaş, öbek öbek okuma eklenince tanıyıcı düşüyor. **Bu bir ürün bulgusu da:**
dükkânda usta Bluetooth kulaklıkla konuşursa aynı şey olur.

Sıradaki ayrım: hangi mikrofon kullanıldı · 5 cümle telefonun/dizüstünün
kendi mikrofonuyla, normal hızda yeniden.

**Yeniden kayıt (13.10 akşam) — kayıt aleti artık hızını ölçüyor:**

| set | cihaz | ort. süre | SNR | dosya ÷ duvar saati |
|---|---|---|---|---|
| ilk | dizüstü iç mikrofon | **12,6 sn** | 28,7 dB | — (ölçülmüyordu) |
| pc2 | Windows · Chrome 155 · 48 kHz | 2,8 sn | 19,3 dB | 0,95–1,03 |
| **tel** | Android 10 · Chrome 153 · 48 kHz | 2,7 sn | **38,8 dB** | 0,97–1,05 |

→ **Alet sağlam.** İlk setteki "yavaşlık" teknik değilmiş: cümleler yavaş
okunmuş ve kayıt uzun açık kalmış (Cemil: *"Gecikmiş olabilirim"*).
Bant enerjisi (dB, 4 / 5 / 6 kHz): derlem −21/−37/−43 · **telefon −45/−40/−39**
· pc2 −43/−50/−52 · ilk −42/−46/−48 → **dizüstü mikrofonu iki kayıtta da
boğuk; telefon ıslıklı seslerde stüdyo düzeyinde.**

**İlk gerçek rakamlar (13.10 21:00–21:24, Chrome 154) — çözümleme, aynı cetvel:**

| koşu | işlem doğru | kelime hatası |
|---|---|---|
| yazı (temiz metin, karşılaştırma) | **%93** | — |
| ilk set (yavaş okuma) | %25–28 | %83 |
| **telefon, normal hız** | **%65** | %30 |
| **bilgisayar, normal hız** | **%66** | %26 |
| bilgisayar · uzak | %69 | %28 |
| bilgisayar · kırpık | %65 | %27 |
| bilgisayar · kompresör 18 dB | %63 | %32 |
| bilgisayar · kompresör 12 dB | %49 | %44 |
| bilgisayar · kompresör 6 dB | %25 | %66 |

⚠️ **Bu rakamlar DÜZENEĞİN İKİ KUSURUNU taşıyor** (düzeltildi, yeniden koşu
bekleniyor): (1) **cümle sonu kesiliyordu** — "yağ filtresi 300 lira" →
"yağ filtresi 3", "balata ara" → "Balat". Telefon kayıtları son heceden
hemen sonra bitiyor (1,71 sn); tanıyıcı son kelimeyi kesinleştirmek için
arkasında sessizlik bekliyor, yalnız kesin sonuçlar toplanıyordu. Artık
geçici sonuç da okunuyor, kayda 0,5 sn baş / 1,0 sn son sessizlik
ekleniyor (mikrofonun açık kalmasının taklidi), dinleme 2,5 sn sonra
kapanıyor. (2) Parçalar boşluksuz birleşiyordu. Telefonda en az 4
cümle doğrudan (1) yüzünden yanlış.

**Gerçek tanıma zorlukları (ürün bulgusu):**
- **plaka harfleri:** TMR → "PNR / PVR / dmr", KLM → "kelime", ABC →
  "abiciğim", KV → "kw", QBC → "KBC/KVC"
- **"sahibine" → "sahibinden" / "tarihinde"** — mesaj işi kayboluyor
- tutarda "TL/lira" düşünce gider tanınmıyor ("2.400 elektrik faturası")
→ §6'daki `phrases` (beklenen kelimeler) tam bu yerler için; ölçmeden
açılmaz.

### 11.1 · Düzeltilmiş düzenekle yeniden koşu (13.10 21:48–21:55)

Aynı kayıtlar, aynı seçenekler (eşitle + kırp); tek fark kesilme düzeltmesi.

| koşu | işlem doğru | kelime hatası | son kelimesi eksik kayıt |
|---|---|---|---|
| telefon — önce | %65 | %30 | 28 / 40 |
| **telefon — düzeltilmiş** | **%74** | **%16** | 11 / 40 |
| bilgisayar — önce | %66 | %26 | 20 / 40 |
| **bilgisayar — düzeltilmiş** | **%68** | **%18** | 8 / 40 |

**Okuma:** kelime hatası yarıya yakın düştü, yani dünkü rakamın büyük payı
tanıyıcının değil **bizim düzeneğimizin** kusuruydu. "Son kelimesi eksik"
sayısının hâlâ 8–11 olması kesilme değil: kalanların çoğu yanlış duyulan
son kelime ("aşınmış" → "açılmıyor") ya da cetvelin **biçim farkını hata
sayması** — "88 bin" ↔ "88.000", "dört bin beş yüz lira" ↔ "4500", "2400" ↔
"2.400". Yani kelime hatası %16–18'in bir kısmı da ölçüm yapaylığı; işlem
doğruluğu (%68–74) asıl rakam.

**Kalan yanlışların üç sınıfı:**
1. **Plaka harfi** (en büyük sınıf): TMR → PNR/PVR/dmr, KLM → "kelime",
   QBC → KBC/KVC, ABC → "Dur daha beceri", 8890 → "88 90". Motor bunları
   kayıtsız plaka sanıp **yeni-arac**'a gidiyor — en tehlikeli yanlış,
   çünkü sessizce yeni kayıt açtırır.
2. **Anlam kayması:** "sahibine" → "sahibinden/tarihinde", "tahsil ettim"
   → "tavsiye ettin", "gideri ekle" → "giderek / gider reklam",
   "Mégane'ın" → "mekanını".
3. **Motorun kendi katı kuralı — tanıma DOĞRU:** tutar yalnız
   "TL/lira" kelimesiyle okunuyor (`04-islem.js` `sesTutarBul`); tanıyıcı
   bu kelimeyi sık düşürüyor ("2.400 elektrik faturası ekle" → gider
   tanınmadı; "motor yağı 4500", "hava filtresi 18"). **Cemil onayına
   açık:** fiyat/gider bağlamında birimsiz sayıyı tutar saymak. Risk:
   plaka ve km sayılarıyla karışması — önce bu cetvelle eşdeğerlik
   ölçülmeden açılmaz.

**Model ne kazandırır (tahmin, ölçülmedi):** 2. sınıfı büyük ölçüde,
1. sınıfı dükkândaki kayıtlı plaka listesiyle kısmen düzeltebilir
("06 pvr 07" → tek benzer kayıtlı plaka 06 TMR 07). Ölçüm aracı hazır
(`model-kos.js --duyulan <dosya>`); anahtar + harcama onayı bekliyor.

### 11.2 · İlk Claude koşusu (14.10, Opus 5.5, telefon dökümleri) — ŞEMA KUSURU

**%64** (18 tam · 15 yarım · 7 yanlış), **$0,20**. Motorun %74'ünün
ALTINDA — ama rakam modeli değil **bizim aracımızı** ölçüyor:

- 15 yarımın **11'inde** model işlemi DOĞRU seçmiş, alanı boş bırakmış.
  "yağ filtresi 300 lira" → `fiyat-guncelle` doğru, ama tutar yok; parça
  adı `terim` ve `suzgec`'e konmuş. Sebep: `model-kos.js`'teki araç
  şemasında **hiçbir alanın açıklaması yok**; model `fiyat`'ın ne olduğunu,
  `suzgec`'in hangi değerleri aldığını ("serviste") bilmiyor.
- Alan boşlukları düzelse kaba hesap **~%78** — motorun biraz üstü.
  Bu bir tahmin; ölçülmeden yazılmaz.
- Gerçek anlam yanlışları (7): "sahibini ara" ↔ mesaj karışması (34,
  39), "yarın gelecek" → plakasiz, bağlamsız "bir de balata değişecek"
  → satir-ekle (beklenen plakasiz), "işçilik 750" → açık kayıtta işçilik
  satırı olduğu için fiyat-guncelle (istem kuralı 2'ye göre savunulabilir;
  cetvelin mi modelin mi hatası — A-M34'ün sorusu).

**Sonuç:** model koşusu bugünkü hâliyle karar verdirmez. Önce şemaya alan
açıklamaları (parasız), sonra iki set yeniden (~$0,40). Bilgisayar seti
bu yüzden KOŞTURULMADI.

Koşucunun iki kusuru bu koşuda düzeldi: workspace'siz anahtar (API 400,
ilk istekte durma eklendi) ve strict şemada `type:['string','null']` +
enum'un reddi.

### 11.3 · Açıklamalı şemayla (14.10, Opus 5.5, aynı telefon dökümleri)

Alan açıklamaları **programın kendi şemalarından** yazıldı (setin
cevaplarından değil). Strict şemanın ikinci sınırı da çıktı: 16'dan fazla
"boş olabilir" alan 400 veriyor → alanlar null'suz, yazılmayan alan boş.

| aynı 40 döküm, aynı cetvel | sonuç | maliyet |
|---|---|---|
| bugünkü motor (desen) | 28 tam · 3 yarım · 9 yanlış · **%74** | 0 |
| Claude, açıklamasız şema | 18 · 15 · 7 · %64 | $0,20 |
| **Claude, açıklamalı şema** | 27 · 6 · 7 · **%75** | $0,19 |

**Ortalama başa baş — ama iki motor FARKLI cümlelerde yanılıyor:**

| | cümle |
|---|---|
| ikisi de tam | 21 |
| yalnız motor tam | 7 |
| yalnız Claude tam | 6 |
| ikisi de değil | 6 |

- **Claude'un kazandığı yer tam da beklenen sınıflar:** yanlış duyulan
  plakayı kayıtlı plakaya bağlamak ("06 dmr 07" → 06 TMR 07, "35 kelime
  45" → 35 KLM 45, "16 XY 88 90"), birimsiz tutar ("2.400 elektrik
  faturası", "motor yağı 4500"), sahip adından araç ("Mehmet Kaya'nın
  arabası").
- **Motorun kazandığı yer:** kısa, kalıplı cümleler — "34 ABC 123 yarın
  gelecek", kayıt içinde "işçilik 750 lira", "balata ara". Claude burada
  ya fazla temkinli (plakasiz) ya da bağlamı fazla yorumluyor.
- **Tavan:** biri ötekinin yanlışını yakalasa 34/40 tam (~%85+). Bu bir
  MİMARİ soru (motor önce, emin değilse model?) ve backend bu fazda yok
  (K-055.1) — **Cemil onayına açık**, karar değil bulgu.
- **Ders:** ilk %64 "model motordan kötü" diye okunsaydı yanlış karar
  çıkardı. Model koşusu, aracın şeması kadar iyi ölçer.

Bilgisayar seti KOŞTURULMADI. (Düzeltme: gerçek harcama **~$0,45** —
koşucunun hesabı önbellek okuma/yazma jetonlarını saymıyordu.)

### 11.4 · Claude'un kazandığı işi motora öğretmek (14.10, dal `ses-motor-iyilestirme`)

**Cemil onayına açık** — motorun davranışını değiştiriyor; ana dala onayla
girer. Ölçüm parasız: dökümler (80) motordan doğrudan geçirildi
(`DEGERLENDIR({set})`), tanıyıcı yok. Düzenek önce DÜNKÜ rakamları
birebir verdi (%93 / %74 / %68) — ölçüm aleti doğrulandı.

**Dört kural, hepsi `04-islem.js`'in ortak okuyucularında:**
1. **Bölünmüş rakam:** "16 XY 88 90" → 16 XY 8890. Türk plaka biçiminden:
   harf sayısı rakam sayısını sınırlar (1→4–5, 2→3–4, 3→2–3).
2. **Sesçe yakın harf:** kayıtsız plaka, il ve rakamı aynı, harfleri
   aynı SES SINIFINDA olan TEK kayıtlı plakaya bağlanır ("06 PVR 07" →
   06 TMR 07). Sınıflar Türkçe ses bilgisinden (patlamalı · sürtünmeli ·
   genizsi · akıcı); tek gözlemsel ekleme M~V. ⚠️ Düz "tek harf farkı"
   OLMAZ: setin "34 QBC 123" tuzağı (gerçekten başka araç) da tek harf
   uzak; Q~A sesçe yakın olmadığı için bağlanmıyor.
3. **Harf yerine kelime:** "35 kelime 45" → 35 KLM 45 (KLM'nin okunuşu
   "ke-le-me"; ünsüz iskeleti aynı, il+rakam birebir).
4. **Birimsiz tutar:** yalnız gider cümlesinde ve açık kayıtta cümlenin
   son sayısı. Telefon geçen cümlede, tek haneli sayıda, "tane/adet"
   önünde OKUNMAZ.

Usta plakanın değiştirildiğini GÖRÜR: onay ekranı *"Duyulan plaka 06 PVR
07 kayıtlı değil — sesçe en yakın kayıtlı araç 06 TMR 07 alındı. Değilse
tekrar söyle."* der.

| aynı cetvel | önce | sonra |
|---|---|---|
| yazı (temiz metin) | %93 | **%93** |
| telefon dökümleri | %74 | **%86** |
| bilgisayar dökümleri | %68 | **%78** |
| Claude (Opus 5.5), telefon | %75 | — |

**Motor artık telefon dökümlerinde Claude'un 11 puan üstünde, internetsiz
ve bedava.**

**Eşdeğerlik:** 568/0 · 392/0 · 16/0 — yazılı derlemde hiçbir davranış
değişmedi. ⚠️ Bu "0" aynı zamanda derlemin yeni kuralları hiç
tetiklemediğini söylüyor; o yüzden ayrıca **zorlama örnekleri** denendi
ve İKİ GERÇEK KUSUR bulundu, kapatıldı: "kira gideri 0532 111 22 33" →
tutar 532 (telefonun parçası); "bu ay 3 fatura geldi" → 3 TL gider (adet).

**Yeni denetim testi 55 · SES-DUYULAN** — bu sınıf yazılı derlemde
görünmediği için. Negatif doğrulama: bağlama kapatıldı → 2 bulgu; telefon
koruması kaldırıldı → 1; sesçe kural gevşetildi ("her harf yakın") →
QBC tuzağı düştü, yeni test + eski PLAKA-ONERISI yakaladı. Geri konunca 0.
320 / 375 / 430 px'te 0 bulgu, art arda iki koşuda 0.

**Kalan yanlışlar (telefon 7, bilgisayar 11):** anlam kayması ("sahibine"
→ "sahibinden/tarihinde", "tahsil ettim" → "tavsiye ettin"), cümle sonunun
yanlış duyulması, "Mégane'ın" → "mekanını". Bunların ilacı tanıyıcıya
beklenen kelimeleri vermek (`phrases`) ya da model — sıradaki ölçüm.

**Risk (yazılı):** kayıtsız gerçek bir araç sesçe yakın tek kayıtlı plakaya
bağlanabilir ("34 APC 123" → 34 ABC 123). Onay ekranı bunu söylüyor; usta
"tekrar söyle" der. Sessiz yanlış değil, söylenen öneri.

## 10 · Önceki hâli: onaya açık maddeler

1. **E yolu** ve *"gürültü uydurulmaz"* kuralının esnetilmesi — gerçek atölye
   sesi ama bizim dükkânın değil.
2. **Kendi sesin** (40 cümle, ~20 dk) — ve istersen 2–4 tanıdığın.
3. **İndirme:** MediaSpeech TR (~618 MB, CC BY 4.0) ve seçilen CC0 gürültü
   dosyaları. Ses dosyaları depoya **girmez** (`.gitignore`, ruhsat
   fotoğraflarıyla aynı kural); yalnız kaynak + lisans listesi girer.

### 11.5 · Türk plakasının kuralları (14.10, dal `plaka-kurallari`)

> **Cemil, 14.10:** *"Türkiye'de Q harfi yok. Plakaların bazı kuralları
> var. Bunları da dahil edebiliriz."*

**Kurallar:** il kodu **01–81** · harfler Türk alfabesinin **23** harfi (Ç Ğ
İ Ö Ş Ü **ve Q W X yok**) · harf sayısı rakam sayısını sınırlar. Tanıyıcının
Türkçe yazdığı harf düzeltilir ("kbi" → KBİ → KBI).

**Q/W/X'li plaka gerçek olamaz → kesinlikle yanlış duyulmuştur:** yeni
araç önerilmez, tek harf uzaktaki TEK kayıtlı plakaya bağlanır (sesçe
yakınlık aranmaz). Tarihçe: tanıyıcı 14.09'da "34 ABC 123"ü gerçekten
"34 QBC 123" duymuştu.

**Kendi verimiz kuralla çelişiyordu — düzeltildi:**
- tohum araç **"16 XY 8890"** (X yok) → **"16 SY 8890"**;
- değerlendirme seti **sürüm 2**: id 11 ("34 qbc 123") artık kayıtlı ABC'ye
  bağlanmalı; "gerçekten başka araç" tuzağı geçerli harflerle **id 41
  "34 ARC 123"** (R~B sesçe yakın değil → bağlanmaz, sorulur).
- Dökümlerden id 24 çıkarıldı: ses "XY" diyor, cümle artık "SY".

| sürüm-2 set, aynı cetvel | önceki motor | plaka kurallı |
|---|---|---|
| yazı (41) | %88 | **%93** |
| telefon dökümü (39) | %85 | %85 |
| bilgisayar dökümü (39) | %76 | %76 |

Dökümlerde fark yok: id 11'de tanıyıcı Q'yu **"K"** yazdı ("34 kbc 123") —
K geçerli harf, plakanın yanlış olduğu anlaşılamıyor.

**Eşdeğerlik 41 / 33 / 16 fark — hepsi okundu, hepsi bilerek:** tohum
plakasının değişmesi, QBC'nin ABC'ye bağlanması, XY'li cümlelerin SY'ye
bağlanması, "99 ZZZ 999"un artık plaka sayılmaması (il 99 yok). Uygulama
farklarının tohum dışı 4'ü TEK kaynaktan: "34 QBC 123 sahibine selam gönder"
artık 34 ABC 123'e selam mesajı hazırlıyor (eskiden ruhsata gidiyordu).

**Test 55 genişledi:** QBC bağlanır, ARC bağlanmaz, "16 XY 88 90" → 16 SY
8890, "88 BIN 900" plaka değil. Negatif: il kodu denetimi kaldırıldı → 1
bulgu; geçersiz harf kuralı kaldırıldı → 2 bulgu; geri konunca 0. 320 /
375 / 430 px'te 0 bulgu.

⚠️ **AÇIK — Cemil onayı (K-047.6 ile çatışma):** 14.09 kararı *"yanlış
duyulan plakada SOR, kendiliğinden düzeltme — yanlış araca mesaj gider"*.
11.4'teki sesçe bağlama ve buradaki Q bağlaması plakayı KENDİSİ seçiyor,
onay ekranında söylüyor. Bu, kararı gevşetiyor ve 14.10'daki onaydan önce
Cemil'e söylenmedi.

**GÜNCELLEME — Cemil "karma"yı seçti (K-059):** gerçek OLAMAYAN plaka
bağlanır; gerçek OLABİLECEK sesçe yakın plaka SORULUR ("Bunu mu demek
istedin"). Sesçe yakın aday artık `benzerPlaka`'nın önerisine giriyor.

| sürüm-2 set | kendiliğinden doğru | + doğru öneri | tek dokunuşla |
|---|---|---|---|
| yazı (41) | %93 | — | %93 |
| telefon (39) | %82 | 2 | ~%87 |
| bilgisayar (39) | %71 | 2 | ~%76 |

Negatif: eski "hep bağla" geri getirildi → 3 bulgu (test 55 ×2 + 44). 320 /
375 / 430 px'te 0 bulgu.

## 12 · Büyük set — 1010 cümle, kurmaca dükkân (14.10)

> **Cemil, 14.10:** *"Daha fazla test yapabiliriz. Belki de 1000 adet okuma
> bile olabilir. Ne kadar çok olursa."* — yazılı + sesli yolu seçti.

`tools/degerlendirme/buyuk-set-uret.js` (tohum 20261014) → `buyuk-set.json`:
62 araçlık kurmaca dükkân (Türk plaka kurallarına uygun, **6 ikiz** —
tek harfi farklı iki kayıtlı plaka, iki aracı olan sahipler, aynı modelden
birden çok araç), 31 açık kayıt; yedi işin her biri için farklı
söyleyişler (plaka başta/sonda/küçük harf/bölünmüş rakam, sayı rakamla ya
da yazıyla, ödemenin farklı sözleri). Set ölçülmeden ÖNCE commit'lendi
(dondu). İki yarı: `gelistirme` / `sinav`.

| | önce | sonra |
|---|---|---|
| geliştirme (505) | %90 | **%99** |
| **sınav (505)** | %87 | **%100** |
| eski 40'lık yazı | %93 | **%98** (id 5, 6 İLK KEZ) |
| telefon dökümü (39) | %82 | %85 |
| bilgisayar dökümü (39) | %71 | %73 |

⚠️ **Sınavın %100'ü ŞİŞKİN — dürüst okuma:** iki yarı AYNI kalıplardan
üretildi. Bir kalıbı düzeltmek iki yarıyı birlikte düzeltir. Bu set
"benim aklıma gelen söyleyişlerin çeşitliliği"ni ölçüyor; ustanın benim
aklıma gelmeyen söyleyişini ÖLÇMÜYOR. Gerçekten bağımsız sınav: başka
birinin yazdığı ya da söylediği cümleler (tamirci!).

**Büyük setin bulduğu motor kusurları (hepsi gerçek, hepsi düzeldi):**
1. **Tek harfli 5 rakamlı plaka HİÇ tanınmıyordu** ("34 M 49109") —
   okuyucu en çok 4 rakam alıyordu. Teslim, bakım, mesaj yanlışlarının
   çoğu buydu.
2. **K-018 ihlali:** yeni araç cümlesinde sahip adı `\b` ile okunuyordu;
   "İsmail Özdemir" gibi Türkçe harfle başlayan ad BOŞ dönüyordu
   (negatif testte doğrulandı).
3. **Araç tarifi motorda hiç yoktu:** "Mehmet Kaya'nın arabası",
   "şu Mégane'ın" — 14.09'dan beri setin beklediği davranış. Şimdi var;
   dar: yalnız genel/servis-ekle bağlamı, yalnız TEK araca uyan tarif.
4. "yağ değişimi" bakım sayılmıyordu · "işçiliği" (k→ğ) parça sanılıyordu
   · "peşin", "EFT", "hesaba yatırdı", "bu sene", "serviste ne var"
   bilinmiyordu · aramada plaka bölünmüş hâliyle aranıyordu.

**Eşdeğerlik:** 576/5 · 400/4 · 16/0 — farkların hepsi okundu: araç
tarifiyle çözülen iki cümle (bilerek) + bir öneri satırının adından "yaz"
düşmesi. **Test 55 genişledi** (5 rakam, K-018 adı, sahip tarifi);
negatif: her biri tek tek bozuldu → bulgu; geri → 0. 320/375/430 px 0 bulgu.

**Kalan küçük kusur (yazıldı, düzeltilmedi):** araç tarifinde "Ne istiyor"
alanına dolgu sözü kalıyor ("geldi yağ değişimi yapılacak", "şu
trigerini…"). Usta onay ekranında görür; zararsız.

## 13 · Söyleyiş seti — Cemil'in örnekleri (16.10)

> **Cemil, 16.10:** *"Türkçe'de yapıyorum yerine yapıyom denilebilir.
> C-Elysée var Citroën. Bunu okuyabilen çok azdır. Ayrıca egzost doğru
> söyleyen çok azdır. … tel no söylerken 33 yerine 23 denip
> değiştirilebilir. … 4487 ise kırk dört seksen yedi diye okunabilir, dört
> bin dört yüz seksen yedi de denebilir. Courier model de örnek, doğru
> söylenemez genellikle. Ve kredi kartı yerine kk kısaltması da
> kullanılabilir. Ege'de Karadeniz'de vb ustaların şivesi olabilir."*

`soyleyis-seti.json` — 30 cümle, sekiz konu, kendi küçük dükkânı. Motor
düzeltilmeden ÖNCE commit'lendi (8a5d07d). Kök Cemil'in bilgisi — benim
üretecim değil; ama cümleleri ben yazdım, düzeltmeleri de ben yaptım →
%100 aynı şişkinliği taşır (§12). Gerçek sınav: kayıtlar.

| konu | önce | sonra | ne yapıldı |
|---|---|---|---|
| konuşma dili ("yapıyom", "yapılcak") | 5/5 | 5/5 | — motor kök arıyor, fiil sonu sorun değil |
| C-Elysée ("se elize", "selize") | 0/4 | 4/4 | söyleyiş sözlüğü + model tek başına "geldi" ile |
| egzoz ("egzost", "eksoz") | 2/4 | 4/4 | terim sözlüğü: metin kayıttaki biçime çevrilir |
| söylerken düzeltme ("33 yok 23") | 1/4 | 4/4 | iki sayı arasındaki yok/pardon/hayır/değil/yani |
| sayı okuyuşu ("kırk dört seksen yedi") | 2/4 | 4/4 | **sayılar toplanıyordu** → Türkçe sayı dilbilgisiyle gruplara bölünüyor |
| Courier ("kuryer", "kurier") | 0/3 | 3/3 | sözlük; "kurye" GERÇEK KELİME → yalnız "Ford" ile |
| kk | 1/3 | 3/3 | "kk" = kart |
| şive ("teslim ettum", "bakuma") | 2/3 | 3/3 | "bakum" kökü |
| **toplam** | **%53** | **%100** | |

**Asıl kusur sayıdaydı:** art arda gelen BÜTÜN sayı sözcükleri tek sayıya
toplanıyordu — "kırk dört seksen yedi" 131, sözcükle söylenen telefon tek
bir anlamsız sayı oluyordu. Artık: birler birlerden, onlar birler/onlardan
sonra yeni sayı başlatır; "otuz iki yüz" iki sayıdır; "dört bin dört yüz
seksen yedi" tek sayıdır. 14 birim deneme, hepsi doğru.

**Gerilemedi:** büyük set %99/%100, eski 40'lık %98, dökümler %85/%73 —
aynı. Eşdeğerlik 576/0 · 400/0 · 16/0. Zorlama: "kurye geldi paket
bıraktı", "kedi geldi" araç sayılmıyor; "750 yani 800" → 800.
Test 55 genişledi; negatif: düzeltme kapatıldı → bulgu; sözlükten "!"
kaldırıldı ("kurye" tek başına araç) → bulgu. 320/375/430 px 0 bulgu.

**Şive — dürüst sınır:** yazıya geçen şive (ettum, bakuma) ele alınabilir;
SESİN kendisini tanıyıcı çözüyor. Ege/Karadeniz konuşmacılarının kaydı
olmadan ölçülemez.

## 14 · 300 gerçek kayıt — büyük setin sınav yarısından (16.10)

Cemil 6 parti × 50 cümle okudu (kurmaca dükkânın plakalarıyla). 299
kayıtta metin var; ortalama kelime hatası %30, kelimesi kelimesine doğru
duyulan 95/299.

**Kayıp nerede (motor değişmeden, aynı cümleler yazıyla %100):**

| sınıf | yanlış |
|---|---|
| **plaka yanlış duyuldu** | 61 |
| cümle bozuk duyuldu | 27 |
| tutar/sayı yanlış duyuldu | 26 |
| duyuş doğru, motor yanlış | **1** |

Tanıyıcı plaka HARFLERİNİ sözcüğe çeviriyor ("RIZ" → "Rus", "UR" →
"Uğur", "JOC" → "George", "KIH" → "kız", "V" → "ve"), il kodunu
kaçırıyor ("16 J 1139" → "Hatice 11 39"), rakama nokta koyuyor ("99.363",
"28.30"), harfi rakama yapıştırıyor ("K ıh11", "10D"). **Rakam grubu
çoğunlukla doğru.**

**Yapılanlar (genel kurallar, cümleye özgü değil):** rakam içi nokta /
derece işareti / yapışık harf-rakam temizliği · aralıklı harf ("Y V I 22")
birleşir · gerçek OLAMAYAN plakada (sözcük, harf yok) rakam TEK kayıtlı
araca uyuyorsa bağlanır ve SÖYLENİR (il duyulduysa il de eşleşmeli; il
yoksa en az 4 hane; adet/zaman sözünden önceki sayı aday değil) · gerçek
OLABİLECEK kayıtsız plakada aynı rakamlı kayıtlılar puanlanır, tek en iyi
ÖNERİLİR (K-059: sorulur) · "sahibinden" + mesaj metni = mesaj · açık
kayıtta yakın söyleyişli parça adı ("friendiski" → Fren diski; en çok %20
fark VE ilk harf aynı/sesçe yakın).

**Dürüst bölme:** kurallar parti 1–3'e bakılarak yazıldı; 4–6 en sonda.

| | önce | kendiliğinden | + doğru öneri |
|---|---|---|---|
| geliştirme (1–3) | %68 | %79 | +9 |
| **sınav (4–6)** | **%67** | **%71** | **+11 → tek dokunuşla ~%78** |

Geliştirmedeki kazanç sınavdakinin iki katı — kuralların bir kısmı gördüğüm
kayıtlara özgü kaldı. **Sınav rakamı asıl rakam.**

**Yakalanan iki tehlike (ilk denemede vardı, düzeltildi):**
- yakın parça adı %30 eşikle "hava filtresi"ni "Yağ filtresi"ne uydurdu —
  YANLIŞ satırın fiyatı değişirdi (eski setlerde 3 gerileme);
- rakamdan bağlama "yarın 123 tane balata gelecek"i 34 ABC 123'e bağladı.
Bu ikisinin bedeli: sınavda 1 kayıt.

Eski setler gerilemedi (büyük %99/%100, esas %98, dökümler %85/%73,
söyleyiş %100); eşdeğerlik 576/0. Test 55 genişledi; negatif: rakamdan
bağlama kapalı / rakamla öneri kapalı / adet koruması yok → her biri bulgu.

**Bundan sonrası duymada:** kalan yanlışların çoğu tanıyıcının cümleyi
bozması ("bu ayki" → "Waikiki", "teslim ettik kartla" → "teslimatik
capture"). Motor bunu düzeltemez. Sıradaki deneme: `phrases` (kayıtlı
plakaları ve parça adlarını tanıyıcıya ipucu vermek).

## 15 · Söyleyiş sözlüğü + "önce plaka" (16.10)

> **Cemil, 16.10:** *"Araç marka modelleri için okunuşları derleyelim …
> egzos, manifolt gibi bir sürü kelime var. Bunları da önceden toplayalım.
> … Araçla ilgili bir şeyse önce plaka söylenecek şeklinde bekleyelim?"*

**Sözlük** `betik/05-soyleyis.js`'e taşındı ve genişledi: 22 marka, ~70
model (Civic: sivik/civik/sivic), 33 terim (manifolt → manifold, şanzuman
→ şanzıman, debriyaç → debriyaj, amortisor → amortisör …). Gerçek Türkçe
kelime olan söyleyiş ("fiyat", "nisan", "kedi", "sivil", "caz") "!"li:
yalnız MARKAYLA birlikte araç sayılır, terim düzeltmede kullanılmaz.
⚠️ Okunuşlar bizim derlememiz — ustaya doğrulatılacak; K-060.1'deki
dörtlü (ustanın onayladığı son hâl) bunu zamanla kendiliğinden besler.

İkinci kullanım: **tanıyıcıya ipucu** — `SOYLEYIS_IFADELER()` dükkânın
plakaları + marka/modelleri + terimler + işlerin sözleri (kurmaca dükkânda
171 ifade). Ölçüm sayfası "beklenen kelimeleri bildir" işaretliyse bu
listeyi verir (`?ipucu=1`, sonuç `…-ipucu.json`). **Ölçülmedi** — Cemil'in
Chrome'unda 300 kaydın yeniden koşusu gerekiyor.

Düzelticide bir kusur birim denemede yakalandı: "amortisor" düzeltilmiyordu
(harfsiz biçimde "amortisör" ile aynı göründüğü için atlanıyordu) —
koruma gerçek yazımla karşılaştıracak şekilde düzeldi.

**"Önce plaka" — öncelik olarak, zorunlu değil.** Cümlenin başındaki (ilk
3 sözcük) rakam grubu plaka kanıtı: orada il kodu olmadan da 3 hane yeter.
Sonda söylenen plaka REDDEDİLMEZ (gerçek kayıtlarda tanıyıcı plakayı her
yerde bozuyor; zorunlu sıra kayıp üretir). Ölçülen etki küçük: geliştirme
ve sınavda birer kayıt (sınav %71 → %72, +11 doğru öneri). Zorlama bir
tehlike buldu — "yarın 123 balata gelecek" 34 ABC 123'e bağlandı —
plaka bölgesinde zaman sözü varsa öncelik uygulanmaz.
Ekrandaki ipucu: *"Önce plakayı, sonra yapılacak işi söyle."*

Gerileme yok (büyük %99/%100, esas %98, söyleyiş %100, dökümler %85/%73);
eşdeğerlik 576/0 · 400/0 · 16/0; 320/375/430 px 0 bulgu.

## 16 · Tanıyıcıya ipucu (`phrases`) — Chrome'da MÜMKÜN DEĞİL (16.10)

Cemil 300 kaydı ipucu açık koşturdu: **300'ün 300'ü `phrases-not-supported`.**
Chrome API'yi gösteriyor (`'phrases' in recognition` doğru) ama Türkçe
tanıma BULUTTA yapılıyor ve bulut tanıyıcı ipucu kabul etmiyor; ipucu yalnız
cihaz üstü tanımada çalışıyor, Türkçe için cihaz üstü tanıma YOK (§2,
12.10 ölçümü). **Ücretsiz Web Speech ile ipucu yolu kapalı.**

Sonuç: `SOYLEYIS_IFADELER()` boşa gitmiyor — aynı liste ileride ipucu kabul
eden bir tanıyıcıya (paralı bulut tanıma, backend fazı; ya da bir dil
modeline bağlam olarak) verilecek. Bugün iyileştirmenin tek yolu motor:
sözlük + plaka kuralları.

**Aletin kusuru (düzeldi):** ölçüm sayfası yalnız AĞ hatasında duruyordu;
300 kaydı aynı hatayla boşuna çaldı. Artık HER TÜRLÜ aynı hata 3 kez üst
üste gelince durur ve hatanın adını söyler. Gerçek dosyalarla denendi:
ipuçlu koşu 3. kayıtta durur, hatasız koşu durmaz.

## 17 · Şiveli yapay ses — ara sonuç (17.10)

> **Cemil, 17.10:** *"Yapay zekaya, şiveli okutma yapıp kaydedebilir miyiz?"*
> → pilot → *"Yeniden dene şiveleri. Biraz da hızlı konuşan birileri olabilir.
> Kelimeleri yuvarlayabilir."* → v2 → *"2'yi kullanalım ve test edelim."*

`tools/degerlendirme/yapay-ses-uret.js` (Gemini 3.8 Flash TTS + Voice
design): sesi tariften kurar, her cümleye konuşma tarzı (hız, yutma) verir,
metni ağızla yazar ("ettuk", "değişcek", "yapıyo"); beklenen cevap
değişmez. ⚠️ Model günde 100 istek — üretim günlere yayılıyor.

Her yapay ses büyük setin sınav yarısından cümle okudu; Cemil'in AYNI
cümleleri okuduğu gerçek kayıtlarla karşılaştırıldı (Speechmatics + ipucu,
aynı motor):

| ses (cümle) | yapay — SM | Cemil — SM | Cemil — Chrome |
|---|---|---|---|
| Karadeniz (50) | %79 | %80 | %71 |
| Ege (25) | %86 | %78 | %67 |
| Doğu (25) | %76 | %82 | %76 |
| İç Anadolu (25) | %86 | %78 | %86 |
| **İstanbul — çok hızlı, yutarak (25)** | **%68** | **%92** | %86 |

**Okuma (küçük örnek — 25 kayıtta ±10 puan olağan):**
- Şiveler (sentetik hâliyle) Speechmatics'i bozmuyor.
- **Hız bozuyor:** heceleri yutan çok hızlı ses aynı cümlelerde %92 → %68.
  Motor/sözlük bunu düzeltemez — duyulmayan hece geri gelmez. Hızlı
  seslerin (hizli1, hizli2) üretimi kota yenilenince.
- Sentetik ses kelime hatasını düşürüyor (%17–23 vs Cemil %30–31) ama
  işlem doğruluğu aynı bantta — beklendiği gibi.

**Bekleyen:** kadın + iki hızlı ses (kota) · Chrome dökümleri (Cemil) →
kademe (b) her ses için.

### 17.1 · Chrome dökümleri gelince — kademe (b) her ses için (17.10)

Cemil yapay sesleri Chrome'da koşturdu (Ege'nin ilk koşusu ağ hatasıyla 3 kayıtta durdu — durma kuralı işledi; ikinci koşu tam).
Aynı cetvel, kademe (b) = motor öneri soruyorsa ya da anlamadıysa
Speechmatics + ipucu (K-062):

| ses | Chrome | Speechmatics | **kademe (b)** | paralıya |
|---|---|---|---|---|
| Karadeniz (50) | %78 | %79 | **%84** | %20 |
| Doğu (25) | %84 | %76 | **%88** | %20 |
| İç Anadolu (25) | %90 | %86 | **%90** | %8 |
| Ege (25) | %78 | %86 | **%86** | %32 |
| İstanbul — çok hızlı (25) | %56 | %67 | **%67** | %42 |

- **Kademe beş seste de iki tanıyıcının iyisine eşit ya da üstünde** —
  K-062 (b) gerçek kayıttan sonra yapay şivelerde de doğrulandı.
- **Kendini ayarlıyor:** konuşma zorlaştıkça paralıya giden pay kendiliğinden
  artıyor (İç Anadolu %8 → hızlı İstanbul %42). Maliyet zor konuşmaya gidiyor.
- **Hız iki tanıyıcıyı da bozuyor** (en iyisi %67). Duyulmayan hece geri
  gelmez; burada çare onay ekranı ve "tekrar söyle". (Fikir — ölçülmeden
  ürüne girmez: aynı cümle iki kez anlaşılmazsa ekran "biraz yavaş söyler
  misin" desin.)

### 17.2 · Kalan üç ses — HIZ bulgusu doğrulandı (18.10)

gemini-3.8-flash-tts kotası (günde 100) dolunca kalan üç ses, AYNI tasarlanmış
sesle ama `gemini-3.8-flash-lite-tts` modeliyle okutuldu (lite ses
tasarlayamıyor ama tasarlanmış sesi okuyabiliyor; ayrı kota; klasör -lite).

| ses (Speechmatics + ipucu, 25 cümle) | konuşma hızı | işlem doğru | kelime hatası |
|---|---|---|---|
| kadın — sakin | 1,5 kelime/sn | **%92** | %15 |
| hızlı-1 — ağzının içinde | 2,5 kelime/sn | **%56** | %45 |
| hızlı-2 — Rizeli, çok hızlı | 2,7 kelime/sn | **%58** | %35 |
| (17.10) İstanbul — hızlı | 2,7 kelime/sn | %67 | %33 |

**Sonuç:** şive değil HIZ belirleyici. Saniyede ~2,5 kelimenin üstünde
(heceler yutulunca) en iyi tanıyıcı bile %56–67'ye iniyor; sakin konuşmada
%86–92. Chrome dökümleri gelince kademe (b) bu üç ses için de hesaplanır.

### 17.3 · Sekiz sesin tam tablosu — kademe (b) (18.10)

Chrome dökümleri üç lite sese de gelince (`parali-chrome-yapay2.json`, 225
kayıt) kademe (b) hepsi için hesaplandı. Kural K-062 (b): Chrome + motor önce;
`oneri` ya da `plakasiz` çıkarsa aynı ses Speechmatics + ipucuna gider.
Kurmaca dükkânda (buyuk-set.json), Chrome'un hata verdiği kayıtlar düşülerek.

| ses | n | konuşma hızı | Chrome | Speechmatics | **kademe (b)** | paralıya |
|---|---|---|---|---|---|---|
| kadın — sakin (lite) | 25 | 1,5 | %82 | %92 | **%90** | %24 |
| İç Anadolu | 25 | — | %90 | %86 | **%90** | %8 |
| Ege | 25 | — | %78 | %86 | **%86** | %32 |
| Doğu | 25 | — | %84 | %76 | **%88** | %20 |
| Karadeniz | 49 | 2,3 | %78 | %79 | **%84** | %20 |
| İstanbul — hızlı | 24 | 2,7 | %56 | %67 | **%67** | %42 |
| hızlı-2 — Rizeli (lite) | 25 | 2,7 | %66 | %58 | **%70** | %32 |
| hızlı-1 — ağzında (lite) | 24 | 2,5 | %44 | %54 | **%54** | %50 |
| **TOPLAM** | **222** | | **%73** | **%75** | **%79** | **%27** |

(Hızlı-1'de bir kaydı Chrome hata verdi → n24; Speechmatics %56 yerine bu 24
kayıtta %54.)

**Okuma:**
- **Kademe sekiz sesin yedisinde iki tanıyıcının iyisine eşit ya da üstünde.**
  Tek istisna kadın sesi: %90, yalnız-Speechmatics %92 (bir cümle; Chrome'un
  "emin" göründüğü ama yanlış anladığı bir kayıt paralıya gitmedi). Toplamda
  kademe tek başına her iki tanıyıcıdan +4–6 puan iyi, paranın %27'siyle.
- **Pay kendini ayarlıyor:** İç Anadolu %8 → hızlı-1 %50. Para zor konuşmaya
  harcanıyor — tasarlandığı gibi.
- **Hız tavanı:** saniyede ~2,5 kelimenin üstünde kademe bile %54–70. Burada
  tanıyıcı değiştirmek çare değil (yutulan hece hiçbir dökümde yok); çare onay
  ekranı + "tekrar söyle". Gerçek ustanın hızı yayında ölçülecek (K-060
  kayıtları).
- **Sınır:** yapay ses, aynı şablon cümleler, ses başına 25 cümle → sayılar
  ±%8 civarı oynar. Sıralama ve yön güvenilir; mutlak yüzde değil.
