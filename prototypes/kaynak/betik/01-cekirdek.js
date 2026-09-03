/* ============ ÇEKİRDEK ============
   Ekran kaydı, gezinme, balon. Ekranlar buraya BAĞLANIR;
   çekirdek hiçbir ekranı bilmez.

   v1 DERSİ: her şey tek bir global kapsamdaydı ve aynı var-hoisting
   hatası SEKİZ kez, sınıf adı çakışması ÜÇ kez yaşandı. Burada her
   ekranın betiği derlemede kendi IIFE'sine sarılıyor — sorun
   disiplinle değil YAPIYLA çözülüyor. */

/* YUKLEME HATASI TOPLAYICISI — denetim bunu ariyor (TOPLAYICI-YOK).
   Sessizce olen bir betik, prototipte en pahali hata turudur: sayfa
   acilir, hicbir sey calismaz ve sebep gorunmez. */
/* Ad denetimin bekledigi ad — tools/saglik-denetim.js 19. test
   window.YUKLEME_HATALARI ariyor. */
window.YUKLEME_HATALARI = [];
window.addEventListener('error', function (e) {
  window.YUKLEME_HATALARI.push((e.message || 'hata') + ' @ ' + (e.filename || '') + ':' + (e.lineno || 0));
});
window.addEventListener('unhandledrejection', function (e) {
  window.YUKLEME_HATALARI.push('soz reddedildi: ' +
    (e.reason && e.reason.message ? e.reason.message : e.reason));
});

var Ekran = (function () {
  var kayit = {};     /* ad -> { ciz } */
  var gecmis = [];
  var acik = null;

  function tanim(ad, o) {
    if (kayit[ad]) { console.warn('Ekran iki kez tanımlandı: ' + ad); return; }
    kayit[ad] = o || {};
  }

  /* Ekranı gösterir. Geçmişe YAZMAZ — onu ac() yapar. */
  function goster(ad) {
    var el = document.getElementById('screen-' + ad);
    if (!el) { console.warn('Ekran yok: ' + ad); return false; }
    var hepsi = document.querySelectorAll('.screen');
    for (var i = 0; i < hepsi.length; i++) hepsi[i].classList.remove('acik');
    el.classList.add('acik');
    acik = ad;
    var k = kayit[ad];
    if (k && typeof k.ciz === 'function') {
      try { k.ciz(el); } catch (e) { console.error('ciz(' + ad + '):', e); }
    }
    document.querySelector('.govde').scrollTop = 0;
    window.scrollTo(0, 0);
    geriTazele();
    return true;
  }

  function ac(ad) {
    if (ad === acik) return;
    if (acik) gecmis.push(acik);
    goster(ad);
  }

  function geri() {
    if (!gecmis.length) return;
    goster(gecmis.pop());
  }

  function geriTazele() {
    var b = document.getElementById('geriBtn');
    if (b) b.disabled = gecmis.length === 0;
  }

  return { tanim: tanim, ac: ac, geri: geri, goster: goster,
           adi: function () { return acik; },
           /* Android geri tusu icin — cikilacak mi, geri mi gidilecek */
           geriVarMi: function () { return gecmis.length > 0; } };
})();

/* Denetim aracı (tools/saglik-denetim.js) bu ada göre çalışıyor —
   25 testi yeniden yazmamak için köprü bırakıldı. */
function showScreen(ad) { Ekran.goster(ad); }

/* ---- GEZİNME: data-git ----
   v1'de her düğme onclick="go('x')" yazıyordu; bu her işlevi GLOBAL
   olmaya zorluyordu ve ölü referanslar ancak çalışma anında
   görülüyordu. Burada gezinme VERİ, kod değil. */
document.addEventListener('click', function (e) {
  var t = e.target.closest('[data-git]');
  if (!t) return;
  e.preventDefault();
  Ekran.ac(t.getAttribute('data-git'));
});

/* ---- balon ---- */
function balon(metin, uyari, sure) {
  var kap = document.getElementById('balonKap');
  if (!kap) return;
  var d = document.createElement('div');
  d.className = 'balon' + (uyari ? ' uyari' : '');
  d.textContent = metin;
  kap.appendChild(d);
  setTimeout(function () { d.remove(); }, sure || 2600);
}

/* ---- ortak biçimleyiciler ---- */
function tutar(n) {
  return (n || 0).toLocaleString('tr-TR');
}
function norm(s) {
  return (s || '').toLocaleLowerCase('tr')
    .replace(/ı/g, 'i').replace(/ü/g, 'u').replace(/ö/g, 'o')
    .replace(/ş/g, 's').replace(/ç/g, 'c').replace(/ğ/g, 'g');
}

/* ---- açılış ---- */
document.addEventListener('DOMContentLoaded', function () {
  var g = document.getElementById('geriBtn');
  if (g) g.addEventListener('click', function () { Ekran.geri(); });
  Ekran.goster('home');
});
