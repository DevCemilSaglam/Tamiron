/* Tamiron prototip saglik denetimi — 34 test.
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

  var b=[];function ek(t,e,d){b.push(t+' | '+e+' | '+d);}

  /* ⚠️ DENETIM OLCTUGU SEYI BOZMAZ (04.09.2026).
     30. test (SAHTE-EYLEM) her dugmeye BASIYOR; 27. test ACIK_KAYIT ve
     ACIK_PLAKA atiyor. Denetim `load` aninda KENDILIGINDEN kostugu icin
     prototip her acilista KIRLI basliyordu: taze yuklenmis bir sayfada
     SESLI_NIYET 'kayit-icinde', ACIK_KAYIT 1 duruyordu ve sesli islem
     coozumleyicisi HER CUMLEYI "satir ekle" diye okuyordu.

     Yani denetim yalniz veriyi degil, SONRAKI OLCUMU de bozuyordu.
     Kendi olctugu seyi degistiren bir arac, olcum aleti degildir.

     Cozum: bastan derin kopya, sonda geri yukleme. */
  var YEDEK = (function () {
    var y = {};
    ['KAYITLAR', 'ARACLAR', 'GIDERLER', 'PARCACILAR', 'DUKKAN'].forEach(function (ad) {
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
      if (YEDEK[ad] === null && ['KAYITLAR','ARACLAR','GIDERLER','PARCACILAR','DUKKAN'].indexOf(ad) >= 0) return;
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
  var re = /getElementById\(\s*'([A-Za-z_][\w-]*)'\s*\)/g;
  var m, gorulen = {};
  while ((m = re.exec(kod))) {
    var id = m[1];
    if (gorulen[id]) continue;
    gorulen[id] = 1;
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
        bas.click();
        var acildi = !govde.hidden && govde.offsetHeight > 0;
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
    var ELE = '[data-git], [data-sesle], [data-arac], [data-kayit], .mic-round, .clear, .akd-bas, .cip, [role="tab"]';
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
    ekranlar.forEach(function (a) {
      showScreen(a);
      document.querySelectorAll('#screen-' + a + ', #screen-' + a + ' .card').forEach(function (kap) {
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

  /* SON IS: prototipi denetim oncesi haline dondur. Ekranlar da
     yeniden cizilsin ki ekranda kirli veriden kalma bir sey durmasin. */
  yedegiGeriYukle();
  showScreen('home');

  console.log('SAGLIK DENETIMI — '+ekranlar.length+' ekran, '+b.length+' bulgu');
  b.forEach(function(x){
console.log('  '+x);});
  return {ekran:ekranlar.length, bulgu:b.length, liste:b,
          olcum: OLCULEBILIR ? 'yapildi' : 'ATLANDI (gorus alani '+window.innerWidth+'px)'};
})()
