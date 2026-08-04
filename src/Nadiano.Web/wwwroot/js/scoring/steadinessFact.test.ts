import { describe, expect, it } from "vitest";
import type { MatchResult } from "./matcher";
import { buildSteadinessFact } from "./steadinessFact";

function matched(expectedOnsetMs: number, playedOnsetMs: number) {
  return { status: "matched" as const, expectedGroupId: `m${expectedOnsetMs}`, pitch: 60, expectedOnsetMs, playedSequence: 0, playedOnsetMs, onsetDeviationMs: 0 };
}

describe("buildSteadinessFact", () => {
  it("returns null when fewer than 3 notes matched (too little data to judge)", () => {
    const result: MatchResult = { expected: [matched(0, 0), matched(500, 500)], additions: [] };

    expect(buildSteadinessFact(result).intervalVariability).toBeNull();
  });

  it("reports zero variability for perfectly even playing", () => {
    const result: MatchResult = { expected: [matched(0, 0), matched(500, 500), matched(1000, 1000), matched(1500, 1500)], additions: [] };

    expect(buildSteadinessFact(result).intervalVariability).toBe(0);
  });

  it("reports higher variability for uneven playing", () => {
    const result: MatchResult = { expected: [matched(0, 0), matched(500, 480), matched(1000, 1050), matched(1500, 1490)], additions: [] };

    const fact = buildSteadinessFact(result);

    expect(fact.intervalVariability).not.toBeNull();
    expect(fact.intervalVariability!).toBeGreaterThan(0);
  });

  it("orders intervals by expected onset, not by array/insertion order", () => {
    const result: MatchResult = { expected: [matched(1000, 1000), matched(0, 0), matched(500, 500)], additions: [] };

    expect(buildSteadinessFact(result).intervalVariability).toBe(0);
  });
});
