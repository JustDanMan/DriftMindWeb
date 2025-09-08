// Register the service worker for PWA capabilities
(function() {
  if (!('serviceWorker' in navigator)) return;
  // Only register on production hostnames or when served via HTTPS/localhost
  const isLocalhost = Boolean(
    window.location.hostname === 'localhost' ||
    window.location.hostname === '127.0.0.1' ||
    window.location.hostname === '' // VS Code simple browser
  );

  window.addEventListener('load', function() {
    const swUrl = '/service-worker.js';
    navigator.serviceWorker.register(swUrl)
      .then((registration) => {
        // Listen for waiting service worker to prompt immediate activation
        if (registration.waiting) {
          registration.waiting.postMessage('skipWaiting');
        }
        registration.addEventListener('updatefound', () => {
          const newWorker = registration.installing;
          if (!newWorker) return;
          newWorker.addEventListener('statechange', () => {
            if (newWorker.state === 'installed' && navigator.serviceWorker.controller) {
              // New content is available; you could notify the user if desired.
              console.debug('New service worker installed.');
            }
          });
        });
      })
      .catch((err) => console.debug('SW registration failed:', err));
  });
})();
