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

## Fotoğraf tarafı — henüz yok

Ruhsat ve parça listesi okuma için **gerçek fotoğraf** gerekiyor
(A-M32): 20–30 ruhsat, 10–15 parça listesi — atölye ışığında, buruşuk,
el yazısı olanlar dahil. Ölçülecek iki rakam: **alan başına doğruluk**
ve **ustanın kaç alan düzelttiği.** İkincisi asıl olan; %95 doğruluk
her ruhsatta bir düzeltme demektir ve usta bunu üç ruhsattan sonra
*"çalışmıyor"* diye okur.

Fotoğraflar gelince koşucusu bu klasöre `ruhsat-kos.js` olarak yazılır;
cetvel yine ortak olur.

---

## Kural

**Set depoda durur ve her turda koşar.** Sağlık denetimi *"ekran doğru
mu"* diye soruyor; bu set *"karar doğru mu"* diye soruyor. Motor
değişince, istem değişince, model değişince **önce burası koşulur.**
