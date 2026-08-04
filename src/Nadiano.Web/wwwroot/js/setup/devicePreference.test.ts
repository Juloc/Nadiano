import { beforeEach, describe, expect, it, vi } from "vitest";
import { clearPreferredDevice, getPreferredDevice, setPreferredDevice } from "./devicePreference";

function createMemoryStorage(): Storage {
  const store = new Map<string, string>();
  return {
    getItem: (key) => store.get(key) ?? null,
    setItem: (key, value) => store.set(key, value),
    removeItem: (key) => store.delete(key),
    clear: () => store.clear(),
    key: (index) => Array.from(store.keys())[index] ?? null,
    get length() {
      return store.size;
    },
  } as Storage;
}

describe("devicePreference", () => {
  beforeEach(() => {
    vi.stubGlobal("localStorage", createMemoryStorage());
  });

  it("returns undefined when nothing has been stored yet", () => {
    expect(getPreferredDevice()).toBeUndefined();
  });

  it("round-trips a stored preference", () => {
    setPreferredDevice({ id: "device-1", name: "Test Piano" });

    expect(getPreferredDevice()).toEqual({ id: "device-1", name: "Test Piano" });
  });

  it("clears a stored preference", () => {
    setPreferredDevice({ id: "device-1", name: "Test Piano" });
    clearPreferredDevice();

    expect(getPreferredDevice()).toBeUndefined();
  });

  it("ignores corrupted storage content instead of throwing", () => {
    localStorage.setItem("nadiano.preferredMidiDevice", "{not-json");

    expect(getPreferredDevice()).toBeUndefined();
  });
});
