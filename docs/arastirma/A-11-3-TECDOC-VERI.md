# A-11.3 · TecDoc verisi — hangi paket, ne kadar, alternatifi ne?

> 30.08.2026 · Cemil'in üç sorusuna cevap. **Karar hâlâ Cemil'in** (K-005).
> Sorular: *"TecDoc verisi ne kadar alınabilir? Hangi solution kullanılmalı, sitede bir çok solution var."* · *"Başka alternatif var mı?"* · *"B'yi yapsak fakat TecDoc verisi kullanmasak ne olabilir?"*

---

## ⚠️ 30.08 · CEMİL DÜZELTMESİ — bu raporun sonucu değişti

> *"Bizim amaçlarımızdan biri de şasi no ve araç marka model vb. ile **uyumsuz parçalardan kaçınmak**. Bunu yapmak istiyoruz. Yoksa TecDoc vb. veriyi nereden aldığımız önemli değil. Veride önemli olan **doğru veri** olması, eksik olmasın."*

Aşağıdaki bölüm 4'te *"Fitment boş kalsa da OEM ile idare edilir"* demiştim. **Bu, amacı ikinci plana atan bir değerlendirmeydi.** Doğrusu:

- **`Fitment` VAZGEÇİLMEZ.** "Uyumsuz parçadan kaçınmak" ürünün çekirdek işlevi ise, araç→parça bağı olmadan bu iş yapılamaz.
- **OEM eşleştirme bunun yerine geçmez.** OEM *parçadan parçaya* çalışır (*elimdeki numaranın muadili ne?*); istenen ise *araçtan parçaya* (*bu araca ne uyar?*). Usta numarayı bilmiyorsa — ki çoğu zaman parçayı sökmeden bilemez — zincir hiç başlamaz.
- **Dolayısıyla A-11.3 "acil değil" DEĞİL.** Şemayı hâlâ bloklamıyor *(o kısım doğru)*, ama **ürünün asıl değeri bu veriye bağlı** — tedarik kaynağı erkenden çözülmeli.
- **Kaynak markası önemli değil, veri kalitesi önemli** (Cemil): TecDoc olması şart değil; **doğru ve eksiksiz** olması şart.

### Bunun vPIC'e etkisi
**vPIC tek başına YETMEZ.** Zincir iki halkalı:
1. **VIN → araç tipi** — vPIC bunu yapar (ücretsiz).
2. **Araç tipi → uyumlu parçalar** — vPIC bunu **yapmaz**; `Fitment` verisi ister.

vPIC birinci halkayı ücretsiz çözer, ama ikinci halka için ayrı bir uyumluluk kaynağı (TecDoc ya da eşdeğeri) gerekir. → **A-11.5 önerisi geçerli ama kısmi çözüm**; A-11.3'ün yerini tutmaz.

---

## Baştan söylenecek sonuç

**A-11.3 kararı şemayı değiştirmiyor.** TecDoc alınsa da alınmasa da **Seçenek B'nin tabloları aynı kalıyor**; değişen, ortak kataloğun *ne zaman ve nasıl dolacağı*.

⚠️ **Ama "şemayı bloklamıyor" ile "ertelenebilir" aynı şey değil** (yukarıdaki Cemil düzeltmesi): uyumsuz parçadan kaçınmak çekirdek işlev olduğu için **veri tedariki erken çözülmeli.** Şema beklemez; ürünün sözü bekler.

Sebebi zaten kendi kararımızda: **K-002 hibrit katalog** (`TenantId` nullable) tam olarak bunun için kurulmuştu. Ortak katalog boş başlar, dükkan kendi ürünlerini ekler; TecDoc sonra gelirse ortak tarafı doldurur.

---

## 1 · Hangi paket? — sitedeki karışıklığın sebebi

TecAlliance'ın ürünleri **iki bambaşka kitleye** satılıyor ve isimleri birbirine benziyor. Karışıklık buradan geliyor:

### ❌ Bize UYMAYAN: "TecDoc Catalogue" ailesi
*Garage Data · Garage Data Pro · Garage Data Truck · Classic*

Bunlar **hazır uygulama** — tamircinin parça aradığı web sitesi ve mobil app'i (Google Play / App Store'da yayında). Kendi yazılımına entegre edilecek ham veri **değil**.

> Bunu alırsak Tamiron'a veri gelmez; tamirciye **ikinci bir program** vermiş oluruz. Yani bu ürün bizim **rakibimiz**, tedarikçimiz değil.

### ✅ Bize UYAN: "Data User" tarafı
| Ürün | İçerik | Bizdeki karşılığı |
|---|---|---|
| **TecDoc Reference Data** | Araç tanımları · jenerik parça kategorileri · teknik özellikler · uyum kriterleri · 42 dilde açıklama | `VehicleType` + `GenericArticle` + `AttributeDefinition` + `AttributeTemplate` |
| **TecDoc OE Parts Data** | OE numaraları · çapraz referanslar · muadil eşleştirmeleri · araç bağlantıları | `ProductReference` + `Fitment` |
| TecDoc VIO | Trafikteki araç popülasyonu (Excel/CSV/SQL) | *(gerekmez — pazar analizi ürünü)* |
| IDP Data Supplier API · TecDoc ONE | Parça **üreticilerinin** kendi verisini TecDoc'a yüklemesi | *(bizim rolümüz değil)* |

**Kısacası:** aranacak şey **"TecDoc Reference Data"** (+ isteğe bağlı **OE Parts Data**). Mağazadaki "Catalogue" paketleri değil.

---

## 2 · Ne kadar?

**Ham veri lisansının fiyatı hiçbir yerde yayınlanmıyor** — teklif bazlı, çoğu AB ülkesinde resmî TecAlliance ortakları üzerinden brokerlik ediliyor.

Bulunabilen referans noktaları (bunlar **tam veri lisansı değil**, büyüklük fikri versin diye):

| Ne | Fiyat | Not |
|---|---|---|
| TecDoc Catalogue online kayıt | ~**219 EUR/yıl** | Tek kullanıcı *katalog erişimi* — bize uymaz |
| Optiseller (3. taraf yeniden satıcı) | ~**600 GBP/yıl** | İlk **5.000 ürünlük** veri; sonrası ek paket |
| Tam Reference/OE veri lisansı | **teklif bazlı** | Yayınlanmıyor; ortak üzerinden görüşülür |

⚠️ İki uyarı:
- **Lisans ticari zorunluluk:** TecDoc verisiyle lisanssız ürün/servis işletilemez.
- Sektör yorumu: kataloğun **küçük bir kısmına** ihtiyaç duyan firmalar için maliyet tarihsel olarak **caydırıcı** olmuş — Optiseller gibi ara satıcılar tam da bu yüzden doğmuş.

> **Yapılacak:** Tamiron'un ihtiyacı (TR pazarı · binek + hafif ticari · Reference Data ağırlıklı) tarif edilip **TecAlliance'tan ve bir TR ortağından teklif alınmalı.** Rakam olmadan bu kalem bütçelenemez.

---

## 3 · Alternatif var mı?

**Doğrudan alternatifi yok.** Avrupa'da TecDoc fiilî standart; Autodata · ALLDATA · Mitchell 1 · HaynesPro **rakip değil, tamamlayıcı** — onlar *tamir/tanılama bilgisi* verir, TecDoc *parça kataloğu*. Aynı işi yapan ikinci bir Avrupa kataloğu yok.

Ama **araç tipi kataloğu için ücretsiz ve ciddi bir kaynak var:**

### ⭐ NHTSA vPIC (ABD Ulaştırma Bakanlığı) — A-11.5'in cevabı olabilir
- **Tamamen ücretsiz.** API anahtarı yok, kullanım limiti yok, ücret yok.
- **İndirilebilir veritabanı** — **MS SQL Server** ve PostgreSQL formatında. *(Bizim MSSQL kararımızla doğrudan uyumlu.)*
- VIN'den **100+ alan**: marka · model · yıl · motor silindir sayısı · hacim · yakıt türü · kasa tipi · üretim tesisi.
- 1981 ve sonrası model yılları.

**Sınırı:** ABD odaklı. VIN standardı global olduğu için marka/model/yıl Avrupa araçlarında da çözülür, ama **Avrupa'ya özgü motor/donanım varyantları eksik kalır** — "1.5 dCi 110 hp" ayrımını TecDoc kadar iyi vermez.

**Diğer seçenekler:** üçüncü taraf "TecDoc API alternatifi" servisleri var (ör. Apify üzerindeki kazıma tabanlı katalog) — **hukuki risk** taşır, TecDoc lisans şartlarına aykırı olabilir; önermiyorum.

**Bir kaynak daha, zaten elimizde olacak:** Akış 7'deki **B2B tedarikçi entegrasyonları**. Onlardan gelen veri ürün + fiyat + stok taşıyor; kısmen uyumluluk da taşıyabilir. Yani katalog **kullandıkça kendiliğinden zenginleşir**.

---

## 4 · "B'yi yapsak ama TecDoc verisi kullanmasak ne olur?"

**Yapı aynen kurulur ve çalışır.** Tablolar değişmez, sadece bazıları başlangıçta boş kalır.

| Katman | TecDoc'suz ne olur |
|---|---|
| `VehicleType` | **vPIC'ten** ya da ruhsat alanlarından türetilerek dolar. Prototipte marka/model/yıl/motor zaten toplanıyor. **Sorun değil.** |
| `GenericArticle` | Dükkan/ortak tanımlı. Başlangıç için **100–200 jenerik tip** yeter (fren balatası, yağ filtresi…). Elle kurulabilir. **Sorun değil.** |
| `Product` | K-002 gereği dükkan kendi ürünlerini ekler; B2B'den de akar. **Sorun değil.** |
| `AttributeTemplate` | En çok kullanılan 20–30 jenerik tip için elle yazılır. **Emek ister ama yapılabilir.** |
| **`Fitment`** | ⚠️ **ZAYIF HALKA.** Milyonlarca satır; elle kurulamaz. |

### Fitment boş kalırsa pratikte ne kaybederiz?
- **"Bu araca uyan balatalar"** sorusu **cevaplanamaz.** Akış 7'deki *"listedeki ürünler araca uyumlu olanlardan süzüldü"* adımı çalışmaz — liste süzülmeden gelir, **usta kendi bilgisiyle seçer** (bugün zaten böyle çalışıyor).
- Karşılaştırma yine **ürün üzerinden** yapılır (senin kararın korunur) — ama ürün kataloğu dükkanın kendi ürünleri + B2B'den gelenler olur, ortak katalog değil.
- **OEM numarası** üzerinden eşleştirme yine mümkün — ama **yalnız usta numarayı biliyorsa.** `ProductReference` *parçadan muadile* gider; *araçtan parçaya* gitmez. ⚠️ **Bu, uyumluluk süzmesinin yerine geçmez** (yukarıdaki Cemil düzeltmesi).

### Ve en önemlisi
`VehicleType.ExternalCode` ile `GenericArticle` dış-kod alanları **baştan duruyor** (A-11.4 kararıyla `TargetType` de öyle). TecDoc sonradan alınırsa **şema kırılmadan** veri akar. Yani:

> **TecDoc'suz başlamak şemayı bloklamaz** — ama **uyumluluk süzmesi olmadan ürün asıl işini yapmaz.** Geçiş yolu, varış noktası değil.

---

## 5 · Öneri

1. **Şema beklemez, ama veri tedariki ERKEN çözülmeli.** A-11.3 şemayı etkilemiyor; yine de uyumluluk süzmesi çekirdek işlev olduğu için tedarik kaynağı geciktirilmemeli.
2. **Teklif iste** — TecAlliance + bir TR ortağı, "Reference Data (+OE Parts Data), TR pazarı, binek + hafif ticari" tarifiyle. Rakam gelmeden bütçelenemez.
3. **vPIC'i ciddiye al** (A-11.5): ücretsiz, MSSQL formatında indirilebiliyor, ilk sürüm için araç tipi kataloğunu doldurmaya yeter. Avrupa varyant detayı eksik kalır — ama o eksik, TecDoc gelince kapanır.
4. **Fitment'ı ürünün çekirdeği say.** Prototip/akış süzme olmadan da çalışır (ekranlar bugün öyle duruyor), ama **ürün sözünü ancak uyumluluk verisiyle tutar.**
5. **Kaynak seçiminde marka değil KALİTE ölçülsün** (Cemil): hangi sağlayıcı olursa olsun kapsam (TR'deki araç parkı), güncellik ve **eksiksizlik** test edilmeli — örnek 20 araç × 10 parça ile doğruluk ölçümü yapılabilir.

---

## ⚠️ Açık kalan

- **A-11.3** — teklif gelmeden karar verilemez. *(Şemayı bloklamıyor.)*
- **A-11.5 / VM-2** — araç tipi kaynağı: **vPIC ile başlanması öneriliyor**, onay bekliyor.

## Kaynaklar

- [TecAlliance — TecDoc Catalogue (hazır uygulama)](https://www.tecalliance.net/tecdoc-catalogue/)
- [TecAlliance — Data Delivery / Data User](https://www.tecalliance.net/tecdoc-data-delivery/)
- [TecAlliance mağazası — Garage Data paketi](https://shop.tecalliance.net/tecdoc-catalogue-garage-data/5637251097.p)
- [NHTSA vPIC — ücretsiz araç veri API'si ve indirilebilir veritabanı](https://vpic.nhtsa.dot.gov/api/)
- [Optiseller üzerinden uygun maliyetli TecDoc erişimi (ChannelX)](https://channelx.world/2019/12/optiseller-give-cost-effective-tecdoc-catalogue-access-for-auto-parts-traders/)
