# N-9 · Parça Fiyat Toplama — oturum hazırlığı

> 30.08.2026 · **Bu bir TASARIM DEĞİL, hazırlıktır.** Akışın kendisi Cemil ile birlikte oturulacak (K-005).
> Amaç: oturduğumuzda tek seansta bitsin — karar noktaları, alternatifler ve sorular önceden çıkarılmış olsun.
> Bağlı: **N-9** · **A-02** (iki ayrı teklif) · **A-03** (fiyatlama zinciri) · **K-009** (kargo/efektif maliyet) · **K-028** (Yedek Parça asistanı) · **A-11/N-14** (ürün yapısı) · **K-002** (hibrit katalog).

---

## 1 · Bu akış neden zor — üç yapısal özelliği

Diğer beş akıştan farklı üç niteliği var; tasarım bunların üstüne kurulmalı:

**a) Asenkron.** Parçacı anında cevap vermez. Akış **saatlerce/günlerce açık kalır**. Diğer akışlarımızın hepsi araç dükkandayken tamamlanıyordu; bu, **bekleyen iş** kavramını getiriyor — bildirim, hatırlatma, zaman aşımı gerekir.

**b) İki uçlu.** Bu akış aynı zamanda **Yedek Parça uygulamasının** (K-028) karşı yüzü. Tamirhane sorar, parçacı cevaplar — **tek akış, iki aktör, iki arayüz**. Yalnız tamirhane tarafını tasarlarsak diğer ucu sonradan zorlanır.

**c) Karışık dünya.** Tedarikçilerin bir kısmı Tamiron kullanacak, çoğu **kullanmayacak** (WhatsApp/telefon). Akış ikisini **aynı anda** desteklemek zorunda — biri "gerçek", diğeri "geçici" değil; ikisi de kalıcı.

---

## 2 · Kararlaşmış olanlar (tekrar tartışılmaz)

- Parça fiyatı toplama **ayrı bir iştir**, Teklif akışında (Akış 3) düğüm olarak durur — N-9.
- **İki ayrı teklif vardır:** parçacı→tamirhane **tedarik teklifi** ≠ tamirhane→müşteri **satış teklifi** (A-02). Ayrı varlıklar.
- Efektif maliyet **kargo dahil** hesaplanır (K-009) — en ucuz birim fiyat, en ucuz toplam demek değildir.
- Müşteri teklifi **versiyonlanır**; onay dijital veya sözlü olabilir (Akış 3).
- Ana menüde **"Fiyatlandırma"** girişi var, yetki matrisiyle sınırlı (K-006).

---

## 3 · Karar noktaları — oturumda konuşulacaklar

### 3.1 Talep nasıl doğar?
Servis kaydı kalemlerinden mi otomatik, yoksa usta elle mi liste yapar? Ara yol: kalemlerden **öneri** gelir, usta düzenler.

### 3.2 Kime sorulur?
- Dükkanın **kayıtlı tedarikçi listesi** mi (her seferinde seçilir)?
- Yoksa parça tipine göre **otomatik öneri** mi ("fren parçası → şu üç parçacı")?
- **Hepsine birden mi**, sırayla mı?

### 3.3 Nasıl sorulur? — akışın kaderini belirleyen soru
| Yol | Nasıl | Not |
|---|---|---|
| Uygulama içi | Parçacı Tamiron kullanıyorsa doğrudan talep düşer | En temiz; ama **başlangıçta kimse kullanmıyor olacak** |
| WhatsApp | Hazır mesaj + parça listesi gönderilir | Türkiye pratiği bu; **A-05 WhatsApp API** kararına bağlı |
| Telefon | Usta arar, cevabı **kendisi girer** | Her zaman gerekli — en az bu olmalı |

> ⚠️ **Yumurta-tavuk:** uygulama içi yol en değerlisi ama parçacılar Tamiron'a girmeden çalışmaz. **Telefon/elle giriş yolu ilk sürümde şart.**

### 3.4 Cevap nasıl toplanır?
K-028'de zaten var: **WhatsApp'tan gelen fiyat görselini listeye çevirme.** Bu akışın en güçlü katma değeri olabilir — parçacılar fiyatı genelde **ekran görüntüsü / kağıt fotoğrafı** olarak gönderiyor.
Sorular: görsel→liste **hangi aktörde** çalışır (tamirhane mi, parçacı mı, ikisi de)? Yanlış okuma nasıl düzeltilir?

### 3.5 Neye göre karşılaştırılır? — yalnız fiyat değil
Karşılaştırma tablosunun sütunları oturumun asıl çıktısı olacak. Adaylar:
**fiyat · kargo (K-009) · toplam efektif maliyet · stok durumu · teslim süresi · parça sınıfı (orijinal / OES / muadil) · marka · garanti · iade koşulu**

> **Parça sınıfı Türkiye'de kritik:** orijinal ile muadil arasında 2–3 kat fark olabiliyor. Aynı satırda karşılaştırmak yanıltıcı — **sınıf ayrı bir eksen**, fiyatla karıştırılmamalı.

### 3.6 Kim seçer, müşteri görür mü?
- Usta tek başına mı seçer?
- Yoksa müşteriye **seçenek** mi sunulur ("orijinal 3.000 ₺ / muadil 1.200 ₺")? Bu **satış aracı** olur ama teklifi karmaşıklaştırır.
- Seçim müşteri teklifine (A-02 satış tarafı) nasıl aktarılır — maliyet + kâr marjı (A-03) burada devreye giriyor.

### 3.7 Fiyatın ömrü
Parça fiyatları oynak. **Teklif geçerlilik süresi** olacak mı? Süre dolunca ne olur — uyarı mı, yeniden sorma mı?

---

## 4 · Üç alternatif iskelet

### İskelet A · Elle giriş (en yalın)
Usta arar, öğrendiği fiyatı kaleme yazar. **Artı:** bugün çalışır, hiçbir entegrasyon yok. **Eksi:** Tamiron'un burada hiçbir katma değeri yok — sadece bir not defteri.

### İskelet B · Talep–teklif döngüsü (RFQ)
İhtiyaç listesi → çok tedarikçiye talep → cevaplar toplanır → **karşılaştırma tablosu** → seçim → satış teklifine aktarım.
**Artı:** asıl değer burada; karşılaştırma ve maliyet geçmişi oluşur. **Eksi:** parçacılar katılmazsa yarım kalır.

### İskelet C · B + köprüler (**muhtemel doğru cevap**)
B'nin iskeleti, ama üç giriş yolu birden: uygulama içi · WhatsApp (görsel→liste) · **elle giriş**. Hangi yoldan gelirse gelsin **aynı karşılaştırma tablosunda** buluşur.
**Artı:** gerçek dünyaya uyuyor, parçacı katılımından bağımsız çalışıyor, katılım arttıkça kendiliğinden iyileşiyor. **Eksi:** en çok iş.

---

## 5 · Oturum için soru sırası (verimli sıra)

1. **İskelet hangisi?** (A / B / C) — geri kalan her şey buna bağlı.
2. **İlk sürümde hangi giriş yolları?** (elle şart; WhatsApp ve uygulama içi opsiyonel)
3. **Karşılaştırma tablosunun sütunları neler?** — parça sınıfı ayrı eksen mi?
4. **Müşteriye seçenek sunulacak mı?**
5. **Bekleyen talep nasıl görünür?** (ana ekranda sayaç? bildirim? zaman aşımı?)
6. **Parçacı tarafı (K-028) aynı anda mı tasarlanacak, sonra mı?**

---

## 6 · Oturuma girmeden önce bilinmesi gerekenler

- **A-11.1 kararı** (ürün yapısı) bu akışı doğrudan etkiler: karşılaştırma **ürün** üzerinden yapılacaksa katalog gerekir; **serbest metin** üzerinden yapılacaksa katalog gerekmez ama geçmiş/analiz kaybolur.
- **A-05** (WhatsApp kanalı) kararı 3.3'ü belirler.
- **A-03** (fiyatlama zinciri) 3.6'nın ikinci yarısını belirler.

> Yani: **A-11.1 kararı verilmeden bu oturum tam verimli geçmez.** Sıralama önerisi: önce A-11.1, sonra N-9.
