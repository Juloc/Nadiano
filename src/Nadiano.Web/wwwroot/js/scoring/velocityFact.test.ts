import { describe, expect, it } from "vitest";
import type { PlayedMidiEvent } from "../midi/types";
import type { MatchResult } from "./matcher";
import { buildVelocityFact } from "./velocityFact";

function noteOn(sequence: number, velocity: number): PlayedMidiEvent {
  return { sequence, kind: "noteOn", receivedAtMs: 0, channel: 0, note: 60, velocity };
}

function matched(playedSequence: number) {
  return { status: "matched" as const, expectedGroupId: "m1-v1-n1", pitch: 60, expectedOnsetMs: 0, playedSequence, playedOnsetMs: 0, onsetDeviationMs: 0 };
}

describe("buildVelocityFact", () => {
  it("computes min, max and average velocity across matched notes", () => {
    const played = [noteOn(0, 40), noteOn(1, 100), noteOn(2, 70)];
    const result: MatchResult = { expected: [matched(0), matched(1), matched(2)], additions: [] };

    expect(buildVelocityFact(result, played)).toEqual({ category: "dynamics", minVelocity: 40, maxVelocity: 100, averageVelocity: 70 });
  });

  it("ignores additions and omissions, using only matched notes", () => {
    const played = [noteOn(0, 50), noteOn(1, 127)];
    const result: MatchResult = {
      expected: [matched(0), { status: "omitted", expectedGroupId: "m1-v1-n2", pitch: 64, expectedOnsetMs: 500 }],
      additions: [{ status: "addition", pitch: 71, playedSequence: 1, playedOnsetMs: 900 }],
    };

    expect(buildVelocityFact(result, played)).toEqual({ category: "dynamics", minVelocity: 50, maxVelocity: 50, averageVelocity: 50 });
  });

  it("returns nulls rather than guessing when nothing matched", () => {
    const result: MatchResult = { expected: [], additions: [] };

    expect(buildVelocityFact(result, [])).toEqual({ category: "dynamics", minVelocity: null, maxVelocity: null, averageVelocity: null });
  });
});
