/* ============ YAKLAŞIM SAYFASI DERLEYİCİ (12.10.2026 · Tur 8.1) ============
   prototypes/yaklasimlar/servis-detay-kaynak.html
     + docs/tokens.css + stil/01-taban.css + stil/03-bilesen.css + ikonlar.svg
   -> prototypes/yaklasimlar/servis-detay.html  (tek dosya, dışa bağımlılıksız)

   NEDEN: Cemil servis detayı için birkaç kurguyu YAN YANA görüp seçecek.
   Karşılaştırılan şey KURGU olmalı, görünüş değil — o yüzden dört yaklaşım
   da prototipin kendi token'ları ve bileşen sınıflarıyla çiziliyor. Stil
   kopyalanmıyor, derlemede gömülüyor: prototip değişirse bu sayfa da
   değişir.

   Çıktı <html>/<head>/<body> İÇERMEZ: Artifact yayını kendi iskeletini
   sarıyor; yerel sunucuda da tarayıcı eksik iskeleti tamamlıyor.

   ⚠️ YERLEŞTİRME FONKSİYONLA (prototip-derle.js'in 12.10 dersi): dizge
   olarak verilen değiştirme metni `$'` kalıbını yorumlar ve betiği bozar.
   Ayrıştırma denetimi de ÜRETİLEN dosyadaki betiğe bakar.

   KULLANIM:  node tools/yaklasim-derle.js */

const fs = require('fs');
const path = require('path');

const KOK = path.join(__dirname, '..');
const KAYNAK = path.join(KOK, 'prototypes', 'yaklasimlar', 'servis-detay-kaynak.html');
const CIKTI = path.join(KOK, 'prototypes', 'yaklasimlar', 'servis-detay.html');
const oku = p => fs.readFileSync(p, 'utf8');

const css = [
  path.join(KOK, 'docs', 'tokens.css'),
  path.join(KOK, 'prototypes', 'kaynak', 'stil', '01-taban.css'),
  path.join(KOK, 'prototypes', 'kaynak', 'stil', '03-bilesen.css')
].map(p => '/* ===== ' + path.relative(KOK, p).replace(/\\/g, '/') + ' ===== */\n' + oku(p)).join('\n');

const ikonlar = oku(path.join(KOK, 'prototypes', 'kaynak', 'ikonlar.svg'));

const cikti = oku(KAYNAK)
  .replace('/* @@STIL@@ */', () => css)
  .replace('<!-- @@IKONLAR@@ -->', () => ikonlar);

const hatalar = [];
if (cikti.indexOf('@@STIL@@') >= 0 || cikti.indexOf('@@IKONLAR@@') >= 0) {
  hatalar.push('yer tutucu yerinde kaldı');
}
const betikler = [...cikti.matchAll(/<script>([\s\S]*?)<\/script>/g)].map(m => m[1]);
if (!betikler.length) hatalar.push('çıktıda <script> yok');
betikler.forEach((b, i) => {
  try { new Function(b); } catch (e) { hatalar.push(`${i + 1}. betik ayrışmıyor — ${e.message}`); }
});
/* Ham hex YALNIZ tokens.css'te olabilir (CLAUDE.md). Kaynağın kendi
   kısmında renk literali varsa yakala. */
const kendi = oku(KAYNAK).replace(/<!--[\s\S]*?-->/g, '').replace(/\/\*[\s\S]*?\*\//g, '');
const hex = kendi.match(/#[0-9a-fA-F]{3,8}\b(?![\w-])/g) || [];
const gercekHex = hex.filter(h => !/^#(i-|y)/.test(h));
if (gercekHex.length) hatalar.push('ham hex: ' + gercekHex.join(' '));

fs.writeFileSync(CIKTI, cikti, 'utf8');
console.log(`prototypes/yaklasimlar/servis-detay.html üretildi — ${Math.round(cikti.length / 1024)} KB`);
if (hatalar.length) {
  hatalar.forEach(h => console.log('  ! ' + h));
  process.exitCode = 1;
} else {
  console.log('  denetim temiz');
}
