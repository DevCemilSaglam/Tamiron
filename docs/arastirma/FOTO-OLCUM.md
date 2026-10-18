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
