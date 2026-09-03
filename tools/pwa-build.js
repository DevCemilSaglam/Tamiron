/* prototypes/omurga.html  ->  app/index.html + app/sw.js
 *
 * Tek kaynak prototiptir; app/ klasoru ONDAN URETILIR. Elle duzenlenmez.
 * Yapilan degisiklikler:
 *   1) Google Fonts baglantilari -> yerel fonts.css (cevrimdisi calissin)
 *   2) PWA basligi: manifest, ikonlar, tema rengi, iOS meta etiketleri
 *   3) iOS guvenli alan (centik / alt cubuk) icin ek CSS
 *   4) service worker kaydi
 * Calistir:  node tools/pwa-build.js
 */
const fs = require('fs');
const path = require('path');
const crypto = require('crypto');

const KOK = path.join(__dirname, '..');
const KAYNAK = path.join(KOK, 'prototypes', 'omurga.html');
const APP = path.join(KOK, 'app');

/* ---------- 1) prototipi oku ---------- */
let html = fs.readFileSync(KAYNAK, 'utf8');
const oncekiUzunluk = html.length;

/* ---------- 2) Google Fonts -> yerel ---------- */
const fontSatirlari = [
  /^\s*<link rel="preconnect" href="https:\/\/fonts\.googleapis\.com"[^>]*>\r?\n/m,
  /^\s*<link rel="preconnect" href="https:\/\/fonts\.gstatic\.com"[^>]*>\r?\n/m,
];
fontSatirlari.forEach((r) => { html = html.replace(r, ''); });

const fontLink = /^\s*<link href="https:\/\/fonts\.googleapis\.com\/css2[^"]*"[^>]*>\r?\n/m;
if (!fontLink.test(html)) {
  console.error('HATA: prototipte Google Fonts baglantisi bulunamadi; font yerellestirme atlandi.');
  process.exit(1);
}
html = html.replace(fontLink, '<link rel="stylesheet" href="fonts.css" />\n');

/* ---------- 3) PWA basligi ---------- */
const pwaBaslik = `<link rel="manifest" href="manifest.json" />
<link rel="apple-touch-icon" href="icons/apple-touch-icon.png" />
<link rel="icon" type="image/svg+xml" href="icons/icon.svg" />
<meta name="theme-color" media="(prefers-color-scheme: light)" content="#F2F5F8" />
<meta name="theme-color" media="(prefers-color-scheme: dark)" content="#0A0F17" />
<meta name="mobile-web-app-capable" content="yes" />
<meta name="apple-mobile-web-app-capable" content="yes" />
<meta name="apple-mobile-web-app-title" content="Tamiron" />
<meta name="apple-mobile-web-app-status-bar-style" content="black-translucent" />
`;
html = html.replace('<link rel="stylesheet" href="fonts.css" />\n',
                    '<link rel="stylesheet" href="fonts.css" />\n' + pwaBaslik);

/* ---------- 4) iOS guvenli alan ---------- */
const guvenliAlan = `<style>
/* PWA (tam ekran) — iOS centik ve alt cubuk alanlari.
   Tarayicida env() 0 doner; masaustunde hicbir sey degismez. */
/* TAM EKRANDA safe-area 0 olur ve header ekranin tepesine YAPISIR.
   max() ile taban bosluk garanti ediliyor; yukseklik prototipin
   kendi 72px'i uzerine ekleniyor (onceden 60px'e eziliyordu).
   31.08: topbar 68 -> 72 px (Cemil: navigasyon biraz daha buyuk). */
.topbar { padding-top: max(env(safe-area-inset-top), 12px);
          height: calc(72px + max(env(safe-area-inset-top), 12px)); }
.footer { padding-bottom: env(safe-area-inset-bottom); }
.drawer { padding-top: env(safe-area-inset-top); }
@supports (height: 100dvh) { .device { height: 100dvh; } }
@media (min-width: 480px) { @supports (height: 100dvh) { .device { height: calc(100dvh - 48px); } } }
</style>
`;
html = html.replace('</head>', guvenliAlan + '</head>');

/* ---------- 5) kurulum dugmesi (drawer icinde) ----------
   NEDEN: Android'de tarayicilar kurulum secenegini farkli yerlere koyuyor
   (Brave "Ekle..." alt menusu, Opera "Ana ekrana ekle", bazilari hic gostermiyor).
   beforeinstallprompt olayini yakalayip KENDI dugmemizi gosteriyoruz — menude
   arama derdi kalmiyor. Dugme yalniz tarayici "kurulabilir" dediginde gorunur,
   yani ayni zamanda teshis aracidir. */
const kurulumDugmesi = `      <button class="btn btn-primary" id="kurBtn" style="display:none;margin:8px 12px 4px;width:calc(100% - 24px)">
        <svg class="ico"><use href="#i-arrow-right"></use></svg> Uygulamayı yükle
      </button>
      <p class="plate-hint" id="kurNot" style="display:none;padding:0 12px 8px">Ana ekrana eklenir, tam ekran açılır.</p>
      <p class="plate-hint" id="kurDurum" style="padding:0 12px 8px;line-height:1.7"></p>
`;
const drawerAyarBasligi = '      <div class="drawer-sec-label"><svg class="ico ico-sm"><use href="#i-cog"></use></svg> Ayarlar</div>';
if (!html.includes(drawerAyarBasligi)) {
  console.error('HATA: drawer Ayarlar basligi bulunamadi; kurulum dugmesi eklenemedi.');
  process.exit(1);
}
html = html.replace(drawerAyarBasligi, kurulumDugmesi + drawerAyarBasligi);

/* ---------- 6) service worker kaydi + kurulum akisi ---------- */
const swKayit = `<script>
/* Service worker — cevrimdisi acilis. file:// uzerinde calismaz (normal). */
if ('serviceWorker' in navigator && location.protocol.startsWith('http')) {
  window.addEventListener('load', function () {
    navigator.serviceWorker.register('sw.js').catch(function () {});
  });
}

/* ---------- ANDROID GERI TUSU ----------
   PWA tam ekran/standalone acildiginda tarayici gecmisi BOS oluyor;
   telefonun geri tusuna basinca uygulama KAPANIYOR. Uygulamanin kendi
   navBack()'ine bagliyoruz: ekran yiginonda geri gidilecek yer varsa
   oraya doner, ANA EKRANDA ise cikisa izin verir (beklenen davranis). */
(function () {
  if (!window.history || typeof window.navBack !== 'function') return;

  function doldur() { history.pushState({ tamiron: 1 }, ''); }
  doldur();

  var sonCikisDenemesi = 0;

  window.addEventListener('popstate', function () {
    var yigin = window.back_stack;
    if (window.drawerOpen || (yigin && yigin.length)) {
      navBack();
      doldur();          /* bir sonraki geri de yakalansin diye yigini tazele */
      sonCikisDenemesi = 0;
      return;
    }
    /* ANA EKRAN: tek basista cikmak kazara oluyor — CIFT BASIS iste.
       Ikinci basis 2 sn icinde gelirse state'i geri koymuyoruz ve
       sistem uygulamayi kapatiyor. */
    var simdi = Date.now();
    if (simdi - sonCikisDenemesi < 2000) return;   /* cikisa izin ver */
    sonCikisDenemesi = simdi;
    doldur();
    if (typeof showToast === 'function') showToast('Çıkmak için tekrar geri tuşuna bas.', false, 1900);
  });

  /* Cekmece aciksa geri tusu ONCE onu kapatsin (telefonda beklenen davranis) */
  var eskiNavBack = window.navBack;
  window.navBack = function () {
    if (window.drawerOpen && typeof closeDrawer === 'function') { closeDrawer(); return; }
    eskiNavBack();
  };
})();

/* Kurulum istemi + TESHIS.
   NEDEN: telefonda "yukle" secenegi hic cikmadiginda sebebi uzaktan goremiyoruz.
   Cekmecede her zaman gorunen bir durum satiri sebebi cihazin KENDISINDE yazar:
   hangi surum yuklu, cevrimdisi servis calisiyor mu, tarayici kurulum istemini
   gonderdi mi. Dugme yalniz istem geldiginde acilir. */
(function () {
  var istem = null;
  var btn = document.getElementById('kurBtn');
  var not = document.getElementById('kurNot');
  var durum = document.getElementById('kurDurum');
  var istemGeldi = false;

  function kuruluMu() {
    return window.matchMedia('(display-mode: standalone)').matches ||
           window.navigator.standalone === true;
  }

  function yaz() {
    if (!durum) return;
    var kurulu = kuruluMu();
    var servis = !('serviceWorker' in navigator) ? 'desteklenmiyor'
               : navigator.serviceWorker.controller ? 'çalışıyor'
               : 'kaydediliyor…';
    var istemMetni = istemGeldi ? 'geldi'
                   : kurulu ? 'gerekmez (kurulu)'
                   : 'gelmedi';
    durum.innerHTML =
      'Sürüm <b>__SURUM__</b><br>' +
      'Açılış: ' + (kurulu ? 'uygulama' : 'tarayıcı') + ' · ' +
      location.protocol.replace(':', '') + '<br>' +
      'Çevrimdışı servis: ' + servis + '<br>' +
      'Kurulum istemi: ' + istemMetni;
  }

  yaz();
  if ('serviceWorker' in navigator && navigator.serviceWorker.ready) {
    navigator.serviceWorker.ready.then(yaz).catch(function () {});
  }

  window.addEventListener('beforeinstallprompt', function (e) {
    e.preventDefault();          /* tarayicinin kendi seridini bastir */
    istem = e;
    istemGeldi = true;
    if (btn) btn.style.display = '';
    if (not) not.style.display = '';
    yaz();
  });

  if (btn) {
    btn.addEventListener('click', function () {
      if (!istem) return;
      istem.prompt();
      istem.userChoice.then(function (secim) {
        if (secim.outcome === 'accepted') { btn.style.display = 'none'; not.style.display = 'none'; }
        istem = null;
        yaz();
      });
    });
  }

  window.addEventListener('appinstalled', function () {
    if (btn) btn.style.display = 'none';
    if (not) not.style.display = 'none';
    yaz();
    if (window.showToast) showToast('Uygulama yüklendi.', false, 2200);
  });
})();
</script>
`;
html = html.replace('</body>', swKayit + '</body>');

/* ---------- 6) yazi tipleri: base64 gomulu TEK fonts.css ----------
   ALT KLASOR YOK. Netlify'a yuklemede fonts/ ve icons/ klasorleri iki kez ust
   uste atlandi (kokteki dosyalar gitti, klasorler gitmedi); ikonlar 404 olunca
   tarayici uygulamayi kurulabilir saymadi. Bu yuzden app/ artik DUZ: her sey
   kokte, hicbir alt klasor yok — hangi yontemle yuklenirse yuklensin calisir. */
const fontSablon = fs.readFileSync(path.join(__dirname, 'fonts-sablon.css'), 'utf8');
let fontBoyut = 0;
const fontCss = fontSablon.replace(/url\((fonts\/[^)]+\.woff2)\)/g, (t, yol) => {
  const dosya = path.join(__dirname, 'fonts', path.basename(yol));
  if (!fs.existsSync(dosya)) {
    console.error('HATA: yazi tipi yok: ' + yol + '   (once: python tools/font-indir.py)');
    process.exit(1);
  }
  const ham = fs.readFileSync(dosya);
  fontBoyut += ham.length;
  return 'url(data:font/woff2;base64,' + ham.toString('base64') + ')';
});

/* ---------- 7) surum damgasi: HEM sw HEM sayfa ----------
   Sayfadaki teshis satiri surumu gosterir; boylece telefonda hangi kopyanin
   yuklu oldugu tahmin degil, okunur bilgi olur. Hash yer tutuculu metin
   uzerinden alinir — yoksa kendi kendine bagimli olurdu. */
let sw = fs.readFileSync(path.join(__dirname, 'sw-sablon.js'), 'utf8');

/* Surum hash'i sw SABLONUNU da kapsar: service worker mantigi degistiginde
   surum degismezse eski onbellek temizlenmez ve degisiklik telefona gecmez. */
const surum = crypto.createHash('sha1')
  .update(html)
  .update(fontCss)
  .update(sw)
  .digest('hex').slice(0, 8);

sw = sw.replace('__SURUM__', surum);
html = html.replace('__SURUM__', surum);

/* ---------- 8) yaz ---------- */
fs.mkdirSync(APP, { recursive: true });
fs.writeFileSync(path.join(APP, 'fonts.css'), fontCss);
fs.writeFileSync(path.join(APP, 'index.html'), html);
fs.writeFileSync(path.join(APP, 'sw.js'), sw);

/* ---------- ozet ---------- */
const kb = (p) => (fs.statSync(p).size / 1024).toFixed(1) + ' KB';
const icerik = fs.readdirSync(APP);
const altKlasor = icerik.filter((a) => fs.statSync(path.join(APP, a)).isDirectory());

console.log('app/index.html  ' + kb(path.join(APP, 'index.html')) +
            '   (prototip ' + (oncekiUzunluk / 1024).toFixed(1) + ' KB)');
console.log('app/fonts.css   ' + kb(path.join(APP, 'fonts.css')) +
            '   (' + (fontBoyut / 1024).toFixed(0) + ' KB yazi tipi gomulu)');
console.log('app/sw.js       surum ' + surum);
console.log('\nYayina hazir: app/ — ' + (icerik.length - altKlasor.length) + ' dosya, ' +
            altKlasor.length + ' alt klasor' +
            (altKlasor.length ? '  << UYARI: duz yapi bozuldu' : '  (duz yapi)'));
