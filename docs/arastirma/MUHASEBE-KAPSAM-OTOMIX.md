# Muhasebe kapsamı — OtoMix'e göre, backend'de (Tur 9.3 · 18.10.2026)

> **Cemil, 18.10:** *"Sonrasında otomix'e göre muhasebe alanını hızlıca
> backend'de yaparız."* · *"Muhasebe bekliyorum."*
> **Bu belge kararı hazırlar; seçim Cemil'in** (K-064.3: önce kapsam, sonra kod).
> Dayanak: `OTOMIX-HARITA.md` (modüller) · `MUHASEBE-KARARI.md` (B seçildi:
> kasa + gider) · `KDV.md` · `MALIYET-VE-KAR.md` · `SANAL-POS.md` ·
> `DISA-AKTARMA.md` · MIMARI §1–3.

## 1 · Ölçüt — v1'den beri aynı

Her modül iki soruyla tartılır:

1. **Tamircinin yedi işinden hangisine hizmet ediyor?** (TAMIRCI-GERIBILDIRIM §7)
2. **Ustadan ne kadar yeni giriş istiyor, ve girilmezse rakam yalan söyler mi?**
   (K-013: yanlış rakam gösteren muhasebe, olmamasından kötüdür.)

Tamircinin tek cümlelik hükmü hâlâ çerçeve: *"çok fazla özellik var,
karmaşık geldi"* (T-1). OtoMix muhasebeyi **altı modüle** açıyor.

## 2 · OtoMix'in modülleri tek tek

| OtoMix modülü | Tamiron'da bugün | Yedi işten | Ustadan yeni giriş | Önerilen faz |
|---|---|---|---|---|
| **Kasa** | ✅ var — tahsilat, ödeme yolu | **4** | **sıfır** (teslimden) | **Faz 1** |
| Gider *(Kasa içinde)* | ✅ var — sesle/elle | 4'ün komşusu | her gider bir giriş | **Faz 1** |
| **Cari → müşteri alacağı** | ✅ var — "kimden alacağım" listesi | 4 | sıfır (teslimdeki "hesaba") | **Faz 1** |
| Cari → **parçacıya borç** | ❌ | 5'in sonucu | alış fiyatı (A-M43, var) + ödeme | **Faz 2** |
| Cari → tam bakiye, ekstre, mutabakat | ❌ | **hiçbiri** | açılış, her kısmi ödeme, iade | **dışarıda** |
| **Fatura — gelen (alış)** | ❌ (A-M44 ertelendi) | 3, 5 | fotoğraf → okunur, usta onaylar | **Faz 2** |
| **Fatura — kesmek (e-Arşiv / e-Fatura)** | ❌ (K-022 sınırı) | 6'nın resmî hâli | KDV, tevkifat, müşteri tipi | **Faz 3** — önce soru (§4) |
| **Banka** | ❌ | hiçbiri | hesap hareketleri, eşleştirme | **dışarıda** |
| **Çek / senet** | ❌ | hiçbiri | vade, ciro, tahsil | **dışarıda** |
| **POS** (sanal POS / ödeme linki) | ❌ (A-M20 faz dışı) | 4 | — | **dışarıda** (SANAL-POS.md) |
| **Stok** (çoklu depo) | ❌ | hiçbiri | her giriş-çıkış, sayım | **dışarıda** |
| **Teklif** | ≈ "müşteriye özet" (6) | 6 | — | özet zaten var |
| **SMS** | ✅ var | **7** | — | Faz 1 (backend) |
| **Raporlar** | ✅ her modülün içinde (iş başına kâr) | 3, 4 | — | Faz 1 |
| Kullanıcı yetkilendirme | ❌ | — | kullanıcı/rol | Faz 1, **tek rol** |

**Neden "dışarıda":** dördü de (banka, çek, stok, tam cari) yedi işin hiçbirine
hizmet etmiyor ve ustadan **sürekli** veri istiyor. Girilmediği gün rakam
yalan söyler. OtoMix'te bunlar var çünkü OtoMix **muhasebeciye de** satılıyor;
Tamiron ustaya satılıyor.

## 3 · Öneri — üç faz

### Faz 1 · Backend'in ilk muhasebe sürümü — "hızlıca" kısmı
Prototipte çalışanın **sunucuya taşınması**; yeni yetenek yok.

- **Kasa:** teslimde ödeme yolu (nakit · kart · havale · hesaba), gün, ay ve yıl
  toplamı.
- **Gider:** sesle ve elle; ses yolu K-062 kademesiyle çalışır.
- **Alacak:** teslimde "hesaba" denen işler, kimden ve ne kadar.
- **İş başına kâr:** satış − alış. **Bilinmeyen alış kâra girmez** (K-057).
- **Dışa aktarma:** CSV (TR ayarı) ve yazdır/PDF (DISA-AKTARMA.md).
- **Veri modelinde YER AÇILIR, ekranda yok:** kalemde `kdvOrani`,
  müşteride `tip` (bireysel/şirket), servis kaydında `tevkifat`. Faz 3'te
  ekran gelince veri baştan yazılmaz (KDV.md A-K1).
- Mimari: `Tamiron.Muhasebe.Ui` (RCL, tamirhane ve yedek parça ortak),
  `Tamiron.Application`'da komutlar. İş mantığı API'de; arayüz karar vermez.

### Faz 2 · Maliyet tarafı
- **Gelen fatura / irsaliye fotoğrafı → alış fiyatı.** Okuyucu, ruhsat
  ölçümünün aynısı (FOTO-OLCUM; Gemini 3.8 Flash / Opus). Usta onaylar,
  satıra bağlanır. Kâr rakamı ancak bununla gerçek olur (MALIYET-VE-KAR §4).
- **Parçacıya borç:** alış girilen ve "sonra ödenecek" denen parçalar,
  parçacı başına. Tam cari değil; yalnız "kime ne borcum var".

### Faz 3 · Fatura kesmek — önce soru
e-Arşiv ve e-Fatura için **özel entegratör** sözleşmesi (yıllık ücret +
kontör), KDV ve tevkifat hesabı (KDV.md §5–7), müşteri tipi gerekiyor.
**Pilot tamirciler kendi faturasını nasıl kesiyor?** Mali müşaviri mi kesiyor,
kâğıt mı, başka program mı? Cevap gelmeden bu faz tasarlanmaz.

## 4 · Cemil'e sorular (cevaplar kapsamı kilitler)

1. **"OtoMix'e göre"** neyi kastediyor: OtoMix'in **görünümünü** (modül tahtası,
   ikonlu kutular; bugün prototipte var) mı, yoksa **altı muhasebe modülünün
   hepsini** mi? Bu belge birincisini varsayıyor; ikincisi T-1 ile çelişir.
2. Pilot tamircilerde **fatura kim kesiyor**? (Faz 3'ün varlığı buna bağlı.)
3. Dükkânda **birden çok kullanıcı** var mı (çırak, muhasebeci)? Faz 1'de tek
   rol varsayıldı.
4. **Parçacıya veresiye** yaygın mı? Yaygınsa "parçacıya borç" Faz 1'e çekilir.

## 5 · Faz 1'in "bitti" tanımı (onaylanırsa)

- Prototipteki muhasebe ekranları **aynı rakamları** API'den alır. Eski ve
  yeni yan yana sınanır, 0 fark (Tur 8.3'teki eşdeğerlik yöntemi).
- Bilinmeyen alış kâra girmiyor testi API'de de var (52. testin karşılığı).
- CSV, Excel'de Türkçe karakterle ve doğru sütunlarla açılıyor.
