import type { OnsetFact, TimingBand } from "./facts";
import type { MatchResult } from "./matcher";

function classifyBand(deviationMs: number, onTimeToleranceMs: number): TimingBand {
  if (Math.abs(deviationMs) <= onTimeToleranceMs) {
    return "onTime";
  }
  return deviationMs < 0 ? "early" : "late";
}

export function buildOnsetFact(result: MatchResult, onTimeToleranceMs: number): OnsetFact {
  const deviations = result.expected
    .filter((outcome) => outcome.status === "matched")
    .map((outcome) => ({
      expectedGroupId: outcome.expectedGroupId,
      pitch: outcome.pitch,
      deviationMs: outcome.onsetDeviationMs,
      band: classifyBand(outcome.onsetDeviationMs, onTimeToleranceMs),
    }));

  return { category: "onset", deviations };
}
