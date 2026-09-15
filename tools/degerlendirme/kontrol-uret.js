/* ============ KONTROL SAYFASI ÜRETİCİSİ ============
   Cemil, 15.09: *"Okuduğun alanları kontrol etmek istiyorum. Ruhsat ve
   parça listesi benim kontrol edebileceğim şekilde ver."*

   JSON kontrol edilecek biçim değil. Bu betik iki ölçüt dosyasını (ve
   varsa kişisel dosyayı) alıp TEK BİR SAYFA üretiyor: fotoğrafla yan
   yana konup işaretlenebilen, düzeltmeleri kaydeden bir sayfa.

   ⚠️ ÇIKTI DEPOYA GİRMEZ: kişisel dosya birleştiğinde içinde gerçek TC
   numarası ve adres olur. Varsayılan çıktı yeri depo dışı; `--cikti`
   ile değiştirilir.

     node tools/degerlendirme/kontrol-uret.js --cikti C:\yol\kontrol.html

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

/* Göreli yol + URL kodlaması: dosya adlarında boşluk, parantez ve
   Türkçe harf var ("Parça listesi"); ham hâlde src'ye konursa kırılır. */
function fotoYolu(id) {
  if (!harita || !harita.yollar || !harita.yollar[id]) return null;
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

const veri = {
  uretim: new Date().toISOString().slice(0, 16).replace('T', ' '),
  kisiselVar: !!kisisel,
  alanAdi: ALAN_ADI,
  ruhsatAlanSira: RUHSAT_SIRA,
  kayitlar
};

const sablon = fs.readFileSync(path.join(KOK, 'kontrol-sablon.html'), 'utf8');
/* </script> dizisi JSON'un içinde geçerse sayfayı erkenden kapatır —
   kaçırılmazsa sessiz bir bozulma olur. */
const govde = JSON.stringify(veri).replace(/<\//g, '<\\/');
/* ⚠️ CHARSET SATIRI ŞART. Artifact yayınında kabuk kendi charset'ini
   ekliyor, ama bu dosya DOĞRUDAN da açılıyor (yerel sunucu, çift tık) ve
   o zaman tarayıcı latin-1 varsayıyor: "Parça" -> "ParÃ§a". Türkçe bir
   sayfada sessiz bozulma. */
const govdeHtml = '<meta charset="utf-8" />\n' +
  sablon.replace('/* @@VERI@@ */', 'var VERI = ' + govde + ';');
fs.writeFileSync(CIKTI, govdeHtml, 'utf8');

console.log('kontrol sayfası: ' + CIKTI);
console.log('  ruhsat: ' + ruhsat.belgeler.length + ' · parça: ' + parca.listeler.length +
            ' · toplam ' + kayitlar.length + ' kayıt');
const fotolu = kayitlar.filter(k => k.foto).length;
console.log(fotolu === kayitlar.length
  ? '  fotoğraflar bağlandı: ' + fotolu + '/' + kayitlar.length
  : '  ⚠️ fotoğrafı olmayan kayıt: ' + (kayitlar.length - fotolu) +
    ' — önce foto-esle.js koşulmalı');
console.log(kisisel
  ? '  kişisel alanlar BİRLEŞTİ (TC/ad/adres) — çıktı depoya konmaz'
  : '  ⚠️ ruhsat-seti-kisisel.json yok — TC/ad/adres sayfada GÖRÜNMEYECEK');
