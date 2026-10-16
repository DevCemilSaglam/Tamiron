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
