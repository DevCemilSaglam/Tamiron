# TAMIRON — TASARIM SİSTEMİ (TASARIM-SISTEMI.md) · v31 · 01.09.2026

> Tamiron'un görsel dili. **Tek kaynak: `tokens.css`** (renk/tip/ölçü). Bu belge o token'ların nasıl kullanılacağını ve bileşen desenlerini tanımlar. Kural: ekran/bileşen kodunda **ham hex/px-renk YOK** — yalnız token. (K-022 tasarım-önce, K-023 tek kaynak.)

---

## 1. MARKA
- **His:** şık, sade, albenili ama abartısız. Işık tema = güvenilir/kurumsal; koyu tema = modern/teknoloji. Tek tutarlı hue (tema değişince marka kaymaz).
- **Renk hikâyesi:** birincil **mavi** (güven + aksiyon), aksan **teal** (modern/AI), **turuncu** üçüncü vurgu (sıcak, mavinin tamamlayıcısı).
- **Logo:** "TAMIRON" ağır kondens (Anton) uppercase; "O" yerine **altıgen somun** — somun **TEAL** tonlarında (`#269689` / `#22766E` / `#5AA79F`). Yerleşimler: inverted (açık zemin), tek-renk (koyu, pop yok), app icon, wordmark.
- ℹ️ **(29.08 düzeltme)** Eski belge "somun turuncu, turuncu yalnız logoda" diyordu; **logoda turuncu HİÇ YOK** (somun teal). Kural kaldırıldı: **turuncu (`--highlight`) serbest bir arayüz vurgu rengidir** — ör. Teklif tip şeridi (§2). Token `--nut` (somun) → **`--highlight`** olarak yeniden adlandırıldı.

## 2. RENK (token → rol)
Tümü `tokens.css`'ten. Değiştirmek için oradaki "MARKA PALETİ" hexleri.
| Token | Rol |
|---|---|
| `--bg` / `--surface` / `--surface-2` | zemin / kart / ikincil yüzey |
| `--text` / `--muted` / `--faint` | metin / ikincil / en soluk |
| `--primary` / `--primary-strong` / `--on-primary` | birincil aksiyon (mavi) / hover / üzeri metin |
| `--accent` / `--accent-soft` / `--on-accent` | aksan (teal) / yumuşak zemin / üzeri metin |
| `--highlight` | üçüncü vurgu (turuncu) |
| `--line` | kenarlık/ayraç |
| `--rail` / `--rail-text` / `--nav-on-bg` / `--nav-on-fg` | sol menü (zemine karışır) / aktif öğe yumuşak pill |
| `--crit` `--warn` `--ok` `--info` (+ `-bg`) | **semantik** durum — markadan AYRI (aşağı) |
| `--tip-bakim` `--tip-tamirat` `--tip-inceleme` `--tip-teklif` | **servis kaydı tipi** — ayırt eder, durum bildirmez (aşağı) |
| `--shadow` | kart gölgesi |

**Semantik ≠ marka:** durum renkleri (crit/warn/ok) aksan rengi SAYILMAZ; yalnız durum bildirir.

**TİP RENKLERİ ≠ durum renkleri (29.08).** Servis kaydı tipini ayırt etmek için dört token; **yeni hex eklenmez**, hepsi mevcut paletten türer:

| Token | Türer | Neden |
|---|---|---|
| `--tip-bakim` | `--accent` (teal) | periyodik / rutin |
| `--tip-tamirat` | `--primary` (mavi) | asıl onarım işi |
| `--tip-inceleme` | `--warn` (amber) | tespit / dikkat |
| `--tip-teklif` | `--highlight` (turuncu) | fiyat / satış |

- **Kural:** ŞerİT TİPİ söyler, ROZET DURUMU söyler — ikisi ayrı okunur. Tip rengi bir kalemi "iyi/kötü" yapmaz.
- **Turuncu serbest** (29.08): eski "yalnız logoda" kısıtı kaldırıldı — logoda zaten turuncu yok (§1). `--highlight` bir vurgu rengi olarak kullanılır; yığılmaması için sayfa başına **tek belirgin turuncu** yeter.

## 3. TİPOGRAFİ (token)
- `--font-display` **Sora** — başlıklar (h1/h2), toplam tutar.
- `--font-body` **Instrument Sans** — metin, arayüz, butonlar.
- `--font-mono` **IBM Plex Mono** — **plaka, tutar, km, OEM/kod** (tabular-nums).
- `--font-logo` **Anton** — yalnız wordmark.
- Kurallar: başlıklara `text-wrap:balance`; metin ~65ch; uppercase etiketlere hafif letter-spacing; rakam kolonlarına `font-variant-numeric:tabular-nums`.

### 3.1 İKON SETİ — **Lucide** (omurga prototipinden, 29.08)
- **Tek set: Lucide.** Emoji ikon olarak KULLANILMAZ (platformlar arası tutarsız, tema-duyarsız).
- **Inline SVG sprite** (`<symbol>` + `<use>`), **`currentColor`** ile boyanır → token rengine otomatik uyar. Varsayılan 24px (`ico-sm` 16px).
- **Dış bağımlılık yok** (CDN yok); ikonlar uygulamaya gömülü.
- Her ikonun yanında **metin veya `aria-label`** bulunur (renk/ikon tek başına anlam taşımaz — §7).

## 4. ÖLÇÜ
`--radius-sm 8` / `--radius-md 12` / `--radius-lg 16` / `--radius-pill 999`. Aralık: flex/grid + `gap` (per-element margin değil). Geniş içerik (tablo/kod) kendi `overflow-x:auto` kabında.

## 5. BİLEŞEN DESENLERİ (referans: kimlik mockup'ı)
- **Sol menü (rail):** zemine karışır (`--rail` ≈ `--bg`), sağda ince `--line`; logo üstte; bölüm etiketi ("Menü") uppercase soluk; nav öğesi ikon+etiket; **aktif = yumuşak pill** (`--nav-on-bg` + `--nav-on-fg`, sert dolu bar DEĞİL); hover hafif.
- **Kart:** `--surface`, `--line` kenarlık, `--radius-lg`, `--shadow`. Zemin (`--bg`) üstünde yüzer.
- **Buton:** birincil = `--primary`/`--on-primary`; ghost = şeffaf + `--line`; aksan ikon = `--accent`. Belirgin `:focus-visible` (2px `--primary`).
- **Durum pill:** `--ok-bg`/`--ok` gibi (ör. "Teklif hazır"). **Her durumun kendi rengi olur** — nötr gri (`muted`) bir SONUÇ için kullanılmaz: gri "durum yok" gibi okunur. Dört sonuç rengi: `ok` tamamlandı · `warn` ertelendi · **`info` bilgi verildi** (mavi, nötr sonuç) · **`crit` reddedildi**. `muted` yalnız gerçekten durumsuz etiketlerde (ör. "Ziyaret · bilgi" öncü rozeti).
- **Öncelik etiketi (4 kademe):** Zorunlu=`crit`, Yapılabilir=`warn`, İyi olur=`ok`, Yapılmayabilir=`surface-2`/`muted` (soluk). Form + renkle kodlanır.
- **PLAKA — TEK BİLEŞEN, HER YERDE AYNI.** Gerçek TR plakasının dili: **`--primary` çerçeve + solda dolu mavi “TR” bloğu + mono numara** (`--surface` zemin, `--text` metin). Plaka nerede görünüyorsa bu bileşenle çizilir — düz mono metin ya da farklı bir rozet **kullanılmaz**.
  - **Üç boyut:** `.plate-sm` (liste satırı, etiket, eyebrow içi) · varsayılan (metin içi) · `.plate-lg` (ekran başlığı). Yalnız ölçü değişir; **renk ve yapı sabittir** — plaka her yerde aynı şeye benzer.
  - Plaka **giriş alanı** da aynı dili kullanır (TR bloğu + mavi çerçeve), böylece yazarken görülen şey kaydedildikten sonra görülenle aynıdır.
- **Tutar/km:** mono + tabular-nums.

### 5.1 UYGULAMA KABUĞU (mobil — omurga prototipinden, 29.08)
> Her ekranın oturduğu ortak çerçeve. Kod fazında `Ui.Core`'a girer; ekranlar yalnız içeriği doldurur.

- **Sabit header (sticky top):** sol = **hamburger** (üstünde bildirim sayı rozeti) + **logo** (buton yüksekliğiyle hizalı, temaya duyarlı, tıklayınca **ana sayfaya reset**); sağ = **geri / ileri**.
  - **Geri/ileri her zaman GÖRÜNÜR;** işlevsizken **disabled + silik** (`pointer-events:none`, düşük opaklık). Gizlenmez → düzen zıplamaz.
- **Sabit ince footer (sticky bottom).** header + ekran + footer = tam yükseklik; yalnız ekran gövdesi kayar.
- **Drawer (sağdan):** Bildirimler → Bölümler → Kullanıcı → Ayarlar sırası. **Arka ekran SABİT kalır** (kaymaz/ölçeklenmez). Kapanış: [X] · overlay · **ESC**.
- **Tema toggle header'da DEĞİL** → drawer → **Ayarlar**. Seçim `localStorage`'da (§6).
- **Sesli giriş her yerden:** menü satırlarının her birinde **kendi mikrofon butonu** (ana butonla birleşik görünür, bağımsız tıklanır). Tek bir "AI ekranı" yoktur.
- **Ana menü (mobil):** tam genişlik **dikey satırlar** (K-024); birincil eylem (Araç Giriş) `--primary` ile öne çıkar.

### 5.2 EK BİLEŞEN DESENLERİ (omurga prototipinden, 29.08)
- **Bildirim listesi (drawer içinde):** başlık **sticky**; **sağa swipe = sil** + her satırda erişilebilir `[x]`; "Temizle" hepsini siler; **liste boşsa bölüm ve rozet tamamen gizlenir.**
- **Zaman tüneli (timeline):** solda tip ikonu, sağda dikey yığın. **Özet bağlamda (ör. "Son 3") yalnız İŞİN TİPİ** (*Bakım · İnceleme · Tamirat · Teklif*) + **alt şerit: solda DURUM rozeti, sağda TARİH** (mono, nowrap). Kayıt numarası ve yapılan işler özete girmez — **detay ayrı ekrana** (Geçmiş). **Tutar/fiyat GÖSTERİLMEZ** (tutar teklif/muhasebe ekranlarının işi).
- **Satır şerİdİ = TİP.** Her timeline satırında **3px sol kenar şeridi** servis kaydı tipini verir (`--tip-*`, §2); tip ikonu aynı renkte. **Zemin boyanmaz.** ⚠️ **Kural:** bir satırın zeminini semantik yumuşak renkle (`--warn-bg` vb.) boyarsan **aynı ailenin rozeti görünmez olur** — rozetin kendi zemini satır zeminiyle çakışır. Nötr zemin + şerit → rozet kontrastını korur.
- **Uyarı şeridi (notice):** ikon + metin, üç ton — `ok` (bilgilendirme/başarı) · `warn` (eksik bilgi) · `accent` (dükkan kuralı/yönlendirme). Akışı kesen ayrı adım yerine **ekran içinde** uyarı verir.
- **Durum geçiş şeridi:** `pill → ok → pill` (ör. *Servis kaydı · bilgi* → **İşleme döndü**) — durum değişimini görünür kılar.
- **TELEFON — TEK BİLEŞEN, HER YERDE AYNI.** İkon + **mono + `--accent`**, **zeminsiz**, `tel:` linki (K-020 "tıklanabilir plaka/kişi" ruhu). İki bağlam:
  - **`.tel-link`** — blok/liste bağlamı (kişi satırı, iletişim kartı): 14.5px, ikonlu, **44px dokunma hedefi** (`min-height`).
  - **`.tel-inline`** — cümle içinde geçen telefon: yalnız mono + aksan, **yükseklik zorlanmaz** (metin akışını bozmaz).
  - E-posta aynı dili gövde fontuyla kullanır (`.mail-link`).
- **Maskeli alan (plaka, telefon):** canlı biçimleme; yazarken de sesli girişte de **aynı biçimleyici**. **Boşken örnek/kural metni yazılmaz** — beklenen biçim **placeholder'daki maske şablonuyla** anlatılır (ör. `05__ ___ __ __`). İpucu satırı yalnız **yazılmaya başlandıktan sonra** konuşur (geçerli / eksik); yeri `min-height` ile sabit tutulur ki düzen zıplamasın.
- **Seçim çipleri:** az seçenekli alanlarda (ör. yakıt: 5 seçenek) açılır liste yerine **pill çipler**; seçili çip `--accent` dolu, diğerleri `--surface-2`. `role="radiogroup"` + `aria-checked`. Mobil-öncelikli (K-024): dokunma hedefi büyük, seçenekler tek bakışta görünür.
- ⚠️ **NATIVE `<select>`, `<datalist>` VE `<input type="date">` KULLANILMAZ.** Açılır listesini işletim sistemi çizer — **tema uygulanmaz** (koyu temada beyaz liste + sistem mavisi seçim), scroll'da da düzelmez. Yerine **özel seçim bileşeni**: tetik düğmesi input görünümünde + sayfa içinde `position:absolute` menü, token'larla boyanır (`--surface` zemin, `--line` kenarlık, `--shadow`), seçili seçenek `--accent-soft` + tık işareti, `max-height` + kendi scroll'u. Kapanış: dışarı tıklama · **ESC** · başka menü açılınca. `role="listbox"` / `role="option"` + `aria-expanded`.
  - **`<datalist>` de aynı kusurda:** öneri listesini yine işletim sistemi çizer **ve** alan doluyken kendini o değere göre filtreler — seçim yapıldıktan sonra **başka bir değer seçilemez** (önce silmek gerekir). Uzun listede autocomplete gerekiyorsa özel seçimin **menü içi arama kutusu** varyantı kullanılır (sticky arama satırı + "sonuç yok" satırı).
  - **Bağımlı seçim:** üst alan değişince alt alanın menüsü yeniden doldurulur **ve seçimi temizlenir** (geçersiz eşleşme kalmasın); tetik düğmesi "önce marka" gibi ne beklediğini söyler.
  - Açılışta **seçili seçenek görünür** hale getirilir (`scrollIntoView`) — uzun listede aşağıdaysa aranmaz.
  - **Tarih de aynı kusurda:** native takvimi işletim sistemi çizer. Yerine **`GG.AA.YYYY` maskeli metin alanı** — klavyeden yazmak takvimden seçmekten zaten hızlıdır. Ayın **gerçek gün sayısı** doğrulanır (31.02 reddedilir); eksik yazılırken uyarı çıkmaz.
- **Liste ekranı (Araçlar / Kişiler):** üstte **standart arama kutusu** (arama ikonu + alan + doluyken ✕ temizle), **hemen altında satırlar**. ⚠️ **Sonuç sayacı konmaz** — liste zaten görünüyor; sayac aramayla liste arasına girip gözü oyalar. Satır: sol tanımlayıcı (plaka rozeti / baş harf rozeti) + ad ve alt bilgi + sağda `chevron`. **Boş sonuçta ne aranabileceğini söyleyen mesaj** ("Eşleşen araç yok. Plaka, marka, model ya da sahip adıyla arayın.") — kuru "sonuç yok" değil.
- **Tarihe bağlı durum rozeti:** bitiş tarihi taşıyan satırlar (muayene, sigorta, garanti…) **kendi durumunu söyler**: geçmişse `crit` *"N gün geçti"*, 30 gün içindeyse `warn` *"N gün kaldı"*, değilse rozetsiz. Bölüm başında kaç tanesinin dikkat istediği yazılır. **Tarihi göstermek yetmez — ne anlama geldiğini de söyle.**
- **Kişi satırı (Kişiler listesi):** avatar + **ad** + bağlı **plaka etiketleri**; altında **telefon ayrı bir `tel:` linki** (mono, aksan zeminli pill). **Rol yazılmaz** — kimin ne olduğu bu listede işe yaramaz; gereken tek şey kişiye ulaşmak.
  - ⚠️ **`<button>` içine `<a>` konmaz** (iç içe etkileşimli öğe). Satırın tamamı tıklanıp aynı zamanda içindeki telefon aranacaksa, satır **dikey ikiye bölünür**: üstte düğme, altında link.
  - **Satır tek hedef gibi davranır ("yayılmış bağlantı"):** asıl düğmenin `::after`'ı `position:absolute; inset:0` ile **tüm satırı** kaplar; hover/aktif durum **satırın kendisine** verilir (iç parçaya değil). İkincil link (telefon) `position:relative; z-index:1` ile **üstte** kalır — ona basınca satırın eylemi değil kendi eylemi çalışır. Klavye odağı da `::after` üzerinde gösterilir.
  - **Dokunma hedefi en az 44px** (telefon gibi ikincil eylemler dahil). ⚠️ **Görsel ağırlık ≠ dokunma alanı:** hedefi büyütmek için font/zemin büyütmek gerekmez — **`min-height` ile alan korunur, metin küçük kalır.** İkincil eylemler zeminsiz bırakılır; dolu pill zemin onları birincil eylem gibi gösterir.
- **Künye listesi (etiket — değer):** `dt` sabit genişlikte soluk etiket, `dd` değer; satırlar ince ayraçla ayrılır. **Kod/numara değerleri mono** (şasi, motor no, km). **Boş alanın satırı hiç çizilmez** — "—" dolgusu yok. Kayıt ekranında toplanan her alanın bir okuma yeri olmalıdır.
- **Renk seçici:** özel seçimin renk varyantı — her seçenekte **kutucuk + ad**, seçilince kutucuk tetik düğmesinde de görünür. Seçim **ad + hex** birlikte tutulur; kutucuk stilden değil **veriden** boyanır (§2 kuralının doğal sonucu).
- **İpucu satırı yer tutmaz:** doğrulama/ipucu metni **boşken görünmez** (`:empty { display:none }`) — alanlar arasında ölü boşluk kalmaz, ipucu geldiğinde araya girer. *(İstisna: tek ipuculu ekranlarda — ör. plaka — yeri sabit tutulur ki buton zıplamasın.)*
- **Bölüm başlığı + eylem:** kart içi bölüm başlığının **sağ ucunda** bir veya iki yuvarlak eylem (ör. ruhsattan oku + sesle doldur; biri dolu aksan, diğeri çerçeveli) — ayrı şerit/kutu yerine başlık satırına yerleşir, dikey yer harcamaz.
- **Tekrarlanan alan grubu (ör. iletişim):** “Ekle” butonu **doğrudan forma yeni bir alan bloğu** ekler — ayrı panel, ayrı “Vazgeç/Kaydet” adımı **yoktur**; blok asıl kayıtla birlikte kaydedilir. Bloklar ince üst ayraçla ayrılır, başlık satırının sağında kaldırma düğmesi bulunur, silince başlıklar yeniden numaralandırılır.
- **Tek-seçim işareti (`primary-flag`):** birden çok blok arasından birini işaretler (ör. *Geçerli iletişim*). `aria-pressed`; seçili olan `--accent-soft` + `--accent`. İşaretli blok silinirse **işaret ilk bloğa düşer** — hiç seçili kalmaz.
- **Her serbest metin alanının KENDİ mikrofonu:** birden çok not alanı varsa (ör. *müşteri ne diyor* / *usta izlenimi*) her biri kendi başlığının sağındaki mikrofonla doldurulur; sesli not **yalnız o alana** yazılır. Tek bir "sesle ekle" düğmesi hangi alana yazacağını belirsiz bırakır.
- **Aç/kapa anahtarı (switch):** `role="switch"` + `aria-checked`; açıkken `--accent` dolu, kapalıyken `--surface-2` + `--line`. Topuz `transform` ile kayar, `prefers-reduced-motion`'da geçiş kapanır. Ayar satırı: solda başlık + açıklama, sağda anahtar.
- **Kalem listesi (parça/işçilik):** her satır **tip rozeti + düzenlenebilir ad + sağa yaslı mono tutar + kaldır**. Tip rozetleri semantik değil ayırt edicidir (parça `--accent`, işçilik `--info`). Altta **toplam şeridi**: ara satırlar soluk, **genel toplam display fontuyla büyük** — müşteriye söylenen rakam tek bakışta okunur. Boş listede ne yapılacağını söyleyen mesaj.
- **Form alanı stili `.field`'a bağlı olmamalı:** başlığı `section-label` ile gelen ya da bir satır içine gömülen alanlar (ör. kalem satırları) `.field` dışında kalır ve **tema almaz** (koyu temada siyah metin). **Taban kural** bunu garanti eder: `input, textarea, select { color: inherit; font-family: var(--font-body); }` — renk daima kapsayıcıdan gelir. Bu hata iki kez tekrarlandığı için kural seviyesine çıkarıldı.
- **Sesle doldur:** çok alanlı formda bölüm başlığının sağında yuvarlak mikrofon butonu; basılınca "Dinliyorum…" göstergesi, ardından alanlar **sırayla** dolar (ne olduğu görünür). Form alanları **boş başlar** — dolu başlarsa sesli girişin etkisi görünmez.
- **Araç rengi = küçük renkli KUTUCUK** (16px, `--line` sınırlı), metin değil. ⚠️ **Renk bir token DEĞİL, kayıttan gelen VERİdir** — `tokens.css`'ten gelmez (K-023 istisnası; araç renginin kod/hex saklanması `VERI-MODELI.md`'ye düşer).

## 6. TEMA
`data-theme="light|dark"` kökte; seçilmezse `prefers-color-scheme`. `body` arka planı daima `var(--bg)` (şeffaf bırakılmaz). Her iki temaya eşit özen; kontrast korunur.

## 7. ERİŞİLEBİLİRLİK
Klavye odağı görünür; `prefers-reduced-motion` saygı; renk tek başına anlam taşımaz (metin/ikon eşlik eder); yeterli kontrast.

> Yeni ekran incelemesinde kontrol: (1) tokens kullanıldı mı (ham renk yok), (2) ışık+koyu tuttu mu, (3) semantik≠marka, (4) ilgili aktörün `.Ui` RCL'sinde mi (host'a sızmadı — K-014/K-015), (5) tipografi/ölçü token'lardan.

---

## Denetim · 30.08.2026 (otomatik ölçüm)
Prototip 375 px mobil genişlikte tarandı: **12 ekran, 308 görünür tıklanabilir öğe.**

### Düzeltildi
| Öğe | Önce | Sonra | Yöntem |
|---|---|---|---|
| `.notif-clear` | 23 px | 44 px | `::before` alan genişletme — **görünüş 23 px kaldı** |
| `.ni-del` | 28 px | 44 px | `::before` — görünüş 28 px kaldı |
| `.switch` | 27 px | 44 px | `::before` — görünüş 27 px kaldı |
| `.mail-link` · `.logout-btn` · `.theme-toggle` | 36 px | 44 px | `min-height` (blok öğeler; dikey komşuluk `::before`'a elverişsiz) |

**Token'lar:** `--shadow-sm` ve `--scrim` prototipte doğmuş ama `tokens.css`'e hiç taşınmamıştı (K-023 "tek kaynak" fiilen bozuktu) → **üç temaya da eklendi.**

### Yanlış pozitifler (ölçüm notu)
- Arama `input` 22 px görünüyor ama sarmalayıcı `.search-box` **48 px** — tıklama alanı zaten yeterli.
- `::after`'ı `inset:0` olan "yayılmış bağlantı" öğeleri satırın tamamını kaplar; ham `getBoundingClientRect` bunu göremez, ölçüm bunu telafi eder.

### ⚠️ AÇIK — Cemil kararı gerekiyor
- **A-D1 · 40 px ikon ailesi.** `.icon-btn` · `.brand-logo` · `.mic-round` **40 px** — kural 44 px diyor. Bu bir çelişki: 40 px, Cemil'in *"logo = icon-btn yüksekliği"* hizalama kararından geliyor (rev. 10). Ya **kural 40 px'e iner**, ya **ikon ailesi 44 px'e çıkar ve logo hizası yeniden kurulur**. Tek başıma değiştirmedim.
- **A-D2 · `.primary-flag` 34 px.** Cemil açıkça *"daha küçük olabilir"* demişti; büyütmek o talebe ters. Alan `::before` ile sessizce büyütülebilir — onay bekliyor.
- **A-D3 · Logo SVG'si ham hex kullanıyor** (`#269689` = `--brand-teal`, `#142230`/`#E7EEF6` = tema metin rengi) ve bu yüzden **tema başına iki ayrı blok** olarak duruyor. `fill="var(--brand-teal)"` ile **tek bloğa** inebilir. Marka varlığına dokunmak Cemil'in onayını ister.
- **A-D4 · `--font-logo` (Anton) ölü token.** Logo artık SVG; font listesinden de çıkmış. Wordmark ileride HTML olarak gerekebilir diye **silmedim**.

### Kural netleştirmesi
- **`.tel-inline` 44 px kuralının DIŞINDADIR.** Cümle içi bağlantıya 44 px dayatmak satır aralığını bozar; WCAG 2.5.8 de "inline" bağlantıları hedef-boyutu kuralından muaf tutar. Ölçümde 19–40 px arası değişmesi bu yüzden hata değildir. **Blok bağlamda `.tel-link` kullanılır ve o 44 px'dir** — ayrım budur.
- **Araç rengi (`renkHex`) K-023 kapsamında DEĞİLDİR.** `#FAFBFC` (Beyaz), `#8A8F98` (Gri) gibi değerler tasarım tokenı değil **veri**dir — dünyadaki nesnenin rengi. Token'a bağlanamaz; veri modeline aittir.

## ⚠️ Koyu tema tuzağı — TABAN RENK KURALI (30.08, üçüncü kez)

```css
input, textarea, select, button { color: inherit; font-family: var(--font-body); }
```

**Neden zorunlu:** tarayıcının `<button>` varsayılan rengi `buttontext` = **siyah**. Kendi rengini tanımlamayan bir düğme, koyu temada **koyu zemin üzerinde siyah yazı** olur — hiç okunmaz. Aynı şey `.field` dışında kalan `input`/`textarea` için de geçerli.

**Bu hata üç kez yaşandı:**
1. Serbest not `textarea`'sı (29.08) — noktasal düzeltildi
2. `.kalem` satırlarındaki `input` (29.08) — taban kurala alındı (`input, textarea, select`)
3. **`.ozet-kutu` ve `.odeme-yol` düğmeleri (30.08)** — `button` taban kurala eklendi

**Kural:** yeni bir bileşen yazarken rengi ya **token'dan** ver ya da hiç verme (kalıtım çalışsın). Ham `color` verme; `buttontext`/`fieldtext` gibi sistem renklerine güvenme.

**Kontrol yöntemi:** her yeni ekrandan sonra iki temada da `getComputedStyle(el).color` ve `backgroundColor` karşılaştır — ikisi de aynı çıkıyorsa (ör. iki temada da `rgb(0,0,0)`) kalıtım kopmuştur.


## Erişilebilirlik denetimi · 30.08.2026

21 ekran klavye, odak ve aria açısından tarandı.

### Düzeltilenler
| Bulgu | Düzeltme |
|---|---|
| Basit mod anahtarında **aria-label yok** (metinsiz düğme) | Etiket eklendi |
| **Çekmecede odak tuzağı yok** — açıkken Tab arkadaki 30 öğeye kaçıyor, klavye kullanıcısı kayboluyordu | Tab/Shift+Tab döngüsü eklendi |
| `focusables()` seçicisi **`[href]`** idi → SVG `<use href="#ikon">` elemanlarını odaklanabilir sayıyordu (14 yerine 29 öğe) | `a[href]` yapıldı |
| `focusables()` yalnız `offsetParent` bakıyordu → **boyutu sıfır** öğeler geçiyor, odak görünmez bir öğeye gidiyordu | Boyut kontrolü eklendi |

### Zaten doğru olanlar
- Pasif ekranlar `display:none` → içerikleri klavyeyle **erişilemiyor** (doğru).
- `:focus-visible` kuralı var; ESC ile çekmece kapanıyor; çekmece kapanınca odak menü düğmesine dönüyor.
- Pozitif `tabindex` yok, etiketsiz form alanı yok, ekran başına tek `h1`.

> **Ders:** `focusables` gibi yardımcıların **SVG'yi de yakalayabileceği** unutulmamalı. `[href]` seçicisi otomotiv ikonları yüzünden sessizce şişiyordu; odak ilk bakışta çalışıyor görünüp yanlış öğeye gidiyordu.


## Boş durum deseni (31.08)

**İki ayrı hâl vardır ve karıştırılmamalıdır:**

| Hâl | Ne söyler | Eylem düğmesi |
|---|---|---|
| **Hiç veri yok** (ilk kullanım) | Buranın ne işe yaradığını ve **nasıl doldurulacağını** | ✅ var |
| **Arama sonucu boş** | Veri var ama eşleşmedi — **ne ile aranabileceğini** | ✗ yok |

```
.bos-durum
  .bd-ikon      52px daire, --surface-2 zemin
  .bd-baslik    display font, 16.5px
  .bd-aciklama  --muted, max 30ch
  .btn          yalnız "hiç veri yok" hâlinde
.bos-durum.iyi  ikon yeşil — boşluk İYİ haber demekse (açık iş yok, yaklaşan kontrol yok)
```

**`.iyi` varyantı neden var:** her boşluk eksiklik değil. *"Açık iş yok"* ya da *"Yaklaşan kontrol yok"* olumlu haberdir; gri yerine yeşil ikonla verilir.

**Uygulandığı yerler:** Araçlar · Kişiler · Geçmiş · Açık İşler · Yaklaşanlar · Parça listesi · Fiyatlandırma. *(Fiyat karşılaştırmada boşluk grup içinde — "Henüz teklif gelmedi" satırı zaten vardı.)*


## Balon (toast) · 31.08

Cemil: *"Ses dinlerken bir balon açılıyor, rengi bizim tasarıma uygun değil."*

**Sorun neydi:** toast `background: var(--text)` / `color: var(--bg)` kullanıyordu — **ters kontrast**. Açık temada koyu, **koyu temada bembeyaz** bir balon çıkıyordu ve arayüzün geri kalanından kopuktu.

**Şimdi:**
| Hâl | Zemin | Yazı | Kenarlık |
|---|---|---|---|
| Normal bildirim | `--surface` | `--text` | `--line` |
| **Dinleme** (sesli işlem) | `--accent-soft` | `--text` | `--accent` |

Kartlarla aynı dil; gölge ve ince kenarlıkla öne çıkıyor. **Dinleme hâli marka aksanıyla ayrılır** — sesli işlem özel bir durum, sıradan bildirimle aynı görünmemeli.

> ⚠️ **Aksan zeminde metin rengi dersi:** ilk denemede dinleme balonunun yazısını `--accent` yapmıştım; `--accent-soft` üzerinde kontrast **açık temada 3.1** çıktı (WCAG AA metin için **4.5** ister). Marka rengini **zemin, kenarlık ve nokta** taşıyor, yazı `--text` kalıyor. Ölçülen: yazı **14.4 / 13.0**, nokta 3.1 / 6.8 (grafik öğe eşiği 3:1).
>
> **Kural:** yumuşak aksan zemin (`--accent-soft`, `--info-bg`, `--ok-bg`…) üzerine **aynı ailenin koyu tonunu metin olarak koyma** — okunmuyor. Metin `--text` kalsın, renk zeminde ve kenarlıkta dursun.


## ⚠️ Tanımsız CSS sınıfı — 9. denetim testi (31.08)

`.row-btns` diye bir sınıf **hiç tanımlı değildi**; doğrusu `.btn-row`. Yanlış ad **9 ekranda** kullanılmış ve düğmeler üst üste binmişti (üçü de tam genişlik alıyordu). Ekranda "biraz bozuk" görünüyordu ama sebebi görünmüyordu.

**`tools/saglik-denetim.js`'e 9. test eklendi:** kullanılan her sınıf `<style>` içinde geçiyor mu?
> ⚠️ SVG'de `className` bir `SVGAnimatedString`'dir, string değil — `toString()` sahte bulgu üretir. Test `baseVal` kullanıyor. *(Aynı tuzağa `focusables()` içinde de düşülmüştü.)*

**Kural:** yeni bir sınıf yazarken önce **mevcut adı ara**. Prototipte `.btn-row` varken `.row-btns` uydurmak sessiz bir hata üretiyor — CSS'te karşılığı olmayan sınıf hata vermez, sadece stil uygulanmaz.


## Ortak liste nesneleri (31.08 — Cemil)

> *"Bir yerde bir nesne olarak servis kaydı varsa, her yerde aynı olmalı. Bir değişiklik hepsinde olmalı."*

Prototipte **servis kaydı dört ayrı yerde**, **araç iki ayrı yerde** çiziliyordu. Biri düzeltilince diğerleri geride kalıyordu — plaka sağa yaslama düzeltmesi bunun canlı örneğiydi: yalnız kişi detayına uygulanmıştı.

### Kural
Bir varlığın liste görünümü **tek fonksiyonda** üretilir. Ekran-özel biçim yazılmaz; farklılık gerekiyorsa fonksiyona **seçenek** eklenir.

| Nesne | Fonksiyon | Kullanan ekranlar |
|---|---|---|
| Araç | `nesneArac(a, {sahip})` | Araçlar · Kişi detayı |
| Servis kaydı | `nesneServisKaydi(k, {plaka, islem, tikla})` | Geçmiş · Araç kartı (son 3) · Kişi detayı · Açık İşler |
| Kişi | `nesneKisi(k)` | Kişiler |
| Plaka | `plakaHtml(p, boyut)` | *(zaten ortaktı — iyi örnek)* |
| Boş durum | `bosDurum(...)` | *(zaten ortaktı)* |

Kaynak veriler farklı şekillerde geliyor (geçmiş kaydı `pill`, açık iş `d` alanını kullanıyor); **`servisKaydiNormalle()`** hepsini tek biçime çevirir. Çizici tek biçimi bilir.

### Şerit artık DURUMU söyler
Önceki kural şeridi **tipe** bağlıyordu (bakım teal, tamirat mavi…). Cemil şeridin duruma göre renklenmesini istedi: bir listede *"hangisi bekliyor"* sorusu *"hangisi bakım"* sorusundan daha sık soruluyor. **Tip bilgisi kaybolmuyor — ikon taşıyor.**

### Ölü CSS bırakılmaz
Ortak nesnelere geçince `.kisi-arac` ve `.is-satir` aileleri (22 kural) ölü kaldı. Bırakılsaydı *"iki ayrı satır biçimi var"* yanılgısı sürerdi. Denetime **16. test: `OLU-SINIF`** eklendi — tanımlı ama hiçbir ekranda kullanılmayan sınıfı bulur.

> ⚠️ Denetim `<style>` metnini okurken **CSS yorumlarını atar**. Yorumda geçen bir sınıf adı tanım değildir: bırakılırsa 16. test sahte bulgu verir, 5. test ise **gerçek eksiği gizler** (yanlış negatif — tehlikelisi bu).


## Flex sütun çocukları küçülmez (31.08)

`.screen` bir **flex sütun** ve aynı zamanda kaydırma kabı. Çocukları varsayılan olarak `flex-shrink: 1` — yani kutu içeriğinden **kısa kalabiliyor** ve içerik görünmez oluyor. İki kez yaşandı: not şeridi 2 px'e, akordiyon 2 px'e indi. İkisi de ekranda **boş alan** gibi duruyordu, hata vermiyordu.

```css
.screen > * { flex-shrink: 0; }
```

Tek tek `flex: none` yamamak yerine sınıfın kökü kapatıldı. Denetime **18. test `KIRPILMIS-KUTU`**: kutunun yüksekliği ilk çocuğundan küçükse içerik görünmüyordur.

## Arama normalizasyonu — aksan katlanır (31.08)

`norm()` yalnızca küçük harfe çeviriyordu: **"kardes" yazan "kardeş"i bulamıyordu.** Telefonda Türkçe harf yazmak zahmetli; tamirci *"sen"*, *"megane"*, *"ozturk"* diye yazar.

Elle harf tablosu tutulmuyor (ilk denemede `é` unutulmuştu) — **Unicode ayrıştırma (NFD)** harfi ve aksanını ayırır, aksan işaretleri silinir. Tek istisna **`ı`**: noktasız i ayrı bir harftir, ayrışmaz, elle eşlenir. Sorgu ve veri aynı fonksiyondan geçtiği için **iki yönlü** çalışır.


## Ekran başlığı yok — eyebrow tek satır (31.08 — Cemil)

> *"Sayfalarda başlık ve açıklama istemiyorum. Ekranları alta atmak istemiyorum. Mümkün olduğunca kontroller üstte olsun."*

Her ekranda **eyebrow + `<h1>` + açıklama paragrafı** vardı; üç satır, yaklaşık 90 px. Kontroller ekranın altına iniyordu — telefonda asıl işi yapan düğme kaydırmadan görünmüyordu.

**Kural:** `.screen-head` içinde **`<h1>` ve `<p>` yok.** Eyebrow ekranın tek başlık satırıdır.

- Dinamik sayaçlar kaybolmadı: **`.eb-say`** olarak eyebrow'un sağına taşındı — *"AÇIK İŞLER · 7 kayıt"*, *"ALACAKLAR · 3 müşteri · 16.900 ₺"*, *"ARAMA · 5 sonuç"*. Aynı `id`'ler durduğu için JS değişmedi.
- Eyebrow biraz büyüdü (12 → 13,5 px) ve **sarabiliyor** — bazı ekranda hem plaka hem sayaç var (fiyat ekranı 434 px istiyordu, yatay taşma veriyordu).
- Ekran üst dolgusu 16 → 12 px, `gap` 16 → 14 px.
- Topbar 68 → **72 px**, düğmeler 44 → **48 px**, ikonlar 21 → 23 px, logo 48 px (hizası bozulmasın). `tools/pwa-build.js` içindeki güvenli alan hesabı da 72 px'e çekildi.

**Ölçüm:** Mal Kabul'de ilk kart artık ekranın **42 px**'inde başlıyor.

### Açıklama silinirken ayrım yapıldı
Yalnızca ekranı *tarif eden* cümleler atıldı. **Kural taşıyanlar** ait oldukları kartın içine küçük ipucu olarak taşındı:

| Ekran | Taşınan kural |
|---|---|
| Ödeme ve teslim | *"Ödeme teslimin ön koşulu değil"* |
| Parça listesi onayı | *"Sınıf ve marka fiyat sorulmadan önce seçilir"* |
| Fotoğraftan liste | *"Girilen rakamlar tahmini fiyat"* |
| Sesle ara | *"Tasarım önizlemesi — tanıma motoru yok"* |
| Alacaklar | canlı vade uyarısı → kendi kabına (`#alUyari`) |

Bilgi kaybolmadı, yeri değişti.

## Sürüm notları
- v1 · 24.08.2026 — marka, renk/tip/ölçü token rolleri, bileşen desenleri, tema, erişilebilirlik.
- **v2 · 29.08.2026** — omurga prototipinden (`prototypes/omurga.html` v12) çıkan kararlar işlendi: **§3.1 ikon seti = Lucide** (emoji yok), **§5.1 uygulama kabuğu** (sabit header/footer, geri-ileri disabled görünür, sağdan drawer, tema drawer'da, satır-başı mikrofon), **§5.2 ek bileşenler** (bildirim listesi, timeline, tip şeridi, notice, durum geçiş şeridi, telefon, plaka maskesi, araç rengi kutucuğu = veri).
- **v3 · 29.08.2026** — **TİP RENKLERİ** (`--tip-bakim/tamirat/inceleme/teklif`, §2) eklendi: ziyaret tipini ayırt eder, durumdan ayrıdır; dördü de mevcut paletten türer (yeni hex yok), `--nut` için dar istisna. §5.2: **satır şeridi = tip** kuralı (zemin boyanmaz).
- **v4 · 29.08.2026** — `--info`/`--info-bg` (mavi, nötr sonuç) eklendi; **durum rozetlerinde gri yasak** kuralı. Maskeli alan kuralı: boşken örnek metni yerine **placeholder maske şablonu**. **Sesle doldur şeridi** deseni.
- **v5 · 29.08.2026** — **Logo düzeltmesi:** somun turuncu değil **teal**; "turuncu yalnız logoda" kuralı kaldırıldı. `--nut` → **`--highlight`** (üçüncü vurgu). §5.2'ye **ruhsat formu** deseni.
- **v6 · 29.08.2026** — §5.2: renk seçici · autocomplete alanı (bağımlı liste) · bölüm başlığı + sağ eylem · satır içi ekleme paneli desenleri.
- **v7 · 29.08.2026** — **Native `<select>` yasağı** + özel seçim bileşeni (tema uyumlu menü, ESC/dışarı tıklama). Renk seçici çip yerine seçim menüsüne döndü. Tekrarlanan alan grubu (panel/kaydet adımı yok) + tek-seçim işareti desenleri.
- **v8 · 29.08.2026** — `<datalist>` de yasağa dahil (native çizim + doluyken filtreleme yüzünden tekrar seçilemiyor); özel seçime **menü içi arama** ve **bağımlı seçim temizleme** kuralları. İpucu satırı boşken yer tutmaz.
- **v9 · 29.08.2026** — §5.2: **künye listesi** deseni (etiket–değer, mono kod alanları, boş satır çizilmez).
- **v10 · 29.08.2026** — §5.2: **liste ekranı** deseni (arama kutusu + sonuç sayısı + satır + yol gösteren boş durum). “Geçerli iletişim” rozeti küçültüldü (10.5px) — ada eşlik eden ikincil bir işaret, başlıkla yarışmaz.
- **v11 · 29.08.2026** — **Plaka tek bileşene indirildi** (çerçeve + TR bloğu + mono numara, üç boyut); düz mono plaka metni ve ayrı rozet biçimleri kaldırıldı.
- **v12 · 29.08.2026** — Native **tarih seçici** de yasağa dahil (GG.AA.YYYY maskesi). **Tarihe bağlı durum rozeti** deseni. **Her not alanının kendi mikrofonu** kuralı. Form alanı stilinin `.field`'a bağlı olmaması (tema hatası kaynağıydı).
- **v13 · 29.08.2026** — **Kalem listesi + toplam şeridi** deseni. Form alanları için **taban kural** (`color: inherit`) — `.field` dışında kalan alanların tema almaması sorunu kökünden çözüldü.
- **v14 · 29.08.2026** — **Aç/kapa anahtarı (switch)** deseni; alan görünürlüğü ayarı için ayar satırı düzeni.
- **v15 · 29.08.2026** — Liste ekranlarından **sonuç sayacı kaldırıldı** (arama → doğrudan liste). Liste satırına **basılı durum** (`:active`) ve görünür klavye odağı eklendi.
- **v16 · 29.08.2026** — **Kişi satırı** deseni: rol gösterilmez, telefon ayrı `tel:` linki (16px mono, 44px dokunma hedefi). "button içine a konmaz" kuralı.
- **v17 · 30.08.2026** — Kişi telefonu **zeminsiz ve daha küçük** (14.5px), dokunma hedefi `min-height` ile korundu. Kural: görsel ağırlık ile dokunma alanı ayrı şeyler.
- **v18 · 30.08.2026** — **Telefon tek bileşene indirildi** (`.tel-link` / `.tel-inline` + `.mail-link`); her ekranda aynı görünüyor. **Yayılmış bağlantı** deseni: satırın tamamı tıklanabilir ve hover satırın kendisinde, içindeki telefon ayrı çalışıyor.
- **v19 · 30.08.2026** — **Otomatik denetim** (12 ekran / 308 öğe): 6 dokunma hedefi 44 px'e çıkarıldı (görünüş korunarak), `--shadow-sm` + `--scrim` tokens.css'e taşındı; `.tel-inline` ve araç `renkHex` **kural dışı** olarak netleşti; A-D1…A-D4 açık bırakıldı.
- **v20 · 30.08.2026** — **Taban renk kuralına `button` eklendi.** Koyu temada `.ozet-kutu` ve `.odeme-yol` siyah yazı çıkıyordu (tarayıcı `buttontext` varsayılanı). Üçüncü kez yaşanan tuzak; kontrol yöntemiyle birlikte kalıcı bölüm açıldı.
- **v21 · 30.08.2026** — **Erişilebilirlik denetimi:** çekmeceye odak tuzağı eklendi; `focusables()` iki hatası düzeltildi (SVG `<use>` yakalıyordu, boyutu sıfır öğeleri geçiriyordu); basit mod anahtarına aria-label.
- **v22 · 31.08.2026** — **Boş durum deseni** (`.bos-durum`): "hiç veri yok" ile "arama sonucu boş" ayrıldı; ilki yol gösterir ve eylem düğmesi taşır, ikincisi ne ile aranabileceğini söyler. `.iyi` varyantı — boşluğun olumlu olduğu yerlerde yeşil ikon.
- **v23 · 31.08.2026** — **Balon (toast) tasarım sistemine alındı:** ters kontrast kaldırıldı (koyu temada beyaz balon çıkıyordu), yüzey rengine geçti; **dinleme hâli** marka aksanıyla ayrıldı. Aksan zeminde metin rengi kuralı yazıldı (ölçüm: 3.1 → 14.4).
- **v24 · 31.08.2026** — **Tanımsız CSS sınıfı testi** (9. sağlık testi): `.row-btns` 9 ekranda bozuk düğmeye yol açmıştı. Servis kaydı satırında plaka sağa yaslandı (`.tl-title` flex), kişi künyesi çipleri eklendi.
- **v25 · 31.08.2026** — **Ortak liste nesneleri**: araç, servis kaydı ve kişi tek fonksiyonda çiziliyor (`nesneArac` · `nesneServisKaydi` · `nesneKisi`). Servis kaydı şeridi artık **tipi değil durumu** söylüyor. 22 ölü CSS kuralı silindi; denetime **16. test `OLU-SINIF`** eklendi ve denetim artık **CSS yorumlarını ayıklıyor**.
- **v26 · 31.08.2026** — **Araç başlığı ortak nesne** (`nesneAracBasligi`); araç künyesi **akordiyon**; kontrol tarihleri **tek satır**; iletişimde geçerli olan **üstte**, tek kişide rozet yok. **Müşteri notları** (yatay kaydırmalı, sesle eklenir, **aramaya dahil**). Terim kararı: her yerde **Müşteri**. `norm()` **aksan katlıyor**. `.screen > * { flex-shrink: 0 }`. Denetim **18 test**.
- **v27 · 31.08.2026** — **Ekran başlıkları ve açıklamaları kaldırıldı** (Cemil): 21 `h1`, 24 paragraf. Eyebrow tek başlık satırı oldu, dinamik sayaçlar `.eb-say` ile yanına taşındı. Topbar 72 px, düğmeler 48 px. Kural taşıyan açıklamalar kartların içine indi.
- **v28 · 01.09.2026** — **Beş yeni desen.**
  **Tabaka** (`.tabaka`): alttan açılan seçim yüzeyi. Kural — *seçim için ekran değiştirme*: kullanıcı bir formun ortasındayken yeni ekran açmak yazdıklarını gözden kaçırtıyor. Tabaka üstte açılır, seçince kapanır, form yerinde kalır. Çekmeceyle aynı erişilebilirlik sözleşmesi: `role="dialog"`, Esc kapatır, Tab tuzağı, arka plan `inert`.
  **Ürün satırı** (`.urun-sat` / `nesneUrun`): katalog ürününün tek çizimi — parça seçicide, stok listesinde, mal kabulde aynı. Değiştirici sınıflar `u-` önekli (`.stok` başka bir bileşenin yeşil noktasını içeri alıyordu — **önek kuralı**).
  **Çip grubu** (`.cip`): süre seçimiyle aynı desen, artık ret sebebi ve indirim oranı gibi yerlerde de kullanılıyor.
  **Hızlı eylem** (`.tl-eylem`): liste satırının içinde, satırın kendi tıklamasını yutmayan ikinci eylem (`stopPropagation`). Satır kaydı açar, düğme işi ilerletir.
  **Açılır menüde grup başlığı** (`.sel-grup`): `selDoldur` artık `{ grup, degerler }` kabul ediyor; arama yapınca boşalan grup başlığı gizleniyor.

  **İki kalıcı tuzak yazıldı:**
  1. **`flex: 1 1 0` + `min-width: 0` sarmayı öldürür.** Öğeler sıfıra kadar daralabildiği için satır hiç dolmaz, `flex-wrap` devreye girmez, `white-space: nowrap` ile yazı kutunun dışına taşar. Doğrusu `flex: 1 1 auto; min-width: max-content`. Ayrıca `.btn` tabanında `width: 100%` varsa `width: auto` da verilmeli, yoksa her düğme ayrı satıra düşer.
  2. **Blok olmayan `<span>` alt satıra inmez.** `.mk-ad`/`.mk-alt` ve `.bb-ad`/`.bb-alt` yan yana akıyordu ("… TRWstok 0 adet", "Tamiron Oto Servis Örnek Mah…"). Ad + alt satır deseninde ikisi de `display: block` olmalı.
- **v29 · 01.09.2026** — **Kural: ekran verisine bakan çizim, ekran açılışında çalışır.** Sayfa yüklenirken bir kez çizmek iki hatayı birden doğuruyor: (1) o anda aşağıda tanımlı `var`'lar henüz `undefined` (bu projede **sekiz kez**), (2) veri sonradan değişince liste bayat kalıyor. **`typeof X === 'undefined'` koruması bir çözüm değil, hata gizleyicidir** — patlamayı önler, yanlış davranışı sürdürür ve denetim "0 bulgu" der. Denetime **22. test YÜKLEME SIRASI** eklendi.
- **v30 · 01.09.2026** — **Nötr griler markaya kaydırıldı; alt başlıklar renkli.** Cemil: *"sayfalarda boşluklar hep gri, biraz renkli olabilir; alt başlıklar renkli olabilir; koyu temada griler daha açık, açık temada daha koyu belirgin olsun."*

  **Token değişiklikleri** (ikisi de `tokens.css` + gömülü kopya):

  | Token | Açık (eski → yeni) | Koyu (eski → yeni) |
  |---|---|---|
  | `--bg` | `#F2F5F8` → `#EDF2F9` | `#0A0F17` → `#0A1220` |
  | `--surface` | `#FFFFFF` (aynı) | `#121A25` → `#14202F` |
  | `--surface-2` | `#E9F0F5` → `#E2EBF5` | `#0E141D` → `#172436` |
  | `--line` | `#DBE3EC` → `#D3DEEB` | `#233042` → `#2A3B51` |
  | `--muted` | `#54637A` → `#44556F` | `#93A2B4` → `#A9B8CB` |
  | `--faint` | `#93A2B4` → `#7E8FA5` | `#6E7E90` → `#8496AB` |

  **İki yeni token:** `--field-bg` (metin alanlarının zemini — beyaz/nötr değil, hafif tinted) ve `--hint` (ipucu metni — `--muted`'dan ayrı, mavimsi).

  **Ölçüm — yüzeye karşı kontrast oranı:**

  | | açık | koyu |
  |---|---|---|
  | `--text` | 16.82 | 14.06 |
  | `--muted` | 7.57 | 8.15 |
  | `--hint` | 6.29 | 7.57 |
  | `--faint` | 3.30 | 5.43 |
  | `--primary` (alt başlık) | 8.46 | 5.09 |

  `--faint` açık temada 3.30 — yalnız **yer tutucu ve ikon** için; gövde metninde kullanılmaz (AA 4.5 altında). Koyu temada 5.43'e çıktı, orada gövde metninde de kullanılabilir.

  **Kural: alt başlık (`.section-label`) `--primary` renginde.** Gri başlık sayfa taranırken kayboluyordu; bölüm başları önce göze çarpsın.

  **Kalıcı tuzak — koyu tema İKİ YERDE tanımlı.** `[data-theme="dark"]` (kullanıcı seçerse) ve `@media (prefers-color-scheme: dark)` (sistem tercihi) aynı değerleri **kopyalar**; CSS'te tek kaynaktan türetilemiyor. Bu turda tam bu oldu: biri güncellendi, öteki unutuldu — *seçilen* koyu temada yeni renkler, *sistem* koyu temasında eski renkler çıkıyordu. Kullanıcıların çoğu "sistem" ayarında olduğu için değişiklik onlara hiç ulaşmayacaktı. Denetime **23. test `TEMA-UYUSMAZLIGI`** eklendi: iki bloğu token token karşılaştırıyor (33 token, fark yok).

---

## v31 — dördüncü tur bileşenleri (01.09.2026, işler 19–28)

### Gizlenen para — `•••`, boşluk değil

Yetkisi olmayan kullanıcıya rakamı **silip boş bırakmak** *"fiyat yok"* gibi okunuyordu. Doğrusu: nokta serisi + **sebebi yazan not**.

```
.para-gizli { letter-spacing: .18em; color: var(--faint); font-family: var(--font-mono); }
```

Şeridin tamamı gizleniyorsa tek satır kalır (`Toplam •••`) ve altında sebep yazar. **Tek tek maskelemek**, "burada bir rakam var ama sana yok" demenin en gürültülü yoludur.

Sebep **iki ayrı metindir** ve ayrımı korunmalıdır:
- yetkisizlik → *"Çırak rolünde fiyatlar görünmez."*
- yanlış kayıt → *"Bu kaydın sorumlusu sen değilsin…"*

### Kesinlik rozeti (`.k-kesinlik`)

Üç kademe, ama **yalnız ikisi işaretlenir**: `kesin` çoğunluktur, işaretlenmez. **İstisna işaretlenir, kural değil** — bu belgedeki genel kural (bkz. "araç dışarıda" işareti).

| Kesinlik | Görünüm |
|---|---|
| kesin | rozet yok |
| olabilir | `--warn-bg` / `--warn` |
| şartlı | `--surface-2` / `--faint`, satır `opacity: .8` |

Rozet **düğmedir** — dokununca kesin → olabilir → şartlı → kesin döner.

### Tavsiye şeridi (`.ep-satir`)

Tavsiye **kalem değildir**, kalem gibi görünmemelidir: **kesik kenarlık** (`border: 1px dashed`), `--surface-2` zemin. Üç kademeli güç, üç ayrı ikon rengi:

| Güç | Ton | Renk |
|---|---|---|
| birlikte değişir | uyarı | `--crit-bg` / `--crit` |
| bakılmalı | soru | `--warn-bg` / `--warn` |
| açılmışken | hatırlatma | `--accent-soft` / `--accent` |

Her tavsiye kırmızı olursa hiçbiri okunmaz. Bastırılan tavsiye `opacity: .72`.

### ⚠️ `.btn` esnek satırda — width geri alınmalı

`.btn` tabanı `width: 100%` veriyor. **Bir flex satırına konulduğunda** doğal genişliği %100 sanılıp bütün yeri alıyor ve kardeşini sıfıra eziyor (`.ep-govde` 0 px kaldı, metin kelime kelime alt alta dizildi). Bu projede **ikinci kez** yaşandı (ilki `.btn-row` sarmaması).

```css
/* flex satırındaki her .btn için: */
.x-btn { flex: none; width: auto; }
```

Sağlık denetiminin **25. testi** (EZILEN-GOVDE) bunu otomatik yakalıyor.

### ⚠️ Sınıf adı tek bileşene aittir

`.rol-cip` zaten personel rozetiydi; aynı ad rol seçme düğmesine verilince hem görünüm bozuldu hem `querySelectorAll` 4 yerine **8 öğe** buldu. Bu projede **üçüncü** sınıf çakışması (`.stok`, `.tablo-kap`, `.rol-cip`). Sınıf adı burada hem stil hem seçici olduğu için çakışma iki yerden birden vuruyor. Denetimin **24. testi** (SINIF-CAKISMASI) bunu yakalıyor.
