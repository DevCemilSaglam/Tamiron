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

/* ---- MÜŞTERİ SATIRI (14.09) ----
   Cemil, telefon testi: *"Müşteriye mesaj ve müşteriler sayfasının
   listelemeleri aynı olacak. Soldaki ikon olmayacak. Servisteki araçlar
   üstte ve renkli olacak. Müşteri adı, altına plaka aynı stilde,
   altında araç marka model ve renk kutucuğu. Altta telefon, altta varsa
   not. Birden fazla aracı olanlar için o an serviste olan ilk araç
   plakası ve araç marka. Birden fazla ise kaç araç ve tüm araçlar
   linki."*

   İKİ EKRAN, TEK SATIR. Müşteri listesi ve mesaj ekranının "kime?"
   listesi bugüne kadar iki ayrı yerde çiziliyordu: biri `.tl-item`
   giyip plakaları düz metin yazıyordu, öteki ARAÇ BAŞINA bir satır
   üretiyordu. Aynı şeyin iki görüntüsü. Ortak nesne kuralı: satır
   burada çizilir.

   OKUMA SIRASI (Cemil'in tarif ettiği sıra, değiştirilmedi):
     1) müşteri adı   — listenin sorusu "kim", plaka ikinci
     2) plaka         — araç kartındaki plakanın aynısı (plakaHtml)
     3) marka/model + renk kutucuğu
     4) telefon       — dokununca arar
     5) varsa not
     6) birden çok araç varsa: kaç araç + "Tüm araçlar"

   ⚠️ SATIRIN İŞİ EKRANA GÖRE DEĞİŞİR, SATIRIN KENDİSİ DEĞİL.
   `hedef` yalnız satırın taşıdığı veri anahtarını seçiyor:
     'musteri' → müşteri detayını açar    (müşteriler listesi)
     'mesaj'   → o araca mesaj yazdırır   (mesaj ekranı)
   Çizim tek, davranış iki — ikisi de zaten var olan delegasyonlar. */
function musteriKartiHtml(m, hedef) {
  var a = musteriAraci(m);
  var serviste = musteriServiste(m);
  var cokArac = m.araclar.length > 1;
  /* Mesaj ekranında satır ARACA gider: hitap, şablon ve telefon araçtan
     üretiliyor (mesajHedefi). Araçsız müşteri listede zaten görünmez. */
  var anahtar = (hedef === 'mesaj')
    ? ' data-mesaj-plaka="' + (a ? a.plaka : '') + '"'
    : ' data-musteri="' + m.anahtar + '"';

  return '<div class="mus-kart' + (serviste ? ' serviste' : '') + '"' + anahtar +
      ' role="button" tabindex="0" aria-label="' + m.ad +
      (a ? ' — ' + a.plaka : '') + '">' +
    '<div class="mus-ad">' + m.ad + '</div>' +
    (a ? plakaHtml(a.plaka, 'plate-lg') : '') +
    (a ? '<div class="mus-arac"><span>' + a.marka + ' ' + a.model + '</span>' +
         renkKutuHtml(a.renk) + '</div>' : '') +
    (m.tel ? telLinkHtml(m.tel, m.ad)
           : '<div class="mus-arac"><span class="pill warn">telefon yok</span></div>') +
    /* NOT — varsa. "Arka cam kolu kırık, müşteri biliyor" gibi bir not,
       ustanın müşteriyle konuşmadan önce bilmek istediği tek şey. */
    ((a && a.not) ? '<div class="mus-not">' + a.not + '</div>' : '') +
    /* BİRDEN FAZLA ARAÇ: satır bir aracı gösteriyor, ötekiler
       kaybolmasın. Link müşteri detayına gider — araçların tamamı
       zaten orada listeleniyor, ikinci bir liste açılmadı. */
    (cokArac
      ? '<div class="mus-cok">' + m.araclar.length + ' araç' +
          '<button class="mus-link" data-musteri="' + m.anahtar + '">' +
            'Tüm araçlar' +
            '<svg class="ico ico-sm"><use href="#i-chevron-right"></use></svg>' +
          '</button>' +
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
    /* ⚠️ 06.09 — TELEFON KARTA GİRDİ (Cemil: "Ayrıca ruhsat sahibi ya
       da iletişim telefon no da olsun ve aranabilsin").

       HANGİ NUMARA: mesajHedefi() ne diyorsa o. Ruhsat firmanınsa
       santral değil, aracı getiren kişi aranır (A-M2). İki ayrı yerde
       iki ayrı "kimi ararız" kuralı olsaydı, listeden aranan kişiyle
       mesaj gönderilen kişi farklı olurdu. */
    (function () {
      var h = mesajHedefi(k.plaka);
      return (h && h.tel) ? telLinkHtml(h.tel, h.ad) : '';
    })() +
    /* ---- AYRAÇ ALTINDAKİ ÜÇ SATIR (Cemil, 06.09) ----
       "Araç sahibi adı altında border'ın altında solda Bakım ya da
        Tamirat yazıyor, o satırın sağında durum olsun. Bu satırın
        altında açıklama olsun. En altta sağda tutar, solda tarih."

       ÜST YARISI ARAÇ, ALT YARISI İŞ. Ayraç ikisini ayırıyor: göz
       önce hangi aracı okuyor, sonra o araçta ne olduğunu. Eskiden
       tür, açıklama ve durum aynı satırda yarışıyordu ve uzun bir
       şikâyet metni durumu bir alt satıra itiyordu — aynı listede
       satırdan satıra farklı bir düzen. */
    '<div class="ak-is">' +
      '<span class="pill ' + (bakimIceriyor(k) ? 'accent' : 'info') + '">' +
        TUR_AD[k.tur] + '</span>' +
      /* ⚠️ İKİ AYRI GERÇEK, İKİ AYRI ROZET (06.09, kuyruk 4).
         Dün ikisini `||` ile tek roze te bindirmiştim ve kart, fiyatı
         girilmemiş bir satır yüzünden *"parça bekliyor"* diyordu —
         parça tezgâhta dururken. Yanlış durum göstermek, durum
         göstermemekten kötüdür (K-013'ün kuralı).

           parcaBekliyor      → parça YOLDA        (usta işaretler)
           fiyatiBekleyen()   → FİYAT gelmemiş     (türetilir)

         İkisi aynı anda doğru olabilir; o zaman iki rozet birden
         çıkar ve bu doğrudur. */
      (!k.teslimTarihi && k.parcaBekliyor
        ? '<span class="pill warn">parça bekliyor</span>' : '') +
      (!k.teslimTarihi && fiyatiBekleyen(k)
        ? '<span class="pill muted">fiyat bekleniyor</span>' : '') +
      /* DURUM SAĞDA. Kelime **"Serviste"** — Cemil, 07.09:
         *"İçeride kelimesi yerine serviste kelimesi kullanılacak."*
         Araç listesinde çerçeve yetiyordu (K-019.2),
         çünkü orada tek soru "bu araç şu an bizde mi". Servis
         listesinde iki durum var (içeride · teslim edildi) ve ikisi
         de yazıyla söyleniyor; biri yazı öbürü çerçeve olsaydı usta
         ikisini karşılaştıramazdı. */
      '<span class="ak-durum">' +
        (k.teslimTarihi
          ? '<span class="pill ok">Teslim edildi</span>'
          : '<span class="pill accent">Serviste</span>') +
      '</span>' +
    '</div>' +
    '<div class="ak-is-ne">' + (k.sikayet || 'ne yapılacağı yazılmamış') + '</div>' +
    '<div class="ak-alt">' +
      '<span class="tl-date">' + (k.teslimTarihi || k.acilis || '—') + '</span>' +
      '<span class="ak-tutar">' +
        (kayitToplami(k) ? tutar(kayitToplami(k)) + ' ₺' : '—') + '</span>' +
    '</div>' +
  '</div>';
}

/* ---- MESAJIN METNİ (14.09'da mesaj ekranından BURAYA taşındı) ----
   Cemil: *"Buradan mesaj sesli olarak gönderebilmeliyiz."*

   Metni bugüne kadar yalnız mesaj ekranı üretiyordu ve ekran
   betikleri derlemede kendi IIFE'sine sarılıyor — sesli işlem o
   fonksiyonu GÖREMİYORDU. Sesli yol mesaj göndermeye başlayınca iki
   seçenek vardı: metni ikinci kez yazmak ya da metni ortak nesne
   yapmak. Birincisi, ustanın onay ekranında gördüğü metinle gerçekte
   giden metnin ayrılması demekti — ve bunu ancak müşteri sorunca
   öğrenirdi.

   `hedefAnahtar` ekranın durumu değil, PARAMETRE: mesaj ekranı hangi
   hedefi seçtiyse onu geçiyor, ses ekranı varsayılanı bırakıyor.
   Ekran durumunu okuyan bir ortak nesne, ortak olmaz. */
function mesajMetniUret(plaka, sablon, hedefAnahtar) {
  var a = aracBul(plaka);
  if (!a) return '';
  /* HİTAP HEDEFE GÖRE (A-M2, 04.09). Önceden hep ruhsat sahibine
     yazıyordu ve firma ruhsatlı araçta mesaj "Demir Nakliyat Ltd.
     Şti. Bey/Hanım," diye başlıyordu — müşteriye giden İLK cümle bu. */
  var basi = mesajHitabi(plaka, hedefAnahtar);
  var arac = a.marka + ' ' + a.model + ' (' + plaka + ')';

  if (sablon === 'bekliyor') {
    /* SÖZ VERİLMİYOR. "Yarın hazır olur" demiyoruz: parçanın ne zaman
       geleceğini biz de bilmiyoruz ve tutulmayan söz, hiç verilmemiş
       sözden kötüdür. Söylenen tek şey doğru olan: parça bekleniyor,
       gelince haber verilecek. */
    return basi + arac + ' için parça bekliyoruz. ' +
           'Parça gelir gelmez işe başlayıp size haber vereceğiz.' + mesajImzasi();
  }
  if (sablon === 'hazir') {
    return basi + arac + ' hazır, teslim alabilirsiniz.' + mesajImzasi();
  }
  /* TESLİM EDİLDİ (14.09) — "Hazır"dan farkı ZAMAN: biri teslimden
     önce, öteki sonra. Tek şablona bindirilseydi metin yalan söylerdi —
     teslim alınmış araca "teslim alabilirsiniz" yazmak gibi. */
  if (sablon === 'teslim') {
    var tk = sonKayit(plaka);
    var tl = (tk && kayitToplami(tk)) ? 'Toplam: ' + tutar(kayitToplami(tk)) + ' ₺\n' : '';
    return basi + arac + ' teslim edildi.\n' + tl +
           (tk ? 'Ayrıntılı özet: ' + ozetLinki(tk) + '\n' : '') +
           '\nBizi tercih ettiğiniz için teşekkür ederiz.' + mesajImzasi();
  }
  /* HAZIR METİNLER — "selam mesajı", "bayram mesajı" (14.09). Sesli yol
     bunları isimle istiyor; mesaj ekranı da aynı metni üretebilsin. */
  if (SABLON_AD[sablon] && hazirMesajMetni(sablon, plaka)) {
    return hazirMesajMetni(sablon, plaka);
  }
  /* SERBEST — metni usta söyleyecek ya da yazacak. Boş bırakmıyoruz:
     hitap ve imza zaten biliniyor, ustanın yazacağı yalnız orta kısım. */
  if (sablon === 'serbest') {
    return basi + mesajImzasi();
  }
  if (sablon === 'hatirlatma') {
    var sb = sonBakim(plaka);
    var ek = sb ? 'Son bakımı ' + sb.teslimTarihi +
                  (sb.km ? ' · ' + sb.km + ' km' : '') + ' idi. ' : '';
    return basi + arac + ' için bakım zamanı geldi. ' + ek +
           'Uygun olduğunuzda bekleriz.' + mesajImzasi();
  }
  /* özet — açık ya da son kayıttan üretilir */
  var k = acikKayit(plaka) || sonKayit(plaka);
  if (!k) return basi + arac + ' için henüz bir kayıt yok.';
  /* ⚠️ 04.09 — MESAJ ARTIK LİSTEYİ TAŞIMIYOR, LİNKİ TAŞIYOR (T-20).
     Tamirci bugün PDF gönderiyor (S8). PDF ölü bir dosyadır: gönderildiği
     anki hâli taşır, iş sürerken güncellenmez ve müşteri onu bir daha
     bulamaz. Link canlıdır — müşteri istediği an bakar, güncelini görür.

     LİSTE YİNE DE ÖZETLENİYOR: WhatsApp'ta linke dokunmayan da olur;
     kalem sayısı ve toplam mesajın içinde duruyor ki link açılmasa bile
     mesaj bir işe yarasın. Tam liste sayfada. */
  var adet = (k.satirlar || []).length;
  return basi + arac + ' için ' + (adet ? adet + ' kalem ' : '') +
         'işlem yapıldı.\nToplam: ' + tutar(kayitToplami(k)) + ' ₺\n\n' +
         'Ayrıntılı özet: ' + ozetLinki(k) + mesajImzasi();
}

/* ---- GÖNDERİLEN MESAJ SATIRI (14.09) ----
   Cemil: *"Gönderilen mesajlar listelenebilmeli."*

   Satır dört soruyu cevaplıyor — kime, hangi araç için, ne, ne zaman.
   Beşincisi (metin) ilk satırıyla duruyor: usta hangi mesaj olduğunu
   metnin başından tanıyor, tamamını okumak istemiyor.

   ⚠️ TIKLANMIYOR ve bu bilinçli: satırın açacağı bir şey yok. Gönderilmiş
   bir mesaj değiştirilemez — dokununca bir şey olmayan satır, ölü uçtur
   (K-021); dokunulacak gibi DURMAYAN satır değildir. */
function mesajSatiriHtml(m) {
  var ilkSatir = (m.metin || '').split('\n')[0];
  if (ilkSatir.length > 90) ilkSatir = ilkSatir.slice(0, 90) + '…';
  return '<div class="tl-item">' +
    '<div class="tl-body">' +
      '<div class="tl-title"><span class="tl-tip">' + (m.ad || 'kime yazılmamış') +
        '</span>' + (m.plaka ? plakaHtml(m.plaka) : '') + '</div>' +
      '<div class="tl-sub">' + ilkSatir + '</div>' +
      '<div class="tl-foot">' +
        '<span class="pill info">' + (KANAL_AD[m.kanal] || m.kanal) + '</span>' +
        '<span class="pill muted">' + (SABLON_AD[m.sablon] || m.sablon) + '</span>' +
        (m.toplu ? '<span class="pill warn">toplu</span>' : '') +
        '<span class="tl-date">' + m.tarih + ' · ' + m.saat + '</span>' +
      '</div>' +
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

  /* ---- 04.09, Cemil (araç ekranındaki "Geçmiş" listesi) ----
     "Başında icon yok. Bakım mı tamirat mı başlık. Marka model
     yazmasın. Ad soyad zaten belli. Başlık, açıklama, Tarih, teslim
     durumu. Basit olsun."

     ⚠️ 06.09 — BU FONKSİYONUN "TAM" HÂLİ SİLİNDİ (kuyruk 4).
     İki hâli vardı: `sade` ve tam. Tam hâl marka/model, sahibi, km,
     satır sayısı, telefon, ödeme yolu ve *"parçayı müşteri
     getirmişti"* rozetini çiziyordu — yaklaşık 60 satır. **Hiçbir yer
     onu çağırmıyordu.** Tek çağrı `{sade:true}` ile ve tek yerden
     (araç ekranı); ana liste 05.09'da `kayitKartiHtml`e geçmişti ve
     tam hâl o gün ulaşılamaz kaldı.

     Ulaşılamaz kod, ölü CSS'ten daha sinsidir: denetimin OLU-SINIF
     testi sınıfları tarıyor, **çağrılmayan bir JS dalını göremiyor.**
     Duruyor, okunuyor, "bu da var" sanılıyor ve çürüyor — nitekim
     içindeki `s.tel`, `s.tutar`, `s.plaka` seçeneklerini kimse
     geçmiyordu.

     *"İleride lazım olur"* gerekçe değildir (CLAUDE.md). Tam hâl
     gerekirse yeniden yazılır; A-M3 hâlâ açık ve cevabı *"evet, o
     rozet gereksin"* olursa **o zaman** yazılacak. */
  return '<div class="tl-item t-' + (bakimIceriyor(k) ? 'bakim' : 'tamirat') + '"' +
      ' data-kayit="' + k.id + '" role="button" tabindex="0">' +
    '<div class="tl-body">' +
      '<div class="tl-title"><span class="tl-tip">' + TUR_AD[k.tur] + '</span></div>' +
      '<div class="tl-sub">' + (k.sikayet || '—') + '</div>' +
      '<div class="tl-foot">' +
        (k.teslimTarihi
          ? '<span class="pill ok">teslim edildi</span>'
          : '<span class="pill accent">serviste</span>') +
        /* Araç geçmişinde de iki bekleme AYRI (kuyruk 4). */
        (!k.teslimTarihi && k.parcaBekliyor
          ? '<span class="pill warn">parça bekliyor</span>' : '') +
        '<span class="tl-date">' + (k.teslimTarihi || k.acilis || '—') + '</span>' +
      '</div>' +
    '</div>' +
  '</div>';
}


/* ---- MÜŞTERİNİN GÖRDÜĞÜ GÖVDE (07.09) ----
   Cemil: *"Müşteri ne görecek popup açılsın. Sadece mesaj gözüksün."*

   Bu gövde İKİ YERDE çiziliyor artık: `ozet` ekranında (usta tam
   sayfayı görsün diye) ve mesaj ekranındaki KATMANDA (hızlı bakış).
   Ortak nesne kuralı: bir nesne bir yerde çizilir. İki kopya olsaydı
   biri düzelir öteki kalırdı — bu turda tam olarak bunun üç örneğini
   gördük.

   ⚠️ ÖNİZLEME ŞERİDİ BURADA YOK. *"Müşterinin gördüğü sayfa. Bu şerit
   yalnız sende görünüyor"* satırı ekranın kendi işi; katmanda
   gösterilseydi *"sadece müşterinin gördüğü"* olmazdı. Gövde yalnız
   müşterinin gerçekten göreceğini üretir. */
function ozetGovdeHtml(k) {
  if (!k) return '';
  var a = aracBul(k.plaka);

  /* DÜKKÂN ÜSTTE: müşteri sayfayı WhatsApp'tan açıyor ve ilk sorusu
     "bu kimden geldi". Telefon aranabilir — sorusu varsa arayacak. */
  var dukkan = '<div class="card"><div class="ozet-bas">' +
    '<div class="oz-ad">' + DUKKAN.ad + '</div>' +
    (DUKKAN.adres ? '<div class="oz-alt">' + DUKKAN.adres + '</div>' : '') +
    (DUKKAN.tel ? telLinkHtml(DUKKAN.tel, DUKKAN.ad) : '') +
  '</div></div>';

  var arac = a
    ? '<div class="card">' +
        '<div class="arac-bas">' + plakaHtml(a.plaka, 'plate-lg') +
          '<div class="veh-title">' + a.marka + ' ' + a.model +
            (a.yil ? ' <span class="yr">' + a.yil + '</span>' : '') + '</div>' +
          '<div class="spec-strip">' +
            '<span class="spec-val">' + TUR_AD[k.tur] + '</span>' +
            (k.km ? '<span class="spec-sep">·</span><span class="spec-val">' +
              k.km + ' km</span>' : '') +
          '</div>' +
        '</div>' +
        /* ⚠️ SATIR SONLARI KORUNUYOR (K-038.1). "Açıklama" alanı çok
           satırlı; HTML satır sonlarını boşluğa çeviriyor ve müşteri
           üç ayrı işi tek cümle hâlinde okuyordu. */
        (k.sikayet
          ? '<p class="plate-hint">' + k.sikayet.replace(/\n/g, '<br>') + '</p>'
          : '') +
      '</div>'
    : '';

  /* ⚠️ YARIM SATIR MÜŞTERİYE GÖSTERİLMEZ (K-038.2).
     Adı DA fiyatı DA yoksa satır yok sayılır. Biri varsa gösterilir —
     adı olup fiyatı olmayan satır gerçek bir iştir, fiyatı olup adı
     olmayan satır gerçek bir paradır. Ustanın ekranında ELENMEZ:
     orada yarım satır görünmeli ki doldurulsun ya da silinsin. */
  var satirlar = (k.satirlar || []).filter(function (s) {
    return (s.ad && s.ad.trim()) || s.fiyat;
  });
  var kalemler = '<div class="section-label">' +
      '<svg class="ico ico-sm"><use href="#i-clipboard-list"></use></svg> Yapılan işler' +
    '</div>' +
    (satirlar.length
      ? '<div class="tutar-serit">' +
          satirlar.map(function (s) {
            var etiket = s.ad || (s.tip === 'iscilik' ? 'İşçilik' : 'Parça');
            var ek = (s.tip === 'parca' && s.marka)
              ? ' <span class="oz-marka">' + s.marka +
                (s.kod ? ' · ' + s.kod : '') + '</span>' : '';
            var musterinin = (s.tip === 'parca' && s.kimGetirdi === 'musteri');
            /* ⚠️ MÜŞTERİ PARÇASINDA RAKAM YOK — fiyat kayıtta dursa bile
               (K-037). Ödemediği bir parçanın yanında rakam görmek,
               ondan ücret isteniyormuş gibi okunur; toplam da onu
               içermiyor. Ekranda olup toplamda olmayan bir rakam,
               müşteri açısından açıklanamaz bir çelişkidir. */
            return '<div class="tutar-sat"><span>' + etiket + ek +
              (musterinin ? ' <span class="pill muted">parçayı siz getirdiniz</span>' : '') +
              '</span><span class="tv">' +
              (musterinin || !s.fiyat ? '—' : tutar(s.fiyat) + ' ₺') + '</span></div>';
          }).join('') +
          '<div class="tutar-sat genel"><span>Toplam</span><span class="tv">' +
            tutar(kayitToplami(k)) + ' ₺</span></div>' +
        '</div>'
      : bosDurumHtml('i-clipboard-list', 'Henüz iş yazılmadı',
          'Araç işleme alındığında yapılanlar burada görünecek.'));

  /* DURUM: müşterinin ikinci sorusu "aracım hazır mı".
     Tek durum var (K-004) ve burada da tek durum gösteriliyor.
     Parça beklendiği YALNIZ usta öyle seçtiyse söylenir (A-M15). */
  var durum = '<div class="notice ' + (k.teslimTarihi ? 'ok' : 'accent') + '">' +
      '<span class="n-ic"><svg class="ico ico-sm"><use href="#' +
        (k.teslimTarihi ? 'i-circle-check' : 'i-clock') + '"></use></svg></span>' +
      '<span class="n-body">' +
        (k.teslimTarihi
          ? 'Aracınız <b>' + k.teslimTarihi + '</b> tarihinde teslim edildi.' +
            (k.odemeYolu === 'almadim'
              ? ' Ödeme <b>alınmadı</b>.'
              : (k.odemeYolu ? ' Ödeme: <b>' + (ODEME_ETIKET[k.odemeYolu] ||
                  k.odemeYolu) + '</b>.' : ''))
          : (k.parcaBekliyor && k.bekliyorMusteriye
              ? 'Aracınız <b>serviste</b>, <b>parça bekleniyor</b>. ' +
                'Parça gelir gelmez işe başlayıp size haber vereceğiz.'
              : 'Aracınız <b>serviste</b>. Hazır olduğunda haber vereceğiz.')) +
      '</span>' +
    '</div>';

  return dukkan + arac + kalemler + durum;
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

/* Musteri satirina dokunmak MUSTERIYI ACAR (08.09). Kayit ve arac
   ile ayni kalip: satir veriyi tasir, davranis tek yerde durur. */
document.addEventListener('click', function (e) {
  if (e.target.closest('[data-tel]')) { e.stopPropagation(); return; }
  var t = e.target.closest('[data-musteri]');
  if (!t) return;
  ACIK_MUSTERI = t.getAttribute('data-musteri');
  Ekran.ac('musteri');
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
