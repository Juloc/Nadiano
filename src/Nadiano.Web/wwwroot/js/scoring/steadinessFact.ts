import type { SteadinessFact } from "./facts";
import type { MatchResult } from "./matcher";

const MINIMUM_MATCHED_NOTES = 3;

/** Coefficient of variation of consecutive actually-played onset intervals. Needs at least 3 matched notes (2 intervals) to mean anything. */
export function buildSteadinessFact(result: MatchResult): SteadinessFact {
  const playedOnsets = result.expected
    .filter((outcome) => outcome.status === "matched")
    .sort((a, b) => a.expectedOnsetMs - b.expectedOnsetMs)
    .map((outcome) => outcome.playedOnsetMs);

  if (playedOnsets.length < MINIMUM_MATCHED_NOTES) {
    return { category: "steadiness", intervalVariability: null };
  }

  const intervals: number[] = [];
  for (let i = 1; i < playedOnsets.length; i += 1) {
    intervals.push(playedOnsets[i]! - playedOnsets[i - 1]!);
  }

  const mean = intervals.reduce((sum, value) => sum + value, 0) / intervals.length;
  if (mean === 0) {
    return { category: "steadiness", intervalVariability: null };
  }

  const variance = intervals.reduce((sum, value) => sum + (value - mean) ** 2, 0) / intervals.length;
  const standardDeviation = Math.sqrt(variance);

  return { category: "steadiness", intervalVariability: standardDeviation / Math.abs(mean) };
}
