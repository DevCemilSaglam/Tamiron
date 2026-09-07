/* ============ PROTOTİP DERLEYİCİ ============
   prototypes/kaynak/**  ->  prototypes/omurga.html

   NEDEN VAR: v1'in prototipi TEK DOSYAYDI ve 10.700 satıra çıktı.
   Sonuçları ölçüldü — aynı var-hoisting hatası SEKİZ kez, sınıf adı
   çakışması ÜÇ kez yaşandı, dosya önizleme sınırını aştı ve bir tur
   boyunca açılmadı. Parçalı yazıp derlemek bunların üçünü de yapısal
   olarak çözüyor.

   KULLANIM:  node tools/prototip-derle.js

   KAYNAK DÜZENİ
     kabuk.html            @@STIL@@ @@EKRANLAR@@ @@BETIK@@ @@IKONLAR@@
     stil/NN-*.css         sırayla eklenir (dosya adındaki numara)
     betik/NN-*.js         sırayla eklenir — ÇEKİRDEK ÖNCE
     ekranlar/NN-ad.html   her biri: markup + <style> + <script>
     ikonlar.svg           <symbol> tanımları

   EKRAN DOSYASI SÖZLEŞMESİ
     - Markup kök öğesi:  <section class="screen" id="screen-ad">
     - <style>  bloğu varsa CSS'in sonuna eklenir
     - <script> bloğu varsa BETİĞE EKLENİR ve IIFE'ye SARILIR
       (global kapsam kirlenmesin — v1'in sekiz kez yaşadığı hata)

   DERLEME DENETİMİ (kırılmadan önce yakalar)
     1. Aynı sınıf adı iki ayrı ekran dosyasında tanımlanmış mı
        (.rol-cip / .stok / .tablo-kap çakışmaları bu sınıftandı)
     2. data-git hedefi var olmayan bir ekranı gösteriyor mu
     3. Ekran id'si dosya adıyla uyuşuyor mu
*/

const fs = require('fs');
const path = require('path');

const KOK = path.join(__dirname, '..');
const KAYNAK = path.join(KOK, 'prototypes', 'kaynak');
const CIKTI = path.join(KOK, 'prototypes', 'omurga.html');
const TOKENS = path.join(KOK, 'docs', 'tokens.css');

const uyarilar = [];

function oku(p) { return fs.readFileSync(p, 'utf8'); }

function siraliDosyalar(klasor, uzanti) {
  if (!fs.existsSync(klasor)) return [];
  return fs.readdirSync(klasor)
    .filter(f => f.endsWith(uzanti))
    .sort()                       /* sıra dosya adındaki numaradan */
    .map(f => ({ ad: f, yol: path.join(klasor, f) }));
}

/* ---- 1. tokens (tek renk/tip/ölçü kaynağı — K-023) ---- */
let css = '/* ===== tokens.css (docs/tokens.css) ===== */\n' + oku(TOKENS) + '\n';

/* derleme denetimi 8: KAPANMAMIŞ YORUM (06.09)
   ⚠️ BU HATA AYLARDIR SESSİZCE DURUYORDU. 03-bilesen.css'te bir yorum
   `*` + `/` ile kapanmıyordu; CSS o noktadan sonraki HER ŞEYİ, bir
   sonraki kapanışa kadar, yorum saydı. İçinde kalan kural
   `.k-sil::before` idi — yani ikincil düğmelerin 44 px dokunma alanı.
   Kural yazılmıştı, denetimden geçmişti, ölçülmüştü; sadece TARAYICI
   onu hiç görmedi.

   Kapanmamış yorum, silinen koddan tehlikelidir: silinen kod yoktur,
   yorumlanan kod VAR GİBİ DURUR. Dosyada 140 açılış, 139 kapanış
   vardı — tek bir sayma bunu söylerdi.

   Sayma, dizge içindeki `/`+`*`ı da sayar; CSS'te dizge nadirdir ve
   yanlış alarm bir yorum satırıyla susturulabilir. JS'te sayma
   yapılmıyor (orada dizgeler yüzünden güvenilmez) — JS'in karşılığı
   denetim 7: üretilen betik ayrışıyor mu. */
function yorumDengesi(ad, metin) {
  const ac = (metin.match(/\/\*/g) || []).length;
  const kap = (metin.match(/\*\//g) || []).length;
  if (ac !== kap) {
    uyarilar.push(
      `${ad}: KAPANMAMIŞ YORUM — ${ac} açılış, ${kap} kapanış. ` +
      'Kapanmayan yorum, kendinden sonraki kuralları sessizce yutar.');
  }
}

/* ---- 2. ortak stil ---- */
for (const d of siraliDosyalar(path.join(KAYNAK, 'stil'), '.css')) {
  const metin = oku(d.yol);
  yorumDengesi('stil/' + d.ad, metin);
  css += `\n/* ===== stil/${d.ad} ===== */\n` + metin + '\n';
}

/* ---- 3. çekirdek betik ---- */
let js = '';
for (const d of siraliDosyalar(path.join(KAYNAK, 'betik'), '.js')) {
  js += `\n/* ===== betik/${d.ad} ===== */\n` + oku(d.yol) + '\n';
}

/* ---- 4. ekranlar ---- */
let ekranMarkup = '';
const ekranAdlari = [];
const sinifSahibi = {};   /* sınıf adı -> ilk tanımlayan dosya */

for (const d of siraliDosyalar(path.join(KAYNAK, 'ekranlar'), '.html')) {
  let icerik = oku(d.yol);

  /* derleme denetimi 6: YORUMUN İÇİNDE ETİKET (06.09)
     Bir ekran başlığında "(aşağıdaki <style> içinde)" yazdım. Aşağıdaki
     çıkarıcı kaçıncı <style> olduğuna bakmıyor: YORUMDAKİ etiketten
     başlayıp dosyanın sonundaki gerçek </style> ile eşleşti ve arada
     kalan HER ŞEYİ — <section>'ın tamamını — CSS sanıp sildi. Derleme
     "id=screen-… bulunamadı" dedi; sebebi bir yorum satırıydı.

     Yorum kod değildir ama derleyici için METİNDİR. Bu denetim o
     hatayı adıyla söylüyor. */
  (icerik.match(/<!--[\s\S]*?-->/g) || []).forEach((yorum) => {
    const et = yorum.match(/<\/?(style|script)[ >]/);
    if (et) {
      uyarilar.push(
        d.ad + ': YORUM İÇİNDE <' + et[1] + '> etiketi — ' +
        'çıkarıcılar yorumu kod sanıyor. Etiketi yorumdan çıkar.');
    }
  });

  /* <style> çıkar */
  icerik = icerik.replace(/<style>([\s\S]*?)<\/style>/g, (_, blok) => {
    yorumDengesi(d.ad + ' (style)', blok);
    css += `\n/* ===== ekran: ${d.ad} ===== */\n` + blok.trim() + '\n';
    /* derleme denetimi 1: sınıf çakışması
       DİKKAT: `m` bayrağı şart. Onsuz `^` yalnız 0. konumu tutuyor ve
       bloğun ilk satırındaki sınıf GÖZDEN KAÇIYOR — ilk yazımda
       .ana-btn çakışması tam böyle kaçtı. Delikli denetim, denetimsiz
       olmaktan kötüdür: yanlış güven verir. */
    const re = /(^|[},])\s*\.([a-zA-Z][\w-]*)/gm;
    let m;
    while ((m = re.exec(blok))) {
      const sinif = m[2];
      if (sinifSahibi[sinif] && sinifSahibi[sinif] !== d.ad) {
        uyarilar.push(`SINIF ÇAKIŞMASI: .${sinif} hem ${sinifSahibi[sinif]} hem ${d.ad} içinde`);
      } else {
        sinifSahibi[sinif] = d.ad;
      }
    }
    return '';
  });

  /* <script> çıkar — IIFE'ye sarılır */
  icerik = icerik.replace(/<script>([\s\S]*?)<\/script>/g, (_, blok) => {
    js += `\n/* ===== ekran: ${d.ad} ===== */\n(function () {\n${blok.trim()}\n})();\n`;
    return '';
  });

  const markup = icerik.trim();

  /* derleme denetimi 3: id dosya adıyla uyuşuyor mu */
  const idEsle = markup.match(/id="screen-([\w-]+)"/);
  const dosyaAdi = d.ad.replace(/^\d+-/, '').replace(/\.html$/, '');
  if (!idEsle) {
    uyarilar.push(`${d.ad}: id="screen-…" bulunamadı`);
  } else {
    ekranAdlari.push(idEsle[1]);
    if (idEsle[1] !== dosyaAdi) {
      uyarilar.push(`${d.ad}: id "screen-${idEsle[1]}" dosya adı "${dosyaAdi}" ile uyuşmuyor`);
    }
  }

  ekranMarkup += '\n' + markup + '\n';
}

/* derleme denetimi 2: data-git hedefleri */
const hedefRe = /data-git="([\w-]+)"/g;
const tumMetin = ekranMarkup + oku(path.join(KAYNAK, 'kabuk.html'));
let h;
while ((h = hedefRe.exec(tumMetin))) {
  if (!ekranAdlari.includes(h[1])) {
    uyarilar.push(`data-git="${h[1]}" — böyle bir ekran yok`);
  }
}

/* derleme denetimi 5: TANIMSIZ TOKEN
   03.09 — `--nav-hover` her yerde kullanılıyordu ama tokens.css'te
   TANIMLI DEĞİLDİ. Tanımsız var() geçersizdir: bildirim düşer, hover
   hiçbir şey yapmaz ve hata mesajı çıkmaz. Ölü ikon referansıyla aynı
   sınıf — sessiz bozulma. Yalnız kendi token'larımız (--x) denetlenir;
   yedek değeri olan var(--x, y) kullanımı zaten güvenlidir. */
{
  const tanimli = new Set();
  const tanimRe = /(--[\w-]+)\s*:/g;
  let td;
  while ((td = tanimRe.exec(css))) tanimli.add(td[1]);

  const kullanimRe2 = /var\(\s*(--[\w-]+)\s*([,)])/g;
  const eksikToken = new Set();
  let tk;
  while ((tk = kullanimRe2.exec(css))) {
    if (tk[2] === ')' && !tanimli.has(tk[1])) eksikToken.add(tk[1]);
  }
  eksikToken.forEach(t => uyarilar.push(`var(${t}) — böyle bir token tanımlı değil`));
}

/* ---- 5. ikonlar ---- */
const ikonYol = path.join(KAYNAK, 'ikonlar.svg');
const ikonlar = fs.existsSync(ikonYol) ? oku(ikonYol) : '';

/* derleme denetimi 4: ÖLÜ İKON REFERANSI
   03.09 — teslim ekranı #i-tamam, parça ekranı #i-parca çağırıyordu;
   ikizlerinin hiçbiri ikonlar.svg'de yoktu. <use> sessizce HİÇBİR ŞEY
   çizer: düğme metni yerinde durur, ikon yerinde boşluk kalır ve
   hiçbir denetim ses çıkarmaz. Sessiz bozulma en pahalı bozulmadır. */
const tanimliIkonlar = (ikonlar.match(/id="([\w-]+)"/g) || [])
  .map(s => s.slice(4, -1));
const kullanimRe = /<use[^>]*href="#([\w-]+)"/g;
const ikonMetni = ekranMarkup + js + oku(path.join(KAYNAK, 'kabuk.html'));
const eksikIkon = new Set();
let u2;
while ((u2 = kullanimRe.exec(ikonMetni))) {
  if (!tanimliIkonlar.includes(u2[1])) eksikIkon.add(u2[1]);
}
eksikIkon.forEach(i => uyarilar.push(`<use href="#${i}"> — ikonlar.svg'de böyle bir ikon yok`));

/* ---- 6. birleştir ---- */
const baslik = `<!--
  ⚠️  BU DOSYA ÜRETİLMİŞTİR — ELLE DÜZENLEME.
  Kaynak: prototypes/kaynak/**
  Üretmek için: node tools/prototip-derle.js
  Üretim: ${new Date().toISOString().slice(0, 16).replace('T', ' ')}
-->
`;

let cikti = oku(path.join(KAYNAK, 'kabuk.html'))
  .replace('<!doctype html>', '<!doctype html>\n' + baslik)
  .replace('/* @@STIL@@ */', css)
  .replace('<!-- @@EKRANLAR@@ -->', ekranMarkup)
  .replace('<!-- @@IKONLAR@@ -->', ikonlar)
  .replace('/* @@BETIK@@ */', js);

/* ---- derleme denetimi 7: ÜRETİLEN BETİK ÇALIŞIYOR MU (06.09) ----
   ⚠️ BU DENETİM OLMADAN DERLEYİCİ YALAN SÖYLÜYORDU. Bir ekran
   betiğinde dizge içinde gerçek satır sonu kalmıştı; derleyici
   "derleme denetimi temiz" dedi, dosya üretildi, tarayıcıda AÇILDI —
   ve içindeki BÜTÜN betik ayrıştırılamadığı için Ekran, showScreen,
   hiçbir şey tanımlanmadı. Prototip sessizce ölü açıldı.

   Beş denetim de doğruydu; hiçbiri betiğin ÇALIŞIP ÇALIŞMADIĞINA
   bakmıyordu. Sınıf çakışması aramak, dosyanın ayrıştığını varsayar.
   En temel soru en sona kalmıştı: bu JavaScript geçerli mi?

   new Function ÇALIŞTIRMAZ, yalnız AYRIŞTIRIR — derleme sırasında
   hiçbir yan etki doğmaz. */
try {
  new Function(js);
} catch (e) {
  uyarilar.push('ÜRETİLEN BETİK AYRIŞMIYOR — ' + e.message +
    ' (prototip tarayıcıda ölü açılır)');
}

fs.writeFileSync(CIKTI, cikti, 'utf8');

/* ---- rapor ---- */
const kb = Math.round(cikti.length / 1024);
console.log(`prototypes/omurga.html üretildi — ${kb} KB · ${ekranAdlari.length} ekran`);
console.log('  ekranlar: ' + (ekranAdlari.join(', ') || '(yok)'));

if (uyarilar.length) {
  console.log('\nDERLEME UYARILARI (' + uyarilar.length + '):');
  uyarilar.forEach(u => console.log('  ! ' + u));
  process.exitCode = 1;
} else {
  console.log('  derleme denetimi temiz');
}
