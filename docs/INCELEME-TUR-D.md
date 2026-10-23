# Tur D — inceleme rehberi (D12 · 23.10.2026)

> Cemil, 23.10: *"Karar verilmesi gerekenler için en iyi, tercih ettiğin kararı verebilirsin.
> … Sonrasında detaylı bir inceleme yapacağız."*

Bu belge o incelemenin **tek giriş noktası**. Tur D'de 19 iş yapıldı (D1–D11, D13–D20),
her biri ayrı commit, her lider kararı `KARARLAR.md` **K-076.1–76.19** altında gerekçesiyle
ve **"Cemil onayına açık"** olarak yazılı. Bir karar geri alınacaksa çoğu tek uç / tek yöntem.

**Kural boyunca korunan:** para harcanmadı, dışarıya veri gitmedi, makine ayarı değiştirilmedi.
SMS sağlayıcısı, ruhsat okuyucusu, CI, zamanlanmış yedek — kodu yazıldı, **koşturulmadı**.

---

## 1 · Önce bunlara bak — prototiple çelişen ya da kapsamı değişen kararlar

| # | Ne | Neden bakmalısın | Geri almak |
|---|---|---|---|
| **76.13** | **Teslimi geri alma** (D14) | Prototip açıkça *"teslim geri alınamaz"* diyor (50-teslim'in başı). Koruma (ayrı ekran) kaldı; arkasına **aynı gün** geri alma eklendi — yanlış araca "Nakit" basmak kasaya olmayan para yazıyordu ve D13'ten beri teslimli kayıt silinmiyor | `POST /records/{id}/deliver/undo` + `ServiceRecord.UndoDelivery` silinir |
| **76.14** | **Sesli komut sunucuda — kapsam DARALTILDI** (D15) | Kuyruk "cümle → işlem sunucuda" diyordu. Yapılan: çözümleme **cihazda kaldı**; sunucuya şemalar + sınama + 273 durumluk korpus geldi. Gerekçe: K-054'e göre model gelince yalnız tanıyıcı değişir — sezgiyi C#'a taşımak iki kez iş | İstenirse sezgisel motorun taşınması ayrı bir tur; korpus 0-fark hedefi olarak hazır |
| **76.12** | **Kayıt silme** (D13) | Prototipte yoktu. "Yeni özellik varsayılan olarak HAYIR"a rağmen girdi: yanlış plakayla açılan kayıt silinemezse "serviste" sayısında sonsuza dek yalan durur. Teslimli kayıt silinmez | `DELETE /records/{id}` + `/restore` silinir |
| **76.2** | **Parçacının fiyatı = ustanın ALIŞI** (D2) | Prototip tek "fiyat" kutusuna yazdırıyordu; veri modeli `Cost`'u *"parçacıdan gelen"* diye tanımlıyor — model kazandı. Müşteri fiyatını usta ayrıca yazar | `ApplyReply` fiyatı `Price`'a yazacak şekilde tek satır |
| **76.4** | **Mesaj önerisinde iki küçük sapma** (D4) | "Şu anki iş" = EN YENİ açık kayıt (prototip ilk bulduğunu alıyordu); marka/model boşsa yalnız plaka (prototip `" (34 ABC 123)"` basıyordu). Prototipin tuhaflığı **korundu**: o işe herhangi bir mesaj gittiyse "teslim edildi" önerilmez | — |
| **76.8** | Ruhsat okuması **sahip adını/telefonu doldurmaz** (D8) | Prototipin okuma eşlemesinde de yok; K-070.5 maskeleme sorusu okuyucu bağlanırken | — |

---

## 2 · Senin yapacakların — para, hesap, makine (hiçbiri yapılmadı)

| Ne | Neden bekliyor | Nerede |
|---|---|---|
| **Netgsm hesabı + onaylı gönderici adı** + sunucuda 3 ortam değişkeni (`Sms__Netgsm__UserCode/Password/Header`) | Para. Bağlanana kadar üretimde kod isteği 503. Biçim Netgsm'in resmi örneğinden; **gerçek hesapla hiç denenmedi** — ilk gönderimden önce panel belgesiyle karşılaştır | K-076.7 |
| **Ruhsat okuyucusu** (Gemini K-067.2c ya da kendi sunucu K-070) | Para + dış servis. `IRegistrationReader` arayüzü hazır, bağlı değil (503 "elle yaz") | K-076.8 |
| **CI'ı otomatiğe almak** (`.github/workflows/backend.yml` → `push` / `pull_request`) | Özel depoda Actions dakika harcar (Windows ×2). Şimdi yalnız elle tetiklenir; **GitHub koşucusunda hiç koşmadı** — ilk tetiklemede LocalDB adımına bak | K-076.11 |
| **Yedek betiğini zamanlamak** (Görev Zamanlayıcısı → `tools\yedek.ps1 -IzTemizle`) ve **uzak kopya** | Makine ayarı; uzak kopya para/karar. Yedek şimdi aynı diskte | K-076.18 |
| **Ters vekil arkasında `ForwardedHeaders`** | Yoksa IP hız sınırı herkese ortak olur — sunucu kurulurken | K-076.10 |
| **Alan adı** `tamiron.app` (özet linki) | Alınmadı; link biçimi prototipinki | K-076.3 |
| **Geliştirme veritabanını sıfırlamak** (tam tohumu görmek için) | Senin veritabanın. Komut: `dotnet tool run dotnet-ef database drop --project src/Tamiron.Infrastructure --startup-project src/Tamiron.Infrastructure`, sonra API'yi başlat | K-076.15 |

---

## 3 · Bütün kararlar — tek satır

| K | İş | Karar | Commit |
|---|---|---|---|
| 76.1 | D1 Parçacılar | Ad + telefon zorunlu; numara dükkân içinde tekil; silme soft | 72e94c3 |
| 76.2 | D2 Parça sorma | Parçacı başına ayrı soru; K-068 kapısı prototip sırasıyla; cevap ham metniyle; fiyat = alış; sorulmamış satıra cevap yok; A-24 kesinleşti | 0dfc68e |
| 76.3 | D3 Özet linki | 16 karakter rastgele kod (~79 bit), giriş yok, kod değişmez; A-M9 kapandı | 86df765 |
| 76.4 | D4 Mesaj önerileri | Öneri = metin, gönderim değil; kural kayıt bazlı; linkli metin özet kodunu doğurur | e86bc90 |
| 76.5 | D5 İzin geçmişi | Araç başına iz; "geri al" eski tarihi İZDEN (son eylem, aynı gün, bir kez); K-073.10 kapandı | 2b6fba7 |
| 76.6 | D6 Yeni dükkân | Numara → kod → dükkân adı; dükkân+kullanıcı+oturum tek işlemde; kayıtlı numara kayıt olamaz | a58e63e |
| 76.7 | D7 SMS | Netgsm XML ucu; ortam değişkeni; geliştirmede HEP günlük; ret → 503 + kod | a83e299 |
| 76.8 | D8 Ruhsat | K-065/K-069 kuralları API'de; çatışmada fotoğraf yeniden okunmaz; fotoğraf saklanmaz | 09a16b5 |
| 76.9 | D9 İz | Toplu gönderim; prototipin temizliği; tanınmayan tür düşer ve sayılır; 90 günde gerçek silme; okuma ucu yok | bdb4a98 |
| 76.10 | D10 Güvenlik | Oturumsuz uçlarda IP sınırı (giriş 10/dk, özet 60/dk); CORS yalnız yapılandırılan; `/health` | f880739 |
| 76.11 | D11 CI | Windows + LocalDB; eşdeğerlik ve şema güncelliği; yalnız elle | b4f8956 |
| 76.12 | D13 Kayıt silme | Soft + geri al; teslimli silinmez | 6092e79 |
| 76.13 | D14 Teslimi geri al | Aynı gün; önce tahsil; hatırlatma gittiyse yok ⚠️ prototiple çelişir | 5656a48 |
| 76.14 | D15 Sesli komut | Şema + sınama + korpus sunucuda; çözümleme cihazda ⚠️ kapsam daraltıldı | 0c45a36 |
| 76.15 | D16 Tohum | Tek eşleyici (eşdeğerlik testi de kullanır); tarihler bugüne kayar | cc3fbc7 |
| 76.16 | D17 İndeksler | Üç indeks + COALESCE'siz tahsil kuralı (Scan → Seek ölçüldü); göçsüz model değişikliği kırmızı | e33f2bf |
| 76.17 | D18 OpenAPI | Yalnız geliştirme/test; bütün uçlar belgede (testli) | a38d254 |
| 76.18 | D19 Yedek | Doğrulanan yedek; dar temizlik (en az 3 kalır); iz temizliği; dışarı gönderim yok | adbc97e |
| 76.19 | D20 Arayüz | Ui.Core + Tamirhane.Ui + Tamirhane.Web; giriş + kayıtlar; prototipin stilleri | 462d160 |

---

## 4 · Nasıl denenir

**Testler:**

```bash
dotnet test Tamiron.sln
```

**Arayüz uçtan uca** (iki terminal ya da `.claude/launch.json`'daki `api` ve `tamirhane-web`):

```bash
dotnet run --project src/Tamiron.Api --launch-profile http
```

```bash
dotnet run --project src/Tamiron.Tamirhane.Web --launch-profile http
```

`http://localhost:5213` → telefon **0532 000 00 00** → "Kod gönder" → kod **API'nin konsolunda**
(*"SMS (geliştirme, gönderilmedi)"*) → kayıtlar. Sayfayı yenile: oturum kalır.

**Uçları elle:** `src/Tamiron.Api/Tamiron.Api.http`; bütün uçların listesi geliştirmede
`http://localhost:5080/openapi/v1.json`.

**Prototip ↔ API eşdeğerliği:** `node tools/muhasebe-esdeger.js` · `node tools/ses-sema-dok.js`
— ikisi de deterministik; değişmemiş prototipte dosyalar aynı çıkar (CI bunu sınar).

---

## 5 · Ölçüm özeti

- **Testler:** tur başında 241 → şimdi **392** (Domain 228 · Api 147 · Arayüz 17), hepsi yeşil.
- **Mutasyon:** her işte kuralı bozan mutasyonlar koşturuldu. 19 işte toplam **~150 mutasyon**;
  kaçanların her biri ya teste dönüştü ya eşdeğer olduğu gösterildi (D8'de 1 eşdeğer).
- **Ölçümün bulduğu açıklar** (her biri artık bir test):
  - D3 · silinmiş kaydın linki açılıyordu (test yoktu).
  - D13 · başka dükkânın silinmiş kaydını geri getirme isteği **404 dönüyordu ama kayıt geri
    geliyordu** — test yanıta bakıyordu, yan etkiye değil.
  - D17 · indeksi modelden silmek hiçbir testi kırmızıya çevirmiyordu → `MigrationsUpToDateTests`:
    göçü unutulmuş model değişikliği artık kırmızı.
  - D17 · indeks eklendi ama kasa sorgusu onu **kullanamıyordu** (COALESCE) — plan ölçülünce
    görüldü; kural indekslenebilir biçime yazıldı (anlam aynı, testli).
  - D19 · "betik doğruluyor mu" testi açıklamadaki sözü buluyordu → yalnız kodda arar.
  - D20 · **tarayıcıda** İngilizce dilde tutar "3,450 ₺" çıkıyordu (WASM kültür verisi);
    sunucudaki testler göremezdi.
- **Kendi hatalarım:** D17 satırına test sayısını 372 yazdım, 371'di — D18'de düzeltildi.
  Birkaç işte test kodla aynı turda yazıldı ve kırmızı koddan önce görülmedi; o işlerde
  testlerin kuralı tuttuğu **mutasyonla** ölçüldü (commit mesajlarında yazılı).

---

## 6 · Bilinen boşluklar (bilerek bırakılan)

- **Kayıt kartında** motor, renk, aranacak telefon, "fiyat bekleniyor" yok — `ListRecords`
  yanıtı genişleyince (D20).
- **Arayüzde** yalnız giriş ve kayıt listesi; kayıt ekranı, arama, süzgeç sonraki tur.
- **Sesli komutun** sunucu çözümleyicisi yok — model kararı bekleniyor (76.14).
- **İz özeti** (A-M42, "hangi adımda vazgeçiyorlar") yok — iz birikiyor, okuma ekranı ayrı iş.
- **İznin nasıl alındığı** (sözlü/yazılı/SMS) alanı yok — İYS ile.
