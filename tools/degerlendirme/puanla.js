/* ============ PUANLAMA — TEK YERDE ============
   İki koşucu var (tarayıcıdaki desen eşleştirici · Node'daki model) ve
   ikisi de AYNI cetvelle ölçülmeli. İki puanlayıcı yazsaydım karne
   karşılaştırılamazdı — bu deponun en sık tekrarlayan hatası, ortak
   nesne kuralının değerlendirme tarafındaki hâli (K-019.1).

   Hem tarayıcıda (fetch + eval) hem Node'da (require) çalışır. */
(function (kok) {

  /* Beklenen alan ile gelen alan. Kurallar:
       null beklentisi  -> "bu alan DOLDURULMAMALI" (undefined de kabul)
       sayı beklentisi  -> sayı olarak karşılaştırılır
       metin beklentisi -> Türkçe küçük harfe indirilip karşılaştırılır
     ⚠️ `toLocaleLowerCase('tr')`: düz `toLowerCase()` "I"yı "i" yapıyor,
     "ı" değil — plaka ve ad karşılaştırmasını sessizce bozar. */
  function alanKarsilastir(beklenen, gelen) {
    var yanlis = [];
    Object.keys(beklenen).forEach(function (alan) {
      if (alan === 'tur') return;
      var b = beklenen[alan];
      var g = gelen ? gelen[alan] : undefined;
      if (b === null) {
        if (g !== null && g !== undefined) yanlis.push(alan + '=' + g);
        return;
      }
      if (typeof b === 'number') {
        if (Number(g) !== b) yanlis.push(alan + '=' + (g === undefined ? '(yok)' : g));
        return;
      }
      if (String(g === undefined || g === null ? '' : g).toLocaleLowerCase('tr') !==
          String(b).toLocaleLowerCase('tr')) {
        yanlis.push(alan + '="' + (g === undefined ? '(yok)' : g) + '"');
      }
    });
    return yanlis;
  }

  /* cevaplar: { <cumle id>: islemNesnesi | {hata: '...'} } */
  function puanla(set, cevaplar) {
    var sonuc = { tam: 0, yarim: 0, yanlis: 0, satirlar: [] };

    set.cumleler.forEach(function (c) {
      var gelen = cevaplar[c.id];
      var hata = gelen && gelen.hata ? gelen.hata : null;
      var turDogru = !hata && gelen && gelen.tur === c.beklenen.tur;
      var alanYanlis = turDogru ? alanKarsilastir(c.beklenen, gelen) : [];
      /* TÜR YANLIŞSA KISMİ PUAN YOK: yanlış türe giden bir cümle, ustayı
         yanlış ekrana götürür — alanlarının doğru olması işe yaramaz. */
      var puan = hata ? 'HATA' : (!turDogru ? 'YANLIS' : (alanYanlis.length ? 'YARIM' : 'TAM'));

      if (puan === 'TAM') sonuc.tam++;
      else if (puan === 'YARIM') sonuc.yarim++;
      else sonuc.yanlis++;

      sonuc.satirlar.push({
        id: c.id, puan: puan, cumle: c.cumle,
        beklenen: c.beklenen.tur,
        gelen: hata ? ('hata: ' + hata) : (gelen ? gelen.tur : '(bos)'),
        alanlar: alanYanlis.join(' · '),
        not: c.not
      });
    });

    var toplam = set.cumleler.length || 1;
    /* YARIM gerçekten yarım: türü bulup alanı kaçıran motor, hiç
       anlamayandan iyidir ama doğru olandan kötüdür. */
    sonuc.puan = Math.round(((sonuc.tam + sonuc.yarim * 0.5) / toplam) * 100);
    sonuc.ozet = sonuc.tam + ' tam · ' + sonuc.yarim + ' yarım · ' +
                 sonuc.yanlis + ' yanlış · %' + sonuc.puan;
    return sonuc;
  }

  kok.DEGERLENDIRME_PUANLA = puanla;
  if (typeof module !== 'undefined' && module.exports) {
    module.exports = { puanla: puanla, alanKarsilastir: alanKarsilastir };
  }
})(typeof globalThis !== 'undefined' ? globalThis : this);
