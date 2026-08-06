import { flushQueuedRequests } from "../offline/requestQueue";

function markAppReady(): void {
  document.documentElement.dataset.nadianoReady = "true";
}

async function registerServiceWorker(): Promise<void> {
  if (!("serviceWorker" in navigator) || !window.isSecureContext) {
    return;
  }

  try {
    await navigator.serviceWorker.register("/service-worker.js", { scope: "/" });
  } catch {
    // PWA installation is optional; browser use remains available without registration.
  }
}

function scheduleQueueFlush(): void {
  const flush = (): void => { void flushQueuedRequests(); };
  window.addEventListener("online", flush);
  document.addEventListener("visibilitychange", () => {
    if (document.visibilityState === "visible") {
      flush();
    }
  });
  flush();
}

markAppReady();
void registerServiceWorker();
scheduleQueueFlush();
