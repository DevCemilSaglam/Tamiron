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

## 10 · Önceki hâli: onaya açık maddeler

1. **E yolu** ve *"gürültü uydurulmaz"* kuralının esnetilmesi — gerçek atölye
   sesi ama bizim dükkânın değil.
2. **Kendi sesin** (40 cümle, ~20 dk) — ve istersen 2–4 tanıdığın.
3. **İndirme:** MediaSpeech TR (~618 MB, CC BY 4.0) ve seçilen CC0 gürültü
   dosyaları. Ses dosyaları depoya **girmez** (`.gitignore`, ruhsat
   fotoğraflarıyla aynı kural); yalnız kaynak + lisans listesi girer.
