# TAMIRON — DEVİR (HANDOFF.md)

> **Son güncelleme:** 06.09.2026 *(K-033)*
> Bu belge *"yarın buraya oturan kişi ne bilmeli"* sorusunu cevaplar.
> Belge listesi değil, **durum** belgesidir.

---

## 1 · Nerede duruyoruz

**Faz:** tasarım / iş akışı. **Backend yok.** Üretilen tek çalışan şey
tıklanabilir prototip.

| | |
|---|---|
| Ekran | **14** — home · ses · ruhsat · kayit · parca-sor · teslim · arac · araclar · kayitlar · mesaj · musteriler · muhasebe · **is-basina** + **ozet** *(müşterinin gördüğü sayfa, ustanın ekranı değil — K-023)* |
| Ana ekran | **Menü** — altı satır, her satırın sağında mikrofon. **Bakım hatırlatma şeridi yok** (K-019.1), **"Sesli işlem" satırı yok** (K-026) |
| Dokunuş | ana yol **12** · kısa yol **9** · **kayıtlı araç 8** · sesli yol **2 dokunuş + 1 konuşma** · müşteriye özet **2** — *hepsi 05.09'da yürütülerek ölçüldü* |
| Kabuk | header (menü+rozet · logo · geri · ileri) + çekmece + tema (3 hâl) |
| Sağlık denetimi | **36 test · 0 bulgu** · **320 · 375 · 430 px**'te ölçüldü (K-025) · denetim **ölçtüğü durumu geri yüklüyor** (K-024.1) ve sekmelere dokunmuyor (K-026.7) |
| Derleme denetimi | **8** kontrol, bulursa `exit 1` — 06.09'da üçü eklendi: **yorum içinde etiket** · **üretilen betik ayrışıyor mu** · **kapanmamış yorum** (K-033.11) |
| Kararlar | **K-001 … K-033** |
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
  betik/ 01-cekirdek 02-veri 03-nesne.js
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

---

## 5 · Cemil'in kararını bekleyenler

| Kod | Soru | Öneri |
|---|---|---|
| **A-2** | Müşteri tarafı bu fazda mı | **Özet linki YAZILDI (K-023).** Müşteri uygulaması hâlâ sonraya |
| **A-3** | Bakım periyodu · şu an 6 ay + 10.000 km | Onay |
| **A-14** | Ruhsat ekranı tabaka mı olsun | Ölçüldü: bir dokunuş ve bir ekran kazandırır |
| **Ç-2** | OEM kodu yönü — TecDoc'un kaderi | K-001'de kaldığı gibi |
| **A-S3** | **Ses gizliliği:** Chrome sesi Google'a gönderiyor | Prototipte kabul, gerçek kullanımda yeniden konuşulur |
| **MUH** | **Muhasebe kapsamı** — nereye kadar | **K-022 taslağı hazır:** kasa+gider. Cari hesap kapsam dışı. Ölçüm: `arastirma/MUHASEBE-KAPSAMI.md` |
| **A-M1** | Müşteri adı iki araçta farklı yazılırsa iki müşteri görünüyor | Tamirci şikâyet edene kadar dokunma |
| **A-M2** | Özet/hatırlatma hangi numaraya gitsin — ruhsattaki mi, ikinci iletişim mi | İkinci iletişim varsa o |
| **A-M3** | Araç geçmişinde *"parçayı müşteri getirmişti"* uyarısı gerçekten gereksiz mi | Kaydın kendi ekranında kalsın |
| ~~A-M4~~ | **KAPANDI (K-020.3)** — ekran görüntüsü gösterdi ki yapışkan başlık bildirimi örtüyordu; kaldırıldı | — |
| **A-M5** | Arama mikrofonu yalnız **araçlar** için istenmişti; **dört listeye birden** kondu. Kalsın mı? | Kalsın — dördü de aynı iş |
| **A-M6** | *"Onay iste"* düğmesi **"Özet gönder"** oldu (artık gerçekten gönderiyor) — ad doğru mu? | Doğru: yaptığı iş T-19 |
| **A-M7** | **İş başına kâr** (`fiyat − alış`) kayıt ekranında gösterilsin mi? Önerildi, **yazılmadı** | Dükkân kârıyla karışma riski var |
| **A-M8** | **Karar bütçesi** kısıt olarak kabul mü (menü ≤8 · ekran başına karar ≤3 · jargon 0) | Kabul — iki rakip incelemesi de istedi |
| **A-M9** | Özet sayfasında **kimlik doğrulama yok**, link tahmin edilebilir (plaka + kayıt no) | Prototipte kabul, backend fazında kapatılacak |

> **A-1 (ekran bütçesi 6) kapandı:** Cemil menüyü ve dört yeni ekranı
> isteyince bütçe cevaplanmış oldu. Ölçülen asıl sayı değişmedi.

**Yapılmamış işler:** ~~müşteri özeti~~ **(K-023'te yazıldı)** · müşteri uygulaması (T-21) · gerçek ruhsat
okuma (`RUHSAT-OKUMA.md` C yolu) · gerçek WhatsApp gönderimi
(`WHATSAPP.md` A yolu, kurulum gerektirmiyor) · SMS + İYS kaydı ·
backend.

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
