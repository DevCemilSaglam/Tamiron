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
const { alanPuanla } = require('./puanla.js');

const KOK = __dirname;
const arg = (ad, varsayilan) => {
  const i = process.argv.indexOf(ad);
  return i > 0 && process.argv[i + 1] ? process.argv[i + 1] : varsayilan;
};

const TUR = arg('--tur', 'ruhsat');
const MODEL = arg('--model', 'claude-opus-5');
const SADECE = arg('--sadece', '');

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
4. TC KİMLİK NO, VERGİ NO, ADRES ve SAHİP ADINI OKUMA, DÖNDÜRME.
   Bu bilgiler bize gerekmiyor; tutmadığımız veri en güvenli veridir.
5. Plakayı boşluklu yaz: "33VAH34" değil "33 VAH 34".
6. Belge yan ya da ters çekilmiş olabilir; döndürüp oku.`
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
const kayitlar = set[AYAR.kayitAlani]
  .filter(k => !SADECE || SADECE.split(',').indexOf(k.id) >= 0);

/* Şema: ölçütteki alanlardan türetiliyor — ikinci bir yerde şema
   tutmak, ölçüt değişince şemanın eskimesi demekti. */
const RUHSAT_SEMA = {
  type: 'object',
  properties: Object.keys(set.alanlar || {}).reduce((o, a) => {
    o[a] = { type: ['string', 'null'] }; return o;
  }, {}),
  required: [], additionalProperties: false
};
const PARCA_SEMA = {
  type: 'object',
  properties: {
    arac: { type: ['string', 'null'] },
    plaka: { type: ['string', 'null'] },
    sase: { type: ['string', 'null'] },
    satirlar: {
      type: 'array',
      items: {
        type: 'object',
        properties: {
          ad: { type: ['string', 'null'] },
          adet: { type: ['string', 'number', 'null'] },
          fiyat: { type: ['number', 'null'] },
          fiyatIkinci: { type: ['number', 'null'] },
          durum: { type: ['string', 'null'] },
          tip: { type: ['string', 'null'], enum: ['parca', 'iscilik', null] },
          iptal: { type: ['boolean', 'null'] },
          okunamadi: { type: ['boolean', 'null'] }
        },
        required: ['ad'], additionalProperties: false
      }
    }
  },
  required: ['satirlar'], additionalProperties: false
};

(async () => {
  let Anthropic;
  try { Anthropic = require('@anthropic-ai/sdk'); }
  catch (e) { console.error('SDK yok. Önce: npm install @anthropic-ai/sdk'); process.exit(1); }
  if (!process.env.ANTHROPIC_API_KEY && !process.env.ANTHROPIC_AUTH_TOKEN) {
    console.error('ANTHROPIC_API_KEY yok. Anahtar bu depoda durmaz — ortamdan okunur.');
    process.exit(1);
  }
  const client = new (Anthropic.default || Anthropic)();

  const cevaplar = {};
  let girdi = 0, cikti = 0, atlanan = 0;

  for (const k of kayitlar) {
    /* Fotoğraf yoksa ATLANIR ve bu SÖYLENİR — ölçemediğini ölçmüş
       gibi raporlayan bir araç, ölçüm aleti değildir (K-024.1). */
    const dosya = ['jpg', 'jpeg', 'png', 'webp']
      .map(u => path.join(KOK, AYAR.klasor, k.id + '.' + u))
      .filter(y => fs.existsSync(y))[0];
    if (!dosya) { cevaplar[k.id] = { hata: 'fotoğraf yok: ' + k.id }; atlanan++; continue; }

    const uzanti = path.extname(dosya).slice(1).toLowerCase();
    const tur = uzanti === 'jpg' ? 'jpeg' : uzanti;
    const veri = fs.readFileSync(dosya).toString('base64');

    try {
      const yanit = await client.messages.create({
        model: MODEL,
        max_tokens: 4000,
        system: [{ type: 'text', text: AYAR.sistem, cache_control: { type: 'ephemeral' } }],
        tools: [{
          name: TUR === 'ruhsat' ? 'ruhsat_alanlari' : 'parca_satirlari',
          description: TUR === 'ruhsat'
            ? 'Ruhsattan okunan alanlar' : 'Parça listesinden okunan satırlar',
          strict: true,
          input_schema: TUR === 'ruhsat' ? RUHSAT_SEMA : PARCA_SEMA
        }],
        tool_choice: { type: 'tool', name: TUR === 'ruhsat' ? 'ruhsat_alanlari' : 'parca_satirlari' },
        messages: [{
          role: 'user',
          content: [
            { type: 'image', source: { type: 'base64', media_type: 'image/' + tur, data: veri } },
            { type: 'text', text: TUR === 'ruhsat'
                ? 'Bu ruhsatın alanlarını çıkar. Okuyamadığını boş bırak.'
                : 'Bu parça listesinin satırlarını çıkar. Ditto işaretlerini çöz, okuyamadığını işaretle.' }
          ]
        }]
      });
      girdi += yanit.usage.input_tokens || 0;
      cikti += yanit.usage.output_tokens || 0;
      const blok = yanit.content.find(b => b.type === 'tool_use');
      cevaplar[k.id] = blok ? blok.input : { hata: 'araç çağrısı gelmedi' };
    } catch (e) {
      cevaplar[k.id] = { hata: e.message };
    }
    process.stdout.write('.');
  }
  process.stdout.write('\n');

  const olcSet = Object.assign({}, set);
  olcSet[AYAR.kayitAlani] = kayitlar;
  const sonuc = alanPuanla(olcSet, cevaplar);
  sonuc.motor = MODEL;
  sonuc.tur = TUR;
  sonuc.jeton = { girdi, cikti };
  sonuc.atlanan = atlanan;

  const dosya = path.join(KOK, 'sonuc-' + TUR + '-' + MODEL + '.json');
  fs.writeFileSync(dosya, JSON.stringify({ sonuc, cevaplar }, null, 2), 'utf8');

  console.log('\nMOTOR: ' + MODEL + ' · TÜR: ' + TUR);
  if (atlanan) console.log('⚠️ ' + atlanan + ' fotoğraf bulunamadı — ölçüm EKSİK.');
  console.log(sonuc.ozet);
  console.log('jeton: ' + girdi + ' girdi · ' + cikti + ' çıktı');
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
