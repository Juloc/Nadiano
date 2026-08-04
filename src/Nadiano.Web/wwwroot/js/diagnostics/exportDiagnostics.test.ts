import { describe, expect, it } from "vitest";
import { buildDiagnosticsExport } from "./exportDiagnostics";

describe("buildDiagnosticsExport", () => {
  const capabilities = { secureContext: true, midiAvailable: true, audioAvailable: true, indexedDbAvailable: true };
  const midi = { normalizedEventCount: 5, ignoredMessageCount: 1 };

  it("omits the raw device id, keeping only sanitized device fields", () => {
    const result = buildDiagnosticsExport(
      "0.1.0-alpha",
      capabilities,
      { id: "raw-internal-id-should-not-appear", name: "Test Piano", manufacturer: "Acme", state: "connected" },
      midi,
      new Date("2026-01-01T00:00:00.000Z"),
    );

    expect(result.selectedDevice).toEqual({ name: "Test Piano", manufacturer: "Acme", state: "connected" });
    expect(JSON.stringify(result)).not.toContain("raw-internal-id-should-not-appear");
  });

  it("omits selectedDevice entirely when no device is selected", () => {
    const result = buildDiagnosticsExport("0.1.0-alpha", capabilities, undefined, midi, new Date());

    expect(result.selectedDevice).toBeUndefined();
  });

  it("includes app version, capabilities and midi counters, and nothing resembling practice history", () => {
    const result = buildDiagnosticsExport("0.1.0-alpha", capabilities, undefined, midi, new Date());

    expect(result.appVersion).toBe("0.1.0-alpha");
    expect(result.capabilities).toEqual(capabilities);
    expect(result.midi).toEqual(midi);
    expect(result).not.toHaveProperty("events");
    expect(result).not.toHaveProperty("attempts");
  });
});
