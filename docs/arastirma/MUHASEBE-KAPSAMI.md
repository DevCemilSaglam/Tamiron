# MUHASEBE KAPSAMI — üç seçenek, ölçülmüş karşılaştırma

**Tarih:** 04.09.2026 · **Yazan:** lider (Cemil bilgisayarda değilken)
**Karar bekleyen madde:** `HANDOFF.md §5 · MUH`
**Referanslar:** [`PARASUT-HARITA.md`](PARASUT-HARITA.md) ·
[`LUCANET-ONE-HARITA.md`](LUCANET-ONE-HARITA.md) · `KARARLAR.md` K-013, K-014

> **Bu belge karar vermez, kararı hazırlar.** Öneri en sonda; seçim Cemil'in.

---

## 1 · Asıl soru "ne kadar muhasebe" değil

K-013'ün cümlesi hâlâ ayakta:

> *"Her gideri gir" diyen bir ekran, girilmediğinde **yalan söyleyen** bir
> ekrandır. Yanlış rakam gösteren muhasebe, muhasebe olmamasından kötüdür.*

Yani seçenekleri **özellik listesiyle** değil, **ustadan istenen veri
girişiyle** ölçmek gerekiyor. İki rakip incelemesi de bağımsız olarak aynı
sonuca vardı:

> *"Karmaşıklığı ekran sayısıyla değil, kullanıcıdan istenen **karar sayısıyla**
> yönetiyor."* (`PARASUT-HARITA.md §4.5`)

**Ölçüt bu belgede şu:** bir muhasebe kalemi, ustadan **kaç yeni giriş** istiyor
ve **girilmezse yalan söylüyor mu?**

| Rakam türü | Girilmezse ne olur |
|---|---|
| **Zaten kayıtlı veriden çıkan** | Hiçbir şey — rakam her zaman doğru |
| **Ustadan yeni giriş isteyen, eksik olabilen** | **Rakam yalan söyler** |
| **Ustadan yeni giriş isteyen, eksik olamayan** | Rakam doğru ama iş yükü var |

---

## 2 · Üç seçenek

### A · Günlük kasa
*"Bugün ne tahsil ettim, kimden alacağım."*

| | |
|---|---|
| Yeni veri girişi | **Sıfır** |
| Nereden çıkıyor | Teslimdeki `PaymentMethod` + satır `Price` (K-004) |
| Yalan söyleyebilir mi | **Hayır** |
| Yedi işten hangisi | **4 · Teslimde ödemeyi nasıl aldığını bilmek** |

**Şu an prototipte olan bu.** Üç rakam bedavaya geliyor: bugün tahsil edilen ·
ödeme yoluna göre dağılım · **veresiye listesi**. Veresiye tek rakam değil
**liste** olarak veriliyor, çünkü ustanın sorusu *"ne kadar alacağım"* değil,
**"kimden alacağım"**.

### B · Kasa + gider
A'nın üstüne dükkânın giderleri.

| | |
|---|---|
| Yeni veri girişi | **Her gider için bir giriş** |
| Yalan söyleyebilir mi | **Evet** — girilmeyen gider görünmez |
| Yedi işten hangisi | Doğrudan hiçbiri; 4'ün komşusu |

K-014 bunu şu gerekçeyle açtı: *"söylemek form doldurmak değil."* Mikrofona
basıp *"2400 TL elektrik faturası ekle"* demek, ustanın zaten yaptığı bir şeyin
yanına eklenebiliyor.

**04.09'da bir delik kapandı:** girişin tek yolu mikrofondu ve Web Speech
Firefox'ta yok (K-015) — o tarayıcıda gider bölümü **hiç kullanılamıyordu**.
Elle giriş eklendi (K-021.1).

### C · Cari hesap
Her müşteri ve parçacı için yürüyen bakiye.

| | |
|---|---|
| Yeni veri girişi | **Açılış bakiyesi · her ödeme · her kısmi ödeme · her iade** |
| Yalan söyleyebilir mi | **Evet, ve en kötüsü** — bakiye bir kez kayarsa bir daha tutmaz |
| Yedi işten hangisi | **Hiçbiri** |

Paraşüt'te cari hesap bir **ekran ailesi**: ekstre · mutabakat · virman · ödeme
hatırlatma (`PARASUT-HARITA.md §2.4`). LucaNet'te ayrı Müşteriler + Tedarikçiler
menüsü, üstüne 5 rapor.

**Tamiron'da müşteri tablosu bile yok** (K-012): müşteri, aracın üstünde iki
alan. Cari hesap açmak, K-012'yi geri almak ve v1'in üç tablolu müşteri
dünyasını (`Party`/`Contact`/`VehicleContact`) geri getirmek demek — tamircinin
*"karmaşık"* dediği yığının tam ortası.

---

## 3 · Rakiplerle kıyas — ve Tamiron'un ölçülmüş sayısı

| | Paraşüt | LucaNet ONE | **Tamiron (bugün)** |
|---|---|---|---|
| Üst menü başlığı | 6 | 13 | **7** |
| Gezinme hedefi | ~32 | ~31 | **12 ekran** |
| Ana formda görünen alan | ~10 | ~20 | ruhsat **7**, kayıt **değişken** |
| **Kullanıcının doldurduğu gerçek alan** | **2** | 4–5 | **ölçüldü, aşağıda** |

### Tamiron'un ana yolu — karar sayımı (04.09, ölçüldü)

Varsayılanı olan alan **karar sayılmaz** (rakiplerle aynı ölçü).

| Ekran | Karar | Ne |
|---|---|---|
| Ruhsat *(fotoğraf yolu — %80)* | **1–3** | tür · *(bakımsa)* km · *(varsa)* telefon |
| Ruhsat *(elle yol — istisna)* | 8 | 7 alan + tür |
| Servis kaydı | **1 + 2n** | şikâyet + her satır için ad ve tutar |
| Teslim | **1** | ödeme yolu |

Ruhsat çekilince **5 alan kendiliğinden doluyor** (plaka · marka · model · yıl ·
motor); boş kalan yalnız telefon ve km.

**Tek parçalı bir tamirat için baştan sona: 5–6 karar.** Paraşüt'ün tek bir
faturası 2 karar istiyor ama o yalnız faturayı kapsıyor; Tamiron'un 5–6 kararı
**araç kabulünden tahsilata kadar** her şeyi kapsıyor.

> **Hiçbir ekranda 3'ten fazla sabit karar yok.** Bu sayı bir kazanç değil, bir
> **kısıt** — korunması gerekiyor (§5).

---

## 4 · Bedavaya gelen dördüncü rakam: PARÇA ALIŞI

`ServiceLine.Cost` (`alis`) **zaten kayıtlı** — T-9 yüzünden: *"Usta kendi fiyat
aldıysa, parça fiyatlarına ekleme yapabiliyor."* Parçacıdan gelen fiyat satıra
yazılıyor.

Yani **tamircinin parçaya ödediği para veri modelinde duruyor** ve ustadan tek
bir yeni giriş istemiyor. §1'in ölçüsüne göre bu, A grubuna ait bir rakam:
**girilmezse yalan söylemiyor**, çünkü zaten girilmiş.

**⚠️ Tek riski çift sayma:** usta parçayı hem servis satırına alış olarak yazıp
hem de elle *"Öz Yedek Parça — triger seti"* diye gider eklerse aynı para iki
kez sayılır. Bu yüzden ekranda **ayrı** duruyor ve karışmıyor; ekran uyarıyı da
yazıyor.

### Kâr neden yine hesaplanmıyor

**İş başına kâr** (`fiyat − alış`) matematiksel olarak doğru çıkar. **Dükkân
kârı** çıkmaz — kira, elektrik, maaş girilmemiş olabilir. İkisi aynı ekranda
durursa usta ikincisini birincisi sanar.

> **Eksik giderle çıkan kâr, kâr olmamasından kötüdür — çünkü usta ona
> *inanır*.** (K-014)

İş başına kâr göstermek ayrı bir karar; §5'te öneri olarak duruyor,
uygulanmadı.

---

## 5 · ÖNERİ

### 5.1 Kapsam: **B — kasa + gider.** C (cari hesap) kapsam dışı.

**Neden C değil:** yedi işten hiçbirine hizmet etmiyor, K-012'yi geri alıyor ve
bir kez kayan bakiye bir daha tutmuyor. *"İleride lazım olur"* gerekçe değil.

**Neden sadece A değil:** gider girişi sesle ve elle mümkün; usta girdiği kadar
görüyor ve ekran bunu **saklamıyor**.

**B'nin sınırı keskin:** fatura yok · KDV yok · tevkifat yok · e-belge yok ·
banka yok · çek/senet yok · stok yok. Bunların hepsi rakiplerde var ve
**hiçbiri yedi işten birine hizmet etmiyor.**

### 5.2 Parça alışı eklensin *(uygulandı)*

Zaten kayıtlı veriden çıkıyor, yeni giriş istemiyor, çift sayma uyarısıyla
birlikte **ayrı** duruyor.

### 5.3 Yeni kısıt: **KARAR BÜTÇESİ**

Her iki rakip incelemesi de bağımsız olarak bunu istedi. Ekran bütçesi (A-2)
karmaşıklığın yalnız bir kaynağını kısıtlıyor; ölçüm üç kaynak olduğunu
gösterdi: **menü genişliği · formdaki karar sayısı · dil.**

Önerilen kısıt:

| Kısıt | Değer | Bugünkü ölçüm |
|---|---|---|
| Üst menüde başlık | en fazla **8** | 7 |
| Ana yolda bir ekranda sabit karar | en fazla **3** | 3 (ruhsat) |
| Alan adında tamircinin ağzından çıkmayan kelime | **0** | 0 |

Üçüncüsü LucaNet'in tersi: *"Senaryo Tipi"*, *"Muhasebeleşebilir mi?"*,
*"Muhtelif Müşteri"* — T-18'in tam karşıtı. `SOZLUK.md` bu testin yeri.

### 5.4 Uygulanmadı, karar Cemil'in

| | Öneri | Neden durdum |
|---|---|---|
| **İş başına kâr** (kayıt ekranında `fiyat − alış`) | Göster | Yedi işin 3'üne hizmet ediyor (T-9) ama **yeni bir rakam**; dükkân kârıyla karıştırılma riski var |
| Gider kategorisi (kira/elektrik/parça…) | **Ekleme** | Paraşüt'ün kategori+etiket boyutu; ustadan her giderde bir karar daha ister |
| Aylık özet | **Ekleme** | *"Bu ay ne kazandım"* sorusu kâr sorusudur; gider eksikken cevaplanamaz |
