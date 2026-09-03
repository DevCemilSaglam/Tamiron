# TAMIRON — NOTLAR (ileride bakılacak fikirler / parking lot)

> Bu dosya **karar değildir**; Cemil'in "zamanı gelince dönüp bakalım" dediği fikirlerin kaydıdır. Her not ileride ya bir **karara (K-)**, bir **açık işe (A-)** ya da bir **akış dalına** dönüşebilir. Belirsizlik yok (K-026): fikir de olsa yazılır. Lider tutar; olgunlaşan not KARARLAR/IS-AKISI'na taşınır.

---

## N-1 · Muhasebe → Paraşüt örnek + bağımsız ürün olabilir
- **Örnek al:** [Paraşüt](https://parasut.com) (Türk ön muhasebe SaaS) — muhasebe modülünün referans/örnek programı.
- **Bağımsızlık:** Muhasebe uygulaması **Tamiron içinde** olacak (Tamirhane + Yedek Parça ortak — K-028) **ama ayrıca da (stand-alone) çalışabilecek** şekilde değerlendirilebilir.
- **Genişleme:** İleride "online Paraşüt benzeri" bağımsız bir muhasebe uygulaması bile yapılabilir.
- İlgili: K-028 (üç uygulama), K-010 (muhasebe/stok ertelendi).

## N-2 · Pazaryeri entegrasyonu → başka bir program örnek + bağımsız ürün olabilir
- **Örnek al:** (isim sonra netleşecek) — pazaryeri entegrasyonu için referans alınacak başka bir program.
- **Bağımsızlık:** Pazaryeri uygulaması da **ayrı bir program** olarak değerlendirilebilir; Tamiron içinde ama ayrıca da çalışabilir.
- **Genel ilke (N-1 + N-2 ortak):** *Muhasebe* ve *Pazaryeri* = Tamiron içinde birer **modül**, AMA gerektiğinde **bağımsız satılabilir/çalışabilir** ürünler. Mimari bunu destekleyecek şekilde modüler tutulmalı.

## N-3 · Ürün alanları yalnız "oto yedek parça" değil (oto elektrik vb.)
- Tamirhaneler için hep **oto yedek parça** düşündük; fakat **oto elektrik** gibi alanlar da var — buralarda yedek parçadan **farklı ürünler** var (**kablo** vb.).
- **Yapılacak:** Bu alanları ve ürün tiplerini **araştır**; ürün yapısını (kodlama/kategoriler) buna göre kur.
- İlgili: **A-11 ürün yapısı araştırması** (ÖNCELİKLİ) — bu araştırmanın kapsamına oto elektrik vb. dahil edilecek. K-002 (katalog hibrit), K-003/K-021.

## N-4 · Güvenlik ayrı bir başlık olarak ele alınmalı
- Güvenlik kısmı ayrı ve bilinçli ele alınmalı — muhtemelen kendi başlığı/dokümanı olacak.
- Kapsam (taslak): kimlik/oturum (K-004), yetki matrisi (K-006) ötesinde; veri güvenliği, tenant izolasyonu (TenantId + global filter), API güvenliği, denetim (audit), gizli veri, ödeme/PII, KVKK vb.
- **Yapılacak:** Backend fazına yaklaşırken ayrı bir `GUVENLIK.md` başlığı açılabilir.

## N-5 · DİNAMİK, dükkan-tanımlı kural motoru (Cemil'in yapı fikri) — lider değerlendirmesi
- **Cemil'in fikri (netleşti 26.08):** *"Kuralları şimdi koyalım demiyorum. Kurallar istenirse **dinamik** konulabilir. Herhangi bir dükkan isterse herhangi bir kuralı **kendisi için** koyabilir."* → Yani **tenant-tanımlı, runtime'da (UI'dan) açılan/kapanan dinamik kural motoru.** Her işleme (araç girişi, teklif vb.) kural takılabilir. Örn: *"02:00–03:00 arası araç girişi yapılmaz."*
- **Lider cevabı: Mantıklı ve değerli — mimaride temiz oturur. Motorun İSKELETİ kodda, KURALLARIN kendisi veride/dükkana ait. İnşa zamanı = backend fazı; ama tasarım fazında hazırlığı yapılabilir.**
  - **Veri:** `Rule` kaydı — `TenantId` + `Trigger/Event` (hangi işlem) + `RuleType` + `Parametreler (JSON)` + `Aktif` + `Öncelik` + denetim alanları + soft-delete (K-027). Multi-tenant + global filter (zaten var) → dükkan bazlı doğal izolasyon.
  - **Motor:** MediatR **pipeline behavior** — işlem (Command) çalışınca o event'e tanımlı **aktif kuralları DB'den çeker, değerlendirir**, ihlalde engeller/uyarır. "Query mantığı gibi" sezgisi doğru.
  - **UI:** dükkanın "Kurallar" ekranı — kural ekle / aç-kapat / parametrele.
  - **Tasarım tercihi (şimdi karar değil, işaret):**
    - **Şablon-bazlı** (sistem hazır kural *tipleri* sunar: çalışma saati kısıtı, zorunlu alan, onay eşiği… dükkan seçip parametreler) → yönetilebilir, çoğu ihtiyacı karşılar. **Önerilen başlangıç.**
    - **Serbest/DSL** (dükkan sıfırdan koşul yazar) → sonsuz esnek ama pahalı/karmaşık; gerçek talep gelince genişlet.
  - **Tasarım fazına düşen HAZIRLIK (şimdi yapılabilir):** İş akışını çıkarırken her dalda **"kural takılabilir nokta (trigger)"** olanları işaretle. Böylece motor kurulunca **tetik listesi hazır** olur. Akış ve motor birbirini besler.
  - **Neden inşa şimdi değil:** Çalışan bir Command/pipeline gerekir (backend yok). Ayrıca boş bir motoru tek kurala göre tasarlamak = erken soyutlama. Önce **trigger noktalarını + kural adaylarını topla.**
  - → İleride **K-kararı** olacak (olgunlaşınca KARARLAR.md'ye). Şimdilik yön/vizyon burada.
  - **Toplanan kural adayları + trigger noktaları (biriktir):**
    - "02:00–03:00 arası araç girişi yapılmaz." → trigger: **Araç Giriş (servis kaydı açılışı)**.
    - "İşleme alınan araçta **video zorunlu**." → trigger: **servis kaydı → "işlem başladı" durumuna geçiş** (26.08).
    - "Usta **onaysız** en fazla ___ TL/kalem **ek arıza** ekleyebilir (üstü müşteri onayı ister)." → trigger: **Tamirat · ek arıza** (26.08).
    - "**Hazır** demeden önce **test sürüşü / foto zorunlu**." → trigger: **Ödeme & Teslim · 'araç hazır' işaretleme** (29.08).
    - "**___ TL üstü borçla teslim yönetici onayı ister**" / "**yeni müşteriye borçla teslim yok**." → trigger: **Ödeme & Teslim · ödenmeden teslim** (29.08).
    - *(yeni kurallar/trigger'lar buraya eklenecek)*

## N-6 · Araç kartı birincil tanımlayıcı — dükkan ayarı (görünüm tercihi, kural değil)
- Araç kartında **Plaka** mı yoksa **Marka-model** mi öne çıksın? → **Dükkan ayarı.** Aynı tip araca çok bakan (marka servisi) → plaka ayırt edici; az/çeşitli araç (4-5) → marka-model ayırt edici.
- **Varsayılan: plaka.** İleride sistem dükkanın araç çeşitliliğine bakıp **otomatik önerebilir.**
- N-5 ile aynı felsefe (dükkana göre esneklik) ama bu bir **görünüm tercihi**, kural motoru değil. İlgili: IS-AKISI "ARAÇ DETAYI ekranı".

### ➕ N-6b · ALAN GÖRÜNÜRLÜĞÜ — dükkan ayarı (29.08.2026)
- **Cemil:** *"Her firma gelen araçla ilgili çok fazla detay almak istemeyebilir, örneğin tarihler. Bunu ayar olarak yapabilir miyiz?"*
- **Karar:** evet — **opsiyonel alan grupları dükkan tarafından açılıp kapatılır.** Prototipte üç grup: **kontrol tarihleri** · **anahtar/radyo kodu** · **motor no**.
- ⚠️ **Kural:** kapalı bir alan **HEM formda HEM araç detayında** gizlenir. Girilmeyen alanın okuma yeri de olmamalı — yoksa detayda hep boş satir durur.
- **Nerede tutulur:** gerçekte **tenant kaydında** (dükkan ayarı). Prototipte `localStorage`.
- **Sınır:** bu bir **görünüm tercihi** — N-5'teki kural motoru DEĞİL. Zorunluluk/onay/eşik gibi şeyler kural motoruna gider; "bu alanı hiç kullanmıyorum" buraya.
- **Genişleme (ileride):** hangi grupların açılabileceği listesi büyüyecek (araç cinsi, emanet, yakıt seviyesi…). Ayarlar drawer'da durmaz, **kendi ekranına** taşınır. Zorunlu alanlar (plaka, şasi no) kapatılamaz.

---

## N-7 · İnceleme/ekspertiz ücreti (ileride)
- Şu an **İnceleme ücretsiz** (Akış 2 kararı). İleride gerekirse **inceleme/ekspertiz ücreti** yolu değerlendirilebilir:
  - Muhtemelen **dükkan kuralı** (N-5) olarak — dükkan isterse inceleme ücreti tanımlar.
  - İşleme dönünce **mahsup** edilebilir (ücret, çıkan işin bedelinden düşülür).
- İlgili: IS-AKISI "Akış 2 · İnceleme".

## N-8 · İşçilik hesaplama esnek (dükkan/teklif ayarı)
- Teklifte işçilik **iki türlü** olabilir: **ayrı kalem** (sabit tutar veya saat×ücret) **veya** **parça üzerine yüzde**.
- Muhtemelen **dükkan ayarı** (varsayılan) + teklif bazında değiştirilebilir. İlgili: IS-AKISI "Akış 3 · Teklif".

## N-9 · Parça Fiyat Toplama = ayrı akış/iş (parçacı + B2B karşılaştırma)
- Teklifteki parça fiyatı **başlı başına bir iş**: parçacıya sorma + **B2B / pazaryeri karşılaştırma** (en uygun fiyat/tedarik).
- Kendi başına bir akış olarak detaylanacak. Bağlantılar: **A-11 ürün yapısı araştırması** (ÖNCELİKLİ), **K-028 Yedek Parça asistanı** (WhatsApp fiyat görseli→liste, B2B fiyat araştırma), ana menü **"Fiyatlandırma"**, K-002 hibrit katalog.
- Teklif akışında şimdilik tek düğüm ("Parça Fiyat Toplama — ayrı akış").
- **➕ (30.08) Oturum hazırlığı yapıldı:** `docs/arastirma/N-9-PARCA-FIYAT-HAZIRLIK.md`. Akışın **üç yapısal özelliği** çıkarıldı — **asenkron** (parçacı anında cevap vermez; "bekleyen iş" kavramı ilk kez burada doğuyor), **iki uçlu** (aynı akış K-028 Yedek Parça uygulamasının karşı yüzü), **karışık dünya** (tedarikçilerin çoğu Tamiron kullanmayacak — elle giriş yolu kalıcı olmak zorunda). Üç iskelet sunuldu (elle / RFQ / RFQ+köprüler); **parça sınıfı (orijinal-OES-muadil) fiyattan AYRI eksen** olarak işaretlendi. ⚠️ **Sıralama uyarısı: A-11.1 kararı verilmeden bu oturum tam verimli geçmez.**

## N-10 · Bakım — kontrol listesi & sonraki bakım hatırlatması
- **Kontrol listesi = dükkan tanımlı** (fren, lastik, sıvılar…). Dükkan konfigürasyonu; her dükkan kendi listesini kurar/düzenler.
- **Sonraki bakım hatırlatması:** şimdilik yalnız **kayıt** (km/tarih). İleride **otomatik bildirim** (SMS/WhatsApp — zamanı/km'si gelince) eklenebilir. İlgili: A-05 kanallar, bireysel app.
- **➕ (29.08) Hesap artık mümkün:** servis başına **geliş km** alınmaya başlandı → **km geçmişi** oluşuyor. *Günlük ortalama km = (son km − önceki km) ÷ geçen gün* → bakım periyoduna bölününce **sonraki bakım TARİHİ tahmini** çıkar. Aynı mantık **kontrol tarihleri** (muayene/sigorta) ile birleşince "yaklaşanlar" listesini besler.
- İlgili: IS-AKISI "Akış 5 · Bakım".

## N-11 · Değerlendirme / puanlama — ERTELENDİ (29.08.2026)
- Teslim sonrası **müşteri değerlendirmesi / puanlama** akışın parçası olarak **şimdi tasarlanmıyor** (Cemil kararı, 29.08). Akış 6'da yalnız kesikli düğüm olarak duruyor.
- Zamanı gelince açılacak sorular: puanlama **kanalı** (app / SMS linki / WhatsApp — A-05), **neyi** puanlar (usta / dükkan / iş), sonucun **kime** görünür olduğu (dükkan içi mi, dışarı mı), kötü puanın **iş akışına etkisi** (uyarı/geri dönüş).
- İlgili: IS-AKISI **Akış 6 · Ödeme & Teslim**, A-05 kanallar, bireysel app.

## N-12 · Rakip program incelemesi — OtoMixSQL (kaynak kaydı)
- **Ne:** `otoservisprogram.com` üzerinden indirilen **7 tanıtım videosu** + 2 ekran görüntüsünün kare kare incelenmesinden çıkarılmış **fonksiyonel şartname**. Gerçekten kullanılan bir programda hangi alanların toplandığını gösteriyor.
- **Nerede:** Şartname `D:\GitRepos\Otosel\Project\otosel\docs\OTOSEL-SARTNAME.md` (17 bölüm) · videolar `C:\Users\Cemil\Downloads\` (OtoMixSQL — Araç Kabul / Araç Kart Tanımlama / Araç Teslim ve Faturalandırma / Servis Geçmiş Kayıtları / Kullanıcı ve Yetki + Gelecek Yağ Bakımı Takibi + Ön Tanıtım).
- ⚠️ **Başka bir projeye ait belge.** Tamiron kararları kendi ayakları üstünde durur (K-026); oradan **doğrudan içerik taşınmaz**, yalnız **karşılaştırma kaynağı** olarak kullanılır ve alınan her şey Tamiron'da yeniden karara bağlanır.
- **29.08 karşılaştırmasından alınanlar (A kovası, uygulandı):** kontrol tarihleri · anahtar/radyo kodu · e-posta · geliş km · yakıt seviyesi · emanet · iki ayrı serbest not.
- **Bekleyenler (B kovası — veri modeli):** araç↔cari bağı · km geçmişi · günlük km → sonraki bakım hesabı.
- **Bekleyenler (C kovası):** ~~ana ekran **özet panel sayacı**~~ **✅ UYGULANDI (30.08)** — ana ekranda üç sayaç (onarımda/teslime hazır/parça bekliyor) + bekleyen fiyat ve muayene şeridi + **Açık İşler** listesi. ~~sorumlu personel~~ **✅ (31.08)** · ~~tahmini bitiş tarihi~~ **✅ (31.08)** · ~~hasar dosyası~~ **✅ (31.08, A-08)**. Kalan: sigorta dosyası · fatura iskonto-tevkifat kademeleri.

## N-13 · Sesle arama (plaka) — analiz · 30.08.2026
> Cemil: *"Ses ile arama vb yapacağız… Plakayı sesle arattığında **hemen** sonuç görmek isterler."*
> ⚠️ Bu not **analiz**dir; kararlar aşağıda **açık** işaretli. Karara bağlanınca A-06 güncellenir.

### Kilit içgörü — bu bir "ses tanıma" işi değil, **kısıtlı alanda eşleştirme** işi
Genel amaçlı STT `34 ABC 123` ifadesini güvenilir yazmaz ("otuz dört be ce de yüz yirmi üç"). Ama üç kısıtımız var ve doğruluğu asıl bunlar taşır:
1. **Aranan şey plaka** — dilbilgisi çok dar: il kodu **01–81** + **1–3 harf** + **2–5 rakam**; harf kümesi yalnız **23 harf** (Q/W/X ve Türkçeye özgü harfler plakada yok — prototipteki `plateNormalize` zaten katlıyor).
2. **Aranan şey KAYITLI** — serbest metin değil, dükkanın araç listesi = **kapalı küme**.
3. **Küme küçük** — dükkan başına ~500–5.000 araç.
> Ham STT tek başına ~%70; kalıp + kapalı küme + fonetik eşleştirme ile **%95+** hedeflenebilir. Doğruluğu getiren STT motoru değil, **arkasındaki katman**.

### Boru hattı
`[bas-konuş] → STT (tr-TR) → NORMALLEŞTİRİCİ → plaka kalıbına oturt (aday üret) → FONETİK BULANIK EŞLEŞTİRME (cihazdaki plaka dizini) → sıralı sonuç`

**Normalleştirici (Türkçeye özel):**
- Sayı sözcüğü → rakam. İl kodu **iki türlü** söylenir: *"otuz dört"* ve *"üç dört"* → **ikisi de aday üretir**.
- Harf okunuşu → harf: be→B · ce→C · de→D · fe→F · ge→G · he→H · je→J · ke/ka→K · le→L · me→M · ne→N · pe→P · re→R · se→S · te→T · ve→V · ye→Y · ze→Z
- Kod sözcüğü → ilk harf: *"Ankara'nın A'sı"* → A
- Türkçeye özgü harfler İngiliz alfabesine katlanır (çe→C, şe→S) — plakada zaten yoklar.

**Fonetik bulanık eşleştirme** — düz Levenshtein **değil**, ses-benzerlik ağırlıklı mesafe: karışan çiftler ucuz (B↔P↔V · C↔Z↔S · D↔T · M↔N · K↔G ≈ 0,3), diğer her şey 1,0. En düşük mesafeli ilk 5 kayıt gösterilir.

### "Hemen sonuç" — gecikme bütçesi (30.08 netleştirildi)
Ölçüm birimi: **parmağını kaldırdığı an → sonuç ekranda.**

| Adım | İyi şebeke | Kötü şebeke | Cihazda STT (MAUI) |
|---|---|---|---|
| STT son sonucu döndürür | 300–800 ms | 1,5–2,5 sn | 200–500 ms |
| Normalleştir + eşleştir (5.000 araç, **cihazda**) | < 10 ms | < 10 ms | < 10 ms |
| Çizim | ~50 ms | ~50 ms | ~50 ms |
| **Toplam** | **0,4–0,9 sn** | **1,5–2,6 sn** | **0,3–0,6 sn** |

**Darboğaz tek yerde: STT'nin cevabı.** Eşleştirme ve çizim ölçülemeyecek kadar küçük — çünkü arama cihazda, sunucuya hiç gitmiyor.

⚠️ Rakamlar **sektör tipikleri üzerinden tahmin, ölçüm değil.** Kesinleştirmek ~1 günlük ölçüm işi. **Kötü şebeke sütunu (2,6 sn) "hemen" hissini bozar;** o riski ortadan kaldıran tek şey **cihaz üstü STT** — PWA'da mümkün değil, MAUI tarafı.

**Hedefi tutan iki tasarım kararı:**
1. **Bas-BIRAK** (bas-konuş değil). Otomatik "konuşma bitti" algısı **sessizlik bekler**; tamirhane gürültüsünde o sessizlik hiç gelmez ve 1–2 sn boşa gider. Parmak kalkınca dinleme biter → bekleme sıfırlanır. **Gürültülü ortamda en çok kazandıran tek karar.**
2. **Ara sonuçlarla canlı daraltma.** Kullanıcı *"otuz dört…"* derken liste daralır; parmağını kaldırdığında sonucu **zaten görüyor** olur → **algılanan gecikme ~0**, tablodaki rakam yalnız doğrulamanın oturmasıdır.

**Karşılaştırma:** aynı plakayı elle yazmak mobil klavyede **6–10 sn** (yağlı elle daha fazla). Sesle toplam **~3 sn** (2 sn konuşma + gecikme) → **2–3 kat hızlı.**

**İki şart bu hedefi tutar:**
- **Eşleştirme CİHAZDA.** Sunucuya gitse +200–800 ms (mobil şebeke) ve internet kesilince ölür. Dükkanın **plaka dizini** (plaka + araç id + marka/model ≈ 40 B/araç → 5.000 araç ≈ 200 KB) cihazda önbellekte, arka planda tazelenir. **Detay sunucudan, arama yerel.** *(K-001 ile uyumlu; yeni olan: dizinin istemcide olması.)*
- **Ara sonuçlarla canlı daraltma.** Kullanıcı *"otuz dört…"* derken liste zaten daralır — "hemen" hissini yaratan asıl şey budur.

### Ortam — tamirhane gürültülü (85–100 dB)
- **Bas-konuş** şart; sürekli dinleme/uyandırma sözü ("Hey Tamiron") gürültüde yanlış tetiklenir ve pil yer → **önerilmiyor**.
- Eller yağlı sorunu gerçek: çözüm uyandırma sözü değil, **büyük dokunma hedefi** + kilit ekranı kısayolu.

### STT motoru — iki katman
| | Web Speech API (PWA) | Cihaz native (MAUI) | Sunucu STT (Whisper/Azure) |
|---|---|---|---|
| Maliyet | ücretsiz | ücretsiz | ~0,006–0,02 $/dk |
| Gecikme | 300–800 ms | 200–500 ms | 800–2.000 ms |
| Çevrimdışı | hayır | **evet** (dil paketi) | hayır |
| iOS | zayıf | iyi | iyi |

**Ayrım nettir ve önemlidir:** **arama = cihazda STT** (kısa, dar, hızlı olmalı) · **dikte/ayrıştırma = sunucuda AI** (uzun serbest not, arıza anlatımı, parça listesi — orada doğruluk gecikmeden önemli; bu zaten **A-06**). İki farklı iş, iki farklı motor.

### Süre tahmini
| İş | Süre |
|---|---|
| Prototipte çalışan sesle plaka arama (Web Speech + normalleştirici + fonetik eşleştirme + ekran) | **1 gün** |
| Türkçe test seti (~200 kayıtlı söyleyiş) + doğruluk ölçümü + eşik ayarı | **1 gün** |
| Üründe: izinler, hata halleri, çevrimdışı dizin, MAUI native STT | **1–2 hafta** |
| Gerçek dükkanda gürültü saha testi | 2–3 gün |
> **Prototip demo ~2 gün · ürün kalitesi ~3 hafta.**

### Kapsam (vb)
Aynı boru hattı hedef dizini değiştirerek kişi adı / telefon / filtre komutuna ("bugün teslim") uzar. **v1'de yalnız plaka** — dar tut, doğruluğu ölç, sonra genişlet.

### Zamanlama
**Cemil kararı (30.08): sesli arama ŞİMDİ yapılmıyor.** Bu not fizibilite/gecikme cevabı olarak duruyor; sırası gelince aşağıdaki açık kararlarla başlanır.

### AÇIK — Cemil kararı bekleyen
> ✅ **30.08 tutarlılık denetimi: 1 ve 3 numaralı sorular ZATEN CEVAPLANMIŞ** — prototipte ana ekranda **her menü kartının kendi mikrofonu** var (`micAracGiris` · `micListe('araclar')` · `micListe('kisiler')`). Yani giriş yeri **kart bağlamı**, kapsam da **plaka + araç + kişi**. Bu iki soruyu gereksiz sormuşum; prototip kararı taşıyordu.

1. ~~Sesli arama nereden başlar?~~ → **kapandı:** her menü kartının kendi mikrofonu (prototipte mevcut).
2. **Tek eşleşmede** doğrudan araç detayına gitsin mi, yoksa her zaman liste mi? *(hâlâ açık)*
3. ~~v1 kapsamı yalnız plaka mı?~~ → **kapandı:** prototip plaka + araç listesi + kişi listesi için mikrofon içeriyor.
4. **Çevrimdışı** çalışması şart mı (dükkanda internet kesilir mi)? *(hâlâ açık)*

İlgili: **A-06** (AI sesli kayıt — dikte tarafı), K-001 (arama soyutlaması), K-014 (web↔mobil eşitliği), N-5 (kural motoru).

## N-14 · A-11 araştırması TAMAM — bulgular `docs/arastirma/A-11-URUN-YAPISI.md` · 30.08.2026
- **Kaynak:** TecDoc Data Format 2.6 resmî spesifikasyonu (179 sayfa) satır satır incelendi + ACES/PIES (Auto Care Association) + pazaryeri/Sahibinden karşılaştırması.
- **Ana bulgu:** TecDoc ve ACES/PIES birbirinden bağımsız geliştikleri hâlde **aynı dört katmana** varmış — araç tipi · jenerik parça tipi · somut ürün · tipli öznitelik. Yapı keyfî değil, problemin doğasından geliyor.
- **K-003'ün cevabı çıktı:** parça araca değil **"bağlantı hedefi"ne** bağlanır ve hedefin bir TİPİ vardır (araç / motor / aks / üretici). Yani **araç ürün değildir**; ayrı `VehicleType` + `VehicleInstance` önerilir.
- **Öznitelik:** ham EAV değil — **tipli** (alfanümerik/sayısal/tarih/liste/değersiz) + **jenerik tip başına şablon** (zorunlu/önerilen) + `K` tipinde kapalı değer listesi + kriterler arası kısıt. Ayrıca **ürün özniteliği ≠ bağlantı özniteliği** (balatanın kalınlığı üründe sabit; "yalnız ABS'li modelde" bağlantının niteliği).
- **N-3 çözüldü:** oto elektrik/kablo için ayrı mimari gerekmiyor — kablo da bir jenerik tiptir, farkı **şablon farkı** olarak çözülür. Araçtan bağımsız ürünler "universal" bayrağıyla aynı yapıda durur.
- **Tamiron'a özel ayrım:** **araç ÖRNEĞİ ≠ araç TİPİ.** Örnek = servise gelen o araba (doğal anahtar VIN, D-1); tip = "Mégane III 1.5 dCi 2012–2016" (parça uyumluluğu buna bağlanır). Prototipteki ruhsat alanları aslında örneği tipe bağlayan alanlar.
- **AÇIK (A-11.1…A-11.5):** hangi seçenek (B öneriliyor) · K-003 onayı · **TecDoc verisi satın alınacak mı** (ticari karar, maliyet/zaman farkı büyük) · ilk sürümde `TargetType` olacak mı · araç tipi kataloğunun kaynağı.

## Sürüm notları
- v1 · 26.08.2026 — N-1…N-5 eklendi (Cemil'in notları + kural-motoru değerlendirmesi).
- v2 · 26.08.2026 — N-6 (araç kartı gösterim ayarı), N-7 (inceleme ücreti ileride); N-5'e video kural adayı eklendi.
- v3 · 26.08.2026 — N-8 (işçilik esnek), N-9 (parça fiyat toplama ayrı akış).
- v4 · 26.08.2026 — N-5'e ek arıza eşiği kuralı; N-10 (bakım kontrol listesi & hatırlatma).
- v7 · 29.08.2026 — **N-6b (alan görünürlüğü dükkan ayarı)**.
- v6 · 29.08.2026 — **N-12 (OtoMixSQL inceleme kaynağı + üç kova)**; N-10'a *günlük km → sonraki bakım hesabı* eklendi.
- v5 · 29.08.2026 — **N-11 (değerlendirme/puanlama ertelendi)**; N-5'e Akış 6'dan iki kural adayı ("hazır" öncesi test/foto zorunlu · borçla teslim eşiği/yönetici onayı).
- v8 · 30.08.2026 — **N-13 (sesle plaka arama analizi: kısıtlı-alan eşleştirme, gecikme bütçesi, iki katmanlı STT, süre tahmini + 4 açık karar)**.
- v9 · 30.08.2026 — N-13 gecikme bütçesi netleştirildi (üç senaryo: iyi/kötü şebeke + cihaz üstü STT), **bas-BIRAK** kararı, elle yazmayla karşılaştırma; sesli arama **şimdilik ertelendi**.
- v10 · 30.08.2026 — **N-14 (A-11 ürün yapısı araştırması tamamlandı)**; bulgular ve üç seçenekli öneri `docs/arastirma/A-11-URUN-YAPISI.md`, beş açık karar işaretlendi.
- v11 · 30.08.2026 — Tutarlılık denetimi: **N-13'ün 1. ve 3. açık soruları kapandı** (prototip zaten cevaplıyor); IS-AKISI ana ekran mikrofon tarifi gerçeğe hizalandı.
- v12 · 30.08.2026 — **N-9 oturum hazırlığı** (`arastirma/N-9-PARCA-FIYAT-HAZIRLIK.md`): üç yapısal özellik, üç iskelet, altı soruluk oturum sırası; A-11.1 önkoşulu işaretlendi.
- v13 · 30.08.2026 — N-12 C kovasından **ana ekran özet paneli uygulandı** (Açık İşler listesiyle birlikte).
