# Gerçek araç kataloğu — nereden? (18.10.2026)

> **Cemil:** *"Gerçek araç kataloğunu bulabilir miyiz?"*
> Bağlam: K-068, yani şase yoksa parça sorarken marka, model, yıl ve motor
> **seçtiriliyor**. Prototipte liste söyleyiş sözlüğünden geliyor (33 marka,
> 125 model); bu bir katalog değil.
> Önceki araştırma: `A-11-3-TECDOC-VERI.md` (30.08).

## İki ayrı ihtiyaç — karıştırılmamalı

| | Soru | Ne lazım |
|---|---|---|
| **1 · Araç kimliği** | *"Bu araç tam olarak ne?"* | marka → model/tip → yıl → motor listesi (seçim için); şaseden araç tipine çözme |
| **2 · Parça uyumu** | *"Bu araca hangi parça uyar?"* | araç tipi → parça bağı (fitment) |

K-068 yalnız **1'i** istiyor. Uyumu bugün **parçacı** kuruyor: ruhsatı ya da
aracı görüyor, kendi kataloğundan buluyor (T-12b). 2, Cemil'in 30.08'de
*"uyumsuz parçadan kaçınmak"* diye koyduğu hedef ve backend fazının işi.

## Adaylar — ölçülen ve okunan

### A · NHTSA vPIC (ABD, ücretsiz) — ❌ Türkiye için yetmez
**Denendi (18.10).** Gerçek müşteri şasesi gönderilmedi; prototipin uydurma
şaseleri kullanıldı. Sorgu şasenin üretici ve model kısmına bakıyor, seri
numarasına değil.
- `WVWZZZAUZGW…` → **yalnız "VOLKSWAGEN"**, model boş.
- `VF3DDBHYAKS…` → **"PEUGEOT 405, 1989"**, yani **yanlış**.

vPIC yalnız ABD'de satılan araçları tanıyor. Türkiye'deki Avrupa kökenli
filoda markadan ötesini vermiyor; yanlış model vermesi boş vermesinden kötü.

### B · TSB Kasko Değer Listesi — ✅ seçim listesi için en uygun, ⚠️ izin gerekir
- **Ne:** Türkiye Sigorta Birliği her ay **Türkiye pazarındaki** araçları
  listeliyor. Her kayıtta marka kodu, tip kodu, marka adı ve tip adı var;
  tip adının içinde motor ve donanım yazıyor (ör. *"MEGANE 1.5 DCI …"*).
  Model yılları da listede.
  [Resmi sayfa](https://www.tsb.org.tr/tr/kasko-deger-listesi).
- **Artısı:** Türkiye'de satılan her aracı Türkçe ve Türkiye adıyla içeriyor.
  Usta "Egea 1.4 Fire"ı orada bulur.
- **Eksisi:**
  - **Yalnız son ~15 model yılı.** Tamirhanede 20 yaşında araç çok, onlar listede yok.
  - **Şaseden çözme yapmıyor.**
  - **Parça uyumu yok.**
- **⚠️ Hukuk:** Sayfada *"Tüm hakları TSB'ye aittir"* yazıyor. Liste
  sigortacılar için yayınlanıyor. Ürüne gömmek için **TSB'den yazılı izin**
  gerekir. Açık kaynak bir istemci var ([tsb-kasko-mcp](https://github.com/hmtkyn/tsb-kasko-mcp));
  kodu MIT lisanslı ama bu verinin kullanım hakkını vermez.

### C · TecDoc (TecAlliance) — ✅ asıl katalog, 💰 ücretli
- **Ne:** Avrupa'nın sanayi standardı. *Reference Data* paketi araç tiplerini
  (marka → model → tip → motor kodu, yıllar), *OE Parts Data* paketi araç →
  parça bağlarını ve muadil numaraları içeriyor. Eski araçları da kapsıyor.
  (A-11-3 §1)
- **Artısı:** **İki ihtiyacı da** tek kaynakta çözer: seçim listesi ve parça
  uyumu. TecAlliance'ın şaseden araç tipine çözme hizmeti de var; fiyatı
  ve Türkiye kapsamı teklifle öğrenilir.
- **Eksisi:** Ücretli, sözleşmeli. Fiyat teklifle belirleniyor.

### D · Kendi listemiz (sözlükten büyüyen)
Bugünkü hâl. Pilot için yeter mi? **Hayır**: *"hepsini seçtirmemiz
gerekiyor"* kapalı bir liste demek, bizimki ise 33 marka ve 125 model.
Ustanın serbest yazdığıyla kalırsa parçacıya yine belirsiz araç gider.

## Öneri — Cemil onayına açık

1. **Pilot (birkaç tamirci):** **TSB'den izin istemek** — e-posta, maliyetsiz.
   İzin gelirse seçim listesi TSB'den, 15 yaşından eski araçta usta
   serbest yazar ve araç *"listede yok"* diye işaretlenir.
   İzin gelmezse seçim için TecDoc Reference Data teklifi alınır.
2. **Backend fazı:** **TecDoc teklifi** (Reference + OE Parts + VIN servisi).
   Parça uyumu hedefi (30.08) ancak bununla gerçekleşir.
3. **vPIC kullanılmaz.**

> Hiçbiri ölçülmeden ürüne girmez: TSB listesinde 46 doğrulanmış ruhsatın
> araçları bulunuyor mu? Bulunma oranı katalog seçiminin ilk sınavı olur.
