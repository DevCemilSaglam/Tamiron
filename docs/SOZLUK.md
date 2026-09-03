# TAMIRON — TERİM SÖZLÜĞÜ (SOZLUK.md)

> Kod/DB İngilizce, UI Türkçe (K-025). Bu tablo ekibin Türkçe kavram ↔ İngilizce kod adı eşlemesini tutar. Yeni kavram eklenince buraya işlenir.

| Türkçe (UI) | İngilizce (kod/DB) | Not |
|---|---|---|
| **Servis Kaydı** | **ServiceRecord** | ⭐ Araç her gelişinde oluşan **tek** kayıt (K-029). *Ziyaret + İş Emri BİRLEŞTİ.* UI: "Son 3 servis kaydı", "Servis No" |
| Servis işi | ServiceJob | Bir serviste birden çok iş olabilir; her işin kendi parça listesi |
| Servis satırı | ServiceLine | Parça / işçilik satırı |
| ~~Ziyaret türü~~ | ~~VisitType~~ | ❌ K-029 ile kalktı — **durum akışta belirir**, baştan tip seçilmez |
| Araç | Vehicle | Ruhsat kartı |
| **Müşteri** | Party | ⭐ Ruhsat sahibi — birey **veya** kurum/dernek/firma. **UI'da her yerde "Müşteri"** (Cemil, 31.08): *"Kişi mi müşteri mi? Müşteri doğru."* ~~Kişi / Taraf~~ kullanılmaz. Kod adları (`kisiAc`, `kisiListesi`) değişmedi — K-025 kodu bağlamaz. |
| Müşteri notu | PartyNote | Serbest metin, müşteriye bağlı (araca değil). **Aramaya dahildir** — *"Ahmet Hasan'ın kardeşi"* yazıp bulunabilir. |
| İletişim | Contact | Ayrı tablo; kişiye veya doğrudan araca bağlı. **Rol tutulmaz** |
| Geçerli iletişim | VehicleContact.IsPrimary | Araç bazında seçilir |
| Sahip | Owner | Araç ruhsat sahibi (kişi/kurum) |
| Cari | Account | ⚠️ **MUHASEBE'nin varlığı, Asistan'ın değil** (VM-5/A-07). Kişilerle karıştırılmaz; worker muhasebeleştirir |
| **Tedarikçi** | Supplier | ⭐ Fiyat veren ve **mal gönderen** taraf. Alt türleri: **Parçacı · Distribütör · B2B**. *(31.08: sözlükte `Supplier`="Parçacı/Distribütör" ama `SupplierAccount`="Tedarikçi hesabı" yazıyordu — iki terim tek kelimede birleştirildi. Cemil "satıcı" önermişti; **"satıcı" bizim kendi satış tarafımızla karışıyor**, müşteriye biz satıcıyız.)* |
| Mal kabul kaydı | GoodsReceipt | Bir kutunun kabulü: **tedarikçi + irsaliye + kalemler**. Geçmişi tutulur, **düzeltilebilir** — mal kabulde hata olur. |
| **Tahmini fiyat** | PriceEstimate | ⭐ **Ustanın kendi yazdığı, doğrulanmamış rakam** (Cemil, 31.08). *"Amacımız fiyat almak, fakat usta afaki fiyat yazabilir."* Teklif **değildir**; gelen teklifle karıştırılmaz, teklif gelince **silinmez** — yan yana durur, fark görünür. |
| Fiyat kaynağı | PriceSource | `usta` (tahmini fiyat) · `tedarikci` (gelen teklif) · `satis` (müşteriye verilen). Ayrı **alan** değil, her fiyatın taşıdığı bilgi. |
| Tahmini fiyat isabeti | EstimateAccuracy | Kapanmış kayıtlarda tahmini fiyat ile gelen teklif farkının ortalaması. Yükseldikçe teklif beklemeden fiyat verilebilir. |
| Tedarik fiyat teklifi | SupplierOffer | Parçacı→tamirhane (A-02) |
| Müşteri teklifi | Quote | Tamirhane→müşteri (A-02) |
| Yapılacaklar | WorkItems / Tasks | 4 öncelik |
| Parça | Product / Part | Katalog |
| Stok | StockItem | |
| Dükkan (tenant) | Tenant | Firma |
| Şube | Branch | |
| Ödeme | Payment | |
| Kullanıcı | AppUser | Personel/usta hesabı |
| Rol / İzin | Role / Permission | Yetki matrisi |

| Araç tipi | VehicleType | "Mégane III 1.5 dCi 2012–2016" — parça uyumluluğu buna bağlanır (A-11) |
| Araç örneği | Vehicle | Servise gelen O araba; doğal anahtar **şasi no (VIN)** — D-1 |
| Plaka geçmişi | VehiclePlate | Plaka araca zamanla bağlıdır (D-1) |
| Km okuması | OdometerReading | Km tek alan değil, zaman serisi (N-10) |
| Jenerik parça tipi | GenericArticle | "fren balatası" — ortak katalog (A-11) |
| Öznitelik tanımı | AttributeDefinition | Tipli: alfanümerik/sayısal/tarih/liste/değersiz |
| Öznitelik şablonu | AttributeTemplate | Jenerik tip başına zorunlu/önerilen |
| Ürün özniteliği | ProductAttribute | Araçtan **bağımsız** sabit özellik (balatanın kalınlığı) |
| Bağlantı özniteliği | FitmentAttribute | Bağlantıya **bağlı** nitelik ("yalnız ABS'li modelde") |
| OE / çapraz referans | ProductReference | Orijinal numara ↔ muadil eşleşmesi |
| Fiyat talebi satırı | PriceRequestLine | İstenen üst ürün + sınıf/marka tercihi |
| Uyumluluk | Fitment | TargetType + TargetId — parça her zaman araca bağlanmaz (A-11.4 ✅) |
| Parça sınıfı | PartClass | **Orijinal · OES · Muadil** — fiyattan AYRI eksen |
| Ürün türü | ArticleKind | Part · Fluid · Consumable · Accessory · Service (cam suyu = Fluid) |
| Tedarikçi hesabı | SupplierAccount | B2B / WhatsApp / elle; **firma başına kargo** |
| Fiyat talebi | PriceRequest | N-9 · bir servis işine bağlı |
| Gelen teklif | PriceQuote | Üç kanaldan da aynı tabloya düşer; **fiyatsız olabilir** (`IsStockOnly`) |
| Servis medyası | ServiceMedia | Foto/video + **Phase** (geliş · işlem · teslim) |
| Geliş hasarı | ServiceDamage | Araç gelirken **var olan** hasar (kapalı bölge kümesi) |
| Mal kabul | GoodsReceipt | Gelen parçaların girişi — **tek servise ait**, istenenle karşılaştırılır |
| ~~Ürün girişi~~ → **Mal Kabul** | GoodsReceipt | **Depoya** gelen kutu. Okutulan parça bekleyen işlerde aranır, eşleşirse araca yönlendirilir ve ustaya bildirilir (31.08) |
| Bekleyen parça | PendingPart | Hangi araç/servis hangi ürünü bekliyor |
| Yapılacak iş | WorkItem | Parça satırından **türetilen** iş cümlesi; usta işaretleyerek ilerler |
| Araç notu | VehicleNote | Araca bağlı serbest not. Müşteri notundan ayrı — araç el değiştirse de kalır |
| Mal kabul satırı | GoodsReceiptLine | Tamam · Uyumsuz · Bekliyor · ListedeYok |
| Araç–iletişim bağı | VehicleContact | `IsPrimary` = geçerli iletişim |
| Dükkan ayarı | TenantSetting | Alan görünürlüğü vb. (N-6b) |
> Kural: yeni entity açan ajan, Türkçe UI etiketi + İngilizce sınıf adını buraya ekler (lider onaylar).
| **Ürün kataloğu** | **ProductCatalog** | ⭐ 01.09'da kuruldu. Dükkânın tanıdığı ürünlerin tek listesi: kod, ad, marka, sınıf, **uyum** (hangi araçlara), **stok**, birim fiyat, tedarikçi. Parça seçici, stok listesi ve mal kabul **aynı kayıttan** çizilir. |
| Stok | StockItem / `stok` | Katalog kaydındaki adet. Mal kabul artırır, araca takılınca azalır, iade azaltır. |
| Uyum | Fitment | Ürünün hangi araçlara uyduğu. Boşsa "her araca". |
| **Tabaka** | Sheet | Alttan açılan seçim yüzeyi (ürün seçici). Ekran değil — form yerinde kalır. |
| İndirim | Discount | Müşteri "pahalı" deyince kayıt düzeyinde yapılan indirim. Kalem kalem kırılmaz. |
| Beklenen parça | ExpectedPart | Sipariş edilmiş, mal kabulde beklenen parça. *(eski "istenen parça")* |
| Listede olmayan | UnlistedItem | Siparişte olmayıp gelen ürün. Usta **kabul** eder (stoğa girer) ya da **iptal** eder. |
| Hızlı eylem | QuickAction | Açık iş satırındaki duruma göre tek dokunuşluk ilerletme. |
| **Ödeyen** | **Payer** | Sigortalı işte gerekiyor: Müşteri / Sigorta / Karma. **AÇIK A-SG1.** |
| Eksper | LossAdjuster | Sigortada ikinci onay makamı; kalem kalem onaylar. **AÇIK A-SG3.** |
| Muafiyet | Deductible | Sigortanın ödemediği, müşteriden alınan pay. **AÇIK A-SG5.** |
| Sovtaj | Salvage | Değişen parçanın akıbeti (sigortaya / müşteriye / hurda). **AÇIK A-SG7.** |

## Dördüncü tur terimleri (01.09.2026, işler 19–28)

| Türkçe (UI) | İngilizce (kod/DB) | Not |
|---|---|---|
| **Hızlı işlem** | QuickJob | *"Yapıldı, para alındı, bitti"* — teklif/onay/fiyat çalışması olmadan kapanan kayıt. Servis kaydı **açılmaz**, kapanmış kayıt olarak geçmişe yazılır |
| Alınan tutar | AmountReceived | Hızlı işlemde ustanın müşteriden aldığı para. **İşçilik bundan türetilir** (alınan − parça) |
| **Kesinlik** | Certainty | Parça/kalem satırının üç hâli: `kesin` \| `olabilir` \| `sartli`. Fiyattaki karşılığı: sabit \| belki \| sorulabilir |
| Şartlı kalem | ConditionalLine | Toplama **girmez**; teklifte *"çıkarsa sorulacak"* satırı olur |
| **Birlikte değişen** | Companion part | Bir parçayla teknik olarak birlikte gereken parça (triger ↔ devirdaim) |
| Tavsiye gücü | SuggestionStrength | `hep` (birlikte değişir) \| `sik` (bakılmalı) \| `bazen` (açılmışken) |
| **Rol** | Role | `yetkili` \| `usta` \| `onburo` \| `cirak` |
| **Yetki kapsamı** | PermissionScope | `hepsi` \| `kendi` \| `yok`. Boolean değil — *"usta sorumlu olmadığı işin fiyatını göremez"* ancak kapsamla yazılabilir |
| Fiyat görme | ViewPrice (`fiyatGor`) | Satış fiyatları |
| Alış görme | ViewCost (`alisGor`) | Dükkânın ödediği + kâr |
| **Stoktan iade** | StockReturn | Rafta duran ürünün tedarikçiye geri gönderilmesi. ~~Garanti talebi~~ ayrı sekme olmaktan çıktı |
| **Mal kabul geçmişi** | GoodsReceiptHistory | Bütün mal kabuller; tedarikçi, irsaliye ya da **plaka** ile aranır |
| Motor seçeneği | EngineOption | Marka-model-yıl-yakıta göre daralan liste. Serbest metin **değil** |
| **Tevkifat** | Withholding | Araç bakım-onarımda 7/10. Oran değil, KDV'nin bölünmesi — bkz. `docs/arastirma/KDV.md` |
| Belirlenmiş alıcı | DesignatedBuyer | Tevkifat yükümlüsü alıcı türü |
