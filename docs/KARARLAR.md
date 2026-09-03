# TAMIRON — KARARLAR (KARARLAR.md)

> Verilen kararların gerekçeleri. Aynı tartışma iki kez açılmasın. Yeni karar en üste eklenir. Yalnız lider düzenler, anında commit.
>
> **Bu liste 03.09.2026'da SIFIRDAN başladı.** v1'in ürün/akış kararları bilinçli olarak devralınmadı — tamircinin geri bildirimi (`arastirma/TAMIRCI-GERIBILDIRIM.md`) akışı yeniden kuruyor ve eski kararların gündemde durması yeni tartışmayı kirletiyordu.
>
> **Devralınan mimari kararlar `MIMARI.md`'dedir** ve burada tekrarlanmaz: teknoloji yığını, üç uygulama ayrımı, tenant/yetki altyapısı, soft-delete, katalog hibrit yapısı, Türkçe UI / İngilizce kod, tokens tek kaynak, tasarım-önce. Bunlar akıştan bağımsızdır ve yeniden tartışılmaz — **değişecekse `MIMARI.md` açılır.**
>
> v1 arşivi: `github.com/DevCemilSaglam/Tamiron-v1` (195 commit; her kararın gerekçesi commit mesajında).

---

## AÇIK — karara bağlanacak

> Sıra önemli: **Ç-1 ve Ç-2 tasarımdan önce** kapanmalı, gerisi akış çizilirken.

| Kod | Konu | Durum |
|---|---|---|
| **Ç-1** | **Bakım takibi km mi ay mı?** Öneri: km *kaydedilir* (o anki değer), hatırlatma *ay* üzerinden kurulur. Bkz. geri bildirim §4 | Cemil'e soruldu |
| **Ç-2** | **OEM kodunu kim üretiyor?** Parçacı veriyorsa TecDoc lisansı ana yoldan çıkar — **ürünün en pahalı kalemi buna bağlı**. Bkz. geri bildirim §4 | Cemil'e soruldu |
| **A-1** | **Tamirat / bakım ayrımı** nasıl tutulacak? (tür alanı mı, iki ayrı kayıt mı) — T-15 | Akış çizilirken |
| **A-2** | **Ekran bütçesi** — ana yol kaç ekran? Öneri: **en fazla 6**; istisnalar ayrı sayılır | Cemil onayı |
| **A-3** | Müşteri tarafı (T-19/T-20/T-21) **bu fazda mı**, sonra mı? Link mi, uygulama mı? | Cemil kararı |
| **A-4** | Ruhsat okuma (T-3) hangi teknolojiyle — OCR servisi mi, elle doğrulamalı mı? | Araştırma |
| **A-5** | WhatsApp API (T-12b) — hangi sağlayıcı, maliyet, onay süreci | Araştırma |

---

*(İlk karar buraya yazılacak. Numaralandırma **K-001**'den başlar.)*
