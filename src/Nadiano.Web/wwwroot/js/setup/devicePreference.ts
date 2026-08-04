export interface PreferredDeviceHint {
  id: string;
  name: string;
}

const STORAGE_KEY = "nadiano.preferredMidiDevice";

/**
 * The preferred device is a hint only — selecting it again on reconnect is
 * attempted, but a missing/changed device must never block the learner
 * (docs/JUNIOR_IMPLEMENTATION_PLAN.md WP-007 step 6).
 */
export function getPreferredDevice(): PreferredDeviceHint | undefined {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) {
      return undefined;
    }

    const parsed = JSON.parse(raw) as unknown;
    if (
      typeof parsed === "object" &&
      parsed !== null &&
      "id" in parsed &&
      "name" in parsed &&
      typeof (parsed as PreferredDeviceHint).id === "string" &&
      typeof (parsed as PreferredDeviceHint).name === "string"
    ) {
      return parsed as PreferredDeviceHint;
    }

    return undefined;
  } catch {
    return undefined;
  }
}

export function setPreferredDevice(hint: PreferredDeviceHint): void {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(hint));
  } catch {
    // Storage may be unavailable (e.g. private browsing) — the preference is only a hint.
  }
}

export function clearPreferredDevice(): void {
  try {
    localStorage.removeItem(STORAGE_KEY);
  } catch {
    // ignore
  }
}
