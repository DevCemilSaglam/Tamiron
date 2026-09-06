/* ============ ORTAK NESNELER ============
   BİR NESNE BİR YERDE ÇİZİLİR. Plaka üç ekranda görünüyor; üç kez
   yazılırsa üç kez ayrışır.

   ⚠️ İMZALAR v1'DEN. plakaHtml(plaka, boyut) ve bosDurum(ikon,
   baslik, aciklama, eylem, sinif) v1'de bu şekildeydi; adı ya da
   parametre sırası değiştirilirse taşınan CSS ve alışkanlık kırılır.
   03.09 — Cemil: "Tasarım eskisi gibi devam etmeli." */

/* ---- PLAKA — gerçek TR plakasının dili, her yerde aynı ---- */
function plakaHtml(plaka, boyut) {
  return '<span class="plate ' + (boyut || 'plate-sm') + '">' +
         '<span class="plate-tr">TR</span>' +
         '<span class="plate-no">' + plaka + '</span></span>';
}

/* ---- BOŞ DURUM — kuru "sonuç yok" değil, ne yapılacağını söyler ---- */
function bosDurumHtml(ikon, baslik, aciklama, eylem, sinif) {
  return '<div class="bos-durum' + (sinif ? ' ' + sinif : '') + '">' +
    '<span class="bd-ikon"><svg class="ico"><use href="#' + ikon + '"></use></svg></span>' +
    '<span class="bd-baslik">' + baslik + '</span>' +
    '<span class="bd-aciklama">' + aciklama + '</span>' +
    (eylem || '') +
  '</div>';
}

/* ---- ARAÇ RENGİ KUTUCUĞU ----
   04.09, Cemil: "renk yazmak yerine kutucuk içerisinde rengi olsun."

   ⚠️ SINIF ADI KODDA BİRLEŞTİRİLMİYOR, HARİTADAN OKUNUYOR.
   'r-' + norm(renk) yazmak kolaydı ama ruhsatta "Gri Metalik" yazan
   araç .r-gri-metalik gibi HİÇ TANIMLANMAMIŞ bir sınıf üretirdi;
   tanımsız sınıf sessizce hiçbir şey yapmaz — kutucuk boş çıkar ve
   boş kutucuk "beyaz" diye okunur. Harita, tanımlı olmayan her şeyi
   .r-bilinmeyen'e düşürüyor.

   ⚠️ TÜRKÇEDE KELİME SINIRI ARANMAZ (K-018): "gri metalik" boşluktan
   BÖLÜNÜP her parça eşitlikle karşılaştırılıyor. */
var RENK_SINIF = {
  beyaz: 'r-beyaz', siyah: 'r-siyah', gri: 'r-gri', gumus: 'r-gumus',
  lacivert: 'r-lacivert', mavi: 'r-mavi', kirmizi: 'r-kirmizi',
  bordo: 'r-bordo', yesil: 'r-yesil', sari: 'r-sari',
  turuncu: 'r-turuncu', kahverengi: 'r-kahverengi', bej: 'r-bej',
  mor: 'r-mor'
};

function renkSinifi(renk) {
  var parcalar = norm(renk).split(/\s+/);
  for (var i = 0; i < parcalar.length; i++) {
    if (RENK_SINIF[parcalar[i]]) return RENK_SINIF[parcalar[i]];
  }
  return 'r-bilinmeyen';
}

/* Kutucuğun ADI title/aria'da durur: renk körü usta için kutucuk tek
   başına bilgi taşımaz, ayrıca ruhsatta "Gri Metalik" yazıyorsa
   kutucuk yalnız "gri"yi gösterebiliyor. */
function renkKutuHtml(renk) {
  var ad = (renk || '').trim();
  return '<span class="renk-kutu ' + renkSinifi(ad) + '" title="' +
         (ad || 'Renk yazılmamış') + '" role="img" aria-label="Renk: ' +
         (ad || 'yazılmamış') + '"></span>';
}

/* ---- ARAÇ KARTI ----
   04.09'da Cemil satır satır tarif etti; ondan önce araç listesi
   servis kaydı satırını (.tl-item) giyiyordu ve araç ikonu, "1 kayıt",
   "son bakım" gibi ustanın sormadığı şeyleri gösteriyordu.

   ORTAK NESNE KURALI: kart burada çizilir, davranışı da burada
   ([data-arac] delegasyonu aşağıda). Araç kartı ileride müşteri
   ekranında da görünürse ikinci bir çizim yazılmaz. */
function aracKartiHtml(a, icerde) {
  var motor = a.motor ? '<span class="ak-motor">' + a.motor + '</span>' : '';
  return '<div class="arac-kart' + (icerde ? ' icerde' : '') + '"' +
      ' data-arac="' + a.plaka + '" role="button" tabindex="0"' +
      ' aria-label="' + a.plaka + ' — ' + a.marka + ' ' + a.model + '">' +
    '<div class="ak-ust">' + plakaHtml(a.plaka, 'plate-lg') + '</div>' +
    '<div class="ak-arac">' +
      '<span>' + a.marka + ' ' + a.model + '</span>' + motor +
      renkKutuHtml(a.renk) +
    '</div>' +
    /* Ruhsat sahibi KİŞİ DE OLABİLİR FİRMA DA (Cemil, 04.09).
       Ayrı bir alan tutulmuyor — ad neyse o yazılıyor; firma adının
       "Ad Soyad" gibi görünmesi diye bir sorun yok. */
    '<div class="ak-sahip"><b>' + (a.sahipAd || 'sahip yazılmamış') + '</b></div>' +
    (a.sahipTel ? telLinkHtml(a.sahipTel, a.sahipAd) : '') +
    /* İKİNCİ İLETİŞİM: ruhsat firmanınsa aranacak kişi başkasıdır.
       Adı olmadan telefon işe yaramıyor — usta kimi aradığını
       bilmeden aramaz. */
    (a.iletisimTel || a.iletisimAd
      ? '<div class="ak-kisi">' +
          '<span class="ak-kisi-ad">İletişim: <b>' +
            (a.iletisimAd || 'ad yazılmamış') + '</b></span>' +
          (a.iletisimTel ? telLinkHtml(a.iletisimTel, a.iletisimAd) : '') +
        '</div>'
      : '') +
  '</div>';
}

/* ---- SERVİS KAYDI KARTI (05.09) ----
   Cemil: "Servis kayıtları araç listelemesi gibi olsun. Araç bilgileri
   belirgin olacak. Sonra altına Tamirat, Bakım veya Tamirat + Bakım
   yazabilir."

   ARAÇ KARTININ DİLİNİ KULLANIYOR — yeni bir görsel dil değil, aynı
   kart. Usta iki listede iki ayrı okuma alışkanlığı kurmasın: plaka
   aynı yerde, marka/model aynı yerde, renk aynı yerde.

   ARAÇ SATIRINDAN FARKI, altına eklenen tek satır: ne yapıldığı.
   İçerideki kayıt, araç listesindeki gibi ÇERÇEVEDEN belli — "içeride"
   yazısı yok (K-019.2'nin kuralı burada da geçerli). */
function kayitKartiHtml(k) {
  var a = aracBul(k.plaka) || { marka: '', model: '', motor: '', renk: '', sahipAd: '' };
  var icerde = !k.teslimTarihi;
  var motor = a.motor ? '<span class="ak-motor">' + a.motor + '</span>' : '';
  return '<div class="arac-kart' + (icerde ? ' icerde' : '') + '"' +
      ' data-kayit="' + k.id + '" role="button" tabindex="0"' +
      ' aria-label="' + k.plaka + ' — ' + TUR_AD[k.tur] + '">' +
    '<div class="ak-ust">' + plakaHtml(k.plaka, 'plate-lg') + '</div>' +
    '<div class="ak-arac">' +
      '<span>' + a.marka + ' ' + a.model + '</span>' + motor +
      renkKutuHtml(a.renk) +
    '</div>' +
    (a.sahipAd ? '<div class="ak-sahip"><b>' + a.sahipAd + '</b></div>' : '') +
    /* İŞ SATIRI — türü ve ne yapıldığı. Türün kendi rengi var:
       bakım içeren iş aksan, tamirat birincil (v1'in şerit kuralı). */
    '<div class="ak-is">' +
      '<span class="pill ' + (bakimIceriyor(k) ? 'accent' : 'info') + '">' +
        TUR_AD[k.tur] + '</span>' +
      (k.sikayet ? '<span class="ak-is-ne">' + k.sikayet + '</span>' : '') +
    '</div>' +
    '<div class="ak-alt">' +
      (k.teslimTarihi
        ? '<span class="pill ok">teslim edildi</span>'
        : (fiyatiBekleyen(k)
            ? '<span class="pill warn">parçası bekleniyor</span>' : '')) +
      (kayitToplami(k)
        ? '<span class="tl-date">' + tutar(kayitToplami(k)) + ' ₺</span>' : '') +
      '<span class="tl-date">' + (k.teslimTarihi || k.acilis || '—') + '</span>' +
    '</div>' +
  '</div>';
}

/* ---- TELEFON BAĞLANTISI ----
   Beş yerde elle yazılıydı ve beşi birbirinden ayrışmaya başlamıştı
   (birinde data-tel yoktu ve numaraya dokunmak hem arıyor hem kaydı
   açıyordu). Ortak nesne kuralı telefona da işliyor. */
function telLinkHtml(tel, kimin) {
  return '<a class="tel-link" href="tel:' + String(tel).replace(/\s/g, '') + '"' +
    ' data-tel="1" aria-label="' + (kimin || 'numarayı') + ' ara">' +
    '<svg class="ico ico-sm"><use href="#i-phone"></use></svg>' + tel + '</a>';
}

/* ---- SERVİS KAYDI SATIRI ----
   v1'in .tl-item zaman tüneli satırı. Ana ekranda ve araç kartında
   AYNI nesne. v1'de dört ziyaret tipi vardı; v2'de iki tür var
   (K-006), o yüzden sol ikon ve şerit iki değer alıyor. */
/* ⚠️ 05.09 — ÜÇÜNCÜ TÜR: "Tamirat + Bakım" (Cemil).
   K-006 iki tür tanımlamıştı (tamirat | bakim) ve gerekçesi sağlamdı:
   ikisi farklı davranıyor (km, hatırlatma). Ama gerçek ziyaret ikisi
   birden olabiliyor — araç triger için geliyor, madem açıldı bakımı da
   yapılıyor. İki ayrı kayıt açmak ustayı aynı aracı iki kez yazmaya
   zorluyordu.

   `ikisi` BAKIM SAYILIR: km sorulur, hatırlatma kurulur. Kural tek
   cümle — "bakım içeriyorsa bakım kuralları işler" (bakimIceriyor). */
var TUR_IKON = { tamirat: 'i-wrench', bakim: 'i-clock', ikisi: 'i-wrench' };
var TUR_AD = { tamirat: 'Tamirat', bakim: 'Bakım', ikisi: 'Tamirat + Bakım' };
var ODEME_ETIKET = { nakit: 'Nakit', kart: 'Kart', havale: 'Havale', almadim: 'Alınmadı' };

function kayitSatiriHtml(k, s) {
  s = s || {};
  var a = aracBul(k.plaka);

  /* ---- SADE HÂL (04.09, Cemil — araç ekranındaki "Geçmiş" listesi) ----
     "Başında icon yok. Bakım mı tamirat mı başlık. Marka model
     yazmasın. Ad soyad zaten belli. Başlık, açıklama, Tarih, teslim
     durumu. Basit olsun."

     Neden AYRI BİR NESNE DEĞİL de aynı nesnenin bir hâli: satır aynı
     satır, gösterdiği alan sayısı farklı. İkinci bir çizim yazmak,
     ortak nesne kuralını (K-008 komşusu) kırmak olurdu — tıklama
     davranışı, tür şeridi ve veri okuma tek yerde kalıyor.

     ⚠️ BURADA BİR ŞEY KAYBOLDU — "parçayı müşteri getirmişti" rozeti
     bu listeden düştü. K-005'in araç ekranındaki görünür yeri oydu.
     Cemil'in sözü bizim gerekçemizi yener (K-019); bilgi kaybolmadı,
     kaydın kendi ekranında duruyor. */
  if (s.sade) {
    return '<div class="tl-item t-' + (bakimIceriyor(k) ? 'bakim' : 'tamirat') + '"' +
        ' data-kayit="' + k.id + '" role="button" tabindex="0">' +
      '<div class="tl-body">' +
        '<div class="tl-title"><span class="tl-tip">' + TUR_AD[k.tur] + '</span></div>' +
        '<div class="tl-sub">' + (k.sikayet || '—') + '</div>' +
        '<div class="tl-foot">' +
          (k.teslimTarihi
            ? '<span class="pill ok">teslim edildi</span>'
            : '<span class="pill accent">içeride</span>') +
          '<span class="tl-date">' + (k.teslimTarihi || k.acilis || '—') + '</span>' +
        '</div>' +
      '</div>' +
    '</div>';
  }

  var bekliyor = fiyatiBekleyen(k);
  /* K-005'İN ASIL İŞE YARADIĞI YER: "bu parçayı geçen sefer müşteri
     getirmişti" bilgisi satırın KENDİSİNE ait, ekrana değil. Araç
     geçmişinde de ana listede de aynı yerde görünmeli — yoksa
     araç ikinci kez geldiğinde tartışma çıkar. */
  var musteriParcalari = (k.satirlar || []).filter(function (l) {
    return l.tip === 'parca' && l.kimGetirdi === 'musteri';
  });
  /* Şerit ve ikon rengi TÜRÜ söyler (v1 kuralı). Durum rozeti yok —
     v2'de tek durum var ve o da teslim (K-004). */
  return '<div class="tl-item t-' + (bakimIceriyor(k) ? 'bakim' : 'tamirat') + '"' +
      ' data-kayit="' + k.id + '" role="button" tabindex="0">' +
    '<span class="tl-ic">' +
      '<svg class="ico ico-sm"><use href="#' + TUR_IKON[k.tur] + '"></use></svg></span>' +
    '<div class="tl-body">' +
      '<div class="tl-title">' +
        '<span class="tl-tip">' + TUR_AD[k.tur] + '</span>' +
        (s.plaka === false ? '' : plakaHtml(k.plaka)) +
      '</div>' +
      '<div class="tl-sub">' + (k.sikayet || '—') + '</div>' +
      /* SATIR DAHA ÇOK ŞEY SÖYLÜYOR (Cemil, 03.09: "servis kaydına
         girmeden birçok bilgiye ulaşabilsin"). Ortak nesne olduğu için
         bu detay üç listede birden düzeliyor; her listeye ayrı yazmak
         üç ayrı görünüm demek olurdu. */
      '<div class="tl-veri">' +
        (a ? '<span><b>' + a.marka + ' ' + a.model + '</b>' +
             (a.yil ? ' ' + a.yil : '') + '</span>' : '') +
        (a && a.sahipAd ? '<span>' + a.sahipAd + '</span>' : '') +
        (k.km ? '<span><b>' + k.km + '</b> km</span>' : '') +
        ((k.satirlar || []).length
          ? '<span><b>' + k.satirlar.length + '</b> satır</span>' : '') +
        '<span>Açılış <b>' + (k.acilis || '—') + '</b></span>' +
      '</div>' +
      /* Telefon LİSTEDE aranabilir: usta "kimi arayacağım" sorusunun
         cevabını satırda görüp oradan arıyor (v1'deki gibi). */
      /* s.tel === false: aracın KENDİ ekranında sahibin telefonu zaten
         tepede duruyor; her geçmiş satırında tekrar etmesi aynı
         numarayı dört kez gösteriyordu (04.09). */
      (s.tel !== false && a && a.sahipTel
        ? telLinkHtml(a.sahipTel, a.sahipAd || 'sahibini')
        : '') +
      '<div class="tl-foot">' +
        (bekliyor
          ? '<span class="pill warn"><svg class="ico ico-sm"><use href="#i-clock"></use></svg> parçası bekleniyor</span>'
          : '') +
        (k.teslimTarihi
          ? '<span class="pill ok">teslim edildi</span>'
          : '<span class="pill accent">içeride</span>') +
        (s.tutar !== false && kayitToplami(k)
          ? '<span class="tl-date">' + tutar(kayitToplami(k)) + ' ₺</span>' : '') +
        (k.odemeYolu
          ? '<span class="tl-date">' + (ODEME_ETIKET[k.odemeYolu] || k.odemeYolu) + '</span>' : '') +
        (k.teslimTarihi ? '<span class="tl-date">' + k.teslimTarihi + '</span>' : '') +
      '</div>' +
      (musteriParcalari.length
        ? '<div class="tl-foot"><span class="pill muted">parçayı müşteri getirmişti: ' +
            musteriParcalari.map(function (l) { return l.ad || 'parça'; }).join(', ') +
          '</span></div>'
        : '') +
    '</div>' +
  '</div>';
}

/* ---- ORTAK NESNENİN DAVRANIŞI DA ORTAK ----
   Satır iki ekranda kullanılıyor; tıklamayı her ekrana ayrı yazmak
   nesneyi ikiye bölmek olurdu. */
document.addEventListener('click', function (e) {
  /* Telefon bağlantısı satırın İÇİNDE ama satırın işi değil: numaraya
     dokunan aramak istiyor, kaydı açmak değil. Bu kontrol olmadan
     arama başlıyor VE kayıt ekranı açılıyordu. */
  if (e.target.closest('[data-tel]')) { e.stopPropagation(); return; }
  var t = e.target.closest('[data-kayit]');
  if (!t) return;
  ACIK_KAYIT = parseInt(t.getAttribute('data-kayit'), 10);
  Ekran.ac('kayit');
});

/* Plakaya dokunmak ARACI ACAR — araclar listesinde de musteri
   listesinde de. Iki ekrana ayri yazmak nesneyi ikiye bolerdi. */
document.addEventListener('click', function (e) {
  if (e.target.closest('[data-tel]')) { e.stopPropagation(); return; }
  var t = e.target.closest('[data-arac]');
  if (!t) return;
  ACIK_PLAKA = t.getAttribute('data-arac');
  Ekran.ac('arac');
});
