# -*- coding: utf-8 -*-
"""Google Fonts -> yerel woff2 + app/fonts.css

Neden: PWA cevrimdisi acildiginda yazi tipleri sistem fontuna dusmesin;
prototip her cihazda ayni gorunsun.

Yalniz *latin* ve *latin-ext* alt kumeleri indirilir — Turkce karakterler
(cgiosuCGIOSU) latin-ext icindedir. Fontlar acik lisansli (OFL).
"""
import io, os, re, sys, urllib.request

KOK = os.path.dirname(os.path.abspath(os.path.join(__file__, '..')))
HEDEF = os.path.join(KOK, 'tools', 'fonts')   # KAYNAK: build bunlari base64 gomer
CSS = os.path.join(KOK, 'tools', 'fonts-sablon.css')  # yollar build'de base64'e cevrilir

UA = ('Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 '
      '(KHTML, like Gecko) Version/17.0 Safari/605.1.15')

PROTOTIP = os.path.join(KOK, 'prototypes', 'omurga.html')


def aileleri_bul():
    """Aile listesini PROTOTIPTEN turetir - elle liste tutmak hataya aciktir
    (agirliklar prototiple ayrisirsa fontlar yanlis kalinlikta gorunur)."""
    ham = io.open(PROTOTIP, encoding='utf-8').read()
    m = re.search(r'fonts\.googleapis\.com/css2\?([^"]+)"', ham)
    if not m:
        raise SystemExit('prototipte Google Fonts baglantisi bulunamadi')
    return re.findall(r'family=([^&]+)', m.group(1))


def cek(url, ua=UA):
    istek = urllib.request.Request(url, headers={'User-Agent': ua})
    with urllib.request.urlopen(istek, timeout=30) as r:
        return r.read()


def main():
    os.makedirs(HEDEF, exist_ok=True)
    bloklar = ["/* Tamiron — yerel yazı tipleri (tools/font-indir.py ile üretildi) */",
               "/* Kaynak: Google Fonts · SIL Open Font License */"]
    toplam = 0

    for aile in aileleri_bul():
        ad_gorunen = aile.split(':')[0].replace('+', ' ')
        print('>>', ad_gorunen)
        css = cek('https://fonts.googleapis.com/css2?family=%s&display=swap' % aile).decode('utf-8')

        for blok in css.split('@font-face')[1:]:
            ur = re.search(r'unicode-range:\s*([^;]+);', blok)
            if ur and 'U+0100' not in ur.group(1) and 'U+0000' not in ur.group(1):
                continue                                    # latin/latin-ext disi
            url = re.search(r'url\((https://[^)]+\.woff2)\)', blok)
            fam = re.search(r"font-family:\s*'([^']+)'", blok)
            agr = re.search(r'font-weight:\s*(\d+)', blok)
            if not (url and fam):
                continue

            kume = 'ext' if (ur and 'U+0100' in ur.group(1)) else 'lat'
            dosya = '%s-%s-%s.woff2' % (fam.group(1).replace(' ', ''),
                                        agr.group(1) if agr else '400', kume)
            yol = os.path.join(HEDEF, dosya)
            if not os.path.exists(yol):
                open(yol, 'wb').write(cek(url.group(1)))
            boy = os.path.getsize(yol)
            toplam += boy
            print('    %-34s %5.1f KB' % (dosya, boy / 1024.0))

            govde = blok.replace(url.group(1), 'fonts/' + dosya)
            bloklar.append('@font-face' + govde[:govde.rindex('}') + 1])

    io.open(CSS, 'w', encoding='utf-8', newline='\n').write('\n'.join(bloklar) + '\n')
    print('\ntoplam %.0f KB, %d dosya -> app/fonts.css' % (toplam / 1024.0, len(os.listdir(HEDEF))))


if __name__ == '__main__':
    sys.exit(main())
