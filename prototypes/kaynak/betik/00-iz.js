/* ============ İZ KAYDI ============
   Cemil, 16.09: *"Log kaydı, nereye tıklanmış, ne yapılmış adım adım,
   hatalar, başarısızlıklar. Kullanıcı bir hata aldığında, yanlış bir
   şey yaptığında uzaktan fark etmeliyiz. Mantığa aykırı işlemleri de
   görebilmeliyiz."*

   NEDEN GEREKLİ — v1'in dersi bu. Tamirci *"çok fazla özellik var,
   karmaşık geldi"* dedi ve bunu **sorduğumuz için** öğrendik. Usta
   destek aramaz; sessizce bırakır. İz kaydı, o sessizliği ölçüye
   çevirir.

   ⚠️ İZ KAYDI USTANIN İŞİ DEĞİL, BİZİM İŞİMİZ. Yedi işten hiçbirine
   hizmet etmiyor (geri bildirim §7). O yüzden ustaya **görünmez** ve
   ona **hiçbir şeye mal olmaz**: ekran yok, ayar yok, soru yok.

   ⚠️ DEĞER YAZILMAZ — ŞEKİL YAZILIR. İz kaydı, müşteri verisinin
   ikinci bir kopyası olamaz. "34 ABC 123 → 34 QBC 123" yazmak yerine
   "plaka alanı 3 kez düzeltildi" yazılır. Bu bir temenni değil,
   MEKANİK: `guvenli()` sayı, doğru/yanlış ve küçük harfli kısa kodlar
   dışında hiçbir şeyi geçirmez. Plaka, ad, telefon biçimce elenir —
   unutmakla değil, geçememekle. */

var Iz = (function () {
  'use strict';

  var TAVAN = 300;              /* halka tampon: son 300 olay */
  var olaylar = [];
  var baslangic = Date.now();
  var sonEkran = null, sonEkranAn = 0;
  var ekranSayaci = {};         /* şüphe kuralı: aynı ekrana kaç kez */
  var hataSayaci = {};

  /* Yalnız ŞEKİL geçer. Değer taşıyan her şey burada ölür. */
  function guvenli(d) {
    if (typeof d === 'number' && isFinite(d)) return Math.round(d * 100) / 100;
    if (typeof d === 'boolean') return d;
    if (typeof d === 'string' && /^[a-z][a-z0-9-]{0,23}$/.test(d)) return d;
    return null;
  }

  function temizle(ek) {
    if (!ek) return null;
    var c = null;
    for (var k in ek) {
      if (!Object.prototype.hasOwnProperty.call(ek, k)) continue;
      var v = guvenli(ek[k]);
      if (v === null) continue;
      c = c || {};
      c[k] = v;
    }
    return c;
  }

  function yaz(tur, ad, ek) {
    var o = { t: Date.now() - baslangic, tur: tur };
    var a = guvenli(ad);
    if (a !== null) o.ad = a;
    var e = temizle(ek);
    if (e) o.ek = e;
    olaylar.push(o);
    if (olaylar.length > TAVAN) olaylar.shift();
    return o;
  }

  /* ---- olay türleri ---- */

  function ekran(ad) {
    var simdi = Date.now();
    if (sonEkran) {
      /* Ekranda geçirilen süre: "burada takıldı mı" sorusunun cevabı.
         Ayrılırken yazılıyor, çünkü girerken bilinmiyor. */
      yaz('sure', sonEkran, { sn: (simdi - sonEkranAn) / 1000 });
    }
    sonEkran = guvenli(ad);
    sonEkranAn = simdi;
    yaz('ekran', ad);

    /* ŞÜPHE: aynı ekrana kısa sürede tekrar tekrar dönmek "aradığını
       bulamadı" demektir. Hata değil — ama hatadan çok şey anlatır. */
    var s = ekranSayaci[ad] = ekranSayaci[ad] || { say: 0, ilk: simdi };
    if (simdi - s.ilk > 20000) { s.say = 0; s.ilk = simdi; }
    s.say++;
    if (s.say === 3) yaz('suphe', 'kararsiz-gezinme', { ekran: guvenli(ad) });
  }

  function dokunus(kod) { yaz('dokunus', kod); }

  function hata(yer, e) {
    var sinif = (e && e.name ? String(e.name) : 'hata').toLowerCase()
      .replace(/[^a-z0-9-]/g, '-').slice(0, 24);
    yaz('hata', yer, { sinif: guvenli(sinif) || 'hata' });
    var n = hataSayaci[yer + '/' + sinif] = (hataSayaci[yer + '/' + sinif] || 0) + 1;
    /* ŞÜPHE: aynı hata ikinci kez — tesadüf değil, kalıp. */
    if (n === 2) yaz('suphe', 'tekrar-eden-hata', { yer: guvenli(yer) });
  }

  /* Mantığa aykırı işlem: kuralı ÇAĞIRAN yerde tanımlı, burada yalnız
     kaydediliyor. Kural listesi `docs/arastirma/IZ-KAYDI.md`'de. */
  function supheli(kod, ek) { yaz('suphe', kod, ek); }

  /* ---- dışarı ---- */
  function dok() { return olaylar.slice(); }
  function sil() { olaylar = []; ekranSayaci = {}; hataSayaci = {}; sonEkran = null; }
  function ozet() {
    var s = { olay: olaylar.length, ekran: 0, dokunus: 0, hata: 0, suphe: 0 };
    for (var i = 0; i < olaylar.length; i++) {
      if (s[olaylar[i].tur] !== undefined) s[olaylar[i].tur]++;
    }
    return s;
  }

  /* Dokunulan şeyin KİMLİĞİ değil, CİNSİ yazılır.
     Düğmenin metnini yazmak cazip ama metinde plaka olabilir ("34 ABC
     123 · Ali Yılmaz" kartı); id'de de olabilir. Sınıf adı ise sınıf
     sözlüğünden (K-008) gelir — yapıyı anlatır, içeriği anlatmaz. */
  function dokunusKodu(el) {
    var nav = el.getAttribute && el.getAttribute('data-nav');
    if (nav) return guvenli('nav-' + String(nav).toLowerCase()) || 'nav';
    var sinif = el.className && typeof el.className === 'string'
      ? el.className.split(/\s+/)[0] : '';
    return guvenli(String(sinif || el.tagName || '').toLowerCase()) || 'bilinmeyen';
  }

  /* Yakalanmamış hatalar da ize düşsün: bunlar bugün hiçbir yere
     yazılmıyor ve kullanıcı ekranın donduğunu görüp kapatıyor. */
  if (typeof window !== 'undefined') {
    document.addEventListener('click', function (o) {
      var t = o.target && o.target.closest
        ? o.target.closest('button, a, [data-nav], [role="button"]') : null;
      if (t) dokunus(dokunusKodu(t));
    }, true);

    window.addEventListener('error', function (o) {
      hata(sonEkran || 'bilinmeyen', { name: (o && o.error && o.error.name) || 'error' });
    });
    window.addEventListener('unhandledrejection', function () {
      hata(sonEkran || 'bilinmeyen', { name: 'sozverilmedi' });
    });
  }

  return {
    yaz: yaz, ekran: ekran, dokunus: dokunus, hata: hata, supheli: supheli,
    dok: dok, sil: sil, ozet: ozet, guvenli: guvenli
  };
})();
