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
/* Hangi musteri acik. Musterinin id'si yok (kendi tablosu yok, A-M1);
   tasinan sey musteriler()'in urettigi anahtar. */
var ACIK_MUSTERI = null;
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
/* Muhasebe ve "iş başına" ekranı AYNI dönemi kullanır. İki ekranda iki
   ayrı dönem tutulsaydı usta muhasebede "Eylül" seçip iş başına
   sayfasına gelince "Bugün" bulurdu ve rakamların neden tutmadığını
   anlamazdı. Ekranlar arası devredilen her değer çekirdekte durur. */
/* Varsayılan BUGÜN (Cemil, 05.09: "Basit modda bugün seçilmeli").
   Ustanın günlük sorusu "bugün ne kaldı"; ay ve yıl ikinci sorudur.
   Ekran açıldığında birinci soruyu cevaplar. */
var muhDonemSecili = 'bugun';

/* Sesli isleme BAGLAMLA gitmenin TEK yolu.
   Once her mikrofon "SESLI_NIYET = x; Ekran.ac('ses')" yaziyordu ve
   ekran ZATEN ACIKKEN Ekran.ac() erken donuyordu: baglam degisiyor,
   ornekler ve baslik ESKI kaliyordu. Tek yer olunca hata da tek. */
function sesleAc(niyet) {
  SESLI_NIYET = niyet || 'genel';
  /* ⚠️ tazele() DEGIL goster() (06.09, K-040). Ses ekrani zaten
     acikken baska bir mikrofona basmak YENI BIR ACILISTIR: baglam
     degisiyor ve az once duyulan cumle bayatliyor. tazele() `ac`
     kancasini calistirmadigi icin eski cumle ekranda kaliyordu.
     goster() gecmise yazmaz — geri dugmesi bozulmuyor. */
  if (Ekran.adi() === 'ses') Ekran.goster('ses');
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

  /* Ekran iki kanca tanımlayabilir:
       ac(el)  — ekran AÇILDIĞINDA bir kez. Açılışa ait sıfırlamalar
                 buraya yazılır (seçim temizleme, alan boşaltma).
       ciz(el) — ekran her ÇİZİLDİĞİNDE. Yalnız DOM üretir.
     `ac` isteğe bağlı; yazmayan ekran hiçbir şey kaybetmez. */
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
    /* ⚠️ AÇILIŞ, ÇİZİMDEN AYRI (06.09 — K-040).
       `ciz` hem ekran açılınca hem her tazelemede koşuyordu ve ekran
       ikisini AYIRT EDEMİYORDU. Açılışa ait bir sıfırlama `ciz`e
       yazılınca, her tazeleme kullanıcının seçimini siliyordu — bu
       hatayı 06.09'da bir turda İKİ KEZ yaptım (tür seçimi ve silme
       onayı, K-033.12).

       `ac` yalnız goster()'den çağrılır; tazele() ona hiç dokunmaz. */
    if (k && typeof k.ac === 'function') {
      try { k.ac(el); } catch (e) { console.error('ac(' + ad + '):', e); }
    }
    if (k && typeof k.ciz === 'function') {
      try { k.ciz(el); } catch (e) { console.error('ciz(' + ad + '):', e); }
    }
    el.scrollTop = 0;
    window.scrollTo(0, 0);
    geriTazele();
    yerTazele();
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
    /* Başlık çizimle değişebilir (kayıt ekranında iş türü) — alt
       şeritteki yazı da onunla birlikte tazelenir. */
    yerTazele();
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

  /* ⚠️ EKRAN İÇİ ADIM DA GERİDİR (08.09 — Cemil: "Müşteriye mesaj
     gönder ekranında bir müşteri seçilip geri gelince ana menüye
     atıyor.").

     Sorun: bazı ekranların İÇİNDE adım var (mesajda "müşteri seç" →
     "mesajı yaz"). Bu adımlar `tazele()` ile değişiyordu, yani geçmişe
     hiç yazılmıyordu — geri tuşu ekranın TAMAMINDAN çıkıyordu ve usta
     bir adım geri gitmek isterken ana menüde buluyordu kendini.

     ÇÖZÜM ÇEKİRDEKTE, EKRANDA DEĞİL: ekran iki kanca tanımlayabilir —
     `geriVar()` (içeride geri alınacak adım var mı) ve `geriAl()`
     (adımı geri al). Geri tuşu ÖNCE ekrana sorar. Her ekranın kendi
     geri düğmesini çizmesi de çözerdi, ama o zaman "geri" iki ayrı şey
     olurdu: alttaki tuş bir yere, ekrandaki düğme başka yere. Tek tuş,
     tek anlam (K-042'nin kuralı).

     Kanca yazmayan ekran hiçbir şey kaybetmez. */
  function ekranGeriVar() {
    var k = acik ? kayit[acik] : null;
    if (!k || typeof k.geriVar !== 'function') return false;
    try { return !!k.geriVar(); }
    catch (e) { console.error('geriVar(' + acik + '):', e); return false; }
  }

  function geri() {
    if (ekranGeriVar()) {
      try { kayit[acik].geriAl(); }
      catch (e) { console.error('geriAl(' + acik + '):', e); }
      tazele();
      geriTazele();
      return;
    }
    if (!gecmis.length) return;
    if (acik) ileriYigin.push(acik);
    goster(gecmis.pop());
  }

  function ileri() {
    if (!ileriYigin.length) return;
    if (acik) gecmis.push(acik);
    goster(ileriYigin.pop());
  }

  /* ⚠️ TEK DURUM, KAÇ GÖVDE OLURSA OLSUN (07.09). Geri/ileri tuşları
     artık hem üstte hem altta; `data-nav` ile işaretli HEPSİ aynı anda
     soluklaşıyor. id ile arasaydık ikinci gövde sessizce hep açık
     kalırdı — ve "geri" gidilemeyecek yerde basılabilir görünürdü. */
  function geriTazele() {
    /* Ekran İÇİNDE geri alınacak bir adım varsa tuş sönük olamaz —
       yoksa gidilebilecek bir yer gidilemez görünür. */
    var yok = { geri: gecmis.length === 0 && !ekranGeriVar(),
                ileri: ileriYigin.length === 0 };
    document.querySelectorAll('[data-nav]').forEach(function (b) {
      var y = b.getAttribute('data-nav');
      if (y === 'geri' || y === 'ileri') b.disabled = yok[y];
    });
  }

  /* ⚠️ ÇEKİRDEK EKRANIN ADINI BİLMİYOR (08.09). Alt şeridin solunda
     "neredeyim" yazacak; buraya bir { ad -> başlık } tablosu koysaydım
     her yeni ekran İKİ yere yazılırdı ve biri mutlaka unutulurdu —
     v1'in en pahalı hata sınıfı. Ekranın başlığı zaten DOM'da duruyor
     (.screen-head .eyebrow); çekirdek onu okuyor.

     Sayaç (.eb-say — "3 bulundu") ELENİYOR: o başlık değil, o an kaç
     şey olduğu. Alt şeritte dursaydı yazı her tuşta değişirdi ve göz
     onu okumayı bırakırdı. */
  function yerTazele() {
    var yuva = document.getElementById('botbarYer');
    if (!yuva) return;
    var bas = acik
      ? document.querySelector('#screen-' + acik + ' .screen-head .eyebrow')
      : null;
    var yazi = '';
    if (bas) {
      var kopya = bas.cloneNode(true);
      kopya.querySelectorAll('.eb-say').forEach(function (s) { s.remove(); });
      yazi = (kopya.textContent || '').replace(/\s+/g, ' ').trim();
    }
    yuva.textContent = yazi;
  }

  return { tanim: tanim, ac: ac, geri: geri, ileri: ileri,
           goster: goster, tazele: tazele,
           /* Denetimin 38. testi ekranların `ciz` kaynağını okuyor —
              kayıt defterine erişimi olmadan bakamaz. */
           tanimlar: function () { return kayit; },
           adi: function () { return acik; },
           /* Android geri tusu icin — cikilacak mi, geri mi gidilecek */
           geriVarMi: function () { return gecmis.length > 0 || ekranGeriVar(); } };
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

/* ================= WEB SPEECH API =================
   ÜCRETSİZ VE ANAHTARSIZ: tarayıcının kendi motoru. Chrome ve Edge'de
   var, Firefox'ta yok. Sesi Google'ın sunucusuna gönderir (K-015).
   Güvenli bağlam ister — file:// ile açılan dosyada çalışmaz. */
function sesMotorSinifi() {
  return window.SpeechRecognition || window.webkitSpeechRecognition || null;
}
function sesMotorVar() {
  return !!sesMotorSinifi() && (window.isSecureContext !== false);
}

function sesMotorNotu() {
  if (!sesMotorSinifi()) {
    return 'Bu tarayıcıda ses tanıma yok (Chrome ya da Edge gerekiyor). ' +
           'Örneklerden birine dokunursan söylenmiş gibi işler.';
  }
  if (window.isSecureContext === false) {
    return 'Mikrofon yalnız güvenli adreste (https) açılır; dosyadan açılan ' +
           'sayfada çalışmaz. Örneklerden birine dokunursan söylenmiş gibi işler.';
  }
  return 'Mikrofona dokun ve söyle. Tarayıcının kendi tanıyıcısı kullanılıyor — ' +
         'ek kurulum yok.';
}

/* ================= DİKTE =================
   ⚠️ 08.09 — Cemil: *"Mesaj ekranında 'aracınız hazır teslim
   alabilirsiniz' dedim, mesaj bölümü için yanlış anlamadıysam dedi.
   Aslında burada text yazması gerek."*

   Haklı ve hata benimdi: mesaj ekranındaki mikrofonun aria-label'ı
   *"Sesle yazdır"* diyordu ama düğme `sesleAc('mesaj')` çağırıyordu —
   yani cümleyi KOMUT sanıp çözümlemeye gönderiyordu. Usta mesajın
   METNİNİ söylüyor; çözümleyecek bir komut yok, YAZILACAK bir cümle
   var.

   İKİ AYRI İŞ, İKİ AYRI YOL:
     sesleAc(niyet)  → KOMUT. "34 ABC 123 sahibine mesaj gönder".
                       Ses ekranı açılır, anlar, ONAY ister.
     Dikte           → METİN. Söylenen ne ise alana yazılır. Anlam
                       aranmaz, onay istenmez — çünkü yorum yok.

   TEK YERDE: düğmeler `data-dikte="<alanId>"` taşıyor, dinleyici
   burada. Her ekran kendi tanıyıcısını kurmuş olsaydı "izin verilmedi"
   mesajı bir ekranda çıkar ötekinde çıkmazdı.

   YAZILANIN ÜSTÜNE YAZMAZ: alanda ne varsa TEMEL alınır, duyulan onun
   ARDINA eklenir. Şablondan gelen metni silmek, ustanın seçtiği şablonu
   sessizce çöpe atmak olurdu. */
var Dikte = (function () {
  var tanima = null, alan = null, dugme = null, temel = '';

  function isaretle(acik) {
    if (dugme) dugme.setAttribute('aria-pressed', String(!!acik));
  }

  function dur() {
    if (tanima) { try { tanima.abort(); } catch (x) {} }
    tanima = null; isaretle(false); alan = null; dugme = null;
  }

  function yaz(ek) {
    if (!alan) return;
    var bosluk = (temel && !/\s$/.test(temel)) ? ' ' : '';
    alan.value = temel + (ek ? bosluk + ek : '');
    /* Ekranlar alanı `input` ile dinliyor (kayıt açıklaması gibi);
       elle value yazmak o olayı doğurmaz. */
    try { alan.dispatchEvent(new Event('input', { bubbles: true })); } catch (x) {}
  }

  function baslat(hedefAlan, hedefDugme) {
    if (!hedefAlan) return;
    /* İkinci dokunuş DURDURUR — açık kalan bir mikrofon, ustanın
       farkında olmadan dinleyen bir mikrofondur. */
    if (tanima) { dur(); balon('Dinleme kapandı.', false, 1600); return; }

    var Sinif = sesMotorSinifi();
    if (!Sinif || !sesMotorVar()) { balon(sesMotorNotu(), false, 4200); return; }

    alan = hedefAlan; dugme = hedefDugme || null; temel = alan.value || '';
    try {
      tanima = new Sinif();
      tanima.lang = 'tr-TR';
      tanima.interimResults = true;
      tanima.maxAlternatives = 1;

      tanima.onresult = function (e) {
        var kesin = '', gecici = '';
        for (var i = 0; i < e.results.length; i++) {
          if (e.results[i].isFinal) kesin += e.results[i][0].transcript;
          else gecici += e.results[i][0].transcript;
        }
        yaz((kesin + gecici).trim());
      };
      tanima.onerror = function (e) {
        var mesaj = ({
          'not-allowed': 'Mikrofon izni verilmedi. Adres çubuğundaki kilit simgesinden izin ver.',
          'service-not-allowed': 'Tarayıcı mikrofonu engelledi. Sayfayı https ya da localhost üzerinden aç.',
          'no-speech': 'Ses duyulmadı. Mikrofona biraz daha yakın söyle.',
          'audio-capture': 'Mikrofon bulunamadı.',
          'network': 'Tanıma sunucusuna ulaşılamadı — internet bağlantısını kontrol et.',
          'aborted': ''
        })[e.error];
        if (mesaj === undefined) mesaj = 'Mikrofon hatası: ' + (e.error || 'bilinmiyor');
        dur();
        if (mesaj) balon(mesaj, false, 4000);
      };
      tanima.onend = function () {
        var vardi = !!tanima;
        dur();
        if (vardi) balon('Yazıldı — istersen elle düzelt.', false, 2000);
      };
      isaretle(true);
      balon('Dinliyorum — söylediğin yazıya geçiyor.', true, 30000);
      tanima.start();
    } catch (x) {
      dur();
      balon('Mikrofon başlatılamadı.', false, 2600);
    }
  }

  return { baslat: baslat, dur: dur };
})();

document.addEventListener('click', function (e) {
  var t = e.target.closest('[data-dikte]');
  if (!t) return;
  Dikte.baslat(document.getElementById(t.getAttribute('data-dikte')), t);
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

/* ---- GERİ ALINABİLİR BALON (07.09) ----
   Cemil: *"Gideri sola kaydırdığında sil."* Bu, K-032'yi geri alıyor —
   orada *"kaydırma silmez, sorar"* demiştim.

   ⚠️ ONUN SÖZÜ KAZANIR ama kaydırmanın sorunu ortadan kalkmıyor:
   cepte kazara kaydırmak hâlâ mümkün. K-032'nin gerekçesi *"görünmeyen
   bir hareket görünmeyen bir sonuç doğurmasın"*dı; onay satırı bunu
   **soru sorarak** çözüyordu, geri alma **cevabı geri vererek**
   çözüyor. İkisi de aynı kuralı korur, ikincisi bir dokunuş ucuz.

   Sonuç GÖRÜNÜR olduğu sürece hareket görünmez kalabilir: satır
   kayboluyor VE ekranda ne olduğunu söyleyen, geri almayı teklif eden
   bir şerit çıkıyor. */
var geriAlFn = null;
function balonGeriAl(metin, fn, sure) {
  geriAlFn = fn || null;
  balon(metin + ' <button class="balon-eylem" data-geri-al="1">Geri al</button>',
        false, sure || 4500);
}
document.addEventListener('click', function (e) {
  if (!e.target.closest('[data-geri-al]')) return;
  var f = geriAlFn; geriAlFn = null;
  var t = document.getElementById('toast');
  if (t) t.classList.remove('show');
  if (f) f();
});

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
/* ---- KELİME VAR MI — K-018'İN ÜÇÜNCÜ TEKRARI (04.09) ----
   `` yasağı yazıldı (K-018), sonra yerine `indexOf` kondu ve AYNI
   HATA geri geldi. Ölçüldü:

     "yeni ARAC bakım"        -> "arac" içinde " ara" var  -> ARAMA sanıldı
     "BEYAZ araba geldi"      -> "beyaz" içinde "yaz" var  -> MESAJ sanıldı
     "GOSTERGE paneli yanıyor"-> "gösterge" içinde "göster"-> ARAMA sanıldı

   Üçü de ustanın en sıradan cümleleri. Kural bir kez daha keskinleşti:
   KISA KOMUT KELİMELERİ İÇİNDE ARANMAZ, BOŞLUKTAN BÖLÜNÜP EŞİTLİKLE
   KARŞILAŞTIRILIR. Uzun ve ayırt edici kökler (mesaj, whatsapp,
   hatırlatma, içeridek) için içinde aramak hâlâ doğru — onlar başka
   kelimenin içine düşmüyor. */
function kelimeVarMi(metin, kelimeler) {
  var parcalar = norm(metin).split(/\s+/);
  for (var i = 0; i < parcalar.length; i++) {
    var kelime = parcalar[i].replace(/[.,!?;:]/g, '');
    if (kelimeler.indexOf(kelime) >= 0) return true;
  }
  return false;
}

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
/* ============ ARKA PLANI KLAVYEYE KAPAT — TEK YERDE ============
   ⚠️ 07.09 — LİSTE DEĞİL YAPI. Eskiden `.topbar` ve `.screen`ler tek
   tek sayılıyordu. Alt gezinme çubuğu eklenince (A3) o liste eksik
   kaldı ve çekmece açıkken **footer'ın tuşları sekme sırasında
   kaldı** — denetimin DRAWER-ODAK-SIZINTISI testi anında yakaladı.

   Sayılan bir liste, her yeni kardeşte güncellenmeyi bekler ve bir gün
   unutulur. Kural artık yapıdan okunuyor: **verilen istisnalar dışında
   her `.device` çocuğu kapanır.**

   İki kullanıcısı var — çekmece ve katman. İkisi de aynı işi ayrı ayrı
   yazsaydı biri mutlaka eksik kalırdı; bugün tam bu oldu. */
/* ⚠️ ÖRTÜLER YIĞIN OLUŞTURUR — 07.09, ölçümle bulundu.
   İlk yazımda bu işlev "kapat" derken inert EKLİYOR, "aç" derken
   KALDIRIYORDU. İki örtü (çekmece ve katman) aynı anda varken bu
   yanlış: katman kapanınca **çekmecenin koyduğu kilidi de**
   kaldırıyordu ve arkadaki 17 öğe sekme sırasına geri dönüyordu.

   Ölçüldü: denetim iki kez koşturulunca ortaya çıkıyordu — denetim
   katmanı açık bırakıyor, sonraki koşuda çekmece açılıp kapanıyor ve
   herkesin kilidi birden kalkıyordu. Tek bir "aç/kapat" ikilisi, iki
   sahibi olan bir duruma yetmiyor.

   Çözüm SAYAÇ değil YIĞIN: hangi örtüler açık, sırasıyla tutuluyor;
   kilit her zaman **en üstteki örtüye göre** yeniden hesaplanıyor.
   Kapanan örtü kendi kaydını siliyor, altındakinin kilidi duruyor. */
var ORTU_YIGINI = [];

function ortuKilidiTazele() {
  var cihaz = document.querySelector('.device');
  if (!cihaz) return;
  var ust = ORTU_YIGINI[ORTU_YIGINI.length - 1] || null;
  Array.prototype.forEach.call(cihaz.children, function (el) {
    if (!ust || ust.haric.indexOf(el.id) >= 0) el.removeAttribute('inert');
    else el.setAttribute('inert', '');
  });
}

function arkaPlaniKapat(kapali, haric, ad) {
  ORTU_YIGINI = ORTU_YIGINI.filter(function (x) { return x.ad !== ad; });
  if (kapali) ORTU_YIGINI.push({ ad: ad, haric: haric });
  ortuKilidiTazele();
}

/* ============ KATMAN (popup) ============
   Cemil, 07.09: *"Müşteri ne görecek popup açılsın"* ve *"telefon
   yazabileceği basit bir popup"*.

   TEK BİLEŞEN, İKİ KULLANICI. İki ayrı popup yazmak iki ayrı odak
   tuzağı, iki ayrı kapatma davranışı ve iki ayrı Esc dinleyicisi
   demekti. Ekran yalnız BAŞLIK ve GÖVDE veriyor; açılma, kapanma,
   odak ve arka plan buranın işi.

   `kapaninca` geri çağrısı: katman kapanınca ekranın tazelenmesi
   gerekebilir (telefon eklendiyse düğmeler açılmalı). */
var Katman = (function () {
  var acikMi = false;
  var kapaninca = null;
  var oncekiOdak = null;

  function oge(id) { return document.getElementById(id); }

  function ac(baslik, govdeHtml, sonra) {
    var k = oge('katman'), o = oge('katmanOrtu');
    if (!k || acikMi) return;
    acikMi = true;
    kapaninca = sonra || null;
    oncekiOdak = document.activeElement;
    oge('katmanBaslik').textContent = baslik || '';
    oge('katmanGovde').innerHTML = govdeHtml || '';
    k.hidden = false; o.hidden = false;
    k.removeAttribute('inert');
    arkaPlaniKapat(true, ['katman', 'katmanOrtu'], 'katman');
    var ilk = k.querySelector('input, button, [tabindex]');
    if (ilk) { try { ilk.focus(); } catch (e) {} }
  }

  function kapa() {
    var k = oge('katman'), o = oge('katmanOrtu');
    if (!k || !acikMi) return;
    acikMi = false;
    k.hidden = true; o.hidden = true;
    k.setAttribute('inert', '');
    /* ⚠️ GÖVDE BOŞALTILIR. Gizlenen ama duran içerik `textContent` ile
       okunuyor ve "temizlenmemiş" sanılıyor (K-040.5). */
    oge('katmanGovde').innerHTML = '';
    arkaPlaniKapat(false, ['katman', 'katmanOrtu'], 'katman');
    if (oncekiOdak && oncekiOdak.focus) { try { oncekiOdak.focus(); } catch (e) {} }
    oncekiOdak = null;
    var f = kapaninca; kapaninca = null;
    if (f) f();
  }

  document.addEventListener('click', function (e) {
    if (e.target.closest('#katmanKapat') || e.target.closest('#katmanOrtu')) kapa();
  });
  document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape' && acikMi) kapa();
  });

  return { ac: ac, kapa: kapa, acikMi: function () { return acikMi; } };
})();

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
  function arkaPlan(kapali) { arkaPlaniKapat(kapali, ['drawer', 'drawerOverlay'], 'cekmece'); }

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

/* ============ GİRİŞ TEMİZLEYİCİ — TEK DELEGASYON ============
   Cemil, 05.09: *"Tutar alanına harf giremeyiz. Inputların veri
   doğruluğundan emin ol."*

   ⚠️ `inputmode="numeric"` HİÇBİR ŞEYİ ENGELLEMEZ. Yalnız telefonun
   hangi klavyeyi açacağını SÖYLER — bir öneridir, bir kısıt değil.
   Masaüstünde, yapıştırmada ve sesle girişte harf rahatça giriyor.
   Ölçüldü: üç tutar alanına da "abc12x3,4.5 TL" yazılabiliyordu.

   NEDEN SESSİZ VE PAHALI: okuma tarafı zaten temizdi
   (`replace(/[^0-9]/g,'')`), yani VERİ bozulmuyordu — ama ustaya
   gösterilen şey bozuluyordu. "abc12x3" yazan usta ekranda onu
   görüyor, kayda **12.345 ₺** düşüyordu. Yanlış rakam göstermek,
   rakam göstermemekten kötüdür (K-013'ün aynı kuralı).

   TEK YERDE: her alan kendi temizleyicisini yazsaydı biri unutulurdu
   — nitekim üçünde de yoktu. `data-giris` özniteliği bir SÖZLEŞME:
   alanı işaretlemek yeter, davranış buradan gelir. */

/* Plaka biçimi — yazarken de okunurken de AYNI biçimleyici (v1 kuralı).
   ⚠️ Ruhsat ekranının IIFE'sindeydi ve oradan çıkamıyordu; plaka üç
   ekranda görünen ORTAK NESNE, biçimi de ortak olmalı. */
function plakaBicim(ham) {
  var t = (ham || '').toLocaleUpperCase('tr').replace(/[^0-9A-ZÇĞİÖŞÜ]/g, '');
  var il = t.slice(0, 2).replace(/[^0-9]/g, '');
  var kalan = t.slice(il.length);
  var harf = kalan.replace(/[^A-ZÇĞİÖŞÜ]/g, '').slice(0, 3);
  var rakam = kalan.slice(harf.length).replace(/[^0-9]/g, '').slice(0, 4);
  return [il, harf, rakam].filter(Boolean).join(' ');
}
function plakaGecerli(p) { return /^\d{2} [A-ZÇĞİÖŞÜ]{1,3} \d{2,4}$/.test(p); }

/* Rakamları binlik nokta ile ayırır: 12345 -> 12.345 (Türkçe biçim). */
function binlikAyir(rakamlar) {
  return rakamlar.replace(/\B(?=(\d{3})+(?!\d))/g, '.');
}

var GIRIS_TEMIZ = {
  /* TUTAR — tam sayı. Kuruş YOK: tamirci "1900" der, "1900,50" demez.
     Sıfır GEÇERLİ (müşteri parçası 0 TL durur — S2, K-005). */
  tutar: function (h) {
    var r = String(h || '').replace(/[^0-9]/g, '').replace(/^0+(?=\d)/, '');
    return r ? binlikAyir(r.slice(0, 9)) : '';
  },
  /* KİLOMETRE — tam sayı, binlik ayraçlı. 7 hane yeter (9.999.999 km). */
  km: function (h) {
    var r = String(h || '').replace(/[^0-9]/g, '').replace(/^0+(?=\d)/, '');
    return r ? binlikAyir(r.slice(0, 7)) : '';
  },
  /* YIL — dört hane. Aralık BURADA denetlenmez: 1899 yazan ustaya
     yazarken kızmak yerine, alan dolunca söylemek doğru (yilGecerli). */
  yil: function (h) {
    return String(h || '').replace(/[^0-9]/g, '').slice(0, 4);
  },
  /* TELEFON — yalnız rakam, "0532 215 30 00" biçiminde gruplanır.
     11 haneden fazlası atılır. Ülke kodu (+90) KABUL EDİLMİYOR:
     tek biçim tutmak, iki biçmi karşılaştırmaktan kolay. */
  tel: function (h) {
    var r = String(h || '').replace(/[^0-9]/g, '').slice(0, 11);
    if (!r) return '';
    var p = [r.slice(0, 4), r.slice(4, 7), r.slice(7, 9), r.slice(9, 11)];
    return p.filter(Boolean).join(' ');
  },
  plaka: function (h) { return plakaBicim(h); },
  /* KOD — şasi no ve motor no. Ruhsatta BÜYÜK harf ve rakamla yazılır;
     boşluk, tire, noktalama yok. Küçük harf yazan ustayı düzeltmek
     yerine sessizce büyütüyoruz: aynı aracın iki farklı yazımı iki
     farklı araç gibi görünmesin.
     ⚠️ 17 hane SINIRI YOK: VIN 17 hanedir ama motor no değildir ve
     ikisi aynı temizleyiciyi kullanıyor. Sınırı olmayan bir alan,
     yanlış sınırı olan alandan iyidir. */
  kod: function (h) {
    return String(h || '').toLocaleUpperCase('tr')
      .replace(/[^0-9A-ZÇĞİÖŞÜ]/g, '').slice(0, 24);
  },
  /* TARİH — gg.aa.yyyy. Nokta KENDİLİĞİNDEN düşüyor: usta "14062018"
     yazınca "14.06.2018" oluyor. Geçerlilik BURADA denetlenmiyor
     (yazarken "32" yazan biri henüz 3'ten sonra 2'ye basmıştır);
     alan dolunca bakılır. */
  tarih: function (h) {
    var r = String(h || '').replace(/[^0-9]/g, '').slice(0, 8);
    var p = [r.slice(0, 2), r.slice(2, 4), r.slice(4, 8)];
    return p.filter(Boolean).join('.');
  }
};

/* Alan geçerli mi — DOLDURULDUKTAN sonra sorulur, yazarken değil. */
function yilGecerli(y) {
  var n = parseInt(String(y).replace(/[^0-9]/g, ''), 10);
  return !y || (n >= 1950 && n <= BUGUN.getFullYear() + 1);
}
function telGecerli(t) {
  var r = String(t || '').replace(/[^0-9]/g, '');
  return !r || (r.length === 11 && r.charAt(0) === '0');
}

/* İMLEÇ KORUNUR. Değeri yeniden yazmak imleci sona atar; usta ortadan
   düzeltmek isteyince her tuşta sona fırlıyordu. Solunda kaç RAKAM
   varsa o kadar rakamdan sonraya konuluyor — binlik noktası eklense
   bile aynı yerde kalıyor. */
function imlecKonumu(sonra, solRakamSayisi) {
  var i = 0, sayac = 0;
  while (i < sonra.length && sayac < solRakamSayisi) {
    if (sonra.charAt(i) >= '0' && sonra.charAt(i) <= '9') sayac++;
    i++;
  }
  return i;
}

document.addEventListener('input', function (e) {
  var t = e.target;
  if (!t || !t.getAttribute) return;
  var tip = t.getAttribute('data-giris');
  if (!tip || !GIRIS_TEMIZ[tip]) return;
  var once = t.value;
  var sonra = GIRIS_TEMIZ[tip](once);
  if (sonra === once) return;
  var konum = 0;
  try { konum = t.selectionStart || 0; } catch (x) {}
  var solRakam = (once.slice(0, konum).match(/[0-9]/g) || []).length;
  t.value = sonra;
  try {
    var y = imlecKonumu(sonra, solRakam);
    t.setSelectionRange(y, y);
  } catch (x) {}
}, true);   /* YAKALAMA evresinde: ekranların kendi input dinleyicileri
               TEMİZLENMİŞ değeri görsün, ham değeri değil. */

/* ============ DÖNEM DÜĞMELERİ — TEK DELEGASYON ============
   Dönem çipleri İKİ ekranda duruyor: muhasebe ve iş başına.

   ⚠️ 05.09'da bu dinleyici MUHASEBE EKRANINDA yazılmıştı ve başında
   `if (Ekran.adi() !== 'muhasebe') return;` vardı. Sonuç: iş başına
   sayfasındaki dönem düğmeleri ÖLÜYDÜ — basıyorsun, çip seçili
   görünmüyor, tablo değişmiyor, hata da yok.

   Bu, K-020.2'nin AYNI hatası: iki ekranın kullandığı bir denetimin
   davranışı tek bir ekranın dosyasında duruyordu. Ortak nesnenin
   davranışı da ortaktır — çekirdekte. */
document.addEventListener('click', function (e) {
  var t = e.target.closest('[data-donem]');
  if (!t) return;
  muhDonemSecili = t.getAttribute('data-donem');
  Ekran.tazele();
});

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

/* ============ SOLA KAYDIRMA — TEK YERDE (06.09) ============
   ⚠️ KAYDIRMA TEK BAŞINA SİLMEZ, onay satırını açar (K-032). Görünmeyen
   bir hareket görünmeyen bir sonuç doğurmamalı.

   NEDEN ÇEKİRDEĞE TAŞINDI: aynı hareket artık İKİ tabloda var —
   muhasebedeki giderler ve servis kaydındaki yapılanlar listesi. Kod
   muhasebe ekranının IIFE'sinin içindeydi ve oradan çıkamıyordu;
   kopyalansaydı iki ayrı eşik, iki ayrı yön hatası doğardı. Nitekim
   yön hatasını (`kayma >= 40` — kayma negatif) bir kez zaten yapmıştım
   ve hiçbir test yakalamamıştı.

   ORTAK NESNE KURALI hareketlere de işliyor: bir nesne bir yerde
   çizilir, bir hareket bir yerde tanımlanır.

   Kullanımı: satıra `class="kaydirilir" data-kaydir="<anahtar>"` yaz,
   ekranın betiğinde `Kaydirma.tanim('<ekran>', function (anahtar) {…})`
   de. Pointer olayları kullanılıyor — dokunmatikte de farede de aynı
   kod. Dikey kaydırma serbest; yatay hareket 40 px'i geçerse açılır. */
var Kaydirma = (function () {
  var isleyiciler = {};
  var basX = 0, basY = 0, satir = null, yatay = false;

  function tanim(ekran, fn) { isleyiciler[ekran] = fn; }

  function tetikle(tr) {
    var fn = isleyiciler[Ekran.adi()];
    if (fn) fn(tr.getAttribute('data-kaydir'));
  }

  document.addEventListener('pointerdown', function (e) {
    if (!isleyiciler[Ekran.adi()]) return;
    var tr = e.target.closest('.tablo tr.kaydirilir');
    if (!tr) return;
    satir = tr; basX = e.clientX; basY = e.clientY; yatay = false;
  });

  document.addEventListener('pointermove', function (e) {
    if (!satir) return;
    var dx = e.clientX - basX, dy = e.clientY - basY;
    if (!yatay && Math.abs(dy) > Math.abs(dx)) { satir = null; return; }
    if (dx > 0) return;                       /* yalnız SOLA */
    yatay = true;
    satir.classList.add('kayiyor');
    var kayma = Math.max(dx, -96);
    Array.prototype.forEach.call(satir.children, function (td) {
      td.style.transform = 'translateX(' + kayma + 'px)';
    });
  });

  function birak() {
    if (!satir) return;
    var tr = satir; satir = null;
    tr.classList.remove('kayiyor');
    var kayma = 0;
    try {
      kayma = parseFloat((tr.children[0].style.transform || '').replace(/[^-\d.]/g, '')) || 0;
    } catch (x) {}
    Array.prototype.forEach.call(tr.children, function (td) { td.style.transform = ''; });
    /* kayma NEGATİF (sola gidiyor). İlk yazımda `kayma >= 40` yazdım ve
       onay hiç açılmadı: kaydırma çalışıyor, sonucu gelmiyordu. Hiçbir
       test yakalamadı; elle işletince çıktı. */
    if (yatay && -kayma >= 40) tetikle(tr);
  }
  document.addEventListener('pointerup', birak);
  document.addEventListener('pointercancel', birak);

  /* KLAVYEYLE DE: kaydırma parmakla yapılır, klavyeyle yapılamaz.
     Satır odaklanabilir ve Delete onay satırını açar. */
  document.addEventListener('keydown', function (e) {
    if (!isleyiciler[Ekran.adi()]) return;
    if (e.key !== 'Delete' && e.key !== 'Backspace') return;
    var tr = e.target.closest && e.target.closest('.tablo tr.kaydirilir');
    if (!tr) return;
    e.preventDefault();
    tetikle(tr);
  });

  return { tanim: tanim };
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

  /* Gezinme tuşları DELEGASYONLA — üstteki ve alttaki gövdeler aynı
     dinleyiciden besleniyor (ortak nesne kuralı). */
  document.addEventListener('click', function (e) {
    var t = e.target.closest('[data-nav]');
    if (!t) return;
    var y = t.getAttribute('data-nav');
    if (y === 'geri') Ekran.geri();
    else if (y === 'ileri') Ekran.ileri();
    else if (y === 'ana') Ekran.ac('home');
  });
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
