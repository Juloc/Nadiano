import { describe, expect, it } from "vitest";
import type { ScoringFacts } from "./facts";
import { nextActionLocalizationKey, recommendNextAction } from "./nextAction";

describe("recommendNextAction", () => {
  it("recommends hands-separate when pitch accuracy is low", () => {
    const facts: ScoringFacts = { pitch: { category: "pitch", totalExpected: 10, correctCount: 5, omittedCount: 5, additionCount: 0, errorLocations: [] } };

    expect(recommendNextAction(facts)).toBe("hands-separate");
  });

  it("recommends repeat-slower when timing is mostly off, even with correct pitches", () => {
    const facts: ScoringFacts = {
      pitch: { category: "pitch", totalExpected: 4, correctCount: 4, omittedCount: 0, additionCount: 0, errorLocations: [] },
      onset: {
        category: "onset",
        deviations: [
          { expectedGroupId: "a", pitch: 60, deviationMs: 200, band: "late" },
          { expectedGroupId: "b", pitch: 62, deviationMs: 180, band: "late" },
          { expectedGroupId: "c", pitch: 64, deviationMs: 10, band: "onTime" },
        ],
      },
    };

    expect(recommendNextAction(facts)).toBe("repeat-slower");
  });

  it("recommends well-done when pitch is fully correct and timing is all on time", () => {
    const facts: ScoringFacts = {
      pitch: { category: "pitch", totalExpected: 4, correctCount: 4, omittedCount: 0, additionCount: 0, errorLocations: [] },
      onset: { category: "onset", deviations: [{ expectedGroupId: "a", pitch: 60, deviationMs: 5, band: "onTime" }] },
    };

    expect(recommendNextAction(facts)).toBe("well-done");
  });

  it("falls back to repeat-section for a middling result that is neither clearly bad nor clean", () => {
    const facts: ScoringFacts = {
      pitch: { category: "pitch", totalExpected: 4, correctCount: 3, omittedCount: 1, additionCount: 0, errorLocations: [{ expectedGroupId: "d", pitch: 67 }] },
    };

    expect(recommendNextAction(facts)).toBe("repeat-section");
  });

  it("never recommends well-done when there were unexpected extra notes", () => {
    const facts: ScoringFacts = {
      pitch: { category: "pitch", totalExpected: 2, correctCount: 2, omittedCount: 0, additionCount: 1, errorLocations: [{ expectedGroupId: "unexpected", pitch: 71 }] },
    };

    expect(recommendNextAction(facts)).toBe("repeat-section");
  });

  it("makes a recommendation even with no facts computed at all", () => {
    expect(recommendNextAction({})).toBe("well-done");
  });
});

describe("nextActionLocalizationKey", () => {
  it("maps every code to a distinct, well-formed resource key", () => {
    const codes = ["hands-separate", "repeat-slower", "repeat-section", "well-done"] as const;
    const keys = codes.map(nextActionLocalizationKey);

    expect(new Set(keys).size).toBe(codes.length);
    for (const key of keys) {
      expect(key).toMatch(/^Practice\.NextAction\.[A-Za-z]+$/);
    }
  });
});
