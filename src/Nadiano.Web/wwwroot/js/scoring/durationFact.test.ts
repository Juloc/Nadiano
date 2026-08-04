import { describe, expect, it } from "vitest";
import type { PlayedMidiEvent } from "../midi/types";
import type { MatchResult } from "./matcher";
import { buildDurationFact } from "./durationFact";
import type { ResolvedExpectedEvent } from "./resolveExpectedEventTiming";

function noteOn(sequence: number, note: number, receivedAtMs: number): PlayedMidiEvent {
  return { sequence, kind: "noteOn", receivedAtMs, channel: 0, note, velocity: 80 };
}

function noteOff(sequence: number, note: number, receivedAtMs: number): PlayedMidiEvent {
  return { sequence, kind: "noteOff", receivedAtMs, channel: 0, note, velocity: 0 };
}

describe("buildDurationFact", () => {
  const expected: ResolvedExpectedEvent[] = [{ groupId: "m1-v1-n1", onsetMs: 0, durationMs: 500, pitch: 60 }];
  const matchResult: MatchResult = {
    expected: [{ status: "matched", expectedGroupId: "m1-v1-n1", pitch: 60, expectedOnsetMs: 0, playedSequence: 0, playedOnsetMs: 0, onsetDeviationMs: 0 }],
    additions: [],
  };

  it("computes a ratio of 1 when the note was held exactly as long as written", () => {
    const played = [noteOn(0, 60, 0), noteOff(1, 60, 500)];

    const fact = buildDurationFact(matchResult, expected, played);

    expect(fact.ratios).toEqual([{ expectedGroupId: "m1-v1-n1", pitch: 60, ratio: 1 }]);
  });

  it("computes a ratio below 1 when the note was released early", () => {
    const played = [noteOn(0, 60, 0), noteOff(1, 60, 250)];

    const fact = buildDurationFact(matchResult, expected, played);

    expect(fact.ratios[0]!.ratio).toBe(0.5);
  });

  it("omits the ratio (does not guess) when no note-off was ever observed", () => {
    const played = [noteOn(0, 60, 0)];

    const fact = buildDurationFact(matchResult, expected, played);

    expect(fact.ratios).toEqual([]);
  });
});
