# KDV — işçilik ve parça aynı oranda mı?

> **Durum:** araştırma notu · 01.09.2026 · iş 24
> **Kaynak (Cemil):** *"KDV oranları işçilik ve parçada farklı olabiliyor mu? Otomix bu konuyu nasıl işlemiş? Araştır, gereken alan/ekran değişikliklerini AÇIK olarak çıkar."*

---

## 0. Kısa cevap

**Bugün ikisi de %20.** Ama bu bir *tesadüf*, kural değil — ve **asıl mesele oran değil, tevkifat.**

Üç ayrı bulgu:

1. **Oran:** Araç bakım-onarımında işçilik de parça da genel orana (%20) tabi. Oranı ayıran bir düzenleme yok.
2. **Ama kalem düzeyinde tutulmalı** — oran devlet kararıyla değişir, geçmiş fatura kendi oranıyla yeniden basılabilmelidir.
3. **Asıl fark tevkifatta:** araç bakım-onarım *hizmeti* **7/10 KDV tevkifatına** tabi; parçanın **düz satışı** değil. Aynı dükkânda aynı gün iki farklı fatura mantığı çalışır.

---

## 1. Oranlar (2026)

Türkiye'de üç oran var: **%1 · %10 · %20**. (10.07.2023'ten beri; öncesi %1/%8/%18. İnternetteki pek çok "2026 listesi" hâlâ %8 yazıyor — **yanlış**.)

| Ne | Oran | Not |
|---|---|---|
| Otomotiv yedek parça teslimi | **%20** | Genel oran; indirimli listelerde yok |
| Araç bakım / onarım işçiliği | **%20** | Genel oran |
| İkinci el araç satışı | %20 | Özel matrah — Tamiron kapsamında değil (şimdilik) |
| İhracat / ihraç kayıtlı | %0 / tecil | Parçacı ayağında çıkabilir |

**Yani bugün ustanın iş emrinde tek oran var.** İşçilik satırına %10, parça satırına %20 yazmasını gerektiren bir hâl yok.

### Peki neden kalem düzeyinde tutuyoruz?

| Sebep | Somut hâl |
|---|---|
| Oran **karar ile** değişir | 2018–2019'da otomotivde geçici indirimli oranlar uygulandı; yarın yine olabilir |
| **Geçmiş fatura** kendi oranıyla durur | 2024 faturasını 2027'de yeniden basarken o günün oranı gerekir |
| **Muaf/istisna müşteri** | İhracat, diplomatik, ihraç kayıtlı teslim — aynı iş emrinde farklı satır |
| Parçacı ayağı | Yedek parça satışı bakım-onarımdan **ayrı bir işlem**; ileride farklılaşabilir |

> **Karar önerisi:** `kdvOrani` **kalemde** tutulur, varsayılanı dükkân ayarından gelir (`AYAR.kdvVarsayilan = 20`). Kalem türüne göre ayrı varsayılan **tutulmaz** — bugün aynı, ileride ayrışırsa ayar ikiye bölünür.

---

## 2. Asıl konu: TEVKİFAT

Bu, araştırmanın Cemil'in sormadığı ama **sorması gereken** kısmı.

**KDV Genel Uygulama Tebliği (I/C-2.1.3.3)** — makine, teçhizat, demirbaş ve **taşıtlara ait bakım ve onarım hizmetleri** kısmi tevkifata tabi: **7/10**.

Yani 10.000 ₺'lik işte 2.000 ₺ KDV'nin **1.400 ₺'sini müşteri doğrudan vergi dairesine** yatırır, dükkâna yalnız **600 ₺** öder. Dükkânın kasasına giren para farklıdır — **tahsilat ekranı bunu bilmek zorunda.**

### Ne zaman uygulanır — üç şart birlikte

| Şart | Detay |
|---|---|
| **Alıcı kim** | KDV mükellefi ya da "belirlenmiş alıcı" (kamu, KİT, banka, büyük şirket…). **Bireysel müşteride tevkifat YOK.** |
| **Tutar** | KDV **dahil** bedel **12.000 ₺**'yi aşıyorsa (2026; 2025'te 9.900 ₺). Fatura düzenleme haddine endeksli, **her yıl değişir.** |
| **İş türü** | Bakım-onarım **hizmeti**. Parçanın düz satışı (tamir olmadan) tevkifata girmez. |

### Üç kritik ayrıntı — hepsi ekrana dokunuyor

**a) Parça, hizmete dâhildir.**
> *"Onarım hizmetlerinde onarılan şeye ait bazı parçaların değiştirilmesi işin tabii bir sonucudur"* — malzeme, yedek parça ve sarf malzemesi **bakım-onarım hizmetine dâhildir.**

Yani tevkifat matrahı **parça + işçilik toplamı**. Parçayı ayrı satırda göstermek onu tevkifat dışına çıkarmaz.

**b) Bedel parçalara bölünemez.**
Tevkifattan kaçmak için tek işi iki faturaya bölmek yasak; denetimde *"bütünlük arz ettiği anlaşılan alımların toplamına"* bakılır.

> **Bu bir ürün kuralıdır:** Tamiron, aynı servis kaydını iki faturaya bölmeyi **kolaylaştırmamalı**; bölünürse **uyarmalı**.

**c) Müşterinin getirdiği parçada tevkifat yok.**
Müşteri parçayı kendi getirmişse o parça hizmet bedeline girmez. Tamiron zaten `kaynak` alanında parçanın nereden geldiğini tutuyor — **"müşteri getirdi" değeri eklenmeli** (bugün yok).

---

## 3. Otomix (ve benzerleri) bunu nasıl işlemiş?

**OtoMixSQL** (otoservisprogram.com) ve **OtoServisim** kılavuzlarından çıkan tablo:

| Konu | Onların yaptığı | Tamiron kararı |
|---|---|---|
| KDV nerede | **Kalem bazında** oran girilebiliyor (miktar · birim fiyat · KDV oranı) | Aynı — kalemde |
| Varsayılan | Firma ayarından tek "Varsayılan KDV Oranı" | Aynı — `AYAR.kdvVarsayilan` |
| İşçilik / parça ayrımı | Kalem **türü** var (Hizmet / Malzeme) ama **KDV'yi ayırmıyor** | Aynı — bizde de tip var, oranı ayırmıyoruz |
| KDV dâhil / hariç | "KDV Dahil" anahtarı; açıkken **içsel KDV** hesaplanıyor | **Bize lazım** — usta "beş bin" derken KDV dâhil düşünür |
| İskonto sırası | `İskonto sonrası tutar × KDV / (100 + KDV)` | Aynı sıra — **iskonto önce, KDV sonra** |
| Donmuş alanlar | Kaydedilince `iskontoOrani`, `iskontoTutari`, `kdvOrani`, `kdvTutari` **sabitlenir** | Aynı — geçmiş kayıt sonradan değişmemeli |
| Tevkifat | Kılavuzlarda **görünmüyor** | **Bizim farkımız burada olacak** |

**Çıkarım:** Otomix'in yaptığı doğru ama eksik. Kalem bazlı KDV + firma varsayılanı + içsel KDV + iskonto sırası — bunlar standart ve biz de aynısını yapmalıyız. **Tevkifatı ve "KDV dâhil düşünen usta"yı ciddiye alan bir ekran ise piyasada yok.**

---

## 4. Ustanın gerçeği: KDV dâhil konuşur

Bu, tasarımın kalbi. Usta müşteriye **"balata beş bin"** der. Bu rakam:
- KDV **dâhildir** (müşterinin cebinden çıkacak para),
- ama sisteme **matrah** olarak girmesi gerekir.

5.000 ₺ KDV dâhil ise matrah 4.166,67 · KDV 833,33.
5.000 ₺ hariç girilirse müşteri 6.000 ₺ öder — **usta bin lira fazla ister, müşteri kızar.**

> **Kural:** Servis kaydı kalem tutarları **varsayılan olarak KDV DÂHİL** girilir. Ekranda tek anahtar: *"Girilen tutarlar KDV dâhil"* (varsayılan **açık**). Toplam şeridi her iki rakamı da gösterir.

Bu, 14. işteki iki fiyat (`tutar` satış / `alis`) kararıyla uyumlu: **`alis` KDV HARİÇ tutulur** (dükkân onu matrah olarak öder ve indirir), `tutar` KDV dâhil girilir. Bu, **A-F7 açık maddesinin cevabıdır.**

---

## 5. Hesap sırası (tek doğru sıra)

```
1. Kalem tutarları           (KDV dâhil girildiyse önce matraha çevrilir)
2. − İskonto                 (kalem ve/veya kayıt düzeyi — iş 11)
3. − Düze yuvarlama          (iş 11; iskontonun bir türü sayılır)
   ────────────────────────
4. = MATRAH
5. + KDV (kalem oranıyla)    → BRÜT TOPLAM
6. − Tevkifat (7/10)         → MÜŞTERİNİN DÜKKÂNA ÖDEYECEĞİ
```

**5. adım kalem kalem yapılır** (farklı oranlar olabilir), **6. adım kayıt düzeyinde** — tevkifat bütün işe bakar.

Kritik: **iskonto KDV'den önce.** Ters sırada yapılırsa KDV fazla hesaplanır ve fatura tutmaz.

---

## 6. Gereken alan değişiklikleri

### Kalem (`KALEMLER[]`)
| Alan | Var mı | Not |
|---|---|---|
| `kdvOrani` | **YOK — eklenecek** | Varsayılan `AYAR.kdvVarsayilan`; kalemde değiştirilebilir |
| `kdvDahil` | **YOK — eklenecek** | Kayıt düzeyi bayrak; kalemde değil |
| `kaynak: 'musteri'` | **YOK — eklenecek** | Müşterinin getirdiği parça — tevkifat matrahına girmez |
| `tutar` / `alis` | var (iş 14) | `tutar` KDV dâhil, `alis` KDV hariç |

### Servis kaydı
| Alan | Not |
|---|---|
| `kdvDahilGiris` | Varsayılan `true` |
| `tevkifatVar` | Türetilir: müşteri tipi + tutar + iş türü |
| `tevkifatOrani` | `7/10` (sabit değil — tebliğ değişir) |

### Müşteri / araç sahibi
| Alan | Not |
|---|---|
| `musteriTipi` | `bireysel` \| `mukellef` \| `belirlenmisAlici` — **tevkifat bu alan olmadan hesaplanamaz** |
| `vergiNo` / `tckn` | Mükellefte zorunlu |

### Dükkân ayarları
| Alan | Not |
|---|---|
| `kdvVarsayilan` | 20 |
| `tevkifatSiniri` | 12.000 (2026) — **yıllık güncellenir, sabit yazılmaz** |
| `kdvDahilVarsayilan` | true |

---

## 7. Gereken ekran değişiklikleri

| # | Ekran | Değişiklik |
|---|---|---|
| E-1 | **Servis kaydı — toplam şeridi** | Matrah · KDV · **Genel toplam** ayrı satır. Bugün tek rakam var. |
| E-2 | **Servis kaydı — kalem satırı** | Kalemde KDV oranı rozeti (`%20`); dokununca değişir. Hepsi aynıysa **gösterme** (gürültü). |
| E-3 | **Servis kaydı — üst** | Tek anahtar: *"Tutarlar KDV dâhil"*. Kapatılınca bütün rakamlar yeniden hesaplanır. |
| E-4 | **Onay / teklif ekranı** | Müşteriye giden belgede **KDV dâhil tek rakam** öne çıkar; kırılım altta küçük. |
| E-5 | **Ödeme ve teslim** | Tevkifat varsa: *"Bu iş tevkifatlı — müşteri size **600 ₺**, vergi dairesine **1.400 ₺** ödeyecek."* **Tahsilat beklentisi bu rakam olmalı**, brüt değil. |
| E-6 | **Müşteri / araç kartı** | Müşteri tipi seçimi (bireysel / mükellef / belirlenmiş alıcı) + vergi no. |
| E-7 | **Ayarlar** | KDV varsayılanı, tevkifat sınırı, "tutarlar KDV dâhil" varsayılanı. |
| E-8 | **Fatura bölme uyarısı** | Aynı servis kaydı ikiye bölünmek istenirse: *"Tek işin bedeli bölünemez (Tebliğ I/C-2.1.3.3)."* |
| E-9 | **Muhasebe** | KDV özeti: hesaplanan · indirilecek · **tevkif edilen** ayrı. |

---

## 8. AÇIK

| Kod | Karar |
|---|---|
| **A-K1** | Tevkifat **prototipe girecek mi**, yoksa backend fazına mı? Öneri: **alan ve ekran yeri şimdi açılsın**, hesap backend'de. Sonradan eklemek toplam şeridini baştan yazdırır. |
| **A-K2** | Müşteri tipi **araca mı kişiye mi** bağlı? Aynı araç bireysel de gelir, şirket adına da. Öneri: **kişiye**, serviste değiştirilebilir. |
| **A-K3** | "Müşteri parçayı kendi getirdi" hâli fiyatlandırmada nasıl görünecek? (Kalem var, tutarı 0, ama işçilik var.) |
| **A-K4** | Tevkifat sınırı yıllık değişiyor — **ayarda mı, sunucudan mı**? Öneri: sunucudan, ayarda gösterilir. |
| **A-K5** | Parçacı ayağında (YedekParca aktörü) düz parça satışı tevkifatsız; aynı ekran iki modda mı çalışacak? |
| **A-K6** | e-Arşiv / e-Fatura entegrasyonu hangi fazda? Tevkifatlı fatura **e-fatura zorunluluğu** getirebilir. |

---

## 9. Kaynaklar

- [Oto Servis Araç Bakım ve Onarım Hizmetlerinde KDV Tevkifatı — WRS İmperium YMM](https://www.wrsimperium.com/oto-servis-arac-bakim-ve-onarim-hizmetlerinde-kdv-tevkifati/) — 7/10 tevkifat, parça hizmete dâhil, bedel bölünemez, Tebliğ I/C-2.1.3.3
- [Yedek parçanın KDV oranı Hk. — Gülbenk Müşavirlik](https://www.gulbenkmusavirlik.com/tr_yedek-parcanin-kdv-orani-hk-4103.html)
- [2026 Yılı Tevkifatlı Fatura Düzenleme Sınırı 12 Bin TL Oldu](https://www.hellac.com/blogdetay/2026-yili-tevkifatli-fatura-duzenleme-siniri-12-bin-tl-oldu) — 9.900 → 12.000 ₺, 01.01.2026
- [KDV Tevkifat Oranları 2026 — Müşavirler Kulübü](https://musavirlerkulubu.com.tr/makale/kdv-tevkifat-oranlari-tablosu-ve-uygulamasi-2026-guncel-rehber)
- [OtoServisim kullanım kılavuzu](https://otoservisim.app/Home/Kilavuz) — kalem bazlı KDV, "KDV dâhil" anahtarı, içsel KDV formülü, iskonto sırası, donmuş alanlar
- [OtoMixSQL — Oto Servis Programı](https://www.otoservisprogram.com/) — kalem bazında miktar/birim fiyat/KDV oranı
- [GİB — Oran ve tarifeler](https://www.gib.gov.tr/yardim-ve-kaynaklar/yararli-bilgiler/oran-ve-tarifeler) — %1 / %10 / %20

> **Uyarı:** Oranlar ve tevkifat sınırı **Cumhurbaşkanı kararı / tebliğ ile değişir**. Bu belgedeki rakamlar 01.09.2026 tarihlidir; koda sabit yazılmaz.
