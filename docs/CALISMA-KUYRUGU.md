# TAMIRON — ÇALIŞMA KUYRUĞU (CALISMA-KUYRUGU.md)

> **Ne işe yarar:** Cemil bilgisayarda değilken lider tek başına
> çalışıyor. Bu dosya **sıradaki işi** ve **hangi işte kalındığını**
> tutar. HANDOFF *durumu* söyler, bu dosya *sırayı*.
>
> **Kural:** bir iş bitmeden bir sonrakine geçilmez. Her iş bitince
> · derleme denetimi · sağlık denetimi 320/375/430 px'te **0 bulgu**
> · commit (gerekçesiyle) · push · bu dosyada **BİTTİ** işareti + tek
> satır sonuç.
>
> **Karar gerektiren yerde durulmaz:** varsayım yazılır, *"Cemil
> onayına açık"* işaretlenir, iş bitirilir. Yalnız yanlış varsayım
> işi çöpe çıkaracaksa durulur ve buraya **BEKLİYOR** yazılır.
>
> **Yeni özellik yok.** Bu kuyruk hiçbir yeni özellik içermiyor;
> hepsi **doğrulama, uzlaştırma ve ölçüm**. Kuyruk sırasında yeni bir
> özellik ihtiyacı doğarsa yazılmaz — buraya *öneri* olarak düşülür.

---

## Tur 3 · 06.09.2026 · son commit `906c553`

Önceki iki kuyruk (04.09 ve 05.09) **bitti**, aşağıda arşivde.
Bu kuyruk **K-033 turundan sonra açık kalan borçları** kapatıyor.
Sırası **riske göre**: en üstteki, yanlışsa en çok şeyi bozan.

| # | İş | Durum | Sonuç |
|---|---|---|---|
| 1 | **Veri modeli uzlaştırması** — `parcaBekliyor` ve `turElle` modelde YOK, model bunları açıkça yasaklıyor | AÇIK | |
| 2 | **Ölü uç turu** — bu turda değişen 8 ekran elle işletilecek | AÇIK | |
| 3 | **Dokunuş sayımı** — ana yol değişti, HANDOFF'taki `12` artık yalan olabilir | AÇIK | |
| 4 | **İki "bekliyor" ayrımı** — `fiyatiBekleyen` ≠ `parcaBekliyor`, her ekranda doğru olan mı yazıyor | AÇIK | |
| 5 | **Muhasebe hâlâ doğru mu** — "hepsi müşteriden" anahtarı fiyat→alış taşıyor, iş başına farkını bozuyor olabilir | AÇIK | |
| 6 | **Müşteri özeti** — yeni yapıya uydu mu; "parça bekliyor" müşteriye söylenmeli mi | AÇIK | |
| 7 | **Açık tema turu** — bu turun bileşenleri çoğunlukla koyu temada görüldü | AÇIK | |
| 8 | **İki yeni denetim testi** — bugün hiçbir testin yakalamadığı iki hata sınıfı | AÇIK | |
| 9 | **320 px elle turu** — denetim 0 diyor; K-025 sessiz kırpmayı da 0 demişti | AÇIK | |
| 10 | **Belge tazeleme** — EKRANLAR · IS-AKISI · SOZLUK · HANDOFF sayıları | AÇIK | |

---

## 1 · Veri modeli uzlaştırması

**Neden ilk sırada:** `VERI-MODELI.md §2` şunu yazıyor —

> *"`Status` YOK… **'Parçası bekleniyor' gibi bilgiler bu kaydın
> verisinden türetilir, saklanmaz.**"*

Dün (K-033.5) ben o kaydın üstüne **`parcaBekliyor` diye saklanan bir
alan** koydum. Ayrıca `turElle` diye ikinci bir saklanan alan var.
**CLAUDE.md açık:** *"Alanlar VERI-MODELI.md ile BİREBİR — modelde
olmayan alan buraya da girmez."* Şu anda prototip belgeyi çiğniyor.

İki yoldan biri seçilecek ve **gerekçesi yazılacak**:

* **(a) Alanlar kalır, model güncellenir.** Savunması: ikisi de
  türetilemez. *"Parça yolda"* ustanın **bildirdiği bir gerçek** — veride
  onu söyleyen hiçbir şey yok (fiyatın boş olması başka şey demek,
  K-033.5). `turElle` ise ustanın **kararı**; karar türetilemez, yoksa
  bir sonraki tahmin onu ezer (K-033.12).
* **(b) Alanlar kalkar.** O zaman K-033.5 ve K-033.12 geri alınır ve
  Cemil'in *"parça bekliyor alanı olmalı"* maddesi karşılıksız kalır.

**Önerim (a)** — ama karar `K-004`'ün sınırını genişlettiği için
gerekçesi KARARLAR'a yazılmalı: **bayrak ≠ durum.** `parcaBekliyor`
akışı değiştirmiyor, hiçbir ekranı kilitlemiyor, teslimi engellemiyor;
`DeliveredAt` hâlâ tek durum alanı.

**Bitti sayılır:** `VERI-MODELI.md` ve `SOZLUK.md` alanları içeriyor,
K-004'ün notu *"bayrak durum değildir"* diye genişletilmiş, karar
KARARLAR'a düşmüş, `02-veri.js`'in tohum verisi belgeyle birebir.

## 2 · Ölü uç turu — bu turda değişen sekiz ekran

**Neden:** 27. test *"ekranda hiç eylem yok"* der; söylemediği şey
**eylem VAR ama işi ilerletmiyor.** 04.09'da bu tur üç yalan buldu
(K-021), 06.09'da elle işletme dört hata daha buldu ki **hiçbirini
36 test yakalamamıştı.**

**Hangi ekranlar:** `ruhsat · kayit · teslim · arac · kayitlar ·
musteriler · mesaj · ozet`.

**Nasıl:** her düğmeye/çipe/alana **gerçekten dokun**, nereye
gittiğini ve **veride ne değiştiğini** yaz. Şu üç soruyu her düğmeye
sor:

1. Düğmenin **adı**, yaptığı işi söylüyor mu?
2. Balonu **olan** bir şeyi mi söylüyor, yoksa *"gönderildi"* gibi
   **olmayan** bir şeyi mi? (Bu turda bir tane daha çıktı — K-033.6.)
3. Basınca **hiçbir şey olmuyorsa**, ekran nedenini söylüyor mu?

**Özellikle bak:** `parca-sor` ekranı satır ekliyor mu, hâlâ eski
`.kalem` dilinde ve yeni tabloyla tutarlı mı; `sesUygula`'nın açtığı
kayıt yeni alanları taşıyor mu; `RUHSAT_ONDOLGU.tur` artık kullanılmıyor
(ruhsat ekranı türü metinden okuyor) — **ölü veri mi kaldı?**

**Bitti sayılır:** bulgular düzeltildi; yeni bir hata sınıfı çıktıysa
8. işe test önerisi olarak yazıldı.

## 3 · Dokunuş sayımı yeniden ölçülür

**Neden:** HANDOFF *"ana yol 12 dokunuş"* diyor ve o sayı **05.09'da**
ölçüldü. O gün ana yolda bir **tür seçimi** vardı; kalktı. Yerine
kayıtsız araçta bir **ruhsat teklifi** geldi. Sayı değişti ve
**değiştiğini bilmiyoruz.**

**Kural (v1'den):** sayı ölçülmeden yazılmaz. *"Herhalde azalmıştır"*
bir ölçüm değildir.

**Nasıl:** beş yol da baştan sona **yürütülecek** ve her dokunuş
sayılacak: ana yol (yeni araç) · kısa yol · kayıtlı araç yolu · sesli
yol · müşteriye özet. Yeni bir yol daha var: **ruhsatı çekerek** açılan
kayıt.

**Bitti sayılır:** HANDOFF tablosundaki sayılar yenilendi; ekran
bütçesi (A-2) aşıldıysa **söylendi**, sessizce geçilmedi.

## 4 · İki "bekliyor" birbirine karışıyor mu

**Neden:** artık iki ayrı şey var ve **ikisi de "bekliyor" diyor**:

| | Ne demek | Nereden gelir |
|---|---|---|
| `fiyatiBekleyen(k)` | **fiyatı** gelmemiş satır var | türetilir |
| `k.parcaBekliyor` | **parça** yolda | usta işaretler |

Bunlar ayrı gerçekler: parça elde olup fiyatı belirsiz olabilir, ya da
fiyatı belli olup parça yolda olabilir. **Ekranların hangisini
söylediği tek tek kontrol edilecek:** ana ekran · servis kayıtları
kartı · araç geçmişi · teslim ekranı · özet · muhasebe.

**Şüphelendiğim yer:** `kayitKartiHtml` şu an ikisini **tek rozete**
bindiriyor (`k.parcaBekliyor || fiyatiBekleyen(k)` → *"parça bekliyor"*).
Bu, fiyatı girilmemiş bir satırı *"parça yolda"* diye gösteriyor
olabilir — **yanlış rakam göstermek gibi, yanlış durum göstermek de
göstermemekten kötüdür** (K-013'ün kuralı).

**Bitti sayılır:** her ekranda hangisinin yazdığı belli; ikisi tek
rozete biniyorsa ya ayrıldı ya da **neden bindiği yazıldı**.

## 5 · Muhasebe hâlâ doğruyu mu söylüyor

**Neden:** *"Parçaları müşteri getirdi"* anahtarı, satırın `fiyat`ını
**`alis`e taşıyor** (K-005'in kuralı: rakam kaybolmasın). Ama
`isKari()` iş başına farkını `alinan − parça alışı` diye hesaplıyor ve
`parcaAlisiToplami()` bütün `alis` toplamını alıyor.

**Soru:** müşteri kendi parçasını getirdiğinde o parça için **biz
ödeme yapmadık** — ama `alis` dolu. Muhasebe o parayı **ödenmiş** gibi
mi sayıyor? Öyleyse iş başına farkı ve parça alışı toplamı **yanlış**.

**Nasıl:** rakamla ölç. Bugünkü tohum veride 2026 iş başına
`12.970 − 7.160 = 5.810` idi. *"Hepsi müşteriden"* anahtarını bir kayıtta
aç, rakamları yeniden oku, **elle hesapla ve karşılaştır.**

**Bitti sayılır:** ya rakam doğru çıktı (ve nedeni yazıldı), ya
`parcaAlislari()` *"müşteri getirdiyse alış sayılmaz"* diye düzeltildi
ve **eski/yeni rakam commit mesajına yazıldı.**

## 6 · Müşteri özeti yeni yapıya uydu mu

**Neden:** `ozet` ekranı müşterinin gördüğü sayfa (K-023) ve bu turda
kaydın yapısı değişti. Kontrol: *"müşteri getirdi"* rozeti hâlâ doğru
çiziliyor mu, boş satır ne gösteriyor, tür değişince başlık değişiyor mu.

**Karar gerektiren yer:** **parça beklediği müşteriye söylensin mi?**
Söylenirse müşteri aramaz — tamircinin şikâyet ettiği telefon trafiği
azalır. Söylenmezse *"neden bu kadar sürdü"* sorusu kalır.
**Varsayım: söylenir**, çünkü mesaj şablonu zaten söylüyor ve sayfanın
mesajla çelişmesi daha kötü. *Cemil onayına açık.*

**Bitti sayılır:** sayfa yeni yapıyla tutarlı; varsayım işaretlendi.

## 7 · Açık tema turu

**Neden:** bu turda yazılan bileşenlerin çoğunu **koyu temada** gördüm:
tablo içindeki çerçevesiz alanlar, `.ms-kart`, `.ak-tutar`, balonun yeni
hâli, `.k-alis`. 03.09'da tam bu sebeple bir hata çıkmıştı — *"iki
temayı da çizmek gerekiyor, birini çizip ötekine bakmamak değil."*

**Nasıl:** `Tema` üç hâlde de (sistem · açık · koyu) sekiz ekran
gezilecek; özellikle **kontrast** ve **çerçevesiz alanların görünürlüğü**.
Denetimin kontrast testi rakamı verir, ama *"bu alan buraya yazılır"*
duygusunu vermez — ekran görüntüsü alınacak.

**Bitti sayılır:** her iki temada ekran görüntüsü alındı, düzeltmeler
token'la yapıldı (**ham hex YOK**).

## 8 · İki yeni denetim testi

Bugün **dört hata** elle işletince çıktı ve 36 test hepsine *"0 bulgu"*
dedi. İkisi otomatikleştirilebilir:

**Test 37 — SESSIZ-KAPALI-DUGME.** Her ekranda `button[disabled]` ara.
Kapalı bir düğmenin **yakınında** nedenini söyleyen bir şey (bir
`.notice`, `.plate-hint` ya da `aria-describedby`) yoksa bulgu.
*Gerekçe:* K-033.3 — kapalı düğme sessizdir, Cemil bastı ve hiçbir şey
olmadı.

**Test 38 — TAZELEMEDE-KAYBOLAN-SECIM.** Her ekranı çiz, `aria-pressed="true"`
olan öğeleri say, `Ekran.tazele()` çağır, yeniden say. Sayı düşüyorsa
bulgu.
*Gerekçe:* K-033.12 — `ciz()` içinde sıfırlanan bir ekran değişkeni,
her tazelemede ustanın seçimini siliyordu. Aynı hatayı **bir turda iki
kez** yaptım.

**Her ikisi de NEGATİF TESTLE doğrulanacak:** hata kasten geri konacak,
test bulgu verecek, sonra geri alınacak. *Delikli denetim, denetimsiz
olmaktan kötüdür.*

⚠️ **Denetimin kendisini kirletme (K-024.1):** test 38 `Ekran.tazele()`
çağırıyor; çağırdığı ekranın durumunu **bozmadan** bırakmalı.

**Bitti sayılır:** iki test eklendi, negatif testle doğrulandı, üç
genişlikte 0 bulgu.

## 9 · 320 px elle turu

**Neden:** denetim üç genişlikte de 0 diyor. K-025'te de 0 diyordu ve
düğme metni **sessizce kırpılıyordu** — kendi `overflow:hidden`'ım
taşma testinden gizliyordu.

**Nasıl:** 320 px'te sekiz ekranın **ekran görüntüsü** alınacak ve
**gözle** okunacak. Özellikle: kayıt tablosunun iki sütunu (rozet + ad
+ tutar aynı satıra sığıyor mu), `.ms-kart` başlığı (uzun firma adı +
sayaç), ruhsat akordiyonunun ikili alanları, teslim sonrası panel.

**Bitti sayılır:** her ekranın 320 px görüntüsü alındı; kırpılan ya da
üst üste binen hiçbir şey yok.

## 10 · Belge tazeleme

`EKRANLAR.md` (kayıt ve ruhsat ekranlarının tarifi değişti) ·
`IS-AKISI.md` (akıştan **tür seçimi adımı** kalktı) · `SOZLUK.md`
(yeni alanlar ve terimler) · `HANDOFF.md` (test sayısı, dokunuş
sayıları, karar aralığı).

**Bitti sayılır:** belgelerde prototiple çelişen tek cümle kalmadı.

---

## Arşiv

### Tur 2 · 05.09.2026 — yedi iş, yedisi de BİTTİ

Ölü uç turu (3 yalan) · A-M2 · muhasebe kapsamı (K-022) · müşteri
özeti (13. ekran) · sesli işlem kapsam denetimi (denetimin kendisi
bozuktu) · üç genişlikte ölçüm (320'de sessiz kırpma) · belge tazeleme
(ana yol 13 değil **12** çıktı). Sonuç: K-019…K-025, denetim 27 → 32 test.

### Tur 1 · 04.09.2026

`4a6dfe5` öncesi; ayrıntısı git geçmişinde.
