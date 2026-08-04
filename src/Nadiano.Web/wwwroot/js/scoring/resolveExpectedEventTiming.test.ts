import { describe, expect, it } from "vitest";
import { resolveExpectedEventTiming } from "./resolveExpectedEventTiming";
import type { ExpectedEventDocument } from "./types";

function doc(overrides: Partial<ExpectedEventDocument> = {}): ExpectedEventDocument {
  return {
    schemaVersion: 1,
    timeBase: "beats",
    tempoMap: [{ beat: 0, bpm: 120 }],
    events: [],
    ...overrides,
  };
}

describe("resolveExpectedEventTiming", () => {
  it("converts beat 0 of measure 1 to the session start time", () => {
    const document = doc({ events: [{ id: "m1-v1-n1", measure: 1, beat: 0, durationBeats: 1, pitches: [60] }] });

    const resolved = resolveExpectedEventTiming(document, 4, 1000);

    expect(resolved[0]).toMatchObject({ onsetMs: 1000, pitch: 60, groupId: "m1-v1-n1" });
  });

  it("accounts for full measures already elapsed", () => {
    const document = doc({ events: [{ id: "m2-v1-n1", measure: 2, beat: 0, durationBeats: 1, pitches: [60] }] });

    // 120 bpm => 500ms/beat, 4 beats/measure => 2000ms per measure.
    const resolved = resolveExpectedEventTiming(document, 4, 0);

    expect(resolved[0]!.onsetMs).toBe(2000);
  });

  it("converts duration in beats to milliseconds using the tempo", () => {
    const document = doc({ events: [{ id: "m1-v1-n1", measure: 1, beat: 0, durationBeats: 2, pitches: [60] }] });

    const resolved = resolveExpectedEventTiming(document, 4, 0);

    expect(resolved[0]!.durationMs).toBe(1000);
  });

  it("expands a chord into one resolved entry per pitch, all sharing the group id and onset", () => {
    const document = doc({ events: [{ id: "m1-v1-n1", measure: 1, beat: 0, durationBeats: 1, pitches: [60, 64, 67] }] });

    const resolved = resolveExpectedEventTiming(document, 4, 0);

    expect(resolved).toHaveLength(3);
    expect(resolved.map((r) => r.pitch)).toEqual([60, 64, 67]);
    expect(resolved.every((r) => r.groupId === "m1-v1-n1" && r.onsetMs === 0)).toBe(true);
  });

  it("defaults to 120bpm when no tempo map entry is present", () => {
    const document = doc({ tempoMap: [], events: [{ id: "m1-v1-n1", measure: 1, beat: 1, durationBeats: 1, pitches: [60] }] });

    const resolved = resolveExpectedEventTiming(document, 4, 0);

    expect(resolved[0]!.onsetMs).toBe(500);
  });
});
