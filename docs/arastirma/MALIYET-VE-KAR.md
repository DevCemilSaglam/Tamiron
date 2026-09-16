# MALİYET VE KÂR — alış fiyatı nereden gelecek

> **Cemil, 16.09:** *"Fatura girişi olunca serviste kullanılan
> parçalara ekleyebilmeli sistem. Fakat bunu müşteri için değil kâr
> zarar muhasebe için yapmalıyız."*
> *"Faturasız çalışıyor da olabilirler. Bunu göz önünde bulundurmak
> gerek."*

---

## 0 · Önce bir yanlış anlamayı kapatalım: **fatura KESMEK ≠ fatura GİRMEK**

`K-022` muhasebe kapsamını çizerken *"fatura yok · KDV yok · e-belge
yok"* dedi. O cümle **müşteriye fatura kesmeyi** kapatıyor: e-belge
entegrasyonu, KDV hesabı, beyan.

Cemil'in istediği bunun tersi yön: **parçacıdan gelen faturayı
girmek.** Aldığımız malın ne tuttuğunu bilmek. Bu:

* e-belge altyapısı istemiyor,
* KDV hesabı istemiyor,
* kimseye belge vermiyor.

**K-022 açılmıyor.** Açık madde `A-M37` (*fatura kesmek kapsama girdi
mi*) yerinde duruyor ve bu notla karışmamalı.

---

## 1 · Bugün ne var — ve içindeki kusur

| | durum |
|---|---|
| `alis` alanı (veri modeli) | **var** — her parça satırında |
| `is-basina` ekranı (iş başına kâr) | **var** — `fiyat − alis` |
| *"Alış müşteriye gitmez"* kuralı | **yazılı** (`98-ozet.html`, VERI-MODELI §3) |
| `alis`'i **dolduran akış** | **YOK** |

Tohum veride 9 parça satırının 7'sinde `alis` dolu — o yüzden kâr
ekranı çalışıyor *görünüyor*. Gerçek kullanımda dolduran hiçbir şey
yok; hepsi `alis: 0` ile açılıyor.

### ⚠️ Kusur: parçacının fiyatı YANLIŞ ALANA yazılıyor

Bugünkü akış (`40-parca-sor.html`):

```
  usta parçacıya WhatsApp'tan sorar
        ↓
  cevap USTANIN kendi WhatsApp'ına gelir
        ↓
  usta gelen fiyatı servis kaydına ELLE yazar   ← buraya dikkat
        ↓
  yazdığı rakam `fiyat`e düşer  (= müşteriye söylenecek tutar)
  `alis` 0 kalır
```

Yani **parçacının verdiği rakam maliyettir**, ama satış alanına
yazılıyor. Sonuç:

* `is-basina` kârı **şişik** gösterir: `fiyat − 0 = fiyat`. Parçadan
  kâr edilmiş gibi görünür, oysa alışa eşit satılmıştır.
* Kâr rakamı yanlış olduğu için **güvenilmez**; usta bir kez bakar,
  tutmadığını görür, bir daha bakmaz.

Bu kusur bugüne kadar görünmedi çünkü **tohum veride `alis` elle
doldurulmuştu.** Ölçüm aletinin kendi verisi, ölçtüğü eksiği gizledi.

---

## 2 · Alış üç yerden gelebilir — fatura yalnız biri

> *"Faturasız çalışıyor da olabilirler."*

Bu cümle tasarımın merkezine konmalı: **faturasızlık istisna değil,
varsayılan.** Fatura gelirse iyi; gelmezse akış aynen çalışmalı.

| kaynak | ne zaman | ustaya maliyeti | güven |
|---|---|---|---|
| **① Parçacının cevabı** | Parçayı sorarken — *zaten oluyor* | **bir rakam** | yüksek |
| **② Elle giriş** | Nakit aldıysa, kâğıt yoksa | bir rakam | yüksek |
| **③ Fatura girişi** | Fatura geldiğinde, **toplu** | bir belge, çok satır | en yüksek |

**Önerim: ① birincil, ③ düzeltici.**

Sebep: usta parçacının fiyatını **zaten görüyor** (kendi WhatsApp'ına
geliyor) ve **zaten bir rakam yazıyor**. Bugün o rakam yanlış alana
gidiyor. Doğru yere gitmesi için ekranda tek bir şey değişir:

```
  ŞİMDİ   [ Fiyat: ____ ]                → alis boş kalır, kâr şişer

  ÖNERİ   [ Parçacı ne dedi: ____ ]      → alis
          [ Müşteriye kaç yazıyorsun: ____ ]  → fiyat
```

İkinci rakam boş bırakılabilir *(o zaman alışına satmıştır — ve bu
DOĞRU bilgidir, sıfır kâr)*. Fatura sonradan gelince ① düzeltilir,
müşteriye söylenmiş rakam **değişmez.**

> ⚠️ **Bu bir ekran değişikliği ve karar Cemil'in.** İki rakam, T-18'in
> (*kullanıcı teknolojiden uzak*) yazma bütçesini artırır. Ama kârı
> yanlış göstermek, kâr ekranını **hiç koymamaktan kötüdür**.

---

## 3 · "Müşteri için değil" — bunun bir sınırı var ve sınanmalı

Cemil'in cümlesi bir **değişmez** tanımlıyor:

> Alış fiyatı, müşterinin gördüğü hiçbir yüzeyde görünmez.

Müşteri yüzeyleri: teslim özeti · müşteriye gönderilen özet/link ·
mesaj metni · *"Müşteri ne görecek"* katmanı.

Bu kural bugün **yorumda yazılı, testte değil**. Yazılı bir kural,
uygulanmıyorsa öğrenilmemiştir (Tur 4'ün dersi). Sağlık denetimine
girdi: 48. test müşteri yüzeylerinde alış rakamını arıyor.

---

## 4 · Fatura girişi ne yapar, ne yapmaz

**Yapar:** bir belgedeki satırları, açık servis kayıtlarındaki
parçalarla eşleştirip `alis` alanlarını **toplu** doldurur.

**Yapmaz:**
* müşteriye söylenmiş `fiyat`'a dokunmaz,
* KDV hesaplamaz, beyan üretmez (K-022),
* eşleşmeyen satırı zorla bir kayda yazmaz — eşleşmeyen satır
  **gider** olarak durur (B seçeneğinin gider listesi).

### Eşleştirme kolay olmayacak

Fatura satırı *"TRIGER SET GATES K015603XS"*, kayıttaki satır
*"Triger seti"*. Bu, `PARCA-TANIMLAMA.md`'deki eşleştirme probleminin
aynısı. **Otomatik eşleştirme öneri olmalı, karar olmamalı**: usta
onaylar. Yanlış eşleşen bir maliyet, yanlış kârdan beterdir — çünkü
iki kaydı birden bozar.

---

## 5 · İzle kesişme (K-053)

Fatura girilince ortaya çıkabilecek yeni bir **mantığa aykırı işlem**:

| kod | ne oldu | neden şüpheli |
|---|---|---|
| `zararina-satis` | Fatura maliyeti, müşteriye söylenen fiyattan yüksek | Ya fiyat düşük yazıldı ya yanlış parça eşleşti |

`IZ-KAYDI.md §4.1`'e eklendi.

---

## 6 · Cemil'in kararına açık

**A-M43 — Alış rakamı nerede girilecek?** Önerim: parça fiyatı sorma
akışında, *"parçacı ne dedi"* + *"müşteriye kaç yazıyorsun"* olarak
iki alan. Bugünkü tek alan kârı şişiriyor.

**A-M44 — Fatura girişi bu faza girsin mi?** Önerim: **hayır, sonra.**
Önce A-M43 çözülsün — alış rakamı hiç girilmiyorken fatura eşleştirmesi
yazmak, olmayan bir alanı doldurmak olur. Fatura, ① kaynağının
düzelticisi olarak ikinci turda gelir.

**A-M45 — Eşleşmeyen fatura satırı ne olur?** Önerim: gider listesine
düşer (MUH seçeneği B ile aynı yer), kayda zorlanmaz.
