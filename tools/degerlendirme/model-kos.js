/* ============ KOŞUCU 2 — MODEL (Node) ============
   Cemil, 14.09: *"Yapay zeka testini yapamayacak mıyız?"*

   Bu dosya testin İKİNCİ yarısı: aynı değerlendirme setini Claude'a
   verir, aynı cetvelle (puanla.js) puanlar ve bugünkü motorun karnesiyle
   karşılaştırılabilir bir rakam üretir.

   ⚠️ ÇALIŞMASI İÇİN API ANAHTARI GEREKİYOR ve anahtar BU DEPODA
   DURMAZ — ortam değişkeninden okunur:

     npm install @anthropic-ai/sdk         (bir kez)
     set ANTHROPIC_API_KEY=sk-ant-...      (Windows: setx ya da oturumluk)
     node tools/degerlendirme/model-kos.js
     node tools/degerlendirme/model-kos.js --model claude-sonnet-5
     node tools/degerlendirme/model-kos.js --model claude-haiku-4-5

   MALİYET: 40 cümle, istem önbelleğiyle birlikte kaba hesap —
     claude-opus-5    ~$0,25
     claude-sonnet-5  ~$0,10
     claude-haiku-4-5 ~$0,05
   Her koşu GERÇEK PARA harcar; Cemil onaylamadan koşturulmaz.

   ⚠️ MODEL SERBEST METİN DÖNDÜRMEZ: şema `strict: true` bir araçla
   dayatılıyor, yani gelen nesne ekranın beklediği biçimde. Bu, işin
   can alıcı yeri — modelin yerine geçtiği şey EKRAN değil, bir
   FONKSİYON (YAPAY-ZEKA.md §6). */

const fs = require('fs');
const path = require('path');
const { puanla } = require('./puanla.js');

const KOK = __dirname;
const set = JSON.parse(fs.readFileSync(path.join(KOK, 'ses-seti.json'), 'utf8'));

const arg = (ad, varsayilan) => {
  const i = process.argv.indexOf(ad);
  return i > 0 && process.argv[i + 1] ? process.argv[i + 1] : varsayilan;
};
const MODEL = arg('--model', 'claude-opus-5');

/* ---- DÜKKÂNIN HÂLİ ----
   Model, bu dükkânın verisini bilmeden "şu Mégane" diyemez. Prototipin
   tohum verisi buraya ÖZET olarak veriliyor; gerçek kullanımda backend
   aynı özeti veri tabanından üretecek.

   ⚠️ TAM VERİ DEĞİL, KARAR İÇİN GEREKENİ: plaka, marka/model, sahip ve
   açık kaydın satır adları. Şasi numarası ya da adres göndermek, işe
   yaramayan veriyi dışarı çıkarmak olurdu (RUHSAT-OKUMA Bulgu 2). */
const veriYolu = path.join(KOK, '..', '..', 'prototypes', 'kaynak', 'betik', '02-veri.js');
const veriMetni = fs.readFileSync(veriYolu, 'utf8');

function dukkanOzeti() {
  /* Tohum veriyi çalıştırmadan okumak yerine, prototipin kendi
     dosyasından ARAÇLAR ve KAYITLAR'ı değerlendiriyoruz. Ayrı bir
     kopya tutmak, iki ayrı dünya demek olurdu. */
  const kapsam = {};
  const govde = veriMetni
    .replace(/^\s*\/\*[\s\S]*?\*\//gm, '')
    .match(/var ARACLAR = \[[\s\S]*?\n\];|var KAYITLAR = \[[\s\S]*?\n\];/g) || [];
  // eslint-disable-next-line no-new-func
  new Function('k', govde.join('\n') + '\nk.ARACLAR=ARACLAR;k.KAYITLAR=KAYITLAR;')(kapsam);

  const araclar = kapsam.ARACLAR.map(a =>
    `${a.plaka} · ${a.marka} ${a.model} · sahibi: ${a.sahipAd || '—'}`).join('\n');
  const acik = kapsam.KAYITLAR.filter(k => !k.teslimTarihi);
  const kayitlar = acik.map(k =>
    `${k.plaka} · ${k.sikayet} · satırlar: ${(k.satirlar || []).map(s => s.ad).join(', ')}`).join('\n');
  /* Değerlendirmede "açık kayıt" bağlamı, tarayıcı koşucusuyla AYNI
     kaydı göstermeli: satırı birden çok olan ilk açık kayıt. */
  const baglamKayit = acik.filter(k => (k.satirlar || []).length > 1)[0] || acik[0];
  return { araclar, kayitlar, baglamKayit };
}

const D = dukkanOzeti();

const SISTEM = `Sen bir tamirhane programının komut çözümleyicisisin.
Ustanın söylediği Türkçe cümleyi, programın anladığı BİR işlem nesnesine
çevirirsin. Uydurmazsın: emin olmadığın alanı boş bırakırsın, cümleden
bir işlem çıkmıyorsa "plakasiz" dersin.

DÜKKÂNDAKİ ARAÇLAR:
${D.araclar}

ŞU AN SERVİSTE OLAN İŞLER:
${D.kayitlar}

AÇIK KAYIT BAĞLAMI (baglam "kayit-icinde" ise bu kayıt açıktır):
${D.baglamKayit ? D.baglamKayit.plaka + ' · satırlar: ' +
  (D.baglamKayit.satirlar || []).map(s => s.ad + ' (' + (s.fiyat || 0) + ' TL)').join(', ') : '—'}

KURALLAR:
1. Plaka söylenmediyse ama araç tarif edildiyse (marka, sahip adı),
   listeden TEK bir araca uyuyorsa o aracın plakasını yaz; birden fazla
   araca uyuyorsa plakayı BOŞ bırak.
2. "kayit-icinde" bağlamında söylenen parça adı açık kayıtta ZATEN VARSA
   "fiyat-guncelle", yoksa "satir-ekle".
3. Ödeme yolu söylenmediyse odeme alanını boş bırak — nakit varsayma.
4. Cümlede iş yoksa (havadan sudan), tur = "plakasiz".
5. Plaka kayıtlı değilse tur = "yeni-arac"; harf grubu bir karakter
   farkıyla kayıtlı bir plakaya benziyorsa oneri alanına onu yaz.`;

const ARAC = {
  name: 'islem',
  description: 'Ustanın cümlesinden çıkan tek işlem.',
  strict: true,
  input_schema: {
    type: 'object',
    properties: {
      tur: {
        type: 'string',
        enum: ['servis-ac', 'yeni-arac', 'teslim', 'satir-ekle', 'fiyat-guncelle',
               'gider-ekle', 'tahsilat-sor', 'mesaj-gonder', 'toplu-mesaj', 'ara',
               'plakasiz']
      },
      plaka: { type: ['string', 'null'] },
      oneri: { type: ['string', 'null'], description: 'benzer kayıtlı plaka' },
      isTuru: { type: ['string', 'null'], enum: ['tamirat', 'bakim', 'ikisi', null] },
      km: { type: ['string', 'null'] },
      tel: { type: ['string', 'null'] },
      odeme: { type: ['string', 'null'], enum: ['nakit', 'kart', 'havale', null] },
      tip: { type: ['string', 'null'], enum: ['parca', 'iscilik', null] },
      fiyat: { type: ['number', 'null'] },
      tutar: { type: ['number', 'null'] },
      donem: { type: ['string', 'null'], enum: ['bugun', 'ay', 'yil', null] },
      sablon: { type: ['string', 'null'],
                enum: ['ozet', 'bekliyor', 'hazir', 'teslim', 'hatirlatma',
                       'selam', 'bayram', 'tesekkur', 'serbest', null] },
      kanal: { type: ['string', 'null'], enum: ['whatsapp', 'sms', null] },
      metinTuru: { type: ['string', 'null'] },
      hedef: { type: ['string', 'null'], enum: ['araclar', 'kayitlar', 'musteriler', null] },
      terim: { type: ['string', 'null'] },
      suzgec: { type: ['string', 'null'] }
    },
    required: ['tur'],
    additionalProperties: false
  }
};

(async () => {
  let Anthropic;
  try { Anthropic = require('@anthropic-ai/sdk'); }
  catch (e) {
    console.error('SDK yok. Önce: npm install @anthropic-ai/sdk');
    process.exit(1);
  }
  if (!process.env.ANTHROPIC_API_KEY && !process.env.ANTHROPIC_AUTH_TOKEN) {
    console.error('ANTHROPIC_API_KEY yok. Anahtar bu depoda durmaz — ortamdan okunur.');
    process.exit(1);
  }
  const client = new (Anthropic.default || Anthropic)();

  const cevaplar = {};
  let girdiJeton = 0, ciktiJeton = 0;

  for (const c of set.cumleler) {
    try {
      const yanit = await client.messages.create({
        model: MODEL,
        max_tokens: 2000,
        /* Sistem istemi HER ÇAĞRIDA AYNI — önbelleğe alınıyor, girdi
           maliyetinin büyük kısmı böyle düşüyor (YAPAY-ZEKA.md §5). */
        system: [{ type: 'text', text: SISTEM, cache_control: { type: 'ephemeral' } }],
        tools: [ARAC],
        tool_choice: { type: 'tool', name: 'islem' },
        messages: [{
          role: 'user',
          content: `Bağlam (hangi ekrandan söylendi): ${c.baglam}\nCümle: "${c.cumle}"`
        }]
      });
      girdiJeton += yanit.usage.input_tokens || 0;
      ciktiJeton += yanit.usage.output_tokens || 0;
      const blok = yanit.content.find(b => b.type === 'tool_use');
      cevaplar[c.id] = blok ? blok.input : { hata: 'araç çağrısı gelmedi' };
    } catch (e) {
      cevaplar[c.id] = { hata: e.message };
    }
    process.stdout.write('.');
  }
  process.stdout.write('\n');

  const sonuc = puanla(set, cevaplar);
  sonuc.motor = MODEL;
  sonuc.jeton = { girdi: girdiJeton, cikti: ciktiJeton };

  const dosya = path.join(KOK, 'sonuc-' + MODEL + '.json');
  fs.writeFileSync(dosya, JSON.stringify({ sonuc, cevaplar }, null, 2), 'utf8');

  console.log('\nMOTOR: ' + MODEL);
  console.log(sonuc.ozet);
  console.log('jeton: ' + girdiJeton + ' girdi · ' + ciktiJeton + ' çıktı');
  sonuc.satirlar.filter(s => s.puan !== 'TAM').forEach(s => {
    console.log('  ' + s.id + ' [' + s.puan + '] "' + s.cumle + '" → bekl:' +
                s.beklenen + ' gelen:' + s.gelen + (s.alanlar ? ' (' + s.alanlar + ')' : ''));
  });
  console.log('\nayrıntı: ' + dosya);
})();
