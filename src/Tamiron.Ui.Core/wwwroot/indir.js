/* G6 · API'den gelen dosyayı tarayıcıya kaydettirir (prototip muhCsv ile aynı yol: Blob + a.download).
   Oturum anahtarı başlıkta gittiği için düz <a href> indiremez — baytları Blazor getirir, bu kaydeder. */
window.tamironIndir = function (ad, base64) {
  var b = atob(base64), u = new Uint8Array(b.length);
  for (var i = 0; i < b.length; i++) u[i] = b.charCodeAt(i);
  var url = URL.createObjectURL(new Blob([u], { type: 'text/csv;charset=utf-8' }));
  var a = document.createElement('a');
  a.href = url; a.download = ad;
  document.body.appendChild(a); a.click(); a.remove();
  setTimeout(function () { URL.revokeObjectURL(url); }, 4000);
};
