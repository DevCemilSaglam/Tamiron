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
    sahipAd: 'Mehmet Kaya', sahipTel: '0532 215 30 00', not: 'Arka cam kolu kırık — müşteri biliyor.' },
  { plaka: '06 TMR 07', marka: 'Fiat', model: 'Egea', yil: '2021',
    motor: '1.4 Fire', vin: 'NM4358004ZT129041',
    sahipAd: 'Hasan Demir', sahipTel: '0542 331 90 07', not: '' },
  { plaka: '35 KLM 45', marka: 'Peugeot', model: '301', yil: '2019',
    motor: '1.6 BlueHDi', vin: 'VF3DDBHYAKS012345',
    sahipAd: 'Ayşe Kaya', sahipTel: '', not: '' },
  { plaka: '16 XY 8890', marka: 'Volkswagen', model: 'Golf', yil: '2016',
    motor: '1.6 TDI', vin: 'WVWZZZAUZGW123456',
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

  /* Teslim edilmiş — ana ekranda GÖRÜNMEZ, araç geçmişinde görünür */
  { id: 4, plaka: '34 ABC 123', tur: 'bakim', km: '142.500',
    sikayet: 'Periyodik bakım',
    acilis: '15.03.2026', teslimTarihi: '15.03.2026',
    odemeYolu: 'nakit', hatirlatmaTarihi: '15.09.2026', hatirlatmaGonderildi: false,
    satirlar: [
      { tip: 'parca', ad: 'Motor yağı', fiyat: 1800, alis: 1400, kimGetirdi: 'tamirci',
        marka: 'Motul', kod: '8100 5W-30', tanim: 'kodlu' },
      { tip: 'iscilik', ad: 'Bakım işçiliği', fiyat: 850, alis: 0, kimGetirdi: '' }
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
  bakimPeriyoduAy: 6            /* A-3 — Cemil onayına açık */
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
