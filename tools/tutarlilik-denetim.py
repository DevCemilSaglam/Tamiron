# -*- coding: utf-8 -*-
"""21 ekran <-> IS-AKISI <-> PROTOTIP-SENARYO <-> VERI-MODELI <-> SOZLUK"""
import io, re, sys
sys.stdout.reconfigure(encoding='utf-8')

def oku(p):
    return io.open(p, encoding='utf-8').read().replace('\r\n', '\n')

def govde(metin):
    """Surum notlari TARIHSEL kayittir — denetimde haric tutulur."""
    for bas in (chr(35)*2 + " Surum notlari", chr(35)*2 + " " + chr(83) + "ürüm notları",
                chr(35)*2 + " 📝 SÜRÜM NOTLARI"):
        if bas in metin:
            return metin.split(bas)[0]
    return metin

proto = oku('prototypes/omurga.html')
akis = oku('docs/IS-AKISI.md')
sen = oku('docs/PROTOTIP-SENARYO.md')
vm = oku('docs/VERI-MODELI.md')
sz = oku('docs/SOZLUK.md')

bulgu = []
def ek(t, d): bulgu.append(t + ' | ' + d)

# ---- 1) ekranlar ----
ekranlar = re.findall(r'id="screen-([a-z-]+)"', proto)
print('PROTOTIP: %d ekran' % len(ekranlar))

# ekran -> senaryoda gecen Turkce adi (kaba esleme)
ad = {
 'home': ['Ana menü', 'Ana ekran'], 'araclar': ['Araçlar listesi'], 'kisiler': ['Kişiler listesi'],
 'plaka': ['Araç Giriş', 'plaka'], 'arac-karti': ['Araç Detayı'], 'gecmis': ['Geçmiş'],
 'yeni-arac': ['Yeni Araç'], 'servis': ['Servis Kaydı'], 'parca-onay': ['Parça listesi', 'Parça Listesi'],
 'fiyat': ['Fiyat Karşılaştırma', 'Fiyat karşılaştırma'], 'islemde': ['İşleme Alındı', 'İşleme alındı'],
 'yaklasan': ['Yaklaşan'], 'acik-isler': ['Açık İşler'], 'fiyatlandirma': ['Fiyatlandırma'],
 'parca-giris': ['Parça Girişi', 'Parça girişi'], 'muhasebe': ['Muhasebe'],
 'hazir': ['Araç Hazır', 'Araç hazır'], 'teslim': ['Ödeme ve Teslim', 'Ödeme ve teslim'],
 'teslim-tamam': ['Teslim Edildi', 'Teslim edildi'], 'onay': ['Onaya Gönderildi'],
 'kapandi': ['Kapandı', 'kapandı'],
}
for e in ekranlar:
    adaylar = ad.get(e, [e])
    if not any(a in sen for a in adaylar):
        ek('SENARYODA-YOK', e)

# ---- 2) ekran sayisi dokumanlarda ----
for dosya, metin in [('SENARYO', sen), ('IS-AKISI', akis)]:
    sayilar = re.findall(r'\*\*(\d+) ekran\*\*', govde(metin))
    yanlis = [s for s in sayilar if s != str(len(ekranlar))]
    if yanlis:
        ek('EKRAN-SAYISI-ESKI', dosya + ': ' + ', '.join(sorted(set(yanlis))) + ' (gercek ' + str(len(ekranlar)) + ')')

# ---- 3) SOZLUK <-> VERI-MODELI ----
kod_adlari = set(re.findall(r'\|\s*\*{0,2}([A-Z][A-Za-z]+(?:\.[A-Za-z]+)?)\*{0,2}\s*\|', sz))
# Henuz MODELLENMEMIS (ileride gelecek) — eksik degil, sirasi gelmemis
atla = {'Not', 'Türkçe', 'UI', 'DB', 'AppUser', 'Branch', 'SupplierOffer', 'Sahip',
        'Quote', 'StockItem', 'Role', 'Permission', 'Tenant', 'Bringer'}
for k in sorted(kod_adlari - atla):
    kok = k.split('.')[0]
    if kok not in vm:
        ek('SOZLUKTE-VAR-MODELDE-YOK', k)

# VERI-MODELI'ndeki ana varliklar SOZLUK'te var mi
varliklar = re.findall(r'^([A-Z][A-Za-z]+)\s{2,}#', vm, re.M)
for v in sorted(set(varliklar)):
    if v not in sz:
        ek('MODELDE-VAR-SOZLUKTE-YOK', v)

# ---- 4) prototipteki durum adlari modelde gecıyor mu ----
durumlar = ['Fiyat bekliyor', 'Parça bekliyor', 'Onay bekliyor', 'Tamamlandı', 'Bilgi verildi']
for d in durumlar:
    if d in proto and d not in vm:
        ek('DURUM-MODELDE-YOK', d)

# ---- 5) eski terim kalintisi ----
for dosya, metin in [('PROTOTIP', proto), ('IS-AKISI', akis), ('SENARYO', sen), ('VERI-MODELI', vm), ('SOZLUK', sz)]:
    for i, l in enumerate(govde(metin).split('\n'), 1):
        if re.search(r'ziyaret türü|tür alt-akış', l) and '~~' not in l and 'DEĞİL' not in l and 'değil' not in l:
            ek('ESKI-TERIM', dosya + ':' + str(i) + ' — ' + l.strip()[:70])

print('\n=== BULGULAR: %d ===' % len(bulgu))
for b in bulgu:
    print('  ' + b)
if not bulgu:
    print('  temiz')
