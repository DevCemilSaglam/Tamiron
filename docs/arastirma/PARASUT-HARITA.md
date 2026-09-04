# Paraşüt — Program Haritası (rakip/referans incelemesi)

**Tarih:** 04.09.2026
**Amaç:** Türkiye'nin en yaygın bulut ön muhasebe programının modül, ekran ve veri
modeli haritasını çıkarmak. Tamiron'un "basit ön muhasebe" ayağı için **tavan
referansı**: neyin standart olduğunu, neyin fazlalık olduğunu ölçmek.

**Kaynaklar (hepsi herkese açık, giriş yapılmadı):**
- `apidocs.parasut.com` — API V4. Veri modelinin **kesin** kaynağı; pazarlama
  metni değil, gerçek kaynak ağacı.
- `parasut.com/kullanim-kilavuzu` — 29 kategori, ~175 madde. Ekran/akış kaynağı.
- `parasut.com/on-muhasebe-programi` + özellik sayfaları.

> **Not:** Uygulama içine (`uygulama.parasut.com`) giriş yapılmadı — parola girmek
> Claude'a kapalı bir işlem. Aşağıdaki menü ağacı API bölümlemesi + kılavuz
> kategorilerinden türetildi; ikisi birbirini doğruluyor. Ekran içi ayrıntı
> (buton yerleşimi, boş durum ekranları, sihirbaz adımları) için oturum gerekir.

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

### 4.4 Açık kalanlar (oturum gerektirir)
- Sol menüdeki gerçek madde sayısı ve sırası
- Yeni kullanıcının ilk giriş (onboarding) adımları
- Fatura oluşturma formundaki zorunlu alan sayısı — **basitlik ölçümü için asıl veri bu**
- Mobil uygulamanın "4 adımda e-fatura" akışının gerçek adımları
