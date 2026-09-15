/* ============ FOTOĞRAF EŞLEŞTİRME ============
   Cemil, 15.09: *"Sana gönderdiğim fotolar C:\Users\Cemil\Desktop\
   Tamiron\Ruhsat altında. Link olarak ver her bir ruhsat için."*

   Ölçütteki r01…r46 / p01…p27 numaraları, fotoğrafların SOHBETE
   DÜŞTÜĞÜ sıradan geliyor. Dosya adlarından bu sırayı geri kurmaya
   çalışmak YANLIŞ SONUÇ verdi ve iki kez gözle yakalandı:

     indirme sırasının 1.si  ->  p16 (Performance blok, Pejo 307)
     indirme sırasının 13.ü  ->  p21 (yan çekilmiş beyaz kâğıt)

   WhatsApp indirme sırası, gönderme sırası değil; üstelik klasörde
   gönderilmemiş 7 eski fotoğraf da duruyor. Adla eşleştirme burada
   her seferinde sessizce kayar — ve kayan bir eşleme, ölçütü çöpe
   çevirir: kontrol eden kişi BAŞKA belgeye bakıp "yanlış okumuşsun"
   der, ölçüt bozulur.

   Bu yüzden ad yerine İÇERİK eşleştiriliyor. Sohbet kaydı (.jsonl)
   gönderilen görselleri base64 olarak taşıyor; her birinin MD5'i
   diskteki dosyanın MD5'iyle karşılaştırılıyor. Aynı bayt = aynı
   fotoğraf. 15.09 koşusunda **73/73 birebir** tuttu.

     node tools/degerlendirme/foto-esle.js <sohbet.jsonl> [foto-kökü]

   Çıktı: foto-harita.json  ->  { kok, yollar: { "r01": "C:\\...jpeg", … } }
   ⚠️ Harita depoya girmez: içinde gerçek dosya yolları var ve
   fotoğrafların kendisi zaten depo dışı (.gitignore). */

const fs = require('fs');
const path = require('path');
const crypto = require('crypto');
const readline = require('readline');

const JSONL = process.argv[2];
const KOK = process.argv[3] || path.join(process.env.USERPROFILE || '', 'Desktop', 'Tamiron');

if (!JSONL || !fs.existsSync(JSONL)) {
  console.error('sohbet kaydı gerekli: node foto-esle.js <sohbet.jsonl> [foto-kökü]');
  process.exit(1);
}

/* ---- 1) Diskteki dosyalar: MD5 -> yol ---- */
function topla(dizin) {
  let cikti = [];
  if (!fs.existsSync(dizin)) return cikti;
  for (const ad of fs.readdirSync(dizin)) {
    const tam = path.join(dizin, ad);
    if (fs.statSync(tam).isDirectory()) cikti = cikti.concat(topla(tam));
    else if (/\.(jpe?g|png|webp)$/i.test(ad)) cikti.push(tam);
  }
  return cikti;
}

const diskDosyalari = [].concat(topla(path.join(KOK, 'Ruhsat')), topla(path.join(KOK, 'Parça listesi')));
const diskte = {};
diskDosyalari.forEach(f => {
  const md5 = crypto.createHash('md5').update(fs.readFileSync(f)).digest('hex');
  (diskte[md5] = diskte[md5] || []).push(f);
});

/* ---- 2) Sohbetteki görseller: geliş sırasıyla ---- */
const gorseller = [];
const rl = readline.createInterface({ input: fs.createReadStream(JSONL), crlfDelay: Infinity });

rl.on('line', (l) => {
  if (l.indexOf('"image"') < 0) return;
  let o; try { o = JSON.parse(l); } catch (e) { return; }
  const m = o.message || o;
  if (!m || m.role !== 'user' || !Array.isArray(m.content)) return;
  for (const p of m.content) {
    if (p && p.type === 'image' && p.source && p.source.data) {
      gorseller.push(crypto.createHash('md5').update(Buffer.from(p.source.data, 'base64')).digest('hex'));
    }
  }
});

rl.on('close', () => {
  /* Ruhsatlar önce gönderildi (46), parça listeleri sonra (27).
     Sayılar tutmazsa DURULUR — eksik bir haritayla devam etmek,
     yanlış fotoğrafı doğru sanmaktan farksız. */
  const RUHSAT = 46, PARCA = 27;
  if (gorseller.length !== RUHSAT + PARCA) {
    console.error('⚠️ sohbette ' + gorseller.length + ' görsel var, beklenen ' +
                  (RUHSAT + PARCA) + ' — harita yazılmadı');
    process.exit(1);
  }

  const harita = {};
  const eksik = [];
  gorseller.forEach((md5, i) => {
    const id = i < RUHSAT
      ? 'r' + String(i + 1).padStart(2, '0')
      : 'p' + String(i - RUHSAT + 1).padStart(2, '0');
    const bulunan = diskte[md5];
    if (!bulunan) { eksik.push(id); return; }
    harita[id] = bulunan[0];
  });

  const cikti = path.join(__dirname, 'foto-harita.json');
  /* Kök de yazılıyor: kontrol sayfası fotoğrafların YANINA üretiliyor
     ve onlara göreli yoldan bakıyor. Mutlak yol gömülseydi sayfa
     başka bir makinede sessizce boş kutular gösterirdi. */
  fs.writeFileSync(cikti, JSON.stringify({ kok: KOK, yollar: harita }, null, 2), 'utf8');

  console.log('disk: ' + diskDosyalari.length + ' dosya · sohbet: ' + gorseller.length + ' görsel');
  console.log('eşleşen: ' + Object.keys(harita).length + '/' + gorseller.length +
              (eksik.length ? '  ⚠️ diskte bulunamayan: ' + eksik.join(', ') : '  (MD5 birebir)'));
  console.log('harita: ' + cikti);
});
