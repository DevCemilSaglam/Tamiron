/* ============ İZ KAYDI ============
   Cemil, 16.09: *"Log kaydı, nereye tıklanmış, ne yapılmış adım adım,
   hatalar, başarısızlıklar. Kullanıcı bir hata aldığında, yanlış bir
   şey yaptığında uzaktan fark etmeliyiz. Mantığa aykırı işlemleri de
   görebilmeliyiz."*

   NEDEN GEREKLİ — v1'in dersi bu. Tamirci *"çok fazla özellik var,
   karmaşık geldi"* dedi ve bunu **sorduğumuz için** öğrendik. Usta
   destek aramaz; sessizce bırakır. İz kaydı, o sessizliği ölçüye
   çevirir.

   ⚠️ DEĞERLER YAZILIR (K-053 — 16.09'da kuralı GERİ ALDIM).
   İlk sürüm değerleri biçimce eliyordu: plaka, ad, girilen metin ize
   düşmesin diye. Cemil kuralı bozdu ve gerekçesi benimkinden güçlü:

     *"Bu zaten müşteri verisi saklıyor olacağız mecburen… Burada amaç
     kullanıcı hangi veriyi girmiş ve ne sorun yaşamış. Kullanıcı
     problem yaşadığında arayacağı kişi olacağım ve ona neyi yanlış
     yaptığını direkt söyleyebilmeliyim."*

   Eleğin koruduğu şey zaten korunmuyordu — müşteri verisi veritabanında
   duruyor. Elek yalnız **desteği kör** ediyordu: telefondaki ustaya
   "şurada plakayı yanlış yazmışsın" diyebilmek için o plakayı görmek
   gerekiyor.

   Kalan tek kural ÖLÇÜ: değer kırpılır (KIRP), çünkü uzun bir not
   halka tamponu tek başına doldurup ötekileri siler — o zaman iz
   yine kör olur, bu kez fazladan. */

var Iz = (function () {
  'use strict';

  var TAVAN = 300;              /* halka tampon: son 300 olay */
  var olaylar = [];
  var baslangic = Date.now();
  var sonEkran = null, sonEkranAn = 0;
  var ekranSayaci = {};         /* şüphe kuralı: aynı ekrana kaç kez */
  var hataSayaci = {};

  var KIRP = 80;                /* bir değerden yazılacak en fazla harf */

  /* Değer geçer; yalnız BOYU kırpılır. Uzun bir not halka tamponu tek
     başına doldurursa iz yine kör kalır — bu kez fazlalıktan. */
  function guvenli(d) {
    if (d === null || d === undefined) return null;
    if (typeof d === 'number' && isFinite(d)) return Math.round(d * 100) / 100;
    if (typeof d === 'boolean') return d;
    var m = String(d);
    if (!m.length) return null;
    return m.length > KIRP ? m.slice(0, KIRP) + '…' : m;
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

  /* Girilen değer. Destek çağrısının cevabı burada: "şu alana şunu
     yazmışsın". Aynı alan arka arkaya değişirse sayılıyor — "plaka
     alanını üç kez düzeltti" tek başına bir işaret. */
  var alanSayaci = {};
  function alan(ad, deger) {
    var n = alanSayaci[ad] = (alanSayaci[ad] || 0) + 1;
    yaz('alan', ad, { deger: deger, kez: n });
    if (n === 3) yaz('suphe', 'alan-tekrar-duzeltildi', { alan: ad });
    duzeltmeBak(ad, deger);
  }

  /* ---- MODELİN OKUDUĞU vs USTANIN YAZDIĞI ----
     Ürünün savunmasının maddesi burada (SAVUNMA.md). Bir modeli
     API'den çağırmak kopyalanabilir; **ustanın düzelttiği okuma**
     kopyalanamaz — o, bu dükkânlarda bu kâğıtlarla geçirilmiş zamandır.
     Her düzeltme hem ölçüte hem eğitime giden bir satırdır ve bugüne
     kadar HİÇBİR YERE yazılmıyordu: usta yanlışı düzeltiyor, düzeltme
     ekranda kalıyor, biz asla öğrenmiyorduk. */
  var modelDegerleri = {}, okumaKaynagi = null, okunanAlan = 0, duzeltilen = 0;

  function okuma(kaynak, degerler) {
    modelDegerleri = {}; okunanAlan = 0; duzeltilen = 0;
    okumaKaynagi = kaynak;
    for (var k in degerler) {
      if (!Object.prototype.hasOwnProperty.call(degerler, k)) continue;
      modelDegerleri[k] = (degerler[k] === null || degerler[k] === undefined)
        ? '' : String(degerler[k]);
      okunanAlan++;
    }
    yaz('okuma', kaynak, { alan: okunanAlan });
  }

  function duzeltmeBak(ad, deger) {
    if (!Object.prototype.hasOwnProperty.call(modelDegerleri, ad)) return;
    var m = modelDegerleri[ad];
    if (String(deger) === m) return;
    duzeltilen++;
    yaz('duzeltme', ad, { model: m, usta: deger, kaynak: okumaKaynagi });
    /* ŞÜPHE: okunan alanların yarısından çoğu düzeltildiyse sorun
       ustada değil OKUMADA. Tek tek bakınca görünmez; oran görünür. */
    if (okunanAlan && duzeltilen === Math.ceil(okunanAlan / 2)) {
      yaz('suphe', 'okuma-cogu-duzeltildi',
          { kaynak: okumaKaynagi, okunan: okunanAlan, duzeltilen: duzeltilen });
    }
  }

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
  function sil() {
    olaylar = []; ekranSayaci = {}; hataSayaci = {}; alanSayaci = {}; sonEkran = null;
    modelDegerleri = {}; okumaKaynagi = null; okunanAlan = 0; duzeltilen = 0;
  }
  function ozet() {
    var s = { olay: olaylar.length, ekran: 0, dokunus: 0, alan: 0,
              okuma: 0, duzeltme: 0, hata: 0, suphe: 0 };
    for (var i = 0; i < olaylar.length; i++) {
      if (s[olaylar[i].tur] !== undefined) s[olaylar[i].tur]++;
    }
    return s;
  }

  /* Dokunulan şeyin hem CİNSİ hem YAZISI. Yazı, telefondaki ustaya
     "hangi düğmeye bastın" diye sormayı gereksiz kılıyor. */
  function dokunusKodu(el) {
    var nav = el.getAttribute && el.getAttribute('data-nav');
    var sinif = el.className && typeof el.className === 'string'
      ? el.className.split(/\s+/)[0] : '';
    return nav ? 'nav-' + String(nav).toLowerCase()
               : (String(sinif || el.tagName || '').toLowerCase() || 'bilinmeyen');
  }
  function dokunusYazisi(el) {
    var y = (el.textContent || '').trim().replace(/\s+/g, ' ');
    return y ? guvenli(y) : null;
  }

  /* Yakalanmamış hatalar da ize düşsün: bunlar bugün hiçbir yere
     yazılmıyor ve kullanıcı ekranın donduğunu görüp kapatıyor. */
  if (typeof window !== 'undefined') {
    document.addEventListener('click', function (o) {
      var t = o.target && o.target.closest
        ? o.target.closest('button, a, [data-nav], [role="button"]') : null;
      if (t) yaz('dokunus', dokunusKodu(t), { yazi: dokunusYazisi(t) });
    }, true);

    /* GİRİLEN DEĞER — Cemil'in asıl istediği. "Kullanıcı hangi veriyi
       girmiş": alan adı + yazdığı. Tek dinleyici, her ekran için:
       ekran ekran çağrı serpiştirilseydi biri unutulur ve tam da
       aranan alan izde eksik olurdu. */
    document.addEventListener('change', function (o) {
      var t = o.target;
      if (!t || !t.tagName) return;
      var tur = String(t.tagName).toLowerCase();
      if (tur !== 'input' && tur !== 'select' && tur !== 'textarea') return;
      var alanAdi = t.id || t.name || t.getAttribute('data-alan') || tur;
      if (t.type === 'checkbox' || t.type === 'radio') {
        alan(alanAdi, t.checked);
      } else {
        alan(alanAdi, t.value);
      }
    }, true);

    window.addEventListener('error', function (o) {
      hata(sonEkran || 'bilinmeyen', { name: (o && o.error && o.error.name) || 'error' });
    });
    window.addEventListener('unhandledrejection', function () {
      hata(sonEkran || 'bilinmeyen', { name: 'sozverilmedi' });
    });
  }

  return {
    yaz: yaz, ekran: ekran, dokunus: dokunus, alan: alan, okuma: okuma,
    hata: hata, supheli: supheli, dok: dok, sil: sil, ozet: ozet,
    guvenli: guvenli, KIRP: KIRP
  };
})();
