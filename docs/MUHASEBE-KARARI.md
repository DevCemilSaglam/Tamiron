# MUHASEBE KARARI — Cemil'in cevaplaması için tek sayfa

**Tarih:** 14.09.2026 · **Bekleyen madde:** `MUH` (KARARLAR.md AÇIK tablosu)
**Nereden geliyor:** ölçümler yeniden yapılmadı; şu üç belge zaten ölçtü —
[`arastirma/MUHASEBE-KAPSAMI.md`](arastirma/MUHASEBE-KAPSAMI.md) ·
[`arastirma/PARASUT-HARITA.md`](arastirma/PARASUT-HARITA.md) ·
[`arastirma/LUCANET-ONE-HARITA.md`](arastirma/LUCANET-ONE-HARITA.md).
Bekleyen üç işin buna bağlı olduğu K-045.5'te yazıldı.

> **Bu belge karar vermez.** Üç seçeneği ustanın gözünden anlatır, bedelini
> yazar ve onaylandığında ne olacağını söyler. Seçim senin.

---

## 1 · Soru

**Tamiron'un para tarafı nereye kadar gitsin?**

Soru *"kaç özellik"* değil. Ölçü tek ve K-013'ten geliyor:

> **Usta o rakamı görmek için kaç yeni şey girmek zorunda — ve girmezse
> rakam yalan söylüyor mu?**

Girilmeyen gider, ekranda **eksilmiş** bir kâr olarak görünmez; **olmayan**
bir gider olarak görünür. Usta ona inanır. *Yanlış rakam gösteren muhasebe,
muhasebe olmamasından kötüdür.*

---

## 2 · Üç seçenek — usta ekranda ne görür, ne girer

### A · Yalnız kasa

**Usta ne görür:** Muhasebe'yi açar, üstte **Bugün · Eylül · 2026** düğmeleri.
Bugün tahsil ettiği toplam, ödemenin nasıl alındığı (nakit · kart · havale) ve
**Alacak** listesi — *kimden* alacağı, adıyla ve plakasıyla.

**Usta ne girer:** **Hiçbir şey.** Bu rakamlar aracı teslim ederken zaten
seçtiği ödeme yolundan ve kayda yazdığı tutarlardan çıkıyor.

**Yalan söyleyebilir mi:** Hayır. Girilmemiş bir veri yok.

**Karşılığı:** *"Bu ay dükkân ne kazandı"* sorusu **cevapsız kalır.** Elektrik,
kira, maaş ekranda hiç yoktur.

---

### B · Kasa + gider ← *bugün prototipte yazılı olan*

**Usta ne görür:** A'daki her şey, **artı** Giderler tablosu: tarih, açıklama,
tutar. Bir gideri silmek için satırı sola kaydırır (silinir, geri alma şeridi
çıkar).

**Usta ne girer:** Gideri **kendisi ekler** — iki yolu var ve ikisi de aynı
yuvarlak düğme biçiminde duruyor:
* mikrofona basıp *"iki bin dört yüz lira elektrik faturası ekle"* der, ya da
* **+** düğmesine basıp iki alan doldurur: açıklama ve tutar.

Parça alışı gider tablosuna **yazılmaz** — o para zaten servis kaydında duruyor
(usta parçaya ne ödedi) ve iş başına farkta düşülüyor. Boş gider listesi bunu
bir kez söylüyor; her açılışta tekrar etmiyor. Muhasebe ekranında *"toplam parça
alışı"* diye bir rakam **yok** — onu 05.09'da sen kaldırttın.

**Yalan söyleyebilir mi:** **Evet — ama sessizce değil.** Girilmeyen gider
listede görünmez; bu yüzden ekranda *"kalan"* ya da *"kâr"* diye bir rakam
**yok.** Kâr sorusu yalnız **iş başına** cevaplanıyor (müşteriden alınan − o iş
için parçaya ödenen), çünkü o iki rakam da zaten kayıtlı.

**Karşılığı:** ustadan günde bir-iki giriş. Girmezse gider listesi boş kalır,
başka hiçbir rakam bozulmaz.

---

### C · Cari hesap (müşteri/parçacı bakiyesi)

**Usta ne görür:** Her müşterinin ve her parçacının **yürüyen bakiyesi**;
ekstre, mutabakat, kısmi ödeme, iade. Paraşüt'te bu bir ekran ailesi, LucaNet'te
ayrı iki menü + beş rapor.

**Usta ne girer:** Açılış bakiyesi · her ödeme · her kısmi ödeme · her iade.
Yani **her para hareketi** ayrıca işlenmek zorunda.

**Yalan söyleyebilir mi:** Evet ve en kötüsü: bakiye bir kez kayarsa **bir daha
tutmaz**; usta hangi rakamın yanlış olduğunu bulamaz.

**Karşılığı:** Tamiron'da müşterinin kendi tablosu bile yok (K-012) — müşteri,
aracın üstünde iki alan. Cari hesap açmak, v1'in üç tablolu müşteri dünyasını
geri getirmek demek; tamircinin *"çok fazla özellik var, karmaşık geldi"* dediği
yığının tam ortası.

---

## 3 · Yan yana

| | **A · Yalnız kasa** | **B · Kasa + gider** | **C · Cari hesap** |
|---|---|---|---|
| Ustadan yeni giriş | yok | gider başına bir giriş | her para hareketi |
| Girilmezse ne olur | — | gider listesi eksik kalır | **bakiye bozulur** |
| Yedi işten hangisi | **4. iş** *(teslimde ödemeyi bilmek)* | 4'ün komşusu | **hiçbiri** |
| Bugün prototipte | var | **var** | yok |
| Tamircinin istediği mi | evet (S8) | söylemedi | söylemedi |

---

## 4 · Öneri: **B**

**Neden A değil:** gider girişi ustayı zorlamıyor — söylemek form doldurmak
değil, ve mikrofonu olmayan tarayıcıda elle giriş de var. Usta girdiği kadarını
görüyor, ekran fazlasını **uydurmuyor**.

**Neden C değil:** yedi işin hiçbirine hizmet etmiyor, K-012'yi geri alıyor ve
bir kez kayan bakiye bir daha tutmuyor. *"İleride lazım olur"* bu depoda gerekçe
değil.

**B'nin sınırı keskin — onay bu sınırı da onaylar:** fatura yok · KDV yok ·
tevkifat yok · e-belge yok · banka yok · çek/senet yok · stok yok. Hepsi
rakiplerde var, hiçbiri yedi işten birine hizmet etmiyor. *(Muhasebe ekranının
"Detaylı" sekmesindeki modül menüsü bunların **yerini** açıyor, kendilerini
değil — A-M11.)*

---

## 5 · Onaylarsan sırayla şunlar yapılır

K-045.5'te üç iş *"önce muhasebe kapsamı"* diye beklemeye alınmıştı. Onay
gelince sıra şu — **yukarıdan aşağı, biri bitmeden öteki başlamaz**:

| # | İş | Neden bu sırada |
|---|---|---|
| 1 | **Ay bazında gider toplamı** (B2) | Dışa aktarılacak tablo bunsuz yok; en küçük iş |
| 2 | **Dışa aktarma** — PDF `window.print()`, Excel CSV (BOM + `;`) | Kütüphane gerektirmiyor (K-045.4). Bekleyen: **A-M22** |
| 3 | **Alacak takibi** — veresiyenin üstüne *"ne zaman alacağım"* | Sanal POS'un gerçek bitişiği; ödeme linki **bu fazda yok**, backend ister (K-045.1 · A-M20) |
| 4 | **Ticari ileti izni alanı** | Toplu mesaj yazıldı (K-047.4) ama izin altyapısı yok — cezası da, numaranın engellenmesi de bunu bekliyor (A-M21 · A-M25) |

**Onay gelmezse ne olur:** prototip bugünkü hâliyle çalışmaya devam eder; bu
dört iş beklemede kalır. Hiçbiri yarım bırakılmış değil — **başlanmamış.**

---

## 5.5 · 16.09'da gelen not: **maliyet tarafı eksik** (A-M43…A-M45)

> **Cemil:** *"Fatura girişi olunca serviste kullanılan parçalara
> ekleyebilmeli sistem. Fakat bunu müşteri için değil kâr zarar
> muhasebe için yapmalıyız."* · *"Faturasız çalışıyor da olabilirler."*

**K-022 açılmıyor.** *Fatura kesmek* (e-belge, KDV — kapsam dışı) ile
*fatura girmek* (parçacıdan gelen belgeyle maliyeti bilmek) ayrı
şeyler. Bu not ikincisi.

**Bulunan kusur:** `alis` alanı ve `is-basina` kâr ekranı **var**, ama
`alis`'i dolduran akış **yok**. Parçacının verdiği rakam bugün `fiyat`e
(müşteri fiyatı) yazılıyor; kâr `fiyat − 0` çıkıyor ve **şişik**
görünüyor. Tohum veride `alis` elle doldurulduğu için bu bugüne kadar
görünmedi.

**Önerim sırası:** önce **A-M43** (alış rakamı parça sorma akışında
girilsin), sonra **A-M44** (fatura girişi ikinci turda, düzeltici
olarak). Ayrıntı: `arastirma/MALIYET-VE-KAR.md`.

---

## 6 · Aynı turda cevaplanması iyi olur (muhasebeye komşu)

| Kod | Soru | Öneri |
|---|---|---|
| **A-M7** | İş başına kâr (`fiyat − alış`) **kayıt ekranında** da görünsün mü? | Görünmesin — kendi sayfası var (`is-basina`); kayıt ekranında dükkân kârıyla karışır |
| **A-M11** | "Detaylı" sekmesindeki beş modül (fatura/KDV · çek · senet · cari) **kapsama girdi mi** | Girmedi — yalnız **yeri** açıldı; B onaylanırsa bu satırlar orada durur ama yazılmaz |
| **A-M8** | Karar bütçesi kısıt olarak kabul mü (menü ≤8 · ekranda karar ≤3 · jargon 0) | Kabul — iki rakip incelemesi de bağımsız olarak bunu istedi |

---

**Cevap üç kelimeyle verilebilir:** *"B olsun"* · *"A yeter"* · *"C'yi konuşalım"*.
