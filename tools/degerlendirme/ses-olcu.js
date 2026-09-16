/* ============ SES KALİTE ÖLÇÜSÜ ============
   Cemil, 16.09: *"Kötü ses kayıtlarını nasıl bulabiliriz?"*

   Bir kaydın kötü olduğu ÜÇ yerde anlaşılmalı ve üçü de aynı cetveli
   kullanmalı — yoksa telefonun "iyi" dediğine bilgisayar "kötü" der:

     1) kayıt sırasında   (ses-kaydet.html — usta hemen tekrar etsin)
     2) set taranırken    (node ses-kalite.js — çöp kayıt ölçüte girmesin)
     3) ileride üründe    (mikrofon kötü duyduysa "tekrar söyler misin")

   Bu yüzden ölçü TEK DOSYADA ve iki dünyada da çalışıyor.

   ⚠️ EŞİKLER İLK TAHMİN. Gerçek dükkân kaydı elimize geçmeden bu
   sayılar bir varsayımdır; ilk 40 kayıttan sonra kalibre edilecek
   (A-M39). Eşiği kalibre etmeden "kaydın kötü" demek, ustayı boşuna
   tekrar konuşturur — ve ikinci kez boşuna tekrarlayan bir program
   bir daha açılmaz. */

(function (disari) {
  'use strict';

  var ESIK = {
    enKisaSaniye: 0.6,      /* bundan kısası cümle değil, kaza */
    sessizTepeDb: -40,      /* tepe bunun altındaysa mikrofon duymamış */
    kirpmaOrani: 0.01,      /* örneklerin %1'i tavana dayandıysa bozulmuş */
    dusukSnrDb: 15,         /* konuşma/gürültü farkı — altı "gürültülü" */
    konusmaYokSnrDb: 6,     /* bunun altında dinamik yok: konuşma değil, düz gürültü */
    cokSessizlikOrani: 0.9  /* kaydın %90'ı sessizse konuşma yok sayılır */
  };

  function db(x) { return x <= 1e-9 ? -120 : 20 * Math.log10(x); }

  function yuzdelik(dizi, p) {
    if (!dizi.length) return 0;
    var s = dizi.slice().sort(function (a, b) { return a - b; });
    var i = Math.min(s.length - 1, Math.max(0, Math.round((s.length - 1) * p)));
    return s[i];
  }

  /* Konuşma seviyesi ve gürültü tabanı. DIŞARI AÇIK, çünkü ses-bozucu.js
     de aynı tanımı kullanmak zorunda: bozucu "6 dB gürültü kat" derken
     ölçünün 6 dB dediği şeyi kastetmeli. İki ayrı tahminciyle çalışınca
     hedef 6 dB'e 8,4 dB ölçülüyordu — etiket yalan söylüyordu. */
  function seviyeler(ornekler, hiz) {
    var pencere = Math.max(1, Math.round(hiz * 0.02));
    var cerceveler = [];
    for (var b = 0; b + pencere <= ornekler.length; b += pencere) {
      var t = 0;
      for (var j = 0; j < pencere; j++) { var x = ornekler[b + j]; t += x * x; }
      cerceveler.push(Math.sqrt(t / pencere));
    }
    return {
      cerceveler: cerceveler,
      konusma: yuzdelik(cerceveler, 0.9),
      taban: yuzdelik(cerceveler, 0.1)
    };
  }

  /* ornekler: -1..1 arası Float32Array / dizi · hiz: örnekleme hızı (Hz) */
  function olc(ornekler, hiz) {
    var n = ornekler.length;
    var sure = n / hiz;

    var tepe = 0, kirpik = 0, kareToplam = 0;
    for (var i = 0; i < n; i++) {
      var v = ornekler[i], m = v < 0 ? -v : v;
      if (m > tepe) tepe = m;
      if (m >= 0.99) kirpik++;
      kareToplam += v * v;
    }
    var rms = Math.sqrt(kareToplam / Math.max(1, n));

    /* 20 ms'lik pencerelerde RMS: konuşma ile gürültü tabanını ayırmak
       için. Tek bir ortalama ikisini birbirine karıştırır.
       Konuşma = üst %10'un eşiği, taban = alt %10; ortalama yerine
       yüzdelik, çünkü tek bir kapı çarpması ortalamayı zıplatır. */
    var sv = seviyeler(ornekler, hiz);
    var cerceveler = sv.cerceveler, konusma = sv.konusma, taban = sv.taban;
    var snr = db(konusma) - db(Math.max(taban, 1e-6));

    /* Sessiz çerçeve: taban ile konuşma arasının alt çeyreği */
    var kapi = taban + (konusma - taban) * 0.25;
    var sessiz = 0;
    for (var k = 0; k < cerceveler.length; k++) if (cerceveler[k] <= kapi) sessiz++;
    var sessizlikOrani = cerceveler.length ? sessiz / cerceveler.length : 1;

    return {
      sure: sure,
      hiz: hiz,
      tepeDb: db(tepe),
      rmsDb: db(rms),
      kirpmaOrani: n ? kirpik / n : 0,
      snrDb: snr,
      sessizlikOrani: sessizlikOrani
    };
  }

  /* Ölçüyü KARARA çevirir. Sıra önemli: en ağır kusur önce söylenir,
     yoksa "gürültülü" diyip asıl sorunun boş kayıt olduğunu gizleriz. */
  function karar(o) {
    var kusurlar = [];
    if (o.tepeDb < ESIK.sessizTepeDb) {
      kusurlar.push({ kod: 'sessiz', agir: true,
        mesaj: 'Ses gelmemiş — mikrofon kapalı ya da çok uzak olabilir' });
    }
    if (o.sure < ESIK.enKisaSaniye) {
      kusurlar.push({ kod: 'kisa', agir: true,
        mesaj: 'Çok kısa (' + o.sure.toFixed(1) + ' sn) — cümle tamamlanmamış' });
    }
    if (o.sessizlikOrani > ESIK.cokSessizlikOrani) {
      kusurlar.push({ kod: 'bos', agir: true,
        mesaj: 'Kaydın neredeyse tamamı sessiz' });
    }
    if (o.kirpmaOrani > ESIK.kirpmaOrani) {
      kusurlar.push({ kod: 'kirpik', agir: true,
        mesaj: 'Ses tavana dayanmış (%' + (o.kirpmaOrani * 100).toFixed(1) +
               ') — mikrofona çok yakın' });
    }
    /* İki ayrı şey, iki ayrı ağırlık:
         · konuşma VAR ama gürültü yüksek  -> zayıf, model yine de deneyebilir
         · hiç konuşma yok, düz gürültü    -> ağır, denemeye değmez
       Ayıran şey DİNAMİK: konuşmanın yüksek ve alçak çerçeveleri vardır,
       düz gürültünün yoktur. Tek eşikle bakılsaydı ikisi aynı görünür ve
       boş kayıt "biraz gürültülü" diye ölçüte girerdi. */
    if (o.snrDb < ESIK.konusmaYokSnrDb) {
      kusurlar.push({ kod: 'konusma-yok', agir: true,
        mesaj: 'Konuşma duyulmuyor (' + o.snrDb.toFixed(0) + ' dB) — kayıtta düz gürültü var' });
    } else if (o.snrDb < ESIK.dusukSnrDb) {
      kusurlar.push({ kod: 'gurultulu', agir: false,
        mesaj: 'Gürültü yüksek (' + o.snrDb.toFixed(0) + ' dB) — anlaşılmayabilir' });
    }

    var agirVar = kusurlar.some(function (k) { return k.agir; });
    return {
      durum: !kusurlar.length ? 'iyi' : (agirVar ? 'kotu' : 'zayif'),
      kusurlar: kusurlar,
      olcum: o
    };
  }

  function degerlendir(ornekler, hiz) { return karar(olc(ornekler, hiz)); }

  disari.ESIK = ESIK;
  disari.seviyeler = seviyeler;
  disari.olc = olc;
  disari.karar = karar;
  disari.degerlendir = degerlendir;

})(typeof module !== 'undefined' && module.exports
   ? module.exports
   : (window.SesOlcu = {}));
