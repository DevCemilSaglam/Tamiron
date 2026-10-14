/* ============ KOŞUCU 1 — BUGÜNKÜ MOTOR (tarayıcıda) ============
   Cemil, 14.09: *"Yapay zeka testini yapamayacak mıyız?"*

   YAPILIYOR — ve bu parçası anahtar istemiyor: bugünkü desen
   eşleştiricinin KARNESİ. Model geldiğinde aşılması gereken çizgi bu;
   çizgi ölçülmeden *"model daha iyi"* denemez.

   NASIL KOŞULUR (prototip localhost'ta açıkken):
     var k = await (await fetch('/tools/degerlendirme/tarayicida-kos.js')).text();
     eval(k); await DEGERLENDIR();

   ⚠️ ÖLÇÜM ALETİ ÖLÇTÜĞÜ ŞEYİ DEĞİŞTİRMEZ (K-024.1): koşucu yalnız
   `SES_COZUMLE` çağırıyor — hiçbir şey uygulanmıyor, ekran açılmıyor,
   veri yazılmıyor. Dokunduğu iki değişken (SESLI_NIYET, ACIK_KAYIT)
   sonda geri yükleniyor. */
window.DEGERLENDIR = async function (secenek) {
  var s = secenek || {};
  var kok = s.kok || '/tools/degerlendirme/';
  var set = s.set || await (await fetch(kok + 'ses-seti.json')).json();

  /* Puanlama ORTAK (puanla.js) — model koşucusu da aynı cetveli
     kullanıyor, yoksa iki karne karşılaştırılamaz. */
  if (!window.DEGERLENDIRME_PUANLA) {
    eval(await (await fetch(kok + 'puanla.js')).text());
  }

  var yedek = { niyet: window.SESLI_NIYET, kayit: window.ACIK_KAYIT,
                araclar: window.ARACLAR, kayitlar: window.KAYITLAR };

  /* KURMACA DÜKKÂN (14.10 · buyuk-set.json): set kendi dükkânını
     getiriyorsa ölçüm o dükkânda yapılır — tohum verinin 9 aracıyla
     1000 cümle sınanamaz. Derin kopya: çözümleyici veriyi değiştirmez
     ama ölçüm aleti ölçtüğü şeyi değiştirmemeli (K-024.1); sonda
     prototipin kendi verisi geri konur. */
  if (s.dukkan) {
    window.ARACLAR = JSON.parse(JSON.stringify(s.dukkan.araclar));
    window.KAYITLAR = JSON.parse(JSON.stringify(s.dukkan.kayitlar));
  }
  /* `yari`: 'gelistirme' | 'sinav' — yalnız o yarı ölçülür. */
  if (s.yari) set = Object.assign({}, set, { cumleler: set.cumleler.filter(function (c) { return c.yari === s.yari; }) });

  /* "kayit-icinde" bağlamı AÇIK BİR KAYIT ister; yoksa çözümleyici o
     dalı hiç denemez ve ölçüm sessizce yanlış olur. Birden çok satırı
     olan açık kayıt seçiliyor — 14/15/40 numaralı cümlelerin sınavı
     "bu ad kayıtta var mı" sorusu. */
  var kayit = (typeof icerideki === 'function')
    ? icerideki().filter(function (k) { return (k.satirlar || []).length > 1; })[0]
    : null;
  var acikKayitId = kayit ? kayit.id : ((window.KAYITLAR || [{}])[0] || {}).id;

  var cevaplar = {};
  try {
    set.cumleler.forEach(function (c) {
      window.SESLI_NIYET = c.baglam;
      /* Cümle kendi açık kaydını söylüyorsa (kayitId) o kayıt. */
      window.ACIK_KAYIT = (c.baglam === 'kayit-icinde') ? (c.kayitId || acikKayitId) : null;
      try { cevaplar[c.id] = window.SES_COZUMLE(c.cumle); }
      catch (e) { cevaplar[c.id] = { hata: e.message }; }
    });
  } finally {
    window.SESLI_NIYET = yedek.niyet;
    window.ACIK_KAYIT = yedek.kayit;
    window.ARACLAR = yedek.araclar;
    window.KAYITLAR = yedek.kayitlar;
  }

  var sonuc = window.DEGERLENDIRME_PUANLA(set, cevaplar);
  sonuc.motor = 'desen-eslestirici';
  sonuc.kayitBaglami = acikKayitId;
  return sonuc;
};
