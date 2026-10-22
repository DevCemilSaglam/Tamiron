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
    'GIZLI-AMA-GORUNUR': 1, 'DRAWER-ARKA-PLAN': 1, 'KOYU-SIYAH': 1,
    'YAPISKAN-ALT': 1
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
    /* ⚠️ SAYI YETMIYOR, KIMLIK GEREKIYOR (16.09).
       Mesaj yillardir "1 oge" diyordu ve bulgu cikinca hangi ogenin
       sizdigini bulmak icin elle arama gerekiyordu -- bir kez de
       tekrar uretilemedi (yalniz tam kosuda cikiyor). Olcum sonucu,
       duzeltmeye YETECEK kadar konusmali. */
    if (dis.length) {
      ek('DRAWER-ODAK-SIZINTISI', 'drawer', dis.length + ' oge: ' +
        dis.slice(0, 3).map(function (el) {
          return el.tagName.toLowerCase() +
            (el.id ? '#' + el.id : '') +
            (el.className && typeof el.className === 'string'
              ? '.' + el.className.split(/\s+/)[0] : '') +
            ' [' + (el.textContent || '').trim().slice(0, 20) + ']';
        }).join(' · '));
    }
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
  /* ⚠️ 13.10 — BU TEST KURALLARIN YARISINI GÖRMÜYORDU. Desen
     `(^|})sec{govde}` idi: her eşleşme bir önceki kuralın kapanış `}`
     işaretini TÜKETİYOR, arka arkaya iki kuraldan ikincisi atlanıyordu —
     kurallar BİRER ATLAYARAK okunuyordu. Hangi yarının görüleceği sıraya
     bağlıydı: 13.10'da tek bir kural eklenince sıra kaydı ve aylardır
     duran gerçek bir çift tanım (.kalem-list) ortaya çıktı. Yalancı geçen
     test, olmayan testten kötüdür. Artık sınır tüketilmiyor ve seçicinin
     önündeki yorum atılıyor.
     NEGATİF DOĞRULAMA: eski desenle 1 bulgu (rastlantı), yeni desenle
     bütün çiftler; çiftler giderilince 0. */
  var re = /([^{}]*)\{([^{}]*)\}/g, m;
  while ((m = re.exec(temiz))) {
    var sec = m[1].replace(/\/\*[\s\S]*?\*\//g, '').trim(), govde = m[2];
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

  /* 46) SUREC YANLIS ANLATIYOR (14.09.2026)
     Cemil: "Surec mesaj gondermeyle birlikte surec seklinde olsun.
     Teslim edildi, parca bekliyor vb hep arka arkaya olan surec
     seklinde islesin. BUNU IYI KONTROL ET."

     YENI HATA SINIFI: OLMAMIS BIR SEYI OLMUS GIBI ANLATAN SIRA.
     Surec seridi bir ANLATIDIR; yanlis sirali ya da eksik bir anlati,
     yanlis rakamdan daha sinsidir — usta ona bakip "ozeti gondermisim"
     diye dusunur ve musteri beklemede kalir.

     Ilk yazimda TAM BU oldu ve elle kontrolde cikti: teslim edilmis bir
     kaydin sureci "Aracınız hazır gonderilebilir" diyordu, cunku
     oneriler PLAKAYA bakiyordu ve ayni aracin baska bir acik kaydi
     vardi. Arac duzeyinde dogru olan cumle, KAYIT duzeyinde yalan.

     Test dort degismezi olcuyor:
       a) her surec acilisla baslar,
       b) teslim edilmis kayitta "Teslim edildi" VAR, "Teslim bekliyor" YOK
          (ve tersi),
       c) o kayda gonderilmis her mesaj sureçte GORUNUR,
       d) siradaki oneriler kaydin haliyle celismez. */
  if (typeof surecAdimlari === 'function' && typeof kayitSiradakiMesajlar === 'function') {
    KAYITLAR.forEach(function (k) {
      var adimlar = surecAdimlari(k);
      if (!adimlar.length) { ek('SUREC', 'kayit#' + k.id, 'surec bos'); return; }
      if (adimlar[0].ad.indexOf('kayıt açıldı') < 0) {
        ek('SUREC', 'kayit#' + k.id, 'ilk adim acilis degil: ' + adimlar[0].ad);
      }
      var adlar = adimlar.map(function (a) { return a.ad; }).join(' | ');
      if (k.teslimTarihi) {
        if (adlar.indexOf('Teslim edildi') < 0) {
          ek('SUREC', 'kayit#' + k.id, 'teslim edilmis ama surecte teslim yok');
        }
        if (adlar.indexOf('Teslim bekliyor') >= 0) {
          ek('SUREC', 'kayit#' + k.id, 'teslim edilmis ama surec "teslim bekliyor" diyor');
        }
      } else if (adlar.indexOf('Teslim bekliyor') < 0) {
        ek('SUREC', 'kayit#' + k.id, 'acik kayit ama surecte "teslim bekliyor" yok');
      }

      /* (c) Mesaj sayisi tutuyor mu. */
      var mesajSay = MESAJLAR.filter(function (m) { return m.kayit === k.id; }).length;
      var surecMesaj = adimlar.filter(function (a) { return a.mesaj; }).length;
      if (mesajSay !== surecMesaj) {
        ek('SUREC', 'kayit#' + k.id, 'gonderilen ' + mesajSay + ' mesajin ' +
           surecMesaj + ' tanesi surecte');
      }

      /* (d) Oneriler kaydin haliyle celisiyor mu. */
      var sirada = kayitSiradakiMesajlar(k).map(function (s) { return s.k; });
      if (k.teslimTarihi) {
        ['ozet', 'bekliyor', 'hazir'].forEach(function (yanlis) {
          if (sirada.indexOf(yanlis) >= 0) {
            ek('SUREC', 'kayit#' + k.id,
               'teslim edilmis kayitta "' + yanlis + '" oneriliyor');
          }
        });
      } else if (sirada.indexOf('teslim') >= 0 || sirada.indexOf('hatirlatma') >= 0) {
        ek('SUREC', 'kayit#' + k.id, 'acik kayitta teslim/hatirlatma oneriliyor');
      }
      if (!k.parcaBekliyor && sirada.indexOf('bekliyor') >= 0) {
        ek('SUREC', 'kayit#' + k.id, 'parca beklemiyorken "parca bekliyoruz" oneriliyor');
      }
    });
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

    /* 47) IZ KAYDI — YAZIYOR MU, VE GIRILEN DEGER ICINDE MI (16.09.2026)
     Cemil: "Kullanici problem yasadiginda arayacagi kisi olacagim ve
     ona neyi yanlis yaptigini direkt soyleyebilmeliyim."

     YENI HATA SINIFI: IZ VAR AMA KOR. Iki turlu kor olur ve ikisi de
     ayni sonucu verir -- telefondaki ustaya yardim edilemez:

       a) BOSLUKTAN: girilen deger ize dusmez. Ekran gecisleri gorunur,
          dokunuslar gorunur, ama "su alana sunu yazmissin" denemez.
          Ilk surumde tam boyleydi: bir elek degerleri bicimce
          eliyordu. Cemil kurali bozdu (K-053) -- musteri verisi zaten
          tutuluyor, elek yalnizca DESTEGI kor ediyordu.
       b) FAZLALIKTAN: tek bir uzun not halka tamponu doldurur ve
          onceki adimlari siler. Usta "bir seye bastim, bozuldu" der;
          izde yalnizca o notun kendisi vardir.

     Bu test ikisini de olcuyor: deger DUSUYOR mu, ve KIRPILIYOR mu. */
  if (typeof Iz === 'undefined') {
    ek('IZ-KAYDI', '(kabuk)', 'Iz modulu yok — uzaktan hicbir sey gorunmez');
  } else {
    Iz.sil();
    var izTus = null;
    if (typeof Ekran !== 'undefined' && Ekran.ac) {
      Ekran.sifirla();
      Ekran.ac('araclar');
      Ekran.ac('musteriler');
      izTus = document.querySelector('.botbar [data-nav="ana"]');
      if (izTus) izTus.click();
    }
    var o = Iz.ozet();
    if (!o.ekran) ek('IZ-KAYDI', '(kabuk)', 'ekran gecisleri ize dusmuyor');
    if (izTus && !o.dokunus) ek('IZ-KAYDI', '(kabuk)', 'dokunuslar ize dusmuyor');

    Iz.hata('deneme', { name: 'TypeError' });
    if (!Iz.ozet().hata) ek('IZ-KAYDI', '(kabuk)', 'hatalar ize dusmuyor');

    /* Suphe: ayni ekrana kisa surede uc kez donmek */
    Iz.sil();
    if (typeof Ekran !== 'undefined' && Ekran.ac) {
      Ekran.ac('araclar'); Ekran.ac('home');
      Ekran.ac('araclar'); Ekran.ac('home');
      Ekran.ac('araclar');
      if (!Iz.ozet().suphe) {
        ek('IZ-KAYDI', '(kabuk)', 'ayni ekrana uc kez donuldu, suphe yazilmadi');
      }
    }

    /* ---- ASIL OLCUM 1: girilen deger ize dusuyor mu ---- */
    Iz.sil();
    var alanDeneme = document.querySelector('.screen input[type="text"], .screen input:not([type])');
    if (alanDeneme) {
      var eski = alanDeneme.value;
      alanDeneme.value = '34 ABC 123';
      alanDeneme.dispatchEvent(new Event('change', { bubbles: true }));
      alanDeneme.value = eski;
      var d = JSON.stringify(Iz.dok());
      if (d.indexOf('34 ABC 123') < 0) {
        ek('IZ-KAYDI', '(kabuk)',
           'girilen deger ize DUSMUYOR — destek "ne yazdin" diye sormak zorunda kalir');
      }
      if (!Iz.ozet().alan) ek('IZ-KAYDI', '(kabuk)', 'alan degisikligi sayilmiyor');
    }

    /* ---- ASIL OLCUM 1b: MODELIN OKUDUGU DUZELTILINCE YAZILIYOR MU ----
       Urunun savunmasinin maddesi bu (SAVUNMA.md): okumayi API veriyor,
       DUZELTMEYI dukkan. Duzeltme yazilmazsa usta yanlisi duzeltir,
       duzeltme ekranda kalir ve biz asla ogrenmeyiz -- yani birikim
       OLMAZ. Kaybi sessiz: her sey calisiyor gorunur. */
    Iz.sil();
    Iz.okuma('deneme', { denemeAlani: 'model boyle okudu' });
    Iz.alan('denemeAlani', 'usta boyle duzeltti');
    var duz = Iz.dok().filter(function (x) { return x.tur === 'duzeltme'; })[0];
    if (!duz) {
      ek('IZ-KAYDI', '(kabuk)',
         'modelin okudugu duzeltildi ama "duzeltme" yazilmadi — birikim olmuyor');
    } else if (!duz.ek || duz.ek.model !== 'model boyle okudu') {
      ek('IZ-KAYDI', '(kabuk)',
         'duzeltmede modelin dedigi yok — neyi yanlis okudugu ogrenilemez');
    }

    /* ---- ASIL OLCUM 2: uzun deger kirpiliyor mu ---- */
    Iz.sil();
    var uzun = new Array(600).join('x');
    Iz.alan('denemeAlani', uzun);
    var yazilan = (Iz.dok()[0] || {}).ek || {};
    if (String(yazilan.deger || '').length > Iz.KIRP + 1) {
      ek('IZ-KAYDI', '(kabuk)',
         'uzun deger kirpilmiyor (' + String(yazilan.deger).length +
         ' harf) — tek not halka tamponu doldurup onceki adimlari siler');
    }

    /* ---- ASIL OLCUM 3: halka tampon tavani tutuyor mu ---- */
    Iz.sil();
    for (var iz_i = 0; iz_i < 400; iz_i++) Iz.yaz('deneme', 'd' + iz_i);
    if (Iz.dok().length > 300) {
      ek('IZ-KAYDI', '(kabuk)', 'halka tampon tavani asiliyor: ' + Iz.dok().length);
    }

    Iz.sil();
    if (typeof Ekran !== 'undefined' && Ekran.sifirla) Ekran.sifirla();
  }

    /* 48) ALIS FIYATI MUSTERI YUZEYINE SIZIYOR MU (16.09.2026)
     Cemil: "Bunu musteri icin degil kar zarar muhasebe icin
     yapmaliyiz."

     YENI HATA SINIFI: IC RAKAM DIS YUZEYE DUSUYOR. `alis` BIZIM
     parcaciya odedigimiz; `fiyat` musteriye soyledigimiz. Ikisi ayni
     satirda, yan yana duruyor -- bir sablon yanlis alani okursa
     musteri dukkanin maliyetini gorur. Bu:
       * pazarlik gucunu bitirir ("demek 1450'ye alip 1900'e satiyorsun"),
       * ve HATA VERMEZ, ekran duzgun cizilir.

     Kural bugune kadar YORUMDA yaziliydi (98-ozet.html, VERI-MODELI
     §3). Tur 4'un dersi: belgede duran bir ders, uygulanmamissa
     ogrenilmemistir. Burada uygulaniyor.

     Olcum yontemi: tohum verideki alis rakamlarinin HICBIRI musteri
     yuzeylerinin metninde gecmemeli. Rakamlar satis fiyatlariyla
     karismasin diye yalniz alis-a OZGU olanlar araniyor. */
  (function () {
    if (typeof KAYITLAR === 'undefined' || typeof Ekran === 'undefined') return;

    /* Yalniz alis'a ozgu rakamlar: ayni sayi bir yerde satis fiyati
       olarak da geciyorsa eslesme tesaduf olur, bulgu da yanlis. */
    var alislar = {}, satislar = {};
    KAYITLAR.forEach(function (k) {
      (k.satirlar || []).forEach(function (s) {
        if (s.alis) alislar[s.alis] = true;
        if (s.fiyat) satislar[s.fiyat] = true;
      });
    });
    var aranacak = Object.keys(alislar).filter(function (a) {
      return !satislar[a] && Number(a) >= 100;   /* kucuk sayi her yerde gecer */
    });
    if (!aranacak.length) return;

    /* ⚠️ BAGLAM KURULMADAN OLCULMEZ (16.09).
       Ilk yazilisinda bu test ekranlari OYLECE aciyordu: ozet ekrani
       "Ozet secilmedi" diyor, teslim ekrani bos geliyor ve testte
       aranacak metin HIC OLUSMUYORDU. Sonuc: bulgu yok -- ama olcum
       de yok. Yalanci gecen bir test, olmayan testten KOTUDUR: korur
       sanilir. Alis rakami TASIYAN bir kayit secilip oyle aciliyor. */
    var hedef = null;
    KAYITLAR.forEach(function (k) {
      if (!hedef && (k.satirlar || []).some(function (s) { return s.alis >= 100; })) hedef = k;
    });
    if (!hedef) return;

    var yuzeyler = ['teslim', 'ozet', 'mesaj'];
    Ekran.sifirla();
    yuzeyler.forEach(function (ad) {
      var el = document.getElementById('screen-' + ad);
      if (!el) return;
      if (typeof ACIK_KAYIT !== 'undefined') ACIK_KAYIT = hedef.id;
      if (typeof OZET_KAYIT !== 'undefined') OZET_KAYIT = hedef.id;
      if (typeof ACIK_PLAKA !== 'undefined') ACIK_PLAKA = hedef.plaka;
      Ekran.ac(ad);
      var metin = el.textContent || '';
      /* Baglam gercekten kuruldu mu? Kurulmadiysa test yine yalanci
         gecer; o yuzden BOS YUZEY DE BULGUDUR. */
      if (metin.replace(/\s+/g, ' ').trim().length < 40) {
        ek('ALIS-SIZINTI', '(' + ad + ')',
           'yuzey bos acildi — test olcmeden geciyor, baglam kurulamadi');
        return;
      }
      aranacak.forEach(function (a) {
        /* Rakamin BASINA ve SONUNA rakam gelmemeli: 1450, 11450'nin
           icinde de gecer ve yanlis bulgu verir. */
        var kalip = new RegExp('(^|[^0-9])' + a + '([^0-9]|$)');
        if (kalip.test(metin.replace(/[.,\s]/g, ''))
            || kalip.test(metin)) {
          ek('ALIS-SIZINTI', '(' + ad + ')',
             'alis rakami ' + a + ' musteri yuzeyinde gorunuyor');
        }
      });
    });
    if (typeof ACIK_KAYIT !== 'undefined') ACIK_KAYIT = null;
    if (typeof OZET_KAYIT !== 'undefined') OZET_KAYIT = null;
    Ekran.sifirla();
  })();

    /* 49) SATIR DUZENLEME — EKLE, TIP CEVIR, YAZILANI KORU (16.09.2026)
     Cemil: "Servis kayit ekrani istedigim gibi degil. Orada parca
     ekle, iscilik ekle duzenle vb daha kolay olmali."

     YENI HATA SINIFI: KOLAYLASTIRMA VERI SILIYOR. Duzenlemeyi
     kolaylastiran her dokunus, ustanin YAZDIGI seyin uzerinden gecme
     riski tasir. Uc yerden bozulur:

       a) ekleme sonucu GORUNMEZ -- satir tablonun altinda belirir,
          odak gitmez, usta bastigi dugmenin sonucunu ARAR (K-032),
       b) tip cevirme AD ve TUTARI silerse, "kolaylastirma" ustaya
          yazdigini yeniden yazdirir -- eskisinden kotudur,
       c) parcadan iscilige cevrilen satirda parcaya OZEL alanlar
          (parcaci fiyati, kim getirdi) kalirsa, iscilik satirinda
          "parcaci 1450 TL" yazar ve toplam yanlis anlasilir.

     (b) ozellikle sinsi: ekranda dogru gorunur, kayip ancak usta
     tutari tekrar yazarken anlasilir. */
  (function () {
    if (typeof KAYITLAR === 'undefined' || typeof Ekran === 'undefined') return;
    var k = KAYITLAR[0];
    if (!k) return;
    var yedek = JSON.parse(JSON.stringify(k.satirlar || []));

    Ekran.sifirla();
    if (typeof ACIK_KAYIT !== 'undefined') ACIK_KAYIT = k.id;
    Ekran.ac('kayit');

    /* Kusur burada yasandi (16.09): "Parca ekle" adimlarin ustundeydi,
       satir tablosu 2. adimin icinde; 1. adimda basilinca satir
       GORUNMEDEN ekleniyordu. 12.10'da adimlar kalkti, 13.10'da "Ekle"
       bir PANEL acti (K-058 — Cemil: C'deki gibi). Soru ayni: basilan
       dugmenin sonucu GORUNUYOR mu. Artik iki adimda soruluyor:
         - Ekle -> panel aciliyor mu, imlec AD alaninda mi,
         - ad bosken Ekle satir EKLEMEMELI ve panel acik kalmali,
         - ad + tutar yazilip Ekle -> satir tabloda GORUNUYOR mu. */
    var ekleTus = document.getElementById('kParcaEkle');
    if (!ekleTus) {
      ek('SATIR-DUZEN', '(kayit)', 'ekle dugmesi yok');
    } else {
      var once = k.satirlar.length;
      ekleTus.click();
      var adAlan = document.getElementById('kYeniAd');
      if (typeof Katman === 'undefined' || !Katman.acikMi() || !adAlan) {
        ek('SATIR-DUZEN', '(kayit)', 'ekle paneli acilmadi');
      } else {
        if (document.activeElement !== adAlan) {
          ek('SATIR-DUZEN', '(kayit)', 'panelde imlec AD alaninda degil — usta once nereye yazacagini arar');
        }
        /* bos ad: eklememeli, konusmali */
        document.getElementById('kYeniEkle').click();
        if (k.satirlar.length !== once || !Katman.acikMi()) {
          ek('SATIR-DUZEN', '(kayit)', 'ad bosken satir eklendi ya da panel kapandi');
          /* ⚠️ TEST BURADA COKMEZ (13.10, negatif testte coktu): panel
             kapandiysa alanlari yok — kayit geri alinir, panel yeniden
             acilir, olcum devam eder. Bulgu veren test, cokup BUTUN
             denetimi dusuren testten iyidir. */
          k.satirlar.length = once;
          if (Katman.acikMi()) Katman.kapa();
          ekleTus.click();
          adAlan = document.getElementById('kYeniAd');
        }
        if (adAlan && document.getElementById('kYeniTutar')) {
          adAlan.value = 'deneme parca';
          document.getElementById('kYeniTutar').value = '777';
          document.getElementById('kYeniEkle').click();
        }
      }
      if (k.satirlar.length !== once + 1) {
        ek('SATIR-DUZEN', '(kayit)', 'panelden ekle satir eklemedi');
      } else {
        if (typeof Katman !== 'undefined' && Katman.acikMi()) {
          ek('SATIR-DUZEN', '(kayit)', 'ekledikten sonra panel acik kaldi');
          Katman.kapa();
        }
        /* (a) sonuc gorunuyor mu */
        var giris = document.querySelector('#kSatirlar [data-satir-ad="' + once + '"]');
        if (!giris || !giris.offsetParent) {
          ek('SATIR-DUZEN', '(kayit)', 'eklenen satir tabloda GORUNMUYOR');
        }
        if (k.satirlar[once].ad !== 'deneme parca' || k.satirlar[once].fiyat !== 777) {
          ek('SATIR-DUZEN', '(kayit)', 'panelde yazilan ad/tutar satira gecmedi');
        }

        /* (b) tip cevrilince yazilan korunuyor mu */
        var yeni = k.satirlar[once];
        yeni.ad = 'deneme parca'; yeni.fiyat = 777; yeni.alis = 555;
        yeni.kimGetirdi = 'tamirci';
        Ekran.tazele();
        var tipTus = document.querySelector('[data-satir-tip="' + once + '"]');
        if (!tipTus) {
          ek('SATIR-DUZEN', '(kayit)',
             'satir tipi degistirilemiyor — yanlis tip secen usta satiri SILIP yeniden ekler');
        } else {
          tipTus.click();
          var sonra = k.satirlar[once];
          if (sonra.tip !== 'iscilik') {
            ek('SATIR-DUZEN', '(kayit)', 'tip dugmesi tipi cevirmedi');
          }
          if (sonra.ad !== 'deneme parca' || sonra.fiyat !== 777) {
            ek('SATIR-DUZEN', '(kayit)',
               'tip cevrilince ad/tutar kayboldu — usta yazdigini tekrar yazar');
          }
          /* (c) parcaya ozel alanlar temizlendi mi */
          if (sonra.alis || sonra.kimGetirdi) {
            ek('SATIR-DUZEN', '(kayit)',
               'iscilige cevrildi ama parcaci alanlari duruyor (alis/kimGetirdi)');
          }
          /* Geri cevrilince parca alani yeniden calisiyor mu */
          var tipTus2 = document.querySelector('[data-satir-tip="' + once + '"]');
          if (tipTus2) {
            tipTus2.click();
            if (k.satirlar[once].tip !== 'parca') {
              ek('SATIR-DUZEN', '(kayit)', 'tip geri cevrilemiyor');
            }
          }
        }
      }
    }

    /* Denetim veriyi kirletti; geri birakiyor (K-024.1). */
    k.satirlar = yedek;
    if (typeof ACIK_KAYIT !== 'undefined') ACIK_KAYIT = null;
    Ekran.sifirla();
  })();

    /* 50) BAGLAM DISI IS — "BUNU MU DEMEK ISTEDIN" (16.09.2026)
     Cemil: "Kullanici farkli ekranda farkli bir islem yapmak
     isteyebilir. Google aramada oldugu gibi bunu mu demek istediniz
     diye yonlendirebilelim."

     YENI HATA SINIFI: BAGLAM DALI COPLUK OLUYOR. Bir ekranin kendi
     sesli dali "geri kalan her sey" dalina donusunce, o ekranda
     soylenen HER cumle o ekranin isine cevriliyor. Yasanan hali:
     kayit acikken

       "servistekileri goster"        -> adi bu olan bir PARCA
       "bugun ne kadar tahsilat"      -> adi bu olan bir PARCA
       "2400 lira elektrik faturasi"  -> adi bu olan bir PARCA

     Usta onaylarsa kayda sacma bir satir giriyor; onaylamazsa
     istedigi is HIC yapilmiyor. Ikisi de sessiz -- ekran duzgun
     cizilir, hata verilmez.

     Olculen uc sey:
       a) taninan bir is, baglam icindeyken de TANINIYOR mu,
       b) baglamin kendi isi ONERI olarak duruyor mu (soru, eylem degil),
       c) baglama AIT bir cumle bosuna yonlendirilmiyor mu.

     (c) onemli: her cumleye oneri koymak, oneriyi gurultuye cevirir. */
  if (typeof window.SES_SOYLE === 'function' && typeof KAYITLAR !== 'undefined') {
    var acikK = null;
    KAYITLAR.forEach(function (k) { if (!acikK && !k.teslim) acikK = k; });
    if (acikK) {
      var oncekiNiyet = window.SESLI_NIYET, oncekiKayit = window.ACIK_KAYIT;

      var sesDene = function (cumle) {
        Ekran.sifirla();
        window.ACIK_KAYIT = acikK.id;
        showScreen('ses');
        window.SESLI_NIYET = 'kayit-icinde';
        window.SES_SOYLE(cumle);
        return {
          durum: (document.getElementById('sesDurum') || {}).textContent || '',
          oneri: document.getElementById('sesBaskaIs')
        };
      };

      /* (a) + (b) — baglam disi bir is */
      var disi = sesDene('bugun ne kadar tahsilat yaptim');
      if (disi.durum.indexOf('tahsilat') < 0) {
        ek('SES-BAGLAM', 'ses',
           'kayit acikken "tahsilat" komutu taninmadi -> "' + disi.durum + '"');
      }
      if (!disi.oneri) {
        ek('SES-BAGLAM', 'ses',
           'baglam disi is taninip baglamin kendi isi ONERI olarak sunulmadi');
      } else {
        /* Oneri bir SORU olmali: dokunmadan once islem degismemeli,
           dokununca da EKRAN DEGISMEMELI (A-M27'nin kurali). */
        var satirOnce = acikK.satirlar.length;
        disi.oneri.click();
        var sonra = (document.getElementById('sesDurum') || {}).textContent || '';
        if (sonra.indexOf('satir') < 0 && sonra.indexOf('satır') < 0) {
          ek('SES-BAGLAM', 'ses', 'oneriye dokununca islem degismedi -> "' + sonra + '"');
        }
        if (acikK.satirlar.length !== satirOnce) {
          ek('SES-BAGLAM', 'ses', 'oneriye dokununca SATIR EKLENDI — soru degil, eylem olmus');
        }
        if (Ekran.adi() !== 'ses') {
          ek('SES-BAGLAM', 'ses', 'oneriye dokununca ekran degisti: ' + Ekran.adi());
        }
        /* Geri donulebilmeli: yeni oneri eski isi gostermeli. */
        if (!document.getElementById('sesBaskaIs')) {
          ek('SES-BAGLAM', 'ses', 'oneriden geri donus yok — usta cumleyi tekrar soyler');
        }
      }

      /* (d) BAGLAM DISI IS BITINCE KAYDA DONUS (7.4)
         Usta kaydin icindeyken baska bir is yapabiliyor; is bitince
         BASKA BIR EKRANDA kaliyor ve kaydi bulmak icin listeye gidip
         aramasi gerekiyordu. Kayit hala ACIK -- eksik olan kapiydi. */
      window.SES_DONUS = null;
      var disi2 = sesDene('2400 lira elektrik faturasi');
      var onayD = document.getElementById('sesOnayla');
      if (!onayD) {
        ek('SES-BAGLAM', 'ses', 'baglam disi iste onay dugmesi yok');
      } else {
        onayD.click();
        if (!window.SES_DONUS || window.SES_DONUS.kayit !== acikK.id) {
          ek('SES-BAGLAM', 'ses',
             'baglam disi is bitti ama KAYDA DONUS teklif edilmiyor — usta ortada kalir');
        } else {
          window.SES_DONUS.ac();
          if (Ekran.adi() !== 'kayit' || window.ACIK_KAYIT !== acikK.id) {
            ek('SES-BAGLAM', 'ses', 'donus teklifi kayda goturmuyor -> ' + Ekran.adi());
          }
        }
      }

      /* (c) baglamin KENDI isi bosuna yonlendirilmemeli */
      var ici = sesDene('balata 450 lira');
      if (ici.oneri) {
        ek('SES-BAGLAM', 'ses',
           'baglama ait cumleye de oneri cikti — oneri gurultuye doner');
      }

      /* ⚠️ BALONU DA GERI BIRAK (K-024.1).
         (d) gercek bir isi ONAYLIYOR ve o isin kendi balonu icinde bir
         "Geri al" DUGMESI var. Balon acik kalinca cekmece testi onu
         "odak sizintisi" sayip bulgu veriyordu -- yani benim testim,
         baska bir testi yalanci kirmiziya dusuruyordu. Veriyi geri
         yuklemek yetmiyor; DOM da geri birakilir. */
      var toast = document.getElementById('toast');
      if (toast) { toast.classList.remove('show'); toast.innerHTML = ''; }

      window.SESLI_NIYET = oncekiNiyet;
      window.ACIK_KAYIT = oncekiKayit;
      Ekran.sifirla();
    }
  }

  /* 51) ISLEM SEMASI EKSIK — TANINAN IS YAPILAMIYOR (12.10.2026 · Tur 8.3)
     Ses artik isi TANIMIYOR; isin sahibi ekran bir sema bildiriyor
     (Islem.tanim) ve ses o semayi dolduruyor. Tanima, onay ekrani ve
     uygulama UC AYRI parcada duruyor: tani() bir nesne uretir, ozet()
     o nesnenin semasini arar, uygula() semanin uygula'sini cagirir.

     YASANAN: semayi kaydeden satir unutulmustu (SEMALAR[tur] = s).
     Taniyicilar ayri bir listede durdugu icin CALISTI -- cumle dogru
     anlasildi, nesne dogru doldu -- ama onay ekrani semayi bulamadi.
     Taniyan yol ile yapan yol AYRISABILIYOR ve hicbir test bunu
     gormuyordu; esdegerlik olcumu yakaladi, denetim degil.

     Olculen: ekranin onerdigi her cumle, her baglamda --
       a) uretilen isin (ve onerinin) semasi VAR mi,
       b) onay ekrani bos ya da "undefined" yazmadan ciziliyor mu,
       c) semanin uygula'si var mi,
       d) modele tarif (ISLEM_SEMALARI) her semayi veriyor mu.
     NEGATIF DOGRULAMA (12.10): `SEMALAR[tur] = s` satiri silindi ->
     bulgu; geri konunca 0. */
  if (typeof Islem !== 'undefined' && typeof window.SES_COZUMLE === 'function' &&
      typeof window.SES_BAGLAMLAR === 'function') {
    var semaYedek = { niyet: window.SESLI_NIYET, kayit: window.ACIK_KAYIT };
    var semaKayit = null;
    KAYITLAR.forEach(function (k) {
      if (!semaKayit && !k.teslimTarihi && (k.satirlar || []).length > 1) semaKayit = k;
    });
    var semaGorulen = {};
    var semaBak = function (o, baglam, cumle) {
      if (!o || o.tur === 'plakasiz') return;
      var nerede = baglam + ' "' + cumle + '"';
      if (!Islem.sema(o.tur)) {
        ek('ISLEM-SEMA', 'ses', nerede + ' -> "' + o.tur + '" taniniyor ama SEMASI YOK');
        return;
      }
      semaGorulen[o.tur] = 1;
      var oz = null;
      try { oz = Islem.ozet(o); } catch (e) {
        ek('ISLEM-SEMA', 'ses', nerede + ' -> onay ekrani patladi: ' + e.message); return;
      }
      if (!oz || !oz.baslik || !oz.eylem) {
        ek('ISLEM-SEMA', 'ses', nerede + ' -> onay ekraninda baslik/eylem yok (' + o.tur + ')');
        return;
      }
      oz.satirlar.forEach(function (s) {
        var m = String(s[1]);
        if (s[1] === undefined || s[1] === null || m === '' ||
            m.indexOf('undefined') >= 0 || m.indexOf('[object') >= 0 || m.indexOf('NaN') >= 0) {
          ek('ISLEM-SEMA', 'ses', nerede + ' -> "' + s[0] + '" satiri bos/bozuk: ' + m);
        }
      });
      if (typeof Islem.sema(o.tur).uygula !== 'function') {
        ek('ISLEM-SEMA', 'ses', nerede + ' -> "' + o.tur + '" onaylanir ama YAPILAMAZ (uygula yok)');
      }
    };
    var semaOrnek = window.SES_BAGLAMLAR();
    var semaCumleler = [];
    Object.keys(semaOrnek).forEach(function (b) { semaCumleler = semaCumleler.concat(semaOrnek[b]); });
    Object.keys(semaOrnek).forEach(function (baglam) {
      semaCumleler.forEach(function (cumle) {
        window.SESLI_NIYET = baglam;
        window.ACIK_KAYIT = (baglam === 'kayit-icinde' && semaKayit) ? semaKayit.id : null;
        var o = null;
        try { o = window.SES_COZUMLE(cumle); } catch (e) {
          ek('ISLEM-SEMA', 'ses', baglam + ' "' + cumle + '" -> cozumleyici patladi: ' + e.message);
          return;
        }
        semaBak(o, baglam, cumle);
        if (o && o.baskaIslem) semaBak(o.baskaIslem, baglam + ' (oneri)', cumle);
      });
    });
    window.SESLI_NIYET = semaYedek.niyet;
    window.ACIK_KAYIT = semaYedek.kayit;

    /* Bos kalan olcum yalanci gecer (48. testin dersi): ornekler hic
       is uretmediyse bunu soyle. */
    if (!Object.keys(semaGorulen).length) {
      ek('ISLEM-SEMA', 'ses', 'ornek cumlelerin hicbiri bir is uretmedi -- olcum yapilamadi');
    }

    /* (d) modele tarif: her sema, bir kez, alanlariyla */
    var tarif = typeof window.ISLEM_SEMALARI === 'function' ? window.ISLEM_SEMALARI() : [];
    var turler = Islem.turler();
    if (!turler.length || tarif.length !== turler.length) {
      ek('ISLEM-SEMA', 'ses', 'modele tarif ' + tarif.length + ' sema veriyor, kayitli ' +
         turler.length);
    }
    tarif.forEach(function (t) {
      var ozl = (t.input_schema || {}).properties || {};
      Object.keys(ozl).forEach(function (a) {
        if (ozl[a].enum && !ozl[a].enum.length) {
          ek('ISLEM-SEMA', 'ses', t.name + '.' + a + ' secim alani ama secenegi yok');
        }
      });
    });
  }

  /* 52) BILINMEYEN ALIS KARA GIRIYOR (12.10.2026 · Tur 8.2 · K-055.2)
     `alis: null` = bilinmiyor, `alis: 0` = bedava. Eskiden ikisi de
     `s.alis || 0` ile sifir sayiliyordu: alisi yazilmamis 2.100 TL'lik
     parca "2.100 TL kar" oluyordu. Tohum verinin her parcasinda alis
     dolu oldugu icin kusur HIC gorunmedi -- olcum aletinin kendi verisi
     olctugu eksigi gizliyordu (MALIYET-VE-KAR §1).

     Olculen:
       a) hesap: bilinmeyen alisli satir SATISIYLA BIRLIKTE disarida;
          sifir alis ise bedava (karda);
       b) ayar gercekten ekrani degistiriyor: kapaliyken alis alani
          YOK, acikken tamircinin aldigi parcada VAR, musteri parcasinda YOK;
       c) giris: bos birakilan alan null kaliyor, "0" 0 oluyor --
          tutar alanlarinin `|| 0` aliskanligi buraya sizmasin.
     NEGATIF DOGRULAMA (12.10): isKari eski `s.alis || 0` haline
     cevrildi -> bulgu; geri konunca 0. */
  if (typeof isKari === 'function' && typeof DUKKAN !== 'undefined' &&
      typeof KAYITLAR !== 'undefined') {
    /* (a) yapay kayitlarla -- tohum veriye bagli kalmasin */
    var alisK = function (alis) {
      return { satirlar: [
        { tip: 'parca', ad: 'deneme', fiyat: 2100, alis: alis, kimGetirdi: 'tamirci' },
        { tip: 'iscilik', ad: 'iscilik', fiyat: 700, alis: null, kimGetirdi: '' } ] };
    };
    var kBilinmeyen = isKari(alisK(null));
    var kBedava = isKari(alisK(0));
    var kBilinen = isKari(alisK(1600));
    if (kBilinmeyen.fark !== 700) {
      ek('ALIS-BILINMIYOR', 'muhasebe', 'alisi bilinmeyen parca kara girdi: fark ' +
         kBilinmeyen.fark + ' (beklenen 700 -- yalniz iscilik)');
    }
    if (kBilinmeyen.bilinmeyen !== 1) {
      ek('ALIS-BILINMIYOR', 'muhasebe', 'hesap disi kalan satir SAYILMIYOR -- ekran soyleyemez');
    }
    if (kBedava.fark !== 2800) {
      ek('ALIS-BILINMIYOR', 'muhasebe', 'sifir alis (bedava) bilinmiyor sayildi: fark ' + kBedava.fark);
    }
    if (kBilinen.fark !== 1200) {
      ek('ALIS-BILINMIYOR', 'muhasebe', 'bilinen alis yanlis dusuldu: fark ' + kBilinen.fark);
    }

    /* (b) + (c) kayit ekraninda -- tamircinin aldigi parcasi olan acik kayit */
    var alisKayit = null;
    KAYITLAR.forEach(function (k) {
      if (alisKayit || k.teslimTarihi) return;
      (k.satirlar || []).forEach(function (s) {
        if (s.tip === 'parca' && s.kimGetirdi === 'tamirci') alisKayit = k;
      });
    });
    if (!alisKayit) {
      ek('ALIS-BILINMIYOR', 'kayit', 'tamirci parcasi olan acik kayit yok -- ekran olculemedi');
    } else {
      var alisOnce = { sor: DUKKAN.alisSor, kayit: window.ACIK_KAYIT,
                       satir: JSON.parse(JSON.stringify(alisKayit.satirlar)) };
      window.ACIK_KAYIT = alisKayit.id;
      DUKKAN.alisSor = false;
      showScreen('kayit');
      if (document.querySelector('#screen-kayit [data-satir-alis]')) {
        ek('ALIS-BILINMIYOR', 'kayit', 'ayar KAPALI ama alis alani gorunuyor -- sormayan dukkan gormemeli');
      }
      DUKKAN.alisSor = true;
      showScreen('kayit');
      var alisAlan = document.querySelector('#screen-kayit [data-satir-alis]');
      if (!alisAlan) {
        ek('ALIS-BILINMIYOR', 'kayit', 'ayar ACIK ama alis alani cikmadi -- ayar ekrani degistirmiyor');
      } else {
        var alisNo = parseInt(alisAlan.getAttribute('data-satir-alis'), 10);
        var alisSatir = alisKayit.satirlar[alisNo];
        if (alisSatir.kimGetirdi === 'musteri') {
          ek('ALIS-BILINMIYOR', 'kayit', 'musterinin getirdigi parcada alis soruluyor');
        }
        alisAlan.value = '';
        alisAlan.dispatchEvent(new Event('input', { bubbles: true }));
        if (alisSatir.alis !== null) {
          ek('ALIS-BILINMIYOR', 'kayit', 'bos birakilan alis ' + JSON.stringify(alisSatir.alis) +
             ' oldu -- null (bilinmiyor) kalmaliydi');
        }
        alisAlan.value = '0';
        alisAlan.dispatchEvent(new Event('input', { bubbles: true }));
        if (alisSatir.alis !== 0) {
          ek('ALIS-BILINMIYOR', 'kayit', '"0" yazilan alis ' + JSON.stringify(alisSatir.alis) +
             ' oldu -- 0 (bedava) olmaliydi');
        }
      }
      /* musteri parcasinda alis alani olmamali */
      document.querySelectorAll('#screen-kayit [data-satir-alis]').forEach(function (el) {
        var s = alisKayit.satirlar[parseInt(el.getAttribute('data-satir-alis'), 10)];
        if (s && s.kimGetirdi === 'musteri') {
          ek('ALIS-BILINMIYOR', 'kayit', 'musteri parcasinda alis alani var');
        }
      });
      alisKayit.satirlar = alisOnce.satir;
      DUKKAN.alisSor = alisOnce.sor;
      /* DOM DA GERI BIRAKILIR (K-041.2): kayit ekrani "ayar acik"
         haliyle cizili kalmasin -- eski ayarla bir kez daha ciz. */
      showScreen('kayit');
      window.ACIK_KAYIT = alisOnce.kayit;
      Ekran.sifirla();
    }
  }

  /* 53) YAPISKAN ALT OGE DIBE OTURMUYOR (12.10.2026 · K-058)
     Kayit ekranina alttan yapisan bir eylem cubugu geldi ve ilk yazista
     dipten 26 px YUKARIDA duruyordu: `position: sticky` kaydirma kabinin
     IC DOLGUSUNU hesaba katiyor, ekranin alt dolgusu cubugu yukari
     itiyordu. Altindan icerik akiyordu; ne tasma ne kirpma testi gorur,
     cunku hicbir sey tasmiyor ya da kesilmiyor. Olcum yakaladi, test
     degil.

     Olculen: her ekranda `sticky` + `bottom` verilmis oge, icerik kabi
     doldurdugunda kabin ALT KENARINA oturuyor mu.
     NEGATIF DOGRULAMA (12.10): kayit ekraninin `padding-bottom: 0`
     duzeltmesi geri alindi -> bulgu; geri konunca 0. */
  if (OLCULEBILIR) {
    ekranlar.forEach(function (a) {
      var kok = document.getElementById('screen-' + a);
      if (!kok) return;
      var yapiskan = Array.prototype.filter.call(kok.querySelectorAll('*'), function (el) {
        var cs = getComputedStyle(el);
        return cs.position === 'sticky' && cs.bottom !== 'auto';
      });
      if (!yapiskan.length) return;
      showScreen(a);
      if (kok.scrollHeight <= kok.clientHeight + 1) return; /* kap dolmadiysa yapisma olmaz */
      kok.scrollTop = 0;
      var kAlt = kok.getBoundingClientRect().bottom;
      yapiskan.forEach(function (el) {
        var fark = Math.round(kAlt - el.getBoundingClientRect().bottom);
        if (Math.abs(fark) > 1) {
          ek('YAPISKAN-ALT', a, (el.id || el.className) + ' dibe ' + fark +
             ' px uzak — altindan icerik gorunur');
        }
      });
      kok.scrollTop = 0;
    });
  }

  /* 54) KARTIN ICINDE YAPISAN OGE (13.10.2026 · Cemil'in ekran goruntusu)
     "Yapilanlar altinda basliklar static, asagiya dogru divin de disina
     cikiyor." Tablo basligi `position: sticky; top: 0` idi. Kart bir
     kaydirma kabi DEGIL — kaydiran ekran; ekran kayinca baslik ekranin
     ustune yapisip satirlarin ve kart kenarinin ustunden kayiyordu.
     IKINCI KEZ yasandi (04.09: cekmecede "Bildirimler" basligi bildirimi
     ortuyordu). Kural listeye degil YAPIYA: bir kartin icinde, kaydirma
     kabi olmayan bir yerde, yapisan oge olmaz.
     NEGATIF DOGRULAMA (13.10): `.tablo th`'ye sticky geri kondu -> bulgu;
     kalkinca 0. */
  ekranlar.forEach(function (a) {
    var kok = document.getElementById('screen-' + a);
    if (!kok) return;
    showScreen(a);
    kok.querySelectorAll('.card *').forEach(function (el) {
      var cs = getComputedStyle(el);
      if (cs.position !== 'sticky' || cs.top === 'auto') return;
      /* Arada KENDI kaydirma kabi varsa (overflow auto/scroll) yapisma
         orada kalir — o mesru. */
      var kap = el.parentElement, mesru = false;
      while (kap && !kap.classList.contains('card')) {
        var o = getComputedStyle(kap).overflowY;
        if (o === 'auto' || o === 'scroll') { mesru = true; break; }
        kap = kap.parentElement;
      }
      if (!mesru) {
        ek('KART-ICI-YAPISKAN', a, (el.tagName.toLowerCase() + (el.className ? '.' + el.className : '')) +
           ' kartin icinde yapisiyor — ekran kayinca kartin disina tasar');
      }
    });
  });

  /* 55) TANIMANIN BOZDUGU METIN (14.10.2026 · SES-OLCUM §11.4)
     Yazili derlem (%93) bu sinifi GORMUYOR: o cumleler dogru yazilmis.
     Gercek seste taniyici plaka harfini kaydiriyor (TMR -> "PVR",
     KLM -> "kelime"), rakami boluyor (8890 -> "88 90"), "TL/lira"yi
     dusuruyor. Motor kayitli araci KAYITSIZ sanip sessizce yeni kayit
     actiriyordu -- en tehlikeli yanlis.
     Olculen: duzeltmeler calisiyor VE uc tuzak kapali kaliyor --
       a) K-059 KARMA: gercek OLAMAYAN plaka (Q/W/X, harf yerine kelime)
          tek kayitli plakaya baglaniyor, onay ekrani duyulani soyluyor;
          gercek OLABILECEK sesce yakin plaka ("06 pvr 07") BAGLANMIYOR,
          ONERILIYOR (K-047.6);
       b) "34 ARC 123" BAGLANMIYOR (R~B sesce yakin degil: gercek bir
          baska arac olabilir -- setin tuzagi); ama "34 QBC 123"
          BAGLANIYOR: Q plakada yok, gercek olamaz (Cemil 14.10);
       b2) il kodu 01-81 disi plaka sayilmiyor ("88 bin 900");
       c) bolunmus rakam ve harf yerine kelime;
       d) birimsiz tutar YALNIZ gider cumlesinde; telefon parcasi ve
          adet ("3 fatura") tutar sayilmiyor.
     NEGATIF DOGRULAMA (14.10): sesPlakaBagla null dondurdu -> bulgu;
     il kodu denetimi kaldirildi -> bulgu; gecersiz harf kurali kaldirildi -> bulgu;
     telefon korumasi kaldirildi -> bulgu; geri konunca 0. */
  (function () {
    if (typeof window.SES_COZUMLE !== 'function') {
      ek('SES-DUYULAN', 'ses', 'SES_COZUMLE yok -- olculemedi');
      return;
    }
    if (!aracBul('06 TMR 07') || !aracBul('35 KLM 45') || !aracBul('16 SY 8890') || !aracBul('34 ABC 123')) {
      ek('SES-DUYULAN', 'ses', 'tohum veride beklenen plakalar yok -- test guncellenmeli');
      return;
    }
    var yed = [window.SESLI_NIYET, window.ACIK_KAYIT];
    window.SESLI_NIYET = 'genel'; window.ACIK_KAYIT = null;
    function coz(m) { try { return window.SES_COZUMLE(m) || {}; } catch (e) { return { hata: e.message }; } }
    /* K-059 karma: gercek OLABILECEK sesce yakin plaka BAGLANMAZ, SORULUR. */
    var o = coz('06 pvr 07 bakima geldi');
    if (o.plaka === '06 TMR 07') {
      ek('SES-DUYULAN', 'ses', '"06 pvr 07" kendiliginden 06 TMR 07ye baglandi -- gercek olabilecek plaka SORULUR (K-059)');
    } else if (o.oneri !== '06 TMR 07') {
      ek('SES-DUYULAN', 'ses', '"06 pvr 07" icin 06 TMR 07 ONERILMEDI: ' + o.tur + ' oneri=' + o.oneri);
    }
    /* Gercek OLAMAYAN plaka baglanir ve onay ekrani soyler. */
    o = coz('34 qbc 123 triger seti degisecek');
    if (o.plaka !== '34 ABC 123' || o.duyulanPlaka !== '34 QBC 123') {
      ek('SES-DUYULAN', 'ses', '"34 QBC 123" kayitli ABCye baglanmadi: ' + o.tur + ' ' + o.plaka);
    } else {
      var oz = Islem.ozet(o);
      if (!oz || (oz.notice || '').indexOf('34 QBC 123') < 0) {
        ek('SES-DUYULAN', 'ses', 'plaka degistirildi ama onay ekrani duyulani SOYLEMIYOR');
      }
    }
    o = coz('34 ARC 123 triger seti degisecek');
    if (o.tur !== 'yeni-arac' || o.plaka !== '34 ARC 123') {
      ek('SES-DUYULAN', 'ses', '"34 ARC 123" kayitli araca baglandi (' + o.tur + ' ' + o.plaka + ') -- R~B sesce yakin degil, gercek plaka olabilir');
    }
    o = coz('16 XY 88 90 sahibine bakim hatirlatmasi gonder');
    if (o.plaka !== '16 SY 8890') ek('SES-DUYULAN', 'ses', 'bolunmus rakam + gecersiz X: ' + o.plaka);
    o = coz('06 TMR 07 kilometre 88 bin 900');
    if (o.plaka !== '06 TMR 07') ek('SES-DUYULAN', 'ses', 'il kodu 81 ustu plaka sayildi: ' + o.plaka);
    o = coz('88 BIN 900 bakima geldi');
    if (o.plaka) ek('SES-DUYULAN', 'ses', '"88 BIN 900" plaka sayildi -- il kodu 01-81');
    /* Buyuk set (14.10) uc sinif daha buldu: */
    var p5 = sesPlakaBul('34 M 49109 fren balatasi bitmis');
    if (!p5 || p5.plaka !== '34 M 49109') ek('SES-DUYULAN', 'ses', 'tek harfli 5 rakamli plaka okunmadi: ' + (p5 && p5.plaka));
    o = coz('41 KV 5588 İsmail Özdemir 0532 111 22 33 debriyaj kaciriyor');
    if (o.sahip !== 'İsmail Özdemir') ek('SES-DUYULAN', 'ses', 'Turkce harfle baslayan ad okunmadi (K-018 \\b): "' + o.sahip + '"');
    /* Soyleyis seti (16.10, Cemil'in ornekleri): */
    o = coz('41 KV 5588 Ahmet Yilmaz 0532 111 33 yok 23 44 debriyaj');
    if (o.tel !== '0532 111 23 44') ek('SES-DUYULAN', 'ses', 'soylerken duzeltme ("33 yok 23") uygulanmadi: tel=' + o.tel);
    o = coz('41 KV 5588 Ahmet Yilmaz sıfır beş yüz otuz iki yüz on bir yirmi üç kırk dört debriyaj');
    if (o.tel !== '0532 111 23 44') ek('SES-DUYULAN', 'ses', 'telefon sozcukle okunmadi (sayilar toplaniyor mu?): tel=' + o.tel);
    o = coz('06 TMR 07 kilometre seksen sekiz bin');
    if (o.km !== '88.000') ek('SES-DUYULAN', 'ses', '"seksen sekiz bin" km olmadi: ' + o.km);
    if (sesSoyleyisDuzelt('eksozdan duman') !== 'egzozdan duman') ek('SES-DUYULAN', 'ses', 'egzoz soyleyisi duzeltilmedi');
    /* Sozluk (betik/05-soyleyis.js, 16.10): harfsiz yazim da duzelir;
       kayittaki bicimin BASI olan soyleyis ikinci kez degismez. */
    if (sesSoyleyisDuzelt('amortisor') !== 'amortisör') ek('SES-DUYULAN', 'ses', 'harfsiz soyleyis ("amortisor") duzeltilmedi');
    if (sesSoyleyisDuzelt('manifold contasi') !== 'manifold contasi') ek('SES-DUYULAN', 'ses', '"manifold" bozuldu: ' + sesSoyleyisDuzelt('manifold contasi'));
    if (typeof SOYLEYIS_IFADELER !== 'function' || SOYLEYIS_IFADELER().indexOf('34 ABC 123') < 0) ek('SES-DUYULAN', 'ses', 'taniyici ipucu listesi dukkanin plakalarini icermiyor');
    /* Yazim bicimi (16.10, Speechmatics): saat gibi yazilan rakam, nokta,
       "3 bin 900"; bicime uymayan sahte plaka atlanir. */
    o = coz('saat 10:30 da 06 TMR 07 gelecek');
    if (o.plaka !== '06 TMR 07') ek('SES-DUYULAN', 'ses', 'bicime uymayan sahte plaka ("30 DA 06") secildi: ' + o.plaka);
    var tmrK2 = KAYITLAR.filter(function (k) { return k.plaka === '06 TMR 07' && !k.teslimTarihi; })[0];
    if (!tmrK2) ek('SES-DUYULAN', 'kayit', '06 TMR 07 icin acik kayit yok -- "3 bin 900" olculemedi');
    else {
      var yk2 = window.ACIK_KAYIT; var tmrKayitId = tmrK2.id; window.SESLI_NIYET = 'kayit-icinde'; window.ACIK_KAYIT = tmrKayitId;
      o = coz('Motor yağı 3 bin 900 lira.');
      if (o.fiyat !== 3900) ek('SES-DUYULAN', 'kayit', '"3 bin 900 lira." 3900 okunmadi: ' + o.fiyat);
      window.SESLI_NIYET = 'genel'; window.ACIK_KAYIT = yk2;
    }
    o = coz('yarın 123 balata gelecek');
    if (o.plaka) ek('SES-DUYULAN', 'ses', 'zaman sozlu sayi ("yarin 123") plakaya baglandi: ' + o.plaka);
    /* 300 gercek kayit (16.10): taniyici plaka harfini sozcuge cevirir,
       il kodunu kacirir, rakama nokta koyar; RAKAM cogunlukla dogru. */
    o = coz('Hatice 88 90 bakıma geldi');
    if (o.plaka !== '16 SY 8890' || !o.duyulanPlaka) ek('SES-DUYULAN', 'ses', 'plakasiz cumlede 4 haneli rakam kayitli araca baglanmadi: ' + o.tur + ' ' + o.plaka);
    o = coz('yarın 123 tane balata gelecek');
    if (o.plaka) ek('SES-DUYULAN', 'ses', '"123 tane" (adet) plakaya baglandi: ' + o.plaka);
    o = coz('16SY 88.90 bakıma geldi');
    if (o.plaka !== '16 SY 8890') ek('SES-DUYULAN', 'ses', 'bitisik harf-rakam / rakamda nokta okunmadi: ' + o.plaka);
    o = coz('38 TMR 07 bakıma geldi');
    if (o.plaka === '06 TMR 07') ek('SES-DUYULAN', 'ses', 'il kodu farkli gercek olabilecek plaka KENDILIGINDEN baglandi (K-059)');
    else if (o.oneri !== '06 TMR 07') ek('SES-DUYULAN', 'ses', 'il kodu kacirilan plakada rakamla oneri cikmadi: oneri=' + o.oneri);
    var tmrKayit = KAYITLAR.filter(function (k) { return k.plaka === '06 TMR 07' && !k.teslimTarihi; })[0];
    if (tmrKayit) {
      var yk = window.ACIK_KAYIT; window.SESLI_NIYET = 'kayit-icinde'; window.ACIK_KAYIT = tmrKayit.id;
      o = coz('hava filtresi 180 lira');
      if (o.tur === 'fiyat-guncelle') ek('SES-DUYULAN', 'kayit', '"hava filtresi" yakin soyleyisle "' + (o.satir && o.satir.ad) + '" satirina uydu -- YANLIS satirin fiyati degisir');
      window.SESLI_NIYET = 'genel'; window.ACIK_KAYIT = yk;
    }
    ARACLAR.push({ plaka: '35 ZZ 4545', marka: 'Citroën', model: 'C-Elysée', sahipAd: '', sahipTel: '' },
                 { plaka: '35 ZZ 4546', marka: 'Ford', model: 'Courier', sahipAd: '', sahipTel: '' });
    o = coz('Se Elize geldi debriyaj kaciriyor');
    if (o.plaka !== '35 ZZ 4545') ek('SES-DUYULAN', 'ses', '"Se Elize" C-Elysee olarak bulunmadi: ' + o.tur + ' ' + o.plaka);
    o = coz('Ford Kurye geldi');
    if (o.plaka !== '35 ZZ 4546') ek('SES-DUYULAN', 'ses', '"Ford Kurye" Courier olarak bulunmadi: ' + o.tur + ' ' + o.plaka);
    o = coz('kurye geldi paket birakti');
    if (o.plaka) ek('SES-DUYULAN', 'ses', '"kurye geldi" (bir insan) ARAC sayildi: ' + o.plaka);
    ARACLAR.pop(); ARACLAR.pop();
    o = coz("Mehmet Kaya'nın arabası geldi yağ değişimi yapılacak");
    if (o.tur !== 'servis-ac' || o.plaka !== '34 ABC 123' || o.isTuru !== 'bakim') {
      ek('SES-DUYULAN', 'ses', 'sahibin adiyla arac bulunmadi / yag degisimi bakim degil: ' + o.tur + ' ' + o.plaka + ' ' + o.isTuru);
    }
    o = coz('35 kelime 45 teslim ettim');
    if (o.plaka !== '35 KLM 45') ek('SES-DUYULAN', 'ses', '"35 kelime 45" 35 KLM 45e baglanmadi: ' + o.tur + ' ' + o.plaka);
    o = coz('2.400 elektrik faturasi ekle');
    if (o.tur !== 'gider-ekle' || o.tutar !== 2400) ek('SES-DUYULAN', 'muhasebe', 'birimsiz gider okunmadi: ' + o.tur + ' ' + o.tutar);
    o = coz('kira gideri 0532 111 22 33');
    if (o.tur === 'gider-ekle') ek('SES-DUYULAN', 'muhasebe', 'telefonun parcasi tutar sayildi: ' + o.tutar);
    o = coz('bu ay 3 fatura geldi');
    if (o.tur === 'gider-ekle') ek('SES-DUYULAN', 'muhasebe', 'adet tutar sayildi: ' + o.tutar);
    window.SESLI_NIYET = yed[0]; window.ACIK_KAYIT = yed[1];
  })();

  /* 56 · RUHSAT-ARAC — ruhsat okundugu AN arac kaydedilir (K-065).
     Hata sinifi (18.10, Cemil): arac yalniz "Kaydi ac"ta yaziliyordu;
     ruhsati okunup servis kaydi acilmayan arac (yalniz parca soran)
     KAYBOLUYORDU. Kayitli plaka sesli komutun plaka adayidir (K-059),
     kaybolan ruhsat motorun taniyamayacagi plaka demekti.
     Ikinci yuz: okumadan SONRAKI duzeltme kayda yazilmazsa kaydedilen
     sey modelin ilk okumasi olur, ustanin son hali degil.
     NEGATIF DOGRULAMA (18.10): ARACLAR.push kaldirildi -> bulgu;
     input esitlemesi kaldirildi -> bulgu; geri konunca 0. */
  (function () {
    if (typeof Ekran === 'undefined' || !Ekran.ac) return;
    /* TOHUM VERIDEN baslar: 30. test her dugmeye basiyor, "Ruhsati cek"
       de dahil — ornek arac ONDAN kaydedilmis olur ve bu test "eklendi
       mi"yi olcemezdi (ilk kosuda yasandi: 0 arac ekledi). */
    if (YEDEK.ARACLAR) window.ARACLAR = JSON.parse(JSON.stringify(YEDEK.ARACLAR));
    var once = ARACLAR.length;
    try {
      /* SASELI bir okuma bulunana kadar okunur: ornek sirasi 30. testin
         tiklamalariyla kayar ve sasesiz ornekte arac KAYDEDILMEMELI
         (K-066) — o ornekte K-065'i aramak yanlis bulgu verir (18.10). */
      var dug = null, pe = null, p = '';
      for (var d0 = 0; d0 < 4; d0++) {
        Ekran.ac('home'); Ekran.ac('ruhsat');
        dug = document.getElementById('ruhsatCek');
        if (!dug) break;
        once = ARACLAR.length;
        dug.click();
        if ((document.getElementById('rVin') || {}).value) break;
      }
      if (!dug) { ek('RUHSAT-ARAC', 'ruhsat', '"Ruhsati cek" dugmesi yok -- olculemedi'); return; }
      pe = document.getElementById('rPlaka');
      p = plakaBicim(pe ? pe.value : '');
      var a = aracBul(p);
      if (!a) { ek('RUHSAT-ARAC', 'ruhsat', 'ruhsat okundu ama arac kaydedilmedi (' + p + ') -- servis kaydi acilmazsa kaybolur'); return; }
      if (ARACLAR.length !== once + 1) ek('RUHSAT-ARAC', 'ruhsat', 'okuma ' + (ARACLAR.length - once) + ' arac ekledi, 1 bekleniyordu');
      var me = document.getElementById('rMarka');
      if (me) {
        me.value = 'DENETIM-MARKA';
        me.dispatchEvent(new Event('input', { bubbles: true }));
        if (a.marka !== 'DENETIM-MARKA') ek('RUHSAT-ARAC', 'ruhsat', 'okumadan sonraki duzeltme kayitli araca yazilmadi: ' + a.marka);
      }

      /* K-068 · ARAC KIMLIGI PARCA SORMANIN KAPISI (Cemil 18.10, K-066'yi
         duzeltir: "ruhsat zorunlu olmamali; parca fiyati isterse ruhsat,
         sase yoksa marka/model hepsini sectirmemiz gerekiyor"). Uc yuz:
           a) kayitsiz plaka yazilinca "Ruhsat adimini gec ve kaydi ac"
              VAR -- kayit acmak ruhsata bagli DEGIL;
           b) sasesi okunmayan ruhsatta arac yine KAYDEDILIR (K-065);
           c) sasesiz aracta parca sorusu GITMEZ; marka+model+yil+motor
              secilince GIDER.
         NEGATIF DOGRULAMA (18.10): aracKimligiTamam her zaman true ->
         bulgu; atla dugmesi kaldirildi -> bulgu; geri konunca 0. */
      Ekran.ac('home'); Ekran.ac('ruhsat');  /* ayni ekran yeniden acilinca alanlar SIFIRLANMIYOR -- once ana ekran */
      var pe2 = document.getElementById('rPlaka');
      pe2.value = '34 ZZ 9911';
      pe2.dispatchEvent(new Event('input', { bubbles: true }));
      if (!document.getElementById('kaydiAc')) {
        ek('RUHSAT-ARAC', 'ruhsat', 'kayitsiz aracta ruhsatsiz "Kaydi ac" yok -- ruhsat zorunlu OLMAMALI (K-068)');
      }
      /* Sasesiz ornege gelene kadar oku (orneklerden biri silik). */
      var saseszi = null;
      for (var d = 0; d < 4 && !saseszi; d++) {
        Ekran.ac('home'); Ekran.ac('ruhsat');
        document.getElementById('sekRuhsat').click();
        document.getElementById('ruhsatCek').click();
        var vin = (document.getElementById('rVin') || {}).value || '';
        if (!vin) saseszi = plakaBicim(document.getElementById('rPlaka').value);
      }
      if (!saseszi) {
        ek('RUHSAT-ARAC', 'ruhsat', 'prototipte sasesi okunmayan ornek ruhsat yok -- arac secimi olculemedi');
      } else if (!aracBul(saseszi)) {
        ek('RUHSAT-ARAC', 'ruhsat', 'sasesi okunmayan ruhsatta arac KAYDEDILMEDI (' + saseszi + ') -- kayit sasesiz de acilir (K-068)');
      } else {
        var sa = aracBul(saseszi);
        sa.marka = ''; sa.model = ''; sa.yil = ''; sa.motor = '';
        var tk = { id: 99901, plaka: saseszi, tur: 'tamirat', sikayet: 'denetim', acilis: '18.10.2026',
                   satirlar: [{ tip: 'parca', ad: 'Denetim parcasi', fiyat: 0, kimGetirdi: 'tamirci' }] };
        KAYITLAR.push(tk);
        var yk = window.ACIK_KAYIT;
        window.ACIK_KAYIT = 99901;
        Ekran.ac('home'); Ekran.ac('parca-sor');
        var cip = document.querySelector('#psParcacilar [data-parcaci]');
        if (cip) cip.click();
        var gon = document.getElementById('psGonder');
        if (!gon || !cip) {
          ek('RUHSAT-ARAC', 'parca-sor', 'gonder dugmesi ya da parcaci yok -- olculemedi');
        } else {
          gon.click();
          if (tk.satirlar[0].sorulan) ek('RUHSAT-ARAC', 'parca-sor', 'sasesiz, araci secilmemis aracta parca sorusu GITTI (K-068)');
          if (document.getElementById('psAracKart').hidden) ek('RUHSAT-ARAC', 'parca-sor', 'sasesiz aracta arac secim karti GORUNMUYOR');
          Ekran.ac('home'); Ekran.ac('parca-sor');
          [['psMarka', 'Opel'], ['psModel', 'Astra'], ['psYil', '2009'], ['psMotor', '1.6']].forEach(function (x) {
            var e = document.getElementById(x[0]);
            e.value = x[1]; e.dispatchEvent(new Event('input', { bubbles: true }));
          });
          var cip2 = document.querySelector('#psParcacilar [data-parcaci]');
          if (cip2) cip2.click();
          document.getElementById('psGonder').click();
          if (!tk.satirlar[0].sorulan) ek('RUHSAT-ARAC', 'parca-sor', 'arac secildikten sonra parca sorusu GITMEDI');
        }
        window.ACIK_KAYIT = yk;
      }
    } finally {
      /* Kalan veri yedegiGeriYukle ile doner; ekran alanlari da temizlensin. */
      Ekran.ac('home'); Ekran.ac('ruhsat');
    }
  })();

  /* 58 · PLAKA-CATISMA (K-069). Hata sinifi (18.10, olcumden): ruhsattan
     okunan plaka, ustanin YAZDIGI plakanin ustune SESSIZCE yaziliyordu;
     Gemini 3.8 Flash 46 ruhsatta 5 plakayi yanlis okudu. Sinanan:
       a) okuma ustanin yazdigi plakayi SILMEZ;
       b) catisma karti cikar;
       c) secilene kadar arac KAYDEDILMEZ (ne yazilanla ne okunanla);
       d) "ruhsattaki" secilince arac okunan plakayla kaydedilir.
     NEGATIF DOGRULAMA (18.10): catisma dali kaldirildi -> bulgu; geri -> 0. */
  (function () {
    if (typeof Ekran === 'undefined' || !Ekran.ac) return;
    if (YEDEK.ARACLAR) window.ARACLAR = JSON.parse(JSON.stringify(YEDEK.ARACLAR));
    try {
      var yaz = '34 ZZ 7788';
      Ekran.ac('home'); Ekran.ac('ruhsat');
      var pe = document.getElementById('rPlaka');
      pe.value = yaz; pe.dispatchEvent(new Event('input', { bubbles: true }));
      document.getElementById('sekRuhsat').click();
      document.getElementById('ruhsatCek').click();
      var simdi = plakaBicim(document.getElementById('rPlaka').value);
      var kart = document.getElementById('rPlakaCatisma');
      if (simdi !== yaz) { ek('PLAKA-CATISMA', 'ruhsat', 'okuma ustanin yazdigi plakayi ezdi: ' + yaz + ' -> ' + simdi); return; }
      if (!kart) { ek('PLAKA-CATISMA', 'ruhsat', 'plakalar farkli ama catisma karti yok'); return; }
      var ruhsatDug = document.getElementById('rPlakaRuhsat');
      var okunan = ruhsatDug ? plakaBicim(ruhsatDug.textContent) : '';
      if (aracBul(yaz) || (okunan && aracBul(okunan))) ek('PLAKA-CATISMA', 'ruhsat', 'catisma cozulmeden arac KAYDEDILDI');
      if (ruhsatDug) {
        ruhsatDug.click();
        if (plakaBicim(document.getElementById('rPlaka').value) !== okunan) ek('PLAKA-CATISMA', 'ruhsat', '"ruhsattaki" secildi ama alan degismedi');
        if (!aracBul(okunan)) ek('PLAKA-CATISMA', 'ruhsat', 'secimden sonra arac kaydedilmedi (' + okunan + ')');
        if (document.getElementById('rPlakaCatisma')) ek('PLAKA-CATISMA', 'ruhsat', 'secimden sonra catisma karti duruyor');
      }
    } finally {
      Ekran.ac('home'); Ekran.ac('ruhsat');
    }
  })();

  /* 59 · ALACAK-TAHSIL (22.10 · MUH sirasi). Hata sinifi: tahsilat TESLIM
     gunune gore sayiliyordu; sonradan odenen veresiye ya hic kasaya
     girmiyordu ya da teslim edildigi (gecmis) doneme yaziliyordu.
     Sinanan, "almadim" bir kayit kayit ekranindan odenince:
       a) alacak listesinden (veresiyeToplami) dusar;
       b) BUGUNUN tahsilatina (bugunTahsilat, "bugun" donemi) girer;
       c) "Geri al" her seyi eski haline dondurur.
     NEGATIF DOGRULAMA (22.10): tahsilTarihi() yalniz teslimTarihi'ni
     dondurunce -> bulgu; geri -> 0. */
  (function () {
    if (typeof Ekran === 'undefined' || !Ekran.ac || typeof tahsilTarihi !== 'function') {
      ek('ALACAK-TAHSIL', 'kayit', 'tahsilTarihi yok'); return;
    }
    var yk = window.ACIK_KAYIT;
    var k = (window.KAYITLAR || []).filter(function (x) {
      return x.teslimTarihi && x.odemeYolu === 'almadim' && x.teslimTarihi !== bugunMetin() && kayitToplami(x) > 0;
    })[0];
    if (!k) { ek('ALACAK-TAHSIL', 'kayit', 'gecmis gunde teslim edilmis veresiye kayit yok -- olculemedi'); return; }
    var ver0 = veresiyeToplami(), bug0 = bugunTahsilat(), top = kayitToplami(k);
    try {
      window.ACIK_KAYIT = k.id;
      Ekran.ac('home'); Ekran.ac('kayit');
      var cip = document.querySelector('#kTahsil [data-tahsil="nakit"]');
      if (!cip) { ek('ALACAK-TAHSIL', 'kayit', 'veresiye kayitta "odeme alindi" secenekleri yok'); return; }
      cip.click();
      if (veresiyeToplami() !== ver0 - top) ek('ALACAK-TAHSIL', 'kayit', 'odenen veresiye alacaktan dusmedi');
      if (bugunTahsilat() !== bug0 + top) ek('ALACAK-TAHSIL', 'kayit', 'sonradan odenen para BUGUNUN tahsilatina girmedi');
      if (tahsilatToplami('bugun') < top) ek('ALACAK-TAHSIL', 'muhasebe', '"bugun" doneminin tahsilatinda yok');
      if (document.getElementById('kTahsil')) ek('ALACAK-TAHSIL', 'kayit', 'odendikten sonra secenekler duruyor');
      var geri = document.querySelector('[data-geri-al]');
      if (!geri) ek('ALACAK-TAHSIL', 'kayit', '"Geri al" yok');
      else {
        geri.click();
        if (k.odemeYolu !== 'almadim' || k.tahsilTarihi || veresiyeToplami() !== ver0)
          ek('ALACAK-TAHSIL', 'kayit', '"Geri al" kaydi eski haline dondurmedi');
      }
    } finally {
      k.odemeYolu = 'almadim'; delete k.tahsilTarihi;
      window.ACIK_KAYIT = yk;
      Ekran.ac('home');
    }
  })();

  /* 57 · GENIS-DUZEN — tablet ve bilgisayar yerlesimi (K-064.2 · 04-genis.css).
     Yalniz genislik >= 768 px iken olcer; telefonda bu dosyanin hicbir
     kurali calismaz. Hata sinifi (18.10): ortalama kurali (align-self:
     center) grid'de karti IKI SATIRLIK alanda dikey ortaladi — Yapilanlar
     yanindaki Islemler'den 28 px asagida basliyordu; hicbir test gormedi.
       a) >= 768: kayitta Yapilanlar ile Islemler YAN YANA ve AYNI HIZADA;
       b) >= 1200: ana ekran (menu) solda GORUNUR, acik ekran onun SAGINDA,
          alt gezinme gizli.
     NEGATIF DOGRULAMA (18.10): align-self: start kaldirildi -> bulgu;
     ana ekranin display kurali kaldirildi -> bulgu; geri konunca 0. */
  (function () {
    if (window.innerWidth < 768 || typeof Ekran === 'undefined') return;
    var yk = window.ACIK_KAYIT;
    var ilk = (window.KAYITLAR || []).filter(function (k) { return !k.teslimTarihi; })[0];
    if (!ilk) { ek('GENIS-DUZEN', 'kayit', 'acik kayit yok -- olculemedi'); return; }
    window.ACIK_KAYIT = ilk.id;
    Ekran.ac('kayit');
    var y = document.querySelector('#screen-kayit .k-yapilanlar'), i = document.getElementById('kIslemler');
    if (!y || !i) {
      ek('GENIS-DUZEN', 'kayit', 'k-yapilanlar ya da kIslemler yok -- olculemedi');
    } else {
      var by = y.getBoundingClientRect(), bi = i.getBoundingClientRect();
      var yanyana = bi.left >= by.right - 1;
      /* 1200–1499 px: yan liste kayit sutununu daraltir, ic duzen TEK
         sutun olur (04-genis.css). Digerlerinde iki sutun. */
      var tekSutun = window.innerWidth >= 1200 && window.innerWidth < 1500;
      if (tekSutun) {
        if (yanyana) ek('GENIS-DUZEN', 'kayit', 'yan liste acikken kayit TEK sutun olmali (sutun dar)');
        else if (bi.top < by.bottom - 1) ek('GENIS-DUZEN', 'kayit', 'tek sutunda kartlar ust uste biniyor');
      } else if (!yanyana) {
        ek('GENIS-DUZEN', 'kayit', 'Yapilanlar ile Islemler yan yana degil (' + Math.round(by.right) + ' / ' + Math.round(bi.left) + ')');
      } else if (Math.abs(by.top - bi.top) > 2) {
        ek('GENIS-DUZEN', 'kayit', 'yan yana kartlar ayni hizada degil: ' + Math.round(by.top) + ' / ' + Math.round(bi.top) + ' px');
      }
    }
    /* d) 19.10 — kalan ekranlar: muhasebe Giderler|Alacak ayni hizada
          (gizli gider formu 18 px kaydiriyordu); form ekranlari <= 720 px;
          >= 1000 px'te arac gecmisi kunyeyle ayni hizada (baslik uzun kartin
          satirina dusup 340 px bosluk aciyordu). Uc hata da olculerek bulundu. */
    Ekran.ac('muhasebe');
    var mg = document.getElementById('muhGiderler'), ma = document.getElementById('muhAlacak');
    if (mg && ma && Math.abs(mg.getBoundingClientRect().top - ma.getBoundingClientRect().top) > 2) {
      ek('GENIS-DUZEN', 'muhasebe', 'Giderler ile Alacak ayni hizada degil: ' + Math.round(mg.getBoundingClientRect().top) + ' / ' + Math.round(ma.getBoundingClientRect().top));
    }
    Ekran.ac('teslim');
    var tk = document.querySelector('#screen-teslim > .card');
    if (tk && tk.getBoundingClientRect().width > 721) ek('GENIS-DUZEN', 'teslim', 'form ekrani 720 px\'ten genis: ' + Math.round(tk.getBoundingClientRect().width));
    if (window.innerWidth >= 1000) {
      var ilkArac = (window.ARACLAR || [])[0];
      if (ilkArac) {
        var ypl = window.ACIK_PLAKA;
        window.ACIK_PLAKA = ilkArac.plaka; Ekran.ac('arac');
        window.ACIK_PLAKA = ypl;
        var ak = document.querySelector('#screen-arac > .card'), ag = document.querySelector('#screen-arac .ar-gecmis-blok');
        if (!ak || !ag) ek('GENIS-DUZEN', 'arac', 'kunye ya da gecmis blogu yok -- olculemedi');
        else if (Math.abs(ak.getBoundingClientRect().top - ag.getBoundingClientRect().top) > 2) ek('GENIS-DUZEN', 'arac', 'gecmis kunyeyle ayni hizada degil: ' + Math.round(ak.getBoundingClientRect().top) + ' / ' + Math.round(ag.getBoundingClientRect().top));
      }
    }
    Ekran.ac('kayit');

    if (window.innerWidth >= 1200) {
      var h = document.getElementById('screen-home'), s = document.getElementById('screen-kayit');
      var bh = h.getBoundingClientRect(), bs = s.getBoundingClientRect();
      if (getComputedStyle(h).display === 'none' || bh.width < 200) ek('GENIS-DUZEN', 'home', 'bilgisayarda menu solda GORUNMUYOR');
      else if (bs.left < bh.right - 1) ek('GENIS-DUZEN', 'kayit', 'acik ekran menunun SAGINDA degil');
      var bb = document.querySelector('.device > .botbar');
      if (bb && getComputedStyle(bb).display !== 'none') ek('GENIS-DUZEN', 'kabuk', 'bilgisayarda alt gezinme gorunuyor -- menu zaten solda');
      /* c) YAN PANEL (Cemil 18.10 "4 evet"): kayit acikken liste menu ile
            kayit ARASINDA gorunur, acik kayit listede isaretli; listeye
            .active VERILMEZ (tek gorunen ekran sozlesmesi). */
      var l = document.getElementById('screen-kayitlar');
      var bl = l.getBoundingClientRect();
      if (!l.classList.contains('yan') || getComputedStyle(l).display === 'none' || bl.width < 200) {
        ek('GENIS-DUZEN', 'kayitlar', 'bilgisayarda kayit acikken liste YANINDA gorunmuyor');
      } else {
        if (!(bl.left >= bh.right - 1 && bl.right <= bs.left + 1)) ek('GENIS-DUZEN', 'kayitlar', 'yan liste menu ile kayit arasinda degil');
        if (l.classList.contains('active')) ek('GENIS-DUZEN', 'kayitlar', 'yan listeye .active verildi -- tek gorunen ekran sozlesmesi bozuldu');
        var sec = l.querySelector('[data-kayit="' + ilk.id + '"]');
        if (!sec || sec.getAttribute('aria-current') !== 'true') ek('GENIS-DUZEN', 'kayitlar', 'acik kayit yan listede isaretli degil');
      }
      Ekran.ac('home');
      if (l.classList.contains('yan')) ek('GENIS-DUZEN', 'kayitlar', 'kayit kapaninca yan liste KALDI');
    }
    window.ACIK_KAYIT = yk;
  })();

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
