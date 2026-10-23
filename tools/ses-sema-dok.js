/* ============ SESLİ İŞLEM ŞEMALARI + KORPUS — prototip → API (D15 · K-076.14) ============
   Prototipin KENDİ çözümleyicisini (betik/*.js + ekranların <script>'leri, şemalar ekranlarda
   bildirilir) Node'da çalıştırır — yeniden yazmaz — ve iki dosya döker:

     src/Tamiron.Api/Ses/islem-semalari.json      Islem.json(): işlem türleri, alanlar,
                                                   zorunlular — API bunu sunar ve doğrular
     tests/Tamiron.Api.Tests/Esdeger/prototip-ses.json
                                                   cümle × bağlam → prototipin işlem nesnesi;
                                                   (1) şema ↔ çözümleyici tutarlılığı sınanır,
                                                   (2) sunucuya taşınacak çözümleyicinin / dil
                                                   modelinin 0-fark hedefidir

   KULLANIM:  node tools/ses-sema-dok.js
   Prototipte bir şema ya da çözümleyici değişirse araç yeniden koşulur (CI'da da: backend.yml).
   Deterministik: değişmemiş prototipte dosyalar bayt bayt aynı çıkar. */
const fs = require('fs');
const path = require('path');
const vm = require('vm');

const KOK = path.join(__dirname, '..');
const BETIK = path.join(KOK, 'prototypes', 'kaynak', 'betik');
const EKRAN = path.join(KOK, 'prototypes', 'kaynak', 'ekranlar');
const SEMA_CIKTI = path.join(KOK, 'src', 'Tamiron.Api', 'Ses', 'islem-semalari.json');
const KORPUS_CIKTI = path.join(KOK, 'tests', 'Tamiron.Api.Tests', 'Esdeger', 'prototip-ses.json');

/* Tarayıcı yok: DOM'a dokunan her şey sessiz bir "her şey" nesnesine düşer (muhasebe-esdeger ile aynı). */
function herSey() {
  const f = function () { return herSey(); };
  return new Proxy(f, {
    get: (t, k) => (k === Symbol.toPrimitive ? () => '' : k === 'length' ? 0 : herSey()),
    apply: () => herSey(), construct: () => herSey(), set: () => true
  });
}
const ctx = {
  console, Math, Date, JSON, Object, Array, String, Number, RegExp, Error, Symbol, Promise, setTimeout, clearTimeout,
  document: herSey(), navigator: herSey(), location: herSey(), history: herSey(),
  localStorage: { getItem: () => null, setItem() {}, removeItem() {} },
  matchMedia: () => ({ matches: false, addEventListener() {}, addListener() {} }),
  addEventListener() {}, requestAnimationFrame() {}, getComputedStyle: () => herSey()
};
ctx.window = ctx; ctx.self = ctx; ctx.globalThis = ctx;
vm.createContext(ctx);
for (const d of fs.readdirSync(BETIK).filter(f => f.endsWith('.js')).sort())
  vm.runInContext(fs.readFileSync(path.join(BETIK, d), 'utf8'), ctx, { filename: d });
/* Ekran betikleri derlemede kendi IIFE'sine sarılır (prototip-derle.js) — burada da. */
for (const d of fs.readdirSync(EKRAN).filter(f => f.endsWith('.html')).sort()) {
  const html = fs.readFileSync(path.join(EKRAN, d), 'utf8');
  for (const m of html.matchAll(/<script>([\s\S]*?)<\/script>/g))
    vm.runInContext('(function () {\n' + m[1] + '\n})();', ctx, { filename: d });
}
const run = (s) => vm.runInContext(s, ctx);

const BAGLAMLAR = ['genel', 'servis-ekle', 'araclar', 'kayitlar', 'musteriler', 'mesaj', 'muhasebe', 'kayit-icinde'];
/* ses-esdeger.js'in ek cümleleri — ekran örnekleri ve tuzaklar. */
const EK = ['balata da degisecek', 'iscilik 750 lira', 'yag filtresi 300 lira',
  'teslim ettim nakit aldim', 'teslim ettim kartla', 'servistekileri goster',
  '34 ABC 123 sahibine selam at', 'bugun ne kadar tahsilat', 'bu ay ne kadar tahsil ettim',
  '2400 lira elektrik faturasi', '850 lira kira gideri ekle', 'herkese sms at yarin kapaliyiz',
  'tum musterilere bayram mesaji gonder', '41 KV 5588 yeni arac bakim',
  '34 ABC 123 beyaz araba geldi', '34 ABC 123 gosterge paneli yaniyor',
  '34 QBC 123 sahibine whatsapp selam mesaji gonder', '99 ZZZ 999 teslim ettim',
  'otuz dort a b c yuz yirmi uc bakima geldi', '41 KV 5588 Ahmet Yilmaz 0532 111 22 33 debriyaj kaciriyor',
  'merhaba', '', 'balata 450 lira', 'parca 300 lira', 'fren hidroligi 120 lira parca',
  '34 ABC 123 ara', 'Mehmet ara', '34 ABC 123 sahibini ara', '500 lira'];

const set = JSON.parse(fs.readFileSync(path.join(KOK, 'tools', 'degerlendirme', 'ses-seti.json'), 'utf8'));
const durumlar = [];
for (const c of set.cumleler) durumlar.push({ kaynak: 'ses-seti#' + c.id, cumle: c.cumle, baglam: c.baglam });
for (const cumle of EK) for (const baglam of BAGLAMLAR) durumlar.push({ kaynak: 'ek', cumle, baglam });

/* Açık kayıt: kayıt içi bağlamda prototipin 1 numaralı kaydı (tohum). Araç nesnesi (`arac`)
   tohumdan gelir ve sunucuda aracın KENDİSİ okunur — korpusta yalnız plakası tutulur. */
const sade = (o) => {
  if (!o || typeof o !== 'object') return o;
  const r = {};
  for (const k of Object.keys(o).sort()) {
    const v = o[k];
    if (k === 'arac') r[k] = v && v.plaka ? v.plaka : v;
    else if (k === 'satir' && v && typeof v === 'object') r[k] = v.ad || null;
    else if (v && typeof v === 'object' && !Array.isArray(v)) r[k] = sade(v);
    else r[k] = v;
  }
  return r;
};
const korpus = durumlar.map(d => ({
  ...d,
  islem: sade(run(`Islem.cozumle(${JSON.stringify(d.cumle)}, ${JSON.stringify(d.baglam)}, ${d.baglam === 'kayit-icinde' ? 1 : 'null'})`))
}));

const semalar = run('Islem.json()');
fs.mkdirSync(path.dirname(SEMA_CIKTI), { recursive: true });
fs.writeFileSync(SEMA_CIKTI, JSON.stringify(semalar, null, 1) + '\n');
fs.writeFileSync(KORPUS_CIKTI, JSON.stringify({ kaynak: 'node tools/ses-sema-dok.js', turler: semalar.map(s => s.name), korpus }, null, 1) + '\n');
const sayac = {};
for (const k of korpus) sayac[k.islem.tur] = (sayac[k.islem.tur] || 0) + 1;
console.log(semalar.length + ' şema · ' + korpus.length + ' durum · ' + JSON.stringify(sayac));
console.log('yazıldı: ' + path.relative(KOK, SEMA_CIKTI) + ', ' + path.relative(KOK, KORPUS_CIKTI));
