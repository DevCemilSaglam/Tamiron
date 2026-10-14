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
     node tools/degerlendirme/model-kos.js --model claude-sonnet-5-5
     node tools/degerlendirme/model-kos.js --model claude-haiku-5-5

   SES + MODEL (13.10): Web Speech'in DUYDUĞU metni modele vermek için
   ses-dosya-kos.html'in indirdiği sonuç dosyası verilir:
     node tools/degerlendirme/model-kos.js --duyulan C:/.../ses-olcum-temiz-kayitlar-tel.json
   Cümle yerine `duyulan` gider; tanınamayan kayıt (hata / boş) puana
   GİRMEZ — ses-dosya-kos.html'deki kuralın aynısı, yoksa iki karne
   farklı paydalarla karşılaştırılmış olur.

   MALİYET: 40 cümle kaba hesap (düşünme jetonları dahil, koşu sonunda
   gerçek rakam usage'dan basılır) —
     claude-opus-5-5   ~$0,25
     claude-sonnet-5-5 ~$0,12
     claude-haiku-5-5  ~$0,01
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
const MODEL = arg('--model', 'claude-opus-5-5');
/* Düşünme derinliği. Boş = modelin kendi varsayılanı. */
const EFOR = arg('--efor', null);

/* $ / 1M jeton — koşu sonunda gerçek maliyeti basmak için. */
const FIYAT = {
  'claude-opus-5-5': [4, 20], 'claude-opus-5': [5, 25],
  'claude-sonnet-5-5': [2, 10], 'claude-sonnet-5': [2, 10],
  'claude-haiku-5-5': [0.10, 0.50], 'claude-haiku-4-5': [1, 5]
};

/* --duyulan: Web Speech'in duyduğu metin cümlenin yerine geçer. */
const DUYULAN_DOSYA = arg('--duyulan', null);
let calisanSet = set;
if (DUYULAN_DOSYA) {
  const kos = JSON.parse(fs.readFileSync(DUYULAN_DOSYA, 'utf8'));
  const duyulan = {};
  (kos.sonuc || []).forEach(r => {
    if (!r.hata && r.duyulan && r.duyulan.trim()) duyulan[r.id] = r.duyulan.trim();
  });
  calisanSet = Object.assign({}, set, {
    cumleler: set.cumleler.filter(c => duyulan[c.id] !== undefined)
      .map(c => Object.assign({}, c, { yazilan: c.cumle, cumle: duyulan[c.id] }))
  });
  console.log('duyulan: ' + calisanSet.cumleler.length + '/' + set.cumleler.length +
              ' kayıt tanınmış (' + path.basename(DUYULAN_DOSYA) + ')');
}

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
   farkıyla kayıtlı bir plakaya benziyorsa oneri alanına onu yaz.
6. Cümle konuşma tanımadan geliyor olabilir: harfler küçük, sayılar
   yazıyla ya da eksik, kelimeler sesçe yanlış duyulmuş olabilir
   ("kelime" ↔ KLM gibi). Anlamı ve dükkândaki araçları kullanarak
   ustanın NE DEDİĞİNİ çıkar; yine de uydurma.

Her cümle için cevabını YALNIZCA islem aracını bir kez çağırarak ver.`;

/* Boş bırakılabilen seçim alanı. ⚠️ `type: ['string','null']` + enum
   strict şemada 400 veriyor ("Enum value ... does not match declared
   type"); anyOf ile yazılır. */
const secim = degerler => ({ anyOf: [{ type: 'string', enum: degerler }, { type: 'null' }] });

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
      /* ⚠️ AÇIKLAMALAR PROGRAMIN KENDİ ŞEMALARINDAN (Islem.tanim alanları,
         ekranlar/*.html) — değerlendirme setinin cevaplarından DEĞİL.
         14.10'da açıklamasız şemayla model işlemi doğru seçip alanı boş
         bıraktı (%64, SES-OLCUM §11.2). Setten yazılsaydı model sınava
         çalıştırılmış olurdu. */
      plaka: { type: ['string', 'null'], description: 'Aracın plakası, büyük harf, boşluklu: "34 ABC 123".' },
      oneri: { type: ['string', 'null'], description: 'yeni-arac: söylenen plaka kayıtlı değil ama bir kayıtlı plakaya çok benziyorsa o kayıtlı plaka.' },
      isTuru: Object.assign(secim(['tamirat', 'bakim', 'ikisi']), { description: 'servis-ac / yeni-arac: işin türü.' }),
      km: { type: ['string', 'null'], description: 'servis-ac / yeni-arac: kilometre, binlik noktalı: "125.000".' },
      tel: { type: ['string', 'null'], description: 'yeni-arac: sahibin telefonu, "0532 111 22 33" biçiminde.' },
      odeme: Object.assign(secim(['nakit', 'kart', 'havale']), { description: 'teslim: ödeme yolu; söylenmediyse null.' }),
      tip: Object.assign(secim(['parca', 'iscilik']), { description: 'satir-ekle: eklenen satır parça mı işçilik mi.' }),
      ad: { type: ['string', 'null'], description: 'satir-ekle / fiyat-guncelle: satırın adı (parça ya da işçilik adı).' },
      fiyat: { type: ['number', 'null'], description: 'satir-ekle / fiyat-guncelle: satırın TL tutarı, sayı olarak.' },
      tutar: { type: ['number', 'null'], description: 'gider-ekle: giderin TL tutarı, sayı olarak.' },
      donem: Object.assign(secim(['bugun', 'ay', 'yil']), { description: 'tahsilat-sor: hangi dönemin tahsilatı.' }),
      sablon: Object.assign(secim(['ozet', 'bekliyor', 'hazir', 'teslim', 'hatirlatma', 'selam', 'bayram', 'tesekkur', 'serbest']),
                            { description: 'mesaj-gonder: tek araç sahibine gidecek mesajın hazır metni.' }),
      kanal: Object.assign(secim(['whatsapp', 'sms']), { description: 'mesaj-gonder / toplu-mesaj: gönderim kanalı; söylenmediyse null.' }),
      metinTuru: Object.assign(secim(['ozet', 'bekliyor', 'hazir', 'teslim', 'hatirlatma', 'selam', 'bayram', 'tesekkur', 'serbest']),
                               { description: 'toplu-mesaj: herkese gidecek mesajın hazır metni.' }),
      hedef: Object.assign(secim(['araclar', 'kayitlar', 'musteriler']),
                           { description: 'ara: nerede aranacak — araçlar, servis kayıtları, müşteriler.' }),
      terim: { type: ['string', 'null'], description: 'ara: aranan şey (plaka, marka, parça, kişi adı).' },
      suzgec: Object.assign(secim(['serviste']), { description: 'ara: yalnız şu an serviste olanlar isteniyorsa "serviste".' })
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
  /* Anahtar bir workspace'e bağlı değilse API her isteği 400 ile
     reddediyor ("must include the anthropic-workspace-id header").
     Kimlik ortamdan okunur — anahtar gibi depoda durmaz. */
  const secenek = {};
  if (process.env.ANTHROPIC_WORKSPACE_ID) {
    secenek.defaultHeaders = { 'anthropic-workspace-id': process.env.ANTHROPIC_WORKSPACE_ID };
  }
  const client = new (Anthropic.default || Anthropic)(secenek);

  const cevaplar = {};
  let girdiJeton = 0, ciktiJeton = 0, onbellekOkunan = 0;

  for (const c of calisanSet.cumleler) {
    try {
      const istek = {
        model: MODEL,
        /* Yeni modellerde düşünme kapatılamıyor; 2000 jetonda düşünme
           cevabı kesebilirdi. */
        max_tokens: 16000,
        /* Sistem istemi HER ÇAĞRIDA AYNI — önbelleğe alınıyor, girdi
           maliyetinin büyük kısmı böyle düşüyor (YAPAY-ZEKA.md §5). */
        system: [{ type: 'text', text: SISTEM, cache_control: { type: 'ephemeral' } }],
        tools: [ARAC],
        /* ⚠️ `tool_choice: {type:'tool'}` Opus 5.5 / Sonnet 5.5'te 400
           döndürüyor (zorunlu araç kaldırıldı). Araç istemle isteniyor,
           biçim `strict: true` ile korunuyor; araç çağrısı gelmezse cevap
           YANLIŞ sayılır — kayırma yok. */
        messages: [{
          role: 'user',
          content: `Bağlam (hangi ekrandan söylendi): ${c.baglam}\nCümle: "${c.cumle}"`
        }]
      };
      if (EFOR) istek.output_config = { effort: EFOR };
      /* Reddedilirse başka modele DÜŞÜLMEZ: ölçülen model değişirse
         rakam o modelin olmaz. Ret cevabı hata olarak yazılır. */
      const yanit = await client.messages.create(istek);
      girdiJeton += yanit.usage.input_tokens || 0;
      ciktiJeton += yanit.usage.output_tokens || 0;
      onbellekOkunan += yanit.usage.cache_read_input_tokens || 0;
      const blok = yanit.content.find(b => b.type === 'tool_use');
      cevaplar[c.id] = yanit.stop_reason === 'refusal' ? { hata: 'model reddetti' }
        : blok ? blok.input : { hata: 'araç çağrısı gelmedi (' + yanit.stop_reason + ')' };
    } catch (e) {
      cevaplar[c.id] = { hata: e.message };
      /* 400/401/403/404 kalıcıdır: 40 isteği boşuna atmak yerine ilk
         hatada dur (13.10: workspace'siz anahtarla 40 istek 400 yedi). */
      if ([400, 401, 403, 404].indexOf(e.status) >= 0) {
        console.error('\nKOŞU DURDU — kalıcı API hatası (' + e.status + '): ' + e.message);
        process.exitCode = 2;
        return;
      }
    }
    process.stdout.write('.');
  }
  process.stdout.write('\n');

  const sonuc = puanla(calisanSet, cevaplar);
  sonuc.motor = MODEL + (DUYULAN_DOSYA ? ' + Web Speech' : '');
  sonuc.jeton = { girdi: girdiJeton, cikti: ciktiJeton, onbellek: onbellekOkunan };
  const f = FIYAT[MODEL];
  if (f) sonuc.maliyetUsd = +((girdiJeton * f[0] + ciktiJeton * f[1]) / 1e6).toFixed(4);

  const ek = DUYULAN_DOSYA ? '-' + path.basename(DUYULAN_DOSYA, '.json') : '';
  const dosya = path.join(KOK, 'sonuc-' + MODEL + ek + '.json');
  fs.writeFileSync(dosya, JSON.stringify({ sonuc, cevaplar }, null, 2), 'utf8');

  console.log('\nMOTOR: ' + sonuc.motor);
  console.log(sonuc.ozet);
  console.log('jeton: ' + girdiJeton + ' girdi · ' + ciktiJeton + ' çıktı · ' +
              onbellekOkunan + ' önbellekten' +
              (sonuc.maliyetUsd != null ? ' · ~$' + sonuc.maliyetUsd : ''));
  sonuc.satirlar.filter(s => s.puan !== 'TAM').forEach(s => {
    const c = calisanSet.cumleler.find(x => x.id === s.id);
    console.log('  ' + s.id + ' [' + s.puan + '] "' + s.cumle + '"' +
                (c && c.yazilan ? ' (söylenen: "' + c.yazilan + '")' : '') + ' → bekl:' +
                s.beklenen + ' gelen:' + s.gelen + (s.alanlar ? ' (' + s.alanlar + ')' : ''));
  });
  console.log('\nayrıntı: ' + dosya);
})();
