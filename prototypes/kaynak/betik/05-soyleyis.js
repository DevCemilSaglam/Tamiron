/* ============ SÖYLEYİŞ SÖZLÜĞÜ (16.10.2026) ============
   Cemil, 16.10: *"Araç marka modelleri için okunuşları derleyelim. Mesela
   civic için okunuşlar nasıl olabilir, bunları motorda önce toparlamamız
   faydalı olur derim. Ayrıca egzos, manifolt gibi bir sürü kelime var.
   Bunları da önceden toplayalım."*

   Usta adı yazıldığı gibi değil SÖYLEDİĞİ gibi söyler; tanıyıcı da
   duyduğunu yazar. Bu sözlük duyulabilecek biçimi kayıttaki biçime bağlar.
   İKİ YERDE kullanılır:
     1) motor — araç tarifi (04-islem.js sesAracTarif) ve terim düzeltme
        (sesSoyleyisDuzelt: "eksoz contası" → "egzoz contası");
     2) tanıyıcıya İPUCU — SOYLEYIS_IFADELER(): kayıttaki doğru biçimler
        Web Speech `phrases` olarak verilir ki tanıyıcı baştan doğru yazsın.

   ⚠️ OKUNUŞLAR BİZİM DERLEMEMİZ — ustaya doğrulatılacak (ve 60.1'deki
   dörtlüden — ustanın onayladığı son hâl — zamanla kendiliğinden büyür).
   ⚠️ "!" ile başlayan söyleyiş GERÇEK BİR TÜRKÇE KELİMEDİR ("fiyat",
   "nisan", "kedi", "kurye", "sivil"): yalnız MARKAYLA birlikte araç sayılır
   ve terim düzeltmesinde KULLANILMAZ.
   Anahtarlar `sade()` biçiminde: küçük harf, Türkçe harfsiz, aksansız,
   tire yerine boşluk ("C-Elysée" → "c elysee"). */

var SOYLEYIS_MARKA = {
  'renault': ['reno', 'renol'],
  'fiat': ['!fiyat', 'fiyet'],
  'volkswagen': ['folksvagen', 'volksvagen', 'folkswagen', 'folksvagin', 'vw', 've ve'],
  'ford': ['fort'],
  'toyota': ['toyoto'],
  'hyundai': ['hundai', 'hunday', 'hyunday', 'hundey'],
  'peugeot': ['pejo', 'pejot', 'puju', 'pijo', 'pojo'],
  'citroen': ['sitroen', 'sitroyen', 'sitroin'],
  'dacia': ['dacya', 'dakya', 'dacia'],
  'skoda': ['sikoda'],
  'nissan': ['!nisan'],
  'kia': ['kiya'],
  'mercedes': ['mersedes', 'merso', 'mercedes benz', 'mersedes benz'],
  'bmw': ['be me ve', 'bemve', 'beemve'],
  'audi': ['odi', 'avdi'],
  'seat': ['siat', '!set'],
  'chevrolet': ['sevrole', 'sevrolet'],
  'mitsubishi': ['mitsubisi', 'mitsibisi'],
  'jeep': ['!cip'],
  'tofas': ['tofas'],
  'togg': ['tog'],
  'chery': ['ceri', '!seri']
};

var SOYLEYIS_MODEL = {
  /* Renault */
  'clio': ['klio'], 'megane': ['megan', 'mejan'], 'symbol': ['sembol', 'simbol'],
  'fluence': ['fluens', 'fluans'], 'captur': ['kaptur', 'kapcir', 'kepcir'],
  'kangoo': ['kango', 'kangu'], 'talisman': ['talisman'],
  /* Fiat */
  'egea': ['ega', 'ejea'], 'linea': ['linya'], 'doblo': ['doblo'],
  'fiorino': ['fiyorino'], 'punto': ['punto'],
  /* Volkswagen */
  'passat': ['pasat'], 'jetta': ['ceta', 'jeta'], 'caddy': ['!kedi', 'keddi', 'kadi'],
  'tiguan': ['tigvan', 'tiguvan'], 't roc': ['ti rok', 'tirok'],
  'transporter': ['transporter'],
  /* Ford */
  'focus': ['fokus'], 'fiesta': ['fiyesta'], 'courier': ['kuryer', 'kurier', 'kuriyer', '!kurye'],
  'transit': ['tranzit'], 'connect': ['konnekt', 'konekt'], 'ranger': ['reyncir', 'rencir'],
  /* Toyota */
  'corolla': ['korolla', 'korola'], 'hilux': ['hailaks', 'hilaks'], 'c hr': ['si eyc ar', 'ce ha re'],
  /* Hyundai — girişte harfe yapışık rakam ayrılır: "i20" → "i 20" */
  'i20': ['i 20', 'i yirmi', 'ay yirmi'], 'i10': ['i 10', 'i on'], 'i30': ['i 30', 'i otuz'],
  'accent': ['aksent', 'eksent'], 'tucson': ['tuson', 'tukson', 'takson'],
  /* Opel */
  'corsa': ['korsa'], 'insignia': ['insinya'], 'mokka': ['moka'],
  /* Peugeot / Citroën */
  'partner': ['partner'], 'c elysee': ['se elize', 'si elize', 'selize', 'sielize', 'c elize', 'se elise', 'elize', 'elysee'],
  'c3': ['c 3', 'se uc', 'si uc'], 'c4': ['c 4', 'se dort', 'si dort'], 'berlingo': ['berlingo'],
  /* Dacia */
  'duster': ['daster'], 'sandero': ['sandero'], 'lodgy': ['lodji', 'loci'], 'dokker': ['doker'],
  /* Honda — Cemil'in örneği */
  'civic': ['sivik', 'civik', 'sivic', '!sivil'], 'city': ['siti'], 'cr v': ['ci ar vi', 'ce re ve'],
  'jazz': ['!caz'],
  /* Skoda / Nissan / Kia / Seat */
  'octavia': ['oktavya', 'oktavia'], 'superb': ['superb'], 'fabia': ['fabya'],
  'qashqai': ['kaskay', 'kaskai'], 'juke': ['cuk', 'cuke'], 'micra': ['mikra'],
  'sportage': ['sportaj', 'sportec'], 'ceed': ['sid', 'cid'], 'picanto': ['pikanto'],
  'leon': ['liyon'], 'ibiza': ['ibiza'],
  /* Tofaş */
  'sahin': ['sahin'], 'dogan': ['!dogan'], 'kartal': ['!kartal']
};

/* Parça / iş adlarının söyleyişleri. Metnin KENDİSİ kayıttaki biçime
   çevrilir — "eksoz contası 900 lira" kayıttaki "Egzoz contası"na uysun,
   onay ekranında usta doğru yazımı görsün. Gerçek kelime olan söyleyiş
   ("kartel") buraya GİRMEZ. */
var SOYLEYIS_TERIM = [
  [['egzost', 'eksoz', 'egzos', 'ekzos', 'eksos', 'ekzoz', 'egsoz'], 'egzoz'],
  [['manifolt', 'menifold', 'menifolt', 'manifol'], 'manifold'],
  [['şanzuman', 'sanzuman', 'şanziman', 'sanziman', 'şanjman'], 'şanzıman'],
  [['debriyaç', 'debriyac', 'debreyaj', 'debriyej', 'debiryaj'], 'debriyaj'],
  [['tirger', 'trigır', 'trigir', 'triyger'], 'triger'],
  [['amortisor', 'amortsör', 'amartisör', 'amörtisör', 'amortisör'], 'amortisör'],
  [['enjektor', 'ejektör', 'encektör', 'injektör'], 'enjektör'],
  [['radyator', 'rodyatör', 'radiatör', 'radyatör'], 'radyatör'],
  [['alternator', 'alternetör'], 'alternatör'],
  [['distribütor', 'distrübitör', 'distribitör'], 'distribütör'],
  [['karbirator', 'karbaratör', 'karbürator'], 'karbüratör'],
  [['termostad', 'termosta'], 'termostat'],
  [['kompresor'], 'kompresör'],
  [['bujı', 'buci'], 'buji'],
  [['rotül', 'rotel'], 'rotil'],
  [['salincak', 'salançak'], 'salıncak'],
  [['rulmen', 'rülman'], 'rulman'],
  [['kanpana'], 'kampana'],
  [['diferansiyal', 'difransiyel'], 'diferansiyel'],
  [['kırank'], 'krank'],
  [['eksantirik', 'egsantrik'], 'eksantrik'],
  [['sekman', 'segmen'], 'segman'],
  [['subap', 'supab'], 'supap'],
  [['silindır', 'slindir'], 'silindir'],
  [['sensor'], 'sensör'],
  [['interkuler', 'intırkulır', 'interkulır'], 'intercooler'],
  [['direksyon', 'dreksiyon'], 'direksiyon'],
  [['silicek'], 'silecek'],
  [['antifiriz'], 'antifriz'],
  [['kece'], 'keçe'],
  [['bicon'], 'bijon'],
  [['civata'], 'cıvata'],
  [['rotbalans'], 'rot balans']
];

/* Tanıyıcıya verilecek İPUCU listesi (Web Speech `phrases`): kayıttaki
   DOĞRU biçimler — dükkânın plakaları, araçlarının marka ve modeli,
   terimler, işlerin sözleri. Söyleyişler değil: tanıyıcının yazmasını
   istediğimiz şey doğru biçim. */
var SOYLEYIS_IS_SOZLERI = ['teslim', 'tahsilat', 'işçilik', 'kilometre', 'WhatsApp', 'SMS',
  'nakit', 'kart', 'havale', 'sahibine', 'balata', 'triger', 'devirdaim', 'gider', 'fatura'];

function SOYLEYIS_IFADELER(araclar) {
  var gorulen = {}, liste = [];
  function ekle(s) { s = String(s || '').trim(); if (s && !gorulen[s]) { gorulen[s] = 1; liste.push(s); } }
  (araclar || (typeof ARACLAR !== 'undefined' ? ARACLAR : [])).forEach(function (a) {
    ekle(a.plaka); ekle(a.marka); ekle(a.model); ekle((a.marka || '') + ' ' + (a.model || ''));
  });
  SOYLEYIS_TERIM.forEach(function (t) { ekle(t[1]); });
  SOYLEYIS_IS_SOZLERI.forEach(ekle);
  return liste;
}
