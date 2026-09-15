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

  /* ============ ALAN PUANLAMASI — FOTOĞRAF OKUMA (15.09) ============
     Sesli komutta ölçülen şey TEK bir karardı (tur). Fotoğrafta ölçülen
     şey ON DÖRT ALAN ve her biri ayrı ayrı yanlış olabilir. Bu yüzden
     ikinci bir cetvel gerekti — ama AYNI DOSYADA, çünkü ikisi de aynı
     soruyu soruyor: *"model ne kadarını doğru yaptı?"*

     ÜÇ KURAL, ÜÇÜ DE GERÇEK BİR ZARARDAN GELİYOR:

     1. KRİTİK ALAN yanlışsa okuma BAŞARISIZ. Plaka yanlışsa yanlış
        araca kayıt açılır; şase yanlışsa parçacı yanlış parça gönderir.
        Öteki on alanın doğru olması bunu telafi etmez.

     2. UYDURMA, BOŞ BIRAKMAKTAN AĞIR. Okunamayan alanı boş bırakan
        model ustaya *"şunu sen yaz"* der; uyduran model ustaya YANLIŞ
        BİLGİYİ DOĞRUYMUŞ GİBİ verir ve usta bunu bir daha kontrol
        etmez. Bu yüzden `beklenen[alan] === null` iken dolu cevap
        **iki kat** ceza alıyor.

     3. YASAK ALAN dönerse okuma BAŞARISIZ — değeri doğru olsa bile.
        TC kimlik no ve adres bize gerekmiyor (RUHSAT-OKUMA Bulgu 2);
        tutmadığın veri en güvenli veridir. Bu, doğruluk değil
        DAVRANIŞ sınavı: model istenmeyeni getirmemeyi de bilmeli. */
  function alanPuanla(set, cevaplar, secenek) {
    var s = secenek || {};
    var kritik = (set.puanlama && set.puanlama.kritik) || s.kritik || [];
    var yasak = (set.gizlilik && set.gizlilik.yasakAlanlar) || s.yasak || [];
    var kayitlar = set.belgeler || set.listeler || [];

    var sonuc = { belge: 0, belgeTam: 0, alanToplam: 0, alanDogru: 0,
                  uydurma: 0, yasakIhlali: 0, satirlar: [] };

    kayitlar.forEach(function (k) {
      var gelen = cevaplar[k.id] || {};
      var beklenen = k.beklenen || {};
      var hata = gelen.hata || null;

      /* Yasak alan denetimi ÖNCE: ihlal varsa okuma zaten başarısız. */
      var ihlal = yasak.filter(function (a) {
        return gelen[a] !== undefined && gelen[a] !== null && gelen[a] !== '';
      });

      var yanlisAlanlar = [], uydurmaAlanlar = [], kritikYanlis = [];
      var alanSayisi = 0, dogruSayisi = 0;

      Object.keys(beklenen).forEach(function (alan) {
        alanSayisi++;
        var b = beklenen[alan];
        var g = gelen[alan];
        var bosG = (g === undefined || g === null || String(g).trim() === '');
        var bosB = (b === undefined || b === null || String(b).trim() === '');

        if (bosB) {
          /* Beklenen BOŞ: doğru cevap boş bırakmaktır. */
          if (bosG) { dogruSayisi++; }
          else { uydurmaAlanlar.push(alan + '="' + g + '"'); }
          return;
        }
        if (bosG) { yanlisAlanlar.push(alan + '=(bos)'); }
        else if (sadeMetin(g) === sadeMetin(b)) { dogruSayisi++; }
        else { yanlisAlanlar.push(alan + '="' + g + '"'); }

        if (kritik.indexOf(alan) >= 0 && sadeMetin(g) !== sadeMetin(b)) {
          kritikYanlis.push(alan);
        }
      });

      var basarili = !hata && !ihlal.length && !kritikYanlis.length &&
                     !uydurmaAlanlar.length && !yanlisAlanlar.length;

      sonuc.belge++;
      if (basarili) sonuc.belgeTam++;
      sonuc.alanToplam += alanSayisi;
      sonuc.alanDogru += dogruSayisi;
      sonuc.uydurma += uydurmaAlanlar.length;
      sonuc.yasakIhlali += ihlal.length;

      sonuc.satirlar.push({
        id: k.id,
        sonuc: hata ? 'HATA' : (ihlal.length ? 'YASAK-ALAN'
              : (kritikYanlis.length ? 'KRITIK-YANLIS'
              : (uydurmaAlanlar.length ? 'UYDURMA'
              : (yanlisAlanlar.length ? 'EKSIK' : 'TAM')))),
        alan: dogruSayisi + '/' + alanSayisi,
        kritikYanlis: kritikYanlis.join(', '),
        uydurma: uydurmaAlanlar.join(' · '),
        yanlis: yanlisAlanlar.join(' · '),
        ihlal: ihlal.join(', '),
        zorluk: (k.zorluk || []).join(' ')
      });
    });

    sonuc.alanPuan = sonuc.alanToplam
      ? Math.round((sonuc.alanDogru / sonuc.alanToplam) * 100) : 0;
    sonuc.belgePuan = sonuc.belge
      ? Math.round((sonuc.belgeTam / sonuc.belge) * 100) : 0;
    /* ⚠️ İKİ RAKAM BİRDEN SÖYLENİYOR ve ikincisi asıl olan:
       alan başına doğruluk %95 olsa bile, BELGE başına tamlık %50 ise
       usta her iki ruhsattan birinde bir şey düzeltiyor demektir — ve
       üç düzeltmeden sonra "bu çalışmıyor" der (RUHSAT-OKUMA §6). */
    sonuc.ozet = 'belge: ' + sonuc.belgeTam + '/' + sonuc.belge +
                 ' tam (%' + sonuc.belgePuan + ') · alan: %' + sonuc.alanPuan +
                 ' · uydurma: ' + sonuc.uydurma +
                 ' · yasak alan: ' + sonuc.yasakIhlali;
    return sonuc;
  }

  /* Karşılaştırma öncesi sadeleştirme: Türkçe küçük harf, tek boşluk,
     noktalama at. "BENZİNLİ - LPG" ile "Benzinli-LPG" aynı cevaptır;
     model biçim yüzünden ceza almamalı. */
  function sadeMetin(x) {
    return String(x === undefined || x === null ? '' : x)
      .toLocaleLowerCase('tr')
      .replace(/[.,;:()/-]/g, ' ')
      .replace(/\s+/g, ' ')
      .trim();
  }

  kok.DEGERLENDIRME_PUANLA = puanla;
  kok.DEGERLENDIRME_ALAN_PUANLA = alanPuanla;
  if (typeof module !== 'undefined' && module.exports) {
    module.exports = { puanla: puanla, alanPuanla: alanPuanla,
                       alanKarsilastir: alanKarsilastir, sadeMetin: sadeMetin };
  }
})(typeof globalThis !== 'undefined' ? globalThis : this);
