# TAMIRON — EKRANLAR

> **v1 · 03.09.2026** · **Akıştan türetildi** (`IS-AKISI.md`), uydurulmadı.
> **Bütçe: ana yol en fazla 6 ekran** (A-1). Bu liste **6.**
>
> Her ekranda: hangi işe hizmet ediyor · üzerinde ne var · **üzerinde ne YOK.**
> "Ne yok" sütunu bilerek var: v1'de ekranlar bir şey eklemek kolay olduğu için şişti.

---

## Türetme — hangi adım hangi ekranda

| Akış adımı | Ekran |
|---|---|
| 1 · Plaka yazıldı / ruhsat çekildi, kayıt açıldı | **2 · Servis kaydı ekle** |
| 2 · Ne istiyor | **3 · Servis kaydı** |
| 3 · Parçacıya sor | **4 · Parça fiyatı sor** |
| 4 · Fiyat geldi | **3 · Servis kaydı** |
| 5 · Parça geldi, kim getirdi | **3 · Servis kaydı** |
| 6 · İş yapıldı | **3 · Servis kaydı** |
| 7 · Teslim + ödeme | **5 · Teslim et** |

**Yedi adım, beş ekran.** Dördü (2, 4, 5, 6) aynı ekranda toplanıyor — **servis kaydı bu ürünün kalbi.**

Buna giriş noktası (menü) ve akış dışı başvuru ekranı (**6 · Araç**) eklenince **6** ediyor.

---

## 0 · ANA MENÜ *(ana ekran — 03.09'da değişti)*

**Cemil:** *"Yine anasayfada buttonlar olmalı. En üstte Sesli İşlem
buttonu, Araç kaydı, araçlar, servis kaydı ekle, servis kayıtları,
müşteriler, muhasebe olacak. Yine buttonların yanında mikrofon buttonu
olacak."*

| Üzerinde ne var | Üzerinde ne YOK |
|---|---|
| Saate göre selamlama | Özet paneli, gösterge tablosu |
| **Sesli işlem** — en üstte, tek düğme | Grafik, rakam yığını |
| ~~Bakım hatırlatma şeridi~~ **04.09'da kalktı (K-019.1)** | **Bakım hatırlatması** — yalnız çekmecedeki Bildirimler + menü rozeti |
| Altı menü satırı, **her birinin sağ ucunda mikrofon** | İç içe menü, alt menü |
| Satır altlarında sayı: *"5 araç kayıtlı"*, *"4 araç içeride"* | Arama kutusu *(listelerin kendi içinde)* |

**Her satırın sağında mikrofon var** çünkü iş hem dokunarak hem
söyleyerek başlatılabilmeli. Mikrofonu tek bir ekrana hapsetmek, sesle
başlamak isteyen ustayı önce **gözle aramaya** zorluyordu.

> **İçerideki araçlar listesi kaybolmadı**, Ekran 8'e (Servis kayıtları)
> taşındı ve orada *"içeride"* süzgeci **açık** gelir.

---

## 1 · ~~İÇERİDEKİ ARAÇLAR (ana ekran)~~ → **Ekran 8'e taşındı**

**Hangi işe:** giriş noktası. Ustanın sabah sorduğu tek soru: *"hangi araçlar bende?"*

| Üzerinde ne var | Üzerinde ne YOK |
|---|---|
| Teslim edilmemiş kayıtların listesi — plaka, araç, ne yapılıyor | Durum filtreleri (tek durum var — K-004) |
| Kocaman **RUHSATI ÇEK** düğmesi | Özet paneli, istatistik, grafik |
| Arama (plaka) | Alt menü, sekme |
| Bakım hatırlatması gelenler *(varsa, kısa bir şerit)* | Takvim, randevu |

> **Liste = teslim edilmemiş kayıtlar.** Başka bir tanım yok. Kayıt teslim edilince listeden düşer.

**Cemil onayına açık (A-4):** *"parçası bekleniyor"* kayıtlar satırda işaretlensin mi? Durum değil, kaydın verisinden türetilir. Öneri: **evet, küçük bir not olarak** — ustaya bugün ne yapabileceğini söyler.

---

## 2 · SERVİS KAYDI EKLE *(kayıt açma)* — 06.09'da yeniden kuruldu

**Hangi işe:** *"aracın bilgilerini girmek"* — tamircinin birinci isteği

**İKİ SEKME** (Cemil, 06.09): *"Plakayı yaz"* (varsayılan, mikrofonlu)
ve *"Ruhsatı çek"* (kamera + galeri). **Plaka kapının kendisidir**
(K-026): yazınca aracın kayıtlı olup olmadığı anlaşılır.

| Üzerinde ne var | Üzerinde ne YOK |
|---|---|
| **Plaka alanı** — mikrofonlu, örnek cümlelerle | Uzun araç formu |
| **Kamerayı aç** · **Galeriden seç** | Müşteri kaydı adımı |
| Okunanlar: plaka · marka · model · yıl · motor · **renk · yakıt** — düzeltilebilir | Sigorta/muayene tarihleri |
| Akordiyon: şasi · motor no · hacim · güç · koltuk · depo · ilk tescil | **TC kimlik no · adres** *(ruhsatta var, tutulmuyor)* |
| **Kilometre** — her zaman görünür, isteğe bağlı | Tür seçimi *(kalktı — K-033.1)* |
| İki düğme: **Ruhsat adımını geç ve kaydı aç** · **Ruhsatı çek** | Kapalı düğme *(hiçbiri disabled değil — K-033.3)* |

> **Tür artık sorulmuyor.** Cemil: *"Tamir, bakım vb çok önemli değil."*
> İş metninden tahmin ediliyor ve **servis kaydında düzeltilebiliyor**
> (K-033.1). Km bu yüzden türe bağlı değil: her zaman görünür, isteğe
> bağlı.
>
> Araç kayıtlıysa **ruhsat istenmez** — açık kaydı varsa oraya götürür.
> Aynı araç iki kez yaratılmaz.

**Birleştirme seçeneği:** Bu ekran, ana ekranın üzerine açılan bir **tabaka** da olabilir — o zaman ana yol 4 ekrana iner. Prototipte deneyip karar veririz.

---

## 3 · SERVİS KAYDI *(kalp)*

**Hangi işe:** *"yapılan işlemleri belirtmek"* + *"fiyatı belirtmek"* — ikinci ve üçüncü istek

Akışın dört adımı burada (2, 4, 5, 6). **Kayıt teslim edilene kadar açık ve düzenlenebilir** (K-003).

> **12–13.10 — K-058: adım şeridi kalktı.** Tek sayfa: künye → Yapılanlar
> (+ toplam) → **Ne oldu** (süreç, `surecHtml`; son adımı **"Teslim
> bekliyor" teslimin kapısı**) → Açıklama (+ iş türü) → Parça bekliyor.
> Altta **yapışkan eylem çubuğu**: Ekle (**panel**) · Sesle · Fotoğraf ·
> Parça sor · Mesaj. Dokunuş: satır 3 · tutar 1 · teslim **3** (önce
> 2/1/4). **Üzerinde YOK:** adım sekmeleri · "devam" düğmeleri · ayrı
> "Teslim et" düğmesi. Aşağıdaki tarih 08.09'un üç adımlı hâlini anlatıyor.

| Üzerinde ne var | Üzerinde ne YOK |
|---|---|
| Araç künyesi + **müşteri bilgileri** (aranabilir telefon — K-033.5) | Durum rozeti, durum geçmişi |
| **Ne istiyor** — çok satırlı, *"alt satıra ekle"* düğmesiyle | Kesinlik kademeleri (kesin/olabilir/şartlı) |
| **İş türü çipleri** — tahmini düzeltmek için (K-033.1) | Garanti süresi, kalan gün |
| **Yapılanlar TABLOSU** — parça ve işçilik tek tabloda (K-033.5) | Katalog seçici tabakası |
| Listenin üstünde: **Parçaları müşteri getirdi** anahtarı (K-005) | Satır başına *"kim getirdi"* sorusu *(kalktı — K-033.5)* |
| **Mikrofon** · **+ Parça** · **+ İşçilik** | Fiyat karşılaştırma tablosu |
| **Fotoğraftan parça listesi** (K-033.5) | İskonto, düze yuvarlama, KDV kırılımı |
| **Parça bekliyor** anahtarı + *"müşteriye haber ver"* (K-034) | Onay bekleme durumu, red paneli |
| **Parça fiyatı sor** → 4. ekran · **Özet gönder** → mesaj | Kabul formu, teklif belgesi |
| **Toplam** — müşteri parçası **girmez** (K-037) | |
| **Teslim et** düğmesi → 5. ekran | |

> **Satırlar tek liste.** Parça mı işçilik mi olduğu satırın kendi işareti — iki ayrı bölüm değil. Usta *"balata taktım, 800 lira işçilik"* der; sistem ikiye böler, ekran bölmez.

**Cemil onayına açık:** iskonto tamamen mi düşsün? Tamirci hiç bahsetmedi ama pazarlık gerçek. Öneri: **toplamı elle değiştirebilmek** yeter — ayrı iskonto mekanizması değil.

---

> **03.09 eklendi · cevap kimden geldi.** Birden fazla parçacıya sorulmuşsa parça satırının altında *"Cevap kimden geldi?"* çipleri çıkar (Kol B). **Tek parçacıya sorulduysa çıkmaz** — ana yol kalınlaşmıyor. Ayrı bir "fiyat karşılaştırma" ekranı böylece gerekmiyor.
>
> ⚠️ **Bu çipler 06.09'da bir gün boyunca KOPUKTU.** Tablo yeniden
> yazılırken düştüler; `parca-sor` yazmaya devam etti, okuyan kalmadı
> ve balon *"cevaplar kayda düşecek"* diye olmayan bir şey vaat etti.
> Geri kondu (kuyruk 2).
>
> **03.09 eklendi · alış fiyatı.** Parçacıdan gelen rakam satırın altında soluk durur (*"parçacı 4.200 ₺"*). Müşteriye söylenecek rakam değil; müşteri parçayı kendi getirse bile **kayıtta kalır** (Kol D).

---

## 4 · PARÇA FİYATI SOR

**Hangi işe:** *"parçacıya fiyat sormak"* — beşinci iş

| Üzerinde ne var | Üzerinde ne YOK |
|---|---|
| Hangi parçalar soruluyor *(kayıttan gelir)* | Parça arama, katalog |
| **Parçacı seç** — kayıtlı firmalar, telefonuyla (T-12b) | Yeni parçacı ekleme *(ayarlarda)* |
| Gidecek mesajın **önizlemesi**: ruhsat fotoğrafı + liste | Mesaj şablonu düzenleme |
| **GÖNDER** | Fiyat karşılaştırma |
| Gelen cevaplar *(geldiyse)* — ham metin + ayrıştırılan | OEM kodu girme alanı (K-001) |

> **Birden fazla parçacı seçilebilir** (Kol B · büyük iş) ama varsayılan tek. Cevaplar aynı yerde birikir.
>
> **03.09 · prototipte işletildi:** seçim çoklu; düğme kaç kişiye gideceğini söylüyor (*"WhatsApp ile 2 parçacıya gönder"*). Tek parçacı seçildiğinde ekran ana yoldaki hâlinden **hiç farklı görünmüyor** — kol ancak kullanılınca ortaya çıkıyor.
>
> Ustadan **OEM kodu istenmiyor** — bilmiyor (S7). Kod parçacıdan gelirse kaydedilir, gelmezse satır *"tanımlanamadı"* kalır ve iş yürür.

---

## 5 · TESLİM ET

**Hangi işe:** *"teslim ettiğinde ödemeyi nasıl aldığını bilmek"* — dördüncü istek

| Üzerinde ne var | Üzerinde ne YOK |
|---|---|
| Toplam — büyük, net | Kısmi ödeme takibi, taksit planı |
| **Ödeme yolu**: nakit · kart · havale · **almadım** | Cari hesap, borç yaşlandırma |
| **TESLİM ET** — kapalı değil, eksikse **konuşur** (K-040.3) | Teslim alan kişi seçimi |
| Teslimden sonra **soruyor**: özet gönder / geç (K-033.6) | Fatura, e-arşiv |
| Bakımsa: hatırlatma tarihi — **ay veya km, hangisi önce** (K-002) | Hatırlatma düzenleme |
| *"Parça bekliyor olarak işaretli"* uyarısı (K-036) | Teslimi engelleyen kilit |

> **Ayrı ekran, çünkü ayrı an.** Servis kaydıyla birleştirilirse yanlışlıkla teslim edilir. Teslim geri alınamaz bir eylemdir.
>
> **"Almadım"** bir ödeme yoludur — veresiye gerçektir ve teslimi bekletmez.
>
> ⚠️ **06.09'a kadar burada bir yalan vardı:** teslim balonu telefonu olan
> araçta *"Özet gönderildi."* diyor ve **hiçbir şey göndermiyordu.**
> Artık ekran soruyor ve gönderim tek dokunuş (K-033.6).

---

## 6 · ARAÇ *(akış dışı — başvuru)*

**Hangi işe:** doğrudan bir işe değil; **geçmişi** taşır. Akışın hiçbir adımı buradan geçmez.

**Sıra (04.09, Cemil tarif etti — K-019.4):** künye → **üç düğme (tek
satır)** → **NOT** → ruhsat *(açılır)* → kilometre *(açılır)* → bakım
bilgileri → geçmiş. Önce **eylem**, sonra **başvuru**.

| Üzerinde ne var | Üzerinde ne YOK |
|---|---|
| Araç künyesi + **renk kutucuğu** | Kontrol tarihleri (muayene/sigorta/kasko) |
| **Kayıt aç · Mesaj · Sesle** — tek satır | Yaklaşan kontroller |
| **Not** — düğmelerin hemen altında, kendi mikrofonuyla | Belge/medya arşivi |
| Ruhsat bilgileri *(açılır bölüm)* | TC kimlik no · adres |
| Kilometre — **yalnız tarih ve km** | **Km tahmini / günlük ortalama** (K-019.5) |
| Bakım bilgileri — son bakım + sıradaki (K-002) | Müşteri kartı *(araçtan ayrı ekran yok)* |
| **Geçmiş** — başlık · açıklama · tarih · teslim durumu | ~~Parçayı kim getirmişti rozeti~~ *(04.09'da düştü, K-019.6)* |

> **Neden var:** K-005'in işe yaraması için. *"Bu parçayı geçen sefer müşteri getirmişti"* bilgisi araç ikinci kez geldiğinde **karşıya çıkmalı**, yoksa tartışma çıkar.
>
> **⚠️ 04.09 — o bilgi bu ekrandan çekildi.** Cemil geçmiş listesini
> dört alana indirdi (*"Basit olsun"*) ve rozet düştü. Bilgi kaydın
> kendi ekranında duruyor; ekranın var oluş gerekçesi zayıfladı.
> Açık madde **A-M3**.
>
> **Müşteri ayrı ekran değil.** Tamirci araçla düşünüyor, plakayla arıyor. Müşteri bilgisi aracın üstünde durur.

---

## Ana yolun dışı — destek ekranları

Bunlar **bütçeye dahil değil** çünkü akışta yer almıyorlar; ayda bir açılır.

| Ekran | Ne için | Üzerinde ne YOK |
|---|---|---|
| **Ayarlar** *(17. ekran — 12.10, K-055.2 · K-057)* | Bakım periyodu · **parça alış fiyatı sorulsun mu** (A-M43) · tema · gizlilik satırı (A-M33). Bakım periyodu ve tema **çekmeceden taşındı**, kopyalanmadı | Dükkân bilgisi düzenleme · parçacı listesi · kullanıcı/şifre · yedekleme · bildirim tercihleri · dil |

Tek ekran. Personel, rol, yetki **yok** — tamirci bahsetmedi (K-004 notu).
Buraya **çekmecedeki tek satırdan** gelinir; ana menü altı satır kaldı (A-M8).

⚠️ 03.09'daki taslak *"dükkân bilgisi · parçacılar"* da sayıyordu. 12.10'da
yalnız K-055.2'nin sakinleri girdi: dükkân bilgisi bugün değiştirilen bir
şey değil, parçacı ağı sonraki faz (A-M46). İstenirse konuşulur.

---

## Sayım

**03.09'da değişti** (ana ekran menü oldu), **04.09'da bir ekran daha
geldi** (müşterinin gördüğü özet — ustanın ekranı değil, K-023).

| | Sayı |
|---|---|
| Ana yolun geçtiği ekran | **5** — servis kaydı ekle · kayıt · parça sor · teslim (+ menü) |
| Menüden açılan ekran | 6 — araçlar · servis kayıtları · müşteriler · muhasebe · müşteriye mesaj · sesli işlem |
| Menü altından açılan | 1 — **iş başına** *(muhasebeden, 05.09)* |
| Başvuru ekranı | 1 — araç |
| **Ustanın ekranı — toplam** | **13** |
| Müşterinin sayfası *(bütçe dışı)* | 1 — özet |
| **Prototipteki toplam** | **14** |

**Bütçe (A-1) 03.09'da menü isteğiyle cevaplandı.** 6 ekran benim önerimdi
ve *"Cemil onayına açık"* işaretliydi.

**Ölçülen asıl sayı 06.09'da yeniden ölçüldü: ana yol 13 dokunuş.**
**Ruhsatsız yolda 8**, **kayıtlı araçta 8** — ikinci kez gelen her araç
için geçerli, yani zamanla asıl yol o olacak.

Sayı 05.09'da 12'ydi ve **arttı**: tür seçimi kalktı (−1) ama ruhsat
sekmesi (+1) ve teslim sonrası kapatma (+1) geldi. Ayrıntısı ve
gerekçesi **K-035**'te; *"herhalde azalmıştır"* bir ölçüm değildir.

> Ekran sayısı bir bütçe kalemi, ama **tek başına ölçü değil.** Asıl
> ölçü, işin kaç dokunuşta bittiği ve kaç ekranın ana yola **girdiği**.
> Beş yeni ekranın hiçbiri ana yolun üstünde durmuyor.

### Yeni ekranlar

| Ekran | Neden var | Üzerinde ne YOK |
|---|---|---|
| **7 · Sesli işlem** | *"Kaydettiği ile işleme çevireceğiz"* — söylenen cümle işleme dönüyor | Tanıma motoru *(A-S1)* · serbest komut dili · onaysız işlem |
| **8 · Servis kayıtları** | Eski ana ekranın listesi + teslim edilmişler | Durum alanı *(süzgeç `teslimTarihi`'nden türetilir, K-004)* |
| **9 · Araçlar** | *"Hangi araçlar kayıtlı"* — menüden gelince gidecek yeri kalmamıştı. **04.09'da satır yeniden çizildi (K-019.2):** plaka · marka/model/motor + **renk kutucuğu** · ruhsat sahibi · telefon · ikinci iletişim. İçeridekiler üstte, **çerçeveden** belli | Araç ekleme formu · araç ikonu · *"içeride"* yazısı · kayıt sayısı · son bakım · VIN · kontrol tarihleri |
| **10 · Müşteriler** | Menüde istendi | **Müşteri tablosu** — liste araçtan türetiliyor |
| **11 · Muhasebe** | Menüde istendi | Kâr · cari hesap · fatura/KDV · gider kategorisi · aylık özet *(K-022: kapsam **kasa+gider** önerildi)* |

---

## Müşterinin gördüğü sayfa — ustanın ekranı DEĞİL

| Ekran | Neden var | Üzerinde ne YOK |
|---|---|---|
| **12 · Özet** | Yedi işten **altıncısı**: müşteriye özet göndermek (T-19/20/21). Tamirci bugün PDF gönderiyor; **link canlıdır, PDF ölü** (K-023) | Fiyat düzenleme · onay/red düğmesi · ödeme alanı · **alış fiyatı** · başka araç · geçmiş kayıt · giriş/parola *(A-M9)* |

**Ekran bütçesi (A-2) ustanın ekranlarını sayıyor.** Bu sayfayı müşteri
görüyor; prototipte ustanın **önizlemesi** olarak duruyor ve tepesinde bunu
yazıyor. Ustanın erişimi: mesaj ekranındaki *"Müşteri ne görecek?"* düğmesi —
yalnız **özet şablonunda** çıkar.

---

**Kıyas:** v1 · 38 ekran.

### Ölçülen dokunuş (06.09.2026 — üçüncü ölçüm)

| Yol | Dokunuş |
|---|---|
| Tam yol — ruhsat çekilerek, parça soruluyor | **13** |
| Kısa yol — ruhsat çekilir, parça sorulmuyor | **10** |
| **Ruhsatsız yol** — plaka yazılır, ruhsat atlanır | **8** |
| Kayıtlı araç yolu | **8** |
| Sesli yol | **2 + 1 konuşma** |
| Müşteriye özet | **2** |

Klavyeyle yazılanlar (parça adı, fiyat, şikâyet) sayılmadı — sayılan
yalnız **dokunma** eylemleri. Ölçüm 375 px'te gerçek tıklamalarla
yapıldı. Kol B (çok parçacı) kullanılmadığında **sıfır** dokunuş
ekliyor; kolun doğru ölçüsü budur.

Ana yolun 6'sından yalnız **5'i akış adımı taşıyor**; 6. (Araç) başvuru ekranı. 2. ekran tabakaya dönüşürse akış **4 ekranda** biter.

---

## AÇIK

| Kod | Karar |
|---|---|
| **A-14** | **2 · Ruhsatı çek** ayrı ekran mı, ana ekranın üstünde tabaka mı? Prototipte ölçülecek |
| **A-15** | **3 · Servis kaydı**'nda iskonto düşsün mü? Öneri: toplamı elle değiştirmek yeter |
| **A-4** | Ana ekranda *"parçası bekleniyor"* işareti — öneri: evet, not olarak |
| **A-16** | Müşteriye giden özet (T-19/T-20) **ne** olacak: PDF mi, link mi, ikisi de mi? A-2 ile bağlı |
