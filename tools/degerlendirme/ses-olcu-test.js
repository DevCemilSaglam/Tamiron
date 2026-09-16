/* ses-olcu.js için sınav. Gerçek kayıt beklemeden koşar: sinyaller
   BİLİNEREK üretiliyor, dolayısıyla doğru cevap da biliniyor.

   Neden gerekli: ölçü üç yerde kullanılıyor (kayıt sayfası, set
   taraması, ileride ürün). Eşiği biri kaydırırsa üçü birden kayar ve
   bu sessizce olur — kayıt "iyi" görünür, model saçmalar, suç modele
   kalır.

     node tools/degerlendirme/ses-olcu-test.js */

const olcu = require('./ses-olcu.js');

const HIZ = 16000;

/* --- sinyal üreteçleri --- */
function bos(sn) { return new Float32Array(Math.round(HIZ * sn)); }

function gurultu(sn, seviye) {
  const a = bos(sn);
  for (let i = 0; i < a.length; i++) a[i] = (Math.random() * 2 - 1) * seviye;
  return a;
}

/* Konuşma taklidi: sessizlik–ses–sessizlik, ses kısmı 200 Hz taşıyıcı
   üstünde genlik oynamalı. Gerçek konuşma değil; ölçünün konuşmayı
   gürültüden AYIRIP ayıramadığını sınamaya yetiyor. */
function konusma(sn, seviye, gurultuSeviye) {
  const a = gurultu(sn, gurultuSeviye || 0);
  const bas = Math.round(a.length * 0.2), son = Math.round(a.length * 0.8);
  for (let i = bas; i < son; i++) {
    const zarf = 0.5 + 0.5 * Math.sin((i - bas) / HIZ * 2 * Math.PI * 3);
    a[i] += Math.sin(i / HIZ * 2 * Math.PI * 200) * seviye * zarf;
  }
  return a;
}

function kirp(a) {
  const b = new Float32Array(a.length);
  for (let i = 0; i < a.length; i++) b[i] = Math.max(-1, Math.min(1, a[i] * 6));
  return b;
}

/* --- sınavlar: [ad, sinyal, beklenen durum, beklenen kusur kodu] --- */
const SINAVLAR = [
  ['temiz konuşma',        konusma(3, 0.3, 0.002), 'iyi',   null],
  ['hiç ses yok',          bos(3),                 'kotu',  'sessiz'],
  ['çok kısık (uzak)',     konusma(3, 0.004, 0.0005), 'kotu', 'sessiz'],
  ['çok kısa',             konusma(0.3, 0.3, 0.002), 'kotu', 'kisa'],
  ['kırpılmış (çok yakın)', kirp(konusma(3, 0.3, 0.002)), 'kotu', 'kirpik'],
  ['gürültülü ama duyulur', konusma(3, 0.12, 0.03), 'zayif', 'gurultulu'],
  ['gürültüye gömülmüş',   konusma(3, 0.05, 0.05),  'kotu',  'konusma-yok'],
  ['sadece gürültü',       gurultu(3, 0.05),        'kotu',  'konusma-yok']
];

let gecen = 0, kalan = [];
SINAVLAR.forEach(([ad, sinyal, beklenenDurum, beklenenKod]) => {
  const s = olcu.degerlendir(sinyal, HIZ);
  const kodlar = s.kusurlar.map((k) => k.kod);
  const durumTamam = s.durum === beklenenDurum;
  const kodTamam = beklenenKod === null ? kodlar.length === 0 : kodlar.indexOf(beklenenKod) >= 0;
  if (durumTamam && kodTamam) {
    gecen++;
    console.log('  ✓ ' + ad.padEnd(24) + s.durum +
      '   snr ' + s.olcum.snrDb.toFixed(0) + ' dB · tepe ' + s.olcum.tepeDb.toFixed(0) + ' dB');
  } else {
    kalan.push(ad);
    console.log('  ✗ ' + ad.padEnd(24) + 'bulunan: ' + s.durum + ' [' + kodlar.join(',') +
      ']  beklenen: ' + beklenenDurum + ' [' + (beklenenKod || '-') + ']');
  }
});

console.log('\n' + gecen + '/' + SINAVLAR.length + ' geçti');
if (kalan.length) { console.error('KALAN: ' + kalan.join(', ')); process.exit(1); }
