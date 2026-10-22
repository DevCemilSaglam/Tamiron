# TAMIRON — KARARLAR (KARARLAR.md)

> Verilen kararların gerekçeleri. Aynı tartışma iki kez açılmasın. Yeni karar **en üste** eklenir.
>
> **Bu liste 03.09.2026'da SIFIRDAN başladı.** v1'in ürün/akış kararları bilinçli olarak devralınmadı — tamircinin geri bildirimi (`arastirma/TAMIRCI-GERIBILDIRIM.md`) akışı yeniden kuruyor ve eski kararların gündemde durması yeni tartışmayı kirletiyordu.
>
> **Devralınan mimari kararlar `MIMARI.md`'dedir** ve burada tekrarlanmaz: teknoloji yığını, üç uygulama ayrımı, tenant/yetki altyapısı, soft-delete, katalog hibrit yapısı, Türkçe UI / İngilizce kod, tokens tek kaynak, tasarım-önce. Bunlar akıştan bağımsızdır — değişecekse `MIMARI.md` açılır.
>
> v1 arşivi: `github.com/DevCemilSaglam/Tamiron-v1` (195 commit; her kararın gerekçesi commit mesajında).

---

## K-073 · Backend fazı açıldı (22.10.2026)

**Cemil:** *"1 ve devam"*. Önerilen üç adımın ilki aşamayı değiştirmekti,
"devam" da kalan ikisinin onayıdır. Bundan önce: *"Tasarım / şablon bitince
backend'e geçeceğiz."*

**73.1 · Aşama değişti.** `CLAUDE.md` artık "backend" diyor. Prototip
davranışın, `VERI-MODELI.md` v2 de şemanın şartnamesi. Prototip silinmez;
backend'in ölçütüdür.

**73.2 · Yığın — MIMARI §2–§3, değişiklik yok.** .NET 8 (makinedeki SDK
8.0.202) · ASP.NET Core Web API · EF Core 8 + MSSQL · MediatR (CQRS-hafif) ·
xUnit.
- Gün-1'de yalnız dört proje var: Domain, Application, Infrastructure, Api.
  Bunlara iki test projesi eşlik ediyor.
- UI (Blazor RCL'leri) sonra gelir.

**73.3 · MediatR 12'de sabit.** 13. sürümden sonra MediatR ticari lisansa
geçti; 12.x MIT lisanslı. Yükseltme ayrı karar ister.

**73.4 · Geliştirme veritabanı LocalDB** (`(localdb)\MSSQLLocalDB`, Windows
kimliği).
- Parola yok, dolayısıyla depoya gizli bilgi girmez.
- Makinedeki `MSSQLSERVER` servisine dokunulmaz: Cemil'in başka işleri
  olabilir.

**73.5 · İlk dilim (Cemil'e önerildiği gibi).**
- İlk dört tablo: `Shop`, `Vehicle`, `ServiceRecord`, `ServiceLine`.
- Ortak alanlar MIMARI §4.6: `TenantId` filtresi, soft-delete, `RowVersion`.
- İlk API kuralı tahsil günü (K-071.1), saf domain servisi olarak ve önce
  testi yazılarak. Kayıt toplamı kuralı (K-037) aynı serviste.

**73.6 · Kiracı (tenant) kimliği — Cemil onayına açık.**
- Kimlik doğrulama henüz yok. Geliştirmede kiracı `X-Tenant-Id`
  başlığından okunuyor (`ITenantContext`).
- Bu geçici. Gerçek oturum açma ayrı bir karar: ustanın nasıl gireceği
  (telefon + SMS kodu mu, parola mı) tamircinin kullanım alışkanlığına
  bağlı.

**73.7 · Kimlik `Guid`, istemcide doğar.** Uygulama çevrimdışı çalışan bir PWA;
kayıt sunucuya sormadan oluşturulabilmeli. `int` kimlik sunucudan dönmeden
bilinmez. (Prototipteki `length + 1` kimliğinin yinelenme hatası da (K-072.4)
bu sınıfı tamamen kapatıyor.)

**73.9 · Prototipten bilinçli sapma: ay sonu (B5, 22.10).** Bakım hatırlatmasının ay
tarihi prototipte JS `setMonth` ile hesaplanıyordu. Bu yöntem ay sonunu taşırıyor:
31 Ağustos + 6 ay → 3 Mart. Backend ay sonuna sabitliyor: 28 Şubat (`DateOnly.AddMonths`).
Hatırlatma, bakım ayını aşıp bir sonraki aya kaymamalı. Prototip ölçüt olmaya
devam ediyor; bu tek fark burada yazılı. Yarım gün yuvarlaması ise prototiple
aynı: JS `Math.round` gibi yukarı (testi var).

**73.8 · İlk dilim bitti — ölçüm.**
- Domain'de 22 test, LocalDB üzerinde API'de 5 test; hepsi yeşil.
- Üç kural bilerek bozuldu, üçünde de testler kırmızıya döndü:
  - kiracı süzgeci kaldırıldı → 2 API testi kırmızı;
  - soft-delete kaldırıldı → 1 test kırmızı;
  - tahsil günü = teslim günü yapıldı → domain'de 1, API'de 1 test kırmızı.
- Geliştirme ortamında API ayağa kalktı; göç ve tohum uygulandı.
  `GET /api/cash?period=Year` prototipin rakamlarını verdi: nakit 2.800 ·
  havale 2.850 · alacak 3.450. Başlıksız istek 401.
- Okuma kilitlenmesin diye `READ_COMMITTED_SNAPSHOT` göçle açıldı
  (MIMARI §4.1).
- Başka dükkânın kaydı **404** döner, 403 değil: 403 kaydın var olduğunu
  söylerdi.

---

## K-072 · Veri modeli v2 — prototiple uzlaştırıldı, sapma derlemede yakalanır (22.10.2026)

**Bağlam.** Cemil: *"Tasarım / şablon bitince backend'e geçeceğiz."* Backend'in
başlangıç belgesi `VERI-MODELI.md`'dir ve 15.09'da kalmıştı. Prototip beş
haftada büyüdü; belgede olmayan alan backend'e taşınırken sessizce kaybolur.
Commit: `6f5977b`.

**72.1 · Belge v2.** Araca ruhsatın teknik alanları ve ileti izni; kayda tahsil
ve bekleme tarihi; dükkâna km periyodu, alış sorusu, gizlilik notu girdi. İki
tablo eklendi: `Expense`, `Message` (A-23 cevaplandı). On tablo.

**72.2 · Tohum = şema; sapma derlemede yakalanır.** Prototipin tohum verisi
(`02-veri.js`) şemanın kendisi sayılır. Kodda yazılan her alan tohumda bir kez
görünür. Tohumdaki her alan belgenin "Prototip eşlemesi" tablosundadır.
**Derleme denetimi 9** ikisini de arar ve uymayanı `exit 1` ile durdurur.
Gerekçe: elle uzlaştırma bir kez yapılır, sonra yine kayar.

**72.3 · İş kuralları listelendi.** Prototipte backend olmadığı için tarayıcıda
duran 11 kural (toplam, tahsil günü, dönem, hatırlatma, araç kimliği, plaka
çatışması, toplu alıcı süzgeci, CSV…) belgede tablo hâlinde. Hepsi **API'ye**
taşınır (CLAUDE.md: arayüz karar vermez).

**72.4 · Uzlaştırmanın bulduğu iki kusur.**
- `GIDERLER.tur`'u hiçbir yer okumuyordu. Alan silindi, modele alınmadı.
- Sesli gider `GIDERLER.length + 1` kimliği veriyordu. Bir gider silinince
  var olan kimlik yeniden verilir ve sola kaydırma yanlış gideri silerdi.
  Kimlik artık tek yerden üretiliyor (`yeniGiderId`); denetim testi 63.

**72.5 · Açık, teknik:** A-24 — parça sorusunun satırla bağı için
`PriceRequestLine` ara tablosu öneriliyor. Backend turunda kesinleşir.

---

## K-071 · Muhasebe sırası (MUH-B) kapandı: alacak tahsili · dışa aktarma · ileti izni (22.10.2026)

**Bağlam.** Cemil: *"Elimizde kalan işlere devam edelim. Tasarım / şablon
bitince backend'e geçeceğiz."* MUH kararının (B — kasa + gider) açtığı dört
işten biri (ay bazlı gider) zaten yazılıydı; kalan üçü bu turda bitti.
Commit'ler: `7c129f4` · `51dc7fd` · `7f49167`.

**71.1 · Alacak tahsili — tahsilat ÖDENDİĞİ GÜNE yazılır.**
- Veresiye ("Almadım") kayıtta, teslim notunun altında Nakit/Kart/Havale
  çipleri. Ayrı tahsilat ekranı yok (A-2).
- Kayda `tahsilTarihi` geldi. Muhasebe tahsilatı bu güne göre sayar;
  "teslim edilen" sayısı teslim gününde kalır.
- Gerekçe: teslim gününe göre saysaydık Ağustos'ta teslim edilip Ekim'de
  ödenen para Ekim kasasında görünmezdi.

**71.2 · Dışa aktarma — A-M22 uygulandı.**
- "Excel'e aktar": tek düz CSV (BOM + `;` + ham rakam). Formül koruması
  var: `=` ile başlayan hücrenin başına `'` konur.
- "Yazdır / PDF": `window.print()` + `stil/05-yazdir.css`. Kâğıt hep açık
  tema.
- Doğrulanmadı: yazdırma önizlemesi gözle; iOS ana ekran uygulamasında
  indirme (gerçek cihaz).

**71.3 · Ticari ileti izni — A-M21 uygulandı.**
- Müşteri künyesinde tek çip, varsayılan YOK; izin tarihle yazılır.
- Toplu mesaj yalnız izni olana gider; onay ekranı gitmeyenleri sayar.
- Özet, "aracınız hazır", bakım hatırlatması izin istemez — tek araca
  giden, yapılan işin devamı.
- Prototipte izin "evet/yok" ve tarih. **İYS kaydı ve iznin NASIL
  alındığı (sözlü/yazılı/SMS onayı) backend fazında** — dükkânın İYS'ye
  kaydı ustanın işi (TOPLU-MESAJ.md).

**71.4 · Yeni denetim testleri:** 59 ALACAK-TAHSIL · 60 DISA-AKTARMA ·
61 GRID-ALANSIZ (yeni hata sınıfı: adlı ızgarada alansız öğe ilk boş
hücreye kaçar — ölçerek bulundu) · 62 ILETI-IZNI. Hepsi negatif doğrulandı;
altı genişlikte 0 bulgu.

**Sonuç:** prototipte muhasebe işi kalmadı. Backend'e geçmeden önce kalan
tasarım işi için `CALISMA-KUYRUGU.md` Tur 9'a bakılır.

---

## K-070 · Hedef: dış servislere bağımlı olmayan yapay zekâ — evde, kademeli (20.10.2026)

> **Cemil, 20.10:** *"Dış servislere bağımlı olmak istemiyorum. Yasal
> yükümlülük ve gelecekte olası fiyat artışları nedeniyle. Baştan ev gibi bir
> ortamda kurgulamak istiyorum. Fakat ilk başta makinenin düşük, giderek artan
> bir şekilde olması düşünüyorum. [...] resim ses vb o sunucuda tutmaya gerek
> yok. Yapay zeka için çalışacak."*

**70.1 · Yön:** ses ve ruhsat okuma **kendi donanımımızda**, evde başlar.
Makine **verisiz** çalışır: fotoğraf ve sesi okur, saklamaz. Uygulama ve
veritabanı Türkiye'deki VPS'te durur; aradaki bağlantıyı makine kendisi
kurar (WireGuard). Dış servise düşülmez; yedek yol elle giriş.

**70.2 · Kademeli donanım (öneri, `arastirma/KENDI-SUNUCU.md` §7):**
- **Aşama 0:** saatlik kiralık kartla ölçüm.
- **Aşama 1:** Mac mini M4 Pro 64 GB + UPS, ~145 bin ₺.
- **Aşama 2:** ikinci aynı makine (yük + yedek).
- **Aşama 3:** Türkiye'de colocation.

**RTX 5090 yolu önerilmez:** kart ~270–390 bin ₺ ve elektriği evi yüksek
kademeye ve 4.000 kWh sınırının üstüne itiyor.

**70.3 · Kapı:** Aşama 0'da şase ≥ 45/46 ve ses ≥ %84 sağlanmadan donanım
alınmaz. Şimdiye dek ölçülen 8–12B yerel modeller bu şartı sağlamadı.
Ölçülecek olan 30B sınıfı.

**70.4 · Önce çözülecek iki engel:**
- (a) **Bireysel internet aboneliğinde ticari kullanım yasak** (Superonline,
  TT sözleşmeleri) → kurumsal abonelik.
- (b) İşyeri adresi ve KVKK güvenliği → avukat ve muhasebeci.

**70.2b · Güncelleme (20.10, `KENDI-SUNUCU.md` §8) — Cemil: "Mac mini ve ups
olarak yapılır", yedek uzak sunucuya.**
- M4 Pro satıştan kalkmış. Güncel seçenek **Mac mini M5 Pro 64 GB / 1 TB**
  (eğitim indirimli ~186 bin ₺).
- Aynı fiyat bandında **Mac Studio M5 Max 64 GB** 1,5–2 kat bant genişliğiyle
  daha hızlı. TR fiyatı en çok %10 fazlaysa o önerilir.
- **Satın alma = ölçüm:** Apple TR çevrimiçi mağazası 14 gün iade tanıyor.
  Aşama 0 bu makinede yapılır, şart sağlanmazsa iade edilir (iade
  edilebilirliği Apple'a teyit ettirilecek).
- **Modeller (yayımlanmış sonuçlara göre):**
  - ruhsat: Qwen3-VL 30B-A3B ana aday, Qwen3-VL 32B ve Qwen2.5-VL 32B
    karşılaştırma, PaddleOCR-VL şasede ikinci okuyucu;
  - ses: Whisper large-v3 (Türkçe CER %2,4–5,2, ek eğitimsiz).
- **Korumalar:** şasede üretici kodu = marka, yıl kodu = model yılı,
  iki okuyucu uyuşmazsa ustaya sor.

**70.5 · Geçiş dönemi:** Aşama 1'e kadar Gemini (K-067.2c) sürer. TC, ad ve
adresin dışarı gitmemesi için maskeleme önerisi açık, Cemil onayına.

---

## K-069 · Ruhsattan okunan plaka ustanın yazdığını EZMEZ — çatışmada usta seçer (18.10.2026 · Cemil onayına açık)

**Nereden çıktı:** fotoğraf ölçümü (FOTO-OLCUM §7). Gemini 3.8 Flash
şaseyi 46/46 okudu ama **5 plakayı yanlış okudu**; silik basılmış F/E ve
R/B harfleri. Prototipte okunan plaka, ustanın aracın önünde **yazdığı**
plakanın üstüne **sessizce** yazılıyordu. Yani en ucuz okuyucunun tek
zayıflığı, en kritik alana sessizce sızıyordu.

**Kural (prototipte, `20-ruhsat.html`):**
- Usta plakayı yazmışsa ve okunan plaka farklıysa **ustanınki alanda kalır**.
- Ekran iki plakayı yan yana gösterip sorar: *"Sen X yazdın, ruhsatta Y
  okundu. Hangisi doğru?"* İki düğme var.
- **Seçim yapılana kadar araç kaydedilmez.** Yanlış plakayla kayıt açılmasın,
  başka bir aracın boş alanları dolmasın. *"Kaydı aç"* da bekler ve
  nedenini söyler.
- Usta plaka yazmadan ruhsat okuttuysa davranış aynı: okunan plaka gelir.

**Neden önemli:** bu kural, ruhsat okumayı Opus yerine **altıda bir fiyatına
Gemini 3.8 Flash'a** bırakmayı mümkün kılan şey. Şase doğru (K-068'in kapısı),
plaka ise ustanın sözüyle sınanıyor (K-067.2b).

**Denetim:** 58. test `PLAKA-CATISMA`. Negatif doğrulandı: çatışma dalı
kaldırılınca *"okuma ustanın yazdığı plakayı ezdi"*. 320/375/430/768/1280 px'te
0 bulgu.

---

## K-068 · Ruhsat isteğe bağlı; PARÇA SORMAK için araç kimliği şart — K-066'yı düzeltir (18.10.2026)

> **Cemil, 18.10:** *"Ruhsat adımını geç düğmesini kaldırdım […] Bu doğru
> olmadı. Araç gelir ve ruhsatı vermek istemeyebilir. Bunu dememiştim. Parça
> fiyat listesi isterse, ruhsat zorunlu olacak. Şase yoksa, model marka
> hepsini seçtirmemiz gerekiyor. Yani aracın parça listesi bilinmez
> olmayacak. […] belki de araç tamir olacak ve parça gerekli değil. Ruhsat
> zorunlu olmamalı. b tamam."*

**68.1 · Liderin hatası.** K-066'da *"şase okunmuyorsa devam etmemeli"*
sözünü **kayıt açmaya** uyguladım. Cemil'in kastettiği **parça sormaktı**.
Aracın parçası gerekmeyebilir; ruhsatı vermek istemeyen müşteri olabilir.
Belirsizliği Cemil'e sormadan en katı okumayla uyguladım. Ders: bir sözü
akışın hangi adımına bağladığımı karardan önce yazmalıyım.

**68.2 · Kural:**
- **Kayıt açmak:** ruhsat isteğe bağlı. *"Ruhsat adımını geç ve kaydı aç"*
  geri geldi. Şasesi okunamayan ruhsatta da araç kaydedilir (K-065 eski
  hâline döndü). Ekran yalnız *"parça sorarken araç seçimi gerekir"* der.
- **Parça sormak:** araç ya **şaseyle** ya da **marka + model + yıl + motor**
  ile tanınmalı (`aracKimligiTamam`, çekirdek). Şase yoksa parça sorma
  ekranında dört alan çıkar; hepsi dolmadan soru gitmez, düğme bunu söyler.
  Seçim araca yazılır, bir kez sorulur. Ekrandan ruhsat çekilebilir; okunan
  şase kayıtlı aracın boş alanına yazılır.
- **Şase mesajda:** varsa parçacıya giden mesajda *"Şase: …"* satırı olur.
- **Seçenekler:** marka ve model listesi söyleyiş sözlüğünden geliyor (tek
  kaynak). Gerçek katalog backend fazında (A-11-3).
- **A-M52 kapandı:** *"yalnız parça sor"* diye ayrı bir yol yok. Parça
  sorusu her zaman bir kayıttan gider ve araç kimliği ister.

**68.3 · K-066.3 (b) onaylandı:** okumadan sonra plaka düzeltilirse kaydedilen
aracın plakası da değişir. **(a)** artık geçersiz: kayıt açmak şaseye bağlı
değil.

**68.4 · Denetim:** 56. test yeniden yazıldı:
- a) kayıtsız araçta ruhsatsız *"Kaydı aç"* VAR;
- b) şasesiz okumada araç kaydedilir;
- c) şasesiz, seçilmemiş araçta parça sorusu GİTMEZ, seçilince GİDER.

Negatif doğrulandı: kimlik kapısı hep açık → bulgu; atla düğmesi kaldırıldı →
bulgu. 320/375/430 px'te 0 bulgu.

---

## K-067 · Fotoğraf okuma ölçüldü: ruhsat ve parça için Opus 5.5 aday (18.10.2026)

> **Cemil, 18.10:** *"Gerçek doğruluğu ve maliyeti ölçelim."* (A-M35 → KAPANDI)

**67.1 · Sonuç** (`arastirma/FOTO-OLCUM.md`). 46 ruhsat ve 27 parça listesi,
üç model:
- **Ruhsat:**
  - Opus 5.5: şase **46/46**, kritik alan yanlışı **0**, plaka uydurma yok.
  - Sonnet 5.5 en önemli tuzakta plaka **uydurdu** → elenir (K-061.3c kuralı).
  - Haiku 5.5 iki şaseyi yanlış okudu; biri 17 karakter, yani sessizce geçer
    → elenir (K-066.4).
- **Parça:** Opus ve Sonnet liste tamlığında eşit (%56). Opus ditto (%100)
  ve fiyatta (%91) önde.

**67.2 · Öneri — Cemil onayına açık: ruhsat ve parça listesi Opus 5.5.**
Maliyet ruhsat başına ~3 cent, dükkân başına ~$6/ay. YAPAY-ZEKA §5'in
*"karar maliyetle değil doğrulukla verilir"* kuralına göre seçim bu.
Ürüne koyma backend fazında (K-055.4).

**67.2b · GÜNCELLEME (18.10 akşam) — ödemesiz yol:** Gemini 3.8 Flash (Cemil'in
önceden yüklediği kredi, ücretli katman) şaseyi **46/46** okudu, ruhsat başına
**0,48 ¢**. Opus'un altıda biri fiyatına (2027'den ~0,96 ¢). Zayıflığı plaka:
5 hata. Plaka ekranda zaten ustadan geliyor; ruhsattan okunanla
karşılaştırılırsa plaka hatası sessiz kalmaz. **Öneri değişti, Cemil onayına
açık:** ruhsat = **Gemini 3.8 Flash + plaka karşılaştırması**; Opus doğruluk
yedeği. (FOTO-OLCUM §7)
**Düzeltme:** Sonnet'i *"r31'de plaka uydurdu"* diye elemiştim; ölçüt
yanlıştı, plaka okunuyordu (34 NE 6451). Sonnet doğru okumuştu. Gerçek kritik
hatası 1 (r24).

**67.2c · KARAR — Cemil 19.10: *"Şimdilik gemini kullanalım."*** Ruhsat okuma
= **Gemini 3.8 Flash** (ücretli katman) + **plaka koruması (K-069)**. Yerel
denemeler (Gemma 4 12B, Qwen3-VL 8B) bu dizüstünde yetmedi. Kendi sunucu
seçeneği: `arastirma/KENDI-SUNUCU.md` (gerekçesi para değil gizlilik;
başa baş ~1.000–2.000 dükkân; sıradaki adım saatlik kiralık ekran kartıyla
~$5–10'luk ölçüm, Cemil onayıyla).

**67.3 · Ölçütte üç plaka yanlıştı.** (+ r31 = **dört**) Üç model aynı şeyi okudu, fotoğraf
modelleri doğruladı. İnsan doğrulaması da yanılır; modellerin birleştiği
ama ölçütten ayrıldığı yer ölçütü sınar. Düzeltildi.

**67.4 · Ölçüm aletinde 7 kusur bulundu ve düzeltildi.** Biri ağır: **parça
listesinin cetveli hiç yoktu** ve her liste "TAM" görünüyordu.
Liste: FOTO-OLCUM §4.

**67.5 · Açık iyileştirmeler (ölçülmeden ürüne girmez):**
- (a) Opus silik koltuk sayısına "5" yazıyor;
- (b) parça listesinde ustanın yazdığını "düzeltiyor" ("Sibop kayıt" →
  "Subap kayışı").
İkisi de istem işi. Bir sonraki koşu bu iki sınıfı hedefler.

---

## K-066 · ~~Ruhsat ZORUNLU; şase okunmadan devam yok~~ — ⚠️ K-068 ile DÜZELTİLDİ (kayıt açmak ruhsata bağlı değil; kapı parça sormada) (18.10.2026)

> **Cemil, 18.10:** *"Yalnız parça sor yok. Ruhsat okunmak zorunda. Servis
> kaydı olmasa da ruhsatı çekmeli. Çünkü bize orada şase no gerekli. Hatta
> şase no okunmuyorsa devam etmemeli."*

**66.1 · Değişen eski karar.** 06.09'da Cemil *"ruhsat çekmeden devam
edebilir"* demişti; ekranda *"Ruhsat adımını geç ve kaydı aç"* düğmesi vardı.
**O karar bu kararla değişti**, düğme kalktı. A-M52 (*"yalnız parça sor"*)
**KAPANDI: böyle bir yol yok.**

**66.2 · Kural (prototipte, `20-ruhsat.html`):**
- **Kayıtsız araç:** ruhsat çekilmeden araç alanları bile açılmaz. Tek düğme
  *"Ruhsatı çek"*.
- **Şase ruhsattan okunmadıysa:** araç **kaydedilmez** (K-065 buna göre
  daraldı), *"Kaydı aç"* çizilmez. Ekran kırmızı uyarıyla *"Şase no okunamadı
  — ruhsatı yeniden çek"* der. Kapı düğmeye değil kurala bağlı: başka yoldan
  basılsa da kayıt açılmaz.
- **Kayıtlı ama şasesiz araç:** ruhsat istenir. Okununca boş şase dolar.
- **Kayıtlı, şaseli araç:** ruhsat yeniden istenmez (bugünkü gibi).
- Şase alanı kapalı akordiyondan **ana alana** çıktı. Devamın bağlı olduğu
  alan gizlenemez.
- Biçim: 17 harf/rakam. **I/O/Q yasaklanmadı:** doğrulanmış sette r08'in
  ruhsatında *"WOLO…"* basılı. Kural belgede yazanı esas alır.
- Prototipe şasesi okunamayan **üçüncü örnek ruhsat** eklendi. Kapı başka türlü
  görülemezdi.

**66.3 · Cemil onayına açık (varsayımla işlendi):**
- **(a) Elle şase.** Okunmuş şaseyi usta düzeltebilir; **okunmamış** şaseyi elle
  yazıp geçemez. *"okunmuyorsa devam etmemeli"* kelimesi kelimesine uygulandı.
  Ruhsat yanında yoksa ya da hiç okunmuyorsa iş durur. Gerçek hayatta bu
  olacak; o anda ne yapılacağı Cemil'in kararı.
- **(b) Okumadan sonra plaka değişirse** kaydedilen aracın plakası da değişir
  (yanlış okunan plakanın düzeltilmesi). Usta aynı ekranda bambaşka bir araca
  geçerse ilk kayıt yeniden adlandırılır. Ekran menüden her açılışta
  sıfırlandığı için bu ancak aynı ekranda kalınırsa olur.

**66.4 · Ölçümle bağı.** Şase artık devamın kapısı. Model seçiminde **şase
doğruluğu** ayrı sayılıyor (`foto-kos.js`: doğru · boş · YANLIŞ). Boş şase
ustaya yeniden çektirir; **yanlış şase** sessizce geçer ve parçacıya yanlış
araç gider. Bu yüzden yanlış şase, boş şaseden ağır.

**66.5 · Denetim:** 56. test genişledi (a: kayıtsız araçta ruhsatsız *"Kaydı
aç"* yok · b: şasesiz okumada araç kaydedilmez · c: şase yokken *"Kaydı aç"*
kayıt açmaz). Negatif doğrulandı: kapı hep açık → 2 bulgu, okumadaki şase
şartı kaldırıldı → 1 bulgu. 320/375/430 px'te 0 bulgu.

---

## K-065 · Ruhsat okunduğu AN araç kaydedilir — servis kaydı açılmasa da (18.10.2026)

> **Cemil, 18.10:** *"İlk ruhsatı okuduğumuzda, kayıt olarak almalıyız.
> Belki dükkana girişi olmayacak aracın, sadece parça soruyor. Fakat plaka
> sözlük için bizi gerekebilir."*

**65.1 · Ne değişti.** Eskiden araç yalnız *"Kaydı aç"*ta yazılıyordu.
Ruhsatı okunup servis kaydı açılmayan araç (yalnız parça soran) kayboluyordu.
Artık okuma bittiği an araç kaydedilir. Usta okumadan **sonra** bir alanı
düzeltirse düzeltme kayda da yazılır, yani kaydedilen şey ustanın son hâlidir.
Servis kaydı yine yalnız *"Kaydı aç"*la açılır.
`RUHSAT-OKUMA.md` §154'teki *"otomatik kaydedilmez"* kuralı **araç için**
bu kararla değişti. Alanlar yine ekranda görünür ve hepsi düzeltilebilir.

**65.2 · Neden doğru (Cemil'in gerekçesi ölçümle örtüşüyor).** Kayıtlı plaka,
sesli komutun plaka bağlamasında **aday listesidir** (K-059: imkânsız plaka en
yakın KAYITLI araca bağlanır, sesçe yakın plaka KAYITLI araçtan önerilir).
Kaybolan her ruhsat, motorun tanıyamayacağı bir plaka demekti. Aynı liste
Speechmatics'e ipucu olarak gider (`SOYLEYIS_IFADELER`, K-063).

**65.3 · İki sınır.**
- **Kayıtlı araç yeniden okunursa üstüne yazılmaz,** yalnız boş alanları
  dolar. Ustanın düzelttiği bilgi modelin yeni okumasına yenilmez.
- **Plaka okunamadıysa araç kaydedilmez** (gerçek setin r31'i). Plakasız kayıt
  yanlış araca bağlanır. Ekran *"plakayı yaz, araç öyle kaydedilir"* der.

**65.4 · Denetim:** 56. test `RUHSAT-ARAC`. Negatif doğrulandı: kayıt kaldırılınca
bulgu, düzeltme eşitlemesi kaldırılınca bulgu.

**65.5 · ~~Açık~~ KAPANDI (K-066: yalnız parça sor yolu YOK) — (A-M52):** *"Yalnız parça sor"* yolu. Bugün
parça sorma bir servis kaydına bağlı (`parca-sor` açık kaydın satırlarını
soruyor). Servis kaydı açmadan parça sormak **akış değişikliğidir**: kural
gereği önce akış, sonra ekran. Bu karar araç kaydını o yola hazır bıraktı.

---

## K-064 · Sıra: ölçüt doğrulama → geniş ekran → muhasebe backend'de (18.10.2026)

> **Cemil, 18.10:** *"Ölçüt doğrulama yapalım. Tasarımla ilgili bilgisayar,
> tablet alanlarını da hızlıca yapmalıyız. Sonrasında otomix'e göre muhasebe
> alanını hızlıca backend'de yaparız."*

**64.1 · Ölçüt doğrulama (A-M34) — durum ölçüldü:**
- **Parça listeleri bitti.** 27 listenin 27'si 16.09'da Cemil tarafından
  "doğru" işaretlendi (kontrol sayfasının veritabanı).
- **Ruhsatlar ~~başlamadı~~ BİTTİ (18.10): 46/46 doğru, 0 düzeltme** (Cemil, yerel
  sayfa). İki set de doğrulandı, TASLAK kalktı (`dogrulama` alanı). **A-M34 KAPANDI.**
  Ölçütü Claude'un okuması oluşturdu ve Cemil fotoğrafla tek tek onayladı. Bu,
  modelin değil Cemil'in onayladığı bir ölçüttür. Sıradaki A-M35 (model
  koşusu) para harcar, Cemil'in onayını bekler.
- *(eski not)* 46 belgede işaret yoktu. Ruhsat sayfası yayınlanmadı,
  çünkü içinde 46 kişinin TC numarası ve adresi var (16.09 güvenlik
  denetimi). Doğrulama **yerel sayfadan** yapılır:
  `Desktop\Tamiron\kontrol-ruhsat.html` (18.10'da yeniden üretildi; 73/73
  fotoğraf bağlı). Fotoğraflar ve kişisel veri bilgisayardan çıkmaz.
- **İş:** önce "Şüpheli alan var" süzgeciyle `guven` notlu 14 belge, sonra
  kalan 32 belge. Düzeltmeler "Düzeltmeleri indir" ile tek dosya olarak çıkar.
  Lider o dosyayı `ruhsat-seti.json`'a işler ve setten TASLAK damgasını
  kaldırır.
- **Sonra A-M35:** üç model aynı sette koşar (A-M30). Her koşu gerçek para
  harcar, Cemil onaylamadan koşturulmaz.

**64.2 · Geniş ekran — tablet ve bilgisayar.** MIMARI zaten "mobil · tablet ·
bilgisayar" diyor. Bugünkü prototip yalnız telefonda ölçülüyor (320/375/430).
Yeni özellik değil, **aynı ekranların geniş düzeni**. Kurallar:
- Akış ve ekran sayısı değişmez (A-2 bütçesi).
- Akış → düzen taslağı → tartış → ekran (CLAUDE.md tasarım-önce kuralı).
- Denetim 768 ve 1280 px'e genişler. Telefon genişliklerinde 0 bulgu korunur.
- **Cemil 18.10 (taslağa cevap):** *"1 sabit, 2 kalsın"*:
  bilgisayarda menü solda **sabit**; tablet dikeyde kayıt listesi **ayrı ekran
  olarak kalır**. Taslak: https://claude.ai/artifact/2CDjYiKzHLdwVewYxxmn3E
- **Prototipte (18.10 akşam):** `stil/04-genis.css`, yalnız @media içinde.
  Ana ekran bilgisayarda sol sütunda hep görünür (ikinci bir menü
  yazılmadı); kayıt iki sütun; listeler iki sütun. Denetim artık 768 ve
  1280 px'te de koşuyor (57. test). **Ertelenen:** liste ile açık kaydı
  AYNI ANDA göstermek, çünkü ekran sistemi tek ekran gösteriyor; bu ayrı bir
  mimari adım ve Cemil onayına açık.
- **Cemil 18.10 gece: *"4 evet"*** — yan panel kuruldu. Bilgisayarda kayıt
  açıkken liste, menü ile kayıt arasında durur (300 | 380 | kalan). Liste
  ikinci kez yazılmadı; listenin kendi ekranı çekirdekteki `yanTazele` ile
  `yan` işaretiyle çizilir. `.active` tek kalır. Açık kayıt listede
  işaretli (`aria-current`). 1.200–1.499 px'te kayıt sütunu daraldığı için
  iç düzen tek sütun, 1.500 px üstünde iki sütun. 57. test genişledi,
  negatif doğrulandı; 320/375/430/768/1280/1600 px'te 0 bulgu.

**64.3b · Cemil 18.10 gece: *"Muhasebe soruları tamam."*** Öneri
(`arastirma/MUHASEBE-KAPSAM-OTOMIX.md`) belgedeki varsayımlarla kabul edildi:
- "OtoMix'e göre" = görünüm (modül tahtası), kapsam değil;
- Faz 1 = prototipte çalışanı backend'e taşı (kasa · gider · alacak · iş
  başına kâr · CSV; KDV/tevkifat için veri modelinde yer);
- Faz 2 = gelen fatura fotoğrafı → alış + parçacıya borç;
- Faz 3 = fatura kesmek, önce soru;
- banka, çek, stok, tam cari ve POS dışarıda; tek rol.
**Backend fazı bununla açılabilir** (CLAUDE.md faz satırı geçiş günü güncellenir).

**64.3 · Muhasebe backend'de, OtoMix'e göre.** Referans:
`arastirma/OTOMIX-HARITA.md`, `MUHASEBE-KAPSAMI.md`, `KDV.md`,
`MALIYET-VE-KAR.md`. Bu madde **faz geçişidir** (CLAUDE.md: *"kod/backend
YOK"*). Geçiş başladığı gün CLAUDE.md'nin faz satırı ve MIMARI ile birlikte
güncellenir. Önce kapsam (OtoMix'in altı modülünden hangileri — T-18 ve
"yeni özellik varsayılan HAYIR" süzgeciyle) Cemil'e tek sayfada sunulur.
Kod ondan sonra.

---

## K-063 · Ses ölçüm dönemi kapandı; söyleyiş sözlüğü üç yerde AKTİF (18.10.2026)

> **Cemil, 18.10:** *"Bu kısmı artık bitirelim. … Hataları düzeltecek,
> kelimeleri tanıyacak sözlüğü aktif kullanmalıyız."*

**63.1 · Dönem kapandı (A-M48).** Son durum `arastirma/SES-OLCUM.md`
§11–17.3: 300 gerçek kayıt + 222 yapay şiveli kayıt. Sekiz yapay seste
Chrome %73 · Speechmatics %75 · **kademe (b) %79, paralıya %27**. Belirleyici
olan şive değil **hız** (~2,5 kelime/sn üstü %54–70). Yeni ölçüm **yok**;
sıradaki veri yayındaki gerçek usta kayıtlarından gelir (K-060).

**63.2 · Sözlük tek kaynak, üç kullanım** (`betik/05-soyleyis.js`):
1. **Motor düzeltir — bugün aktif.** Her cümle `sesSoyleyisDuzelt`'ten
   geçer ("selise" → C-Elysée, "egzos" → egzoz, "yapıyom" vb.).
2. **Tanıyıcıya ipucu — backend fazında aktif.** Kademe (b)'de paralıya
   giden ses `SOYLEYIS_IFADELER(dükkânın araçları)` ile gider (K-061, K-062).
   Chrome ipucu kabul etmiyor (SES-OLCUM §16).
3. **Büyür — backend fazında.** K-060 kayıtlarında ve onay ekranında ustanın
   düzelttiği her *"duyulan → doğrusu"* çifti sözlüğe **aday** olur
   (K-054 ① düzeltme birikimi).

**63.3 · ~~Açık~~ KAPANDI — Cemil 18.10: *"Onaylı girişi kabul edelim. Ama
bunu sistematik yapmak gerek."*** Süreç: **`docs/SOYLEYIS-BAKIM.md`** (üç
kanal · eşik · haftalık döngü · regresyonda bozulan cümle SIFIR şartı · ret
listesi · geri alma). Özü: aday listesi biriktirilir, lider haftada bir gözden
geçirir, Cemil onaylar. Neden otomatik değil: yanlış bir eşleme o kelimeyi
içeren HER cümleyi bozar (Tur 8.4'te bulanık eşleme "hava filtresi"ni
"Yağ filtresi" yaptı). Dükkâna özgü sözcük (müşteri adı, takma ad)
yalnız o dükkânın sözlüğüne girer.
**Ek (Cemil 18.10): pilot döneminde GÜNLÜK, eşik ≥ 2.** Cemil sistemi birkaç
tamirciye test için vermeyi düşünüyor; başta sözlük hızlı büyümeli. Sınavlar ve
onay aynen kalır (SOYLEYIS-BAKIM §4.1). Normal döneme geçiş Cemil'in kararıdır.

**63.4 · Ustaya doğrulatma duruyor:** `arastirma/SOYLEYIS-SOZLUGU.md`
(üretilir: `node tools/soyleyis-liste.js`) Cemil/ustanın gözden geçirmesini
bekliyor — eksik söyleyiş en ucuz kazanç.

---

## K-062 · Kademeli ses tanıma: önce ücretsiz + motor, gerekirse paralı (17.10.2026)

> **Cemil, 17.10:** *"Motoru speechmatics'i minimum kullanacak şekilde
> tasarlayabilir miyiz? Yani önce motor, sonra paralı model gibi?"*

**62.1 · Yapı:** Chrome tanır → motor anlar → motor emin değilse aynı ses
Speechmatics'e (ipuçlu) gider. Ölçüldü (300 kayıt, canlandırma —
PARALI-TANIMA "Kademeli tanıma"): sınavda **%79, paralıya %7** = tamamen
Speechmatics'in doğruluğu, maliyetin 1/14'ü.

**62.2 · ~~Açık~~ KAPANDI — Cemil 17.10: *"Tamam b'de karar kılalım."* (A-M51) → (b): öneri soruyorsa YA DA anlaşılmadıysa paralıya.** Seçenekler:
- (a) yalnız motor ÖNERİ soruyorsa — geliştirmede seçilen kural; sınav %79,
  paralıya %7;
- (b) öneri soruyorsa YA DA anlaşılmadıysa — sınav %84, paralıya %25;
  geliştirmede (a) ile eşitti, sınavda öne çıktı (sınava bakılarak seçmek
  ölçümü kirletir — bu yüzden karar Cemil'in, rakam açıkça yazıldı).
Maliyet ikisinde de dükkân başına ayda kuruşlar (~$0,03 / ~$0,11).

**62.3 · Backend fazında:** tarayıcı tanırken sesi aynı anda kaydeder;
paralı katman backend'den (anahtar tarayıcıda durmaz). Bu fazda kod yok.

---

## K-061 · Ses tanıyıcı adayı: Speechmatics + ipucu; Google/Azure backend fazında (16.10.2026)

> **Cemil, 16.10:** Google Cloud'un Türkiye hesabı için istediği bir kerelik
> **1.500 TL ön ödemeyi** gördükten sonra: *"Evet, 1 olarak devam edelim."*

**61.1 · Aday:** **Speechmatics Enhanced + kelime ipucu** (dükkânın plakaları,
marka/modelleri, terimler — `SOYLEYIS_IFADELER()`). 300 gerçek kayıtta
(PARALI-TANIMA "İlk tur"): sınav %79 (Chrome %72), yanlış 33 → 20, plakası
doğru duyulan 55 → 82/127. Dükkân başına ayda ~$0,45. AssemblyAI elendi
(Türkçede ipucu etkisiz).

**61.2 · Bu fazda değişen yok.** Prototip Chrome'un ücretsiz tanıyıcısıyla
kalır; paralı tanıyıcının anahtarı tarayıcıda duramaz (YAPAY-ZEKA §4) →
backend fazı (K-055.1).

**61.3 · Google Chirp 3 ve Azure ölçülmedi — backend fazında.** Google,
Türkiye hesabını etkinleştirmek için **bir kerelik en az 1.500 TL ön ödeme**
istiyor (ücret değil, bakiye; ölçüm ~20 TL tutardı). Ölçüm için parayı
aylarca kullanılmayacak bir hesaba kilitlemek gereksiz bulundu. Gerçek
kullanım başlayınca ön ödeme harcanacak bakiye olur; o zaman Google ve
Azure aynı 300 kayıtla (`parali-tanima-kos.js`) Speechmatics'le
karşılaştırılır. Google projesi açık, ücretsiz bekliyor.

**61.3b · Azure ELENDİ, Google'ın ön ödemesiz yolu denenir (Cemil, 16.10):**
*"Azure pahalı, onu eleyelim. Fakat Google'ı merak ediyorum. Onu denemenin
bir yolu yok mu? Fark yoksa speechmatics ile devam ederiz."* Yol: Gemini
API'nin **gemini-3.5-transcribe** modeli (Google AI Studio anahtarı, ücretsiz
katman, Türkiye listede, ön ödeme yok; ipucu `customVocabulary`). Chirp 3
değil ama Google'ın tanıyıcısı. ⚠️ Ücretsiz katman içeriği Google'ın ürün
geliştirmesinde kullanılabilir — yalnız test kayıtları; yayında değil.

**61.3c · Gemini 3.5 Transcribe ELENDİ (16.10):** Cemil 250 TL ön ödemeyle
denetti. İpucu listesi verilince **söylenmemiş ama kayıtlı plaka yazıyor**
(52 kayıtta 4, %7,7) — iş sessizce yanlış araca gider, motorun hiçbir
koruması yakalayamaz. Speechmatics aynı listeyle 300 kayıtta 0. Kural:
**tanıyıcı karşılaştırmasında "söylenmemiş kayıtlı plaka" sayısına
doğruluktan ÖNCE bakılır** — sıfır olmayan tanıyıcı aday olamaz.

**61.4 · Motor tanıyıcıdan bağımsız kalır.** Yazım biçimi toparlama
(16.10) hangi tanıyıcı seçilirse seçilsin çalışır; ölçüm koşucusu ve cetvel
hazır — tanıyıcı değişince yalnız bir koşu.

---

## K-060 · Yayında ses kayıtları SAKLANIR — yanlış da olsa (16.10.2026)

> **Cemil, 16.10:** *"Ses kayıtlarını yanlış da olsa saklayalım, böylece
> nerede sorun olduğunu tespit edebiliriz, bunu yayına çıkan proje için
> yazıyorum."*

**Kapsam genişletmesi Cemil'den** — K-053'ün (*"izde DEĞERLER yazılır …
kullanıcı problem yaşadığında arayacağı kişi olacağım"*) doğal devamı.
Backend fazında yapılır; bu fazda kod yok.

**60.1 · Saklanan DÖRTLÜ, tek ses değil.** Hatanın DUYMADA mı ANLAMADA mı
olduğunu yalnız dördü birlikte söyler (16.10'daki 300 kaydın çözümlemesi
tam bu dörtlüyle yapıldı — SES-OLCUM §14):

| | ne |
|---|---|
| 1 | ses dosyası |
| 2 | tanıyıcının yazdığı metin |
| 3 | motorun anladığı işlem (JSON) |
| 4 | ustanın onay ekranında düzeltip onayladığı SON hâl (ya da "tekrar söyle") |

4 ile 3'ün farkı = motorun hatası; 2 ile sesin farkı = tanıyıcının hatası.
**Ustanın düzelttiği son hâl kendiliğinden doğru cevaptır** — elle etiket
gerekmeden büyüyen bir değerlendirme seti.

**60.2 · ~~Açık~~ KAPANDI — Cemil 16.10: *"Gizlilik notu ve süre'de tamamız."* (A-M50)** — not metni ve 90 gün aşağıdaki gibi; prototipte `GIZLILIK_CUMLESI` güncellendi.
- **Gizlilik notu** bugün *"internet üzerinden GÖNDERİLİR"* diyor; *"…ve
  SAKLANIR"* demeli. Önerilen metin: *"Söyledikleriniz ve çektiğiniz
  fotoğraflar yazıya çevrilmek üzere internet üzerinden gönderilir; hatalı
  anlaşılanları düzeltebilmemiz için kayıtlar saklanır."*
- **Süre** — K-053.4'ün düzenine göre öneri: ses + metin AYRINTI olarak 90
  gün, sonra yalnız istatistik (hangi iş, hangi alan, ne sıklıkla yanlış).
  Ses 90 günden sonra silinir.
- Seste MÜŞTERİNİN adı ve telefonu geçiyor (üçüncü kişi verisi); K-053
  bunu zaten kabul etti — not olarak.

---

## K-059 · Sesli komutta plaka: Türk plaka kuralları + "karma" bağlama (14.10.2026)

> **Cemil, 14.10:** *"Türkiye'de Q harfi yok. Plakaların bazı kuralları
> var. Bunları da dahil edebiliriz."* — ve yanlış duyulan plaka için üç
> yoldan **"Karma"**yı seçti.

**59.1 · Plaka kuralları motorda.** İl kodu **01–81** · harfler Türk
alfabesinin **23** harfi (Ç Ğ İ Ö Ş Ü, Q W X **yok**) · harf sayısı rakam
sayısını sınırlar (1→4–5, 2→3–4, 3→2–3). Tanıyıcının Türkçe yazdığı harf
düzeltilir. Kendi verimiz kurala aykırıydı: tohum **16 XY 8890 → 16 SY
8890**; değerlendirme seti sürüm 2.

**59.2 · Karma bağlama — K-047.6'yı daraltır, kaldırmaz.**

| Duyulan plaka | Davranış |
|---|---|
| **Gerçek olamaz** (Q/W/X'li, "35 kelime 45") | Tek harf uzaktaki TEK kayıtlı plakaya **bağlanır**; onay ekranı *"Duyulan … geçerli bir plaka değil — en yakın kayıtlı araç … alındı"* der |
| **Gerçek olabilir** ama sesçe yakın ("06 PVR 07") | **Bağlanmaz — SORULUR:** *"Bunu mu demek istedin: 06 TMR 07"* (K-047.6) |

**Gerekçe:** K-047.6'nın korktuğu şey yanlış araca mesaj gitmesi.
Gerçek olamayan bir plaka için o risk yok — yeni araç ihtimali sıfır.
Gerçek olabilecek plakada risk aynen duruyor; orada usta seçer.

**59.3 · Hata — kayda geçsin.** 14.10'da ilk iyileştirme (§11.4) gerçek
olabilecek plakayı da kendiliğinden bağlıyordu ve K-047.6 ile çatışması
Cemil onaylamadan ÖNCE söylenmedi. Lider kararlar defterini kontrol
etmeden davranış değiştirdi. Ders: **ses/plaka davranışına dokunmadan önce
K-047.6 ve A-M27 okunur.** Test 55 artık iki yönü de ölçüyor (bağlanması
gereken ve SORULMASI gereken).

**Ölçüm (sürüm-2 set, dökümler):** yazı %93 · telefon %82 kendiliğinden
(+2 doğru öneri → tek dokunuşla ~%87) · bilgisayar %71 (+2 → ~%76).

---

## K-058 · Servis detayı: adım yok — "Ne oldu" + alttaki eylem çubuğu (12.10.2026 · Tur 8.1)

> **Cemil, 12.10** (dört kurguyu yan yana gördükten sonra): *"Zaman
> çizelgesinin Ne oldu alanı olsun. Eylem merkezli olanın altta
> buttonlarının olması olacak. Fakat burada ses ve kamera buttonları da
> gerekli. Teslim alanı burada olmayabilir."*

A-M49 kapandı. Cemil **önerimi (B) seçmedi, birleşim seçti** (D + C) —
ve gerekçesi ekranın kendisinde: B'nin üstünlüğü satır düzenindeydi, o
zaten bugünkü tabloda var; Cemil'in istediği, **işlerin elin altında**
durması ve kaydın **hikâyesinin** okunmasıydı.

### 58.1 · Ne yapıldı

| | önce | sonra |
|---|---|---|
| Yapı | 3 adımlı sihirbaz (İş · Yapılanlar · Teslim) | **tek sayfa:** künye → Yapılanlar (+ toplam + Teslim) → **Ne oldu** → Açıklama → Parça bekliyor |
| İşler | adımlara dağılmış düğmeler | **alttaki yapışkan çubuk:** Ekle · **Sesle** · **Fotoğraf** · Parça sor · Mesaj |
| Ekle | tabloya boş satır | **panel** (13.10 — C'deki gibi): Parça/İşçilik · ad · tutar (· alış) · Ekle |
| Teslim | 3. adımın içinde | **çubukta değil** — **"Ne oldu"nun son adımı ("Teslim bekliyor") teslimin kapısı** (13.10) |
| Süreç | "Süreç" kartı | **"Ne oldu"** — aynı ortak nesne (`surecHtml`), yeni çizim yazılmadı |

### 58.2 · Ölçüldü — gerçek prototipte, aynı görevler (`tools/servis-detay-olc.js`, 320 × 760)

| | satır ekle | tutar | teslim | ilk karede satırlar | ilk karede teslim |
|---|---|---|---|---|---|
| önce (3 adım) | 2 | 1 | 4 | yok | yok |
| 12.10 ilk kurulum | 2 | 1 | 3 | var | yok |
| **13.10 düzeltilmiş** | **3** | **1** | **3** | **var** | yok |

Satır eklemek bir dokunuş uzadı (panel); bedeli bilerek seçildi —
panelde işçilik de doğrudan ekleniyor.

Çubuk düğmeleri 320 px'te 53 × 64 px; taşma 0. Ses eşdeğerliği:
tanıma 568/0 · onay ekranı 392/0 (uygulamadaki farkların hepsi K-057'nin
alış değişikliği).

### 58.2b · 13.10 — kendi okumamı yeniden kontrol ettim (Cemil istedi)

Cemil cümlesini tekrar kontrol etmemi istedi. Karşılaştırma sayfasının
kendi kaynağına bakınca üç sapma çıktı ve Cemil üçünü de cevapladı:

| sapma | 12.10'da ben | Cemil, 13.10 |
|---|---|---|
| D'nin *"Ne oldu"* alanında **satırların kendisi** vardı; ben eski süreç kartının adını değiştirmiştim | tablo + süreç | **"İkisi de kalsın"** — bugünkü hâl doğru |
| C'de "Ekle" bir **panel** açıyordu; ben boş satır bırakıyordum | boş satır | **"C'deki gibi panel aç"** |
| *"Teslim alanı burada olmayabilir"*: "burada"yı çubuk diye okuyup teslimi tablonun altına koymuştum | tablonun altında düğme | **"Ne oldu'nun son adımında"** |

**Ders:** birleşim isteyen bir cümlede her parçanın **neyi taşıdığını**
kaynağından kontrol etmeden kurmamak. "Ne oldu alanı" bir başlık değil,
içeriğiyle bir alandı; "altta buttonlar" yalnız düğmeler değil,
davranışlarıyla düğmelerdi.

### 58.2c · 13.10 öğle — Cemil telefonda baktı (ekran görüntüleriyle)

> *"Yapılanlar altında başlıklar static, aşağıya doğru divin de dışına
> çıkıyor. Parça / İşçilik belirtmeye gerek. Renkleri farklı olsun, yeter.
> Hatta İş + Tutar başlığı da olmasın. … tarih, altında saat olsun.
> Teslim et burada olmasın. Şimdi gönderilebilir de orada olmasın. Teslim
> et, parça bekliyor vb aynı benzer işlemler."*

| istek | yapılan |
|---|---|
| başlık kartın dışına taşıyor | **tablo başlığı yapışmıyor — BÜTÜN tablolarda** (muhasebe ×2, iş başına da aynıydı); kayıtta başlık satırı **yok** |
| PARÇA/İŞÇİLİK yazısı | **renkli nokta:** parça mavi (`--primary`), işçilik turuncu (`--highlight` — uyarı rengi değil). Nokta hâlâ düğme (tür değişir); anlamı Ekle panelindeki çiplerde aynı noktalarla öğreniliyor |
| saat tarihin altında | `surecHtml`'de tarih / saat iki satır, sağ sütun dar |
| teslim ve öneriler "Ne oldu"da olmasın | "Ne oldu" **yalnız geçmiş**; teslim · parça bekliyor · şimdi gönderilebilir **İşlemler** kartında bir arada |

⚠️ **Cemil onayına açık:** *"aynı benzer işlemler"*i **"bir arada dursunlar"**
diye okudum. *"Alttaki çubukta dursunlar"* da olabilir — çubuk 5'ten 7
düğmeye çıkardı ve 320 px'te okunmazdı.

**Yolda çıkan üç şey:**
1. **Sınıf çakışması testi kuralların YARISINI görmüyordu** — desen her
   eşleşmede bir önceki kuralın `}`ini tüketiyor, kuralları birer atlayarak
   okuyordu. Tek bir kural eklenince sıra kaydı ve aylardır duran iki
   gerçek çift tanım çıktı (`.kalem-list` birebir kopya · `.ico` tabanda
   alt küme). Test düzeldi, çiftler silindi. **Yalancı geçen test.**
2. **Araç ekranında borçlu kaydın satırı 22 px** (`role="button"`,
   K-033'ten beri). Denetim yalnız veresiyeli bir araç çizili kaldığında
   görüyordu — kapsamı tesadüfe bağlıydı. Düğme gibi davranan satır artık
   48 px.
3. **Çubukta "Fotoğraf" → "Fotoğr / af"** (kelime ortadan bölünüyordu).
   Kelimeler bütün kalıyor; etiket Cemil'in kelimesiyle **"Kamera"**.

**54. test (KART-ICI-YAPISKAN):** kartın içinde, kendi kaydırma kabı
olmayan yapışkan öğe → bulgu. Negatif: başlığa yapışkanlık geri kondu →
6 bulgu (muhasebe 4, iş başına 2); kalkınca 0.

Ölçüldü (320 px): dokunuş 3/1/3 · satırlar ilk karede · denetim 54 test
0 bulgu 320/375/430 — ilk koşu ve art arda iki koşu.

### 58.3 · Varsayımlar — Cemil onayına açık (13.10'da güncellendi)

- **Satır düzeni dokunulmadı:** tablo yerinde düzenleniyor (ad · tutar ·
  alış · rozet · kaydırarak silme). C'nin *"satıra dokun → panel"*
  kısmı alınmadı; Cemil yalnız alttaki düğmeleri istedi.
- ~~"Ekle" PARÇA ekler~~ → **13.10: panel** (Cemil).
- **Teslim kapısı ilk karede değil:** "Ne oldu", Yapılanlar'ın altında.
- **Panelden eklenen satıra odak verilmiyor**, yalnız ekrana kaydırılıyor —
  telefonda klavye yeniden açılmasın; yazılacak her şey panelde yazıldı.
- **Klavye:** `role="button"` taşıyan öğeler (künye, mesaj adımları,
  teslim kapısı) artık Enter/Boşluk ile açılıyor — üçü de açılmıyordu;
  kural çekirdekte, yapıya bağlı.
- **"Mesaj"** eski *"Özet gönder"*in yerinde: mesaj ekranını özet
  şablonuyla açar; orada başka şablon seçilir.
- **Teslim ilk karede değil** — künye kartı (müşterinin büyük adı ve
  telefonu, Cemil 07.09) 320 px'te ilk karenin yarısını kaplıyor.
  Künye sıkıştırılırsa teslim de ilk kareye girer; **sormadan
  dokunmadım.**
- **Gözle kontrol yapılamadı:** önizleme bölmesi bu oturumun sonunda
  0 × 0 açıldı; bütün düzen ölçümleri sabit boyutlu çerçevede yapıldı.
  Telefonda ilk bakış Cemil'in.

### 58.4 · Yeni hata sınıfı → 53. test (YAPISKAN-ALT)

Çubuk ilk yazışta dipten **26 px yukarıda** duruyordu: `sticky`, kaydırma
kabının iç dolgusunu hesaba katıyor. Altından içerik akıyordu; hiçbir
şey taşmadığı ya da kesilmediği için hiçbir test görmedi — ölçüm
yakaladı. Test her ekranda alttan yapışan öğenin kabın dibine oturduğunu
ölçüyor; negatif testle doğrulandı (düzeltme geri alındı → 1 bulgu).

---

## K-057 · Bilinmeyen alış kâra girmez; ayarlar kendi ekranında (12.10.2026 · Tur 8.2)

K-055.2'nin uygulaması. Üç parça; üçü de Cemil'in kararından türüyor,
yeni kapsam yok.

### 57.1 · `alis: null` ≠ `alis: 0` — ve satış da hesaptan çıkar

| değer | anlamı | kâr hesabında |
|---|---|---|
| `null` | **bilinmiyor** (yazılmamış) | satır **satışıyla birlikte** dışarıda; ekran kaç satır olduğunu söyler |
| `0` | bedava aldım | alış 0 — satış kârdır |
| sayı | parçacıya ödenen | satıştan düşülür |

⚠️ **Yalnız alışı sıfır saymamak yetmezdi** — satış fiyatı farka girmeye
devam eder ve şişme aynen sürer. Bu yüzden satır bütünüyle dışarıda.
Eksik bir rakam yanlış bir rakamdan iyidir (K-013); ama eksik olduğu
**söylenir**: iş başına ekranında satırın altında *"2.100 hesap dışı"*,
altta bir not ve — ayar kapalıysa — çözümün kendisi (*"Alış fiyatını
sor"* → ayarlar, K-021).

**Ölçüldü (aynı veri, eski ve yeni formül):**

| | eski | yeni |
|---|---|---|
| 7 no'lu kayıt (silecek motoru, alışı yazılmamış) | 2.800 ₺ | **700 ₺** (yalnız işçilik) |
| bu ay | 2.800 ₺ | 700 ₺ |
| bu yıl | 7.410 ₺ | **5.310 ₺** — elle: 1.250 + 1.350 + 700 + 1.000 + 1.010 |

**Tohum veri değişti, bilerek:** silecek motorunun alışı boşaltıldı.
Tohumdaki her parçada alış dolu olunca kusur hiç görünmüyordu
(MALIYET-VE-KAR §1 — ölçüm aletinin verisi eksiği gizliyordu). İşçilik
ve müşteri parçası satırları `0` → `null` (alış kavramı yok). Yeni satır
açan her yol (elle · fotoğraf · ses · tip çevirme) artık `null` yazıyor.

### 57.2 · Alış alanı — ayara bağlı, zorunlu değil

Kayıt ekranında, **yalnız ayar açıksa** ve **parçayı tamirci aldıysa**,
parçanın altında küçük bir *"alış ___ ₺"* alanı. Boş geçilir, hiçbir şey
uyarmaz. Girişte boş → `null`, *"0"* → `0` (tutar alanlarının `|| 0`
alışkanlığı buraya sızmasın). Ayar kapalıyken alan **ve** eski
*"parçacı 1.450 ₺"* yazısı görünmez — sormayan dükkân alışı hiç görmez.
320 px'te ölçüldü: alan 84×48, taşma 0.

### 57.3 · Ayarlar = 17. ekran; çekmecede tek satır

Dört sakin: bakım periyodu · **parça alış fiyatı** · tema · gizlilik.
İkisi çekmeceden **taşındı, kopyalanmadı** — iki yerde duran ayar iki
ayrı cevaptır (K-019.1). Ana menü altı satır kaldı (A-M8).

Seçim bir anahtar değil **iki çip** (*Sorulmasın / Sorulsun*): teknolojiden
uzak kullanıcıya bir anahtarın hangi yönünün "açık" olduğunu sormak
yerine iki kelime (T-18); bakım periyoduyla aynı bileşen.

**Gizlilik (A-M33):** ayarlarda tek satır + **ilk kullanımda bir kez**
not — ses ya da ruhsat ekranı, hangisi önce açılırsa. Cümle ve "bir
kez" kuralı tek yerde (`gizlilikIlkAcilis`, 03-nesne).

### 57.4 · Varsayımlar — Cemil onayına açık

- **Varsayılan KAPALI.** K-055.2 *"alış soran dükkân açar"* diyor.
- **Prototipte ayar kalıcı değil** — öteki veri gibi açılışta tohuma
  döner; `DUKKAN` içinde durduğu için denetim onu yedekleyip geri
  yüklüyor. Tema ayrı: o **cihazın** tercihi, `localStorage`'da kalıyor.
- **Gizlilik cümlesi:** *"Söyledikleriniz ve çektiğiniz fotoğraflar,
  yazıya çevrilmek üzere internet üzerinden gönderilir."* — öneriden
  biraz uzatıldı (*"işlenmek"* jargondu).

### 57.5 · 52. test (ALIS-BILINMIYOR)

Hesap (yapay kayıtla: bilinmeyen 700 · bedava 2.800 · bilinen 1.200) ·
ayar ekranı gerçekten değiştiriyor mu · giriş boş/0 ayrımını koruyor
mu. **Üç negatif test:** eski formül → 2 bulgu · `|| 0` girişi → 1 bulgu
· ayarı yok sayan alan → 1 bulgu; geri konunca 0. Testin kendi
kirliliği de düzeltildi: kayıt ekranı "ayar açık" hâlinde çizili
kalıyordu (K-041.2 — DOM da geri bırakılır).

---

## K-056 · Ses işi tanımaz; işin sahibi ekran şemasını bildirir (12.10.2026 · Tur 8.3)

> **Cemil, 12.10:** *"Kaydedilen ses bir json dolduracak. Bu ikisi
> eşleşmeli. Hangi sayfada olduğuna göre bu json'u dolduracak ve işlem
> yapacak."*

### 56.1 · Karar: şema, sahibi olan ekranda

Her sesli iş **bir şemadır** ve şemayı işin **sahibi olan ekran** bildirir
(`Islem.tanim`, çekirdek `betik/04-islem.js`). Şema tek yerde dört şeyi
söyler: **alanlar** (JSON'un kendisi, tipleriyle) · **tanıma** (genel yolda
ve kendi bağlamında) · **onay ekranının sözleri** · **uygula**.

| | önce | sonra |
|---|---|---|
| Ses ekranında işe özgü dal | çözümleyicide 15 `return`, onay ekranında 12, uygulamada 12 `o.tur ===` | **0** |
| Yeni iş eklemek | üç yerde üç dal | **bir şema**, sahibinin dosyasında |
| "Bunu mu demek istedin" (7.3) | yalnız kayıt ekranı için elle | **varsayılan işi olan her bağlam** için aynı yol |
| Onay ekranı | iş başına elle çizilmiş | şemadan; `sor: true` alan **soru** olur (çip / kutu) |

12 şema · 7 ekran: kayıt (satır-ekle · fiyat-güncelle · servis-aç) ·
teslim · araç · ruhsat (yeni-araç · bilinmeyen-araç) · mesaj (tekli ·
toplu) · muhasebe (tahsilat · gider) · araçlar (ara).

**Neden ekranda, ses ekranında değil:** ortak nesne kuralı. Teslim
işinin ödeme seçimi teslim ekranının durumudur; eskiden ses ekranı o
ekranın düğmesine **dışarıdan tıklıyordu.** İşi bilen yer, işi yapan yer.

### 56.2 · Davranış DEĞİŞMEDİ — ve bu ölçüldü

Yeniden yapılandırma bir özellik değildir; usta bir fark görmemeli.
Eski ve yeni derleme yan yana yüklenip karşılaştırıldı
(`tools/ses-esdeger.js`, kalıcı araç):

| ölçüm | durum | fark |
|---|---|---|
| işlem nesnesi (cümle × 8 bağlam) | 568 | 0 |
| onay ekranı (metin · düğme · seçim · kutu) | 392 | 0 |
| uygulama (veri · ekran · balon · kayda dönüş) | 16 iş | 0 |
| değerlendirme seti | 40 | **%93 → %93** |

Araç negatif testle doğrulandı (öncelik bozuldu → 18 fark; gider şeması
bozuldu → 30 + 10 fark). ⚠️ **Sınırı:** toplu/tekli mesaj önceliğini
takas etmek **0 fark** verdi — derlemde plakalı toplu mesaj cümlesi yok.
0 fark *"aynı"* değil, *"bu derlemde aynı"* demektir.

### 56.3 · Bilerek taşınan iki kusur

- `satir-ekle` hâlâ `alis: 0` yazıyor → **8.2'nin işi** (K-055.2).
- Söylenmeyen tutar `0` (null değil) — bugünkü bütün şemalar 0'ı
  *"söylenmedi"* okuyor. Satış tutarında null/0 ayrımı 8.2'de bakılacak.

### 56.4 · Bulunan iki hata sınıfı

1. **Derleyici betiği bozuyordu ve 7. denetimi "temiz" diyordu.**
   `replace(yer, js)` dizgedeki `$'` kalıbını *"eşleşmeden sonrası"*
   diye yorumladı; 04-islem.js'teki bir düzenli ifade dizgesi tam bu
   kalıbı taşıyordu. Denetim yerleştirmeden **önceki** betiği ölçüyordu.
   Şimdi yerleştirme fonksiyonla, denetim **üretilen dosyadan** —
   negatif testle doğrulandı. Bugüne kadar tutması tesadüftü.
2. **Tanıyan yol ile yapan yol ayrışabiliyor.** Şemayı kayda düşüren
   satırı unutmuştum: cümle doğru anlaşıldı, onay ekranı şemayı bulamadı
   ve çöktü. Eşdeğerlik aracı yakaladı, denetim değil → **51. test
   (ISLEM-SEMA)**, negatif testle doğrulandı. Ses ekranı da artık
   şemasız işi *"Anlayamadım"* sayıyor (güvenli yön: hiçbir şey yapma)
   ve izine yazıyor.

### 56.5 · Backend köprüsü

`ISLEM_SEMALARI()` aynı 12 şemayı araç tanımı biçiminde veriyor (`name`
· `description` · `input_schema`). Model geldiğinde değişen yalnız
**tanıyıcıdır**; alanlar, onay ekranı ve uygulama aynı kalır (K-054 ①:
model doldurur, usta onay ekranında düzeltir).

⚠️ **Cemil onayına açık — 8.1'le kesişme:** kayıt ekranının şeması
ekranın **işlerine** bağlandı, düzenine değil (satır ekle · fiyat
güncelle · servis aç). Servis detayı hangi kurguyla yeniden kurulursa
kurulsun bu işler aynı kalıyor; kurgu yeni bir iş getirirse o iş bir
şema olur.

---

## K-055 · Bekleyen on beş madde kapandı; bir ayar sayfası ve iki yeni iş açıldı (12.10.2026)

> **Cemil, 12.10:** *"Öncelikle kararlar. Ardından sunumu tekrar gözden
> geçirelim… Backend'e yolumuz var henüz."*

3,5 haftalık aradan sonra açık maddelerin tamamı cevaplandı. İki yerde
Cemil önerimden saptı; **ikisi de önerimden iyiydi** ve ikisi de aynı
sebepten: *benim önerim ustaya iş yüklüyordu.*

### 55.1 · Backend bu fazda açılmıyor (A-M31) — ve bedeli

Karar: **hayır.** Bu, veriyi bize **ulaştıran** her şeyi backend turuna
taşır: A-M40 (iz), A-M42 (çıkarım sayfası), A-M46 (parçacı ağı),
A-M47 (düzeltmeler).

**Bedeli açık yazılıyor, kazara ertelenmiş sayılmasın:** K-054 *"①
düzeltme birikimi bugün başladı"* diyor. Bugün tam doğru değil —
`Iz.okuma()` ustanın düzeltmesini yakalıyor ama dükkânda kalıyor.
**Sayaç kuruldu, saymaya başlamadı.** Birikimin başlangıç tarihi,
backend turunun tarihidir.

### 55.2 · Alış fiyatı ZORUNLU DEĞİL — ve ayarlar sayfası açıldı (A-M43)

> **Cemil:** *"Alış fiyatı girilmeyebilir. Zorunlu ya da beklenen alan
> olmamalı. Hatta bunun için bir ayar alanı olabilir. Hatta müşteri
> kullanım için ayar sayfamız olsun. Tercihleri / ayarları burada
> saklayalım."*

Önerim *"parçacı ne dedi"* + *"müşteriye kaç yazıyorsun"* iki alandı ve
**T-18'i çiğniyordu**: teknolojiden uzak ustaya her satırda iki rakam
yazdırmak. Cemil üçüncü yolu gösterdi: **alan dursun, beklenmesin.**

| | karar |
|---|---|
| Alış alanı | **var**, ama zorunlu değil — boş geçilir, hiçbir şey uyarmaz |
| Boş alışın anlamı | *"bilinmiyor"* — **sıfır kâr değil.** Kâr ekranı o satırı hesaba katmaz, kâr **şişmez** |
| Görünürlüğü | **ayara bağlı** — alış soran dükkân açar, sormayan hiç görmez |

⚠️ **`alis: 0` ile `alis: null` aynı şey değildir.** Bugünkü kusur
(kâr şişik) tam burada: 0, *"bedava aldım"* demek. Bilinmeyen alış
toplamdan **elenir**, sıfır sayılmaz. Bu ayrım kodda ve denetimde
karşılığını bulmalı.

**Ayarlar sayfası (17. ekran) kapsama girdi.** Kapsamı yalnız Cemil
genişletir; genişletti. İlk üç sakini:

1. *"Parça alış fiyatı sorulsun mu"* (A-M43)
2. Gizlilik cümlesi — ilk kullanımda bir kez (A-M33)
3. Tema (bugün çekmecede duruyor)

**Menü bütçesi (A-M8: ≤ 8 başlık) sınanacak** — ayarlar menüye satır
ekliyorsa çekmecede kalmalı.

### 55.3 · Muhasebe ve fatura sonraki faza (A-M44 · A-M45 · A-M37)

Üçü de **sonraki faz.** Cemil: *"Fatura girişi bu fazda değil.
Muhasebede ele alacağız."* `MALIYET-VE-KAR.md §4` ve `A-M45` oraya
girdi sayılır. K-022'nin *"fatura kesmek yok"* sınırı **duruyor**.

### 55.4 · Ölçüm: doğrulama Cemil'de, koşu yapılacak (A-M34 · A-M35 · A-M30)

A-M34 **Cemil** — `guven` notlu 14 belge + 42 ditto satırı.
A-M35 **evet**, A-M34'ten sonra. A-M30: *"Ölçüp seçelim"* — üç model
aynı sette koşar, karar rakamla verilir. Ürüne koyma backend turunda.

### 55.5 · Ses kaydı alınamıyor — yeni açık madde (A-M39 → **A-M48**)

> **Cemil:** *"Bu sesi alabilecek imkânım henüz yok. Buna çözüm
> bulmalıyız."*

Alet hazır (`ses-kaydet.html`), **erişim yok.** Bu, ses tarafını
ölçülemez bırakıyor: *"Web Speech yetiyor"* bir tahmin olarak kalır.
A-M39 kapanmadı, **biçim değiştirdi** → **A-M48: dükkân sesine
erişmeden ses tarafı nasıl ölçülür?** Seçenekler araştırılacak (hazır
Türkçe konuşma derlemleri · sentetik üretim + `ses-bozucu.js` ·
tanıdık bir dükkânda tek seferlik kayıt · ölçümü erteleyip riski
yazmak). Çözümsüzse **riskin kendisi yazılır** — ölçülmemiş bir şey
"çalışıyor" diye anlatılmaz.

### 55.6 · İzin AMACI netleşti (A-M40 · A-M42 · A-M47 · A-M46)

> **Cemil:** *"Kullanımı tespit etmek, yani kullanmayanları ya da az
> kullananları tespit etmek, sorun / hata yakalamak amaç."*

Bu cevap *"ne zaman gelsin"* sorusunu **"ne için geliyor"** diye
yeniden yazdı ve özetin içeriğini belirledi. İz iki soruyu
cevaplayacak: **kim kullanmıyor** · **nerede takılıyor**. Günlük kısır
özetin omurgası budur; ayrıntı yalnız istendiğinde.

A-M42 ve A-M47 önerildiği gibi onaylandı. A-M46 **sonraki faz.**

### 55.7 · Gizlilik: tek satır; TC/vergi no zorlamaz (A-M33 · A-M38 · A-M36)

A-M33 **evet** — ayarlarda tek satır (55.2'nin sayfası).

A-M38'de yine hafifletti: *"İhtiyaç olursa elle, zaten ihtiyaç olursa
foto çekim de ona göre olacaktır. Belki çekim esnasında okunmuyorsa
bilgi ya da uyarı verilebilir."* → **Ekran kimseyi ikinci fotoğrafa
zorlamaz.** Alan ihtiyaç anında elle doldurulur; çekimde okunamadıysa
yalnızca **bilgi** verilir. (46 fotoğrafın 29'unda kadraj dışıydı —
K-051.)

A-M36 **ertelendi:** *"Uygulama kullanıma başladığında buna karar
verebiliriz."* Saklama süresi gerçek veri tutulmadan önce kapanmalı;
hatırlatıcı burada durur.

### 55.8 · İki yeni iş açıldı (Tur 8)

> **Cemil:** *"Servis detay sayfası kullanışlı olmadı gibi görüyorum.
> Burası için iş mekanizmasına uygun birkaç farklı yaklaşımda sayfa
> oluştur."* · *"İşlemleri için bir yol / sistem oluşturmak istiyorduk.
> Kaydedilen ses bir json dolduracak. Bu ikisi eşleşmeli. Hangi sayfada
> olduğuna göre bu json'u dolduracak ve işlem yapacak."*

İkisi de **yeni özellik değil**: biri var olan ekranın yeniden
kurulması, öteki Tur 7'de dal dal yazılmış bir davranışın **sisteme**
dönüştürülmesi. Ayrıntı `CALISMA-KUYRUGU.md` Tur 8'de.

⚠️ Servis detayı **ikinci kez** şikâyet konusu (7.1'de de *"istediğim
gibi değil"* denmişti). 7.1 bir kusuru düzeltti (görünmeyen satır),
**sayfanın kurgusunu** değiştirmedi. Bu kez istenen o.

---

## K-054 · Savunma özellikte değil, birikimde (16.09.2026)

> **Cemil:** *"Kopyalanamaz bir özellik eklemek."* · *"Taklit edilemez
> olması için ne yapılabilir? Bu alanda hızlı ilerlemeli ve alanı zapt
> etmeli ve savunmalıyız."*

**Soruyu olduğu gibi cevaplarsam yanlış cevap veririm: kopyalanamaz
özellik diye bir şey yok.** Sesle işlem, ruhsat okuma, parça listesi
okuma — hepsi API çağrısı, hepsi günler içinde kopyalanır. **Yapay zeka
özellikleri en kolay kopyalananlardır**, çünkü zekâ bizde değil API'de.

Dahası: bu deponun kurucu dersi *"çok fazla özellik var, karmaşık
geldi"*. Özellik ekleyerek savunma kurmak, ürünü bir kez öldüren şeyi
tekrarlamaktır.

### 54.1 · Kopyalanamayan dört şey

Ortak özellikleri: **zamanla birikir ve satın alınamaz.**

| # | ne | durum |
|---|---|---|
| ① | **Düzeltme birikimi** — model okur, usta düzeltir | **bugün başladı** |
| ② | **Parça sözlüğü** — usta dili ↔ katalog | biriktiriliyor |
| ③ | **Parçacı ağı** — çift taraflı, en güçlüsü | **bugün kurulmuyor** (A-M46) |
| ④ | **Destek ilişkisi** — arayıp cevap alabildiği kişi | iz kaydı bunun aleti |

**①'in mekanizması bugün kondu.** Bugüne kadar usta yanlışı
düzeltiyor, düzeltme ekranda kalıyor, **biz asla öğrenmiyorduk** —
birikim hiç başlamamıştı. Artık `Iz.okuma()` modelin yazdığını
bildiriyor, ustanın değiştirdiği her alan `duzeltme` olarak düşüyor:
*model dedi / usta yazdı*. Okumayı API veriyor, **düzeltmeyi dükkân**.

**③ bir çelişki değil, bir seçim:** parça fiyatı cevabı bugün ustanın
kendi WhatsApp'ına geliyor (sade, kurulumsuz, alışkanlığı bozmuyor).
Bedeli, fiyat akışının bizden geçmemesi ve ağın birikmemesi. Seçim
bilinçli olmalı — **A-M46**.

### 54.2 · Savunma OLMAYANLAR

İlk olmak · daha çok özellik · daha ucuz olmak · kodun gizliliği ·
patent · modelin kendisi. Bunları savunma sanmak en pahalı hatadır,
çünkü güvenip yavaşlatır.

### 54.3 · Hız neyi satın alır

Hız tek başına savunma değil; **birikimin başlangıç tarihini öne
çeker.** Bundan bir ölçüt çıkıyor:

> Bir iş ① ② ③ ④'ten birini büyütmüyorsa, onu hızlı yapmanın
> **stratejik** değeri yoktur.

Bu, *"yeni özellik varsayılan HAYIR"* kuralıyla çelişmiyor; onu
güçlendiriyor: **az iş yap, ama yaptığın iş biriksin.**

### 54.4 · Sonuç

**Kopyalanamaz özellik aramak yerine kopyalanamaz BİRİKİM kurmalıyız.**
Ürün rakibe açık kitap gibi görünsün; kitabı okuyup aynısını yazan,
üç yıllık düzeltme yığınımızı, parça sözlüğümüzü ve parçacı ağımızı
yazamaz.

Ayrıntı: `arastirma/SAVUNMA.md`. **Açık:** A-M46 (parçacı ağı) ·
A-M47 (düzeltmeler dükkândan çıkacak mı).

---

## K-053 · İzde DEĞERLER yazılır — bir gün önceki kuralımı geri alıyorum (16.09.2026)

> **Cemil:** *"Bu zaten müşteri verisi saklıyor olacağız mecburen. Bu
> yüzden müşteri verisini dert etmeni istemiyorum. Burada amaç kullanıcı
> hangi veriyi girmiş ve ne sorun yaşamış. Kullanıcı problem
> yaşadığında arayacağı kişi olacağım ve ona neyi yanlış yaptığını
> direkt söyleyebilmeliyim."*

**K-052.2'yi geri alıyorum.** Orada *"değer yazılmaz, şekil yazılır"*
demiş ve bunu bir elekle mekanik hâle getirmiştim.

### 53.1 · Neden yanılmışım — koruduğunu sandığım şey zaten korunmuyordu

Eleğin gerekçesi *"iz, müşteri verisinin ikinci kopyası olmasın"*dı.
Ama **birinci kopya zaten var**: plaka, ad, adres, TC veritabanında
duruyor (K-050). İz bunları yazmayınca veri korunmuyor — yalnızca
**destek körleşiyor.**

Telefondaki ustaya *"şurada plakayı yanlış yazmışsın"* diyebilmek için
o plakayı görmek gerekiyor. Elek tam olarak bunu engelliyordu.

> **Ders — K-050'nin dersinin ikizi:** *"Bu veri bize gerekmiyor"*
> demek için tamircinin yaptığı her işi bilmek gerekiyordu. Bu kez
> eksik bildiğim iş **destek**ti: ürünü kullananla telefonda konuşan
> kişi Cemil olacak ve elinde ekranın o anki hâli olmalı.
>
> İki kez aynı yerden düştüm: gizlilik kaygısıyla verilen bir karar,
> **kapsamı eksik biliniyorsa yanlış olur.**

### 53.2 · Yeni kural

| | K-052 (dün) | K-053 (bugün) |
|---|---|---|
| Girilen değer | elenir | **yazılır** |
| Dokunulan düğmenin yazısı | yazılmaz | **yazılır** |
| Kalan tek sınır | — | **boy**: değer 80 harfte kırpılır |

Boy sınırının sebebi gizlilik değil **ölçü**: tek bir uzun not halka
tamponu doldurup önceki adımları siler; iz yine kör kalır, bu kez
fazlalıktan.

### 53.3 · İzin işi üç tane

1. **Destek** — usta arayınca ne girdiğini ve nerede takıldığını görmek.
2. **Vazgeçme** — *"neden kullanmaktan vazgeçti"*: hangi adımdan sonra
   bir daha dönmedi.
3. **Zorlanma** — nerede yavaşlıyor, nerede geri gidip geliyor,
   hangi alanı üç kez düzeltiyor.

İkincisi ve üçüncüsü tek bir kullanıcıya bakarak görülmez; **zaman
içinde** görünür. Bu yüzden saklama iki aşamalı.

### 53.4 · Saklama (A-M41 KAPANDI)

> **Cemil:** *"Gün sayısı bir yerden sonra istatistiğe dönebilir, süre
> senin dediğin gibi olur."*

| aşama | süre | ne durur |
|---|---|---|
| **Ayrıntı** | cihazda 7 gün · bizde 90 gün | olay olay, değerler dahil |
| **İstatistik** | süresiz | ekran başına süre, tamamlanma oranı, şüphe sayıları, vazgeçme noktası |

Ayrıntı desteğin işine yarar ve eskiyince değerini yitirir; istatistik
**eskidikçe değer kazanır** — *"bu adımı altı ay önce %70 tamamlıyordu,
şimdi %40"* ancak böyle görülür.

### 53.5 · İzden çıkarım

> **Cemil:** *"İzden çıkarım yapabilmeliyiz."*

İz bir dosya yığını değil, **cevap veren bir şey** olmalı. Sorulacak
sorular şunlar ve hepsi aynı olay akışından çıkar:

* Hangi ekranda **en çok** vakit geçiyor? (yavaşlık)
* Hangi adımdan sonra kullanıcı bir daha **dönmedi**? (vazgeçme)
* Hangi alan **en çok** düzeltiliyor? (form yanlış soruyor)
* Hangi hata **en çok kişide** var? (yaygın kusur)
* Aynı şüphe kodu **hangi dükkânlarda** tekrar ediyor? (tasarım hatası,
  kullanıcı hatası değil)

**Açık madde A-M42:** bu soruların cevabı nerede görünecek? Öneri: ayrı
bir ekran değil, **tek sayfalık bir özet** — dükkân listesi, yanında
"zorlanma" işareti. Ürüne değil, bizim tarafa ait.

---

## K-052 · İz kaydı: giriyor, ustaya görünmez (16.09.2026) · ⚠️ 52.2 **K-053 ile geri alındı**

> **Cemil:** *"Log kaydı, nereye tıklanmış, ne yapılmış adım adım,
> hatalar, başarısızlıklar… Mantığa aykırı işlemleri de
> görebilmeliyiz."*

**Giriyor.** Gerekçesi bu deponun 1 numaralı belgesinde yazılı: tamirci
*"çok fazla özellik var"* dedi ve bunu **sorduğumuz için** öğrendik.
Usta destek aramaz, sessizce bırakır. İz kaydı o sessizliği ölçüye
çevirir.

### 52.1 · Ustanın işi değil, bizim işimiz

Yedi işten hiçbirine hizmet etmiyor. O yüzden **görünmez**: ekran yok,
ayar yok, soru yok, ustaya maliyeti sıfır. Ekran bütçesine (A-2)
dokunmuyor.

### 52.2 · Asıl tehlike ölçenin sızdırması

Bir iz kaydı az yazarsa işe yaramaz; **çok yazarsa müşteri verisinin
ikinci kopyası olur** ve bu fark edilmez — çünkü ayrıntılı iz *daha
iyi* çalışır. K-050/K-051 ile TC ve adres zaten tutuluyor; iznin
onları da taşıması, veriyi koruduğumuz yeri ikiye çıkarırdı.

> **Kural:** *değer yazılmaz, şekil yazılır.*
> `"34 ABC 123 → 34 QBC 123"` değil, `"plaka alanı 3 kez düzeltildi"`.

Temenni değil **mekanik**: `Iz.guvenli()` yalnız sayı, doğru/yanlış ve
küçük harfli kısa kod geçirir. Plaka, ad, telefon biçimce elenir —
*unutmakla değil, geçememekle.* 47. denetim iki yönden ölçüyor: tohum
verinin her plakası/adı/telefonu izde aranır (**bugün sızmıyor**) ve
eleğe bilerek plaka verilir (**yarın da sızamaz**). Negatif doğrulama
yapıldı.

### 52.3 · İki katman

| | nerede | ne var | ne zaman çıkar |
|---|---|---|---|
| **Ayrıntılı** | dükkânda, cihazda | değerler dahil | yalnız usta *"sorun bildir"* derse |
| **Kısır** | bizde | ekran · dokunuş · hata sınıfı · süre · şüphe kodu | varsayılan |

Hata ayıklamak için bazen değer gerekiyor (*"hangi plakayı yanlış
duydu"*); o bilgi **dükkânda** kalır.

### 52.4 · "Mantığa aykırı" hatadan ayrı bir şey

Program çalışıyor, kimse hata almıyor, ama olan iş yanlış: teslimden
sonra işlem · aynı araca ikinci açık kayıt · kilometrenin geri gitmesi
· parça beklerken teslim · fiyatsız teslim. Ayrıca **zorlanma
işaretleri**: aynı ekrana üç kez dönmek, aynı hatanın tekrarı, kaydı
açıp 10 saniyede silmek. Tam liste `arastirma/IZ-KAYDI.md §4`'te; ikisi
prototipe girdi.

### 52.5 · Ders

**Ölçüm aleti de ürünün parçasıdır ve onun da bir gizlilik yüzeyi
vardır.** İz kaydını yazarken sorulacak ilk soru *"ne görmek
istiyorum"* değil, *"bu ne taşıyor"*.

**Açık maddeler:** A-M40 (iz ne zaman gönderilir) · A-M41 (saklama
süresi — A-M36'daki KVKK cümlesiyle birlikte).

---

## K-051 · TC/vergi no ruhsat fotoğrafının **kadrajı dışında** kalıyor — 46 fotoğrafın 29'unda yok (15.09.2026)

> **Cemil:** *"TC / Vergi no alanlarını göremiyorum."*

Göremiyordu, çünkü **çoğu fotoğrafta yok.** Bu bir arayüz kusuru
sanmıştım; 29 fotoğrafı tek tek açınca ürün kararı çıktı.

### 51.1 · Rakam

Cemil'in gönderdiği **46 gerçek ruhsat** fotoğrafı elle tarandı:

| Sahip bölümü (TC/vergi no · soyad · ad · adres) | Kaç fotoğraf |
|---|---|
| **Okunuyor** | **17** (%37) |
| Değer sağ kenarda **kesik** — etiket var, değer yok | 18 |
| Sahip sayfası **hiç çekilmemiş** | 10 |
| Parlamada kaybolmuş | 1 |

Sebep tesadüf değil, **belgenin geometrisi**: yeni tip ruhsat açılır
iki sayfadır; araç bilgileri solda, sahip bilgileri sağda. Usta belgeyi
tek eliyle tutup çekiyor, araç alanlarına odaklanıyor ve **sağ kenar
kadraja girmiyor.** 18 fotoğrafta `(Y.4) T.C. KİMLİK NO` etiketi
görünüyor, karşısındaki değer görünmüyor.

### 51.2 · Bunun ürüne söylediği

K-050 fatura için bu alanları tutmaya karar verdi. **Karar duruyor** —
ama şu varsayım çöktü: *"ruhsatı okutunca fatura bilgileri de gelir."*

* **Fotoğraftan TC beklemek, üç fotoğraftan ikisinde boş dönmek
  demektir.** Model bunu doğru yapıyorsa boş bırakacak; usta "okumadı"
  sanıp güvenini yitirecek. Modelin hatası değil, **kadrajın** hatası.
* **Uyduran model burada en tehlikelidir.** Eksik TC'yi tamamlamak,
  yanlış kişiye fatura kesmek demek. Ölçütte bu 29 kaydın her alanına
  *neden* boş olduğu yazıldı; **değer döndüren cevap başarısız sayılır.**
* Doğru çözüm alanı zorlamak değil, **çekimi yönlendirmek**: ruhsat
  okuma ekranı *"sağ sayfa da görünsün"* diyecek, ya da fatura alanları
  ilk faturada bir kez elle girilecek (bir müşteri için bir kez).

### 51.3 · Ölçüte ne işlendi

`ruhsat-seti.json`'da 29 belgenin kişisel alanlarına **gerekçe notu**
kondu (`guven`): *"sahip sayfası fotoğrafta hiç yok"*, *"değer sağ
kenarda kesik"*, kısmen görünenlerde ise ne kadarının göründüğü
(`'28702…'`). Böylece boş alan ile **okunamayan** alan ayrıldı: ilki
ölçüt eksiği, ikincisi ölçütün kendisi.

### 51.4 · Ders

**"Model okuyamadı" ile "fotoğrafta yoktu" aynı şey değil ve karneyi
ters çevirirler.** Ölçüt yazarken eksik alanı sessiz bırakmak, sonraki
turda modeli haksız yere suçlamaya yol açardı. Boş bırakılan her alanın
yanına **neden** yazılmalı.

**Açık madde:** A-M38 — ruhsat okuma ekranı sağ sayfayı da isteyecek mi,
yoksa fatura alanları elle mi girilecek?

---

## K-050 · TC/vergi no ve adres tutuluyor — "gerekmiyor" dediğim şey gerekiyormuş (15.09.2026)

> **Cemil:** *"`yasakAlanlar` alanları da gerekli. Çünkü bu alanlara
> fatura kesilecek vb."*

**Bir gün önce yazdığım kuralı geri alıyorum ve gerekçesi benimkinden
güçlü.**

### 50.1 · Neden yanılmışım — "gerekmiyor" demek, her işi bilmeyi gerektirir

`RUHSAT-OKUMA.md` Bulgu 2 şöyle diyordu: *"Ruhsatta TC kimlik no ve
adres var, yedi işten hiçbiri bunları istemiyor; tutmadığın veri en
güvenli veridir."* K-049 bu kuralı bir **sınav maddesine** çevirmişti:
model bu alanları döndürürse, değeri doğru olsa bile okuma başarısız.

Cümle yanlış değildi, **eksikti**. Baktığım tek iş **araç kaydı
açmaktı**. Fatura kesmek ikinci bir iş ve **fatura tam olarak bu
alanlara kesilir**: kişide 11 haneli TC, firmada 10 haneli vergi no,
ikisinde de adres.

> **Ders:** *"Bu veri bize gerekmiyor"* demek için tamircinin yaptığı
> **her işi** bilmek gerekiyor. Gizlilik kaygısıyla verilen bir karar
> bile, kapsamı eksik biliniyorsa yanlış olur. Kuralı yazan ben,
> kapsamı bilen Cemil.

### 50.2 · Ne değişti

| | Önce | Sonra |
|---|---|---|
| TC / vergi no · adres | okunmaz, tutulmaz | **okunur, tutulur** (`Vehicle.TaxOrNationalId` · `Address`) |
| Değerlendirme setinde | `yasakAlanlar` — dönerse başarısız | **ölçülen alan** — dönmezse eksik |
| Ruhsat ekranı | *"burada tutulmaz"* açıklaması | **üç alan** (TC/vergi · adres · vergi dairesi) |
| Araç ekranı | yok | **"Fatura bilgileri" akordiyonu** |

**Fatura bilgileri ruhsat bilgilerinden AYRI akordiyonda** ve bu
bilinçli: ikisi ayrı soruya cevap veriyor — *"bu araç ne"* ve *"bu
fatura kime kesilecek"*. Tek akordiyona doldurulsaydı usta fatura
keserken dokuz alanın içinden üçünü aramak zorunda kalırdı.

### 50.3 · Vergi dairesi ruhsatta YOK — ve bu ekranda yazıyor

Fatura için gerekiyor, belgede yazmıyor. Üç yerde birden ele alındı:
alan **var**, okuma haritasında **yok**, ekran da *"ruhsatta yazmaz,
gerekiyorsa müşteriye sorulur"* diyor. Model istemine de yazıldı:
*"vergi dairesi ruhsatta yazmaz — böyle bir alan döndürme."*

> **Olmayan bir alanı doldurulabilir sanmak, modeli uydurmaya davet
> etmektir.** Boş alan ustaya *"sen yaz"* der; uydurulmuş vergi
> dairesiyle kesilen fatura **yanlış faturadır**.

### 50.4 · Değer depoya girmiyor — alan giriyor

Alanlar artık ölçülüyor, ama **46 gerçek insanın TC numarası ve adresi
bir GitHub deposunda kalıcı olarak durmaz.** Ayrım şu:

* **Alan** (şema, kural, ölçüt) → depoda.
* **Değer** (gerçek TC, gerçek adres) → `*-kisisel.json`, **.gitignore**'da.

Fotoğrafların kendisi zaten depoya girmiyordu; içindekini yazıya dökünce
girmesi **tutarsız** olurdu. Koşucu yerel dosyayı varsa ölçüte karıştırıyor,
yoksa bu dört alanı **ölçmüyor ve ölçmediğini söylüyor** — ölçemediğini
ölçmüş gibi raporlayan araç, ölçüm aleti değildir (K-024.1).

Negatif testle doğrulandı: kişisel alanı olmayan belgeye TC yazan model
**UYDURMA** alıyor; alanları hiç okumayan model belge tamlığında
**%100 → %48**'e düşüyor.

### 50.5 · İki sonucu var ve ikisi de yazıldı

**① Aydınlatma metni artık gerekli.** A-M33 *"iyi olur"* diye
duruyordu; TC ve adres tutuluyorsa KVKK aydınlatması ve **saklama
süresi** gerekiyor (**A-M36** açıldı).

**② Fatura, muhasebe kapsamının dışındaydı.** `MUHASEBE-KARARI.md`
B seçeneğinin sınırını *"fatura yok · KDV yok · e-belge yok"* diye
çizmişti. *"Bu alanlara fatura kesilecek"* cümlesi o sınıra
dokunuyor — **fatura alanlarını tutmak** ile **fatura kesmek** ayrı
işler; bu turda yalnız birincisi yapıldı (**A-M37**).

> **Cemil onayına açık:** **A-M36** (saklama süresi + aydınlatma
> metni) · **A-M37** (fatura kesmek MUH kapsamına giriyor mu —
> giriyorsa K-022'nin sınırı değişir).

---

## K-049 · Fotoğraflar geldi: ölçüt yazıldı, ama ölçütü MODEL yazdı (15.09.2026)

Cemil **46 ruhsat + 27 parça listesi** gönderdi (A-M32 kapandı). İkisinin
de değerlendirme seti yazıldı: `ruhsat-seti.json` (14 alan × 46 belge) ve
`parca-seti.json` (**258 satır** × 27 liste).

### 49.1 · ⚠️ Bu ölçüt kendi kendini sınayamaz

**Fotoğrafları okuyup alanları yazan modelin kendisiydi.** Yani elimizdeki
şey bir ölçüt değil, **ölçüt adayı**. Bir modelin kendi okumasını kendi
ölçütü yapması, sınavı kendi yazıp kendi puanlaması olur — ve çıkan rakam
ne kadar yüksek olursa olsun hiçbir şey söylemez.

Bu yüzden iki dosyanın başına da büyük harfle **TASLAK** yazıldı ve
şüphelenilen her alanın yanına `guven` notu düşüldü (46 belgenin
**14'ünde** var). Doğrulama oradan başlar.

> **Kural:** Cemil doğrulayıp onaylamadan bu setten çıkacak rakam
> **karne değil, tahmindir.** (A-M34)

### 49.2 · Ölçülen şey doğruluk değil, ÜÇ DAVRANIŞ

Cetvel (`puanla.js` · `alanPuanla`) üç şeyi ayrı sayıyor, çünkü üçünün
ustaya maliyeti farklı:

| Ne | Neden ayrı sayılıyor |
|---|---|
| **Kritik alan** (plaka · marka · ticari ad · model yılı · şase) | Plaka yanlışsa **yanlış araca kayıt açılır**; şase yanlışsa parçacı **yanlış parça gönderir**. Öteki on alanın doğruluğu bunu telafi etmez |
| **Uydurma** | Okunamayan alanı boş bırakan model ustaya *"sen yaz"* der; **uyduran model yanlış bilgiyi doğruymuş gibi verir** ve usta onu bir daha kontrol etmez |
| **Yasak alan** | TC no · adres · sahip adı döndürülürse, **değeri doğru olsa bile** okuma başarısız (RUHSAT-OKUMA Bulgu 2 · A-31). Bu bir doğruluk değil **davranış** sınavı |

**İki rakam birden veriliyor ve ikincisi asıl olan:** alan başına
doğruluk %97 olsa bile *belge başına tamlık* %83 ise, usta her altı
ruhsattan birinde bir şey düzeltiyor demektir — ve **üç düzeltmeden
sonra "bu çalışmıyor" der.**

### 49.3 · Fotoğraflar depoya girmiyor

`.gitignore`: `tools/degerlendirme/ruhsatlar/` · `parcalar/` ·
`sonuc-*.json`. Ruhsatlarda **TC kimlik no ve adres** var, parça
listelerinde müşteri ve parçacı adı geçiyor. Ölçüm için gerekliler,
**saklanmaları için bir sebep yok.**

Aynı kural ölçüte de işledi: `ruhsat-seti.json`'da sahip adı **yok**,
yalnız *"belgede sahip bilgisi var mı"* işareti var.

### 49.4 · Gerçek dünya, uydurulmuş örnekten daha zengin çıktı

Prototipin tohum verisinde olmayan, **ancak gerçek belgede görülen**
hâller:

* **Eski tip ruhsat** (3 belge) — yakıt, motor gücü, silindir hacmi ve
  koltuk sayısı **alanları yok**. Bunları dolduran model uyduruyor
  demektir; setin en sessiz tuzağı bu.
* **Okunamayan plaka** (`r31` · VW Passat) ve **parmağın kapattığı
  tarih** (`r45` · Peugeot 308) — doğru cevabın **boş bırakmak** olduğu
  iki kayıt.
* **Gelecek tarihli tescil** (`r23` — 25/06/2026). Belgede öyle yazıyor;
  model **düzeltmemeli**.
* **Ditto işareti** — parça listelerinde **42 satır**. Bir satırın anlamı
  ÜSTÜNDEKİ satırda: *"Sağ ön çamurluk sinyali / `" " "` çıtası"*.
  Çözülmezse o satır parçacıya sorulamaz.
* **Üstü çizili satır** (`p15` · Devirdaim, `p25` · tamamen karalanmış) —
  listeden atılmaz, **iptal işaretlenir**. Neden çizdiğini usta bilir.
* **Bir satırda iki fiyat** (`p09`): `1500 YN / 2500 ORJ` — yan sanayi ve
  orijinal. Tek fiyat alanına sıkıştıran model **yanlış rakam** verir.
* **Fiyat yerine durum**: *Gelmez · Yok · Geliniyor · Klimacıya
  bakılacak.* Bunlar sayı değil **cevap**.
* **Çarpım satırı** (`p23`): `4 buji = 230 ORJ x4  920` — toplamı birim
  fiyat sanan model ustaya dört kat pahalı gösterir.
* **Parça değil İŞ olan satırlar**: rektifiye (9000 ₺), yağ bakımı, rot
  ayarı, disk taşlama. Tamiron'da satır tipi parça/işçilik ayrımı var;
  hepsini "parça" diye kaydeden liste yanlış kayıt üretir.
* **Araç bilgisi gezici**: kimi listede üstte, kimisinde **en altta**
  (`p20` · *98 model Golf*), kimisinde yok; biri **motor koduyla**
  (`p11` · X16XEL), biri **şase ile** (`p02`), biri **plakayla** (`p06`).

### 49.5 · Tutarlılık çiftleri — tek kâğıt, tek cevap

Cemil farkında olmadan en değerli malzemeyi de gönderdi: **aynı belgenin
farklı çekimleri** (`r04·r15·r43`, `r10·r18`, `r33·r42`) ve **aynı
listenin fiyatsız/fiyatlı hâli** (`p24`→`p23`, `p26`→`p02`→`p08`).

İkincisi T-12/T-13'ün tarif ettiği akışın **fotoğrafı**: ustanın yazdığı
liste gidiyor, parçacı üstüne fiyat yazıp geri gönderiyor. Aynı kâğıdın
gidişi ve dönüşü elimizde.

> **Cemil onayına açık:** **A-M34** (ölçüt doğrulaması — modelin kendi
> okumasını ölçüt yapmasının önüne geçen tek adım) · **A-M35**
> (fotoğraflar diske konup koşulacak mı, maliyeti Opus 5 ile ~$0,60).

---

## K-048 · Üçüncü telefon turu: ölçek, süreç ve yapay zekanın yeri (14.09.2026)

Cemil on bir madde yazdı. Üçü hata, dördü ölçek, biri akış, üçü yapay
zeka. Hepsinin altında tek bir soru vardı: **prototip, gerçek hacimde ve
gerçek zekâyla ne olacak?**

### 48.1 · Ekranın kendi örneği yalan söylüyordu

Cemil: *"Servis kayıtlarında sesli olarak servistekileri göster
seçeneğini seçtim ama sonuç bulunamadı çıktı."*

07.09'da *"içeride"* kelimesi **"serviste"** oldu; ekranların hepsi
güncellendi, **çözümleyici eski kelimede kaldı.** Örnek çip cümleyi
öneriyor, cümle düz aramaya düşüyor, liste boş dönüyordu. Üstelik
çözümleyicinin ürettiği `suzgec` alanını **hiçbir ekran okumuyordu**.

> **Bir terimi değiştirmek, o terimi ARAYAN yerleri de değiştirmektir.**
> Örnek çipin kendisi negatif testti ve kimse dokunmamıştı.

**45. test (`ORNEK-CUMLE`)** her örneği işletiyor ve arama örneklerinde
listenin boş dönüp dönmediğine bakıyor. Yazılır yazılmaz **üç gerçek
hata daha** buldu: iki örnek olmayan bir plakayı arıyordu, biri
(*"bugün ne kadar tahsil ettim"*) çözümleyicide karşılığı olmayan bir
şey vaat ediyordu.

**Örnekler artık bayatlayamaz:** sabit plaka yazmak veriyi iki yerde
tutmaktı. Yer tutucular (`{plaka}`, `{serviste}`, `{teslimli}`,
`{sahip}`) çizim anında gerçek kayıtlardan doluyor.

**Tahsilat sorusu silinmedi, YAZILDI:** *"bugün ne kadar tahsil ettim"*
tamircinin dördüncü işinin ta kendisi ve cevabı zaten kayıtlı veriden
çıkıyor. Yeni alan istemeyen bir soru, yeni özellik değildir.

### 48.2 · Ölçek: üç mesajla güzel, üç yüz mesajla kullanılamaz

Cemil: *"Gönderilen mesajlar binlerce olduğunda nasıl listelenecek?
Filtrelemek gerekir mi? Yeni bir sayfa daha uygun olabilir."*

Sabah yazdığım arşiv akordiyonu **tohum veriyle** güzel görünüyordu: iki
mesaj. Günde beş mesaj gönderen bir dükkân bir yılda **1.800 satır**
eder. **Örnek veriye bakarak tasarlamak, ölçeksiz tasarlamaktır** —
72 mesajla denedim ve sayfalama, kanal süzgeci ve arama olmadan liste
kullanılamıyordu.

Üç soru, üç ayrı yer: *"bu müşteriye ne gönderdim"* → mesaj yazımında
**kapalı** akordiyon, son 5 (Cemil'in sayısı) · *"tümünü gör"* → arşiv
sayfası, plaka süzgeciyle · *"dün ne gönderdim"* → arşiv sayfası.
Ekran 15 → 16, **ama menüye satır eklenmedi** (karar bütçesi, A-M8).

**Bir kararımı geri aldım:** sabah *"mesaj satırı tıklanmaz, çünkü
gönderilmiş mesaj değişmez"* yazmıştım. Cemil *"detayı görülebilsin"*
dedi ve haklı — **değişmemesi bakılamaması demek değilmiş.**

### 48.3 · İki liste birbirine benzedi

Cemil: *"Araçlar, müşteriler, müşteriye mesaj aynı sayfalar oldu.
Farkları olmalı."*

08.09'da *"listelemeleri aynı olsun"* demişti ve satırı ortak nesne
yaptım — doğruydu. Ama ikisi de **aynı şeyi bağırıyordu: büyük plaka.**
Oysa iki liste iki ayrı soruyu cevaplıyor: araçlar *"hangi araç"*,
müşteriler *"hangi kişi"*. Müşteri satırında plaka **hiyerarşide indi**
(aynı bileşen, küçük boy), ad büyüdü. Satır 190 px'den 130 px'e indi —
**A-M28 de böyle kapandı.**

> *"Aynı olsun"* ile *"aynı görünsün"* aynı şey değilmiş: ortak olması
> gereken **satırın kendisi ve kuralı**, bağırdığı şey değil.

### 48.4 · Süreç: kaydın omurgası

Cemil: *"Süreç mesaj göndermeyle birlikte süreç şeklinde olsun. Teslim
edildi, parça bekliyor vb hep arka arkaya olan süreç şeklinde işlesin.
Bunu iyi kontrol et."*

Ekran bugüne kadar **"şu an"ı** gösteriyordu: bir rozet *"serviste"*,
bir bayrak *"parça bekliyor"*. Ama iş bir anlık durum değil, arka arkaya
gelen olaylar — ve **mesajlar o olayların içinde**, yanında değil.

**Yeni durum alanı açılmadı** (K-004 duruyor): süreç türetiliyor. Tek
eklenen alan `bekliyorTarihi` — bayrağın ne zaman kalktığını hiçbir yer
tutmuyordu ve **zamansız bir olay sıraya giremez.**

*"Bunu iyi kontrol et"* dedi; kontrol **iki hata** buldu:

1. Teslim edilmiş bir kaydın süreci *"Aracınız hazır gönderilebilir"*
   diyordu — öneriler **plakaya** bakıyordu ve aynı aracın başka bir
   açık kaydı vardı. **Araç düzeyinde doğru olan cümle, kayıt düzeyinde
   yalan.** Şablon koşulları kayıt bazına çevrildi.
2. *"Teslim edildi"* mesajı teslimin **üstünde** görünüyordu: aynı gün
   oldukları için sıra tarihten çıkmıyordu. Aynı gün olanlar artık
   **neden sırasına** göre diziliyor.

**46. test (`SUREC`)** dört değişmezi ölçüyor ve ikinci hatanın sınıfını
kapatıyor: *olmamış bir şeyi olmuş gibi anlatan sıra.*

### 48.5 · Ses açık kayıtta üç iş yapıyor

Cemil: *"Yeni işlemler için sesli olarak söyleyebilir. Bunlar parça
olabilir, işçilik olabilir. Fiyat güncellenebilir. Teslime
edilebilir."*

Sıra **dar olandan geniş**e: teslim → fiyat güncelleme → satır ekleme.
Ters sırada *"teslim ettim nakit aldım"* cümlesi kayda *"teslim ettim"*
diye bir satır eklerdi.

Fiyat güncellemede **A-M27'nin kuralı tekrar işliyor:** söylenen ad iki
satıra birden uyuyorsa güncelleme yok, yeni satır eklenir. *Tahmin
etmek, ustanın yazdığı rakamı sessizce değiştirmenin yoludur.*

Foto ve mikrofon adım 2'nin içindeydi; **ustanın eli yağlıyken
söyleyeceği şey adımı beklemiyor.** İkisi de her adımda görünür yere
taşındı, kopya bırakılmadı.

### 48.6 · Yapay zeka: dört iş, tek boru, bir şart

Cemil: *"Ses, parça listesi okuma, ruhsat okuma, sesi modele çevirip iş
yapılabilmesi... Bunu nasıl yaparız?"* + *"Mesaj gönderirken, mesajı
yapay zeka düzenleyecek."*

Cevap `arastirma/YAPAY-ZEKA.md`'de; model adları, fiyatlar ve API
davranışı **14.09.2026 tarihli Claude API referansından** alındı,
hafızadan yazılmadı. Dört bulgu:

**① Model sesi duymuyor.** Claude API metin ve görüntü alıyor, ses
almıyor. *"Sesi modele çevirmek"* iki adım: tanıma tarayıcıda kalıyor
(bugünkü Web Speech, ücretsiz), model **anlama** işini alıyor. Bugünkü
desen eşleştirici *"34 ABC 123 triger seti değişecek"*i anlıyor,
*"şu Mégane'ın trigerini değiştirdik, balatalara da baktık"*ı
anlamıyor — kazanç orada.

**② Anahtar tarayıcıda duramaz.** Bu belgedeki tek *"olmazsa olmaz"*.
Tarayıcıya konan API anahtarı herkese konmuştur. Yapay zeka,
`MIMARI.md`'de zaten öngörülen backend'in **ilk gerçek işi** oluyor —
dört küçük uç nokta yetiyor.

**③ Sözleşme zaten yazılı.** Model serbest metin değil, ekranın
beklediği **işlem nesnesini** döndürmeli; o nesneyi prototipin
çözümleyicisi bugün üretiyor. Yani model **prototipin yerine değil,
bir fonksiyonun yerine** geçiyor: onay ekranı, `sesUygula`, balonlar,
geri alma — hepsi kalıyor.

**④ Onay ekranı yapay zekayla birlikte kalkmaz, önemli hâle gelir.**
K-017 sahte çözümleyici için yazılmıştı; model de yanılır ve
**kendinden emin** yanılır.

**Ölçüm nasıl yapılacak** (Cemil'in asıl sorusu): elimizde zaten
`SES_COZUMLE` köprüsü ve 45. test var. Cümle + beklenen çıktı listesi
eklenince değerlendirme seti olur ve **aynı liste iki motora** verilir.
Fotoğraf tarafı için elde veri yok — **20–30 gerçek ruhsat ve 10–15
gerçek parça listesi fotoğrafı** gerekiyor (A-M32); onlar olmadan
doğruluk hakkında yazılan her cümle tahmindir.

**Mesaj düzeltmenin arayüzü bugün yazıldı** ve düzeltme **sahte olduğu
ekranda yazılı**. Sahte düzeltme bilerek küçük tutuldu (boşluk, ilk
harf, imza): zeki görünen sahte bir çıktı, olmayan bir yeteneği
göstermek olurdu — bu depoda en pahalı yalan o.

### 48.7 · Araç kendi hatasını söylemeye başladı

Bu turda *"geri"* yapmadan ikinci kez *"gom"* çağırdım: denetim **iki
kez** gömüldü, her fonksiyon iki kez tanımlandı ve 10. test haklı olarak
*"YINELENEN-FN | stilMetni x2"* dedi. **Bulgu gerçekti ama prototipte
değil, ARAÇTA** — yarım saat yanlış yerde hata arattırabilirdi.
`denetim-kos.py` artık üst üste gömmeyi reddediyor; *"geri"* de yedek
yokken yığın izi basmak yerine *"yapılacak bir şey yok"* diyor.

### 48.8 · Bu turun dersi

**Örnek veriyle tasarlamak, ölçeksiz tasarlamaktır.** İki mesajla güzel
görünen akordiyon, üç yüz mesajla kullanılamaz; dört müşteriyle güzel
görünen liste, kırk müşteriyle okunamaz. Bu turda ilk kez **veriyi
büyütüp baktım** (72 mesaj) ve tasarım oracıkta değişti. Bundan sonraki
her liste için soru şu: *bunun yüz katı olsa ne olur?*

> **Cemil onayına açık:** **A-M30** (hangi model) · **A-M31** (backend
> bu fazda mı) · **A-M32** (gerçek fotoğraflar toplanacak mı) ·
> **A-M33** (gizlilik cümlesi nerede yazsın).

---

## K-047 · İkinci telefon turu: dört madde, üçü "aynı şeyi iki yerde yapma" (14.09.2026)

Cemil telefondan dört madde yazdı. Görünürde dördü ayrı iş; üçü aynı
hataya çıktı — **aynı iş iki yerde yapılıyordu ve ikisi ayrışmıştı.**

### 47.1 · Ana tuşu gidiş-dönüş oldu

Cemil: *"Alttaki menü ikonuna basınca anasayfaya gidiyor. Tekrar basınca
önceki ekrana dönsün."*

Dönüş yolu **zaten vardı** (geri tuşu) ve çalışıyordu. Ama usta ana
menüye **hangi tuşla gittiyse aynı tuşla dönmeyi** bekliyor. İki ayrı
tuş öğretmek yerine aynı tuşa ikinci anlam verildi.

**Bir tuşun iki işi varsa ikincisi görünmeli:** tuş ana menüde "dönüş"
hâline geçiyor — adı değişiyor, vurgulanıyor; dönülecek yer yoksa
geri/ileri gibi sönük (K-042'nin kuralı: gidilemeyecek yer gidilebilir
görünmesin).

**Dönüş hedefi tek kapıda siliniyor** (`goster`): ana tuşu dışında her
gezinme onu düşürür. Silinmeseydi tuş, ustanın çoktan terk ettiği bir
ekrana atardı — ve **ekran doğru çizilirdi**, yanlış olan nereye
gittiği olurdu.

### 47.2 · Aynı liste, iki ekran, iki ayrı çizim

Cemil: *"Müşteriye mesaj ve müşteriler sayfasının listelemeleri aynı
olacak. Soldaki ikon olmayacak. Servisteki araçlar üstte ve renkli
olacak. Müşteri adı, altına plaka aynı stilde, altında araç marka model
ve renk kutucuğu. Altta telefon altta varsa not."*

**İkisi de kendi içinde doğruydu.** Müşteriler ekranı müşteri başına
satır üretiyordu, mesaj ekranı **araç başına**; ikisi de `.tl-item`
giyiyordu — oysa o bir **servis kaydı** satırıdır ve soldaki ikon da
oradan geliyordu. İki aracı olan müşteri bir listede iki kez, ötekinde
bir kez görünüyordu. Hiçbir test bunu göremezdi, çünkü **her ekran tek
başına tutarlıydı.**

Üç şey tek yere taşındı: satırın çizimi (`musteriKartiHtml`), hangi
satırların hangi sırada geleceği (`musteriListesi`), satırın hangi aracı
gösterdiği (`musteriAraci`). **Satırın işi ekrana göre değişiyor
(müşteri detayı · mesaj), satırın kendisi değil.**

> Sol ikon kalkınca `.tl-ic`nin kullananı kalmadı ve **OLU-SINIF testi
> aynı turda söyledi.** Ölü CSS bırakılmaz: duran bir kural "bu da var"
> diye okunur.

**Satırlar Cemil'in tarifiyle uzadı.** 08.09'da *"çok boşluklu"* denen
şey ayrıntının **listede** olmasıydı; ayrıntı hâlâ müşteri detayında,
listede duran şey ustanın tarif ettiği beş satır (A-M28).

### 47.3 · Şablonun koşulu artık "daha önce ne gönderildi"yi de soruyor

Cemil: *"Gönderilen mesajlar listelenebilmeli."* +
*"Teslim edilmiş ve mesaj gönderilmemiş araca teslim edildi mesajı
gönderebilir. Yine serviste olan ve işin özeti gönderilmemiş araca işin
özeti gönderilebilir. Ve gerçekten parça bekliyor durumunda olana parça
bekleniyor mesajı gidebilir."*

Üç cümlenin ortak yanı: her şablonun bir **koşulu** var ve koşul iki
şeyi birden soruyor — kaydın durumu ve **daha önce ne gönderildiği**.
İkincisi bugüne kadar hiç yoktu; "işin özeti" gönderilmiş araca ikinci
kez teklif ediliyordu.

**Arşiv bir görünüm değil, kuralın kaynağı.** Ve "iyi olur" diye
yazılmadı: K-045.3'te ölçülmüştü — ticari elektronik iletiye ilişkin
kayıtlar **üç yıl** saklanır, **ispat yükü gönderende**. Alanlar oradan
geliyor: kime, hangi kanaldan, ne zaman, hangi metin.

**"Ara" arşive yazılmaz** — telefon bir mesaj değil; yazılsaydı aranan
müşteriye *"özeti gönderdin"* denirdi.

Altı şablon, altısı da koşullu:

| Şablon | Koşul |
|---|---|
| İşin özeti | serviste **ve** özeti gönderilmemiş |
| Parça bekliyoruz | serviste **ve** gerçekten parça bekliyor |
| Aracınız hazır | serviste *(A-M17 kapandı: teslim edilmişte artık çıkmıyor)* |
| Teslim edildi | **açık kaydı yok** ve son kaydı teslim edilmiş ve hiç mesaj gitmemiş |
| Bakım hatırlatması | serviste değil **ve** bakımı var |
| Serbest mesaj | her zaman |

> **Bir koşul fazla çıktı ve elle işletince görüldü:** *"Teslim edildi"*,
> bugün serviste olan ama geçmişte teslim edilmiş bir bakımı olan araca
> da çıkıyordu. Koşul doğru cümleyle yazılmıştı; yanlış olan **hangi
> kayda baktığıydı** (son kayıt ≠ açık kayıt).

### 47.4 · Sesli yol artık gönderiyor — ve toplu gönderim yazıldı

Cemil: *"Mesaj gönderde ses kaydettim. '34 qbc 123 sahibine WhatsApp
üzerinden selam mesajı gönder.' Buradan mesaj sesli olarak
gönderebilmeliyiz."* + *"Burada da bir onay ekranı gerekli. Mesela tüm
müşterilere bayram mesajı gönder diyecek sesli olarak. Uygun bir bayram
mesajı bulacak ve onay ekranı ile SMS yada WhatsApp üzerinden mesaj
gönderecek."*

Eskiden sesli yol mesaj ekranını **açıyordu**: usta söylüyor, onay
ekranında onaylıyor, sonra bir kez daha gönder düğmesine basıyordu.
**Onay ekranı zaten bir onaydı**; ikincisi aynı soruyu iki kez sormaktı.

Onay ekranında artık üç şey var: **gidecek metin** (düzenlenebilir —
usta neyi gönderdiğini göndermeden önce okuyor), **kanal seçimi**
(söylenmediyse WhatsApp; SMS paralı ve İYS kaydı istiyor, varsayılan
olamaz) ve *"Ekranda aç"* (özet linkine bakmak ya da hedefi değiştirmek
isteyen için — ölü uç yok).

**Metin üretimi ortak nesne oldu.** Ekran betikleri kendi IIFE'sine
sarıldığı için sesli işlem mesaj ekranındaki fonksiyonu **göremiyordu**.
İki seçenek vardı: metni ikinci kez yazmak ya da ortak nesne yapmak.
Birincisi, **onay ekranında görülen metinle gerçekte gidenin
ayrılması** demekti — ve usta bunu ancak müşteri sorunca öğrenirdi.
Hitap ve imza da aynı sebeple tek yere taşındı; firmaya *"Ltd. Şti.
Bey/Hanım"* yazan hata oradan çıkmıştı.

**Hazır metinler:** bayram · selam · teşekkür. Usta metni yazmıyor,
**türünü** söylüyor. ⚠️ **Bayramın adı yazılmaz** — *"Ramazan
Bayramınız"* yazsaydık Kurban'da yanlış mesaj giderdi ve hangi bayram
olduğunu bilen bir takvimimiz yok. **Bilmediğini söylememek, yanlış
söylemekten iyidir.**

#### ⚠️ Toplu mesaj K-045.2 ile çelişiyor — ve Cemil kazanıyor

K-045.2'de *"kampanya bu fazda yazılmaz"* demiştim. Gerekçe sağlamdı ve
**hâlâ geçerli**: izin altyapısı yok (araç kaydında telefon var, **izin
yok**) ve kendi numarasından toplu gönderim Meta'nın şartlarına aykırı;
numara engellenirse **yalnız kampanya durmaz** — özet, *"aracınız
hazır"* ve bakım hatırlatması da durur.

**Kapsamı Cemil genişletti** (CLAUDE.md: kapsamı yalnız o genişletir).
Gerekçe silinmedi, **ustanın gözüne yazıldı**: onay ekranında risk
uyarısı duruyor, kimlere gideceği **adlarıyla** görünüyor ve her alıcı
**kendi arşiv kaydını** alıyor (ispat yükü kişi başına).

Gerçek kullanımda sıra değişmiyor: **İYS kaydı → onay toplama →
WhatsApp Business API → şablon onayı** (A-M25).

### 47.6 · Yanlış duyulan plaka: üçüncü yol — SOR (14.09, A-M27)

Cemil'in ses kaydı *"34 **qbc** 123 sahibine WhatsApp üzerinden selam
mesajı gönder"* oldu: tanıma **A**'yı **Q** duydu. Usta doğru söylemişti,
program doğru davranmıştı (*"bu araç kayıtlı değil"*) ve **iş yine de
durdu.**

İki kolay yol da yanlıştı:

| Yol | Sonucu |
|---|---|
| Kendiliğinden düzelt | **Yanlış araca mesaj gider** — ve usta bunu ancak müşteri sorunca öğrenir |
| Hiç düzeltme | Usta kayıtlı bir araç için ruhsat çekmeye gönderilir |

Üçüncü yol: **sor.** Onay ekranında bir düğme çıkıyor — *"Bunu mu demek
istedin: 34 ABC 123"* — ve **dokunulmadıkça hiçbir şey olmuyor.**

**Kural dar tutuldu, çünkü yanlışın bedeli ağır:** il kodu ve rakam
grubu **birebir aynı** olacak · harf grubu **en fazla bir karakter**
farklı (harf yutulmuş ya da uydurulmuş olabilir) · ve **tek aday**
kalacak. **İki aday varsa öneri yok** — hangisi olduğunu bilmiyorsak
tahmin etmek, ustaya yanlış aracı onaylatmanın yoludur.

⚠️ **DÜZELTME İŞLEMİ DEĞİL CÜMLEYİ DEĞİŞTİRİYOR.** İşlemin plakasını
elle değiştirmek ikinci bir çözümleme yolu açardı: plaka yenisi,
şablon/kanal/metin eskisi — usta onayladığından başka bir şey
gönderirdi. Cümledeki plaka değiştirilip **aynı çözümleyiciye** veriliyor.
Ve etiket *"Duyulan"* değil **"Düzeltildi"** oluyor: söylemediği bir şey
ustaya söylemiş gibi gösterilmiyor.

**44. test (`PLAKA-ONERISI`)** bu hata sınıfını üç yönden ölçüyor: hiç
önermemek · fazla önermek (iki aday varken seçmek) · **sormadan
uygulamak**. Üçü de negatif testle doğrulandı; sonuncusu için kod bilerek
bozuldu (öneriye dokununca gönderiyordu) ve test *"soru değil, eylem
olmuş"* dedi.

### 47.5 · Bu turun dersi

**"İki ekran da doğru" cümlesi bir hata sınıfını gizliyor.** Bu turun üç
maddesi de aralarındaki tutarsızlıktı: iki liste, iki metin üreteci, iki
gönderim yolu. Hiçbiri kendi içinde bozuk değildi — ve bu yüzden hiçbir
test görmüyordu. Üç yeni test de tam bunu ölçüyor: **ekranın iç
doğruluğunu değil, iki yer arasındaki aynılığı.**

| # | Test | Ne ölçüyor |
|---|---|---|
| 41 | `ANA-TUS` | Duruma göre anlam değiştiren tuş: ikinci dokunuş dönüyor mu, dönüş hedefi fazla yaşıyor mu |
| 42 | `AYRISAN-LISTE` | Aynı verinin iki ekranda iki görüntüsü |
| 43 | `MESAJ-ARSIV` | Sonucu başka yerde duran eylem — **iki gönderim yolu da** işletiliyor |
| 44 | `PLAKA-ONERISI` | Tahmin eden ekran: hiç önermemek · fazla önermek · **sormadan uygulamak** |

Üçü de negatif testle doğrulandı. Denetim artık gezinme yığınını ve
mesaj arşivini de geri yüklüyor (K-024.1 / K-041.2'nin dersi iki yeni
yüzeyde), ve yedeklenen tablo listesi **tek yere** bağlandı — iki yere
yazılıydı, yenisi birine yazılıp ötekinde unutulabilirdi.

> **Cemil onayına açık:** **A-M25** (toplu mesaj yazıldı, K-045.2 ile
> çelişiyor) · **A-M26** ("gönderildi" sayılma anı) · **A-M27** (ses
> plakayı yanlış duyunca) · **A-M28** (müşteri satırının boyu) ·
> **A-M29** (müşterinin öteki aracına mesaj).

---

## K-046 · Telefon testi: sekiz geri bildirim, biri akışın kendisi (08.09.2026)

Cemil prototipi **ilk kez telefondan** kullandı (Netlify'a deploy edilmiş
PWA). Sekiz madde geldi. Yedisi tek tek düzeltilecek şeylerdi; sonuncusu
ekranın kurgusuydu.

⚠️ **BU TURUN DERSİ:** maddelerin en az üçü — boşluk, tekrar eden cümle,
tıklanınca hiçbir şey yapmayan satır — masaüstü tarayıcıda **görünmeyen**
hatalardı. Genişlik ölçüyorduk (320/375/430), dokunuş sayıyorduk; ama
kimse telefonu eline alıp *"şimdi ne yapacağım"* diye bakmamıştı.
**Ölçüm, kullanmanın yerine geçmiyor.**

### 46.1 · Alt gezinme sağa yaslandı, solda "neredeyim" yazıyor

Cemil: *"Footerda navigasyon buttonlar sağa yaslı olsun. Ortasinda bir
şey olmasın. Solda herhangi bir şey koyabiliriz."*

`space-around` üç tuşu şeride yayıyordu. Üçü **tek denetimin gövdesi**
(K-042), öyleyse tek grup — ve grup sağda.

Sol boş kalmadı: **bulunulan ekranın adı** yazıyor. Ekranın kendi başlığı
(`.screen-head .eyebrow`) sayfayla birlikte kayıyor; aşağı inen usta
nerede olduğunu göremiyordu. Alt şerit sabit, cevap da sabit olsun.

⚠️ **Çekirdek ekranın adını BİLMİYOR.** Buraya bir `{ ad -> başlık }`
tablosu koysaydım her yeni ekran iki yere yazılırdı ve biri unutulurdu —
v1'in en pahalı hata sınıfı. `yerTazele()` ekranın kendi başlığını
**DOM'dan okuyor**; sayaç (`.eb-say`) eleniyor, çünkü o başlık değil, o an
kaç şey olduğu.

### 46.2 · Müşteri listesi satıra indi, ayrıntı kendi ekranına çıktı

İki cümle aynı yeri işaret ediyordu:
*"Müşteriler sayfası neden çok boşluklu, bunu düzenleyelim."* ·
*"Müşteriler listesinde müşteriye tıklayıp müşteri detayı görebilmeliyim.
Tıklayınca herhangi bir şey yapmıyor."*

06.09'da müşteri bir **kart** olmuştu: ad, telefon, ayraç, araç satırları,
mesaj düğmesi. Üç müşteri telefon ekranını dolduruyordu. **Liste aramak
içindir, okumak için değil.**

Kart satıra indi (`.tl-item` — mesaj ekranındaki müşteri seçme listesinin
aynısı), taşınan ayrıntı yeni `musteri` ekranına gitti. **Hiçbir şey
silinmedi, yeri değişti.**

⚠️ **BU BİR KARARIN GERİ ALINMASIDIR.** `90-musteriler.html` şöyle
diyordu: *"müşterinin kendi ekranı YOK ve gerekmiyor; bilgi zaten aracın
üstünde."* Gerekçe kâğıt üstünde doğruydu, telefonda yanlış çıktı: usta
müşterinin **adına** dokunuyor çünkü aradığı şey müşteri; hangi aracı
olduğunu zaten bilmiyor, ona bakmaya geliyor. Dokunulan yerin hiçbir şey
yapmaması, ekranın en pahalı hâlidir — usta bozuk mu diye ikinci kez
dokunuyor.

**Ekran sayısı 14 → 15.** Ana yol büyümedi (A-1): ekran akışın üstünde
değil, listenin dibinde. Ana yol hâlâ `ruhsat → kayit → teslim`.

### 46.3 · "Parçayı müşteri getirdi" dört yerden bir yere indi

Cemil: *"Her yere parçayı müşteri getirdi bilgisini yazma. Bu lazım
olursa kullanılacak bir özellik."* + *"Servis kayıtlarında yine parçayı
müşteri getirdi yazıyor. Bu konuyu abartmışsin sanki?"*

Sayıyla haklıydı. Aynı cümle **dört yerde** duruyordu:

| Nerede | Ne oldu |
|---|---|
| "Yapılanlar" başlığındaki toplu düğme | **kaldırıldı** |
| Her parça satırının altındaki çip (basılı değilken *"tamirci getirdi"* yazıyordu) | **koşullu hâle geldi** |
| Tutar hücresinin `title` metni | **kaldırıldı** |
| Örnek verinin açıklama metni | **kaldırıldı** |

**Yeni kural — tek slot, üç hâl:**

| Satırın hâli | Slotta ne var |
|---|---|
| müşteri getirdi | **bilgi** — dokunulunca geri alınır; tutar hücresi `—` |
| tutar yazılmamış | **teklif** — soluk, noktalı altı çizili bir bağlantı |
| tutar yazılmış | **hiçbir şey** — parayı sen alıyorsan parçayı sen getirmişsindir |

A-M13'ün cevabı korunuyor (*"balatayı müşteri getirdi, yağı ben aldım"*):
işaret **hâlâ satır başına**. Değişen tek şey, sorulmadığı zaman
görünmemesi.

⚠️ **Üstü çizili tutar da kalktı.** Cemil: *"Değişen parça fiyatı yerine
bilgi olarak sadece yazılabilir."* Üstü çizili bir rakam hâlâ bir
rakamdır: göz onu toplamda arar, bulamaz ve tabloya güveni azalır. K-037
duruyor — `s.fiyat` **silinmiyor**, yalnız gösterilmiyor.

⚠️ **06.09'dan bugüne aynı satırın üç hâli oldu:** 26 px'lik iki çip (çok
küçük) → 44 px'lik tek çip (ama her satırda) → koşullu slot. İlk iki
turda sorduğum soru *"düğme ne kadar büyük olmalı"*ydı; doğru soru **"her
satırda görünmesi gerekiyor mu"**ymuş.

### 46.4 · Ekran içi adım da "geri"dir

Cemil: *"Müşteriye mesaj gönder ekranında bir müşteri seçilip geri gelince
ana menüye atıyor."*

Mesaj ekranının **içinde** iki adım var (kime → ne). Bu adımlar
`tazele()` ile değişiyordu, yani geçmişe hiç yazılmıyordu; geri tuşu
ekranın tamamından çıkıyordu ve usta bir adım geri gitmek isterken ana
menüde buluyordu kendini.

**Çözüm çekirdekte, ekranda değil:** ekran iki kanca tanımlayabilir —
`geriVar()` (içeride geri alınacak adım var mı) ve `geriAl()`. Geri tuşu
**önce ekrana sorar**. Her ekranın kendi geri düğmesini çizmesi de
çözerdi, ama o zaman "geri" iki ayrı şey olurdu: alttaki tuş bir yere,
ekrandaki düğme başka yere. **Tek tuş, tek anlam** (K-042'nin kuralı).

### 46.5 · Mesaj ekranındaki mikrofon KOMUT değil DİKTE

Cemil: *"Mesaj ekranında 'aracınız hazır teslim alabilirsiniz' dedim,
mesaj bölümü için yanlış anlamadıysam dedi. Aslında burada text yazması
gerek."*

Hata bendeydi ve ekranda yazılıydı: düğmenin `aria-label` metni *"Sesle
yazdır"* diyordu ama düğme `sesleAc('mesaj')` çağırıyordu — cümleyi
**komut** sanıp çözümlemeye gönderiyordu. Usta mesajın **metnini**
söylüyor; çözümlenecek komut yok, yazılacak cümle var.

**İki ayrı iş, iki ayrı yol:**

| | Ne yapar | Nerede |
|---|---|---|
| `sesleAc(niyet)` | KOMUT — anlar, **onay ister** | arama kutuları, menü, kayıt içi satır ekleme |
| `Dikte` | METİN — söylenen alana yazılır, onay yok | mesaj metni |

`Dikte` çekirdekte ve `data-dikte="<alanId>"` ile bağlanıyor: her ekran
kendi tanıyıcısını kursaydı *"izin verilmedi"* mesajı birinde çıkar
ötekinde çıkmazdı. Motor tespiti (`sesMotorSinifi` · `sesMotorVar` ·
`sesMotorNotu`) ses ekranından çekirdeğe taşındı — iki iş aynı motoru
soruyor, tek yerden.

**Yazılanın üstüne yazmıyor:** alanda ne varsa temel alınıp ardına
ekleniyor. Şablondan gelen metni silmek, ustanın seçtiği şablonu sessizce
çöpe atmak olurdu.

### 46.6 · Servis kaydı üç adıma bölündü

Cemil: *"Servis kaydı ekle, düzenle sayfalarını akışa uydurmak gerekiyor.
Bunun için senaryolar oluştur ve buna uygun çalışmasını sağla. Sayfada
herşey yapılabiliyor gözüküyor. Adımlar birbirini takip etmeli."*

Ekran, `IS-AKISI.md`'nin 2., 3., 4., 5., 6. ve 7. adımlarının **hepsini
aynı anda** gösteriyordu: açıklama, tablo, parça ekle, sesle ekle,
fotoğraftan oku, parça bekliyor, parçacıya sor, özet gönder, teslim et.
**Dokuz eylem, tek ekran, hiçbiri ötekinden önce değil.**

| Adım | IS-AKISI karşılığı | Önkoşul |
|---|---|---|
| **1 · İş** | Adım 2 · ne istiyor | yok |
| **2 · Yapılanlar** | Adım 3-4-5-6 | açıklama **ya da** satır var |
| **3 · Teslim** | Adım 7 | en az bir satır |

Senaryolar ayrı belgede: **`SENARYO-SERVIS-KAYDI.md`** — altı senaryo ve
her turda koşulacak on bir maddelik kontrol listesi.

⚠️ **ADIM KİLİTLİ DEĞİL, KONUŞUYOR** (K-040.3). Hiçbir sekme `disabled`
değil; hazır olmayan adıma dokunulunca ekran ne eksik olduğunu söyler ve
eksiğin durduğu adıma götürür. Sönük sekme ustaya *"burası bozuk"*
dedirtir.

⚠️ **K-003 DURUYOR.** Kayıt teslim edilene kadar açık; geriye serbestçe
dönülür, her adımdan sonra satır eklenebilir (Kol C — *"ek iş yapılabilir
teslim etmeden önce"*). Sıralanan şey ekranın **sunumu**, kaydın hayatı
değil.

**Adım kaydın üstünde** (`k.adim`), ekranın değil — bu dosyanın `turElle`
dersinin aynısı. Yazılmamışsa veriden türetiliyor, yani örnek veri ve eski
kayıtlar da doğru adımda açılıyor.

**"Servis kaydı ekle" ekranı yeniden kurulmadı** çünkü zaten sıralıydı:
plaka → tanıma → (kayıtsızsa) araç bilgileri → eylem. Tek kusuru
düzeltildi: **plaka yokken "Kaydı aç" düğmesi görünüyordu.** Ekranın en
büyük, en renkli öğesi henüz yapılamayacak bir şeyi teklif ediyordu.
Düğme sönük yapılmadı, **henüz gösterilmiyor** — sönük düğme "bozuk" der,
olmayan düğme "sıra sende değil" der.

### 46.7 · Sağlık denetimi ilk çizimde bir ölü uç yakaladı

Yeni `musteri` ekranı, müşteri bulunamadığında hiçbir eylemi olmayan bir
ekran çiziyordu — denetim `OLU-UC | musteri | ekranda hicbir eylem yok`
dedi ve çıkış yolu eklendi (K-021). **Aracın değeri tam burada: yeni bir
ekran, eski bir hata sınıfına ilk çizimde düştü.**

Denetim üç genişlikte (320 · 375 · 430) **0 bulgu** ile kapandı.

⚠️ **`tools/tutarlilik-denetim.py` ÇALIŞMIYOR** — var olmayan bir belgeyi
(`docs/PROTOTIP-SENARYO.md`) okumaya çalışıp hata veriyor. Bu turdan önce
de böyleydi; burada yalnız **kayda geçiyor**.

---

## K-045 · Üç araştırma: ikisi "yazma", biri "başka türlü yaz" dedi (08.09.2026)

**Kuyruk 4/D.** Kod yok; karar malzemesi. Üçü de aynı biçimde bitiyor:
*yedi işe hizmet ediyor mu, etmiyorsa neden.*

### 45.1 · Sanal POS — soru "yapabilir miyiz" değil, "parayı kim tutuyor"

`arastirma/SANAL-POS.md`

İki model var ve aralarındaki fark teknik değil **cezai**:

| | Parayı BİZ topluyoruz | Parayı DÜKKÂN topluyor |
|---|---|---|
| Sıfat | **Ödeme kuruluşu** | **Teknik hizmet sağlayıcı** |
| Lisans | TCMB izni **zorunlu** | gerekmiyor (6493 m.12/2-ğ) |
| İzinsiz | **1–3 yıl hapis + 5.000 güne kadar adli para cezası** (m.28) | — |

Ayırt edici ölçü tek cümle: **işlemin hiçbir anında fonun sahibi
olmamak.** Para bir an bile bizim hesabımıza uğrarsa istisna düşer.

> Bu, projede karşılaştığımız en sert kısıt. Muhasebede yanlış rakam
> göstermek ustayı yanıltır; burada yanlış model **suçtur**.

**Karar: Model A hiç konuşulmaz. Model B yazılabilir ama bu fazda
yazılmaz** — ödeme kuruluşu sözleşmesi ve *"ödendi"* bilgisini okuyacak
bir **backend** gerektiriyor, bu fazda backend yok.

**Gerçek bitişiği ALACAK.** Tamircinin dördüncü isteği *"ödemeyi nasıl
aldığını bilmek"* — bir **kayıt** isteği. Ama veresiyesi olan müşteriye
özetle birlikte bir ödeme linki gitmesi, ustanın *"ne zaman alacağım"*
sorusuna gerçekten dokunuyor. **Sıra: önce alacak (B), sonra link.**

### 45.2 · Toplu mesaj — muafiyetimizin bittiği yer

`arastirma/TOPLU-MESAJ.md`

`SMS-VE-IYS.md`'de bulduğumuz muafiyet (bakım hatırlatmasına onay
gerekmiyor) **kampanyayı kapsamıyor**: muafiyet *"temin edilen hizmete
ilişkin bakıma yönelik"* iletiler için. *"Bu ay yağ değişiminde %20
indirim"* temin edilmiş bir hizmete ilişkin değil, **yeni bir satışa**
yöneliktir.

**Aynı ürün, aynı liste, aynı kanal — farklı hukuk.**

⚠️ **Ve daha ağırı: toplu mesaj WhatsApp modelimizi çökertiyor.**
`WHATSAPP.md`'de A yolunu seçmiştik — *"usta kendi WhatsApp'ından
gönderiyor, hiçbir kurulum yok"*. Kişisel hesaptan ya da Business
**uygulamasından** toplu gönderim Meta'nın şartlarına aykırı ve
**numaranın engellenmesine** yol açıyor.

> **Ustanın numarası banlanırsa yalnız kampanya durmaz** — özet
> gönderme, *"aracınız hazır"*, *"parça bekliyoruz"* ve bakım
> hatırlatması da durur. **Kampanya özelliği, çalışan altı işi riske
> atıyor.** Tek başına erteleme gerekçesi budur.

**Karar: kampanya bu fazda yazılmaz.** Önündeki engel teknik değil,
**onay toplama**: bugün araç kaydında telefon var ama **izin yok**.
Önerilen küçük adım, iznin kaydı için tek bir alan — bugün hiçbir şeyi
bozmaz, ileride önü açar.

**Cezalar güncellendi:** `SMS-VE-IYS.md`'de *"1.000–5.000 TL, bunlar
2014 tutarları, güncel değil"* diye işaretlemiştik. Doğruymuş — Eylül
2026 taraması: onaysız ileti **2.859–14.309 TL**, **toplu gönderimde
10 katına kadar**; ret hakkının işlenmemesi **3.799–28.544 TL**.
*Bir araştırma notunun kendi belirsizliğini işaretlemesi, bir sonraki
turda onu düzeltilebilir kılıyor.*

### 45.3 · C3'ün cevabı buradan çıktı — arşiv seçenek değil, zorunluluk

Kuyruk 4/C3'te mesaj arşivini *"saklanacak yeri belli olmayan veri"*
diye ertelemiş, **ne kadar süre saklanacağını** D2'ye bırakmıştık.

Cevap mevzuatta: onay kayıtları geçerlilik bittikten, **ticari
elektronik iletilere ilişkin diğer kayıtlar** kayıt tarihinden itibaren
**ÜÇ YIL** saklanır *(süre 04.01.2020'de bir yıldan üçe çıkarıldı)*.
Ve **ispat yükü gönderende**.

**C3'ün gerekçesi değişti:** arşiv *"spam kontrolü için"* değil,
**yasal zorunluluk** olduğu için yazılacak — ve o zaman alanları da
belli: kime, hangi kanaldan, ne zaman, hangi metin, hangi onaya
dayanarak.

### 45.4 · Dışa aktarma — kütüphane gerekmiyor

`arastirma/DISA-AKTARMA.md`

İlk refleks *"jsPDF + SheetJS ekleyelim"*di. Ölçünce ikisi de gereksiz
çıktı:

* **PDF = `window.print()`** + bir `@media print` stili. **0 KB**,
  Türkçe sorunsuz (tarayıcının kendi metin motoru), her cihazda aynı.
  jsPDF'in varsayılan yazı tipleri UTF-8 desteklemiyor; Türkçe için
  **TTF gömmek** gerekiyor — yüz KB'lar.
* **Excel = CSV**, ama **iki tuzağıyla**: başına **UTF-8 BOM** (yoksa
  Excel ANSI sanıp `Dükkân` → `DÃ¼kkÃ¢n` yapıyor) ve ayraç olarak
  **`;`** (Excel virgülü değil, **bölgesel liste ayırıcısını**
  kullanıyor; Türkiye'de virgül ondalık ayırıcı olduğu için ayraç
  noktalı virgül). Virgülle yazılan CSV Türkçe Excel'de **tek sütuna**
  düşer.

**Üçüncü tuzak bizim kendi kuralımızdan:** ekranda `1.900` yazıyoruz;
CSV'ye **ham rakam** (`1900`) yazılmalı, yoksa Excel'de toplam
alınamaz. K-037'nin kuralı: aynı veri, iki okuyucuya iki gösterim.

**Karar: ikisi de yazılsın, kütüphanesiz** — ama B2 (ay bazında gider
toplamı) olmadan dışa aktarılacak tablo yok.

### 45.5 · Üçünün ortak sonucu

Üç istek de **K-022'nin kapsam kararına** dayanıyor. Sanal POS alacağı,
dışa aktarma gider tablosunu, kampanya da izin altyapısını bekliyor.

> **Hiçbiri "olmaz" demiyor; üçü de "önce şu" diyor.** Ve o "şu" aynı
> şey: muhasebe kapsamının Cemil tarafından açıkça genişletilmesi.

**Ölçüm:** kod değişmedi; prototipe dokunulmadı. Üç belge, on üç
kaynak, altı doğrulanamayan nokta işaretlendi.

> **Cemil onayına açık:** **A-M20** (sanal POS faz dışı) ·
> **A-M21** (kampanya ertelendi, yerine izin alanı önerildi) ·
> **A-M22** (PDF için kütüphane değil tarayıcı yazdırması).

---

## K-044 · Kaydırma artık siler — onay değil, GERİ ALMA (07.09.2026)

**Kuyruk 4/C.** Üç karar; biri uygulandı, ikisi gerekçesiyle yazıldı ve
bilerek yazılmadı.

### 44.1 · C1 — K-032 geri alındı, gerekçesi alınmadı

Cemil: *"Gideri sola kaydırdığında sil."*

K-032'de onay satırı koymuştum ve gerekçem şuydu: **görünmeyen bir
hareket, görünmeyen bir sonuç doğurmasın** — cepte kazara kaydırmak bir
gideri silmemeli.

**Gerekçe hâlâ doğru. Çözüm değişti:**

| | Nasıl korur | Bedeli |
|---|---|---|
| Onay satırı | **Soru sorarak** — silmeden önce düşündürür | Her silme **iki dokunuş** |
| Geri alma | **Cevabı geri vererek** — sildikten sonra düşündürür | Kaza olursa bir dokunuş |

İkincisi hem ucuz hem de doğru anda çalışıyor: usta silmek istediğinde
düşünmüyor zaten — **düşünmesi gereken an, yanlışlıkla sildiği an.**

Sonuç **görünür** olduğu sürece hareket görünmez kalabilir: satır
kayboluyor ve balonda ne olduğu yazıyor, yanında *"Geri al"* duruyor.
Geri alınan satır **aynı yerine** dönüyor — sona eklenseydi usta
*"geri aldım ama başka yere gitti"* derdi.

⚠️ **AYNI HAREKET KAYIT TABLOSUNDA DA VAR.** Cemil yalnız gider için
söyledi; ikisi farklı davranırsa **aynı jest iki anlama gelir** ve usta
hangisinde ne olacağını akılda tutmak zorunda kalır. Aynısı yapıldı —
K-040 (çekmece/katman) ve K-042 (header/footer) ile aynı kural: **bir
hareket, bir anlam.** *Cemil onayına açık (A-M19).*

Denetim üç ölü sınıfı anında söyledi (`.silme-onay`, `.onay-btn`,
`.onay-satir`) ve silindi. Ölçüldü: gider 2→1→2 (sıra korundu), kayıt
satırı 3→2→3, geri alma düğmesi **44 px**.

### 44.2 · C2 — çek/senet tahsilat sayılmaz, YAZILMADI

Cemil: *"Tahsilat çek ya da senetle yapılırsa tahsilat alanında
gösterilmeli mi?"*

**Karar: gösterilir ama AYRI.** Çek elde bir kâğıttır; kasaya **girmemiş**
paradır. *"Tahsil edildi"* toplamına katılırsa muhasebe yalan söyler —
usta *"bugün 8.000 aldım"* diye okur, kasada 3.000 vardır. Vadesi
gelince tahsilata dönüşür.

**Yazılmadı:** uygulaması `tahsilat kalemi` kavramını gerektiriyor ve o
**B kümesinde** — K-022'nin kapsamını genişletme onayı Cemil'den
gelmedi. Karar hazır, kod onayı bekliyor.

### 44.3 · C3 — mesaj arşivi, YAZILMADI (bilerek)

Cemil: *"Gönderilen mesajlar kayıt edilmeli. Ard arda spam olabilecek
ya da kötü niyetli bir şey var mı kontrol edilebilir mi?"*

**Karar:** arşiv **evet**; kontrol **kural tabanlı** başlar (aynı
numaraya kısa sürede N mesaj → uyarı). Yapay zekâ ile içerik denetimi
bu fazın dışında (E kümesi).

**Neden şimdi yazılmadı:** arşiv **saklanacak ama gösterilecek yeri
olmayan** bir veri olurdu. Bu turda tam bunu eleştirdim: gösterilen ama
girilemeyen alan sonsuza kadar boş kalır (K-033.2), saklanan ama
okunmayan alan da öyle. Arşivin bir yeri olmalı — çekmecede mi, mesaj
ekranında bir sekme mi — ve o **ekran bütçesi** kararıdır (A-2).

> Ayrıca yasal bir yanı var: gönderilen mesajı saklamak, müşteri
> verisini saklamaktır. **Ne kadar süre?** `SMS-VE-IYS.md`'nin devamı
> olarak D2 araştırmasında cevaplanacak. Veri modeline alan eklemeden
> önce bu sorunun cevabı gerekiyor — *"tutmadığın veri en güvenli
> veridir"* (RUHSAT-OKUMA.md Bulgu 2).

**Ölçüm:** derleme temiz · 14 ekran · 40 test / 0 bulgu ·
`load` koşusu 0 · 320 / 375 / 430 px · silme ve geri alma iki tabloda
da uçtan uca işletildi.

---

## K-043 · İki popup değil bir katman; ve örtüler yığın oluşturur (07.09.2026)

**Kuyruk 4/A14-A15.** Cemil iki yerde popup istedi: *"Müşteri ne görecek
popup açılsın"* ve *"telefon yazabileceği basit bir popup"*.

### 43.1 · İki popup yazılmadı, bir bileşen yazıldı

İki ayrı overlay demek iki ayrı odak tuzağı, iki ayrı kapatma
davranışı, iki ayrı Esc dinleyicisi demekti — **biri mutlaka eksik
kalırdı.** Aynı gün, aynı sebeple çekmecenin odak tuzağı zaten
delinmişti (K-042.2).

`Katman` çekirdekte: ekran yalnız **başlık** ve **gövde** veriyor;
açılma, kapanma, odak, Esc, örtüye dokunma ve arka planın kilidi
bileşenin işi.

> Çekmecenin kardeşi ama aynısı değil: **çekmece yandan girer ve
> gezinmedir; katman ortada durur ve tek bir soruyu sorar.** İkisini
> tek bileşen yapmak, iki farklı işi tek ada sıkıştırmak olurdu.

### 43.2 · Önizleme artık ekran değil katman

Eskiden *"Müşteri ne görecek?"* ayrı bir ekrana gidiyordu: usta mesajı
yazıyor, bakmak için ekrandan çıkıyor, geri geliyordu. **Bakmak için
işi bırakmak gerekiyordu.**

*"Sadece mesaj gözüksün"* şöyle okundu: katman **müşterinin gerçekten
göreceğini** gösteriyor, ustanın önizleme şeridi (*"bu şerit yalnız
sende görünüyor"*) katmanda **yok**. Olsaydı zaten *"sadece müşterinin
gördüğü"* olmazdı. Mesaj metninin kendisi üstteki alanda duruyor;
ikinci kez göstermek onu tekrar etmek olurdu. **Cemil onayına açık
(A-M18).**

Gövde artık **ortak nesne** (`ozetGovdeHtml`): hem `ozet` ekranı hem
katman aynı işlevi çağırıyor. İki kopya olsaydı biri düzelir öteki
kalırdı — bu turda bunun üç örneğini gördük.

⚠️ **Önizleme taşınınca `ozet` ekranı YETİM KALDI** ve denetimin YETIM
testi anında söyledi: *"ozet | gidilmiyor"*. Ekran silinmedi — o
**müşterinin sayfasının kendisi**, linkin ucundaki şey. Katmanın
dibine *"Tam sayfayı aç"* kondu: bakmak isteyene açık, ana yola
binmiyor.

### 43.3 · Telefon yoksa çözüm uyarının yanında

Uyarı *"önce araç kaydına telefon yaz"* diyordu ve usta mesaj
ekranından çıkıp aracı bulup telefonu yazıp geri gelmek zorundaydı —
**dört ekran**. Artık uyarının altında **Telefon ekle** düğmesi var;
katman açılıyor, numara yazılıyor, kaydediliyor ve gönderme düğmeleri
kendiliğinden açılıyor.

Ölçüldü uçtan uca: boş kaydet → *"Telefonu yaz"*; geçersiz → *"11 hane
olmalı"*; `05321112233` → **`0532 111 22 33`** olarak biçimlendi, araca
yazıldı, üç düğme de açıldı.

> **Sesle giriş bu turda yazılmadı.** Cemil *"yazabileceği/
> söyleyebileceği"* dedi; mikrofon ses ekranına yeni bir niyet
> (`telefon`) ve çözümleyici işi gerektiriyor. **Yarım çalışan bir
> mikrofon koymaktansa yazmadım** — kuyruğa A16 olarak düşüldü.

### 43.4 · Asıl bulgu: örtüler **yığın** oluşturur

İlk yazımda arka planı kapatan işlev *"aç"* derken `inert` **ekliyor**,
*"kapat"* derken **kaldırıyordu**. Tek örtü varken doğru; ikinci örtü
gelince yanlış:

**Katman kapanınca çekmecenin koyduğu kilidi de kaldırıyordu** ve
arkadaki **17 öğe** sekme sırasına geri dönüyordu.

Ölçüm ilginçti: hata ancak denetim **iki kez** koşturulunca çıkıyordu.
Sebep zincirleme — denetimin 30. testi her düğmeye basıyor, düğmelerden
biri artık **katman açıyor**, denetim onu **açık bırakıyor**, sonraki
koşuda çekmece açılıp kapanıyor ve herkesin kilidi birden kalkıyor.
*(K-041.2'nin akordiyonlar için öğrendiği ders, yeni bir örtüye
uygulanmamıştı — bu turun dördüncü kez tekrarlayan dersi.)*

Çözüm sayaç değil **yığın**: hangi örtüler açık sırasıyla tutuluyor,
kilit her zaman **en üstteki örtüye göre** yeniden hesaplanıyor.
Kapanan örtü kendi kaydını siliyor, altındakinin kilidi duruyor.

**40. test** eklendi (KATMAN-ODAK-SIZINTISI + ORTU-YIGINI) ve **iki
negatif testle** doğrulandı: odak tuzağı bozulunca 26 öğe, yığın
bozulunca *"çekmecenin kilidi kalktı"*; düzeltilince ikisi de sustu.

**Ölçüm:** derleme temiz · 14 ekran · **40 test** / 0 bulgu ·
`load` koşusu 0 · 320 / 375 / 430 px · katman ve telefon ekleme uçtan
uca işletildi.

---

## K-042 · Alt gezinme bir kopya değil, aynı denetimin ikinci gövdesi (07.09.2026)

**Kuyruk 4/A.** Cemil'in on dört maddesi; onu metin ve kaldırma, dördü
yapısal. Yapısal olan biri üç kusur doğurdu ve üçü de **aynı cinsten**.

### 42.1 · Aynı iş iki yerde durur, iki kere yazılmaz

Cemil: *"Footerda da navigasyon tuşları olsun. Headerdaki ile
entegre/birlikte çalışsın."*

Kolay yol: alta iki düğme daha koyup `id`'lerini `geriBtnAlt`,
`ileriBtnAlt` yapmak. **Yapılmadı** — o zaman geri davranışı iki yerde
yazılırdı ve bir gün biri değişip öteki kalırdı. v1'in en pahalı
alışkanlığı buydu.

`id` yerine **`data-nav`**: üstteki ve alttaki tuşlar aynı denetimin
iki gövdesi. Tek delegasyon ikisini de dinliyor, tek `geriTazele()`
ikisini de soluklaştırıyor. Ölçüldü: gezinilmemişken **ikisi de** kapalı,
gezinince **ikisi de** açık.

> Alta ayrıca **ana menü** tuşu kondu. Logo zaten ana ekrana gidiyor
> ama logo üstte; alt çubuğun amacı **başparmakla ulaşılabilirlik**ti,
> ortasını boş bırakmak o amacı yarım bırakırdı.

### 42.2 · Bir liste, bir gün eksik kalır

Alt çubuk eklenir eklenmez denetim **DRAWER-ODAK-SIZINTISI** verdi:
çekmece açıkken footer'ın tuşları sekme sırasında kalıyordu.

Sebep: `arkaPlan()` kapanacak öğeleri **tek tek sayıyordu**
(`.topbar` + `.screen`ler). Yeni kardeş o listeye kendini eklemez.

Kural yapıdan okunacak biçimde yeniden yazıldı: **çekmece ve örtüsü
dışındaki her `.device` çocuğu kapanır.** Yarın bir çubuk daha
eklenirse kendiliğinden kapsanır. Ölçüldü: çekmece açıkken dışarıda
odaklanabilir öğe **0**.

### 42.3 · Ölçemediğini bilen araç, yine de suçluyordu

Aynı anda yükleme koşusu **on bulgu** verdi. Hepsi sahteydi: önizleme
bölmesi sıfır genişlikte açılıyor, her kutunun `clientWidth`i 0 ve
taşma testleri patlıyor.

⚠️ **`OLCULEBILIR` bayrağı zaten vardı** — 05.09'da tam bu iş için
yazılmış, ama **yalnız bir teste** bağlanmıştı. Sonradan eklenen düzen
testleri onu hiç sormadı. Denetim `olcum: ATLANDI` diye yazdığı hâlde
on kutuyu suçluyordu.

**K-041.3'ün aynı dersi:** bir kural bir yerde uygulanmışsa ötekine de
uygulanacağı garanti değildir. Bayrak artık tek tek testlere değil,
**`ek()`in kendisine** bağlı — yeni bir düzen testi yazanın
hatırlaması gerekmiyor.

Bir de 39. testin kendi yanlış pozitifi çıktı: `textarea` metni
**sarar**, yatay kırpma onun hata biçimi değil. Ölçülen 15-16 px fark
dikey kaydırma çubuğunun payıydı. Test artık yalnız `<input>`e bakıyor.

### 42.4 · Kalanlar — hepsi Cemil'in tarifiyle

| Ne | Neden |
|---|---|
| *"İçeride"* → **"Serviste"** | Tek terim. Tamircinin dili |
| Sürüm/tanı şeridi **gizlendi** | Hata ayıklama aracıydı ve her ekranın en üstünde duruyordu. Silinmedi: `TAMIRON_TESHIS()` ile konsoldan okunuyor. **Görünür olması gereken değil, sorulduğunda bulunabilir olması gereken** bir şey |
| Ruhsat ekranından **km kalktı** | Kayıt açma anı, ustanın göstergeye bakmadığı andır. Sesin duyduğu km kayboluyor değil — doğrudan kayda işleniyor |
| *"Şunları diyebilirsin"* yalnız **boş hâlde** | Usta konuşmuşken ona başka cümleler önermek, söylediğinin yanlış olduğunu ima ediyor. Çipler silinmedi: prototipte konuşmanın tek yolu onlar (A-M16) |
| Kayıtsız araç metni kısaldı | Uzun cümle, ekrandaki iki düğmenin zaten söylediğini tekrar ediyordu |
| *"Ne istiyor?"* → **"Açıklama"** | Soru biçimi alanı daraltıyordu; alan işin kendisini taşıyor |
| *"Alt satıra ekle"* **kalktı** | Benim uydurduğum bir sorunu çözüyordu — `textarea` zaten çok satırlı. Klavyenin işini ekrana taşımıştım |
| Sahibi/iletişim **aynı dilde** | İkisi ayrı bileşenlerden geliyordu ve aynı soruya iki görünüm veriyordu. Rol küçük, **ad büyük**; boşluk `gap` ile — metnin içindeki tek boşluğa güvenilmiyor |
| Gider **+** mikrofonla yan yana | `space-between` üç çocuğun **arasını** eşitliyordu; + ortada kalıyordu. `.lbl-eylem` ikisini tek çocuğa indirdi |

**Ölçüm:** derleme temiz · 14 ekran · 39 test / **0 bulgu** ·
`load` koşusu 0 (ve *"ATLANDI"* diyor) · 320 / 375 / 430 px · alt
gezinme, çekmece odağı ve kişi satırı elle işletildi.

> **Cemil onayına açık (A-M16):** ses ekranındaki örnek çipleri boş
> hâlde duruyor. Tamamen kalksın mı? Kalkarsa prototipte sesli yol
> denenemez (gerçek mikrofon yalnız https'te çalışır — K-015).

---

## K-041 · 320 px'te göz gördü, 38 test görmedi (06.09.2026)

**Kuyruk 9.** Denetim üç genişlikte de 0 diyordu. Ekran görüntüsüne
bakınca servis kaydı tablosunda şunlar yazıyordu:

> **Motor yağ** · **Yağ filtres** · **Bakım işç**

### 41.1 · `<input>` sessizce keser

Ölçüldü (320 px): ad alanı **65-66 px** görünüyor, metin **71-88 px**
istiyor. Taşma **5-23 px**.

`<input>` taşan metni ne sarar ne üç nokta koyar — **sessizce keser.**
Ustanın gördüğü *"Bakım işç"*; eksik olduğunu söyleyen hiçbir işaret
yok. K-025'in kuralı: **kesilen metin, taşan metinden tehlikelidir.**

11. test (`KIRPILAN-METIN`) `overflow:hidden` kutulara bakıyor;
**form alanları onun kapsamında değildi.** Kural belgede vardı, o
yüzeye hiç uygulanmamıştı.

**Düzeltme, yalnız dar ekranda:** tutar sütunu 96 → 74 px, tip rozeti
küçülüyor. Kazanç ~35 px; ad alanı 65 → **108 px** oldu ve üç satırın
üçünde de taşma **sıfır**. 375 px ve üstünde hiçbir şey değişmedi —
dar ekranın çözümü geniş ekrana bulaşmaz.

**39. test (SESSIZCE-KIRPILAN-ALAN)** eklendi: değeri olan görünür her
`input`/`textarea` için `scrollWidth > clientWidth`. Negatif testle
doğrulandı (alan 40 px'e daraltıldı → yakaladı; geri alındı → sustu).

### 41.2 · Denetim VERİYİ geri yüklüyordu, DOM'u yüklemiyordu

Düzeltmeyi ölçerken denetim ruhsat akordiyonu için iki bulgu vermeye
başladı — akordiyon **çalışıyordu**.

Mekanizma: 28. test her akordiyon başlığına **iki kez** basıyor (aç,
kapat — restore). 30. test ise **her düğmeye bir kez** basıyor ve
akordiyon başlığı da bir düğme. Net sonuç **tek tıklama**: denetim
akordiyonu **açık bırakıyordu.** Bir sonraki koşuda 28. test onu açık
buluyor, ilk tıklaması **kapatıyor** ve iki yanlış bulgu veriyordu.

K-024.1'de denetimin **veriyi** kirletmesini düzeltmiştik (derin kopya
+ geri yükleme). Bu, aynı hatanın ikinci yüzü: **DOM durumu.**

Uzun süre görünmedi çünkü ruhsat ekranı her çizimde alanlarını
temizliyordu. `ac` kancası gelince (K-040) ekran durumunu korumaya
başladı — **doğru davranış, gizlenen hatayı ortaya çıkardı.**

### 41.3 · Aylardır hiç bakmadığım bir koşu vardı

Bunu kovalarken şunu fark ettim: denetim `load` anında **kendiliğinden**
koşuyor ve sonucu `window.DENETIM`'e yazıyor. Ben her seferinde
`DENETIM_CALISTIR()` çağırıp **kendi koşumun** sonucunu okuyordum.

`window.DENETIM`'e bakınca **1 bulgu** çıktı:

> `OLU-ID | getElementById('kToplam') — böyle bir öge yok`

Yanlış pozitifti: `#kToplam` servis kaydının tablosuyla birlikte **JS
tarafından** yazılıyor; ekran çizilmeden DOM'da yok. Elle koşturunca
30. test kayıt ekranını satırlarıyla çizmiş oluyor ve id var oluyordu —
bu yüzden **yalnız ilk koşuda** görünüyordu ve ben ilk koşuya hiç
bakmıyordum.

⚠️ **Bu dersin bir kopyası `TASARIM-SISTEMI.md`'de zaten yazılıydı:**
*"TANIMSIZ-SINIF testi `.plate-lg`'yi GEÇ yakaladı — onu JS üretiyor,
ekran çizilmeden DOM'da olmuyor."* Ders belgede duruyordu, bu teste
uygulanmamıştı. **Yazılmış bir ders, uygulanmamışsa öğrenilmemiştir.**

Çözüm DOM'u kurcalamak değil: betiğin kendisi o id'yi yazıyorsa
(`id="kToplam"`) referans ölü değildir. Statik, yan etkisiz; bir yazım
hatası yine yakalanır — yanlış yazılan id ne DOM'da ne betikte bulunur.

**Ölçüm:** derleme temiz · 14 ekran · **39 test** · `load` koşusu **0**,
arka arkaya üç elle koşu **0** · 320 / 375 / 430 px · 320'de sekiz
ekran gözle gezildi.

---

## K-040 · `ciz` iki iş yapıyordu: açılış ile çizim ayrıldı (06.09.2026)

**Kuyruk 8.** İki yeni denetim testi yazıldı (36 → **38**) ve biri
yazılırken asıl mesele çıktı: bu turda aynı hatayı iki kez yapmamın
sebebi dikkatsizlik değil, **çekirdeğin yapısıydı.**

### 40.1 · Test 38'in ilk tasarımı boştu — yazmadan önce yakalandı

Önce şöyle bir test tasarladım: *ekranı çiz, `aria-pressed="true"`
sayısını say, `Ekran.tazele()` çağır, yeniden say; azaldıysa bulgu.*

Yazmadan önce **dünkü iki hataya karşı denedim:**

* `kTurElle` — tür seçimi kaydın üstünde durduğu için çip **basılı
  kalıyor**; sayı düşmüyor. ✗
* `kSilinecek` — silme onayında `aria-pressed` **hiç yok**. ✗

**İkisini de yakalamıyordu.** 29. testte bir kez yaşadığımız şeyin
aynısı olurdu: 0 bulgu veren, hiçbir şeye bakmayan bir test.
**Boş test, testsizlikten kötüdür** — yanlış güven verir.

### 40.2 · Kök sebep: `ciz` hem "açıldı" hem "yeniden çiz" demekti

İki hatanın ortak yanı şuydu: **açılışa ait bir sıfırlama `ciz`
içindeydi.** `Ekran.tazele()` de `ciz`i çağırdığı için her tazeleme
kullanıcının seçimini siliyordu. Ekran, "açıldım" ile "yeniden
çiziliyorum" arasını **ayırt edemiyordu.**

Çekirdeğe `ac` kancası eklendi:

```
ac(el)   — ekran AÇILDIĞINDA bir kez  (yalnız goster'den)
ciz(el)  — her çizimde               (goster ve tazele'den)
```

Açılışa ait sıfırlamalar taşındı: `teslim` (ödeme seçimi, teslim
hâli), `ruhsat` (alanların temizlenmesi, sekme, ön dolgu), `kayit`
(silme onayı), `ses` (duyulan cümle).

**Üçü latent hataydı.** Bugün onlara ulaşan bir yol yoktu — ama
ruhsat ekranına `Ekran.tazele()` çağıran tek satırlık bir ekleme,
ustanın yazdığı plakayı silecekti ve o satırı yazan kişi bunu
bilemezdi.

> İlk çözümüm (`kCizilenKayit` bekçisi, K-033.12) doğru çalışıyordu ama
> **sorunun kendisini çözmüyordu**: `ciz` hâlâ açılış işi yapıyordu ve
> aynı tuzak öteki ekranlarda duruyordu. Bir örneği düzeltmek, sınıfı
> düzeltmek değildir.

`sesleAc` de düzeldi: ses ekranı açıkken başka bir mikrofona basmak
**yeni bir açılıştır** (bağlam değişiyor, duyulan cümle bayatlıyor).
`tazele()` yerine `goster()` çağırıyor.

### 40.3 · Test 37: kapalı düğme sebebini KENDİ üstünde taşır

Cemil'in *"Kaydı Aç'a bastım, bir şey olmuyor"* şikâyeti (K-033.3).
Ölçülen: görünür ve `disabled` bir düğme, sebebini `aria-describedby`
ya da `title` ile **kendine bağlı** taşıyor mu.

⚠️ **Neden "ekranda bir yerde açıklama var mı" diye bakmıyor:**
baksaydı K-033.3'ü **kaçırırdı**. O ekranda uyarı şeritleri vardı — ama
başka şeyler hakkında. Ekranda bir açıklama bulunması, **bu** düğmenin
neden kapalı olduğunu söylemez. Kural tek cümle: **ya düğmeye bağla, ya
düğmeyi kapatma.**

İki bulgu verdi, ikisi de düğmeyi **konuşturarak** kapandı:

* `psGonder` — parçacı seçilmeden basılınca *"Önce kime soracağını
  seç."*; sorulacak parça yokken *"Sorulacak parça yok."* (kapalı düğme
  bu **iki ayrı sebebi aynı sessizlikle** söylüyordu)
* `tsTeslim` — *"Ödemeyi nasıl aldığını seç — almadıysan 'Almadım' de."*

Kapalı kalması doğru olan ikisinde sebep düğmeye bağlandı: mesaj
ekranının üç kanalı `aria-describedby="msjUyari"`, araç ekranının
mesaj düğmesi `title`.

### 40.4 · Testin kendisi de ölçülür — 38 iki kez yanlış bulgu verdi

`var tAlinan = 0, tParca = 0, tFark = 0;` dizisinde `var` yalnız
ilkinin önünde duruyor; ikinci ve üçüncüyü **sıfırlama sandı**.
İlk düzeltmem de yetmedi (virgülü dışladım ama `tFark`ın hemen solunda
virgül değil **boşluk** vardı). Doğru çözüm karakter dışlamak değil,
**bildirimin tamamını atmak**tı.

Her ikisi de negatif testle doğrulandı: hata enjekte edilince ateşledi,
`title` eklenince ve yerel bildirimde sustu.

### 40.5 · Beşinci ölçüm tuzağı

`ac` kancasının ses cümlesini temizlemediğini sandım. Temizlemişti —
element `hidden` idi ve ben `textContent` okumuştum; **`textContent`
gizli metni de verir.** `innerText` vermiyor.

Bunun üstüne bir de gerçek bir küçük düzeltme çıktı: gizlenen öğenin
içeriği DOM'da duruyordu. Görünmüyor ama okunuyor — beni yanılttıysa
denetimi de yanıltır. **Ekranda olmayan şey DOM'da da durmasın.**

**Ölçüm:** derleme temiz · 14 ekran · **38 test** / 0 bulgu ·
320 / 375 / 430 px · dört ekranda açılış/tazeleme davranışı elle
doğrulandı (plaka, ödeme seçimi, silme onayı, duyulan cümle).

---

## K-039 · Tema turu temiz çıktı; asıl bulgu ÖLÇÜM ARACINDA (06.09.2026)

**Kuyruk 7.** Bu iş bir hata bulmadı; **üç ölçüm hatası** buldu ve ikisi
benim kendi aracımdaydı. Yazılmasının sebebi bu.

### 39.1 · Kuyruğun kendi varsayımı yanlıştı

Kuyruğa şunu yazmıştım: *"bu turun bileşenlerini çoğunlukla koyu temada
gördüm."* Ölçünce `prefers-color-scheme` **açık** çıktı — yani K-033
turunun bütün ekran görüntüleri ve bütün denetim koşuları **açık
temadaydı.** Ölçülmemiş olan koyu temaydı; tam tersi.

> Kuyruğa yazılan gerekçe de bir iddiadır ve o da ölçülür.

Kuyruğa ayrıca *"denetimin kontrast testi rakamı verir"* yazmıştım.
**Öyle bir test yok.** Denetimin 36 testi arasında `KOYU-SIYAH` ve
`TEMA-UYUSMAZLIGI` var; kontrast oranı ölçen bir test yok. Kontrast bu
turda **elle** ölçüldü.

### 39.2 · Sonuç: iki temada da temiz

Denetim üç hâlde de koşturuldu — `data-theme="light"`, `"dark"` ve
sistem: **üçünde de 0 bulgu.** K-033 turunun yeni bileşenleri için WCAG
kontrast oranı elle hesaplandı (eşik: normal metin 4.5):

| Bileşen | Açık | Koyu |
|---|---|---|
| Tablo ad alanı | 15.20 | 13.69 |
| `tutar-disi` (üzeri çizili tutar) | **5.00** | **5.28** |
| `k-alis` (parçacı fiyatı) | **5.00** | **5.28** |
| Tablo başlığı · çipler | 6.28 | 7.75 |
| `ak-tutar` · `ak-is-ne` · balon | 16.82 | 14.06 |
| `ms-say` · `ms-arac-ad` | 7.57 | 8.15 |

En düşük **5.00** — eşiğin üstünde. Koyu temada ekran görüntüsü de
alındı: çerçevesiz tablo alanlarının kesik alt çizgisi, zebra şeridi ve
üzeri çizili tutar okunuyor.

### 39.3 · Ölçüm aracının iki hatası — ikisi de "bulgu" gibi göründü

**(a) `color(srgb …)` okunamıyordu.** Zebra şeridinin zemini bu
biçimde geliyor; ayrıştırıcım `0.886275` gibi 0–1 aralığındaki
değerleri 0–255 sanıp parlaklığı sıfıra yakın hesapladı. Sonuç: tablo
ad alanı için **1.24** kontrast — yani "görünmez". Gerçek değer
**15.20**.

**(b) Tema geçişinin ara karesi okunuyordu.** `data-theme` değişince
`.icon-btn`in zemini `0.15s` geçişle kayıyor. Geçiş bitmeden okunan
`backgroundColor`, iki temanın **arasında** bir renk veriyordu:
geri düğmesi açık temada `rgb(48,58,72)`, koyu temada
`rgb(175,179,184)` — ikisi de yanlış, ikisi de "kontrast 1.5" gibi
korkutucu bir rakam üretiyordu. İki `requestAnimationFrame` **yetmedi**;
geçiş süresi kadar (700 ms) beklemek gerekti.

> **Kural:** tema değiştirdikten sonra renk okunmaz — **geçiş bitene
> kadar beklenir.** Bir sonraki oturum aynı tuzağa düşmesin diye
> `HANDOFF.md`'ye yazıldı.

Bu, bu oturumda ölçüm aracımın beni **dördüncü** yanıltışı (önce
kopmuş DOM düğümüne tıklama, sonra `innerHTML.length` imzası, sonra
paylaşılan `muhDonemSecili` kirlenmesi). Hepsinin ortak dersi aynı:
**önce "bulgu" diye görünen şeyin ölçüm hatası olup olmadığına bakılır.**
K-024.1 sağlık denetimi için yazılmıştı; her ölçüm için geçerli.

### 39.4 · Kontrast testi denetime girsin mi?

Girebilir — bu turda elle yapılan hesap otomatikleştirilebilir. Ama iki
şartı var: `color(srgb …)` biçimini okumalı ve **tema geçişini
beklemeli**; ikisini de bugün elle öğrendik. **8. işe aday olarak
düşüldü**, orada tartılacak.

**Ölçüm:** derleme temiz · 14 ekran · 36 test / 0 bulgu ·
açık · koyu · sistem · 320 / 375 / 430 px.

---

## K-038 · Müşterinin sayfası, ustanın ekranından geri kalmış (06.09.2026)

**Kuyruk 6.** `ozet` müşterinin gördüğü tek sayfa (K-023). K-033 turunda
kaydın yapısı değişti; sayfa değişmedi. Üç yerden geri kalmış:

### 38.1 · Çok satırlı şikâyet tek satıra yapışıyordu

Dün *"ne istiyor"* alanı `textarea` oldu ve **alt satıra ekleme**
düğmesi geldi (K-033.5) — Cemil'in isteği: *"x işi için geldi, sonra
bir şey daha eklemek isteyebilir."* Usta artık üç ayrı iş yazıyor.

Özet sayfası metni düz basıyordu; HTML satır sonlarını **boşluğa**
çeviriyor. Müşteri üç işi tek cümle hâlinde okuyordu.

Ustanın ekranında sorun yoktu — orası `textarea`, satırları kendi
gösteriyor. Kırılan yer, metnin **metin olarak basıldığı tek yerdi**.

> **Ders:** bir alanı çok satırlı yapmak, onu **gösteren her yeri**
> gözden geçirmeyi gerektirir. Alanın kendisi çalışıyordu; kırılan,
> onu okuyan uçtu.

### 38.2 · Yarım satır müşteriye soru işareti gibi gidiyordu

Usta *"+ Parça"*ya basıp adını henüz yazmamışsa satır boş duruyor.
Müşterinin sayfasında bu **"Parça — "** diye çıkıyordu: bilgi değil,
soru işareti.

**Ölçü:** adı **da** fiyatı **da** yoksa satır yok sayılır. Biri varsa
gösterilir — adı olup fiyatı olmayan satır gerçek bir iştir (fiyatı
henüz girilmemiş), fiyatı olup adı olmayan satır gerçek bir paradır.
Yalnız ikisi birden boşsa ortada bir şey yoktur.

⚠️ **Ustanın ekranında elenmiyor** — orada yarım satır **görünmeli** ki
usta onu doldursun ya da silsin. K-037.4'ün kuralı burada da işliyor:
aynı veri, iki okuyucuya iki gösterim. Usta **kaydı**, müşteri
**faturayı** okuyor.

### 38.3 · Sayfa ile mesaj iki farklı hikâye anlatıyordu

Mesaj şablonu *"Parça bekliyoruz"* diyordu (K-033.9); sayfa *"Aracınız
serviste. Hazır olduğunda haber vereceğiz."* diyordu. Müşteri mesajı
okuyup linke dokununca **iki ayrı hikâye** görüyordu — ve **canlı olan,
yani doğru saydığı, sayfaydı.**

Sayfa artık parça beklendiğini söylüyor. **Söz vermiyor:** *"yarın
hazır"* demiyoruz — parçanın ne zaman geleceğini biz de bilmiyoruz ve
tutulmayan söz, hiç verilmemiş sözden kötüdür (aynı gerekçe mesaj
şablonunda da yazılı).

**`fiyatiBekleyen` burada YOK ve olmayacak:** o dükkânın kendi işi
(parçacıdan rakam gelmedi). Müşteriyi ilgilendiren şey parçanın yolda
olması. K-036'da ayırdığımız iki bekleme, burada da ayrı kalıyor.

> Varsayım — **Cemil onayına açık (A-M15):** parça beklendiğini
> müşteriye **söylüyoruz**. Söylemezsek *"neden bu kadar sürdü"* sorusu
> havada kalır; söylersek müşteri aramaz — tamircinin şikâyet ettiği
> telefon trafiği azalır. Ters düşünen varsa: bazı usta işin
> gecikmesini müşteriye duyurmak istemeyebilir.

**Ölçüm:** derleme temiz · 14 ekran · 36 test / 0 bulgu ·
320 / 375 / 430 px · dört davranış tek tek yürütüldü (durum metni,
üç satırlık şikâyet, yarım satırın elenmesi, adı olup fiyatı olmayan
satırın görünmesi).

---

## K-037 · Kural veriyi silerek işletilmez — müşteri parçası toplamda elenir (06.09.2026)

**Kuyruk 5.** Sorulan soru şuydu: *"Parçaları müşteri getirdi"* anahtarı
`fiyat`ı `alis`e taşıyor; muhasebe o parayı **ödenmiş** gibi sayıyor mu?

### 37.1 · Cevap: hayır. Muhasebe doğruydu

`isKari()` ve `parcaAlislari()` zaten `kimGetirdi === 'tamirci'`
süzüyordu; müşterinin getirdiği parça alış toplamına **hiç girmiyordu.**

Rakamla doğrulandı — 2026 iş başına:

| | alınan | parça alışı | fark |
|---|---|---|---|
| Başlangıç | 12.970 | 7.160 | **5.810** |
| Bir parça müşteriye geçirilince | 10.870 | 5.560 | **5.310** |

Elle hesap: alınan −2.100 (satış), alış −1.600 (o parçayı almadık),
fark −500 (o parçanın kârı). **Tutuyor.** Şüphem yanlıştı ve bu, işin
doğru sonucudur: doğrulama, hata bulmak zorunda değildir.

### 37.2 · Ama anahtar, satış fiyatını yok ediyordu

Kontrol sırasında başka bir şey çıktı. Anahtar şunu yapıyordu:

```js
if (kim === 'musteri') { if (s.fiyat && !s.alis) s.alis = s.fiyat; s.fiyat = 0; }
```

**Tek dokunuş, geri alınamaz kayıp.** Anahtarı açıp kapatan usta,
yazdığı satış fiyatını bir daha bulamıyordu. Ölçüldü: 5.810 → 5.310 →
**3.710**. Üçüncü rakam yanlıştı — iş, satmadığı bir parçayı almış
görünüyordu.

Bu, **K-032'nin kuralının aynısı**: görünmeyen bir sonuç doğuran bir
dokunuş olmamalı. Orada sola kaydırma bir gideri siliyordu; burada bir
anahtar bir fiyatı siliyor.

İkinci sorun daha sessizdi: `alis` **bizim ödediğimiz** paradır. Müşteri
kendi parçasını getirdiyse biz hiçbir şey ödemedik; oraya parçacının
teklifini yazmak alanın anlamını bozuyordu.

### 37.3 · Kural yerini değiştirdi, hükmünü değil

**S2/K-005 aynen duruyor:** müşterinin getirdiği parça faturaya girmez.
Değişen tek şey, kuralın **nerede işletildiği**:

* önce: satırın `fiyat`ı **sıfırlanıyordu** → veri gitti
* şimdi: `kayitToplami()` müşteri parçasını **toplama katmıyor** → veri duruyor

> **Genel kural:** bir iş kuralı, veriyi **silerek** değil, **okurken
> eleyerek** işletilir. Silme geri alınamaz; eleme her zaman geri
> alınabilir ve nedeni okunabilir yerde durur.

Ölçüldü, gidiş-dönüş tam: 5.810 → 5.310 → **5.810**.

### 37.4 · Rakam duruyorsa GÖRÜNMESİ ayrı bir karar

Fiyat artık silinmediği için iki ekranda ne göstereceğimizi ayrıca
seçmek gerekti:

* **Ustanın ekranı:** rakam duruyor ama **üstü çizili** ve soluk —
  usta girdiğini görsün, toplamda aramasın. Alan düzenlenebilir kalıyor;
  parçacının verdiği rakam ileride işine yarayabilir (Kol D).
* **Müşterinin özeti:** rakam **hiç gösterilmiyor**, `—` yazıyor.
  Ödemediği bir parçanın yanında rakam görmek, ondan ücret isteniyormuş
  gibi okunur; üstelik toplam onu içermiyor — ekranda olup toplamda
  olmayan bir rakam, müşteri açısından **açıklanamaz bir çelişkidir.**

Aynı veri, iki okuyucuya iki farklı gösterim. Bu bir tutarsızlık değil:
usta **kaydı**, müşteri **faturayı** okuyor.

**Ölçüm:** derleme temiz · 14 ekran · 36 test / 0 bulgu ·
320 / 375 / 430 px · muhasebe rakamları gidiş-dönüş doğrulandı.

---

## K-036 · İki "bekliyor" ayrıldı; ulaşılamaz kod silindi (06.09.2026)

**Kuyruk 4.** K-034 ikisinin **ayrı gerçek** olduğunu yazmıştı; bu
karar onu **ekranlara** uyguluyor.

### 36.1 · Tek rozete binmişlerdi

Servis kaydı kartı şöyle yazıyordu:

```js
(k.parcaBekliyor || fiyatiBekleyen(k)) ? 'parça bekliyor' : ''
```

Yani **fiyatı girilmemiş bir satır**, kartta *"parça bekliyor"* diye
görünüyordu — parça tezgâhta dururken. Usta listeye bakıp *"bu araç
parça bekliyor"* diye okuyor, aslında beklenen **parçacının rakamı**.

Artık iki ayrı rozet:

| Rozet | Kaynak | Ne demek |
|---|---|---|
| `parça bekliyor` (warn) | `parcaBekliyor` — usta işaretler | parça **yolda** |
| `fiyat bekleniyor` (muted) | `fiyatiBekleyen()` — türetilir | **rakam** gelmemiş |

İkisi aynı anda doğru olabilir; o zaman ikisi birden çıkar ve bu
doğrudur. 320 px'te ölçüldü: dört rozet iki satıra sarıyor, durum
yine sağda kalıyor, kırpılma yok.

**Teslim ekranı da öğrendi.** Bayrak kayıtta duruyordu ama teslim
ekranı ondan habersizdi: *"parça bekliyor"* işaretli bir araç, işaret
hâlâ açıkken teslim edilebiliyordu ve ekran hiçbir şey demiyordu.
Artık söylüyor — **engellemiyor**. Bayrak durum değildir (K-034);
teslimi kilitlemez. Usta parçayı almış ve işareti kaldırmayı unutmuş
olabilir, o zaman bu satır ona hatırlatır.

### 36.2 · `kayitSatiriHtml`'in yarısı bir gündür ulaşılamazdı

Fonksiyonun iki hâli vardı: `sade` ve tam. Tam hâl marka/model,
sahibi, km, satır sayısı, telefon, ödeme yolu ve *"parçayı müşteri
getirmişti"* rozetini çiziyordu — yaklaşık **60 satır**.

**Hiçbir yer onu çağırmıyordu.** Tek çağrı `{sade:true}` ile ve tek
yerden (araç ekranının geçmiş listesi). Ana liste 05.09'da
`kayitKartiHtml`e geçmişti; tam hâl o gün ulaşılamaz kalmış ve kimse
fark etmemişti.

⚠️ **Ulaşılamaz JS, ölü CSS'ten sinsidir.** Denetimin OLU-SINIF testi
**sınıfları** tarıyor; çağrılmayan bir JS dalını göremiyor. Kod
duruyor, okunuyor, *"bu da var"* sanılıyor ve çürüyor — nitekim
içindeki `s.tel`, `s.tutar`, `s.plaka` seçeneklerini kimse geçmiyordu.

Silindi. *"İleride lazım olur"* gerekçe değildir (CLAUDE.md). **A-M3
hâlâ açık** — Cemil *"o rozet geri gelsin"* derse o zaman yazılır;
duran ama çalışmayan bir kod, o kararı vermiş gibi yapıyordu.

### 36.3 · Bunu yakalayacak bir test yazılabilir mi?

Kısmen. *"Hiç çağrılmayan fonksiyon"* denetimi statik analiz ister ve
prototipin tek dosyalık yapısında güvenilmez (her şey global). Ama
**ölçülebilir bir yakını var**: bir fonksiyonun bir **seçeneği** hiç
geçilmiyorsa o seçenek ölüdür. Bu, 8. işteki test önerilerine eklendi
— yazılabilirliği orada tartılacak.

Bu kez yakalayan şey bir test değil, **kuyruğun kendisi** oldu: *"her
ekranda hangisi yazıyor"* diye tek tek bakınca ortaya çıktı.

**Ölçüm:** derleme temiz · 14 ekran · 36 test / 0 bulgu ·
320 / 375 / 430 px · rozetler sekiz kartta tek tek doğrulandı.

---

## K-035 · Ana yol 12 değil 13; sadeleştirme bir yerde ödendi (06.09.2026)

**Kuyruk 3.** K-033 turunda akış üç yerden değişti ve *"herhalde
azalmıştır"* diye düşünmek kolaydı: koca bir soru (tür seçimi) ana
yoldan kalkmıştı. **Ölçünce arttığı çıktı.**

### 35.1 · Ölçüm

Beş yol da tarayıcıda **baştan sona yürütüldü**, dokunmalar sayıldı
(klavye sayılmıyor — 05.09'un kuralı).

| Yol | 05.09 | **06.09** | Fark |
|---|---|---|---|
| Tam yol (ruhsat çekilerek, parçacıya sorularak) | 12 | **13** | **+1** |
| Kısa yol (ruhsat çekilir, fiyat sorulmaz) | 9 | **10** | **+1** |
| **Ruhsatsız yol** *(yeni)* | — | **8** | — |
| Kayıtlı araç yolu | 8 | **8** | 0 |
| Sesli yol | 2 + konuşma | **2 + konuşma** | 0 |
| Müşteriye özet | 2 | **2** | 0 |

### 35.2 · +1 nereden geldi

| Değişiklik | Etki |
|---|---|
| *"Ne için geldi?"* kalktı (K-033.1) | **−1** · her yolda |
| Ruhsat sekmesine geçiş (K-033.2) | **+1** · yalnız kamera/galeri yolunda |
| Teslimden sonra *"Geç / Kapat"* (K-033.6) | **+1** · her yolda |

Kayıtlı araç yolunda sekme yok — −1 ile +1 birbirini götürdü, sayı
değişmedi. **Ruhsat kullanılan yollarda net +1.**

### 35.3 · Ödediğimiz şeyin karşılığında ne aldık

**Teslim sonrası dokunuş, bir yalanın bedeli.** Eskiden teslim balonu
telefonu olan araçta *"Özet gönderildi."* diyor ve hiçbir şey
göndermiyordu (K-033.6). Şimdi ekran gerçekten soruyor.

Ve bu dokunuş **müşteriye özet gönderen usta için ucuz**: eskiden
teslimden sonra özet göndermek için menüye dönüp mesaj ekranını bulmak
gerekiyordu (3+ dokunuş); şimdi tek dokunuş. Yani *altıncı işi yapan*
usta kazandı, yapmayan **bir dokunuş** ödedi.

> **Cemil onayına açık (A-M14):** teslimden sonraki *"Geç / Kapat"*
> dokunuşu kabul mü? Alternatifi, teslim edilir edilmez ana ekrana
> dönmek ve özet teklifini hiç göstermemek — o zaman altıncı iş yine
> gömülür. Zamanlayıcıyla kendiliğinden kapanmak **istenmiyor**: gizli
> bir sayaç, ustanın okumadığı bir şeyi kapatır.

### 35.4 · A-8 ölçümle kapandı

`IS-AKISI.md` A-8: *"Tamirat/bakım seçimi 1. adımda mı sorulsun?"* —
o gün önerim **"açıkça sorulsun, belirsizlik bırakmaktan iyidir"** idi.
Cemil sordurtmadı, ölçüm de onu haklı çıkardı: soru ana yolda bir
dokunuş yiyordu ve kalkınca **hiçbir şey kaybolmadı** (tür metinden
okunuyor, kayıt ekranında düzeltiliyor). **A-8 kapandı.**

### 35.5 · Yan bulgu: aynı ad iki kez

03.09'da ölçüm bir bulgu vermişti — *"ilk iki dokunuş aynı işi
yapıyor"*: ana ekrandaki *"Ruhsatı çek"* ekranı açıyor, ekrandaki aynı
adlı düğme kamerayı açıyordu. Menü gelince (K-011) düzelmişti.

**06.09'da aynı bulgu geri geldi, başka yerde:** ruhsat sekmesinin adı
*"Ruhsatı çek"*, içindeki düğmenin adı da *"Ruhsatı çek"*.

Dokunuş sayısı düşmüyor (sekme Cemil'in istediği yapı) ama **ad
çakışması kalktı**: sekme **kaynağı** söyler, düğme **eylemi** —
*"Kamerayı aç"* ve *"Galeriden seç"*.

**Ölçüm:** derleme temiz · 14 ekran · 36 test / 0 bulgu ·
320 / 375 / 430 px · beş yol da yürütülerek sayıldı.

---

## K-034 · Bayrak durum değildir — K-004'ün sınırı çizildi (06.09.2026)

**Kuyruk 1.** Dün kendi koyduğum bir alan, kendi belgemizle çelişiyordu.

`VERI-MODELI.md §2` şunu yazıyordu:

> *"`Status` YOK… **'Parçası bekleniyor' gibi bilgiler bu kaydın
> verisinden türetilir, saklanmaz.**"*

K-033.5'te servis kaydının üstüne **saklanan** bir `parcaBekliyor` alanı
koydum. `CLAUDE.md` de açık: *"Alanlar VERI-MODELI.md ile BİREBİR —
modelde olmayan alan buraya da girmez."* Yani prototip belgeyi
çiğniyordu. **Belge ile kod çelişirken devam etmek, iki kaynaklı bir
dünya kurmaktır** ve sonra hangisinin doğru olduğu tartışılır.

### 34.1 · Belge yanlıştı, alan değil

Eski cümlenin dayandığı varsayım şuydu: *"parçası bekleniyor"* zaten
veriden okunabiliyor (`fiyatiBekleyen` — fiyatı girilmemiş satır var).

**Okunamıyor. İkisi ayrı gerçek:**

| | Ne demek | Nereden gelir |
|---|---|---|
| `fiyatiBekleyen(k)` | **fiyatı** gelmemiş satır var | türetilir |
| `WaitingForPart` | **parça** yolda | usta işaretler |

Parça elde olup fiyatı belirsiz olabilir; fiyatı belli olup parça yolda
olabilir. İkisini tek şeye bindirmek **ikisini de yanlış söylemekti** —
K-013'ün kuralı: yanlış bilgi göstermek, göstermemekten kötüdür.

Aynısı `TypeSetManually` için de geçerli: tür artık iş metninden **tahmin
ediliyor** (K-033.1) ve ustanın elle seçimi bu işaret olmadan bir sonraki
tahminle **eziliyordu** (K-033.12'de tam bu yaşandı). **Karar türetilemez.**

### 34.2 · Sınır: *"bu alan dolduğunda kullanıcının yapabilecekleri değişiyor mu?"*

K-004 hâlâ duruyor — **tek durum alanı `DeliveredAt`.** Ama "durum" ile
"bayrak" arasındaki fark bugüne kadar yazılı değildi ve bu yüzden her
yeni alan aynı tartışmayı açıyordu.

**Ayrım tek soruyla yapılıyor:**

> Bu alan dolduğunda **kullanıcının yapabilecekleri** değişiyor mu?

* **Değişiyorsa DURUMDUR** ve girmez. v1'in yedi durumu böyleydi: *onay
  bekliyor* olan kayda satır eklenemiyordu, *kapalı* olan teslim
  edilemiyordu. Tamircinin *"karmaşık"* dediği şey buydu.
* **Değişmiyorsa BAYRAKTIR** ve girebilir.

`WaitingForPart` işaretli kayıt hâlâ açık, satır eklenebilir, teslim
edilebilir, mesaj gönderilebilir. **Hiçbir kapıyı kapatmıyor** — yalnız
ekranda bir şey daha yazıyor. `TypeSetManually` ise kullanıcıya hiç
görünmüyor; yalnız tahminin ne zaman susacağını söylüyor.

### 34.3 · Yol ayrımında ikinci seçenek de gerçekti

Alanları **kaldırmak** da bir yoldu. O zaman K-033.5 ve K-033.12 geri
alınırdı — ve Cemil'in *"Parça bekliyor alanı olmalı"* maddesi
karşılıksız kalırdı. **Tamircinin sözü bizim akıl yürütmemizi yener**
(CLAUDE.md); burada Cemil'in isteği ile belgenin cümlesi çarpıştı ve
belgenin cümlesi **yanlış olduğu için** düştü, isteğe yenildiği için
değil. Fark önemli: belge doğru olsaydı istek reddedilirdi.

### 34.4 · Bu turda çıkan iki küçük uzlaşmazlık daha

* `SOZLUK.md` plaka alanına `Vehicle.Plate`, `VERI-MODELI.md`
  `PlateNo` diyordu. **Model belgesi esas** — sözlük düzeltildi.
* Sözlükte iş türü hâlâ **iki değerliydi** (`Repair | Maintenance`);
  K-026.3'te `Both` eklenmişti. Düzeltildi.
* `ReminderSent` sözlükte hiç yoktu; eklendi (*"arama da gönderim
  sayılır"* notuyla — K-033.9).

**Ölçüm:** derleme temiz · 14 ekran · 36 test / 0 bulgu ·
320 / 375 / 430 px. Kod değişmedi; değişen belgeler ve tohum verinin
belgeyle birebirliği doğrulandı.

---

## K-033 · Tür ekrandan kalktı; kalem listesi tablo oldu; balon kutu oldu (06.09.2026)

Cemil bir turda **otuz bir madde** yazdı. Hepsi uygulandı; aşağıda
gerekçeleriyle, ve arada **yalnız elle işletince çıkan dört hata** ile.

### 33.1 · *"Ne için geldi?"* ekrandan kalktı — tür artık METİNDEN okunuyor

> *"Servis kaydı ekle sayfası: Ne için geldi ekranı olmayacak. Tamir,
> bakım vb çok önemli değil. Biz bu ai'ya json'da seçtirebiliriz."*

Üç çipli bir soruydu ve **ana yolda her kayıt açılışında** soruluyordu.
Cemil haklı: tamirci zaten *"yağ filtre bakım"* diyor; türü ondan
okumak, ona ikinci kez sormaktan iyi.

**Tür VERİ MODELİNDEN kalkmadı, EKRANDAN kalktı.** K-002 duruyor: bakım
kilometre ister ve teslimde hatırlatma kurar. Tür artık iş metninden
tahmin ediliyor (`isTuruTahmin`) — bakım kökleri (`bakim · yag · filtre
· periyodik · servis`) ve tamirat kökleri (`ariza · degis · kacir ·
ses …`) aranıyor, ikisi birden geçerse *"Tamirat + Bakım"* oluyor.

⚠️ **Burada K-018'i bilerek esnettim.** Türkçede kelime sınırı aranmaz
diye `\b`'yi yasakladık ve `kelimeVarMi()` **eşitlikle** karşılaştırıyor.
Ama usta *"bakıma geldi"*, *"yağı değişecek"*, *"filtreleri"* diyor ve
hiçbiri kök hâliyle eşit değil. Bu yüzden burada **ön ek eşleşmesi**
var (`kelime.indexOf(kok) === 0`).

Aynı teknik aramada tehlikeliydi, burada değil — çünkü **yanlışın bedeli
farklı**. Aramada yanlış eşleşme sonucu yok eder; burada yalnız bir ön
dolgu yanlış gelir ve **düzeltilebilir**. Ölçü teknik değil, bedeldir.

**Düzeltme yeri: servis kaydının kendi ekranı.** Orada üç çip duruyor ve
metin değiştikçe tahmin kendini güncelliyor — ta ki usta elle bir şey
seçene kadar. **Ustanın kararı bizim tahminimizi yener.**
Düzeltilemeyen bir tahmin, tahmin değil hükümdür.

> Aynı soru **araç ekranından da** kalktı: *"Kayıt aç"* düğmesi artık
> ara bir panel açmıyor, doğrudan kaydı açıyor. Aynı soruyu iki ekranda
> iki farklı biçimde sormak, hiç sormamaktan kötü.

### 33.2 · Ruhsat sekmesi artık zıplamıyor; kamera **ve** galeri; alanlar tamamlandı

Cemil üç şey bildirdi, üçü de doğruydu:

**(a) Hata:** *"Ruhsatı çek butonuna basınca Plakayı yaz tabına döndü."*
Bunu ben yazmıştım ve gerekçem *"okunanı plaka sekmesinde göstermek"*ti.
Yanlıştı: usta **bastığı düğmenin sonucunu göremiyor**, ekran altından
kayıyordu. Artık okuma bitince **sekmede kalınıyor** ve okunan plaka
orada, plaka bileşeniyle gösteriliyor.

**(b)** *"Ruhsat tabında ruhsatı çekebilir ya da galeriden seçebilir."*
Ruhsat çoğu zaman **zaten çekilmiştir** — müşteri WhatsApp'tan
yollamıştır. Yalnız kamera sunmak, o fotoğrafı ikinci kez çektiriyordu.

**(c)** *"Ruhsat bilgileri eksik."* Araç ekranı şasi no, motor no, motor
hacmi, güç, koltuk ve ilk tescili **gösteriyordu** ama hiçbir yerde
**girilemiyordu**. Gösterilen ama girilemeyen alan sonsuza kadar boş
kalır. Hepsi eklendi; ana yolu şişirmesin diye **akordiyonda**, sayacı
kaç alanın dolu olduğunu açmadan söylüyor.

TC kimlik no ve adres **yine girmedi** (RUHSAT-OKUMA.md Bulgu 2).

İki yeni giriş koruması doğdu: `data-giris="kod"` (şasi/motor no —
büyük harfe çeviriyor, noktalamayı atıyor) ve `data-giris="tarih"`
(gg.aa.yyyy — noktayı kendisi koyuyor). **Denetimin sözlüğü de
genişletildi:** yeni bir tip eklenip denetime yazılmazsa test *"böyle
bir tip yok"* der ve doğru kodu hata sanar.

### 33.3 · **Kapalı düğme sessizdir**

> *"Kaydı Aç buttonuna bastım, bir şey olmuyor."*

Düğme `disabled` idi: plaka **ve tür** ister, tür seçilmemişti. Ekran
bunu hiçbir yerde söylemiyordu.

**Kural:** bir düğme kapalıysa neden kapalı olduğunu söylemez.
Düğmeler artık kapanmıyor — eksik varsa **basınca söylüyor** ve eksik
alana götürüyor ("Önce plakayı yaz — ör. 34 ABC 123", imleç plakaya).

### 33.4 · Servis kaydı kartı: ayracın üstü ARAÇ, altı İŞ

> *"Listede araç sahibi adı altında border'ın altında solda Bakım ya da
> Tamirat, o satırın sağında durum. Bu satırın altında açıklama. En
> altta sağda tutar, solda tarih. Ayrıca ruhsat sahibi ya da iletişim
> telefon no da olsun ve aranabilsin."*

Üç satır, her birinin işi ayrı: **tür ↔ durum · açıklama · tarih ↔
tutar.** Açıklama artık kendi satırında; eskiden türün yanındaydı ve
uzun bir şikâyet satırı sarıp durumu aşağı itiyordu — aynı listede
karttan karta farklı bir düzen çıkıyordu, göz sabit bir yere
bakamıyordu.

**Telefon karta girdi** ve `mesajHedefi()` ne diyorsa o: ruhsat
firmanınsa santral değil, **aracı getiren kişi** aranıyor (A-M2). İki
ayrı "kimi ararız" kuralı olsaydı listeden aranan kişiyle mesaj gönderilen
kişi farklı olurdu.

**"İçeride" burada YAZIYLA duruyor** — Cemil: *"İçeride olarak
yazdığımız durum için kullanmak gerekirse serviste kullan."* Araç
listesinde çerçeve yetiyordu (K-019.2), çünkü orada tek soru vardı: *bu
araç şu an bizde mi.* Servis listesinde **iki durum** var (içeride ·
teslim edildi) ve ikisi de yazıyla söyleniyor; biri yazı öbürü çerçeve
olsaydı usta ikisini karşılaştıramazdı.

### 33.5 · Kalem listesi **TABLO** oldu; müşteri kayda girdi; "parça bekliyor" kendi alanını aldı

> *"Parça eklendikçe tablo olarak üstte dursun… Böyle 10 tane parça
> eklense, okunabilir olmaz."*

`.kalem` satırı bir **kart** gibi davranıyordu: tip rozeti, ad, fiyat,
silme düğmesi ve altında **iki soru** ("cevap kimden geldi", "parçayı
kim getirdi") — satır başına beş-altı öğe. Üç satırda idare ediyordu,
on satırda duvar oluyordu.

Tablo **iki sütun**: *İş* ve *Tutar*. Rakamlar tek hizada, sağa yaslı.
İkincil bilgi adın altında küçük duruyor — kendi sütununu istemiyor
(K-031: tablo **sığar**, kaydırılmaz).

**Satır başına "kim getirdi" düğmesi kaldırıldı.** Önce küçük bir
anahtar koymuştum; denetim beşini birden **26 px** ölçtü ve haklıydı.
Büyütmek de çözüm değildi: on satırda on tane 44 px'lik düğme, Cemil'in
kaldırttığı gürültünün aynısı. Cemil zaten söylemişti —
*"parçayı müşteri getirdi alanına gerek yok. Listenin üzerinde müşteri
getirdi seçilirse parçalara ücret yazmaz."* **K-005 kaybolmadı:**
sorumluluk anahtarı listenin üstündeki tek düğmeden kuruluyor, satırda
yalnız **sonucu** rozet olarak duruyor.

Üstteki düğme bir **anahtar**: basınca hepsi müşteriye geçiyor
(fiyat → alış, fiyat 0), tekrar basınca geri alınıyor. Sonradan parça
eklenince düğme kendiliğinden sönüyor — çünkü artık "hepsi" doğru
değil. **Geri alma şart:** yanlışlıkla basılan bir düğme on satırın
fiyatını sıfırlıyor.

**Ne istiyor** artık `textarea` (Cemil: *"text alanı daha büyük olmalı"*)
ve **alt satıra ekleme** düğmesi var — telefon klavyesinde Enter çoğu
zaman "bitti" demektir, alt satıra geçmenin görünür bir yolu olmadan
usta her şeyi tek satıra sıkıştırıyordu.

**Müşteri bilgileri kayda girdi.** Kayıt ekranı ustanın en çok durduğu
ekran ve *"kimi arayacağım"*ın cevabı orada yoktu.

**"Parça bekliyor" kendi bayrağını aldı.** Bugüne kadar bu bilgi
`fiyatiBekleyen`'den türetiliyordu — o **başka bir şey**: fiyatı
gelmemiş demek. Parça sipariş edilip yolda olabilir ve fiyatı belli
olabilir; ya da fiyat belli olmayabilir ve parça elde olabilir. İkisini
tek alana bindirmek, ikisini de yanlış söylemekti. **Bu yeni bir DURUM
değil** — K-004 duruyor, tek durum alanı hâlâ `teslimTarihi`; bu bir
bayrak, akışı değiştirmiyor. *(A-4 böylece kapandı.)*

**Fotoğraftan parça listesi** eklendi: parçacı irsaliyeyi WhatsApp'tan
yolluyor, on kalemi tek tek yazmak ustanın en sevmediği iş. Okunan
satırların **fiyatı boş geliyor** — okunmamış bir rakamı doluymuş gibi
göstermek, ustanın kontrol etmediği bir rakamı müşteriye söyletmek olurdu.

### 33.6 · Teslimden sonra mesaj **sorulur** — ve bir yalan daha kalktı

> *"Teslim et deyince ödemenin nasıl olduğu da sormalı ve mesaj
> gönderilebilmeli veya geç deyip kapatabilir."*

⚠️ **Eskiden burada yalan vardı.** Teslim balonu, telefonu olan araçta
*"Özet gönderildi."* diyordu ve **hiçbir şey gönderilmiyordu.** 04.09'da
aynı hatayı *"Onay iste"* düğmesinde yapmış ve düzeltmiştim (K-021);
kural taşınmamış, ikinci kopyası burada kalmıştı. **Bir kez böyle kanan
usta, bir daha hiçbir "gönderildi" yazısına inanmaz.**

Artık teslim ekranı kapanmıyor: teslim edildi bilgisini, ödeme yolunu ve
kurulan hatırlatma tarihini gösteriyor, sonra **"Müşteriye özet gönder"**
ya da **"Geç, gönderme"** soruyor. "Geç" gerçek bir seçenek — müşteri
zaten yanındaysa mesaj gereksiz.

### 33.7 · Araç ekranı: **ödeme durumu** açıldı, **not gizlendi**

> *"Aracın borcu, ödemesi varsa, araç bilgilerinin altında bir alan açıp
> burada gösterebilirsin… Not alanı gizli olsun. V1'de bu alanı güzel
> yapmıştık."*

Sıra: künye+müşteri → düğmeler → **ödeme durumu (varsa)** → ruhsat →
kilometre → bakım → **not (akordiyon)** → geçmiş.

**Ödeme durumu yalnız borç varsa** çiziliyor; borcu olmayan araçta
"borç yok" yazmak, olmayan bir sorunu her açılışta hatırlatmak olurdu.
Hesap muhasebedeki *Alacak* listesiyle **aynı kaynaktan** — iki yerde
iki ayrı hesap olsaydı biri diğerini yalanlardı.

⚠️ **Not akordiyona girdi ve bu 04.09'da verdiğim kararı düzeltiyor.**
O gün notu düğmelerin hemen altına koymuştum; gerekçem *"not en taze
bilgi, dipte kimse görmüyor"* idi. Gerekçe yanlış değildi, **ölçü**
yanlıştı: dört araçtan üçünün notu boş ve boş bir `textarea` ekranın
en üstünde iki satır yer kaplıyordu. Akordiyonun sayacı notun dolu olup
olmadığını **açmadan** söylüyor.

### 33.8 · Müşteri kartı: plaka **çipin içinde değil, kendi satırında**

> *"Müşteriler listesini düzenle, plaka belli olsun. Plaka listelerken
> göstermesin. Bu alanı tekrar düzenle."*

Araçlar en altta, `.cip` **düğmelerinin** içinde plaka bileşeni olarak
duruyordu. Çip bir **seçim** aracıdır — süzgeç, şablon, tür seçimi hep
çip. Plakayı çipin içine koyunca iki şey bozuldu: plaka *"seçilecek bir
şey"* gibi göründü, ve çipin çerçevesi plakanın çerçevesiyle iç içe
girdi. Yanında *"Mesaj"* çipi de vardı; plaka ile mesaj düğmesi aynı
cinsten iki nesne gibi duruyordu.

Müşteri artık bir **kart**: üstte adı ve kaç aracı olduğu, altında
telefonu, ayracın altında araçları — her biri kendi satırında, gerçek
plaka bileşeniyle. **Eylem eylemdir, plaka veridir.**

### 33.9 · Mesaj: üç kanal, dördüncü şablon, seçim yukarıda

> *"Müşteri seçilebilir olmalı… whatsapp, telefon uygulama, mesaj olarak
> gönderebiliriz."*

**"Değiştir" yukarı çıktı.** Düğme vardı ama ekranın **dibindeydi** —
metnin, önizlemenin ve gönder düğmesinin altında. Yanlış müşteriyle
açılan ekranda usta önce metni okuyor, *"bu kim"* diyordu. **Seçim,
seçimin sonucundan önce görünmeli.**

**Üç kanal:** WhatsApp (birincil, bedava) · SMS (paralı, İYS istiyor —
ama WhatsApp'ı olmayan müşteri var) · Ara. Üçü de **aynı metni** taşır;
kanala göre metin değiştirmek, ustanın gördüğüyle gidenin ayrılması
demekti.

**Dördüncü şablon: "Parça bekliyoruz."** Yeni bir iş değil, var olan
işin sessiz kalan yarısı: müşteri aracını bıraktı ve haber bekliyor;
haber gelmeyince arıyor, usta telefonda vakit kaybediyor. Mesaj
**söz vermiyor** — *"yarın hazır"* demiyoruz, parçanın ne zaman
geleceğini biz de bilmiyoruz ve **tutulmayan söz, hiç verilmemiş sözden
kötüdür.**

Balonlar artık hepsi *"açılıyor"* diyor, *"gönderildi"* demiyor.

### 33.10 · Balonun sorunu yarıçap değil **GENİŞLİKTİ**

Cemil bir ekran görüntüsü yolladı: hata mesajı **yumurta** gibi
duruyordu, metin eğrinin içine giriyordu.

İlk teşhisim yarıçaptı (`--radius-pill`, 999 px) ve düzelttim. **Yarım
çözümdü.** Ölçünce asıl sebep çıktı: balon `position:absolute; left:50%`
ile ortalanıyor ve `right` verilmiyor. Böyle bir öğe için tarayıcının
hesapladığı **kullanılabilir genişlik, kapsayıcının sağ yarısıdır** —
375 px'lik telefonda 187 px. `max-width: calc(100% - 32px)` yazsam da
öğe hiç 188 px'i geçemiyordu; uzun cümle üç satıra sarıyor, kutu
yükseliyor ve pill yarıçapıyla yumurtaya dönüyordu.

`width: max-content` shrink-to-fit hesabını devre dışı bırakıyor:
genişliği artık metin belirliyor, `max-width` tavanı koyuyor.
**Ölçüldü (375 px): önce 188×88 (üç satır) → sonra 343×62 (iki satır).**

> Arada `text-wrap: balance` denedim ve geri aldım: satırları eşitlemek
> için kutuyu **daraltıyor** — satır sayısını azaltmıyor, artırıyor.

### 33.11 · Üç yeni **derleme** denetimi — çünkü derleyici yalan söylüyordu

**(6) Yorum içinde etiket.** Bir ekran başlığına *"(aşağıdaki `<style>`
içinde)"* yazdım. Stil çıkarıcı kaçıncı etiket olduğuna bakmıyor:
**yorumdaki** etiketten başlayıp dosyanın sonundaki gerçek kapanışa
kadar her şeyi — `<section>`'ın tamamını — CSS sanıp sildi. Derleme
*"id=screen-… bulunamadı"* dedi; sebebi bir yorum satırıydı. **Yorum kod
değildir ama derleyici için metindir.**

**(7) Üretilen betik ayrışıyor mu.** Bir ekran betiğinde dizge içinde
gerçek satır sonu kaldı. Derleyici *"derleme denetimi temiz"* dedi, dosya
üretildi, tarayıcıda **açıldı** — ve içindeki bütün betik
ayrıştırılamadığı için `Ekran`, `showScreen`, hiçbir şey tanımlanmadı.
**Prototip sessizce ölü açıldı.** Beş denetim de doğruydu; hiçbiri
betiğin **çalışıp çalışmadığına** bakmıyordu. En temel soru en sona
kalmıştı: *bu JavaScript geçerli mi?*

**(8) Kapanmamış yorum.** `03-bilesen.css`'te bir yorum kapanmıyordu ve
CSS o noktadan sonra bir sonraki kapanışa kadar her şeyi yorum saydı.
İçinde kalan kural `.k-sil::before` idi — **ikincil düğmelerin 44 px
dokunma alanı.** Kural yazılmıştı, denetimden geçmişti, ölçülmüştü;
sadece tarayıcı onu hiç görmedi. Dosyada 140 açılış, 139 kapanış vardı;
tek bir sayma bunu söylerdi. **Kapanmamış yorum, silinen koddan
tehlikelidir: silinen kod yoktur, yorumlanan kod var gibi durur.**

Üçü de negatif testle doğrulandı.

### 33.12 · **ÇİZİM, KARARI SİLMEZ** — aynı hatayı bir turda iki kez yaptım

`ciz()` fonksiyonuna ekran değişkenlerini sıfırlatmıştım:

* `kTurElle` — usta *"Tamirat"* seçiyor, sonraki harfte tahmin yeniden
  koşuyor ve **seçim geri alınıyordu**.
* `kSilinecek` — kaydırma onayı açmak için `Ekran.tazele()` çağırıyor,
  tazeleme onayı **siliyordu**; satır kayıyor, hiçbir şey açılmıyordu.

İkincisi K-032'de yaşadığım işaret hatasının **birebir aynı belirtisi**,
tamamen başka sebeple.

**Kural:** `ciz()` **ekranı** çizer, **kararı** değil. Sıfırlanacak tek
şey, *başka bir kayda geçildiğinde* sıfırlanandır. Bir SEÇİM ekranın
değil **verinin** bilgisidir — ekran değişkeni çizimle ölür, kayıt
yaşadığı sürece seçim de yaşamalı. (`turElle` kaydın üstüne taşındı.)

⚠️ **İkisini de hiçbir test yakalamadı; ikisi de elle işletince çıktı.**
Denetim 36 test koşuyor ve 0 bulgu veriyordu.

### 33.13 · Kaydırma hareketi çekirdeğe taşındı

Sola kaydırıp silme artık **iki** tabloda var: muhasebedeki giderler ve
servis kaydındaki yapılanlar. Kod muhasebe ekranının IIFE'sinin
içindeydi ve oradan çıkamıyordu; kopyalansaydı iki ayrı eşik, iki ayrı
yön hatası doğardı — **yön hatasını K-032'de bir kez zaten yapmıştım ve
hiçbir test yakalamamıştı.**

`Kaydirma` modülü çekirdekte; ekranlar yalnız **ne anlama geldiğini**
yazıyor. **Ortak nesne kuralı hareketlere de işliyor:** bir nesne bir
yerde çizilir, bir hareket bir yerde tanımlanır.

**Ölçüm:** derleme temiz · 14 ekran · 36 test / **0 bulgu** ·
320 / 375 / 430 px · ana yollar elle işletildi (ruhsat okuma, tür
tahmini, hepsi-müşteri anahtarı, kaydırarak silme, teslim→mesaj,
üç kanal).

> **Cemil onayına açık (A-M13):** tek bir parçayı ayrı işaretlemenin yolu
> artık **yok** — "hepsi müşteriden" anahtarı var, satır başına anahtar
> yok. Senin sözün bizim gerekçemizi yener diye böyle yaptım; ama
> gerçekte *"balatayı müşteri getirdi, yağı ben aldım"* durumu oluyorsa
> satır başına bir yol gerekir.

---

## K-032 · Kaydırma silmez, sorar (05.09.2026)

### 32.1 · Sola kaydırma **onay satırını** açar

> *"Giderde sola kaydırırsa sorarak silsin."*

**Kaydırma tek başına silmez.** K-031'in dersi burada da geçerli: görünmeyen
bir hareket, **görünmeyen bir sonuç doğurmamalı.** Cepte kazara kaydırmak bir
gideri silmemeli.

| Hareket | Sonuç |
|---|---|
| < 40 px sola | hiçbir şey — satır yerine döner |
| ≥ 40 px sola | **onay satırı**: *"Dükkân kirası silinsin mi? · Vazgeç · Sil"* |
| Onay satırında **Sil** | siler, balon söyler |

**Onay satırı, silinecek satırın YERİNE çiziliyor:** ayrı bir kutu açmak
tabloyu bozar, balon ise kaybolur gider.

**Klavyeyle de silinebilir.** Kaydırma parmakla yapılır, klavyeyle yapılamaz;
satır odaklanabilir ve **Delete** aynı onay satırını açıyor. Erişilebilirlik
borcu, doğduğu turda kapandı.

> **Cemil onayına açık (A-M12):** kaydırma **keşfedilebilir mi?** Ekranda onu
> söyleyen bir şey yok — *"buraya araç işleri dışındaki giderler"* şeridi
> senin isteğinle kalktığı için yerine ipucu koymadım. Bir kez öğrenilen bir
> hareket ama **öğrenilene kadar** silme yolu görünmüyor.

### 32.2 · Silme sütunu kalkınca tutarlar sağa yaslandı

*"Tutarları sağa yasla"* ile *"kaydırarak sil"* **aynı düzeltmeyle** çözüldü:
`×` düğmesi **40 px** yer kaplıyordu ve rakamlar kartın sağ kenarından o kadar
içeride duruyordu.

**Ölçüldü:** tutarların sağı **340**, kartın iç sağ kenarı **341** — tam yaslı.

### 32.3 · Daire gerçekten daire değildi

*"Giderler + butonu mikrofonla hizalı olsun."* Ölçtüm: düğmeler **44 × 48** —
**oval**. Sebep, genel bir `min-height: var(--dokun)` kuralının 44 px'lik
yüksekliği ezmesi. **Dokunma hedefi kuralı (48) haklıydı, yanlış olan
genişlikti.** İki ölçü de `--dokun` oldu: **48 × 48**, ikisi aynı hizada.

`.section-label.with-action`'ın `margin-bottom: 14px`'i de kalktı — kart içinde
boşluğu **kap veriyor** (K-028.3) ve ikisi üst üste biniyordu.

### 32.4 · Zebra, açıklama, plaka

| | |
|---|---|
| **Zebra** | `tbody tr:nth-child(odd)` — çok hafif, zemin değil bir ton. İki sütunda göz satırı kaybetmez ama **uzun listede** kaybeder: tutarın hangi açıklamaya ait olduğu bir an duraksatıyordu |
| **Açıklama şeridi** | kalktı. Kural duruyor ama **her açılışta tekrar etmek ustayı azarlamak** oluyordu; yeri **boş durum** — ilk kez gider yazan orada okur, sonra bir daha görmez |
| **Alacakta plaka** | düz metin değil **gerçek plaka bileşeni** (`plakaHtml`). Plaka her ekranda aynı dili konuşur (K-008) |

### 32.5 · Denetim iki ölü sınıf yakaladı

Bu turda iki bileşen kaldırıldı (`.lbl-not` ve `.satir-sil`) ve **ikisini de
denetimin `OLU-SINIF` testi** buldu. Ölü CSS bırakılmaz: sözlükte duran ama
kullanılmayan sınıf, bir sonraki okuyucuya *"bu var"* der.

**Bir de kendi hatam elle işletirken çıktı:** kaydırma eşiği `kayma >= 40` diye
yazılmıştı ama `kayma` **negatif** (sola gidiyor). Kaydırma çalışıyor, onay hiç
açılmıyordu — **hiçbir test görmezdi**, çünkü hepsi "0 bulgu" veriyordu.
İşletmeden bilinmezdi.

---

## K-031 · Görünmeyen kaydırma, kırpmanın kardeşidir (05.09.2026)

> *"Gider ve alacak tabloları telefonun ekranına sığmıyor, sağa kaydırmak
> gerekiyor. **Kullanıcı sağa kaydıracağını bilmez.**"*

### 31.1 · ⚠️ Bu, K-025'te verdiğim kararı DÜZELTİYOR

K-025'te *"kesilen metin, taşan metinden tehlikelidir"* deyip tabloya
`overflow-x: auto` koymuştum. Kırpmaya göre doğruydu — ama **üçüncü bir
seçenek** vardı ve o daha doğru: **sığdırmak.**

| | Kullanıcı ne görür | Eksik olduğunu bilir mi |
|---|---|---|
| Kırpma | yarım kelime | belki |
| **Görünmeyen kaydırma** | **hiçbir şey** | **hayır** |
| Sığdırma | hepsi | — |

**Görünmeyen kaydırma, kırpmanın kardeşidir.** İkisinde de kullanıcı bir
şeyin eksik olduğunu bilmez; fark, kırpmada yarım kelime görmesi,
kaydırmada **hiç görmemesi.**

### 31.2 · Nasıl sığdı: sütun sayısı azaldı

**İkincil bilgi kendi sütununu istemiyor** — ana metnin altında küçük yazıyor
(`.t-alt`).

| Tablo | Önce | Sonra |
|---|---|---|
| Gider | Tarih · Açıklama · Tutar · sil | **Açıklama** *(altında tarih)* · Tutar · sil |
| Alacak | Kim · Araç · Teslim · Tutar | **Müşteri** *(altında plaka · tarih)* · Tutar |
| İş başına | Araç · İş · Teslim · Alınan · Parça · Fark | **İş** *(altında tür · marka · tarih · alınan · parça)* · **Fark** |

**Ölçüldü (375 px):** üçü de **305 = 305** — taşma sıfır.
Negatif test eski davranışı geri getirdiğinde: gider **36 px**, alacak
**125 px**, iş başına **317 px** kayıyordu.

`table-layout: fixed` + `overflow-wrap: anywhere` metni sarıyor; sayı sütunu
`nowrap` ve sabit 96 px — rakam sarmaz, *"2.4"* alt satıra *"00"* olmaz.

### 31.3 · 36. test: GIZLI-KAYDIRMA

Ekran içinde yatay kaydırılabilen **ve gerçekten taşan** bir kutu varsa bulgu.
Dikey kaydırma kapsam dışı: **telefonda aşağı kaydırmak öğrenilmiş
davranıştır, sağa kaydırmak değil.**

### 31.4 · Öteki dört madde

| | |
|---|---|
| Gider başlığı | *"Ne için"* → **Açıklama** |
| Alacak başlığı | *"Kim"* → **Müşteri** — ustanın kelimesi |
| *"dönemden bağımsız"* şerhi | kalktı *(ve CSS'te ölü kalan `.lbl-not` denetimin `OLU-SINIF` testiyle yakalanıp silindi)* |
| Gider ekle | mikrofonun **solunda, aynı yuvarlak içinde `+`** |

**`.mic-round` → `.daire-btn`.** Aynı biçim artık iki iş yapıyor; sınıf adının
*"mic"* demesi yanlış olurdu. Bileşen **yuvarlak eylem düğmesi**, içindeki
ikon ne iş yaptığını söylüyor.

**Neden aynı biçim:** elle giriş ve sesle giriş aynı işin iki yolu. Biri düz
düğme, öteki yuvarlak dururken biri *"asıl"*, öteki *"yedek"* gibi okunuyordu
— oysa elle giriş yedek değil: mikrofonsuz tarayıcıda **tek yol** o (K-021.1).

### 31.5 · Giderlere araç işi yazılmaz

> *"Giderlerde yedek parça olmamalı. Yedek parçalar işten alınarak düşülüyor.
> Tabi kullanıcı isterse ekleyebilir."*

Parçanın alış fiyatı **zaten servis satırında** duruyor ve iş başına farkta
düşülüyor. Buraya elle yazmak **aynı parayı iki kez saymaktır.**

**Tohum veriden çıkarıldı.** *"Öz Yedek Parça — triger seti 4.200 ₺"* örnek
veride duruyordu ve ekran onu gider sayıyordu: **örneğin kendisi yanlış
alışkanlık öğretiyordu.** Yerine *"Dükkân kirası"* kondu.

**Yasak değil, uyarı** (Cemil: *"kullanıcı isterse ekleyebilir"*): ekran
*"buraya araç işleri dışındaki giderler yazılır… ayrıca yazarsan iki kez
sayılır"* diyor.

---

## K-030 · `inputmode` bir kısıt değil, bir öneridir (05.09.2026)

> *"Tutar alanına harf giremeyiz. Inputların veri doğruluğundan emin ol."*

### 30.1 · Ölçüldü: üç tutar alanı da harf kabul ediyordu

`inputmode="numeric"` yalnız **telefonun hangi klavyeyi açacağını söyler.**
Masaüstünde, **yapıştırmada** ve sesle girişte harf rahatça giriyordu.

| Alan | Yazılan | Kalan *(önce)* |
|---|---|---|
| Servis satırı tutarı | `abc12x3,4.5 TL` | `abc12x3,4.5 TL` |
| Gider tutarı | `abc12x3,4.5 TL` | `abc12x3,4.5 TL` |

**Neden sessiz ve pahalı:** okuma tarafı **zaten temizdi**
(`replace(/[^0-9]/g,'')`), yani **veri bozulmuyordu** — ama ustaya
**gösterilen** şey bozuluyordu. `abc12x3` yazan usta ekranda onu görüyor,
kayda **12.345 ₺** düşüyordu. Yanlış rakam göstermek, rakam göstermemekten
kötüdür (K-013'ün aynı kuralı, başka bir yerde).

### 30.2 · Tek delegasyon, `data-giris` sözleşmesi

Her alan kendi temizleyicisini yazsaydı biri unutulurdu — **nitekim üçünde de
yoktu.** `data-giris` bir sözleşme: alanı işaretlemek yeter, davranış
çekirdekten gelir.

| Tip | Kural | Örnek |
|---|---|---|
| `tutar` | tam sayı, binlik nokta. **Kuruş yok** — tamirci *"1900"* der | `abc12x3,4.5 TL` → **12.345** |
| `km` | tam sayı, binlik nokta | `142500xy` → **142.500** |
| `yil` | dört hane | `20a18999` → **2018** |
| `tel` | 11 hane, gruplanır. Ülke kodu **kabul edilmiyor** — tek biçim tutmak, iki biçimi karşılaştırmaktan kolay | `0532abc2153000999` → **0532 215 30 00** |
| `plaka` | v1'in biçimleyicisi, **çekirdeğe taşındı** | `34abc123` → **34 ABC 123** |

**Sıfır geçerli:** müşteri parçası 0 ₺ durur (S2, K-005).

**İmleç korunuyor.** Değeri yeniden yazmak imleci sona atar; usta ortadan
düzeltmek isteyince her tuşta sona fırlıyordu. Solunda kaç **rakam** varsa o
kadar rakamdan sonraya konuluyor — binlik noktası eklense bile aynı yerde.

**Yakalama evresinde dinleniyor** (`addEventListener(..., true)`): ekranların
kendi `input` dinleyicileri **temizlenmiş** değeri görüyor, hamı değil.

### 30.3 · Yazarken azarlanmaz, yazıldıktan sonra söylenir

Yıl ve telefon **aralığı** yazarken denetlenmiyor: *"20"* yazan biri henüz
2018'e gidiyordur. Alan **dolduğunda** söyleniyor ve *"Kaydı aç"* kapanıyor.

**Telefon eksik olabilir ama yanlış olamaz** — yanlış numaraya giden bir
hatırlatma, gitmeyen hatırlatmadan kötüdür.

### 30.4 · 35. test ve iki kez düzeltilen ölçüt

Test **iki şeyi** ölçüyor:
- **(a)** işaretli alana biçim dışı karakter girip kalıyor mu
- **(b)** biçimli görünen *(sayısal klavye ya da `.mono`)* ama **işaretsiz**
  alan var mı — yani koruması **unutulmuş** alan

(b) olmadan test yalnız bugünkü alanları korurdu; yarın eklenen alan yine
korumasız girer ve kimse görmezdi.

**Ölçüt iki kez yanlıştı, ikisini de negatif test buldu:**

1. *"Harf var mı"* diye soruyordu — **plaka harf içerir** (34 ABC 123) ve test
   onu bulgu sandı. Her tipin kendi **kabul deseni** var; ölçülen şey harf
   değil, **tipin dışına çıkan karakter**.
2. `if (!el.offsetHeight) return;` vardı — ruhsattaki yıl/km/telefon **ancak
   kayıtsız plaka yazılınca** açılıyor, o yüzden test onları **hiç
   görmüyordu**: korumaları silinse bile *"0 bulgu"* derdi. **Bir alanın
   koruması, o an görünür olup olmamasından bağımsızdır.**

**Negatif doğrulama:** bütün `data-giris` öznitelikleri silindi → **4 bulgu**
(üçü gizli alanlardan), geri konunca **0**.

---

## K-029 · Boşluk kuralının açığı: SARMALAYICI KAP (05.09.2026)

> *"Elementleri neden birbirine yapıştırıyorsun? Bunu genel olarak düzelt."*
> — Cemil, **üçüncü kez** ve bu sefer sebep başkaydı.

### 29.1 · Sebep: kabın gap'i torununa işlemez

K-026.6'da `.screen`, K-028.3'te `.card` flex + gap oldu ve kural yazıldı:
**boşluğu kap verir.** Ama aynı turlarda ekran gövdesine **düz `<div>`
sarmalayıcılar** eklendi — sekmeler, kipler, gövdeler. Onların **kendi gap'i
yoktu.**

`.screen`'in gap'i sarmalayıcıya uygulanıyor, **içindeki kardeşlere
uygulanmıyordu.** Ölçüldü:

```
muhBasitGovde: cip-grup -> card           = 0 px
muhBasitGovde: card -> section-label      = 0 px
muhBasitGovde: btn-row -> DIV             = 0 px
ruhsatGovde:   section-label -> cip-grup  = 0 px
sekmePlaka:    card -> notice             = 0 px
```

**Sekme eklemek ritmi sessizce kırdı.** Kural doğruydu, ama kuralın
uygulanacağı yer bir turda **çoğaldı** ve yeni kaplar kuralın dışında kaldı.

**Çözüm bir sınıf: `.yigin`** (flex sütun, gap 18). Sarmalayıcıya sınıf
vermek, kuralı görünür ve **denetlenebilir** yapıyor — anonim bir `<div>`
hiçbir araca kendini göstermiyordu.

### 29.2 · 33. testin kapsamı yanlıştı — iki kez

**İlk hâli** yalnız `.screen` ve `.card`'a bakıyordu: sarmalayıcıların içini
**hiç görmüyordu**. Cemil'in gördüğü sıfırlar tam oradaydı.

**Bütün kaplara bakmayı denedim: 165 "bulgu"** çıktı ve çoğu **meşruydu.** Bir
bileşenin **içi** kasıtlı olarak sıkıdır:

| Yer | Boşluk | Neden kasıtlı |
|---|---|---|
| `.tl-body`: başlık → alt satır | 8 px | **Birbirine ait** — az boşluk bunu söyler |
| `.arac-bas`: plaka → başlık | 12 px | Aynı künyenin parçaları |
| `.ozet-bas`: ad → adres | 2 px | Tek bir blok |

165 bulgulu bir test **okunmaz** olur ve okunmayan test, olmayan testtir.

**Doğru kapsam: blok kapları** — `.screen`, `.card`, `.yigin`. Bunlar **bölüm
listeler**; içleri birbirine ait değildir.

> **Ders:** bir testin eşiği kadar **kapsamı** da yanlış olabilir. İkisi de
> "0 bulgu" der; biri gevşek olduğu için, öteki bakmadığı için.

### 29.3 · Aynı turda açılan öteki boşluklar

| Yer | Önce | Sonra | Neden |
|---|---|---|---|
| `.tutar-serit` satırları | 6 | **10** | *Teslim edilen / Nakit / Kart / Havale* tek blok gibi okunuyordu — Cemil'in ekran görüntüsü |
| `.menu-list` | 9 | **12** | Menü satırları yapışıktı |
| `.tl-item` arası | 10 | **12** | |
| `.arac-kart` arası | 12 | **14** | |
| `.notif-list` | 8 | **10** | |
| `.kalem-list` | 10 | **12** | |

`.tutar-serit`'in `margin-top`'u da **kaldırıldı** — kart içinde kabın gap'i
zaten veriyordu, ikisi toplanıyordu.

**Negatif doğrulama:** `.yigin { gap: 0 }` geri konunca test **12 bulgu**
verdi ve bulguların ilk dördü **tam olarak Cemil'in gördüğü sıfırlardı**;
kural açılınca 0.

---

## K-028 · Modül tahtası OtoMix dilinde; boşluğu KAP verir; gizli olan gizli kalır (05.09.2026)

### 28.1 · "Otomix gibi" — tahmin edilmedi, bakıldı

> *"Detaylı muhasebe alanı Otomix gibi olacak. Buttonlar buna göre olmalı."*

OtoMixSQL'in modül dili çıkarıldı (`arastirma/OTOMIX-HARITA.md`):
**Cari · Fatura · Kasa · Banka · Çek · POS · Stok · Teklif · e-Fatura/e-Arşiv
· SMS.** Paraşüt ve LucaNet'te olduğu gibi **ölçülmüş bir referans**, tahmin
değil.

**Alınan şey düzen ve dil, kapsam değil.** Masaüstü programların klasik modül
tahtası: **ikonlu kare kutular, ızgara hâlinde.**

| | Ana menü | Modül tahtası |
|---|---|---|
| Biçim | `.menu-row` — **liste** | `.modul-kutu` — **ızgara** |
| Neden | Yukarıdan aşağı okunur, **sırası anlam taşır** | Gözle **taranır**, hepsi aynı ağırlıkta |

**Sıra OtoMix'in sırası değil, Tamiron'un sırası: çalışanlar önce.** OtoMix'te
modüller klasik sırada duruyor çünkü hepsi yazılmış; burada yarısı yazılmadı
ve çalışanı aşağı atmak, ustayı **çalışmayanların arasında aratmak** olurdu.

| Çalışan | Yazılmadı *(solgun, dokunulamaz)* |
|---|---|
| **Kasa** *(basit kipin kendisi)* · **Raporlar** *(iş başına)* · **SMS** *(mesaj)* | Cari · Fatura · Banka · Çek · POS · Stok · Teklif |

> **OtoMix, Tamiron'un en yakın rakibi:** tek program hem servis hem muhasebe
> yapıyor. Paraşüt ve LucaNet'in gövdesi fatura; ikisinin gövdesi **iş emri**.
> Fark, OtoMix'in muhasebeyi **altı modüle** açması — tamirci tam bu yığına
> *"karmaşık geldi"* demişti (T-1).

### 28.2 · Gezinme hedefi kodda BİRLEŞTİRİLMEZ

Modül düğmeleri `' data-git="' + m.git + '"'` diye yazılıyordu. Sonuç:
derlemedeki *"ölü data-git hedefi"* denetimi ve sağlık denetiminin **YETİM**
testi bu hedefi **göremiyordu** — ekran açılıyordu ama hiçbir araç bunu
bilmiyordu. `git` alanına bir yazım hatası girseydi düğme **sessizce** hiçbir
şey yapmazdı.

**Aynı sınıf renk kutucuğunda yaşanmıştı** (K-019: sınıf adı kodda
birleştirilmez, haritadan okunur). Hedef artık bir **etiket**; ekran adı
işleyicide **literal** yazılıyor ve bilinmeyen etiket **balon** veriyor —
menüye modül ekleyip bağlamayı unutmak sessiz kalmıyor.

### 28.3 · Boşluğu KAP verir — kart içi ritim de tek yerden

> *"Elementler yine birbirine yakın. Kontrol et."*

Birinci turda (K-026.6) `.screen` gap'i 18'e çıkarılmıştı ama **kart içi hâlâ
margin'lerle** yürüyordu ve her yer **14 px**'te kalmıştı. Cemil haklıydı;
ölçüm onu doğruladı: **on bir ekranda 14 px.**

İki sessiz sebep vardı:
- `.screen-head { margin-bottom: -4px }` — ritmi 18'den **14'e geri çekiyordu**
- `.section-label { margin-bottom: 14px }` — kart içinde ikinci bir kaynak

Artık `.card` da **flex sütun + gap: 16**. Form alanlarının `margin-top`
kuralları **kaldırıldı**: boşluğu artık kabın gap'i veriyor. İkisi birden
olsaydı **toplanır** ve alanlar arası 34 px'e çıkardı — *"çok yakın"*ın tersi
kadar yanlış.

> **Kural: BOŞLUĞU KAP VERİR.** Bileşen kendi dışındaki boşluğu yazmaz.

**33. testin eşiği 8 → 16.** İlk eşik *"0 bulgu"* veriyordu ama hedefin
altındaki her şeyi geçiriyordu. **Bir test ölçmüyorsa değil, YANLIŞ EŞİKLE
ölçüyorsa da yanlış güven verir.** Eşik artık tasarım hedefiyle aynı.

### 28.4 · ⚠️ GİZLİ OLAN GÖRÜNÜYORDU — ve bunu ekran görüntüsü buldu

`.card`'ı flex yapmak **gider formunu kapalıyken ekranda bıraktı.** `hidden`
özniteliği tarayıcının kendi stilinde `display: none` demektir — ve
**yazdığımız her `display` kuralına yenilir.**

Öznitelik duruyordu, form görünüyordu. **Hiçbir test görmedi**, derleme de
görmedi; ekran görüntüsüne bakarken yakalandı. Sessiz bozulmanın ders kitabı
hâli: **kod doğru, öznitelik doğru, görünüş yanlış.**

Çözüm tek satır ve bundan sonra hangi bileşene `display` verilirse verilsin
geçerli: `[hidden] { display: none !important; }`

**34. test (GIZLI-AMA-GORUNUR)** eklendi ve negatif testle doğrulandı
(kural kapatılınca 13 bulgu, açılınca 0).

### 28.5 · Basit kipte varsayılan BUGÜN

> *"Basit modda bugün seçilmeli."*

Ustanın günlük sorusu *"bugün ne kaldı"*; ay ve yıl **ikinci** sorudur. Ekran
açıldığında birinci soruyu cevaplar — servis kayıtlarındaki *"içeride açık
gelir"* kuralının aynısı.

### 28.6 · Alacak DÖNEME TABİ DEĞİL

> *"Alacak olmalı."*

Alacak bir **hareket** değil, bir **bakiye**: geçen aydan kalan alacak bu ay da
alacaktır. Dönem süzgecine sokunca *"Bugün"* seçiliyken alacak **gizleniyordu**
— ustanın en çok sorduğu şey ekrandan kayboluyordu. Tahsilat ve gider döneme
tabi, **alacak değil**; başlığın yanında bu yazıyor.

**Ölçüldü:** *Bugün* seçiliyken alacak görünüyor — Demir Nakliyat, 06 TMR 07,
28.08.2026, **3.450 ₺**.

---

## K-027 · Muhasebe: dönem her şeyi süzer, rakamlar tabloda, derinlik menüde (05.09.2026)

Cemil'in muhasebe turu. Dokuz düzeltme, üçü **kural** değiştirdi.

### 27.1 · Dönem düğmeleri ADINI söylüyor — ve her şeyi süzüyor

> *"Basit tabının altına Bugün, bulunduğumuz ay hangisi ise, sonra da
> bulunduğumuz yıl hangisi ise butonları olacak."*
> *"Sayfadaki alanları yukarı gün, ay filtresine göre yap."*

**"Bu ay" değil `Eylül`, "Hepsi" değil `2026`.** Fark küçük görünüyor ama
*"bu ay"* ustaya **hangi** ay olduğunu söylemiyor; ekran görüntüsü alınıp bir
hafta sonra bakıldığında ya da ay dönüldüğünde rakamın hangi pencereye ait
olduğu kayboluyordu. Ad yazınca **rakam kendi tarihini taşıyor**.

**Süzgeç artık ekranın tamamına işliyor.** Önce yalnız *"bugün"* kartı sabitti
ve ödeme dağılımı **bütün** kayıtları topluyordu: iki rakam aynı ekranda **iki
farklı pencereye** bakıyordu ve hangisinin ne olduğu yazmıyordu.

*"Hepsi"* penceresi kalktı — dükkânın açılışından beri toplam, ustanın sorduğu
bir soru değil.

### 27.2 · "Kâr değil" açıklaması kalktı — RAKAM DA KALKTI

> *"Bu rakam kâr değil diye açıklama var. Onu kaldır."*

Açıklamayı kaldırıp **rakamı bırakmak, uyarısız bir yanlış rakam bırakmak**
olurdu. K-014 hâlâ geçerli: *eksik giderle çıkan kâr, kârdan kötüdür — çünkü
usta ona inanır.* O yüzden *"Kalan"* satırının kendisi kaldırıldı.

**Yerine geçen şey daha iyi:** iş başına fark. O rakam **her zaman doğru**,
çünkü iki bileşeni de zaten kayıtlı (satır fiyatı ve `alis`, T-9). Girilmemiş
bir şeye bağlı değil, dolayısıyla yalan söyleyemez.

### 27.3 · Rakamlar tabloya geçti

> *"Gider alanı excel gibi tablo şeklinde olsun."*
> *"Alınamayan (Veresiye) değil Alacak olsun başlık. Yine tablo gibi
> okunabilir olabilir."*

Gerçek `<table>` kullanıldı: rakam sütunu **hizalı** olmalı. `.tutar-serit`
iki sütunluk bir şerit; üç–dört sütun gerekince satırlar birbirini tutmuyordu.
Tablo ayrıca ekran okuyucuya *"bu bir tablo"* diyor.

**Dar ekranda kırpılmaz, kaydırılır** — K-025'in dersi doğrudan uygulandı:
sarmalayıcı `overflow-x: auto`.

**"Alacak" ustanın kelimesi.** *"Alınamayan"* bir **kayıp** gibi okunuyordu;
oysa para duruyor, yalnız henüz alınmadı. İlk sütun **KİM** — ustanın sorusu
*"ne kadar alacağım"* değil, **"kimden alacağım"** (K-013).

### 27.4 · İş başına kendi sayfasına çıktı

> *"Hatta iş başına bu ay başka sayfa olabilir, tamamını içerecek,
> filtrelenebilecek."*

Muhasebenin basit kipi bir **bakış** ekranı: açılır, okunur, kapanır. İş başına
liste ise **taranan** bir şey — usta hangi işin ne bıraktığını arar,
karşılaştırır, dönemi değiştirir. İkisi aynı ekranda dururken muhasebe **iki
farklı işi** birden yapıyordu.

Ekran 13 → **14**. *"Toplam parça alışı"* bölümü basit kipten kalktı; parça
alışı artık her işin **kendi satırında** duruyor. İki yerde iki liste, iki ayrı
bakım ve zamanla **iki ayrı rakam** demektir.

### 27.5 · Detaylı sekmesi bir MENÜ — ve yazılmamış modül açılır gibi durmaz

> *"Detaylı alanda ise, muhasebe modüllerine gidebilsin. Çek, senet, fatura vb.
> Menü şeklinde olsun."*

| Modül | Durum |
|---|---|
| İş başına · Parça alışı | **açılır** |
| Fatura · KDV · Çek · Senet · Cari hesap | **devre dışı**, *"yazılmadı"* rozetiyle |

**Menüde görünüp dokununca hiçbir şey yapmayan bir madde**, tam olarak 27.
testin kovaladığı şeydir (ölü uç) — ve en pahalı hâli budur, çünkü usta *"bu
program bunu yapıyor"* sanır. Devre dışı düğme ölü uç değildir: dokunulamaz ve
nedenini söyler.

**Kapsam genişlemedi, YERİ AÇILDI.** `hazir: false` olanların hepsi ustadan
**yeni veri** ister ve K-022'nin gerekçesi duruyor. Cemil kapsamı genişletirse
menü zaten yerinde.

> **Cemil onayına açık (A-M11):** bu beş modül **kapsama girdi mi**, yoksa
> yalnız *"ileride buraya gelecek"* mi? Varsayım: **yeri açıldı, kapsam
> açılmadı** — K-022 duruyor.

### 27.6 · Dönem çipleri iki ekranda; delegasyon çekirdekte

`[data-donem]` dinleyicisi muhasebe ekranında yazılmıştı ve başında
`if (Ekran.adi() !== 'muhasebe') return;` vardı. Sonuç: **iş başına
sayfasındaki dönem düğmeleri ölüydü** — basıyorsun, çip seçili görünmüyor,
tablo değişmiyor, hata da yok.

**Bu, K-020.2'nin AYNI hatası:** iki ekranın kullandığı bir denetimin davranışı
tek bir ekranın dosyasında duruyordu. Ortak nesnenin **davranışı da ortaktır**
— çekirdeğe taşındı. Aynı sebeple `muhDonemSecili` de çekirdekte: iki ekranda
iki ayrı dönem tutulsaydı usta muhasebede *Eylül* seçip iş başına sayfasında
*Bugün* bulurdu ve rakamların neden tutmadığını anlamazdı.

**Ölçüldü:** Eylül → 1 iş / 2.800 ₺ · 2026 → 5 iş / 12.970 alınan · 7.160 parça
· **5.810 fark**. Dönem iki ekran arasında taşınıyor.

### 27.7 · Kapsam notu ekrandan kalktı

> *"En alttaki karar bilgisini kaldır. Sende var zaten karar."*

Haklı: ekran, kendi kapsam tartışmasını **kullanıcıya** anlatıyordu. O tartışma
`KARARLAR.md`'de ve `arastirma/MUHASEBE-KAPSAMI.md`'de duruyor — ustanın
ekranında değil. Ekranın *"burası yarım"* demesi (K-013) hâlâ doğru bir ilke,
ama **kime** dediği önemli: yarımlık artık *"yazılmadı"* rozetiyle **modül
menüsünde** söyleniyor.

---

## K-026 · Plaka kapının kendisi; ses bir kapı değil; muhasebe iki kipli (05.09.2026)

Cemil dokuz madde verdi. Hepsi uygulandı; üçü bir **kararı** değiştirdi.

### 26.1 · "Sesli işlem" menüden kalktı — ses kalktı DEĞİL

> *"En üstten sesli işlemi kaldıralım. Çünkü karışıklık olacak orada."*

Menüde altı satır var ve **her birinin sağında kendi mikrofonu** duruyor —
*"o satırın işini söyleyerek yap."* Yedinci satır olarak duran *"Sesli işlem"*
ise hangi işi yapacağını söylemiyordu; usta için iki ayrı şey gibi görünüyordu,
oysa aynı şey.

**Kalkan bir KAPI, yetenek değil.** Altı mikrofon duruyor, arama kutularının
mikrofonları duruyor (K-020), ses ekranı bağlamıyla açılmaya devam ediyor
(K-017). K-016 hâlâ tutuyor: sesin yaptığı her işin elle yolu var.

### 26.2 · PLAKA, "kayıtlı mı" sorusunun kendisi

> *"Kayıtlı araç mı geldi kısmını kaldır… Eğer daha önce gelmiş araç ise
> ruhsata gerek yok… Değilse ruhsatı çekmesini isteriz. Plakayı yaz alanı ve
> tab olabilir. Ses açık tab olur."*
> *"Ruhsatı çek ve plakayı yaz tab olsun."*

Ekranın tepesinde *"Kayıtlı araç mı geldi?"* diye bir çip listesi vardı ve
kayıtlı araçları **tek tek sayıyordu**. Dört araçta çalışır, kırk araçta
çalışmaz. **Plakanın kendisi zaten bu sorunun cevabı.**

| Plaka yazıldı | Ne oluyor |
|---|---|
| **Kayıtlı** | *"Bu araç kayıtlı — Renault Mégane · Mehmet Kaya"* · **ruhsat istenmez**, tür seçilir; açık kaydı varsa oraya götüren düğme çıkar |
| **Kayıtsız** | *"Bu araç kayıtlı değil"* · ruhsat istenir; ruhsat yoksa araç alanları açılır ve **elle ya da sesle** doldurulur |

**İki sekme:** *Plakayı yaz* (varsayılan, **mikrofonlu** — Cemil: *"ses açık
tab olur"*) ve *Ruhsatı çek*. Ruhsat okununca ekran **plaka sekmesine düşer**:
okunanı görmeden düzeltilemez.

**Sesle ne söylenebileceği YAZIYOR** (*"Sesle girişte beklediğimiz bilgileri
gösterelim"*). Boş bir mikrofon, ustaya *"ne diyeceğim"* diye düşündürüyordu.

### 26.3 · Üçüncü tür: "Tamirat + Bakım" — K-006 genişledi

> *"Sonra altına Tamirat, Bakım veya Tamirat + Bakım yazabilir."*

K-006 iki tür tanımlamıştı ve gerekçesi sağlamdı (km ve hatırlatma yalnız
bakımda). Ama **gerçek ziyaret ikisi birden olabiliyor** — araç triger için
geliyor, madem açıldı bakımı da yapılıyor. İki ayrı kayıt açmak ustayı aynı
aracı iki kez yazmaya zorluyordu.

**Kural tek cümle:** *bakım içeriyorsa bakım kuralları işler* — `bakimIceriyor()`.
Bu fonksiyon olmadan `tur === 'bakim'` kontrolü **sekiz yerde** duruyordu; üçüncü
tür eklenince sekizinin de değişmesi gerekirdi ve biri unutulsa **hatırlatma
sessizce kurulmazdı.**

> **Cemil onayına açık (A-M10):** *"Tamirat + Bakım"* tek kayıt mı olmalıydı,
> yoksa liste satırında iki kaydın **özeti** mi? Tek kayıt varsayıldı.

### 26.4 · Servis kayıtları araç kartı diline geçti

> *"Servis kayıtları araç listelemesi gibi olsun. Araç bilgileri belirgin
> olacak."* · *"Üstte yeni servis kaydı buttonunu kaldır. İçeride teslim
> edilmiş filtresini kaldır."*

**Düğme neden gereksiz:** menüde zaten *"Servis kaydı ekle"* var ve bu ekrana da
oradan geliniyor. Aynı işi iki yerde göstermek *"hangisinden gireceğim"*
sorusunu doğuruyordu — 03.09'da *"Araç kaydı"* menü maddesi tam bu sebeple
kalkmıştı.

**Süzgeç neden gereksiz:** liste artık araç kartı dilini kullanıyor ve
**içeridekiler üstte, çerçevesi farklı** duruyor. Süzgeç, çerçevenin zaten
söylediğini ikinci kez soruyordu. **K-004 duruyor:** *"içeride"* bir durum
değil, `teslimTarihi`'nden türetilen bir görünüm.

**Arama genişledi:** liste artık marka ve sahibi **gösterdiği** için onlarda da
aranıyor. Gösterilen bir şeyin aranamaması, ustaya *"yok"* dedirtir.

### 26.5 · Muhasebe iki kipli — basit varsayılan, detaylı opt-in

> *"Basit muhasebe olarak düşünelim. Fakat daha fazlasına ihtiyaç duyanlar için
> de bir geliştirme yapmalıyız."*

| Kip | İçinde ne var | Ustadan istediği karar |
|---|---|---|
| **Basit** *(varsayılan)* | bugün tahsilat · ödeme dağılımı · giderler · parça alışı · veresiye | **sıfır** |
| **Detaylı** *(opt-in)* | dönem (bugün/bu ay/hepsi) · para nereye gitti · **iş başına fark** | 1 (dönem) |

**Karar bütçesi (K-022.4) böyle korunuyor:** ustanın çoğunluğu için ekran hâlâ
sıfır kararlı; isteyen bir dokunuşla derinleşiyor.

**Detaylı kip HİÇBİR YENİ ALAN İSTEMİYOR.** *"Daha fazlası"* demek **daha fazla
veri girmek** değil — var olan veriye başka bir pencereden bakmak. Teslim
tarihi, satır fiyatı ve alış fiyatı zaten kayıtlı.

**"Kâr" denmedi, "Kalan" dendi.** Kira, elektrik, maaş girilmemiş olabilir;
*kâr* deseydik usta ona **inanırdı** (K-014). Ekran bunu açıkça yazıyor.
**İş başına fark** ise her zaman doğru: müşteriden alınan eksi o iş için
parçaya ödenen — ikisi de kayıtlı.

### 26.6 · Dikey ritim 14 → 18 px

> *"Elementler birbirine çok yakın. Bunu genel olarak kontrol et."*

Ölçüldü: altı ekranda kart ve alan araları **12–14 px**, bir yerde **0 px**
(kalem listesi ile *"+ Parça"* düğmeleri bitişikti ve düğmeler listenin bir
parçası gibi okunuyordu).

Tek tek yamamak yerine **ritim tek yerden büyütüldü** — v1 dersi: boyutlar
bileşene gömülüyse *"hepsini büyüt"* demek yüzlerce satıra dokunmak demek.

**33. test (SIKISIK)** eklendi: iki kardeş blok arası **8 px'ten az** ise bulgu.
`.field` içindeki etiket–kutu 6 px'i **kasıtlı** ve ölçüme girmiyor — birbirine
ait olduklarını söylüyor. Negatif testle doğrulandı (0 px geri konunca 1 bulgu).

### 26.7 · Denetim ölçtüğü şeyi YİNE bozuyordu — ikinci yüz

K-024.1 veri kirliliğini kapatmıştı. Bu turda aynı hatanın **ikinci yüzü**
çıktı: 30. test (SAHTE-EYLEM) her düğmeye basarken **sekmelere de** basıyordu ve
sekme, ekranın kipini **ekranın kendi değişkeninde** değiştiriyor — denetimin
veri yedeği oraya ulaşamaz. Sonuç: denetimden sonra muhasebe **detaylı kipte**
açılıyordu.

İki düzeltme:
- `role="tab"` sahte-eylem testinden **elendi** — sekme hiçbir eylem *iddia*
  etmiyor, dolayısıyla sahte eylem olamaz. **Test etmediği bir şeye dokunmasın.**
- Muhasebe ekranı **kipini her çizimde uyguluyor**. Önce gizleme yalnız
  tıklamanın yan etkisiydi; ekran başka bir yoldan çizilince değişken *"basit"*
  diyor ama ekranda detay duruyordu. **Çizim, değişkenin dediğini göstermeli.**

### 26.8 · "Müşteri getirdi" bir bakışta okunuyor

Bilgi zaten çiplerde vardı ama **çip bir seçim aracı**: hangisinin basılı
olduğunu anlamak için bakmak gerekiyordu. Oysa bu K-005'in **sorumluluk
anahtarı** — sorun çıktığında işçilik alınıp alınmayacağını belirler. Rozet
yalnız **müşteri** getirdiyse çıkıyor; tamirci getirmesi normal hâl, her satıra
rozet koymak gürültü olurdu.

---

## K-025 · Ölçüm üç genişlikte yapılır; kırpılan metin taşma sayılmaz (04.09.2026)

**Karar:** Sağlık denetimi bundan sonra **320 · 375 · 430 px**'te koşulur.
Tek genişlikte ölçmek, o genişlikte doğru olan bir tasarımı *"doğru"* sanmaktır.

| Genişlik | Ne temsil ediyor | Sonuç (04.09) |
|---|---|---|
| **320 px** | Küçük/eski telefon — en dar gerçek cihaz | 0 bulgu *(düzeltmeden sonra)* |
| **375 px** | Referans ölçü, bugüne kadarki tek ölçüm | 0 bulgu |
| **430 px** | Büyük telefon | 0 bulgu |

### 25.1 · Bulunan hata: 320 px'te düğme metni SESSİZCE kırpılıyordu

Araç ekranının üç düğmesi (K-019.4) **375 px'e göre** ölçülmüştü. 320 px'te
*"Açık kayıt"* **102 px** istiyor, kutu **89 px**.

**Hiçbir test görmedi ve göz de görmedi.** Sebebi kendi yazdığım
`overflow: hidden`: kutu taşmıyordu, **kesiyordu.** 20. test (IC-TASMA)
`overflowX !== 'visible'` olan kutuları **kasten atlıyor** — doğru, çünkü
onlar taşmıyor. Taşmamalarının sebebi içeriği kesmeleri.

Düğme *"Açık kay"* yazıyordu.

**Düzeltme — dar ekranda İKON kalkar, YAZI kalır.** Anlamı yazı taşır; ikon
süstür. 24 px ikon + 5 px boşluk = 29 px, sorunu fazlasıyla çözüyor.
`overflow: hidden` de **kaldırıldı** — bir daha olursa taşma testi görsün.

**Ölçüldü:** 320 px ikonlu 102 > 89 (kırpık) → ikonsuz **89 = 89** ·
375 px **107 = 107** · 430 px **125 = 125**.

### 25.2 · 32. test: KIRPILAN-METIN

> **Kesilen metin, taşan metinden tehlikelidir:** taşan görünür, kesilen
> görünmez. Kullanıcı eksik bir kelime okur ve eksik olduğunu bilmez.

Ölçüm: `overflow` görünür olmayan bir kutuda `scrollWidth > clientWidth` ve
kutunun **metni varsa** bulgu. İki şey **kasten** dışarıda:
- `overflow: auto/scroll` — kullanıcı kaydırabiliyorsa kırpılmış sayılmaz
- `text-overflow: ellipsis` — tasarımcı bilerek kısaltmış ve **üç nokta
  kullanıcıya bunu söylüyor**

Negatif doğrulama gerekmedi: test **yazıldığı anda gerçek bir bulgu verdi**
(`btn "Açık kayıt" 102>89`), düzeltmeden sonra 0'a düştü. Bir testin
doğruluğunun en iyi kanıtı, yazılırken var olan bir hatayı bulmasıdır.

---

## K-024 · K-016 tutuyor — ama denetimin kendisi bozuktu (04.09.2026)

**Karar:** *"Ses var olan işi kısaltır, yeni yetenek açmaz"* (K-016) **ölçüldü
ve doğrulandı.** Sesin yaptığı dokuz işin **dokuzunun da** elle yolu var.

| Sesin yaptığı | Elle karşılığı |
|---|---|
| `servis-ac` | araç ekranı → **Kayıt aç** → tür seç |
| `yeni-arac` | menü → Servis kaydı ekle → ruhsat |
| `teslim` (+ödeme) | kayıt → Teslim et → ödeme çipi |
| `satir-ekle` | kayıt → **+ Parça / + İşçilik** |
| `gider-ekle` | muhasebe → **Gider ekle** ← *bugün eklendi (K-021.1)* |
| `mesaj` | mesaj ekranı → müşteri + şablon |
| `ara` (+süzgeç) | her listenin arama kutusu + kayıtlardaki çipler |
| `bilinmeyen-arac` | ruhsat → elle gir |
| `teslim-edilecek-kayit-yok` | araç ekranı |

**⚠️ K-016 bu sabah ÇİĞNENMİŞ DURUMDAYDI.** `gider-ekle` yalnız sesin
yapabildiği bir işti; elle yolu **yoktu** ve Web Speech Firefox'ta çalışmıyor
(K-015). Ölü uç turunda bulundu (K-021.1). Kural yazılıydı, denetlenmiyordu.

### 24.1 · ⚠️ DENETİM ÖLÇTÜĞÜ ŞEYİ BOZUYORDU

Bu turun en pahalı bulgusu.

**30. test (SAHTE-EYLEM) her düğmeye basıyor**, 27. test `ACIK_KAYIT` ve
`ACIK_PLAKA` atıyor. Denetim `load` anında **kendiliğinden koştuğu** için
prototip **her açılışta kirli başlıyordu**: taze yüklenmiş bir sayfada
`SESLI_NIYET = 'kayit-icinde'`, `ACIK_KAYIT = 1` duruyordu.

Sonuç: sesli işlem çözümleyicisi **her cümleyi *"satır ekle"* diye okuyordu.**
Dokuz farklı cümle denendi, dokuzu da aynı cevabı verdi.

> **Kendi ölçtüğü şeyi değiştiren bir araç, ölçüm aleti değildir.**

Denetim artık başta **derin kopya** alıyor, sonda **geri yüklüyor**.

**Ve düzeltir düzeltmez GERÇEK bir hata ortaya çıktı:** özet ekranı boş
durumda **ölü uçtu** — hiçbir dokunulabilir öge yoktu. 27. test bunu
göremiyordu, çünkü `OZET_KAYIT` denetimin **kendi tıklamalarıyla** doluyordu
ve ekran hep dolu çiziliyordu. Test *"geçiyor"* değildi, **yanlış şeye
bakıyordu.**

### 24.2 · K-018 ÜÇÜNCÜ KEZ — kısa kelime, başka kelimenin içinde

`` yasaklandı (K-018), yerine `indexOf` kondu ve **aynı hata geri geldi.**
Ölçüldü:

| Usta ne dedi | Program ne anladı | Neden |
|---|---|---|
| *"41 KV 5588 yeni **araç** bakım"* | **arama** | `"arac"` içinde `" ara"` var |
| *"34 ABC 123 **beyaz** araba geldi"* | **mesaj** | `"beyaz"` içinde `"yaz"` var |
| *"34 ABC 123 **gösterge** paneli yanıyor"* | **arama** | `"gösterge"` içinde `"göster"` var |

Üçü de ustanın **en sıradan** cümleleri. Kural bir kez daha keskinleşti:

> **Kısa komut kelimeleri İÇİNDE aranmaz** — boşluktan bölünüp **eşitlikle**
> karşılaştırılır (`kelimeVarMi`). Uzun ve ayırt edici kökler (`mesaj`,
> `whatsapp`, `hatırlatma`, `içeridek`) için içinde aramak hâlâ doğru; onlar
> başka kelimenin içine düşmüyor.

**31. test (SES-KELIME-TUZAGI)** bu üç cümleyi ölçüyor. Neden kural değil
**davranış** ölçülüyor: kodda `indexOf` aramak yanlış pozitif yağdırır —
uzun kökler için `indexOf` doğrudur. Ölçülecek şey, tuzak cümlenin doğru
okunup okunmadığı. Negatif testle doğrulandı (eski davranış → 3 bulgu).

### 24.3 · Eski derleme ölçülmez — araca koruma kondu

*"derle → geri → göm"* sırasıyla koşuldu; `geri` kendi yedeğini yazıyor ve
**taze derlemeyi ezdi.** Sonra o eski dosyaya denetim gömüldü. Tarayıcıda
ölçülen şey kaynağın hâli **değildi** ve bu ancak *"eklediklerim neden
görünmüyor"* diye bakınca fark edildi.

> ***"0 bulgu"* raporlamanın en sinsi yolu bu:** denetim doğru koşuyor, doğru
> ölçüyor, ama **yanlış dosyayı** ölçüyor.

`denetim-kos.py gom` artık kaynak `omurga.html`'den yeniyse **reddediyor**.
Negatif testle doğrulandı.

---

## K-023 · Müşteriye PDF değil LİNK gider; özet ustanın ekranı değildir (04.09.2026)

**Karar:** Yedi işten altıncısı (*müşteriye özet göndermek*) yazıldı.
Müşteri bir **sayfa** görüyor, bir dosya değil.

### 23.1 · PDF neden değil

Tamircinin bugünkü işi (S8): *"müşteriye iş listesinin yani tutarın iş,
parça açıklaması ve Toplam tutar olarak pdf gönderiyorlarmış. Bunu WhatsApp
üzerinden uygulama linki gönderme fikri mantıklı geldi."*

| | PDF | Link |
|---|---|---|
| Üretmek | **sunucu ister** | ister istemez |
| Gönderildikten sonra | **ölü** — o anki hâli taşır | canlı, iş sürerken güncellenir |
| Müşteri bir hafta sonra | dosyayı bulamaz | linke yine dokunur |
| T-21 (*"müşteride de program olsun"*) | karşılamaz | **en ucuz hâli** — kurulum yok |

**Mesaj yine de bir işe yarıyor:** WhatsApp'ta linke dokunmayan da olur, o
yüzden **kalem sayısı ve toplam mesajın içinde** duruyor. Tam liste sayfada.

### 23.2 · Bu ekran ustanın ekranı DEĞİL

Ekran sayısı 12 → **13**, ama ekran bütçesi (A-2) **ustanın** ekranlarını
sayıyor; bu sayfayı müşteri görüyor. Prototipte ustanın **önizlemesi** olarak
duruyor ve **tepesinde bunu yazıyor** — yazmasaydı bir sonraki oturumda
*"13. ekran nereden çıktı"* diye sorulurdu.

Ustanın erişimi mesaj ekranındaki **"Müşteri ne görecek?"** düğmesi. Yalnız
**özet şablonunda** çıkıyor; öteki iki şablon link taşımıyor, gösterilecek
sayfa da yok.

### 23.3 · Sayfanın üzerinde ne YOK ve neden

| Yok | Neden |
|---|---|
| Fiyat düzenleme, satır ekleme | Müşterinin sayfası — salt okunur |
| Onay / red düğmesi | **Onay mekanizması kurulmuyor** (K-003, S4) |
| Ödeme alanı | Para dükkânda alınıyor; kapsam dışı |
| Alış fiyatı (`alis`) | **Müşteriye gitmez** (`VERI-MODELI §3`) |
| Başka araç, geçmiş kayıt | Link tek kaydın |

**Müşteri parçası SÖYLENİYOR.** Fiyatsız bir satırı sessizce göstermek
müşteriye *"bu bedavaydı"* diye okunur; oysa parayı o ödemiş (S2, K-005).
Satırda *"parçayı siz getirdiniz"* yazıyor.

**Durum tek** (K-004): *"Aracınız serviste"* ya da *"… tarihinde teslim
edildi"*. Müşterinin ikinci sorusu bu.

> **Cemil onayına açık (A-M9):** sayfada **kimlik doğrulama yok** — linki olan
> görür. Link plaka + kayıt numarasından türüyor, yani **tahmin edilebilir**.
> Gerçek kullanımda tahmin edilemez bir kod gerekir. Prototipte kabul;
> backend fazında kapatılacak.

### 23.4 · Ekranlar arası devredilen değer çekirdekte durur

`ozetLinki()` iki ekranda gerekiyordu (mesaj metni ve özet sayfası). Ekran
betikleri derlemede **kendi IIFE'sine sarıldığı** için özet ekranında
tanımlansaydı mesaj ekranı onu **göremezdi** — ve hata, mesaj metninde sessizce
eksik bir link olarak çıkardı. `OZET_KAYIT` ile birlikte çekirdeğe alındı;
`MESAJ_HEDEF`, `ARAMA_TERIMI`, `RUHSAT_ONDOLGU` zaten orada.

---

## K-022 · Muhasebe kapsamı: kasa + gider. Cari hesap kapsam dışı (04.09.2026)

> **Karar taslağı — Cemil onayına açık (MUH).** Lider Cemil yokken
> hazırladı; ölçümler ve gerekçe `arastirma/MUHASEBE-KAPSAMI.md`'de.
> Kod tarafında yalnız **hiçbir yeni alan istemeyen** kısım yazıldı.

### 22.1 · Seçenek B — kasa + gider

| Seçenek | Yeni veri girişi | Yalan söyleyebilir mi | Yedi işten hangisi |
|---|---|---|---|
| A · günlük kasa | **sıfır** | hayır | **4** — teslimde ödemeyi bilmek |
| **B · kasa + gider** | her gider için bir giriş | evet, eksik girilirse | 4'ün komşusu |
| C · cari hesap | açılış + her ödeme + her kısmi ödeme | **evet, ve kalıcı** | **hiçbiri** |

**C neden kapsam dışı:** yedi işten hiçbirine hizmet etmiyor · **K-012'yi geri
alıyor** (müşteri tablosu yok, müşteri aracın üstünde iki alan) · bir kez kayan
bakiye bir daha tutmaz. Paraşüt'te cari hesap bir **ekran ailesi**: ekstre,
mutabakat, virman, ödeme hatırlatma. Bu, tamircinin *"karmaşık"* dediği yığının
tam ortası. *"İleride lazım olur"* gerekçe değildir.

**B'nin sınırı keskin:** fatura yok · KDV yok · tevkifat yok · e-belge yok ·
banka yok · çek/senet yok · stok yok · gider kategorisi yok · aylık özet yok.
Hepsi rakiplerde var; **hiçbiri yedi işten birine hizmet etmiyor.**

### 22.2 · Parça alışı bedavaya geldi — ve bu bir kural örneği

`ServiceLine.Cost` (`alis`) **zaten kayıtlı**, çünkü T-9 var: *"Usta kendi fiyat
aldıysa, parça fiyatlarına ekleme yapabiliyor."* Tamircinin parçaya ödediği para
veri modelinde duruyor ve ustadan **tek bir yeni giriş istemiyor.**

K-013'ün ölçüsü şuydu: *"girilmezse yalan söyleyen rakam gösterilmez."* Bu rakam
o gruba ait değil — **her zaman doğru.** Bu yüzden yazıldı.

**İki süzgeç, ikisi de gerekçeli:**
- **Yalnız tamircinin aldığı parçalar.** Müşteri getirdiyse para tamirciden
  çıkmamıştır (K-005); sayılsaydı ödenmemiş bir para gider gibi görünürdü.
- **Yalnız teslim edilmiş kayıtlar.** İçerideki araçta parça daha alınmamış
  olabilir; olmamış bir harcamayı gider saymak ustayı olduğundan yoksul gösterir.

**⚠️ Çift sayma tehlikesi ekranda YAZILI.** Usta parçayı hem satıra alış olarak
yazıp hem elle *"Öz Yedek Parça — triger seti"* diye gider ekleyebilir. Bu
yüzden parça alışı, elle giderlerden **ayrı** duruyor ve ekran uyarıyor.
Karıştırılsaydı çift sayma **sessizce** olurdu.

**Ölçüldü (04.09):** 5 satır · **7.160 ₺** · içerideki kayıtlar sayılmadı ·
müşteri parçası sayılmadı.

### 22.3 · Kâr yine hesaplanmıyor — ama sebebi inceldi

**İş başına kâr** (`fiyat − alış`) matematiksel olarak doğru çıkar; **dükkân
kârı** çıkmaz, çünkü kira/elektrik/maaş girilmemiş olabilir. İkisi aynı ekranda
dururken usta ikincisini birincisi sanar.

İş başına kâr **önerildi ama yazılmadı** — yeni bir rakam ve karıştırılma riski
var. Cemil onayına açık (A-M7).

### 22.4 · YENİ KISIT: KARAR BÜTÇESİ

İki rakip incelemesi de **bağımsız olarak** bunu istedi. Ekran bütçesi (A-2)
karmaşıklığın yalnız bir kaynağını kısıtlıyor; ölçüm **üç** kaynak olduğunu
gösterdi: **menü genişliği · formdaki karar sayısı · dil.**

| Kısıt | Değer | Bugünkü ölçüm |
|---|---|---|
| Üst menüde başlık | en fazla **8** | **7** |
| Ana yolda bir ekranda sabit karar | en fazla **3** | **3** (ruhsat) |
| Alan adında tamircinin ağzından çıkmayan kelime | **0** | 0 |

**Varsayılanı olan alan karar sayılmaz** — rakiplerle aynı ölçü. Paraşüt'ün
fatura formunda ~10 alan görünüyor ama gerçekten doldurulan **2**; LucaNet'te
~20 alan, hiçbiri katlanmamış, en az 4–5 karar.

**Tamiron'un ana yolu ölçüldü (04.09):** ruhsat fotoğrafı çekilince 5 alan
kendiliğinden doluyor; tek parçalı bir tamirat baştan sona **5–6 karar**.

Üçüncü kısıt LucaNet'in tersi: *"Senaryo Tipi"*, *"Muhasebeleşebilir mi?"*,
*"Muhtelif Müşteri"* — T-18'in tam karşıtı. `SOZLUK.md` bu testin yeri.

---

## K-021 · Ölü uç turu: iki sahte eylem ve bir yalan söyleyen düğme (04.09.2026)

**Karar:** On iki ekranın hepsi tek tek **işletildi** — çizildiğine
bakılmadı, dokunuldu. Üç bulgu çıktı ve üçü de düzeltildi.

### 21.1 · Muhasebede gider yalnız SESLE eklenebiliyordu

Gider bölümünün tek girişi mikrofondu; boş durumu bile *"Mikrofona
basıp '2400 TL elektrik faturası ekle' demen yeter"* diyordu.

**Neden ölü uç:** K-015 — Web Speech **Firefox'ta yok** ve **güvenli
bağlam** ister. Mikrofonu açılmayan bir tarayıcıda gider bölümü
**tamamen kullanılamıyordu** ve ekran bunu söylemiyordu.

K-014 zaten *"sesle **ve elle** giriş"* diyordu; elle olan hiç
yazılmamıştı. **Ses bir kısayoldur, tek kapı değil** (K-016).

Elle giriş eklendi: iki alan (ne için · tutar) + Ekle. Tutar alanı
rakam dışını atıyor — usta *"2.400 TL"* yazınca `parseInt` tek başına
**2** okuyordu.

### 21.2 · "Onay iste" düğmesi yalan söylüyordu

Düğme *"Onay iste"* diyordu, basınca balon *"Özet gönderildi —
0532…"* diyordu ve **hiçbir şey gönderilmiyordu.** Mesaj ekranı zaten
varken oraya bile gitmiyordu.

**Üç ayrı sorun tek düğmede:** düğme ile balon farklı şey söylüyor ·
sahte bir gönderim rapor ediliyor · gerçek yol varken kullanılmıyor.

Düğme **"Özet gönder"** oldu ve mesaj ekranına özet şablonuyla
gidiyor. Ad neden değişti: tamirci onay için *"işin içine
katmayalım"* dedi (S4, K-003); düğmenin gerçekte yaptığı iş ise yedi
işten **altıncısı** — müşteriye özet göndermek (T-19).
**Cemil onayına açık (A-M6).**

> **Sessiz bir ölü uçtan pahalı:** bir kez *"gönderildi"* yazısına
> kanan usta, bir daha **hiçbir** onay yazısına inanmaz.

**Denetime 30. test (SAHTE-EYLEM) eklendi.** Ölçüm: balon tamamlanmış
bir eylem bildiriyorsa (*gönderildi · eklendi · kaydedildi · silindi ·
güncellendi*), ya **veri** ya **ekran** değişmiş olmalı; ikisi de aynı
kaldıysa balon yalan söylüyor. Negatif testle doğrulandı (eski davranış
geri kondu → 1 bulgu; kaldırılınca → 0).

### 21.3 · A-M2 uygulandı — mesaj ikinci iletişime gidiyor

**Varsayım (Cemil onayına açık):** ikinci iletişim varsa özet ve
hatırlatma **ona** gider, yoksa ruhsattaki numaraya. Usta mesaj
ekranından hedefi **değiştirebiliyor** — varsayım bir kilit değil,
varsayılan. Kime gittiği **göndermeden önce** yazıyor.

**Test bir hata daha buldurdu:** firma ruhsatlı araçta mesaj
*"Demir Nakliyat Ltd. Şti. **Bey/Hanım**,"* diye başlıyordu — müşteriye
giden **ilk cümle** bu. Ad firma işaretleri taşıyorsa (`Ltd · Şti ·
A.Ş. · San · Tic …`) *"Bey/Hanım"* yazılmıyor. Ayrı bir "tüzel kişi"
alanı **açılmadı**; ada bakılıyor.

⚠️ Firma araması **boşluktan bölüp eşitlikle** yapılıyor (K-018):
desenle arasaydık *"Astan"* içindeki *"as"* firma sanılırdı. Ölçüldü —
`firmaMi('Astan Aslan')` = false.

### 21.4 · `Ekran.ac()` zaten açık ekranda sessizce hiçbir şey yapmıyordu

Aynı ekrana ikinci kez gidilince düz `return` ediliyordu. v2'de
ekranlara **kuyrukta veri bırakılarak** gidiliyor (`MESAJ_HEDEF`,
`ARAMA_TERIMI`, `RUHSAT_ONDOLGU`, `SESLI_NIYET`): veri bırakılıyor,
ekran açıldı sanılıyor, `ciz()` koşmadığı için ekran **bir önceki
müşteriyi göstermeye devam ediyordu** — ve hata mesajı yok.

**Bu, K-017'nin hata sınıfının aynısı.** Orada çözüm sesli işleme özel
yazılmıştı (`sesleAc` tek giriş noktası) ve **kural taşınmadı**; aynı
tuzak mesaj ekranında yeniden kuruldu. Artık kökünden kapalı: zaten
açık ekrana gidilirse **tazelenir**, geçmişe ikinci kez yazılmaz.

> **Buna test yazılmadı, çünkü sınıf artık İMKÂNSIZ.** Yapısal olarak
> kapatılabilen bir hata için test yazmak, kapıyı kilitleyip yanına
> bekçi dikmektir. Ölçüldü: geri düğmesi aynı ekranı iki kez saymıyor.

---

## K-020 · Mikrofon, işin yanında durur — kendi yanında değil (04.09.2026)

**Karar (Cemil, üç madde):**

> *"Araçlar arama mic button olsun."*
> *"Sesli işlem buttonunun yanında mik button olmasın."*
> *"Bildirimler başlığı üstte kalıyor. Ss'te mevcut."*

### 20.1 · Arama kutusunun kendi mikrofonu var

Mikrofon **arama kutusunun sağ ucunda**, `.mic-round.ghost` olarak.
Bastığında sesli işlem **o listenin bağlamıyla** açılıyor (K-017):
araçlarda `araclar`, kayıtlarda `kayitlar`, müşterilerde `musteriler`,
mesajda `mesaj`.

> **Cemil onayına açık (A-M5):** Cemil yalnız **araçları** söyledi;
> mikrofon **dört arama kutusuna birden** kondu. Gerekçe: dördü de
> aynı iş (*"aradığımı söyleyerek bulayım"*) ve menüde bu iş için
> zaten dört ayrı mikrofon var; birine koyup ötekine koymamak,
> ustaya *"hangisinde çalışıyordu"* diye düşündürür. İstenmezse
> üç satır silinerek geri alınır.

### 20.2 · Sesli işlem satırının yanında mikrofon YOK

Öteki menü satırlarında mikrofonun bir işi var: *"bu satırın işini
söyleyerek yap."* **Sesli işlem satırının kendisi zaten o iş.** Yanına
ikinci bir mikrofon koymak, aynı düğmeyi iki kez göstermekti.

**Bu bir hatayı ortaya çıkardı:** `[data-sesle]` dinleyicisi **ana
ekranın betiğinde** duruyordu. Ana ekranın dışında mikrofon yokken
sorun değildi; arama kutularına mikrofon eklenince dört ekranın
davranışı, hiç ilgisi olmayan bir ekranın dosyasından gelmeye başladı.
Ana ekran yeniden yazılsa bütün mikrofonlar **sessizce** ölürdü —
tıklıyorsun, hiçbir şey olmuyor, hata da yok. Delegasyon çekirdeğe
taşındı (ortak nesne kuralı: davranış tek yerde).

### 20.3 · A-M4 kapandı — "Bildirimler" başlığı yapışmıyor

Ekran görüntüsü soruyu cevapladı: başlık `position: sticky` idi ve
çekmece kaydırılınca **altındaki bildirimin üstüne biniyordu**;
görüntüde bildirim kartının yalnız bir dilimi duruyor. Kural kalktı.

Yapışkan başlık **uzun** listeler içindir. Burada liste birkaç satır:
kazandırdığı şey yok, kaybettirdiği şey bildirimin kendisi.

**Denetime 29. test (ORTEN-YAPISKAN) eklendi** — ve ilk yazımı
**boştu**: negatif test bulgu vermedi, çünkü çekmece o anki veriyle
taşmıyordu. Kaydırma yoksa yapışkan öge hiç ayrılmaz. Test *"bu ekran
iyi"* demiyordu, *"bugünkü veri az"* diyordu. Doğru soru şu: **bu kap
kaydırılırsa başlık içeriği örter mi?** Test artık kabı 200 px'e
kısaltıp taşmayı **zorluyor**; ölçüm veriden bağımsız.

> **Ders (üçüncü kez):** bir testin yazılmış olması, ölçüyor olması
> demek değil. Negatif test olmasaydı bu delik *"29 test, 0 bulgu"*
> diye rapor edilecekti.

---

## K-019 · Araç ekranları Cemil'in tarifiyle yeniden kuruldu (04.09.2026)

**Karar:** Cemil araçlar listesinin satırını ve araç detayının sırasını
satır satır tarif etti. Tarif **olduğu gibi uygulandı**; bizim üç
gerekçemiz düştü. Aşağıda her biri ve neyi kaybettiğimiz yazıyor —
kaybı yazmayan bir karar, aynı tartışmayı altı ay sonra yeniden açar.

### 19.1 · Ana ekranda bakım hatırlatması YOK

> *"Anasayfada bir aracın bakımı geldi alanini istemiyorum. Sadece
> bildirimlerde gözüksün."*

**Düşen gerekçe:** K-011 ve T-17 *"ustanın 'bugün kimi arayacağım'
sorusu ilk ekranda cevaplanmalı"* diyordu ve şerit bu yüzden menüde
kalmıştı. **Ana ekran bir MENÜDÜR, pano değil** — Cemil haklı.

**Bilgi kaybolmadı:** aynı hatırlatmalar çekmecenin **Bildirimler**
bölümünde duruyor ve menü düğmesinin **rozeti** sayıyı ana ekrandan
gösteriyor. Soru hâlâ ilk ekranda cevaplanıyor, bir rakamla.

**Yan etkisi bir hatayı ortaya çıkardı:** aynı soru İKİ YERDE
süzülüyordu. Çekmece kendi `bekleyenHatirlatmalar()`'ını yazmıştı,
veri katmanındaki `hatirlatmasiGelenler()` ise ayrıca `tur === 'bakim'`
süzüyordu — hatırlatma tarihi olan bir **tamirat** kaydı çekmecede
*"Bakım zamanı geldi"* diye görünüyor, ana ekranda görünmüyordu.
Şerit kalkınca tek kaynak kaldı. **Aynı soruyu iki yerde cevaplamak,
iki ayrı cevap demektir.**

### 19.2 · Araçlar listesi: araç kartı, servis kaydı satırı değil

> *"Araç iconı olmayacak. Sol üstte plaka, altında marka model motor
> renk (renk yazmak yerine kutucuk içerisinde rengi olsun). Altında
> ruhsat sahibi. İçeride olanları üstte olsun. Div çerçeve farklı olsun
> fakat divin içerisinde text olarak içeride yazmasına gerek yok.
> Ruhsat sahibi telefon. Eğer ikinci bir iletişim varsa adı ve
> Telefonu. Ruhsat sahibi firma da olabilir hani."*
> *"Altında 1 kayıt, son bakım vb yazmasın."*
> *"Ayrıca liste telefondan okunmak için daha iyi gözüksün."*

**Düşen gerekçe:** 03.09'da Cemil *"araçlar listesi yine detaylı
olmalı"* demişti ve satıra model, motor, yakıt, son km, kayıt sayısı,
son bakım tarihi doldurulmuştu. **Detay ile kalabalık aynı şey
değilmiş.** Kalabalık listede plaka bile geç bulunuyor.

**Kaldırılanlar:** araç ikonu · *"içeride"* rozeti · kayıt sayısı ·
son bakım tarihi · son km · model yılı · yakıt.
**İçerideki araç** yalnız **çerçeveden** ve **sıradan** belli oluyor.

### 19.3 · Ruhsat sahibi firma olabilir → ikinci iletişim

`Vehicle`'a iki alan girdi: `ContactName` · `ContactPhone`
(`VERI-MODELI.md §1`). **Yeni tablo açılmadı** — K-012'nin kuralı
sürüyor: ekran gerekiyorsa tablo gerekmiyor olabilir.

**Adsız telefon yazılmaz.** Usta kimi aradığını bilmeden aramaz;
numara tek başına bir eylem değil, bir veri parçasıdır.

> **Cemil onayına açık (A-M2):** özet ve bakım hatırlatması hangi
> numaraya gidecek — ruhsattaki mi, ikinci iletişim mi? Varsayım:
> **ikinci iletişim varsa o**, çünkü aracı getiren o. Mesaj ekranı
> şu an hâlâ ruhsattaki numarayı kullanıyor.

### 19.4 · Araç detayının sırası: önce EYLEM, sonra BAŞVURU

> *"Araç detayda, işlem buttonlarının altında ruhsat bilgileri akordion
> olarak yer alsın. Altında km bilgileri yine akordion olarak olsun.
> Altına km bakım bilgileri diğer divler gibi başlıklı olsun."*
> *"Araç detayda işlem buttonları tek satırda yer alsın."*
> *"Buttonlarla ruhsat arasında not alanı olsun. v1'deki ile aynı."*

Yeni sıra: **künye → üç düğme (tek satır) → NOT → ruhsat (açılır) →
kilometre (açılır) → bakım bilgileri → geçmiş.**

Ruhsat ve kilometre **başvuru** bilgisidir — günde bir kez bakılır ama
ekranın yarısını kaplıyorlardı ve usta geçmişi görmek için iki ekran
boyu kaydırıyordu. Not ise aracın **en taze** bilgisi (*"arka cam kolu
kırık"*) ve ekranın dibinde kimse görmüyordu.

Üç düğme 375 px'e sığsın diye etiketler kısaldı (*Kayıt aç · Mesaj ·
Sesle*); 48 px dokunma hedefi korunuyor.

### 19.5 · Kilometre geçmişinde TAHMİN YOK

> *"Km geçmişinde tahmin için alanı olmasın. Tahmin de olmasın.
> Tarih ve km."*

Günlük ortalama satırı ve iki ölçüm arasındaki `+8.700` farkı **ekrandan
kalktı.**

**⚠️ HESAP KALKMADI.** K-002'nin *"ay veya km, hangisi önce gelirse"*
kuralı `gunlukKm()` ile çalışmaya devam ediyor; sonucu **Bakım
bilgileri** kartında **tek bir tarih** olarak duruyor. Kalkan şey,
ustanın önüne konan **ham tahmin**. Ara hesabı göstermek, ustadan onu
denetlemesini istemek demekti — o iş bizim.

### 19.6 · Araç geçmişi sadeleşti — ve K-005'in rozeti düştü

> *"Geçmiş altında listele: Başında icon yok. Bakım mı tamirat mı
> başlık. Marka model yazmasın. Ad soyad zaten belli. Başlık, açıklama,
> Tarih, teslim durumu. Basit olsun."*

**⚠️ BU BİR ÇELİŞKİ VE KAYBEDEN BİZİZ.** Araç ekranının var oluş
gerekçesi K-005'ti: *"bu parçayı geçen sefer müşteri getirmişti"*
bilgisi araç ikinci kez geldiğinde **karşıya çıkmalı** — çünkü
sorumluluğu o belirliyor (T-8: müşteri getirdiyse tekrar işçilik
alınır, tamirci aldıysa ücretsiz değişir). O bilgi geçmiş satırında
bir rozet olarak duruyordu; Cemil'in saydığı dört alanın içinde yok.

**Rozet kaldırıldı.** Tamircinin/Cemil'in sözü bizim akıl yürütmemizi
yener. Bilgi **kaybolmadı**: `SuppliedBy` satırın kendi verisi ve
kaydın kendi ekranında duruyor.

> **Cemil onayına açık (A-M3):** geçmiş satırında *"parçayı müşteri
> getirmişti"* uyarısı olmadan usta tartışmaya hazırlıksız yakalanır
> mı? Alternatif: liste sade kalır, **kaydı açınca** en üstte görünür.
> Şu anki hâl bu — kaydın kendi ekranında görünüyor.

---

## K-018 · Türkçe metinde `\b` kullanılmaz (04.09.2026)

**Karar:** Kelime atmak/aramak için JavaScript'in `\b` sınırı
**kullanılmaz.** Bunun yerine `kelimeleriAt()`: boşluktan böl,
eşitlikle karşılaştır.

**Neden — yaşanmış hata:** `/\bde\b/` deseni *"değişecek"* kelimesinin
başındaki **"de"yi tutuyordu.** JavaScript'in `\b`'si ASCII sınırına
bakar; `ğ`, `ş`, `ı`, `ç` kelime karakteri **sayılmaz**, o yüzden
"de" ile "ğişecek" arasında sınır var sanılıyor. Sesle *"balata da
değişecek"* denince kayda **"balata ğişecek"** düşüyordu.

Aynı tuzak `/\bara\b/` ile **"araç"** kelimesini **"ç"**ye indirirdi —
o hata henüz kimse söylemeden bulundu, çünkü aynı desen kullanılıyordu.

**Genel ders:** İngilizce için yazılmış metin araçları Türkçede sessizce
yanlış çalışır. Sessizce, çünkü hata mesajı yok — yalnız yanlış sonuç.

### ⚠️ 04.09 — AYNI TUZAĞA BU KARARDAN SONRA BİR KEZ DAHA DÜŞÜLDÜ

Sesle söylenen plakayı toparlamak için harf birleştirici yazdım ve
yine `\b` kullandım:
`/\b([A-Za-zÇĞİÖŞÜçğıöşü])(\s+[A-Za-zÇĞİÖŞÜçğıöşü])+\b/`

*"beş yüz"* içindeki **"ş y"** ikilisini "harf harf söylenmiş plaka"
sandı — çünkü `e`↔`ş` arasında da, `y`↔`ü` arasında da `\b` var.
Sonuç **"beşyüz"** oldu, *"kırk bir KV beş bin beş yüz seksen sekiz"*
plakası **41 KV 5000** olarak okundu.

**Kararı yazmak yetmiyor.** Kural artık şöyle keskin: *Türkçe metinde
kelime sınırı ARANMAZ, boşluktan BÖLÜNÜR.* İki fonksiyon da
(`kelimeleriAt`, `sesHarfleriBirlestir`) bu şekilde yazıldı.

---

## K-017 · Sesli işlem bağlamıyla açılır (03.09.2026)

**Karar:** Sesli işlem ekranı **hangi ekrandan/hangi mikrofondan**
açıldığını bilir; başlığında yazar (*"Sesli işlem · Araçlarda ara"*) ve
örneklerini ona göre gösterir. Bağlama geçiş **tek fonksiyondan**
yapılır: `sesleAc(niyet)`.

**Neden:** Ustanın *"ne diyebilirim"* sorusu geldiği yere göre farklı.
Muhasebeden gelen *"2400 TL elektrik faturası ekle"* der, açık servis
kaydından gelen *"balata da değişecek"*. Herkese aynı örnekleri
göstermek, ekranı **herkes için biraz yanlış** yapıyordu.

**Neden tek fonksiyon:** her mikrofon kendi başına `SESLI_NIYET = x;
Ekran.ac('ses')` yazıyordu ve ekran **zaten açıkken** `Ekran.ac()` erken
dönüyordu: bağlam değişiyor, başlık ve örnekler eski kalıyordu. Tek
giriş noktası olunca hata da tek yerde.

---

## K-016 · Ses, var olan işi kısaltır; yeni yetenek açmaz (03.09.2026)

**Karar:** Sesli işlemin yaptığı her şey **elle de yapılabilir.** Ses
yeni bir yol açmaz, var olanı kısaltır — yedi işin dışına çıkmaz.

**Dört hâl ve üçüncüsü:**
`dinliyor → duyuldu → NE YAPILACAĞI ANLAŞILDI → onay`
Üçüncü adım olmadan ses, bir arama kutusundan farksız kalır. Asıl iş
tanıma değil, **tanımadan sonrası**.

**⚠️ ONAYSIZ HİÇBİR ŞEY YAPILMAZ.** Yanlış duyulan bir cümlenin kaydı
kendiliğinden açması, ustayı sesten **tamamen** soğutur: bir kez yanlış
kayıt açan mikrofona ikinci kez basılmaz. Anlaşılan işlem yazıyla
gösterilir, usta onaylar.

---

## K-015 · Ses tanıma: tarayıcının kendi API'si (03.09.2026)

**Cemil:** *"Buna apiyi bağlayabilir miyiz? Ücretsiz bir api. Nasıl
çalışıyor, test etmiş oluruz."*

**Karar:** **Web Speech API** (`SpeechRecognition` /
`webkitSpeechRecognition`) kullanılıyor. Tarayıcının kendi motoru.

| | |
|---|---|
| Ücret | **Yok** |
| Anahtar / hesap | **Yok** — kayıt olmak gerekmiyor |
| Kurulum | **Yok** — `new webkitSpeechRecognition()`, `lang = 'tr-TR'` |
| Nerede var | Chrome, Edge · **Firefox'ta yok** |
| Şart | **Güvenli bağlam** (https ya da localhost) + mikrofon izni |

**⚠️ `file://` ile açılan dosyada çalışmaz.** Prototip şu an dosyadan
açıldığı için mikrofon açılmıyor; ekran bunu **yazıyor** ve örnek
cümleler her zaman duruyor. `app/` bir https adresine konursa mikrofon
çalışır.

**⚠️ GİZLİLİK — bu bir karardır, ayrıntı değil:** Chrome bu API'de sesi
**Google'ın sunucusuna gönderir.** Cihazda kalmaz. Tamirhanede
konuşulanlar (müşteri adı, telefon, plaka) bu yoldan dışarı çıkar.
Ücretsiz olmasının bedeli bu.

**Cemil onayına açık (A-S3):** kabul mü, yoksa cihazda çalışan bir
motor mu aranmalı? Alternatifler ücretli (Google STT, Azure) ya da
ağır (cihaz üstü Whisper). Öneri: **prototipte kabul**, gerçek
kullanıma geçerken yeniden konuşulur.

---

## K-014 · Gider girişi sesle mümkün; kâr yine hesaplanmıyor (03.09.2026)

**Karar:** Muhasebeye **gider girişi eklendi** — ama yalnız sesle ve
elle giriş; **kâr ve cari hesap hâlâ hesaplanmıyor.**

**K-013'ün itirazı neydi:** *"Gider için ustanın her gideri girmesi
gerekir; girilmezse rakam yalan söyler."*

**Neden zayıfladı:** Cemil *"x TL elektrik faturası ekle gibi
örnekler"* isteyince giriş maliyeti düştü. **Söylemek form doldurmak
değil.** Mikrofona basıp *"2400 TL elektrik faturası ekle"* demek,
ustanın zaten yaptığı bir şeyin yanına eklenebilir.

**Neden tamamen kalkmadı:** girilen gider görünür, **girilmeyen
görünmez.** Eksik giderle çıkan kâr, kâr olmamasından kötüdür — çünkü
usta ona *inanır*. Ekran bu yüzden giderlerin **"girildiği kadar
doğru"** olduğunu yazıyor.

---

## K-013 · Muhasebe, yeni alan istemeden başlar (03.09.2026)

**Karar:** Muhasebe ekranı yalnız **zaten kayıtlı olan** veriyi okur:
teslimdeki `PaymentMethod` ve satırlardaki `Price`. Gider, kâr, cari
hesap ve fatura **yazılmadı** — kapsam kararı verilene kadar da
yazılmayacak.

**Neden:** Bu dördü ustadan **yeni veri** ister. *"Her gideri gir"*
diyen bir ekran, girilmediğinde **yalan söyleyen** bir ekrandır. Yanlış
rakam gösteren muhasebe, muhasebe olmamasından kötüdür.

**Bedavaya gelen üç rakam:** bugün tahsil edilen · ödeme yoluna göre
dağılım · **veresiye listesi.** *"Almadım"* bir ödeme yolu olduğu için
(K-004 teslim akışı) alacak zaten kayıtlı. Liste olarak veriliyor,
tek rakam olarak değil: ustanın sorusu *"ne kadar alacağım"* değil,
**"kimden alacağım"**.

**Ekran kendi yarım olduğunu yazıyor.** Bir ekranın eksiğini söylemesi,
saklamasından iyidir.

**Cemil onayına açık:** kapsam. *"Sonra geçeceğiz"* dedin — seçenekler
günlük kasa · kasa+gider · cari hesap olarak duruyor.

---

## K-012 · Müşteri ekranı var, müşteri tablosu yok (03.09.2026)

**Karar:** *"Müşteriler"* ekranı açıldı ama `Customer` tablosu
**açılmadı**. Liste araçtan türetiliyor: aynı `sahipAd` + `sahipTel`
tek satırda toplanıyor, araçları altında sıralanıyor.

**Neden:** v1'de müşteri dünyası **üç tabloydu** (`Party`, `Contact`,
`VehicleContact`) ve tamircinin *"karmaşık"* dediği yığının içindeydi.
İhtiyaç duyulan şey **ekrandı** — *"Ahmet kimdi, hangi arabaydı"* —
tablo değil. Ekranı tablosuz vermek mümkün olduğu sürece tablo
açılmaz.

**Bedeli açık ve kabul edildi (A-M1):** aynı kişinin adı iki araçta
farklı yazılmışsa **iki müşteri görünür.** Birleştirme yok. Tamirci
bundan şikâyet ederse tablo o zaman konuşulur — şikâyet etmeden değil.

---

## K-011 · Ana ekran menüdür; her satırın kendi mikrofonu vardır (03.09.2026)

**Karar:** Ana ekran, *"içerideki araçlar"* listesi olmaktan çıkıp
**menü** oldu: en üstte Sesli işlem, altında altı satır (araç kaydı ·
araçlar · servis kaydı ekle · servis kayıtları · müşteriler ·
muhasebe), **her satırın sağ ucunda mikrofon.**

**Cemil:** *"Yine anasayfada buttonlar olmalı… Yine buttonların yanında
mikrofon buttonu olacak."*

**Mikrofon neden her satırda:** iş hem dokunarak hem söyleyerek
başlatılabilmeli. Mikrofonu tek bir ekrana hapsetmek, sesle başlamak
isteyen ustayı önce **gözle aramaya** zorluyordu — sesin bütün kazancı
orada gidiyor.

**Kaybolan bir şey yok:** içerideki araçlar listesi kendi ekranına
geçti ve orada *"içeride"* süzgeci **açık** geliyor. Bakım hatırlatma
şeridi ana ekranda **kaldı** — ustanın *"bugün kimi arayacağım"*
sorusu ilk ekranda cevaplanmalı (T-17).

**Ölçüldü:** ana yol yine **13 dokunuş.** Menü satırı, eski
*"Ruhsatı çek"* düğmesinin yerini aldı; bir tık eklemedi.

**T-18 (*"iç içe menü yok"*) ne oldu:** menü **tek katman**. Menüden
açılan hiçbir ekranın kendi menüsü yok; hepsi liste ya da tek iş.
Yasak olan iç içelik, menünün kendisi değil.

---

## K-010 · Tema üç hâllidir; "sistem" gerçek bir seçenektir (03.09.2026)

**Karar:** Tema üç hâl arasında döner: **sistem · açık · koyu.** Seçim
yapılmamışken kökte `data-theme` **bulunmaz** ve `prefers-color-scheme`
devreye girer. Seçim `localStorage`'da tutulur; erişilemediği yerde
(gizli sekme, `data:` URL) tema yine çalışır.

**Neden:** Kökte sabit `data-theme="light"` yazılıydı. Telefonunu akşam
koyuya alan usta, uygulamanın da geçmesini bekler; sabit değer onun
tercihini **sessizce eziyordu.** İki hâlli bir anahtar bu tercihi
ifade edemez — "sistem"i atmak, kullanıcının kararını atmaktır.

**Düğme şu anki hâli söyler**, bir sonrakini değil: v1'de *"Koyu tema"*
yazan düğme koyu temadayken de aynı şeyi yazıyordu ve hangi hâlde
olunduğu okunamıyordu.

---

## K-009 · Çekmece geri geldi (03.09.2026)

**Karar:** `TASARIM-SISTEMI.md` v2.0'daki *"çekmece yok"* kararı geri
alındı. Header v1'in düzenine döndü: menü (rozetli) · logo · ara ·
geri · ileri.

**Cemil:** *"Header aynı şekilde tutmamız gerekiyordu. Tema konusu da
devam edecek."*

**Kararın hatası:** çekmeceyi kaldırırken **içindekilerin nereye
gideceği** sorulmamıştı. Tema anahtarının evi çekmeceydi. Bir kabı
kaldırırken içindekiler de kalkmaz — kalkarsa sessizce kaybolurlar,
ki tam bu oldu.

**T-18 (*"iç içe menü yok"*) nasıl korunuyor:** ana yolun **hiçbir
adımı çekmeceden geçmiyor.** Ölçüldü — çekmece hiç açılmadan tam yol
yine 13 dokunuş. Çekmece bir **kestirme**, bir yol değil.

**Çekmecenin içi v2'nin:** v1'in on yedi maddesinden üçü kaldı (araç
girişi, içerideki araçlar, plaka ara) + bakım periyodu + tema.
Bildirimler uydurma değil, vadesi gelmiş bakım hatırlatmaları.

**Cemil onayına açık:** rozet yalnız bakım hatırlatmalarını sayıyor.
İleride başka bildirim türü girerse rozet ne sayacak?

---

## K-008 · Tasarım sistemi bağlayıcı bir sınıf sözlüğüdür (03.09.2026)

**Karar:** Bir ekran, `TASARIM-SISTEMI.md`'nin sonundaki sınıf sözlüğünde
olmayan bir bileşen sınıfı **açamaz**. Yeni bir görsel ihtiyaç varsa önce
sözlüğe girer, sonra kullanılır.

**Neden:** Tasarım sistemini v1'den **belge olarak** taşımıştık ve belge
doğruydu. Yine de altı ekranı yazarken bileşenleri sıfırdan uydurdum
(`.ana-btn`, `.kart`, `.plaka`, `.ekran-bas`, `.tutar-buyuk`). Prototip
yabancı çıktı. Cemil: *"Eski tasarımla hiç ilgisi yok. Tasarım eskisi gibi
devam etmeli. Yazdık bunları."*

*"Kalem listesi kullanılır"* yazması, ekranın `.kalem .k-tip .k-ad`
yazacağını garanti etmiyor. Bağlayıcı olan **sınıf adının kendisi.**

**Bekçisi denetim:** `TANIMSIZ-SINIF` sözlükte olmayanı, `OLU-SINIF`
sözlükte kalıp kullanılmayanı yakalar. Bu iş sırasında ikisi birlikte
**60 bulgu** çıkardı; içlerinde taşındığı sanılan ama taşınmamış dokuz v1
kuralı vardı (`.btn-primary`, `.btn-ghost`, `.btn-lg`, `.plate-sm`,
`.plate-lg`, `.arac-bas`, `.veh-title`, `.spec-strip`, `.k-katalog`) —
ana düğmeler **zeminsiz**, liste plakaları **büyük** çiziliyordu.

---

## K-007 · Kol B'de cevabın kaynağı satırda tutulur (03.09.2026)

**Karar:** Birden fazla parçacıya sorulduğunda hangi cevabın kullanıldığı
**satırın kendisinde** tutulur (`ServiceLine.SelectedReplyId`). Ayrı bir
fiyat karşılaştırma ekranı **açılmaz**; çipler yalnız birden fazla
parçacıya sorulmuşsa görünür.

**Neden:** v1'de bu bir **ekran ailesiydi** (karşılaştırma tablosu, en
uygun seçili, teklif detayı) ve tamirci *"karmaşık"* dediği yığının
içindeydi. Ama bilgi gerçek: `PriceRequest` modelde zaten her parçacı için
ayrı kayıt. Bilgiyi tutup ekranı açmamak mümkün — çünkü soru *"hangisi
daha ucuz"* değil, *"bu satırın fiyatı kimden geldi"*.

**Ana yola maliyeti sıfır:** tek parçacıya sorulduğunda çip çıkmıyor,
ekran ana yoldaki hâlinden farksız.

**Cemil onayına açık:** usta hiçbirini seçmezse alan boş kalıyor ve akış
duruyor mu? Öneri: **durmaz** — rakam satırda, kaynağı bilinmese de iş
yürür.

---

## K-006 · Tamirat ve bakım iki ayrı türdür (03.09.2026)

**Karar:** Servis kaydının bir **türü** vardır: `tamirat` | `bakim`. Kayıt açılırken seçilir.

**Gerekçe (T-15):** Tamirci açıkça *"tamirat ve bakım iki ayrı durum var"* dedi. İkisi farklı davranıyor:

| | Tamirat | Bakım |
|---|---|---|
| Neden geliyor | Bir şey bozuldu | Zamanı geldi |
| km | Bilgi | **Anlamlı** — bir sonrakinin dayanağı (K-002) |
| Tekrarı | Belirsiz | **Öngörülebilir** → hatırlatma (K-002) |
| Parça | Değişken | Çoğunlukla aynı küme (yağ, filtreler) |

**Kapsam dışı:** Alt türler (periyodik/ara bakım, ağır/hafif tamirat) yok. İki değer, o kadar.

**⚠️ Bu, v1'in K-029 kararının TERSİ.** Orada *"baştan tür seçilmez, durum akışta belirir"* denmişti. O karar bizim çıkarımımızdı; bu, kullanıcının sözü. Kullanıcı kazanır.

---

## K-005 · Parçayı kim getirdi — sorumluluk anahtarı (03.09.2026)

**Karar:** Her parça satırında **`kimGetirdi`** alanı: `tamirci` | `musteri`. Kayıtta ve araç geçmişinde **görünür** durur.

**Gerekçe (T-7, T-8):** Bu bir bilgi alanı değil, **ileride kimin ödeyeceğini belirleyen anahtar**:

- **Tamirci aldıysa** → parça sorun çıkarınca **ücretsiz** değiştirilir (parçanın sorumluluğu onda)
- **Müşteri getirdiyse** → **tekrar işçilik alınır** (parçayı seçen o)

Araç ikinci kez geldiğinde bu bilginin **karşına çıkması** gerekir; yoksa tartışma çıkar.

**Ek (S2):** Müşteri parçasının satırı **fiyatsız ya da 0 TL** durur. Ayrı bir mekanizma kurulmaz — sıradan bir satır, tutarı yok.

**Kapsam dışı — GARANTİ SÜRESİ TAKİBİ YOK (S3).** Cemil: *"Garanti süresi yok."* Ne bitiş tarihi, ne kalan gün, ne uyarı. v1'deki garanti sekmesi ve süre hesabı tamamen düşer. Tutulan tek şey: **kim getirdi.**

---

## K-004 · Tek durum: teslim edildi mi (03.09.2026)

**Karar:** Servis kaydının **tek durum alanı** vardır: **teslim edildi / edilmedi.**

**Gerekçe (T-6, S1):** Cemil: *"Sadece teslim durum olarak kalacak."*

v1'de yedi durum vardı — *fiyat bekliyor · parça bekliyor · onay bekliyor · işlemde · hazır · red · kapandı.* Her biri ekran, filtre, rozet ve geçiş kuralı üretiyordu. Tamircinin gerçekte sorduğu tek soru: **"bu araç çıktı mı, hâlâ bende mi?"**

**Sonuç:** Durum makinesi diye bir şey kalmıyor. Açık iş listesi = teslim edilmemiş kayıtlar.

**Kapsam dışı:** Durum filtreleri, durum rozetleri, duruma göre hızlı eylemler, durum geçiş kuralları.

**Cemil onayına açık:** *"Parça bekliyor"* bilgisi tamircinin işine yarıyor olabilir (bugün ne yapabileceğini söyler). Ama bu bir **durum** değil, kaydın kendi verisinden **türetilebilir** ("fiyatı sorulmuş ama parçası gelmemiş"). Önerim: durum olarak tutulmasın, listede gösterilsin. Akış çizilirken (3. iş) bakılacak.

---

## K-003 · Onay bir akış değil, isteğe bağlı bir eylemdir (03.09.2026)

**Karar:** Müşteri onayı **zorunlu adım değildir.** Varsayılan: usta işi ekler ve devam eder. İsteyen **"onay iste"** düğmesine basar; kayda o zaman bir onay isteği düşer.

**Gerekçe (T-10, S4):** Cemil, tamircinin anlattığını aktarıyor:

> *"Ruhsatı çekti, parçayı aldı, lifte kaldırdı. Bu sırada balataları gördü ve dedi ki balatalar nanay. Müşteri tamam derse, değişiyor. Burada müşteri onayı vb işin içine katmayalım."*

Onay **zaten alınıyor** — sözlü, yüz yüze, telefonda. Sisteme onay adımı koymak var olan bir şeyi ikinci kez yaptırmak olur.

> *"Onay iste diye ayrıca bir alan olabilir. O zaman müşteri onayı gerekebilir."*

Yani onay bir **eylem**, bir **kol** değil.

**Bizi ilgilendiren tek şey (S4):** *"Yapılan işe ek iş yapılabilir teslim etmeden önce."* Kayıt teslim edilene kadar **açıktır ve satır eklenebilir.** Hepsi bu.

**Kapsam dışı — v1'in bütün onay kolu:** onay ekranı, "müşteriye gönder · onay iste" akışı, red paneli, red sebebi çipleri, indirim teklifi, "yeniden gönder", "yaptırmıyor" çıkışı, `onay`/`red` durumları. v1'in ekranlarının yaklaşık dörtte biri buydu.

---

## K-002 · Bakım takibi: km kaydedilir, hatırlatma ay üzerinden (03.09.2026)

**Karar:**
- **km** bakım kaydında **bir kez yazılır** — araç geldiğinde usta göstergeden okur. Sürekli takip edilmez.
- **Hatırlatma** kayıt tarihinden **ay** sayılarak kurulur.

**Gerekçe (T-16, T-16b, S5):** Görünürde çelişki vardı:

- Tamirci: *"Bakımda km önemli."*
- Cemil: *"Km'yi takip edemeyiz sanırım, ay olarak takip edilebilir."*

Çelişki değil, **iki ayrı iş.** Km'yi *sürekli izlemek* imkânsız — araç dükkânda değil, kimse bize km göndermiyor. Ama *o anki km'yi yazmak* bedavadır ve geçmişte değerlidir: *"geçen bakım 142.000'de yapılmış"* ustanın işine yarar. **Hatırlatmanın dayanağı ise zamandır**, çünkü elimizde ölçebildiğimiz tek şey o.

**Cemil onayına açık:** Varsayılan periyot kaç ay? Öneri: **6 ay**, dükkân ayarından değiştirilebilir, kayıtta da elle değiştirilebilir.

**~~Kapsam dışı: km tahmini, km-bazlı bildirim.~~**

---

### ⚠️ 04.09.2026 — GÜNCELLENDİ: ay **veya** km, hangisi önce gelirse

Cemil sordu (*"Bakım zamanı hatırlatması neye göre olacak?"*) ve
**ay + km, hangisi önce gelirse** seçeneğini seçti. Yukarıdaki
*"hatırlatmanın dayanağı zamandır"* cümlesi artık **yarım doğru**.

**Yeni kural:**

| | |
|---|---|
| Ay hedefi | Teslim + `bakimPeriyoduAy` (varsayılan 6) |
| Km hedefi | Son bakımın km'si + `bakimPeriyoduKm` (varsayılan 10.000) |
| Hangisi kurulur | **Önce geleni** |

**Km hedefi tarihe nasıl çevriliyor:** aracın **kendi geçmişinden.**
İki bakım arasındaki km farkı ve gün farkı, günlük ortalamayı veriyor;
10.000 km'nin kaç günde dolacağı buradan çıkıyor.

> **⚠️ TEK ÖLÇÜMLE TAHMİN YAPILMAZ.** Bir aracın tek km kaydı varsa
> günlük ortalama **uydurulmaz** ve ay tek başına çalışır. Uydurma bir
> ortalama, ustayı yanlış güne çağırır — hatırlatmanın tek işi doğru
> günde çalmaktır.

**Eski gerekçe neden çürüdü:** *"km'yi izleyemiyoruz"* doğruydu ama
**izlemek gerekmiyormuş.** Araç her gelişinde km zaten yazılıyor; iki
nokta bir eğim veriyor. İzleme değil, **iki ölçüm** yetiyor.

**Ölçüldü (04.09):** 34 ABC 123 · 15.03'te 142.500, 20.06'da 151.200 →
97 günde 8.700 km → günde **89,7 km** → 10.000 km **09.10.2026**'da
dolar; 6 ay ise 20.12.2026. **Km kazanıyor**, hatırlatma 09.10'a
kuruluyor.

**Kapsam dışı kalmaya devam ediyor:** araçtan otomatik km okuma,
müşteriden km sorma.

---

## K-001 · Parça uyumunu parçacı çözer; TecDoc ana yoldan çıkar (03.09.2026)

**Karar:** Uyumlu parçayı bulmak **bizim işimiz değil, yedek parçacının işidir.** Ona ruhsatı ve istenen parçayı göndeririz, o uygun parçayı bulup fiyatıyla döner. **TecDoc / `Fitment` verisi ana yolun ön koşulu değildir.**

**Gerekçe (T-12, T-13, T-14, S7):**

Tamircinin akışı zaten böyle işliyor: *"Yedek parçacıdan fiyat alırken, ruhsatı ve gereken parçaları gönderiyor."* Cemil: *"Yedek parçacı kolayca bu işi çözüyor."* Uyum bilgisi **parçacının kataloğunda zaten var** ve bunu günde yüz kere yapıyor.

**⚠️ Bu, v1'in en pahalı kalemini kapsam dışına çıkarır.** v1'de A-11.3 maddesi *"ertelenemez"* diye işaretliydi: *"şasi no ve marka/model ile uyumsuz parçalardan kaçınmak `Fitment` verisi olmadan yapılamaz."* Doğruydu — **eğer uyumu biz bulacaksak.** Bulmuyoruz.

**Ayrım (Cemil'in asıl istediği):**

| | Uyumu bulmak | Ne satıldığını bilmek |
|---|---|---|
| Kim için | Tamirci — işi yapabilsin | **Biz** — veri biriksin |
| Ne zaman | Fiyat sorulmadan **önce** | Cevap geldikten **sonra** |
| Yanılırsa | **İş durur, yanlış parça takılır** | Bir satır eksik kalır |
| Maliyeti | Eksiksiz lisanslı veri | Kullandıkça biriken kendi verimiz |

Cemil'in istediği **sağ sütun**: *"hangi parçanın satıldığını tespit etmek için araya bir işlem sokmaya çalışıyorum."* Bu **akışı tıkamaz** — 2. işte (`PARCA-TANIMLAMA.md`) detaylandırılıyor.

**Kapsam dışı (şimdilik):** TecDoc lisansı, `Fitment` tablosu, VIN'den araç tipi çözümleme (NHTSA vPIC), araca göre parça süzme. Devralınan araştırma notları (`arastirma/A-11-*`) **arşiv olarak duruyor** — ihtiyaç doğarsa oradan devam edilir.

**Cemil onayına açık:** *"Bazı parçacılar kod vermiyor, sadece fiyat veriyor."* Bu durumda satır **"tanımlanamadı"** olarak kalır ve akış durmaz. Kabul mü?

---

## AÇIK — karara bağlanacak

> **14.09'da tazelendi (Tur 5/5.2).** Üç şey yapıldı: ① kodda çoktan
> karşılığı olan maddeler **prototipte açılıp doğrulandı** — iddia
> edilmedi; ② aynı şeyi iki kez soran satırlar birleştirildi (A-2, A-M2
> ve A-M17 tabloda **ikişer kez** duruyordu); ③ her açık maddeye tek
> satır soru + net öneri yazıldı.
>
> **Hiçbir madde Cemil'in cevabı olmadan kapatılmadı.** Kapananlar
> aşağıda, arşiv bölümünde ve hepsinin yanında *neyle* kapandığı yazılı.
>
> **Üç blok, üç farklı cevap cinsi:**
> **A** kapsamı belirler (iş açar ya da kapatır) ·
> **B** ekranı değiştirir (evet dersen kod yazılır) ·
> **C** *"böyle kalsın mı"* onayıdır (hayır dersen geri alınır).
> C bloğunun tamamına tek cümleyle *"hepsi kalsın"* denebilir.

### A · Kapsamı belirleyenler — **14.09'da CEVAPLANDI**

> Cemil: *"Karar bekleyen bir şey varsa onu da bitirelim."* Bu cümleyi
> **önerilerin onayı** olarak okudum: her satırın önerisi yazılıydı ve
> hepsi uygulandı. ⚠️ İkisi kapsam kararı — **ters çevirmek tek
> cümlelik iş**, söylemen yeter (A-M25 ve MUH).

| Kod | Karar | Sonucu |
|---|---|---|
| **MUH** | **B — kasa + gider.** Cari hesap kapsam dışı | Bekleyen dört iş sıraya girdi: ay bazlı gider toplamı → dışa aktarma → alacak takibi → ticari ileti izni alanı |
| **A-M8** | **Karar bütçesi kısıt:** menü ≤ 8 başlık · ana yolda ekran başına ≤ 3 sabit karar · jargon 0 | Bu turda sınandı: ekran 16 oldu ama **menüye satır eklenmedi** |
| **A-M25** | **Toplu mesaj kalıyor** — izin altyapısı yazılana kadar gerçek gönderim yok | Onay ekranındaki risk uyarısı kalıcı |
| **A-M21** | **Ticari ileti izni alanı eklenecek** | Kuyruğa girdi (muhasebe sırasının 4. işi) |
| **A-M20** | **Sanal POS faz dışı** | Ödeme linki backend fazında yeniden konuşulur |
| **A-M22** | **PDF = tarayıcı yazdırması**, kütüphane yok | Dışa aktarma işi bu kararla yazılacak |
| **A-2** | **Müşteri uygulaması sonraya** — özet linki yeterli | T-21 faz dışı kalmaya devam |

### B · Ekranı değiştirenler — **14.09'da CEVAPLANDI**

| Kod | Karar | Durum |
|---|---|---|
| **A-M14** | Teslimde *"özet gönder / geç"* **sorulmaya devam** | Değişiklik yok |
| **A-M7** | İş başına kâr kayıt ekranında **gösterilmeyecek** | Değişiklik yok; `is-basina` sayfası duruyor |
| **A-M28** | Müşteri satırı **kısaltıldı** (plaka küçüldü, 190 → 130 px) | **Uygulandı** (K-048.3) |
| **A-M29** | Öteki araca mesaj için *"Tüm araçlar"* yolu **yeterli** | Değişiklik yok |
| **A-M16** | Ses örnek çipleri **kalıyor** | Ve artık veriden doluyor (K-048.1) |
| **A-M12** | Kaydırarak silmeye **ipucu eklenmeyecek** | Değişiklik yok |
| **A-3** | Bakım periyodu **6 ay** (km ile birlikte, hangisi önce gelirse) | Değişiklik yok |
| **A-14** | Ruhsat **ayrı ekran kalıyor** | Katmana taşımak "geri"yi değiştirirdi |

### C · "Böyle kalsın mı" — **14.09'da HEPSİ KALDI**

A-M1 · A-M2 · A-M3 · A-M5 · A-M6 · A-M9 · A-M10 · A-M11 · A-M18 ·
A-M19 · A-M23 · A-M24 · A-M26 — on üçü de bugünkü hâliyle onaylandı.
Gerekçeleri K-047'nin C tablosunda duruyor; biri geri alınacaksa
oradan bakılır.

### Şimdi cevap bekleyenler

> **12.10.2026 · on beş maddenin hepsi kapandı** (K-055). Kapananlar
> arşiv bölümünde, her birinin yanında *neyle* kapandığı yazılı.
> Aşağıda yalnız **yeni açılan** ve **bilinçli ertelenen** duruyor.

| Kod | Soru | Durum |
|---|---|---|
| **A-M48** | **Dükkân sesine erişmeden ses tarafı nasıl ölçülür?** | **ONAYLANDI (12.10) — E yolu; Cemil 40 cümleyi 13.10 sabahı okuyacak.** Hazırlık bitti: derlem + 6 CC0 atölye gürültüsü indi (`tools/degerlendirme/ses-kaynaklari.md`), koşucu derlem ve gürültü kiplerini kazandı, bozma hesabı ortak modülde (`ses-bozma.js`, bayt bayt eşdeğer). Sıra: `SES-OLCUM.md` §9. — İlk araştırma (`arastirma/SES-OLCUM.md`). Bulgu: Chrome 152'nin tanıyıcısı **dosyadan beslenebiliyor** (`start(track)`; mikrofon izni yokken dinlemeye başladı) → dükkân **kaydı** şart değil. E: konuşma evden (Cemil 40 cümleyi okur, ~20 dk) + gürültü açık arşivden (Freesound CC0 atölye kayıtları) + `ses-bozucu.js` + yeni `ses-dosya-kos.html` + aynı cetvel → SNR eğrisi. Onaylar: ① *"gürültü uydurulmaz"* kuralının esnetilmesi (gerçek atölye sesi, bizim dükkânın değil) ② kendi sesin ③ indirme (MediaSpeech TR ~618 MB CC BY 4.0 + CC0 gürültü). Ölçülemeyen kalır: dükkân akustiği · ustanın ağzı · gerçek telefon/bağlantı → sunumda risk olarak yazılır |
| ~~A-M49~~ | **Cemil, 12.10:** *"Zaman çizelgesinin Ne oldu alanı olsun. Eylem merkezli olanın altta buttonlarının olması olacak… ses ve kamera buttonları da gerekli. Teslim alanı burada olmayabilir."* → **K-058** (önerim B'ydi; birleşim seçildi) |
| **A-M36** | TC/vergi no ve adres tutuluyorsa **KVKK aydınlatma + saklama süresi** (K-050.5) | **ERTELENDİ** — Cemil: *"Uygulama kullanıma başladığında karar veririz."* Aydınlatma A-M33 ile çözüldü; **saklama süresi gerçek veri tutulmadan önce kapanmalı** |

### Sonraki fazın soruları — şimdi cevap gerekmiyor

| Kod | Soru | Ne zaman |
|---|---|---|
| **A-5** | Ruhsat okuma hangi teknolojiyle — OCR servisi, doğruluk, elle düzeltme | 9. iş |
| **A-6** | WhatsApp API sağlayıcısı ve maliyeti | 8. iş |
| **A-7** | SMS sağlayıcısı + **İYS kaydı** (yasal zorunluluk) | 10. iş |
| **Ç-2** | OEM kodu yönü — TecDoc'un kaderi | K-001'de kaldığı gibi |
| **A-S3** | Ses gizliliği: Chrome sesi Google'a gönderiyor (K-015) | Prototipte kabul; gerçek kullanımda konuşulur |

### Kapandı — arşiv

| Kod | Nasıl kapandı |
|---|---|
| ~~A-M31~~ | **Cemil, 12.10:** *"Backend'e yolumuz var henüz."* → bu fazda **açılmıyor** (K-055.1) |
| ~~A-M43~~ | **Cemil, 12.10:** *"Alış fiyatı girilmeyebilir… ayar sayfamız olsun."* → alan **zorunlu değil**, görünürlüğü ayara bağlı; `alis: null` ≠ `alis: 0` (K-055.2) |
| ~~A-M44~~ | **Cemil, 12.10:** *"Fatura girişi bu fazda değil. Muhasebede ele alacağız."* |
| ~~A-M45~~ | **Cemil, 12.10:** *"Sonraki faz, genel değerlendirme."* |
| ~~A-M37~~ | **Cemil, 12.10:** *"Hayır, sonraki faz."* → K-022'nin sınırı duruyor |
| ~~A-M34~~ | **Cemil, 12.10:** *"Ben"* → ölçütü Cemil doğrulayacak (14 belge + 42 ditto satırı) |
| ~~A-M35~~ | **Cemil, 12.10:** *"Evet"* → A-M34'ten sonra koşulacak |
| ~~A-M30~~ | **Cemil, 12.10:** *"Ölçüp seçelim."* → karar rakamla; ürüne koyma backend turunda |
| ~~A-M39~~ | **Biçim değiştirdi (12.10):** erişim yok → **A-M48** olarak yeniden açıldı (K-055.5) |
| ~~A-M40~~ | **Cemil, 12.10:** amacı yazdı — *"kullanmayanları/az kullananları tespit, sorun-hata yakalamak"* → özetin içeriği bu iki soru (K-055.6) |
| ~~A-M42~~ | **Cemil, 12.10:** önerildiği gibi — tek sayfalık özet, bizim tarafta |
| ~~A-M47~~ | **Cemil, 12.10:** önerildiği gibi — ayrı ve baştan |
| ~~A-M46~~ | **Cemil, 12.10:** *"Sonraki faz."* → parçacı ağı bilinçli ertelendi (K-055.1) |
| ~~A-M33~~ | **Cemil, 12.10:** *"Evet"* → ayarlarda tek satır; ayar sayfası A-M43 ile açıldı |
| ~~A-M38~~ | **Cemil, 12.10:** *"İhtiyaç olursa elle…"* → ekran **zorlamaz**, çekimde yalnız bilgi (K-055.7) |
| ~~A-1~~ | **A-M8'e bağlandı (14.09)** — "ekran bütçesi 6" fiilen aşıldı; asıl kısıt ekran sayısı değil, ekrandaki karar sayısı |
| ~~A-4~~ | **K-033.5** — türetilmiyor, kendi bayrağı var (`parcaBekliyor`); türetilen `fiyatiBekleyen` başka bir şey: *fiyatı gelmemiş* |
| ~~A-M4~~ | **K-020.3** — ekran görüntüsü gösterdi ki yapışkan başlık bildirimi örtüyordu; yapışkanlık kaldırıldı |
| ~~A-M13~~ | **Cemil, 07.09:** *"Evet, olabilir."* — satır başına *"kim getirdi"* işareti geri geldi |
| ~~A-M15~~ | **Cemil, 07.09:** *"İsterse, evet."* — parça beklendiğini müşteriye söylemek ustanın seçimi |
| ~~A-M17~~ | **K-047.3** — her şablona koşul yazılınca *"Aracınız hazır"* teslim edilmiş araçtan eledi |
| ~~A-M27~~ | **Cemil, 14.09:** *"a, b ve c'yi sırayla yapalım"* — c = bu madde. Yanlış duyulan plakada *"bunu mu demek istedin"* önerisi **yazılacak** (Tur 5/5.3) |
