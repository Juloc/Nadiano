import type { ExpectedEventDocument } from "../scoring/types";

function element<T extends HTMLElement>(id: string): T | undefined {
  return document.getElementById(id) as T | null ?? undefined;
}

async function initFingeringCues(): Promise<void> {
  const workspace = element<HTMLElement>("practice-workspace");
  const section = element<HTMLElement>("workspace-fingering-section");
  const list = element<HTMLUListElement>("workspace-fingering-list");
  const expectedEventsUrl = workspace?.dataset.expectedEventsUrl;
  if (!workspace || !section || !list || !expectedEventsUrl) {
    return;
  }

  try {
    const response = await fetch(expectedEventsUrl, { headers: { Accept: "application/json" } });
    if (!response.ok) {
      return;
    }

    const eventDocument = await response.json() as ExpectedEventDocument;
    const eventsWithFingering = eventDocument.events.filter((event) => (event.fingering?.length ?? 0) > 0);
    if (eventsWithFingering.length === 0) {
      return;
    }

    const indonesian = document.documentElement.lang.startsWith("id");
    const rows = eventsWithFingering.map((event) => {
      const item = document.createElement("li");
      const hand = event.hand === "left"
        ? (indonesian ? "tangan kiri" : "linke Hand")
        : event.hand === "right"
          ? (indonesian ? "tangan kanan" : "rechte Hand")
          : (indonesian ? "kedua tangan" : "beide Hände");
      item.textContent = `${event.id} · ${indonesian ? "birama" : "Takt"} ${event.measure} · ${hand} · ${indonesian ? "jari" : "Finger"} ${event.fingering?.join("-")}`;
      return item;
    });

    list.replaceChildren(...rows);
    section.hidden = false;
  } catch {
    section.hidden = true;
  }
}

void initFingeringCues();
