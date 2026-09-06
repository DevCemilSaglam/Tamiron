# OtoMixSQL — Modül Haritası (rakip/referans incelemesi)

**Tarih:** 05.09.2026
**Neden çıkarıldı:** Cemil, *"detaylı muhasebe alanı Otomix gibi olacak,
butonlar buna göre olmalı"* dedi. **Tahmin edilmedi — bakıldı.**

**Kaynak:** `otoservisprogram.com` (OtoMixSQL ürün sayfası, açık web).
Paraşüt ve LucaNet incelemelerinden farkı: o ikisi **canlı uygulamadan**
okunmuştu (Cemil'in hesabı), bu yalnız **ürün sayfasından**. Ekran içi
davranış görülmedi — modül **adları** ve **kümeleri** doğrulanmış, form
alanları ve akış **doğrulanmamıştır.**

İkiz belgeler: [`PARASUT-HARITA.md`](PARASUT-HARITA.md) ·
[`LUCANET-ONE-HARITA.md`](LUCANET-ONE-HARITA.md)

---

## 1 · Modüller (ürün sayfasında adlandırıldığı gibi)

| Küme | Modül |
|---|---|
| **Servis** | Oto Servis · Randevu / Ajanda |
| **Para** | **Cari** · **Fatura** · **Kasa** · **Banka** · **Çek** · **POS** |
| **Stok** | Stok *(çoklu depo)* |
| **Satış** | Teklif |
| **e-Belge** *(e-Connect, yalnız Pro)* | e-Fatura · e-Arşiv Fatura |
| **İletişim** | SMS |
| Yatay | Raporlamalar *(her modülün kendi raporu)* · Kullanıcı Yetkilendirme |

**Öne çıkan iki şey:**

1. **Muhasebe altı ayrı modül** (cari · fatura · kasa · banka · çek · POS).
   Paraşüt bunları **üç** başlıkta topluyordu (Satışlar · Giderler · Nakit).
   Aynı iş, iki kat başlık — LucaNet'te de aynı desen vardı (13 üst başlık).
2. **Rapor ayrı bir modül değil**, her modülün içinde. Paraşüt'te de böyleydi;
   üç programın üçü de aynı kararı vermiş. *"Raporlar"* diye ayrı bir menü
   açmak, ustayı raporu **aramaya** gönderiyor.

---

## 2 · Tamiron'a ne alındı

Cemil *"Otomix gibi"* derken **modül tahtasını** kastediyor: masaüstü
programların klasik görünümü — **ikonlu kare kutular, ızgara hâlinde.**
Alınan şey **dil ve düzen**, kapsam değil.

### 2.1 Düzen: liste değil ızgara

| | Ana menü | Muhasebe modülleri |
|---|---|---|
| Biçim | `.menu-row` — satır | `.modul-kutu` — ızgara |
| Neden | **Liste**: yukarıdan aşağı okunur, **sırası anlam taşır** | **Harita**: gözle taranır, hepsi aynı ağırlıkta |

### 2.2 Tamiron'un modül tahtası

| Modül | Durum | Nereye gider |
|---|---|---|
| **Kasa** | çalışıyor | basit kipin kendisi — tahsilat, gider, alacak |
| **Raporlar** | çalışıyor | `is-basina` |
| **SMS** | çalışıyor | `mesaj` — özet ve bakım hatırlatması (7. iş) |
| Cari · Fatura · Banka · Çek · POS · Stok · Teklif | **yazılmadı** | solgun, dokunulamaz |

**Sıra Otomix'in sırası değil, Tamiron'un sırası:** çalışanlar önce.
Otomix'te modüller klasik sırada duruyor çünkü **hepsi yazılmış**. Burada
yarısı yazılmadı; çalışanı aşağı atmak, ustayı **çalışmayanların arasında
aratmak** olurdu.

### 2.3 Neden yazılmadı işaretli duruyorlar

Yedisi de ustadan **yeni veri** ister ve K-022'nin gerekçesi duruyor. Ama
**menüde görünüp dokununca hiçbir şey yapmayan** bir kutu, tam olarak ölü
uçtur — ve en pahalı hâli budur, çünkü usta *"bu program bunu yapıyor"*
sanır. Devre dışı kutu ölü uç değildir: dokunulamaz ve nedenini söyler.

> **Kapsam genişlemedi, YERİ AÇILDI.** Cemil kapsamı genişletirse tahtada
> yerleri hazır. Açık madde **A-M11**.

---

## 3 · Üç program yan yana

| | Paraşüt | LucaNet ONE | **OtoMixSQL** | **Tamiron** |
|---|---|---|---|---|
| Üst başlık | 6 | 13 | ~11 modül | **6 menü + 10 modül kutusu** |
| Muhasebe kümesi | 3 başlık | 6+ başlık | **6 modül** | **1 ekran (Kasa) + 9 kutu** |
| Rapor | modüllere dağık | ayrı bölüm | modüllere dağık | modüllere dağık |
| Servis/iş emri | **yok** | **yok** | **var** | **var** — ürünün gövdesi |

**OtoMix, Tamiron'un en yakın rakibi:** tek program hem servis hem muhasebe
yapıyor. Paraşüt ve LucaNet'in gövdesi fatura; Tamiron'un ve OtoMix'in
gövdesi **iş emri**. Fark, OtoMix'in muhasebeyi **altı modüle** açması —
Tamiron'un tamircisi *"çok fazla özellik var, karmaşık geldi"* demişti (T-1).

> Tahta aynı, **üzerindekilerin sayısı** farklı. Kapsam kararı (MUH) bu
> farkın nerede duracağını belirliyor.
