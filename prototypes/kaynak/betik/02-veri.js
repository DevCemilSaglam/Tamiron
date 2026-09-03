/* ============ SAHTE VERİ ============
   Backend yok. Alanlar VERI-MODELI.md ile birebir — modelde olmayan
   alan buraya da girmez.

   ⚠️ İSKELET: 13. işte gerçek örnek veriyle genişletilecek. */

/* Vehicle — VERI-MODELI §1 */
var ARACLAR = [
  { plaka: '34 ABC 123', marka: 'Renault', model: 'Mégane', yil: '2018',
    motor: '1.5 dCi', vin: 'VF1RFB00858123456',
    sahipAd: 'Mehmet Kaya', sahipTel: '0532 215 30 00', not: '' },
  { plaka: '06 TMR 07', marka: 'Fiat', model: 'Egea', yil: '2021',
    motor: '1.4 Fire', vin: 'NM4358004ZT129041',
    sahipAd: 'Hasan Demir', sahipTel: '0542 331 90 07', not: '' },
  { plaka: '35 KLM 45', marka: 'Peugeot', model: '301', yil: '2019',
    motor: '1.6 BlueHDi', vin: 'VF3DDBHYAKS012345',
    sahipAd: 'Ayşe Kaya', sahipTel: '', not: '' }
];

/* Supplier — VERI-MODELI §4. İki alan: ad + telefon. */
var PARCACILAR = [
  { ad: 'Öz Yedek Parça', tel: '0532 111 22 33' },
  { ad: 'Ahmet Oto Yedek Parça', tel: '0533 444 55 66' }
];
