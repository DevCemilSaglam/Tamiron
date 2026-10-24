/* ============ MUHASEBE EŞDEĞERLİĞİ — prototip ↔ API (C10) ============
   MUHASEBE-KAPSAM-OTOMIX §5, Faz 1'in "bitti" tanımı: "Prototipteki muhasebe
   ekranları aynı rakamları API'den alır. Eski ve yeni yan yana sınanır, 0 fark."

   Bu araç PROTOTİPİN KENDİ FONKSİYONLARINI (01-cekirdek, 02-veri, 03-nesne)
   Node'da çalıştırır — yeniden yazmaz — ve tohum veriyle birlikte muhasebe
   rakamlarını JSON'a döker. API testi (PrototypeEquivalenceTests) aynı tohumu
   bir dükkâna yazar, aynı "bugün"le her rakamı karşılaştırır.

   KULLANIM:  node tools/muhasebe-esdeger.js
   ÇIKTI:     tests/Tamiron.Api.Tests/Esdeger/prototip-muhasebe.json
   Prototipte muhasebe kuralı değişirse bu araç yeniden koşturulur; test kırmızıya
   dönerse backend ile prototip ayrışmış demektir.

   İki senaryo: (1) tohum; (2) veresiye BUGÜN kartla tahsil edildi (K-071.1). */
const fs = require('fs');
const path = require('path');
const vm = require('vm');

const KOK = path.join(__dirname, '..');
const BETIK = path.join(KOK, 'prototypes', 'kaynak', 'betik');
const CIKTI = path.join(KOK, 'tests', 'Tamiron.Api.Tests', 'Esdeger', 'prototip-muhasebe.json');

/* Tarayıcı yok: DOM'a dokunan her şey sessiz bir "her şey" nesnesine düşer.
   Yalnız hesap fonksiyonları çağrılır; ekran çizilmez. */
function herSey() {
  const f = function () { return herSey(); };
  return new Proxy(f, {
    get: (t, k) => (k === Symbol.toPrimitive ? () => '' : k === 'length' ? 0 : herSey()),
    apply: () => herSey(), construct: () => herSey(), set: () => true
  });
}
function baglam() {
  const ctx = {
    console, Math, Date, JSON, Object, Array, String, Number, RegExp, Error, Symbol, Promise, setTimeout, clearTimeout,
    document: herSey(), navigator: herSey(), location: herSey(), history: herSey(),
    localStorage: { getItem: () => null, setItem() {}, removeItem() {} },
    matchMedia: () => ({ matches: false, addEventListener() {}, addListener() {} }),
    addEventListener() {}, requestAnimationFrame() {}, getComputedStyle: () => herSey()
  };
  ctx.window = ctx; ctx.self = ctx; ctx.globalThis = ctx;
  vm.createContext(ctx);
  for (const d of ['01-cekirdek.js', '02-veri.js', '03-nesne.js']) {
    vm.runInContext(fs.readFileSync(path.join(BETIK, d), 'utf8'), ctx, { filename: d });
  }
  return ctx;
}

function senaryo(ad, degistir) {
  const c = baglam();
  if (degistir) vm.runInContext(degistir, c);
  const donemler = ['bugun', 'ay', 'yil'];
  const run = (s) => vm.runInContext(s, c);
  const sonuc = {
    ad,
    bugun: run('bugunMetin()'),
    araclar: run('JSON.parse(JSON.stringify(ARACLAR))'),
    kayitlar: run('JSON.parse(JSON.stringify(KAYITLAR))'),
    giderler: run('JSON.parse(JSON.stringify(GIDERLER))'),
    veresiye: run('veresiyeToplami()'),
    bugunTahsilat: run('bugunTahsilat()'),
    donemler: {}
  };
  for (const d of donemler) {
    sonuc.donemler[d] = run(`(function () {
      var d = { nakit: 0, kart: 0, havale: 0 };
      // K-078: kasa ÖDEMELERDEN — kısmi ödemenin her parçası kendi gününde, kendi yolunda.
      donemOdemeleri('${d}').forEach(function (o) { if (d[o.yol] !== undefined) d[o.yol] += o.tutar; });
      var kar = donemKayitlari('${d}').map(function (k) { var r = isKari(k); r.id = k.id; return r; });
      return {
        nakit: d.nakit, kart: d.kart, havale: d.havale,
        tahsilat: tahsilatToplami('${d}'),
        teslim: donemKayitlari('${d}').length,
        gider: giderToplamiDonem('${d}'),
        parcaAlisi: parcaAlisiToplami('${d}'),
        isKari: kar,
        csv: muhasebeCsv('${d}')
      };
    })()`);
  }
  return sonuc;
}

const out = [
  senaryo('tohum'),
  // Veresiye (id 8, 06 TMR 07) BUGÜN kartla ödendi — prototipin "Ödeme alındı"sı.
  senaryo('veresiye-bugun-kart', "var k8 = KAYITLAR.filter(function (k) { return k.id === 8; })[0]; k8.odemeYolu = 'kart'; k8.tahsilTarihi = bugunMetin();"),
  // KENAR DURUMLAR — tohumda olmayan ama kuralın ayırdığı hâller (C10'da mutasyonla
  // bulundu: bunlar yokken "boş şikâyet" ve "bedava parça" hataları eşdeğerlik testinden kaçıyordu).
  //  · bugün teslim edilen işin şikâyeti BOŞ → CSV "Servis kaydı" yazar;
  //  · bedava parça (alış 0) bilinir, hesap dışı kalmaz; parça alışı listesine girmez;
  //  · müşterinin getirdiği parça toplama, farka ve alışa girmez.
  senaryo('kenar-durumlar',
    "var k7 = KAYITLAR.filter(function (k) { return k.id === 7; })[0]; k7.sikayet = '';" +
    "k7.satirlar.push({ tip: 'parca', ad: 'Conta', fiyat: 50, alis: 0, kimGetirdi: 'tamirci' });" +
    "k7.satirlar.push({ tip: 'parca', ad: 'Silecek lastiği', fiyat: 300, alis: 120, kimGetirdi: 'musteri' });")
];
fs.mkdirSync(path.dirname(CIKTI), { recursive: true });
fs.writeFileSync(CIKTI, JSON.stringify(out, null, 2) + '\n', 'utf8');
for (const s of out) {
  const y = s.donemler.yil;
  console.log(`${s.ad}: bugün=${s.bugun} · yıl tahsilat ${y.tahsilat} (nakit ${y.nakit}/kart ${y.kart}/havale ${y.havale}) · teslim ${y.teslim} · gider ${y.gider} · parça alışı ${y.parcaAlisi} · veresiye ${s.veresiye}`);
}
console.log('yazıldı: ' + path.relative(KOK, CIKTI));

/* D16 · Geliştirme tohumu: aynı "tohum" senaryosu API'nin yanına (DevSeed okur, PrototypeSeed yazar).
   Tek kaynak prototip: 02-veri.js değişince tohum da değişir — CI farkı yakalar. */
const TOHUM = path.join(KOK, 'src', 'Tamiron.Api', 'Seed', 'prototip-tohum.json');
const t = out[0];
const EOL = String.fromCharCode(10);
fs.mkdirSync(path.dirname(TOHUM), { recursive: true });
fs.writeFileSync(TOHUM, JSON.stringify({ bugun: t.bugun, araclar: t.araclar, kayitlar: t.kayitlar, giderler: t.giderler }, null, 2) + EOL, 'utf8');
console.log('yazıldı: ' + path.relative(KOK, TOHUM));
