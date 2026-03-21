// OpenHR Service Worker — Enhanced PWA
// Network-first for API, cache-first for static assets, background sync for offline actions.

const CACHE_NAME = 'openhr-v2';
const STATIC_CACHE = 'openhr-static-v2';
const DATA_CACHE = 'openhr-data-v2';
const OFFLINE_URL = '/offline.html';

// Static assets — cache-first strategy
const STATIC_ASSETS = [
    '/',
    '/css/app.css',
    '/css/openhr-theme.css',
    '/css/mobile.css',
    '/css/pwa-enhancements.css',
    '/css/accessibility.css',
    '/css/performance.css',
    '/css/print.css',
    '/js/push-notifications.js',
    '/js/signalr-notifications.js',
    '/js/download.js',
    '/favicon.png',
    OFFLINE_URL
];

// API routes whose responses we cache for offline access
const CACHEABLE_API_ROUTES = [
    '/api/v1/minsida/schema',
    '/api/v1/minsida/ledighet',
    '/api/v1/minsida/lonespecifikationer',
    '/api/v1/notiser',
    '/api/v1/notiser/olasta',
    '/api/v1/self-service/saldon',
    '/api/v1/schema',
    '/api/v1/anstallda'
];

// Offline action queue name (Background Sync)
const SYNC_QUEUE = 'openhr-offline-actions';


// ============================================================
// Install — Pre-cache static assets
// ============================================================
self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(STATIC_CACHE).then(cache => cache.addAll(STATIC_ASSETS))
    );
    self.skipWaiting();
});


// ============================================================
// Activate — Clean up old caches
// ============================================================
self.addEventListener('activate', event => {
    const currentCaches = [CACHE_NAME, STATIC_CACHE, DATA_CACHE];
    event.waitUntil(
        caches.keys().then(keys =>
            Promise.all(
                keys.filter(k => !currentCaches.includes(k)).map(k => caches.delete(k))
            )
        ).then(() => self.clients.claim())
    );
});


// ============================================================
// Fetch — Strategy-based routing
// ============================================================
self.addEventListener('fetch', event => {
    const url = new URL(event.request.url);

    // Skip non-GET requests (POST/PUT/DELETE go to network or background sync)
    if (event.request.method !== 'GET') {
        return;
    }

    // API calls — Network-first, cache-fallback
    if (url.pathname.startsWith('/api/')) {
        event.respondWith(networkFirstWithCache(event.request));
        return;
    }

    // Static assets — Cache-first, network-fallback
    if (isStaticAsset(url.pathname)) {
        event.respondWith(cacheFirstWithNetwork(event.request));
        return;
    }

    // Navigation requests — Network-first, offline page fallback
    if (event.request.mode === 'navigate') {
        event.respondWith(
            fetch(event.request)
                .then(response => {
                    // Cache successful navigation responses
                    const clone = response.clone();
                    caches.open(CACHE_NAME).then(cache => cache.put(event.request, clone));
                    return response;
                })
                .catch(() =>
                    caches.match(event.request).then(cached => cached || caches.match(OFFLINE_URL))
                )
        );
        return;
    }

    // Default — Network with cache fallback
    event.respondWith(
        fetch(event.request).catch(() => caches.match(event.request))
    );
});


// ============================================================
// Network-First Strategy (API calls)
// Cache the response for offline, return cached data if network fails.
// ============================================================
async function networkFirstWithCache(request) {
    const url = new URL(request.url);
    const isCacheable = CACHEABLE_API_ROUTES.some(route => url.pathname.startsWith(route));

    try {
        const response = await fetch(request);

        if (response.ok && isCacheable) {
            const cache = await caches.open(DATA_CACHE);
            cache.put(request, response.clone());
        }

        return response;
    } catch (error) {
        if (isCacheable) {
            const cached = await caches.match(request);
            if (cached) {
                return cached;
            }
        }

        return new Response(
            JSON.stringify({ error: 'Offline', message: 'Ingen internetanslutning. Data visas från cache om tillgängligt.' }),
            { status: 503, headers: { 'Content-Type': 'application/json' } }
        );
    }
}


// ============================================================
// Cache-First Strategy (Static assets)
// ============================================================
async function cacheFirstWithNetwork(request) {
    const cached = await caches.match(request);
    if (cached) {
        // Update cache in background (stale-while-revalidate)
        fetch(request).then(response => {
            if (response.ok) {
                caches.open(STATIC_CACHE).then(cache => cache.put(request, response));
            }
        }).catch(() => { /* ignore network errors for background update */ });
        return cached;
    }

    try {
        const response = await fetch(request);
        if (response.ok) {
            const cache = await caches.open(STATIC_CACHE);
            cache.put(request, response.clone());
        }
        return response;
    } catch {
        return caches.match(OFFLINE_URL);
    }
}


// ============================================================
// Background Sync — Queue offline actions
// ============================================================
self.addEventListener('sync', event => {
    if (event.tag === SYNC_QUEUE) {
        event.waitUntil(processOfflineQueue());
    }
});

async function processOfflineQueue() {
    const cache = await caches.open(DATA_CACHE);
    const requests = await cache.keys();

    const offlineRequests = requests.filter(r =>
        r.url.includes('__offline_queue__')
    );

    for (const request of offlineRequests) {
        try {
            const cachedResponse = await cache.match(request);
            if (!cachedResponse) continue;

            const body = await cachedResponse.json();
            const originalUrl = body._originalUrl;
            const method = body._method || 'POST';

            delete body._originalUrl;
            delete body._method;
            delete body._queuedAt;

            const response = await fetch(originalUrl, {
                method: method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });

            if (response.ok) {
                await cache.delete(request);
                // Notify the client that sync completed
                const clients = await self.clients.matchAll();
                clients.forEach(client => {
                    client.postMessage({
                        type: 'SYNC_COMPLETE',
                        url: originalUrl,
                        success: true
                    });
                });
            }
        } catch {
            // Will retry on next sync event
            console.warn('Offline sync retry pending for:', request.url);
        }
    }
}


// ============================================================
// Push Notifications — Handle incoming push events
// ============================================================
self.addEventListener('push', event => {
    let data = { title: 'OpenHR', body: 'Ny notis', icon: '/favicon.png' };

    if (event.data) {
        try {
            data = Object.assign(data, event.data.json());
        } catch {
            data.body = event.data.text();
        }
    }

    const options = {
        body: data.body,
        icon: data.icon || '/favicon.png',
        badge: '/favicon.png',
        tag: data.tag || 'openhr-notification',
        data: { url: data.url || '/notiser' },
        actions: data.actions || [],
        vibrate: [100, 50, 100],
        requireInteraction: data.requireInteraction || false
    };

    event.waitUntil(
        self.registration.showNotification(data.title, options)
    );
});


// ============================================================
// Notification Click — Navigate to relevant page
// ============================================================
self.addEventListener('notificationclick', event => {
    event.notification.close();

    const url = event.notification.data?.url || '/notiser';

    event.waitUntil(
        self.clients.matchAll({ type: 'window', includeUncontrolled: true }).then(clients => {
            // Focus existing window if possible
            for (const client of clients) {
                if (client.url.includes(self.location.origin) && 'focus' in client) {
                    client.navigate(url);
                    return client.focus();
                }
            }
            // Open new window
            return self.clients.openWindow(url);
        })
    );
});


// ============================================================
// Message Handler — Accept offline action queue from client
// ============================================================
self.addEventListener('message', event => {
    if (event.data && event.data.type === 'QUEUE_OFFLINE_ACTION') {
        const { url, method, body } = event.data;
        queueOfflineAction(url, method, body);
    }

    if (event.data && event.data.type === 'SKIP_WAITING') {
        self.skipWaiting();
    }
});

async function queueOfflineAction(url, method, body) {
    const cache = await caches.open(DATA_CACHE);
    const queueKey = `__offline_queue__/${Date.now()}/${url}`;
    const queuedBody = {
        ...body,
        _originalUrl: url,
        _method: method,
        _queuedAt: new Date().toISOString()
    };

    await cache.put(
        new Request(queueKey),
        new Response(JSON.stringify(queuedBody), {
            headers: { 'Content-Type': 'application/json' }
        })
    );

    // Request background sync
    if ('sync' in self.registration) {
        await self.registration.sync.register(SYNC_QUEUE);
    }
}


// ============================================================
// Helpers
// ============================================================
function isStaticAsset(pathname) {
    return pathname.startsWith('/css/') ||
           pathname.startsWith('/js/') ||
           pathname.startsWith('/fonts/') ||
           pathname.startsWith('/_content/') ||
           pathname.endsWith('.png') ||
           pathname.endsWith('.jpg') ||
           pathname.endsWith('.svg') ||
           pathname.endsWith('.woff2') ||
           pathname.endsWith('.ico');
}
