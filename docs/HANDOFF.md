# TAMIRON — DEVİR (HANDOFF.md)

> **Son güncelleme:** 18.10.2026 akşam *(K-064…K-069 · 9.1 ölçüt bitti, 9.2 geniş ekran prototipte, 9.3 muhasebe önerisi Cemil'de · yerel Gemma koşusu arka planda)*
> Bu belge *"yarın buraya oturan kişi ne bilmeli"* sorusunu cevaplar.
> Belge listesi değil, **durum** belgesidir.

---

## 1 · Nerede duruyoruz

**Faz:** tasarım / iş akışı. **Backend yok.** Üretilen tek çalışan şey
tıklanabilir prototip.

| | |
|---|---|
| Ekran | **17** — **ayarlar** *(12.10, K-057 — bakım periyodu · parça alış · tema · gizlilik; çekmecede tek satır)* · home · ses · ruhsat · kayit · parca-sor · teslim · arac · araclar · kayitlar · mesaj · musteriler · musteri · muhasebe · is-basina · **mesajlar** *(gönderilen mesaj arşivi, K-048.2)* + **ozet** *(müşterinin gördüğü sayfa — K-023)*. **Menü hâlâ altı satır** |
| Ana ekran | **Menü** — altı satır, her satırın sağında mikrofon. **Bakım hatırlatma şeridi yok** (K-019.1), **"Sesli işlem" satırı yok** (K-026) |
| Mesaj | Altı şablon, **her biri kayıt bazlı koşulla** (K-048.4) · **arşiv kendi sayfasında** (arama · kanal süzgeci · sayfalama) · mesaj yazımında **son 5** kapalı akordiyon · **sesli gönderim onay ekranından** · **toplu mesaj** (K-047.4) · **"yapay zeka düzeltsin"** — prototipte sahte, öyle yazılı (K-048.6) |
| Süreç | Servis kaydının omurgası: açılış → parça bekleme → **mesajlar** → teslim → hatırlatma, ve *"şimdi gönderilebilir"* önerileri. Yeni durum alanı yok, türetiliyor (K-048.4) |
| Yapay zeka | **Plan yazıldı, kod yok:** `arastirma/YAPAY-ZEKA.md` — dört iş (ses anlama · ruhsat · parça listesi · mesaj düzeltme), tek şart (**anahtar backend'de**), ölçüm yolu (değerlendirme seti) |
| Dokunuş | ana yol **13** · kısa yol **10** · **ruhsatsız yol 8** · **kayıtlı araç 8** · sesli yol **2 dokunuş + 1 konuşma** · müşteriye özet **2** — *hepsi 06.09'da yeniden yürütülerek ölçüldü.* Ana yol **+1**: tür seçimi −1, ruhsat sekmesi +1, teslim sonrası kapatma +1 (A-M14) |
| Kabuk | header (menü+rozet · logo · geri · ileri) + **alt gezinme çubuğu** (aynı `data-nav` denetimi — K-042; ortadaki tuş **gidiş-dönüş**, K-047.1) + çekmece + **katman/popup** (K-043) + tema (3 hâl) |
| Sağlık denetimi | **55 test · 0 bulgu** (55 · SES-DUYULAN: dal `ses-motor-iyilestirme`, onay bekliyor) · ⚠️ 13.10: sınıf çakışması testi kuralların yarısını görmüyordu, düzeldi (K-058.2c) · **320 · 375 · 430 px**'te ölçüldü (K-025) · denetim **ölçtüğü durumu geri yüklüyor** — veriyi (K-024.1), **DOM durumunu** (K-041.2), **gezinme yığınını ve mesaj arşivini** (K-047.5) — ve sekmelere dokunmuyor (K-026.7) |
| Derleme denetimi | **8** kontrol, bulursa `exit 1` (K-033.11). **7. kontrol artık ÜRETİLEN dosyadaki betiği ayrıştırıyor** — yerleştirmeden önceki betiğe bakıyordu ve `$'` bozulmasını görmedi (K-056.4). **`pwa-build` de enjekte ettiği betiği doğruluyor** (07.09) |
| Kararlar | **K-001 … K-064** · A-M48 kapandı (K-063) · sözlük bakımı onaylı giriş (`SOYLEYIS-BAKIM.md`) · açık: **A-M34** (ruhsat doğrulaması Cemil'de) |
| Ses | Cümle → **işlem nesnesi** → onay. **İşi ses tanımaz: sahibi olan ekran şemasını bildirir** (`Islem.tanim`, `betik/04-islem.js`; 12 şema · 7 ekran — K-056). Onay ekranı ve uygulama şemadan. `ISLEM_SEMALARI()` = modele araç tanımı. Bağlam dışı iş tanınıyor, bağlamın kendi işi **öneri** olur, iş bitince **"Kayda dön"** (Tur 7) |
| Sıradaki | **Cemil'in cevabını bekleyenler:** (1) K-067.2b ruhsat = Gemini 3.8 Flash + plaka karşılaştırması (K-069) · (2) 9.3 muhasebe kapsamı, dört soru (`arastirma/MUHASEBE-KAPSAM-OTOMIX.md`) · (3) araç kataloğu: TSB izni / TecDoc teklifi (`arastirma/ARAC-KATALOG.md`) · (4) bilgisayarda liste+kayıt aynı anda (mimari adım) · (5) Anthropic kredisi yüklenince Opus maliyet denemeleri. **Arka planda:** yerel Gemma 4 12B ruhsat koşusu (`D:\TamironGecici\gemma-ruhsat.log`, sonuç `tools/degerlendirme/sonuc-ruhsat-gemma4-12b.json`; bellek bekçisi açık) |
| Değerlendirme | `tools/degerlendirme/` — **üç set, tek cetvel**: ses (40 cümle · bugünkü motor **%93**) · **ruhsat (46 belge · 14 teknik + 4 kişisel alan; kişisel set `.gitignore`'da)** · **parça listesi (27 liste · 258 satır)**. Fotoğraf setleri **TASLAK**: ölçütü model yazdı, doğrulanmadı (K-049.1, A-M34) |
| Paket | `app/` — 9 dosya, çevrimdışı, düz yapı |

---

## 2 · Önce okunacaklar (sırayla)

1. **`arastirma/TAMIRCI-GERIBILDIRIM.md`** — bu deponun 1 numaralı
   belgesi. Tamircinin sözü bizim akıl yürütmemizi **yener**.
2. `KARARLAR.md` — K-001…K-018 + açık maddeler
3. `IS-AKISI.md` — yedi adım, beş kol, sesli yol, ölçümler
4. `EKRANLAR.md` — ekranlar; her ekranın *"üzerinde ne YOK"* sütunu
5. `VERI-MODELI.md` — sekiz tablo
6. `TASARIM-SISTEMI.md` — **sonundaki sınıf sözlüğü bağlayıcı** (K-008)
7. `SOZLUK.md` — Türkçe↔İngilizce; v1'den düşen 13 terim de burada

**Bu iş parçacığında yazılmadı, dışarıdan geldi:**
`arastirma/PARASUT-HARITA.md`, `arastirma/LUCANET-ONE-HARITA.md` ve `arastirma/OTOMIX-HARITA.md` —
ön muhasebe referans incelemeleri. Muhasebe kapsamı konuşulurken
bunlara bakılmalı.

---

## 3 · Prototipe nasıl dokunulur

```
prototypes/kaynak/**        <- ELLE BURASI DÜZENLENİR
  kabuk.html                   header + çekmece + toast + yuvalar
  ikonlar.svg                  33 ikon (Lucide, v1'den)
  stil/  01-taban 02-kabuk 03-bilesen.css
  betik/ 00-iz 01-cekirdek 02-veri 03-nesne 04-islem.js
  ekranlar/ 10-home 15-ses 20-ruhsat 30-kayit 40-parca-sor
            50-teslim 60-arac 70-araclar 80-kayitlar
            85-mesaj 90-musteriler 95-muhasebe
        |
        |  node tools/prototip-derle.js
        v
prototypes/omurga.html      <- ÜRETİLMİŞTİR, elle düzenlenmez
```

**Derleme beş şeyi denetler:** ekran dosyaları arası sınıf çakışması ·
ölü `data-git` hedefi · `id`/dosya adı uyuşmazlığı · **ölü ikon
referansı** · **tanımsız token** (`var(--x)` ama `--x` yok).

Her ekran betiği derlemede **kendi IIFE'sine sarılır** — v1'in sekiz
`var` sızıntısı hatası yapısal olarak imkânsız.

**Sağlık denetimi:**

```
python tools/denetim-kos.py geri    # varsa onceki gommeyi kaldir
node tools/prototip-derle.js        # SIRA ONEMLI (asagi bak)
python tools/denetim-kos.py gom
# tarayicida 320 / 375 / 430 px'te:  window.DENETIM_CALISTIR()
python tools/denetim-kos.py geri
```

**⚠️ SIRA: `geri` DAİMA `derle`den ÖNCE.** `geri` kendi yedeğini yazar ve
taze derlemeyi **ezer**; sonra o eski dosyaya denetim gömülür ve tarayıcıda
ölçülen şey kaynağın hâli **olmaz**. `gom` artık kaynak omurgadan yeniyse
**reddediyor** (K-024.3) — ama sıra yine de doğru kurulmalı.

Denetim ölçüm yapamadıysa **söyler** (`olcum: "ATLANDI"`); sıfır
genişlikte koşarsa sahte "0 bulgu" vermez.

**Paket:** `node tools/pwa-build.js` → `app/`

**Sesli işlem denemesi:** `.claude/launch.json` → `preview_start` →
`http://localhost:8123/prototypes/omurga.html`. Web Speech API
**güvenli bağlam ister**; `file://` ile mikrofon açılmaz, localhost
açılır. Çözümleme dışarıdan sınanabilir: `window.SES_COZUMLE(cumle)`,
`window.SES_METIN(cumle)`.

---

## 4 · Bu turda öğrenilen beş şey

**Belge taşımak sistem taşımak değildir.** Tasarım sistemi v1'den belge
olarak devralınmıştı ve doğruydu; ekranlar yine de yabancı çıktı çünkü
sınıf adları uydurulmuştu. Bağlayıcı olan **sınıf sözlüğü** (K-008).

**Bir kolun ekran açmaması, çalıştığı anlamına gelmez.** Beş istisna
kolu belgede *"ekran açmıyor"* diye geçtiği için doğru sayılmıştı;
işletilince ikisi çalışmıyordu.

**Bir kabı kaldırırken içindekiler de kalkmaz.** *"Çekmece yok"*
kararı alınırken tema anahtarının nereye gideceği sorulmamıştı; tema
kökte sabit bir değere düştü ve kullanıcının telefon tercihini sessizce
ezdi (K-009, K-010).

**Ekran çiziliyor olması çalıştığı anlamına gelmez.** Araç ekranı
açılıyordu ama oradan hiçbir şey yapılamıyordu — servis kaydı açmak
için ana ekrana dönüp ruhsatı yeniden çekmek gerekiyordu. Denetime
**27. test (OLU-UC)** eklendi.

**Kuralı yazmak yetmiyor.** K-018 (*Türkçede `\b` kullanma*) yazıldı,
sonra aynı tuzağa bir kez daha düşüldü: *"beş yüz"* içindeki "ş y"
harf dizisi sanıldı ve plaka yanlış okundu. Kural keskinleştirildi:
**Türkçe metinde sınır aranmaz, boşluktan bölünür.**

### 04–05.09 turunda öğrenilen beş şey daha

**Detay ile kalabalık aynı şey değil.** 03.09'da *"araçlar listesi yine
detaylı olmalı"* denmişti; satıra model, motor, yakıt, son km, kayıt sayısı
ve son bakım dolduruldu. Cemil hepsini kaldırttı — **kalabalık listede plaka
bile geç bulunuyor** (K-019.2).

**Aynı soruyu iki yerde cevaplamak, iki ayrı cevap demektir.** *"Hatırlatması
gelmiş kayıtlar"* iki ayrı süzgeçle hesaplanıyordu ve biri `tur === 'bakim'`
süzüyordu: aynı kayıt çekmecede görünüp ana ekranda görünmüyordu (K-019.1).

**Ölçüm aleti ölçtüğü şeyi değiştirmemeli.** Denetim `load` anında
kendiliğinden koşuyor ve 30. test her düğmeye basıyordu; prototip **her
açılışta kirli başlıyordu** ve ses çözümleyici her cümleyi *"satır ekle"*
diye okuyordu. Düzeltilir düzeltilmez, kirli durumun **gizlediği** gerçek bir
ölü uç ortaya çıktı — test *"geçmiyordu"*, **yanlış şeye bakıyordu**
(K-024.1).

**Bir kuralın uygulanacağı yer çoğalabilir (05.09).** *"Boşluğu kap verir"*
kuralı doğruydu; aynı turda eklenen sarmalayıcı div'ler kuralın **dışında**
kaldı ve ritim sessizce kırıldı. Kurala sınıf verildi (`.yigin`) ki hem
görünsün hem denetlenebilsin — anonim bir `<div>` hiçbir araca kendini
göstermiyor. K-029.

**Kaydırma silmez, sorar (05.09).** Görünmeyen bir hareket, görünmeyen bir
sonuç doğurmamalı — cepte kazara kaydırmak bir gideri silmemeli. K-032.

**Görünmeyen kaydırma, kırpmanın kardeşidir (05.09).** K-025'te *"kesilen
metin taşan metinden tehlikelidir"* deyip yatay kaydırma koymuştum; üçüncü
seçenek vardı ve daha doğruydu — **sığdırmak**. Kullanıcı sağa kaydıracağını
bilmez. K-031.

**`inputmode` bir kısıt değil, bir öneridir (05.09).** Tutar alanlarına harf
giriyordu; okuma tarafı temiz olduğu için **veri** bozulmuyordu ama ustaya
**gösterilen** bozuluyordu — `abc12x3` yazan usta kayda 12.345 ₺ düşürüyordu.
K-030.

**Bir testin eşiği kadar KAPSAMI da yanlış olabilir (05.09).** İkisi de
*"0 bulgu"* der: biri gevşek olduğu için, öteki bakmadığı için. K-029.2.

**Kesilen metin, taşan metinden tehlikelidir.** `overflow:hidden` bir kutu
taşmaz, **keser** — ve taşma testi taşmayan kutuları kasten atlar. 320 px'te
bir düğme *"Açık kay"* yazıyordu ve hiçbir test görmedi (K-025).

**K-018 üçüncü kez.** `` yasaklandı, yerine `indexOf` kondu, aynı hata geri
geldi: *"yeni **araç**"* → arama, *"**beyaz** araba"* → mesaj, *"**gösterge**
paneli"* → arama. Kural artık şöyle: **kısa komut kelimeleri içinde aranmaz**
(K-024.2).

### 06.09 turunda öğrenilen beş şey daha

**Derleyici "temiz" derken yalan söyleyebilir.** Beş denetimi de geçen bir
derleme, **ayrıştırılamayan** bir betik üretti: prototip tarayıcıda açıldı,
hiçbir şey tanımlanmadı, hata da görünmedi. Denetimler dosyanın *ayrıştığını
varsayıyordu.* En temel soru en sona kalmıştı — *bu JavaScript geçerli mi?*
Artık `new Function(js)` ile soruluyor (denetim 7). K-033.11.

**Kapanmamış bir yorum, silinen koddan tehlikelidir.** `.k-sil::before`
kuralı — ikincil düğmelerin 44 px dokunma alanı — aylardır bir yorumun
içindeydi. Silinen kod yoktur; **yorumlanan kod var gibi durur.** Dosyada
140 açılış, 139 kapanış vardı: tek bir sayma bunu söylerdi (denetim 8).

**`ciz()` ekranı çizer, KARARI değil.** Aynı turda iki kez düştüm: `ciz()`
içinde sıfırlanan bir ekran değişkeni, `Ekran.tazele()` her koştuğunda
kullanıcının seçimini siliyordu. Bir SEÇİM ekranın değil **verinin**
bilgisidir. **İkisini de hiçbir test yakalamadı** — 36 test 0 bulgu
veriyordu; ikisi de elle işletince çıktı. K-033.12.

**Kapalı düğme sessizdir.** `disabled` bir düğme, neden kapalı olduğunu
söylemez. Cemil *"Kaydı Aç"a bastı, bir şey olmadı* diye yazdı ve haklıydı.
Düğmeler artık kapanmıyor: eksik varsa **basınca söylüyor** ve eksik alana
götürüyor. K-033.3.

**Yarıçapı düzeltmek, genişliği düzeltmez.** Balon yumurta gibi
görünüyordu; teşhisim `border-radius` idi ve düzelttim — yarım çözümdü.
Asıl sebep: `position:absolute; left:50%` ile ortalanan, `right` verilmeyen
bir öğe kapsayıcının **yarısını** geçemiyor. 375 px'te tavan 187 px'ti,
`max-width` yazmak işe yaramıyordu. **Belirtiyi düzeltmek, sebebi
düzeltmek değildir.** K-033.10.

### Ölçüm tuzakları — bu oturumda dört kez düşüldü

Hepsi *"bulgu"* gibi göründü, hiçbiri bulgu değildi. **Önce aracına bak.**

| Tuzak | Belirtisi |
|---|---|
| Ekranı tazeleyip **önceden yakalanmış** düğmeye tıklamak | Kopmuş düğüm hiçbir şey yapmaz → "sessiz düğme" |
| İmza olarak `innerHTML.length` | Sekme takasında bir `hidden` eklenip biri kalkar, `aria-pressed` true↔false olur → **uzunluk aynı**, çalışan düğme sessiz görünür |
| Paylaşılan durumu sıfırlamadan taramak | `muhDonemSecili` iki ekranda ortak; önceki tıklama sonrakini kirletir (K-024.1) |
| Tema değişiminden **hemen sonra** renk okumak | `0.15s` geçişin ara karesi okunur; iki `requestAnimationFrame` yetmez, ~700 ms beklenir (K-039.3) |
| Yalnız `DENETIM_CALISTIR()` sonucuna bakmak | Denetim `load` anında da koşuyor ve sonucu `window.DENETIM`'e yazıyor. **İlk koşu farklı çıkabilir** — ekranlar henüz çizilmemiş olur. İkisine de bak (K-041.3) |
| `textContent` ile "ekranda ne yazıyor" diye bakmak | **Gizli metni de verir.** `hidden` bir öğenin eski içeriği okunur ve "temizlenmemiş" sanılır — `innerText` kullan ya da `hidden`a bak (K-040.5) |
| Derleme komutunu **boruya sokmak** | `node tools/pwa-build.js \| tail -1` — bir boru hattının çıkış kodu **son** komutundur (`tail`), o hep başarılı. Derleme çöker, zincir devam eder, **bozuk yapı commit'lenir.** 07.09'da tam bu oldu. Derleme komutları boruya sokulmaz; gerekirse `; echo "EXIT=$?"` eklenir |
| Bir örtü açıkken **ikincisini** açıp kapamak | `inert` tek bir "aç/kapat" ikilisiyle yönetilirse, kapanan örtü **öbürünün** kilidini de kaldırır. Örtüler yığın oluşturur (K-043.4) |

Ayrıca: `getComputedStyle` zemini `color(srgb 0.88 0.92 0.96 / 0.55)`
biçiminde verebilir — `rgb()` bekleyen bir ayrıştırıcı bunu 0–255 sanıp
kontrastı yanlış hesaplar.

### 14.09 turunda öğrenilen iki şey daha

**"İki ekran da doğru" cümlesi bir hata sınıfını gizliyor.** Bu turun
üç maddesi de ekranların **arasındaki** tutarsızlıktı: iki müşteri
listesi, iki metin üreteci, iki gönderim yolu. Hiçbiri kendi içinde
bozuk değildi — ve bu yüzden hiçbir test görmüyordu. Yeni testler
(41 · 42 · 43) ekranın iç doğruluğunu değil **iki yer arasındaki
aynılığı** ölçüyor.

**Koşul doğru cümleyle yazılıp yanlış yere bakabilir.** *"Teslim
edilmiş araca teslim mesajı"* kuralı doğruydu; **son kayda** bakıyordu,
oysa araç bugün serviste olabilir ve teslim edilmiş bakımı geçmişte
kalmış olabilir. Ekranı açıp dört araçta denemeden görülmedi.

### 07–08.09 turunda öğrenilen üç şey daha

**Bir yerde uygulanan kural, ötekine kendiliğinden geçmez.** Bu tur
**dört kez** aynı biçimde tekrarlandı: `OLCULEBILIR` bayrağı tek teste
bağlıydı · `arkaPlan()` kapanacakları tek tek sayıyordu · denetim
akordiyonu geri yüklüyordu ama katmanı bırakıyordu · `prototip-derle`
üretilen betiği doğruluyordu ama `pwa-build` doğrulamıyordu.
**Çözüm hep aynı oldu: kuralı listeye değil YAPIYA bağlamak.**

**Ölçüm aracına, ölçtüğü şey kadar bakılır.** Bu turun altı
"bulgusu"ndan **altısı da** ölçüm hatası çıktı (yukarıdaki tablo).
K-024.1 sağlık denetimi için yazılmıştı; her ölçüm için geçerli.

**Bir araştırma notunun kendi belirsizliğini işaretlemesi işe yarıyor.**
`SMS-VE-IYS.md` 03.09'da *"bunlar 2014 tutarları, güncel değil"* diye
işaretlemişti; 08.09'da o not sayesinde doğrudan doğrulandı ve
düzeltildi. **Bilmediğini yazan belge, bilmediğini gizleyenden
kullanışlıdır.**

---

### 12.10 turunda öğrenilen iki şey

**Yeniden yapılandırmanın ölçüsü "bir şey değişmedi"dir — ve bu ÖLÇÜLÜR.**
Ses şemaya taşınırken eski derleme kopyalanıp yenisiyle yan yana
yüklendi; ~70 cümle × 8 bağlam nesne, onay ekranı ve uygulama düzeyinde
birebir karşılaştırıldı (`tools/ses-esdeger.js`). Bu araç benim hatamı
denetimden **önce** yakaladı. 8.1'de servis detayı yeniden kurulurken
de aynı araç koşmalı.

**Derleyici de bir ölçüm aletidir.** `replace(yer, js)` dizgedeki `$'`'ı
yorumlayıp betiği bozdu; 7. denetim yerleştirmeden ÖNCEKİ betiğe
bakıyordu ve "temiz" dedi. Kaynak doğru, araç yalancıydı. Ölçülecek
şey tarayıcının aldığı dosyadır.

### Bayat sayfa — ölçülen şey yüklenen şey değil (16.09.2026)

Yerel sunucudan açılan prototipte tarayıcı **eski kopyayı** tuttu:
sunucuda yeni derleme duruyordu, sayfada eski kod çalışıyordu.
`location.reload()` ve sekmeyi kapatıp açmak yetmedi; sorgu dizesi
(`?v=2`) da işe yaramadı çünkü önizleme adresi kökene sadeleştiriyor.

**Belirtisi yanıltıcı:** yeni yazılan kod "çalışmıyor" görünüyor ve
insan koda bakmaya başlıyor. Yarım saat kodda hata aradım; hata
koddaydı değil, **yüklenmemişti**.

**Kural:** yeni koda dayanan her ölçümden önce sayfanın TAZE olduğu
doğrulanır —

```js
document.documentElement.outerHTML.indexOf('<yeni eklenen bir dize>') >= 0
```

ve bayatsa **yeni bir port** açılır (yeni köken = yeni önbellek).
K-024.1'in kardeşi: ölçüm aleti ölçtüğü şeyi bozmamalı — ve
**ölçtüğünü sandığı şeyi ölçmeli.**

## 5 · Cemil'in kararını bekleyenler

⚠️ **LİSTE BURADA DEĞİL, `KARARLAR.md`'NİN SONUNDA** (14.09).

Burada ikinci bir liste duruyordu ve iki liste ayrışmıştı — aynı soruyu
iki yerde tutmak, iki ayrı cevap demektir (K-019.1).

**12.10 itibarıyla on beş maddenin hepsi cevaplandı** (K-055). Açık
kalan iki madde:

| Kod | Soru | Durum |
|---|---|---|
| **A-M48** | Dükkân sesine erişmeden ses tarafı nasıl ölçülür? | **AÇIK** — Tur 8.4'ün araştırma işi |
| **A-M36** | KVKK saklama süresi | **ERTELENDİ** — gerçek veri tutulmadan önce kapanmalı |

**12.10'un üç kalıcı kararı:**

- **Backend bu fazda yok** (A-M31) → iz, çıkarım sayfası, parçacı ağı,
  düzeltmelerin dışarı çıkması: hepsi backend turunda. **Bedeli:**
  birikim mekanizması çalışıyor ama **sayaç sıfır** — düzeltmeler
  dükkânda kalıyor (K-055.1).
- **Alış fiyatı zorunlu değil** (A-M43) → `alis: null` ≠ `alis: 0`;
  bilinmeyen alış kâra **girmez** — **yapıldı** (K-057; yıl 7.410 → 5.310 ₺).
- **Ayarlar sayfası kapsama girdi** → **yapıldı**, 17. ekran (K-057).

Kapsam kararlarından ikisi **tek cümleyle geri alınabilir**: `MUH`
(kasa+gider) ve `A-M25` (toplu mesaj kalıyor).

**Yapılmamış işler:** müşteri uygulaması (T-21) · **gerçek ruhsat ve
parça listesi okuma** (`YAPAY-ZEKA.md` — backend bekliyor) · **sesli
komutun modele bağlanması** (aynı yerde) · gerçek WhatsApp gönderimi
(`WHATSAPP.md` A yolu) · SMS + İYS kaydı · **muhasebe kararının açtığı
dört iş** (ay bazlı gider toplamı → dışa aktarma → alacak → izin alanı) ·
fatura girişi (A-M44, muhasebeyle) · backend.

**Sunum kaynağı:** *"Tamiron — Özellikler ve Baştan Sona Kullanım"*
(belge bağlacında, 9 bölüm). 12.10'da dört yeri tazelendi: maliyet
kararı · ses ölçümü · fatura girişi · birikim sayacının sıfır olduğu.
⚠️ **Cevapsız soru:** sunum **tamirciye mi yatırımcıya mı**? *"Neden
kopyalanamaz"* bölümü yatırımcı sorusuna hazırlıktı; tamirciye
yapılacaksa o bölüm çıkar, *"baştan sona"* büyür.

---

## 6 · Değişmez kurallar

- **Yedi iş dışında özellik yok.** *"İleride lazım olur"* gerekçe değil.
  Kapsamı yalnız **Cemil** genişletir; genişletince `KARARLAR.md`'ye yazılır.
- **Tamircinin sözü bizim akıl yürütmemizi yener.**
- Ham hex yok — yalnız `tokens.css`. Yazı boyutu ve dokunma hedefi de token.
- Türkçe arayüz, İngilizce kod. MudBlazor yok.
- **Türkçe metinde `\b` kullanılmaz** (K-018) — boşluktan bölünür.
- Prototip tek dosya değildir; `omurga.html` **üretilir**.
- Ortak nesne kuralı: plaka, kayıt satırı, boş durum **tek yerde**
  çizilir; **davranışı da orada durur**.
- Sınıf sözlüğünde olmayan bileşen sınıfı açılmaz (K-008).
- `KARARLAR.md`'ye yalnız lider dokunur; belirsizlik *"Cemil onayına
  açık"* işaretlenir ve **iş durmaz**.
- **Ölçmeden "yapıldı" denmez.** Her işten sonra derleme + sağlık
  denetimi + tarayıcıda 375 px'te ölçüm.
- Yeni bir hata sınıfı bulunursa **denetime testi eklenir** ve negatif
  testle doğrulanır.
