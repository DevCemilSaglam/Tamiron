# Kendi sunucumuzda yapay zekâ — ses ve ruhsat (19.10.2026)

> **Cemil:** *"Şimdilik gemini kullanalım. Peki ileride ses ve ruhsat için kendi
> sunucumuzu kullanmak istersek, ihtiyaçlar ve maliyet ne olacaktır? Kendi
> sunucumuza kurduğumuz ai ile çalışabilir miyiz?"*

## 1 · Kısa cevap

**Evet, teknik olarak mümkün.** İki iş için açık (indirilebilir) modeller var:
- **Ses → yazı:** Whisper large-v3 / large-v3-turbo.
- **Ruhsat fotoğrafı → alanlar:** Qwen3-VL 30B/32B, Gemma 4 31B gibi görüntü
  okuyan modeller.

Motorumuz (söyleyiş sözlüğü, plaka kuralları, kademe) **aynı kalır**; değişen
yalnız tanıyıcı ve okuyucu olur.

**Ama iki şart var:**
1. **Doğruluk ölçülmeden geçilmez.** Bu dizüstünde denenen küçük modeller
   (Gemma 4 12B, Qwen3-VL 8B) ruhsatta başarısız oldu (FOTO-OLCUM §7).
   Büyük sürümler sunucu ekran kartı ister ve henüz ölçülmedi.
2. **Para kazandırmaz, gizlilik kazandırır.** Ölçek büyümeden API daha ucuz
   (§4).

## 2 · Yük — 100 dükkân varsayımıyla

| | dükkân başına/gün | 100 dükkân/gün |
|---|---|---|
| Ruhsat okuma | ~5 | ~500 |
| Sesli komut (~5 sn) | ~20 | ~2.000 (~3 saat ses) |

Bu yük küçük; **tek bir ekran kartı** rahat taşır. Sorun ortalama yük değil,
**gecikme** ve **sabah sıkışması**: usta konuştuğunda 1–2 saniyede cevap
bekler.

## 3 · Donanım

| İş | Model | Ekran kartı belleği (4-bit) | Hedef süre |
|---|---|---|---|
| Ses | Whisper large-v3-turbo | ~6 GB | 5 sn ses → < 1 sn |
| Ruhsat | Qwen3-VL 30B-A3B / 32B ya da Gemma 4 31B | ~20 GB | ruhsat başına 5–15 sn |

**Önerilen tek sunucu:** 48 GB ekran kartı (RTX 6000 Ada ya da L40S). İkisi
birlikte sığar, büyümeye yer kalır. 96 GB'lık RTX PRO 6000 daha rahat.

**Fiyat (Ekim 2026, KDV hariç, kaynaklar aşağıda):**
- Hetzner GEX131, Almanya, RTX PRO 6000 96 GB: **~€889/ay**.
- Edgevana, **Türkiye lokasyonu**, RTX 6000 Ada 48 GB: **~$1.053/ay**
  (saatlik ~$0,72).
- Turksat, Vargonen gibi yerli bulutlar GPU sunmaya başlamış; fiyat için
  teklif istemek gerekir.

**Görünmeyen maliyetler:**
- **Yedek:** sunucu çökünce ses durmasın. İkinci sunucu ya da API'ye düşme
  (bugünkü kademe bunu zaten yapabilir).
- **Bakım:** model güncelleme, izleme, güvenlik. Bunlar insan zamanı ister.

## 4 · API ile karşılaştırma

| | dükkân başına/ay | 100 dükkân | 1.000 dükkân |
|---|---|---|---|
| **API** (Gemini ruhsat ~0,5 ¢ × 150 + Speechmatics kademe ~$0,11) | **~$1** | **~$100** | ~$1.000 |
| **Kendi sunucu** (1 ana + yedek) | — | ~$1.000–2.000 | ~$2.000–3.000 |

**Başa baş noktası kabaca 1.000–2.000 dükkân.** Daha önce kendi sunucunun
gerekçesi **para değil**:
- **Veri Türkiye'de kalır** (KVKK, yurt dışına aktarım).
- Hiçbir şirketin fiyat ya da koşul değişikliğine bağlı kalınmaz. Gemini 3.8
  Flash'ın fiyatı 2027'de ikiye katlanıyor.

## 5 · Nasıl karar veririz — sıra

1. **Bugün:** Gemini (ruhsat) + Speechmatics (ses, kademe b). Ruhsatta plaka
   koruması (K-069) var.
2. **Ucuz deneme (~$5–10):** Türkiye lokasyonlu bir ekran kartı **saatlik**
   kiralanır, birkaç saat açık kalır:
   - **Ses:** Whisper, elimizdeki **300 gerçek + 222 yapay kayıtla** ölçülür.
     Kayıtlar Cemil'in test kayıtları; müşteri verisi değil.
   - **Ruhsat:** Qwen3-VL 32B / Gemma 4 31B, **46 doğrulanmış ruhsatla**,
     aynı cetvelle ölçülür. ⚠️ Fotoğraflarda gerçek kişilerin TC no'su var.
     Ya yalnız araç bölümü kırpılarak denenir ya da Cemil'in kararıyla.
3. Sonuç Gemini/Speechmatics düzeyindeyse **hangi işin** (ses mi ruhsat mı)
   önce taşınacağı rakamla seçilir.
4. Yayında biriken düzeltme kayıtları (K-060, K-063) açık modeli **bize göre
   eğitmeye** (ince ayar) yarar. Kendi sunucunun uzun vadeli asıl
   kazancı bu: model ustanın ağzına alışır.

## Kaynaklar
- [Hetzner GEX131 duyurusu](https://www.hetzner.com/pressroom/new-gex131/) ·
  [Hetzner GPU karşılaştırması (13.10.2026)](https://wz-it.com/en/blog/hetzner-gpu-server-comparison/)
- [Edgevana RTX 6000 Ada — Türkiye](https://nodes.edgevana.com/gpu-servers/rtx-6000-ada/t-rkiye)
- [Türkiye GPU-as-a-Service pazarı (Turksat, Vargonen)](https://lucintel.com/gpu-as-a-service-market-in-turkey.aspx)
- [Ollama Qwen3-VL](https://ollama.com/library/qwen3-vl) · [Ollama Gemma 4](https://ollama.com/library/gemma4)
- Whisper Türkçe: [BuzzASR model kartı](https://huggingface.co/BuzzASR/turkish).
  Sıfırdan large-v3, FLEURS'ta CER %2,4. Okunan metin üzerinde ölçülmüş;
  atölye ve plaka diline uymaz, **kendi kayıtlarımızla ölçülmeli.**
