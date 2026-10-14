/* ============ BÜYÜK DEĞERLENDİRME SETİ — ÜRETİCİ (14.10.2026) ============
   Cemil, 14.10: *"Daha fazla test yapabiliriz. Belki de 1000 adet okuma
   bile olabilir. Ne kadar çok olursa. Ayrıca beklediğimiz durumları da
   dikkate almalıyız."*

   NEDEN: 40 cümlelik set küçük ve motor ona BAKILARAK düzeltildi —
   "motor %86, Claude %75" rakamı sınavın sorularını görerek çalışmanın
   rakamıdır. Bu üretici:
     1) KURMACA BİR DÜKKÂN kurar (~60 araç, açık kayıtlar, satırlar) —
        plakalar Türk plaka kurallarına uygun (K-059.1) ve bilerek
        BİRBİRİNE ÇOK YAKIN plakalar içerir;
     2) yedi işin her biri için farklı söyleyişlerle ~1200 cümle üretir;
     3) seti İKİYE böler: `gelistirme` (motor buna bakılarak düzeltilebilir)
        ve `sinav` (BAKILMAZ — yalnız puanı okunur).

   ⚠️ CÜMLELER BİZİM TAHMİNİMİZ. Tamircinin komutu nasıl söylediğine dair
   elimizde cümle yok (TAMIRCI-GERIBILDIRIM yedi işi söylüyor, cümleyi
   değil). Bu set ANLAMA katmanını genişlikte sınar; ustanın gerçek
   konuşmasını ancak kendi kaydı gösterir.

   ⚠️ TOHUMLU: aynı tohum aynı seti verir. Set dosyası commit'lenir ve
   DONDURULUR; motoru düzelttikten sonra seti yeniden üretip "puan arttı"
   demek hiledir.

   KULLANIM:  node tools/degerlendirme/buyuk-set-uret.js [tohum]
   ÇIKTI:     tools/degerlendirme/buyuk-set.json
   ÖLÇÜM:     prototipte  eval(tarayicida-kos.js); DEGERLENDIR({ set, dukkan })
*/
'use strict';
const fs = require('fs');
const path = require('path');

const TOHUM = parseInt(process.argv[2] || '20261014', 10);

/* ---- tohumlu rastgele (mulberry32) ---- */
let durum = TOHUM >>> 0;
function rnd() {
  durum = (durum + 0x6D2B79F5) >>> 0;
  let t = durum;
  t = Math.imul(t ^ (t >>> 15), t | 1);
  t ^= t + Math.imul(t ^ (t >>> 7), t | 61);
  return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
}
const sec = a => a[Math.floor(rnd() * a.length)];
const tam = (a, b) => a + Math.floor(rnd() * (b - a + 1));

/* ---- TÜRK PLAKASI (K-059.1) ---- */
const PLAKA_HARFLERI = 'ABCDEFGHIJKLMNOPRSTUVYZ';
const ILLER = ['34', '34', '34', '34', '06', '06', '35', '35', '16', '41', '41', '07', '01',
               '42', '27', '55', '61', '38', '26', '33', '20', '10', '54', '59', '45', '09'];
function harfler(n) { let s = ''; for (let i = 0; i < n; i++) s += sec(PLAKA_HARFLERI.split('')); return s; }
function rakamlar(n) { let s = String(tam(1, 9)); for (let i = 1; i < n; i++) s += tam(0, 9); return s; }
function plakaUret() {
  const hs = sec([1, 2, 2, 3, 3, 3]);
  const sinir = { 1: [4, 5], 2: [3, 4], 3: [2, 3] }[hs];
  return sec(ILLER) + ' ' + harfler(hs) + ' ' + rakamlar(tam(sinir[0], sinir[1]));
}

/* ---- dükkânın dünyası ---- */
const ADLAR = ['Mehmet', 'Ahmet', 'Mustafa', 'Ali', 'Hüseyin', 'Hasan', 'İbrahim', 'İsmail',
  'Osman', 'Yusuf', 'Murat', 'Ömer', 'Ramazan', 'Halil', 'Süleyman', 'Abdullah', 'Mahmut',
  'Recep', 'Salih', 'Kemal', 'Fatma', 'Ayşe', 'Emine', 'Hatice', 'Zeynep', 'Elif', 'Meryem',
  'Şerife', 'Sultan', 'Zehra', 'Hacer', 'Hülya', 'Serkan', 'Volkan', 'Burak', 'Emre', 'Cem'];
const SOYADLAR = ['Yılmaz', 'Kaya', 'Demir', 'Şahin', 'Çelik', 'Yıldız', 'Yıldırım', 'Öztürk',
  'Aydın', 'Özdemir', 'Arslan', 'Doğan', 'Kılıç', 'Aslan', 'Çetin', 'Kara', 'Koç', 'Kurt',
  'Özkan', 'Şimşek', 'Polat', 'Korkmaz', 'Erdoğan', 'Güneş', 'Aksoy', 'Tekin', 'Bulut'];
const MODELLER = [['Renault', 'Clio'], ['Renault', 'Mégane'], ['Renault', 'Symbol'],
  ['Fiat', 'Egea'], ['Fiat', 'Doblo'], ['Fiat', 'Linea'], ['Volkswagen', 'Golf'],
  ['Volkswagen', 'Passat'], ['Volkswagen', 'Polo'], ['Volkswagen', 'Caddy'],
  ['Ford', 'Focus'], ['Ford', 'Transit'], ['Ford', 'Courier'], ['Toyota', 'Corolla'],
  ['Hyundai', 'i20'], ['Hyundai', 'Accent'], ['Opel', 'Astra'], ['Opel', 'Corsa'],
  ['Peugeot', '301'], ['Peugeot', '3008'], ['Dacia', 'Duster'], ['Honda', 'Civic'],
  ['Skoda', 'Octavia'], ['Citroën', 'C-Elysée'], ['Hyundai', 'Tucson'], ['Nissan', 'Qashqai']];
const TELEFON = () => '05' + tam(30, 59) + ' ' + tam(100, 999) + ' ' + tam(10, 99) + ' ' + tam(10, 99);

const SIKAYET = ['triger seti değişecek', 'fren balatası bitmiş', 'debriyaj kaçırıyor',
  'motor sarsıntı yapıyor', 'klima soğutmuyor', 'akü bitmiş', 'rot balans yapılacak',
  'yağ kaçırıyor', 'egzozdan duman atıyor', 'şanzımandan ses geliyor', 'far yanmıyor',
  'direksiyon titriyor', 'ön takımdan ses geliyor', 'radyatör su kaçırıyor',
  'marş basmıyor', 'arka amortisör değişecek', 'conta yakmış', 'enjektör arızası var',
  'arıza lambası yanıyor', 'buji değişecek'];
const BAKIM = ['bakıma geldi', 'periyodik bakım', 'yağ değişimi yapılacak', 'bakımı yapılacak'];
const PARCALAR = ['Fren balatası', 'Fren diski', 'Hava filtresi', 'Polen filtresi',
  'Yakıt filtresi', 'Yağ filtresi', 'Motor yağı', 'Triger seti', 'Su pompası', 'Termostat',
  'Buji', 'Ateşleme bobini', 'Amortisör', 'Rotil', 'Salıncak', 'Debriyaj seti', 'Akü',
  'Silecek', 'Far ampulü', 'Radyatör', 'Antifriz', 'V kayışı', 'Egzoz contası'];
const ISCILIK = ['Söküm takım', 'Rot balans', 'Bakım işçiliği', 'Arıza tespit', 'Kaynak'];
const GIDERLER = [['elektrik faturası', 'fatura'], ['su faturası', 'fatura'],
  ['doğalgaz faturası', 'fatura'], ['internet faturası', 'fatura'], ['kira', 'kira'],
  ['dükkân kirası', 'kira'], ['çay ocağı', 'gider'], ['yemek', 'gider'], ['yakıt', 'gider'],
  ['temizlik malzemesi', 'gider']];

/* ---- Türkçe sayı yazısı ---- */
const BIR = ['', 'bir', 'iki', 'üç', 'dört', 'beş', 'altı', 'yedi', 'sekiz', 'dokuz'];
const ON = ['', 'on', 'yirmi', 'otuz', 'kırk', 'elli', 'altmış', 'yetmiş', 'seksen', 'doksan'];
function yuzlu(n) {
  const y = Math.floor(n / 100), o = Math.floor((n % 100) / 10), b = n % 10;
  return [y ? (y === 1 ? 'yüz' : BIR[y] + ' yüz') : '', ON[o], BIR[b]].filter(Boolean).join(' ');
}
function sayiYazi(n) {
  const bin = Math.floor(n / 1000), kalan = n % 1000;
  const b = bin ? (bin === 1 ? 'bin' : yuzlu(bin) + ' bin') : '';
  return [b, yuzlu(kalan)].filter(Boolean).join(' ');
}
const binlik = n => n.toLocaleString('tr-TR');
/* "rotil DE", "balata DA" — son ünlüye göre. */
const deDa = s => /[aıou][^aeıioöuü]*$/i.test(s) ? 'da' : 'de';
function tutarSoyle(n) {
  return sec([
    () => n + ' lira', () => n + ' TL', () => binlik(n) + ' lira', () => sayiYazi(n) + ' lira'
  ])();
}
const TUTAR = () => sec([tam(2, 30) * 50, tam(2, 60) * 100, tam(1, 12) * 500, tam(3, 9) * 1000 + sec([0, 0, 500])]);

/* ---- DÜKKÂN ---- */
function dukkanKur() {
  const araclar = [], kullanilan = {};
  const plakaEkle = p => { if (kullanilan[p]) return false; kullanilan[p] = 1; return true; };
  const sahipler = [];
  for (let i = 0; i < 48; i++) {
    let ad = sec(ADLAR) + ' ' + sec(SOYADLAR);
    while (sahipler.indexOf(ad) >= 0) ad = sec(ADLAR) + ' ' + sec(SOYADLAR);
    sahipler.push(ad);
  }
  for (let i = 0; i < 56; i++) {
    let p = plakaUret(); while (!plakaEkle(p)) p = plakaUret();
    const m = sec(MODELLER);
    /* Bazı sahiplerin İKİ aracı var — "Mehmet'in arabası" belirsiz kalsın. */
    const sahip = i < 48 ? sahipler[i] : sahipler[tam(0, 47)];
    araclar.push({ plaka: p, marka: m[0], model: m[1], yil: String(tam(2008, 2024)),
                   sahipAd: sahip, sahipTel: rnd() < 0.85 ? TELEFON() : '' });
  }
  /* BİRBİRİNE ÇOK YAKIN KAYITLI PLAKALAR: tek harfi farklı ikiz. Biri
     söylendiğinde ÖTEKİ seçilmemeli; ikisinin arasındaki yanlış duyuş
     iki aday üretir → öneri YOK (K-047.6). */
  const ikizler = [];
  for (let i = 0; i < 6; i++) {
    const a = araclar[i * 3];
    const par = a.plaka.split(' ');
    const j = tam(0, par[1].length - 1);
    let yeniHarf = sec(PLAKA_HARFLERI.split('').filter(h => h !== par[1][j]));
    const p = par[0] + ' ' + par[1].slice(0, j) + yeniHarf + par[1].slice(j + 1) + ' ' + par[2];
    if (!plakaEkle(p)) continue;
    const m = sec(MODELLER);
    araclar.push({ plaka: p, marka: m[0], model: m[1], yil: String(tam(2008, 2024)),
                   sahipAd: sec(ADLAR) + ' ' + sec(SOYADLAR), sahipTel: TELEFON() });
    ikizler.push([a.plaka, p]);
  }
  /* Açık kayıtlar: araçların ~yarısı serviste. Satırları var. */
  const kayitlar = [];
  let id = 1;
  araclar.forEach((a, i) => {
    if (i % 2 === 1) return;
    const satirlar = [];
    const n = tam(1, 4);
    const kullanilanParca = {};
    for (let k = 0; k < n; k++) {
      let ad = sec(PARCALAR); while (kullanilanParca[ad]) ad = sec(PARCALAR);
      kullanilanParca[ad] = 1;
      satirlar.push({ tip: 'parca', ad: ad, fiyat: rnd() < 0.7 ? TUTAR() : 0, alis: null,
                      kimGetirdi: 'tamirci', marka: '', kod: '', tanim: 'tanimlanamadi' });
    }
    if (rnd() < 0.6) satirlar.push({ tip: 'iscilik', ad: sec(ISCILIK), fiyat: TUTAR(), alis: null, kimGetirdi: '' });
    kayitlar.push({ id: id++, plaka: a.plaka, tur: rnd() < 0.3 ? 'bakim' : 'tamirat', km: '',
                    sikayet: sec(SIKAYET), acilis: '10.10.2026', teslimTarihi: null,
                    odemeYolu: null, hatirlatmaTarihi: null, hatirlatmaGonderildi: false,
                    parcaBekliyor: false, bekliyorMusteriye: false, satirlar: satirlar });
  });
  /* Teslim edilmiş eski kayıtlar da var (hatırlatma, arama için). */
  araclar.forEach((a, i) => {
    if (i % 2 === 0 || rnd() < 0.5) return;
    kayitlar.push({ id: id++, plaka: a.plaka, tur: 'bakim', km: binlik(tam(40, 220) * 1000),
                    sikayet: 'Periyodik bakım', acilis: '15.03.2026', teslimTarihi: '15.03.2026',
                    odemeYolu: sec(['nakit', 'kart', 'havale']), hatirlatmaTarihi: '15.09.2026',
                    hatirlatmaGonderildi: false, parcaBekliyor: false, bekliyorMusteriye: false,
                    satirlar: [{ tip: 'parca', ad: 'Motor yağı', fiyat: TUTAR(), alis: null,
                                 kimGetirdi: 'tamirci', marka: '', kod: '', tanim: 'tanimlanamadi' }] });
  });
  return { araclar, kayitlar, ikizler };
}

/* ---- CÜMLELER ---- */
function cumleleriUret(D) {
  const C = [];
  const ekle = (sablon, baglam, cumle, beklenen, ek) =>
    C.push(Object.assign({ sablon, baglam, cumle, beklenen }, ek || {}));
  const acikKayit = D.kayitlar.filter(k => !k.teslimTarihi);
  const acikPlaka = {}; acikKayit.forEach(k => { acikPlaka[k.plaka] = k; });
  const kayitliPlaka = {}; D.araclar.forEach(a => { kayitliPlaka[a.plaka] = a; });
  const sahipSayisi = {}; D.araclar.forEach(a => { sahipSayisi[a.sahipAd] = (sahipSayisi[a.sahipAd] || 0) + 1; });
  const modelSayisi = {}; D.araclar.forEach(a => { const k = a.marka + ' ' + a.model; modelSayisi[k] = (modelSayisi[k] || 0) + 1; });
  const markaSayisi = {}; D.araclar.forEach(a => { markaSayisi[a.marka] = (markaSayisi[a.marka] || 0) + 1; });
  const plakaYaz = p => sec([p, p, p.toLocaleLowerCase('tr'), p.replace(/ (\d+)$/, (m, r) => ' ' + (r.length === 4 ? r.slice(0, 2) + ' ' + r.slice(2) : r))]);
  const kayitliOlmayan = () => { let p = plakaUret(); while (kayitliPlaka[p]) p = plakaUret(); return p; };
  const tekrar = (n, f) => { for (let i = 0; i < n; i++) f(i); };

  /* 1) SERVİS AÇ — kayıtlı araç */
  tekrar(140, () => {
    const a = sec(D.araclar), s = sec(SIKAYET);
    const bicim = tam(0, 2);
    const cumle = bicim === 0 ? plakaYaz(a.plaka) + ' ' + s
                : bicim === 1 ? s + ' ' + plakaYaz(a.plaka)
                : plakaYaz(a.plaka) + ' geldi ' + s;
    ekle('servis-ac/sikayet', sec(['genel', 'genel', 'servis-ekle']), cumle,
         { tur: 'servis-ac', plaka: a.plaka });
  });
  tekrar(60, () => {
    const a = sec(D.araclar), b = sec(BAKIM);
    const km = tam(20, 240);
    const kmli = rnd() < 0.5;
    const kmSoz = sec([' kilometre ' + km + ' bin', ' ' + km + ' bin kilometrede', ' kilometre ' + sayiYazi(km) + ' bin']);
    const bek = { tur: 'servis-ac', plaka: a.plaka, isTuru: 'bakim' };
    if (kmli) bek.km = binlik(km * 1000);
    ekle('servis-ac/bakim', 'genel', plakaYaz(a.plaka) + ' ' + b + (kmli ? kmSoz : ''), bek);
  });
  /* sahibinin adıyla — yalnız TEK aracı olan sahip; iki aracı olan
     sahipte doğru cevap plakayı UYDURMAMAK (plaka boş). */
  tekrar(40, () => {
    const a = sec(D.araclar), s = sec(SIKAYET);
    const tek = sahipSayisi[a.sahipAd] === 1;
    /* Türkçe iyelik eki: son ünlüye göre (ın/in/un/ün), ünlüyle bitiyorsa n'li */
    const son = (a.sahipAd.match(/[aeıioöuü](?=[^aeıioöuü]*$)/i) || ['e'])[0].toLocaleLowerCase('tr');
    const uyum = { a: "'ın", ı: "'ın", e: "'in", i: "'in", o: "'un", u: "'un", ö: "'ün", ü: "'ün" }[son] || "'in";
    const sonHarfUnlu = /[aeıioöuü]$/i.test(a.sahipAd);
    const iyelik = sonHarfUnlu ? uyum.replace("'", "'n") : uyum;
    ekle('servis-ac/sahip', 'genel', a.sahipAd + iyelik + ' arabası geldi ' + s,
         tek ? { tur: 'servis-ac', plaka: a.plaka } : { tur: 'plakasiz' },
         { not: tek ? 'sahibin tek aracı' : 'sahibin İKİ aracı var — hangisi bilinmiyor: ANLAMAMAK doğru' });
  });
  /* marka/model ile */
  tekrar(30, () => {
    const a = sec(D.araclar), s = sec(SIKAYET);
    const k = a.marka + ' ' + a.model;
    const tek = modelSayisi[k] === 1;
    ekle('servis-ac/model', 'genel', k + ' geldi ' + s,
         tek ? { tur: 'servis-ac', plaka: a.plaka } : { tur: 'plakasiz' },
         { not: tek ? 'dükkânda tek ' + k : 'birden çok ' + k + ' — hangisi bilinmiyor: ANLAMAMAK doğru' });
  });

  /* 2) YENİ ARAÇ — kayıtsız plaka */
  tekrar(60, () => {
    const p = kayitliOlmayan(), s = sec(SIKAYET);
    const telli = rnd() < 0.4, tel = TELEFON();
    const ad = sec(ADLAR) + ' ' + sec(SOYADLAR);
    const cumle = telli ? plakaYaz(p) + ' ' + ad + ' ' + tel + ' ' + s : plakaYaz(p) + ' ' + s;
    const bek = { tur: 'yeni-arac', plaka: p };
    if (telli) bek.tel = tel;
    ekle('yeni-arac', sec(['genel', 'servis-ekle', 'servis-ekle']), cumle, bek);
  });

  /* 3) TESLİM — açık kaydı olan araç */
  const ODEME = { nakit: ['nakit aldım', 'nakit ödedi', 'parayı nakit aldım', 'peşin aldım'],
                  kart: ['kartla aldım', 'kartla ödedi', 'kredi kartıyla ödedi', 'karttan çektim'],
                  havale: ['havale yaptı', 'havaleyle ödedi', 'EFT yaptı', 'hesaba yatırdı'],
                  yok: ['parayı almadım', 'sonra ödeyecek', 'veresiye yazdım', 'ödemeyi sonra yapacak'] };
  const TESLIM = ['teslim ettim', 'teslim edildi', 'teslim ettik', 'aracı teslim ettim'];
  tekrar(90, i => {
    const k = sec(acikKayit), od = sec(['nakit', 'nakit', 'kart', 'kart', 'havale', 'yok']);
    const soz = sec(ODEME[od]);
    const icerde = i % 3 === 0;
    const cumle = icerde ? sec(TESLIM) + ' ' + soz : plakaYaz(k.plaka) + ' ' + sec(TESLIM) + ' ' + soz;
    const bek = { tur: 'teslim', odeme: od === 'yok' ? null : od };
    if (!icerde) bek.plaka = k.plaka;
    ekle('teslim/' + od, icerde ? 'kayit-icinde' : sec(['genel', 'genel', 'servis-ekle']), cumle, bek,
         icerde ? { kayitId: k.id } : null);
  });

  /* 4) SATIR EKLE / FİYAT — açık kaydın içinde */
  tekrar(120, () => {
    const k = sec(acikKayit);
    const varOlan = k.satirlar.map(s => s.ad);
    let ad = sec(PARCALAR); while (varOlan.indexOf(ad) >= 0) ad = sec(PARCALAR);
    const fiyatli = rnd() < 0.65, n = TUTAR();
    const adSoz = ad.toLocaleLowerCase('tr');
    const cumle = fiyatli ? sec([adSoz + ' ' + tutarSoyle(n), adSoz + ' taktık ' + tutarSoyle(n),
                                 adSoz + ' değişti ' + tutarSoyle(n)])
                          : sec([adSoz + ' ' + deDa(adSoz) + ' değişecek', adSoz + ' değişecek', adSoz + ' takılacak']);
    const bek = { tur: 'satir-ekle', tip: 'parca' };
    if (fiyatli) bek.fiyat = n;
    ekle('satir-ekle/parca', 'kayit-icinde', cumle, bek, { kayitId: k.id });
  });
  tekrar(40, () => {
    const k = sec(acikKayit), n = TUTAR();
    ekle('satir-ekle/iscilik', 'kayit-icinde', sec(['işçilik ' + tutarSoyle(n), 'işçilik ücreti ' + tutarSoyle(n),
         'işçiliği ' + tutarSoyle(n) + ' yaz']), { tur: 'satir-ekle', tip: 'iscilik', fiyat: n }, { kayitId: k.id });
  });
  tekrar(80, () => {
    const k = sec(acikKayit), s = sec(k.satirlar), n = TUTAR();
    const adSoz = s.ad.toLocaleLowerCase('tr');
    ekle('fiyat-guncelle', 'kayit-icinde', sec([adSoz + ' ' + tutarSoyle(n), adSoz + ' fiyatı ' + tutarSoyle(n),
         adSoz + ' ' + tutarSoyle(n) + ' olsun']), { tur: 'fiyat-guncelle', fiyat: n }, { kayitId: k.id });
  });

  /* 5) GİDER / TAHSİLAT — muhasebe */
  tekrar(80, () => {
    const g = sec(GIDERLER), n = TUTAR();
    /* Ana ekranda "yemek 300 lira" bir gider DEĞİL, belirsiz bir cümle:
       orada yalnız gider/fatura/kira sözü geçen cümle gider sayılır. */
    const baglam = sec(['muhasebe', 'muhasebe', 'genel']);
    const kelimeli = g[1] !== 'gider';
    const bicimler = baglam === 'genel' && !kelimeli
      ? [tutarSoyle(n) + ' ' + g[0] + ' gideri ekle', g[0] + ' gideri ' + tutarSoyle(n)]
      : [tutarSoyle(n) + ' ' + g[0] + ' ekle', tutarSoyle(n) + ' ' + g[0] + ' gideri ekle',
         g[0] + ' ' + tutarSoyle(n), g[0] + ' gideri ' + tutarSoyle(n)];
    ekle('gider-ekle', baglam, sec(bicimler), { tur: 'gider-ekle', tutar: n });
  });
  const DONEM = { bugun: ['bugün ne kadar tahsil ettim', 'bugün kasaya ne girdi', 'bugün ne kadar para aldım', 'bugünkü tahsilat ne kadar'],
                  ay: ['bu ay ne kadar tahsil ettim', 'bu ay kasaya ne girdi', 'bu ayki tahsilat ne kadar'],
                  yil: ['bu yıl ne kadar tahsil ettim', 'bu yıl kasaya ne girdi', 'bu seneki tahsilat ne kadar'] };
  tekrar(40, () => {
    const d = sec(['bugun', 'bugun', 'ay', 'ay', 'yil']);
    ekle('tahsilat-sor/' + d, sec(['muhasebe', 'genel']), sec(DONEM[d]), { tur: 'tahsilat-sor', donem: d });
  });

  /* 6) MESAJ */
  const SABLON = { selam: ['selam mesajı gönder', 'selam yaz'], hatirlatma: ['bakım hatırlatması gönder', 'bakım hatırlatması at'],
                   hazir: ['aracınız hazır yaz', 'aracınız hazır de'], bekliyor: ['parça bekliyoruz de', 'parça bekliyoruz yaz'],
                   tesekkur: ['teşekkür mesajı gönder'] };
  const KANAL = { whatsapp: ['WhatsApp üzerinden', "WhatsApp'tan"], sms: ['SMS ile', "SMS'le"] };
  tekrar(90, () => {
    const a = sec(D.araclar), sb = sec(Object.keys(SABLON));
    const kanalli = rnd() < 0.4, kn = sec(['whatsapp', 'sms']);
    const cumle = plakaYaz(a.plaka) + ' sahibine ' + (kanalli ? sec(KANAL[kn]) + ' ' : '') + sec(SABLON[sb]);
    const bek = { tur: 'mesaj-gonder', plaka: a.plaka, sablon: sb };
    if (kanalli) bek.kanal = kn;
    ekle('mesaj-gonder/' + sb, sec(['mesaj', 'mesaj', 'genel']), cumle, bek);
  });
  tekrar(30, () => {
    const t = sec(['bayram', 'tesekkur', 'serbest']);
    const cumle = t === 'bayram' ? sec(['tüm müşterilere bayram mesajı gönder', 'herkese bayram mesajı at', 'bütün müşterilere bayramlarını kutla'])
                : t === 'tesekkur' ? sec(['tüm müşterilere teşekkür mesajı gönder', 'herkese teşekkür mesajı at'])
                : sec(['herkese SMS at yarın kapalıyız', 'tüm müşterilere yarın kapalıyız yaz', 'herkese dükkan pazartesi açık yaz']);
    const bek = { tur: 'toplu-mesaj' };
    if (t !== 'serbest') bek.metinTuru = t;
    if (/SMS/.test(cumle)) bek.kanal = 'sms';
    ekle('toplu-mesaj/' + t, sec(['mesaj', 'genel']), cumle, bek);
  });

  /* 7) ARA */
  tekrar(70, () => {
    const t = tam(0, 4);
    if (t === 0) { const a = sec(D.araclar); ekle('ara/plaka', 'araclar', plakaYaz(a.plaka) + ' ara', { tur: 'ara', hedef: 'araclar', terim: a.plaka }); }
    else if (t === 1) { const m = sec(Object.keys(markaSayisi)); ekle('ara/marka', 'araclar', m + ' ara', { tur: 'ara', hedef: 'araclar', terim: m }); }
    else if (t === 2) { ekle('ara/serviste', 'kayitlar', sec(['servistekileri göster', 'içeridekileri göster', 'serviste ne var']), { tur: 'ara', hedef: 'kayitlar', suzgec: 'serviste' }); }
    else if (t === 3) { const p = sec(PARCALAR).toLocaleLowerCase('tr'); ekle('ara/parca', 'kayitlar', p + ' ara', { tur: 'ara', hedef: 'kayitlar', terim: p }); }
    else { const a = sec(D.araclar); ekle('ara/musteri', 'musteriler', a.sahipAd + ' ara', { tur: 'ara', hedef: 'musteriler', terim: a.sahipAd }); }
  });

  /* 8) ANLAMSIZ — doğru cevap ANLAMAMAK */
  const SOHBET = ['hava çok soğuk bugün', 'çay söyle', 'öğlen yemeğe çıkıyorum', 'bugün çok iş var',
                  'kapıyı kapat', 'müzik aç', 'saat kaç', 'yarın erken geleceğim', 'bir de balata değişecek',
                  'ne diyordum ben', 'tamam tamam', 'şimdi geliyorum'];
  tekrar(40, () => ekle('plakasiz', 'genel', sec(SOHBET), { tur: 'plakasiz' }));

  return C;
}

const D = dukkanKur();
const cumleler = cumleleriUret(D);
/* Karıştır, numarala, ikiye böl: tek id geliştirme, çift id sınav. */
for (let i = cumleler.length - 1; i > 0; i--) { const j = Math.floor(rnd() * (i + 1)); [cumleler[i], cumleler[j]] = [cumleler[j], cumleler[i]]; }
cumleler.forEach((c, i) => { c.id = i + 1; c.yari = (i % 2 === 0) ? 'gelistirme' : 'sinav'; });

const cikti = {
  ad: 'Büyük sesli komut seti (üretilmiş)', surum: 1, tohum: TOHUM, tarih: '14.10.2026',
  aciklama: 'Kurmaca dükkân + yedi iş için üretilmiş cümleler. Cümleler BİZİM TAHMİNİMİZ — ustanın gerçek konuşması değil. ' +
            '`sinav` yarısına motor düzeltilirken BAKILMAZ.',
  kurallar: ['ses-seti.json ile aynı puanlama (puanla.js)',
             'beklenen.plaka null ise doğru cevap plakayı UYDURMAMAK'],
  dukkan: { araclar: D.araclar, kayitlar: D.kayitlar, ikizler: D.ikizler },
  cumleler
};
const yol = path.join(__dirname, 'buyuk-set.json');
fs.writeFileSync(yol, JSON.stringify(cikti, null, 1), 'utf8');
const say = {}; cumleler.forEach(c => { const k = c.beklenen.tur; say[k] = (say[k] || 0) + 1; });
console.log('tohum ' + TOHUM + ' · ' + D.araclar.length + ' araç · ' + D.kayitlar.filter(k => !k.teslimTarihi).length +
            ' açık kayıt · ' + D.ikizler.length + ' ikiz plaka · ' + cumleler.length + ' cümle');
console.log(JSON.stringify(say));
console.log('→ ' + yol);
