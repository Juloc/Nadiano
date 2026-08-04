import type { PlayedMidiEvent } from "../midi/types";
import { buildDurationFact } from "./durationFact";
import type { ScoringFacts } from "./facts";
import type { MatchResult } from "./matcher";
import { buildOnsetFact } from "./onsetFact";
import { buildPedalFact } from "./pedalFact";
import { buildPitchFact } from "./pitchFact";
import type { ResolvedExpectedEvent } from "./resolveExpectedEventTiming";
import { buildSteadinessFact } from "./steadinessFact";
import { buildVelocityFact } from "./velocityFact";

/** Matches docs/CONTENT_MODEL.md AssessmentCategory. "articulation" has no fact builder yet (out of alpha scope, see docs/JUNIOR_IMPLEMENTATION_PLAN.md WP-038) and is silently a no-op if declared. */
export type AssessmentCategory = "pitch" | "onset" | "duration" | "steadiness" | "articulation" | "dynamics" | "pedal";

/**
 * Builds only the facts the lesson actually declares
 * (docs/JUNIOR_IMPLEMENTATION_PLAN.md WP-015 acceptance: "categories can be
 * disabled by lesson"). A category absent from `enabledCategories` never
 * appears in the result, even if the underlying data exists.
 */
export function computeScoringFacts(
  enabledCategories: readonly AssessmentCategory[],
  matchResult: MatchResult,
  resolvedExpected: readonly ResolvedExpectedEvent[],
  played: readonly PlayedMidiEvent[],
  onTimeToleranceMs: number,
): ScoringFacts {
  const enabled = new Set(enabledCategories);
  const facts: ScoringFacts = {};

  if (enabled.has("pitch")) {
    facts.pitch = buildPitchFact(matchResult);
  }

  if (enabled.has("onset")) {
    facts.onset = buildOnsetFact(matchResult, onTimeToleranceMs);
  }

  if (enabled.has("duration")) {
    facts.duration = buildDurationFact(matchResult, resolvedExpected, played);
  }

  if (enabled.has("steadiness")) {
    facts.steadiness = buildSteadinessFact(matchResult);
  }

  if (enabled.has("dynamics")) {
    facts.dynamics = buildVelocityFact(matchResult, played);
  }

  if (enabled.has("pedal")) {
    facts.pedal = buildPedalFact(played);
  }

  return facts;
}
