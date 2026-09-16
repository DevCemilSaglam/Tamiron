# SAVUNMA — taklit edilemezlik, dürüst hâliyle

> **Cemil, 15–16.09:** *"Kopyalanamaz bir özellik eklemek."*
> *"Taklit edilemez olması için ne yapılabilir? Bu alanda hızlı
> ilerlemeli ve alanı zapt etmeli ve savunmalıyız. Nasıl, neler
> yapılabilir?"*

---

## 0 · Önce kötü haber: **kopyalanamaz özellik diye bir şey yok**

Soruyu olduğu gibi cevaplarsam yanlış cevap veririm. Bu ürünün
yapabileceği hiçbir **özellik** taklit edilemez değil:

| özellik | rakip ne kadarda kopyalar |
|---|---|
| Sesle işlem söylemek | API çağrısı — **günler** |
| Ruhsatı fotoğraftan okumak | API çağrısı — **günler** |
| Parça listesini okumak | API çağrısı — **günler** |
| WhatsApp'tan parçacıya sormak | bağlantı işi — **haftalar** |
| Ekranlar, akış, tasarım | bakarak — **haftalar** |

**Yapay zeka özellikleri en kolay kopyalananlardır**, çünkü zekâ bizde
değil API'de. Herkes aynı modeli çağırabiliyor. Bir özelliğe savunma
yüklemek, kumu duvar sanmaktır.

Dahası: bu deponun kurucu dersi *"çok fazla özellik var, karmaşık
geldi"*. **Özellik ekleyerek savunma kurmak, ürünü öldüren şeyi
tekrarlamaktır.** Savunma genişlikten değil, **derinlikten** gelmeli.

---

## 1 · Gerçekten kopyalanamayan dört şey

Kopyalanamayan şeyin ortak özelliği şu: **zamanla birikir ve satın
alınamaz.** Rakip para verip alamaz, ancak aynı yılları geçirerek elde
eder.

### ① Düzeltme birikimi — *bugün elimizde olan tek gerçek varlık*

Model ruhsatı okur, **usta düzeltir**. O düzeltme:

```
   model dedi : 34 ABC 123      ← API'nin ürettiği, herkeste var
   usta yazdı : 34 QBC 123      ← YALNIZ BİZDE
```

Okumayı API veriyor; **düzeltmeyi dükkân veriyor.** Bin dükkân bir yıl
kullanınca elimizde Türkiye'nin ruhsatlarından, el yazısı parça
listelerinden, dükkân gürültüsünde söylenmiş cümlelerinden oluşan bir
düzeltme yığını olur. Bugün başlayan bir rakibin elinde **sıfır** var
ve parayla alamaz.

**Bugün ne yapıldı:** düzeltme artık kendiliğinden yazılıyor
(`Iz.okuma` + `duzeltme`, K-053). Bugüne kadar usta yanlışı düzeltiyor,
düzeltme ekranda kalıyor, **biz asla öğrenmiyorduk** — birikim hiç
başlamamıştı. 47. denetim testi bunu koruyor.

> Elimizdeki 46 ruhsat + 27 parça listesi ölçütü bu birikimin
> **ilk gününün** kaydı. Değerini bugün değil, üçüncü yılında gösterir.

### ② Parça sözlüğü — usta dilinden kataloğa

*"Triger seti"* ↔ *"TRIGER SET GATES K015603XS"*. Bu eşleşme
TecDoc'ta hazır durmuyor; **dükkânın söyleyişiyle** kataloğun yazışı
arasındaki köprü. Her onaylanan eşleşme sözlüğü büyütüyor
(`PARCA-TANIMLAMA.md`).

Türkiye'ye ve tamirci ağzına özel, yıllarla birikiyor, bize maliyeti
sıfır (usta zaten onaylıyor). **Rakip bunu ancak aynı yılları geçirerek
kurar.**

### ③ Parçacı ağı — *en güçlü olanı, ama bugün kurulmuyor*

Yedi işten **beşincisi**: parçacıya fiyat sormak. Burada gerçek bir
**çift taraflı ağ** var:

```
   daha çok dükkân  →  daha çok parçacı  →  daha hızlı ve ucuz fiyat
          ↑                                            │
          └────────────────────────────────────────────┘
```

Bu tek başına ötekilerin hepsinden güçlü: rakip **iki tarafı birden**
kurmak zorunda kalır.

> ⚠️ **Ama bugünkü akış bu ağı kurmuyor — bilerek.** `40-parca-sor`
> ekranı soruyu WhatsApp'tan gönderiyor ve **cevap ustanın kendi
> WhatsApp'ına** geliyor. Sade, kurulum istemiyor, tamircinin
> alışkanlığını bozmuyor — hepsi doğru gerekçeler. Bedeli: **fiyat
> akışı bizden geçmiyor**, dolayısıyla ağ birikmiyor.
>
> Bu bir çelişki değil, bir **seçim**: bugün sadelik, yarın ağ. Ama
> seçimin farkında olunmalı. **A-M46.**

### ④ Destek ilişkisi — Cemil'in kendi notu

> *"Kullanıcı problem yaşadığında arayacağı kişi olacağım."*

Teknolojiden uzak bir kullanıcı (T-18) için **arayıp cevap alabildiği
kişi** bir özellikten daha bağlayıcıdır. Bu kopyalanamaz çünkü
kopyalanacak bir şey yok ortada — ilişki var.

İz kaydı (K-052/K-053) bunun aletidir: telefonda *"şurada plakayı
yanlış yazmışsın"* diyebilmek, rakibin *"bir ekran görüntüsü atar
mısınız"* demesinden başka bir üründür.

---

## 2 · Savunma OLMAYAN şeyler

Bunları savunma sanmak, en pahalı hatadır — güvenip yavaşlamaya yol
açar:

| | neden değil |
|---|---|
| **İlk olmak** | Yalnızca zaman kazandırır. Zamanı birikime çevirmezsen hiçbir şeydir |
| **Daha çok özellik** | Bu ürünü bir kez öldürdü (v1) |
| **Daha ucuz olmak** | En zayıf savunma; rakip bir gecede eşitler |
| **Kaynak kodun gizliliği** | Kopyalanan şey kod değil, fikir |
| **Patent** | Yazılım iş yöntemi; Türkiye'de zayıf, pahalı, yavaş |
| **Modelin kendisi** | Bizim değil |

---

## 3 · "Alanı zapt etmek" — hız neyi satın alır

Hız tek başına savunma değil; **birikimin başlangıç tarihini öne
çeker.** Bu yüzden hızın yöneleceği yer bellidir:

> **Kural:** Bir iş, ① ② ③ ④'ten birini büyütmüyorsa onu hızlı yapmanın
> stratejik değeri yoktur. Yalnız ürünü iyileştirir — ki o da gerekli,
> ama savunma değildir.

Bu kural, *"yeni özellik varsayılan HAYIR"* kuralıyla **çelişmiyor**;
onu güçlendiriyor. İkisi birlikte şunu söylüyor: az iş yap, ama
yaptığın iş biriksin.

### Sıradaki üç iş bu ölçüte göre

| iş | hangi birikimi büyütür |
|---|---|
| **A-M39** — 40 ses kaydı | ① — ses tarafının ilk günü |
| **A-M34** — ölçütün doğrulanması | ① — düzeltmelerin ilk yığını |
| **A-M46** — parçacı ağı kararı | ③ — en güçlüsü, henüz açılmadı |

---

## 4 · Sonuç — bir cümle

**Kopyalanamaz özellik aramak yerine, kopyalanamaz BİRİKİM kurmalıyız.**
Ürün rakibe açık kitap gibi görünsün; kitabı okuyup aynısını yazan,
bizim üç yıllık düzeltme yığınımızı, parça sözlüğümüzü ve parçacı
ağımızı yazamaz.

---

## 5 · Cemil'in kararına açık

**A-M46 — Parçacı ağı.** Fiyat cevabı bugün ustanın kendi WhatsApp'ına
geliyor; akış bizden geçmiyor ve ağ birikmiyor. Cevabın da üründen
geçmesi, en güçlü savunmayı açar ama sadeliği bozar ve parçacı tarafına
bir kurulum yükler. **Ne zaman ve hangi biçimde?**
Önerim: **bu fazda değil**, ama kararı bilinçli verelim — kazara
ertelenmiş olmasın.

**A-M47 — Düzeltmeler dükkândan çıkacak mı?** ① ancak düzeltmeler bize
ulaşırsa birikir. A-M40'ın (izin gönderilmesi) içinde çözülebilir ama
farklı bir sorudur: iz *destek* için, düzeltme *birikim* için.
Önerim: düzeltmeler **ayrı ve baştan** gelsin; hacmi küçük, değeri
büyük.
