/* ============ SES EŞDEĞERLİK ÖLÇÜMÜ (12.10.2026 · Tur 8.3) ============
   SORU: ses tarafını yeniden yapılandırdım — usta için bir şey değişti mi?

   Değerlendirme seti (%93) yalnız 40 cümlenin TÜRÜNE ve kritik alanına
   bakar; davranış değişikliğinin çoğunu görmez. Bu araç ESKİ derleme ile
   YENİ derlemeyi aynı sayfada yan yana yükler ve üç şeyi BİREBİR
   karşılaştırır:
     1) işlem nesnesi   — ~70 cümle × 8 bağlam (değerlendirme seti +
                           ekran örnekleri + tuzaklar)
     2) onay ekranı     — başlık, satırlar, düğmeler, seçili çipler, kutu
     3) uygulama        — 16 işlem sırayla onaylanır; veri, açılan ekran,
                           balon ve "Kayda dön" her adımdan sonra
   Fark = davranış değişti. Bilerek değiştirdiysen farkı oku ve yaz;
   bilmeden değiştiyse hata.

   İLK KULLANIM 12.10: şema geçişi. 568/0 · 392/0 · 16/0. Araç bir hatayı
   denetimden ÖNCE yakaladı (şema kayda düşmüyordu) → 51. test doğdu.
   NEGATİF DOĞRULAMA: öncelik sırası bozuldu → 18 fark; gider şemasının
   düğmesi ve yazdığı veri bozuldu → 30 ekran + 10 uygulama farkı.
   ⚠️ Görünmeyen değişiklik de olur: toplu/tekli mesajın önceliğini
   takas etmek 0 fark verdi — derlemde plakalı TOPLU mesaj cümlesi yok.
   0 fark "aynı" demek değil, "bu derlemde aynı" demektir.

   NASIL (prototip localhost'ta açıkken):
     1. DEĞİŞİKLİKTEN ÖNCE:  node tools/prototip-derle.js
                             cp prototypes/omurga.html prototypes/_taban.html
     2. değişikliği yap, derle (denetimi GÖMME — gömülü denetim load
        anında koşar ve ölçülen veriyi oynatır)
     3. konsolda:
          eval(await (await fetch('/tools/ses-esdeger.js')).text());
          await SES_ESDEGER();
   `_taban.html` .gitignore'da — geçici bir ölçü çubuğu, kaynak değil.

   ⚠️ ÖLÇÜM ALETİ ÖLÇTÜĞÜ ŞEYİ DEĞİŞTİRMEZ (K-024.1): iki kopya kendi
   iframe'inde koşar; açık sayfaya dokunulmaz. */
window.SES_ESDEGER = async function () {
  var nesne = await (async function () {
  var BAGLAMLAR = ['genel', 'servis-ekle', 'araclar', 'kayitlar', 'musteriler',
                   'mesaj', 'muhasebe', 'kayit-icinde'];
  var EK = ['balata da degisecek', 'iscilik 750 lira', 'yag filtresi 300 lira',
    'teslim ettim nakit aldim', 'teslim ettim kartla', 'servistekileri goster',
    '34 ABC 123 sahibine selam at', 'bugun ne kadar tahsilat', 'bu ay ne kadar tahsil ettim',
    '2400 lira elektrik faturasi', '850 lira kira gideri ekle', 'herkese sms at yarin kapaliyiz',
    'tum musterilere bayram mesaji gonder', '41 KV 5588 yeni arac bakim',
    '34 ABC 123 beyaz araba geldi', '34 ABC 123 gosterge paneli yaniyor',
    '34 QBC 123 sahibine whatsapp selam mesaji gonder', '99 ZZZ 999 teslim ettim',
    'otuz dort a b c yuz yirmi uc bakima geldi', '41 KV 5588 Ahmet Yilmaz 0532 111 22 33 debriyaj kaciriyor',
    'merhaba', '', 'balata 450 lira', 'parca 300 lira', 'fren hidroligi 120 lira parca',
    '34 ABC 123 ara', 'Mehmet ara', '34 ABC 123 sahibini ara', '500 lira'];

  function yukle(src) {
    return new Promise(function (ok) {
      var f = document.createElement('iframe');
      f.style.cssText = 'position:fixed;left:-9999px;width:375px;height:800px';
      f.onload = function () { setTimeout(function () { ok(f.contentWindow); }, 800); };
      f.src = src + '?t=' + Date.now();
      document.body.appendChild(f);
    });
  }
  var A = await yukle('/prototypes/_taban.html');
  var B = await yukle('/prototypes/omurga.html');

  var set = await (await fetch('/tools/degerlendirme/ses-seti.json?t=' + Date.now())).json();
  var cumleler = set.cumleler.map(function (c) { return c.cumle; }).concat(EK);
  var orn = A.SES_BAGLAMLAR();
  Object.keys(orn).forEach(function (k) { cumleler = cumleler.concat(orn[k]); });
  var disarida = A.eval("(ARACLAR.filter(function(a){return !aracServiste(a.plaka);})[0]||{}).plaka");
  cumleler.push(disarida + ' teslim ettim nakit', disarida + ' teslim ettim');
  cumleler = cumleler.filter(function (c, i) { return cumleler.indexOf(c) === i; });

  function acikKayit(W) {
    return W.eval("(icerideki().filter(function(k){return (k.satirlar||[]).length>1;})[0]||{}).id");
  }
  function duz(o) { return JSON.stringify(sirala(o)); }
  function sirala(o) {
    if (o === null || typeof o !== 'object') return o;
    if (Array.isArray(o)) return o.map(sirala);
    var c = {};
    Object.keys(o).sort().forEach(function (k) {
      var v = o[k];
      if (v === undefined) return;
      if (k === 'arac' && v) v = 'ARAC:' + v.plaka;
      else if (k === 'kayit' && v) v = 'KAYIT:' + v.id;
      else if (k === 'satir' && v) v = 'SATIR:' + v.ad;
      c[k] = sirala(v);
    });
    return c;
  }
  var kA = acikKayit(A), kB = acikKayit(B);
  var fark = [], toplam = 0;
  cumleler.forEach(function (c) {
    BAGLAMLAR.forEach(function (b) {
      [A, B].forEach(function (W, i) {
        W.SESLI_NIYET = b;
        W.ACIK_KAYIT = b === 'kayit-icinde' ? (i ? kB : kA) : null;
      });
      var a, bb;
      try { a = duz(A.SES_COZUMLE(c)); } catch (e) { a = 'HATA ' + e.message; }
      try { bb = duz(B.SES_COZUMLE(c)); } catch (e) { bb = 'HATA ' + e.message; }
      toplam++;
      if (a !== bb) fark.push({ b: b, c: c, eski: a, yeni: bb });
    });
  });
  window.ESDEGER = { toplam: toplam, cumle: cumleler.length, fark: fark,
                     taze: !!B.ISLEM_SEMALARI };
  return { toplam: toplam, cumle: cumleler.length, farkSayisi: fark.length,
           taze: !!B.ISLEM_SEMALARI, ilkFarklar: fark.slice(0, 6), farklar: fark };
})();
  var ekranVeIs = await (async function () {
  var BAGLAMLAR = ['genel', 'servis-ekle', 'araclar', 'kayitlar', 'musteriler',
                   'mesaj', 'muhasebe', 'kayit-icinde'];
  function yukle(src) {
    return new Promise(function (ok) {
      var f = document.createElement('iframe');
      f.style.cssText = 'position:fixed;left:-9999px;width:375px;height:800px';
      f.onload = function () { setTimeout(function () { ok(f.contentWindow); }, 800); };
      f.src = src + '?t=' + Date.now();
      document.body.appendChild(f);
    });
  }
  document.querySelectorAll('iframe').forEach(function (f) { f.remove(); });
  var A = await yukle('/prototypes/_taban.html');
  var B = await yukle('/prototypes/omurga.html');
  var kayitId = function (W) {
    return W.eval("(icerideki().filter(function(k){return (k.satirlar||[]).length>1;})[0]||{}).id");
  };
  var kA = kayitId(A), kB = kayitId(B);

  /* ---- 1) ONAY EKRANI: metin + düğmeler + seçili çipler + kutu içeriği ---- */
  function ekranImza(W) {
    var g = W.document.getElementById('sesGovde');
    var dugme = [].map.call(g.querySelectorAll('button'), function (b) {
      return (b.textContent || '').trim() + (b.hasAttribute('aria-pressed') ? '[' + b.getAttribute('aria-pressed') + ']' : '');
    });
    var kutu = [].map.call(g.querySelectorAll('textarea'), function (t) { return t.value; });
    var etiket = [].map.call(g.querySelectorAll('label, .section-label, [role=group]'), function (e) {
      return (e.getAttribute('aria-label') || '') + '|' + (e.textContent || '').trim();
    });
    return JSON.stringify({
      baslik: W.document.getElementById('sesBaslik').textContent,
      durum: W.document.getElementById('sesDurum').textContent,
      aciklama: W.document.getElementById('sesAciklama').textContent,
      metin: (g.textContent || '').replace(/\s+/g, ' ').trim(),
      dugme: dugme, kutu: kutu, etiket: etiket,
      onayla: !!W.document.getElementById('sesOnayla'),
      oneri: !!W.document.getElementById('sesOneri'),
      baska: !!W.document.getElementById('sesBaskaIs'),
      ekranda: !!W.document.getElementById('sesEkrandaAc')
    });
  }
  var set = await (await fetch('/tools/degerlendirme/ses-seti.json?t=' + Date.now())).json();
  var cumleler = set.cumleler.map(function (c) { return c.cumle; });
  var orn = A.SES_BAGLAMLAR();
  Object.keys(orn).forEach(function (k) { cumleler = cumleler.concat(orn[k]); });
  var disarida = A.eval("(ARACLAR.filter(function(a){return !aracServiste(a.plaka);})[0]||{}).plaka");
  cumleler.push(disarida + ' teslim ettim nakit', '34 QBC 123 sahibine selam gonder',
                'yag filtresi 300 lira', 'teslim ettim kartla', 'merhaba');
  cumleler = cumleler.filter(function (c, i) { return cumleler.indexOf(c) === i; });

  var ekranFark = [], ekranToplam = 0;
  cumleler.forEach(function (c) {
    BAGLAMLAR.forEach(function (b) {
      var im = [A, B].map(function (W, i) {
        W.SESLI_NIYET = b;
        W.ACIK_KAYIT = b === 'kayit-icinde' ? (i ? kB : kA) : null;
        try { W.SES_SOYLE(c); return ekranImza(W); } catch (e) { return 'HATA ' + e.message; }
      });
      ekranToplam++;
      if (im[0] !== im[1]) ekranFark.push({ b: b, c: c, eski: im[0], yeni: im[1] });
    });
  });

  /* ---- 2) UYGULAMA: aynı sırayla onayla, sonra veriyi ve ekranı karşılaştır ---- */
  function durum(W) {
    return JSON.stringify(W.eval('(' + function () {
      return {
        kayitlar: KAYITLAR, giderler: GIDERLER,
        mesajlar: MESAJLAR.map(function (m) { var c = Object.assign({}, m); delete c.saat; return c; }),
        acikKayit: ACIK_KAYIT, acikPlaka: ACIK_PLAKA, arama: ARAMA_TERIMI,
        ondolgu: RUHSAT_ONDOLGU, mesajHedef: MESAJ_HEDEF, donem: muhDonemSecili,
        ekran: Ekran.adi(),
        balon: (document.getElementById('toast') || {}).textContent || '',
        odemeSecili: [].map.call(document.querySelectorAll('#tsOdeme [aria-pressed=true]'),
                                 function (b) { return b.getAttribute('data-odeme'); }),
        donus: window.SES_DONUS ? window.SES_DONUS.kayit : null
      };
    } + ')()'));
  }
  /* [bağlam, cümle, ek eylem] — her şema en az bir kez uygulanıyor */
  var IS = [
    ['genel', '34 ABC 123 triger seti değişecek'],
    ['servis-ekle', '41 KV 5588 Ahmet Yılmaz 0532 111 22 33 debriyaj kaçırıyor'],
    ['kayit-icinde', 'balata 450 lira'],
    ['kayit-icinde', 'yağ filtresi 300 lira'],
    ['kayit-icinde', 'bugün ne kadar tahsilat yaptım'],
    ['kayit-icinde', 'bugün ne kadar tahsilat yaptım', 'baska'],
    ['kayit-icinde', '2400 lira elektrik faturası'],
    ['muhasebe', '850 lira kira gideri ekle'],
    ['muhasebe', 'bu ay ne kadar tahsil ettim'],
    ['mesaj', '34 ABC 123 sahibine WhatsApp üzerinden selam mesajı gönder', 'sms'],
    ['mesaj', '34 ABC 123 sahibine selam gönder', 'ekranda'],
    ['mesaj', 'tüm müşterilere bayram mesajı gönder', 'metin'],
    ['genel', '34 QBC 123 sahibine selam gönder'],
    ['kayitlar', 'servistekileri göster'],
    ['genel', disarida + ' teslim ettim'],
    ['kayit-icinde', 'teslim ettim nakit aldım']
  ];
  var isFark = [];
  IS.forEach(function (x, n) {
    var s = [A, B].map(function (W, i) {
      try {
        W.Ekran.goster('ses');
        W.SESLI_NIYET = x[0];
        W.ACIK_KAYIT = x[0] === 'kayit-icinde' ? (i ? kB : kA) : null;
        W.SES_DONUS = null;
        W.SES_SOYLE(x[1]);
        var d = W.document;
        if (x[2] === 'baska') d.getElementById('sesBaskaIs').click();
        if (x[2] === 'sms') d.querySelector('#sesGovde .cip-grup button:nth-child(2)').click();
        if (x[2] === 'metin') d.querySelector('#sesGovde textarea').value = 'Yarın kapalıyız.';
        if (x[2] === 'ekranda') d.getElementById('sesEkrandaAc').click();
        else d.getElementById('sesOnayla').click();
        return durum(W);
      } catch (e) { return 'HATA ' + e.message; }
    });
    if (s[0] !== s[1]) isFark.push({ n: n, is: x, eski: s[0], yeni: s[1] });
  });

  var r = { ekranToplam: ekranToplam, ekranFark: ekranFark.length, isToplam: IS.length,
            isFark: isFark.length, taze: !!B.ISLEM_SEMALARI,
            ilkEkranFark: ekranFark.slice(0, 2), ilkIsFark: isFark.slice(0, 1),
            ekranFarklar: ekranFark, isFarklar: isFark };
  window.ESDEGER2 = { ekranFark: ekranFark, isFark: isFark };
  return r;
})();
  document.querySelectorAll('iframe').forEach(function (f) { f.remove(); });
  return { nesne: nesne, ekranVeIs: ekranVeIs };
};
