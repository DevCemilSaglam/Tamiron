/* ============ SESLİ İŞLEM ŞEMALARI + KORPUS — prototip → API (D15 · K-076.14) ============
   Prototipin KENDİ çözümleyicisini (betik/*.js + ekranların <script>'leri, şemalar ekranlarda
   bildirilir) Node'da çalıştırır — yeniden yazmaz — ve iki dosya döker:

     src/Tamiron.Api/Ses/islem-semalari.json      Islem.json(): işlem türleri, alanlar,
                                                   zorunlular — API bunu sunar ve doğrular
     tests/Tamiron.Api.Tests/Esdeger/prototip-ses.json
                                                   cümle × bağlam → prototipin işlem nesnesi;
                                                   (1) şema ↔ çözümleyici tutarlılığı sınanır,
                                                   (2) sunucuya taşınacak çözümleyicinin / dil
                                                   modelinin 0-fark hedefidir

   KULLANIM:  node tools/ses-sema-dok.js
   Prototipte bir şema ya da çözümleyici değişirse araç yeniden koşulur (CI'da da: backend.yml).
   Deterministik: değişmemiş prototipte dosyalar bayt bayt aynı çıkar. */
const fs = require('fs');
const path = require('path');
const vm = require('vm');

const KOK = path.join(__dirname, '..');
const BETIK = path.join(KOK, 'prototypes', 'kaynak', 'betik');
const EKRAN = path.join(KOK, 'prototypes', 'kaynak', 'ekranlar');
const SEMA_CIKTI = path.join(KOK, 'src', 'Tamiron.Api', 'Ses', 'islem-semalari.json');
const KORPUS_CIKTI = path.join(KOK, 'tests', 'Tamiron.Api.Tests', 'Esdeger', 'prototip-ses.json');

/* Tarayıcı yok: DOM'a dokunan her şey sessiz bir "her şey" nesnesine düşer (muhasebe-esdeger ile aynı). */
function herSey() {
  const f = function () { return herSey(); };
  return new Proxy(f, {
    get: (t, k) => (k === Symbol.toPrimitive ? () => '' : k === 'length' ? 0 : herSey()),
    apply: () => herSey(), construct: () => herSey(), set: () => true
  });
}
const ctx = {
  console, Math, Date, JSON, Object, Array, String, Number, RegExp, Error, Symbol, Promise, setTimeout, clearTimeout,
  document: herSey(), navigator: herSey(), location: herSey(), history: herSey(),
  localStorage: { getItem: () => null, setItem() {}, removeItem() {} },
  matchMedia: () => ({ matches: false, addEventListener() {}, addListener() {} }),
  addEventListener() {}, requestAnimationFrame() {}, getComputedStyle: () => herSey()
};
ctx.window = ctx; ctx.self = ctx; ctx.globalThis = ctx;
vm.createContext(ctx);
for (const d of fs.readdirSync(BETIK).filter(f => f.endsWith('.js')).sort())
  vm.runInContext(fs.readFileSync(path.join(BETIK, d), 'utf8'), ctx, { filename: d });
/* Ekran betikleri derlemede kendi IIFE'sine sarılır (prototip-derle.js) — burada da. */
for (const d of fs.readdirSync(EKRAN).filter(f => f.endsWith('.html')).sort()) {
  const html = fs.readFileSync(path.join(EKRAN, d), 'utf8');
  for (const m of html.matchAll(/<script>([\s\S]*?)<\/script>/g))
    vm.runInContext('(function () {\n' + m[1] + '\n})();', ctx, { filename: d });
}
const run = (s) => vm.runInContext(s, ctx);

const BAGLAMLAR = ['genel', 'servis-ekle', 'araclar', 'kayitlar', 'musteriler', 'mesaj', 'muhasebe', 'kayit-icinde'];
/* ses-esdeger.js'in ek cümleleri — ekran örnekleri ve tuzaklar. */
const EK = ['balata da degisecek', 'iscilik 750 lira', 'yag filtresi 300 lira',
  'teslim ettim nakit aldim', 'teslim ettim kartla', 'servistekileri goster',
  '34 ABC 123 sahibine selam at', 'bugun ne kadar tahsilat', 'bu ay ne kadar tahsil ettim',
  '2400 lira elektrik faturasi', '850 lira kira gideri ekle', 'herkese sms at yarin kapaliyiz',
  'tum musterilere bayram mesaji gonder', '41 KV 5588 yeni arac bakim',
  '34 ABC 123 beyaz araba geldi', '34 ABC 123 gosterge paneli yaniyor',
  '34 QBC 123 sahibine whatsapp selam mesaji gonder', '99 ZZZ 999 teslim ettim',
  'otuz dort a b c yuz yirmi uc bakima geldi', '41 KV 5588 Ahmet Yilmaz 0532 111 22 33 debriyaj kaciriyor',
  'merhaba', '', 'balata 450 lira', 'parca 300 lira', 'fren hidroligi 120 lira parca',
  '34 ABC 123 ara', 'Mehmet ara', '34 ABC 123 sahibini ara', '500 lira'];

/* E3/E4 (K-077.1) — C#'a taşınan çözümleyicinin 0-fark hedefi için EKSİK DALLAR ve prototipin
   kendi notlarındaki tuzaklar. Beklenen değer yine prototipin çıktısı; burada yalnız cümle seçilir. */
const EK2 = [
  // fiyat-guncelle (kayıt içi, 1 numaralı kaydın satırı "Triger seti") — tam, kısmi, bozuk söyleyiş
  'triger seti 4500 lira olsun', 'triger 4500', 'Driver seti 4500 lira', 'triger seti fiyati 4500 yap',
  // teslim / teslim-edilecek-kayit-yok / bilinmeyen-arac
  '34 ABC 123 teslim ettim nakit', '16 SY 8890 teslim ettim', '06 TMR 07 teslim ettim kk ile', 'teslim ettim eft yaptı',
  '99 ZZZ 999 sahibine mesaj gönder', '06 TMR 08 sahibine selam yaz',
  // mesaj şablonları ve tuzaklar
  '34 ABC 123 sahibine hazır mesajı gönder', '06 TMR 07 sahibine bekliyor mesajı sms ile',
  '35 KLM 45 hatırlatma mesajı gönder', '34 ABC 123 sahibine bayram mesajı', '16 SY 8890 teşekkür mesajı yaz',
  '34 ABC 123 sahibinden parça aldım', '34 ABC 123 sahibinden bekliyor mesajı', '06 TMR 07 teslim mesajı gönder',
  // toplu
  'bütün müşterilere bayram mesajı gönder', 'herkese yarın kapalıyız yaz', 'tüm müşterilere mesaj gönder',
  // ara
  'Mehmet Kaya ara', '06 TMR 07', 'serviste ne var', 'Renault ara', 'içerideki araçlar',
  // gider / tahsilat
  'elektrik faturası 2.400', 'bu yıl ne kadar kasa', 'kira 15000', 'bu ay 3 fatura geldi', 'kira gideri 0532 111 22 33',
  'su faturası 2 bin 400', 'kasada ne var', 'ne kadar tahsil ettim bu sene',
  // servis-ac / yeni-arac / tarif / bağlama
  "Mehmet Kaya'nın arabası geldi yağ değişimi", "Mégane'ın triger seti değişecek", 'Fiat Egea balata',
  '35 kelime 45 bakım', '34 QBC 123 bakıma geldi', '06 PVR 07 fren balatası', '16 SY 88 90 geldi',
  'okulda 8890 geldi debriyaj', '34 ABC 123 kilometre 88 bin 900 bakım', '34ABC123 eksoz patladı',
  '34 ABC 123 33 yok 23 bin km bakım', 'saat 10:30 da 06 TMR 07 gelecek', '41 KV 5588 İsmail Özdemir 05321112233 ön takım',
  '34 M 49109 periyodik bakım', '06 TMR 07 bakuma geldi',
  // kayıt içi satır
  'işçiliği 600 lira yaz', 'motor yağı 4500', 'buji 4', 'yarın 123 tane balata gelecek', 'amortisor 2.750',
];

const set = JSON.parse(fs.readFileSync(path.join(KOK, 'tools', 'degerlendirme', 'ses-seti.json'), 'utf8'));
const durumlar = [];
for (const c of set.cumleler) durumlar.push({ kaynak: 'ses-seti#' + c.id, cumle: c.cumle, baglam: c.baglam });
for (const cumle of EK) for (const baglam of BAGLAMLAR) durumlar.push({ kaynak: 'ek', cumle, baglam });
for (const cumle of EK2) for (const baglam of BAGLAMLAR) durumlar.push({ kaynak: 'ek2', cumle, baglam });
/* Mutasyonla bulunan iki kör nokta (E3/E4):
   - TEK ADAY kuralı (A-M27): 1 numaralı kaydın tek satırı var, "iki satır uyuyor" hiç oluşmuyordu.
     3 numaralı kayıtta "Ön fren balatası" ve "Balata değişimi" — "balata" ikisine birden uyar.
   - JS \b ASCII'dir: hazırlık Türkçe harfle rakamı ayırıyor ama "é"yi ayırmıyor ("Mégane34 …"). */
const EK3 = ['balata 900 lira olsun', 'ön fren balatası 900 lira', 'balata değişimi 850', 'fren 700 lira', 'teslim ettim havale'];
for (const cumle of EK3) durumlar.push({ kaynak: 'ek3', cumle, baglam: 'kayit-icinde', kayit: 3 });
for (const cumle of ['Mégane34 ABC 123 geldi', 'Clio16 SY 8890 bakım', 'café06 TMR 07 balata'])
  for (const baglam of BAGLAMLAR) durumlar.push({ kaynak: 'ek3', cumle, baglam });
/* E5 (K-077.2) · Ödeme yolunu düzelt — "değil"den sonraki yol, en son söylenen yol, "almadım"
   sesle düzeltilmez. Kayıt içi: 7 (teslim, nakit) ve 1 (AÇIK — düzeltme yok, teslim işi). */
const EK4 = ['nakit değil kartla aldım', 'kartla değil nakit aldım', 'yanlış yazdım havale olacak',
  'ödeme yanlış kk olacak', 'nakit yanlış kart olacak', 'ödemeyi karta çevir', 'nakit değil eft',
  'parayı almadım nakit değil', 'ödemeyi değiştir', 'teslim ettim nakit değil kartla',
  // "kk" kelime olarak aranır: "hakkını" içindeki kk KART değildir (mutasyonla bulundu, E5)
  'kart değil nakit hakkını verdim'];
// 8 = veresiye ("almadım"): düzeltme hedefi DEĞİL — kayıt açıkken de (mutasyonla bulundu, E5)
for (const cumle of EK4) for (const kayit of [7, 1, 8]) durumlar.push({ kaynak: 'ek4', cumle, baglam: 'kayit-icinde', kayit });
for (const cumle of ['35 KLM 45 ödemesi nakit değil kart', '06 TMR 07 ödemeyi havaleye çevir', '34 ABC 123 kartla değil nakit aldım',
  '16 SY 8890 nakit değil eft', '34 ABC 123 ödeme yanlış', '99 ZZZ 999 nakit değil kart'])
  for (const baglam of BAGLAMLAR) durumlar.push({ kaynak: 'ek4', cumle, baglam });

/* EK5 · "2 bin aldım" — elden taksit (K-078 · F7). Kayıt içinde: 8 veresiye (hedef), 7 ödenmiş ve
   1 açık (hedef DEĞİL — açık kayıtta "aldım" parçadır). Plakayla her bağlamda: 06 TMR 07'nin veresiyesi
   var, 34 ABC 123'ün yok. Tuzaklar: lirasız sayı fiilden önce değilse tutar değil; "değil" kuralı;
   "kalanını/hepsini"; tek haneli sayı; plaka rakamı tutar sanılmasın. */
const EK5 = ['2 bin aldım nakit', 'hepsini aldım havale', 'kalanını kartla aldım', '500 aldım', '1500 lira ödedi kartla',
  'balata aldım 500', 'nakit değil kartla 2 bin aldım', '5 aldım', 'müşteri 750 verdi', 'eft ile 1000 yatırdı',
  'aldım 2000', 'iki bin lira aldım kartla',
  // F7 mutasyonla bulunan iki kör nokta: fiilsiz tutar ödeme DEĞİL; "verdikten" fiil değil (sınır).
  '2000 lira nakit', '500 verdikten sonra 300 aldım'];
for (const cumle of EK5) for (const kayit of [8, 7, 1]) durumlar.push({ kaynak: 'ek5', cumle, baglam: 'kayit-icinde', kayit });
for (const cumle of ['06 TMR 07 için 2 bin aldım nakit', '06 TMR 07 için 2000 lira aldım', '06 TMR 07 kalanını kartla aldım',
  '06 TMR 07 5000 aldım', '34 ABC 123 için 2 bin aldım', '06 TMR 07 hepsini aldım', '06 TMR 07 07 aldım'])
  for (const baglam of BAGLAMLAR) durumlar.push({ kaynak: 'ek5', cumle, baglam });

/* Açık kayıt: kayıt içi bağlamda prototipin 1 numaralı kaydı (tohum). Araç nesnesi (`arac`)
   tohumdan gelir ve sunucuda aracın KENDİSİ okunur — korpusta yalnız plakası tutulur. */
const sade = (o) => {
  if (!o || typeof o !== 'object') return o;
  const r = {};
  for (const k of Object.keys(o).sort()) {
    const v = o[k];
    if (k === 'arac') r[k] = v && v.plaka ? v.plaka : v;
    else if (k === 'satir' && v && typeof v === 'object') r[k] = v.ad || null;
    // Kayıt nesnesinin tamamı değil KİMLİĞİ: hangi kayıt olduğu ölçülür, kaydın içeriği tohumdur.
    else if (k === 'kayit' && v && typeof v === 'object') r[k] = v.id;
    else if (v && typeof v === 'object' && !Array.isArray(v)) r[k] = sade(v);
    else r[k] = v;
  }
  return r;
};
const korpus = durumlar.map(d => ({
  ...d,
  islem: sade(run(`Islem.cozumle(${JSON.stringify(d.cumle)}, ${JSON.stringify(d.baglam)}, ${d.baglam === 'kayit-icinde' ? (d.kayit || 1) : 'null'})`))
}));

/* E3 (K-077.1) · Cihazdaki C# çözümleyicinin VERİSİ — söyleyiş sözlüğü ve hazır metinler. Kural
   değil veri; tek kaynak prototip (05-soyleyis.js, 02-veri.js HAZIR_METIN). */
const SOZLUK_CIKTI = path.join(KOK, 'src', 'Tamiron.Ui.Core', 'Ses', 'soyleyis.json');
fs.mkdirSync(path.dirname(SOZLUK_CIKTI), { recursive: true });
fs.writeFileSync(SOZLUK_CIKTI, JSON.stringify(run(`({
  terim: SOYLEYIS_TERIM, marka: SOYLEYIS_MARKA, model: SOYLEYIS_MODEL,
  hazir: HAZIR_METIN.map(function (h) { return { k: h.k, kokler: h.kokler, metin: h.metin }; })
})`), null, 1) + '\n');

const semalar = run('Islem.json()');
fs.mkdirSync(path.dirname(SEMA_CIKTI), { recursive: true });
fs.writeFileSync(SEMA_CIKTI, JSON.stringify(semalar, null, 1) + '\n');
fs.writeFileSync(KORPUS_CIKTI, JSON.stringify({ kaynak: 'node tools/ses-sema-dok.js', turler: semalar.map(s => s.name), korpus }, null, 1) + '\n');
const sayac = {};
for (const k of korpus) sayac[k.islem.tur] = (sayac[k.islem.tur] || 0) + 1;
console.log(semalar.length + ' şema · ' + korpus.length + ' durum · ' + JSON.stringify(sayac));
console.log('yazıldı: ' + path.relative(KOK, SEMA_CIKTI) + ', ' + path.relative(KOK, KORPUS_CIKTI));
