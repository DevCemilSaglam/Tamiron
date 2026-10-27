// G11 · Tema cihazda tutulur (sunucuya gitmez). Seçilmemişse işletim sistemine göre (tokens.css).
// Sayfa çizilmeden ÖNCE uygulanır — açılışta yanlış renk parlaması olmasın.
(function () {
  var ANAHTAR = 'tamiron.tema';
  function uygula(t) {
    if (t === 'light' || t === 'dark') document.documentElement.setAttribute('data-theme', t);
    else document.documentElement.removeAttribute('data-theme');
  }
  try { uygula(localStorage.getItem(ANAHTAR)); } catch (e) { }
  window.tamironTema = {
    al: function () { try { return localStorage.getItem(ANAHTAR) || ''; } catch (e) { return ''; } },
    yaz: function (t) {
      try { if (t) localStorage.setItem(ANAHTAR, t); else localStorage.removeItem(ANAHTAR); } catch (e) { }
      uygula(t);
    }
  };
})();
