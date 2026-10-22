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
| B5 | **Teslim** — ödeme yolu + bakım hatırlatma tarihi (ay VEYA km, hangisi önce) | `50-teslim` · `hatirlatmaHesapla` · A-3 | SIRADA |
| B6 | **Araç** — oku/güncelle (ruhsat alanları), plaka değişimi, parça-sor kimlik kapısı | `60-arac` · K-033.2c · K-068 | SIRADA |
| B7 | **Listeler** — serviste olanlar, araçlar, kayıtlar (serviste üstte) | `70-araclar` · `80-kayitlar` | SIRADA |
| B8 | **Gider** (`Expense`) — ekle/sil, dönem toplamı; kasa özetine gider | `95-muhasebe` · K-022 | SIRADA |
| B9 | **Alacak listesi** + **CSV dışa aktarma** | `95-muhasebe` · K-071.2 | SIRADA |
| B10 | **Müşteriler** (araçtan türetilir) + **ileti izni** | `musteriler()` · K-071.3 | SIRADA |
| B11 | **Mesaj arşivi** (`Message`) + toplu alıcı süzgeci (izinliler) — gönderim YOK, kayıt | K-048.2 · K-071.3 | SIRADA |
| B12 | **Hatırlatması gelenler** sorgusu | `hatirlatmasiGelenler` | SIRADA |
| — | Oturum açma | K-073.6 | **BEKLİYOR (Cemil)** |
