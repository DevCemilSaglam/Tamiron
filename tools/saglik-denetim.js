/* Tamiron prototip saglik denetimi — 40 test.
   Kullanim: prototypes/omurga.html'i 375px genislikte ac, konsola yapistir. */
(function(){
/* Stil metni: yorumlar ATILIR. Yorumda gecen bir sinif adi TANIM
   degildir; birakilirsa 16. test sahte bulgu verir, 5. test ise
   gercek eksigi gizler. */
function stilMetni() {
  var t = '';
  document.querySelectorAll('style').forEach(function (s) { t += s.textContent; });
  return t.replace(/\/\*[\s\S]*?\*\//g, ' ');
}

  /* ⚠️ ÖLÇÜLEMİYORSA DÜZEN TESTİ KONUŞMAZ (07.09).
     `OLCULEBILIR` bayrağı 05.09'da yazılmıştı ama YALNIZ BİR TESTE
     (YATAY-TASMA) bağlanmıştı. Sonradan eklenen düzen testleri onu
     hiç sormadı; önizleme bölmesi sıfır genişlikte açılınca denetim
     **on sahte bulgu** üretti — `olcum: ATLANDI` diye yazdığı hâlde.
     Ölçmediğini bilen ama yine de suçlayan bir araç, susan bir araçtan
     kötüdür.

     Ders K-041.3'ün aynısı: bir kural bir yerde uygulanmışsa, ötekine
     de uygulanacağı garanti değildir. Bu yüzden bayrak artık tek tek
     testlere değil, **`ek()`in kendisine** bağlı — yeni bir düzen
     testi yazan kişinin hatırlaması gerekmiyor. */
  var OLCUME_BAGLI = {
    'YATAY-TASMA': 1, 'IC-TASMA': 1, 'KIRPILAN-METIN': 1, 'KIRPILMIS-KUTU': 1,
    'GIZLI-KAYDIRMA': 1, 'SESSIZCE-KIRPILAN-ALAN': 1, 'SIKISIK': 1,
    'KUCUK-HEDEF': 1, 'ORTEN-YAPISKAN': 1, 'EZILEN-GOVDE': 1,
    'GIZLI-AMA-GORUNUR': 1, 'DRAWER-ARKA-PLAN': 1, 'KOYU-SIYAH': 1
  };
  var b=[];function ek(t,e,d){
    if (!OLCULEBILIR && OLCUME_BAGLI[t]) return;
    b.push(t+' | '+e+' | '+d);
  }

  /* ⚠️ DENETIM OLCTUGU SEYI BOZMAZ (04.09.2026).
     30. test (SAHTE-EYLEM) her dugmeye BASIYOR; 27. test ACIK_KAYIT ve
     ACIK_PLAKA atiyor. Denetim `load` aninda KENDILIGINDEN kostugu icin
     prototip her acilista KIRLI basliyordu: taze yuklenmis bir sayfada
     SESLI_NIYET 'kayit-icinde', ACIK_KAYIT 1 duruyordu ve sesli islem
     coozumleyicisi HER CUMLEYI "satir ekle" diye okuyordu.

     Yani denetim yalniz veriyi degil, SONRAKI OLCUMU de bozuyordu.
     Kendi olctugu seyi degistiren bir arac, olcum aleti degildir.

     Cozum: bastan derin kopya, sonda geri yukleme. */
  /* ⚠️ TABLO LISTESI TEK YERDE (14.09). Ayni liste iki yere yazilmisti
     (yedek alan ve geri yukleyen). Yeni bir tablo — MESAJLAR — gelince
     birine yazip otekini unutmak, denetimin o tabloyu KIRLETIP geri
     yuklememesi demekti: 30. test her dugmeye basiyor ve dugmelerden
     biri artik mesaj gonderiyor. Kurali listeye degil YAPIYA bagla. */
  var VERI_TABLOLARI = ['KAYITLAR', 'ARACLAR', 'GIDERLER', 'PARCACILAR', 'DUKKAN', 'MESAJLAR'];
  var YEDEK = (function () {
    var y = {};
    VERI_TABLOLARI.forEach(function (ad) {
      try { y[ad] = JSON.parse(JSON.stringify(window[ad])); } catch (e) { y[ad] = null; }
    });
    ['ACIK_KAYIT', 'ACIK_PLAKA', 'SESLI_NIYET', 'MESAJ_HEDEF',
     'RUHSAT_ONDOLGU', 'ARAMA_TERIMI', 'OZET_KAYIT'].forEach(function (ad) {
      y[ad] = window[ad];
    });
    return y;
  })();
  function yedegiGeriYukle() {
    Object.keys(YEDEK).forEach(function (ad) {
      if (YEDEK[ad] === null && VERI_TABLOLARI.indexOf(ad) >= 0) return;
      try { window[ad] = YEDEK[ad]; } catch (e) {}
    });
  }

  /* OLCULEBILIR MI? Onizleme bolmesi bazen SIFIR GENISLIKTE aciliyor;
     o hâlde her kutunun clientWidth'i 0 cikiyor ve tasma testleri
     ONLARCA SAHTE BULGU uretiyor. Olcemeyen bir test, olcum yapmis
     gibi rapor etmemeli — atladigini soylemeli. */
  var OLCULEBILIR = window.innerWidth >= 320;
  var ekranlar=Array.from(document.querySelectorAll('.screen')).map(function(e){return e.id.replace(/^screen-/,'');});
  ekranlar.forEach(function(a){try{showScreen(a);}catch(e){ek('ACILMIYOR',a,e.message);}});
  var fnRe=/^([A-Za-z_$][\w$]*)\s*\(/;
  document.querySelectorAll('[onclick]').forEach(function(el){
    var k=el.getAttribute('onclick')||'';
    k.split(';').forEach(function(pc){pc=pc.trim();if(!pc)return;var m=pc.match(fnRe);if(!m)return;var ad=m[1];
      if(['go','showScreen','setTimeout','if','return','window','document'].indexOf(ad)>=0)return;
      if(typeof window[ad]!=='function'){var e=el.closest('.screen');ek('OLU-FN',e?e.id.replace(/^screen-/,''):'(kabuk)',ad);}});
    (k.match(/go\('([a-z-]+)'\)/g)||[]).forEach(function(x){var t=x.match(/go\('([a-z-]+)'\)/)[1];
      if(!document.getElementById('screen-'+t)){var e=el.closest('.screen');ek('OLU-EKRAN',e?e.id.replace(/^screen-/,''):'(kabuk)',t);}});
  });
  var tan={};document.querySelectorAll('symbol[id]').forEach(function(s){tan[s.id]=1;});
  document.querySelectorAll('use').forEach(function(u){var id=(u.getAttribute('href')||'').replace('#','');
    if(id&&!tan[id]){var e=u.closest('.screen');ek('OLU-IKON',e?e.id.replace(/^screen-/,''):'(kabuk)','#'+id);}});
  var kod=document.documentElement.innerHTML;
  ekranlar.forEach(function(a){ if(a==='home')return;
    /* v2 gezinmesi IKI yoldan olur ve IKISI DE gecerli:
         data-git="ad"   — bildirimsel, cekirdekteki delegasyon yakalar
         Ekran.ac('ad')  — kod icinden (ornegin bir kayit acilirken)
       Ikincisi eklenmeseydi 'kayit' ve 'arac' ekranlari yetim
       gorunurdu; oysa ikisine de gidiliyor. */
    if(!new RegExp("(?:go|drawerGo)\\('"+a+"'\\)|showScreen\\('"+a+"'\\)|"+
                   "Ekran\\.ac\\('"+a+"'\\)|data-git=\""+a+"\"").test(kod))
      ek('YETIM',a,'gidilmiyor'); });
  /* 9) TANIMSIZ CSS SINIFI — kullaniliyor ama hicbir kuralda gecmiyor.
     .row-btns hatasi boyle olustu: dogru sinif .btn-row idi, 9 ekranda
     yanlis yazilmis ve butonlar ust uste binmisti. */
  var stil = stilMetni();
  var gorulen={};
  document.querySelectorAll('.screen [class]').forEach(function(el){
    /* SVG'de className bir SVGAnimatedString'dir, string DEGIL —
       toString() "[object SVGAnimatedString]" verir ve sahte bulgu uretir. */
    var cn = el.className;
    cn = (typeof cn === 'string') ? cn : ((cn && cn.baseVal) || '');
    cn.split(/\s+/).forEach(function(c){
      if(!c || gorulen[c]) return;
      gorulen[c]=1;
      if(stil.indexOf('.'+c)<0){
        var e=el.closest('.screen');
        ek('TANIMSIZ-SINIF', e?e.id.replace(/^screen-/,''):'(kabuk)', '.'+c);
      }
    });
  });

  var SEC='button, a[href], [role="button"], input:not([type=hidden]), textarea, .chip, .sel-trigger';
  ekranlar.forEach(function(a){showScreen(a);
    document.querySelectorAll('#screen-'+a+' '+SEC).forEach(function(el){
      var r=el.getBoundingClientRect(); if(!r.width||!r.height) return;
      if(getComputedStyle(el).visibility==='hidden') return;
      var bf=getComputedStyle(el,'::before'),hh=r.height;
      if(bf.content!=='none'&&bf.position==='absolute'){var bh=parseFloat(bf.height); if(!isNaN(bh))hh=Math.max(hh,bh);}
      var af=getComputedStyle(el,'::after');
      if(af.position==='absolute'&&(parseFloat(af.top)===0||af.inset==='0px')&&el.offsetParent) hh=Math.max(hh,el.offsetParent.getBoundingClientRect().height);
      if(hh<44){var s=(el.className||'').toString().split(' ')[0]||el.tagName;
        if(s==='tel-inline')return; if(el.tagName==='INPUT'&&el.closest('.search-box'))return;
        ek('KUCUK-HEDEF',a,s+'='+Math.round(hh));}});});
  document.documentElement.setAttribute('data-theme','dark');
  ekranlar.forEach(function(a){showScreen(a);
    document.querySelectorAll('#screen-'+a+' button, #screen-'+a+' input, #screen-'+a+' textarea, #screen-'+a+' select').forEach(function(el){
      var r=el.getBoundingClientRect(); if(!r.width||!r.height)return;
      if(getComputedStyle(el).color==='rgb(0, 0, 0)') ek('KOYU-SIYAH',a,(el.className||'').toString().split(' ')[0]||el.tagName);});});
  document.documentElement.setAttribute('data-theme','light');
  ekranlar.forEach(function(a){showScreen(a);var el=document.getElementById('screen-'+a);
    if(OLCULEBILIR && el.scrollWidth>el.clientWidth+2) ek('YATAY-TASMA',a,el.scrollWidth+'>'+el.clientWidth);});

/* 20) IC TASMA — ekran koku degil, ICERIDEKI bir kutu tasiyor.
   Onceki 19 test bunu kaciriyordu: "+ Parca / + Iscilik butonlari birbirine
   girmis" (Cemil, 01.09) = .btn-row 393 > 373. Kok temizdi.
   Hale elemesi: 44 px dokunma hedefi ::before'lari (inset -19px -8px)
   gorunmez ama scrollWidth'i buyutuyor; tasma haleden buyuk degilse
   sayilmaz. */
  function hale(el){
    var en=0;
    ['::before','::after'].forEach(function(ps){
      var c=getComputedStyle(el,ps);
      if(c.content==='none'||c.position!=='absolute')return;
      var r=parseFloat(c.right); if(r<0&&-r>en)en=-r;
    });
    return en;
  }
  ekranlar.forEach(function(a){showScreen(a);
    document.querySelectorAll('#screen-'+a+' *').forEach(function(el){
      var ov=el.scrollWidth-el.clientWidth;
      if(ov<=2)return;
      if(getComputedStyle(el).overflowX!=='visible')return;
      var h=hale(el);
      el.querySelectorAll('*').forEach(function(c){var x=hale(c); if(x>h)h=x;});
      if(ov<=h+2)return;
      if(OLCULEBILIR) ek('IC-TASMA',a,((el.className||'').toString().split(' ')[0]||el.tagName)+' '+el.scrollWidth+'>'+el.clientWidth);});});
/* 10) YINELENEN FONKSIYON — ayni ad iki kez tanimlanmis.
   Ikincisi birinciyi SESSIZCE eziyor. odemeYollariCiz boyle bulundu:
   olu kopya VAR OLMAYAN bir alana (ODEME_YOLLARI[i].durum) bakiyordu. */
(function () {
  var kod = '';
  document.querySelectorAll('script').forEach(function (s) { kod += s.textContent; });
  var re = new RegExp('(?:^|\\n)function\\s+([A-Za-z_$][\\w$]*)\\s*\\(', 'g');
  var m, say = {};
  while ((m = re.exec(kod))) { say[m[1]] = (say[m[1]] || 0) + 1; }
  Object.keys(say).forEach(function (ad) {
    if (say[ad] > 1) ek('YINELENEN-FN', '(betik)', ad + ' x' + say[ad]);
  });
})();

/* 11) YINELENEN UST DUZEY VAR — ayni ad iki kez tanimlanmis.
   Ikinci atama birinciyi eziyor ve TIP bile degisebiliyor. Bulunanlar:
   - BUGUN: once [29,8,2026] dizisi, sonra new Date(...). Dizi bekleyen
     kod BUGUN[2] okuyordu -> undefined -> fark NaN -> arac kartindaki
     "x gun kaldi/gecti" rozetleri HIC GORUNMUYORDU.
   - ODEME_YOLLARI: once odeme DURUMLARI, sonra odeme YONTEMLERI. */
(function () {
  var kod = '';
  document.querySelectorAll('script').forEach(function (s) { kod += s.textContent; });
  var re = new RegExp('(?:^|\\n)var\\s+([A-Za-z_$][\\w$]*)\\s*=', 'g');
  var m, say = {};
  while ((m = re.exec(kod))) { say[m[1]] = (say[m[1]] || 0) + 1; }
  Object.keys(say).forEach(function (ad) {
    if (say[ad] > 1) ek('YINELENEN-VAR', '(betik)', ad + ' x' + say[ad]);
  });
})();

/* 12-14) KLAVYE TURU (31.08)
   DIKKAT — secici kapsami: 'kok a[href], button, ...' yazilirsa VIRGUL
   kapsami kirar, ilk parca disindakiler TUM belgeden gelir. Her parca
   ayri ayri kapsanmali; bu testin ilk halinde 39 sahte bulgu cikti. */
(function () {
  var PARCA = ['a[href]', 'button', 'input:not([type=hidden])', 'select', 'textarea', '[tabindex]'];
  function kapsa(kok) { return PARCA.map(function (x) { return kok + ' ' + x; }).join(', '); }
  function ekranAdi(el) { var s = el.closest('.screen'); return s ? s.id.replace(/^screen-/, '') : '(kabuk)'; }

  /* 12) adsiz dugme / etiketsiz alan */
  ekranlar.forEach(function (a) {
    showScreen(a);
    document.querySelectorAll('#screen-' + a + ' button, #screen-' + a + ' a[href]').forEach(function (el) {
      if (el.offsetParent === null) return;
      var metin = (el.textContent || '').replace(/\s+/g, ' ').trim();
      if (!(metin || el.getAttribute('aria-label') || el.getAttribute('title')))
        ek('ADSIZ-DUGME', a, (el.className || '').toString().split(' ')[0] || el.tagName);
    });
    document.querySelectorAll('#screen-' + a + ' input:not([type=hidden]), #screen-' + a + ' textarea').forEach(function (el) {
      if (el.offsetParent === null) return;
      var lbl = el.id ? document.querySelector('label[for="' + el.id + '"]') : null;
      if (!lbl && !el.getAttribute('aria-label') && !el.closest('label'))
        ek('ETIKETSIZ-ALAN', a, el.id || (el.className || '').toString().split(' ')[0] || el.tagName);
    });
  });

  /* 13) pozitif tabindex — sekme sirasini bozar */
  document.querySelectorAll('[tabindex]').forEach(function (el) {
    var t = parseInt(el.getAttribute('tabindex'), 10);
    if (t > 0) ek('POZITIF-TABINDEX', ekranAdi(el), el.tagName + ' tabindex=' + t);
  });

  /* 14) cekmece acikken arkada odaklanabilir oge kalmamali */
  if (typeof openDrawer === 'function') {
    showScreen('home');
    openDrawer();
    var dr = document.getElementById('drawer');
    var dis = Array.prototype.slice.call(document.querySelectorAll(kapsa('body'))).filter(function (el) {
      return el.offsetParent !== null && !el.disabled && !dr.contains(el) && !el.closest('[inert]');
    });
    if (dis.length) ek('DRAWER-ODAK-SIZINTISI', 'drawer', dis.length + ' oge');
    if (!document.querySelector('.topbar').inert) ek('DRAWER-ARKA-PLAN', 'drawer', 'topbar inert degil');
    closeDrawer();
    if (document.querySelector('.topbar').inert) ek('DRAWER-ARKA-PLAN', 'drawer', 'kapaninca inert kalkmiyor');
  }
})();

/* 15) GORUNMEZ KARAKTER (31.08)
   Servis gecmisindeki 17 kaydin numarasi U+0001 idi: ekranda "Servis #"
   yaziyor, numara hic gorunmuyordu. Goz denetimi bunu YAKALAYAMAZ cunku
   karakter cizilmiyor. Kontrol karakterleri, sifir genislikli bosluklar
   ve kirilmayan bosluk taranir. */
(function () {
  var re = new RegExp('[\\u0000-\\u0008\\u000B\\u000C\\u000E-\\u001F\\u007F\\u00A0\\u200B-\\u200F\\uFEFF]');
  ekranlar.forEach(function (a) {
    showScreen(a);
    var el = document.getElementById('screen-' + a);
    var yur = document.createTreeWalker(el, NodeFilter.SHOW_TEXT, null);
    var d;
    while ((d = yur.nextNode())) {
      var m = d.nodeValue.match(re);
      if (!m) continue;
      var kod = 'U+' + ('000' + m[0].charCodeAt(0).toString(16).toUpperCase()).slice(-4);
      ek('GORUNMEZ-KARAKTER', a, kod + ' - "' + d.nodeValue.replace(re, '?').trim().slice(0, 40) + '"');
    }
  });
})();

/* 16) OLU CSS SINIFI (31.08)
   15. testin TERSI: o kullanilan-ama-tanimsiz sinifi arar, bu
   tanimli-ama-kullanilmayani. Ortak nesnelere gecince .kisi-arac ve
   .is-satir aileleri olu kaldi; birakilsa "iki ayri satir bicimi var"
   yanilgisi surerdi. Durum/tema varyantlari (d-*, t-*, is-*) elle
   eklenip cikarildigi icin BEYAZ LISTE ile disarida tutuluyor. */
(function () {
  var BEYAZ = /^(d-|t-|c-|tip-|is-|sel-|drawer|toast|screen|active|open|dark|light)/;
  var stil = stilMetni();
  var tanimli = {};
  /* Noktadan ONCE harf/rakam olmamali: "tokens.css" icindeki .css
     bir SINIF degil, dosya uzantisi. Ilk yazimda 23 bulgunun biri
     bu yuzden sahteydi. */
  var re = new RegExp('(^|[^A-Za-z0-9_-])\\.([A-Za-z_][\\w-]*)', 'g');
  var m;
  while ((m = re.exec(stil))) { tanimli[m[2]] = true; }

  var kullanilan = {};
  ekranlar.forEach(function (a) {
    showScreen(a);
    document.querySelectorAll('[class]').forEach(function (el) {
      var cn = el.className;
      cn = (typeof cn === 'string') ? cn : ((cn && cn.baseVal) || '');
      cn.split(/\s+/).forEach(function (c) { if (c) kullanilan[c] = true; });
    });
  });
  /* JS icinde string olarak uretilen siniflar da sayilir */
  var kod = '';
  document.querySelectorAll('script').forEach(function (s) { kod += s.textContent; });

  Object.keys(tanimli).forEach(function (c) {
    if (kullanilan[c] || BEYAZ.test(c)) return;
    if (kod.indexOf("'" + c) >= 0 || kod.indexOf('\"' + c) >= 0 || kod.indexOf(' ' + c) >= 0) return;
    ek('OLU-SINIF', '(stil)', '.' + c);
  });
})();

/* 17) YETIM VERI ANAHTARI (31.08)
   Not anahtarlari GERCEK bir kayda ait olmali:
   NOTLAR.musteri -> musteri adi, NOTLAR.arac -> kayitli plaka.
   Tohum veriye 'Ali Yildiz' diye OLMAYAN bir musteri yazilmisti:
   not hicbir ekranda gorunmuyor, arama bos donuyor, hata da yok. */
(function () {
  if (typeof NOTLAR === 'undefined') return;
  if (typeof kisileriTuret === 'function') {
    var m = {};
    kisileriTuret().forEach(function (k) { m[k.ad] = true; });
    Object.keys(NOTLAR.musteri || {}).forEach(function (ad) {
      if (!m[ad]) ek('YETIM-VERI', '(veri)', 'NOTLAR.musteri["' + ad + '"] — boyle bir musteri yok');
    });
  }
  if (typeof ARACLAR !== 'undefined') {
    var a = {};
    ARACLAR.forEach(function (x) { a[x.plaka] = true; });
    Object.keys(NOTLAR.arac || {}).forEach(function (pl) {
      if (!a[pl]) ek('YETIM-VERI', '(veri)', 'NOTLAR.arac["' + pl + '"] — boyle bir plaka yok');
    });
    /* ACIK_ISLER de ARACLAR'a bagli: olmayan plaka yazilinca ekran
       sessizce VARSAYILAN araca dusuyor. Bilgilendirmede teslim
       listesinden secilen isin musterisi yanlis geliyordu (01.09). */
    if (typeof ACIK_ISLER !== 'undefined' && ACIK_ISLER) {
      ACIK_ISLER.forEach(function (x) {
        if (!a[x.plaka]) ek('YETIM-VERI', '(veri)', 'ACIK_ISLER["' + x.plaka + '"] — boyle bir plaka yok');
      });
    }
  }
})();

/* 18) KIRPILMIS KUTU (31.08)
   Kutunun yuksekligi ILK COCUGUNDAN kucukse icerik gorunmuyordur.
   Not seridi boyle bulundu: .screen bir flex sutun, serit 2 px'e
   ezilmis, icindeki 28 px'lik kart gorunmuyordu. Gorsel denetim
   bunu 'bos alan' sanip gecer. */
(function () {
  ekranlar.forEach(function (a) {
    showScreen(a);
    document.querySelectorAll('#screen-' + a + ' > *, #screen-' + a + ' .card > *').forEach(function (el) {
      var c = el.firstElementChild;
      if (!c) return;
      var eh = el.getBoundingClientRect().height;
      var ch = c.getBoundingClientRect().height;
      if (!ch || eh >= ch - 1) return;
      if (getComputedStyle(el).overflowY === 'auto' && el.scrollHeight > el.clientHeight + 4) return;
      ek('KIRPILMIS-KUTU', a, ((el.className || '').toString().split(' ')[0] || el.tagName) +
        ' ' + Math.round(eh) + 'px < icerik ' + Math.round(ch) + 'px');
    });
  });
})();

/* 19) YUKLEME HATASI (01.09)
   Sayfa acilirken atilan hata sessiz kaliyordu: ekran cizilmiyor ama
   denetim '0 bulgu' diyordu. Bu projede ALTI kez var hoisting
   yuzunden yasandi — yuklenirken calisan fonksiyon, HENUZ ATANMAMIS
   bir var'a dokunuyor. Sayfanin basindaki toplayici okunur. */
(function () {
  if (typeof window.YUKLEME_HATALARI === 'undefined') {
    ek('TOPLAYICI-YOK', '(kabuk)', 'sayfada yukleme hatasi toplayicisi yok');
    return;
  }
  window.YUKLEME_HATALARI.forEach(function (m) {
    ek('YUKLEME-HATASI', '(kabuk)', m);
  });
})();

/* 21) OLU ID (01.09)
   JS bir id ile oge ariyor ama sayfada boyle bir oge YOK.
   Cizim fonksiyonu o satirda PATLIYOR; ekran yarim kaliyor ya da hic
   acilmiyor. 20 test bunu kaciriyordu cunku ACILMIYOR testi ekrani
   BOS durumda aciyor ve cizici erken donuyor — hata ancak gercek bir
   kayit acilinca cikiyor.
   Bulunan: fdArac — fiyat calismasi detayi acilmiyordu. */
(function () {
  /* DENETIMIN KENDI BETIGI TARANMAZ. Denetim gomulu calisiyor ve kendi
     icinde getElementById('drawer') gecen bir test var (14). Cekmece
     v2'de kaldirilinca denetim KENDINI olu referansla sucladi. Kendi
     kodunu olcen bir arac, olctugu seye karismamali. */
  var kod = '';
  document.querySelectorAll('script').forEach(function (sc) {
    if (sc.textContent.indexOf('SAGLIK DENETIMI') >= 0) return;
    kod += sc.textContent;
  });
  /* ⚠️ 06.09 — JS'IN URETTIGI ID'LER DE SAYILIR.
     `#kToplam` servis kaydinin tablosuyla birlikte JS tarafindan
     yaziliyor; ekran cizilmeden DOM'da YOK. Test onu "olu referans"
     sanip her `load` kosusunda bulgu veriyordu — ve bu bulguyu aylarca
     kimse gormedi, cunku hep ELLE kosturup o kosunun sonucunu okuduk;
     manuel kosuda 30. test kayit ekranini satirlariyla cizmis oluyor
     ve id var oluyordu.

     Ayni dersin bir kopyasi TASARIM-SISTEMI'nde zaten yaziliydi:
     "TANIMSIZ-SINIF testi .plate-lg'yi GEC yakaladi — onu JS uretiyor,
     ekran cizilmeden DOM'da olmuyor." Ders belgede vardi, bu testte
     uygulanmamisti.

     Cozum DOM'u kurcalamak degil: betigin KENDISI o id'yi yaziyorsa
     (id="kToplam") referans olu degildir. Statik, yan etkisiz.
     Bir yazim hatasi yine yakalanir — yanlis yazilan id ne DOM'da
     ne betikte bulunur. */
  var uretilenId = {};
  var uid = /id="([A-Za-z_][\w-]*)"/g, um;
  while ((um = uid.exec(kod))) uretilenId[um[1]] = 1;

  var re = /getElementById\(\s*'([A-Za-z_][\w-]*)'\s*\)/g;
  var m, gorulen = {};
  while ((m = re.exec(kod))) {
    var id = m[1];
    if (gorulen[id]) continue;
    gorulen[id] = 1;
    if (uretilenId[id]) continue;
    if (!document.getElementById(id)) ek('OLU-ID', '(betik)', "getElementById('" + id + "') — boyle bir oge yok");
  }
})();

/* 22) YUKLEME SIRASI (01.09)
   Sayfa yuklenirken calisan bir fonksiyon, DAHA ASAGIDA tanimlanan bir
   ust duzey var'a bakiyor. Bu projede YEDINCI kez yasandi.
   19. test yalniz PATLAYANINI yakalar; kodda "typeof X === 'undefined'"
   korumasi varsa hata atilmaz, fonksiyon SESSIZCE yanlis is yapar ve
   denetim "0 bulgu" der.
   Bulunan: araclariCiz() / kisileriCiz() yuklemede calisiyor ama
   ACIK_ISLER 3300 satir asagida tanimli -> "icerideki araclar ustte ve
   kenarligi farkli" DOM'a hic girmiyordu. */
(function () {
  var kod = '';
  document.querySelectorAll('script').forEach(function (sc) { kod += '\n' + sc.textContent; });

  /* Sutun 0'daki ust duzey var tanimlari: ad -> ilk gorundugu yer */
  var varYeri = {}, m;
  var varRe = /\nvar\s+([A-Za-z_$][\w$]*)\s*=/g;
  while ((m = varRe.exec(kod))) {
    if (!(m[1] in varYeri)) varYeri[m[1]] = m.index;
  }

  /* Sutun 0'daki cagrilar: fn();  — yukleme sirasinda calisirlar */
  var cagriRe = /\n([A-Za-z_$][\w$]*)\(\s*\)\s*;/g;
  while ((m = cagriRe.exec(kod))) {
    var ad = m[1], yer = m.index;
    var fn = window[ad];
    if (typeof fn !== 'function') continue;
    var govde = fn.toString();
    Object.keys(varYeri).forEach(function (v) {
      if (varYeri[v] < yer) return;                       /* zaten yukarida */
      if (!new RegExp('\\b' + v + '\\b').test(govde)) return;
      ek('YUKLEME-SIRASI', '(betik)',
         ad + '() yuklemede calisiyor ama ' + v + ' DAHA ASAGIDA tanimli');
    });
  }
})();

/* 23) TEMA UYUSMAZLIGI (01.09)
   Koyu tema IKI YERDE tanimli: [data-theme="dark"] (kullanici secerse)
   ve @media (prefers-color-scheme: dark) (sistem tercihi). CSS'te
   tek kaynaktan turetilemiyor, kopyalaniyor — biri guncellenip oteki
   unutuluyor. 01.09 renk turunda tam bu oldu: secilen koyu temada yeni
   renkler, SISTEM koyu temasinda eski renkler cikiyordu. Kullanicilarin
   cogu "sistem" ayarinda oldugu icin degisiklik onlara ulasmayacakti. */
(function () {
  var css = stilMetni();
  function blok(bas) {
    var i = css.indexOf(bas);
    if (i < 0) return null;
    var a = css.indexOf('{', i), d = 0, j = a;
    for (; j < css.length; j++) {
      if (css[j] === '{') d++;
      else if (css[j] === '}') { d--; if (!d) break; }
    }
    return css.slice(a + 1, j);
  }
  function tokenlar(metin) {
    var m, t = {}, re = /(--[\w-]+)\s*:\s*([^;}]+)/g;
    while ((m = re.exec(metin))) t[m[1]] = m[2].trim().replace(/\s+/g, ' ');
    return t;
  }
  var a = blok('[data-theme="dark"]');
  var b = blok('prefers-color-scheme: dark');
  if (!a || !b) return;
  var ta = tokenlar(a), tb = tokenlar(b);
  Object.keys(ta).forEach(function (k) {
    if (!(k in tb)) { ek('TEMA-UYUSMAZLIGI', '(stil)', k + ' sistem koyu temasinda YOK'); return; }
    if (ta[k].toLowerCase() !== tb[k].toLowerCase()) {
      ek('TEMA-UYUSMAZLIGI', '(stil)', k + ': secili="' + ta[k] + '" sistem="' + tb[k] + '"');
    }
  });
})();

/* 24) SINIF CAKISMASI (01.09)
   Ayni CSS sinif adi IKI AYRI BILESEN icin tanimlanmis. Bu projede
   uc kez oldu: .stok (yesil nokta ::before baska bir bilesenden
   miras kaldi), .rol-cip (personel rozeti ile rol secme dugmesi ayni
   adi kullandi — hem gorunum bozuldu hem querySelectorAll 4 yerine 8
   oge buldu). Sinif adi burada hem STIL hem SECICI oldugu icin
   cakisma iki yerden birden vuruyor.

   Yalniz SADE ve TEK secicilere bakilir (".ad { }"). Sayilmayanlar,
   cunku hepsi mesru: @media icindeki geri donusler (.device kucuk
   ekranda baska), grup secici (.a, .b, .c { min-height: 44px } —
   dokunma hedefi), durum/varyant (.x:hover, .x[aria-pressed], .x.y),
   baglamda daraltma (.kart .x).
   Kurucu ozellik = bileseni TANIMLAYAN alan (display/padding/flex...);
   yalniz renk veren ikinci kural varyanttir, cakisma degil. */
(function () {
  var css = stilMetni();
  /* @media ve benzeri @-bloklarini govdesiyle birlikte cikar. */
  var temiz = '', i = 0, j, d;
  while (i < css.length) {
    var at = css.indexOf('@', i);
    if (at < 0) { temiz += css.slice(i); break; }
    temiz += css.slice(i, at);
    var a = css.indexOf('{', at);
    if (a < 0) break;
    d = 0;
    for (j = a; j < css.length; j++) {
      if (css[j] === '{') d++;
      else if (css[j] === '}') { d--; if (!d) break; }
    }
    i = j + 1;
  }

  var KURUCU = /(^|;)\s*(display|position|padding|flex|grid-template|grid-template-columns)\s*:/;
  var yer = {};
  var re = /(^|[}])([^{}]*)\{([^{}]*)\}/g, m;
  while ((m = re.exec(temiz))) {
    var sec = m[2].trim(), govde = m[3];
    if (!/^\.[a-zA-Z][\w-]*$/.test(sec)) continue;
    if (!KURUCU.test(';' + govde)) continue;
    var ad = sec.slice(1);
    (yer[ad] = yer[ad] || []).push(govde.trim().replace(/\s+/g, ' ').slice(0, 40));
  }
  Object.keys(yer).forEach(function (ad) {
    if (yer[ad].length < 2) return;
    ek('SINIF-CAKISMASI', '(stil)',
       '.' + ad + ' iki kez kurucu tanimlanmis: "' + yer[ad][0] + '" / "' + yer[ad][1] + '"');
  });
})();

/* 25) EZILEN GOVDE (01.09)
   Bir esnek satirda (display:flex) YAN OGE govdeyi sifira eziyor.
   Bu projede ikinci kez: .btn tabani "width: 100%" veriyor ve bir
   flex satirina konunca dogal genisligi 100% sanilip butun yeri
   aliyor. Ilk seferinde .btn-row sarmiyordu, ikincisinde eksik parca
   satirinda metin kelime kelime alt alta dizildi (.ep-govde = 0px).

   Tasma degil — 20. test bunu yakalamiyor, cunku icerik tasmiyor,
   SIKISIYOR. Olcut: esnek kabin icinde METNI OLAN bir cocuk 8px'den
   dar kaldiysa ve kardesi kabin genisliginin %70'inden fazlasini
   aldiysa bulgu verilir. */
(function () {
  ekranlar.forEach(function (a) {
    showScreen(a);
    document.querySelectorAll('#screen-' + a + ' *').forEach(function (kap) {
      if (getComputedStyle(kap).display.indexOf('flex') < 0) return;
      var kw = kap.getBoundingClientRect().width;
      if (kw < 40) return;
      var cocuklar = Array.prototype.slice.call(kap.children);
      if (cocuklar.length < 2) return;
      var ezik = null, obur = 0;
      cocuklar.forEach(function (c) {
        /* GIZLI OGE EZILMIS DEGILDIR — display:none genisligi zaten
           sifirdir. Ilk yazimda uc sahte bulgu bundandi (gizlenmis
           menu satiri, kapali "dinliyorum" ipucu, gizli rozet). */
        if (c.offsetParent === null && getComputedStyle(c).position !== 'fixed') return;
        var w = c.getBoundingClientRect().width;
        if (w < 8 && (c.textContent || '').trim().length > 3) ezik = c;
        if (w > obur) obur = w;
      });
      if (ezik && obur > kw * 0.7) {
        if (OLCULEBILIR) ek('EZILEN-GOVDE', a,
           (kap.className || kap.tagName) + ' icinde ' +
           (ezik.className || ezik.tagName) + ' 0px kaldi (kardesi ' +
           Math.round(obur) + '/' + Math.round(kw) + 'px)');
      }
    });
  });
})();

  /* 26) YETIM VERI — kayit, olmayan bir araca bagli (03.09.2026)
     Bir bakim kaydini elle eklerken plakasini ARACLAR'da olmayan bir
     plaka yazdim. Ekran patlamiyor, yalnizca marka/model bos ciziliyor
     ve neden bos oldugu hicbir yerde yazmiyor. Sessiz bozulma. */
  if (typeof KAYITLAR !== 'undefined' && typeof ARACLAR !== 'undefined') {
    var plakalar = ARACLAR.map(function (a) { return a.plaka; });
    KAYITLAR.forEach(function (k) {
      if (plakalar.indexOf(k.plaka) === -1) {
        ek('YETIM-VERI', '(veri)', 'kayit #' + k.id + ' -> "' + k.plaka + '" ARACLAR icinde yok');
      }
    });
  }

  /* 27) OLU UC — ekran aciliyor ama oradan HICBIR SEY yapilamiyor
     (04.09.2026). Arac ekrani tam boyleydi: araci goruyordun, servis
     kaydi acmak icin ana ekrana donup ruhsati YENIDEN cekmen
     gerekiyordu. Ekran "calisiyor" gorunuyordu cunku ciziliyordu;
     calismadigi ancak kullanmaya calisinca anlasiliyordu.

     Geri dugmesi eylem SAYILMAZ: her ekranda var ve hicbir isi
     ilerletmiyor. Sayilan sey, ekranin KENDI icindeki eylemler. */
  {
    /* Ekranlarin cogu bir kayit/arac secili degilken bos ciziliyor —
       once onlari doldur, yoksa test sahte bulgu verir. */
    try {
      if (typeof KAYITLAR !== 'undefined' && KAYITLAR.length) {
        window.ACIK_KAYIT = KAYITLAR[0].id;
        window.ACIK_PLAKA = KAYITLAR[0].plaka;
      }
    } catch (e) {}
    ekranlar.forEach(function (a) {
      showScreen(a);
      var s = document.getElementById('screen-' + a);
      if (!s) return;
      var eylem = s.querySelectorAll('button, a[href], input, textarea, [data-git], [role="button"]');
      if (!eylem.length) ek('OLU-UC', a, 'ekranda hicbir eylem yok');
    });
    showScreen('home');
  }

  /* 28) ACILMAYAN BOLUM — akordiyon ciziliyor ama ACILMIYOR (04.09.2026)
     27. testin kucuk kardesi. OLU-UC "ekranda hic eylem yok" diyor;
     bu test "eylem VAR ama HICBIR SEY yapmiyor" diyor.

     Neden ayri bir hata sinifi: akordiyon KAPALI aciliyor. Kapali bir
     bolum, calismayan bir bolumden GORSEL OLARAK AYIRT EDILEMEZ —
     ikisi de ayni sekilde durur. aria-controls yanlis yazilsa, govde
     id'si degisse ya da olay dinleyicisi baglanmasa ekran yine
     "dogru" gorunur ve ruhsat bilgileri kimsenin ulasamadigi bir
     yerde kalir. Ustanin bunu fark etmesinin tek yolu dokunmak.

     NEGATIF DOGRULAMA (04.09): aria-controls degeri elle bozuldu
     (arRuhsat -> arRuhsatX) ve test 1 bulgu verdi; geri alininca
     0 bulguya dondu. Delikli denetim, denetimsiz olmaktan kotudur. */
  {
    ekranlar.forEach(function (a) {
      showScreen(a);
      var basliklar = document.querySelectorAll('#screen-' + a + ' .akd-bas');
      for (var i = 0; i < basliklar.length; i++) {
        var bas = basliklar[i];
        var hedef = bas.getAttribute('aria-controls');
        var govde = hedef ? document.getElementById(hedef) : null;
        if (!govde) { ek('ACILMAYAN-BOLUM', a, (hedef || '(aria-controls yok)') + ' — govde yok'); continue; }
        /* ⚠️ 06.09 — GORUNMEYEN BASLIK OLCULMEZ. Ruhsat ekraninda
           "Ruhsattaki diger bilgiler" akordiyonu, arac KAYITSIZ oldugu
           zaman acilan bir kartin icinde; plaka yazilmamisken o kart
           hidden ve akordiyon da onunla birlikte gorunmuyor. Test
           tiklayip "acilmiyor" diyordu — oysa olcemedigi bir seyi
           bozuk ilan ediyordu (K-024'un tekrari: denetim olctugu seyi
           bozmamali, olcemedigini de suclamamali).
           aria-expanded KONTROLU YINE YAPILIYOR: o, gorunurlukten
           bagimsiz ve gercek bir baglanti hatasini yakalar. */
        var gorunur = bas.offsetParent !== null && bas.offsetHeight > 0;
        bas.click();
        var acildi = !govde.hidden && (govde.offsetHeight > 0 || !gorunur);
        if (!acildi) { ek('ACILMAYAN-BOLUM', a, hedef + ' — dokununca acilmiyor'); }
        if (bas.getAttribute('aria-expanded') !== 'true') {
          ek('ACILMAYAN-BOLUM', a, hedef + ' — aria-expanded guncellenmiyor');
        }
        bas.click();                       /* geri kapat: ekran denetim sonrasi bozulmasin */
        if (acildi && !govde.hidden) { ek('ACILMAYAN-BOLUM', a, hedef + ' — bir daha kapanmiyor'); }
      }
    });
    showScreen('home');
  }

  /* 29) ORTEN YAPISKAN — position:sticky bir baslik, kaydirinca
     ALTINDAKI ICERIGI ORTUYOR (04.09.2026).

     Cemil'in ekran goruntusu buldu: cekmecedeki "BILDIRIMLER" basligi
     sticky idi ve cekmece kaydirilinca bildirim kartinin uzerine
     binip yalniz bir dilimini birakiyordu.

     Neden hicbir test yakalamadi: HER SEY DOGRU CIZILIYORDU. Kutu
     yerinde, metin yerinde, dokunma hedefi yerinde. Bozulan sey ancak
     KAYDIRINCA ortaya cikiyor — durgun ekranda olcen hicbir test
     goremez. O yuzden bu test once KAYDIRIYOR, sonra olcuyor.

     NEGATIF DOGRULAMA (04.09): kural gecici olarak geri kondu
     (#notifSection .drawer-sec-label { position: sticky; top: 0 })
     ve test 1 bulgu verdi; kaldirilinca 0 bulguya dondu. */
  {
    var kaplar = [];
    var cekmeceGovde = document.querySelector('.drawer-body');
    if (cekmeceGovde) { openDrawer(); kaplar.push(cekmeceGovde); }
    ekranlar.forEach(function (a) {
      showScreen(a);
      var e = document.getElementById('screen-' + a);
      if (e) kaplar.push(e);
    });

    kaplar.forEach(function (kap) {
      var eskiKaydirma = kap.scrollTop;
      var eskiYukseklik = kap.style.maxHeight;
      /* ⚠️ TASMA ZORLANIYOR — ILK YAZIMDA BU YOKTU VE TEST BOSTU.
         Negatif test bulgu VERMEDI: yapiskan basligi geri koydum, test
         0 dedi. Sebep, kabin o anki veriyle TASMAMASIYDI — kaydirma
         yoksa yapiskan oge hic ayrilmaz ve hicbir seyi ortmez.
         Yani test "bu ekran iyi" demiyordu, "bugunku veri az" diyordu.
         Sorulacak soru su: BU KAP KAYDIRILIRSA basligi icerigi orter
         mi? Kabi kisaltip kaydirtiyoruz; olcum veriden bagimsiz olsun.
         (Bildirim sayisi artinca cekmece zaten kaydirilir.) */
      kap.style.maxHeight = '200px';
      kap.scrollTop = kap.scrollHeight;
      Array.prototype.forEach.call(kap.querySelectorAll('*'), function (el) {
        if (getComputedStyle(el).position !== 'sticky') return;
        var yr = el.getBoundingClientRect();
        if (!yr.height) return;
        /* Yapiskan ogeden SONRA gelen kardesler ortulmemeli. */
        var k = el.nextElementSibling;
        while (k) {
          var kr = k.getBoundingClientRect();
          if (kr.height) {
            var ortusme = Math.min(yr.bottom, kr.bottom) - Math.max(yr.top, kr.top);
            if (ortusme > 2) {
              ek('ORTEN-YAPISKAN',
                 (kap.id || 'cekmece').replace(/^screen-/, ''),
                 ((el.className || '').toString().split(' ')[0] || el.tagName) +
                 ' ' + Math.round(ortusme) + 'px ortuyor');
              break;
            }
          }
          k = k.nextElementSibling;
        }
      });
      kap.style.maxHeight = eskiYukseklik;
      kap.scrollTop = eskiKaydirma;
    });
    if (cekmeceGovde) closeDrawer();
    showScreen('home');
  }

  /* 30) SAHTE EYLEM — dugme "gonderildi" diyor, HICBIR SEY OLMUYOR
     (04.09.2026).

     Kayit ekranindaki "Onay iste" dugmesi tam boyleydi: basiyordun,
     balon "Ozet gonderildi — 0532..." diyordu ve hicbir sey
     gonderilmiyordu. Mesaj ekrani ZATEN VARDI, oraya bile gitmiyordu.

     Neden 27. ve 28. testler kacirdi: dugme VAR, tikliniyor, hatta
     GERI BILDIRIM de veriyor. Ekran "calisiyor" gorunmenin otesinde,
     "calisti" DIYOR. Bir kez boyle kanan usta bir daha hicbir
     "gonderildi" yazisina inanmaz — bu yuzden sessiz bir olu uctan
     daha pahali.

     OLCUM: balon tamamlanmis bir eylem BILDIRIYORSA (gonderildi,
     eklendi, kaydedildi, silindi, guncellendi), ya veri ya ekran
     degismis olmali. Ikisi de ayni kaldiysa balon yalan soyluyor. */
  {
    var EYLEM_SOZU = ['gonderildi', 'eklendi', 'kaydedildi', 'silindi',
                      'guncellendi', 'olusturuldu', 'kaldirildi'];
    /* Turkce metinde kelime siniri ARANMAZ (K-018): balonun metni
       kucultulup aksani katlaniyor, sonra icerik araniyor. */
    function sadelestir(x) {
      return (x || '').toLocaleLowerCase('tr')
        .replace(/ı/g, 'i').replace(/ü/g, 'u').replace(/ö/g, 'o')
        .replace(/ş/g, 's').replace(/ç/g, 'c').replace(/ğ/g, 'g');
    }
    function veriParmakIzi() {
      var p = [];
      ['KAYITLAR', 'ARACLAR', 'GIDERLER', 'PARCACILAR'].forEach(function (ad) {
        try { p.push(ad + ':' + JSON.stringify(window[ad])); } catch (e) { p.push(ad + ':?'); }
      });
      return p.join('|');
    }
    var toast = document.getElementById('toast');
    /* Gezinme ve mikrofon dugmeleri elenir: isleri baska bir ekrani
       acmak, kendileri bir sey "yapmiyor" ve yaptigini da soylemiyor. */
    /* ⚠️ role="tab" DE ELENIR (05.09). Sekme bir GORUNUM degistirici:
       hicbir eylem IDDIA etmiyor, dolayisiyla "sahte eylem" olamaz.
       Ama tiklaninca ekranin kipini degistiriyor ve o kip EKRANIN KENDI
       degiskeninde duruyor — denetimin veri yedegi oraya ulasamaz.
       Sonuc: denetimden sonra muhasebe "detayli" kipte aciliyordu ve
       bu, K-024.1'in AYNI hatasinin ikinci yuzuydu (arac olctugu seyi
       degistiriyor). Test etmedigi bir seye dokunmasin. */
    var ELE = '[data-git], [data-sesle], [data-arac], [data-kayit], .daire-btn, .clear, .akd-bas, .cip, [role="tab"]';
    ekranlar.forEach(function (a) {
      showScreen(a);
      var dugmeler = Array.prototype.slice.call(
        document.querySelectorAll('#screen-' + a + ' button'));
      dugmeler.forEach(function (d) {
        if (d.disabled || d.closest(ELE) || d.matches(ELE)) return;
        var etiket = (d.id || (d.className || '').toString().split(' ')[0] || 'button');
        if (toast) toast.textContent = '';
        var oncekiVeri = veriParmakIzi();
        var oncekiEkran = a;
        var kap = document.getElementById('screen-' + a);
        var oncekiHtml = kap ? kap.innerHTML : '';
        try { d.click(); } catch (e) { return; }
        var balonMetni = sadelestir(toast ? toast.textContent : '');
        var iddia = EYLEM_SOZU.some(function (s) { return balonMetni.indexOf(s) >= 0; });
        if (!iddia) { showScreen(a); return; }
        var yeniEkran = (typeof Ekran !== 'undefined') ? Ekran.adi() : a;
        var yeniHtml = kap ? kap.innerHTML : '';
        if (yeniEkran === oncekiEkran && yeniHtml === oncekiHtml &&
            veriParmakIzi() === oncekiVeri) {
          ek('SAHTE-EYLEM', a, etiket + ' — balon "' +
             (toast.textContent || '').slice(0, 40) + '" diyor, hicbir sey degismedi');
        }
        showScreen(a);
      });
    });
    if (toast) { toast.textContent = ''; toast.classList.remove('show'); }
    showScreen('home');
  }

  /* 31) SES KELIME TUZAGI — kisa komut kelimesi baska bir kelimenin
     ICINDE bulunuyor (04.09.2026). K-018'IN UCUNCU TEKRARI.

     `\b` yasagi yazildi, yerine indexOf kondu ve ayni hata geri geldi:
       "yeni ARAC bakim"          -> "arac" icinde " ara"    -> ARAMA
       "BEYAZ araba geldi"        -> "beyaz" icinde "yaz"    -> MESAJ
       "GOSTERGE paneli yaniyor"  -> "gosterge" icinde "goster" -> ARAMA
     Ucu de ustanin en siradan cumleleri; ucunde de program ustanin
     dedigini degil, harf dizisini duyuyordu.

     Neden KURAL degil DAVRANIS olculuyor: kodda "indexOf" aramak
     yanlis pozitif yagdirir (uzun ve ayirt edici kokler icin indexOf
     DOGRU). Olculecek sey, tuzak cumlenin dogru okunup okunmadigi.

     NEGATIF DOGRULAMA (04.09): kelimeVarMi cagrilari indexOf'a geri
     cevrildi -> 3 bulgu; duzeltilince -> 0. */
  if (typeof window.SES_COZUMLE === 'function') {
    var sesYedekNiyet = window.SESLI_NIYET, sesYedekKayit = window.ACIK_KAYIT;
    window.SESLI_NIYET = 'genel';
    window.ACIK_KAYIT = null;
    /* [cumle, OLMAMASI GEREKEN tur] — plakali bir arac tarifi asla
       arama ya da mesaj olarak okunmamali. */
    [['41 KV 5588 yeni arac bakim', 'ara'],
     ['34 ABC 123 beyaz araba geldi', 'mesaj'],
     ['34 ABC 123 gosterge paneli yaniyor', 'ara']].forEach(function (x) {
      var o = null;
      try { o = window.SES_COZUMLE(x[0]); } catch (e) { return; }
      if (o && o.tur === x[1]) {
        ek('SES-KELIME-TUZAGI', 'ses',
           '"' + x[0] + '" -> ' + x[1] + ' sanildi (kisa kelime icinde bulundu)');
      }
    });
    window.SESLI_NIYET = sesYedekNiyet;
    window.ACIK_KAYIT = sesYedekKayit;
  }

  /* 32) KIRPILAN METIN — overflow:hidden bir kutuda icerik SESSIZCE
     kesiliyor (04.09.2026).

     20. test (IC-TASMA) overflowX'i 'visible' OLMAYAN kutulari
     ATLIYOR — dogru, cunku onlar tasmiyor. Ama tasmamalarinin sebebi
     icerigi KESMELERI. Kesilen sey metinse kullanici eksik bir kelime
     goruyor ve bunu bilmiyor.

     Yasanan: arac ekranindaki uc dugme 375 px'e gore olculmustu; 320
     px'te "Kayit ac" 96 px istiyor, kutu 89 px. overflow:hidden oldugu
     icin ne tasma testi ne de goz yakaladi — dugme "Kayit a" yaziyordu.

     Kasitli kisaltma AYRI: text-overflow:ellipsis konmussa tasarimci
     bilerek kisaltmis, uc nokta da kullaniciya bunu SOYLUYOR. */
  ekranlar.forEach(function (a) {
    showScreen(a);
    document.querySelectorAll('#screen-' + a + ' *').forEach(function (el) {
      if (!el.clientWidth) return;
      var c = getComputedStyle(el);
      if (c.overflowX === 'visible') return;                /* IC-TASMA'nin isi */
      if (c.overflowX === 'auto' || c.overflowX === 'scroll') return;  /* kaydirilabilir */
      if (c.textOverflow === 'ellipsis') return;            /* kasitli kisaltma */
      if (el.scrollWidth <= el.clientWidth + 1) return;
      /* Yalniz METIN tasiyan kutular: bos bir kap kirpilirsa gorunur
         bir sey kaybolmuyor. */
      if (!(el.textContent || '').trim()) return;
      ek('KIRPILAN-METIN', a,
         ((el.className || '').toString().split(' ')[0] || el.tagName) +
         ' "' + (el.textContent || '').trim().slice(0, 20) + '" ' +
         el.scrollWidth + '>' + el.clientWidth);
    });
  });

  /* 33) SIKISIK KUTU — iki kardes blok BITISIK duruyor (05.09.2026).

     Cemil ekran goruntusu gonderdi: "elementler birbirine cok yakin.
     Bunu genel olarak kontrol et." Olculdu — alti ekranda kart ve alan
     aralari 12–14 px'ti, bir yerde SIFIR: kalem listesinin son satiri
     ile "+ Parca / + Iscilik" dugmeleri bitisikti ve dugmeler listenin
     bir parcasi gibi okunuyordu.

     Neden hicbir test gormedi: bitisiklik bir HATA DEGIL, bir OLCU.
     Kutu dogru yerde, metin dogru, dokunma hedefi yeterli — yalniz
     nefes yok. Goz yakalar, test yakalamaz; o yuzden olculuyor.

     ⚠️ YALNIZ UST DUZEY KARDESLER: .field icindeki etiket-kutu ciftinin
     6 px'i KASITLI (birbirine ait olduklarini soyluyor). Olculen sey,
     BIRBIRINE AIT OLMAYAN iki blogun arasi. */
  {
    /* ⚠️ ESIK 8 -> 16 (05.09, ikinci tur). Ilk esik 8 px'ti ve
       "0 bulgu" veriyordu — ama Cemil ekrana bakip "yine yakin" dedi
       ve olcum onu dogruladi: on bir ekranda araliklar 14 px'te
       kalmisti, yani ESIGIN ALTINDA ama HEDEFIN ALTINDA.

       Test bir seyi olcmuyorsa degil, YANLIS ESIKLE olcuyorsa da
       yanlis guven verir. Esik artik tasarim hedefiyle ayni: kap
       gap'i 16-18, dolayisiyla 16'nin altina dusen her sey bulgu. */
    var ESIK = 16;
    /* ⚠️ KAPSAM: yalniz BLOK KAPLARI (05.09, ucuncu tur).
       Ilk yazimda yalniz .screen ve .card'a bakiliyordu ve Cemil'in
       gordugu SIFIR bosluklari kaciriyordu: sarmalayici div'ler
       (#muhBasitGovde, #ruhsatGovde, #sekmePlaka) araya girince
       .screen'in gap'i icerideki kardeslere ISLEMIYORDU.

       Butun kaplara bakmak da yanlis: bir BILESENIN ICI (.tl-body'de
       baslik->alt satir 8 px, .arac-bas'ta plaka->baslik 12 px)
       KASITLI olarak sikidir — o parcalar BIRBIRINE AITTIR ve
       aralarindaki az bosluk tam da bunu soyler. Butun kaplari olcunce
       165 "bulgu" cikti ve cogu mesruydu; boyle bir test okunmaz olur.

       Olculen sey: BOLUM listeleyen kaplar. Sinif verilmis olmasi
       sart — yeni bir sarmalayici eklendiginde .yigin yazilmazsa
       ritim sessizce kirilir, testin gormesi de bu yuzden. */
    var BLOK_KAP = '.screen, .card, .yigin';
    ekranlar.forEach(function (a) {
      showScreen(a);
      document.querySelectorAll('#screen-' + a + ', #screen-' + a + ' ' +
          BLOK_KAP.split(', ').join(', #screen-' + a + ' ')).forEach(function (kap) {
        var cocuk = Array.prototype.filter.call(kap.children, function (c) {
          var r = c.getBoundingClientRect();
          return r.height > 0 && getComputedStyle(c).display !== 'none';
        });
        for (var i = 1; i < cocuk.length; i++) {
          var ust = cocuk[i - 1].getBoundingClientRect();
          var alt = cocuk[i].getBoundingClientRect();
          /* Yan yana duran ogeler (ayni satirda) dikey olcuye girmez. */
          if (alt.top < ust.bottom - 2) continue;
          var bosluk = alt.top - ust.bottom;
          if (bosluk >= ESIK) continue;
          ek('SIKISIK', a,
             ((cocuk[i-1].className || '').toString().split(' ')[0] || cocuk[i-1].tagName) +
             ' -> ' +
             ((cocuk[i].className || '').toString().split(' ')[0] || cocuk[i].tagName) +
             ' = ' + Math.round(bosluk) + 'px');
        }
      });
    });
    showScreen('home');
  }

  /* 34) GIZLI AMA GORUNUR — `hidden` ozniteligi var, oge EKRANDA
     (05.09.2026).

     `hidden`, tarayicinin kendi stilinde `display: none` demektir ve
     YAZDIGIMIZ HER `display` KURALINA YENILIR. Bugun `.card` flex
     yapildi ve gider formu KAPALIYKEN ekranda durdu: oznitelik
     duruyordu, form goruunuyordu.

     Hicbir test gormedi cunku hicbiri "gizli olan gercekten gizli mi"
     diye sormuyordu; ekran goruntusunde yakalandi. Bu, sessiz
     bozulmanin ders kitabi hali: kod dogru, oznitelik dogru, GORUNUS
     yanlis.

     NEGATIF DOGRULAMA (05.09): [hidden]{display:none!important}
     kurali gecici olarak kapatildi -> 3 bulgu; acilinca -> 0. */
  ekranlar.forEach(function (a) {
    showScreen(a);
    document.querySelectorAll('#screen-' + a + ' [hidden]').forEach(function (el) {
      var r = el.getBoundingClientRect();
      if (r.width > 0 || r.height > 0) {
        ek('GIZLI-AMA-GORUNUR', a,
           ((el.id || (el.className || '').toString().split(' ')[0]) || el.tagName) +
           ' ' + Math.round(r.width) + 'x' + Math.round(r.height));
      }
    });
  });
  showScreen('home');

  /* 35) SAYISAL ALANA HARF GIRIYOR (05.09.2026)
     Cemil: "Tutar alanina harf giremeyiz. Inputlarin veri
     dogrulugundan emin ol."

     ⚠️ inputmode="numeric" HICBIR SEYI ENGELLEMEZ — telefonun hangi
     klavyeyi acacagini soyler, bir ONERIDIR. Masaustunde, yapistirmada
     ve sesle giriste harf rahatca giriyordu. Uc tutar alaninda da yoktu.

     NEDEN SESSIZ VE PAHALI: okuma tarafi zaten temizdi
     (replace(/[^0-9]/g,'')), yani VERI bozulmuyordu — ama ustaya
     GOSTERILEN sey bozuluyordu. "abc12x3" yazan usta ekranda onu
     goruyor, kayda 12.345 TL dusuyordu.

     IKI SEY OLCULUYOR:
       a) data-giris olan alana harf yazilinca harf KALIYOR mu
       b) sayisal gorunen (inputmode=numeric/tel) ama data-giris
          TASIMAYAN alan var mi — yani korumasi UNUTULMUS alan

     (b) olmadan test yalniz bugunku alanlari korur; yarin eklenen
     alan yine korumasiz girer ve kimse gormez.

     NEGATIF DOGRULAMA (05.09): data-giris oznitelikleri gecici olarak
     silindi -> 6 bulgu; geri konunca -> 0. */
  {
    var HARFLI = 'abc12x3,4.5 TL';
    ekranlar.forEach(function (a) {
      showScreen(a);
      /* Gider formu kapali gelir; alanini gorebilmek icin acilir. */
      var ge = document.getElementById('muhGiderEkle');
      if (a === 'muhasebe' && ge) ge.click();

      /* ⚠️ GORUNURLUK SARTI YOK. Ilk yazimda `if (!el.offsetHeight)
         return;` vardi ve negatif test bunu ele verdi: ruhsat
         ekranindaki yil/km/telefon alanlari ancak KAYITSIZ bir plaka
         yazilinca aciliyor, o yuzden test onlari HIC gormuyordu —
         korumalari silinse bile "0 bulgu" derdi.
         Bir alanin korumasi, o an gorunur olup olmamasindan bagimsizdir. */
      document.querySelectorAll('#screen-' + a + ' input').forEach(function (el) {
        if (el.type === 'search' || el.type === 'hidden') return;
        var tip = el.getAttribute('data-giris');
        var mod = el.getAttribute('inputmode');
        var ad = el.id || (el.className || '').toString().split(' ')[0] || 'input';
        /* BICIMLI ALAN = sayisal klavye isteyen ya da mono yazan alan.
           Mono yaziyorsa bir BICIMI vardir (plaka, km, tutar, telefon);
           bicimi olan alan korumasiz birakilmaz. */
        var bicimli = (mod === 'numeric' || mod === 'tel' || mod === 'decimal') ||
                      /(^| )mono( |$)/.test((el.className || '').toString());

        /* (b) bicimli gorunuyor ama korumasi yok */
        if (!tip && bicimli) {
          ek('KORUMASIZ-ALAN', a, ad + ' bicimli alan ama data-giris yok');
          return;
        }
        if (!tip) return;

        /* (a) bicim disi karakter kaliyor mu.
           ⚠️ "HARF VAR MI" diye sormak YANLIS: plaka HARF ICERIR
           (34 ABC 123) ve ilk yazimda test onu bulgu sandi. Her tipin
           kendi KABUL DESENI var; olculen sey harf degil, tipin
           disina cikan karakter. */
        var KABUL = {
          tutar: /^[0-9.]*$/,
          km:    /^[0-9.]*$/,
          yil:   /^[0-9]*$/,
          tel:   /^[0-9 ]*$/,
          plaka: /^[0-9A-ZÇĞİÖŞÜ ]*$/,
      /* Ruhsatin sasi/motor numarasi ve ilk tescil tarihi (06.09).
         Denetimin sozlugu, cekirdegin sozlugunden GERI KALAMAZ: yeni
         bir data-giris tipi eklenip buraya yazilmazsa test "boyle bir
         tip yok" der ve dogru kodu hata sanar. */
      kod:   /^[0-9A-ZÇĞİÖŞÜ]*$/,
      tarih: /^[0-9.]*$/
        };
        var eski = el.value;
        el.value = HARFLI;
        el.dispatchEvent(new Event('input', { bubbles: true }));
        var desen = KABUL[tip];
        if (!desen) {
          ek('KORUMASIZ-ALAN', a, ad + ' data-giris="' + tip + '" — boyle bir tip yok');
        } else if (!desen.test(el.value)) {
          ek('SAYISAL-ALANDA-HARF', a, ad + ' (' + tip + ') -> "' + el.value + '"');
        }
        el.value = eski;
        el.dispatchEvent(new Event('input', { bubbles: true }));
      });
    });
    showScreen('home');
  }

  /* 36) GORUNMEYEN YATAY KAYDIRMA (05.09.2026)
     Cemil: "Gider ve alacak tablolari telefonun ekranina sigmiyor,
     saga kaydirmak gerekiyor. KULLANICI SAGA KAYDIRACAGINI BILMEZ."

     ⚠️ BU, K-025'TE VERDIGIM KARARIN ACIGI. Orada "kesilen metin,
     tasan metinden tehlikelidir" deyip overflow-x:auto koymustum.
     Kirpmaya gore dogruydu ama UCUNCU bir secenek vardi ve o daha
     dogru: SIGDIRMAK.

     Gorunmeyen kaydirma, kirpmanin kardesidir: ikisinde de kullanici
     bir seyin eksik oldugunu BILMEZ. Fark, kirpmada yarim kelime
     gormesi; kaydirmada HIC gormemesi.

     Olculen: ekran icinde YATAY kaydirilabilen ve gercekten TASAN bir
     kutu var mi. Dikey kaydirma (.screen'in kendisi, cekmece govdesi)
     kapsam disi — telefonda asagi kaydirmak ogrenilmis davranistir,
     saga kaydirmak degil.

     NEGATIF DOGRULAMA (05.09): .tablo'ya table-layout:auto ve
     white-space:nowrap geri konunca 3 bulgu; kaldirilinca 0. */
  ekranlar.forEach(function (a) {
    showScreen(a);
    document.querySelectorAll('#screen-' + a + ' *').forEach(function (el) {
      var c = getComputedStyle(el);
      if (c.overflowX !== 'auto' && c.overflowX !== 'scroll') return;
      var tasma = el.scrollWidth - el.clientWidth;
      if (tasma <= 2) return;
      ek('GIZLI-KAYDIRMA', a,
         ((el.className || '').toString().split(' ')[0] || el.id || el.tagName) +
         ' saga ' + Math.round(tasma) + 'px kayiyor');
    });
  });
  showScreen('home');


  /* 37) SESSIZ KAPALI DUGME (06.09.2026)
     Cemil: *"Kaydi Ac buttonuna bastim, bir sey olmuyor."* Dugme
     `disabled` idi ve NEDEN kapali oldugunu hicbir sey soylemiyordu
     (K-033.3). Kapali dugme sessizdir: basan kisi ne eksik oldugunu
     ogrenemez, dugmenin bozuk oldugunu saniir.

     OLCULEN: gorunur ve `disabled` bir dugme, sebebini KENDINE BAGLI
     olarak tasiyor mu — `aria-describedby` (dolu bir ogeye isaret
     eden) ya da `title`.

     ⚠️ NEDEN "EKRANDA BIR YERDE ACIKLAMA VAR MI" DIYE BAKMIYOR:
     baksaydi K-033.3'u KACIRIRDI. O ekranda uyari seritleri vardi —
     ama baska seyler hakkinda. Ekranda bir yerde bir aciklama olmasi,
     BU dugmenin neden kapali oldugunu soylemez. Baglanti acik olmali:
     ya dugmeye bagla, ya dugmeyi kapatma.

     Ekran disi dugmeler (header'in geri/ileri oku) kapsam disi —
     onlarda kapali olmak zaten "gidecek yer yok" demek. */
  ekranlar.forEach(function (a) {
    showScreen(a);
    document.querySelectorAll('#screen-' + a + ' button[disabled]').forEach(function (d) {
      if (d.offsetParent === null && d.offsetHeight === 0) return;   /* gorunmuyorsa olculmez */
      var ad = d.id || (d.textContent || '').trim().slice(0, 24) || d.className;
      var ac = d.getAttribute('aria-describedby');
      var hedef = ac ? document.getElementById(ac) : null;
      var acikli = (hedef && (hedef.textContent || '').trim().length > 0) ||
                   ((d.getAttribute('title') || '').trim().length > 0);
      if (!acikli) {
        ek('SESSIZ-KAPALI-DUGME', a, ad + ' kapali ama sebebi dugmeye bagli degil');
      }
    });
  });
  showScreen('home');

  /* 38) CIZIMDE SIFIRLAMA (06.09.2026)
     06.09'da AYNI HATAYI BIR TURDA IKI KEZ yaptim: `ciz()` icine
     acilisa ait bir sifirlama yazdim (`kSilinecek = null`,
     `kTurElle = false`). `Ekran.tazele()` ciz'i cagiriyor; her
     tazeleme kullanicinin SECIMINI siliyordu. Ikisini de 36 test
     kacirdi; elle isletince cikti (K-033.12).

     KOK SEBEP `ciz`in IKI ISI BIRDEN yapmasiydi: "ekran acildi" ve
     "yeniden ciz". Ekran ikisini ayirt edemiyordu. Cekirdege `ac`
     kancasi eklendi (K-040) — acilis isi oraya yazilir.

     OLCULEN, STATIK: her ekranin `ciz` kaynaginda UST DUZEY bir
     sifirlama var mi (`x = null|false|0|''`). Ust duzey sart: bir
     `if` blogunun icindeki `MESAJ_HEDEF = null` mesru — o, "oku ve
     temizle" desenidir ve tazelemede zaten kosmuyor.

     ⚠️ NEDEN DAVRANIS TESTI DEGIL: once "ekrani ciz, aria-pressed
     say, tazele, yeniden say" diye bir test tasarladim. Yazmadan once
     dunku iki hataya karsi denedim: IKISINI DE YAKALAMIYORDU (tur
     secimi kaydin ustunde durdugu icin cip basili kaliyor; silme
     onayinda aria-pressed hic yok). Bos bir test, testsizlikten
     kotudur — 29. testte bunu bir kez yasadik. */
  {
    var tanimlar = (typeof Ekran !== 'undefined' && Ekran.tanimlar) ? Ekran.tanimlar() : null;
    if (!tanimlar) {
      ek('CIZIMDE-SIFIRLAMA', '(cekirdek)', 'Ekran.tanimlar() yok — test bakamiyor');
    } else {
      Object.keys(tanimlar).forEach(function (a) {
        var fn = tanimlar[a] && tanimlar[a].ciz;
        if (typeof fn !== 'function') return;
        var src = fn.toString();
        var govde = src.slice(src.indexOf('{') + 1);
        var derinlik = 1, i = 0, ustDuzey = '';
        while (i < govde.length && derinlik > 0) {
          var c = govde.charAt(i);
          if (c === '{') derinlik++;
          else if (c === '}') derinlik--;
          if (derinlik === 1 && c !== '{' && c !== '}') ustDuzey += c;
          i++;
        }
        /* Yorumlar atilir: yorumda gecen bir ornek kod TANIM degildir
           (4. testin ogrettigi ders). */
        ustDuzey = ustDuzey.replace(/\/\*[\s\S]*?\*\//g, ' ').replace(/\/\/[^\n]*/g, ' ');
        /* ⚠️ YEREL BILDIRIMLER BASTAN ATILIR. Ilk yazimda
           `var a = 0, b = 0, c = 0;` dizisinin IKINCI ve UCUNCU
           degiskenlerini sifirlama sandim: `var` yalniz ilkinin
           onunde duruyor. is-basina ekraninin yerel toplayicisi
           (`tFark = 0`) boyle yanlis bulgu verdi.

           Ilk duzeltmem de yetmedi — virgulu disladim ama `tFark`in
           HEMEN solunda virgul degil BOSLUK vardi. Dogru cozum tek
           tek karakter dislamak degil, BILDIRIMIN TAMAMINI atmak:
           yerel degisken tanimlamak sifirlama degildir.

           Test iki kez yanlis bulgu verdi ve iki kez duzeltildi;
           bir testin kendisi de olculur. */
        ustDuzey = ustDuzey.replace(/(^|[^\w$])(?:var|let|const)\s[^;]*;/g, ' ');
        var re = /(^|[^\w$.])([A-Za-z_$][\w$]*)\s*=(?!=)\s*(null|false|0|''|"")\s*;/g;
        var m;
        while ((m = re.exec(ustDuzey))) {
          ek('CIZIMDE-SIFIRLAMA', a,
             m[2] + ' = ' + m[3] + ' — acilis isi, `ac` kancasina tasinmali');
        }
      });
    }
  }
  showScreen('home');


  /* 39) SESSIZCE KIRPILAN ALAN (06.09.2026)
     11. test (KIRPILAN-METIN) `overflow:hidden` kutulara bakiyor;
     `<input>` ve `<textarea>` ONUN KAPSAMINDA DEGIL. Oysa bir input
     tasan metni ne sarar ne uc nokta koyar — SESSIZCE keser. Ekranda
     "Bakim isc" yazar, ustanin "Bakim isciligi" yazdigini soyleyen
     hicbir isaret olmaz.

     320 px'te bulundu ve ELLE bulundu (kuyruk 9): 36 test "0 bulgu"
     diyordu, ekran goruntusune bakinca uc alanin kirpildigi goruldu.
     Olculen tasma 5-23 px idi.

     K-025'in kurali burada da isliyor: KESILEN metin, TASAN metinden
     tehlikelidir. Bu test o kurali, kuralin hic bakmadigi bir yuzeye
     tasiyor.

     ⚠️ DEGERI OLAN alanlara bakilir. Bos bir input'un scrollWidth'i
     zaten clientWidth'e esittir; yer tutucu (placeholder) kirpilirsa
     bu bir kayip degil — asil metin degil. */
  ekranlar.forEach(function (a) {
    showScreen(a);
    document.querySelectorAll('#screen-' + a + ' input')
      .forEach(function (el) {
        /* ⚠️ TEXTAREA HARIC (07.09). Textarea metni SARAR; yatay
           kirpma onun hata bicimi degil. Olculen 15-16 px fark,
           dikey kaydirma cubugunun payiydi — testin kendi yanlis
           pozitifi. Tek satirlik <input> ise gercekten keser. */
        if (el.tagName === 'TEXTAREA') return;
        if (el.type === 'hidden' || el.type === 'checkbox' || el.type === 'radio') return;
        if (el.offsetParent === null && el.offsetHeight === 0) return;
        if (!el.value) return;
        var tasma = el.scrollWidth - el.clientWidth;
        if (tasma <= 2) return;
        ek('SESSIZCE-KIRPILAN-ALAN', a,
           (el.id || el.getAttribute('data-satir-ad') !== null ? (el.id || 'satir adi') :
            (el.className || 'input')) +
           ' "' + el.value.slice(0, 20) + '" ' + tasma + 'px kesiliyor');
      });
  });
  showScreen('home');


  /* 40) KATMAN ODAK TUZAGI + ORTU YIGINI (07.09.2026)
     14. test cekmecenin odak tuzagini olcuyor. 07.09'da IKINCI bir
     ortu geldi (katman/popup) ve ayni hata sinifi yeni bir yuzeyde
     dogdu — ustelik yalnizca kendi tuzagi degil, IKISI BIRDEN acikken
     ortaya cikan bir hata:

     Ilk yazimda arka plani kapatan islev "ac" derken inert EKLIYOR,
     "kapat" derken KALDIRIYORDU. Katman kapaninca CEKMECENIN koydugu
     kilidi de kaldiriyor ve arkadaki 17 oge sekme sirasina donuyordu.
     Elle bulundu ve ancak denetim IKI KEZ kosturulunca uretiliyordu —
     cunku denetim katmani acik birakiyordu.

     Bu test iki seyi birden olcuyor:
       a) katman acikken disarida odaklanabilir oge var mi
       b) cekmece acikken katman ac/kapa yapilinca CEKMECENIN kilidi
          duruyor mu (ortu yigini)

     Katmani denetim kendisi aciyor ve kendisi kapatiyor — olctugu
     durumu geri birakiyor (K-024.1). */
  if (typeof Katman !== 'undefined' && Katman.ac && typeof openDrawer === 'function') {
    showScreen('home');
    var kmOge = document.getElementById('katman');
    Katman.ac('denetim', '<button id="denetimKatmanDugmesi">tamam</button>');
    /* kapsa() 12. testin IIFE'sinin icinde; buradan gorunmuyor.
       Odaklanabilir secici burada kendi basina yaziliyor. */
    var ODAKLANABILIR = 'button, a[href], input, textarea, select, [tabindex]';
    var kmDis = Array.prototype.slice.call(document.querySelectorAll(ODAKLANABILIR)).filter(function (el) {
      return el.offsetParent !== null && !el.disabled && !kmOge.contains(el) && !el.closest('[inert]');
    });
    if (kmDis.length) ek('KATMAN-ODAK-SIZINTISI', 'katman', kmDis.length + ' oge');
    Katman.kapa();

    /* ORTU YIGINI: cekmece altta, katman ustte. */
    openDrawer();
    Katman.ac('denetim', '<button id="denetimKatmanDugmesi2">tamam</button>');
    Katman.kapa();
    if (!document.querySelector('.topbar').inert) {
      ek('ORTU-YIGINI', 'katman', 'katman kapaninca cekmecenin kilidi kalkti');
    }
    closeDrawer();
    if (document.querySelector('.topbar').inert) {
      ek('ORTU-YIGINI', 'katman', 'hepsi kapaliyken kilit kaldi');
    }
  }
  showScreen('home');

  /* 43) GONDERIM ARSIVE YAZMIYOR (14.09.2026)
     Cemil: "Gönderilen mesajlar listelenebilmeli."

     YENI HATA SINIFI: SONUCU BASKA YERDE DURAN EYLEM. Gonderme yolu
     iki yerde (mesaj ekrani ve sesli islem) ve ikisi de arsive yazmak
     zorunda. Yazmayan yol SESSIZ bozulur: balon "WhatsApp aciliyor"
     der, mesaj gercekten gider, ekran dogru gorunur — yalniz liste
     eksik kalir ve "isin ozeti gonderilmemis araca" kurali yanlis
     calisir. 30. test (SAHTE-EYLEM) bunu goremez: orada olculen sey
     "bir sey degisti mi", burada olculen "DOGRU YERDE mi degisti".

     Yol GERCEKTEN isletiliyor: musteri satirina tiklanip gonder
     dugmesine basiliyor. Denetim MESAJLAR'i yedekliyor (VERI_TABLOLARI)
     ve sonda geri yukluyor. */
  if (typeof MESAJLAR !== 'undefined' && typeof mesajHedefi === 'function') {
    var mesajTanimA = (typeof Ekran !== 'undefined' && Ekran.tanimlar) ? Ekran.tanimlar()['mesaj'] : null;
    if (mesajTanimA && mesajTanimA.geriAl) { try { mesajTanimA.geriAl(); } catch (e) {} }
    showScreen('mesaj');
    var msjSatirlar = document.querySelectorAll('#msjListe .mus-kart[data-mesaj-plaka]');
    var telliSatir = null;
    for (var mi = 0; mi < msjSatirlar.length; mi++) {
      var mpl = msjSatirlar[mi].getAttribute('data-mesaj-plaka');
      var mhd = mesajHedefi(mpl);
      if (mhd && mhd.tel) { telliSatir = msjSatirlar[mi]; break; }
    }
    /* Olcemeyen test, olcum yapmis gibi rapor etmez. */
    if (!telliSatir) {
      ek('MESAJ-ARSIV', 'mesaj', 'telefonu olan musteri yok — yol isletilemedi');
    } else {
      var arsivOnce = MESAJLAR.length;
      telliSatir.click();
      var msjGonderD = document.getElementById('msjGonder');
      if (!msjGonderD || msjGonderD.disabled) {
        ek('MESAJ-ARSIV', 'mesaj', 'musteri secildi ama gonder dugmesi kapali');
      } else {
        msjGonderD.click();
        if (MESAJLAR.length !== arsivOnce + 1) {
          ek('MESAJ-ARSIV', 'mesaj', 'WhatsApp ile gonderildi ama arsive yazilmadi');
        } else {
          var sonMsj = MESAJLAR[MESAJLAR.length - 1];
          ['plaka', 'tel', 'kanal', 'metin'].forEach(function (alan) {
            if (!sonMsj[alan]) ek('MESAJ-ARSIV', 'mesaj', 'arsiv kaydinda "' + alan + '" bos');
          });
        }
      }
    }
    if (mesajTanimA && mesajTanimA.geriAl) { try { mesajTanimA.geriAl(); } catch (e) {} }

    /* IKINCI YOL: SESLI ISLEM (14.09). Cemil: "Buradan mesaj sesli
       olarak gonderebilmeliyiz." Ayni kural iki yoldan da gecmeli —
       ve tam bu yuzden test iki yolu da isletiyor: bir gun biri
       degisir, oteki oldugu gibi kalir. */
    if (typeof window.SES_SOYLE === 'function') {
      showScreen('ses');
      /* Baglam ve acik kayit, sesli cozumleyicinin ILK dalini
         tetikleyebilir ("kayit-icinde" satir ekler). Ikisi de yedekte,
         sonda geri yukleniyor. */
      window.SESLI_NIYET = 'mesaj';
      window.ACIK_KAYIT = null;
      var sesArsivOnce = MESAJLAR.length;
      window.SES_SOYLE('34 ABC 123 sahibine whatsapp selam mesaji gonder');
      var sesOnayD = document.getElementById('sesOnayla');
      if (!sesOnayD) {
        ek('MESAJ-ARSIV', 'ses', 'sesli mesajin onay dugmesi cizilmedi');
      } else {
        sesOnayD.click();
        if (MESAJLAR.length !== sesArsivOnce + 1) {
          ek('MESAJ-ARSIV', 'ses', 'sesle gonderildi ama arsive yazilmadi');
        } else if (!MESAJLAR[MESAJLAR.length - 1].metin) {
          ek('MESAJ-ARSIV', 'ses', 'sesli gonderimin arsiv kaydinda metin bos');
        }
      }
    }
    showScreen('home');
  }

  /* 45) ORNEK CUMLE CALISMIYOR (14.09.2026)
     Cemil: "Servis kayitlari listesinde sesli olarak servistekileri
     goster secenegini sectim ama sonuc bulunamadi cikti."

     YENI HATA SINIFI: EKRANIN KENDI ORNEGI YALAN SOYLUYOR. Ses
     ekranindaki cipler ustaya "sunu diyebilirsin" diyor; bir terim
     degisince (07.09'da "iceride" -> "serviste") cozumleyici eski
     kelimede kaldi ve ornegin KENDISI calismaz oldu. Usta ekranin
     onerdigi cumleyi soyluyor ve "sonuc bulunamadi" goruyor — bu,
     sesten tamamen sogutan hata turudur.

     Ornek cipler zaten birer negatif testti; kimse isletmiyordu.
     Bu test hepsini isletiyor:
       · cozumlenemeyen ornek  -> bulgu
       · arama ornegi BOS liste donduruyorsa -> bulgu
     Olctugu durumu geri birakiyor: arama kutulari temizleniyor,
     ekran ana menuye donuyor. */
  if (typeof window.SES_BAGLAMLAR === 'function' && typeof window.SES_COZUMLE === 'function') {
    var ORNEKLER = window.SES_BAGLAMLAR();
    var LISTE_YUVA = { araclar: '#arcListe', kayitlar: '#kytListe', musteriler: '#msListe' };
    var ARAMA_KUTU = { araclar: 'arcQ', kayitlar: 'kytQ', musteriler: 'msQ' };
    Object.keys(ORNEKLER).forEach(function (baglam) {
      (ORNEKLER[baglam] || []).forEach(function (cumle) {
        window.SESLI_NIYET = baglam;
        window.ACIK_KAYIT = (baglam === 'kayit-icinde' && KAYITLAR.length) ? KAYITLAR[0].id : null;
        var o;
        try { o = window.SES_COZUMLE(cumle); }
        catch (e) { ek('ORNEK-CUMLE', baglam, '"' + cumle + '" — cozumleyici patladi: ' + e.message); return; }
        if (!o || o.tur === 'plakasiz') {
          ek('ORNEK-CUMLE', baglam, '"' + cumle + '" — anlasilmadi');
          return;
        }
        /* ARAMA ORNEGI GERCEKTEN SONUC DONDURUYOR MU. Bir ornegin
           "anlasilmasi" yetmiyor; ustanin gordugu sey LISTE. */
        if (o.tur === 'ara' && LISTE_YUVA[o.hedef]) {
          window.ARAMA_TERIMI = { hedef: o.hedef, terim: o.terim, suzgec: o.suzgec || null };
          showScreen(o.hedef);
          var yuva = document.querySelector(LISTE_YUVA[o.hedef]);
          var satirSay = yuva ? yuva.querySelectorAll('.arac-kart, .mus-kart, .tl-item').length : 0;
          if (!satirSay) {
            ek('ORNEK-CUMLE', baglam, '"' + cumle + '" — liste BOS dondu (' + o.hedef + ')');
          }
          /* Arama kutusunu ve suzgeci temizle: olcum izini birakmasin. */
          var kutu = document.getElementById(ARAMA_KUTU[o.hedef]);
          if (kutu) { kutu.value = ''; kutu.dispatchEvent(new Event('input', { bubbles: true })); }
          var suzgecKapat = document.getElementById('kytSuzgecKapat');
          if (suzgecKapat) suzgecKapat.click();
          showScreen(o.hedef);
        }
      });
    });
    showScreen('home');
  }

  /* 44) YANLIS DUYULAN PLAKA — ONERI (14.09.2026 · A-M27)
     Cemil'in ses kaydinda tanima "34 ABC 123" yerine "34 QBC 123"
     duydu ve akis durdu.

     YENI HATA SINIFI: TAHMIN EDEN EKRAN. Bir oneri iki yonden bozulur
     ve ikisi de sessizdir:
       a) HIC ONERMEZ  — usta ruhsat cekmeye gonderilir (eski hal),
       b) FAZLA ONERIR — iki aday varken birini secer, ya da oneriyi
          SORMADAN uygular. Ikincisi en pahalisi: ekran dogru gorunur,
          mesaj YANLIS ARACA gider ve usta bunu musteri sorunca ogrenir.

     Bu yuzden test yalnizca "oneri cikti mi" diye bakmiyor; oneriye
     dokunulunca HICBIR SEYIN GONDERILMEDIGINI de olcuyor.

     Denetim ARACLAR'a gecici bir arac ekliyor (iki aday durumunu
     uretmek icin) — yedekten geri yukleniyor (K-024.1). */
  if (typeof benzerPlaka === 'function' && typeof ARACLAR !== 'undefined' && ARACLAR.length) {
    var ilkPlaka = ARACLAR[0].plaka;
    var parca = String(ilkPlaka).split(/\s+/);
    if (parca.length !== 3 || parca[1].length < 2) {
      ek('PLAKA-ONERISI', '(veri)', 'ilk plaka uc parcali degil: ' + ilkPlaka);
    } else {
      /* Harf grubunun ILK harfini degistir: bir karakter farkli. */
      var baskaHarf = (parca[1][0] === 'Z' ? 'Y' : 'Z') + parca[1].slice(1);
      var yanlisPlaka = parca[0] + ' ' + baskaHarf + ' ' + parca[2];

      if (benzerPlaka(yanlisPlaka) !== ilkPlaka) {
        ek('PLAKA-ONERISI', '(veri)', yanlisPlaka + ' -> beklenen "' + ilkPlaka +
           '", gelen "' + benzerPlaka(yanlisPlaka) + '"');
      }
      /* Kayitli plakaya oneri yapilmaz. */
      if (benzerPlaka(ilkPlaka) !== null) {
        ek('PLAKA-ONERISI', '(veri)', 'kayitli plakaya oneri cikti: ' + ilkPlaka);
      }
      /* Uzak plaka: il kodu da rakam da farkli. */
      if (benzerPlaka('81 ZZZ 9999') !== null) {
        ek('PLAKA-ONERISI', '(veri)', 'alakasiz plakaya oneri cikti');
      }
      /* IKI ADAY -> ONERI YOK. Gecici arac ekleniyor. */
      var ikinciHarf = (parca[1][0] === 'X' ? 'W' : 'X') + parca[1].slice(1);
      ARACLAR.push({ plaka: parca[0] + ' ' + ikinciHarf + ' ' + parca[2],
                     marka: 'Denetim', model: 'Gecici', sahipAd: '', sahipTel: '' });
      var ortaHarf = (parca[1][0] === 'V' ? 'U' : 'V') + parca[1].slice(1);
      if (benzerPlaka(parca[0] + ' ' + ortaHarf + ' ' + parca[2]) !== null) {
        ek('PLAKA-ONERISI', '(veri)', 'iki aday varken oneri cikti — tahmin ediliyor');
      }
      ARACLAR.pop();

      /* EKRANDA: oneri bir DUGME mi, ve dokununca bir sey GONDERIYOR mu. */
      if (typeof window.SES_SOYLE === 'function' && typeof MESAJLAR !== 'undefined') {
        showScreen('ses');
        window.SESLI_NIYET = 'mesaj';
        window.ACIK_KAYIT = null;
        var mesajOnce = MESAJLAR.length;
        window.SES_SOYLE(yanlisPlaka + ' sahibine whatsapp selam mesaji gonder');
        var oneriD = document.getElementById('sesOneri');
        if (!oneriD) {
          ek('PLAKA-ONERISI', 'ses', 'yanlis duyulan plakada oneri dugmesi cizilmedi');
        } else {
          oneriD.click();
          if (MESAJLAR.length !== mesajOnce) {
            ek('PLAKA-ONERISI', 'ses', 'oneriye dokununca MESAJ GONDERILDI — soru degil, eylem olmus');
          }
          if (Ekran.adi() !== 'ses') {
            ek('PLAKA-ONERISI', 'ses', 'oneriye dokununca ekran degisti: ' + Ekran.adi());
          }
          /* Duzeltme gercekten islemis mi: artik oneri kalmamali ve
             islem gonderilebilir olmali. */
          if (document.getElementById('sesOneri')) {
            ek('PLAKA-ONERISI', 'ses', 'duzeltmeden sonra oneri hala duruyor');
          }
        }
      }
    }
    showScreen('home');
  }

  /* 42) AYRISAN LISTE — AYNI LISTE IKI EKRANDA (14.09.2026)
     Cemil: "Müşteriye mesaj ve müşteriler sayfasının listelemeleri
     aynı olacak."

     YENI HATA SINIFI: AYNI VERININ IKI EKRANDA IKI GORUNTUSU.
     K-019.1 bunun SUZGEC halini yasamisti (ayni hatirlatma cekmecede
     var, ana ekranda yok). Ikinci hali CIZIM: mesaj ekrani ARAC basina
     satir uretiyordu, musteriler ekrani MUSTERI basina — iki aracli
     musteri birinde iki kez, otekinde bir kez goruluyordu. Hicbir test
     bunu goremezdi, cunku her iki ekran da KENDI icinde dogruydu.

     Bu test ikisini KARSILASTIRIYOR: ayni bilesen mi, ayni satirlar mi,
     ayni sirada mi. Ekranlarin ic dogrulugu degil, ARALARINDAKI tutarlilik
     olculuyor — bir nesne bir yerde cizilir kuralinin bekcisi. */
  if (typeof musteriListesi === 'function' && musteriListesi('').length) {
    /* Mesaj ekrani "yazim" adiminda kalmis olabilir (30. test dugmelere
       basiyor); listeyi gorebilmek icin ekranin KENDI geri adimi
       kullaniliyor — denetim ekranin ic degiskenine elle dokunmuyor. */
    var mesajTanim = (typeof Ekran !== 'undefined' && Ekran.tanimlar) ? Ekran.tanimlar()['mesaj'] : null;
    if (mesajTanim && mesajTanim.geriAl) { try { mesajTanim.geriAl(); } catch (e) {} }
    showScreen('musteriler');
    var listeSol = Array.prototype.slice.call(document.querySelectorAll('#msListe .mus-kart'));
    showScreen('mesaj');
    var listeSag = Array.prototype.slice.call(document.querySelectorAll('#msjListe .mus-kart'));
    var musAdi = function (el) {
      var x = el.querySelector('.mus-ad');
      return x ? (x.textContent || '').trim() : '(adsiz)';
    };
    if (!listeSol.length || !listeSag.length) {
      ek('AYRISAN-LISTE', 'musteriler/mesaj',
         'ortak musteri satiri kullanilmiyor: musteriler=' + listeSol.length +
         ' mesaj=' + listeSag.length);
    } else if (listeSol.map(musAdi).join(' | ') !== listeSag.map(musAdi).join(' | ')) {
      ek('AYRISAN-LISTE', 'musteriler/mesaj',
         'ayni liste iki ekranda farkli: [' + listeSol.map(musAdi).join(', ') +
         '] <-> [' + listeSag.map(musAdi).join(', ') + ']');
    }
    showScreen('home');
  }

  /* 41) ANA TUSU GIDIS-DONUS (14.09.2026)
     Cemil: "Alttaki menü ikonuna basınca anasayfaya gidiyor. Tekrar
     basınca önceki ekrana dönsün."

     YENI HATA SINIFI: BIR TUS IKI IS YAPIYOR. Bugune kadar her tusun
     tek anlami vardi; bu tusun anlami DURUMA bagli. Boyle bir tus iki
     yerden bozulur:
       a) ikinci dokunus donmez (donus hedefi hic yazilmamistir ya da
          bir yerde silinmistir) — usta ayni ekrana iki kez bakar,
       b) donus hedefi FAZLA yasar: usta ana menuden baska bir ekrana
          gecer, tus onu COKTAN TERK ETTIGI ekrana atar.
     (b) daha sinsi, cunku ekran dogru CIZILIR — yanlis olan NEREYE
     gittigidir. Ikisi de burada olculuyor.

     Denetim gezinme yigini kirletiyor (Ekran.ac cagiriyor); olctugu
     durumu geri birakiyor: Ekran.sifirla(). */
  if (typeof Ekran !== 'undefined' && Ekran.sifirla && Ekran.ana) {
    var anaTus = document.querySelector('.botbar [data-nav="ana"]');
    if (!anaTus) ek('ANA-TUS', '(kabuk)', 'alt seritte ana tusu yok');
    else {
      Ekran.sifirla();
      Ekran.ac('araclar');
      anaTus.click();
      if (Ekran.adi() !== 'home') {
        ek('ANA-TUS', '(kabuk)', 'ilk dokunus ana menuye gitmedi -> ' + Ekran.adi());
      }
      if (anaTus.disabled) {
        ek('ANA-TUS', '(kabuk)', 'ana menude tus sonuk — oysa donulecek ekran var');
      }
      anaTus.click();
      if (Ekran.adi() !== 'araclar') {
        ek('ANA-TUS', '(kabuk)', 'ikinci dokunus onceki ekrana donmedi -> ' + Ekran.adi());
      }

      /* Donus hedefi, ana tusu disinda her gezinmede DUSMELI. */
      Ekran.sifirla();
      Ekran.ac('araclar');
      anaTus.click();                 /* home; donus hedefi = araclar */
      Ekran.ac('musteriler');         /* usta baska bir yoldan gezindi */
      Ekran.ac('home');
      if (!anaTus.disabled) {
        ek('ANA-TUS', '(kabuk)', 'baska yolla gezinildi ama donus hedefi silinmedi');
      }
      Ekran.sifirla();
    }
  }

  /* SON IS: prototipi denetim oncesi haline dondur. Ekranlar da
     yeniden cizilsin ki ekranda kirli veriden kalma bir sey durmasin.

     ⚠️ 06.09 — VERIYI GERI YUKLEMEK YETMIYORDU, DOM'U DA GERI YUKLE.
     K-024.1'de denetimin VERIYI kirletmesini duzeltmistik (derin kopya
     + geri yukleme). Ayni hatanin ikinci yuzu bugun cikti: denetim
     DOM DURUMUNU kirletiyordu.

     Mekanizma: 28. test her akordiyon basligina iki kez basiyor (ac,
     kapat — restore); 30. test ise HER DUGMEYE bir kez basiyor ve
     akordiyon basligi da bir dugme. Net sonuc TEK tiklama: denetim
     akordiyonu ACIK birakiyordu. Bir sonraki kosuda 28. test onu acik
     buluyor, ilk tiklamasi KAPATIYOR ve "dokununca acilmiyor" +
     "aria-expanded guncellenmiyor" diye IKI YANLIS BULGU veriyordu.

     Uzun sure gorunmedi cunku ruhsat ekrani her cizimde alanlarini
     temizliyordu; akordiyonun kabi gizleniyor ve tablo basa donuyordu.
     `ac` kancasi gelince (K-040) ekran durumunu korumaya basladi —
     dogru davranis, gizlenen hatayi ortaya cikardi.

     Akordiyonun BELGELENMIS varsayilani KAPALI (akordiyonHtml
     `hidden` uretir), o yuzden hepsini kapatmak dogru geri yuklemedir. */
  /* ⚠️ KATMAN DA KAPATILIR (07.09). 30. test her dugmeye basiyor ve
     artik dugmelerden biri KATMAN aciyor ("Musteri ne gorecek").
     Denetim onu acik birakinca sonraki kosuda ortu yigini karisiyor
     ve odak tuzagi delik gorunuyordu. K-041.2'nin akordiyonlar icin
     ogrendigi ders, yeni bir ortuye uygulanmamisti. */
  if (typeof Katman !== 'undefined' && Katman.acikMi && Katman.acikMi()) Katman.kapa();

  document.querySelectorAll('.akd-bas').forEach(function (bas) {
    var g = document.getElementById(bas.getAttribute('aria-controls') || '');
    bas.setAttribute('aria-expanded', 'false');
    if (g) g.hidden = true;
  });
  yedegiGeriYukle();
  /* GEZINME YIGINI DA GERI YUKLENIR (14.09). 30. test her dugmeye
     basiyor ve artik dugmelerden biri gezinme durumunu DEGISTIRIYOR
     (ana tusu, donus hedefi). showScreen yigina dokunmuyor; yigini
     ancak sifirla temizler. K-041.2'nin dersi yeni bir yuzeyde. */
  if (typeof Ekran !== 'undefined' && Ekran.sifirla) Ekran.sifirla();
  else showScreen('home');

  console.log('SAGLIK DENETIMI — '+ekranlar.length+' ekran, '+b.length+' bulgu');
  b.forEach(function(x){
console.log('  '+x);});
  return {ekran:ekranlar.length, bulgu:b.length, liste:b,
          olcum: OLCULEBILIR ? 'yapildi' : 'ATLANDI (gorus alani '+window.innerWidth+'px)'};
})()
