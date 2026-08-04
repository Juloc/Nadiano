import { describe, expect, it } from "vitest";
import type { MatchResult } from "./matcher";
import { computeScoringFacts } from "./computeScoringFacts";

const matchResult: MatchResult = {
  expected: [{ status: "matched", expectedGroupId: "m1-v1-n1", pitch: 60, expectedOnsetMs: 0, playedSequence: 0, playedOnsetMs: 0, onsetDeviationMs: 0 }],
  additions: [],
};

describe("computeScoringFacts", () => {
  it("only includes facts for categories the lesson enabled", () => {
    const facts = computeScoringFacts(["pitch"], matchResult, [], [], 50);

    expect(facts.pitch).toBeDefined();
    expect(facts.onset).toBeUndefined();
    expect(facts.duration).toBeUndefined();
    expect(facts.steadiness).toBeUndefined();
    expect(facts.dynamics).toBeUndefined();
    expect(facts.pedal).toBeUndefined();
  });

  it("includes multiple facts when multiple categories are enabled", () => {
    const facts = computeScoringFacts(["pitch", "onset"], matchResult, [], [], 50);

    expect(facts.pitch).toBeDefined();
    expect(facts.onset).toBeDefined();
    expect(facts.duration).toBeUndefined();
  });

  it("includes nothing when no categories are enabled", () => {
    const facts = computeScoringFacts([], matchResult, [], [], 50);

    expect(facts).toEqual({});
  });

  it("does not throw for a category with no fact builder yet (articulation)", () => {
    expect(() => computeScoringFacts(["articulation"], matchResult, [], [], 50)).not.toThrow();
    expect(computeScoringFacts(["articulation"], matchResult, [], [], 50)).toEqual({});
  });
});
