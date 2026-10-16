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
  'chery': ['ceri', '!seri'],
  /* 16.10 genişletme — ticari ve sık gelen markalar */
  'iveco': ['iveko', 'ivekko', 'ayveko'],
  'isuzu': ['izuzu', 'isuzi'],
  'mazda': ['mazda'],
  'suzuki': ['suzuki', 'sizuki'],
  'volvo': ['volvo'],
  'subaru': ['subaru'],
  'land rover': ['lend rover', 'land rover', 'lendrover'],
  'porsche': ['porse', 'porsh'],
  'cupra': ['kupra'],
  'mg': ['me ge', 'em ci', 'emci'],
  'byd': ['be ye de', 'bi way di']
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
  'sahin': ['sahin'], 'dogan': ['!dogan'], 'kartal': ['!kartal'],
  /* 16.10 genişletme — TİCARİ (minibüs, kamyonet: tamirhanenin sık müşterisi) */
  'sprinter': ['ispirinter', 'sipirinter', 'sprinter'], 'vito': ['vito', 'vitto'],
  'ducato': ['dukato', 'dukkato'], 'transit custom': ['tranzit kastim', 'transit kastim', 'tranzit kustom'],
  'trafic': ['!trafik'], 'master': ['!master', 'mastir'], 'daily': ['deyli', 'dayli', '!deli'],
  'boxer': ['bokser', 'boksir'], 'jumper': ['cumper', 'campır', 'camper'], 'expert': ['ekspert', 'eksper'],
  'crafter': ['krafter', 'kraftır'], 'amarok': ['amarok'], 'navara': ['navara'], 'l200': ['l 200', 'el iki yuz'],
  'h 100': ['h100', 'ha yuz', 'hayuz'], 'bongo': ['bongo'], 'd max': ['di maks', 'dimaks', 'de maks'],
  /* Binek — eksikler */
  'astra': ['astra'], 'vectra': ['vektra'], 'meriva': ['meriva'], 'zafira': ['zafira'],
  'mondeo': ['mondeo'], 'kuga': ['kuga'], 'puma': ['puma'],
  'golf': ['golf'], 'polo': ['polo'], 'scirocco': ['siroko', 'sirokko'], 'touran': ['!turan'],
  'rifter': ['rifter'],
  'c5 aircross': ['c5 eyrkros', 'se bes eyrkros'],
  'tipo': ['tipo'], 'auris': ['oris'], 'yaris': ['yaris'], 'corolla cross': ['korolla kros'], 'arteon': ['arteon'], 'bora': ['!bora'], 'panda': ['panda'], 'palio': ['palyo', 'palio'], 'albea': ['albea', 'albeya'],
  'taliant': ['talyant', 'taliyant'], 'austral': ['ostral'], 'koleos': ['koleos'],
  'hr v': ['eyc ar vi', 'ha re ve'],
  'kona': ['kona'], 'bayon': ['bayon'], 'santa fe': ['santa fe', 'santafe'], 'ix35': ['i iks 35', 'ay iks otuz bes'],
  'stonic': ['stonik'], 'niro': ['niro'], 'sorento': ['sorento'], 'rio': ['riyo'],
  'x trail': ['iks treyl', 'ekstreyl'], 'note': ['!not'], 'kamiq': ['kamik'], 'karoq': ['karok'],
  'kodiaq': ['kodiyak', 'kodyak'], 'scala': ['skala'], 'arona': ['arona'], 'ateca': ['ateka'],
  'formentor': ['formentor'], 'swift': ['svift', 'suift'], 'vitara': ['vitara'], 'cx 5': ['si eks 5', 'ce iks bes']
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
  [['rotbalans'], 'rot balans'],
  /* 16.10 genişletme — yalnız YANLIŞ söyleyiş; eş anlamlı DEĞİL ("klakson"
     ile "korna" ikisi de doğru, biri ötekine çevrilmez). */
  [['porya', 'porye', 'poyre'], 'poyra'],
  [['kalipır', 'kalipir', '!kaliber'], 'kaliper'],
  [['musir', 'müsir'], 'müşir'],
  [['hareret', 'hararat'], 'hararet'],
  [['susdurucu', 'susdurugu'], 'susturucu'],
  [['rektefiye', 'rektifye', 'rektafiye'], 'rektifiye'],
  [['lamda'], 'lambda'],
  [['devirdaym', 'devridaim', 'devir daim'], 'devirdaim'],
  [['debi metre', 'debimetere'], 'debimetre'],
  [['türbo'], 'turbo'],
  [['katalizor', 'katalizatör'], 'katalizör'],
  [['şarz dinamosu', 'sarz dinamosu', 'şarj dinamusu'], 'şarj dinamosu'],
  [['edblu', 'ad blu', 'adblu', 'ed blu'], 'AdBlue'],
  [['helozon', 'helezun'], 'helezon'],
  [['cant'], 'jant'],
  [['vitez'], 'vites'],
  [['e ge re', 'egr valfi'], 'EGR'],
  [['de pe fe'], 'DPF'],
  [['a be se'], 'ABS'],
  [['e se pe'], 'ESP'],
  [['marş motur', 'mars motoru'], 'marş motoru'],
  [['aks körüğü', 'aks köryüğü', 'aks korugu'], 'aks körüğü'],
  [['gergi rulmani', 'gergi rülmanı'], 'gergi rulmanı'],
  [['tirger kayışı', 'triger kayisi'], 'triger kayışı'],
  [['silindir kapağı', 'silindir kapagi', 'slindir kapağı'], 'silindir kapağı'],
  [['yağ müşiri', 'yag musiri'], 'yağ müşiri'],
  [['fren hidroliği', 'fren hidroligi', 'fren hidrolik yağı'], 'fren hidroliği'],
  [['debriyaj merkezi', 'debriyaç merkezi'], 'debriyaj merkezi'],
  [['viraj demiri', 'viraj demirı'], 'viraj demiri'],
  [['zet rot', 'z rot'], 'Z rot'],
  [['enjektör memesi', 'enjektor memesi'], 'enjektör memesi']
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
