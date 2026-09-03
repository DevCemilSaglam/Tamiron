# TAMIRON — AI EKİP ÇALIŞMA SİSTEMİ (AI-EKIP-SISTEMI.md) · v1

> Birden fazla yapay zekânın tek projede çakışmadan, hafıza kaybı olmadan çalışmasını sağlayan sistem. Temel gerçek: **AI'lar birbiriyle konuşmaz, oturumlar arası hafıza taşımaz.** Tek köprü, repoda yaşayan ortak dosyalar ve Cemil'in taşıdığı bilgidir.

---

## 0. TASARIM-ÖNCE FAZ + DOKÜMAN DİSİPLİNİ
- **Şu an tasarım-önce fazdayız:** iş akışı/şema → diyagram → ekran. Ajanlar şimdi "modül kodlama" değil; **iş akışı, ekran tasarımı ve araştırma** üretir. Kodlama; akış + ekran + DB oturunca başlar.
- **Doküman disiplini (K-026):** projede yapılan her işlem/karar/değişiklik dokümanlara işlenir; ekran değişince yaşayan diyagram (`IS-AKISI.md`) güncellenir. **Belirsizlik yok.**

---

## 1. ROLLER
| # | Rol | Kim | Görev |
|---|---|---|---|
| 1 | **Proje Lideri** | Claude (sohbet) | Mimari, veri modeli, **tasarım yönü**; işi dilimlere böler, her iş için ajan prompt'u yazar, çıktıyı inceler, onaylar/düzelttirir |
| 2–3 | **Kodlayıcı A/B** | Ajan editör (Claude Code / Cursor / Antigravity) | Verilen prompt'a göre bir modülü/ekranı kodlar |
| 4 | **Test Ajanı** | Ajan editör | Modülün testini bağımsız yazar/çalıştırır |
| — | **Araştırma / Tasarım Ajanı** | Ajan editör | Araştırma (ör. A-11 ürün yapısı) veya ekran tasarımı üretir; lider yönlendirir, sonucu birlikte değerlendiririz |
| — | **Kurye + Karar** | Cemil | Prompt'u ajana verir, çıktıyı lidere getirir, onayları verir |

**Altın kural:** İki ajan aynı anda aynı dosyaya/modüle dokunmaz. İş dikey modül dilimlerine bölünür. Ortak/merkezi kod (çekirdek, tasarım sistemi) önce lider tarafından kurulur.

---

## 2. ORTAK BEYİN DOSYALARI (/docs)
Her AI oturumu bunları okuyarak başlar (sıra önemli):
1. **HANDOFF.md** — session devir: güncel durum + operasyonel dersler (ÖNCE bunu).
2. **MIMARI.md** — teknoloji + mimari (değişmez).
3. **KARARLAR.md** — kararların gerekçeleri + AÇIK KARARLAR.
4. **TASARIM-SISTEMI.md** + **tokens.css** — tasarım sistemi (renk/tip/ölçü tek kaynak).
5. **IS-AKISI.md** — yaşayan iş akışı diyagramı (tasarım-önce omurga).
6. **SOZLUK.md** — Türkçe↔İngilizce terim eşlemesi (K-025).
7. *(oluşunca)* **ILERLEME.md** (iş tahtası, yalnız lider) · **TAMIRON-SARTNAME.md** · **VERI-MODELI.md**.

---

## 3. SESSION DEVAMLILIĞI
Her iş birimi için **YENİ, TEMİZ oturum**. Ortak beyin dosyaları sayesinde sıfırdan başlasa bile nerede kalındığını bilir.
- **Açılış:** "Şu dosyaları oku: HANDOFF, MIMARI, KARARLAR, TASARIM-SISTEMI+tokens, IS-AKISI, SOZLUK (+ oluşunca ILERLEME/VERI-MODELI). Sıradaki görevi al."
- **Kapanış:** kendi branch'ine commit; çıktıyı Cemil'e özetle (lider ILERLEME'yi günceller).

---

## 4. GIT AKIŞI (operasyonel dersler — MUTLAKA uy)
- Tek repo: `tamiron`. `/docs`, `/src`, `/tests`.
- **Her modül kendi dalında** (`feature/...`). İki ajan farklı dalda çalışır.
- **Çalışma düzeni SIRALI:** her iş, bir öncekinin main'e merge'inden sonra güncel main üstüne dallanır → migration zinciri lineer, snapshot çakışması yok.
- **TEK çalışma dizini var; ajanlar branch değiştirerek çalışır.** Lider docs edit'ini ANINDA commit'ler (hedefli `git add docs/...`, asla `-A`). **Ajan aktif/kirli ağaçken lider branch işlemi (checkout/merge) YAPMAZ.** HEAD her turda doğrulanır.
- İş biten dal, lider onayından sonra main'e **FF merge**. `main` her zaman çalışır durumda; yarım iş main'e girmez.
- **İnceleme akışı:** ajan branch'e commit → `git log main..<branch>` + kritik dosyaları oku + `dotnet test` → ONAYLANDI ise `git checkout main && git merge <branch>` (FF) → ILERLEME board + inceleme notu → commit. **PUSH'u lider yapar** (Cemil'e bırakma zorunlu değil).

---

## 5. TDD KURALI
Kodlayıcı, iş kuralını yazmadan testini yazar. Test Ajanı bağımsız doğrular (kör nokta). Testler geçmeden modül "bitti" sayılmaz, main'e girmez. Para/vergi = saf domain servisi, kapsamlı test (MIMARI 3.4).

---

## 6. AJAN PROMPT ŞABLONU (lider üretir)
```
## GÖREV: [modül/ekran adı]
### ÖNCE OKU
docs/HANDOFF.md, docs/MIMARI.md, docs/KARARLAR.md, docs/TASARIM-SISTEMI.md (+tokens.css),
docs/IS-AKISI.md, docs/SOZLUK.md  (+ oluşunca docs/ILERLEME.md, docs/VERI-MODELI.md)
### YAPILACAK
[net, tek modül/ekran, ölçülebilir çıktı]
### KAPSAM DIŞI
[hangi klasör/dosyanın dışına çıkılmayacağı; hangi ortak dosyaların yalnız OKUnacağı]
### KURALLAR
- TenantId + global filter (tenant tablosuysa) · Command/Query ayrı (MediatR)
- Önce test, sonra kod · Soft-delete + RowVersion + denetim alanları
- UI: ilgili aktörün `.Ui` RCL'sine (Tamirhane.Ui / YedekParca.Ui / Bireysel.Ui); ortak bileşen `Ui.Core`, ortak muhasebe `Muhasebe.Ui`. Web↔mobil eşitliği uygulama içinde (K-014/K-015); host'a ekran eklenmez.
- TASARIM: ham hex YOK — yalnız tokens.css token'ları. Tasarım sistemine (TASARIM-SISTEMI.md) uy.
- ILERLEME.md / KARARLAR.md'ye DOKUNMA (yalnız lider — K-005).
### BİTİNCE
- Testleri çalıştır (yeşil göster) · feature/[modül] dalına commit
- Çıktıyı özetle [ne yapıldı / dosyalar / açık sorun] → Cemil lidere iletir.
```

> **DOCS SAHİPLİĞİ (K-005):** `ILERLEME.md` ve `KARARLAR.md` yalnız liderindir; lider her düzenlemeyi anında hedefli commit'ler. Çalışan ajanlar bu iki dosyaya dokunmaz.

---

## 7. TELEFONDAN DEVAM
Claude Code bilgisayarda çalışır, mobil uygulamadan uzaktan komut verilebilir. Bilgisayar açık kalır, telefondan görev/takip. Lider (Claude) mobil sohbetten de erişilebilir.

---

## 8. İŞ DURUM TAHTASI (ILERLEME.md)
Her iş tek satır: `[#12] Teklif ekranı — KODLANIYOR — Ajan-1 — branch: feature/teklif`
Durumlar: **BEKLIYOR · KODLANIYOR · TESTTE · DUZELTMEDE · ONAYLANDI**.

## 9. ÜÇ PROMPT TİPİ (hepsini lider üretir)
1. **Kodlama** (Bölüm 6) → çalışan ajana. 2. **Test** (senaryolar/sınır durumlar/şartname uyumu) → test ajanına. 3. **Düzeltme** (ne yanlış/ne düzeltilecek) → tekrar çalışan ajana.
> Test kriterlerini lider belirler (şartname+mimari); test "kod ne yaptıysa" değil "şartname ne diyorsa" doğrular.

## 10. PARALELLİK
Aynı anda çalışan her ajan farklı modülde (farklı branch/klasör). Çekirdek + tasarım sistemi aşamasında tek çalışan (lider). Modüller ayrıştıktan sonra 2-3 paralel. Modül sayısı paralel ajan sayısını belirler.

## 11. İŞ DÖNGÜSÜ
Lider işi belirler + prompt üretir → Cemil ajana verir (temiz oturum) → ajan kodlar+commit → Cemil lidere getirir → lider test prompt'u / inceleme → ONAYLANDI + merge veya DUZELTMEDE. Darboğaz liderin prompt üretmesi + incelemesidir; ajanlar değiştirilebilir işçiler.
