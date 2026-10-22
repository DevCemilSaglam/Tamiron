# TAMIRON — Claude Code Talimatı

Tamiron = tamirhane için servis kaydı + parça tedariki + basit ön muhasebe programı.
**Faz: BACKEND (22.10.2026'dan beri, K-073).** Tasarım/şablon bitti; prototip
(`prototypes/`) artık **davranışın şartnamesi**, `docs/VERI-MODELI.md` v2 **şemanın
şartnamesi**. Backend `src/` + `tests/` altında, yığın `MIMARI.md` §2–§4.

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

## Ürün kuralları (her fazda geçerli)

- **Basitlik ölçülebilir bir kısıttır, temenni değil.** Kullanıcı teknolojiden uzak (T-18). Ana yol için **ekran bütçesi** var (A-2); aşılacaksa önce konuşulur.
- **Yeni özellik varsayılan olarak HAYIR.** Bir şey ancak tamircinin yedi işinden birine hizmet ediyorsa girer (geri bildirim §7). *"İleride lazım olur"* gerekçe değildir.
- **Tamircinin sözü bizim akıl yürütmemizi yener.** Bir tasarım kararı geri bildirimle çelişiyorsa geri bildirim kazanır; çelişki `TAMIRCI-GERIBILDIRIM.md §4`'e yazılır.
- Tasarım-önce: **akış → diyagram → tartış → ekran.** Ekran akıştan önce çizilmez.
- Her karar `KARARLAR.md`'ye işlenir; belirsizlik bırakılmaz.
- İş mantığı/karar/yetki **API'de** — arayüz karar vermez. **Ama arayüz tasarımı birinci sınıf iştir**: ham hex YOK, yalnız `tokens.css` token'ları. MudBlazor yok.
- Kod/DB İngilizce, **UI %100 Türkçe.**
- `KARARLAR.md`'ye yalnız lider dokunur. Ne yaptığını commit mesajında yaz — **gerekçesiyle birlikte** (v1'in en değerli mirası bu alışkanlıktı).
- Türkçe konuş.

## Backend fazının kuralları (K-073)

- **Önce test, sonra kod** (MIMARI §4.4). Para/tarih hesabı = **saf domain servisi**, DB'siz test edilir. Testsiz birleşmez.
- **Prototipteki kural API'ye taşınırken davranışı korunur.** Kaynak: `VERI-MODELI.md` "İş kuralları" tablosu. Prototipteki fonksiyonun davranışı testin beklenen değeridir; davranış değişecekse önce karar.
- **Şemaya alan eklemek = önce `VERI-MODELI.md`.** Prototipe alan girerse derleme denetimi 9 yakalar; backend'de aynı disiplin.
- **Gün-1'de kalabalık yok** (MIMARI §3): yalnız Domain · Application · Infrastructure · Api + testler. UI projeleri, Redis, Docker, Hangfire ihtiyaç anında.
- Gizli bilgi (bağlantı dizgesi parolası, API anahtarı) **depoya girmez**: geliştirmede LocalDB + Windows kimliği; gerisi kullanıcı ortam değişkeni.
- Derle ve test et: `dotnet build Tamiron.sln` · `dotnet test Tamiron.sln`.

## Araçlar
- `tools/saglik-denetim.js` — prototip sağlık denetimi. **Her testi gerçekten yaşanmış bir hata sınıfını** otomatikleştiriyor; ekranları değil hata türlerini test eder, o yüzden yeni prototipte de geçerli.
- `tools/denetim-kos.py gom | geri` — denetimi prototipe geçici gömer/geri alır.
- `tools/pwa-build.js` — `app/` üretimi (çevrimdışı çalışan düz paket).
