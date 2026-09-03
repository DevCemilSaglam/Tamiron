# Prototip sağlık denetimi — 19 test

Prototipe ekran eklendiğinde **her seferinde** çalıştır. Tarayıcıda `prototypes/omurga.html` açık ve 375 px genişlikteyken konsola yapıştır.

| # | Test | Ne yakalar |
|---|---|---|
| 1 | Ekran açılışı | `showScreen()` hata veren ekran |
| 2 | Ölü fonksiyon | `onclick`'te tanımsız fonksiyon çağrısı |
| 3 | Ölü ekran | `go('x')` — hedef ekran yok |
| 4 | Ölü ikon | `<use href="#i-...">` — sembol tanımsız |
| 5 | **Yetim ekran** | Hiçbir yerden gidilmeyen ekran *(bu testte 2 ekran yakalandı)* |
| 6 | 44 px dokunma hedefi | Küçük kalan tıklama alanı |
| 7 | Koyu tema | `rgb(0,0,0)` kalan yazı *(button varsayılanı)* |
| 8 | Yatay taşma | Ekranın yana kayması |
| 9 | **Tanımsız CSS sınıfı** | Kullanılan ama hiçbir kuralda geçmeyen sınıf *(bu testte `.row-btns` yakalandı — 9 ekranda butonlar bozuktu)* |

**İstisnalar (bulgu sayılmaz):** `.tel-inline` (cümle içi, WCAG 2.5.8) · `.search-box` içindeki `input` (sarmalayıcı 48 px).

Betik: `tools/saglik-denetim.js`
