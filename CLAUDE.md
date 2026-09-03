# TAMIRON — Claude Code Talimatı

Tamiron = tamirhane için servis kaydı + parça tedariki + basit ön muhasebe programı.
**Faz: tasarım/iş-akışı — kod/backend YOK.**

## Bu depo 03.09.2026'da sıfırdan başladı

v1 (`github.com/DevCemilSaglam/Tamiron-v1`, 195 commit) bir prototipe kadar gitti ve **bir tamirciye gösterildi.** Tamircinin hükmü: *"çok fazla özellik var, karmaşık geldi."* Akış yeniden kuruluyor.

**Devralınanlar:** mimari kararlar · tasarım sistemi · dünya bilgisi araştırmaları (KDV/tevkifat, sigorta, ürün yapısı, TecDoc) · sağlık denetimi aracı.
**Devralınmayanlar:** eski prototip · eski akış · eski ürün kararları · 68 senaryo · 38 ekran.

## Her oturuma başlarken oku
1. **`docs/arastirma/TAMIRCI-GERIBILDIRIM.md`** — bu deponun 1 numaralı belgesi. **ÖNCE BUNU.**
2. `docs/KARARLAR.md` — açık maddeler + verilen kararlar
3. `docs/MIMARI.md` — devralınan, tartışmaya kapalı mimari
4. `docs/TASARIM-SISTEMI.md` + `docs/tokens.css` — tek renk/tip kaynağı
5. `docs/SOZLUK.md` — Türkçe↔İngilizce terim eşlemesi

## Bu fazın kuralları

- **Basitlik ölçülebilir bir kısıttır, temenni değil.** Kullanıcı teknolojiden uzak (T-18). Ana yol için **ekran bütçesi** var (A-2); aşılacaksa önce konuşulur.
- **Yeni özellik varsayılan olarak HAYIR.** Bir şey ancak tamircinin yedi işinden birine hizmet ediyorsa girer (geri bildirim §7). *"İleride lazım olur"* gerekçe değildir.
- **Tamircinin sözü bizim akıl yürütmemizi yener.** Bir tasarım kararı geri bildirimle çelişiyorsa geri bildirim kazanır; çelişki `TAMIRCI-GERIBILDIRIM.md §4`'e yazılır.
- Tasarım-önce: **akış → diyagram → tartış → ekran.** Ekran akıştan önce çizilmez.
- Her karar `KARARLAR.md`'ye işlenir; belirsizlik bırakılmaz.
- İş mantığı/karar/yetki **API'de** — arayüz karar vermez. **Ama arayüz tasarımı birinci sınıf iştir**: ham hex YOK, yalnız `tokens.css` token'ları. MudBlazor yok.
- Kod/DB İngilizce, **UI %100 Türkçe.**
- `KARARLAR.md`'ye yalnız lider dokunur. Ne yaptığını commit mesajında yaz — **gerekçesiyle birlikte** (v1'in en değerli mirası bu alışkanlıktı).
- Türkçe konuş.

## Araçlar
- `tools/saglik-denetim.js` — prototip sağlık denetimi. **Her testi gerçekten yaşanmış bir hata sınıfını** otomatikleştiriyor; ekranları değil hata türlerini test eder, o yüzden yeni prototipte de geçerli.
- `tools/denetim-kos.py gom | geri` — denetimi prototipe geçici gömer/geri alır.
- `tools/pwa-build.js` — `app/` üretimi (çevrimdışı çalışan düz paket).
