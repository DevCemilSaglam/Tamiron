/* ============ KONTROL SAYFASI ÜRETİCİSİ ============
   Cemil, 15.09: *"Okuduğun alanları kontrol etmek istiyorum. Ruhsat ve
   parça listesi benim kontrol edebileceğim şekilde ver."*

   JSON kontrol edilecek biçim değil. Bu betik iki ölçüt dosyasını (ve
   varsa kişisel dosyayı) alıp TEK BİR SAYFA üretiyor: fotoğrafla yan
   yana konup işaretlenebilen, düzeltmeleri kaydeden bir sayfa.

   ⚠️ ÇIKTI DEPOYA GİRMEZ: kişisel dosya birleştiğinde içinde gerçek TC
   numarası ve adres olur. Varsayılan çıktı yeri depo dışı; `--cikti`
   ile değiştirilir.

   İKİ sayfa üretir: kontrol-ruhsat.html ve kontrol-parca.html.
   `--cikti` yalnız KLASÖRÜ belirlemek için kullanılır.

     node tools/degerlendirme/kontrol-uret.js

   Şablon `kontrol-sablon.html`; bu betik yalnız VERİYİ enjekte ediyor.
   Tasarım şablonda, veri burada — ikisi karışırsa biri ötekini bozar. */

const fs = require('fs');
const path = require('path');

const KOK = __dirname;
const arg = (ad, v) => { const i = process.argv.indexOf(ad); return i > 0 ? process.argv[i + 1] : v; };

/* Fotoğraf haritası (foto-esle.js üretir). Varsa sayfa fotoğrafların
   YANINA yazılır ve onlara göreli yoldan bakar: çift tıkla açıldığında
   da, yerel sunucudan açıldığında da resimler gelir. Mutlak `file:///`
   yolu gömülseydi sunucudan açıldığında tarayıcı hepsini engellerdi —
   sayfa açılır, kutular boş kalırdı; sessiz bir bozulma. */
const HARITA_YOL = path.join(KOK, 'foto-harita.json');
let harita = null;
if (fs.existsSync(HARITA_YOL)) harita = JSON.parse(fs.readFileSync(HARITA_YOL, 'utf8'));

const VARSAYILAN_CIKTI = harita && harita.kok
  ? path.join(harita.kok, 'kontrol.html')
  : path.join(KOK, 'kontrol.html');
const CIKTI = arg('--cikti', VARSAYILAN_CIKTI);

/* YAYIN KİPİ (--yayin <klasör>): sayfalar artifact olarak yayınlanmak
   üzere üretilir. Cemil, 16.09: *"Ruhsat ve parça listesi görseller
   bilgisayarda. Telefondan göremiyorum."*
   Fark iki yerde:
     · fotoğraf yolu diskteki dosya değil, yayına giden `foto/<id>.jpg`
     · öteki sayfaya geçiş dosya adı değil, artifact ADRESİ olur
   Yerel kipte hiçbir şey değişmiyor — iki kip tek şablonu paylaşıyor. */
const YAYIN = arg('--yayin', null);
const BAGLANTI = { ruhsat: arg('--url-ruhsat', null), parca: arg('--url-parca', null) };

function fotoYolu(id) {
  if (!harita || !harita.yollar || !harita.yollar[id]) return null;
  if (YAYIN) return 'foto/' + id + '.jpg';
  /* Göreli yol + URL kodlaması: dosya adlarında boşluk, parantez ve
     Türkçe harf var ("Parça listesi"); ham hâlde src'ye konursa kırılır. */
  const goreli = path.relative(path.dirname(CIKTI), harita.yollar[id]);
  return goreli.split(path.sep).map(encodeURIComponent).join('/');
}

const ruhsat = JSON.parse(fs.readFileSync(path.join(KOK, 'ruhsat-seti.json'), 'utf8'));
const parca = JSON.parse(fs.readFileSync(path.join(KOK, 'parca-seti.json'), 'utf8'));

/* Kişisel değerler ayrı dosyada (K-050) — varsa birleşir, yoksa o dört
   alan sayfada hiç görünmez ve bu SÖYLENİR. */
const KISISEL_YOL = path.join(KOK, 'ruhsat-seti-kisisel.json');
let kisisel = null;
if (fs.existsSync(KISISEL_YOL)) kisisel = JSON.parse(fs.readFileSync(KISISEL_YOL, 'utf8'));

const ALAN_ADI = {
  plaka: 'Plaka', ilkTescil: 'İlk tescil', marka: 'Marka', tip: 'Tip',
  ticariAd: 'Ticari ad', modelYili: 'Model yılı', cinsi: 'Cinsi', renk: 'Renk',
  motorNo: 'Motor no', sase: 'Şase no', silindir: 'Silindir hacmi',
  guc: 'Motor gücü (kW)', yakit: 'Yakıt', koltuk: 'Koltuk',
  tcVergiNo: 'TC / Vergi no', unvanSoyad: 'Soyad / Ünvan', ad: 'Ad', adres: 'Adres',
  hepsi: 'Belgenin tamamı'
};

/* Sıra ÖNEMLİ: belgede alanlar bu sırada duruyor. Ekranda başka sırada
   dizmek, ustanın gözünü belge üstünde ileri geri gezdirir. */
const RUHSAT_SIRA = ['plaka', 'ilkTescil', 'marka', 'tip', 'ticariAd', 'modelYili',
  'cinsi', 'renk', 'motorNo', 'sase', 'silindir', 'guc', 'yakit', 'koltuk',
  'tcVergiNo', 'unvanSoyad', 'ad', 'adres'];

const kayitlar = [];

ruhsat.belgeler.forEach(b => {
  const beklenen = Object.assign({}, b.beklenen);
  if (kisisel) {
    const ek = (kisisel.belgeler || {})[b.id];
    Object.keys(kisisel.alanlar || {}).forEach(a => {
      beklenen[a] = ek ? (ek[a] === undefined ? null : ek[a]) : null;
    });
  }
  kayitlar.push({
    id: b.id, tur: 'ruhsat', foto: fotoYolu(b.id),
    kagit: (b.belgeTipi === 'eski' ? 'eski tip belge' : 'yeni tip belge'),
    zorluk: b.zorluk || [], beklenen,
    guven: b.guven || null, not: b.not || '',
    ayni: b.ayniBelge || ''
  });
});

parca.listeler.forEach(l => {
  kayitlar.push({
    id: l.id, tur: 'parca', foto: fotoYolu(l.id), kagit: l.kagit || '',
    zorluk: l.zorluk || [], arac: l.arac || '', plaka: l.plaka || '', sase: l.sase || '',
    satirlar: l.satirlar || [], guven: l.guven || null, not: l.not || '',
    ayni: l.ayniListe || ''
  });
});

/* İKİ AYRI SAYFA (Cemil, 15.09: "Ayrı bir sayfa olsun").
   Ruhsat ile parça listesi iki ayrı iş: biri alan doğruluyor, öteki
   satır. Tek listede 73 kart alt alta gelince ikisi birbirine karışıyor
   ve ilerleme çubuğu "ne kadar kaldı"yı söylemez oluyordu. İşaretler
   ortak yerde durduğu için sayfalar arasında geçiş kaybettirmiyor. */
const SAYFALAR = [
  { dosya: 'kontrol-ruhsat.html', tur: 'ruhsat', baslik: 'Ruhsat Kontrolü',
    kayitlar: kayitlar.filter(k => k.tur === 'ruhsat') },
  { dosya: 'kontrol-parca.html', tur: 'parca', baslik: 'Parça Listesi Kontrolü',
    kayitlar: kayitlar.filter(k => k.tur === 'parca') }
];

const sablon = fs.readFileSync(path.join(KOK, 'kontrol-sablon.html'), 'utf8');
const cikisDizini = YAYIN || path.dirname(CIKTI);
if (YAYIN && !fs.existsSync(YAYIN)) fs.mkdirSync(YAYIN, { recursive: true });

SAYFALAR.forEach((sayfa, i) => {
  const oteki = SAYFALAR[(i + 1) % SAYFALAR.length];
  /* Yayında geçiş bağlantısı öteki artifact'in adresi olmalı; dosya adı
     orada bir yere gitmez. Adres henüz yoksa bağlantı hiç çizilmiyor —
     tıklanıp hiçbir şey yapmayan bir bağlantıdan iyidir. */
  const otekiHedef = YAYIN ? BAGLANTI[oteki.tur] : oteki.dosya;
  const veri = {
    uretim: new Date().toISOString().slice(0, 16).replace('T', ' '),
    baslik: sayfa.baslik,
    oteki: otekiHedef ? { ad: oteki.baslik + ' →', dosya: otekiHedef } : null,
    kisiselVar: !!kisisel,
    alanAdi: ALAN_ADI,
    ruhsatAlanSira: RUHSAT_SIRA,
    kayitlar: sayfa.kayitlar
  };
  /* </script> dizisi JSON'un içinde geçerse sayfayı erkenden kapatır —
     kaçırılmazsa sessiz bir bozulma olur. */
  const govde = JSON.stringify(veri).replace(/<\//g, '<\\/');
  /* ⚠️ CHARSET SATIRI ŞART. Bu dosya doğrudan açılıyor (çift tık, yerel
     sunucu) ve charset'siz tarayıcı latin-1 varsayıyor: "Parça" ->
     "ParÃ§a". Türkçe bir sayfada sessiz bozulma. */
  const html = '<meta charset="utf-8" />\n' +
    sablon.replace('/* @@VERI@@ */', 'var VERI = ' + govde + ';')
          .replace('@@BASLIK@@', sayfa.baslik);
  const yol = path.join(cikisDizini, sayfa.dosya);
  fs.writeFileSync(yol, html, 'utf8');
  console.log(sayfa.baslik + ': ' + yol + '  (' + sayfa.kayitlar.length + ' kayıt)');
});

const fotolu = kayitlar.filter(k => k.foto).length;
console.log(fotolu === kayitlar.length
  ? '  fotoğraflar bağlandı: ' + fotolu + '/' + kayitlar.length
  : '  ⚠️ fotoğrafı olmayan kayıt: ' + (kayitlar.length - fotolu) +
    ' — önce foto-esle.js koşulmalı');
console.log(kisisel
  ? '  kişisel alanlar BİRLEŞTİ (TC/ad/adres) — çıktı depoya konmaz'
  : '  ⚠️ ruhsat-seti-kisisel.json yok — TC/ad/adres sayfada GÖRÜNMEYECEK');
