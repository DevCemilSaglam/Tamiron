/* ============ KOŞUCU 3 — FOTOĞRAF OKUMA (Node) ============
   Cemil, 15.09: 46 ruhsat + 27 parça listesi fotoğrafı gönderdi.

   Bu koşucu fotoğrafı Claude'a verir, alanları/satırları ister ve
   ölçütle karşılaştırır. Ses koşucusuyla AYNI cetveli kullanıyor
   (puanla.js) — ikisi ayrı cetvel kullansaydı iki karne
   karşılaştırılamazdı.

   ⚠️ FOTOĞRAFLAR DEPODA DEĞİL: içlerinde TC kimlik no ve adres var
   (RUHSAT-OKUMA Bulgu 2 · A-31). `ruhsatlar/` ve `parcalar/` klasörleri
   .gitignore'da. Koşmadan önce fotoğrafları oraya koy:

     tools/degerlendirme/ruhsatlar/r01.jpg … r46.jpg
     tools/degerlendirme/parcalar/p01.jpg … p27.jpg

   Sıra, Cemil'in gönderdiği sıradır; her kaydın plakası/kâğıdı
   ölçütte yazılı olduğu için eşleştirme gözle doğrulanabilir.

   KULLANIM:
     npm install @anthropic-ai/sdk
     set ANTHROPIC_API_KEY=sk-ant-...
     node tools/degerlendirme/foto-kos.js --tur ruhsat
     node tools/degerlendirme/foto-kos.js --tur parca --model claude-sonnet-5
     node tools/degerlendirme/foto-kos.js --tur ruhsat --sadece r31,r45

   MALİYET: fotoğraf başına ~1.500–2.500 jeton girdi. 46 ruhsat için
   kaba hesap — Opus 5 ~$0,60 · Sonnet 5 ~$0,25 · Haiku 4.5 ~$0,12.
   Her koşu GERÇEK PARA harcar; Cemil onaylamadan koşturulmaz. */

const fs = require('fs');
const path = require('path');
const { alanPuanla, listePuanla } = require('./puanla.js');

const KOK = __dirname;
const arg = (ad, varsayilan) => {
  const i = process.argv.indexOf(ad);
  return i > 0 && process.argv[i + 1] ? process.argv[i + 1] : varsayilan;
};

const TUR = arg('--tur', 'ruhsat');
const MODEL = arg('--model', 'claude-opus-5-5');

/* $ / 1M jeton — [girdi, çıktı, önbellek okuma]; model-kos.js ile aynı
   tablo. Koşu sonunda GERÇEK maliyet basılır (A-M35'in sorusu bu). */
const FIYAT = {
  'claude-opus-5-5': [4, 20, 0.20], 'claude-opus-5': [5, 25],
  'claude-sonnet-5-5': [2, 10, 0.20], 'claude-sonnet-5': [2, 10],
  'claude-haiku-5-5': [0.10, 0.50], 'claude-haiku-4-5': [1, 5],
  /* Gemini ücretli katman (ai.google.dev/gemini-api/docs/pricing, 18.10):
     3.8 Flash 31.12.2026'ya kadar bu fiyat, 01.01.2027'den İKİ KATI
     ($1,50 / $7,50). Çıktıya düşünme jetonları dahil. Önbellek yok. */
  'gemini-3.8-flash': [0.75, 3.75, 0.75], 'gemini-3.5-flash-lite': [0.30, 2.50, 0.30]
  /* ollama: yerel, ücret yok — FIYAT'ta yoksa maliyet basılmaz. */
};

/* FOTOĞRAF HARİTASI (18.10): fotoğraflar depoya kopyalanmaz — içlerinde
   TC no ve adres var. `foto-harita.json` (foto-esle.js üretir, MD5 ile
   eşleşmiş, .gitignore'da) hangi kaydın hangi dosya olduğunu söyler. */
const HARITA_YOL = path.join(KOK, 'foto-harita.json');
const HARITA = fs.existsSync(HARITA_YOL) ? JSON.parse(fs.readFileSync(HARITA_YOL, 'utf8')).yollar || {} : {};
const SADECE = arg('--sadece', '');
/* MALİYET DENEMELERİ (18.10, K-067): Opus'ta ruhsat maliyetinin üçte
   ikisi ÇIKTI — 18 kısa alan ~150 jeton, kalan ~850 jeton modelin
   düşünmesi. `--efor low|medium|high` düşünmeyi kısar. `--foto-kok`
   küçültülmüş fotoğrafların klasörü (foto-kucult.ps1); haritadan önce
   bakılır. `--etiket` sonuç dosyasının adına eklenir — iki deneme aynı
   modelde birbirinin üstüne yazmasın. */
const EFOR = arg('--efor', null);
const FOTO_KOK = arg('--foto-kok', null);
const ETIKET = arg('--etiket', '');
/* SAĞLAYICI (18.10): Cemil "şu an ödeme yapamam, başka ücretsiz model yok
   mu?" dedi. Aynı istem, aynı şema, AYNI CETVEL — yalnız okuyan değişir:
     claude  — Anthropic API (varsayılan)
     gemini  — Google Gemini API, Cemil'in ÖNCEDEN YÜKLEDİĞİ kredi (ücretli
               katman: içerik eğitimde kullanılmaz). ⚠️ Ücretsiz katmanla
               KOŞULMAZ: ruhsatta gerçek kişilerin TC no'su ve adresi var.
     ollama  — bilgisayardaki yerel açık model (Gemma 4); fotoğraf
               bilgisayardan ÇIKMAZ, ücret yok, yavaş. */
const SAGLAYICI = arg('--saglayici', 'claude');

const AYAR = {
  ruhsat: {
    set: 'ruhsat-seti.json', klasor: 'ruhsatlar', kayitAlani: 'belgeler',
    /* ⚠️ İSTEM İKİ ŞEY SÖYLÜYOR VE İKİNCİSİ DAHA ÖNEMLİ:
       ne okunacağı, ve NE OKUNMAYACAĞI. */
    sistem: `Sen bir tamirhane programının ruhsat okuyucususun. Türk araç
tescil belgesinin (ruhsat) fotoğrafını alır, alanlarını çıkarırsın.

KURALLAR:
1. YALNIZ BELGEDE YAZANI OKU. Aracın ne olması gerektiğini bilmen
   önemli değil; belgede ne yazıyorsa o. Belgedeki rakam sana tuhaf
   gelse bile düzeltme.
2. OKUYAMADIĞIN ALANI BOŞ BIRAK (null). Parmak kapatıyorsa, silikse,
   kırpılmışsa: null. Uydurma. Yanlış bilgi, eksik bilgiden çok daha
   pahalıdır — usta onu bir daha kontrol etmez.
3. BELGEDE OLMAYAN ALANI BOŞ BIRAK. Eski tip ruhsatlarda yakıt cinsi,
   motor gücü, silindir hacmi ve koltuk sayısı YOKTUR.
4. TC KİMLİK NO / VERGİ NO, SOYADI-ÜNVAN, ADI ve ADRESİ DE OKU.
   Bunlar faturanın kesileceği bilgiler (K-050). Kişide 11 haneli TC,
   firmada 10 haneli vergi numarası olur.
5. VERGİ DAİRESİ ruhsatta YAZMAZ — böyle bir alan döndürme.
6. Plakayı boşluklu yaz: "33VAH34" değil "33 VAH 34".
7. Belge yan ya da ters çekilmiş olabilir; döndürüp oku.`
  },
  parca: {
    set: 'parca-seti.json', klasor: 'parcalar', kayitAlani: 'listeler',
    sistem: `Sen bir tamirhane programının parça listesi okuyucususun.
Ustanın ya da parçacının el yazısıyla yazdığı parça listesinin
fotoğrafını alır, satırlarını çıkarırsın.

KURALLAR:
1. DITTO İŞARETLERİNİ ÇÖZ. Bir satır " " " ya da benzeri bir tekrar
   işaretiyle başlıyorsa, üstündeki satırın konusunu devralır:
   'Sağ ön çamurluk sinyali' / '  "   "   " çıtası'
   -> ikinci satır 'Sağ ön çamurluk çıtası' demektir.
2. ÜSTÜ ÇİZİLİ SATIRI SİLME, "iptal": true diye işaretle. Neden
   çizildiğini usta bilir, program bilmez.
3. FİYAT YERİNE YAZILMIŞ CEVABI fiyat sanma: 'Yok', 'Gelmez',
   'Geliniyor', 'Klimacıya bakılacak' birer DURUMdur -> "durum" alanı.
4. BİR SATIRDA İKİ FİYAT olabilir: 'YN' yan sanayi, 'ORJ' orijinal
   demektir -> fiyat ve fiyatIkinci.
5. ÇARPIM SATIRINA DİKKAT: '4 buji = 230 ORJ x4 920' -> birim 230,
   adet 4, toplam 920. Toplamı birim fiyat sanma.
6. OKUYAMADIĞIN SATIRI UYDURMA: "okunamadi": true yaz.
7. Araç bilgisi listenin ÜSTÜNDE de ALTINDA da olabilir, hiç de
   olmayabilir. Yoksa null.
8. Bazı satırlar parça değil İŞÇİLİKTİR (rektifiye, yağ bakımı, rot
   ayarı, disk taşlama) -> "tip": "iscilik".`
  }
}[TUR];

if (!AYAR) { console.error('--tur ruhsat ya da parca olmalı'); process.exit(1); }

const set = JSON.parse(fs.readFileSync(path.join(KOK, AYAR.set), 'utf8'));

/* ---- KİŞİSEL ÖLÇÜT (15.09 · K-050) ----
   TC/vergi no, ad ve adres artık ÖLÇÜLÜYOR — fatura bunlara kesiliyor.
   Ama değerleri depoda durmuyor: gerçek insanların kimlik numarası bir
   GitHub deposunda kalıcı olarak kalmaz. Yerel dosya varsa ölçüte
   karışıyor; yoksa bu alanlar ÖLÇÜME GİRMİYOR ve koşucu bunu SÖYLÜYOR —
   ölçemediğini ölçmüş gibi raporlayan araç, ölçüm aleti değildir. */
const KISISEL_YOL = path.join(KOK, AYAR.set.replace('.json', '-kisisel.json'));
let kisiselVar = false;
if (fs.existsSync(KISISEL_YOL)) {
  const kisisel = JSON.parse(fs.readFileSync(KISISEL_YOL, 'utf8'));
  const tablo = kisisel.belgeler || kisisel.listeler || {};
  set[AYAR.kayitAlani].forEach(k => {
    const ek = tablo[k.id];
    /* Dosyada adı geçmeyen belgede doğru cevap BOŞTUR — o yüzden
       alanlar yine ekleniyor, null değerle. Eklenmezse "uydurma"
       sınavı hiç koşmaz. */
    (kisisel.alanlar ? Object.keys(kisisel.alanlar) : []).forEach(alan => {
      k.beklenen[alan] = ek ? (ek[alan] === undefined ? null : ek[alan]) : null;
    });
  });
  kisiselVar = true;
}

const kayitlar = set[AYAR.kayitAlani]
  .filter(k => !SADECE || SADECE.split(',').indexOf(k.id) >= 0);

/* Şema: ölçütteki alanlardan türetiliyor — ikinci bir yerde şema
   tutmak, ölçüt değişince şemanın eskimesi demekti. */
const RUHSAT_SEMA = {
  type: 'object',
  properties: Object.keys(set.alanlar || {}).reduce((o, a) => {
    /* ⚠️ 18.10: `['string','null']` BİRLEŞİMİ YOK. Strict şemada çok
       sayıda birleşim 400 döndürüyor (13.10, model-kos). Okunamayan alan
       GÖNDERİLMEZ ya da boş yazılır; ikisi de aşağıda null'a çevrilir. */
    /* Açıklama ÖLÇÜTTEN gelir (kutu kodu + Türkçe ad): 18.10 deneme
       koşusunda çıplak "silindir/guc/yakit" adlarıyla model en temiz
       ruhsatta bile bu üç alanı boş bıraktı. Kişisel notu modele gitmez. */
    o[a] = { type: 'string', description: String(set.alanlar[a]).split(' — kişisel')[0] }; return o;
  }, {}),
  /* HEPSİ ZORUNLU (18.10): isteğe bağlı bırakıldığında model en temiz
     ruhsatta bile silindir/güç/yakıtı, parça listesinde BÜTÜN fiyatları
     hiç göndermedi. Okunamayan alan boş metin ("") yazılır → null. */
  required: Object.keys(set.alanlar || {}), additionalProperties: false
};
const PARCA_SEMA = {
  type: 'object',
  properties: {
    arac: { type: 'string' },
    plaka: { type: 'string' },
    sase: { type: 'string' },
    satirlar: {
      type: 'array',
      items: {
        type: 'object',
        properties: {
          ad: { type: 'string', description: 'Parçanın ya da işin adı; ditto çözülmüş hâliyle' },
          adet: { type: 'string', description: 'Adet/miktar yazıyorsa; yoksa ""' },
          fiyat: { type: 'string', description: 'Fiyat RAKAMI (birim değil satır fiyatı); yoksa ""' },
          fiyatIkinci: { type: 'string', description: 'Satırda ikinci fiyat varsa (ör. YN/ORJ); yoksa ""' },
          durum: { type: 'string', description: 'Fiyat yerine yazılan durum: yok, gelmez, geliniyor...; yoksa ""' },
          tip: { type: 'string', enum: ['parca', 'iscilik'] },
          iptal: { type: 'boolean', description: 'Satırın üstü çiziliyse true' },
          okunamadi: { type: 'boolean' }
        },
        required: ['ad', 'adet', 'fiyat', 'fiyatIkinci', 'durum', 'tip', 'iptal', 'okunamadi'],
        additionalProperties: false
      }
    }
  },
  required: ['arac', 'plaka', 'sase', 'satirlar'], additionalProperties: false
};

/* Fiyat metin gelir ("1.500", "1500 TL") → sayı; boş → null. */
function fiyatSayi(v) {
  if (v == null || v === '') return null;
  const t = String(v).replace(/[^0-9.,]/g, '').replace(/\.(?=\d{3}(\D|$))/g, '').replace(',', '.');
  const n = parseFloat(t);
  return isNaN(n) ? null : n;
}

/* Gemini/Ollama şeması: JSON Schema'nın ortak alt kümesi. additionalProperties
   Gemini'nin responseSchema'sında yok — çıkarılır. */
function semaSade(o) {
  if (Array.isArray(o)) return o.map(semaSade);
  if (o && typeof o === 'object') {
    const y = {};
    Object.keys(o).forEach(k => { if (k !== 'additionalProperties') y[k] = semaSade(o[k]); });
    return y;
  }
  return o;
}
const bekle = ms => new Promise(r => setTimeout(r, ms));
class KaliciHata extends Error { constructor(st, m) { super(m); this.status = st; } }

(async () => {
  let client = null;
  if (SAGLAYICI === 'claude') {
    let Anthropic;
    try { Anthropic = require('@anthropic-ai/sdk'); }
    catch (e) { console.error('SDK yok. Önce: npm install @anthropic-ai/sdk'); process.exit(1); }
    if (!process.env.ANTHROPIC_API_KEY && !process.env.ANTHROPIC_AUTH_TOKEN) {
      console.error('ANTHROPIC_API_KEY yok. Anahtar bu depoda durmaz — ortamdan okunur.');
      process.exit(1);
    }
    const secenek = {};
    if (process.env.ANTHROPIC_WORKSPACE_ID) {
      secenek.defaultHeaders = { 'anthropic-workspace-id': process.env.ANTHROPIC_WORKSPACE_ID };
    }
    client = new (Anthropic.default || Anthropic)(secenek);
  } else if (SAGLAYICI === 'gemini' && !process.env.GEMINI_API_KEY) {
    console.error('GEMINI_API_KEY yok. Anahtar bu depoda durmaz — ortamdan okunur.');
    process.exit(1);
  }

  const ISTEM = TUR === 'ruhsat'
    ? 'Bu ruhsatın alanlarını çıkar. Okuyamadığın alanı boş metin ("") bırak; uydurma.'
    : 'Bu parça listesinin satırlarını çıkar. Ditto işaretlerini çöz, okuyamadığını işaretle. Boş alanlara "" yaz.';
  const SEMA = TUR === 'ruhsat' ? RUHSAT_SEMA : PARCA_SEMA;

  /* Tek okuma → { cevap, girdi, cikti }. Kalıcı hata KaliciHata atar. */
  async function oku(veri, tur) {
    if (SAGLAYICI === 'gemini') {
      const govde = {
        systemInstruction: { parts: [{ text: AYAR.sistem }] },
        contents: [{ role: 'user', parts: [{ inlineData: { mimeType: 'image/' + tur, data: veri } }, { text: ISTEM }] }],
        generationConfig: { responseMimeType: 'application/json', responseSchema: semaSade(SEMA), temperature: 0 }
      };
      for (let deneme = 0; ; deneme++) {
        const y = await fetch('https://generativelanguage.googleapis.com/v1beta/models/' + MODEL + ':generateContent',
          { method: 'POST', headers: { 'x-goog-api-key': process.env.GEMINI_API_KEY, 'content-type': 'application/json' }, body: JSON.stringify(govde) });
        const metin = await y.text();
        /* 429 dakikalık sınırsa bekle; ama "kredi bitti" 429'u da olabilir
           (16.10 ses koşusu) — metinde kredi/ödeme geçiyorsa KALICI. */
        const krediBitti = /credit|prepay|billing|quota exceeded for/i.test(metin);
        if ((y.status === 429 || y.status === 503) && !krediBitti && deneme < 6) { await bekle(5000 * (deneme + 1)); continue; }
        if (!y.ok) throw new KaliciHata(krediBitti ? 402 : y.status, y.status + ' — ' + metin.slice(0, 400));
        const d = JSON.parse(metin);
        const parca = (((d.candidates || [])[0] || {}).content || {}).parts || [];
        const yazi = parca.filter(p => !p.thought).map(p => p.text || '').join('').trim();
        if (!yazi) throw new Error('boş yanıt: ' + metin.slice(0, 200));
        const u = d.usageMetadata || {};
        return { cevap: JSON.parse(yazi), girdi: u.promptTokenCount || 0,
                 cikti: (u.candidatesTokenCount || 0) + (u.thoughtsTokenCount || 0) };
      }
    }
    if (SAGLAYICI === 'ollama') {
      /* ⚠️ fetch DEĞİL http (18.10): Node'un fetch'i yanıt başlığını 300 sn
         bekliyor; işlemcide çalışan yerel model bir ruhsata bundan uzun
         sürebiliyor ve "fetch failed" "Ollama çalışmıyor" gibi görünüyordu.
         Burada zaman aşımı YOK — yavaşlık ölçülen şeyin kendisi. */
      /* num_ctx 8192 (18.10): varsayılan bağlam çok büyük — bellek 22 GB'lık
         makinede tükendi, Windows sayfa dosyası C'yi doldurdu. Bir ruhsat
         (görüntü + istem + 18 alan) ~3.000 jetonda kalıyor. */
      /* think:false (18.10): Gemma 4 koşusunda 9 ruhsatın 3'ünde içerik BOŞ
         geldi ("Unexpected end of JSON input") — model çıktıyı düşünmeye
         harcadı. Alan okumak düşünme istemiyor. */
      const govde = JSON.stringify({ model: MODEL, stream: false, think: false, format: semaSade(SEMA), options: { temperature: 0, num_ctx: 8192 },
        messages: [{ role: 'system', content: AYAR.sistem }, { role: 'user', content: ISTEM, images: [veri] }] });
      const { durum, metin } = await new Promise((coz, red) => {
        const r = require('http').request({ host: '127.0.0.1', port: 11434, path: '/api/chat', method: 'POST',
          headers: { 'content-type': 'application/json', 'content-length': Buffer.byteLength(govde) } }, y => {
          let m = ''; y.setEncoding('utf8'); y.on('data', c => { m += c; }); y.on('end', () => coz({ durum: y.statusCode, metin: m }));
        });
        r.on('error', e => red(new KaliciHata(503, 'Ollama çalışmıyor (127.0.0.1:11434): ' + e.message)));
        r.end(govde);
      });
      if (durum !== 200) throw new KaliciHata(durum, durum + ' — ' + metin.slice(0, 400));
      const d = JSON.parse(metin);
      return { cevap: JSON.parse(((d.message || {}).content || '').trim()), girdi: d.prompt_eval_count || 0, cikti: d.eval_count || 0 };
    }
    return null; /* claude: aşağıdaki özgün yol */
  }

  /* Boş metin ve gönderilmeyen alan = okunamadı (null). Ölçüt boşu null
     yazıyor; '' ile null ayrı sayılsaydı doğru davranış yanlış görünürdü. */
  function bosuNullYap(o) {
    if (Array.isArray(o)) return o.map(bosuNullYap);
    if (o && typeof o === 'object') {
      const y = {};
      /* "null" YAZISI da boştur: 18.10 deneme koşusunda model boş alana
         'null' metni yazdı ve dört alan "uydurma" sayıldı — biçim hatası,
         içerik değil. */
      Object.keys(o).forEach(k => { const v = bosuNullYap(o[k]); y[k] = (v === '' || (typeof v === 'string' && v.trim().toLowerCase() === 'null') ? null : v); });
      return y;
    }
    return o;
  }

  function sayilastir(c) {
    (c.satirlar || []).forEach(r => { r.fiyat = fiyatSayi(r.fiyat); r.fiyatIkinci = fiyatSayi(r.fiyatIkinci); });
    return c;
  }

  const cevaplar = {};
  let durdu = false, sure = 0;
  let girdi = 0, cikti = 0, onbellekOkunan = 0, onbellegeYazilan = 0, atlanan = 0;

  for (const k of kayitlar) {
    /* Fotoğraf yoksa ATLANIR ve bu SÖYLENİR — ölçemediğini ölçmüş
       gibi raporlayan bir araç, ölçüm aleti değildir (K-024.1). */
    /* Öncelik: --foto-kok (küçültülmüş) → depo klasörü → harita. */
    const dosya = (FOTO_KOK ? [path.join(FOTO_KOK, k.id + '.jpg')] : [])
      .concat(['jpg', 'jpeg', 'png', 'webp'].map(u => path.join(KOK, AYAR.klasor, k.id + '.' + u)))
      .concat(HARITA[k.id] ? [HARITA[k.id]] : [])
      .filter(y => fs.existsSync(y))[0];
    if (!dosya) { cevaplar[k.id] = { hata: 'fotoğraf yok: ' + k.id }; atlanan++; continue; }

    const uzanti = path.extname(dosya).slice(1).toLowerCase();
    const tur = uzanti === 'jpg' ? 'jpeg' : uzanti;
    const veri = fs.readFileSync(dosya).toString('base64');

    try {
      if (SAGLAYICI !== 'claude') {
        const t0 = Date.now();
        const r = await oku(veri, tur);
        girdi += r.girdi; cikti += r.cikti; sure += Date.now() - t0;
        cevaplar[k.id] = sayilastir(bosuNullYap(r.cevap));
        process.stdout.write('.');
        continue;
      }
      const istek = {
        model: MODEL,
        /* Yeni modellerde düşünme kapatılamıyor; düşük sınır cevabı keser. */
        max_tokens: 16000,
        system: [{ type: 'text', text: AYAR.sistem, cache_control: { type: 'ephemeral' } }],
        tools: [{
          name: TUR === 'ruhsat' ? 'ruhsat_alanlari' : 'parca_satirlari',
          description: TUR === 'ruhsat'
            ? 'Ruhsattan okunan alanlar' : 'Parça listesinden okunan satırlar',
          strict: true,
          input_schema: TUR === 'ruhsat' ? RUHSAT_SEMA : PARCA_SEMA
        }],
        /* ⚠️ `tool_choice: {type:'tool'}` Opus 5.5 / Sonnet 5.5'te 400
           (model-kos, 13.10). Araç istemle isteniyor; gelmezse cevap
           HATA sayılır — kayırma yok. */
        messages: [{
          role: 'user',
          content: [
            { type: 'image', source: { type: 'base64', media_type: 'image/' + tur, data: veri } },
            { type: 'text', text: TUR === 'ruhsat'
                ? 'Bu ruhsatın alanlarını çıkar ve YALNIZ ruhsat_alanlari aracını çağırarak cevap ver. Okuyamadığın alanı gönderme.'
                : 'Bu parça listesinin satırlarını çıkar ve YALNIZ parca_satirlari aracını çağırarak cevap ver. Ditto işaretlerini çöz, okuyamadığını işaretle.' }
          ]
        }]
      };
      if (EFOR) istek.output_config = { effort: EFOR };
      const yanit = await client.messages.create(istek);
      girdi += yanit.usage.input_tokens || 0;
      cikti += yanit.usage.output_tokens || 0;
      onbellekOkunan += yanit.usage.cache_read_input_tokens || 0;
      onbellegeYazilan += yanit.usage.cache_creation_input_tokens || 0;
      const blok = yanit.content.find(b => b.type === 'tool_use');
      cevaplar[k.id] = yanit.stop_reason === 'refusal' ? { hata: 'model reddetti' }
        : blok ? sayilastir(bosuNullYap(blok.input)) : { hata: 'araç çağrısı gelmedi (' + yanit.stop_reason + ')' };
    } catch (e) {
      cevaplar[k.id] = { hata: e.message };
      /* 400/401/402/403/404 kalıcıdır — ilk hatada dur (model-kos, 13.10).
         402 = Gemini kredisi bitti; 503 burada yalnız Ollama çalışmıyorsa. */
      if ([400, 401, 402, 403, 404].indexOf(e.status) >= 0 || (e instanceof KaliciHata)) {
        console.error('\nKOŞU DURDU — kalıcı API hatası (' + e.status + '): ' + e.message);
        process.exitCode = 2;
        /* 18.10: kredi bittiğinde 38 okuma YAZILMADAN kayboldu (~$0,7).
           Durunca o ana kadarki cevaplar puanlanıp YARIM diye kaydedilir —
           ödenmiş okuma çöpe gitmez. */
        delete cevaplar[k.id];
        durdu = true;
        break;
      }
    }
    process.stdout.write('.');
  }
  process.stdout.write('\n');

  const olcSet = Object.assign({}, set);
  olcSet[AYAR.kayitAlani] = durdu ? kayitlar.filter(k => cevaplar[k.id]) : kayitlar;
  /* Parça listesi KENDİ cetveliyle (18.10) — alanPuanla listeyi ölçemiyordu. */
  const sonuc = TUR === 'parca' ? listePuanla(olcSet, cevaplar) : alanPuanla(olcSet, cevaplar);
  sonuc.motor = MODEL + (EFOR ? ' efor=' + EFOR : '') + (FOTO_KOK ? ' foto=' + path.basename(FOTO_KOK) : '');
  sonuc.tur = TUR;
  sonuc.jeton = { girdi, cikti, onbellek: onbellekOkunan, onbellegeYazilan };
  sonuc.atlanan = atlanan;
  sonuc.saglayici = SAGLAYICI;
  if (sure) sonuc.belgeBasinaSaniye = +(sure / 1000 / Math.max(1, kayitlar.length - atlanan)).toFixed(1);
  const f = FIYAT[MODEL];
  if (f) {
    const okuma = f[2] != null ? f[2] : f[0] / 10;
    sonuc.maliyetUsd = +((girdi * f[0] + cikti * f[1] + onbellekOkunan * okuma +
                          onbellegeYazilan * f[0] * 1.25) / 1e6).toFixed(4);
    sonuc.belgeBasinaUsd = +(sonuc.maliyetUsd / Math.max(1, kayitlar.length - atlanan)).toFixed(5);
  }
  /* ŞASİ AYRICA (K-066, Cemil 18.10): şasi okunamazsa iş DURUYOR. O
     yüzden şasinin üç hâli ayrı sayılır: doğru · boş bıraktı (usta
     yeniden çeker) · YANLIŞ okudu (en pahalısı: parçacı yanlış parça
     gönderir). */
  if (TUR === 'ruhsat') {
    /* O→0, I→1, Q→0: şase standardında (ISO 3779) bu üç harf YOKTUR;
       "WOLO…" ile "W0L0…" AYNI şasedir (18.10 Haiku koşusu: 3 "yanlışın"
       2'si yalnız buydu). Uzunluk farkı ise gerçek hatadır. */
    const norm = v => (v == null ? '' : String(v).toUpperCase().replace(/[^A-Z0-9]/g, '')
      .replace(/O/g, '0').replace(/I/g, '1').replace(/Q/g, '0'));
    const sase = { dogru: 0, bos: 0, yanlis: 0, yanlisListe: [] };
    kayitlar.forEach(k => {
      const c = cevaplar[k.id]; if (!c || c.hata) return;
      const b = norm(k.beklenen.sase), o = norm(c.sase);
      if (!o) sase.bos++;
      else if (o === b) sase.dogru++;
      else { sase.yanlis++; sase.yanlisListe.push(k.id + ': ' + c.sase + ' (doğrusu ' + k.beklenen.sase + ')'); }
    });
    sonuc.sase = sase;
  }

  const dosya = path.join(KOK, 'sonuc-' + TUR + '-' + MODEL.replace(/[^A-Za-z0-9.-]/g, '-') + (ETIKET ? '-' + ETIKET : '') + (durdu ? '-YARIM' : '') + '.json');
  fs.writeFileSync(dosya, JSON.stringify({ sonuc, cevaplar }, null, 2), 'utf8');

  console.log('\nMOTOR: ' + MODEL + ' · TÜR: ' + TUR);
  if (atlanan) console.log('⚠️ ' + atlanan + ' fotoğraf bulunamadı — ölçüm EKSİK.');
  if (TUR === 'ruhsat' && !kisiselVar) {
    console.log('⚠️ ' + path.basename(KISISEL_YOL) + ' yok — TC/vergi no, ad ve ' +
                'adres ÖLÇÜLMEDİ. Fatura alanları bu koşuda sınanmadı.');
  }
  console.log(sonuc.ozet);
  console.log('jeton: ' + girdi + ' girdi · ' + cikti + ' çıktı · ' + onbellekOkunan + ' önbellekten' +
              (sonuc.maliyetUsd != null ? ' · ~$' + sonuc.maliyetUsd + ' (belge başına ~$' + sonuc.belgeBasinaUsd + ')' : ''));
  if (sonuc.belgeBasinaSaniye) console.log('süre: belge başına ~' + sonuc.belgeBasinaSaniye + ' sn');
  if (sonuc.sase) console.log('ŞASİ: ' + sonuc.sase.dogru + ' doğru · ' + sonuc.sase.bos + ' boş · ' +
              sonuc.sase.yanlis + ' YANLIŞ' + (sonuc.sase.yanlisListe.length ? '\n    ' + sonuc.sase.yanlisListe.join('\n    ') : ''));
  sonuc.satirlar.filter(s => s.sonuc !== 'TAM').forEach(s => {
    console.log('  ' + s.id + ' [' + s.sonuc + '] ' + s.alan +
      (s.zorluk ? ' {' + s.zorluk + '}' : '') +
      (s.ihlal ? ' YASAK:' + s.ihlal : '') +
      (s.kritikYanlis ? ' KRİTİK:' + s.kritikYanlis : '') +
      (s.uydurma ? ' UYDURMA:' + s.uydurma : '') +
      (s.yanlis ? ' | ' + s.yanlis : ''));
  });
  console.log('\nayrıntı: ' + dosya);
})();
