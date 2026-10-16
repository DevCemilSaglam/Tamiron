/* SÖYLEYİŞ SÖZLÜĞÜNÜ GÖZDEN GEÇİRME LİSTESİ (16.10.2026)
   Cemil, 16.10: *"2'ye bakalım, daha da arttırabiliriz sözlüğü."*
   Kaynak TEK: prototypes/kaynak/betik/05-soyleyis.js. Bu betik oradan
   docs/arastirma/SOYLEYIS-SOZLUGU.md'yi ÜRETİR — liste elle yazılmaz, yoksa
   sözlükle ayrışır (ortak nesne kuralı).
     node tools/soyleyis-liste.js */
'use strict';
const fs = require('fs');
const path = require('path');
const kok = path.join(__dirname, '..');
const k = {};
new Function('k', fs.readFileSync(path.join(kok, 'prototypes', 'kaynak', 'betik', '05-soyleyis.js'), 'utf8') +
  '\nk.B = SOYLEYIS_MARKA; k.M = SOYLEYIS_MODEL; k.T = SOYLEYIS_TERIM;')(k);

const goster = a => a.map(s => s.charAt(0) === '!' ? '**' + s.slice(1) + '** ⚑' : s).join(', ');
let md = '# Söyleyiş sözlüğü — gözden geçirme listesi\n\n' +
  '> **ÜRETİLİR** — elle düzenleme; kaynak `prototypes/kaynak/betik/05-soyleyis.js`, ' +
  'yeniden üret: `node tools/soyleyis-liste.js`.\n\n' +
  'Sol: kayıttaki doğru biçim. Sağ: ustanın söyleyebileceği / tanıyıcının yazabileceği biçimler.\n' +
  '⚑ = gerçek bir Türkçe kelime: yalnız MARKAYLA birlikte araç sayılır, terim düzeltmede kullanılmaz.\n\n' +
  '**Eksik ya da yanlış gördüğün her satırı söyle** — özellikle ustaların gerçekten nasıl söylediği.\n\n' +
  `Toplam: ${Object.keys(k.B).length} marka · ${Object.keys(k.M).length} model · ${k.T.length} terim.\n\n`;
md += '## Markalar\n\n| Doğru biçim | Söyleyişler |\n|---|---|\n';
Object.keys(k.B).sort().forEach(a => { md += `| ${a} | ${goster(k.B[a])} |\n`; });
md += '\n## Modeller\n\n| Doğru biçim | Söyleyişler |\n|---|---|\n';
Object.keys(k.M).sort().forEach(a => { md += `| ${a} | ${goster(k.M[a])} |\n`; });
md += '\n## Parça ve iş terimleri\n\n| Doğru biçim | Söyleyişler |\n|---|---|\n';
k.T.slice().sort((x, y) => x[1].localeCompare(y[1], 'tr')).forEach(t => { md += `| ${t[1]} | ${goster(t[0])} |\n`; });
const hedef = path.join(kok, 'docs', 'arastirma', 'SOYLEYIS-SOZLUGU.md');
fs.writeFileSync(hedef, md, 'utf8');
console.log('→ ' + hedef);
