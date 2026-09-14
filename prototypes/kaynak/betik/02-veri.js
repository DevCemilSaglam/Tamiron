/* ============ SAHTE VERİ ============
   Backend yok. Alanlar VERI-MODELI.md ile BİREBİR — modelde olmayan
   alan buraya da girmez. Modelde olmayan bir alana ihtiyaç duyulursa
   önce VERI-MODELI.md açılır.

   ⚠️ Ruhsatta TC kimlik no ve adres VAR ama burada YOK
   (RUHSAT-OKUMA.md Bulgu 2: tutmadığın veri en güvenli veridir). */

var BUGUN = new Date(2026, 8, 3);   /* prototipin "bugün"ü — TEK KAYNAK */

/* ---- Vehicle (VERI-MODELI §1) ---- */
var ARACLAR = [
  { plaka: '34 ABC 123', marka: 'Renault', model: 'Mégane', yil: '2018',
    motor: '1.5 dCi', vin: 'VF1RFB00858123456',
    /* "Gri Metalik" — ruhsatta renk TEK KELİME OLMAYABİLİR. Renk
       kutucuğu bu yüzden boşluktan bölüp parça parça bakıyor
       (renkSinifi, K-018). Tek kelime beklenseydi kutucuk boş
       çıkardı ve boş kutucuk "beyaz" diye okunurdu. */
    renk: 'Gri Metalik', yakit: 'Dizel', motorNo: 'K9K872C123456',
    silindir: '1461', guc: '110', koltuk: '5', ilkTescil: '14.06.2018',
    depo: '50',
    sahipAd: 'Mehmet Kaya', sahipTel: '0532 215 30 00', not: 'Arka cam kolu kırık — müşteri biliyor.' },
  { plaka: '06 TMR 07', marka: 'Fiat', model: 'Egea', yil: '2021',
    motor: '1.4 Fire', vin: 'NM4358004ZT129041',
    renk: 'Beyaz', yakit: 'Benzin', motorNo: '843A1000098765',
    silindir: '1368', guc: '95', koltuk: '5', ilkTescil: '02.03.2021',
    depo: '45',
    /* RUHSAT SAHİBİ FİRMA (Cemil, 04.09: "Ruhsat sahibi firma da
       olabilir hani"). Firmanın numarası santraldir; aracı getiren
       ve aranacak kişi başkasıdır. İkinci iletişim olmadan usta
       santrali arayıp "hangi araç" diye anlatmak zorunda kalıyordu. */
    sahipAd: 'Demir Nakliyat Ltd. Şti.', sahipTel: '0312 395 12 00',
    iletisimAd: 'Hasan Demir', iletisimTel: '0542 331 90 07', not: '' },
  { plaka: '35 KLM 45', marka: 'Peugeot', model: '301', yil: '2019',
    motor: '1.6 BlueHDi', vin: 'VF3DDBHYAKS012345',
    renk: 'Lacivert', yakit: 'Dizel', motorNo: 'DV6FD10JBCE4455',
    silindir: '1560', guc: '100', koltuk: '5', ilkTescil: '21.11.2019',
    depo: '50',
    sahipAd: 'Ayşe Kaya', sahipTel: '', not: '' },
  { plaka: '16 XY 8890', marka: 'Volkswagen', model: 'Golf', yil: '2016',
    motor: '1.6 TDI', vin: 'WVWZZZAUZGW123456',
    renk: 'Siyah', yakit: 'Dizel', motorNo: 'CLH1234567',
    silindir: '1598', guc: '110', koltuk: '5', ilkTescil: '08.08.2016',
    depo: '50',
    sahipAd: 'Selim Aydın', sahipTel: '0537 662 08 41', not: '' }
];

/* ---- ServiceRecord (VERI-MODELI §2) ----
   `tur`: repair | maintenance (K-006)
   `teslimTarihi` DOLUYSA araç çıkmıştır — TEK DURUM ALANI (K-004).
   `Total` alanı YOK: satırlardan hesaplanır. */
var KAYITLAR = [
  /* PARÇA BEKLİYOR — prototipte GÖRÜNSÜN diye bir kayıtta açık (06.09).
     Çalıştığı görülemeyen bir alan, çalışmıyor sayılır. */
  { id: 1, plaka: '34 ABC 123', tur: 'tamirat', km: '',
    sikayet: 'Triger seti değişecek',
    acilis: '03.09.2026', teslimTarihi: null,
    odemeYolu: null, hatirlatmaTarihi: null, hatirlatmaGonderildi: false, parcaBekliyor: true, bekliyorMusteriye: true,
    satirlar: [
      { tip: 'parca', ad: 'Triger seti', fiyat: 0, alis: 0, kimGetirdi: 'tamirci',
        marka: '', kod: '', tanim: 'tanimlanamadi' }
    ] },

  { id: 2, plaka: '06 TMR 07', tur: 'bakim', km: '86.400',
    sikayet: 'Periyodik bakım',
    acilis: '02.09.2026', teslimTarihi: null,
    odemeYolu: null, hatirlatmaTarihi: null, hatirlatmaGonderildi: false, parcaBekliyor: false, bekliyorMusteriye: false,
    satirlar: [
      { tip: 'parca', ad: 'Motor yağı', fiyat: 1900, alis: 1450, kimGetirdi: 'tamirci',
        marka: 'Motul', kod: '8100 5W-30', tanim: 'kodlu' },
      { tip: 'parca', ad: 'Yağ filtresi', fiyat: 260, alis: 180, kimGetirdi: 'tamirci',
        marka: 'Mann', kod: 'W 71180', tanim: 'kodlu' },
      { tip: 'iscilik', ad: 'Bakım işçiliği', fiyat: 900, alis: 0, kimGetirdi: '' }
    ] },

  { id: 3, plaka: '35 KLM 45', tur: 'tamirat', km: '',
    /* ⚠️ 08.09 — AÇIKLAMADAN "parçayı müşteri getirdi" ÇIKARILDI.
       Cemil: *"Servis kayıtlarında yine parçayı müşteri getirdi
       yazıyor."* Doğrusu buydu: bilgi zaten satırın üstünde
       (kimGetirdi: 'musteri'). Açıklamaya da yazılınca aynı şey
       kayıt listesinde de, kayıt detayında da, müşterinin özet
       sayfasında da görünüyordu — örnek veri, abartının kendisiydi. */
    sikayet: 'Ön balata değişimi',
    acilis: '01.09.2026', teslimTarihi: null,
    odemeYolu: null, hatirlatmaTarihi: null, hatirlatmaGonderildi: false, parcaBekliyor: false, bekliyorMusteriye: false,
    satirlar: [
      { tip: 'parca', ad: 'Ön fren balatası', fiyat: 0, alis: 0, kimGetirdi: 'musteri',
        marka: '', kod: '', tanim: 'tanimlanamadi' },
      { tip: 'iscilik', ad: 'Balata değişimi', fiyat: 800, alis: 0, kimGetirdi: '' }
    ] },

  /* Teslim edilmiş — ana ekranda GÖRÜNMEZ, araç geçmişinde görünür.
     HATIRLATMASI GEÇMİŞTE: 15.03'te teslim edilmiş, 6 ay sonrası
     15.09'du ama tarih 03.09 — henüz gelmedi, rozet çıkmaz.
     Vadesi GELMİŞ bir hatırlatma aşağıda (id 5) duruyor; yoksa
     yedinci iş prototipte hiç görünmüyordu. */
  { id: 4, plaka: '34 ABC 123', tur: 'bakim', km: '142.500',
    sikayet: 'Periyodik bakım',
    acilis: '15.03.2026', teslimTarihi: '15.03.2026',
    odemeYolu: 'nakit', hatirlatmaTarihi: '15.09.2026', hatirlatmaGonderildi: false, parcaBekliyor: false, bekliyorMusteriye: false,
    satirlar: [
      { tip: 'parca', ad: 'Motor yağı', fiyat: 1800, alis: 1400, kimGetirdi: 'tamirci',
        marka: 'Motul', kod: '8100 5W-30', tanim: 'kodlu' },
      { tip: 'iscilik', ad: 'Bakım işçiliği', fiyat: 850, alis: 0, kimGetirdi: '' }
    ] },

  /* İKİNCİ KM ÖLÇÜMÜ — ay/km kuralı ancak iki ölçümle çalışıyor
     (03.09). Tek ölçümlü araçta günlük km tahmin edilmiyor; bu kayıt
     olmadan "hangisi önce gelirse" kuralının km yarısı prototipte
     hiç görünmüyordu. 15.03'te 142.500, 20.06'da 151.200 —
     97 günde 8.700 km. */
  { id: 6, plaka: '34 ABC 123', tur: 'bakim', km: '151.200',
    sikayet: 'Ara bakım',
    acilis: '20.06.2026', teslimTarihi: '20.06.2026',
    odemeYolu: 'havale', hatirlatmaTarihi: '20.12.2026', hatirlatmaGonderildi: false, parcaBekliyor: false, bekliyorMusteriye: false,
    satirlar: [
      { tip: 'parca', ad: 'Motor yağı', fiyat: 1950, alis: 1500, kimGetirdi: 'tamirci',
        marka: 'Motul', kod: '8100 5W-30', tanim: 'kodlu' },
      { tip: 'iscilik', ad: 'Bakım işçiliği', fiyat: 900, alis: 0, kimGetirdi: '' }
    ] },

  /* MUHASEBE EKRANI BOŞ GÖRÜNMESİN (04.09). Tohum veride bugün teslim
     edilmiş kayıt ve veresiye YOKTU; muhasebe ve ana menü "Bugün 0 ₺ ·
     0 ₺ veresiye" diyordu. Ekran doğru çalışıyordu ama gösterecek
     şey yoktu — çalıştığı görülemeyen bir ekran, çalışmıyor sayılır. */
  { id: 7, plaka: '35 KLM 45', tur: 'tamirat', km: '',
    sikayet: 'Ön cam silecek motoru',
    acilis: '03.09.2026', teslimTarihi: '03.09.2026',
    odemeYolu: 'nakit', hatirlatmaTarihi: null, hatirlatmaGonderildi: false, parcaBekliyor: false, bekliyorMusteriye: false,
    satirlar: [
      { tip: 'parca', ad: 'Silecek motoru', fiyat: 2100, alis: 1600, kimGetirdi: 'tamirci',
        marka: 'Valeo', kod: '579713', tanim: 'kodlu' },
      { tip: 'iscilik', ad: 'Söküm takım', fiyat: 700, alis: 0, kimGetirdi: '' }
    ] },

  /* VERESİYE — "almadım" bir ödeme yoludur (K-004). Muhasebedeki
     "kimden alacağım" listesi bu kayıttan besleniyor. */
  { id: 8, plaka: '06 TMR 07', tur: 'tamirat', km: '',
    sikayet: 'Akü değişimi',
    acilis: '28.08.2026', teslimTarihi: '28.08.2026',
    odemeYolu: 'almadim', hatirlatmaTarihi: null, hatirlatmaGonderildi: false, parcaBekliyor: false, bekliyorMusteriye: false,
    satirlar: [
      { tip: 'parca', ad: 'Akü 60 Ah', fiyat: 3200, alis: 2450, kimGetirdi: 'tamirci',
        marka: 'Mutlu', kod: 'SFB 60', tanim: 'kodlu' },
      { tip: 'iscilik', ad: 'Takma', fiyat: 250, alis: 0, kimGetirdi: '' }
    ] },

  /* YEDİNCİ İŞ GÖRÜNSÜN: hatırlatması GELMİŞ ama gönderilmemiş bakım.
     Menü rozeti ve çekmecedeki bildirim bu kayıttan besleniyor. */
  { id: 5, plaka: '16 XY 8890', tur: 'bakim', km: '88.000',
    sikayet: 'Periyodik bakım',
    acilis: '20.02.2026', teslimTarihi: '20.02.2026',
    odemeYolu: 'kart', hatirlatmaTarihi: '20.08.2026', hatirlatmaGonderildi: false, parcaBekliyor: false, bekliyorMusteriye: false,
    satirlar: [
      { tip: 'parca', ad: 'Yağ filtresi', fiyat: 320, alis: 210, kimGetirdi: 'tamirci',
        marka: '', kod: '', tanim: 'tanimlanamadi' },
      { tip: 'iscilik', ad: 'Bakım işçiliği', fiyat: 900, alis: 0, kimGetirdi: '' }
    ] }
];

/* ---- GÖNDERİLEN MESAJLAR (14.09) ----
   Cemil: *"Gönderilen mesajlar listelenebilmeli."*

   ⚠️ BU ARŞİV "İYİ OLUR" DİYE YAZILMADI. K-045.3'te ölçülmüştü:
   ticari elektronik iletiye ilişkin kayıtlar **üç yıl** saklanır ve
   **ispat yükü gönderende**. Alanlar da oradan geliyor — kime, hangi
   kanaldan, ne zaman, hangi metin. Prototipte bellekte duruyor;
   backend fazında saklanacak yer bu alanları bekliyor.

   İKİNCİ İŞİ: HANGİ MESAJ GÖNDERİLDİ sorusu. Cemil aynı turda
   *"teslim edilmiş ve mesaj gönderilmemiş araca"*, *"işin özeti
   gönderilmemiş araca"* dedi — yani şablonun çıkıp çıkmayacağı bu
   listeden okunuyor. Arşiv olmasaydı o kurallar yazılamazdı.

   Tohumda iki kayıt var: gönderilmiş bir özet ve bir hatırlatma.
   Boş liste, çalıştığı görülemeyen bir alandır. */
var MESAJLAR = [
  { id: 1, tarih: '02.09.2026', saat: '10:20', plaka: '06 TMR 07',
    ad: 'Hasan Demir', tel: '0542 331 90 07', kanal: 'whatsapp',
    sablon: 'ozet', kayit: 2, toplu: false,
    metin: 'Hasan Bey/Hanım, Fiat Egea (06 TMR 07) için 3 kalem işlem yapıldı.' },
  { id: 2, tarih: '28.08.2026', saat: '17:05', plaka: '06 TMR 07',
    ad: 'Hasan Demir', tel: '0542 331 90 07', kanal: 'sms',
    sablon: 'teslim', kayit: 8, toplu: false,
    metin: 'Hasan Bey/Hanım, Fiat Egea (06 TMR 07) teslim edildi.' }
];

/* ---- HAZIR METİNLER (14.09) ----
   Cemil: *"Mesela tüm müşterilere bayram mesajı gönder diyecek sesli
   olarak. Uygun bir bayram mesajı bulacak."* ve *"34 ABC 123 sahibine
   WhatsApp üzerinden selam mesajı gönder."*

   Usta metni yazmıyor, TÜRÜNÜ söylüyor. Üç tür var çünkü ustanın
   söylediği üç şey bu; dördüncüsü icat edilmedi.

   ⚠️ BAYRAMIN ADI YAZILMAZ. "Ramazan Bayramınız" yazsaydık Kurban'da
   yanlış mesaj giderdi ve hangi bayram olduğunu bilen bir takvimimiz
   yok. Bilmediğimiz şeyi söylememek, yanlış söylemekten iyidir.

   ⚠️ TÜRKÇEDE KELİME SINIRI ARANMAZ (K-018): "bayramınız" kök hâliyle
   eşit değil, o yüzden kelime kelime bölünüp KÖKLE BAŞLIYOR MU diye
   bakılıyor (kokleBasliyorMu). */
var HAZIR_METIN = [
  { k: 'bayram', ad: 'Bayram mesajı', kokler: ['bayram'],
    metin: 'Bayramınız mübarek olsun. Sağlıklı, huzurlu ve kazasız günler dileriz.' },
  /* ⚠️ METİN "MERHABA" İLE BAŞLAMAZ: hitap zaten başta duruyor ve
     "Mehmet Bey/Hanım, Merhaba, nasılsınız?" diye iki selam çıkıyordu.
     İlk denemede ekrana bakmadan görülmeyecek bir ayrıntı. */
  { k: 'selam', ad: 'Selam mesajı', kokler: ['selam', 'merhaba'],
    metin: 'nasılsınız? Aracınızla ilgili bir ihtiyacınız olursa bekleriz; ' +
           'iyi günler dileriz.' },
  { k: 'tesekkur', ad: 'Teşekkür mesajı', kokler: ['tesekkur'],
    metin: 'Bizi tercih ettiğiniz için teşekkür ederiz. İyi günlerde kullanın.' }
];

function hazirMetinBul(cumle) {
  for (var i = 0; i < HAZIR_METIN.length; i++) {
    if (kokleBasliyorMu(cumle, HAZIR_METIN[i].kokler)) return HAZIR_METIN[i];
  }
  return null;
}

/* ---- HİTAP VE İMZA — TEK YERDE (14.09) ----
   Hitap kuralı mesaj ekranının içinde yazılıydı (firma ise "Bey/Hanım"
   yok, A-M2 hedefi). Sesli yol da mesaj üretmeye başlayınca kural
   ikinci kez yazılacaktı; ikinci kopya, firmaya "Ltd. Şti. Bey/Hanım"
   diye yazan ilk hatanın geri dönüş yoludur. */
function mesajHitabi(plaka, hedefAnahtar) {
  var h = plaka ? mesajHedefi(plaka, hedefAnahtar) : null;
  if (!h || !h.ad) return 'Merhaba, ';
  return h.kisi ? h.ad + ' Bey/Hanım, ' : h.ad + ', ';
}
function mesajImzasi() {
  return '\n\n' + DUKKAN.ad + '\n' + DUKKAN.tel;
}
/* Hazır metnin tam hâli: hitap + metin + imza. Tek müşteriye de toplu
   gönderime de aynı yerden çıkıyor — usta iki yerde iki farklı mesaj
   görmesin. */
function hazirMesajMetni(kod, plaka) {
  var h = HAZIR_METIN.filter(function (x) { return x.k === kod; })[0];
  if (!h) return '';
  return mesajHitabi(plaka) + h.metin + mesajImzasi();
}

/* ŞABLONUN ADI TEK YERDE. Üç yerde yazılıydı: mesaj ekranının çipleri,
   sesli işlemin onay ekranı ("Yapılan işin özeti") ve şimdi arşiv
   satırı. Usta onay ekranında bir ad, çipte başka bir ad görüyordu. */
var SABLON_AD = {
  ozet: 'İşin özeti', bekliyor: 'Parça bekliyoruz', hazir: 'Aracınız hazır',
  teslim: 'Teslim edildi', hatirlatma: 'Bakım hatırlatması',
  serbest: 'Serbest mesaj'
};
var KANAL_AD = { whatsapp: 'WhatsApp', sms: 'SMS', ara: 'Telefon' };
/* Hazır metinlerin adları da şablon adıdır — arşiv satırı "bayram"
   değil "Bayram mesajı" yazsın. Liste tek yerde, ad da tek yerde. */
HAZIR_METIN.forEach(function (h) { SABLON_AD[h.k] = h.ad; });

/* ---- TOPLU GÖNDERİMİN ALICILARI (14.09) ----
   Telefonu olan her müşteri, bir kez. Hangi numara: satırda görünen
   araç üzerinden mesajHedefi() — yani listede gördüğü kişi kim ise o
   (firma santrali değil, aracı getiren kişi · A-M2). */
function topluAlicilar() {
  var liste = [];
  musteriler().forEach(function (m) {
    var a = musteriAraci(m);
    var h = a ? mesajHedefi(a.plaka) : null;
    if (h && h.tel) liste.push({ ad: h.ad, tel: h.tel, plaka: a.plaka });
  });
  return liste;
}

/* Mesaj kaydı TEK YERDEN düşer. İki gönderim yolu var (mesaj ekranı ve
   sesli işlem) ve ikisi ayrı yazsaydı biri arşive yazmayı unuturdu —
   bu turda tam olarak bunun testi yazıldı (43. denetim). */
function mesajKaydet(o) {
  var d = new Date();
  var k = {
    id: MESAJLAR.length + 1,
    tarih: bugunMetin(),
    saat: ('0' + d.getHours()).slice(-2) + ':' + ('0' + d.getMinutes()).slice(-2),
    plaka: o.plaka || '', ad: o.ad || '', tel: o.tel || '',
    kanal: o.kanal || 'whatsapp', sablon: o.sablon || 'serbest',
    kayit: o.kayit || null, toplu: !!o.toplu, metin: o.metin || ''
  };
  MESAJLAR.push(k);
  return k;
}

/* En yeni üstte — liste "ne gönderdim" sorusunu cevaplıyor ve o soru
   hep son gönderilenle başlar. */
function mesajArsivi(plaka) {
  return MESAJLAR.filter(function (m) { return !plaka || m.plaka === plaka; })
                 .slice().reverse();
}

/* ---- MESAJ SÜZGECİ (14.09) ----
   Cemil: *"Gönderilen mesajlar binlerce olduğunda nasıl listelenecek?
   Filtrelemek gerekir mi? Yeni bir sayfa daha uygun olabilir."*

   Haklı ve sayı onu doğruluyor: günde beş mesaj gönderen bir dükkân
   bir yılda 1.800 satır eder. Akordiyonun içine sığmaz, mesaj yazma
   ekranının dibinde durması da doğru olmaz.

   SÜZGEÇ TEK YERDE: liste iki ekranda görünüyor (mesaj ekranındaki
   "son mesajlar" ve kendi sayfası). İki süzgeç iki ayrı cevap
   demektir (K-019.1). */
function mesajListesi(secenek) {
  var s = secenek || {};
  var q = norm(s.q || '');
  return mesajArsivi(s.plaka).filter(function (m) {
    if (s.kanal && m.kanal !== s.kanal) return false;
    if (!q) return true;
    return norm(m.ad + ' ' + m.plaka + ' ' + m.tel + ' ' + m.metin + ' ' +
                (SABLON_AD[m.sablon] || '')).indexOf(q) >= 0;
  });
}

/* ---- ŞABLON KURALLARI (14.09'da mesaj ekranından BURAYA taşındı) ----
   Cemil: *"Teslim edilmiş ve mesaj gönderilmemiş araca teslim edildi
   mesajı gönderebilir. Yine serviste olan ve işin özeti gönderilmemiş
   araca işin özeti gönderilebilir. Ve gerçekten parça bekliyor
   durumunda olana parça bekleniyor mesajı gidebilir."*

   ⚠️ NEDEN VERİ KATMANINDA: aynı kuralı İKİ yer soruyor — mesaj
   ekranının çip listesi ve servis kaydındaki SÜREÇ şeridi (*"şimdi ne
   gönderilebilir"*). Ekranda kalsaydı süreç kendi kopyasını yazardı ve
   ikisi bir gün ayrışırdı: ekran *"özet gönderilebilir"* derken süreç
   sessiz kalırdı (K-019.1). */
var MESAJ_SABLONLARI = [
  /* ⚠️ KOŞULLAR KAYIT BAZLI, ARAÇ BAZLI DEĞİL (14.09, ikinci yazım).
     İlk yazımda koşullar plakaya bakıyordu ve SÜREÇ ŞERİDİ yanlış
     öneri gösterdi: teslim edilmiş bir kaydın süreci *"Aracınız hazır
     gönderilebilir"* diyordu — çünkü AYNI ARACIN başka bir açık kaydı
     vardı. Araç düzeyinde doğru olan cümle, kayıt düzeyinde yalan.

     Cemil "bunu iyi kontrol et" dedi ve kontrol bunu buldu: aynı
     aracın iki kaydı olduğunda soru "bu araçta ne var" değil, "BU
     İŞTE ne var". */
  { k: 'ozet',
    uygunKayit: function (k) {
      return !!k && !k.teslimTarihi && !kayitMesajiVar(k.id, 'ozet');
    } },
  { k: 'bekliyor',
    uygunKayit: function (k) { return !!k && !k.teslimTarihi && !!k.parcaBekliyor; } },
  { k: 'hazir',
    /* A-M17: teslim edilmiş kayıtta "hazır" yalandı; yalnız açık işte. */
    uygunKayit: function (k) { return !!k && !k.teslimTarihi; } },
  { k: 'teslim',
    uygunKayit: function (k) { return !!k && !!k.teslimTarihi && !kayitMesajiVar(k.id); } },
  { k: 'hatirlatma',
    /* Hatırlatma teslim edilmiş BAKIM kaydının işi; kurulmuş ve henüz
       gönderilmemişse sırada. */
    uygunKayit: function (k) {
      return !!k && !!k.teslimTarihi && bakimIceriyor(k) &&
             !!k.hatirlatmaTarihi && !k.hatirlatmaGonderildi;
    } },
  { k: 'serbest', uygunKayit: function () { return true; } }
];

/* Aracın "şu anki işi": açık kayıt varsa o, yoksa sonuncusu. Mesaj
   ekranı araç seçiyor; süreç şeridi kaydı biliyor. İkisi de aynı
   kural tablosunu kullanıyor — tek kaynak (K-019.1). */
function aracinIsi(plaka) {
  return acikKayit(plaka) || sonKayit(plaka);
}
function uygunSablonlar(plaka) {
  var k = aracinIsi(plaka);
  return MESAJ_SABLONLARI.filter(function (s) { return s.uygunKayit(k); });
}
/* Süreç şeridinin sorduğu soru: "bu işte şimdi hangi mesaj sırada".
   `serbest` elenir — o bir öneri değil, her zaman açık bir kapı. */
function kayitSiradakiMesajlar(k) {
  return MESAJ_SABLONLARI.filter(function (s) {
    return s.k !== 'serbest' && s.uygunKayit(k);
  });
}

function mesajBul(id) {
  for (var i = 0; i < MESAJLAR.length; i++) if (MESAJLAR[i].id === id) return MESAJLAR[i];
  return null;
}

/* Bir KAYIT için o şablon gönderilmiş mi. Araç değil KAYIT: aynı araç
   üç kez gelir, her gelişin kendi özeti vardır. */
function kayitMesajiVar(kayitId, sablon) {
  return MESAJLAR.some(function (m) {
    return m.kayit === kayitId && (!sablon || m.sablon === sablon);
  });
}

/* ---- Supplier (VERI-MODELI §4) — iki alan ---- */
var PARCACILAR = [
  { ad: 'Öz Yedek Parça', tel: '0532 111 22 33' },
  { ad: 'Ahmet Oto Yedek Parça', tel: '0533 444 55 66' },
  { ad: 'Ankara Bosch Bayi', tel: '0312 222 33 44' }
];

/* ---- Shop (VERI-MODELI §8) ---- */
var DUKKAN = {
  ad: 'Ahmet Usta Oto Servis',
  adres: 'Ostim OSB, Ankara',
  tel: '0532 000 00 00',
  /* HATIRLATMA: AY **VEYA** KM — hangisi önce gelirse (Cemil, 03.09).
     K-002 yalnız ayı sayıyordu; gerçek bakım kuralı ikisi birden. */
  bakimPeriyoduAy: 6,
  bakimPeriyoduKm: 10000
};

/* ---- Sorgular (tek yerde; ekranlar kendi filtresini yazmaz) ---- */
function aracBul(plaka) {
  for (var i = 0; i < ARACLAR.length; i++) if (ARACLAR[i].plaka === plaka) return ARACLAR[i];
  return null;
}
/* ---- YANLIŞ DUYULAN PLAKA (14.09 · A-M27) ----
   Cemil'in ses kaydı *"34 qbc 123 sahibine WhatsApp üzerinden selam
   mesajı gönder"* oldu: tanıma **A**'yı **Q** duydu ve akış
   *"bu araç kayıtlı değil"* deyip ruhsat ekranına gitti. Usta doğru
   söylemişti; program doğru davranmıştı; **iş yine de durdu.**

   KURAL DAR TUTULDU, ÇÜNKÜ YANLIŞIN BEDELİ AĞIR: yanlış araca mesaj
   gitmesi, hiç gitmemesinden kötüdür.
     · il kodu ve rakam grubu **birebir aynı** olacak
     · harf grubu **en fazla bir karakter** farklı olacak
     · ve **tek bir aday** kalacak — iki aday varsa öneri YOK,
       bugünkü davranış aynen sürer

   ⚠️ ÖNERİ BİR SORUDUR, DÜZELTME DEĞİL. Bu fonksiyon yalnız adayı
   söyler; hiçbir şey seçmez, hiçbir şey göndermez. Ekran onu bir
   düğme olarak gösteriyor ve usta dokunmadıkça hiçbir şey olmuyor.

   ⚠️ TÜRKÇEDE SINIR ARANMAZ (K-018): plaka boşluktan bölünüyor,
   parçalar eşitlikle karşılaştırılıyor. */
function plakaParcala(plaka) {
  var p = String(plaka || '').toLocaleUpperCase('tr').split(/\s+/).filter(Boolean);
  if (p.length !== 3) return null;
  return { il: p[0], harf: p[1], rakam: p[2] };
}

/* Harf grubu kaç karakter farklı: aynı uzunlukta yer yer farklılık,
   bir eksik/fazla harfte tek ekleme-çıkarma. Tanıma harf yutabiliyor
   da ("ABC" yerine "AC"), harf uydurabiliyor da. */
function harfFarki(a, b) {
  if (a === b) return 0;
  if (a.length === b.length) {
    var f = 0;
    for (var i = 0; i < a.length; i++) if (a[i] !== b[i]) f++;
    return f;
  }
  if (Math.abs(a.length - b.length) !== 1) return 99;
  var uzun = a.length > b.length ? a : b;
  var kisa = a.length > b.length ? b : a;
  for (var j = 0; j < uzun.length; j++) {
    if (uzun.slice(0, j) + uzun.slice(j + 1) === kisa) return 1;
  }
  return 99;
}

function benzerPlaka(plaka) {
  var p = plakaParcala(plaka);
  if (!p) return null;
  /* Kayıtlı bir plakaya öneri yapılmaz — sorulacak bir şey yok. */
  if (aracBul(plaka)) return null;
  var adaylar = [];
  ARACLAR.forEach(function (a) {
    var q = plakaParcala(a.plaka);
    if (!q) return;
    if (q.il !== p.il || q.rakam !== p.rakam) return;
    if (harfFarki(p.harf, q.harf) > 1) return;
    adaylar.push(a.plaka);
  });
  /* İKİ ADAY = ÖNERİ YOK. Hangisi olduğunu bilmiyorsak tahmin etmek,
     ustaya yanlış aracı onaylatmanın yoludur. */
  return adaylar.length === 1 ? adaylar[0] : null;
}

function kayitBul(id) {
  for (var i = 0; i < KAYITLAR.length; i++) if (KAYITLAR[i].id === id) return KAYITLAR[i];
  return null;
}
/* BAKIM İÇERİYOR MU — tür `bakim` ya da `ikisi` ise evet (05.09).
   Tek cümlelik kural: bakım içeren her kayıtta km sorulur ve hatırlatma
   kurulur. Bu fonksiyon olmadan `tur === 'bakim'` kontrolü SEKİZ yerde
   duruyordu ve üçüncü tür eklenince sekizinin de yanlış cevap vermesi
   gerekirdi — biri unutulsa hatırlatma sessizce kurulmazdı. */
function bakimIceriyor(k) {
  return !!k && (k.tur === 'bakim' || k.tur === 'ikisi');
}

/* ---- İŞ TÜRÜNÜ METİNDEN TAHMİN ET (06.09) ----
   Cemil: *"Ne için geldi ekranı olmayacak. Tamir, bakım vb çok önemli
   değil. Biz bu ai'ya json'da seçtirebiliriz."*

   Tür ekrandan kalktı ama VERİDEN kalkmadı: K-002 bakımda km ister ve
   hatırlatma kurar. O yüzden iş metninden okunuyor.

   ⚠️ BU BİR TAHMİNDİR, HÜKÜM DEĞİL. Yanlış tahmin edilirse servis
   kaydının kendi ekranından düzeltilir. Düzeltilemeyen bir tahmin,
   ustanın hiç sormadığı bir kararı onun adına vermek olurdu.

   ⚠️ TÜRKÇEDE KELİME SINIRI ARANMAZ (K-018) — ama burada kelimeVarMi()
   de yetmiyor: usta "bakıma geldi", "yağı değişecek", "filtreleri"
   diyor ve hiçbiri kök hâliyle eşit değil. Bu yüzden kelime kelime
   bölünüp KÖKLE BAŞLIYOR MU diye bakılıyor.

   Ön ek eşleşmesi bir arama süzgecinde tehlikeliydi (yanlış eşleşme
   sonucu yok eder); burada tehlikesiz, çünkü sonuç DÜZELTİLEBİLİR bir
   ön dolgu. Aynı tekniğin iki yerde farklı sonuç vermesinin sebebi bu:
   önemli olan teknik değil, yanlışın BEDELİ. */
var BAKIM_KOKLERI = ['bakim', 'yag', 'filtre', 'periyodik', 'servis'];
var TAMIRAT_KOKLERI = ['ariza', 'degis', 'kacir', 'ses', 'titre', 'yanmi',
                       'calismi', 'kirik', 'onar', 'tamir'];

function kokleBasliyorMu(metin, kokler) {
  var parcalar = norm(metin).split(/\s+/);
  for (var i = 0; i < parcalar.length; i++) {
    var kelime = parcalar[i].replace(/[.,!?;:]/g, '');
    if (!kelime) continue;
    for (var j = 0; j < kokler.length; j++) {
      if (kelime.indexOf(kokler[j]) === 0) return true;
    }
  }
  return false;
}

function isTuruTahmin(metin) {
  var bakim = kokleBasliyorMu(metin, BAKIM_KOKLERI);
  var tamirat = kokleBasliyorMu(metin, TAMIRAT_KOKLERI);
  if (bakim && tamirat) return 'ikisi';
  if (bakim) return 'bakim';
  /* VARSAYILAN TAMİRAT. Boş metinde de tamirat: bakım demek, olmayan
     bir hatırlatmayı kurmak demektir — sessizce yanlış hatırlatma
     göndermektense sessizce hatırlatma kurmamak daha az zarar verir. */
  return 'tamirat';
}

/* İÇERİDEKİ = teslim edilmemiş. Başka tanım yok (K-004). */
function icerideki() {
  return KAYITLAR.filter(function (k) { return !k.teslimTarihi; });
}
function aracKayitlari(plaka) {
  return KAYITLAR.filter(function (k) { return k.plaka === plaka; });
}
/* Aracın AÇIK kaydı ve SON kaydı — iki soru, iki cevap, tek yer (14.09).
   Mesaj ekranı hangi şablonun çıkacağını bunlara soruyor; üç ayrı
   ekranda üç ayrı süzgeç yazılsaydı biri bir gün başka cevap verirdi
   (K-019.1). */
function acikKayit(plaka) {
  return icerideki().filter(function (k) { return k.plaka === plaka; })[0] || null;
}
function sonKayit(plaka) {
  var l = aracKayitlari(plaka);
  return l.length ? l[l.length - 1] : null;
}
/* MÜŞTERİYE SÖYLENEN RAKAM.
   ⚠️ 06.09 — MÜŞTERİ PARÇASI TOPLAMA GİRMEZ ve bu artık BURADA
   söyleniyor, satırın fiyatı silinerek değil (K-037).

   Eskiden kural şöyle işliyordu: parça müşteriden geldi işaretlenince
   `fiyat` **sıfırlanıyordu**. Sonuç doğruydu ama yol yanlıştı — bir
   arayüz düğmesi, satırın satış fiyatını **geri dönüşsüz siliyordu**.
   Anahtarı açıp kapatan usta, yazdığı rakamı kaybediyordu.

   S2/K-005 hâlâ aynı: müşterinin getirdiği parça faturaya girmez.
   Değişen tek şey, kuralın nerede işletildiği. Veri korunuyor,
   TOPLAM karar veriyor. */
function kayitToplami(k) {
  return (k.satirlar || []).reduce(function (t, s) {
    if (s.tip === 'parca' && s.kimGetirdi === 'musteri') return t;
    return t + (s.fiyat || 0);
  }, 0);
}
/* Parçası sorulmuş ama fiyatı gelmemiş — DURUM DEĞİL, veriden türetilir
   (K-004 notu, A-4). */
function fiyatiBekleyen(k) {
  return (k.satirlar || []).some(function (s) {
    return s.tip === 'parca' && s.kimGetirdi === 'tamirci' && !s.fiyat;
  });
}

/* ---- MESAJ KİME GİDER (A-M2, 04.09) ----
   Ruhsat sahibi FİRMA olabilir (K-019.3) ve firmanın numarası
   santraldir. Özet ya da bakım hatırlatması santrale gitmez — aracı
   getiren kişiye gider.

   **Varsayım — Cemil onayına açık (A-M2):** ikinci iletişim varsa
   mesaj ONA gider, yoksa ruhsattaki numaraya. Usta yine de mesaj
   ekranından hedefi değiştirebiliyor; varsayım bir KİLİT değil,
   varsayılan.

   ⚠️ "BEY/HANIM" HERKESE YAZILMAZ. Firma adına hitap edilince mesaj
   "Demir Nakliyat Ltd. Şti. Bey/Hanım," diye başlıyordu — müşteriye
   giden ilk cümle bu ve ciddiyetsiz görünüyor. Firma olup olmadığı
   ada bakılarak anlaşılıyor; ayrı bir alan tutmuyoruz (K-012'nin
   kuralı: ekran gerekiyorsa tablo/alan gerekmiyor olabilir). */
var FIRMA_ISARETLERI = ['ltd', 'ltd.', 'sti', 'sti.', 'şti', 'şti.',
  'a.s.', 'a.ş.', 'as', 'aş', 'anonim', 'limited', 'san', 'san.',
  'tic', 'tic.', 'sirketi', 'şirketi', 'kollektif', 'komandit'];

/* ⚠️ Türkçede kelime sınırı ARANMAZ (K-018): ad boşluktan BÖLÜNÜP
   her parça eşitlikle karşılaştırılıyor. Desenle arasaydık "Astan"
   içindeki "as" firma sanılırdı. */
function firmaMi(ad) {
  var parcalar = norm(ad).split(/\s+/);
  for (var i = 0; i < parcalar.length; i++) {
    if (FIRMA_ISARETLERI.indexOf(parcalar[i]) >= 0) return true;
  }
  return false;
}

function mesajHedefleri(plaka) {
  var a = aracBul(plaka);
  if (!a) return [];
  var liste = [];
  if (a.iletisimTel || a.iletisimAd) {
    liste.push({ anahtar: 'iletisim', ad: a.iletisimAd || 'iletişim',
                 tel: a.iletisimTel || '', rol: 'İletişim',
                 kisi: !firmaMi(a.iletisimAd) });
  }
  liste.push({ anahtar: 'sahip', ad: a.sahipAd || '', tel: a.sahipTel || '',
               rol: 'Ruhsat sahibi', kisi: !firmaMi(a.sahipAd) });
  return liste;
}

/* Varsayılan hedef: telefonu OLAN ilk sıradaki. Telefonsuz bir hedefi
   varsayılan yapmak, ustayı "neden gönderemiyorum"a düşürüyordu. */
function mesajHedefi(plaka, anahtar) {
  var h = mesajHedefleri(plaka);
  if (!h.length) return null;
  if (anahtar) {
    for (var i = 0; i < h.length; i++) if (h[i].anahtar === anahtar) return h[i];
  }
  for (var j = 0; j < h.length; j++) if (h[j].tel) return h[j];
  return h[0];
}

/* ---- MÜŞTERİ: ayrı tablo değil, ARAÇTAN TÜRETİLİR ----
   VERI-MODELI'nde Customer YOK; müşteri aracın üstünde duruyor
   (sahipAd + sahipTel). Menüye "Müşteriler" gelince yeni tablo
   açmadan aynı veriden liste çıkıyor: bir sahip birden fazla araca
   sahipse tek satır olur, araçları altında toplanır. */
function musteriler() {
  var harita = {};
  ARACLAR.forEach(function (a) {
    var ad = (a.sahipAd || '').trim();
    if (!ad) return;                      /* sahibi yazılmamış araç listede yok */
    var anahtar = norm(ad) + '|' + (a.sahipTel || '');
    if (!harita[anahtar]) {
      /* ⚠️ ANAHTAR NESNENİN ÜSTÜNDE (08.09). Müşteri detayı ekranı
         açılırken "hangi müşteri" bilgisini taşımak gerekiyor ve
         müşterinin id'si YOK — kendi tablosu olmadığı için (A-M1).
         Anahtarı burada bir kez üretip nesneye yazmak, onu çağıran
         her yerde YENİDEN kurmaktan güvenli: kural değişirse tek
         yerde değişir. */
      harita[anahtar] = { anahtar: anahtar, ad: ad, tel: a.sahipTel || '', araclar: [] };
    }
    harita[anahtar].araclar.push(a);
  });
  return Object.keys(harita).map(function (k) { return harita[k]; })
    .sort(function (x, y) { return x.ad.localeCompare(y.ad, 'tr'); });
}

/* Müşteri TÜRETİLMİŞ bir görünüm; "bul" da türetmeden geçer. Aracın
   sahibi adı değişirse anahtar değişir ve burası null döner — detay
   ekranı bunu bir hata değil, bir durum olarak karşılıyor. */
function musteriBul(anahtar) {
  if (!anahtar) return null;
  return musteriler().filter(function (m) { return m.anahtar === anahtar; })[0] || null;
}

/* ---- MÜŞTERİ LİSTESİ: ARAMA VE SIRA TEK YERDE (14.09) ----
   Cemil: *"Müşteriye mesaj ve müşteriler sayfasının listelemeleri aynı
   olacak."*

   Aynı liste iki ekranda çiziliyordu ve süzgeci de sırası da iki ayrı
   yerde yazılıydı. K-019.1'in dersi tam buydu: **aynı soruyu iki yerde
   cevaplamak, iki ayrı cevap demektir.** Satırın kendisi ortak nesne
   (musteriKartiHtml); hangi satırların, hangi sırada geleceği de ortak
   olmalı — yoksa iki liste bir gün sessizce ayrışır. */

/* Araç ŞU AN serviste mi — tek tanım, K-004'ün tek durumu. */
function aracServiste(plaka) {
  return icerideki().some(function (k) { return k.plaka === plaka; });
}

/* Satırın göstereceği araç: Cemil'in kuralı — *"o an serviste olan ilk
   araç plakası"*; serviste aracı yoksa ilk aracı. Satır hangi aracı
   gösteriyorsa mesaj da ona gider; ikisi ayrılırsa usta gördüğünden
   başka bir araç için mesaj yazar. */
function musteriAraci(m) {
  if (!m || !m.araclar.length) return null;
  return m.araclar.filter(function (a) { return aracServiste(a.plaka); })[0] || m.araclar[0];
}
function musteriServiste(m) {
  return !!m && m.araclar.some(function (a) { return aracServiste(a.plaka); });
}

function musteriListesi(q) {
  var n = norm(q || '');
  return musteriler().filter(function (m) {
    if (!n) return true;
    var alan = m.ad + ' ' + m.tel + ' ' +
               m.araclar.map(function (a) { return a.plaka + ' ' + a.marka; }).join(' ');
    return norm(alan).indexOf(n) >= 0;
  }).sort(function (x, y) {
    /* SERVİSTEKİLER ÜSTTE (Cemil, 14.09). Araç ve kayıt listelerindeki
       sıranın aynısı: usta üç listede üç ayrı sıra öğrenmesin. */
    var f = (musteriServiste(y) ? 1 : 0) - (musteriServiste(x) ? 1 : 0);
    return f || x.ad.localeCompare(y.ad, 'tr');
  });
}

/* ---- MUHASEBENİN HAMMADDESİ ----
   Kapsam Cemil'in kararını bekliyor ("sonra geçeceğiz"). Bu iki sorgu
   HİÇBİR YENİ ALAN İSTEMİYOR: teslimdeki ödeme yolu ve satır fiyatları
   zaten kayıtlı. "Almadım" bir ödeme yoludur, yani veresiye de veri. */
function bugunTahsilat() {
  var b = ('0' + BUGUN.getDate()).slice(-2) + '.' +
          ('0' + (BUGUN.getMonth() + 1)).slice(-2) + '.' + BUGUN.getFullYear();
  return KAYITLAR.filter(function (k) {
    return k.teslimTarihi === b && k.odemeYolu && k.odemeYolu !== 'almadim';
  }).reduce(function (t, k) { return t + kayitToplami(k); }, 0);
}
function veresiyeToplami() {
  return KAYITLAR.filter(function (k) { return k.odemeYolu === 'almadim'; })
    .reduce(function (t, k) { return t + kayitToplami(k); }, 0);
}
function odemeDagilimi() {
  var d = { nakit: 0, kart: 0, havale: 0, almadim: 0 };
  KAYITLAR.forEach(function (k) {
    if (k.teslimTarihi && k.odemeYolu && d[k.odemeYolu] !== undefined) {
      d[k.odemeYolu] += kayitToplami(k);
    }
  });
  return d;
}

/* ---- GİDER (muhasebe) ----
   03.09 — Cemil: "Muhasebe için x tl elektrik faturası ekle gibi
   örnekler." Gider girişi K-013'te "usta her gideri girmez, girmezse
   rakam yalan söyler" diye ertelenmişti. SESLE GİRİLİYORSA o itiraz
   zayıflıyor: "beş yüz lira elektrik faturası" demek, form doldurmak
   değil. K-014'e bakılsın. */
/* ⚠️ 05.09 — PARÇA GİDERİ TOHUM VERİDEN ÇIKARILDI.
   Cemil: "Giderlerde yedek parça olmamalı. Yedek parçalar işten
   alınarak düşülüyor."
   Tohum veride "Öz Yedek Parça — triger seti 4.200 ₺" duruyordu ve
   ekran onu gider olarak sayıyordu; oysa aynı para servis satırındaki
   `alis` alanında da duruyor ve iş başına farkta düşülüyor. Örnek
   verinin kendisi YANLIŞ ALIŞKANLIK öğretiyordu. */
var GIDERLER = [
  { id: 1, tarih: '01.09.2026', aciklama: 'Elektrik faturası', tutar: 2400, tur: 'fatura' },
  { id: 2, tarih: '02.09.2026', aciklama: 'Dükkân kirası', tutar: 12000, tur: 'kira' }
];

/* ---- KM GEÇMİŞİ ----
   Ayrı tablo YOK: kilometre zaten servis kaydında duruyor (K-002).
   Geçmiş, kayıtlardan türetiliyor — bir aracın km'si iki yerde
   tutulsaydı ikisi ayrışırdı. */
function kmGecmisi(plaka) {
  return aracKayitlari(plaka)
    .filter(function (k) { return k.km; })
    .map(function (k) {
      return { tarih: k.teslimTarihi || k.acilis, km: parseInt(String(k.km).replace(/\D/g, ''), 10), kayit: k };
    })
    .filter(function (x) { return x.km > 0; })
    .sort(function (a, b) { return tariheCevir(a.tarih) - tariheCevir(b.tarih); });
}

/* Günlük ortalama km — iki ölçüm arasındaki farktan. Tek ölçüm varsa
   tahmin YAPILMAZ; uydurma bir ortalama, yanlış hatırlatma demektir. */
function gunlukKm(plaka) {
  var g = kmGecmisi(plaka);
  if (g.length < 2) return 0;
  var ilk = g[0], son = g[g.length - 1];
  var gun = (tariheCevir(son.tarih) - tariheCevir(ilk.tarih)) / 86400000;
  if (gun <= 0) return 0;
  return (son.km - ilk.km) / gun;
}

/* ---- BAKIM HATIRLATMASI: AY **VEYA** KM, HANGİSİ ÖNCE GELİRSE ----
   Cemil, 03.09'da bu kuralı seçti. K-002 yalnız ayı sayıyordu; gerçek
   bakım kuralı "6 ay veya 10.000 km"dir.

   KM TAHMİNİ ANCAK İKİ ÖLÇÜM VARSA yapılır. Tek ölçümle günlük km
   uydurmak, ustayı yanlış güne çağırır — ay tek başına çalışır. */
function hatirlatmaHesapla(kayit) {
  var d = new Date(tariheCevir(kayit.teslimTarihi || kayit.acilis).getTime());
  d.setMonth(d.getMonth() + DUKKAN.bakimPeriyoduAy);
  var ayTarihi = d;

  var sonuc = { ayTarihi: ayTarihi, kmHedef: 0, kmTarihi: null, hangisi: 'ay' };

  var baslangicKm = parseInt(String(kayit.km || '').replace(/\D/g, ''), 10);
  if (baslangicKm > 0) {
    sonuc.kmHedef = baslangicKm + DUKKAN.bakimPeriyoduKm;
    var gk = gunlukKm(kayit.plaka);
    if (gk > 0) {
      var gerekenGun = DUKKAN.bakimPeriyoduKm / gk;
      var kd = new Date(tariheCevir(kayit.teslimTarihi || kayit.acilis).getTime());
      kd.setDate(kd.getDate() + Math.round(gerekenGun));
      sonuc.kmTarihi = kd;
      if (kd < ayTarihi) sonuc.hangisi = 'km';
    }
  }
  sonuc.tarih = (sonuc.hangisi === 'km') ? sonuc.kmTarihi : sonuc.ayTarihi;
  return sonuc;
}

function gunAyYil(d) {
  return ('0' + d.getDate()).slice(-2) + '.' +
         ('0' + (d.getMonth() + 1)).slice(-2) + '.' + d.getFullYear();
}
/* BUGÜNÜN METNİ TEK YERDE. '03.09.2026' on üç yerde elle yazılıydı;
   prototipin günü değişince on üç yerin on ikisi eski günde kalırdı
   ve hangisinin unutulduğu ancak rakam tutmayınca anlaşılırdı. */
function bugunMetin() { return gunAyYil(BUGUN); }

/* Tarihi gelmiş ve gönderilmemiş hatırlatmalar — ana ekran, çekmece
   rozeti ve araç ekranı AYNI kaynaktan besleniyor. */
function hatirlatmasiGelenler() {
  return KAYITLAR.filter(function (k) {
    return bakimIceriyor(k) && k.hatirlatmaTarihi && !k.hatirlatmaGonderildi &&
           tariheCevir(k.hatirlatmaTarihi) <= BUGUN;
  });
}

/* Bir aracın SON bakımı ve bir sonraki hatırlatması. */
function sonBakim(plaka) {
  var b = aracKayitlari(plaka).filter(function (k) {
    return bakimIceriyor(k) && k.teslimTarihi;
  });
  return b.length ? b[b.length - 1] : null;
}

function giderToplami() {
  return GIDERLER.reduce(function (t, g) { return t + (g.tutar || 0); }, 0);
}

/* ---- DÖNEM SÜZGECİ (05.09 — muhasebenin "detaylı" kipi) ----
   Cemil: "Muhasebe sayfasında daha detaylı işlem yapılabilir. Basit
   muhasebe olarak düşünelim. Fakat daha fazlasına ihtiyaç duyanlar
   için de bir geliştirme yapmalıyız."

   Dönem HİÇBİR YENİ ALAN İSTEMİYOR: teslim tarihi ve gider tarihi
   zaten kayıtlı. Yalnız üç pencere var — gün, ay, hepsi. Dördüncüsü
   (hafta, çeyrek, yıl) ustanın sormadığı sorulardır ve her biri bir
   karar daha demek (K-022.4 karar bütçesi). */
/* ⚠️ 05.09 — DÖNEM DÜĞMELERİ AYIN VE YILIN ADINI SÖYLER (Cemil:
   "Bugün, bulunduğumuz ay hangisi ise, sonra da bulunduğumuz yıl
   hangisi ise butonları olacak").

   "Bu ay" değil **Eylül**, "Hepsi" değil **2026**. Fark küçük görünüyor
   ama "bu ay" ustaya HANGİ ay olduğunu söylemiyor; ekran görüntüsü
   alınıp bir hafta sonra bakıldığında ya da ay dönüldüğünde rakamın
   hangi pencereye ait olduğu kayboluyordu. Ad yazınca rakam kendi
   tarihini taşıyor. */
var AYLAR = ['Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran',
             'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'];

var DONEMLER = ['bugun', 'ay', 'yil'];

function donemAdi(donem) {
  if (donem === 'bugun') return 'Bugün';
  if (donem === 'ay') return AYLAR[BUGUN.getMonth()];
  return String(BUGUN.getFullYear());
}

function donemCipleriHtml() {
  return DONEMLER.map(function (d) {
    return '<button class="cip" data-donem="' + d + '" aria-pressed="' +
      (muhDonemSecili === d) + '">' + donemAdi(d) + '</button>';
  }).join('');
}

function donemBasi(donem) {
  if (donem === 'bugun') return new Date(BUGUN.getFullYear(), BUGUN.getMonth(), BUGUN.getDate());
  if (donem === 'ay') return new Date(BUGUN.getFullYear(), BUGUN.getMonth(), 1);
  /* yıl — 1 Ocak'tan bugüne. "Hepsi" penceresi kalktı: dükkânın açılış
     tarihinden beri toplam, ustanın sorduğu bir soru değil. */
  return new Date(BUGUN.getFullYear(), 0, 1);
}
function donemIcinde(tarihMetni, donem) {
  if (!tarihMetni) return false;
  var bas = donemBasi(donem);
  if (!bas) return true;
  var t = tariheCevir(tarihMetni);
  return t >= bas && t <= BUGUN;
}

/* Teslim edilmiş kayıtlar — döneme göre. */
function donemKayitlari(donem) {
  return KAYITLAR.filter(function (k) {
    return k.teslimTarihi && donemIcinde(k.teslimTarihi, donem);
  });
}

/* ---- İŞ BAŞINA KÂR (yalnız DETAYLI kipte) ----
   ⚠️ BU DÜKKÂN KÂRI DEĞİL. Bir işin kendi içindeki fark: müşteriden
   alınan − o iş için parçaya ödenen. Kira, elektrik, maaş BURAYA
   GİRMEZ ve ekran bunu yazıyor.

   Neden gösterilebiliyor: iki rakam da ZATEN KAYITLI (fiyat ve alis,
   T-9). Girilmeyen bir şey yok, o yüzden yalan söyleyemez. Dükkân kârı
   ise girilmemiş giderler yüzünden yalan söylerdi — K-014'ün itirazı
   orada hâlâ geçerli. */
function isKari(k) {
  var alinan = kayitToplami(k);
  var parca = (k.satirlar || []).reduce(function (t, s) {
    return t + ((s.tip === 'parca' && s.kimGetirdi === 'tamirci') ? (s.alis || 0) : 0);
  }, 0);
  return { alinan: alinan, parca: parca, fark: alinan - parca };
}

/* ---- PARÇA ALIŞI — BEDAVAYA GELEN DÖRDÜNCÜ RAKAM (04.09) ----
   `alis` (ServiceLine.Cost) ZATEN KAYITLI, çünkü T-9 var: "Usta kendi
   fiyat aldıysa, parça fiyatlarına ekleme yapabiliyor." Parçacıdan
   gelen fiyat satıra yazılıyor.

   Yani tamircinin parçaya ödediği para veri modelinde duruyor ve
   ustadan TEK BİR YENİ GİRİŞ istemiyor. K-013'ün ölçüsüne göre bu
   rakam "girilmezse yalan söyleyen" gruba değil, "zaten kayıtlı"
   gruba ait.

   YALNIZ TAMİRCİNİN ALDIĞI PARÇALAR: müşteri kendi getirdiyse para
   tamirciden çıkmamıştır (K-005). kimGetirdi === 'musteri' olan satır
   sayılsaydı, ödenmemiş bir para gider gibi görünürdü.

   YALNIZ TESLİM EDİLMİŞ KAYITLAR: içerideki araçta parça daha
   alınmamış olabilir — henüz olmamış bir harcamayı gider saymak,
   ustayı olduğundan yoksul gösterir. */
function parcaAlislari(donem) {
  var liste = [];
  KAYITLAR.forEach(function (k) {
    if (!k.teslimTarihi) return;
    if (donem && !donemIcinde(k.teslimTarihi, donem)) return;
    (k.satirlar || []).forEach(function (s) {
      if (s.tip !== 'parca' || s.kimGetirdi !== 'tamirci') return;
      if (!s.alis) return;
      liste.push({ tarih: k.teslimTarihi, plaka: k.plaka,
                   ad: s.ad || 'parça', tutar: s.alis, kayit: k.id });
    });
  });
  return liste;
}
function parcaAlisiToplami(donem) {
  return parcaAlislari(donem).reduce(function (t, x) { return t + x.tutar; }, 0);
}
function giderToplamiDonem(donem) {
  return GIDERLER.reduce(function (t, g) {
    return t + (donemIcinde(g.tarih, donem) ? (g.tutar || 0) : 0);
  }, 0);
}
function tahsilatToplami(donem) {
  return donemKayitlari(donem).reduce(function (t, k) {
    return t + ((k.odemeYolu && k.odemeYolu !== 'almadim') ? kayitToplami(k) : 0);
  }, 0);
}
