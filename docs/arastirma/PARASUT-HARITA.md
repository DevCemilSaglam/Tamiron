# Paraşüt — Program Haritası (rakip/referans incelemesi)

**Tarih:** 04.09.2026
**Amaç:** Türkiye'nin en yaygın bulut ön muhasebe programının modül, ekran ve veri
modeli haritasını çıkarmak. Tamiron'un "basit ön muhasebe" ayağı için **tavan
referansı**: neyin standart olduğunu, neyin fazlalık olduğunu ölçmek.

**Kaynaklar:**
- **Canlı uygulama** (`uygulama.parasut.com`, deneme hesabı) — menü ağacı ve
  fatura formu doğrudan okundu. §2.0 ve §2.10 bu kaynaktan, **doğrulanmış**.
- `apidocs.parasut.com` — API V4. Veri modelinin **kesin** kaynağı.
- `parasut.com/kullanim-kilavuzu` — 29 kategori, ~175 madde.
- `parasut.com/on-muhasebe-programi` + özellik sayfaları.

> **Yöntem notu:** Şifre Claude tarafından girilmedi; Cemil kendi Chrome'unda
> giriş yaptı, oturum eklenti üzerinden okundu. Uygulamada hiçbir kayıt
> oluşturulmadı/değiştirilmedi — yalnız okundu.

---

## 1. Üst düzey modül haritası

API dokümanı programı **6 ana bölüme** ayırıyor. Kılavuz kategorileri de aynı
bölümlemeyi izliyor, üstüne 3 destek alanı ekliyor:

| # | Bölüm | İçerik |
|---|-------|--------|
| 1 | **Satışlar** | Teklif · Satış Faturası · Müşteri/Tedarikçi (tahsilat) |
| 2 | **Giderler** | Fiş/Fatura · Banka Gideri · Maaş · Vergi · Çalışan · Müşteri/Tedarikçi (ödeme) |
| 3 | **Resmileştirme (e-Belge)** | e-Fatura Gelen Kutusu · e-Arşiv · e-Fatura · e-SMM |
| 4 | **Nakit** | Kasa ve Banka Hesapları · İşlemler (borç/alacak) |
| 5 | **Stok** | Ürün · Depo · İrsaliye · Stok Hareketi · Depo Stok Seviyesi · Stok Güncelleme |
| 6 | **Ayarlar** | Kategori · Etiket |
| + | Raporlama | (API'de yok, arayüzde var) |
| + | Kullanıcı/Hesap | Yetkilendirme, firma bilgisi, çoklu firma, abonelik |
| + | Entegrasyonlar | Banka, e-ticaret, kargo, CRM, finansman |

---

## 2.0 Gerçek menü ağacı (uygulamadan okundu — doğrulanmış)

```
GÜNCEL DURUM                    /
ASİSTAN  [BETA]                 /parasut-asistan
SATIŞLAR
  Teklifler                     /teklifler
  Faturalar                     /satislar
  Müşteriler                    /musteriler
  Satışlar Raporu               /raporlar/satislar
  Tahsilatlar Raporu            /raporlar/tahsilatlar
  Gelir Gider Raporu            /raporlar/gelir-ve-gider-raporu
GİDERLER
  Gider Listesi                 /giderler
  Tedarikçiler                  /tedarikciler
  Çalışanlar                    /calisanlar
  Giderler Raporu               /raporlar/giderler
  Ödemeler Raporu               /raporlar/odemeler
  KDV Raporu                    /raporlar/kdv
NAKİT
  Kasa ve Bankalar              /kasa-ve-bankalar
  Çekler                        /cekler
  Kasa / Banka Raporu           /raporlar/kasa-banka
  Nakit Akışı Raporu            /raporlar/nakit-akisi
STOK
  Hizmet ve Ürünler             /hizmet-ve-urunler
  Depolar                       /depolar
  Depolar Arası Transfer        /depolar-arasi-transfer
  Giden İrsaliyeler             /giden-irsaliyeler
  Gelen İrsaliyeler             /gelen-irsaliyeler
  Fiyat Listeleri               /fiyat-listeleri
  Stok Geçmişi                  /stok-hareketleri
  Stoktaki Ürünler Raporu       /raporlar/stoktaki-urunler
E-TİCARET  [YENİ]               /e-ticaret/sozlesmeler
— — —
UYGULAMALAR                     /entegrasyonlar
PAZARYERİ                       (dış site)
MENÜYÜ SAKLA                    (menüyü daraltır)
AYARLAR
  Firma Bilgileri               /firma-bilgileri
  Kategori ve Etiketler         /kategori-ve-etiketler
  Kullanıcılar                  /kullanicilar
  Yazdırma Şablonları           /fatura-sablonlari
```

**Toplam ~32 gezinme hedefi** (form/detay ekranları hariç). 6 üst başlık, en
derin dal 8 madde.

**Beklentiyi bozan üç bulgu:**

1. **"Raporlar" diye bir bölüm yok.** 9 rapor, ait oldukları modüllerin içine
   dağıtılmış. Kullanıcı rapor aramaya değil, bulunduğu bölümde raporu görmeye
   yönlendiriliyor. Tamiron için doğrudan kopyalanabilir bir karar: ayrı rapor
   menüsü açmak yerine raporu işin yanına koymak.
2. **Müşteriler ve Tedarikçiler arayüzde AYRI menü maddesi** — oysa API'de ikisi
   de tek `contact` varlığı. Yani veri modeli birleşik, arayüz ayrık. Tamiron'da
   da aynısı yapılabilir: tek tablo, iki liste ekranı.
3. **Kılavuzda anlatılan "Krediler" menüde yok.** Muhtemelen Kasa/Banka içine
   gömülü. Kılavuz ile ürünün birebir örtüşmediğinin hatırlatıcısı.

### 2.10 Hızlı İşlemler (ana ekran kısayolları — doğrulanmış)

Güncel Durum ekranında sabit **4 kısayol**:
`SATIŞ FATURASI OLUŞTUR` · `HIZLI FİŞ/FATURA OLUŞTUR` · `ALIŞ FATURASI OLUŞTUR` ·
`MÜŞTERİ OLUŞTUR`

32 ekranlık bir programda ana ekrandan **yalnız 4 eylem** öne çıkarılmış.

### 2.11 Yeni Satış Faturası formu — alan sayımı (doğrulanmış)

Ekranda görünen alanlar:

| Alan | Tip | Varsayılan |
|---|---|---|
| Fatura İsmi | metin | boş |
| Müşteri | arama + yeni isim yazılabilir | boş |
| Müşteri Bilgileri | salt okunur | seçimle dolar |
| Tahsilat Durumu | 2 seçenek | **Tahsil Edilecek** |
| Düzenleme Tarihi | tarih | **bugün** |
| Vade Tarihi | Aynı gün / 7 / 14 / 30 / 60 + tarih | **Aynı gün** |
| Stok Takibi | 2 seçenek | **Stok çıkışı yapılsın** |
| Satır: Hizmet/Ürün · Miktar · Birim · Br. Fiyat · Vergi · Toplam | — | **1,00 · Adet · 0,00 · KDV %20** |
| Fatura Kategorisi (sağ panel) | seçim | Kategorisiz |
| Etiketler (sağ panel) | seçim | Etiketsiz |

Katlanmış hâlde duran opsiyoneller: `+ FATURA NO EKLE` · `↻ DÖVİZ DEĞİŞTİR` ·
`+ SİPARİŞ BİLGİSİ EKLE`

> **Bu belgedeki en önemli ölçüm:** ekranda ~10 alan görünüyor ama kullanıcının
> gerçekten doldurması gereken alan sayısı **2** — müşteri ve ürün/fiyat. Geri
> kalan her şeyin ya akıllı varsayılanı var, ya opsiyonel, ya da katlanmış.
> Paraşüt "az alan" değil, **az karar** tasarlamış. Tamiron'un basitlik hedefi
> ekran/alan saymakla değil, **kullanıcıdan istenen karar sayısını** saymakla
> ölçülmeli.

*(Formun alt kısmı — notlar, indirim, genel toplam — iframe içinde kaldığı ve
görüntü alanına sığmadığı için okunamadı.)*

---

## 2. Ekran ekran

### 2.1 Genel Bakış (Dashboard)
Nakit akışı grafiği · anlık gelir–gider · yaklaşan tahsilat/ödeme hatırlatmaları.
Kılavuzdaki "tek ekranda toplama" vaadi buranın satış argümanı.

### 2.2 Satışlar
- **Teklifler** — oluştur, e-posta gönder, durum güncelle, PDF, arşivle,
  **onaylanan teklifi faturaya çevir**.
- **Satış Faturaları** — oluştur/düzenle/sil · **tahsilat ekle** · iptal ·
  geri al · arşivle.
  Kılavuzdaki alt akışlar: iade faturası · tevkifatlı fatura · ihraç kayıtlı ·
  dövizli fatura · mahsuplaştırma · ara toplam indirimi · otomatik tekrarlayan
  fatura · toplu işlem · fatura kopyalama · yazdırma şablonu · e-posta ile paylaşma.
- **Serbest Meslek Makbuzu** — ayrı belge tipi (stopaj + KDV otomatik).
- **Tahsilat Makbuzu** — ayrı yazdırılabilir belge.

### 2.3 Giderler
Kılavuz gider türlerini **5'e** ayırıyor:
1. **Hızlı Fiş/Fatura** — tek tutar, kalem yok
2. **Detaylı Fiş/Fatura** — kalemli, stok etkiler
3. **Banka Gideri** — komisyon/masraf
4. **Maaş / Prim / Avans**
5. **Vergi / SGK Primi**

Hepsinde ortak: ödeme kaydet · arşivle · iptal/geri al · tekrarlayan gider.
Ayrıca **cepten harcama** (fotoğrafla) ve **yapay zekâ ile fiş okutma (OCR)**.

### 2.4 Cari Hesaplar (Müşteri / Tedarikçi)
Tek varlık: `contact`. Müşteri ve tedarikçi **aynı kayıt**, yön işlemle belirlenir.
- Bakiye takibi · **ekstre oluşturma / mutabakat** · ödeme hatırlatma
- Tahsilat ve ödeme doğrudan cari üzerinde de yapılabilir (faturasız)
- **Cariler arası virman**

### 2.5 Stok
- **Ürün/Hizmet** — hizmet ile ürün aynı listede, stok takibi opsiyonel
- **Depolar** — çoklu depo, depo bazlı stok seviyesi
- **İrsaliyeler** — sevk irsaliyesi, e-İrsaliye, gelen irsaliye yanıtlama,
  **birden fazla irsaliyeden tek fatura**, parçalı gönderim
- **Stok Hareketleri** — salt okunur hareket defteri
- **Stok Güncelleme** — sayım/düzeltme kaydı
- Barkod ile ürün çağırma · minimum stok uyarısı

### 2.6 Nakit
- **Kasa ve Banka Hesapları** — hesap listesi, hesap içi işlem defteri,
  borç/alacak işlemi, hesaplar arası transfer, **bakiye sabitleme**, arşivleme
- **Çekler** — alınan / verilen çek ayrı yönetim, vade takibi, çek kontrolü, dışa aktarma
- **Krediler** — kredi takibi
- **Banka mutabakatı** + **akıllı banka entegrasyonu** (12 banka; hareketleri
  otomatik çeker, cariyle eşleştirir, açık faturayı otomatik kapatır, öğrenir)

### 2.7 e-Belgeler
e-Fatura · e-Arşiv (+interaktif) · e-İrsaliye · e-İhracat · e-SMM.
**Gelen e-Faturalar** ayrı bir menü. Başvuru süreçleri, iptal/iade,
arşivleme/raporlama, "Anahtar Teslim" kurulum hizmeti.

### 2.8 Raporlar
Ön tanımlı raporlar: Gelir–Gider ve Kârlılık · Nakit Akışı · KDV Raporları ·
Vade Takibi · Cari Ekstre. Gelir/gider kalemleri **kategori ve etiket** ile
detaylandırılıyor.

### 2.9 Ayarlar
Kategori · Etiket · firma bilgileri · kullanıcı davet + **yetkilendirme** ·
çoklu firma · 2FA · veri dışa aktarma / hesap sıfırlama · abonelik yönetimi.

---

## 3. Veri modeli (API V4'ten, kesin)

```
contact           (müşteri/tedarikçi — TEK varlık, yön işlemde)
employee          (çalışan — contact'tan ayrı)
product           (ürün/hizmet)
warehouse         (depo)
  stock_movement · depo stok seviyesi · stok güncelleme
estimate          (teklif)          --dönüşüm-->  sales_invoice
sales_invoice     (satış faturası)  --tahsilat--> transaction
purchase_bill     (fiş/fatura: basic | detailed)
bank_fee | salary | tax   (gider alt tipleri, her biri ayrı endpoint)
shipment_document (irsaliye)
account           (kasa/banka)      -->          transaction (debit/credit)
e-belge: e_archive | e_invoice | e_smm  (+ e-fatura gelen kutusu)
category · tag    (raporlama boyutları)
```

**Dikkat çeken kararlar:**
- Müşteri ve tedarikçi ayrılmamış → tek cari kartı.
- Çalışan cariden **ayrılmış** → maaş cari bakiyeyi kirletmiyor.
- Gider türleri ayrı endpoint → her tür kendi formuna sahip, tek "gider" formu yok.
- Teklif → fatura dönüşümü birinci sınıf işlem.
- Resmileştirme faturadan **ayrı katman** → önce fatura, sonra resmileştir.
- Raporlama API'de yok; kategori + etiket iki boyut olarak veriyi süslüyor.

---

## 4. Tamiron için çıkarımlar

**Tamircinin hükmü:** *"çok fazla özellik var, karmaşık geldi."* Paraşüt bu
karmaşıklığın **tavanı** — kopyalanacak değil, ölçülecek referans.

### 4.1 Almaya değer görünenler (ön muhasebe iskeleti)
| Paraşüt kararı | Tamiron'a değeri |
|---|---|
| Müşteri = tedarikçi, tek cari kartı | Araç sahibi ile parçacı aynı yapıda tutulabilir; ekran sayısı düşer |
| Teklif → fatura tek tıkla dönüşüm | Tamiron'un **onay → iş → fatura** akışının aynısı |
| Fatura üzerinde tahsilat ekleme | Ayrı "tahsilat ekranı" açmaya gerek yok |
| Cari ekstre / mutabakat | Tamircinin *"kimden ne alacağım"* derdinin doğrudan karşılığı |
| Hızlı gider (tek tutar, kalemsiz) | Parça alımı dışındaki her gider için tek alanlık form |
| Kategori + etiket = raporlama boyutu | Ayrı rapor ekranları yerine iki alan |

### 4.2 Almamaya değer görünenler (T-18 / A-2 ekran bütçesi)
- 5 ayrı gider türü ekranı — tamirhanede en fazla 2 gerekir (parça alımı, diğer)
- Çek/senet, kredi, banka mutabakatı, bakiye sabitleme
- Çoklu depo + depo transferi + stok sayımı — tek tamirhane, tek depo
- İrsaliye katmanının tamamı
- Maaş/prim/avans, SGK, vergi modülleri
- e-Belge çeşitliliği (5 tip) — Tamiron'da en fazla e-Arşiv + e-Fatura konuşulur
- Entegrasyon pazarı (e-ticaret, kargo, CRM, finansman)

### 4.3 Doğrudan kıyas
Paraşüt'ün ana yolu — **cari → teklif → fatura → tahsilat → rapor** — 5 durak.
Tamiron'un ana yolu — **araç kabul → teşhis → parça → onay → iş → teslim → tahsilat**.
İki akış yalnızca son iki durakta kesişiyor. Yani Paraşüt'ün *tamamı* Tamiron'un
yalnızca kuyruğuna denk geliyor; gövdesi (servis kaydı, parça tedariki) Paraşüt'te
**yok**. Bu, "Tamiron bir muhasebe programı değildir; kuyruğunda muhasebe olan bir
servis programıdır" tezini destekliyor.

### 4.4 Açık kalanlar
- ~~Sol menüdeki gerçek madde sayısı ve sırası~~ → **§2.0'da kapandı** (~32 hedef)
- ~~Fatura formundaki alan sayısı~~ → **§2.11'de kapandı** (görünen ~10, gerçekte doldurulan 2)
- Fatura formunun alt yarısı (notlar, indirim, genel toplam) — iframe, okunamadı
- Yeni kullanıcının ilk giriş (onboarding) adımları
- Mobil uygulamanın "4 adımda e-fatura" akışının gerçek adımları

### 4.5 Tek cümlelik sonuç
Paraşüt 32 ekranlık bir program ama ana ekranda 4 kısayol, en sık kullanılan
formda 2 gerçek karar var. **Karmaşıklığı ekran sayısıyla değil, kullanıcıdan
istenen karar sayısıyla yönetiyor.** Tamircinin "karmaşık geldi" tepkisi de
muhtemelen ekran sayısına değil, karar sayısına verilmiş bir tepkiydi —
A-2 ekran bütçesinin yanına bir **karar bütçesi** koymak gerekebilir.
