# Hangi parça satıldı? — tanımlama stratejisi

> **Durum:** karar notu · 03.09.2026 · iş 2 · dayanak **K-001**
> **Kaynak (Cemil):** *"Yedek parçacı kolayca bu işi çözüyor. Fakat ben hangi parçanın satıldığını tespit etmek için araya bir işlem sokmaya çalışıyorum. Ve bazı yedek parçacılar oem kodu vermiyor. Sadece fiyat veriyor. Hangi parça olduğunu tespit edebilirsek, bu veri bize yarayacaktır."*

---

## 1. İki ayrı ihtiyaç — ve neden karıştırılmamalı

Bu cümlede iki şey iç içe duruyor. Ayırınca maliyet tablosu tersine dönüyor.

| | **Uyumu bulmak** | **Ne satıldığını bilmek** |
|---|---|---|
| Kim için | **Tamirci** — işi yapabilsin | **Biz** — veri biriksin |
| Sorusu | *"Bu araca hangi parça uyar?"* | *"Sonuçta hangi parça takıldı?"* |
| Ne zaman | Fiyat sorulmadan **ÖNCE** | Cevap geldikten **SONRA** |
| Yanılırsa | **İş durur, yanlış parça takılır, araç ikinci kez lifte kalkar** | Bir satır eksik kalır |
| Eksik olursa | **Kabul edilemez** | Kabul edilebilir — zamanla dolar |
| Maliyeti | Eksiksiz lisanslı veri (TecDoc) | **Sıfırdan başlar, kullandıkça birikir** |

**Sol sütun bizim işimiz değil (K-001).** Parçacı bunu günde yüz kere yapıyor, kataloğu var, ruhsatı görüyor.

**Cemil'in istediği sağ sütun.** Ve sağ sütun **akışı hiç durdurmaz** — bu, bütün stratejinin dayandığı nokta.

> **Kural: tanımlama hiçbir zaman bir adımı bekletmez.** Fiyat geldiyse iş yürür. Tanımlama arkada olur, olmazsa olmaz.

---

## 2. Nerede oluyor

```
Biz gönderiyoruz          Parçacı dönüyor           Biz ayrıştırıyoruz
─────────────────         ───────────────           ──────────────────
ruhsat fotoğrafı     →    serbest metin        →    kod? marka? fiyat?
"triger seti"             "Gates K015607XS               ↓
                           2450 TL"              kayda yaz + katalogda ara
```

Parçacının cevabı **serbest metindir** — biçim dayatamayız, WhatsApp'tan yazıyor. Gerçekte gelen üç tip:

| Gelen | Ne var |
|---|---|
| `Gates K015607XS 2450` | ad + **marka** + **kod** + fiyat |
| `triger seti 2450, Gates` | ad + marka + fiyat |
| `2450` | yalnız **fiyat** |

---

## 3. Üç kademe — hiçbiri tıkamıyor

### Kademe 1 · Kod geldi → doğrudan kaydet
Satır kesinleşir. **Ve bize bir şey öğretir:**

```
(araç tipi: Mégane 2018 1.5 dCi) + (istenen: triger seti) → Gates K015607XS
```

Bu üçlü kendi tablomuza yazılır. **Bir dahakine aynı araca aynı parça sorulduğunda elimizde bir aday var.**

### Kademe 2 · Kod yok, marka var → kendi birikimimizden dene
`triger seti + Gates + Mégane 2018` ile geçmişte eşleşen kod var mı? Varsa **ustaya sorulur, dayatılmaz:**

> Gates triger seti — daha önce bu araca **K015607XS** takılmıştı. Aynısı mı?
> **[Evet] [Farklı] [Bilmiyorum]**

*Bilmiyorum* da geçerli bir cevaptır ve akış devam eder.

### Kademe 3 · Sadece fiyat geldi → **tanımlanamadı**
Satır `triger seti · 2.450 ₺ · tanımlanamadı` olarak kalır. **İş yürür.** Rozet ne suçlama ne uyarıdır — sadece o satırın veri değeri düşük demektir.

---

## 4. Kod veren parçacılar bize katalog öğretiyor

Cemil'in *"bazı parçacılar kod vermiyor"* dediği şey aslında **kaybedilen bir fırsat değil, kazanılan bir kaldıraç.**

Kod veren azınlık, kod vermeyen çoğunluk için de **cevabı üretiyor** — çünkü sorulan parçalar aynı parçalar.

**Neden hızlı doyar:** Bir tamirhanede sorulan parçalar **uzun kuyruk değildir.** Balata, disk, triger seti, devirdaim, filtre takımı, akü, amortisör, rotil… Aynı yirmi-otuz kalem, aynı on-onbeş araç modeli için tekrar tekrar sorulur. **Kapsama, katalog büyüklüğüyle değil, tekrar sıklığıyla dolar.**

Ve biriken veri **iki yöne birden** çalışır:

- **Bize:** hangi parça ne sıklıkta gidiyor, hangi parçacı hangi kalemde ucuz, fiyat zamanla nasıl değişti
- **Tamirciye:** *"bu araca geçen sefer Gates takılmıştı"* — bu operasyonel olarak da değerli, sadece bizim istatistiğimiz değil

İkincisi önemli: **tamirciye bir faydası olmayan veri toplamayız.**

---

## 5. Maliyet karşılaştırması

| | **TecDoc lisansı** | **Kendi birikimimiz** |
|---|---|---|
| Başlangıç maliyeti | **Teklif bazlı — fiyat yayınlanmıyor** (v1 araştırması: `A-11-3-TECDOC-VERI.md`) | **0** |
| Ne zaman kullanılabilir | Sözleşme + entegrasyondan sonra | **İlk günden** |
| Kapsama | Gün 1'de eksiksiz | Yavaş dolar, hep eksik kalır |
| Doğruluk | Üreticiden | Parçacının söylediği |
| Bakım | Lisans yenileme, veri güncelleme | Kendiliğinden |
| Yanlışsa | Bizim sorunumuz | Parçacının verdiği bilgi — kayıtta öyle durur |

**Karar (K-001):** Kendi birikimimizle başlanır. TecDoc **kapı dışı değil, ana yolda değil** — ihtiyaç doğarsa devralınan araştırma (`A-11-URUN-YAPISI.md`, `A-11-3-TECDOC-VERI.md`) hazır duruyor.

**Geçişi kolay tutmak için tek şart:** kendi tablomuzun adlandırması TecDoc kavramlarıyla **uyumlu** kalsın (araç tipi / jenerik parça / somut ürün ayrımı — devralınan dört katman yapısı). Böylece bir gün lisans alınırsa biriken veri atılmaz, üstüne oturur.

---

## 6. Tutulacak en az veri

Yedi işten **"parçacıya fiyat sor"** işine hizmet ediyor. Fazlası yok.

| Alan | Neden |
|---|---|
| `istenenAd` | Ustanın yazdığı — *"triger seti"* |
| `aracTipi` | Ruhsattan gelen — eşleşmenin diğer yarısı |
| `parcaci` | Kim cevapladı |
| `gelenMetin` | **Ham cevap.** Ayrıştırma yanlışsa geri dönülecek tek yer |
| `marka` · `kod` | Ayrıştırılabildiyse |
| `fiyat` | Her zaman var |
| `tanimDurumu` | `kodlu` \| `eslestirildi` \| `tanimlanamadi` |

**Ham metin saklanır** — bu, v1'de öğrenilen bir ders: türetilmiş veriyi saklayıp kaynağı atmak, ayrıştırma hatasını kalıcı hâle getiriyor.

---

## 7. AÇIK

| Kod | Karar |
|---|---|
| **A-P1** | Ayrıştırma nerede — istemcide basit desen mi, sunucuda AI mı? Öneri: **sunucuda**, çünkü desen zamanla iyileşecek ve istemci güncellemesi beklemeden düzelmeli |
| **A-P2** | Kademe 2'deki *"bu mu?"* sorusu **ne zaman** sorulsun — fiyat gelince mi, parça takılırken mi, hiç sorulmasın mı (sessizce eşleştirilip rozetle mi gösterilsin)? |
| **A-P3** | Aynı parça iki parçacıdan **iki farklı kodla** dönerse hangisi doğru? Öneri: ikisi de saklanır, seçilen kaydedilir |
| **A-P4** | Biriken veri **dükkâna mı ait, ortak mı?** Bir dükkânın öğrendiği eşleşme diğerine yarar — ama kendi tedarikçi fiyatı özeldir. Öneri: **eşleşme ortak, fiyat özel.** (Devralınan katalog hibrit kararıyla uyumlu.) |
| **A-P5** | *"Tanımlanamadı"* satırlar birikirse ne yapılır — dönemsel olarak ustaya toplu sorulur mu, yoksa öylece kalır mı? |
