# TAMIRON — KARARLAR (KARARLAR.md)

> Verilen kararların gerekçeleri. Aynı tartışma iki kez açılmasın. Yeni karar **en üste** eklenir.
>
> **Bu liste 03.09.2026'da SIFIRDAN başladı.** v1'in ürün/akış kararları bilinçli olarak devralınmadı — tamircinin geri bildirimi (`arastirma/TAMIRCI-GERIBILDIRIM.md`) akışı yeniden kuruyor ve eski kararların gündemde durması yeni tartışmayı kirletiyordu.
>
> **Devralınan mimari kararlar `MIMARI.md`'dedir** ve burada tekrarlanmaz: teknoloji yığını, üç uygulama ayrımı, tenant/yetki altyapısı, soft-delete, katalog hibrit yapısı, Türkçe UI / İngilizce kod, tokens tek kaynak, tasarım-önce. Bunlar akıştan bağımsızdır — değişecekse `MIMARI.md` açılır.
>
> v1 arşivi: `github.com/DevCemilSaglam/Tamiron-v1` (195 commit; her kararın gerekçesi commit mesajında).

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

**Kapsam dışı:** km tahmini ("ayda ortalama şu kadar gidiyor"), km-bazlı bildirim, araçtan otomatik km okuma.

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

| Kod | Konu | Ne zaman |
|---|---|---|
| **A-1** | **Ekran bütçesi** — ana yol için en fazla 6 önerildi. Onay? | Cemil |
| **A-2** | Müşteri tarafı (T-19/T-20/T-21) **bu fazda mı**? Öneri: özet linki evet, müşteri uygulaması sonra | Cemil |
| **A-3** | Bakım periyodu varsayılanı kaç ay? Öneri: 6 | Cemil |
| **A-4** | *"Parça bekliyor"* listede türetilerek gösterilsin mi (K-004 notu) | Akış çizilirken |
| **A-5** | Ruhsat okuma hangi teknolojiyle — OCR servisi, doğruluk, elle düzeltme | 9. iş |
| **A-6** | WhatsApp API sağlayıcısı ve maliyeti | 8. iş |
| **A-7** | SMS sağlayıcısı + **İYS kaydı** (yasal zorunluluk) | 10. iş |
