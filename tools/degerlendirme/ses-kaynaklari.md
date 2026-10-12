# Ses ölçümü — dış kaynaklar ve lisansları (A-M48 · 12.10.2026)

Ses dosyaları **depoya girmez** (`.gitignore`: `tools/degerlendirme/sesler/`,
`*.wav`). Burada yalnız **nereden geldikleri ve hangi şartla** kullanıldıkları
durur — dosyalar silinirse aynı adreslerden yeniden indirilir.

Cemil 12.10'da onayladı: açık arşivden gerçek atölye gürültüsü (*"gürültü
uydurulmaz"* kuralının esnetilmesi) ve indirme (`arastirma/SES-OLCUM.md` §8).

## Gürültü — Freesound, CC0 (kamu malı)

Lisans **her dosyanın kendi sayfasında** doğrulandı (12.10); arama süzgecine
güvenilmedi. İndirilen hâl Freesound'un yüksek kaliteli **MP3 önizlemesi**
(özgün dosya giriş istiyor) — tanıma ölçümü için yeterli; koşucu MP3'ü kendisi
çözüyor.

| Dosya (`sesler/gurultu/`) | Kayıt | Kaydeden | Süre | Sayfa |
|---|---|---|---|---|
| `tamirhane-ortami.mp3` | Car/Auto Mechanic Shop | producerdan | 14 sn | https://freesound.org/people/producerdan/sounds/266626/ |
| `atolye-taslama.mp3` | metal shop — working on car parts with disc grinder | kyles | 240 sn | https://freesound.org/people/kyles/sounds/362061/ |
| `havali-somun-sokucu.mp3` | IMPACT WRENCH | SamuelGremaud | 42 sn | https://freesound.org/people/SamuelGremaud/sounds/458855/ |
| `kompresor.mp3` | compressor_working | decembered | 33 sn | https://freesound.org/people/decembered/sounds/259194/ |
| `circir.mp3` | Tools Ratchet | CapsLok | 20 sn | https://freesound.org/people/CapsLok/sounds/181634/ |
| `metal-metale.mp3` | metal on metal | jasonLON | 26 sn | https://freesound.org/people/jasonLON/sounds/125407/ |

**Elenenler** (lisans CC0 ama atölye sesi değil): Short Whoosh · Oxygen
Concentrator · Gun Version 2 · Dog Barking · Air Conditioning · Jack Hammer ·
assembly line factory. *Gürültü, ölçülen ortamın sesi olmalı* — kuralın
esnetilmesi bu yüzden "açık arşivden **atölye** sesi" ile sınırlı.

## Konuşma derlemi — MediaSpeech TR (OpenSLR 108)

- **Lisans:** Creative Commons Attribution 4.0 (CC BY 4.0) — kullanımda
  **kaynak gösterilir**: *MediaSpeech, Kolobov ve ark., OpenSLR 108*.
- **Adres:** https://www.openslr.org/108 · arşiv `TR.tgz` (618.578.461 bayt, 12.10)
- **İçerik:** ~10 saat Türkçe medya konuşması, 2.513 kayıt (`.flac` + `.txt`).
- **Sabit alt küme:** `sesler/derlem-100/` — dosya adına göre sıralanınca İLK
  100 kayıt. Her koşuda aynı 100 dosya ölçülsün diye rastgele seçilmedi.

⚠️ Derlemde dükkân kelimesi yok (*triger, balata*, plaka okunuşu). Tanıyıcının
**genel** Türkçe tabanını verir; bizim cümlemizi değil (SES-OLCUM.md §3).
