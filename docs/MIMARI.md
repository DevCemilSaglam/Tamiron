# TAMIRON — MİMARİ (MIMARI.md) · v2 · 25.08.2026

> Tamiron'un teknoloji ve mimari kararları — kendi içinde tam. Her AI oturumu bunu okuyarak başlar. Karar değişecekse önce burası + `KARARLAR.md`; kod sonra. Kimse tek başına değiştirmez (lider onayı).

---

## 0. DOKÜMAN DİSİPLİNİ (en üst kural)
- Projede yapılan **her işlem / karar / değişiklik dokümanlara işlenir.** Belirsizlik istenmez.
- İş akışı: **şema/iş-akışı → diyagram → tartış → ekran.** Ekranda değişiklik olunca **diyagram da güncellenir** (yaşayan diyagram — `IS-AKISI.md`).
- Kararlar `KARARLAR.md`'de, mimari burada, ilerleme `ILERLEME.md`'de — yalnız lider (K-005).

---

## 1. ÜRÜN & AKTÖRLER — ÜÇ AYRI UYGULAMA
Tamiron baştan **üç ayrı arayüz uygulamasına** ayrılır; **ortak backend/çekirdek** hepsine hizmet eder.

| Uygulama | Platform | İçerik |
|---|---|---|
| **Tamirhane** (önce başlar) | mobil · tablet · bilgisayar | **Asistan** (+ Muhasebe) |
| **Yedek Parça** | mobil · tablet · bilgisayar | kendi akışı + Muhasebe |
| **Bireysel** | **yalnız mobil** | tamamen farklı deneyim |

- **Tamirhane içi ayrım:** Asistan + Muhasebe. Muhasebe şimdilik **yalnız başlık** (sonra ele alınır); **önce Asistan.**
- **Muhasebe ORTAK:** Tamirhane ve Yedek Parça muhasebesi ortak (fark çok az) → ayrı bir paylaşımlı UI olur.
- **Bireysel tamamen farklı** (kendi ekranları, yalnız mobil).
- **Fiyatlandırma = ayrı aksiyon** (üç aktör de kullanır: tamirhane → parçacı + B2B; yedek parça → B2B; bireysel → parçacı). Fiyat araştır/karşılaştır. Sonra ele alınacak.
- İki mod: **Basit / Gelişmiş.** Hedef: 1 yıl sonunda 100 süper / 50 iyi / 20 beklenti.

---

## 2. PROJE YAPISI (solution — baştan kurulur)
```
Tamiron.sln
── Çekirdek (ortak backend)
   Tamiron.Domain
   Tamiron.Application        (CQRS / MediatR)
   Tamiron.Infrastructure     (EF Core, MSSQL)
   Tamiron.Api                (tek Web API — tüm aktörlere; rol/tenant'a göre yetki)
── Paylaşımlı UI
   Tamiron.Ui.Core            (RCL: tasarım sistemi/tokens, ortak bileşenler, ApiClient)
   Tamiron.Muhasebe.Ui        (RCL: ORTAK muhasebe ekranları — Tamirhane + YedekParca)
── Tamirhane (ÖNCE)
   Tamiron.Tamirhane.Ui       (RCL: Asistan ekranları)
   Tamiron.Tamirhane.Web      (Blazor WASM PWA)
   Tamiron.Tamirhane.Mobile   (MAUI Blazor Hybrid)
── Yedek Parça
   Tamiron.YedekParca.Ui      (RCL) + .Web + .Mobile   (Muhasebe.Ui ortak)
── Bireysel (yalnız mobil)
   Tamiron.Bireysel.Ui        (RCL) + Tamiron.Bireysel.Mobile (MAUI)
── tests/  (Domain / Application / UI testleri)
```
- Proje/namespace adları **ASCII** (Tamirhane, YedekParca, Bireysel — Türkçe özel karakter yok; K-025 ruhu).
- **K-015 (web↔mobil eşitliği) her uygulama İÇİNDE geçerli:** Tamirhane.Web ≡ Tamirhane.Mobile (tüm ekran kodu ilgili `.Ui` RCL'sinde; host'a ekran eklenmez).
- **Önce Tamirhane** kurulur; Yedek Parça ve Bireysel sonra (çekirdek + Ui.Core hazır olunca hızlı gelir).

---

## 3. TEKNOLOJİ YIĞINI
**Çekirdek (gün-1):** C# / ASP.NET Core (Web API) · Blazor WebAssembly (PWA) · .NET MAUI Blazor Hybrid · paylaşımlı RCL'ler · **Microsoft SQL Server (MSSQL)** — tüm verinin tek kaynağı.

**Bileşen kütüphanesi:** **MudBlazor YOK.** Kendi **tasarım sistemimiz** (`tokens.css`) üstünde **özel/hafif bileşenler** (marka paletiyle). Material görünümüne bağlı kalmayız; kolay ve palete uyan yolu seçeriz.

**İhtiyaç anında eklenir (gün-1 DEĞİL — "başta kalabalık yok"):**
- Elasticsearch (katalog büyüyünce, arama) · Redis (cache/oturum/SignalR) · SignalR (gerçek-zaman) · Hangfire (arka plan işleri, fiyat toplama) · Docker.
- **AI / ses** (speech-to-text + LLM, A-06) · **ödeme / sanal pos** (A-04) · **WhatsApp API** (A-05) — kullanıldıkça.

**MongoDB yok. Dış bulut servisi yok** (GitHub yalnız kod deposu).

---

## 4. MİMARİ İLKELER

### 4.1 CQRS-hafif + HIZLI OKUMA
Command/Query ayrı (ayrı sınıf/klasör), ikisi de aynı MSSQL; MediatR. **Okuma/listeleme HIZLI olmalı; bir yazma kilidi okumayı ENGELLEMEZ** (birisi kaydı kitledi diye başkası okuyamaz olmasın). → SQL Server `READ_COMMITTED_SNAPSHOT`/snapshot isolation; okumalar `AsNoTracking`.

### 4.2 Multi-tenant
Dükkana özel her tabloda **`TenantId` + global query filter** (altyapıda zorunlu). Herkes yalnız kendi verisini görür (güvenlik + KVKK). **Bireysel kullanıcı modeli AÇIK (A-01):** ya ayrı tablolar, ya bireysel de bir "tenant" — A-01'de karara bağlanacak.

### 4.3 Katalog — HİBRİT (ortak + dükkan ürünü)
- **`Product.TenantId` NULLABLE:** **null = ORTAK ürün** (tüm dükkanlar okur; yalnız **grant-admin/platform yetkilisi** değiştirir) · **dolu = o dükkanın kendi eklediği ürün.**
- Global filter: bir dükkan hem **ortak (null)** hem **kendi (TenantId=kendi)** ürünlerini görür; başka dükkanın özel ürününü görmez.
- **Fiyat/tedarikçi teklifi tenant'a özel.** **İki ayrı "teklif":** parçacı→tamirhane **fiyat teklifi** (tedarik) ≠ tamirhane→müşteri **teklifi** (satış) — A-02.

### 4.4 TDD + saf para servisi
"Önce test, sonra kod": bir hesabın **ne sonuç vermesi gerektiği test olarak yazılır**, kod testi geçince doğruluğu kesinleşir. **Para/fiyat/vergi hesapları = SAF domain servisi** (DB/arayüzden bağımsız) → tek başına, kesin test edilir. Testsiz birleşmez.

### 4.5 Dinamik ürün — YAPI ARAŞTIRILACAK
Ürün özellikleri sabit kolon değil, esnek **attribute** ile. **Görev (lider):** pazaryeri entegrasyonlarına (Trendyol/Hepsiburada/N11 vb.) uyacak **standart ürün özellikleri + her ürüne uyabilecek ortak alanlar** araştırılıp **ürün veri yapısı** çıkarılacak (A-11 ile birlikte).

### 4.6 Silme — SOFT-DELETE (arşiv ihtiyaç anında)
Silme = **soft-delete** (`DateTimeDeleted`; geri alınabilir — yanlış işlem geri alınır, veri kaybı önlenir). Global filter silinmişi gizler; **undo = flag'i kaldır** (satır yerinde, ilişkiler sağlam). **Arşiv baştan kurulmaz** ("başta kalabalık yok"): ileride bir tablo *gerçekten* büyürse, O tabloya periyodik arşiv job'u eklenir (eski silinmiş kayıtlar arşiv tablosuna taşınır, ana tablo yalınlaşır). **`RowVersion`** (eşzamanlılık) + denetim alanları (`DateTimeAdded/Updated/Deleted`) her tabloda.

### 4.7 Tasarım birinci sınıf + tasarım-önce
İş mantığı/karar/yetki **sunucuda** (arayüz karar vermez) **AMA arayüz tasarımı birinci sınıf** — şık, sade, tutarlı. **Tasarım-önce:** şema→diyagram→tartış→**ekran**; ekran değişince diyagram güncellenir. **Tek tasarım sistemi:** `tokens.css` (ham hex YOK). Marka: mavi + teal + turuncu vurgu (logo somunu TEAL). Bkz. `TASARIM-SISTEMI.md`.

---

## 5. FAZLAMA
- **Faz 1 — Tamirhane / Asistan:** tasarım sistemi + kabuk (menü/tema) + çekirdek ekranlar (Araç Giriş → …). Çekirdek backend (MSSQL + API + CQRS). Muhasebe yalnız başlık.
- **Faz 2:** Muhasebe (Tamirhane+YedekParca ortak) · Yedek Parça uygulaması · fiyatlandırma · ödeme/kanallar (ihtiyaç anında).
- **Faz 3:** Bireysel (mobil) · fiyat ağı/marketplace genişleme · okuma tarafı ayrıştırma (gerekirse).

---

## 6. DEĞİŞMEZ İLKELER (özet)
1. **Her işlem dokümana işlenir; belirsizlik yok** (§0).
2. İş mantığı API'de; **arayüz tasarımı birinci sınıf**; tek tasarım sistemi; tasarım-önce.
3. Tenant tablolarında TenantId + global filter. **Katalog hibrit** (null=ortak, dolu=dükkan ürünü).
4. Command/Query ayrı; **hızlı okuma, kilit okumayı engellemez.**
5. Önce test sonra kod; para = saf domain servisi.
6. Silme = **soft-delete** (undo kolay); arşiv **ihtiyaç anında** (tablo büyürse). RowVersion + denetim alanları her tabloda.
7. **Üç ayrı uygulama** (Tamirhane/YedekParca/Bireysel); her uygulamada web↔mobil eşitliği (K-015); host'a ekran eklenmez.
8. **İhtiyaç anında altyapı** (ES/Redis/SignalR/Hangfire/Docker/AI/ödeme/WhatsApp) — başta kalabalık yok.
9. Kararı lider değiştirir; kimse tek başına mimariyi değiştirmez.
