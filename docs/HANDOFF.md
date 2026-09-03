# TAMIRON — DEVİR (HANDOFF.md)

> **Son güncelleme:** 03.09.2026 · v2 birinci turun sonu (işler 12–16)
> Bu belge *"yarın buraya oturan kişi ne bilmeli"* sorusunu cevaplar.
> Belge listesi değil, **durum** belgesidir.

---

## 1 · Nerede duruyoruz

**Faz:** tasarım / iş akışı. **Kod ve backend yok, olmayacak da** — bu
turda üretilen tek çalışan şey tıklanabilir prototip.

| | |
|---|---|
| Ana yol | **7 adım · 6 ekran** — hepsi prototipte çalışıyor |
| Dokunuş | tam yol **13**, kısa yol **9** (ölçüldü, `IS-AKISI.md` §senaryo) |
| İstisna kolları | **5 kol · 0 yeni ekran · 0 yeni adım** — hepsi işletildi |
| Sağlık denetimi | 25 test · **0 bulgu** · 375 px'te ölçüldü |
| Kararlar | K-001 … K-008 |
| Paket | `app/` üretiliyor (9 dosya, çevrimdışı, düz yapı) |

---

## 2 · Önce okunacaklar (sırayla)

1. **`arastirma/TAMIRCI-GERIBILDIRIM.md`** — bu deponun 1 numaralı belgesi.
   Tamircinin sözü bizim akıl yürütmemizi yener.
2. `KARARLAR.md` — K-001…K-008 + açık maddeler
3. `IS-AKISI.md` — yedi adım, beş kol, ölçülmüş senaryo
4. `EKRANLAR.md` — altı ekran; her ekranın *"üzerinde ne YOK"* sütunu
5. `VERI-MODELI.md` — sekiz tablo
6. `TASARIM-SISTEMI.md` — **sonundaki sınıf sözlüğü bağlayıcı** (K-008)

---

## 3 · Prototipe nasıl dokunulur

```
prototypes/kaynak/**        <- ELLE BURASI DÜZENLENİR
  kabuk.html                   header + toast + yuvalar
  stil/01-taban 02-kabuk 03-bilesen.css
  betik/01-cekirdek 02-veri 03-nesne.js
  ekranlar/10-home … 60-arac.html
        |
        |  node tools/prototip-derle.js
        v
prototypes/omurga.html      <- ÜRETİLMİŞTİR, elle düzenlenmez
```

**Derleme dört şeyi denetler** ve bulursa `exit 1` verir:
ekran dosyaları arası **sınıf çakışması** · ölü `data-git` hedefi ·
`id` ile dosya adı uyuşmazlığı · **ölü ikon referansı**.

Her ekran betiği derlemede **kendi IIFE'sine sarılır** — v1'in sekiz
`var` sızıntısı hatası bu yüzden yapısal olarak imkânsız.

**Sağlık denetimi:**

```
python tools/denetim-kos.py gom     # denetimi omurgaya gomer
# tarayicida:  window.DENETIM_CALISTIR()
python tools/denetim-kos.py geri    # geri alir
```

Denetim **ölçüm yapılamadıysa söyler** (`olcum: "ATLANDI"`). Sıfır
genişlikte koşarsa sahte "0 bulgu" vermez.

**Paket:** `node tools/pwa-build.js` → `app/`

---

## 4 · Bu turda öğrenilen üç şey

**Belge taşımak sistem taşımak değildir.** Tasarım sistemi belge olarak
devralınmıştı ve doğruydu; ekranlar yine de yabancı çıktı çünkü sınıf
adları uydurulmuştu. Bağlayıcı olan sınıf sözlüğü (K-008).

**Bir kolun ekran açmaması, çalıştığı anlamına gelmez.** Beş kol
belgede *"ekran açmıyor"* diye geçtiği için doğru sayılmıştı; işletilince
ikisi çalışmıyordu (Kol B'de çoklu seçim yoktu, Kol D fiyatı siliyordu).

**Delikli denetim, denetimsiz olmaktan kötüdür** — yanlış güven verir.
Bu turda üç denetim deliği kapandı: sınıf çakışması regex'inde eksik `m`
bayrağı, ölü ikon referansının hiç test edilmemesi, `pwa-build`'in
kabuğun sınıf adlarına sessizce bağımlı olması.

---

## 5 · Sıradaki iş bekleyen açıklar

**Ürün kararı bekleyenler (Cemil):**

| Kod | Soru | Öneri |
|---|---|---|
| **A-1** | Ekran bütçesi 6 onaylanıyor mu | evet |
| **A-2** | Müşteri tarafı bu fazda mı | özet linki evet, müşteri uygulaması sonra |
| **A-3** | Bakım periyodu kaç ay | 6 |
| **A-14** | Ekran 2 tabaka mı olsun | ölçüldü: tabaka olursa **13 → 12** dokunuş, **6 → 5** ekran |
| **Ç-2** | OEM kodu yönü — TecDoc'un kaderini belirler | K-001 |
| **K-007** | Kol B'de hiçbir cevap seçilmezse akış durur mu | durmaz |

**Yapılmamış işler:** müşteri tarafı (T-19/20/21) · gerçek ruhsat okuma
(`RUHSAT-OKUMA.md` C yolu) · gerçek WhatsApp gönderimi
(`WHATSAPP.md` A yolu, kurulum gerektirmiyor) · SMS + İYS kaydı
(`SMS-VE-IYS.md`: 6563 m.6 muafiyeti hatırlatmayı kapsıyor, m.8 çıkış
hakkı yine de şart) · backend.

---

## 6 · Değişmez kurallar

- **Yedi iş dışında özellik yok.** *"İleride lazım olur"* gerekçe değildir.
- Ham hex yok — yalnız `tokens.css`. Yazı boyutu ve dokunma hedefi de token.
- Türkçe arayüz, İngilizce kod. MudBlazor yok.
- Prototip tek dosya değildir; `omurga.html` **üretilir**.
- Ortak nesne kuralı: plaka, kayıt satırı ve boş durum **tek yerde** çizilir.
- `KARARLAR.md`'ye yalnız lider dokunur; belirsizlik *"Cemil onayına açık"*
  işaretlenir ve iş durmaz.
