(function () {
  var b = document.body;
  var logo = '<svg viewBox="0 0 40 40"><rect width="40" height="40" rx="8" fill="#111"/><path d="M10 30V10h5.2l9.6 13.6V10H30v20h-5.2L15.2 16.4V30H10z" fill="#cf0a2c"/></svg>';
  var top = document.createElement('div');
  top.className = 'top';
  top.innerHTML = '<h1>' + b.dataset.title + (b.dataset.sub ? '<small>' + b.dataset.sub + '</small>' : '') + '</h1><div class="brand">' + logo + 'New Balance Shop</div>';
  b.insertBefore(top, b.firstChild);
  var foot = document.createElement('div');
  foot.className = 'foot';
  foot.innerHTML = '<span><b>New Balance Shop</b> — курсовий проєкт, Team 1</span><span>' + b.dataset.title + '</span>';
  b.appendChild(foot);
})();
