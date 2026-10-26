/* G3 · Prototip ekranlarının <style> bloklarını Blazor arayüzüne taşır (K-080.7).
   prototip-derle.js ile AYNI çıkarma: her ekran dosyası sırayla, her <style>...</style>
   bloğunun içi. Çıktı: src/Tamiron.Ui.Core/wwwroot/04-ekranlar.css (elle düzenlenmez).
   StyleCopyTests aynı çıkarmayı yapıp dosyayla karşılaştırır — prototipte stil değişirse
   test kırmızı olur ve bu araç yeniden koşulur:  node tools/ekran-stil.js */
const fs = require('fs');
const path = require('path');
const K = path.join(__dirname, '..');
const dir = path.join(K, 'prototypes', 'kaynak', 'ekranlar');
const parts = fs.readdirSync(dir).filter(f => f.endsWith('.html')).sort().flatMap(f => {
  const s = fs.readFileSync(path.join(dir, f), 'utf8').replace(/\r\n/g, '\n');
  return [...s.matchAll(/<style>([\s\S]*?)<\/style>/g)].map(m => `/* ---- ${f} ---- */\n${m[1].trim()}\n`);
});
const out = path.join(K, 'src', 'Tamiron.Ui.Core', 'wwwroot', '04-ekranlar.css');
fs.writeFileSync(out, '/* ÜRETİLDİ: node tools/ekran-stil.js — elle düzenleme. Kaynak: prototypes/kaynak/ekranlar/*.html <style> */\n' + parts.join('\n'));
console.log(out, parts.length, 'blok');
