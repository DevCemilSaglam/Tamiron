# TAMIRON — ÇALIŞMA KUYRUĞU (CALISMA-KUYRUGU.md)

> **Ne işe yarar:** Cemil bilgisayarda değilken lider tek başına
> çalışıyor. Bu dosya **sıradaki işi** ve **hangi işte kalındığını**
> tutar. HANDOFF *durumu* söyler, bu dosya *sırayı*.
>
> **Kural:** bir iş bitmeden bir sonrakine geçilmez. Her iş bitince
> · derleme denetimi · 375 px'te sağlık denetimi (0 bulgu) · commit
> · push · bu dosyada **BİTTİ** işareti + tek satır sonuç.
>
> **Karar gerektiren yerde durulmaz:** varsayım yazılır, *"Cemil
> onayına açık"* işaretlenir, iş bitirilir. Yalnız yanlış varsayım
> işi çöpe çıkaracaksa durulur ve buraya **BEKLİYOR** yazılır.

Başlangıç: 04.09.2026 · son commit `4a6dfe5`

---

## Sıra

| # | İş | Durum | Sonuç |
|---|---|---|---|
| 1 | **Ölü uç turu** — 12 ekranın hepsini gerçekten işlet | **BİTTİ** | 3 bulgu: muhasebede elle gider yok · "Onay iste" sahte · `Ekran.ac()` zaten açık ekranda ölü. Üçü de düzeltildi, 30. test eklendi (K-021) |
| 2 | **A-M2** — özet/hatırlatma ikinci iletişime gitsin | **BİTTİ** | Hedef seçilebilir, kime gittiği göndermeden önce yazıyor; firmaya "Bey/Hanım" denmiyor (K-021.3) |
| 3 | **Muhasebe kapsamı (MUH)** — üç seçeneği ölç, öneri + karar taslağı | **BİTTİ** | `arastirma/MUHASEBE-KAPSAMI.md` + K-022 taslağı. Öneri B (kasa+gider), cari hesap kapsam dışı. Parça alışı yeni alan istemeden eklendi (7.160 ₺ ölçüldü) + karar bütçesi kısıtı önerildi |
| 4 | **Müşteri özeti (T-19/20/21)** — WhatsApp'tan gidecek özet | **BİTTİ** | `ozet` ekranı: müşterinin gördüğü salt-okunur sayfa. Mesaj artık liste değil **link** taşıyor. Ustanın önizlemesi mesaj ekranından (K-023) |
| 5 | **Sesli işlem kapsam denetimi** — K-016 gerçekten tutuyor mu | **BİTTİ** | Tutuyor (9/9 işin elle yolu var) ama **denetimin kendisi bozuktu**: ölçtüğü durumu kirletiyordu. Düzeltilince özet ekranında gizli ölü uç çıktı. K-018 üçüncü kez (31. test) (K-024) |
| 6 | **Dar ve geniş ekran ölçümü** — 320 px ve 430 px | SIRADA | |
| 7 | **Belge tazeleme** — HANDOFF · EKRANLAR · IS-AKISI + dokunuş sayımı | SIRADA | |

---

## 1 · Ölü uç turu

**Neden:** 27. test *"ekranda hiç eylem yok"* diyor. Söylemediği şey:
**eylem VAR ama işi ilerletmiyor.** Araç ekranı tam böyleydi ve ancak
elle işletilince bulundu. On iki ekranın hepsi açılıp **işletilecek**.

**Nasıl:** her ekranda her düğmeye/çipe/alana dokun, nereye gittiğini
yaz. Gitmiyorsa ya da geldiği yere geri atıyorsa bulgu.

**Çıktı:** bulgular düzeltilir; yeni bir hata sınıfı çıkarsa denetime
testi eklenir ve **negatif testle** doğrulanır.

## 2 · A-M2 — özet ve hatırlatma hangi numaraya

**Varsayım:** ikinci iletişim varsa **ona** gider (aracı getiren o),
yoksa ruhsattaki numaraya. Mesaj ekranı şu an hep ruhsattakini
kullanıyor.

**Çıktı:** mesaj ekranı hedef numarayı **gösterir ve seçtirir** —
usta yanlış kişiye özet göndermesin.

## 3 · Muhasebe kapsamı (MUH) — Cemil'in en acil kararı

Seçenekler: **günlük kasa** · **kasa + gider** · **cari hesap.**
Referans: `arastirma/PARASUT-HARITA.md` · `arastirma/LUCANET-ONE-HARITA.md`.

**Ölçüt yedi iş:** bir kalem yedi işten birine hizmet etmiyorsa girmez.
Asıl soru *"ne kadar muhasebe"* değil, **"usta ne kadar veri girer"** —
girilmeyen veriyle çıkan rakam yalan söyler (K-013).

**Çıktı:** karşılaştırma belgesi + **öneri** + K-021 taslağı
(Cemil onayına açık). Kod tarafında yalnız **zaten kayıtlı veriden**
çıkan şey yapılır.

## 4 · Müşteri özeti (T-19/20/21)

Bugün tamirci müşteriye **PDF** gönderiyor: iş · parça açıklaması ·
toplam tutar. Cemil *"WhatsApp'tan uygulama linki"* fikrini beğendi
(A-2 önerisi: **özet linki evet, müşteri uygulaması sonra**).

**Çıktı:** salt-okunur özet — kaydın satırları, toplam, dükkân bilgisi.
Ekran bütçesine dikkat: bu **ustanın ekranı değil**, müşterinin gördüğü
sayfa.

## 5 · Sesli işlem kapsam denetimi

K-016: *"ses var olan işi kısaltır, yeni yetenek açmaz."* Her niyet
(`genel · servis-ekle · kayitlar · araclar · musteriler · mesaj ·
muhasebe`) işletilip **elle de yapılabiliyor mu** diye bakılacak.
Yapılamıyorsa ya elle yolu açılır ya ses kısılır.

## 6 · Dar ve geniş ekran ölçümü

Bütün ölçümler 375 px'te yapıldı. 320 px (küçük telefon) ve 430 px
(büyük telefon) ölçülmedi. Denetim ikisinde de koşulacak.

## 7 · Belge tazeleme

HANDOFF durumu · EKRANLAR sayımı · IS-AKISI dokunuş ölçümü yeniden.
**Ölçülmemiş sayı belgeye yazılmaz.**
