/* İkon üretim sayfasını sunar ve ürettiği PNG'leri diske yazar.
   Base64'ü sohbetten geçirmemek için: sayfa POST /yaz ile gönderiyor,
   sunucu doğrudan hedef klasörlere yazıyor (prototypes/pwa ve app).

     node sunucu.js <port> <hedef1> [hedef2] */
const http = require('http');
const fs = require('fs');
const path = require('path');

const PORT = Number(process.argv[2] || 8143);
const HEDEFLER = process.argv.slice(3);
const KOK = __dirname;

const TUR = { '.html': 'text/html; charset=utf-8', '.svg': 'image/svg+xml', '.png': 'image/png' };

http.createServer((istek, cevap) => {
  if (istek.method === 'POST' && istek.url === '/yaz') {
    let govde = '';
    istek.on('data', (p) => { govde += p; });
    istek.on('end', () => {
      let yazilan = [];
      try {
        const paket = JSON.parse(govde);
        Object.keys(paket).forEach((ad) => {
          /* Ad dışarıdan geliyor: yalnız beklenen dosya adları yazılır,
             yol ayracı taşıyan bir ad klasör dışına çıkabilirdi. */
          if (!/^[a-z0-9-]+\.png$/.test(ad)) return;
          const bayt = Buffer.from(paket[ad], 'base64');
          HEDEFLER.forEach((h) => {
            fs.writeFileSync(path.join(h, ad), bayt);
          });
          yazilan.push(ad + ' (' + (bayt.length / 1024).toFixed(1) + ' KB)');
        });
      } catch (e) {
        cevap.writeHead(400); cevap.end('bozuk paket'); return;
      }
      console.log('YAZILDI: ' + yazilan.join(' · '));
      cevap.writeHead(200, { 'content-type': 'text/plain; charset=utf-8' });
      cevap.end('tamam: ' + yazilan.length);
    });
    return;
  }

  const ad = (istek.url === '/' ? '/uret.html' : istek.url).split('?')[0];
  /* Marka SVG'si burada kopya durmuyor: TEK KAYNAK prototypes/pwa.
     Kopyalansaydı ikisi ayrışır ve ikon sessizce eskir. */
  const yol = path.basename(ad) === 'icon.svg'
    ? path.join(KOK, '..', '..', 'prototypes', 'pwa', 'icon.svg')
    : path.join(KOK, path.basename(ad));
  if (!fs.existsSync(yol)) { cevap.writeHead(404); cevap.end('yok'); return; }
  cevap.writeHead(200, { 'content-type': TUR[path.extname(yol)] || 'application/octet-stream' });
  cevap.end(fs.readFileSync(yol));
}).listen(PORT, '127.0.0.1', () => {
  console.log('ikon sunucusu ' + PORT + ' · hedef: ' + HEDEFLER.join(' , '));
});
