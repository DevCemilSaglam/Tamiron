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
| — | Oturum açma | K-073.6 | **BEKLİYOR (Cemil)** |
| — | İzin geçmişi (log) tablosu — kim, ne zaman, nasıl verdi/kaldırdı; İYS ile | K-071.3 · K-073.10 | **BEKLİYOR (Cemil)** — iznin nasıl alındığı (sözlü/yazılı/SMS onayı) ürün kararı |
