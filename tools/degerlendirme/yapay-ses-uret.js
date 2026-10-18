/* ============ ŞİVELİ YAPAY SES ÜRETİCİ (17.10.2026) ============
   Cemil, 17.10: *"Yapay zekaya, şiveli okutma yapıp kaydedebilir miyiz?
   Daha fazla test yapalım."*

   Gemini TTS (gemini-3.8-flash-tts) + "Voice design": sesi bir TARİFTEN
   kurar ("Trabzonlu, 50'lerinde, hızlı konuşan bir usta"), sonra büyük
   setin SINAV yarısından cümleleri o sesle okutur. Çıktı ölçüm araçlarıyla
   AYNI biçimde: sesler/yapay-<ses>/sNNN.wav + dizin.json (set: 'buyuk')
   → ses-dosya-kos.html?klasor=yapay-<ses> (Chrome) ve
     parali-tanima-kos.js --klasor yapay-<ses> (Speechmatics).

     node tools/degerlendirme/yapay-ses-uret.js --pilot          (6 ses × 3 cümle)
     node tools/degerlendirme/yapay-ses-uret.js --adet 50        (6 ses × 50 cümle)
     (--sesler karadeniz,ege  · yalnız bunlar)

   ⚠️ YAPAY SES GERÇEK USTANIN YERİNE GEÇMEZ: temiz, tutarlı, nefessiz;
   tanıyıcılar sentetik sese genelde DAHA İYİ çalışır → rakamı şişirir.
   Burada ölçülen: aynı cümle, FARKLI ağız/hız/ses rengiyle — çeşitlilik.
   Şivenin inandırıcılığına önce Cemil kulakla karar verir (pilot).
   ⚠️ Anahtar ortamdan (GEMINI_API_KEY); ses kimlikleri sesler/ altında
   (git dışı) saklanır, yeniden oluşturulmaz (proje başına 200 sınırı). */
'use strict';
const fs = require('fs');
const path = require('path');

const KOK = __dirname;
const SES_KOK = path.join(KOK, 'sesler');
const arg = (ad, v) => { const i = process.argv.indexOf(ad); return i > 0 && process.argv[i + 1] && !process.argv[i + 1].startsWith('--') ? process.argv[i + 1] : v; };
const PILOT = process.argv.indexOf('--pilot') > 0;
const ADET = PILOT ? 3 : parseInt(arg('--adet', '50'), 10);
/* --model (18.10): gemini-3.8-flash-tts günde 100 istekte kalınca ayrı kotalı
   gemini-3.8-flash-lite-tts. Lite sesler AYRI klasöre (-lite) — iki model
   karışmasın, sonuçlar ayrı okunsun. */
const MODEL = arg('--model', 'gemini-3.8-flash-tts');
const MS = MODEL === 'gemini-3.8-flash-tts' ? '' : '-lite';
const API = 'https://generativelanguage.googleapis.com/v1beta/';

/* ---- AĞIZ: metnin KENDİSİ ağızla yazılır (v2 · Cemil 17.10: "kelimeleri
   yuvarlayabilir"). TTS metni olduğu gibi okur; usta da gerçekte böyle
   söyler. Beklenen cevap DEĞİŞMEZ — motorun ağızlı söyleyişi anlayıp
   anlamadığı da ölçülür. Rakam ve plaka harfine dokunulmaz. */
function kelimeKelime(metin, f) {
  return metin.split(' ').map(w => /\d/.test(w) || /^[A-ZÇĞİÖŞÜ]{1,3}$/.test(w) ? w : f(w)).join(' ');
}
const AGIZ = {
  /* Karadeniz: geçmiş zamanın 1. kişisi u'ya kayar (aldım → aldum, ettik → ettuk) */
  karadeniz: m => kelimeKelime(m, w => w.replace(/([dt])[ıiuü]([mk])$/i, '$1u$2')),
  /* Ege: -iyor → -iyo, -ecek → -cek (değişecek → değişcek) */
  ege: m => kelimeKelime(m, w => w.replace(/([ıiuü])yor/g, '$1yo').replace(/([^aeıioöuü])[ae](cak|cek)$/i, '$1$2')),
  /* Konuşma dili: -iyor → -iyo, -acak → -cak, "bir şey" → "bişey" */
  konusma: m => kelimeKelime(m.replace(/bir şey/g, 'bişey'), w => w.replace(/([ıiuü])yor/g, '$1yo').replace(/([^aeıioöuü])[ae](cak|cek)$/i, '$1$2')),
  yok: m => m
};

/* Ses tarifleri — Türkçe yazıldı ki model Türk konuşmacı kursun.
   v1 (17.10 pilot): Cemil "yeniden dene şiveleri; hızlı konuşan birileri
   olabilir; kelimeleri yuvarlayabilir" dedi → v2: tarifler belirgin ses
   özellikleriyle, her cümleye KONUŞMA TARZI (speech_metadata.style — hız,
   yutma), metne AĞIZ, iki yeni çok hızlı ses. v1 yeniden üretilebilsin
   diye korunuyor (--surum 1). */
const SESLER_V2 = {
  karadeniz: { cins: 'male', agiz: 'karadeniz', stil: 'speaking very fast and impatiently, with a strong Black Sea Turkish accent, swallowing word endings',
    tarif: "Trabzonlu, 50'lerinde bir oto tamircisi. ÇOK BELİRGİN Karadeniz şivesi: 'ı' sesini 'u'ya, 'e'yi 'a'ya kaydırır, vurguyu kelimenin başına koyar, cümlenin sonunu yükseltir. Hızlı, kesik kesik, telaşlı konuşur; kelimeleri yarım bırakır." },
  ege: { cins: 'male', agiz: 'ege', stil: 'speaking in a relaxed drawl with a strong Aegean Turkish accent, stretching vowels',
    tarif: "Aydınlı, 40'larında bir oto tamircisi. ÇOK BELİRGİN Ege ağzı: ünlüleri uzatır, kelime sonlarını yayarak söyler, '-iyor' yerine '-iyo' der, tonlaması iniş çıkışlı ve şarkı söyler gibidir." },
  dogu: { cins: 'male', agiz: 'konusma', stil: 'speaking fast with a strong Southeastern Turkish accent, hard consonants',
    tarif: "Diyarbakırlı, 30'larında bir oto tamircisi. ÇOK BELİRGİN Güneydoğu aksanı: 'k' ve 'g' seslerini gırtlaktan sert çıkarır, 'e'yi kapalı söyler, vurgusu standart Türkçeden farklıdır. Hızlı konuşur." },
  icanadolu: { cins: 'male', agiz: 'konusma', stil: 'speaking slowly in a low gravelly voice with a strong Central Anatolian Turkish accent, mumbling',
    tarif: "Konyalı, 60'larında yaşlı bir oto tamircisi. ÇOK BELİRGİN İç Anadolu ağzı: ağır ağır, genizden, boğuk ve hafif mırıldanarak konuşur; kelimeleri birbirine bağlar." },
  istanbul: { cins: 'male', agiz: 'konusma', stil: 'speaking extremely fast, slurring and swallowing syllables, shouting over workshop noise',
    tarif: "İstanbullu, 20'lerinde genç bir oto tamircisi. Sanayi ağzıyla, ÇOK hızlı, heceleri yutarak ve kelimeleri birbirine yapıştırarak konuşur; sanki aynı anda başka işle uğraşıyor." },
  kadin: { cins: 'female', agiz: 'yok', stil: 'speaking at a normal pace, clearly',
    tarif: "Ankaralı, 40'larında bir servis danışmanı kadın. Standart Türkçeyle, net ve orta hızda konuşuyor." },
  hizli1: { cins: 'male', agiz: 'konusma', stil: 'speaking as fast as possible, mumbling, half the words barely audible',
    tarif: "35 yaşında, işi çok yoğun bir oto tamircisi. Ağzının içinde, çok hızlı, kelimelerin sonlarını yutarak ve telefonla konuşuyormuş gibi dağınık konuşur." },
  hizli2: { cins: 'male', agiz: 'karadeniz', stil: 'speaking extremely fast with a Black Sea accent, words running together',
    tarif: "Rizeli, 45 yaşında bir oto tamircisi. Karadeniz şivesiyle ve makineli tüfek gibi çok hızlı konuşur; kelimeleri birbirine ekler, nefes almadan söyler." }
};
const SURUM = arg('--surum', '2');
const SESLER = SURUM === '1' ? {
  karadeniz: { cins: 'male', tarif: "Trabzonlu, 50'lerinde bir oto tamircisi. Belirgin Karadeniz şivesiyle, hızlı ve aceleci konuşuyor; ünlüleri Karadeniz ağzıyla söylüyor, cümleleri yutuyor." },
  ege: { cins: 'male', tarif: "Aydınlı, 40'larında bir oto tamircisi. Belirgin Ege ağzıyla, ağır ve rahat konuşuyor; kelime sonlarını uzatıyor, 'gelion, gidion' der gibi söylüyor." },
  dogu: { cins: 'male', tarif: "Diyarbakırlı, 30'larında bir oto tamircisi. Güneydoğu aksanıyla, sert ünsüzlerle ve hızlı konuşuyor." },
  icanadolu: { cins: 'male', tarif: "Konyalı, 60'larında yaşlı bir oto tamircisi. İç Anadolu ağzıyla, yavaş ve tane tane, biraz boğuk sesle konuşuyor." },
  istanbul: { cins: 'male', tarif: "İstanbullu, 20'lerinde genç bir oto tamircisi. Standart İstanbul Türkçesiyle ama çok hızlı, gürültülü bir atölyede bağırır gibi konuşuyor." },
  kadin: { cins: 'female', tarif: "Ankaralı, 40'larında bir servis danışmanı kadın. Standart Türkçeyle, net ve orta hızda konuşuyor." }
} : SESLER_V2;
const ON = SURUM === '1' ? 'yapay-' : 'yapay' + SURUM + '-';   /* klasör ve kimlik öneki */

const anahtar = process.env.GEMINI_API_KEY;
if (!anahtar) { console.error('GEMINI_API_KEY yok — anahtar depoda durmaz, ortamdan okunur.'); process.exit(1); }
const H = { 'x-goog-api-key': anahtar, 'content-type': 'application/json' };
const bekle = ms => new Promise(r => setTimeout(r, ms));
async function iste(url, govde) {
  for (let deneme = 0; ; deneme++) {
    const y = await fetch(url, { method: 'POST', headers: H, body: JSON.stringify(govde) });
    const metin = await y.text();
    if ((y.status === 429 || y.status === 503) && deneme < 5 && !/per_day|PerDay/i.test(metin)) { await bekle(6000 * (deneme + 1)); continue; }
    if (!y.ok) throw new Error(y.status + ' ' + url + ' — ' + metin.slice(0, 500));
    return JSON.parse(metin);
  }
}

/* Ses kimlikleri bir kez oluşturulur, saklanır. */
const KIMLIK_DOSYA = path.join(SES_KOK, 'yapay-ses-kimlikleri.json');
const kimlik = fs.existsSync(KIMLIK_DOSYA) ? JSON.parse(fs.readFileSync(KIMLIK_DOSYA, 'utf8')) : {};
async function sesKimligi(ad) {
  /* Ses kimliği MODELDEN BAĞIMSIZ: lite model ses tasarlayamıyor ama
     tasarlanmış sesi okuyabiliyor (18.10 API cevabı) → aynı ses, iki model. */
  const anahtarAd = ON + ad;
  if (kimlik[anahtarAd]) return kimlik[anahtarAd];
  const s = SESLER[ad];
  const d = await iste(API + 'voices', { store: true, voice: {
    model: 'gemini-3.8-flash-tts', type: 'prompted', display_name: 'tamiron-' + ON + ad, gender: s.cins, language_code: 'tr-TR',
    prompted: { input: s.tarif } } });
  if (!d.id) throw new Error('ses kimliği gelmedi: ' + JSON.stringify(d).slice(0, 300));
  kimlik[anahtarAd] = d.id;
  fs.writeFileSync(KIMLIK_DOSYA, JSON.stringify(kimlik, null, 1));
  if (d.sample_audio && d.sample_audio.data) {
    fs.mkdirSync(path.join(SES_KOK, ON + ad + MS), { recursive: true });
    fs.writeFileSync(path.join(SES_KOK, ON + ad + MS, 'ornek-ses.wav'), Buffer.from(d.sample_audio.data, 'base64'));
  }
  return d.id;
}

async function oku(sesId, metin, stil) {
  const parca = { type: 'text', text: metin };
  /* Konuşma tarzı (hız, yutma) metne değil açıklamaya yazılır — belge:
     "text alanı yalnız okunacak metindir". */
  if (stil) parca.annotations = [{ type: 'speech_metadata', style: stil }];
  const d = await iste(API + 'interactions', {
    model: MODEL,
    input: [{ type: 'user_input', content: [parca] }],
    response_format: { type: 'audio' },
    generation_config: { speech_config: [{ voice: sesId }] }
  });
  let veri = null;
  (d.steps || []).forEach(s => { if (s.type === 'model_output') (s.content || []).forEach(c => { if (c.type === 'audio' && c.data) veri = c.data; }); });
  if (!veri) throw new Error('ses gelmedi — ham yanıt: ' + JSON.stringify(d).slice(0, 400));
  return Buffer.from(veri, 'base64');
}

(async () => {
  const SET = JSON.parse(fs.readFileSync(path.join(KOK, 'buyuk-set.json'), 'utf8'));
  const sinav = SET.cumleler.filter(c => c.yari === 'sinav');
  const secilen = (arg('--sesler', Object.keys(SESLER).join(','))).split(',').filter(a => SESLER[a]);
  let toplam = 0;
  for (let si = 0; si < secilen.length; si++) {
    const ad = secilen[si];
    const id = await sesKimligi(ad);
    const klasor = path.join(SES_KOK, ON + ad + MS);
    const agiz = AGIZ[SESLER[ad].agiz || 'yok'];
    fs.mkdirSync(klasor, { recursive: true });
    /* Her ses FARKLI cümleler okur (kaydırma) — aynı 50 cümle 6 sesle
       değil, 300 farklı cümle; ses başına cümle türleri de karışık. */
    const cumleler = sinav.slice(si * ADET, si * ADET + ADET);
    const dizin = [];
    for (const c of cumleler) {
      const dosya = 's' + c.id + '.wav';
      const yol = path.join(klasor, dosya);
      const okunan = agiz(c.cumle);
      if (!fs.existsSync(yol)) fs.writeFileSync(yol, await oku(id, okunan, SESLER[ad].stil));
      dizin.push({ id: c.id, dosya: dosya, cumle: c.cumle, okunan: okunan, durum: 'iyi', kusurlar: [] });
      process.stdout.write('.'); toplam++;
    }
    fs.writeFileSync(path.join(klasor, 'dizin.json'), JSON.stringify({ hiz: 24000, set: 'buyuk', parti: ON + ad + MS,
      yapay: { model: MODEL, surum: SURUM, ses: ad, tarif: SESLER[ad].tarif, stil: SESLER[ad].stil || null, agiz: SESLER[ad].agiz || null }, kayitlar: dizin }, null, 1));
    console.log(' ' + ad + ': ' + cumleler.length + ' cümle → ' + klasor);
  }
  console.log(toplam + ' kayıt hazır.');
})().catch(e => { console.error('DURDU — ' + e.message); process.exitCode = 2; });
