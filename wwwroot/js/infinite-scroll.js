(function(){
  window.InfiniteScroll = {
    observers: {},
    init: function(sentinelId, buttonId) {
      const sentinel = document.getElementById(sentinelId);
      const button = document.getElementById(buttonId);
      if (!sentinel || !button) return;

      // Disconnect existing observer for this sentinel if any
      if (this.observers[sentinelId]) {
        try { this.observers[sentinelId].disconnect(); } catch {}
        delete this.observers[sentinelId];
      }

      const observer = new IntersectionObserver(entries => {
        entries.forEach(entry => {
          if (entry.isIntersecting) {
            // Avoid rapid re-triggering
            if (!button.disabled) {
              button.click();
            }
          }
        });
      }, { root: null, rootMargin: '200px', threshold: 0 });

      observer.observe(sentinel);
      this.observers[sentinelId] = observer;
    },
    dispose: function(sentinelId) {
      const obs = this.observers[sentinelId];
      if (obs) {
        try { obs.disconnect(); } catch {}
        delete this.observers[sentinelId];
      }
    }
  };
})();
