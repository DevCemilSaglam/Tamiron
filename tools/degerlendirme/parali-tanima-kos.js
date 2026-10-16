/* ============ PARALI TANIYICI KOŞUCUSU (16.10.2026) ============
   Cemil, 16.10: *"Paralı tanıyıcı seçeneklerini araştır, maliyet ver."*
   → *"Tamam, deneyelim."* (arastirma/PARALI-TANIMA.md)

   Büyük setin 300 gerçek kaydını (sesler/buyuk-1..6) bir paralı tanıyıcıya
   verir; DÖKÜMLERİ ses-dosya-kos.html ile AYNI biçimde yazar ki aynı motor,
   aynı cetvel puanlasın (puanlama tarayıcıda: tarayicida-kos.js).

     node tools/degerlendirme/parali-tanima-kos.js --saglayici assemblyai
     node tools/degerlendirme/parali-tanima-kos.js --saglayici speechmatics --ipucu
     (--partiler 1-6 · varsayılan hepsi; --sinir N · ilk N kayıt, deneme için)

   ⚠️ ANAHTAR DEPODA DURMAZ — ortamdan: ASSEMBLYAI_API_KEY,
   SPEECHMATICS_API_KEY, GEMINI_API_KEY. ⚠️ GERÇEK PARA: 300 kayıt ≈ 20 dk ses ≈ $0,05–0,50
   sağlayıcı başına (Cemil onayladı, 16.10).
   ⚠️ İLK API HATASINDA DURUR ve hatayı olduğu gibi basar — Chrome'daki
   ipucu koşusu 300 kaydı aynı hatayla boşuna çalmıştı (SES-OLCUM §16).

   --ipucu: dükkânın plakaları + marka/model + terimler
   (betik/05-soyleyis.js SOYLEYIS_IFADELER — prototipin kendi listesi). */
'use strict';
const fs = require('fs');
const path = require('path');

const KOK = __dirname;
const SES = path.join(KOK, 'sesler');
const arg = (ad, v) => { const i = process.argv.indexOf(ad); return i > 0 && process.argv[i + 1] && !process.argv[i + 1].startsWith('--') ? process.argv[i + 1] : v; };
const var_ = ad => process.argv.indexOf(ad) > 0;

const SAGLAYICI = arg('--saglayici', '');
const IPUCU = var_('--ipucu');
const [P1, P2] = arg('--partiler', '1-6').split('-').map(Number);
const SINIR = parseInt(arg('--sinir', '0'), 10) || 0;
/* --model: Gemini modeli. Varsayılan yazıya çevirme modeli; ücretsiz katmanda
   yoksa genel bir Flash modeli verilir (istemle çevirir). */
const GEMINI_MODEL = arg('--model', 'gemini-3.5-transcribe');
const ES_ZAMAN = 4;

/* ---- kayıtlar ---- */
const SET = JSON.parse(fs.readFileSync(path.join(KOK, 'buyuk-set.json'), 'utf8'));
const CUMLE = {}; SET.cumleler.forEach(c => { CUMLE[c.id] = c; });
let isler = [];
for (let p = P1; p <= (P2 || P1); p++) {
  const klasor = path.join(SES, 'buyuk-' + p);
  if (!fs.existsSync(path.join(klasor, 'dizin.json'))) { console.error('yok: ' + klasor); continue; }
  const dz = JSON.parse(fs.readFileSync(path.join(klasor, 'dizin.json'), 'utf8'));
  dz.kayitlar.filter(k => /^s\d+\.wav$/i.test(k.dosya) && CUMLE[k.id])
    .forEach(k => isler.push({ parti: p, id: k.id, dosya: path.join(klasor, k.dosya), ad: k.dosya }));
}
if (SINIR) isler = isler.slice(0, SINIR);

/* ---- ipucu listesi: prototipin KENDİ sözlüğü ---- */
let IFADELER = [];
if (IPUCU) {
  const kap = {};
  new Function('k', fs.readFileSync(path.join(KOK, '..', '..', 'prototypes', 'kaynak', 'betik', '05-soyleyis.js'), 'utf8') +
    '\nk.f = SOYLEYIS_IFADELER;')(kap);
  IFADELER = kap.f(SET.dukkan.araclar);
}

/* ---- kelime hatası (ses-dosya-kos.html ile aynı fikir) ---- */
function kelimeler(s) {
  return (s || '').toLocaleLowerCase('tr').replace(/[.,!?;:'"]/g, ' ').split(/\s+/).filter(Boolean);
}
function kho(dogru, duyulan) {
  const a = kelimeler(dogru), b = kelimeler(duyulan);
  if (!a.length) return b.length ? 1 : 0;
  const d = []; for (let j = 0; j <= b.length; j++) d[j] = j;
  for (let i = 1; i <= a.length; i++) {
    let onceki = d[0]; d[0] = i;
    for (let j = 1; j <= b.length; j++) {
      const t = d[j];
      d[j] = Math.min(d[j] + 1, d[j - 1] + 1, onceki + (a[i - 1] === b[j - 1] ? 0 : 1));
      onceki = t;
    }
  }
  return Math.min(1, d[b.length] / a.length);
}

const bekle = ms => new Promise(r => setTimeout(r, ms));
class ApiHatasi extends Error {}
/* 429 (dakikalık sınır) ve 503 (geçici yoğunluk) KALICI DEĞİL: bekleyip
   yeniden dener (en çok 6 kez, artan bekleme). Diğer her hata kalıcı → dur. */
async function iste(url, secenek) {
  for (let deneme = 0; ; deneme++) {
    const y = await fetch(url, secenek);
    const metin = await y.text();
    if ((y.status === 429 || y.status === 503) && deneme < 6) { await bekle(5000 * (deneme + 1)); continue; }
    if (!y.ok) throw new ApiHatasi(y.status + ' ' + url + ' — ' + metin.slice(0, 500));
    try { return JSON.parse(metin); } catch (e) { return metin; }
  }
}

/* ---- SAĞLAYICILAR ---- */
const SAG = {
  /* AssemblyAI — api.assemblyai.com · /v2/upload → /v2/transcript → bekle.
     speech_models: önce universal-3-pro, desteklemiyorsa universal-2'ye
     düşer (belge). keyterms_prompt: 5–50 karakter dışı yok sayılır. */
  assemblyai: {
    anahtar: 'ASSEMBLYAI_API_KEY',
    async tani(dosya, anahtar) {
      const H = { authorization: anahtar };
      const yuk = await iste('https://api.assemblyai.com/v2/upload',
        { method: 'POST', headers: Object.assign({ 'content-type': 'application/octet-stream' }, H), body: fs.readFileSync(dosya) });
      const govde = { audio_url: yuk.upload_url, speech_models: ['universal-3-pro', 'universal-2'], language_code: 'tr' };
      if (IPUCU) govde.keyterms_prompt = IFADELER.filter(s => s.length >= 5 && s.length <= 50).slice(0, 200);
      const is = await iste('https://api.assemblyai.com/v2/transcript',
        { method: 'POST', headers: Object.assign({ 'content-type': 'application/json' }, H), body: JSON.stringify(govde) });
      for (let n = 0; n < 120; n++) {
        await bekle(1000);
        const d = await iste('https://api.assemblyai.com/v2/transcript/' + is.id, { headers: H });
        if (d.status === 'completed') return { yazi: d.text || '', model: d.speech_model || d.speech_model_used || null };
        if (d.status === 'error') throw new ApiHatasi('transcript error — ' + d.error);
      }
      throw new ApiHatasi('zaman aşımı ' + is.id);
    }
  },
  /* Google Gemini 3.5 Transcribe (16.10 · K-061.3'ün ön ödemesiz yolu):
     generativelanguage.googleapis.com …/models/gemini-3.5-transcribe:
     generateContent · ses inlineData (base64, <20 MB) · ipucu
     generationConfig.audioTranscriptionConfig.customVocabulary (≤1.000
     terim) · dil languageCodes. Metin parts[].text; yoksa
     audioTranscription.words birleştirilir.
     ⚠️ Ücretsiz katman: içerik Google'ın ürün geliştirmesinde kullanılabilir
     — yalnız test kaydı (Cemil'in okuduğu kurmaca plakalar); yayında DEĞİL.
     Dakikalık istek sınırı → tek tek gider, 429'da bekleyip yeniden dener. */
  gemini: {
    anahtar: 'GEMINI_API_KEY',
    esZaman: 1,
    async tani(dosya, anahtar) {
      const ses = { inlineData: { mimeType: 'audio/wav', data: fs.readFileSync(dosya).toString('base64') } };
      let govde;
      if (/transcribe/.test(GEMINI_MODEL)) {
        const atc = { languageCodes: ['tr-TR'] };
        if (IPUCU) atc.customVocabulary = IFADELER.slice(0, 1000);
        govde = { contents: [{ parts: [ses] }], generationConfig: { audioTranscriptionConfig: atc } };
      } else {
        /* Genel Gemini modeli (ör. Flash — ücretsiz katman): yazıya çevirme
           İSTEMLE istenir; ipucu istemin içinde. Yorum yok, düz metin. */
        let istem = 'Bu Türkçe ses kaydını kelimesi kelimesine yazıya çevir. Yalnız söyleneni yaz; ' +
                    'yorum, açıklama, tırnak ekleme. Sayıları rakamla yaz.';
        if (IPUCU) istem += '\nKayıtta geçebilecek özel kelimeler (doğru yazımlarıyla): ' + IFADELER.join(', ');
        govde = { contents: [{ parts: [{ text: istem }, ses] }] };
      }
      const d = await iste('https://generativelanguage.googleapis.com/v1beta/models/' + GEMINI_MODEL + ':generateContent',
        { method: 'POST', headers: { 'x-goog-api-key': anahtar, 'content-type': 'application/json' }, body: JSON.stringify(govde) });
      const parcalar = (((d.candidates || [])[0] || {}).content || {}).parts || [];
      /* Yazıya çevirme modeli metni parts[].audioTranscription.text'te verir
         (16.10 ham yanıt); genel model parts[].text'te. */
      let yazi = parcalar.map(p => p.text || (p.audioTranscription || {}).text || '').join(' ').trim();
      if (!yazi) {
        const k = [];
        parcalar.forEach(p => ((p.audioTranscription || {}).words || []).forEach(w => k.push(w.word)));
        yazi = k.join(' ').trim();
      }
      /* ⚠️ BOŞ YANIT HATADIR (16.10): ilk denemede metinsiz bir yanıt
         "duyulan: ''" diye yazıldı ve kelime hatası %100 sayıldı — ölçüm
         sessizce bozulurdu. Ham yanıtın başı hataya yazılır. */
      if (!yazi) throw new ApiHatasi('metin yok — ham yanıt: ' + JSON.stringify(d).slice(0, 400));
      return { yazi: yazi, model: GEMINI_MODEL };
    }
  },
  /* Speechmatics — asr.api.speechmatics.com/v2/jobs (multipart: data_file +
     config), durum GET /v2/jobs/{id}, metin GET …/transcript?format=txt.
     additional_vocab: {content}; 6 sözcükten uzunu düşer. */
  speechmatics: {
    anahtar: 'SPEECHMATICS_API_KEY',
    async tani(dosya, anahtar) {
      const H = { Authorization: 'Bearer ' + anahtar };
      const tc = { language: 'tr', operating_point: 'enhanced' };
      if (IPUCU) tc.additional_vocab = IFADELER.map(s => ({ content: s }));
      const form = new FormData();
      form.append('data_file', new Blob([fs.readFileSync(dosya)], { type: 'audio/wav' }), path.basename(dosya));
      form.append('config', JSON.stringify({ type: 'transcription', transcription_config: tc }));
      const is = await iste('https://asr.api.speechmatics.com/v2/jobs/', { method: 'POST', headers: H, body: form });
      for (let n = 0; n < 120; n++) {
        await bekle(1500);
        const d = await iste('https://asr.api.speechmatics.com/v2/jobs/' + is.id, { headers: H });
        const st = d.job && d.job.status;
        if (st === 'done') {
          const t = await iste('https://asr.api.speechmatics.com/v2/jobs/' + is.id + '/transcript?format=txt', { headers: H });
          return { yazi: String(t).trim(), model: 'enhanced' };
        }
        if (st === 'rejected' || st === 'deleted' || st === 'expired') throw new ApiHatasi('job ' + st + ' — ' + JSON.stringify(d.job.errors || ''));
      }
      throw new ApiHatasi('zaman aşımı ' + is.id);
    }
  }
};

(async () => {
  const S = SAG[SAGLAYICI];
  if (!S) { console.error('--saglayici ' + Object.keys(SAG).join(' | ')); process.exit(1); }
  const anahtar = process.env[S.anahtar];
  if (!anahtar) { console.error(S.anahtar + ' yok — anahtar depoda durmaz, ortamdan okunur.'); process.exit(1); }
  if (!isler.length) { console.error('kayıt yok (sesler/buyuk-N/)'); process.exit(1); }

  const sn = isler.reduce((t, x) => t + Math.max(0, fs.statSync(x.dosya).size - 44) / 32000, 0);
  console.log(SAGLAYICI + (IPUCU ? ' + ipucu (' + IFADELER.length + ' ifade)' : '') + ' · ' + isler.length +
              ' kayıt · ≈' + (sn / 60).toFixed(1) + ' dk ses');

  const sonuc = new Array(isler.length);
  let sira = 0, durdu = null, bitti = 0;
  async function isci() {
    while (!durdu && sira < isler.length) {
      const k = sira++, x = isler[k], c = CUMLE[x.id];
      try {
        const r = await S.tani(x.dosya, anahtar);
        sonuc[k] = { dosya: x.ad, id: x.id, parti: x.parti, tur: 'temiz', beklenen: c.cumle,
                     duyulan: r.yazi, hata: null, kho: kho(c.cumle, r.yazi), model: r.model };
      } catch (e) {
        if (!durdu) durdu = e;   /* İLK HATADA DUR */
      }
      process.stdout.write(++bitti % 10 ? '' : '.');
    }
  }
  await Promise.all(Array.from({ length: S.esZaman || ES_ZAMAN }, isci));
  process.stdout.write('\n');
  const tamam = sonuc.filter(Boolean);
  if (durdu) console.error('\nDURDU — ' + durdu.message + '\n(' + tamam.length + ' kayıt tamamlanmıştı)');

  const ort = tamam.length ? tamam.reduce((t, x) => t + x.kho, 0) / tamam.length : null;
  const dosya = path.join(SES, 'parali-' + SAGLAYICI + (SAGLAYICI === 'gemini' && GEMINI_MODEL !== 'gemini-3.5-transcribe' ? '-' + GEMINI_MODEL : '') + (IPUCU ? '-ipucu' : '') + '.json');
  fs.writeFileSync(dosya, JSON.stringify({ tarih: new Date().toISOString(), saglayici: SAGLAYICI, ipucu: IPUCU,
    ifadeSayisi: IFADELER.length, kayit: tamam.length, ortalamaKho: ort, sonuc: tamam }, null, 1), 'utf8');
  console.log(tamam.length + ' kayıt · ortalama kelime hatası ' + (ort === null ? '—' : '%' + Math.round(ort * 100)));
  console.log('→ ' + dosya);
  if (durdu) process.exitCode = 2;
})();
