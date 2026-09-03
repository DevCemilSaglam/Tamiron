# TAMIRON — TASARIM SİSTEMİ (TASARIM-SISTEMI.md) · v2.0 · 03.09.2026

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
- **Uyarı şeridi (notice):** ikon + metin, üç ton — `ok` (bilgilendirme/başarı) · `warn` (eksik bilgi) · `accent` (dükkan kuralı/yönlendirme). Akışı kesen ayrı adım yerine **ekran içinde** uyarı verir.
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
- **Künye listesi (etiket — değer):** `dt` sabit genişlikte soluk etiket, `dd` değer; satırlar ince ayraçla ayrılır. **Kod/numara değerleri mono** (şasi, motor no, km). **Boş alanın satırı hiç çizilmez** — "—" dolgusu yok. Kayıt ekranında toplanan her alanın bir okuma yeri olmalıdır.
- **İpucu satırı yer tutmaz:** doğrulama/ipucu metni **boşken görünmez** (`:empty { display:none }`) — alanlar arasında ölü boşluk kalmaz, ipucu geldiğinde araya girer. *(İstisna: tek ipuculu ekranlarda — ör. plaka — yeri sabit tutulur ki buton zıplamasın.)*
- **Bölüm başlığı + eylem:** kart içi bölüm başlığının **sağ ucunda** bir veya iki yuvarlak eylem (ör. ruhsattan oku + sesle doldur; biri dolu aksan, diğeri çerçeveli) — ayrı şerit/kutu yerine başlık satırına yerleşir, dikey yer harcamaz.
- **Tekrarlanan alan grubu (ör. iletişim):** “Ekle” butonu **doğrudan forma yeni bir alan bloğu** ekler — ayrı panel, ayrı “Vazgeç/Kaydet” adımı **yoktur**; blok asıl kayıtla birlikte kaydedilir. Bloklar ince üst ayraçla ayrılır, başlık satırının sağında kaldırma düğmesi bulunur, silince başlıklar yeniden numaralandırılır.
- **Tek-seçim işareti (`primary-flag`):** birden çok blok arasından birini işaretler (ör. *Geçerli iletişim*). `aria-pressed`; seçili olan `--accent-soft` + `--accent`. İşaretli blok silinirse **işaret ilk bloğa düşer** — hiç seçili kalmaz.
- **Her serbest metin alanının KENDİ mikrofonu:** birden çok not alanı varsa (ör. *müşteri ne diyor* / *usta izlenimi*) her biri kendi başlığının sağındaki mikrofonla doldurulur; sesli not **yalnız o alana** yazılır. Tek bir "sesle ekle" düğmesi hangi alana yazacağını belirsiz bırakır.
- **Aç/kapa anahtarı (switch):** `role="switch"` + `aria-checked`; açıkken `--accent` dolu, kapalıyken `--surface-2` + `--line`. Topuz `transform` ile kayar, `prefers-reduced-motion`'da geçiş kapanır. Ayar satırı: solda başlık + açıklama, sağda anahtar.
- **Kalem listesi (parça/işçilik):** her satır **tip rozeti + düzenlenebilir ad + sağa yaslı mono tutar + kaldır**. Tip rozetleri semantik değil ayırt edicidir (parça `--accent`, işçilik `--info`). Altta **toplam şeridi**: ara satırlar soluk, **genel toplam display fontuyla büyük** — müşteriye söylenen rakam tek bakışta okunur. Boş listede ne yapılacağını söyleyen mesaj.
- **Form alanı stili `.field`'a bağlı olmamalı:** başlığı `section-label` ile gelen ya da bir satır içine gömülen alanlar (ör. kalem satırları) `.field` dışında kalır ve **tema almaz** (koyu temada siyah metin). **Taban kural** bunu garanti eder: `input, textarea, select { color: inherit; font-family: var(--font-body); }` — renk daima kapsayıcıdan gelir. Bu hata iki kez tekrarlandığı için kural seviyesine çıkarıldı.
- **Sesle doldur:** çok alanlı formda bölüm başlığının sağında yuvarlak mikrofon butonu; basılınca "Dinliyorum…" göstergesi, ardından alanlar **sırayla** dolar (ne olduğu görünür). Form alanları **boş başlar** — dolu başlarsa sesli girişin etkisi görünmez.

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

---

# v2.0 — tamircinin geri bildirimine göre ayıklama (03.09.2026)

Bu belge v1'den **devralındı ve budandı.** Zanaat bilgisi (plaka bileşeni, telefon bileşeni, native seçim tuzağı, koyu tema taban rengi, dokunma hedefi, yayılmış bağlantı…) **akıştan bağımsızdır ve olduğu gibi durur.** Eski ekranlara bağlı desenler çıkarıldı.

## Düşen dokuz desen

| Desen | Neden düştü |
|---|---|
| Öncelik etiketi (4 kademe) | v2'de öncelik kavramı yok |
| Zaman tüneli özet kuralları (*Bakım · İnceleme · Tamirat · Teklif*) | İki tür kaldı: tamirat / bakım (**K-006**) |
| Satır şeridi = tip (`--tip-*`) | Dört tip yok |
| Durum geçiş şeridi (`pill → ok → pill`) | Durum makinesi yok (**K-004**) |
| Tarihe bağlı durum rozeti (muayene/sigorta/garanti) | Kontrol tarihleri modelde yok |
| Kişi satırı (Kişiler listesi) | Ayrı müşteri ekranı yok — müşteri aracın üstünde |
| Renk seçici · araç rengi kutucuğu | Renk saklanmıyor |
| Bildirim listesi (çekmece içinde) | **Çekmece yok** — aşağıya bak |

## ⚠️ Çekmece (drawer) kalktı

v1'in kabuğunda sağdan açılan bir çekmece vardı: bildirimler, bölümler, kullanıcı, ayarlar. **v2'de yok.**

**Gerekçe:** Altı ekran var (`EKRANLAR.md`) ve hepsine ana ekrandan ya da akışın kendisinden ulaşılıyor. Çekmece, **on üç maddeyi bir yere sığdırmak** için vardı. T-18 zaten *"iç içe menü yok"* diyor — çekmece tanım gereği iç içe bir menüdür.

**Ayarlar** ana ekrandan tek düğmeyle açılır. **Bildirim** yok: teslim edilmemiş kayıtlar zaten ana ekranın kendisi.

> Kabuk sadeleşiyor: **sabit header (logo + geri) + ekran gövdesi.** Footer ve çekmece yok.

## Yazı ölçeği artık token — ve taban büyüdü

v1'de yazı boyutları bileşen CSS'ine gömülüydü; *"hepsini büyüt"* demek yüzlerce satıra dokunmak demekti. Kullanıcımız **teknolojiden uzak ve yaşlı olabilir** (T-18) — taban boyut **tek yerden** değişebilmeli.

```
--tip-xs   13px   rozet, küçük etiket — ASGARİ, bundan küçük yok
--tip-sm   15px   alt bilgi, ikincil satır
--tip-md   17px   TABAN — gövde, buton, alan     (v1'de 16)
--tip-lg   20px   bölüm başlığı
--tip-xl   26px   ekran başlığı
--tip-2xl  34px   toplam tutar
```

**Dokunma hedefi de token:** `--dokun: 48px` (v1'de kural yazılıydı ama her bileşende elle yazılıyordu; 44 → **48**).

## ⚠️ En soluk metin ölçüldü ve düzeltildi

`--faint` açık temada **AA'yı geçmiyordu.** T-18 kapsamında ölçüldü:

| | surface | bg | surface-2 |
|---|---|---|---|
| **Eski** `#7E8FA5` | 3.30 | **2.94** | **2.79** |
| **Yeni** `#596A80` | **5.53** | **4.92** | **4.59** |

Üç zeminde de AA geçiyor. Koyu tema zaten geçiyordu (5.43 / 6.19 / 5.17) — dokunulmadı.

> **Kural: bir renk üç zeminde de ölçülür** (`--surface`, `--bg`, `--surface-2`). v1'de yalnız `--surface` üstünde bakılmıştı ve `--surface-2` gözden kaçmıştı.

## Yeni ekranlara göre bileşen ihtiyacı

`EKRANLAR.md`'deki altı ekranın kullandığı desenler — **hepsi zaten yukarıda var**, yeni bileşen gerekmiyor:

| Ekran | Kullandığı desenler |
|---|---|
| 1 · İçerideki araçlar | liste ekranı · plaka · boş durum · arama kutusu |
| 2 · Ruhsatı çek | maskeli alan · künye listesi · seçim çipleri (tamirat/bakım) · ipucu satırı |
| 3 · Servis kaydı | kalem listesi · toplam şeridi · her alanın kendi mikrofonu · bölüm başlığı + eylem |
| 4 · Parça fiyatı sor | özel seçim (parçacı) · kart · buton |
| 5 · Teslim et | toplam şeridi · seçim çipleri (ödeme yolu) · buton |
| 6 · Araç | künye listesi · plaka · telefon bileşeni · liste |

**Yeni desen yazılmadı.** v1'in zanaatı yeni ürünü karşılıyor — düşen şey desenler değil, **ekranlardı**.
