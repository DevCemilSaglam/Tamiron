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
| 1 · Ruhsatı çek, kayıt açıldı | **2 · Ruhsatı çek** |
| 2 · Ne istiyor | **3 · Servis kaydı** |
| 3 · Parçacıya sor | **4 · Parça fiyatı sor** |
| 4 · Fiyat geldi | **3 · Servis kaydı** |
| 5 · Parça geldi, kim getirdi | **3 · Servis kaydı** |
| 6 · İş yapıldı | **3 · Servis kaydı** |
| 7 · Teslim + ödeme | **5 · Teslim et** |

**Yedi adım, beş ekran.** Dördü (2, 4, 5, 6) aynı ekranda toplanıyor — **servis kaydı bu ürünün kalbi.**

Buna giriş noktası (**1 · İçerideki araçlar**) ve akış dışı başvuru ekranı (**6 · Araç**) eklenince **6** ediyor.

---

## 1 · İÇERİDEKİ ARAÇLAR *(ana ekran)*

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

## 2 · RUHSATI ÇEK *(kayıt açma)*

**Hangi işe:** *"aracın bilgilerini girmek"* — tamircinin birinci isteği

| Üzerinde ne var | Üzerinde ne YOK |
|---|---|
| Kamera — ruhsata tut | Uzun araç formu |
| Okunanlar: plaka · marka · model · yıl · motor · şasi — **düzeltilebilir** | Renk, yakıt, anahtar kodu, radyo kodu |
| **Plakayı yaz** kaçış yolu | Müşteri kaydı adımı |
| İki büyük düğme: **TAMİRAT** · **BAKIM** (K-006) | Tür alt kırılımları |
| Bakım seçilirse: **km** alanı (K-002) | Sigorta/muayene tarihleri |

> Araç kayıtlıysa okuma sonucu doğrudan onu açar, yeni kayıt oluşturur. Aynı araç iki kez yaratılmaz.

**Birleştirme seçeneği:** Bu ekran, ana ekranın üzerine açılan bir **tabaka** da olabilir — o zaman ana yol 4 ekrana iner. Prototipte deneyip karar veririz.

---

## 3 · SERVİS KAYDI *(kalp)*

**Hangi işe:** *"yapılan işlemleri belirtmek"* + *"fiyatı belirtmek"* — ikinci ve üçüncü istek

Akışın dört adımı burada (2, 4, 5, 6). **Kayıt teslim edilene kadar açık ve düzenlenebilir** (K-003).

| Üzerinde ne var | Üzerinde ne YOK |
|---|---|
| Araç künyesi — plaka, marka model, tamirat/bakım | Durum rozeti, durum geçmişi |
| **Satırlar** — parça ve işçilik, tek liste | Kesinlik kademeleri (kesin/olabilir/şartlı) |
| Her parça satırında: **kim getirdi** (tamirci/müşteri — K-005) | Garanti süresi, kalan gün |
| **Mikrofon** — yapılanı söyle (T-4) | Fotoğraftan liste okuma |
| **+ Satır** düğmesi | Katalog seçici tabakası |
| **Parça fiyatı sor** düğmesi → 4. ekran | Fiyat karşılaştırma tablosu |
| **Toplam** | İskonto, düze yuvarlama, KDV kırılımı |
| **Onay iste** düğmesi (Kol E) | Onay bekleme durumu, red paneli |
| **Teslim et** düğmesi → 5. ekran | Kabul formu, teklif belgesi |

> **Satırlar tek liste.** Parça mı işçilik mi olduğu satırın kendi işareti — iki ayrı bölüm değil. Usta *"balata taktım, 800 lira işçilik"* der; sistem ikiye böler, ekran bölmez.

**Cemil onayına açık:** iskonto tamamen mi düşsün? Tamirci hiç bahsetmedi ama pazarlık gerçek. Öneri: **toplamı elle değiştirebilmek** yeter — ayrı iskonto mekanizması değil.

---

> **03.09 eklendi · cevap kimden geldi.** Birden fazla parçacıya sorulmuşsa parça satırının altında *"Cevap kimden geldi?"* çipleri çıkar (Kol B). **Tek parçacıya sorulduysa çıkmaz** — ana yol kalınlaşmıyor. Ayrı bir "fiyat karşılaştırma" ekranı böylece gerekmiyor.
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
| **TESLİM ET** | Teslim alan kişi seçimi |
| Teslimden sonra: **müşteriye özet gönder** (T-19/T-20) | Fatura, e-arşiv |
| Bakımsa: *"6 ay sonra hatırlatılacak"* bilgisi (K-002) | Hatırlatma düzenleme |

> **Ayrı ekran, çünkü ayrı an.** Servis kaydıyla birleştirilirse yanlışlıkla teslim edilir. Teslim geri alınamaz bir eylemdir.
>
> **"Almadım"** bir ödeme yoludur — veresiye gerçektir ve teslimi bekletmez.

---

## 6 · ARAÇ *(akış dışı — başvuru)*

**Hangi işe:** doğrudan bir işe değil; **geçmişi** taşır. Akışın hiçbir adımı buradan geçmez.

| Üzerinde ne var | Üzerinde ne YOK |
|---|---|
| Araç künyesi | Kontrol tarihleri (muayene/sigorta/kasko) |
| **Geçmiş kayıtlar** — ne yapılmış, ne zaman, kaç km | Yaklaşan kontroller |
| **Parçayı kim getirmişti** — geçmiş satırlarda görünür (K-005) | Belge/medya arşivi |
| Son bakım + bir sonraki hatırlatma | Müşteri kartı *(araçtan ayrı ekran yok)* |
| Notlar | Not silme/geri alma mekanizması |

> **Neden var:** K-005'in işe yaraması için. *"Bu parçayı geçen sefer müşteri getirmişti"* bilgisi araç ikinci kez geldiğinde **karşıya çıkmalı**, yoksa tartışma çıkar.
>
> **Müşteri ayrı ekran değil.** Tamirci araçla düşünüyor, plakayla arıyor. Müşteri bilgisi aracın üstünde durur.

---

## Ana yolun dışı — destek ekranları

Bunlar **bütçeye dahil değil** çünkü akışta yer almıyorlar; ayda bir açılır.

| Ekran | Ne için |
|---|---|
| **Ayarlar** | Dükkân bilgisi · **parçacılar** (ad + telefon, T-12b) · bakım periyodu · tema |

Tek ekran. Personel, rol, yetki **yok** — tamirci bahsetmedi (K-004 notu).

---

## Sayım

| | Sayı |
|---|---|
| Ana yol ekranı | **6** *(bütçe: 6)* ✅ |
| Destek ekranı | 1 |
| **Toplam** | **7** |

**Kıyas:** v1 · 38 ekran.

### Ölçülen dokunuş (03.09.2026, kollar eklendikten sonra)

| Yol | Dokunuş |
|---|---|
| Tam yol — parça soruluyor | **13** |
| Kısa yol — parça sorulmuyor | **9** |
| Tam yol + Kol B (çok parçacı) | 15 · **+2, yalnız kol kullanılınca** |

Klavyeyle yazılanlar (parça adı, fiyat, şikâyet) sayılmadı — sayılan
yalnız **dokunma** eylemleri. Ölçüm 375 px'te gerçek tıklamalarla
yapıldı; beş kol eklendikten sonra tam ve kısa yol **değişmedi**.

Ana yolun 6'sından yalnız **5'i akış adımı taşıyor**; 6. (Araç) başvuru ekranı. 2. ekran tabakaya dönüşürse akış **4 ekranda** biter.

---

## AÇIK

| Kod | Karar |
|---|---|
| **A-14** | **2 · Ruhsatı çek** ayrı ekran mı, ana ekranın üstünde tabaka mı? Prototipte ölçülecek |
| **A-15** | **3 · Servis kaydı**'nda iskonto düşsün mü? Öneri: toplamı elle değiştirmek yeter |
| **A-4** | Ana ekranda *"parçası bekleniyor"* işareti — öneri: evet, not olarak |
| **A-16** | Müşteriye giden özet (T-19/T-20) **ne** olacak: PDF mi, link mi, ikisi de mi? A-2 ile bağlı |
