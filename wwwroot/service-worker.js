/*
  Minimal Service Worker for Blazor Server PWA
  - Caches a small set of static assets and an offline fallback page
  - Shows offline.html on navigation requests when the network is unavailable
*/

const CACHE_VERSION = 'dmw-pwa-v3';
const STATIC_CACHE = `static-${CACHE_VERSION}`;

// Keep this list small and only include static, fingerprinted or stable assets
const PRECACHE_ASSETS = [
  '/',
  '/offline.html',
  '/app.css',
  '/DriftMindWeb.styles.css',
  '/bootstrap/bootstrap.min.css',
  '/favicon.png',
  '/favicon.ico',
  '/favicon.svg',
  '/site.webmanifest'
];

self.addEventListener('install', (event) => {
  // Pre-cache core assets for quick load and offline fallback
  event.waitUntil(
    caches.open(STATIC_CACHE).then((cache) => cache.addAll(PRECACHE_ASSETS)).then(() => self.skipWaiting())
  );
});

self.addEventListener('activate', (event) => {
  // Clean up old caches
  event.waitUntil(
    caches.keys().then((keys) =>
      Promise.all(keys.filter((k) => k !== STATIC_CACHE).map((k) => caches.delete(k)))
    ).then(() => self.clients.claim())
  );
});

self.addEventListener('fetch', (event) => {
  const { request } = event;
  // Only handle GET requests
  if (request.method !== 'GET') {
    return;
  }

  // Navigation requests: network-first with offline fallback page
  if (request.mode === 'navigate' || (request.destination === 'document')) {
    event.respondWith(
      fetch(request)
        .then((response) => {
          // Optionally, cache the successful response of the shell route
          return response;
        })
        .catch(async () => {
          const cache = await caches.open(STATIC_CACHE);
          const cached = await cache.match('/offline.html');
          return cached || new Response('Offline', { status: 503, statusText: 'Offline' });
        })
    );
    return;
  }

  // Static assets: cache-first, fall back to network
  if (request.destination === 'style' || request.destination === 'script' || request.destination === 'image' || request.destination === 'font') {
    event.respondWith(
      caches.match(request).then((cached) => {
        if (cached) return cached;
        return fetch(request)
          .then((response) => {
            // Cache a copy of successful responses
            const copy = response.clone();
            caches.open(STATIC_CACHE).then((cache) => cache.put(request, copy)).catch(() => {});
            return response;
          })
          .catch(() => cached);
      })
    );
    return;
  }
  // For everything else, try network, no special handling
});

// Listen for messages to trigger SW updates immediately
self.addEventListener('message', (event) => {
  if (event.data === 'skipWaiting') {
    self.skipWaiting();
  }
});
