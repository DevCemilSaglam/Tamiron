/* ============ SERVİS DETAYI — GERÇEK PROTOTİPTE ÖLÇÜM (12.10 · Tur 8.1) ============
   Karşılaştırma sayfasındaki (prototypes/yaklasimlar/servis-detay.html)
   üç görevi GERÇEK prototipte yürütür — aynı kayıt, aynı sayım kuralı:
   dokunmak 1 · alana yazmak için dokunmak 1, alan zaten odaktaysa 0.
   Sonuç sayfadaki "Bugün" satırına sabit yazıldı.

   NE ZAMAN: seçilen kurgu (A-M49) kayıt ekranına kurulunca TEKRAR koş —
   sayfadaki kurgunun sözü, prototipteki hâliyle tutuyor mu? Seçicileri
   yeni ekrana göre güncelle.

   NASIL (prototip localhost'ta açık, 320 px):
     await eval(await (await fetch('/tools/servis-detay-olc.js')).text())

   ⚠️ Kayıt 1'in satırlarını DEĞİŞTİRİR (karşılaştırmanın kaydına
   benzetir). Ölçümden sonra sayfayı yenile. */
(async function () {
  function bekle(ms) { return new Promise(function (r) { setTimeout(r, ms); }); }
  function hazirla() {
    var k = kayitBul(1);
    /* Karşılaştırma sayfasıyla AYNI kayıt: triger seti (fiyatsız) + işçilik */
    k.satirlar = [
      { tip: 'parca', ad: 'Triger seti', fiyat: 0, alis: null, kimGetirdi: 'tamirci', marka: '', kod: '', tanim: 'tanimlanamadi' },
      { tip: 'iscilik', ad: 'Triger işçiliği', fiyat: 1500, alis: null, kimGetirdi: '' }
    ];
    k.teslimTarihi = null; k.odemeYolu = null;
    ACIK_KAYIT = 1;
    Ekran.ac('kayit');
    return k;
  }
  var n = 0, iz = [];
  function tik(sec) {
    var el = typeof sec === 'string' ? document.querySelector(sec) : sec;
    if (!el) throw new Error('yok: ' + sec);
    el.click(); n++; iz.push('tik ' + (typeof sec === 'string' ? sec : el.id || el.className));
  }
  function yaz(sec, metin) {
    var el = typeof sec === 'string' ? document.querySelector(sec) : sec;
    if (!el) throw new Error('yok: ' + sec);
    if (document.activeElement !== el) { el.focus(); n++; iz.push('odak ' + sec); }
    el.value = metin; el.dispatchEvent(new Event('input', { bubbles: true }));
  }
  /* İlk kare: ekranın üstünden, eylem çubuğu varsa ÇUBUĞUN üst kenarına
     kadar (çubuğun altında kalan şey görünmez); yoksa 610 px.
     12.10 (K-058): adımlar kalktı — sekme yok, çubuk var. */
  var k = hazirla();
  await bekle(700);
  var ekran = document.querySelector('#screen-kayit');
  var ust = ekran.getBoundingClientRect().top;
  var cubuk = document.getElementById('kCubuk');
  var alt = cubuk ? cubuk.getBoundingClientRect().top : ust + 610;
  var gor = function (el) { if (!el) return false; var r = el.getBoundingClientRect(); return r.height > 0 && r.top < alt - 8 && r.bottom > ust && !el.closest('[hidden]'); };
  var ilk = {
    adim: (document.querySelector('#kAdimSerit [aria-selected="true"]') || {}).textContent || 'adımsız',
    satir: gor(document.querySelector('#screen-kayit [data-satir-ad]')),
    teslim: gor(document.querySelector('#screen-kayit [data-git="teslim"]')),
    karar: Array.prototype.filter.call(ekran.querySelectorAll('button, input, textarea'), gor).length
  };
  var sonuc = { gorev: {}, iz: {} };

  /* G1 — 13.10'dan beri PANEL (K-058): Ekle → ad → tutar → Ekle.
     Eski ekranda (kSatiraOdaklan) satır anında ekleniyordu; o yol da
     duruyor ki eski bir sürümde koşulabilsin. */
  k = hazirla(); n = 0; iz = []; await bekle(200);
  tik('#kParcaEkle'); await bekle(200);
  if (document.getElementById('kYeniAd')) {
    yaz('#kYeniAd', 'Balata');
    yaz('#kYeniTutar', '450');
    tik('#kYeniEkle'); await bekle(200);
    var son = k.satirlar.length - 1;
  } else {
    var son = k.satirlar.length - 1;
    yaz('[data-satir-ad="' + son + '"]', 'Balata');
    yaz('[data-satir-fiyat="' + son + '"]', '450');
  }
  sonuc.gorev.G1 = (k.satirlar[son].ad === 'Balata' && k.satirlar[son].fiyat === 450) ? n : null; sonuc.iz.G1 = iz.slice();

  /* G2 */
  k = hazirla(); n = 0; iz = []; await bekle(200);
  var f0 = document.querySelector('[data-satir-fiyat="0"]');
  if (f0.closest('[hidden]') && document.getElementById('kAdimTab2')) { tik('#kAdimTab2'); await bekle(150); f0 = document.querySelector('[data-satir-fiyat="0"]'); }
  yaz(f0, '3800');
  sonuc.gorev.G2 = k.satirlar[0].fiyat === 3800 ? n : null; sonuc.iz.G2 = iz.slice();

  /* G3 */
  k = hazirla(); n = 0; iz = []; await bekle(200);
  var ts = document.querySelector('#screen-kayit [data-git="teslim"]');
  if (ts.closest('[hidden]') && document.getElementById('kAdimTab3')) { tik('#kAdimTab3'); await bekle(150); }
  tik('#screen-kayit [data-git="teslim"]'); await bekle(200);
  tik('#tsOdeme [data-odeme="nakit"]'); await bekle(100);
  tik('#tsTeslim'); await bekle(100);
  sonuc.gorev.G3 = (k.teslimTarihi && k.odemeYolu === 'nakit') ? n : null; sonuc.iz.G3 = iz.slice();

  sonuc.satirIlkKare = ilk.satir; sonuc.teslimIlkKare = ilk.teslim; sonuc.karar = ilk.karar;
  sonuc.adim = ilk.adim; sonuc.tasma = 0; sonuc.kirpma = 0;
  return sonuc;
})()
