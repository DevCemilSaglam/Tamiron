/* ============ ORTAK NESNELER ============
   BİR NESNE BİR YERDE ÇİZİLİR. Plaka üç ekranda görünüyor; üç kez
   yazılırsa üç kez ayrışır. v1'de bu kural sonradan konmuştu ve
   konana kadar plaka her ekranda biraz farklıydı.

   Ekran dosyaları bu fonksiyonları ÇAĞIRIR, kendi kopyasını yazmaz. */

/* ---- PLAKA — gerçek TR plakasının dili ---- */
function plakaHtml(p, kucuk) {
  return '<span class="plaka' + (kucuk ? ' plaka-sm' : '') + '">' +
           '<span class="plaka-tr">TR</span>' +
           '<span class="plaka-no">' + p + '</span>' +
         '</span>';
}

/* ---- SERVİS KAYDI SATIRI — ana ekranda ve araç kartında aynı ---- */
function kayitSatiriHtml(k) {
  var a = aracBul(k.plaka);
  var bekliyor = fiyatiBekleyen(k);
  return '<button class="kayit-sat" data-kayit="' + k.id + '">' +
    '<span class="kayit-ust">' +
      plakaHtml(k.plaka, true) +
      '<span class="kayit-tur ' + k.tur + '">' + (k.tur === 'bakim' ? 'Bakım' : 'Tamirat') + '</span>' +
    '</span>' +
    '<span class="kayit-arac">' + (a ? a.marka + ' ' + a.model : '') + '</span>' +
    '<span class="kayit-is">' + (k.sikayet || '—') + '</span>' +
    (bekliyor ? '<span class="kayit-not">parçası bekleniyor</span>' : '') +
    '</button>';
}

/* ---- PARA ---- */
function paraHtml(n) {
  return '<span class="para">' + tutar(n) + ' ₺</span>';
}

/* ---- BOŞ DURUM — kuru "sonuç yok" değil, ne yapılacağını söyler ---- */
function bosDurumHtml(baslik, aciklama) {
  return '<div class="bos-durum">' +
    '<strong>' + baslik + '</strong>' +
    '<span>' + aciklama + '</span>' +
  '</div>';
}

/* ---- ORTAK NESNENİN DAVRANIŞI DA ORTAK ----
   kayitSatiriHtml() iki ekranda kullanılıyor (ana ekran, araç kartı).
   Tıklama davranışını her ekrana ayrı yazmak, nesneyi iki yere
   bölmek olurdu. Nesne neredeyse davranışı da orada. */
document.addEventListener('click', function (e) {
  var t = e.target.closest('[data-kayit]');
  if (!t) return;
  ACIK_KAYIT = parseInt(t.getAttribute('data-kayit'), 10);
  Ekran.ac('kayit');
});
