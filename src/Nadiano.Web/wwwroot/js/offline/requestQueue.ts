export interface QueuedRequest {
  id: string;
  url: string;
  method: "POST";
  body: string;
  contentType: string;
  createdAtUtc: string;
}

const DATABASE_NAME = "nadiano-offline";
const DATABASE_VERSION = 1;
const STORE_NAME = "requests";

function openDatabase(): Promise<IDBDatabase> {
  return new Promise((resolve, reject) => {
    const request = indexedDB.open(DATABASE_NAME, DATABASE_VERSION);
    request.onupgradeneeded = () => {
      const database = request.result;
      if (!database.objectStoreNames.contains(STORE_NAME)) {
        database.createObjectStore(STORE_NAME, { keyPath: "id" });
      }
    };
    request.onsuccess = () => resolve(request.result);
    request.onerror = () => reject(request.error ?? new Error("IndexedDB could not be opened."));
  });
}

function completeTransaction(transaction: IDBTransaction): Promise<void> {
  return new Promise((resolve, reject) => {
    transaction.oncomplete = () => resolve();
    transaction.onerror = () => reject(transaction.error ?? new Error("IndexedDB transaction failed."));
    transaction.onabort = () => reject(transaction.error ?? new Error("IndexedDB transaction was aborted."));
  });
}

export async function queueRequest(request: QueuedRequest): Promise<void> {
  const database = await openDatabase();
  try {
    const transaction = database.transaction(STORE_NAME, "readwrite");
    transaction.objectStore(STORE_NAME).put(request);
    await completeTransaction(transaction);
  } finally {
    database.close();
  }
}

export async function listQueuedRequests(): Promise<QueuedRequest[]> {
  const database = await openDatabase();
  try {
    const transaction = database.transaction(STORE_NAME, "readonly");
    const request = transaction.objectStore(STORE_NAME).getAll();
    const result = await new Promise<QueuedRequest[]>((resolve, reject) => {
      request.onsuccess = () => resolve(request.result as QueuedRequest[]);
      request.onerror = () => reject(request.error ?? new Error("Queued requests could not be read."));
    });
    await completeTransaction(transaction);
    return result.sort((left, right) => left.createdAtUtc.localeCompare(right.createdAtUtc));
  } finally {
    database.close();
  }
}

async function deleteQueuedRequest(id: string): Promise<void> {
  const database = await openDatabase();
  try {
    const transaction = database.transaction(STORE_NAME, "readwrite");
    transaction.objectStore(STORE_NAME).delete(id);
    await completeTransaction(transaction);
  } finally {
    database.close();
  }
}

export async function flushQueuedRequests(): Promise<number> {
  if (!navigator.onLine) {
    return 0;
  }

  let completed = 0;
  for (const request of await listQueuedRequests()) {
    try {
      const response = await fetch(request.url, {
        method: request.method,
        headers: { "Content-Type": request.contentType, "X-Nadiano-Offline-Request": request.id },
        body: request.body,
      });
      if (response.ok || response.status === 409) {
        await deleteQueuedRequest(request.id);
        completed += 1;
      }
    } catch {
      break;
    }
  }
  return completed;
}

export async function postOrQueue(url: string, body: string, id: string): Promise<Response | undefined> {
  try {
    const response = await fetch(url, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body,
    });
    if (response.ok || response.status === 409) {
      return response;
    }
    if (response.status < 500) {
      return response;
    }
  } catch {
    // Network interruption is handled by the durable queue below.
  }

  await queueRequest({
    id,
    url,
    method: "POST",
    body,
    contentType: "application/json",
    createdAtUtc: new Date().toISOString(),
  });
  return undefined;
}
