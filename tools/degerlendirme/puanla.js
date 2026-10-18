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
        /* 18.10 (A-M35 koşusu) iki düzeltme:
           · alan karşılaştırması sadeAlan ile — "CİTROEN"/"CITROEN" büyük
             harf yazımıdır, okuma hatası değil; şasede O/0, I/1 aynıdır
             (ISO 3779'da O, I, Q yok).
           · BOŞ bırakılan kritik alan KRİTİK-YANLIŞ değil EKSİK: boş şase
             ustaya ruhsatı yeniden çektirir, YANLIŞ şase sessizce geçer
             (K-066). İkisini aynı saymak temkinli modeli cezalandırır. */
        var esit = sadeAlan(alan, g) === sadeAlan(alan, b);
        if (bosG) { yanlisAlanlar.push(alan + '=(bos)'); }
        else if (esit) { dogruSayisi++; }
        else { yanlisAlanlar.push(alan + '="' + g + '"'); }

        if (kritik.indexOf(alan) >= 0 && !bosG && !esit) {
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

  /* ============ PARÇA LİSTESİ CETVELİ (18.10) ============
     ⚠️ BU CETVEL YOKTU. Parça seti `beklenen` değil `satirlar` taşıyor;
     alanPuanla ona hiç alan bulamıyor ve her listeye "TAM" diyordu
     (deneme koşusu: "alan %0 · belge %100"). Ölçemediğini ölçmüş gibi
     raporlayan araç ölçüm aleti değildir (K-024.1).

     Satır satır: beklenen her satır, gelen satırlardan EN YAKIN adla
     (sadeleştirilmiş, ≥ %80 benzerlik) bire bir eşlenir. Sayılanlar:
     bulunan satır · ditto satırı (anlamı üstteki satırda) · fiyat ·
     iptal (üstü çizili) · işçilik satırı · FAZLA satır (uydurma).
     Liste TAM = her satır bulundu, fazla yok, fiyat/iptal/tip doğru. */
  function benzerlik(a, b) {
    a = sadeMetin(a); b = sadeMetin(b);
    if (a === b) return 1;
    /* AD + NOT aynı satırdır (18.10 Opus koşusu): "Ön amortisör (Sağ-Sol)"
       ile "Ön amortisör", "Buji kablo sağ-sol" ile "Buji kablo". Model
       satıra kâğıttaki notu eklemiş; satırı kaçırmamış. Kısa olan uzunun
       BAŞINDA geçmeli ve en az 4 harf olmalı — "Rot" gibi kısa bir ad her
       satıra uymasın. */
    var kisa = a.length <= b.length ? a : b, uzun = a.length <= b.length ? b : a;
    if (kisa.length >= 4 && uzun.indexOf(kisa) === 0) return 0.9;
    var m = a.length, n = b.length;
    if (!m || !n) return 0;
    var d = [], i, j;
    for (i = 0; i <= m; i++) { d[i] = [i]; }
    for (j = 1; j <= n; j++) { d[0][j] = j; }
    for (i = 1; i <= m; i++) {
      for (j = 1; j <= n; j++) {
        d[i][j] = Math.min(d[i - 1][j] + 1, d[i][j - 1] + 1,
                           d[i - 1][j - 1] + (a[i - 1] === b[j - 1] ? 0 : 1));
      }
    }
    return 1 - d[m][n] / Math.max(m, n);
  }

  function listePuanla(set, cevaplar) {
    var listeler = set.listeler || [];
    var t = { liste: 0, listeTam: 0, satir: 0, bulunan: 0, ditto: 0, dittoBulunan: 0,
              fiyat: 0, fiyatDogru: 0, iptal: 0, iptalDogru: 0, iscilik: 0, iscilikDogru: 0,
              fazla: 0, hata: 0, satirlar: [] };
    listeler.forEach(function (L) {
      var c = cevaplar[L.id] || {};
      var gelen = (c.satirlar || []).slice();
      var kullanildi = gelen.map(function () { return false; });
      var sorun = [];
      t.liste++;
      if (c.hata) { t.hata++; t.satirlar.push({ id: L.id, sonuc: 'HATA', yanlis: c.hata }); return; }
      (L.satirlar || []).forEach(function (s) {
        t.satir++;
        if (s.ditto) t.ditto++;
        /* Paydalar EŞLEŞMEDEN ÖNCE sayılır: bulunamayan satırın fiyatı
           da yanlıştır (yoksa boş cevap "fiyat 0/0" ile temiz görünür). */
        var fiyatVar = s.hasOwnProperty('fiyat');
        if (fiyatVar) t.fiyat++;
        if (s.iptal) t.iptal++;
        if (s.tip === 'iscilik') t.iscilik++;
        var en = -1, enB = 0;
        gelen.forEach(function (g, i) {
          if (kullanildi[i]) return;
          var b = benzerlik(g.ad, s.ad);
          if (b > enB) { enB = b; en = i; }
        });
        if (en < 0 || enB < 0.8) { sorun.push('YOK:' + s.ad); return; }
        kullanildi[en] = true;
        var g = gelen[en];
        t.bulunan++;
        if (s.ditto) t.dittoBulunan++;
        if (fiyatVar) {
          if ((g.fiyat == null ? null : +g.fiyat) === (s.fiyat == null ? null : +s.fiyat)) t.fiyatDogru++;
          else sorun.push('FİYAT:' + s.ad + '=' + g.fiyat + ' (doğrusu ' + s.fiyat + ')');
        }
        if (s.iptal) {
          if (g.iptal) t.iptalDogru++; else sorun.push('İPTAL-YOK:' + s.ad);
        } else if (g.iptal) sorun.push('YANLIŞ-İPTAL:' + s.ad);
        if (s.tip === 'iscilik') {
          if (g.tip === 'iscilik') t.iscilikDogru++; else sorun.push('İŞÇİLİK-DEĞİL:' + s.ad);
        }
      });
      var fazla = gelen.filter(function (g, i) { return !kullanildi[i]; });
      t.fazla += fazla.length;
      fazla.forEach(function (g) { sorun.push('FAZLA:' + g.ad); });
      if (!sorun.length) t.listeTam++;
      t.satirlar.push({ id: L.id, sonuc: sorun.length ? 'EKSIK' : 'TAM',
                        alan: (L.satirlar || []).length + ' satır',
                        yanlis: sorun.join(' · '), zorluk: (L.zorluk || []).join(' ') });
    });
    var yz = function (a, b) { return b ? Math.round(100 * a / b) : 0; };
    t.ozet = 'liste: ' + t.listeTam + '/' + t.liste + ' tam (%' + yz(t.listeTam, t.liste) + ')' +
             ' · satır bulundu %' + yz(t.bulunan, t.satir) + ' (' + t.bulunan + '/' + t.satir + ')' +
             ' · ditto %' + yz(t.dittoBulunan, t.ditto) +
             ' · fiyat %' + yz(t.fiyatDogru, t.fiyat) + ' (' + t.fiyatDogru + '/' + t.fiyat + ')' +
             ' · iptal ' + t.iptalDogru + '/' + t.iptal + ' · işçilik ' + t.iscilikDogru + '/' + t.iscilik +
             ' · FAZLA satır: ' + t.fazla + (t.hata ? ' · HATA: ' + t.hata : '');
    return t;
  }

  /* Belge alanı karşılaştırması: sadeMetin + ı/i katlama; şase ve motor
     no'da O→0, I→1, Q→0 (yalnız alanPuanla — ses cetveli etkilenmez). */
  function sadeAlan(alan, x) {
    var t = sadeMetin(x).replace(/ı/g, 'i');
    if (alan === 'sase' || alan === 'motorNo') {
      t = t.toUpperCase().replace(/[^A-Z0-9]/g, '').replace(/O/g, '0').replace(/I/g, '1').replace(/Q/g, '0');
    }
    return t;
  }

  kok.DEGERLENDIRME_PUANLA = puanla;
  kok.DEGERLENDIRME_ALAN_PUANLA = alanPuanla;
  if (typeof module !== 'undefined' && module.exports) {
    module.exports = { puanla: puanla, alanPuanla: alanPuanla, listePuanla: listePuanla,
                       alanKarsilastir: alanKarsilastir, sadeMetin: sadeMetin };
  }
})(typeof globalThis !== 'undefined' ? globalThis : this);
