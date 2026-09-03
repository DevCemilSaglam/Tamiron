/* Tamiron uygulama ikonu -> PNG (harici bagimlilik yok; node + zlib yeter).
   Sekil marka somununun sadelestirilmis hali: yuvarlak kosreli koyu zemin
   uzerinde teal altigen + orta delik. 4x superornekleme ile kenarlar yumusar. */
const zlib = require('zlib');
const fs = require('fs');
const path = require('path');

const ZEMIN = [0x0f, 0x1e, 0x30];   // --text koyu lacivert
const TEAL = [0x27, 0x96, 0x89];   // --brand-teal
const TEAL_A = [0x34, 0xc1, 0xaf];   // ust yuz (hafif acik)

function karistir(a, b, t) {
  return [
    Math.round(a[0] + (b[0] - a[0]) * t),
    Math.round(a[1] + (b[1] - a[1]) * t),
    Math.round(a[2] + (b[2] - a[2]) * t)
  ];
}

/* Yuvarlak koseli kare icinde mi? */
function kareIcinde(x, y, boyut, r) {
  const ix = Math.min(x, boyut - x), iy = Math.min(y, boyut - y);
  if (ix >= r || iy >= r) return ix >= 0 && iy >= 0;
  const dx = r - ix, dy = r - iy;
  return dx * dx + dy * dy <= r * r;
}

/* Duzgun altigen icinde mi? (tepe yukari) */
function altigenIcinde(x, y, cx, cy, R, aci) {
  const dx = x - cx, dy = y - cy;
  const c = Math.cos(-aci), s = Math.sin(-aci);
  const px = dx * c - dy * s, py = dx * s + dy * c;
  const q = Math.abs(px), w = Math.abs(py);
  /* apothem = R*cos(30) */
  const a = R * Math.cos(Math.PI / 6);
  return w <= a && q * Math.cos(Math.PI / 6) + w * 0.5 <= a;
}

/* maskeli=true  -> Android "maskable" ikonu:
   kose yuvarlatmasi YOK (maske kirpinca kosede seffaflik kalmasin) ve
   somun kucultulur ki kirpma sonrasi guvenli alanda (%80) kalsin. */
function ikonUret(boyut, maskeli) {
  const SS = 4;                     /* superornekleme */
  const N = boyut * SS;
  const r = maskeli ? 0 : N * 0.22; /* kose yaricapi */
  const cx = N / 2, cy = N / 2;
  const R = N * (maskeli ? 0.26 : 0.335);   /* altigen yaricapi */
  const delik = N * (maskeli ? 0.105 : 0.135);
  const aci = 0.18;                 /* markadaki hafif egim */

  const ham = Buffer.alloc(boyut * boyut * 4);
  for (let y = 0; y < boyut; y++) {
    for (let x = 0; x < boyut; x++) {
      let rs = 0, gs = 0, bs = 0, as = 0;
      for (let sy = 0; sy < SS; sy++) {
        for (let sx = 0; sx < SS; sx++) {
          const px = x * SS + sx + 0.5, py = y * SS + sy + 0.5;
          if (!kareIcinde(px, py, N, r)) continue;      /* disi seffaf */
          let renk = ZEMIN;
          if (altigenIcinde(px, py, cx, cy, R, aci)) {
            const d = Math.hypot(px - cx, py - cy);
            if (d > delik) {
              /* ust yarida hafif acik ton — somunun hacmi */
              renk = karistir(TEAL, TEAL_A, Math.max(0, (cy - py) / R) * 0.75);
            }
          }
          rs += renk[0]; gs += renk[1]; bs += renk[2]; as += 255;
        }
      }
      const n = SS * SS, i = (y * boyut + x) * 4;
      if (as === 0) { ham[i] = ham[i + 1] = ham[i + 2] = ham[i + 3] = 0; continue; }
      const kapsam = as / (n * 255);
      ham[i] = Math.round(rs / (as / 255));
      ham[i + 1] = Math.round(gs / (as / 255));
      ham[i + 2] = Math.round(bs / (as / 255));
      ham[i + 3] = Math.round(kapsam * 255);
    }
  }
  return ham;
}

/* --- PNG yazici --- */
const CRC = (() => {
  const t = new Int32Array(256);
  for (let n = 0; n < 256; n++) {
    let c = n;
    for (let k = 0; k < 8; k++) c = c & 1 ? 0xedb88320 ^ (c >>> 1) : c >>> 1;
    t[n] = c;
  }
  return t;
})();
function crc32(buf) {
  let c = 0xffffffff;
  for (let i = 0; i < buf.length; i++) c = CRC[(c ^ buf[i]) & 0xff] ^ (c >>> 8);
  return (c ^ 0xffffffff) >>> 0;
}
function parca(tip, veri) {
  const uz = Buffer.alloc(4); uz.writeUInt32BE(veri.length, 0);
  const govde = Buffer.concat([Buffer.from(tip, 'ascii'), veri]);
  const c = Buffer.alloc(4); c.writeUInt32BE(crc32(govde), 0);
  return Buffer.concat([uz, govde, c]);
}
function pngYaz(ham, boyut) {
  const ihdr = Buffer.alloc(13);
  ihdr.writeUInt32BE(boyut, 0); ihdr.writeUInt32BE(boyut, 4);
  ihdr[8] = 8; ihdr[9] = 6; ihdr[10] = 0; ihdr[11] = 0; ihdr[12] = 0;   /* 8-bit RGBA */
  const satirlar = Buffer.alloc((boyut * 4 + 1) * boyut);
  for (let y = 0; y < boyut; y++) {
    satirlar[y * (boyut * 4 + 1)] = 0;                                   /* filtre: none */
    ham.copy(satirlar, y * (boyut * 4 + 1) + 1, y * boyut * 4, (y + 1) * boyut * 4);
  }
  return Buffer.concat([
    Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]),
    parca('IHDR', ihdr),
    parca('IDAT', zlib.deflateSync(satirlar, { level: 9 })),
    parca('IEND', Buffer.alloc(0))
  ]);
}

const hedef = path.join(__dirname, '..', 'app');   /* kok — alt klasor yok */
fs.mkdirSync(hedef, { recursive: true });
[
  [180, 'apple-touch-icon.png', false],
  [192, 'icon-192.png', false],
  [512, 'icon-512.png', false],
  [512, 'icon-maskable-512.png', true]   /* Android adaptif ikon */
].forEach(([b, ad, maskeli]) => {
  const png = pngYaz(ikonUret(b, maskeli), b);
  fs.writeFileSync(path.join(hedef, ad), png);
  console.log(ad.padEnd(26), b + 'px', (png.length / 1024).toFixed(1) + ' KB');
});
