# Tur G — inceleme rehberi (G12 · 27.10.2026)

> Tur G: **yerelde ana yol** (K-079.3). Backend artık temel (K-079.2); her ekran Blazor'da, kurallar
> API'de. Kurallar aynı: önce test (kırmızıyı gör), mutasyon, tarayıcıda uçtan uca; para harcayan,
> dışarıya veri gönderen, makine ayarı değiştiren adım YOK, Netlify'a yayın YOK.
>
> **Karar bekleyen her şey §1'de, tek listede.** Gerisi okumak için.

---

## 1 · Karar bekleyenler — TEK LİSTE

### 1a · Senin cevabın olmadan ilerlemeyen

| # | Soru | Benim önerim | Nerede |
|---|---|---|---|
| 1 | **Ses (G13):** satır mikrofonları ve sesli işlem. Tarayıcının konuşma tanıması (Chrome) sesi Google'a gönderir. Hangi sağlayıcı — tarayıcınınki mi, bir ses servisi mi (para), yoksa ses şimdilik yok mu? | Yerelde bitiş için **şimdilik yok**; ustaya gösterilince ihtiyaç ölçülsün. Çözümleyici hazır (D15), yalnız "kulak" eksik | K-080.3 · K-067 · G13 |
| 2 | **Telefondan erişim:** uygulama yalnız bu bilgisayarın tarayıcısında açılıyor (`localhost`). Aynı Wi-Fi'deki telefondan açmak için ağa dinleme + **Windows güvenlik duvarı kuralı** gerekir (makine ayarı) | Usta telefonla kullanacaksa gerekli; kuralı sen ekle ya da "ekle" de, adımları yazayım | K-080.2 |
| 3 | **90 günlük silme betiği zamanlansın mı?** Ruhsat okuma ayrıntısını 90 günden sonra silen betik var ama elle çalışıyor. Zamanlamak = Windows görev zamanlayıcısı (makine ayarı). Zamanlanırsa Ayarlar'daki gizlilik cümlesine "90 gün saklanır" geri gelir | Evet — söz verdiğimiz şeyi kendiliğinden yapsın | K-086 · K-053.4 |
| 4 | **Teslimi geri alma** (yalnız aynı gün): ucu sunucuda hazır, ekranda düğmesi yok. ⚠️ Prototip "teslim geri alınamaz" diyordu | Evet, "Düzenle" altında — yanlış teslim gerçek bir hata, aynı gün sınırı kasayı korur | K-076.13 · K-080.8 |
| 5 | **Toplu mesaj (bayram):** sunucu hazır (yalnız izinliye), ekranı yok. WhatsApp'ta toplu gönderim usta için **her kişiyi tek tek açmak** demek | Ustaya sor: kaç müşteriye atıyor? 10'dan azsa tek tek liste; çoksa SMS paketi gerekir (para) | K-084 |

### 1b · Verdiğim kararlar — itirazın yoksa kalır (Cemil onayına açık)

| K | Karar | Neden |
|---|---|---|
| **80** | Yerel kurulum: tek komut (`tools\yerel-baslat.ps1`), tek adres, **giriş kodu sunucu penceresinde** (SMS paketi yok), boş `TamironYerel` | Usta tek şeye tıklasın; SMS para |
| **81** | Avans **sahibe göre** (ad + telefon); aynı adın farklı telefonu ayrı müşteri | Müşteri araçtan türüyor (A-M1); birleştirme yok |
| 81 | **Avanstan düşülen ödemenin yolu düzeltilmez** — geri al, yeniden yaz | Nakit ↔ avans çevirmek geriye dönük kasa ve bakiye değiştirir |
| **82** | Araç ekranında not **alandan çıkınca kaydolur**, ayrı düğme yok | Bir dokunuş az; istenirse "Kaydet" eklenir |
| **83** | Müşteri detayının adresi `/musteri?key=` — **telefon tarayıcı geçmişinde görünür** | Müşteri tablosu yok; yerelde dışarı çıkmaz. Sunucuya geçilirse kısa kimlik |
| 83 | Araç ekranından müşteriye bağlantı **yok** | Araç ekranı müşteri anahtarını bilmiyor; istenirse API verir |
| **84** | Mesaj: önce kayıt, sonra **ustanın kendi WhatsApp'ı metin hazır açılır**; ekran "gönderildi" demez | Gönderen usta (WHATSAPP.md A yolu) — para yok, dış servis yok |
| 84 | **"Ara" da mesaj arşivine düşer** ("Arandı") | ⚠️ Prototip aramayı arşive yazmıyordu; sunucu hatırlatmayı aramayla da kapatıyor |
| **85** | Parça sor: **ilk parçacının WhatsApp'ı açılır, ötekiler kendi düğmesiyle** | Tarayıcı tek dokunuşta tek pencere açtırır — prototipin "sırayla açılır"ı telefonda mümkün değil |
| 85 | Parça sor ekranında **"Ruhsatı çek" yok** (kayıt açarken var) — eksik kimlik elle yazılır | Bir kopya ekran parçası az; istenirse eklenir |
| **86** | Ayar "alış sorulmasın"ken **"parçacı 1.200 ₺" okunur kalır** | ⚠️ Prototip gizliyordu; o rakamı usta parça sorda kendisi yazdı, gizlemek bilgiyi kaybettirir |
| **88** | Çekmecede **Parçacılar** (prototipte yoktu — liste sabitti) | Ana menüye satır eklenmedi (A-M8) |
| 88 | Üst çubukta **geri/ileri yok**, alt gezinme yok, "Plaka ara" yok | Telefonun ve tarayıcının kendi geri tuşu var; her listenin arama kutusu var |

### 1c · Bu turda bulunan, kararı sende olan bir hata

- **"Kaya & Oğulları Bey/Hanım"** — mesaj hitabı firma adını kişi sanıyor (`MessageText.IsFirm` "Oğulları"nı
  tanımıyor). Firma sözcükleri listesi genişletilsin mi (Oğulları, Ticaret, Ltd, A.Ş., Otomotiv …)?
  Önerim: evet, liste + test; tek satırlık iş.

### 1d · Önceki turlardan hâlâ açık (hatırlatma)

- **Tur D 1c:** 76.12 kayıt silme (yeni özellik) · 76.2 parçacı fiyatı = alış · 76.4 mesajda "şu anki iş" =
  en yeni açık kayıt · 76.8 ruhsat okuması sahip adını doldurmaz. İtirazın yoksa kalıyorlar.
- **Sonraya bıraktıkların:** SMS paketi + şirket · yedek zamanlama + uzak kopya · alan adı ·
  ForwardedHeaders · Gemini ruhsat okuması varsayılan kapalı (`Gemini__Enabled`, ruhsat başına yarım kuruş).

---

## 2 · Yerelde nasıl açılır

```
powershell -ExecutionPolicy Bypass -File tools\yerel-baslat.ps1
```

Tarayıcıda `http://localhost:5090` açılır. İlk kez: **"Dükkânımı kaydet"** (ad + telefon). Giriş kodu SMS ile
gelmez — **sunucu penceresinde yazar** ("SMS GÖNDERİLMEDİ (paket yok) — kod burada → …").

Veritabanı `TamironYerel` (geliştirmenin `TamironDev`'inden ayrı). Bu turda denemelerde oluşan veriler orada:
34 ABC 123 (Renault Clio, sahibi "Kaya & Oğulları", 0532 000 00 01), iki servis kaydı, "Deneme Parça"
parçacısı, bir avans. Gerçek veri değil — silmek istersen veritabanını sil, açılışta boş kurulur.

---

## 3 · Ne yapıldı — 16 commit

| İş | Ne | Commit |
|---|---|---|
| G1 | Tek komut + giriş/dükkân kaydı + ana menü | f68ab12 |
| G2 | Kayıt aç: plaka sorusu API'de, yeni araç kayıtla tek istekte | 15c0811 |
| G3 | Servis kaydı: satırlar, toplam, bekliyor, açıklama | 7237e0b |
| G4 | Teslim: beş yol, kısmi tutar, hatırlatma önceden | 6f8e51f |
| G5 | Kayıtta ödeme: alınanlar, kalan dolu tahsilat, geri al, "Düzenle" | 6f4ef3a |
| G6 | Muhasebe: dönem, kasa, gider, alacak, CSV | 4306282 |
| G2b | Ruhsatı çek sekmesi (okuyucu kapalıyken "elle yaz") | 6d5fb40 |
| G7–G8 | Müşteri avansı: tasarım, sunucu, ekran | 20bf176 · 69e47fc |
| G9a | Araç ekranı + araçlar listesi | 16290c7 |
| G9b | Müşteriler + müşteri detayı + ileti izni | 717b4c6 |
| G9c | Müşteriye mesaj (WhatsApp/SMS/Ara) | c71ee90 |
| G10a | Parçacılar + parça sorusu önizlemesi | 2e5ff68 |
| G10b | Parça fiyatı sor + cevabı kayda yazma | 1393d6e |
| G11a | Ayarlar + kayıtta alış alanı + sayfa önbelleğe takılmaz | b18d929 |
| G11b | Kabuk: rozetli menü, çekmecede hatırlatmalar | 8495fd3 |

**Ölçüm:** 683 test yeşil (domain 288 · arayüz 193 · API 202). Her iş mutasyonla ölçüldü — bu turun ikinci
yarısında (G9a–G11b) son hâlinde 86 mutasyonun 86'sı yakalandı; ilk koşuda kaçan 5 boşluk önce testle
kapatıldı.

**Tarayıcıda bulunan ve testle kapanan** (bunlar birim testten kaçmıştı):
- Hata bildirimi ekranın başındaydı — telefonda dipteki düğmeye basan görmüyordu (parça sor).
- Cevap kutularının etiketleri bağlı değildi (ekran okuyucu).
- `index.html` önbelleğe takılıyordu — güncellemeden sonra **eski sayfa** açılıyordu (K-087).
- Stil dosyasında olmayan bir düğme türü ("btn-danger") renksiz kalırdı — düğme testi artık türün tanımlı
  olmasını istiyor.
- Adında "&" olan müşteri ("Kaya & Oğulları") adresi bölüyordu.

---

## 4 · Usta açısından — ana yol

Menüden (ya da çekmeceden) **Servis kaydı ekle** → plaka → **Kaydı aç** → satırlar (parça / işçilik) →
gerekirse **Parça fiyatı sor** (parçacıya WhatsApp, gelen fiyatı yaz) → **Teslim et** (yol + istersen kısmi) →
müşteriye **Mesaj** (özet, "aracınız hazır"). Bakımı gelen araç menü düğmesinin rozetinde; dokununca
hatırlatma mesajı hazır açılır. Muhasebe: bugün/ay/yıl kasa, gider, alacak, CSV.
