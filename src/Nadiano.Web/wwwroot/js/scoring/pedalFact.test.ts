import { describe, expect, it } from "vitest";
import type { PlayedMidiEvent } from "../midi/types";
import { buildPedalFact } from "./pedalFact";

describe("buildPedalFact", () => {
  it("reports on/off observations from CC64 without judging correctness", () => {
    const played: PlayedMidiEvent[] = [
      { sequence: 0, kind: "controlChange", receivedAtMs: 100, channel: 0, controller: 64, value: 127 },
      { sequence: 1, kind: "controlChange", receivedAtMs: 900, channel: 0, controller: 64, value: 0 },
    ];

    expect(buildPedalFact(played).observations).toEqual([
      { atMs: 100, state: "on" },
      { atMs: 900, state: "off" },
    ]);
  });

  it("ignores non-sustain control changes and note events", () => {
    const played: PlayedMidiEvent[] = [
      { sequence: 0, kind: "controlChange", receivedAtMs: 0, channel: 0, controller: 7, value: 100 },
      { sequence: 1, kind: "noteOn", receivedAtMs: 0, channel: 0, note: 60, velocity: 80 },
    ];

    expect(buildPedalFact(played).observations).toEqual([]);
  });
});
