# Paralı ses tanıma — seçenekler ve maliyet (16.10.2026)

> **Cemil, 16.10:** *"Paralı tanıyıcı seçeneklerini araştır, maliyet ver."*

**Neden:** Chrome'un ücretsiz tanıyıcısı (Web Speech) Türkçede **ipucu kabul
etmiyor** (SES-OLCUM §16) ve 300 gerçek kayıtta kaybın neredeyse tamamı
DUYMADA (§14: plaka harfleri sözcüğe dönüyor). Paralı tanıyıcıların çoğu
**kelime ipucu** alıyor: dükkânın plakaları, araçların marka/modeli, parça
adları (`SOYLEYIS_IFADELER()` — hazır, 171 ifade).

⚠️ Fiyatlar 16.10.2026'da web'den; kaynaklar yer yer çelişiyor — karar
öncesi resmî sayfadan yeniden bakılır. Türkçe doğruluğu HİÇBİRİNDE
ölçülmedi; aşağıdaki tablo **maliyet ve özellik** tablosudur, kalite değil.

## Kullanım varsayımı

Kayıtlarda komut ~3–4 sn. Günde 40 komut × 26 gün ≈ **1.040 komut / ay ≈
70 dk ses / dükkân**. Faturalama birimi belirleyici: istek başına 15 sn
asgari olan sağlayıcıda 4 sn'lik komut 15 sn sayılır (×4).

## Tablo

| Sağlayıcı | Türkçe | Kelime ipucu Türkçede | Fiyat (liste) | ≈ dükkân başına / ay |
|---|---|---|---|---|
| **Google Chirp 3** (V2) | ✅ tr-TR (eu) | ✅ "model adaptation" (resmî dil tablosu) | ~$0,016/dk · ayda 60 dk ücretsiz (hesap başına) | **~$1,10** |
| **Azure Speech** gerçek zamanlı | ✅ tr-TR | ⚠️ phrase list "etkin dillerde" — tr-TR listede açıkça görünmedi, doğrulanacak | $1/saat (≈$0,0167/dk), saniye bazlı · ayda 5 saat ücretsiz | ~$1,15 |
| **AssemblyAI** Universal-3.5 Pro | ✅ (resmî Türkçe sayfası) | ✅ keyterm (aynı sayfa) | $0,21/sa + keyterm $0,05/sa (dosya); Sync API $0,45/sa (keyterm dahil) | ~$0,30 – $0,55 |
| **ElevenLabs** Scribe v2 | 90+ dil (Türkçe doğrulanacak) | keyterm (≤1.000 terim) — Türkçede doğrulanacak | dosya $0,22 + $0,05/sa · gerçek zamanlı $0,39 + $0,08/sa | ~$0,30 – $0,55 |
| **Speechmatics** Enhanced | ✅ | ✅ özel sözlük ≤1.000 kelime, ücretsiz | $0,38/sa (Standard $0,23) | ~$0,30 – $0,45 |
| **Soniox** | ✅ | ✅ "custom context" — **token ile ücretli** | $0,10/sa dosya, $0,12/sa canlı + bağlam $3,5–4/1M token | ~$2,30 (171 ifadelik bağlam her istekte ~$0,002) |
| **Deepgram** Nova-3 | ✅ tr / tr-TR | ⚠️ keyterm Nova-3'te var, Türkçe desteği belgede yazmıyor | ~$0,0077/dk + keyterm $0,0013/dk | ~$0,65 |
| **Amazon Transcribe** | ⚠️ tr-TR doğrulanamadı | özel sözlük | ~$0,01–0,024/dk, **istek başına 15 sn asgari** | ~$2,60 – $6,20 |
| (OpenAI gpt-4o-transcribe) | — | — | ~$0,006/dk; bir kaynak 26.08.2026'da kullanımdan kaldırıldığını yazıyor, doğrulanamadı | — |

## Okuma

1. **Maliyet karar verdirmiyor.** Amazon dışındaki hepsi dükkân başına
   ayda **$0,30 – $2,30** (≈ bir çay parası). 100 dükkânda bile ayda
   $30 – $230. Karar Türkçe DOĞRULUĞUyla ve **kelime ipucunun Türkçede
   gerçekten çalışmasıyla** verilir.
2. **Kelime ipucu Türkçede belgeyle doğrulananlar:** Google Chirp 3,
   AssemblyAI, Speechmatics, Soniox. Azure, ElevenLabs, Deepgram için
   belge belirsiz — ölçümde anlaşılır (Chrome'da `phrases-not-supported`
   dersini aldık: API'nin var olması, Türkçede çalıştığı anlamına gelmiyor).
3. **Amazon** 15 sn asgari yüzünden kısa komutta pahalı ve tr-TR belirsiz
   — ilk elemede çıkar.
4. **Gecikme:** dosya (async) API'leri ucuz ama usta konuşmayı bitirdikten
   sonra dosya yüklenir; 1–3 sn bekleme olur. Gerçek zamanlı API'ler biraz
   daha pahalı, beklemesiz. Bir komut 4 sn olduğu için dosya API'si de
   kabul edilebilir — ölçülür.
5. **Mimari:** anahtar tarayıcıda DURAMAZ (YAPAY-ZEKA §4) → backend fazı.
   Ses yurtdışına gider; K-060 bunu kabul etti, gizlilik notu söylüyor.
   AB bölgesi seçeneği olanlar (Google eu, Azure Batı Avrupa) KVKK
   açısından tercih edilir.

## Öneri — ölçmeden seçilmez, ölçüm ucuz

Elimizde **300 gerçek kayıt** (WAV) ve aynı cetvel var. 300 kayıt ≈ 20 dk
ses → sağlayıcı başına ölçüm **$0,05 – $0,50**. Model koşucusu gibi bir
Node betiği (`model-kos.js` deseni, anahtar ortamdan) aynı dökümleri
üretir; motor + cetvel aynen puanlar.

İlk tur önerisi (belgeyle Türkçe + ipucu doğrulananlar): **Google Chirp 3,
AssemblyAI, Speechmatics** — her biri ipuçlu ve ipuçsuz. Karşılaştırma:
bugünkü Web Speech %72 (sınav yarısı, §14).

Gereken: her sağlayıcıda hesap + anahtar (Cemil) ve harcama onayı.

## Kaynaklar

- Google: [V2 desteklenen diller (tr-TR, model adaptation)](https://docs.cloud.google.com/speech-to-text/v2/docs/speech-to-text-supported-languages) · [fiyat özeti 2026](https://convertaudiototext.com/blog/google-cloud-speech-to-text-pricing-2026)
- Azure: [dil desteği](https://learn.microsoft.com/en-us/azure/ai-services/speech-service/language-support?tabs=stt) · [phrase list](https://learn.microsoft.com/en-us/azure/ai-services/speech-service/improve-accuracy-phrase-list) · [fiyat özeti](https://apio.sh/apis/azure-speech-to-text)
- AssemblyAI: [Türkçe](https://www.assemblyai.com/languages/turkish) · [fiyat](https://www.assemblyai.com/pricing)
- ElevenLabs: [API fiyatları](https://elevenlabs.io/pricing/api)
- Speechmatics: [fiyat](https://www.speechmatics.com/pricing)
- Soniox: [fiyat](https://soniox.com/pricing) · [Türkçe](https://soniox.com/compare/soniox-vs-speechmatics/turkish)
- Deepgram: [diller](https://developers.deepgram.com/docs/models-languages-overview) · [keyterm](https://developers.deepgram.com/docs/keyterm) · [fiyat özeti](https://www.cartesia.ai/learn/deepgram-pricing)
- Amazon: [fiyat özeti](https://costgoat.com/pricing/amazon-transcribe) · [15 sn asgari](https://diyai.io/ai-tools/speech-to-text/reviews/aws-transcribe-review/)
- OpenAI: [fiyat ve durum](https://costgoat.com/pricing/openai-transcription)

## İlk tur sonuç (16.10) — aynı 300 kayıt, aynı motor, aynı cetvel

Koşucu: `tools/degerlendirme/parali-tanima-kos.js`. Her sağlayıcı ipuçsuz ve
ipuçlu (171 ifade: dükkânın plakaları + marka/model + terimler). Puanlama
tarayıcıda, Chrome dökümleriyle aynı işlevle (düzenek Chrome'un bilinen
%72'sini birebir verdi — doğrulandı).

| sınav yarısı (parti 4–6) | kendiliğinden | yanlış | plakası doğru duyulan (300'de) | kelime hatası |
|---|---|---|---|---|
| Chrome (ücretsiz) | %72 (+11 doğru öneri) | 33 | 55/127 | %30 |
| Speechmatics | %59 | 49 | **15**/127 | %35 |
| **Speechmatics + ipucu** | **%79** (+1) | **20** | **82/127** | **%26** |
| AssemblyAI (ipuçlu = ipuçsuz) | %54 | 52 | 38/115 | %56 |

- **Speechmatics'te ipucu GERÇEKTEN çalışıyor** — plaka 15 → 82. Chrome'un
  55'inin çok üstünde; kendiliğinden doğru %72 → %79, yanlış 33 → 20.
- **AssemblyAI'de ipucu etkisiz** — iki koşu birebir aynı; Türkçe için
  universal-2'ye düşüyor (yeni model Türkçeyi desteklemiyor) ve ipucunu yok
  sayıyor. 26 kayıtta metin dönmedi. İlk turda ELENDİ.
- **Tek dokunuşla doğru** karşılaştırması daha yakın: Chrome ~%79 (motorun
  öneri katmanı Chrome'un plaka hatalarını telafi ediyor), Speechmatics +
  ipucu ~%80. Fark, ustanın **daha az dokunması** ve **daha az yanlış**.
- Motor Chrome'un hatalarına bakılarak ayarlandı (SES-OLCUM §14) — bu,
  karşılaştırmayı Chrome lehine çarpıtır; Speechmatics'e özgü ayar yapılmadı
  (yalnız genel yazım biçimi, aşağıda).

**Yazım biçimi (motor, genel):** Speechmatics sayıyı ve noktalamayı farklı
yazıyor — "06:52", "57,01", "Rotil 1500.", "7 bin", "3 bin 900". Motor
artık hepsini tek biçime indiriyor; ayrıca plaka okuyucusu biçime uymayan
sahte adayı ("saat 10:30 da …" → "30 DA 06") atlayıp sıradakini alıyor.
Speechmatics + ipucu %77 → %79; Chrome değişmedi.

**Maliyet (tahmin):** 4 koşu × ~16 dk ≈ 64 dk ses → AssemblyAI ~$0,16,
Speechmatics ~$0,40 (deneme kredisi içinde kalmış olmalı).

**Öneri:** ikinci turda **Google Chirp 3** (belgede Türkçe + ipucu) ölçülsün;
Speechmatics ile karşılaştırılıp backend fazının tanıyıcısı seçilsin.
Speechmatics dükkân başına ayda ~$0,45 (Enhanced, $0,38/saat).

## Google — ön ödeme (16.10) · KARAR K-061

Google Cloud, Türkiye hesabını etkinleştirmek için **bir kerelik en az
1.500 TL ön ödeme** istiyor (bakiyeye geçer, kullanım oradan düşer). Ölçüm
~20 TL tutacaktı → ödenmedi. Google ve Azure backend fazında, gerçek
kullanım başlayınca ölçülür. Aday: **Speechmatics + ipucu**.

## Google Gemini 3.5 Transcribe (16.10) — ELENDİ: ipucuyla plaka UYDURUYOR

Ödemesiz yol denendi (ücretsiz katmanda yazıya çevirme modeli yok — 402);
Cemil 250 TL ön ödeme yükledi (kredi 1 yılda sona erer, iade yok). Model
**günde 100 istek** kotasına takıldı (önizleme) — ipuçlu 52, ipuçsuz 35 kayıt.

**Bulgu — koşuyu tamamlamaya gerek bırakmadı:** ipucu listesi verilince
Gemini SÖYLENMEMİŞ ama dükkânda KAYITLI plakaları yazıyor:

| | kayıt | söylenmemiş kayıtlı plaka yazdı |
|---|---|---|
| **Gemini + ipucu** | 52 | **4 (%7,7)** |
| Gemini | 35 | 0 |
| Speechmatics + ipucu | 300 | **0** |
| Chrome | 300 | 1 (%0,3 — GS/GT ikizi, sesçe) |

Örnek: "34 BU 440 sahibine parça bekliyoruz" → "38 ODV 111 parçayı
bekliyoruz"; "antifriz 1200 TL" → "34 M 49109". **En tehlikeli hata
sınıfı:** plaka geçerli ve kayıtlı — motorun hiçbir koruması (K-059,
geçersiz plaka, öneri) onu yakalayamaz; iş SESSİZCE yanlış araca gider.
Gemini ipucunu "olasılık" değil "doldurulacak cevap" gibi kullanıyor (bir dil
modeli). Speechmatics aynı listeyle 300 kayıtta hiç uydurmadı.

Aynı 52 kayıtta kelime hatası: Gemini + ipucu %38 · Chrome %41 ·
Speechmatics + ipucu %31 — doğrulukta da önde değil.

**Sonuç:** Speechmatics + ipucu. Google'ın klasik tanıyıcısı (Chirp 3) dil
modeli değil, bu uydurmayı yapmayabilir — backend fazında, ön ödeme gerçek
kullanıma dönüşünce ölçülür (K-061.3).

**Ölçüm aletine eklenen ders:** "söylenmemiş kayıtlı plaka" sayacı —
tanıyıcı karşılaştırmasında doğruluktan ÖNCE bakılır.
