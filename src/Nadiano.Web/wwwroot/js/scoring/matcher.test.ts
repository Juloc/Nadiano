import { describe, expect, it } from "vitest";
import type { PlayedMidiEvent } from "../midi/types";
import { matchEvents } from "./matcher";
import type { ResolvedExpectedEvent } from "./resolveExpectedEventTiming";
import { NORMAL_MODE_POLICY, type ScoringPolicy } from "./ScoringPolicy";

function expectedSlot(overrides: Partial<ResolvedExpectedEvent>): ResolvedExpectedEvent {
  return { groupId: "m1-v1-n1", onsetMs: 0, durationMs: 500, pitch: 60, ...overrides };
}

function noteOn(sequence: number, note: number, receivedAtMs: number): PlayedMidiEvent {
  return { sequence, kind: "noteOn", receivedAtMs, channel: 0, note, velocity: 80 };
}

const policy: ScoringPolicy = { onTimeToleranceMs: 50, matchWindowMs: 200, chordRollToleranceMs: 50 };

describe("matchEvents", () => {
  it("matches a played note at exactly the expected onset with zero deviation", () => {
    const result = matchEvents([expectedSlot({ onsetMs: 1000, pitch: 60 })], [noteOn(0, 60, 1000)], policy);

    expect(result.expected).toEqual([
      { status: "matched", expectedGroupId: "m1-v1-n1", pitch: 60, expectedOnsetMs: 1000, playedSequence: 0, playedOnsetMs: 1000, onsetDeviationMs: 0 },
    ]);
    expect(result.additions).toEqual([]);
  });

  it("never matches a wrong pitch, even at the exact expected time", () => {
    const result = matchEvents([expectedSlot({ onsetMs: 1000, pitch: 60 })], [noteOn(0, 61, 1000)], policy);

    expect(result.expected).toEqual([{ status: "omitted", expectedGroupId: "m1-v1-n1", pitch: 60, expectedOnsetMs: 1000 }]);
    expect(result.additions).toEqual([{ status: "addition", pitch: 61, playedSequence: 0, playedOnsetMs: 1000 }]);
  });

  it("picks the nearest-onset candidate among several same-pitch played notes", () => {
    const played = [noteOn(0, 60, 700), noteOn(1, 60, 1050), noteOn(2, 60, 1400)];

    const result = matchEvents([expectedSlot({ onsetMs: 1000, pitch: 60 })], played, policy);

    expect(result.expected[0]).toMatchObject({ status: "matched", playedSequence: 1, onsetDeviationMs: 50 });
  });

  it("does not let one played note satisfy two expected attacks (repeated notes)", () => {
    const expected = [expectedSlot({ groupId: "m1-v1-n1", onsetMs: 0, pitch: 60 }), expectedSlot({ groupId: "m1-v1-n2", onsetMs: 500, pitch: 60 })];
    const played = [noteOn(0, 60, 10)];

    const result = matchEvents(expected, played, policy);

    expect(result.expected).toEqual([
      { status: "matched", expectedGroupId: "m1-v1-n1", pitch: 60, expectedOnsetMs: 0, playedSequence: 0, playedOnsetMs: 10, onsetDeviationMs: 10 },
      { status: "omitted", expectedGroupId: "m1-v1-n2", pitch: 60, expectedOnsetMs: 500 },
    ]);
  });

  it("matches every note of a chord independently", () => {
    const expected = [
      expectedSlot({ groupId: "m1-v1-n1", onsetMs: 0, pitch: 60 }),
      expectedSlot({ groupId: "m1-v1-n1", onsetMs: 0, pitch: 64 }),
      expectedSlot({ groupId: "m1-v1-n1", onsetMs: 0, pitch: 67 }),
    ];
    const played = [noteOn(0, 67, 5), noteOn(1, 60, 0), noteOn(2, 64, 10)];

    const result = matchEvents(expected, played, policy);

    expect(result.expected.every((outcome) => outcome.status === "matched")).toBe(true);
    expect(result.additions).toEqual([]);
  });

  it("reports an omission when nothing arrives inside the match window", () => {
    const result = matchEvents([expectedSlot({ onsetMs: 1000, pitch: 60 })], [noteOn(0, 60, 5000)], policy);

    expect(result.expected).toEqual([{ status: "omitted", expectedGroupId: "m1-v1-n1", pitch: 60, expectedOnsetMs: 1000 }]);
    expect(result.additions).toEqual([{ status: "addition", pitch: 60, playedSequence: 0, playedOnsetMs: 5000 }]);
  });

  it("reports an unexpected played note as an addition, not attached to any expected slot", () => {
    const result = matchEvents([expectedSlot({ onsetMs: 0, pitch: 60 })], [noteOn(0, 60, 0), noteOn(1, 64, 5)], policy);

    expect(result.additions).toEqual([{ status: "addition", pitch: 64, playedSequence: 1, playedOnsetMs: 5 }]);
  });

  it("ignores non-note-on events entirely (sustain overlap does not affect matching)", () => {
    const played: PlayedMidiEvent[] = [
      { sequence: 0, kind: "controlChange", receivedAtMs: 0, channel: 0, controller: 64, value: 127 },
      noteOn(1, 60, 5),
    ];

    const result = matchEvents([expectedSlot({ onsetMs: 0, pitch: 60 })], played, policy);

    expect(result.expected[0]).toMatchObject({ status: "matched", playedSequence: 1 });
    expect(result.additions).toEqual([]);
  });

  it("produces byte-equivalent (deep-equal) output for the same input on repeated calls", () => {
    const expected = [expectedSlot({ onsetMs: 0, pitch: 60 }), expectedSlot({ groupId: "m1-v1-n2", onsetMs: 500, pitch: 64 })];
    const played = [noteOn(0, 60, 10), noteOn(1, 64, 520), noteOn(2, 71, 9999)];

    const first = matchEvents(expected, played, policy);
    const second = matchEvents(expected, played, policy);

    expect(first).toEqual(second);
  });

  describe("match window boundary (documented, not incidental)", () => {
    it("matches a candidate exactly at the outer edge of the match window (inclusive)", () => {
      const result = matchEvents([expectedSlot({ onsetMs: 1000, pitch: 60 })], [noteOn(0, 60, 1000 + policy.matchWindowMs)], policy);

      expect(result.expected[0]).toMatchObject({ status: "matched" });
    });

    it("does not match a candidate one millisecond beyond the match window", () => {
      const result = matchEvents([expectedSlot({ onsetMs: 1000, pitch: 60 })], [noteOn(0, 60, 1000 + policy.matchWindowMs + 1)], policy);

      expect(result.expected[0]).toMatchObject({ status: "omitted" });
    });

    it("resolves competing expected slots in onset order: the earlier slot wins the only candidate even if a later slot was objectively closer", () => {
      // Slot A (onset 0) and slot B (onset 300) both want pitch 60. One played
      // note at 250 is 250ms from A (inside a 250ms window, at the edge) and
      // only 50ms from B. The matcher assigns candidates to expected slots in
      // onset order, so A — being earlier — claims it first, leaving B
      // omitted despite being the closer match. This is a deliberate,
      // documented tie-break, not an accident of iteration order.
      const widerPolicy: ScoringPolicy = { ...NORMAL_MODE_POLICY, matchWindowMs: 250 };
      const expected = [expectedSlot({ groupId: "a", onsetMs: 0, pitch: 60 }), expectedSlot({ groupId: "b", onsetMs: 300, pitch: 60 })];
      const played = [noteOn(0, 60, 250)];

      const result = matchEvents(expected, played, widerPolicy);

      expect(result.expected).toEqual([
        { status: "matched", expectedGroupId: "a", pitch: 60, expectedOnsetMs: 0, playedSequence: 0, playedOnsetMs: 250, onsetDeviationMs: 250 },
        { status: "omitted", expectedGroupId: "b", pitch: 60, expectedOnsetMs: 300 },
      ]);
    });
  });
});
