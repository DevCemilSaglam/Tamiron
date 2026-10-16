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
const MODEL = 'gemini-3.8-flash-tts';
const API = 'https://generativelanguage.googleapis.com/v1beta/';

/* Ses tarifleri — Türkçe yazıldı ki model Türk konuşmacı kursun. */
const SESLER = {
  karadeniz: { cins: 'male', tarif: "Trabzonlu, 50'lerinde bir oto tamircisi. Belirgin Karadeniz şivesiyle, hızlı ve aceleci konuşuyor; ünlüleri Karadeniz ağzıyla söylüyor, cümleleri yutuyor." },
  ege: { cins: 'male', tarif: "Aydınlı, 40'larında bir oto tamircisi. Belirgin Ege ağzıyla, ağır ve rahat konuşuyor; kelime sonlarını uzatıyor, 'gelion, gidion' der gibi söylüyor." },
  dogu: { cins: 'male', tarif: "Diyarbakırlı, 30'larında bir oto tamircisi. Güneydoğu aksanıyla, sert ünsüzlerle ve hızlı konuşuyor." },
  icanadolu: { cins: 'male', tarif: "Konyalı, 60'larında yaşlı bir oto tamircisi. İç Anadolu ağzıyla, yavaş ve tane tane, biraz boğuk sesle konuşuyor." },
  istanbul: { cins: 'male', tarif: "İstanbullu, 20'lerinde genç bir oto tamircisi. Standart İstanbul Türkçesiyle ama çok hızlı, gürültülü bir atölyede bağırır gibi konuşuyor." },
  kadin: { cins: 'female', tarif: "Ankaralı, 40'larında bir servis danışmanı kadın. Standart Türkçeyle, net ve orta hızda konuşuyor." }
};

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
  if (kimlik[ad]) return kimlik[ad];
  const s = SESLER[ad];
  const d = await iste(API + 'voices', { store: true, voice: {
    model: MODEL, type: 'prompted', display_name: 'tamiron-' + ad, gender: s.cins, language_code: 'tr-TR',
    prompted: { input: s.tarif } } });
  if (!d.id) throw new Error('ses kimliği gelmedi: ' + JSON.stringify(d).slice(0, 300));
  kimlik[ad] = d.id;
  fs.writeFileSync(KIMLIK_DOSYA, JSON.stringify(kimlik, null, 1));
  if (d.sample_audio && d.sample_audio.data) {
    fs.mkdirSync(path.join(SES_KOK, 'yapay-' + ad), { recursive: true });
    fs.writeFileSync(path.join(SES_KOK, 'yapay-' + ad, 'ornek-ses.wav'), Buffer.from(d.sample_audio.data, 'base64'));
  }
  return d.id;
}

async function oku(sesId, metin) {
  const d = await iste(API + 'interactions', {
    model: MODEL,
    input: [{ type: 'user_input', content: [{ type: 'text', text: metin }] }],
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
    const klasor = path.join(SES_KOK, 'yapay-' + ad);
    fs.mkdirSync(klasor, { recursive: true });
    /* Her ses FARKLI cümleler okur (kaydırma) — aynı 50 cümle 6 sesle
       değil, 300 farklı cümle; ses başına cümle türleri de karışık. */
    const cumleler = sinav.slice(si * ADET, si * ADET + ADET);
    const dizin = [];
    for (const c of cumleler) {
      const dosya = 's' + c.id + '.wav';
      const yol = path.join(klasor, dosya);
      if (!fs.existsSync(yol)) fs.writeFileSync(yol, await oku(id, c.cumle));
      dizin.push({ id: c.id, dosya: dosya, cumle: c.cumle, durum: 'iyi', kusurlar: [] });
      process.stdout.write('.'); toplam++;
    }
    fs.writeFileSync(path.join(klasor, 'dizin.json'), JSON.stringify({ hiz: 24000, set: 'buyuk', parti: 'yapay-' + ad,
      yapay: { model: MODEL, ses: ad, tarif: SESLER[ad].tarif }, kayitlar: dizin }, null, 1));
    console.log(' ' + ad + ': ' + cumleler.length + ' cümle → ' + klasor);
  }
  console.log(toplam + ' kayıt hazır.');
})().catch(e => { console.error('DURDU — ' + e.message); process.exitCode = 2; });
