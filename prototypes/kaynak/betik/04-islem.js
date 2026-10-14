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
/* ---- TÜRK PLAKASININ KURALLARI (Cemil, 14.10: "Türkiye'de Q harfi
   yok. Plakaların bazı kuralları var.") ----
   - İl kodu 01–81. Dışındaki "iki rakam + harf + rakam" plaka değildir
     ("kilometre 88 bin 900" → "88 BİN 900" plaka sanılmasın).
   - Harfler: Türk alfabesinin 23 harfi — Ç Ğ İ Ö Ş Ü YOK, Q W X YOK.
   - Harf sayısı rakam sayısını sınırlar (aşağıda `sinir`).
   Tanıyıcı Türkçe yazar: "kbi" büyütülünce "KBİ" olur, "ç" duyabilir.
   Plakada bu harfler olmadığına göre karşılıkları kastedilmiştir. */
var PLAKA_HARFLERI = 'ABCDEFGHIJKLMNOPRSTUVYZ';
var PLAKA_TR_HARF = { 'Ç': 'C', 'Ğ': 'G', 'İ': 'I', 'Ö': 'O', 'Ş': 'S', 'Ü': 'U' };

/* Q W X'in karşılığı yok: böyle bir plaka KESİNLİKLE yanlış duyulmuştur
   (sesPlakaBagla bunu bilir). Tarihçe: 14.09'da tanıyıcı "34 ABC 123"ü
   "34 QBC 123" duydu — A-M27'nin doğduğu kayıt. */
function plakaHarfiGecersizMi(harf) {
  return harf.split('').some(function (h) { return PLAKA_HARFLERI.indexOf(h) < 0; });
}

function sesPlakaBul(metin) {
  var m = metin.toLocaleUpperCase('tr')
               .match(/\b(\d{2})\s*([A-ZÇĞİÖŞÜ]{1,3})\s*(\d{2,5})\b/);
  if (!m) return null;
  /* ⚠️ 5 RAKAM (14.10 · büyük set): tek harfli plaka 4–5 rakamlıdır
     ("34 M 49109"); okuyucu en çok 4 rakam alıyordu ve bu plakaların
     HİÇBİRİ tanınmıyordu. Harf sayısına göre fazla rakam = geçersiz. */
  var il = parseInt(m[1], 10);
  if (il < 1 || il > 81) return null;
  var harf = m[2].replace(/[ÇĞİÖŞÜ]/g, function (h) { return PLAKA_TR_HARF[h]; });
  var rakam = m[3], uzunluk = m[0].length;
  /* RAKAM GRUBU BÖLÜNMÜŞ MÜ (14.10 ölçümü): tanıyıcı "8890"ı "88 90"
     yazıyor. Türk plakasında harf sayısı rakam sayısını sınırlar —
     1 harf → 4–5 rakam, 2 harf → 3–4, 3 harf → 2–3. Rakam grubu harf
     sayısına göre KISAYSA ve hemen ardından rakam geliyorsa ikisi bir
     plakadır. Biçime uyan grup (06 TMR 07) birleştirilmez. */
  var sinir = { 1: [4, 5], 2: [3, 4], 3: [2, 3] }[m[2].length];
  if (rakam.length < sinir[0]) {
    var ek = metin.toLocaleUpperCase('tr').slice(m.index + uzunluk).match(/^\s+(\d{1,3})(?!\d)/);
    if (ek && rakam.length + ek[1].length <= sinir[1]) {
      rakam += ek[1];
      uzunluk += ek[0].length;
    }
  }
  return { plaka: m[1] + ' ' + harf + ' ' + rakam, indeks: m.index, uzunluk: uzunluk,
           gecersiz: plakaHarfiGecersizMi(harf) || rakam.length > sinir[1] };
}

/* ---- DUYULAN PLAKAYI KAYITLI PLAKAYA BAĞLAMAK (14.10) ----
   Ölçüm (SES-OLCUM §11.3): sesli komutta kaybın en büyük kalemi plaka
   harfi — TMR "PNR/PVR/DMR", KLM "kelime" duyuluyor ve motor bunu
   KAYITSIZ araç sanıp yeni kayıt açtırıyor.

   ⚠️ DÜZ "TEK HARF FARKI" YETMEZ, YANLIŞ BİLE OLUR: "34 ARC 123" de
   kayıtlı 34 ABC 123'e tek harf uzak ama GERÇEK bir plaka olabilir —
   başka bir araç (değerlendirme setinin tuzağı → yeni-arac + öneri).
   Ayırt eden SESÇE yakınlık: Türkçede harf adları "be, ce, de, te…"
   diye okunur; tanıyıcının karıştırdığı, ünsüzü AYNI BİÇİMDE çıkan
   harflerdir (te/de/pe, me/ne). R ile B sesçe yakın değil.

   GEÇERSİZ HARFLİ PLAKA BAŞKADIR: "34 QBC 123" gerçek olamaz (Q plakada
   yok) — yeni araç değil, KESİNLİKLE yanlış duyulmuş bir plaka. Orada
   sesçe yakınlık aranmaz; tek harf uzaktaki TEK kayıtlı plaka yeter.
   (14.10'a kadar set QBC'yi "başka araç" tuzağı sayıyordu — Cemil'in
   kuralıyla çelişiyordu; tuzak geçerli harflerle yeniden kuruldu.)

   Sınıflar Türkçe ses bilgisinden (çıkış biçimi), ölçümden değil —
   TEK İSTİSNA M~V: 14.10'da "TMR" iki kez "PVR" duyuldu (dudak sesi). */
var SES_SINIFI = (function () {
  var s = {};
  [['B', 'P', 'D', 'T', 'G', 'K'],          // patlamalı
   ['C', 'Ç'],                              // yarı patlamalı
   ['F', 'V', 'S', 'Z', 'Ş', 'J', 'H'],     // sürtünmeli
   ['M', 'N', 'V'],                         // genizsi + gözlenen M~V
   ['L', 'R']].forEach(function (g, i) {
    g.forEach(function (h) { (s[h] = s[h] || []).push(i); });
  });
  return s;
})();

function sesceYakinHarf(a, b) {
  if (a === b) return true;
  var x = SES_SINIFI[a] || [], y = SES_SINIFI[b] || [];
  return x.some(function (i) { return y.indexOf(i) >= 0; });
}

/* Harf adlarının okunuşu: KLM → "KELEME". Tanıyıcı harf yerine kelime
   yazdığında ("35 kelime 45") karşılaştırma bununla yapılır. */
var HARF_OKUNUS = { A: 'A', E: 'E', I: 'I', İ: 'İ', O: 'O', Ö: 'Ö', U: 'U', Ü: 'Ü' };
function harfOkunusu(harf) {
  return harf.split('').map(function (h) { return HARF_OKUNUS[h] || (h + 'E'); }).join('');
}
function iskelet(s) { return s.replace(/[AEIİOÖUÜ]/g, ''); }

/* İki metin arası düzenleme uzaklığı (ekle/sil/değiştir). */
function duzenlemeUzakligi(a, b) {
  var o = [], i, j;
  for (j = 0; j <= b.length; j++) o[j] = j;
  for (i = 1; i <= a.length; i++) {
    var sol = i - 1; o[0] = i;
    for (j = 1; j <= b.length; j++) {
      var ust = o[j];
      o[j] = Math.min(o[j] + 1, o[j - 1] + 1, sol + (a[i - 1] === b[j - 1] ? 0 : 1));
      sol = ust;
    }
  }
  return o[b.length];
}

/* ---- ARACI TARİFİNDEN BULMAK (14.10 · büyük set) ----
   "Mehmet Kaya'nın arabası geldi", "şu Mégane'ın trigerini değiştirdik",
   "Ford Focus geldi …". 14.09'dan beri değerlendirme setinde (id 5, 6)
   beklenen davranıştı ve motorda HİÇ YOKTU — Claude çözüyordu.
   Kurallar dar:
   - yalnız genel / servis-ekle bağlamında (araçlar ekranında "Renault ara"
     bir ARAMADIR, araç seçimi değil);
   - sahip: "Ad Soyad" + iyelik + "araba/araç" ("'ın arabası");
   - model: "Marka Model" birlikte, ya da model + iyelik ("Mégane'ın");
     yalnız rakamdan oluşan model adı (301) iyelikle aranmaz — tutarla
     karışır;
   - TEK ARAÇ: sahibin iki aracı ya da aynı modelden iki araç varsa
     bağlanmaz — "uydurmak, anlamamaktan kötüdür". */
function regKacir(s) { return s.replace(/[.*+?^${}()|[\]\\]/g, '\\$&'); }

function sesAracTarif(metin, baglam) {
  if (typeof ARACLAR === 'undefined') return null;
  if (baglam && baglam !== 'genel' && baglam !== 'servis-ekle') return null;
  var d = norm(metin), bulunan = [];
  var iyelik = "'?n?[iu]n";
  ARACLAR.forEach(function (a) {
    var ad = norm(a.sahipAd || '').trim();
    if (ad.indexOf(' ') < 0) return;
    var m = d.match(new RegExp('(^|\\s)(' + regKacir(ad) + iyelik + '\\s+(?:araba|arac)\\S*)'));
    if (m) bulunan.push({ a: a, i: m.index + m[1].length, n: m[2].length, tur: 'sahip' });
  });
  if (!bulunan.length) {
    ARACLAR.forEach(function (a) {
      var mm = norm((a.marka || '') + ' ' + (a.model || '')).trim();
      var mo = norm(a.model || '').trim();
      var m = mm.indexOf(' ') > 0 ? d.match(new RegExp('(^|\\s)(' + regKacir(mm) + ")(?=\\s|$|')")) : null;
      if (!m && mo && !/^\d+$/.test(mo)) m = d.match(new RegExp('(^|\\s)(' + regKacir(mo) + iyelik + ')(?=\\s|$)'));
      if (m) bulunan.push({ a: a, i: m.index + m[1].length, n: m[2].length, tur: 'model' });
    });
  }
  if (!bulunan.length) return null;
  if (bulunan.length !== 1) return { belirsiz: true };
  var b = bulunan[0];
  return { plaka: b.a.plaka, indeks: b.i, uzunluk: b.n, tarif: b.tur };
}

/* Duyulan plakayı kayıtlı TEK bir plakaya bağlar; bağlayamazsa null.
   p: sesPlakaBul sonucu (ya da null). Döner: { plaka, duyulan, indeks,
   uzunluk } — `duyulan` onay ekranında gösterilir, usta görsün.
   ⚠️ İKİ ADAY = BAĞLAMA YOK (benzerPlaka'nın kuralı): hangisi olduğunu
   bilmiyorsak tahmin etmek ustaya yanlış aracı onaylatmaktır. */
function sesPlakaBagla(metin, p) {
  if (typeof ARACLAR === 'undefined') return null;
  if (p && aracBul(p.plaka)) return null;
  /* K-059 (Cemil, 14.10 — "karma"): GERÇEK OLABİLECEK plaka burada
     BAĞLANMAZ. "06 PVR 07" var olan başka bir araç olabilir; sesçe yakın
     kayıtlı plaka benzerPlaka'da ÖNERİ olur, usta seçer (K-047.6).
     Yalnız gerçek OLAMAYAN plaka bağlanır: Q/W/X'li ya da harf yerine
     kelime ("35 kelime 45"). Orada yeni araç ihtimali yok. */
  if (p && !p.gecersiz) return null;
  var adaylar = [], duyulan, indeks, uzunluk;
  if (p) {
    var q = plakaParcala(p.plaka);
    duyulan = p.plaka; indeks = p.indeks; uzunluk = p.uzunluk;
    ARACLAR.forEach(function (a) {
      var r = plakaParcala(a.plaka);
      if (!r || r.il !== q.il || r.rakam !== q.rakam) return;
      /* Tek harf farkı yeter (harfFarki bir eksik/fazla harfi de sayar). */
      if (harfFarki(q.harf, r.harf) <= 1) adaylar.push(a.plaka);
    });
  } else {
    /* Harf yerine kelime: "35 kelime 45". İl ve rakam BİREBİR kayıtlı
       bir plakayla aynı olmalı; kelimenin ünsüz iskeleti harflerin
       okunuşununkiyle aynı, okunuşa en çok 2 düzenleme uzak. */
    var b = metin.toLocaleUpperCase('tr').match(/(^|\s)(\d{2})\s+([A-ZÇĞİÖŞÜ]{4,9})\s+(\d{2,4})(?!\d)/);
    if (!b) return null;
    duyulan = b[2] + ' ' + b[3] + ' ' + b[4];
    indeks = b.index + b[1].length; uzunluk = b[0].length - b[1].length;
    ARACLAR.forEach(function (a) {
      var r = plakaParcala(a.plaka);
      if (!r || r.il !== b[2] || r.rakam !== b[4]) return;
      var ok = harfOkunusu(r.harf);
      if (iskelet(ok) !== iskelet(b[3])) return;
      if (duzenlemeUzakligi(ok, b[3]) > 2) return;
      adaylar.push(a.plaka);
    });
  }
  if (adaylar.length !== 1) return null;
  return { plaka: adaylar[0], duyulan: duyulan, indeks: indeks, uzunluk: uzunluk };
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

/* BİRİMSİZ TUTAR (14.10 ölçümü): tanıyıcı "TL/lira"yı sık düşürüyor —
   "2.400 elektrik faturası ekle", "motor yağı 4500". Tanıma DOĞRU, motor
   okumuyordu. ⚠️ GENEL KURAL DEĞİL: çıplak sayı plaka, km, telefon da
   olabilir. Yalnız tutardan başka bir şey olamayacak iki yerde okunur:
   gider cümlesinde (gider/fatura/kira geçiyor) ve açık kayıtta cümlenin
   SON kelimesi sayıysa. Çağıran taraf plakayı önceden ayıklar (c.dk).
   Döner: { tutar, parca } — parca metinden silinecek ham hâl. */
/* Zorlama örnekleriyle (14.10) bulunan iki tuzak:
   - "kira gideri 0532 111 22 33" → 532: boşluklu telefonun parçası.
     Telefon geçen cümlede çıplak sayı HİÇ okunmaz.
   - "bu ay 3 fatura geldi", "buji 4" → 3 / 4 TL: tek haneli sayı ve
     "tane/adet"ten önceki sayı ADETTİR. Tutar en az iki hane. */
function ciplakSonuc(m, d) {
  if (!m || sesTelefonBul(d)) return null;
  var rakam = m[2].replace(/\./g, '');
  if (rakam.length < 2 || rakam.length >= 10) return null;
  return { tutar: parseInt(rakam, 10), parca: m[2] };
}

function sesCiplakTutar(d) {
  return ciplakSonuc(d.match(/(^|\s)(\d{1,3}(?:\.\d{3})+|\d+)(?=\s|$)(?!\s+(tane|adet))/i), d);
}

function sesSonTutar(d) {
  return ciplakSonuc(d.match(/(^|\s)(\d{1,3}(?:\.\d{3})+|\d+)\s*$/), d);
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
  /* "yağ değişimi" bakımdır (14.10 · büyük set: hep tamirat çıkıyordu);
     "yağ kaçırıyor" DEĞİL — bu yüzden kök değil, ikili kelime. */
  var isTuru = (dk.indexOf('bakim') >= 0 || dk.indexOf('periyodik') >= 0 ||
                dk.indexOf('yag degis') >= 0) ? 'bakim' : 'tamirat';
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
  /* ⚠️ K-018: Türkçe metinde \b YOK — JS'te \b yalnız ASCII harfi tanır;
     "İsmail Özdemir"in baş harfi sözcük sınırı sayılmıyordu. */
  var adE = kalan.match(/(?:^|[^A-Za-zÇĞİÖŞÜçğıöşü])([A-ZÇĞİÖŞÜ][a-zçğıöşü]+\s+[A-ZÇĞİÖŞÜ][a-zçğıöşü]+)(?![A-Za-zçğıöşü])/);
  var sahip = adE ? adE[1] : '';
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
    /* Duyulan plaka kayıtlı değilse sesçe yakın TEK kayıtlı plakaya
       bağlanır; duyulan hâli işlem nesnesine `duyulanPlaka` olarak
       geçer ki onay ekranı söylesin (cozumle sonunda). */
    var bag = sesPlakaBagla(metin, p);
    if (bag) p = bag;
    /* Plaka hiç söylenmediyse araç tarifinden (sahip / model). */
    if (!p) {
      var tarif = sesAracTarif(metin, baglam);
      if (tarif && !tarif.belirsiz) p = tarif;
    }
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
    return duyulaniIsaretle(doldur(c), c);
  }

  /* Plaka kayıtlıya bağlandıysa (sesPlakaBagla) işlem DUYULANI da
     taşır — usta onay ekranında ne duyulduğunu, neyin alındığını görür. */
  function duyulaniIsaretle(r, c) {
    if (r && c.p && c.p.duyulan && r.plaka === c.p.plaka) r.duyulanPlaka = c.p.duyulan;
    return r;
  }

  function doldur(c) {
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
      notice: (o.duyulanPlaka
                ? 'Duyulan "' + o.duyulanPlaka + '" geçerli bir plaka değil — en yakın kayıtlı araç ' +
                  o.plaka + ' alındı. Değilse tekrar söyle. '
                : '') + (deger(o, s.notice) || ''),
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
