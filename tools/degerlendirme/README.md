# DEĞERLENDİRME — iki motor, tek cetvel

> Cemil, 14.09: *"Yapay zeka testini yapamayacak mıyız?"*
> **Yapıyoruz.** Testin yarısı bugün koştu ve bir rakam üretti; öteki
> yarısı bir API anahtarı bekliyor.

---

## Neyi ölçüyor

Ustanın söylediği cümlenin **doğru işleme** çevrilip çevrilmediğini.
Ölçülen şey ekran değil, ekrana giden karar:

```
   cümle  ->  [ motor ]  ->  işlem nesnesi  ->  (karşılaştır)  ->  puan
                  ^
       bugünkü desen eşleştirici  ·  ya da  ·  Claude
```

| Dosya | Ne |
|---|---|
| `ses-seti.json` | **40 cümle + beklenen çıktı.** Kolaylar, tuzaklar (K-018), zorlar ve *"anlamamalı"* olanlar |
| `puanla.js` | **Cetvel — tek yerde.** İki koşucu da bunu kullanır; iki cetvel olsa karneler karşılaştırılamazdı |
| `tarayicida-kos.js` | Koşucu 1 — bugünkü motor (tarayıcıda, **anahtarsız**) |
| `model-kos.js` | Koşucu 2 — Claude (Node, **anahtar ister**) |
| `ses-bozma.js` | **Bozmalar — tek yerde.** Uzak · kırpık · SNR karıştırma; Node'da `ses-bozucu.js`, tarayıcıda `ses-dosya-kos.html` aynı fonksiyonu çağırıyor (12.10) |
| `ses-kaynaklari.md` | Dış ses kaynakları ve **lisansları** — dosyalar depoda değil, adresleri burada |
| `ses-dosya-kos.html` | Koşucu 3 — **ses dosyası** → tarayıcının tanıyıcısı → aynı çözümleyici → aynı cetvel. Bozma türü başına karne + kelime hatası. Mikrofon gerekmez (`start(track)`, Chrome 152'de denendi). ⚠️ Gerçek Chrome'da baştan sona **henüz doğrulanmadı** — `docs/arastirma/SES-OLCUM.md` (A-M48) |

**Puanlama:** tür yanlışsa **kısmi puan yok** (yanlış tür, ustayı yanlış
ekrana götürür). Tür doğru ama alan yanlışsa **yarım**. `plakasiz`
beklenen cümlelerde doğru cevap **anlamamaktır** — uydurmak,
anlamamaktan kötüdür.

---

## Koşucu 1 — bugünkü motor (anahtarsız)

Prototip `localhost:8123`'te açıkken konsola:

```js
var k = await (await fetch('/tools/degerlendirme/tarayicida-kos.js')).text();
eval(k); await DEGERLENDIR();
```

### 14.09.2026 karnesi

| Koşu | Sonuç |
|---|---|
| İlk koşu | **%83** — 33 tam · 7 yanlış |
| Setin kendi iki hatası düzeltildikten sonra | %88 |
| Motorun iki eksiği kapatıldıktan sonra | **%93** — 37 tam · 3 yanlış |

**İlk koşu setin kendisini de sınadı:** iki cümlenin beklentisi yanlıştı
(serviste olmayan bir araca *"teslim ettim"* dedirtmişim; açık kayıtta
olmayan bir parçanın fiyatını güncelletmeye çalışmışım). *Değerlendirme
seti de bir programdır ve ilk koşusunda hata verir.*

**İki gerçek eksik bulundu ve kapatıldı:**
* *"herkese **sms** at yarın kapalıyız"* hiç anlaşılmıyordu — `sms`
  artık mesaj komutu sayılıyor.
* *"...**sahibine** parça bekliyoruz de"* servis kaydı sanılıyordu.
  Türkçe burada işi kolaylaştırdı: **"sahibine" yönelme hâli (mesaj),
  "sahibini" belirtme hâli (arama)** — ikisi ayrı kelime.

### Kalan üç cümle — modelin sınavı

| # | Cümle | Bugün |
|---|---|---|
| 5 | *"şu Mégane'ın trigerini değiştirdik"* | anlamıyor (plaka yok, marka var) |
| 6 | *"Mehmet Kaya'nın arabası geldi yağ değişimi yapılacak"* | anlamıyor (sahip adından araç) |
| 39 | *"34 ABC 123 sahibini ara telefonla"* | arama sanıyor (gerçekten belirsiz) |

**Desen eşleştiriciyle bu üçü çözülmez.** Çözmeye çalışmak — her marka
adını, her sahip adını kelime listesine eklemek — motoru bakımı imkânsız
bir yığına çevirir. **Model tam buraya giriyor.**

---

## Koşucu 2 — model (anahtar ister)

```bash
npm install @anthropic-ai/sdk
set ANTHROPIC_API_KEY=sk-ant-...
node tools/degerlendirme/model-kos.js
node tools/degerlendirme/model-kos.js --model claude-sonnet-5-5
node tools/degerlendirme/model-kos.js --model claude-haiku-5-5
# ses + model: Web Speech'in duyduğu metni modele ver
node tools/degerlendirme/model-kos.js --duyulan <ses-olcum-....json>
```

`--duyulan`: ses-dosya-kos.html'in indirdiği sonuç dosyasındaki `duyulan`
metinler cümlenin yerine geçer; tanınamayan kayıt puana girmez (ses
koşucusuyla aynı payda). `--efor low|medium|high` düşünme derinliği.

⚠️ Opus 5.5 / Sonnet 5.5 zorunlu araç çağrısını (`tool_choice: tool`)
400 ile reddediyor; koşucu aracı istemle istiyor, araç gelmezse cevap
YANLIŞ sayılıyor. Ret (`refusal`) başka modele düşürülmüyor — ölçülen
model değişirse rakam o modelin olmaz.

**Anahtar bu depoda durmaz** — ortam değişkeninden okunur; commit'lenen
hiçbir dosyada anahtar yoktur ve olmayacaktır.

**Maliyet (40 cümle kaba hesap; gerçek rakam koşu sonunda basılır):**
Opus 5.5 ~$0,25 · Sonnet 5.5 ~$0,12 · Haiku 5.5 ~$0,01. Üçünü de koşmak **yarım dolardan
az** — ve A-M30 (*"hangi model"*) kararını rakamla verdirir.

**İki motor aynı dünyayı görüyor:** model koşucusu dükkân özetini
prototipin kendi tohum verisinden (`02-veri.js`) üretiyor ve
*"kayit-icinde"* bağlamı için tarayıcı koşucusuyla **aynı kaydı**
seçiyor (06 TMR 07). Ayrı bir kopya tutulsaydı iki motor iki ayrı
dünyada sınanmış olurdu.

**Çıktı serbest metin değil:** şema `strict: true` bir araçla
dayatılıyor. Modelin döndürdüğü nesne, ekranın bugün beklediği nesnenin
aynısı — model **prototipin yerine değil, bir fonksiyonun yerine**
geçiyor (`YAPAY-ZEKA.md` §6).

---

## Fotoğraf tarafı — ölçüt yazıldı (15.09)

Cemil **46 ruhsat + 27 parça listesi** gönderdi. İkisinin de ölçütü
yazıldı; koşucusu hazır, üç şey bekliyor (aşağıda).

| Dosya | Ne |
|---|---|
| `ruhsat-seti.json` | 46 belge · **18 alan** (dördü kişisel — fatura için, K-050) · zorluk etiketleri |
| `parca-seti.json` | 27 liste · **258 satır** · 42 ditto · iptal/durum/çarpım kuralları |
| `foto-kos.js` | Koşucu — `--tur ruhsat` \| `--tur parca` |
| `foto-esle.js` | Kaydı fotoğrafıyla eşleştirir — **ada göre değil, MD5'e göre** |

### ⚠️ Kişisel alanların çoğu fotoğrafta YOK (K-051)

46 ruhsat fotoğrafı sahip bölümü için tek tek tarandı:

| Sahip bölümü (TC/vergi no · soyad · ad · adres) | Kaç fotoğraf |
|---|---|
| **Okunuyor** | **17** (%37) |
| Değer sağ kenarda **kesik** — etiket var, değer yok | 18 |
| Sahip sayfası **hiç çekilmemiş** | 10 |
| Parlamada kaybolmuş | 1 |

Sebep belgenin geometrisi: sahip bilgileri **sağ sayfada**, usta araç
alanlarına odaklanıp çekiyor, sağ kenar kadraja girmiyor. Bu 29 kaydın
her kişisel alanına **neden boş olduğu** yazıldı (`guven` notu).
**Boş alan ile okunamayan alan aynı şey değil** — ilki ölçüt eksiği,
ikincisi ölçütün kendisi; ayırt edilmezse sonraki turda model haksız
yere suçlanır.

### Ölçüt DOĞRULANDI (A-M34 kapandı)

Alanları fotoğraflardan okuyan Claude'du; bir modelin kendi okumasını kendi
ölçütü yapması sınavı kendi yazıp kendi puanlaması olurdu. Bu yüzden iki set
de Cemil doğrulayana kadar TASLAK'tı. **Cemil fotoğrafla yan yana tek tek
karşılaştırdı: parça 27/27 (16.09), ruhsat 46/46 (18.10), 0 düzeltme.** Her
dosyada `dogrulama` alanı kimin, ne zaman, hangi yolla onayladığını yazar.
Artık bu setten çıkan rakam bir karnedir.

### Kontrol sayfası — ölçütü doğrulamanın yolu

`kontrol-sablon.html` + `kontrol-uret.js` → **iki sayfa**:
`kontrol-ruhsat.html` (46 belge) ve `kontrol-parca.html` (27 liste).
Her kaydın **fotoğrafı yanında**, **Doğru / Düzelt / Fotoğrafı
bulamadım** diye işaretleniyor; işaretler ortak yerde durduğu için
sayfalar arası geçiş bir şey kaybettirmiyor.

Ayrı sayfa Cemil'in isteği (15.09): ruhsat **alan** doğruluyor, parça
listesi **satır** — 73 kart tek listede alt alta gelince ikisi
birbirine karışıyor ve ilerleme çubuğu *"ne kadar kaldı"*yı söylemez
oluyordu.

```
node tools/degerlendirme/foto-esle.js <sohbet.jsonl> C:\...\Desktop\Tamiron
node tools/degerlendirme/kontrol-uret.js
```

Sayfa **fotoğrafların yanına** yazılıyor (`…\Desktop\Tamiron\kontrol.html`)
ve resimlere göreli yoldan bakıyor; çift tıkla da, yerel sunucudan da
açılır.

* Süzgeç varsayılanı **Bakılmadı** — kaldığın yerden devam edersin.
* **Şüpheli alan var** süzgeci şüpheli 21 kaydı ayırıyor (`guven` notu
  olan 14 belge + okunamayan satırı olan listeler); doğrulama oradan
  başlarsa iş yarıya iner.
* Fotoğraf **yan çekilmişse** `Döndür ⟳`; tam boy için `Büyüt`.
* Kişisel alanlar (TC/ad/adres) **açık geliyor**. Maskeli
  başlatmıştım; Cemil *"TC / Vergi no alanlarını göremiyorum"* dedi.
  Sayfanın tek işi bu alanları doğrulatmak — doğrulanacak şeyi
  varsayılan gizlemek, aracı kendi işine engel yapar. Gizleme düğmesi
  duruyor.
* **Üretilmiş sayfa depoya girmez** (`.gitignore`): kişisel dosya
  birleştiğinde içinde gerçek TC ve adres olur. Şablon ve üreteç depoda,
  çıktısı değil.

### Fotoğrafı kayda bağlamak — ada göre DEĞİL

`foto-esle.js`, sohbet kaydındaki (`.jsonl`) görsellerin **MD5**'ini
diskteki dosyalarınkiyle karşılaştırıyor. 15.09 koşusunda **73/73
birebir** tuttu.

Dosya adından sıralamak iki kez denendi, ikisi de yanlış çıktı ve
**gözle yakalandı**: indirme sırasının 1.si `p16`, 13.ü `p21`. WhatsApp
indirme sırası gönderme sırası değil; üstelik klasörde gönderilmemiş
7 eski fotoğraf da duruyordu (80 dosya, 73 görsel). *Kayan bir eşleme
ölçütü sessizce çöpe çevirir:* kontrol eden kişi başka belgeye bakıp
**"yanlış okumuşsun"** der, ölçüt bozulur. Doğrulama üç kayıtta gözle
de tekrarlandı (p16 · p21 · r45).

### Düzeltmeler bana nasıl ulaşıyor

* **Artifact'te açılırsa** → doğrudan veritabanına yazılır, oradan
  okurum.
* **Diskten açılırsa** (fotoğraflı olan bu) → `claude` nesnesi yoktur;
  düzeltmeler tarayıcıda durur ve **“Düzeltmeleri indir”** tek dosya
  çıkarır. Sayfa bunu şerit olarak söylüyor — eskiden sessizce
  `ReferenceError` atıp hiçbir şey demiyordu.


### Koşmak için üç şey

1. **Fotoğraflar diske** — `foto-harita.json` hangi kaydın hangi
   dosya olduğunu söylüyor (yukarıdaki MD5 eşleştirmesi).
   **Fotoğraflar da harita da `.gitignore`'da** — ruhsatta TC no ve
   adres var.
2. **API anahtarı** — ortam değişkeninde, depoda değil.
3. **Ölçütün doğrulanması** — yukarıdaki `guven` notları.

### Ne ölçülüyor — ve neden iki rakam birden

```
belge: 38/46 tam (%83) · alan: %97 · uydurma: 2 · yasak alan: 0
```

**Alan başına doğruluk (%97) yanıltıcıdır.** Asıl rakam **belge başına
tamlık**: %83, her altı ruhsattan birinde ustanın bir şey düzelttiği
anlamına gelir — ve usta üç düzeltmeden sonra *"bu çalışmıyor"* der.

Ayrı sayılan iki davranış var, ikisi de doğruluktan önemli:

* **uydurma** — okunamayan alanı boş bırakmak yerine doldurmak. Boş
  alan ustaya *"sen yaz"* der; uydurulan alan **yanlış bilgiyi doğru
  gibi** verir ve usta onu bir daha kontrol etmez.
* **kişisel alanlar** — TC/vergi no, soyad-ünvan, ad, adres. 15.09'a
  kadar bunlar *yasaktı*; Cemil **fatura bunlara kesiliyor** deyince
  kural geri alındı (K-050). Artık okunmamaları eksiklik. Değerleri
  `ruhsat-seti-kisisel.json`'da — depoya girmez; dosya yoksa bu dört
  alan ölçülmez ve koşucu bunu söyler.

### Setin en sert dört kaydı

| # | Ne | Doğru davranış |
|---|---|---|
| `r31` | VW Passat — **plaka okunamıyor** (sol sütun soluk) | Plakayı boş bırak. Plaka döndüren her cevap başarısız |
| `r45` | Peugeot 308 — **parmak tescil tarihini kapatıyor** | Tarihi boş bırak |
| `r03·r07·r12` | **Eski tip ruhsat** — yakıt, güç, silindir, koltuk **alanları yok** | Doldurma |
| `p09` | Öznur listesi — **iki fiyat (YN/ORJ) + durum metinleri + ditto** bir arada | Üçünü de ayır |

Ve üç **tutarlılık** çifti: aynı belgenin farklı çekimleri
(`r04·r15·r43`, `r10·r18`, `r33·r42`) ile aynı listenin fiyatsız/fiyatlı
hâli (`p24`→`p23`, `p26`→`p02`→`p08`). Aynı kâğıt, aynı cevabı vermeli.

---

## Kural

**Set depoda durur ve her turda koşar.** Sağlık denetimi *"ekran doğru
mu"* diye soruyor; bu set *"karar doğru mu"* diye soruyor. Motor
değişince, istem değişince, model değişince **önce burası koşulur.**
