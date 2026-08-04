import { afterEach, describe, expect, it, vi } from "vitest";
import { detectCapabilities } from "./capabilities";

describe("detectCapabilities", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("reports every capability as false, not throw, when window/navigator are absent", () => {
    const result = detectCapabilities();

    expect(result).toEqual({
      secureContext: false,
      midiAvailable: false,
      audioAvailable: false,
      indexedDbAvailable: false,
    });
  });

  it("reports capabilities as true when the browser provides them", () => {
    vi.stubGlobal("window", { isSecureContext: true, AudioContext: class {}, indexedDB: {} });
    vi.stubGlobal("navigator", { requestMIDIAccess: () => Promise.resolve() });

    const result = detectCapabilities();

    expect(result).toEqual({
      secureContext: true,
      midiAvailable: true,
      audioAvailable: true,
      indexedDbAvailable: true,
    });
  });

  it("does not report a secure context on an insecure origin even if other APIs exist", () => {
    vi.stubGlobal("window", { isSecureContext: false, AudioContext: class {}, indexedDB: {} });

    const result = detectCapabilities();

    expect(result.secureContext).toBe(false);
  });
});
