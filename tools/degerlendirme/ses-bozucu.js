/* ============ SES BOZUCU ============
   Cemil, 16.09: *"Kötü ses kayıtlarını nasıl bulabiliriz?"*

   İki cevabı var ve ikisi de gerekli:

     BULMAK   — eldeki kaydın kötü olduğunu anlamak    -> ses-olcu.js
     ÜRETMEK  — kötü kaydı bilerek yapmak              -> bu dosya

   Neden üretiyoruz: ustayı kırk cümleyi bir de kompresörün yanında,
   bir de telefon ahizesinde, bir de bağırarak söylemeye zorlayamayız.
   Ama TEMİZ kaydı kirletmek kolay ve **kontrollü**: "12 dB gürültüde
   tanıma %70'e düşüyor, 6 dB'de %30'a" gibi bir eğri ancak gürültü
   miktarını biz seçersek çıkar. Dükkânda kaydedilen rastgele gürültü
   bunu vermez — ne kadar gürültü olduğunu bilmiyoruz ki.

   ⚠️ GÜRÜLTÜ UYDURULMAZ. Karıştırılan ses, kayıt sayfasının ilk
   adımında alınan **dükkânın kendi 20 saniyesidir** (`ortam.wav`).
   Beyaz gürültü kompresörü de radyoyu da taklit etmiyor; onunla
   ölçülen dayanıklılık gerçekte tutmaz.

     node ses-bozucu.js <kayitlar-klasoru> [--cikti <klasor>]

   Üretilenler (her kayıt için):
     <ad>.snr18.wav   hafif dükkân gürültüsü
     <ad>.snr12.wav   normal dükkân
     <ad>.snr6.wav    kompresör çalışıyor
     <ad>.uzak.wav    telefon tezgâhta, usta iki adım ötede
     <ad>.kirpik.wav  mikrofona çok yakın / bağırarak */

const fs = require('fs');
const path = require('path');
const olcu = require('./ses-olcu.js');

/* ---------- WAV oku/yaz (16-bit PCM mono) ---------- */
function wavOku(yol) {
  const b = fs.readFileSync(yol);
  if (b.toString('ascii', 0, 4) !== 'RIFF' || b.toString('ascii', 8, 12) !== 'WAVE') {
    throw new Error(path.basename(yol) + ': WAV değil');
  }
  let hiz = 16000, kanal = 1, bit = 16, veriBas = -1, veriBoy = 0, i = 12;
  while (i + 8 <= b.length) {
    const tip = b.toString('ascii', i, i + 4);
    const boy = b.readUInt32LE(i + 4);
    if (tip === 'fmt ') {
      kanal = b.readUInt16LE(i + 10);
      hiz = b.readUInt32LE(i + 12);
      bit = b.readUInt16LE(i + 22);
    } else if (tip === 'data') { veriBas = i + 8; veriBoy = boy; break; }
    i += 8 + boy + (boy % 2);
  }
  if (veriBas < 0) throw new Error(path.basename(yol) + ': data parçası yok');
  if (bit !== 16) throw new Error(path.basename(yol) + ': yalnız 16-bit destekleniyor');

  const n = Math.floor(veriBoy / 2 / kanal);
  const a = new Float32Array(n);
  for (let k = 0; k < n; k++) {
    /* Çok kanallıysa kanallar ortalanır — tek kanala indirmek, bir
       kanalı atmaktan iyidir: atılan kanalda konuşma olabilir. */
    let t = 0;
    for (let c = 0; c < kanal; c++) t += b.readInt16LE(veriBas + (k * kanal + c) * 2) / 32768;
    a[k] = t / kanal;
  }
  return { ornekler: a, hiz };
}

function wavYaz(yol, ornekler, hiz) {
  const n = ornekler.length;
  const b = Buffer.alloc(44 + n * 2);
  b.write('RIFF', 0, 'ascii'); b.writeUInt32LE(36 + n * 2, 4); b.write('WAVE', 8, 'ascii');
  b.write('fmt ', 12, 'ascii'); b.writeUInt32LE(16, 16); b.writeUInt16LE(1, 20);
  b.writeUInt16LE(1, 22); b.writeUInt32LE(hiz, 24); b.writeUInt32LE(hiz * 2, 28);
  b.writeUInt16LE(2, 32); b.writeUInt16LE(16, 34);
  b.write('data', 36, 'ascii'); b.writeUInt32LE(n * 2, 40);
  for (let i = 0; i < n; i++) {
    const v = Math.max(-1, Math.min(1, ornekler[i]));
    b.writeInt16LE(Math.round(v < 0 ? v * 32768 : v * 32767), 44 + i * 2);
  }
  fs.writeFileSync(yol, b);
  return b.length;
}

/* ---------- bozmalar ---------- */
function rms(a, bas, son) {
  let t = 0;
  for (let i = bas; i < son; i++) t += a[i] * a[i];
  return Math.sqrt(t / Math.max(1, son - bas));
}

function gurultuKat(ornekler, ortam, hedefSnrDb, hiz) {
  /* ⚠️ Seviyeler ses-olcu.js'ten geliyor — kendi tahmincimizi yazmıyoruz.
     Yazdığımızda "6 dB" etiketli dosya 8,4 dB ölçülüyordu: etiket ile
     cetvel ayrı tanım kullanınca eğri kayıyor ve kimse fark etmiyor. */
  const konusma = olcu.seviyeler(ornekler, hiz).konusma;
  const ortamSev = olcu.seviyeler(ortam, hiz).taban || rms(ortam, 0, ortam.length) || 1e-6;
  const hedefGurultu = konusma / Math.pow(10, hedefSnrDb / 20);
  const k = hedefGurultu / ortamSev;
  const c = new Float32Array(ornekler.length);
  /* Ortam kaydı kısaysa başa sarılır — dükkân sesi zaten döngüsel. */
  for (let i = 0; i < ornekler.length; i++) {
    c[i] = ornekler[i] + ortam[i % ortam.length] * k;
  }
  return c;
}

/* Uzaklık: seviye düşer ve tizler söner (yüksek frekanslar havada
   daha çabuk yutulur). Tek kutuplu alçak geçiren yeterli — amaç
   gerçekçi bir akustik model değil, tanımayı zorlamak. */
function uzaklastir(a, dbDusus, hiz) {
  const kazanc = Math.pow(10, -Math.abs(dbDusus) / 20);
  const kesim = 2200;
  const alfa = 1 / (1 + hiz / (2 * Math.PI * kesim));
  const c = new Float32Array(a.length);
  let onceki = 0;
  for (let i = 0; i < a.length; i++) {
    onceki = onceki + alfa * (a[i] - onceki);
    c[i] = onceki * kazanc;
  }
  return c;
}

function kirp(a, kazanc) {
  const c = new Float32Array(a.length);
  for (let i = 0; i < a.length; i++) c[i] = Math.max(-1, Math.min(1, a[i] * kazanc));
  return c;
}

/* ---------- koşucu ---------- */
const arg = (ad, v) => { const i = process.argv.indexOf(ad); return i > 0 ? process.argv[i + 1] : v; };
const KAYNAK = process.argv[2];
if (!KAYNAK || !fs.existsSync(KAYNAK)) {
  console.error('kullanım: node ses-bozucu.js <kayitlar-klasoru> [--cikti <klasor>]');
  process.exit(1);
}
const CIKTI = arg('--cikti', path.join(KAYNAK, 'bozuk'));
fs.mkdirSync(CIKTI, { recursive: true });

const ortamYol = path.join(KAYNAK, 'ortam.wav');
let ortam = null;
if (fs.existsSync(ortamYol)) {
  ortam = wavOku(ortamYol).ornekler;
  console.log('dükkân sesi: ortam.wav · ' + (ortam.length / 16000).toFixed(1) + ' sn');
} else {
  console.error('⚠️ ortam.wav YOK — gürültü varyantları üretilmeyecek.');
  console.error('   Kayıt sayfasının ilk adımı (dükkânın sesi) atlanmış olmalı.');
  console.error('   Beyaz gürültüyle doldurmuyorum: uydurma gürültüyle ölçülen');
  console.error('   dayanıklılık gerçekte tutmaz ve bunu fark etmek zor olur.');
}

const dosyalar = fs.readdirSync(KAYNAK)
  .filter((f) => /^s\d+\.wav$/i.test(f))
  .sort();

if (!dosyalar.length) { console.error('kayıt bulunamadı (s01.wav, s02.wav …)'); process.exit(1); }

let uretilen = 0;
const karne = { iyi: 0, zayif: 0, kotu: 0 };

dosyalar.forEach((f) => {
  const { ornekler, hiz } = wavOku(path.join(KAYNAK, f));
  const taban = f.replace(/\.wav$/i, '');

  const varyantlar = [
    ['uzak', uzaklastir(ornekler, 12, hiz)],
    ['kirpik', kirp(ornekler, 6)]
  ];
  if (ortam) {
    [18, 12, 6].forEach((snr) => {
      varyantlar.push(['snr' + snr, gurultuKat(ornekler, ortam, snr, hiz)]);
    });
  }

  varyantlar.forEach(([etiket, sinyal]) => {
    const ad = taban + '.' + etiket + '.wav';
    wavYaz(path.join(CIKTI, ad), sinyal, hiz);
    karne[olcu.degerlendir(sinyal, hiz).durum]++;
    uretilen++;
  });
});

console.log(dosyalar.length + ' kayıt -> ' + uretilen + ' bozuk varyant · ' + CIKTI);
console.log('  ölçü: iyi ' + karne.iyi + ' · zayıf ' + karne.zayif + ' · kötü ' + karne.kotu);
console.log('\nBunlar ölçütün ZOR ucu: tanıma burada düşerse dükkânda da düşer.');
