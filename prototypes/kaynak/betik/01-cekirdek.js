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

/* Hangi servis kaydi acik — ekranlar arasi tek paylasilan durum.
   Ortulu global birakilmiyor; nerede tanimlandigi belli olsun. */
var ACIK_KAYIT = null;
var ACIK_PLAKA = null;
/* Sesli isleme NE ICIN gidildigi. Ayni ekran farkli beklentilerle
   aciliyor: "servis-ekle" plaka bekler, "araclar" arama bekler. */
var SESLI_NIYET = null;

/* SESIN EKRANLARA BIRAKTIGI NOTLAR. Usta ayni seyi iki kez
   soylemesin: ses ne duyduysa gidecegi ekrana yaziyor, ekran da
   acilirken okuyup TEMIZLIYOR (birakilsa bir sonraki acilisa sizar). */
var RUHSAT_ONDOLGU = null;   /* {plaka, sahipAd, sahipTel, is, tur, km} */
var MESAJ_HEDEF = null;      /* {plaka, sablon} */
/* Müşteriye gönderilen özetin hangi kayda ait olduğu. Müşteri linkten
   gelseydi bu bilgi linkin içinde olurdu; prototipte usta hangi kaydı
   önizliyorsa o. */
var OZET_KAYIT = null;      /* kayit id */
var ARAMA_TERIMI = null;     /* {hedef, terim, suzgec} */

/* Sesli isleme BAGLAMLA gitmenin TEK yolu.
   Once her mikrofon "SESLI_NIYET = x; Ekran.ac('ses')" yaziyordu ve
   ekran ZATEN ACIKKEN Ekran.ac() erken donuyordu: baglam degisiyor,
   ornekler ve baslik ESKI kaliyordu. Tek yer olunca hata da tek. */
function sesleAc(niyet) {
  SESLI_NIYET = niyet || 'genel';
  if (Ekran.adi() === 'ses') Ekran.tazele();
  else Ekran.ac('ses');
}

var Ekran = (function () {
  var kayit = {};     /* ad -> { ciz } */
  var gecmis = [];
  /* v1'in header'inda ILERI dugmesi vardi (Cemil 03.09: "Header ayni
     sekilde"). Ileri yigini geri'ye basildikca dolar, YENI bir ekran
     acilinca temizlenir — tarayicinin kurali, ogrenilmis davranis. */
  var ileriYigin = [];
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
    /* v1 sozlesmesi: gorunur ekran .active — bilesen CSS'i ve denetim
       bu ada gore yazilmis, degistirilmez. */
    for (var i = 0; i < hepsi.length; i++) hepsi[i].classList.remove('active');
    el.classList.add('active');
    acik = ad;
    var k = kayit[ad];
    if (k && typeof k.ciz === 'function') {
      try { k.ciz(el); } catch (e) { console.error('ciz(' + ad + '):', e); }
    }
    el.scrollTop = 0;
    window.scrollTo(0, 0);
    geriTazele();
    return true;
  }

  /* Acik ekrani yeniden cizer — KAYDIRMAYI VE ODAGI BOZMADAN.
     goster() sayfayi basa atar; arama kutusunda her tusta basa
     atlamak ekrani kullanilamaz yapardi. */
  function tazele() {
    if (!acik) return;
    var k = kayit[acik];
    var el = document.getElementById('screen-' + acik);
    if (k && el && typeof k.ciz === 'function') {
      try { k.ciz(el); } catch (e) { console.error('ciz(' + acik + '):', e); }
    }
  }

  function ac(ad) {
    /* ⚠️ 04.09 — ZATEN ACIK EKRAN SESSIZCE CIZILMIYORDU.
       Eskiden burada duz "return" vardı: aynı ekrana ikinci kez
       gidilince hiçbir şey olmuyordu. Sorun, v2'de ekranlara
       KUYRUKTA VERI birakilarak gidilmesi: MESAJ_HEDEF, ARAMA_TERIMI,
       RUHSAT_ONDOLGU, SESLI_NIYET. Veri bırakılıyor, ekran açılıyor
       sanılıyor, ama ciz() koşmadığı için ekran BIR ONCEKI musteriyi
       gostermeye devam ediyordu — ve hata mesajı yok.

       Aynı tuzağa K-017'de sesli işlemle düşülmüştü ve orada tek
       giriş noktası (sesleAc) yazılarak çözülmüştü. Çözüm oraya özel
       kalınca kural taşınmadı; burada kökünden kapatılıyor:
       zaten açık ekrana gidilirse EKRAN TAZELENIR, geçmişe ikinci
       kez yazılmaz (yoksa geri düğmesi aynı ekranda sayar). */
    if (ad === acik) { tazele(); return; }
    if (acik) gecmis.push(acik);
    ileriYigin.length = 0;      /* yeni yol acildi — ileri gecmisi olur */
    goster(ad);
  }

  function geri() {
    if (!gecmis.length) return;
    if (acik) ileriYigin.push(acik);
    goster(gecmis.pop());
  }

  function ileri() {
    if (!ileriYigin.length) return;
    if (acik) gecmis.push(acik);
    goster(ileriYigin.pop());
  }

  function geriTazele() {
    var b = document.getElementById('geriBtn');
    if (b) b.disabled = gecmis.length === 0;
    var f = document.getElementById('ileriBtn');
    if (f) f.disabled = ileriYigin.length === 0;
  }

  return { tanim: tanim, ac: ac, geri: geri, ileri: ileri,
           goster: goster, tazele: tazele,
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
/* v1'in balonu: TEK oge, .show ile acilir. Yigin degil — ust uste
   balon birikmesi v1'de bilerek yapilmamisti. */
var balonSayaci = null;
function balon(metin, dinliyor, sure) {
  var t = document.getElementById('toast');
  if (!t) return;
  t.innerHTML = (dinliyor ? '<span class="dot-live"></span>' : '') +
                '<span>' + metin + '</span>';
  t.classList.toggle('dinliyor', !!dinliyor);
  t.classList.add('show');
  if (balonSayaci) clearTimeout(balonSayaci);
  balonSayaci = setTimeout(function () { t.classList.remove('show'); }, sure || 2600);
}

/* ============ SELAMLAMA — v1'den, üstüne CUMA ============
   Cemil, 03.09: "Hayırlı akşamlar, usta alanı için değişen selamlamalar
   hazırlamıştık. Onu buraya alalım. Buna bir de cuma günü için Hayırlı
   cumalar mesajı da ekleyelim, cuma günü göstersin."

   İki kural v1'den geliyor ve ikisi de bir hatadan doğdu:
   · SAAT PENCERESİ — sabit "günaydın" saat 13'te de günaydın diyordu
     (Cemil, 01.09: "saat 13, bana günaydın mesajı veriyor").
   · TEKRAR ETMEZ — düz rastgele seçim 200 denemede 26 kez üst üste
     aynı selamı veriyordu; "her açılışta farklı gelsin" denmişti. */
var SELAM_HITAP = 'usta';
var SELAMLAR = [
  { metin: 'Hayırlı işler',    saat: null },
  { metin: 'Bol kazançlar',    saat: null },
  { metin: 'Kolay gelsin',     saat: null },
  { metin: 'Bereketli olsun',  saat: null },
  { metin: 'Günaydın',         saat: [5, 11] },
  { metin: 'Hayırlı sabahlar', saat: [5, 11] },
  { metin: 'İyi günler',       saat: [11, 18] },
  { metin: 'İyi akşamlar',     saat: [18, 23] },
  { metin: 'Hayırlı akşamlar', saat: [18, 23] },
  { metin: 'Hayırlı geceler',  saat: [23, 5] },
  /* CUMA — yalnız cuma günü havuza girer. Sabahtan akşama kadar
     geçerli; cuma gecesi artık cumartesidir, o yüzden 23'te biter. */
  { metin: 'Hayırlı cumalar',  saat: [5, 23], gun: 5 }
];

function selamSaatUyar(s, saat) {
  if (!s.saat) return true;
  var a = s.saat[0], b = s.saat[1];
  return (a < b) ? (saat >= a && saat < b) : (saat >= a || saat < b);
}

var sonSelam = '';
function selamMetni(simdi) {
  var d = simdi || new Date();
  var saat = d.getHours(), gun = d.getDay();      /* 5 = cuma */
  var uygun = SELAMLAR.filter(function (s) {
    if (s.gun !== undefined && s.gun !== gun) return false;
    return selamSaatUyar(s, saat);
  });
  if (!uygun.length) uygun = [SELAMLAR[0]];
  var i = Math.floor(Math.random() * uygun.length);
  if (uygun.length > 1 && uygun[i].metin === sonSelam) i = (i + 1) % uygun.length;
  sonSelam = uygun[i].metin;
  return uygun[i].metin + ', ' + SELAM_HITAP;
}

/* ---- ortak biçimleyiciler ---- */
/* ⚠️ TÜRKÇEDE \b KULLANMA. JavaScript'in \b'si ASCII sınırına bakar:
   "ğ", "ş", "ı", "ç" kelime karakteri SAYILMAZ. Bu yüzden
   /\bde\b/ deseni "değişecek" kelimesinin başındaki "de"yi tutuyor ve
   geriye "ğişecek" kalıyordu (03.09'da sesle satır eklerken çıktı).
   Aynı tuzak /\bara\b/ ile "araç" kelimesini "ç"ye indirirdi.
   Kural: kelime atılacaksa BOŞLUKTAN BÖL, eşitlikle karşılaştır. */
function kelimeleriAt(metin, atilacaklar) {
  var kume = {};
  atilacaklar.forEach(function (k) { kume[norm(k)] = 1; });
  return (metin || '').split(/\s+/)
    .filter(function (p) { return p && !kume[norm(p.replace(/[.,!?]/g, ''))]; })
    .join(' ').trim();
}

/* "03.09.2026" -> Date. Cekmece'nin icinde ozeldi; ana ekran da
   hatirlatma tarihi karsilastirinca ORTAK NESNE KURALI devreye girdi:
   iki yerde iki ayri cozumleyici, iki ayri hata demektir. */
function tariheCevir(g) {
  var p = (g || '').split('.');
  return new Date(+p[2], (+p[1] || 1) - 1, +p[0] || 1);
}

/* ---- MÜŞTERİ ÖZETİNİN ADRESİ ----
   Kod plakadan ve kayıt numarasından türüyor. Backend yok: kod
   ÜRETİLİYOR ama çözülmüyor — sayfanın nasıl adresleneceği bir tasarım
   sorusu, uygulaması backend fazında.

   ⚠️ Plakada Türkçe harf yok ama BOŞLUK var; boşluk atılıyor, kelime
   sınırı ARANMIYOR (K-018).

   İki ekran kullanıyor (mesaj metni ve özet sayfası) — o yüzden burada:
   ekran betikleri derlemede kendi IIFE'sine sarılır ve bir ekranda
   tanımlanan fonksiyonu öteki göremez. */
function ozetKodu(k) {
  return (k.plaka || '').split(' ').join('').toLowerCase() + '-' + k.id;
}
function ozetLinki(k) {
  return 'tamiron.app/o/' + ozetKodu(k);
}

function tutar(n) {
  return (n || 0).toLocaleString('tr-TR');
}
function norm(s) {
  return (s || '').toLocaleLowerCase('tr')
    .replace(/ı/g, 'i').replace(/ü/g, 'u').replace(/ö/g, 'o')
    .replace(/ş/g, 's').replace(/ç/g, 'c').replace(/ğ/g, 'g');
}

/* ============ TEMA — ÜÇ HÂL ============
   Cemil, 03.09: "Tema konusu da devam edecek."

   Üç hâl var, iki değil: SİSTEM · AÇIK · KOYU. Sistem hâli gerçek bir
   seçenektir — telefonu akşam koyuya geçen usta, uygulamanın da
   geçmesini bekler. v1'de köke sabit data-theme="light" yazılıydı ve
   bu, sistem tercihini sessizce eziyordu.

   Seçim yoksa kökte data-theme YOKTUR; tokens.css'in
   prefers-color-scheme bloğu devreye girer. */
var Tema = (function () {
  var SIRA = ['sistem', 'acik', 'koyu'];
  var ANAHTAR = 'tamiron-tema';
  var secim = 'sistem';

  function sistemKoyuMu() {
    return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
  }

  function uygula() {
    var k = document.documentElement;
    if (secim === 'sistem') k.removeAttribute('data-theme');
    else k.setAttribute('data-theme', secim === 'koyu' ? 'dark' : 'light');
    ciz();
  }

  function ciz() {
    var btn = document.getElementById('temaBtn');
    if (!btn) return;
    /* Düğme BİR SONRAKİ hâli değil, ŞU ANKİ hâli söyler — v1'de
       "Koyu tema" yazan düğme koyu temadayken de öyle yazıyordu ve
       hangi hâlde olduğu okunamıyordu. */
    var etiket = { sistem: 'Telefona uy', acik: 'Açık tema', koyu: 'Koyu tema' }[secim];
    var ikon = (secim === 'koyu' || (secim === 'sistem' && sistemKoyuMu())) ? 'i-moon' : 'i-sun';
    document.getElementById('temaEtiket').textContent = etiket;
    document.getElementById('temaIkon').innerHTML = '<use href="#' + ikon + '"></use>';
    btn.setAttribute('aria-label', 'Tema: ' + etiket + ' — değiştirmek için dokun');
    var ip = document.getElementById('temaIpucu');
    if (ip) {
      ip.textContent = secim === 'sistem'
        ? 'Şu an telefonun ayarını izliyor (' + (sistemKoyuMu() ? 'koyu' : 'açık') + ').'
        : 'Telefonun ayarından bağımsız.';
    }
  }

  function dondur() {
    secim = SIRA[(SIRA.indexOf(secim) + 1) % SIRA.length];
    try { localStorage.setItem(ANAHTAR, secim); } catch (e) {}
    uygula();
  }

  function baslat() {
    try {
      var s = localStorage.getItem(ANAHTAR);
      if (SIRA.indexOf(s) !== -1) secim = s;
    } catch (e) {}      /* gizli sekmede localStorage atar — tema yine çalışsın */
    uygula();
    /* Sistem hâlindeyken telefon temasını değiştirirse ikon/ipucu
       kendiliğinden güncellensin. */
    if (window.matchMedia) {
      var mq = window.matchMedia('(prefers-color-scheme: dark)');
      var d = function () { if (secim === 'sistem') ciz(); };
      if (mq.addEventListener) mq.addEventListener('change', d);
      else if (mq.addListener) mq.addListener(d);
    }
  }

  return { baslat: baslat, dondur: dondur, secimi: function () { return secim; } };
})();

/* ============ ÇEKMECE ============
   K-009 ile geri geldi. Sağdan açılır, karartma üstüne biner.
   ERİŞİLEBİLİRLİK v1'den: aria-hidden + inert (arkadaki ekran
   klavyeye kapanır), ESC kapatır, odak çekmeceye girer ve
   kapanınca menü düğmesine döner. */
var Cekmece = (function () {
  var acikMi = false;

  function ogeler() {
    return {
      c: document.getElementById('drawer'),
      k: document.getElementById('drawerOverlay'),
      b: document.getElementById('menuBtn')
    };
  }

  /* ARKA PLAN KLAVYEYE KAPANIR. Yalnız çekmeceyi görünür yapmak
     yetmiyor: sekme tuşu arkadaki ekranın düğmelerinde gezinmeye
     devam ediyordu. Denetimin DRAWER-ODAK-SIZINTISI testi tam bu
     hatayı otomatikleştiriyor ve ilk yazımda yakaladı. */
  function arkaPlan(kapali) {
    var ust = document.querySelector('.topbar');
    if (ust) { if (kapali) ust.setAttribute('inert', ''); else ust.removeAttribute('inert'); }
    var ekranlar = document.querySelectorAll('.screen');
    for (var i = 0; i < ekranlar.length; i++) {
      if (kapali) ekranlar[i].setAttribute('inert', '');
      else ekranlar[i].removeAttribute('inert');
    }
  }

  function ac() {
    var o = ogeler();
    if (!o.c || acikMi) return;
    acikMi = true;
    o.c.classList.add('open');
    o.k.classList.add('open');
    o.c.removeAttribute('inert');
    o.c.setAttribute('aria-hidden', 'false');
    o.b.setAttribute('aria-expanded', 'true');
    arkaPlan(true);
    cizBildirimler();
    cizPeriyot();
    var ilk = o.c.querySelector('button, [tabindex]');
    if (ilk) ilk.focus();
  }

  function kapa() {
    var o = ogeler();
    if (!o.c || !acikMi) return;
    acikMi = false;
    o.c.classList.remove('open');
    o.k.classList.remove('open');
    o.c.setAttribute('inert', '');
    o.c.setAttribute('aria-hidden', 'true');
    arkaPlan(false);
    o.b.setAttribute('aria-expanded', 'false');
    o.b.focus();
  }

  /* BİLDİRİM = BAKIM HATIRLATMASI (yedinci iş). Uydurulmuş bildirim
     yok: yalnız tarihi gelmiş ve henüz gönderilmemiş hatırlatmalar.

     ⚠️ 04.09 — BURADA İKİNCİ BİR SÜZGEÇ VARDI ve ötekinden farklıydı.
     Çekmece kendi bekleyenHatirlatmalar()'ını yazmıştı; veri
     katmanındaki hatirlatmasiGelenler() ise ayrıca tur === 'bakim'
     süzüyordu. Yani hatırlatma tarihi olan bir TAMİRAT kaydı çekmecede
     "Bakım zamanı geldi" diye görünüyor, ana ekranda görünmüyordu.
     Aynı soruyu iki yerde cevaplamak, iki ayrı cevap demektir.
     Ana ekrandaki şerit 04.09'da kalkınca (Cemil: "Anasayfada bir
     aracın bakımı geldi alanını istemiyorum") tek kaynak kaldı. */
  function cizBildirimler() {
    var liste = (typeof hatirlatmasiGelenler === 'function')
      ? hatirlatmasiGelenler() : [];
    var bol = document.getElementById('notifSection');
    var rozet = document.getElementById('menuBadge');
    if (rozet) rozet.textContent = liste.length ? String(liste.length) : '';
    if (!bol) return;
    bol.hidden = liste.length === 0;
    document.getElementById('notifCount').textContent = String(liste.length);
    document.getElementById('notifList').innerHTML = liste.map(function (k) {
      var a = (typeof aracBul === 'function') ? aracBul(k.plaka) : null;
      return '<button class="notif-item" data-hatirlatma="' + k.id + '">' +
        '<span class="ni-ic warn"><svg class="ico ico-sm">' +
          '<use href="#i-calendar-clock"></use></svg></span>' +
        '<span class="ni-body">' +
          '<span class="ni-title">Bakım zamanı geldi</span>' +
          '<span class="ni-sub">' + k.plaka +
            (a ? ' · ' + a.marka + ' ' + a.model : '') + '</span>' +
        '</span>' +
      '</button>';
    }).join('');
  }

  /* Bakım periyodu — teslimde kurulan hatırlatmanın kaynağı (K-002).
     A-3 hâlâ açık; varsayılan 6 ay. */
  function cizPeriyot() {
    var g = document.getElementById('periyotGrup');
    if (!g || typeof DUKKAN === 'undefined') return;
    g.innerHTML = [3, 6, 12].map(function (ay) {
      return '<button class="cip" data-periyot="' + ay + '" aria-pressed="' +
        (DUKKAN.bakimPeriyoduAy === ay) + '">' + ay + ' ay</button>';
    }).join('');
  }

  return { ac: ac, kapa: kapa, ciz: cizBildirimler,
           acikMi: function () { return acikMi; } };
})();

/* ============ MİKROFONLAR — TEK DELEGASYON ============
   Mikrofon sesli işleme NE İÇİN basıldığı bilgisiyle gider (K-017):
   "araçlarda ara" ile "gider ekle" aynı ekranı açıyor ama ekran ne
   beklediğini biliyor ve örneklerini ona göre gösteriyor.

   ⚠️ 04.09'a kadar bu dinleyici ANA EKRANIN betiğindeydi. Ana ekran
   dışındaki hiçbir mikrofon yokken sorun değildi; arama kutularına
   mikrofon eklenince dört ekranın davranışı, hiç ilgisi olmayan bir
   ekranın dosyasından gelmeye başladı. Ana ekran bir gün kalkarsa
   ya da yeniden yazılırsa bütün mikrofonlar SESSİZCE ölürdü —
   tıklıyorsun, hiçbir şey olmuyor, hata da yok. */
document.addEventListener('click', function (e) {
  var m = e.target.closest('[data-sesle]');
  if (!m) return;
  sesleAc(m.getAttribute('data-sesle'));
});

/* ============ AÇILIR BÖLÜM (akordiyon) ============
   04.09, Cemil: "ruhsat bilgileri akordion olarak yer alsın. Altında
   km bilgileri yine akordion olarak olsun."

   DAVRANIŞ TEK YERDE (ortak nesne kuralı). Her ekran kendi açma
   kapama kodunu yazsaydı biri aria-expanded'ı unuturdu ve bölüm
   ekran okuyucuda "kapalı" görünürken açık dururdu.

   Açık/kapalı hâl EKRAN YENİDEN ÇİZİLİNCE SIFIRLANIR — bölümler
   innerHTML ile yeniden yazılıyor. Bu kasıtlı: usta bir ekrandan
   çıkıp döndüğünde ekranı yine sade bulur. */
var Akordiyon = (function () {
  function ac(bas, acik) {
    var govde = document.getElementById(bas.getAttribute('aria-controls'));
    if (!govde) return;
    bas.setAttribute('aria-expanded', acik ? 'true' : 'false');
    govde.hidden = !acik;
  }
  document.addEventListener('click', function (e) {
    var bas = e.target.closest('.akd-bas');
    if (!bas) return;
    ac(bas, bas.getAttribute('aria-expanded') !== 'true');
  });
  return { ac: ac };
})();

/* Akordiyon başlığı — govde id'si ile bağlanır. Sayaç (kaç satır var)
   açmadan içeriğin dolu mu boş mu olduğunu söyler; "içinde bir şey var
   mı" diye açtırmak, hiç açtırmamaktan kötüdür. */
function akordiyonHtml(id, ikon, baslik, sayac, govdeHtml) {
  return '<div class="akordiyon">' +
    '<button class="akd-bas" aria-expanded="false" aria-controls="' + id + '">' +
      '<svg class="ico ico-sm"><use href="#' + ikon + '"></use></svg>' + baslik +
      (sayac ? '<span class="akd-say">' + sayac + '</span>' : '<span class="akd-say"></span>') +
      '<svg class="ico ico-sm akd-ok"><use href="#i-chevron-down"></use></svg>' +
    '</button>' +
    '<div class="akd-govde" id="' + id + '" hidden>' + govdeHtml + '</div>' +
  '</div>';
}

/* Denetimin 14. testi (DRAWER-ODAK-SIZINTISI) v1'in adlarini ariyor.
   Cekmece v2'de modul icinde; test yeniden yazilmasin diye kopru. */
function openDrawer() { Cekmece.ac(); }
function closeDrawer() { Cekmece.kapa(); }

/* ---- açılış ---- */
document.addEventListener('DOMContentLoaded', function () {
  var bagla = function (id, fn) {
    var e = document.getElementById(id);
    if (e) e.addEventListener('click', fn);
  };

  bagla('geriBtn', function () { Ekran.geri(); });
  bagla('ileriBtn', function () { Ekran.ileri(); });
  bagla('menuBtn', function () { Cekmece.ac(); });
  bagla('drawerClose', function () { Cekmece.kapa(); });
  bagla('drawerOverlay', function () { Cekmece.kapa(); });
  bagla('brandHome', function () { Ekran.ac('home'); });
  bagla('temaBtn', function () { Tema.dondur(); });

  /* ÜSTTEKİ ARA DÜĞMESİ KALKTI (Cemil, 03.09: "Üstteki aramayı
     kaldırabiliriz"). Her listenin kendi arama kutusu var; header'daki
     düğme "hangi listede arıyorum" sorusunu doğuruyordu. Çekmecedeki
     "Plaka ara" duruyor — o, listeye GİDEN bir yol. */
  bagla('drawerAra', function () {
    Cekmece.kapa();
    Ekran.ac('araclar');
    var k = document.getElementById('arcQ');
    if (k) { k.focus(); k.select(); }
  });

  document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape' && Cekmece.acikMi()) Cekmece.kapa();
  });

  /* Çekmece içinden gezinince çekmece kapanmalı — açık kalırsa
     kullanıcı gittiği ekranı göremez. */
  document.addEventListener('click', function (e) {
    var t = e.target.closest('[data-git], [data-hatirlatma], [data-periyot]');
    if (!t || !Cekmece.acikMi()) return;

    var hp = t.getAttribute('data-periyot');
    if (hp) {
      DUKKAN.bakimPeriyoduAy = parseInt(hp, 10);
      Cekmece.ciz();
      var g = document.getElementById('periyotGrup');
      if (g) {
        var cipler = g.querySelectorAll('[data-periyot]');
        for (var i = 0; i < cipler.length; i++) {
          cipler[i].setAttribute('aria-pressed',
            String(+cipler[i].getAttribute('data-periyot') === DUKKAN.bakimPeriyoduAy));
        }
      }
      balon('Bakım periyodu ' + hp + ' ay oldu.');
      return;                        /* ayar çekmecede kalır, kapatmaz */
    }

    var hb = t.getAttribute('data-hatirlatma');
    if (hb) {
      ACIK_KAYIT = parseInt(hb, 10);
      var kk = (typeof kayitBul === 'function') ? kayitBul(ACIK_KAYIT) : null;
      if (kk) { ACIK_PLAKA = kk.plaka; }
      Cekmece.kapa();
      Ekran.ac('arac');
      return;
    }

    Cekmece.kapa();
  });

  Tema.baslat();
  Ekran.goster('home');
  Cekmece.ciz();          /* rozet açılışta doğru sayıyı göstersin */
});
