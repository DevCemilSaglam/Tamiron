/* Tamiron prototip — service worker
   Amac: uygulama cevrimdisi da acilsin (demo sirasinda internet olmayabilir).
   Strateji: kurulumda tum varliklari onbellege al; sonra ONBELLEK-ONCE, ag yedek.
   SURUM build sirasinda tools/pwa-build.js tarafindan damgalanir; degisince
   eski onbellek silinir ve yeni surum yuklenir. */
const SURUM = '9cb45fa9';
const ONBELLEK = 'tamiron-' + SURUM;

const VARLIKLAR = [
  './',
  './index.html',
  './manifest.json',
  './fonts.css',
  './icon.svg',
  './icon-192.png',
  './icon-512.png',
  './icon-maskable-512.png',
  './apple-touch-icon.png'
];
/* Not: yazi tipleri fonts.css icine base64 gomulu — ayri varlik yok. */

self.addEventListener('install', (e) => {
  /* DAYANIKLI KURULUM: cache.addAll TEK bir varlik 404 dondugunde kurulumun
     TAMAMINI iptal eder; o zaman service worker aktiflesemez ve cevrimdisi
     calisma hic devreye girmez. Varliklari tek tek ekliyoruz — eksik bir font
     ya da ikon yuzunden uygulama kurulamaz hale gelmesin. */
  e.waitUntil(
    caches.open(ONBELLEK)
      .then((c) => Promise.allSettled(VARLIKLAR.map((v) => c.add(v))).then((sonuc) => {
        const basarisiz = sonuc.filter((s) => s.status === 'rejected').length;
        if (basarisiz) {
          console.warn('[sw] ' + basarisiz + '/' + VARLIKLAR.length +
                       ' varlik onbellege alinamadi (eksik dosya olabilir).');
        }
      }))
      .then(() => self.skipWaiting())
  );
});

self.addEventListener('activate', (e) => {
  e.waitUntil(
    caches.keys()
      .then((adlar) => Promise.all(
        adlar.filter((a) => a.startsWith('tamiron-') && a !== ONBELLEK)
             .map((a) => caches.delete(a))
      ))
      .then(() => self.clients.claim())
  );
});

self.addEventListener('fetch', (e) => {
  if (e.request.method !== 'GET') return;

  /* Gezinme istekleri: agi dene, olmazsa onbellekten index.html ver
     (boylece cevrimdisi acilista bos sayfa gelmez). */
  if (e.request.mode === 'navigate') {
    e.respondWith(
      fetch(e.request).catch(() => caches.match('./index.html'))
    );
    return;
  }

  e.respondWith(
    caches.match(e.request).then((bulunan) => {
      if (bulunan) return bulunan;
      return fetch(e.request).then((yanit) => {
        /* ayni kaynaktan gelen basarili yanitlari onbellege ekle */
        if (yanit && yanit.status === 200 && yanit.type === 'basic') {
          const kopya = yanit.clone();
          caches.open(ONBELLEK).then((c) => c.put(e.request, kopya));
        }
        return yanit;
      });
      /* ag da onbellek de yoksa: taraycinin kendi ag hatasi gorunur */
    })
  );
});
