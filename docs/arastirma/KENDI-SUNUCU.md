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

## 6 · Evde kendi sunucumuz (20.10) — ⚠️ fiyatları §7 düzeltir

> Cemil: *"Sunucuyu satın alıp evde kullanmak. Daha manuel olacaktır fakat
> artık 1000mb internet alabiliyoruz evlere. Maliyet ne olur?"*

### Donanım (tek seferlik, kabaca — fiyatlar doğrulanmalı)

| Kalem | Tahmin | Not |
|---|---|---|
| RTX 5090 32 GB | **~110–150 bin ₺** | 2025 lansman piyasası 128–150 bin ₺ ([log](https://www.log.com.tr/?p=930842), [ntv](https://www.ntv.com.tr/teknoloji/nvidia-rtx-5000-serisini-tanitti-turkiye-fiyati-belli-oldu,w5yP2QLzG06Isy5TUqTcJg)); Ekim 2026 fiyatı bulunamadı |
| Gövde: işlemci, 64 GB RAM, 1.200 W güç kaynağı, NVMe, kasa | ~50–70 bin ₺ | |
| UPS (kesintisiz güç, ~1.500 VA) | ~10–15 bin ₺ | Elektrik kesilince düzgün kapanması için |
| **Toplam** | **~170–235 bin ₺** | Bir kez |

32 GB yeter mi: Whisper (~3–6 GB) ile 30B sınıfı görüntü modeli (4-bit,
~17–20 GB) sığar; pay az. 48 GB'lık profesyonel kart (RTX 6000 Ada) bunun
iki-üç katı fiyatta.

### Aylık

| Kalem | Tahmin | Not |
|---|---|---|
| Elektrik | **~600–1.200 ₺** | 24 saat açık, ortalama 200–400 W → 145–290 kWh. ~3,9 ₺/kWh üst kademe ([Nisan 2026 zammı](https://uzmanpara.milliyet.com.tr/uzmanpara/elektrik-zammi-2026-nisanda-ne-kadar-oldu-elektrige-ne-kadar-zam-geldi-iste-yeni-tarife-7565631)). ⚠️ Mesken desteği **yıllık 4.000 kWh** ile sınırlı; evin tüketimi + sunucu sınırı aşarsa fiyat sübvansiyonsuz tarifeye çıkar. |
| İnternet | mevcut fiber | Yük küçük (bkz. aşağı) |
| Türkiye'de küçük VPS (uygulama + veritabanı + bağlantı noktası) | ~300–600 ₺ | Ev sunucusu yalnız yapay zekâyı çalıştırır |

### İnternet yeter mi — evet, rahat
1.000 dükkânda bile günde ~5.000 ruhsat fotoğrafı (~1,5 GB) ve ~20.000
sesli komut (~2 GB) **eve gelir**. Evden çıkan yalnız kısa metin cevaplar.
1.000 Mbps'in küçük bir parçası. Asimetrik tarifelerdeki düşük upload da
sorun değil.

**Sabit IP gerekmez:** ev sunucusu, Türkiye'deki VPS'e **kendisi bağlanır**
(WireGuard tüneli). Dışarıdan eve hiç kapı açılmaz. Bu, güvenlik için de
daha iyi. ⚠️ Cloudflare Tunnel gibi yabancı aracılar **kullanılmaz**; veri
yine yurt dışından geçer.

### Karşılaştırma

| | Bir kez | Aylık | 12 ay toplam |
|---|---|---|---|
| **Ev sunucusu** | ~170–235 bin ₺ | ~1–2 bin ₺ | ~185–260 bin ₺ |
| Türkiye'de kiralık dedicated (48 GB) | — | ~$1.000 (~40–45 bin ₺) | ~480–540 bin ₺ |
| API (Gemini + Speechmatics), 100 dükkân | — | ~$75–135 (~3–6 bin ₺) | ~40–70 bin ₺ |

- **Kiralamaya göre** ev sunucusu ~5–6 ayda kendini öder.
- **API'ye göre** küçük ölçekte pahalı. Gerekçesi yine para değil, **verinin
  Türkiye'de ve elimizde kalması**.

### Riskler — "manuel" kısmı
1. **Tek nokta arıza:** elektrik, internet, ya da kartın kendisi gidince ses ve
   ruhsat okuma durur. Gizlilik amaçsa yurt dışı API'ye düşmek seçenek
   değil. Yedek yol **elle giriş**: usta plakayı ve işi yazar; ürün yine
   çalışır, yalnız yavaşlar.
2. **Ev aboneliği sözleşmesi:** bazı ev tarifeleri ticari kullanımı ya da
   sunucu barındırmayı yasaklayabilir. Operatörün sözleşmesine bakılmalı;
   gerekirse kurumsal tarife.
3. **KVKK güvenliği:** müşteri verisinin işlendiği yer ev. Disk şifreleme,
   kilitli oda, erişim kaydı ve yedekleme gerekir. Veritabanı evde değil
   VPS'te durursa evdeki kutu **verisiz** kalır: fotoğraf ve sesi okur,
   saklamaz. Bu riski büyük ölçüde azaltır.
4. **Bakım:** güncelleme, izleme ve arıza müdahalesi senin zamanın.
5. **Kapasite:** tek kart ruhsat için ~300–500 dükkân taşır; sonrası ikinci kart.

### Önerilen sıra
1. **Önce saatlik kiralık kartta ölçüm (birkaç yüz ₺).** 200 bin ₺'lik
   donanımı, modellerin bizim ruhsatlarımızı ve seslerimizi doğru okuduğu
   görülmeden almak riskli. Dizüstündeki 8–12B modeller başarısızdı.
2. Ölçüm iyiyse ev sunucusu: **yalnız yapay zekâ, verisiz.** Uygulama ve
   veritabanı Türkiye'deki VPS'te, aralarında WireGuard.
3. Yedek yol elle giriş; ileride ikinci bir kart ya da kiralık sunucu.


## 7 · KARAR ve kademeli ev planı — güncel fiyatlarla (20.10)

> **Cemil:** *"Dış servislere bağımlı olmak istemiyorum. Yasal yükümlülük ve
> gelecekte olası fiyat artışları nedeniyle. Baştan ev gibi bir ortamda
> kurgulamak istiyorum. Fakat ilk başta makinenin düşük, giderek artan bir
> şekilde olması düşünüyorum. [...] resim ses vb o sunucuda tutmaya gerek yok.
> Yapay zeka için çalışacak. Ve evde internet, yüksek elektrik tüketimi vb gibi
> konulara tam araştır."*

### 7.1 · §6'daki düzeltme
§6 RTX 5090'ı ~110–150 bin ₺ yazmıştı; bu **2025 lansman** fiyatıydı.
**Ekim 2026'da ~270–390 bin ₺**
([akakçe](https://www.akakce.com/ekran-karti/en-ucuz-asus-rtx-5090-tuf-rtx5090-32g-gaming-512-bit-gddr7-32-gb-fiyati,903116906.html),
[cimri](https://www.cimri.com/ekran-kartlari/en-ucuz-asus-tuf-gaming-geforce-rtx-5090-oc-tuf-rtx5090-o32g-32gb-gddr7-ekran-karti-fiyatlari,2431811975),
[itopya](https://www.itopya.com/ekran-karti_k11?gpumodel=geforce-rtx-5090-q7614)).
Sıfır RTX 3090 105 bin ₺ ([vatan](https://www.vatanbilgisayar.com/msi-geforce-rtx-3090-gaming-x-trio-24g-24gb-gddr6x-384bit-nvidia-ekran-karti.html)).
Bellek kıtlığı ekran kartlarını pahalandırmış.

### 7.2 · Ev için doğru sınıf: birleşik bellekli küçük bilgisayar
Ekran kartlı kasa yerine işlemci ve grafik belleği **ortak** olan makineler.
30B sınıfı modeli az elektrikle, sessizce çalıştırırlar.

| Makine | Bellek | Fiyat | Güç | Not |
|---|---|---|---|---|
| **Mac mini M4 Pro** | 64 GB | **~135 bin ₺** ([PT](https://www.pt.com.tr/mac-mini-m4-pro-14cpu-20gpu-64gb-1tb-10-gigabit-ethernet-z1jv000vg)) | boşta **3,7 W** ([ENERGY STAR](https://www.energystar.gov/productfinder/product/certified-computers/details/4513881)), yükte ~60–150 W | Türkiye'de garantili satılıyor, sessiz. M5 Pro'lu sürüm de var; fiyatı [Apple TR](https://www.apple.com/tr/shop/buy-mac/mac-mini)'de bakılmalı |
| Ryzen AI Max+ 395 mini PC | 64 / 128 GB | ~$1.960–2.200 / ~$3.300–3.600 + gümrük, KDV ([karşılaştırma](https://computingforgeeks.com/ryzen-ai-max-395-mini-pc-comparison/)) | yükte ~120–140 W | Linux, sunucuya daha yakın. Türkiye'de satıcı bulunamadı → ithalat ve garanti sorunu |
| RTX 5090 iş istasyonu | 32 GB | ~340–460 bin ₺ (kart + kasa) | yükte **~600–700 W** | En hızlı; ama pahalı, sıcak, gürültülü, elektriği ağır |

**Hız (tahmin, ölçülmedi):**
- Qwen3-VL 30B-A3B, M4 Pro'da ~33 jeton/sn
  ([willitrunai](https://willitrunai.com/de/can-run/qwen-3-vl-30b-a3b-on-m4-pro-64gb)).
- Strix Halo'da benzer metin modeli ~86 jeton/sn
  ([strix-halo-llm-perf](https://github.com/visorcraft/strix-halo-llm-perf)).
- Ruhsat ≈ **15–25 sn**, ses (Whisper, 5 sn) ≈ **1 sn**.

### 7.3 · Elektrik — ayrıntı
Ekim 2026 mesken tarifesi (ikincil kaynaklar, EPDK kararıyla teyit edilmeli:
[akıllıtarife](https://akillitarife.com/enerji/rehber/tarife/elektrik),
[verinokta](https://verinokta.com/fiyatlar/elektrik-tarifeleri/)):
- **Kademe:** günde 8 kWh (ayda ~240 kWh) altı **düşük kademe ~3,1–3,2 ₺/kWh**
  (vergili); üstü **yüksek kademe** (vergisiz ~4,32 ₺).
- **Yıllık 4.000 kWh sınırı:** aşan abone son kaynak tarifesine geçer,
  **~6,49 ₺/kWh** (tek kaynak).

| Makine (7/24, ortalama) | Aylık kWh | Aylık maliyet | Etkisi |
|---|---|---|---|
| Mac mini (~20 W ortalama) | ~15 | **~50–100 ₺** | Kademeyi neredeyse oynatmaz |
| Strix Halo (~40 W) | ~30 | ~100–200 ₺ | Küçük |
| RTX 5090 kasası (~250 W) | ~180 | **~800–1.200 ₺** | Evi büyük olasılıkla **yüksek kademeye** ve **4.000 kWh sınırının üstüne** iter; o zaman evin TÜM tüketimi pahalanır |

**Sonuç:** RTX 5090'ın asıl maliyeti kartın fiyatı kadar **evin elektrik
tarifesini bozması**.

### 7.4 · İnternet — ayrıntı
- **⚠️ Bireysel abonelikte ticari kullanım YASAK.**
  - [Superonline](https://superonlinetr.net/paket/fiber-taahhutsuz-tanisma-paketleri-18):
    *"bireysel kullanım içindir, ticari amaçla kullanılamaz"*; tespit edilirse
    hattı kapatma hakkı.
  - [Türk Telekom](https://www.turktelekom.com.tr/tt-yardim/SozlesmeVeTaahhutnameler/abonelik_sozlesmesi_2015.pdf):
    *"ticari ve başka bir amaçla kullanamaz, üçüncü şahıslara sunamaz."*

  → **Kurumsal (iş) abonelik gerekir.**
- **Fiyat:**
  - TurkNet bireysel GigaFiber, 1.000 Mbps simetrik: **949,90 ₺/ay**
    (19.06.2026'dan beri; [technopat](https://www.technopat.net/2026/06/20/turkiye-1000-mbps-internet-fiyatlari/)).
  - TurkNet **İş GigaFiber**: eski bir listede 574,90 ₺
    ([enuygunfinans](https://www.enuygunfinans.com/internet-baglantilari/turknet/1000-mbps-turknet-is-gigafiber-max-paketi-1000-dk-her-yone-telefon-hatti),
    2024). Güncel fiyat ve **sabit IP** TurkNet iş satışına sorulmalı.
- **Hız fazlasıyla yeter:** 1.000 dükkânda bile eve gelen günde ~3,5 GB.
- **Sabit IP gerekmez:** ev makinesi Türkiye'deki VPS'e WireGuard ile
  **kendisi bağlanır**. Dışarıdan eve kapı açılmaz; yabancı tünel
  (Cloudflare vb.) kullanılmaz.
- GigaFiber yalnız TurkNet'in kendi altyapısı olan adreste; önce adres
  sorgusu yapılmalı.

### 7.5 · Kesinti, ısı, gürültü
- **UPS:** line-interactive 2.000 VA ~9,5 bin ₺
  ([vatan](https://www.vatanbilgisayar.com/tuncmatik-digitech-eco-2000va-line-interactive-siyah-ups.html)).
  Makineyle birlikte **modem de** UPS'e bağlanır; makinede "elektrik gelince
  kendiliğinden aç" ayarı açılır.
- **Mac mini:** sessiz, ısı sorunu yok, rafta durur.
  **RTX 5090 kasası:** yükte yüksek ses ve oda ısısı.
- **Yedek yol:** makine ya da internet giderse yurt dışı servise **düşülmez**.
  Usta elle yazar; program yavaşlar ama çalışır.

### 7.6 · Kademeli plan

| Aşama | Ne zaman | Donanım | Bir kez | Aylık |
|---|---|---|---|---|
| **0 · Ölçüm** | şimdi | Türkiye'de saatlik kiralık kart, 2–3 saat | birkaç yüz ₺ | — |
| **1 · Pilot** | 1 → ~100–150 dükkân | **Mac mini M4 Pro 64 GB** + UPS | **~145 bin ₺** | ~1,5–2,5 bin ₺ (iş interneti + VPS + elektrik) |
| **2 · Büyüme** | ~150 → ~300 dükkân | **İkinci aynı makine.** Yük ikiye bölünür; biri giderse öteki çalışır (yedeklilik) | +~135 bin ₺ | +~100 ₺ |
| **3 · Ölçek** | 300+ dükkân | Üçüncü makine, ya da kendi donanımımızı **Türkiye'de bir veri merkezine** taşımak (colocation): yine bizim makinemiz, ama elektrik ve internet profesyonel | teklifle | teklifle |

**Neden "küçük makineler, çoğaltarak":**
1. Her aşama bir öncekini çöpe atmaz.
2. İki makine aynı zamanda **yedek** demek; tek büyük kart tek arıza
   noktasıydı.
3. Elektrik ve ısı evin sınırları içinde kalır.

### 7.7 · Aşama 0'ın geçme şartı (ölçmeden alınmaz)
Aynı cetvel, aynı setler:
- **Ruhsat (46):** şase ≥ 45/46, kritik yanlış ≤ 1, plaka uydurma yok (Gemini
  ve Opus düzeyi).
- **Ses (300 gerçek + 222 yapay):** kademeli sonuç ≥ Speechmatics düzeyi
  (%84 sınav).

Model adayları: Qwen3-VL 30B-A3B (birleşik bellekli makinede hızlı),
Qwen3-VL 32B, Gemma 4 31B; ses için Whisper large-v3-turbo.

**Şartı geçemezse** ev planı ertelenir ve bağımlılık başka yoldan
azaltılır: TC, ad ve adres maskelenir, kalanı için standart sözleşme yapılır.

### 7.8 · Yasal ve idari — avukat ve muhasebeciyle netleşecekler
- **İşyeri adresi:** şirket ev adresinde mi? Kurumsal internet ve gerekirse
  elektrik aboneliğinin türü buna bağlı.
- **KVKK veri güvenliği:** makine verisiz olsa da fotoğraf ve ses bellekte
  işleniyor. Disk şifreleme, kilitli oda, erişim kaydı ve VPS ile şifreli
  tünel; kurulum bunlarla yapılır.
