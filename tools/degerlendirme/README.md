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
node tools/degerlendirme/model-kos.js --model claude-sonnet-5
node tools/degerlendirme/model-kos.js --model claude-haiku-4-5
```

**Anahtar bu depoda durmaz** — ortam değişkeninden okunur; commit'lenen
hiçbir dosyada anahtar yoktur ve olmayacaktır.

**Maliyet (40 cümle, istem önbellekli kaba hesap):** Opus 5 ~$0,25 ·
Sonnet 5 ~$0,10 · Haiku 4.5 ~$0,05. Üçünü de koşmak **yarım dolardan
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
| `ruhsat-seti.json` | 46 belge · 14 alan · zorluk etiketleri · **yasak alanlar** |
| `parca-seti.json` | 27 liste · **258 satır** · 42 ditto · iptal/durum/çarpım kuralları |
| `foto-kos.js` | Koşucu — `--tur ruhsat` \| `--tur parca` |

### ⚠️ Bu ölçüt henüz TASLAK

Fotoğrafları okuyup alanları yazan **modelin kendisiydi** (Claude,
görüntü). Bir modelin kendi okumasını kendi ölçütü yapması, sınavı
kendi yazıp kendi puanlaması olur. Şüphelenilen her alanın yanında
`guven` notu var; doğrulama oradan başlar. **Cemil doğrulayana kadar
buradan çıkacak rakam bir karne değil, bir tahmindir.**

### Koşmak için üç şey

1. **Fotoğraflar diske** — `ruhsatlar/r01.jpg … r46.jpg` ve
   `parcalar/p01.jpg … p27.jpg`. Sıra Cemil'in gönderdiği sıradır;
   her kaydın plakası/kâğıdı ölçütte yazılı, eşleştirme gözle
   doğrulanabilir. **Klasörler `.gitignore`'da** — ruhsatta TC no ve
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
* **yasak alan** — TC kimlik no, adres, sahip adı. Model bunları
  döndürürse **değeri doğru olsa bile** o okuma başarısızdır
  (RUHSAT-OKUMA Bulgu 2 · A-31).

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
