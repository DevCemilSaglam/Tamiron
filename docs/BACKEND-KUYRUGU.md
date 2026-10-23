# TAMIRON — BACKEND KUYRUĞU VE ÇALIŞMA DÖNGÜSÜ

> **22.10.2026 · K-073.** Cemil: *"Sen devam et. Birkaç saat sonra gelince kontrol
> edeceğim. Bu esnada kendine bir döngü yaz ve onu uygula."*
> Bu dosya döngünün **kuralı** ve **sırası**. Her tur buradan okur, buraya yazar.

---

## Döngü — her tur

1. **Oku:** bu dosyada ilk `SIRADA` işi bul. Yoksa → *Durma koşulları*.
2. **Şartnameyi bul:** prototipteki fonksiyon/ekran (`prototypes/kaynak/**`) +
   `VERI-MODELI.md` + ilgili K-maddesi. Prototipin davranışı testin beklenen değeridir.
3. **Önce test** (MIMARI §4.4): domain testi (saf) ve gerekiyorsa API testi (LocalDB).
   Kırmızı koştuğunu gör.
4. **Kod:** kural domain'de; Application ince (MediatR); API yalnız uç + eşleme.
5. **Ölç:** `dotnet test Tamiron.sln` hepsi yeşil. İşin **ana kuralı** bilerek
   bozulur → en az bir test kırmızı → geri alınır → yeşil (mutasyon).
6. **Belge:** yeni alan/tablo → önce `VERI-MODELI.md`. Yeni karar → `KARARLAR.md`
   (K-073'ün alt maddesi ya da yeni K). Bu dosyada işi `BİTTİ` + tek satır sonuç.
7. **Commit + push** — ne yapıldı, NEDEN, ölçüm. Bir iş = bir commit.
8. Sonraki tura geç.

## Durma koşulları — döngü kendini durdurur

- Kuyrukta `SIRADA` iş kalmadı.
- Bir iş **Cemil kararı** gerektiriyor ve varsayım işi çöpe çıkarabilir →
  `BEKLİYOR` yaz, bir sonraki işe geç; hepsi bekliyorsa dur.
- Aynı iş iki turda yeşile dönmedi → `TAKILDI` + neden yaz, sonraki işe geç.
- Para harcayan, dışarıya veri gönderen ya da makinenin ayarını değiştiren her
  adım → **yapılmaz**, `BEKLİYOR`.

## Dokunulmayacaklar (Cemil'de)

- **Oturum açma yöntemi (K-073.6)** — SMS kodu / parola. Kiracı başlıktan kalır.
- Yapay zekâ / ses / ruhsat okuma sağlayıcıları (K-067, K-070 — kenara kondu).
- WhatsApp/SMS **gerçek gönderimi** (dış servis, para).

---

## Kuyruk

| # | İş | Şartname | Durum |
|---|---|---|---|
| B1 | Domain + tahsil + kasa özeti + kiracı + soft-delete | K-071.1 · K-073.5 | **BİTTİ** (`5219db6`) — 27 test, 3 mutasyon |
| B2 | **Ana yol: kayıt aç** — plakayla araç bul ya da oluştur, kayıt aç; plaka biçimi | prototip `20-ruhsat` · `plakaBicim` · K-059 · K-068 (kayıt açmak kimlik istemez) | **BİTTİ** — `POST /api/records`; `Plate`, `ServiceTypeGuess` domain'de. 50 test, 4/4 mutasyon. Not: aynı aracın ikinci açık kaydı engellenmiyor (prototip de engellemiyor) |
| B3 | **Satırlar** — ekle · düzenle · sil (soft) · kim getirdi · alış null≠0 | `30-kayit` · K-037 · K-057 | **BİTTİ** — ekle/güncelle (tür çevirme dahil)/sil/geri al uçları; `ServiceLine.Position` (VERI-MODELI'ne önce yazıldı) + göç. 59 test, 5/5 mutasyon |
| B4 | **Kayıt bayrakları** — tür (elle), şikâyet, parça bekliyor (+tarih), müşteriye göster | `30-kayit` · K-034 · A-M15 | **BİTTİ** — `PATCH /api/records/{id}` (null = dokunma); tür tahmini domain'e taşındı (kurucu + `SetComplaint`), km biçimi domain'de. 72 test, 6/6 mutasyon |
| B5 | **Teslim** — ödeme yolu + bakım hatırlatma tarihi (ay VEYA km, hangisi önce) | `50-teslim` · `hatirlatmaHesapla` · A-3 | **BİTTİ** — `POST .../deliver`, `GET .../delivery-preview`; `MaintenanceReminder` domain'de, dükkânın periyodu + aracın km geçmişi. Sapma: ay sonu sabitlenir (K-073.9). 83 test, 6/7 mutasyon + 1 eşdeğer. Açık: teslim öncesi "eksikler" uyarısı (kEksikler) — B7'de listelerle |
| B6 | **Araç** — oku/güncelle (ruhsat alanları), plaka değişimi, parça-sor kimlik kapısı | `60-arac` · K-033.2c · K-068 | **BİTTİ** — `GET /api/vehicles/{id}`, `GET /api/vehicles?plate=`, `PATCH /api/vehicles/{id}` (null = dokunma, "" = temizle); `VehicleFields` (tel · kod · yıl), `MissingForParts`. 106 test, 8/8 mutasyon. Not: kırmızı aşaması ayrıca koşulmadı, mutasyonlar kapattı. Parça sorma kapısının uygulanması parça-sor uçlarıyla gelir (kuyrukta yok — öneri) |
| B7 | **Listeler** — serviste olanlar, araçlar, kayıtlar (serviste üstte) | `70-araclar` · `80-kayitlar` | **BİTTİ** — `GET /api/records?q=&inShop=`, `GET /api/vehicles/list?q=`; `TextSearch` (Türkçe katlama), tür adları; kayıtta teslim eksikleri (`Warnings`, kEksikler — B5'ten kalan). 123 test, 9/9 mutasyon. Arama bellekte (bir dükkân ölçeği); büyürse katlanmış arama sütunu |
| B8 | **Gider** (`Expense`) — ekle/sil, dönem toplamı | `95-muhasebe` · K-022 | **BİTTİ** — `GET/POST /api/expenses`, sil/geri al (dönemin güncel listesini döner); `Expense` tablosu + göç. **Kasa özetinden gider düşülmez** — kuyruktaki "kasa özetine gider" yanlıştı: "Kalan" satırı K-014'te bilerek kaldırılmıştı. 130 test, 6/6 mutasyon (geri almadaki kiracı denetimi dahil) |
| B9 | **Alacak listesi** + **CSV dışa aktarma** | `95-muhasebe` · K-071.2 | **BİTTİ** — `GET /api/receivables` (döneme tabi değil), `GET /api/export/cash.csv?period=` (BOM'lu UTF-8 indirme); `Csv` domain'de. 144 test, 8/8 mutasyon (biri ilk denemede tesadüfen ayırt edilemedi, gerçekçi hâliyle yeniden koşuldu). Yan bulgu: prototipte görünmez BOM karakteri kaçışa çevrildi |
| B10 | **Müşteriler** (araçtan türetilir) + **ileti izni** | `musteriler()` · K-071.3 | **BİTTİ** — `GET /api/customers?q=`, `GET /api/customers/detail?key=`, `PUT /api/customers/consent`; `Customers`/`Customer` domain'de (Türkçe alfabe, serviste üstte). 153 test, 8/8 mutasyon. Sapma: izni geri almak eski tarihi getirmez (K-073.10) |
| B11 | **Mesaj arşivi** (`Message`) + toplu alıcı süzgeci (izinliler) — gönderim YOK, kayıt | K-048.2 · K-071.3 | **BİTTİ** — `POST/GET /api/messages`, `GET /api/messages/bulk-preview`, `POST /api/messages/bulk` (yalnız izinliler, kişi başına kayıt, kendi hitabıyla); `Message` tablosu + göç; hedef/hitap/imza domain'de. **Gönderim yok, kayıt.** 165 test, 9/9 mutasyon. Test saati artık ilerliyor (sabit saat "en yeni üstte"yi sınanamaz yapıyordu) |
| B12 | **Hatırlatması gelenler** sorgusu | `hatirlatmasiGelenler` | **BİTTİ** — `GET /api/reminders/due` (en çok bekleyen üstte, gecikme günü, aranacak telefon); "hatırlatma" şablonlu mesaj — **arama dahil** — aracın açık hatırlatmalarını kapatır. 168 test, 6/6 mutasyon |
| B13 | **Oturum açma — SMS kodu, oturum çıkışla biter** | K-074 | **BİTTİ** — `POST /api/auth/code` · `/verify` · `/logout` · `/logout-all` · `GET /api/auth/sessions`; `User` · `LoginCode` · `Session` + göç; `X-Tenant-Id` kaldırıldı, dükkân oturumdan. 182 test, 9/9 mutasyon ("oturum 30 günde düşsün" mutasyonu da kırmızı → Cemil'in kararı testle korunuyor). Geliştirmede uçtan uca denendi |
| — | *— Muhasebe turu (C) · 22.10 · Cemil: "Muhasebeyi bitir. … 10-15 farklı işlemi benden karar beklemeyen, bitir." Kapsam: MUHASEBE-KAPSAM-OTOMIX Faz 1 (K-064.3b onaylı) —* | | |
| C1 | **İş başına fark** — satış − alış; alışı bilinmeyen parça SATIŞIYLA hesap dışı, sayısı söylenir; dönem toplamı | `isKari` · 96-is-basina · K-057 | **BİTTİ** — `GET /api/profit?period=`; `JobProfit` domain'de (prototip örnekleri birebir: 700 ₺ + 1 bilinmeyen, 1.350 ₺). 188 test, 6/6 mutasyon |
| C2 | **Parça alışları** — dönemde teslim edilen işlerde tamircinin aldığı, alışı yazılı parçalar + toplam | `parcaAlislari` | **BİTTİ** — `GET /api/part-purchases?period=`; kural `ServiceLine.IsShopPurchase` domain'de. 193 test, 4/4 mutasyon. Sıra: en yeni teslim üstte (prototipte kayıt sırasıydı) |
| C3 | **Ana ekran özeti** — bugün tahsilat · veresiye toplamı · serviste · müşteri sayısı · hatırlatma rozeti | 10-home · çekmece | **BİTTİ** — `GET /api/home`; her rakam kendi kuralıyla (CollectedOn, Unpaid bakiye, IsReminderDue, Customers.From). 196 test, 4/4 mutasyon — biri ilk koşuda yakalanmadı (tohumda araç = müşteri = 3, tesadüf), ayırt eden test eklendi |
| C4 | **Araç ödeme durumu** — aracın ödenmemiş işleri ve toplamı | 60-arac `arOdeme` | **BİTTİ** — yeni hesap YAZILMADI: `GET /api/receivables?vehicleId=` (alacak listesinin aynısı, araca süzülmüş — prototip notu); satıra açıklama (şikâyet, boşsa tür adı) eklendi. 198 test, 2/2 mutasyon |
| C5 | **Dükkân ayarları** — ad, adres, telefon, bakım ay/km, alış sorusu (oku/yaz, sınırlar) | 99-ayarlar · A-3 · A-M43 | **BİTTİ** — `GET/PATCH /api/shop` (periyot yalnız 3·6·12 ay, alış sorusu), `POST /api/shop/privacy-notice-shown` (ilk an korunur); teslimdeki hatırlatma yeni periyodu kullanıyor. Ad/adres/telefon **düzenlenmiyor** — prototipte yok (yeni özellik; gerekiyorsa Cemil). 209 test, 4/4 mutasyon (biri test genişletilince yakalandı) |
| C6 | **KDV/tevkifat için YER** — kalemde KDV oranı (dükkân varsayılanından, kayıtta donar), araçta sahip tipi, kayıtta tevkifat oranı; ekran yok | MUHASEBE-KAPSAM Faz 1 · KDV.md | **BİTTİ** — `ServiceLine.VatRate` (eklenirken dükkân varsayılanı, DONAR), `Shop.DefaultVatRate` (20), `Vehicle.OwnerType`, `ServiceRecord.WithholdingRate`; hesap ve ekran YOK. **Yakalanan hata:** EF göçü eski dükkânlara KDV 0 yazacaktı → 20; yeni hata sınıfı için **göç yükseltme testi** (`MigrationUpgradeTests`). 214 test, 4/4 mutasyon |
| C7 | **Hata sözleşmesi** — bozuk/eksik gövde ve geçersiz dönem 400; eşzamanlı değişiklik 409; hepsi Türkçe | — | **BİTTİ** — ÖLÇÜLDÜ: okunamayan istek GÖVDESİZ 400, eşzamanlı yazma 500 dönüyordu. Şimdi 400 `request.invalid` · 409 `record.changed` · 422 kural; hepsi Türkçe Problem Details. RowVersion'un kaybolan yazmayı gerçekten yakaladığı testli. 219 test, 3/3 mutasyon |
| C8 | **Muhasebe ekranı tek çağrı** — özet + gider + alacak bir istekte | 95-muhasebe | **BİTTİ** — `GET /api/accounting?period=`; yeni hesap yok, her parça kendi sorgusundan; test her dönemde parçaları ayrı uçlarla JSON düzeyinde karşılaştırıyor. 222 test, 3/3 mutasyon |
| C9 | **Kasa sorguları SQL'de** — teslim edilmiş bütün kayıtları belleğe yüklemesin; sonuç birebir aynı | — | **BİTTİ** — kasa özeti, ana ekran, iş başına fark, parça alışları, alacak, CSV: süzgeç SQL'de. Tahsil günü kuralının SQL biçimi `ServiceRecord.CollectedBetween`; bellekteki `CollectedOn` ile **her durumda eşitliği testli**. Taban önce eski kodla yeşil koşturuldu (300 eski iş), sonra aynı test yeni kodla. 227 test, 5/6 mutasyon + 1 eşdeğer (teslim günü hep "bugün", üst sınır hiçbir satırı elemez) |
| C10 | **Eşdeğerlik:** prototipin muhasebe rakamları (02-veri.js, node'da) = API'nin rakamları, 0 fark | MUHASEBE-KAPSAM §5 "bitti" tanımı | **BİTTİ — 0 FARK.** `tools/muhasebe-esdeger.js` prototipin KENDİ fonksiyonlarını Node'da çalıştırır; `PrototypeEquivalenceTests` üç senaryoda (tohum · veresiye bugün tahsil · kenar durumlar) kasa, teslim, veresiye, gider, parça alışı, iş başına fark ve **CSV'yi bayt bayt** karşılaştırır. Kenar senaryosu mutasyonla bulunan iki kör noktadan doğdu (boş şikâyet, bedava parça tohumda yoktu). 230 test, 5/5 mutasyon |
| C11 | **Belge:** VERI-MODELI iş kuralları tablosuna backend karşılıkları; `.http` muhasebe istekleri | — | **BİTTİ** — tabloya "Backend'de" sütunu (15 kural; K-069 plaka çatışması "henüz yok — ruhsat okuma ucu AI sağlayıcısını bekliyor"); `.http`'ye muhasebe istekleri; K-075; HANDOFF. **Muhasebe turu kapandı** |
| — | *— Tur D · 23.10 · Cemil: "Karar verilmesi gerekenler için en iyi, tercih ettiğin kararı verebilirsin. Projede yapılacak işleri listele. Bir döngü yaz. … günlük limit dolana dek devam et. Sonrasında detaylı bir inceleme yapacağız." Kural: her lider kararı KARARLAR'a gerekçesiyle ve "Cemil onayına açık" diye yazılır; para harcayan, dışarıya veri gönderen ya da makinenin ayarını değiştiren adım YİNE yapılmaz (kod yazılır, koşturulmaz) —* | | |
| D1 | **Parçacılar** — ekle, düzenle, sil (soft); ad + telefon (iki alan, VERI-MODELI §4) | 40-parca-sor · ayarlar | **BİTTİ** — `GET/POST /api/suppliers`, `PUT/DELETE /api/suppliers/{id}`; `Supplier` + göç; numara dükkân içinde tekil (K-076.1). 241 test, 3/3 mutasyon (biri test "Zafer" ile ayırt edici yapılınca) |
| D2 | **Parça sorma** — soru (kayıt + parçacı + metin, satır bağı A-24), K-068 kimlik KAPISI, cevap (ham metin), cevabı satıra uygula (fiyat/alış) | 40-parca-sor · K-068 · VERI-MODELI §5–6 | SIRADA |
| D3 | **Müşteri özet linki** — kimliksiz, salt okunur sayfa verisi; tahmin edilemez kod (karar) | `ozetGovdeHtml` · K-023 | SIRADA |
| D4 | **Mesaj önerileri** — kayda uygun hazır metinler (MESAJ_SABLONLARI.uygunKayit) + metnin kendisi (hitap + metin + imza) | `uygunSablonlar` · `hazirMesajMetni` | SIRADA |
| D5 | **İzin geçmişi** — kim, ne zaman, nasıl; geri alma eski tarihi DOĞRU kaynaktan getirir (K-073.10'u kapatır) — karar | K-071.3 · K-073.10 | SIRADA |
| D6 | **Yeni dükkân kaydı** — telefonla kendi kendine kayıt (SMS kodu), dükkân adı — karar | K-074.5 | SIRADA |
| D7 | **SMS sağlayıcı adaptörü** — sağlayıcı seçimi (karar), ortam değişkeniyle yapılandırma, sahte HTTP ile test; gerçek gönderim YOK | K-074.5 | SIRADA |
| D8 | **Ruhsat okuma ucu** — fotoğraf → alanlar (okuyucu arayüzü, K-067.2c Gemini), okununca araç kaydı (K-065), plaka çatışması (K-069); gerçek çağrı YOK | K-065 · K-067 · K-069 | SIRADA |
| D9 | **İz kaydı** — istemcinin olay iz'i (destek için, K-053) | `00-iz.js` · K-053 | SIRADA |
| D10 | **Güvenlik/işletim** — giriş uçlarında IP hız sınırı, CORS (PWA), sağlık ucu | — | SIRADA |
| D11 | **CI** — GitHub Actions: build + test (LocalDB'li Windows koşucusu) | — | SIRADA |
| D12 | **İnceleme rehberi** — Cemil'in "detaylı inceleme"si için tek belge: ne yapıldı, hangi kararlar onayını bekliyor, nasıl denenir | — | SIRADA (en son) |
| — | **SMS sağlayıcısı** (Netgsm / İleti Merkezi …) — hesap, fiyat, gönderici adı | K-074.5 | **BEKLİYOR (Cemil)** — paralı dış servis; bağlanana kadar üretimde kod isteği 503 |
| — | **Yeni dükkânın kaydı** — ilk usta sisteme nasıl girer | K-074.5 | **BEKLİYOR (Cemil)** — ürün akışı; şimdilik yalnız kayıtlı telefon girer |
| — | İzin geçmişi (log) tablosu — kim, ne zaman, nasıl verdi/kaldırdı; İYS ile | K-071.3 · K-073.10 | **BEKLİYOR (Cemil)** — iznin nasıl alındığı (sözlü/yazılı/SMS onayı) ürün kararı |
