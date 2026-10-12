/* ============ SES BOZMA — ORTAK (12.10.2026 · A-M48) ============
   ses-bozucu.js'in içindeydi; dosyadan tanıma koşucusu (ses-dosya-kos.html)
   aynı bozmaları TARAYICIDA yapmak zorunda kalınca ortak oldu.

   NEDEN ORTAK: aynı hesap iki yerde yazılsaydı biri ötekinden kayardı.
   Bunun bedeli zaten bir kez ödendi — ses-bozucu.js'in kendi tahmincisi
   "6 dB" etiketli dosyayı 8,4 dB ölçüyordu (aşağıdaki not). Etiket ile
   cetvel aynı tanımı kullanmalı; dosya da aynı fonksiyonu.

   Seviyeler ses-olcu.js'ten (o da iki dünyada çalışıyor):
     Node    : require('./ses-bozma.js')         — ses-olcu'yu kendisi ister
     Tarayıcı: <script src="ses-olcu.js"> sonra <script src="ses-bozma.js">
               → window.SesBozma */
(function (disari, olcu) {
  'use strict';

  function rms(a, bas, son) {
    var t = 0;
    for (var i = bas; i < son; i++) t += a[i] * a[i];
    return Math.sqrt(t / Math.max(1, son - bas));
  }

  /* ⚠️ Seviyeler ses-olcu.js'ten geliyor — kendi tahmincimizi yazmıyoruz.
     Yazdığımızda "6 dB" etiketli dosya 8,4 dB ölçülüyordu: etiket ile
     cetvel ayrı tanım kullanınca eğri kayıyor ve kimse fark etmiyor. */
  function gurultuKat(ornekler, ortam, hedefSnrDb, hiz) {
    var konusma = olcu.seviyeler(ornekler, hiz).konusma;
    var ortamSev = olcu.seviyeler(ortam, hiz).taban || rms(ortam, 0, ortam.length) || 1e-6;
    var hedefGurultu = konusma / Math.pow(10, hedefSnrDb / 20);
    var k = hedefGurultu / ortamSev;
    var c = new Float32Array(ornekler.length);
    /* Ortam kaydı kısaysa başa sarılır — dükkân sesi zaten döngüsel. */
    for (var i = 0; i < ornekler.length; i++) {
      c[i] = ornekler[i] + ortam[i % ortam.length] * k;
    }
    return c;
  }

  /* Uzaklık: seviye düşer ve tizler söner (yüksek frekanslar havada
     daha çabuk yutulur). Tek kutuplu alçak geçiren yeterli — amaç
     gerçekçi bir akustik model değil, tanımayı zorlamak. */
  function uzaklastir(a, dbDusus, hiz) {
    var kazanc = Math.pow(10, -Math.abs(dbDusus) / 20);
    var kesim = 2200;
    var alfa = 1 / (1 + hiz / (2 * Math.PI * kesim));
    var c = new Float32Array(a.length);
    var onceki = 0;
    for (var i = 0; i < a.length; i++) {
      onceki = onceki + alfa * (a[i] - onceki);
      c[i] = onceki * kazanc;
    }
    return c;
  }

  function kirp(a, kazanc) {
    var c = new Float32Array(a.length);
    for (var i = 0; i < a.length; i++) c[i] = Math.max(-1, Math.min(1, a[i] * kazanc));
    return c;
  }

  /* Bir kaydın bütün varyantları — iki dünyada AYNI liste ve AYNI ad. */
  function varyantlar(ornekler, ortam, hiz) {
    var v = [['uzak', uzaklastir(ornekler, 12, hiz)], ['kirpik', kirp(ornekler, 6)]];
    if (ortam) [18, 12, 6].forEach(function (snr) { v.push(['snr' + snr, gurultuKat(ornekler, ortam, snr, hiz)]); });
    return v;
  }

  disari.rms = rms;
  disari.gurultuKat = gurultuKat;
  disari.uzaklastir = uzaklastir;
  disari.kirp = kirp;
  disari.varyantlar = varyantlar;
})(typeof module !== 'undefined' && module.exports ? module.exports : (window.SesBozma = {}),
   typeof module !== 'undefined' && module.exports ? require('./ses-olcu.js') : window.SesOlcu);
