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
  { id: 1, plaka: '34 ABC 123', tur: 'tamirat', km: '',
    sikayet: 'Triger seti değişecek',
    acilis: '03.09.2026', teslimTarihi: null,
    odemeYolu: null, hatirlatmaTarihi: null, hatirlatmaGonderildi: false,
    satirlar: [
      { tip: 'parca', ad: 'Triger seti', fiyat: 0, alis: 0, kimGetirdi: 'tamirci',
        marka: '', kod: '', tanim: 'tanimlanamadi' }
    ] },

  { id: 2, plaka: '06 TMR 07', tur: 'bakim', km: '86.400',
    sikayet: 'Periyodik bakım',
    acilis: '02.09.2026', teslimTarihi: null,
    odemeYolu: null, hatirlatmaTarihi: null, hatirlatmaGonderildi: false,
    satirlar: [
      { tip: 'parca', ad: 'Motor yağı', fiyat: 1900, alis: 1450, kimGetirdi: 'tamirci',
        marka: 'Motul', kod: '8100 5W-30', tanim: 'kodlu' },
      { tip: 'parca', ad: 'Yağ filtresi', fiyat: 260, alis: 180, kimGetirdi: 'tamirci',
        marka: 'Mann', kod: 'W 71180', tanim: 'kodlu' },
      { tip: 'iscilik', ad: 'Bakım işçiliği', fiyat: 900, alis: 0, kimGetirdi: '' }
    ] },

  { id: 3, plaka: '35 KLM 45', tur: 'tamirat', km: '',
    sikayet: 'Ön balata değişimi — parçayı müşteri getirdi',
    acilis: '01.09.2026', teslimTarihi: null,
    odemeYolu: null, hatirlatmaTarihi: null, hatirlatmaGonderildi: false,
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
    odemeYolu: 'nakit', hatirlatmaTarihi: '15.09.2026', hatirlatmaGonderildi: false,
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
    odemeYolu: 'havale', hatirlatmaTarihi: '20.12.2026', hatirlatmaGonderildi: false,
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
    odemeYolu: 'nakit', hatirlatmaTarihi: null, hatirlatmaGonderildi: false,
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
    odemeYolu: 'almadim', hatirlatmaTarihi: null, hatirlatmaGonderildi: false,
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
    odemeYolu: 'kart', hatirlatmaTarihi: '20.08.2026', hatirlatmaGonderildi: false,
    satirlar: [
      { tip: 'parca', ad: 'Yağ filtresi', fiyat: 320, alis: 210, kimGetirdi: 'tamirci',
        marka: '', kod: '', tanim: 'tanimlanamadi' },
      { tip: 'iscilik', ad: 'Bakım işçiliği', fiyat: 900, alis: 0, kimGetirdi: '' }
    ] }
];

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

/* İÇERİDEKİ = teslim edilmemiş. Başka tanım yok (K-004). */
function icerideki() {
  return KAYITLAR.filter(function (k) { return !k.teslimTarihi; });
}
function aracKayitlari(plaka) {
  return KAYITLAR.filter(function (k) { return k.plaka === plaka; });
}
function kayitToplami(k) {
  return (k.satirlar || []).reduce(function (t, s) { return t + (s.fiyat || 0); }, 0);
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
      harita[anahtar] = { ad: ad, tel: a.sahipTel || '', araclar: [] };
    }
    harita[anahtar].araclar.push(a);
  });
  return Object.keys(harita).map(function (k) { return harita[k]; })
    .sort(function (x, y) { return x.ad.localeCompare(y.ad, 'tr'); });
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
var GIDERLER = [
  { id: 1, tarih: '01.09.2026', aciklama: 'Elektrik faturası', tutar: 2400, tur: 'fatura' },
  { id: 2, tarih: '02.09.2026', aciklama: 'Öz Yedek Parça — triger seti', tutar: 4200, tur: 'parca' }
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
function donemBasi(donem) {
  if (donem === 'bugun') return new Date(BUGUN.getFullYear(), BUGUN.getMonth(), BUGUN.getDate());
  if (donem === 'ay') return new Date(BUGUN.getFullYear(), BUGUN.getMonth(), 1);
  return null;    /* hepsi */
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
