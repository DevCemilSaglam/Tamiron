# Luca Net One — Program Haritası + Paraşüt Kıyası

**Tarih:** 04.09.2026
**Amaç:** İkinci bir Türk ön muhasebe programını haritalamak ve Paraşüt'le
kıyaslamak. İki ürün aynı işi çok farklı karmaşıklıkla çözüyor; aradaki fark
Tamiron için doğrudan ölçü.

**Kaynak:** Canlı uygulama (`lucanetone.com.tr`, Cemil'in hesabı, firma 5369).
Menü ağacı ve satış faturası formu doğrudan okundu.

> **Yöntem notu:** Şifre Claude tarafından girilmedi; Cemil kendi Chrome'unda
> giriş yaptı, oturum eklenti üzerinden okundu. Hiçbir kayıt oluşturulmadı veya
> değiştirilmedi — yalnız okundu.

İkiz belge: [`PARASUT-HARITA.md`](PARASUT-HARITA.md)

---

## 1. Menü ağacı (doğrulanmış)

```
Ana Sayfa                        /company/5369
Gelirler
  Satış Faturaları               /sale-invoices
  Alış İade Faturaları           /purchase-return-invoices
Giderler
  Alış Faturaları                /purchase-invoices
  Satış İade Faturaları          /sale-return-invoices
e-Faturalar
  Gelen e-Faturalar              /incoming-invoices
  Giden e-Faturalar              /outgoing-invoices
e-Arşiv Faturaları
  Giden e-Arşiv Faturaları       /outgoing-archive-invoices
İrsaliye
  Alış İrsaliyesi                /dispatches?type=purchase
  Alış İade İrsaliyesi           /dispatches?type=purchase-return
  Satış İrsaliyesi               /dispatches?type=sales
  Satış İade İrsaliyesi          /dispatches?type=sales-return
  İrsaliyeden Fatura             /dispatches-to-invoices
e-İrsaliyeler
  Gelen e-İrsaliyeler            /incoming-dispatches
  Giden e-İrsaliyeler            /outgoing-dispatches
Müşteri/Tedarikçi
  Müşteriler                     /customers
  Tedarikçiler                   /suppliers
Ürün/Hizmet
  Ürünler                        /products
  Hizmetler                      /services
  Ürün Hareketleri               /stock-movements
Depo
  Depolar                        /warehouses
  Depo Hareketleri               /warehouse-transfers
Finans
  Kasalar                        /cashes
  Bankalar                       /banks
Çek/Senet
  Çekler                         /checks
  Senetler                       /bonds
Raporlar
  Satış-Alış Faturaları          /reports/sale-purchase-invoice
  Banka Hesap İşlemleri          /reports/bank-account-transaction
  Müşteri-Tedarikçi İşlemleri    /reports/customer-transaction
  Stok Hareket Raporu            /reports/stock-movement
  Çek-Senet Hareketleri          /reports/...
— — —
Paketler                         /packages
```

**~31 gezinme hedefi, 13 üst başlık.**

---

## 2. Ana Sayfa

Tek kart: **"Özet Mini Bilanço — Yıl bazında finansal durumunuzu tek bakışta görün"**

- **Borçlar ve Alacaklar** — çubuk grafik, altında Net Bakiye
- **Finansal Dağılım Analizi** — halka grafik: Çekler / Senetler / Kasalar / Bankalar
- **Varlıklar** — Tahsil Edilecek Faturalar · Tahsil Edilecek Alacaklar ·
  Tahsil Edilecek Çekler · Devir Edilen Bakiye
- **Kaynaklar** — Ödenecek Faturalar · Ödenecek Borçlar · Ödenecek Çekler ·
  Toplam Yükümlülük
- Sağ üstte: `E-belge kalan kontör: 0` rozeti, `Takvim` düğmesi

**Hızlı işlem kısayolu yok.** Ana sayfa tamamen okuma ekranı — bir şey yapmak
için menüye gitmek gerekiyor.

---

## 3. Satış faturası listesi

**13 sütun:** Fatura No · Müşteri · Fatura Türü · Fatura Tipi · Senaryo Tipi ·
Tutar · Döviz Cinsi · Durum · İptal/İtiraz · Vade Tarihi · Tarih ·
Muhasebeleşme Durumu · İşlemler

**6 filtre:** Müşteri · Fatura Tipi · Senaryo Tipi · Durum · Entegratör Durum · Tarih

Ayrıca `Excelden Aktar` ve `Sütun` (sütun seçici) düğmeleri.

---

## 4. Yeni Satış Faturası formu (doğrulanmış)

**Alıcı Bilgileri**
- Müşteri tipi: `Müşteri` / `Geçici Müşteri` / `Muhtelif Müşteri` (3 seçenek)
- Müşteri Seçiniz (açılır liste — serbest metinle yeni müşteri yazılamıyor)

**Sekme 1 — Fatura Bilgileri (9 alan)**
Fatura Tipi · Fatura Seri No · Fatura Tarihi · Sevk Tarihi · Vade Tarihi ·
Döviz Cinsi · Döviz Kuru · Entegrasyon Tipi · Bayi

**Sekme 2 — Diğer Bilgiler (9 alan)**
Sipariş Numarası · Sipariş Tarihi · (açılır liste) · İrsaliye Var Mı? ·
Muhasebeleşebilir mi? · (işaret kutusu) · İnternet Satışı Mı? · (açılır liste) ·
Fatura Özel Kod

**Kalem satırı (7 alan)**
Ürün · Birim · Miktar · Birim Fiyat · İskonto · Vergi · Tutar
+ `Yeni satır ekle` · `Satırı klonla` · `Yeni Fatura Kalemi Ekle`

**Toplam ~20 form alanı**, hiçbiri katlanmamış — yalnızca iki sekmeye bölünmüş.

---

## 5. Paraşüt ile doğrudan kıyas

| | **Paraşüt** | **Luca Net One** |
|---|---|---|
| Üst menü başlığı | **6** | **13** |
| Toplam gezinme hedefi | ~32 | ~31 |
| Raporlar | Modüllere dağıtılmış (9 rapor) | **Ayrı bölüm** (5 rapor) |
| Ana ekran | 5 finansal kutu + **4 hızlı işlem** | Mini bilanço, **kısayol yok** |
| Fatura formu alan sayısı | ~10 görünür | **~20 görünür** |
| Katlanmış opsiyonel alan | 3 (`+ ekle` düğmeleri) | **0** |
| Kullanıcının doldurduğu gerçek alan | **2** | **en az 4–5** |
| Müşteri girişi | Seç **veya yeni isim yaz** | Yalnız listeden seç |
| Vade tarihi | Hızlı düğmeler (Aynı gün/7/14/30/60) | Boş tarih alanı |
| Liste sütunu (fatura) | — | 13 sütun, 6 filtre |
| Dil | Günlük Türkçe | Muhasebe jargonu |

**Jargon farkı — Luca'da kullanıcıdan beklenen kelimeler:**
"Senaryo Tipi", "Entegratör Durum", "Muhasebeleşebilir mi?", "Entegrasyon Tipi",
"Muhtelif Müşteri", "Bayi", "Fatura Özel Kod", "Devir Edilen Bakiye".

Paraşüt aynı işi "Tahsil Edilecek / Tahsil Edildi", "Stok çıkışı yapılsın /
yapılmasın" gibi düz cümlelerle soruyor.

---

## 6. Tamiron için çıkarım

İki program **neredeyse aynı sayıda ekrana** sahip (~31 vs ~32). Ama:

- Luca 13 üst başlıkla açılıyor, Paraşüt 6 ile → **aynı içerik, iki kat menü yükü**
- Luca'nın fatura formunda ~20 alan var, Paraşüt'te ~10 → **aynı belge, iki kat karar**
- Luca ana sayfada hiçbir eylem sunmuyor, Paraşüt 4 kısayol veriyor

> **Sonuç:** Ekran sayısı karmaşıklığı ölçmüyor. Karmaşıklık üç yerde birikiyor:
> **(1) üst menü genişliği, (2) formdaki karar sayısı, (3) kullanılan dil.**
> Tamiron'un A-2 ekran bütçesi bunların yalnızca birini kısıtlıyor. Bütçeye
> şu ikisini eklemek gerekiyor:
> - **üst menüde en fazla N başlık**
> - **ana yoldaki her formda en fazla N karar** (varsayılanı olan alan karar sayılmaz)

Bu, `PARASUT-HARITA.md §4.5`'teki "karar bütçesi" önerisini ikinci bir örnekle
doğruluyor. Karar Cemil'in: `KARARLAR.md`'ye işlenmedi.

### Tamircinin diline dair
Luca'nın jargonu (`Senaryo Tipi`, `Muhasebeleşebilir mi?`) T-18'in tam karşıtı.
Tamiron'da bir alan adı tamircinin ağzından çıkmayan bir kelime içeriyorsa,
yanlış alan adıdır. `SOZLUK.md` bu testin yeri.
