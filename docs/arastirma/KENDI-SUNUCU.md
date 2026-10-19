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

## 3b · Türkiye'de dedicated — ne var, ne kadar (19.10)

| Seçenek | Ekran kartı | Fiyat | Not |
|---|---|---|---|
| [Edgevana — Türkiye](https://nodes.edgevana.com/gpu-servers/rtx-6000-ada/t-rkiye) | RTX 6000 Ada 48 GB, bare metal | **~$1.053/ay** (~$0,72/saat) | Tek net, doğrulanabilir Türkiye fiyatı |
| [gpukiralama.com.tr](https://gpukiralama.com.tr/) — İstanbul Maslak | L40S 48 GB, RTX 4090/5090, A100… | **teklifle** | *"verileriniz yurt dışına aktarılmaz"*, saatlik kiralama (en az 1 saat) |
| [CPUNow](https://cpunow.com/gpu-sunucu) — "Türkiye veri merkezi" (Bursa?) | RTX 4090 24 GB, bare metal | 5.499 ₺/ay *(listede)* | ⚠️ Liste tutarsız: 4×V100 de aynı fiyat → **teklif alınmadan güvenilmez** |
| Turksat · Vargonen | — | teklifle | GPU hizmeti duyurulmuş, fiyat yok |

Yurt dışı karşılaştırma: RTX 4090 tek kartlı sunucu ~€300–550/ay, L40S ~$0,63/saat.

**Hangi kart:**
- **24 GB (RTX 4090):** Whisper (~3–6 GB) ile 30B sınıfı görüntü modeli (4-bit,
  ~17–20 GB) **aynı karta zor sığar**; iki kart ya da iki küçük sunucu gerekir.
- **32 GB (RTX 5090):** sığar, ama payı az.
- **48 GB (L40S / RTX 6000 Ada):** ikisi rahat sığar, büyümeye yer var.
  **Önerilen.**

**İşimizi görür mü?**
- **Kapasite: evet.** Tek 48 GB kart ses için yüzlerce dükkân, ruhsat için
  ~300–500 dükkân taşır (ruhsat ~10 sn varsayımıyla; hızlı MoE modelle daha
  fazla).
- **Doğruluk: BİLİNMİYOR — asıl soru bu.** Dizüstündeki başarısızlık hızla
  ilgili değil, **model boyutuyla** ilgiliydi (8–12B). Ekran kartı yalnız hızı
  çözer. 30B sınıfı modeller (Qwen3-VL 30B/32B, Gemma 4 31B) şaseyi Gemini
  kadar okuyor mu, ölçülmedi. Whisper'ın atölye diline ve plakalara
  uyumu da ölçülmedi.
- **Ölçüm yolu:** gpukiralama.com.tr gibi bir Türkiye sağlayıcısından **saatlik**
  L40S, 2–3 saat. Elimizdeki 522 ses kaydı ve 46 ruhsat aynı cetvelle ölçülür.
  Tahmini maliyet birkaç yüz TL. Veri Türkiye'den çıkmaz.

## 4 · API ile karşılaştırma — 100 ve 1.000 dükkân (19.10 düzeltildi)

> ⚠️ İlk yazımda 1.000 dükkân için kendi sunucuyu ~$2.000–3.000 demiştim ve
> başa baş noktasını 1.000–2.000 dükkân diye yazmıştım. Hesap yükten yeniden
> yapıldı; **ikisi de iyimserdi.**

**Varsayımlar:** ayda 26 iş günü, dükkân başına günde 5 ruhsat ve 20 sesli
komut. Ruhsat sayısı **üst sınır**: K-065 gereği ruhsat yalnız **yeni araçta**
çekiliyor, müdavim müşteri arttıkça düşer.

**API (bugünkü yol):**

| | 100 dükkân | 1.000 dükkân |
|---|---|---|
| Ruhsat, Gemini 3.8 Flash (0,48 ¢ · 2027'den 0,96 ¢) | $62 · $125 | **$624 · $1.248** |
| Ses, Speechmatics kademe b (~$0,11/dükkân) | $11 | $110 |
| **Toplam / ay** | **~$75–135** | **~$730–1.360** |
| Dükkân başına | ~$0,75–1,35 | ~$0,73–1,36 |

API **doğrusal** büyür: dükkân başına maliyet 10 dükkânda da 1.000'de de aynı.

**Kendi sunucu, 1.000 dükkân:**
- **Ruhsat:** günde 5.000 ruhsat × ~10 sn ≈ **14 GPU-saat/gün**. İş 10 saatlik
  mesaiye, üstelik sabaha yığılıyor; zirvede **~3 ekran kartı** gerekir.
- **Ses:** günde 20.000 komut ≈ 28 saat ses. Whisper için küçük bir yük, ama
  1 saniyelik gecikme için ayrılmış kapasite ister.
- **Toplam:** 3 sunucu + 1 yedek ≈ **~$4.000/ay**, artı bakım için insan zamanı.
  Dükkân başına **~$4**.

**Sonuç — 1.000 dükkânda kendi sunucu UCUZ DEĞİL:** API ~$0,7–1,4, kendi
sunucu ~$4. Kendi sunucunun her ekran kartı ruhsat zirvesinde ~350 dükkân
taşıyor, yani dükkân başına ~$2,9. Bu, API'nin her iki fiyatından da pahalı.

**Hesabı ne değiştirir:** ruhsat modelinin hızı. Qwen3-VL 30B-A3B gibi her
adımda yalnız ~3B parametre çalıştıran bir model ruhsatı ~2–3 sn'de okursa,
bir kart ~1.500 dükkân taşır. O zaman dükkân başına ~$0,7, yani Gemini'nin
2027 fiyatıyla **başa baş**. Bu hız ve o modelin doğruluğu **ölçülmedi**.

**Kendi sunucunun gerekçesi yine para değil:**
- **Veri Türkiye'de kalır** (KVKK, yurt dışına aktarım).
- Hiçbir şirketin fiyat ya da koşul değişikliğine bağlı kalınmaz. Gemini 3.8
  Flash'ın fiyatı 2027'de ikiye katlanıyor.
- Model, biriken düzeltmelerle ustanın ağzına alıştırılabilir (ince ayar).

**Ölçek:** dükkân başına aylık yapay zekâ maliyeti ~$1. Bu, abonelik fiyatının
yanında küçük; yani bu kararı maliyet değil gizlilik ve bağımsızlık belirler.

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
