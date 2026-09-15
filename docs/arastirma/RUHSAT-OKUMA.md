# Ruhsat okuma — kayıt nasıl açılıyor

> **Durum:** araştırma notu · 03.09.2026 · iş 9
> **Akıştaki yeri:** Adım 1 · Ekran 2 (`IS-AKISI.md`, `EKRANLAR.md`)
> **Kaynak (Cemil):** *"Ruhsatı çekti, kayıt açıldı."* (T-3)

---

## 0 · İki bulgu, önce onlar

Araştırmanın en önemli iki çıktısı, teknoloji seçimiyle ilgili değil.

### ⚠️ Bulgu 1 · **Telefon numarası ruhsatta YOK**

Ruhsat araç ve sahip bilgisi taşıyor — **iletişim bilgisi taşımıyor.** Ama bizim iki işimiz telefona bağlı:

- **Bakım hatırlatması** (K-002, T-17) — telefonsuz gönderilemez
- **Müşteriye özet** (T-19, T-20) — telefonsuz gönderilemez

**Sonuç: Adım 1 ruhsatla bitmiyor.** Telefon ayrıca sorulacak. Bu, Ekran 2'ye **bir alan** ekliyor.

> Öneri: telefon **zorunlu değil.** Boş bırakılabilsin — usta acele ediyorsa iş durmasın. Telefon yoksa hatırlatma ve özet sessizce atlanır (zaten öyle kurgulandı). Sonradan araç kartından eklenir.

### ⚠️ Bulgu 2 · ~~Ruhsatta TC kimlik no ve adres var — bize gerekmiyor~~ · **15.09'da GERİ ALINDI**

> **Cemil, 15.09:** *"Bu alanlar da gerekli. Çünkü bu alanlara fatura kesilecek vb."*
> **Kural değişti (K-050): TC/vergi no ve adres OKUNUYOR ve TUTULUYOR.**

Eski gerekçe silinmedi, çünkü **yanlış değildi — eksikti**:

> *"Yedi işten hiçbiri bunları istemiyor. Okuduğumuzun tamamını
> saklamıyoruz; tutmadığın veri en güvenli veridir."*

Bu cümle **tek bir işe** bakıyordu: araç kaydı açmak. Fatura kesmek
ikinci bir iştir ve **fatura tam olarak bu alanlara kesilir** — kişide
TC kimlik no, firmada vergi no, ikisinde de adres. *"Bize gerekmiyor"*
demek için tamircinin yaptığı her işi bilmek gerekiyordu; bilmiyorduk.

**Değişmeyen üç şey:**

* **Fotoğraf yine saklanmıyor** (A-31). Okunur, alanlar alınır, fotoğraf
  atılır. Alanı tutmakla görüntüyü tutmak ayrı kararlar.
* **Vergi dairesi ruhsatta YOK.** Fatura için gerekiyor ama belgede
  yazmıyor — **usta yazar**. Okuma istemine *"böyle bir alan
  döndürme"* diye yazıldı; olmayan alanı istemek, modeli uydurmaya
  davet etmektir.
* **Okuduğumuzun tamamını saklamıyoruz** kuralı duruyor; genişleyen şey
  listenin kendisi, kuralın kendisi değil.

**Yeni yükümlülük:** bu alanlar tutuluyorsa aydınlatma metni (A-M33)
*"iyi olur"* olmaktan çıkıp **gerekli** hâle geliyor, ve saklama süresi
konuşulmalı (A-M36).

---

## 1 · Ruhsatta ne var

**Araç Tescil Belgesi** (eski adıyla ruhsat). 2018'den beri tescil işlemleri noterlerde.

| Alan | Bize lazım mı |
|---|---|
| **Plaka** | ✅ **Kritik** — kaydın anahtarı |
| **Marka** | ✅ Parçacıya giden mesajda |
| **Tip / ticari adı** (model) | ✅ Aynı |
| **Model yılı** | ✅ Aynı |
| **Şasi no (VIN)** — 17 hane | ✅ Alınır; zorunlu değil |
| **Motor no** | ⚪ Hayır |
| **Silindir hacmi / motor gücü** | ✅ Parçacının doğru parçayı bulması için (T-14) |
| **Renk · yakıt · araç cinsi** | ⚪ Hayır |
| Sahip **adı soyadı** | ✅ Özette geçer |
| Sahip **TC kimlik no** | ⛔ **Saklanmaz** |
| Sahip **adres** | ⛔ **Saklanmaz** |
| Belge seri no · tescil tarihi | ⚪ Hayır |
| Net/azami ağırlık · koltuk sayısı | ⚪ Hayır |
| **Telefon** | ❌ **Ruhsatta yok** — ayrıca sorulacak |

**Okuduğumuz 8 alan, sakladığımız 7.**

> **Karekod ruhsatta değil, PLAKADA.** Yeni karekodlu plakalarda il kodu ile harf grubu arasında 15×15 mm bir kare kod var ve 12 haneli seri numarası taşıyor. **Bu bize kısayol vermiyor** — kodun arkasındaki kayda erişim EGM'de, bizde değil. Ruhsat fotoğrafı okumaktan kaçış yolu değil.

---

## 2 · Üç yol

| | **A · Cihazda OCR** | **B · Bulut OCR** | **C · Görüntü anlayan model** |
|---|---|---|---|
| Ne | ML Kit — telefonda çalışır | Cloud Vision · Textract vb. | Fotoğrafı modele verip **alan alan JSON** istemek |
| Çıktı | **Ham metin satırları** | Ham metin + konum blokları | **Doğrudan alanlar** |
| Alanları kim ayırıyor | **Biz** — kendi ayrıştırıcımız | **Biz** | **Model** |
| Düzen değişirse | **Kırılır** | Kırılır | Dayanır |
| İnternet | **Gerekmez** | Gerekir | Gerekir |
| Ücret | **0** | İlk 1000/ay ücretsiz, sonra $1,50/1000 | Görsel başına, ötekilerden yüksek |
| Kurulum | Mobil SDK | API anahtarı | API anahtarı |

---

## 3 · Maliyet kararı vermiyor — hacim çok küçük

Kritik nokta: **her araç bir kez okunur.** Tanınan araç zaten kayıtlı, ruhsatı tekrar okunmaz (Adım 1: *"araç kayıtlıysa açar"*).

| | |
|---|---|
| Dükkâna günde gelen araç | ~10–30 |
| Bunların **yeni** olanı | birkaç tane |
| Aylık okuma (kaba) | **150–300** |

Cloud Vision'da ilk 1000/ay ücretsiz → **bu hacim bedava.** Görüntü anlayan modelde bile aylık maliyet birkaç dolar.

> **Öyleyse seçim maliyetle yapılmıyor.** İki soru karar veriyor:
> 1. **Buruşmuş, katlanmış, yağlı elle tutulmuş** bir ruhsat, atölye ışığında çekilince okunuyor mu?
> 2. Usta arkasından **kaç alan düzeltiyor?**

---

## 4 · Doğruluk — dürüst hâli

**Türk ruhsatı için doğrulanmış bir doğruluk oranı bulunamadı.** Uydurma rakam yazmıyorum. Ama işin şekli şunu söylüyor:

| | Zorluk |
|---|---|
| **Karakter tanıma** | **Kolay** — basılı Latin metin, OCR'ın en rahat olduğu hâl |
| **Alan eşleme** | **Zor** — hangi metnin hangi alan olduğu; düzen ve etiketler değişebiliyor |

Asıl risk **karakterde değil, eşlemede.** A ve B yollarında bu bizim yazacağımız ayrıştırıcının işi ve düzen değişince kırılır. C yolunda modelin işi.

**En riskli alan: şasi no.** 17 karakter, harf-rakam karışık; `0/O` ve `1/I` bazı yazı tiplerinde karışıyor. (VIN standardında `I`, `O`, `Q` **kullanılmaz** — bu bir doğrulama kuralı olarak işimize yarar.)

---

## 5 · Alanların hepsi eşit değil

Bu, tasarımın anahtarı:

| | Yanlış okunursa |
|---|---|
| **Plaka** | **Kayıt yanlış araca bağlanır** — geri dönüşü zahmetli |
| Marka · model · yıl · motor | Parçacıya yanlış bilgi gider — fark edilir, düzeltilir |
| Şasi | Kimse fark etmez, kimseyi yaralamaz |
| Sahip adı | Özette yanlış isim — utanç verici ama iş durmaz |

> **Yalnız plaka doğrulanır**, gerisi *en iyi çaba*. Plaka biçim denetiminden geçer (`34 ABC 123`) ve **ekranda büyük gösterilir** — usta bir bakışta yanlışı görür.

---

## 6 · Önerim: **C yolu, düzeltilebilir sonuçla**

**Neden C:** A ve B, bizim bir **ruhsat ayrıştırıcısı yazmamızı** gerektiriyor. Ruhsat düzeni değişirse (eski tip / yeni tip / farklı basım) ayrıştırıcı kırılır ve bunu ancak kullanıcı şikâyet edince öğreniriz. C'de bu iş modelin tarafında ve düzen farkına dayanıklı.

Maliyet bu hacimde karar vermiyor (§3), o yüzden **kırılganlığı satın almanın anlamı yok.**

**Nasıl kurulur:**

1. Usta ruhsata telefonu tutar
2. Okunan alanlar **ekranda görünür ve hepsi düzeltilebilir** — otomatik kaydedilmez
3. **Plaka büyük ve üstte** — tek doğrulanan alan
4. **Telefon alanı** boş, isteğe bağlı (Bulgu 1)
5. Okuma tutmadıysa: **"Plakayı yaz"** — kaçış yolu her zaman açık (`IS-AKISI.md` Adım 1)

> **Okuma bir kolaylıktır, ön koşul değildir.** Akış zaten böyle kurulmuştu; araştırma bunu değiştirmiyor, pekiştiriyor.

**A yolu ne zaman anlamlı olur:** İnternetsiz atölye gerçek bir sorunsa, ML Kit **çevrimdışı** çalışıyor ve bedava. Ama o zaman ayrıştırıcı yazma yükü geri geliyor. Şimdilik gerekmiyor.

---

## 7 · AÇIK

| Kod | Karar |
|---|---|
| **A-29** | **Telefon alanı Ekran 2'ye ekleniyor** (Bulgu 1). Zorunlu olmasın — onay? |
| **A-30** | **Gerçek ruhsatla denenmeli.** Buruşuk, yağlı, atölye ışığında çekilmiş birkaç fotoğrafla. Denemeden doğruluk sözü verilmez |
| **A-31** | Ruhsat fotoğrafı **saklanacak mı**? Öneri: **hayır** — TC kimlik no ve adres içeriyor (Bulgu 2). Okunur, alanlar alınır, fotoğraf atılır |
| **A-32** | Şasi no için **VIN doğrulaması** (I/O/Q yok + 17 hane) uygulanacak mı? Ucuz ve yararlı — öneri: evet |
| **A-33** | C yolunda hangi model/servis? Maliyet ve gecikme ölçülmeli |

---

## Kaynaklar

- [EGM · Trafik Tescil Hakkında](https://www.egm.gov.tr/trafik-tescil-hakkinda) — tescil belgesi ve işlemler
- [Ruhsat seri no nerede yazar — Türkiye Sigorta](https://www.turkiyesigorta.com.tr/blog/arac/ruhsat-seri-no-nedir-nerede-yazar) — yeni tip / eski tip belge düzeni farkı
- [Karekodlu plaka — TÜRSAB](https://www.tursab.org.tr/arac-belgelendirme-karekodlu-plaka) · [Karekodlu plaka rehberi 2026](https://turkehliyet.com/blog/karekodlu-plaka-zorunlulugu-degistirme-ucreti-2026) — karekod **plakada**, 15×15 mm, 12 haneli seri
- [Google Cloud Vision — Fiyatlandırma](https://cloud.google.com/vision/pricing) — ilk 1000/ay ücretsiz, sonra $1,50/1000
- [Google ML Kit · Text Recognition](https://developers.google.com/ml-kit/vision/text-recognition/v2/android) — cihazda, çevrimdışı, ücretsiz

> Rakamlar **03.09.2026** tarihlidir.
