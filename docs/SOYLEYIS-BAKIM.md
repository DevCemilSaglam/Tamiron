# Söyleyiş sözlüğü — bakım süreci (SOYLEYIS-BAKIM.md)

> **Karar:** K-063.3 — Cemil, 18.10.2026: *"Onaylı girişi kabul edelim. Ama
> bunu sistematik yapmak gerek."*
> **Kural tek cümle:** sözlüğe hiçbir şey kendiliğinden girmez; her aday
> **kanıtla** gelir, **regresyonla** sınanır, **Cemil onayıyla** girer,
> **gerekçesiyle** commit edilir.

Sözlük: `prototypes/kaynak/betik/05-soyleyis.js` (tek kaynak).
Gözden geçirme listesi: `arastirma/SOYLEYIS-SOZLUGU.md` (üretilir:
`node tools/soyleyis-liste.js`).

---

## 1 · Neden otomatik değil

Sözlükteki bir satır **her cümleye** uygulanır. Yanlış bir eşleme, o sesi
içeren her cümleyi sessizce bozar. Bu riski ölçümlerde yaşadık: Tur 8.4'te
bulanık eşleme "hava filtresi"ni "Yağ filtresi" yaptı. Söyleyiş gerçek bir Türkçe kelimeyse (⚑) daha da
tehlikeli: o kelimeyi ayrı anlamda kullanan ustanın doğru cümlesi bozulur.

Aynı sebeple **ret de kayıt altına alınır**: reddedilen aday her hafta
yeniden sorulmaz.

## 2 · Aday nereden gelir — üç kanal

| Kanal | Ne yakalar | Ne zaman açık |
|---|---|---|
| **A · Onay ekranı düzeltmesi** | Usta onay ekranında bir alanı değiştirdi: *tanıyıcının duyduğu* ↔ *ustanın son hâli*. Kelime hizalamasıyla farklı olan parça çifti çıkarılır | Backend (K-060 kaydıyla birlikte) |
| **B · "Tekrar söyle" çiftleri** | Aynı kayıtta iki deneme: birincisi anlaşılmadı, ikincisi anlaşıldı. İlk dökümde ikincinin kelimesi farklı yazılmışsa aday | Backend |
| **C · Bildirim** | Cemil ya da usta: *"ustalar X'e Y der"* | **Bugün açık** (sohbet / SOYLEYIS-SOZLUGU.md üstünden) |

A ve B'de aday **otomatik toplanır**, ama sözlüğe **girmez**: yalnız aday
listesine düşer.

## 3 · Aday kaydı

```json
{
  "duyulan": "selise",            // tanıyıcının yazdığı / ustanın söylediği biçim
  "dogru": "C-Elysée",            // kayıttaki doğru biçim
  "tur": "model",                 // marka | model | terim | is-sozu
  "kapsam": "genel",              // genel | dukkan:<id>
  "kanal": "A",                   // A | B | C
  "sayac": 4,                     // kaç kez görüldü
  "gunler": 3,                    // kaç farklı günde
  "dukkanlar": 2,                 // kaç farklı dükkânda
  "ornekler": ["... selise yağ bakımı ..."],  // en çok 3; plaka ve telefon MASKELİ
  "ilk": "2027-01-04", "son": "2027-01-09"
}
```

**Kişisel veri yok:** örnek cümlelerde plaka `## XX ###`, telefon `#`,
müşteri adı `[ad]` olarak maskelenir (K-060 gizlilik cümlesi).

## 4 · Eşik — gözden geçirme listesine ne düşer

| Kapsam | Eşik |
|---|---|
| **Genel** (her dükkân) | ≥ 3 kez **ve** ≥ 2 farklı dükkân |
| **Dükkâna özel** (takma ad, müşteri adı, yerel söyleyiş) | ≥ 3 kez **ve** ≥ 2 farklı gün |
| **Kanal C** (bildirim) | eşik yok — doğrudan listeye |

Eşiğin altındaki aday silinmez, beklemede kalır. 90 gün eşiğe ulaşmazsa
düşer (K-060'ın saklama süresiyle aynı).

## 5 · Haftalık döngü

**Her pazartesi, lider:**

1. **Listeyi üret.** Eşiği geçen adaylar, sayaca göre sıralı.
2. **Her aday için üç sınav:**
   - **Gerçek kelime mi?** Duyulan biçim Türkçede başka bir anlamı olan bir
     kelimeyse ⚑ ile işaretlenir. O zaman yalnız markayla birlikte geçtiğinde
     araç sayılır, terim düzeltmede kullanılmaz (05-soyleyis.js'teki "!" kuralı).
   - **Çakışma:** duyulan biçim başka bir doğru biçimin söyleyişiyle ya da
     dükkânın bir plakası / müşteri adıyla çakışıyor mu? Çakışıyorsa ret ya da
     dükkâna özel.
   - **Regresyon:** aday eklenmiş sözlükle **bütün setler** yeniden koşulur:
     ses-seti, büyük set, söyleyiş seti ve gerçek kayıt dökümleri. Eski ve yeni
     çıktı `tools/ses-esdeger.js` ile karşılaştırılır. **Şart: bozulan cümle
     sıfır.** Düzelen cümle sayısı yazılır.
3. **Tek sayfa çıkar:** her aday bir satır. Satırda:
   - duyulan → doğru
   - kaç kez, kaç dükkân
   - bir örnek
   - regresyon sonucu (*+2 düzeldi, 0 bozuldu*)
   - lider önerisi (kabul / dükkâna özel / ret + neden)
4. **Cemil onaylar:** her satıra evet ya da hayır.

**Onaydan sonra:**

5. Kabul edilenler `05-soyleyis.js`'e eklenir. Dükkâna özel olanlar o
   dükkânın verisine girer.
6. `SOYLEYIS-SOZLUGU.md` yeniden üretilir.
7. Ret listesine (`soyleyis-ret.json`) ret nedeniyle birlikte yazılır.
8. Commit mesajı şunları söyler: kaç aday geldi, kaçı girdi, regresyon
   sonucu ve **neden**.

**Bir aday regresyonda bir cümleyi bozuyorsa Cemil'e kabul önerisiyle
gitmez.** Ya daraltılır (yalnız markayla birlikte, dükkâna özel) ya da
reddedilir.

## 6 · Geri alma

Her ekleme tek commit'tir. Eklemeden sonraki iki haftada o kelimeyi içeren
cümlelerde onay ekranında düzeltme oranı **artarsa** ekleme geri alınır ve
ret listesine "geri alındı" diye yazılır.

## 7 · İzlenen tek rakam

**Haftalık düzeltme oranı:** onay ekranında ustanın değiştirdiği alanların,
gösterilen alanlara oranı. Sözlük işini yapıyorsa bu oran düşer. Raporda
genel oran ve sözlük kelimesi içeren cümlelerin oranı ayrı verilir.

## 8 · Bugün (prototip fazı) ne yapılır

- Yalnız **kanal C** açık. Cemil ya da usta söyler; lider aynı sınavları
  uygular (⚑ · çakışma · regresyon) ve tek satırlık bir onay ister.
- Kanal A ve B'nin verisi backend ve K-060 kayıtlarıyla başlar. Aday
  toplayıcı (`tools/soyleyis-aday.js`) o fazda yazılır. **Bu fazda kod yok.**
