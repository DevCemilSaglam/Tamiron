# Sigortadan yapılan işler — bizi ilgilendiren kısım

> **Durum:** araştırma notu · 01.09.2026 · iş 17
> **Soru (Cemil):** *"Sigortadan yapılan araçlar için yaklaşım ne olmalı, bizi ilgilendiren kısım nedir?"*
> **Kapsam:** tasarım kararı üretmek. Mevzuat metni değildir; oran/limit/süre gibi sayılar **bilerek yazılmadı** — bunlar poliçeye ve şirkete göre değişir, dükkândan doğrulanmalı.

---

## 1. Tek cümlelik özet

Sigortalı iş, tamirhane açısından **başka bir tamir işi değil; başka bir ÖDEYEN ve fazladan bir ONAY MAKAMI** demektir. Tamir aynı tamirdir. Değişen üç şey var:

1. **Parayı kim ödüyor** — müşteri değil (ya da tamamı müşteri değil).
2. **Kim onaylıyor** — müşterinin "tamam" demesi yetmez; **eksper** de onaylar.
3. **Ne zaman ödeniyor** — teslimde değil, faturadan sonra, gecikmeli.

Bizim programın işi bu üçünü kayıt altında tutmak. Sigorta şirketinin sistemi olmak değil.

---

## 2. Pratikte hangi durumlar var?

| Durum | Parayı ödeyen | Tamirhaneyi ilgilendiren |
|---|---|---|
| **Kendi kaskosundan** onarım | Sigorta şirketi (muafiyet varsa bir kısmı müşteri) | Eksper onayı, hasar dosya no, muafiyeti müşteriden tahsil |
| **Karşı tarafın trafik sigortasından** | Karşı tarafın şirketi | Kusur oranı netleşmeden iş başlamayabilir; dosya daha yavaş |
| **Anlaşmalı servis** olarak çalışma | Sigorta şirketi doğrudan servise | Şirketin fiyat listesi/işçilik saat ücreti bağlayıcı |
| **Anlaşmasız servis** | Genelde müşteriye ödenir, müşteri servise öder | Servis müşteriden tahsil eder; sigortayla muhatap müşteri |
| **Müşteri "sigortaya vermeyeceğim"** der | Müşteri | Normal iş; ama poliçe/hasar kaydı sorulmuş olabilir |

**Tasarım açısından önemli olan:** bu ayrımlar **serbest metin notu değil, bir ALAN olmalı**. "Sigortalı iş" bir etiket değil, kaydın para tarafını baştan sona değiştiren bir seçim.

---

## 3. Akış — sigortalı bir iş normal işten nerede ayrılır

```
        ARAÇ GELDİ
             │
             ▼
   ÖDEYEN KİM?  ──── müşteri ──────────────► normal akış (Akış 1)
             │
        sigorta / karma
             │
             ▼
   HASAR DOSYASI VAR MI?
     yoksa: müşteri ihbar eder → dosya no gelir
             │
             ▼
   EKSPER GÖREVLENDİRME
     araç görülür / fotoğraf istenir
             │
             ▼
   EKSPER ONAYI  ◄──── bizim hazırladığımız PARÇA + İŞÇİLİK listesi
     │      │
     │      └── kısmi onay: "orijinal değil muadil", "değişmesin onarılsın"
     ▼
   ONARIM  → değişen parçaların akıbeti (sovtaj) kayda geçer
             │
             ▼
   FATURA   → sigortaya mı, müşteriye mi?
             │
             ▼
   TAHSİLAT → sigorta payı (gecikmeli) + muafiyet/fark (müşteriden, teslimde)
```

**Kritik nokta:** bizim mevcut akışımızda onay **tek makam** (müşteri). Sigortalı işte **iki makam** var ve eksperin onayı bizim listemizi **değiştirebilir**. Bu, kalem düzeyinde bir durum demek.

---

## 4. Bizi ilgilendiren yedi başlık

### 4.1 Fiyatın dördüncü kaynağı
Bugün üç kaynağımız var (K: *fiyatın üç kaynağı*): **usta tahmini · tedarikçi teklifi · satış fiyatı**. Sigortalı işte bir dördüncü giriyor: **sigortanın kabul ettiği tutar**. Bu üçünden de farklı olabilir ve genelde **daha düşüktür**. Aradaki farkı kimin ödeyeceği (müşteri mi, servis mi yutuyor) kaydın parçası olmalı.

### 4.2 İki alacaklı, tek servis kaydı
Muafiyet ya da kabul edilmeyen kalem varsa **aynı kayıtta iki borçlu** olur: sigorta ve müşteri. Tahsilat ekranımız bugün tek borçlu varsayıyor. Kısmi ödeme altyapısı (çok kayıt) var — bunun üstüne **taraf** bilgisi gerekiyor.

### 4.3 Vade ve yaşlandırma
Sigorta ödemesi teslimde gelmez. Alacaklar ekranında **"sigorta alacağı"** ayrı bir grup olmalı; müşteri alacağıyla aynı listede yaşlandırılırsa dükkânın gerçek nakit durumu yanlış görünür.

### 4.4 Belge ve delil yükü daha ağır
Normal işte fotoğraf iyi olur; sigortalı işte **fotoğraf işin şartıdır**. Poliçe, ruhsat, hasar ihbar formu, hasarlı parçanın fotoğrafı, şasi fotoğrafı istenebilir. Bunlar **kontrol listesi** ister — mal kabuldeki "beklenen parçalar" mantığının aynısı: eksik varsa iş ilerlemesin.

> Not: 01.09'da hasar şeması kaldırıldı (delil fotoğraf). Sigortalı iş bu kararı **desteklemiyor da çürütmüyor da**: eksperin istediği şey şema değil, fotoğraf. Ama hangi bölgenin hasarlı olduğunun **fatura kalemleriyle tutarlı** olması gerekiyor.

### 4.5 Değişen parçanın akıbeti (sovtaj)
Sigortalı işte çıkan parça çoğu zaman **sigortanındır** — istenirse teslim edilir. Bugün "takıldı → stoktan düş" var (iş 10); sigortalı işte **çıkan parça da bir kayıt**: bekletiliyor mu, sigortaya mı gitti, hurdaya mı.

### 4.6 Eksper kalem kalem konuşur
Eksper "listenin tamamı" demez; **kalem kalem** onaylar, indirir, "muadil olsun" der. Yani kalem satırımızda **ikinci bir durum** gerekiyor: *eksper onayladı / şartlı onayladı / reddetti*.

### 4.7 Anlaşmalı servis = başka fiyat listesi
Şirketle anlaşmalıysak işçilik saat ücreti ve parça iskontosu **bizim listemiz değil, anlaşmanın listesi**. Bu ayarlarda şirket bazında tutulacak bir şey.

---

## 5. Ne YAPMAYALIM

- **Sigorta şirketi entegrasyonu.** Her şirketin ayrı sistemi/portalı var; bu bir ürün değil, bir proje kalemi. Bizim işimiz kaydı doğru tutmak ve belgeyi hazır etmek.
- **Eksper yazılımı taklidi.** Hasar değerlendirmesi bizim işimiz değil; biz eksperin kararını **kaydeden** tarafız.
- **Kusur/tazminat hesabı.** Kusur oranı sigortanın işi. Bizde sadece "ödeyen kim" sonucu görünür.
- **Poliçe geçerlilik sorgusu.** Doğrulanmadan varsayılmaz; müşteri beyanı + dosya no yeter.

---

## 6. AÇIK — Cemil kararı bekleyen ekran/alan değişiklikleri

| Kod | Karar | Etkilediği ekran |
|---|---|---|
| **A-SG1** | Servis kaydına **"Ödeyen"** alanı: Müşteri / Sigorta / Karma. Karma seçilince muafiyet alanı açılsın mı? | servis kaydı |
| **A-SG2** | **Sigorta bloğu**: şirket, poliçe no, hasar dosya no, eksper adı-telefonu, muafiyet tutarı. Akordiyon mu, ayrı ekran mı? | servis kaydı |
| **A-SG3** | Kalem satırına **eksper durumu** (onaylı / şartlı / red). Kalem satırı zaten kalabalık — ikinci satıra mı insin? | servis kaydı, parça/işçilik |
| **A-SG4** | Fiyatın **dördüncü kaynağı** "sigorta kabul". Üç kaynak kararına eklenecek mi, yoksa ayrı bir alan mı? | fiyat modeli (K kararı) |
| **A-SG5** | Tahsilatta **taraf** ayrımı: sigorta payı / müşteri payı. Tek kayıtta iki alacaklı. | tahsilat, ödeme |
| **A-SG6** | Alacaklarda **"sigorta alacağı"** ayrı grup + vade takibi. | alacaklar |
| **A-SG7** | **Sovtaj**: çıkan parçanın akıbeti (sigortaya / müşteriye / hurda). Ayrı kayıt mı, kalem notu mu? | işlemde, iade-stok |
| **A-SG8** | **Belge kontrol listesi** (poliçe · ruhsat · ihbar formu · fotoğraflar). Eksikken iş ilerlemesin mi? | servis kaydı |
| **A-SG9** | **Anlaşmalı şirket listesi** ve o şirkete ait işçilik saat ücreti / iskonto. | ayarlar |
| **A-SG10** | Fatura **kime** kesilecek (sigorta / müşteri) — temlik-vekâlet durumu bir alan mı, seçim mi? | teslim, muhasebe |

**Öneri (uygulanmadı, karar Cemil'in):** ilk adımda yalnız **A-SG1 + A-SG2 + A-SG6** yapılsın. Bu üçü "sigortalı iş"i görünür kılar ve nakit tablosunu düzeltir; gerisi (eksper kalem durumu, sovtaj, anlaşmalı fiyat listesi) gerçek bir sigortalı iş akışı izlenmeden tasarlanırsa yanlış çıkar.

## 7. Doğrulanacaklar (dükkânda sorulacak)

1. Bu dükkân hangi şirketlerle **anlaşmalı** çalışıyor, hiç çalışıyor mu?
2. Eksper onayı **yazılı** mı geliyor (portal/e-posta), yoksa telefonla mı?
3. Muafiyet müşteriden **teslimde** mi alınıyor?
4. Sigorta ödemesi ortalama **kaç günde** geliyor?
5. Çıkan parçayı sigorta **istiyor mu**, ne sıklıkla?
6. Fatura sigortaya mı kesiliyor, müşteriye mi?
