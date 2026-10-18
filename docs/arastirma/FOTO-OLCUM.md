# Fotoğraf okuma ölçümü — ruhsat ve parça listesi (A-M35)

> **18.10.2026.** Cemil: *"Gerçek doğruluğu ve maliyeti ölçelim."*
> Set: 46 gerçek ruhsat + 27 gerçek parça listesi (Cemil'in fotoğrafları,
> ölçütü Cemil doğruladı — K-064.1). Koşucu `tools/degerlendirme/foto-kos.js`,
> cetvel `puanla.js`. Fotoğraflar ve sonuç dosyaları depoya girmez.
> Toplam harcama: **~$3,7** (deneme koşuları dahil).

## 1 · Sonuç

### Ruhsat (46 belge)

| | Haiku 5.5 | Sonnet 5.5 | **Opus 5.5** |
|---|---|---|---|
| **Şase** (K-066'nın kapısı) | 44 doğru · **2 YANLIŞ** | 45 doğru · 1 boş | **46/46 doğru** |
| Kritik alan yanlış (plaka · marka · ticari ad · model yılı · şase) | 4 | 1 | **0** |
| **Plaka uydurma** (r31: plaka okunamıyor) | yok | **"34 NE 6451" UYDURDU** | yok |
| Uydurma (belgede olmayanı doldurma) | 4 | 8 | 3 |
| Alan doğruluğu | %94 | %96 | %96 |
| Belge tam (hiç düzeltme yok) | %39 | %57 | %54 |
| Belge başına maliyet | ~$0,001 | ~$0,016 | **~$0,029** |

### Parça listesi (27 liste · 258 satır)

| | Haiku 5.5 | Sonnet 5.5 | **Opus 5.5** |
|---|---|---|---|
| Satır bulundu | %86 | %94 | **%95** |
| Ditto (" " ") çözüldü | %69 | %95 | **%100** |
| Fiyat doğru | %76 | %87 | **%91** |
| İptal (üstü çizili) · işçilik satırı | 2/2 · 4/4 | 2/2 · 3/4 | **2/2 · 4/4** |
| Fazla satır (uydurma) | 31 | 14 | 13 |
| Liste tam | %33 | %56 | %56 |
| Liste başına maliyet | ~$0,001 | ~$0,017 | **~$0,030** |

## 2 · Okuma

- **Ruhsatta tek güvenilir model Opus 5.5.** 46 şasenin 46'sı doğru,
  hiçbir kritik alan yanlış değil, plaka uydurmuyor.
  - **Sonnet elenir:** en önemli tuzakta (r31, plaka tamamen silik) plaka
    uydurdu. Kural K-061.3c: *uydurulan plaka doğruluktan önce sayılır*,
    çünkü yanlış araca kayıt açar.
  - **Haiku elenir:** iki şaseyi yanlış okudu. r39'u 16 karakter okudu;
    17 karakter kapısı bunu yakalar. r08'de ise tek harfi (F→P) yanlış
    okudu ve 17 karakteri tam; bu **sessizce geçer** ve parçacıya yanlış
    araç gider (K-066.4). r45'te parmağın kapattığı tarihe "03/2008"
    uydurdu.
- **"Belge tam" %54 neden düşük:** kalan farklar kritik olmayan alanlarda.
  Adres (10, kısaltma/eksik), cinsi yazımı (5, ör. "OTOMOBİL (AA SEDAN)"),
  silindir (5), tip (3). Usta bunları ekranda görür ve düzeltir. İz kaydı
  bu düzeltmeleri biriktirir (K-054).
- **Opus'un 3 uydurması:** iki ruhsatta silik koltuk sayısına "5", bir
  ruhsatta silindire değer yazdı. Kritik değil, ama "okuyamadığını boş
  bırak" kuralı tam tutmuyor. İstem iyileştirmesi adayı.
- **Parça listesinde Opus ile Sonnet başa baş.** Liste tamlığı ikisinde de
  %56. Opus ditto (%100) ve fiyatta (%91) önde. Kalan hataların türü:
  - **Gerçek yanlış okuma:** "İticiler" → "İticekler", "Kapler" → "Kepler".
  - **Ustanın yazdığını "düzeltme":** "Sibop kayıt" → "Subap kayışı",
    "Arka gren keçe" → "Arka krank keçesi". İstem *"yazıldığı gibi al"*
    diyor; model yine de düzeltiyor. İstem iyileştirmesi adayı.

## 3 · Maliyet — bir dükkân

Günde 5 ruhsat + 2 parça listesi, Opus 5.5:
5 × $0,029 + 2 × $0,030 ≈ **$0,21/gün ≈ $6/ay**. Ayda 150–300 ruhsat
yalnız ruhsat için **$4–9/ay**.

> ⚠️ Rakamlar `foto-kos.js`'teki fiyat tablosundan hesaplandı (önbellek
> dahil). Kesin tutar için Anthropic Console'daki harcamaya bakılmalı.
> Haiku fiyatı tablodaki değerle ($0,10/$0,50) Console'dan doğrulanmadı.

## 4 · Ölçüm aletinde bulunan ve düzeltilen hatalar

Koşudan önce ve sonra **yedi** kusur çıktı. Hepsi ya rakamı
şişiriyordu ya da modeli haksız yere cezalandırıyordu.

1. **Parça listesinin cetveli YOKTU.** `alanPuanla` satırları ölçemiyor ve
   her listeye "TAM" diyordu. `listePuanla` yazıldı; kendisiyle %100, boş
   cevapla %0 verdiği doğrulandı.
2. **İsteğe bağlı alanlar hiç gelmedi.** Model en temiz ruhsatta bile
   silindir, güç ve yakıtı, parça listesinde ise bütün fiyatları
   göndermedi. Şemadaki bütün alanlar zorunlu yapıldı; okunamayan alan `""`.
3. **Boş alana "null" yazısı yazıldı** → artık null sayılıyor.
4. **İ/I farkı** ("CİTROEN"/"CITROEN") kritik yanlış sayılıyordu → katlandı.
5. **Şasede O/0 farkı** ("WOLO"/"W0L0") yanlış sayılıyordu → ISO 3779'a
   göre aynı.
6. **Boş kritik alan "kritik yanlış" sayılıyordu** → artık "eksik". Boş
   alan ustaya yeniden çektirir, yanlış alan sessizce geçer.
7. **"Ad + not" ayrı satır sayılıyordu** ("Ön amortisör (Sağ-Sol)") → aynı
   satır.

## 5 · Ölçütte bulunan hata — insan doğrulaması da yanılır

Üç model **aynı üç plakayı** ölçütten farklı okudu. Fotoğrafa bakıldı:
**ölçüt yanlıştı.** Hepsini 15.09'da Claude okumuş, 18.10'da Cemil de
gözden kaçırmıştı.

| | ölçüt (yanlış) | fotoğraf = modeller |
|---|---|---|
| r26 | 16 LEC 87 | **16 DEC 87** |
| r39 | 41 ER 309 | **41 FR 309** |
| r41 | 41 BBH 883 | **41 RRH 883** |

Düzeltildi (`ruhsat-seti.json` → `dogrulama.duzeltme`). **Ders:** bağımsız
modellerin birleştiği ama ölçütten ayrıldığı yer, ölçütün kendisini sınar.
Gelecek her yeni sette bu kontrol koşudan sonra yapılır.

## 6 · Maliyeti düşürmek (18.10, sürüyor)

> Cemil: *"Ruhsat okuma tutarını düşürmenin yoluna ihtiyacımız var."*

**Maliyet nereden geliyor (Opus 5.5, ruhsat başına ~2,9 cent):**

| | jeton | pay |
|---|---|---|
| girdi (fotoğraf + istem) | ~2.200 | ~0,9 cent |
| **çıktı** | ~1.000 | **~2,0 cent** |

18 kısa alanın cevabı ~150 jeton; çıktının kalanı modelin düşünmesi.

**Denemeler** (`foto-kos.js --efor`, `--foto-kok`, `foto-kucult.ps1`):
- Tek belge (r27), `efor=low` + 1100 px: girdi **2.199 → 1.305** (−%40),
  çıktı ~650. Okuma tam doğru.
- **Tam koşu YARIM KALDI:** Anthropic kredisi bitti ("credit balance is
  too low"). 38 okuma (~$0,7) yapılmıştı ama koşucu durunca sonucu
  yazmıyordu, kayboldular. **Düzeltildi:** artık durunca o ana kadarki
  cevaplar `-YARIM` diye kaydediliyor. Geçersiz anahtarla sınandı.
- Kredi yüklenince üç deneme koşulacak: Opus `low`, Opus `low` + 1100 px,
  Sonnet `low`. Tahmin ~$1,5.

**Göndermeden önce kalite denetimi (yeniden çektirme) maliyeti düşürür mü?**
Opus'un sonuçlarına bakıldı:
- Kalite sorunu etiketli 23 fotoğrafın **11'i**, temiz 23 fotoğrafın
  **14'ü** tam okundu. Opus kötü fotoğrafta da iyi okuyor.
- Yeniden çekmenin kurtaracağı tek örnek r11: plaka parlamanın altında.
  r31'de belgenin kendisi silik; yeniden çekmek kurtarmaz.
- **Sonuç:** maliyeti çok az düşürür (en çok ~%5). Asıl değeri doğrulukta
  ve **kırpmada**: telefonda belgenin kenarını bulup yalnız belgeyi
  göndermek. Fotoğrafların çoğunda belgenin çevresi masa, el ve kılıf;
  kırpma girdiyi küçültmenin en temiz yolu.

## 7 · Ödemesiz yol: Gemini (önceden yüklü kredi) ve yerel model (18.10)

> Cemil: *"Şu an ödeme yapamam. Başka ücretsiz modeller yok mu?"*
> ⚠️ **Ücretsiz bulut katmanları kullanılmadı**: ruhsatta gerçek kişilerin TC
> numarası ve adresi var. Gemini, Cemil'in daha önce yüklediği kredinin
> kalanıyla **ücretli katmanda** koşuldu; Google'ın fiyat sayfasına göre
> ücretli katman verisi ürün geliştirmede kullanılmıyor. Yerel modelde
> fotoğraf bilgisayardan çıkmadı.

### Ruhsat — 46 belge, aynı cetvel

| | şase | kritik yanlış (hepsi plaka) | uydurma | belge tam | belge başına | süre |
|---|---|---|---|---|---|---|
| **Opus 5.5** | **46/46** | **0** | 3 | %54 | 2,98 ¢ | — |
| Sonnet 5.5 | 45 + 1 boş | 1 (r24 287→267) | 7 | %57 | 1,56 ¢ | — |
| **Gemini 3.8 Flash** | **46/46** | 5 | 12 | %48 | **0,48 ¢**¹ | 8 sn |
| Gemini 3.5 Flash-Lite | 44 · **2 yanlış** | 8 | 32 | %30 | 0,11 ¢ | 1,6 sn |
| Haiku 5.5 | 44 · **2 yanlış** | 4 | 4 | %39 | 0,10 ¢ | — |
| Gemma 4 12B (yerel) | koşuyor | | | | **0** | ~470 sn |

¹ Gemini 3.8 Flash'ın fiyatı 01.01.2027'de iki katına çıkıyor (~0,96 ¢).

**Okuma:**
- **Gemini 3.8 Flash, Opus'un altıda biri fiyatına şaseyi 46/46 okudu.**
  K-068'e göre parça sormanın kapısı şase, yani en önemli alanda Opus'a
  eşit.
- **Zayıflığı plakada.** 5 plakayı yanlış okudu:
  - r39 "ER", r41 "BBH": 15.09'daki ilk okumamızın yaptığı hatanın aynısı;
    silik basılmış F/E ve R/B harfleri.
  - r11 477→677 (parlamanın altında).
  - r44 "41 HH 036".
  - r31 "NES 451".
- **Ama ekranda plaka zaten ustadan geliyor.** "Plakayı yaz" varsayılan sekme
  ve ses de plakayla başlıyor. Ruhsattan okunan plaka ustanın yazdığıyla
  **karşılaştırılabilir**; tutmazsa ekran sorar. Böylece plaka hatası
  sessizce geçmez, şase hatası ise geçerdi. Bu karşılaştırma **henüz
  prototipte yok**; Cemil onayına açık bir öneri.
- Flash-Lite ve Haiku şasede hata yaptı → elenir (K-066.4).

### Ölçütte bir düzeltme daha: r31

Sonnet "34 NE 6451", Gemini "34 NES 451" okudu. Ölçütte plaka boştu
(*"sol sütun tamamen soluk"*). Fotoğraf büyütüldü: **silik ama okunuyor,
34 NE 6451.** Ölçüt düzeltildi.
**Sonnet'i K-067'de "plaka uydurdu" diye elemiştim. O gerekçe yanlıştı;
Sonnet doğru okumuştu.** Sonnet'in gerçek kritik hatası 1: r24'te 287→267.
Bu, ölçütte bulunan 4. plaka hatası. 46 belgenin 4'ünde plaka ölçütü
yanlıştı ve hepsini, birden fazla bağımsız modelin aynı şeyi okuması
gösterdi.

### Yerel model (Gemma 4 12B)
Ücretsiz ve veri dışarı çıkmıyor, ama bu dizüstünde **ruhsat başına ~8
dakika**. İlk denemede (r27) şase doğruydu, 18 alanın 17'si tamdı. Tam koşu
arka planda (~6 saat). Ürün için anlamı: hız ancak ekran kartlı bir
sunucuda kabul edilebilir olur; o da ayrı bir maliyet.

⚠️ **Olay (18.10, 17:30):** ilk yerel koşuda bellek tükendi; boş RAM 0,1 GB'a,
C'deki boş yer sayfa dosyası yüzünden 4 GB'a indi. Sebep: model D'ye
taşınırken durdurulan ilk Ollama'nın `llama-server` alt süreci kapanmamıştı;
iki kopya 22 GB yer tutuyordu. Fazla süreçler kapatıldı, bağlam penceresi
8.192'ye indirildi. Ayrıca bir **bellek bekçisi** kuruldu: boş RAM 2 GB'ın
altına düşerse koşuyu durdurur. Model C'den `D:\Ollama\models`'e taşındı
(`OLLAMA_MODELS`).

### Yerel Gemma 4 12B — sonuç: ELENİR (18.10 gece)
Koşu 9 ruhsattan sonra yarıda kaldı: yerel model sunucusu çöktü (500,
"connection forcibly closed"). Koşucu yarım sonucu kaydetti. Okunan 9 belgede:
- **şase 2 doğru, 4 YANLIŞ**, 3 belgede içerik boş geldi (model çıktıyı
  düşünmeye harcadı; artık `think:false`);
- belge tam 1/9;
- ruhsat başına ~69 sn (ilk tek belgelik denemede 467 sn; bağlam
  küçültülünce hızlandı).

12B Gemma, 17 karakterlik şaseyi güvenilir okuyamıyor. Ruhsatta en kritik
alan şase (K-068) olduğu için elenir. Sırada OCR'da güçlü bilinen
**Qwen3-VL 8B** (yerel, 6,1 GB) var.
