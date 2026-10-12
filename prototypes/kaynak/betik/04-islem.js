/* ============ İŞLEM ŞEMALARI (Tur 8.3 · 12.10.2026) ============
   Cemil, 12.10: *"İşlemleri için bir yol / sistem oluşturmak
   istiyorduk. Kaydedilen ses bir json dolduracak. Bu ikisi eşleşmeli.
   Hangi sayfada olduğuna göre bu json'u dolduracak ve işlem yapacak."*

   NEDEN: ses ekranı her iş için ÜÇ ayrı yerde elle dal yazıyordu —
   çözümleyicide (`if … return {tur:…}`), onay ekranında (`if (o.tur
   === …) return {baslik, satirlar…}`) ve uygulamada (`if (o.tur ===
   …) {…}`). Yeni bir iş üç dal demekti; üçü ayrışabiliyordu ve Tur 7'de
   ayrıştı: kayıt dalı "geri kalan her şey" dalına dönmüştü ve kayıt
   açıkken söylenen HER cümle parça oluyordu.

   ŞİMDİ: işin sahibi ekran, işini TEK YERDE bildirir —

     Islem.tanim('gider-ekle', {
       ekran: 'muhasebe',            sahibi
       baglam: ['muhasebe'],         hangi bağlamda "kendi işi"
       oncelik: 20,                  genel yolda kaçıncı denenir
       baslik / eylem / notice,      onay ekranının sözleri
       alanlar: [ {ad, etiket, tip, …} ],   JSON'un kendisi
       tani(c)      → genel yolda tanır ve doldurur (yoksa null)
       baglamda(c)  → yalnız kendi bağlamında tanır
       varsayilan   → bağlamında hiçbir şey tutmazsa bu olur
       uygula(o)    → onaydan sonra yapılan iş
     });

   ses → işlem nesnesi (JSON) → onay ekranı ŞEMADAN çizilir → uygula
   ŞEMADAN çağrılır. Ses ekranında işe özgü tek bir dal kalmadı.

   ⚠️ İŞLEM NESNESİNİN BİÇİMİ DEĞİŞMEDİ: düz bir nesne, `tur` + alanlar.
   Değerlendirme seti (ses-seti.json) ve denetim bu biçimi okuyor; geçiş
   sırasında 568 cümle×bağlam durumu eski çözümleyiciyle BİREBİR
   karşılaştırıldı (0 fark). Şema, nesneyi değiştirmedi — TARİF etti.

   BACKEND KÖPRÜSÜ: `Islem.json()` aynı şemaları bir dil modelinin
   anlayacağı araç tanımı biçiminde verir. Model geldiğinde tanıyan
   fonksiyon (`tani`) değişir; alanlar, onay ekranı ve uygulama
   DEĞİŞMEZ. (K-054: model doldurur, usta onay ekranında düzeltir.) */

/* ---- METİNDEN OKUYUCULAR — her şema aynı okuyucuyu kullanır ----
   Ses ekranının içindeydiler; şemalar başka ekranlarda durduğu için
   buraya taşındı (ekran betikleri kendi IIFE'sinde, birbirini görmez). */

/* ⚠️ Birleştirme ÇÖZÜMLEMENİN GİRİŞİNDE bir kez yapılır, burada
   değil: indeks/uzunluk döndüren bir fonksiyon metni değiştirirse,
   çağıran taraf ORİJİNAL metni yanlış yerden keser. */
function sesPlakaBul(metin) {
  var m = metin.toLocaleUpperCase('tr')
               .match(/\b(\d{2})\s*([A-ZÇĞİÖŞÜ]{1,3})\s*(\d{2,4})\b/);
  if (!m) return null;
  return { plaka: m[1] + ' ' + m[2] + ' ' + m[3], indeks: m.index, uzunluk: m[0].length };
}

function sesTelefonBul(metin) {
  var m = metin.replace(/\s/g, '').match(/0\d{10}/);
  if (!m) return null;
  var t = m[0];
  return t.slice(0, 4) + ' ' + t.slice(4, 7) + ' ' + t.slice(7, 9) + ' ' + t.slice(9);
}

/* "2400 TL", "850 lira" — rakam bekliyoruz; kelimeyle söylenen sayı
   girişte rakama çevrildi (sesSayilariCevir).
   ⚠️ SÖYLENMEYEN TUTAR 0 DÖNER, null DEĞİL. Bugünkü bütün şemalar
   0'ı "söylenmedi" diye okuyor (onay ekranında "— (sonra yazılır)").
   K-055.2'nin `alis: null ≠ 0` ayrımı ALIŞ içindir; satış tutarında
   bu ayrım 8.2'de ele alınacak, burada davranış değiştirilmedi. */
function sesTutarBul(d) {
  var m = d.match(/(\d[\d.]*)\s*(tl|lira|₺)/);
  if (m) return parseInt(m[1].replace(/\./g, ''), 10);
  return 0;
}

/* Metinden tutarı söküp atar — açıklama/ad kalsın diye. */
function sesTutarsiz(metin) {
  return metin.replace(/(\d[\d.]*)\s*(tl|lira|₺)/gi, '');
}

/* SERVİS KAYDININ ALANLARI — plakadan sonra kalan cümleden. İki şema
   okuyor (servis-ac: kayıtlı araç · yeni-arac: ruhsat bekleyen) ve
   ikisi ayrı ekranlarda; ayrı ayrı yazılsaydı biri km'yi "88 bin"den
   okur, öteki okumazdı. */
function sesServisOku(c) {
  var dk = c.dk, kalan = c.kalan;
  var isTuru = (dk.indexOf('bakim') >= 0 || dk.indexOf('periyodik') >= 0) ? 'bakim' : 'tamirat';
  var km = '';
  var kmE = dk.match(/(\d[\d.]*)\s*(bin)?\s*(km|kilometre)/) ||
            dk.match(/(?:km|kilometre)\s*(\d[\d.]*)\s*(bin)?/);
  if (kmE) {
    km = kmE[1].replace(/\./g, '');
    if ((kmE[2] || '') === 'bin') km = String(parseInt(km, 10) * 1000);
    km = parseInt(km, 10).toLocaleString('tr-TR');
  }
  var tel = sesTelefonBul(kalan);
  /* Sahip adı: telefon ve km çıkarıldıktan sonra kalan BAŞ HARFİ BÜYÜK
     iki kelime. Kaba ama ustanın söyleyiş sırasına uyuyor:
     "plaka · ad soyad · telefon · iş". */
  var adE = kalan.match(/\b([A-ZÇĞİÖŞÜ][a-zçğıöşü]+)\s+([A-ZÇĞİÖŞÜ][a-zçğıöşü]+)\b/);
  var sahip = adE ? adE[0] : '';
  var is = kalan
    .replace(/(\d[\d.]*)\s*(bin)?\s*(km|kilometre)/gi, '')
    .replace(/(km|kilometre)\s*(\d[\d.]*)\s*(bin)?/gi, '')
    .replace(/0\s*\d[\d\s]{9,}/g, '')
    .replace(sahip, '')
    .replace(/\s+/g, ' ').trim();
  return { isTuru: isTuru, km: km, tel: tel, sahip: sahip, is: is };
}

/* İş türünün sesle söylenebilen iki hâli ("ikisi" ekranda seçilir). */
var SES_IS_TURU = { tamirat: TUR_AD.tamirat, bakim: TUR_AD.bakim };

var Islem = (function () {
  var SEMALAR = {};
  var GENEL = [];          /* tani'si olanlar, oncelik sırasında */

  /* ---- ALAN TİPLERİ ----
     Bir tip üç şeyi bilir: onay ekranında nasıl GÖRÜNÜR, ne zaman BOŞ
     sayılır, modele nasıl TARİF edilir. Tip başına bir kez yazılır;
     şemalar yalnız tipin adını söyler. */
  var TIP = {
    metin:   { json: { type: 'string' } },
    /* `aracla: true` → kayıtlı aracın marka/modeli yanına yazılır:
       usta plakadan çok aracı tanır. */
    plaka:   { json: { type: 'string', pattern: '^\\d{2} [A-ZÇĞİÖŞÜ]{1,3} \\d{2,4}$' },
               goster: function (v, a, o) {
                 return a.aracla && o.arac ? v + ' · ' + o.arac.marka + ' ' + o.arac.model : v;
               } },
    telefon: { json: { type: 'string', description: '0 ile başlayan 11 hane' } },
    km:      { json: { type: 'string', description: 'binlik noktalı: 88.000' },
               goster: function (v) { return v + ' km'; } },
    /* ⚠️ 0 = SÖYLENMEDİ (yukarıdaki sesTutarBul notu). */
    para:    { json: { type: 'integer', description: 'Türk lirası, kuruşsuz' },
               bos: function (v) { return !v; },
               goster: function (v) { return tutar(v) + ' ₺'; } },
    secim:   { json: function (a) { return { type: 'string', enum: Object.keys(a.secenek) }; },
               goster: function (v, a) { return a.secenek[v]; } },
    /* Açık kayıttaki bir satır. Model satırın ADINI söyler; hangi satır
       olduğunu sistem bulur (fiyat-guncelle'nin tek aday kuralı). */
    satir:   { json: { type: 'string', description: 'Açık kayıttaki satırın adı' },
               goster: function (v) { return v.ad || '—'; } },
    uzunMetin: { json: { type: 'string' } }
  };

  function bosMu(a, v) {
    if (v === undefined || v === null || v === '') return true;
    var t = TIP[a.tip];
    return !!(t && t.bos && t.bos(v));
  }

  function tanim(tur, s) {
    /* İki ekran aynı işi bildirirse hangisinin geçerli olduğu yükleme
       sırasına kalır — sessiz bir çakışma. Patlasın ki görülsün. */
    if (SEMALAR[tur]) throw new Error('İşlem şeması iki kez bildirildi: ' + tur);
    s.tur = tur;
    s.alanlar = s.alanlar || [];
    s.baglam = s.baglam || [];
    s.alanlar.forEach(function (a) {
      if (a.turet) return;   /* türetilen alanın tipi yok: sistem hesaplar */
      if (!TIP[a.tip]) throw new Error(tur + '.' + a.ad + ': tanımsız alan tipi "' + a.tip + '"');
    });
    SEMALAR[tur] = s;
    if (s.tani) {
      /* SIRA DAR OLANDAN GENİŞE — ve sıra AÇIKÇA yazılır. Şemalar ayrı
         ekran dosyalarında durduğu için yükleme sırası tesadüftür;
         aynı öncelik iki kez verilirse sıra yine tesadüfe kalırdı. */
      GENEL.forEach(function (g) {
        if (g.oncelik === s.oncelik) {
          throw new Error('Aynı öncelik iki şemada: ' + g.tur + ' / ' + tur + ' (' + s.oncelik + ')');
        }
      });
      GENEL.push(s);
      GENEL.sort(function (a, b) { return a.oncelik - b.oncelik; });
    }
  }

  function anlasilmadi(metin) { return { tur: 'plakasiz', metin: metin }; }

  /* Bütün şemaların okuduğu ortak bağlam — metin bir kez hazırlanır. */
  function baglamKur(metin, baglam, kayitId) {
    var p = sesPlakaBul(metin);
    var c = { metin: metin, d: norm(metin), p: p, baglam: baglam || 'genel',
              kayitId: kayitId || null, kayit: kayitId ? kayitBul(kayitId) : null };
    if (p) {
      c.kalan = (metin.slice(0, p.indeks) + metin.slice(p.indeks + p.uzunluk))
                .replace(/\s+/g, ' ').trim();
      c.dk = norm(c.kalan);
      c.arac = aracBul(p.plaka);
    }
    return c;
  }

  function kendiSemalari(baglam) {
    return Object.keys(SEMALAR).map(function (k) { return SEMALAR[k]; })
      .filter(function (s) { return s.baglam.indexOf(baglam) >= 0; })
      .sort(function (a, b) { return (a.oncelik || 0) - (b.oncelik || 0); });
  }

  function genelCozumle(c) {
    for (var i = 0; i < GENEL.length; i++) {
      var r = GENEL[i].tani(c);
      if (r) return r;
    }
    return anlasilmadi(c.metin);
  }

  /* Bir sonuç, bağlamın kendi işini ÖNERİYE düşürecek kadar güçlü mü.
     "Anlaşılmadı" değildir; bazı şemalar kendini zayıf ilan eder. */
  function gucluMu(o) {
    var s = o && SEMALAR[o.tur];
    return !!(s && s.guclu !== false);
  }

  /* ---- DOLDURMA ----
     1) Bağlamın kendi işleri, kendi sıralarıyla (dar olan önce).
     2) Genel yol — bütün şemalar, öncelik sırasıyla.
     3) Bağlamın VARSAYILAN işi: genel yol güçlü bir iş bulduysa o asıl
        olur, varsayılan ÖNERİ olarak durur ("bunu mu demek istedin" —
        7.3); bulamadıysa varsayılan asıl olur.
     Kural Tur 7'de yalnız kayıt ekranı için elle yazılmıştı; artık
     varsayılan işi olan HER bağlam için aynı yoldan geçiyor. */
  function cozumle(metin, baglam, kayitId) {
    var c = baglamKur(metin, baglam, kayitId);
    var kendi = kendiSemalari(c.baglam);
    var varsayilan = null;
    for (var i = 0; i < kendi.length; i++) {
      var s = kendi[i];
      if (s.varsayilan) { varsayilan = varsayilan || s; continue; }
      var r = s.baglamda ? s.baglamda(c) : null;
      if (r) return r;
    }
    var yedek = varsayilan && varsayilan.baglamda ? varsayilan.baglamda(c) : null;
    var genel = genelCozumle(c);
    if (!yedek) return genel;
    if (gucluMu(genel)) { genel.baskaIslem = yedek; return genel; }
    return yedek;
  }

  /* ---- ONAY EKRANININ MODELİ — şemadan üretilir ----
     Satırlar alan sırasıyla gelir. `sor: true` olan alan satır olmaz;
     onay ekranında ustaya SORULUR (çip ya da metin kutusu) ve cevabı
     işleme geri yazılır. */
  function deger(o, x) {
    return typeof x === 'function' ? x(o) : x;
  }

  function ozet(o) {
    var s = o && SEMALAR[o.tur];
    if (!s) return null;
    var satirlar = [], sorulan = [];
    s.alanlar.forEach(function (a) {
      if (a.ozette === false) return;
      if (a.sor) { sorulan.push(a); return; }
      var v = a.turet ? a.turet(o) : o[a.ad];
      var metin;
      if (a.goster) metin = a.goster(o);
      else if (a.turet) metin = v;
      else if (bosMu(a, v)) {
        if (a.bosGizle) return;
        metin = a.bos !== undefined ? a.bos : '—';
      } else {
        var t = TIP[a.tip];
        metin = t.goster ? t.goster(v, a, o) : v;
        if ((metin === undefined || metin === null || metin === '') && a.bos !== undefined) metin = a.bos;
      }
      satirlar.push([a.etiket, metin]);
    });
    return {
      baslik: deger(o, s.baslik),
      satirlar: satirlar,
      sorulan: sorulan,
      notice: deger(o, s.notice) || '',
      eylem: deger(o, s.eylem),
      eylem2: deger(o, s.eylem2) || ''
    };
  }

  function uygula(o) {
    var s = o && SEMALAR[o.tur];
    if (!s || !s.uygula) { balon('Bu iş yapılamıyor.', false, 2600); return; }
    s.uygula(o);
  }

  /* Bir iş, açıldığı bağlamın kendi işi mi — "Kayda dön" teklifi
     yalnız BAŞKA bir ekrana götüren işler için çıkar (7.4). */
  function kendiIsiMi(tur, baglam) {
    var s = SEMALAR[tur];
    return !!(s && s.baglam.indexOf(baglam) >= 0);
  }

  /* ---- MODELE TARİF — araç tanımı biçiminde ----
     Türetilen alanlar (toplam, alıcı listesi…) modelin değil sistemin
     işidir; tarife girmez. */
  function json() {
    return Object.keys(SEMALAR).sort().map(function (tur) {
      var s = SEMALAR[tur], ozellik = {}, zorunlu = [];
      s.alanlar.forEach(function (a) {
        if (a.turet) return;
        var t = TIP[a.tip].json;
        var j = typeof t === 'function' ? t(a) : JSON.parse(JSON.stringify(t));
        j.description = a.etiket + (j.description ? ' — ' + j.description : '');
        ozellik[a.ad] = j;
        if (a.zorunlu) zorunlu.push(a.ad);
      });
      return {
        name: tur,
        description: (typeof s.baslik === 'string' ? s.baslik : s.tarif || tur) +
                     ' · ekran: ' + s.ekran,
        input_schema: { type: 'object', properties: ozellik, required: zorunlu }
      };
    });
  }

  return {
    tanim: tanim, cozumle: cozumle, ozet: ozet, uygula: uygula,
    kendiIsiMi: kendiIsiMi, json: json, anlasilmadi: anlasilmadi,
    sema: function (tur) { return SEMALAR[tur] || null; },
    turler: function () { return Object.keys(SEMALAR).sort(); }
  };
})();

/* Denetim ve değerlendirme dışarıdan okuyabilsin. */
window.ISLEM_SEMALARI = function () { return Islem.json(); };
