import { describe, expect, it } from "vitest";
import type { MatchResult } from "./matcher";
import { buildPitchFact } from "./pitchFact";

describe("buildPitchFact", () => {
  it("counts matched, omitted and additions separately", () => {
    const result: MatchResult = {
      expected: [
        { status: "matched", expectedGroupId: "m1-v1-n1", pitch: 60, expectedOnsetMs: 0, playedSequence: 0, playedOnsetMs: 0, onsetDeviationMs: 0 },
        { status: "omitted", expectedGroupId: "m1-v1-n2", pitch: 62, expectedOnsetMs: 500 },
      ],
      additions: [{ status: "addition", pitch: 71, playedSequence: 1, playedOnsetMs: 100 }],
    };

    const fact = buildPitchFact(result);

    expect(fact).toEqual({
      category: "pitch",
      totalExpected: 2,
      correctCount: 1,
      omittedCount: 1,
      additionCount: 1,
      errorLocations: [
        { expectedGroupId: "m1-v1-n2", pitch: 62 },
        { expectedGroupId: "unexpected", pitch: 71 },
      ],
    });
  });

  it("reports full correctness when everything matched and nothing extra was played", () => {
    const result: MatchResult = {
      expected: [{ status: "matched", expectedGroupId: "m1-v1-n1", pitch: 60, expectedOnsetMs: 0, playedSequence: 0, playedOnsetMs: 0, onsetDeviationMs: 0 }],
      additions: [],
    };

    const fact = buildPitchFact(result);

    expect(fact.correctCount).toBe(1);
    expect(fact.errorLocations).toEqual([]);
  });
});
