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

/* ---- SERVİS KAYDI SATIRI ----
   v1'in .tl-item zaman tüneli satırı. Ana ekranda ve araç kartında
   AYNI nesne. v1'de dört ziyaret tipi vardı; v2'de iki tür var
   (K-006), o yüzden sol ikon ve şerit iki değer alıyor. */
var TUR_IKON = { tamirat: 'i-wrench', bakim: 'i-clock' };
var TUR_AD = { tamirat: 'Tamirat', bakim: 'Bakım' };
var ODEME_ETIKET = { nakit: 'Nakit', kart: 'Kart', havale: 'Havale', almadim: 'Alınmadı' };

function kayitSatiriHtml(k, s) {
  s = s || {};
  var a = aracBul(k.plaka);
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
  return '<div class="tl-item t-' + (k.tur === 'bakim' ? 'bakim' : 'tamirat') + '"' +
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
        ? '<a class="tel-link" href="tel:' + a.sahipTel.replace(/\s/g, '') + '"' +
            ' data-tel="1" aria-label="' + (a.sahipAd || 'sahibini') + ' ara">' +
            '<svg class="ico ico-sm"><use href="#i-phone"></use></svg>' +
            a.sahipTel + '</a>'
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
