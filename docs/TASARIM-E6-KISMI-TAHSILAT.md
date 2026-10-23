# E6 · Elden taksit = kısmi tahsilat — TASARIM (Cemil onayından önce kod yok)

> **Cemil, 23.10 (K-077.3):** *"İkisi de olabilir. Ama kredi kartını taksitli söylemek zorunda
> değil. Ve elden taksit de x miktar aldım şeklinde olur."*

Bu belge **akış → kural → muhasebe etkisi → sorular** sırasıyla yazıldı. Kod, Cemil §5'teki
soruları cevaplayınca yazılır. Muhasebe hesabını değiştirdiği ve prototipte karşılığı olmadığı için
(prototip "kısmi ödeme takibi yok" diyor — 50-teslim, K-022) önce konuşulur.

---

## 1 · Bugün nasıl

Bir kaydın ödemesi **tek parça**: teslimde yol seçilir (nakit · kart · havale · almadım). "Almadım"
seçildiyse kayıt veresiye kalır; sonra **tamamı** "Ödeme alındı" ile bir günde kapanır.
Kasa = o dönemde tahsil edilmiş kayıtların **toplamları**.

**Eksik:** müşteri 5.000'lik işin 2.000'ini bugün, 3.000'ini ay sonu verirse bugün ya hiçbir şey
yazılamıyor (veresiye) ya da 5.000 birden (yalan).

## 2 · Önerilen akış — yeni EKRAN YOK

```
Teslim ekranı · "Ödeme nasıl alındı?"
  [Nakit] [Kart] [Havale] [Almadım] [Bir kısmını aldım]   ← tek yeni çip
                                         │
                                         ▼
                              Tutar: [ 2.000 ] ₺   Yol: [Nakit][Kart][Havale]
                              Kalan 3.000 ₺ veresiye kalır.

Kayıt ekranı (veresiye kayıt) · "Düzenle" altında:  "Ödeme alındı"
                              Tutar: [ 3.000 ] ₺ (kalan önerilir)   Yol: [..]
                              → kalan 0 olunca kayıt "ödendi"

Sesle:  "06 TMR 07 için 2 bin aldım nakit"  → onay ekranı → aynı iş
```

Ana yol (tam ödeme) **değişmez**: bugün nasıl tek dokunuşsa öyle kalır. Kısmi tahsilat yalnız
seçilince görünür.

## 3 · Kural

| Konu | Öneri |
|---|---|
| Kayıt | Kayıt başına **ödemeler listesi**: tutar · yol · gün. Cari hesap (ekstre, vade, mutabakat) YOK |
| Kalan | Toplam − alınanların toplamı. Kalan > 0 = veresiye |
| Tam ödeme | Tek ödeme satırı = toplam (bugünkü davranışın aynısı) |
| Fazla ödeme | Kalandan fazla girilemez (422 "Kalan 3.000 ₺ — fazlası girilemez") |
| Kart taksiti | **Bilgi değil** — "Kart" yeter, taksit sayısı sorulmaz (Cemil) |
| Düzeltme | Yanlış girilen ödeme "Düzenle" altında silinir / yolu düzeltilir (E1'in genişlemesi) |
| Teslimden sonra satır değişirse | Kalan yeniden hesaplanır; alınan para değişmez (para bir olgudur) |

## 4 · Muhasebe etkisi

- **Kasa:** "o dönemde tahsil edilmiş kayıtların toplamı" yerine **o dönemde alınan ödemelerin
  toplamı** (yola göre). Tam ödemeli işte sonuç AYNI — prototip eşdeğerliği (0 fark) sürer.
- **Alacak:** kayıt başına **kalan**.
- **İş başına fark ve parça alışı:** değişmez (teslim günü esas).
- **CSV:** soru — kayıt başına mı, ödeme başına mı (§5).
- **Göç:** var olan kayıtlar için ödeme satırları ÜRETİLİR — teslimde ödenmişler: (toplam, yol,
  teslim günü); sonradan tahsil edilmişler: (toplam, yol, tahsil günü); veresiyeler: satır yok.
  Göçten önce ve sonra kasa rakamları aynı olmalı (`MigrationUpgradeTests` gibi ölçülür).
- E1'deki "ödeme yolunu düzelt" ödeme satırının yolunu düzeltmeye dönüşür.

## 5 · Cemil'e sorular

1. **Teslimde kısmi ödeme** olur mu ("işi aldı, 2.000 verdi, kalanı sonra"), yoksa kısmi tahsilat
   yalnız sonradan mı (veresiyeye ara ödeme)?
2. **Fazla ödeme** hiç olmaz diyebilir miyiz (girilemez), yoksa "üste para" (bahşiş/yuvarlama) yazılsın mı?
3. **CSV** (muhasebeciye giden): kayıt başına bir satır (alınan toplam) mı, her ödeme ayrı satır mı?
   Muhasebeci açısından ayrı satır doğru (kasaya giriş günü), ama dosya uzar.
4. **Sesle** "2 bin aldım" kayıt içinde mi söylenir (kayıt açıkken), yoksa plakayla her yerden mi?
