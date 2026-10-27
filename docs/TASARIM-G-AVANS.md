# Müşteri avansı — tasarım (G7 · 27.10.2026)

> Cemil'in cevapları (K-079.1): avans **aracın sahibine** bağlı · iş kapanınca **usta seçer**
> ("avanstan düş" teklif edilir, kendiliğinden düşmez) · kullanılmayan avans iade edilirse
> **kasadan çıkış**. Kapsam dar: cari hesabın en küçük parçası — **bakiye**. Ekstre, vade, faiz,
> mutabakat YOK (K-022). Yeni ekran YOK: kayıt ve teslim ekranlarının içinde (ekran bütçesi, A-2).

## 1 · Akış

1. Müşteri işi bırakırken ya da önceden para verir → usta aracın kaydında **"Avans al"**: tutar +
   nakit/kart/havale. Para **o gün kasaya girer** (yoluyla).
2. İş teslim edilirken ya da sonradan ödeme alınırken, sahibinin avansı varsa çiplerde
   **"Avanstan"** çıkar (bakiye yazılı). Usta seçerse ödeme avanstan düşülür — kasaya **ikinci kez
   girmez** (para avans günü girmişti).
3. Kullanılmayan avans geri verilirse → **"Avansı iade et"**: tutar + yol, o gün **kasadan çıkış**.
4. Yanlış girilen avans / iade **"Geri al"** ile silinir (soft-delete) — bakiyeyi eksiye
   düşürecekse geri alınamaz (önce avanstan düşülen ödeme geri alınır).

## 2 · Kurallar (domain)

| Kural | Neden |
|---|---|
| Avans hareketi: alındı (+) ya da iade (−); tutar > 0; yol nakit · kart · havale | "Almadım" para değildir |
| **Bakiye = alınanlar − iadeler − avanstan düşülen ödemeler** | Tek kaynak: hareketler + ödemeler; ayrı "bakiye" alanı yok (ikisi ayrışmasın) |
| Bakiye **sahibe göre**: aynı sahip (ad + telefon, A-M1 anahtarı) olan araçların toplamı; sahip yazılmamışsa yalnız o araç | K-079.1(a) "aracın sahibine" — iki aracı olan müşterinin avansı ikisinde de kullanılır |
| Avanstan düşülen tutar ≤ bakiye **ve** ≤ kalan | Eksi bakiye = borç, o ayrı bir kavram (cari) |
| İade ≤ bakiye | Verilmemiş parayı geri vermek kasayı uydurur |
| Avanstan düşülen ödeme kasaya yazılmaz; avansın kendisi alındığı gün yazılır | Para bir kez gelir, bir kez sayılır |
| İade kasadan çıkıştır: kendi yolunda, kendi gününde eksi | K-079.1(c) |
| Avanstan düşülen ödemenin yolu **düzeltilmez** (nakde/karta çevrilmez, nakit avansa çevrilmez) | Çevirmek geriye dönük kasa ve bakiye değiştirir — geri al, yeniden yaz |

## 3 · Veri

`Advance` (VERI-MODELI §2c): `VehicleId` (alındığı araç) · `Amount` · `Kind` (Received/Refunded) ·
`Method` · `On` (gün) · soft-delete. Ödeme tarafında yeni yol **`PaymentMethod.Advance`**.

## 4 · Muhasebe

- **Kasa** (dönem): nakit/kart/havale = o dönemde alınan ödemeler **+ alınan avanslar − iadeler**,
  avanstan düşülen ödemeler HARİÇ.
- **Alacak**: değişmez (kalan; avanstan düşülen ödeme kalanı azaltır).
- **CSV**: avans alındı / iade satırları kendi gününde (iade eksi); avanstan düşülen ödeme
  "avanstan" yazılır, kasa sütununa girmez.

## 5 · Ekran (G8)

- Kayıt ekranı künyesinde sahibin **avans bakiyesi** (varsa) + **"Avans al"** · **"İade et"**.
- Teslim ve "Ödeme alındı" çiplerinde bakiye varsa **"Avanstan (X ₺)"**.
- Muhasebe: kasa satırları avansı içerir (ayrı satır YOK — usta "kasada ne var"ı sorar).

## 6 · Cemil onayına açık (lider kararları)

- Bakiyenin **ad + telefon** anahtarıyla birleşmesi: aynı adı farklı telefonla yazılmış sahip iki
  ayrı müşteri sayılır (müşteriler listesiyle aynı kural, B10).
- Avanstan düşülen ödemenin yolu düzeltilmez (geri al + yeniden yaz).
- Muhasebede avans **ayrı satır değil**, kasanın içinde; CSV'de ayrı satır.
