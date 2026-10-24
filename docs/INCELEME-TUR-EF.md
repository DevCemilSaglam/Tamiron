# Tur E + F — inceleme rehberi (F8 · 24.10.2026)

> Tur E: Cemil'in tur D cevapları (K-077) — ses çözümleyicisi cihazda, ödeme düzeltme, Gemini
> okuyucusu, plaka kuralı. Tur F: **elden taksit = kısmi tahsilat** (K-078) — prototipten
> sunucuya, göçle birlikte. Kurallar aynı: önce prototip (şartname), önce test, mutasyon,
> eşdeğerlik 0 fark; para harcayan, dışarıya veri gönderen, makine ayarı değiştiren adım YOK.
>
> **Karar bekleyen her şey §1'de, tek listede.** Gerisi okumak için.

---

## 1 · Karar bekleyenler — TEK LİSTE

### 1a · Senin cevabın olmadan ilerlemeyen

| # | Soru | Benim önerim | Nerede |
|---|---|---|---|
| 1 | **Müşteri avansı** (*"hesabına önceden para verebilir"*): (a) kişiye mi araca mı bağlı (iki aracı olan müşteri)? (b) iş kapanınca avans **kendiliğinden** mi düşsün, usta mı seçsin? (c) kullanılmayan avans iade edilirse kasadan çıkış mı? | (a) aracın sahibine (müşteri araçtan türüyor, A-M1) · (b) usta seçer — "avanstan düş" teklif edilir, kendiliğinden düşmez · (c) evet, kasadan çıkış | TASARIM-E6 §6 · K-078 |
| 2 | **Prototipte teslimden sonra satır değişince "alınan" da değişiyor** (tam ödemede ödeme türetiliyor). Sunucu tasarıma uyuyor: alınan sabit, kalan değişir. Prototip de böyle olsun mu? | Evet — prototipte tam ödeme de teslimde `odemeler`e yazılsın | K-078.4 |
| 3 | **Prototip Netlify'a yayınlansın mı?** Yayınlanmamış değişiklikler: plaka kuralı (E7), sesle ödeme düzeltme (E5), elden taksit — teslimde "Bir kısmını aldım", kayıtta tutarlı "Ödeme alındı", sesle "2 bin aldım" (F1/F7), CSV alacak sırası | Evet — telefonda denemen için; yayın herkese açık | — |
| 4 | **Gemini ile ilk gerçek ruhsat okuması** (para harcar): bir ruhsatla, sonucu ölçüm setindekiyle karşılaştırırım | Anahtarı ortam değişkenine koyduğunda söyle | K-077.5 |
| 5 | **CI'ı bir kez elle koştur** — Node 20 uyarısı giderildi (`d45d603`), uyarı sayısı 0 olmalı | — | K-076.11 |

### 1b · Verdiğim kararlar — itirazın yoksa kalır (Cemil onayına açık)

| K | Karar | Neden |
|---|---|---|
| **78** | Kısmi tahsilat koda geçti, **avans ayrı ve sonra** | Dört cevabın verildi; avans yeni kavram, soruları açık. Tasarım avansı kapatmıyor (aynı ödemeler listesine bir yol olarak girer) |
| 78.1 | Ödeme satırı **teslimde mi alındı** bilgisini tutar | "Sonradan tahsil edildiyse önce tahsili geri al" kuralı kısmi tahsilatta ancak bununla korunur |
| 78.3 | **0 ₺'lik işte tahsil edilecek kalan yok** | Prototip öyle (`veresiyeMi`); eskiden 0'lık "tahsilat" kasaya yazılabiliyordu |
| 78.6 | CSV'de **alacak satırları teslim günü sırasıyla** — prototip düzeltildi | Prototip rastgele sırayla yazıyordu; muhasebeci tarih sırasıyla okur |
| 78.7 | Sesle "x aldım": **yalnız kalanı olan iş**; plakayla her ekranda; **yol söylenmezse onay ekranı sorar**; muhasebe ekranında gider sanılmaz | "Her yerde olabilir"; açık kayıtta "balata aldım 500" parçadır |
| 77.2 / E5 | Sesle ödeme düzeltme: **veresiye hedef değil**; "nakit değil kartla" → kart (teslimde de) | Veresiyeye "havaleye çevir" geriye dönük kasaya para yazardı — o tahsilattır |

### 1c · Tur D'den — açıkça "tamam" demediğin altı karar

Cevaplarında (K-077) bunlara değinmedin. İtirazın yoksa kalıyorlar; ikisi prototiple çelişiyor.

| K | Ne | Çelişki |
|---|---|---|
| 76.13 | **Teslimi geri alma** — yalnız aynı gün | ⚠️ Prototip "teslim geri alınamaz" diyor |
| 76.12 | **Kayıt silme** (yanlış plakayla açılan) — teslimli silinmez | Prototipte yoktu (yeni özellik) |
| 76.2 | Parçacının verdiği fiyat = ustanın **alışı** | Prototip tek kutuya yazıyordu; veri modeli kazandı |
| 76.4 | Mesaj önerisinde "şu anki iş" = en yeni açık kayıt | ⚠️ Prototip ilk bulduğunu alıyordu |
| 76.8 | Ruhsat okuması sahip adını/telefonunu **doldurmaz** | Maskeleme sorusu (K-070.5) okuyucu bağlanınca |
| 76.14 | Ses çözümleme **cihazda** — K-077.1'de "Evet" dedin | ✓ kapandı (listede kalsın diye) |

### 1d · Sonraya bıraktıkların — hatırlatma

- **SMS paketi + şirket**: ustalar kullanmaya başlayınca. Gönderici adı, İYS ve fatura şirket ister;
  şahıs mı limited mi → muhasebeci (sorular `arastirma/KENDI-SUNUCU.md` §7.8). K-077.8 önlemleri yazılı.
- **Yedek** zamanlama + uzak kopya (K-077.6) · **alan adı** · **ForwardedHeaders** (sunucu kurulunca).

---

## 2 · Ne yapıldı

| İş | Ne | Commit |
|---|---|---|
| E1 | Ödeme yolunu düzelt (dokunarak) — `POST /api/records/{id}/payment` | (tur E) |
| E2 | Gemini ruhsat okuyucusu — ölçülen istekle birebir, **bağlı değil** | f9bd92b |
| E3/E4 | Ses çözümleyicisi C#'ta, cihazda — prototiple 0 fark | de0934c |
| E7 | Plaka kuralı: harf sayısı rakam sayısını sınırlar (prototip + sunucu) | 1ec2c5d |
| E5 | Sesle ödeme düzeltme — önce prototip, sonra C# | fc7dbd9 |
| — | CI: Node 20 uyarısı (depo açılınca okunabildi) | d45d603 |
| F1+F2 | Prototipte ödemeler listesi; `Payment` veri modelinde | 2c77fdf |
| F3+F4 | Sunucuda ödemeler + var olan veriye ödeme satırı üreten göç | 64be054 |
| F5+F6 | Kasa/alacak ödemelerden; `deliver`/`collect` tutar alır; CSV her ödeme ayrı satır | 8ba4b55 |
| F7 | Sesle "2 bin aldım" — prototip + C# | 8bdbebd |

**Usta açısından değişen** (prototipte görünür, Blazor arayüzünde kayıt ekranı henüz yok):
- Teslimde tek yeni çip: **"Bir kısmını aldım"** → tutar + yol. Toplamdan fazlası yazılırsa araç
  teslim EDİLMEZ, yazılan silinmez.
- Kayıtta "Ödeme alındı" alanı **kalanla dolu** gelir: tamamını alan bugünkü gibi yalnız çipe
  dokunur; bir kısmını alan rakamı değiştirir. "Geri al" son taksiti siler.
- Muhasebe: kasa o gün alınan para; alacak = kalan ("1.000 ₺ alındı" alt satırı); CSV'de her ödeme
  kendi gününde ayrı satır.
- Sesle: "06 TMR 07 için 2 bin aldım nakit", kayıt açıkken "kalanını kartla aldım".

---

## 3 · Nasıl denenir

```bash
dotnet test Tamiron.sln
```

**Prototip** (yerel): `.claude/launch.json` → `tamiron` → `http://localhost:8123/prototypes/omurga.html`.
06 TMR 07'nin veresiyesini aç → tutarı 1.000 yap → Nakit. Ya da mikrofon: "500 aldım".

**Eşdeğerlik:** `node tools/muhasebe-esdeger.js` (7 senaryo, 4'ü kısmi) · `node tools/ses-sema-dok.js` (913 durum).

**Göç:** geliştirme veritabanın `Payments` + `PaymentSequence` göçlerini API ilk açıldığında alır;
var olan ödenmiş kayıtlara ödeme satırı üretilir, kasa rakamı değişmez (testli).

---

## 4 · Ölçüm özeti

- **Testler:** 424 → **462** (Domain 274 · Api 168 · Arayüz 20), hepsi yeşil.
- **Eşdeğerlik:** muhasebe 7 senaryo bayt bayt 0 fark; ses 913/913 0 fark (815 eski durumun hiçbiri değişmedi).
- **Mutasyon:** E5 9/9 · F3 16/16 · F5 13/14 + 1 eşdeğer (EF `SUM`'ı zaten `COALESCE` ile sarıyor — üretilen SQL'den doğrulandı) · F7 11/11 (yeşil tabanda; ilk gerçek koşuda 4 kaçtı → 2 korpus cümlesi + 2 özel dünyalı test).
- **Ölçümün bulduğu hatalar** (her biri artık bir test):
  - F3 · teslim/tahsil/düzeltme uçları kaydı **satırsız** yüklüyordu → teslimde ödeme satırı
    hiç yazılmıyordu ve **hiçbir test görmüyordu**. Satırlar + ödemeler artık her yüklemede gelir.
  - F5 · "son taksiti geri al" **yanlış taksiti** siliyordu — veritabanı sırası yazılış sırası
    değil. `Payment.Sequence`.
  - F5 · prototip CSV'de alacakları rastgele sırayla yazıyordu (sunucu tarih sırasıyla).
  - F7 · sesle ödemede yol söylenmezse onay ekranı **sormuyordu** — yolsuz ödeme yazılırdı.
  - F7 · "06 TMR 07 07 aldım" → 7 ₺ (plakanın sonu iki kez duyulmuş).
  - E5 · veresiye kayda "havaleye çevir" geriye dönük kasaya para yazardı; "nakit değil kartla"
    teslimi **nakit** öneriyordu.
- **Kendi hatam:** F7'nin ilk mutasyon koşusu "11/11 yakalandı" dedi — taban zaten kırmızıydı
  (yalnız arayüz testlerini koşturmuştum, şema testi kırmızıydı). Mutasyon aracı artık önce tabanı
  ölçer, kırmızıysa koşmaz (K-078.8). Yeşil tabanda gerçek sonuç 7/11'di — dördü teste dönüştü.

---

## 5 · Bilinen boşluklar

- **Blazor arayüzünde** kayıt ekranı, teslim ve "Düzenle" yok — sunucu uçları hazır.
- **Avans** yok (1a-1).
- Ses: "iki bin lira" (sayı yazıyla) okunmuyor — tutar okuyucusunun eski sınırı, yeni değil.
- Ses: araçlar/kayıtlar/müşteriler ekranında plakalı **teslim** cümlesi aramaya düşüyor
  (bu ekranların kendi işi arama) — "2 bin aldım" için öncelikle aşıldı, teslim için aşılmadı.
