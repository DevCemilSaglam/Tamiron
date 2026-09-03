# -*- coding: utf-8 -*-
"""Denetimi calistirmak icin omurga.html'e GECICI olarak denetim betigini
   gomer (onizleme bolmesi yalniz bu dosyayi acabiliyor).

   Onizleme bolmesi ~500 KB'lik bir dosyayi acamiyor; omurga buyudukce
   denetim betigi eklenince sinir asiliyordu. Gomerken betigin YORUMLARI
   ATILIR — yorumlar tools/saglik-denetim.js icinde duruyor, calisirken
   gerekmiyorlar.

   kullanim: python tools/denetim-kos.py gom   |   python tools/denetim-kos.py geri
   Sonuc: sayfada window.DENETIM
"""
import io
import os
import re
import shutil
import sys

p = 'prototypes/omurga.html'
yedek = 'prototypes/.omurga.denetim-yedek'

if sys.argv[1] == 'gom':
    shutil.copyfile(p, yedek)
    h = io.open(p, encoding='utf-8').read()
    a = io.open('tools/saglik-denetim.js', encoding='utf-8').read()
    # YALNIZ blok yorumlar atilir. GIRINTI ATILAMAZ: 11. ve 22. testler
    # "sutun 0'daki var" ile ust duzey degiskeni ayirt ediyor; girinti
    # silinince her yerel degisken ust duzey sanildi (28 sahte bulgu).
    a = re.sub(r'/\*[\s\S]*?\*/', '', a)
    a = re.sub(r'\n{3,}', '\n\n', a)
    h = h.replace('</body>',
                  "<script>window.addEventListener('load',function(){try{window.DENETIM="
                  + a + ";}catch(e){window.DENETIM={hata:e.message};}});</scr" + "ipt></body>", 1)
    # Onizleme bolmesinin dosya siniri var ve omurga her turda buyuyor.
    # GECICI kopyadan SATIR BASINDAKI yorum bloklari atilir — bunlar
    # dosyadaki aciklamalar, calisirken gerekmiyorlar. Satir icindeki
    # yorumlara DOKUNULMAZ (string icinde "/*" gecerse bozulmasin).
    # Denetimin kendisi zaten yorumlari ayikliyor, sonuc degismiyor.
    h = re.sub(r'(?m)^[ \t]*/\*[\s\S]*?\*/[ \t]*\n', '', h)
    h = re.sub(r'(?m)^[ \t]*<!--[\s\S]*?-->[ \t]*\n', '', h)
    # GIRINTI TEK BOSLUGA INDIRILIR (01.09). Yorumlari atmak artik
    # yetmiyordu: dosya 513 KB'ye cikip onizleme sinirini asti, sayfa
    # hic acilmadi. Bu uc satir 488 KB'ye indiriyor.
    #
    # DIKKAT — girinti SILINMEZ, TEK BOSLUGA indirilir. 11. ve 22.
    # testler "sutun 0'daki var" ile ust duzey degiskeni ayirt ediyor;
    # girintili satir tek boslukla da olsa SUTUN 0'DA DEGIL, ayrim
    # korunuyor. (Girintiyi tamamen silmek daha once 28 sahte bulgu
    # vermisti.) Dosyada ne ters tirnakli sablon dize ne de <pre> var,
    # satir basi bosluklari anlam tasimiyor.
    h = re.sub(r'(?m)^[ \t]{2,}', ' ', h)
    h = re.sub(r'(?m)^[ \t]*\n', '', h)
    h = re.sub(r'(?m)[ \t]+$', '', h)
    io.open(p, 'w', encoding='utf-8').write(h)

    # SOZDIZIMI KONTROLU. Uretilen HTML dizelerinde kacis hatasi
    # (ornek: onclick="f('x')") butun betik blogunu cokertiyor;
    # sayfa aciliyor ama HICBIR fonksiyon tanimli olmuyor. Denetim de
    # "showScreen is not defined" diye anlamsiz bir hata veriyor.
    # node --check saniyesinde ve kesin soyluyor.
    import re as _re, subprocess, tempfile, os as _os
    bloklar = _re.findall(r'<script>([\s\S]*?)</script>', h)
    if bloklar:
        yol = _os.path.join(tempfile.gettempdir(), 'tamiron-sozdizimi.js')
        io.open(yol, 'w', encoding='utf-8').write(bloklar[-1])
        r = subprocess.run(['node', '--check', yol], capture_output=True, text=True)
        if r.returncode:
            print('SOZDIZIMI HATASI:')
            print((r.stderr or '').strip()[:600])
        else:
            print('sozdizimi temiz')

    print('gomuldu (%d KB)' % (len(h) // 1024))
else:
    shutil.copyfile(yedek, p)
    os.remove(yedek)
    print('geri alindi')
