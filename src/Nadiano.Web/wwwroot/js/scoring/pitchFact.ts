import type { PitchErrorLocation, PitchFact } from "./facts";
import type { MatchResult } from "./matcher";

export function buildPitchFact(result: MatchResult): PitchFact {
  const correctCount = result.expected.filter((o) => o.status === "matched").length;
  const omitted = result.expected.filter((o) => o.status === "omitted");

  const errorLocations: PitchErrorLocation[] = [
    ...omitted.map((o) => ({ expectedGroupId: o.expectedGroupId, pitch: o.pitch })),
    ...result.additions.map((a) => ({ expectedGroupId: "unexpected", pitch: a.pitch })),
  ];

  return {
    category: "pitch",
    totalExpected: result.expected.length,
    correctCount,
    omittedCount: omitted.length,
    additionCount: result.additions.length,
    errorLocations,
  };
}
