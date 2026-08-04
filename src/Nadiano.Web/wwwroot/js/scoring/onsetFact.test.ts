import { describe, expect, it } from "vitest";
import type { MatchResult } from "./matcher";
import { buildOnsetFact } from "./onsetFact";

function matched(deviationMs: number) {
  return { status: "matched" as const, expectedGroupId: "m1-v1-n1", pitch: 60, expectedOnsetMs: 0, playedSequence: 0, playedOnsetMs: deviationMs, onsetDeviationMs: deviationMs };
}

describe("buildOnsetFact", () => {
  it("classifies a small deviation as on time", () => {
    const result: MatchResult = { expected: [matched(20)], additions: [] };

    expect(buildOnsetFact(result, 50).deviations[0]).toMatchObject({ deviationMs: 20, band: "onTime" });
  });

  it("classifies a negative deviation beyond tolerance as early", () => {
    const result: MatchResult = { expected: [matched(-100)], additions: [] };

    expect(buildOnsetFact(result, 50).deviations[0]).toMatchObject({ band: "early" });
  });

  it("classifies a positive deviation beyond tolerance as late", () => {
    const result: MatchResult = { expected: [matched(100)], additions: [] };

    expect(buildOnsetFact(result, 50).deviations[0]).toMatchObject({ band: "late" });
  });

  it("treats the tolerance boundary itself as on time (inclusive)", () => {
    const result: MatchResult = { expected: [matched(50)], additions: [] };

    expect(buildOnsetFact(result, 50).deviations[0]).toMatchObject({ band: "onTime" });
  });

  it("ignores omitted expected slots (they have no played onset to deviate from)", () => {
    const result: MatchResult = { expected: [{ status: "omitted", expectedGroupId: "m1-v1-n1", pitch: 60, expectedOnsetMs: 0 }], additions: [] };

    expect(buildOnsetFact(result, 50).deviations).toEqual([]);
  });
});
