const CACHE_VERSION = "nadiano-public-v1";
const PUBLIC_SHELL = [
  "/",
  "/Practice/Beta",
  "/css/site.css",
  "/css/beta.css",
  "/dist/pages/main.js",
  "/dist/pages/beta-practice.js",
  "/manifest.webmanifest",
  "/icons/nadiano-192.png",
  "/icons/nadiano-512.png"
];

self.addEventListener("install", (event) => {
  event.waitUntil(caches.open(CACHE_VERSION).then((cache) => cache.addAll(PUBLIC_SHELL)));
  self.skipWaiting();
});

self.addEventListener("activate", (event) => {
  event.waitUntil(
    caches.keys()
      .then((keys) => Promise.all(keys.filter((key) => key !== CACHE_VERSION).map((key) => caches.delete(key))))
      .then(() => self.clients.claim())
  );
});

function isPrivateRequest(url) {
  return url.pathname.startsWith("/api/library/") ||
    url.pathname.startsWith("/Library") ||
    url.pathname.startsWith("/api/profiles/");
}

function isStaticPublicRequest(url) {
  return url.pathname.startsWith("/css/") ||
    url.pathname.startsWith("/dist/") ||
    url.pathname.startsWith("/icons/") ||
    url.pathname === "/manifest.webmanifest";
}

self.addEventListener("fetch", (event) => {
  const request = event.request;
  const url = new URL(request.url);
  if (request.method !== "GET" || url.origin !== self.location.origin || isPrivateRequest(url) || url.pathname.startsWith("/api/")) {
    return;
  }

  if (isStaticPublicRequest(url)) {
    event.respondWith(
      caches.match(request).then((cached) => {
        const refreshed = fetch(request).then((response) => {
          if (response.ok) {
            const copy = response.clone();
            void caches.open(CACHE_VERSION).then((cache) => cache.put(request, copy));
          }
          return response;
        });
        return cached || refreshed;
      })
    );
    return;
  }

  if (request.mode === "navigate") {
    event.respondWith(
      fetch(request)
        .then((response) => {
          if (response.ok && (url.pathname === "/" || url.pathname === "/Practice/Beta")) {
            const copy = response.clone();
            void caches.open(CACHE_VERSION).then((cache) => cache.put(request, copy));
          }
          return response;
        })
        .catch(async () => (await caches.match(request)) || (await caches.match("/")))
    );
  }
});
