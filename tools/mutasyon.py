"""Backend mutasyon olcumu (BACKEND-KUYRUGU.md, dongu adim 5).

Bir kurali bilerek bozar, `dotnet test` kosar, geri alir. Her mutasyonda en az
bir test KIRMIZI olmali; olmuyorsa o kurali hicbir test korumuyor demektir.

KULLANIM:  python tools/mutasyon.py mutasyonlar.json
  json: [{"ad": "...", "dosya": "src/...cs", "eski": "...", "yeni": "..."}]
Dosya her durumda (hata dahil) eski haline doner.
"""
import io, json, subprocess, sys

K = __file__.replace('\\', '/').rsplit('/tools/', 1)[0]

def kos():
    r = subprocess.run(['dotnet', 'test', 'Tamiron.sln'], cwd=K, capture_output=True,
                       text=True, encoding='utf-8', errors='replace')
    ozet = [l.strip() for l in r.stdout.splitlines() if 'Passed!' in l or 'Failed!' in l]
    return r.returncode == 0, ozet

def main(yol):
    # utf-8-sig: PowerShell'in Out-File'i dosya basina BOM koyar; json onu reddederdi.
    mutlar = json.load(io.open(yol, encoding='utf-8-sig'))
    # TABAN ÖNCE (24.10): taban kırmızıyken her mutasyon "yakalandı" görünür — ölçüm yalan
    # söyler. F7'de oldu: VoiceSchemaTests zaten kırmızıydı, 11/11 "yakalandı" çıktı.
    gecti, ozet = kos()
    if not gecti:
        print('TABAN KIRMIZI — mutasyon koşulmadı:', ' | '.join(ozet)); sys.exit(2)
    yakalanmayan = 0
    for m in mutlar:
        p = K + '/' + m['dosya']
        s0 = io.open(p, encoding='utf-8', newline='').read()
        eski = m['eski'] if m['eski'] in s0 else m['eski'].replace('\n', '\r\n')
        if eski not in s0:
            print('DESEN YOK:', m['ad']); yakalanmayan += 1; continue
        try:
            io.open(p, 'w', encoding='utf-8', newline='').write(s0.replace(eski, m['yeni'], 1))
            gecti, ozet = kos()
            durum = 'YAKALANMADI' if gecti else 'yakalandi'
            if gecti: yakalanmayan += 1
            print(f"{durum:12} {m['ad']}  {' | '.join(ozet)}")
        finally:
            io.open(p, 'w', encoding='utf-8', newline='').write(s0)
    gecti, ozet = kos()
    print('GERI', 'yesil' if gecti else 'KIRMIZI', ' | '.join(ozet))
    sys.exit(1 if yakalanmayan or not gecti else 0)

if __name__ == '__main__':
    main(sys.argv[1])
