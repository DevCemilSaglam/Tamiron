# SERVİS KAYDI — SENARYOLAR

> Cemil, 08.09.2026 (telefon testi):
> *"Servis kaydı ekle, düzenle sayfalarını akışa uydurmak gerekiyor.
> Bunun için senaryolar oluştur ve buna uygun çalışmasını sağla. Sayfada
> herşey yapılabiliyor gözüküyor. Adımlar birbirini takip etmeli. Bu iki
> sayfa için ekstra uğraşmanı ve tekrar tekrar kontrol etmeni istiyorum."*

Bu belge **yeni bir akış tanımlamıyor.** [`IS-AKISI.md`](IS-AKISI.md)
zaten yedi adımı ve beş istisna kolunu yazıyor. Burada yapılan tek şey,
o akışı **iki ekranın üstüne oturtmak** ve *"ekran doğru çalışıyor mu"*
sorusunu her seferinde aynı biçimde sormak.

⚠️ **v1'de 68 senaryo vardı ve devralınmadı.** Buradaki altı senaryo
tamircinin yedi işine bağlı; yedincisi eklenecekse önce hangi işe
hizmet ettiği yazılır.

---

## 0 · İki ekran, akışın neresi

| Ekran | IS-AKISI adımı |
|---|---|
| **`ruhsat`** — "Servis kaydı ekle" | Adım 1 · plaka / ruhsat, kayıt açılır |
| **`kayit`** — servis kaydının kendisi | Adım 2–7 |

`kayit` ekranı **üç adıma** bölündü. Üçü de IS-AKISI'nın adımlarının
birebir karşılığı:

| Adım | Adı | IS-AKISI karşılığı | Önkoşulu |
|---|---|---|---|
| 1 | **İş** | Adım 2 · ne istiyor | yok |
| 2 | **Yapılanlar** | Adım 3-4-5-6 · fiyat sor · fiyat geldi · parça geldi · iş yapıldı | açıklama yazılmış **ya da** satır var |
| 3 | **Teslim** | Adım 7 · teslim ve ödeme | en az bir satır var |

**Adım kilitli değil, konuşuyor** (K-040.3). Hazır olmayan adıma
dokunulunca ekran ne eksik olduğunu söyler ve eksiğin durduğu adıma
götürür. Sönük bir sekme ustaya *"burası bozuk"* dedirtir.

**K-003 duruyor:** kayıt teslim edilene kadar açıktır, geriye serbestçe
dönülür, her adımdan sonra satır eklenebilir. Sıralanan şey ekranın
**sunumu**, kaydın hayatı değil.

Bulunulan adım **kaydın üstünde** durur (`k.adim`) — ekran değişkeninde
değil. Usta araç geçmişine bakıp geri geldiğinde bıraktığı adımda olur.
`k.adim` yoksa veriden türetilir: teslim edilmişse 3, satır varsa 2,
açıklama varsa 2, hiçbiri yoksa 1.

---

## 1 · SENARYOLAR

### S-1 · Sıradan iş — parça elde, fiyat belli
Ustanın günlerinin çoğu.

1. Menü → **Servis kaydı ekle** → plaka yazılır → kayıt açılır
2. **Adım 1 · İş** — *"triger seti değişecek"* yazılır (ya da söylenir).
   Tür metinden tahmin edilir, çip elle düzeltilebilir.
3. **Yapılanlara geç**
4. **Adım 2 · Yapılanlar** — Parça + İşçilik satırları, tutarlar yazılır
5. **Teslime geç**
6. **Adım 3 · Teslim** — kalem sayısı ve toplam görünür → **Teslim et**

**Beklenen:** Adım 1'de tablo, "parça bekliyor" ve "teslim et" GÖRÜNMEZ.
Adım 3'te özet doğru toplamı gösterir.

---

### S-2 · Parça sipariş edilecek — fiyat sorulacak
IS-AKISI Adım 3-4 ve Kol B'nin ana yola düşen hâli.

1. Adım 1 · iş yazılır → Adım 2
2. **Parça fiyatı sor** → parçacı ekranı → WhatsApp mesajı hazırlanır
3. Fiyat gelir → satırın tutarı yazılır
4. Parça yoldaysa **Parça bekliyor** işaretlenir → *"Müşteriye haber ver"*
5. Parça gelince iş yapılır, işçilik satırı eklenir
6. Adım 3 · **Teslim**

**Beklenen:** *"Parça fiyatı sor"* düğmesi Adım 2'nin içinde — teslim
düğmesinin yanında değil. Adım 3'ün özeti *"parça hâlâ bekliyor
işaretli"* uyarısını verir ama **teslimi engellemez.**

---

### S-3 · Parçayı müşteri getirdi *(IS-AKISI Kol D)*
Cemil, 08.09: *"Bu lazım olursa kullanılacak bir özellik."*

1. Adım 2'de parça satırı eklenir, adı yazılır
2. Tutar **boş bırakılır** → satırın altında soluk bir *"müşteri
   getirdi"* bağlantısı belirir
3. Dokunulur → tutar hücresi **—** olur, satırın altında *"müşteri
   getirdi"* bilgisi kalır, satır toplama girmez
4. Yanlışlıkla işaretlendiyse bilgiye dokunmak geri alır

**Beklenen:**
- Tutar yazılmış bir satırda bu bağlantı **görünmez** (parayı sen
  alıyorsan parçayı sen getirmişsindir)
- Tutar yazılınca bağlantı düşer, silinince geri gelir
- `s.fiyat` **silinmez** (K-037) — işaret geri alınınca rakam geri gelir
- Bu cümle ekranda **tek yerde** durur: bölüm başlığındaki toplu düğme
  kaldırıldı, tutar hücresinin `title`ı kaldırıldı, örnek verinin
  açıklamasından çıkarıldı

⚠️ **AÇIK (A-M23):** tutarı yazılmış bir satır artık *"müşteri getirdi"*
diye işaretlenemiyor — önce tutarı silmek gerekiyor. Bilinçli bir
daraltma (her satırda düğme göstermemek için) ama bir sürtünme.
Cemil'in kararına açık.

---

### S-4 · Bakım — km ve hatırlatma
1. Adım 1 · açıklama *"periyodik bakım"* → tür **Bakım**'a düşer
2. Adım 2 · yağ, filtre, işçilik satırları
3. Adım 3 · teslim → teslim ekranında km ve hatırlatma kurulur (K-002)

**Beklenen:** tür çipi elle değiştirildiyse metin bir daha ezmez
(`k.turElle`).

---

### S-5 · Ek iş — teslim etmeden yeni satır *(IS-AKISI Kol C)*
Cemil: *"Yapılan işe ek iş yapılabilir teslim etmeden önce."*

1. Kayıt Adım 3'te bekliyor
2. Balata da eskimiş → şeritten **2 · Yapılanlar**'a dönülür
3. Satır eklenir → tekrar Adım 3

**Beklenen:** adım şeridi geriye dönüşe izin verir; hiçbir şey
kilitlenmez; toplam güncellenir.

---

### S-6 · Yarım kayıt — usta yarıda bıraktı
1. Kayıt açılır, hiçbir şey yazılmaz
2. **Yapılanlara geç** → *"Önce ne yapılacağını yaz."* balonu, adım 1'de
   kalınır, imleç açıklama alanına gider
3. Usta çıkar, ertesi gün servis kayıtları listesinden geri gelir

**Beklenen:** kayıt Adım 1'de açılır (veriden türetilir), açıklama boş
görünür, liste kartında *"ne yapılacağı yazılmamış"* yazar.

---

## 2 · HER TURDA KONTROL EDİLECEKLER

Bu liste `tools/saglik-denetim.js`'in **yerine geçmez**, onun göremediği
şeyi kontrol eder: akışın sırası.

- [ ] Adım 1'de tablo, parça bekliyor ve teslim düğmesi görünmüyor
- [ ] Adım 2'de açıklama alanı görünmüyor (şeritten dönülebiliyor)
- [ ] Adım 3'te özet doğru kalem sayısı ve toplamı gösteriyor
- [ ] Boş açıklamayla "Yapılanlara geç" → balon + adım 1'de kalır
- [ ] Satırsız "Teslime geç" → balon + adım 2'de kalır
- [ ] Şeritteki tamamlanmış adımlarda onay işareti var
- [ ] Kayıttan çıkıp geri gelince aynı adım açılıyor
- [ ] Tutarı olan parça satırında "müşteri getirdi" bağlantısı yok
- [ ] "müşteri getirdi" işaretli satır toplama girmiyor, tutar hücresi —
- [ ] İşaret geri alınınca yazılı tutar geri geliyor
- [ ] Servis kayıtları listesinde hiçbir yerde "parçayı müşteri getirdi"
      yazmıyor

---

## 3 · AÇIK

| No | Soru |
|---|---|
| **A-M23** | Tutarı yazılmış satır *"müşteri getirdi"* yapılamıyor — önce tutar silinecek. Kabul mü? |
| **A-M24** | Üç adım doğru bölünme mi, yoksa "Yapılanlar" kendi içinde ikiye mi ayrılmalı (parça / işçilik)? |
