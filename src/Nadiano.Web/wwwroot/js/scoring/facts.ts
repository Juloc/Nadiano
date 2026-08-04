/**
 * Structured, traceable scoring facts (docs/JUNIOR_IMPLEMENTATION_PLAN.md
 * WP-015). Every displayed claim in the practice workspace (WP-016) must
 * come from one of these fields — never freeform text generated ad hoc.
 * None of these types can represent a physical-technique judgment: that is
 * intentional, not an omission (docs/AGENTS.md "Never claim MIDI can detect
 * posture, tension or the actual finger used").
 */

export type TimingBand = "onTime" | "early" | "late";

export interface PitchErrorLocation {
  expectedGroupId: string;
  pitch: number;
}

export interface PitchFact {
  category: "pitch";
  totalExpected: number;
  correctCount: number;
  omittedCount: number;
  additionCount: number;
  errorLocations: PitchErrorLocation[];
}

export interface OnsetDeviation {
  expectedGroupId: string;
  pitch: number;
  deviationMs: number;
  band: TimingBand;
}

export interface OnsetFact {
  category: "onset";
  deviations: OnsetDeviation[];
}

export interface DurationRatio {
  expectedGroupId: string;
  pitch: number;
  /** playedDurationMs / expectedDurationMs. Below 1 = held shorter than written; above 1 = held longer. */
  ratio: number;
}

export interface DurationFact {
  category: "duration";
  ratios: DurationRatio[];
}

export interface SteadinessFact {
  category: "steadiness";
  /** Coefficient of variation of consecutive matched-onset intervals. Lower = steadier. Null when too few notes to judge. */
  intervalVariability: number | null;
}

/** Lesson-facing category is "dynamics" (docs/CONTENT_MODEL.md AssessmentCategory); the underlying MIDI measurement is velocity. */
export interface VelocityFact {
  category: "dynamics";
  minVelocity: number | null;
  maxVelocity: number | null;
  averageVelocity: number | null;
}

export interface PedalObservation {
  atMs: number;
  state: "on" | "off";
}

export interface PedalFact {
  category: "pedal";
  observations: PedalObservation[];
}

export interface ScoringFacts {
  pitch?: PitchFact;
  onset?: OnsetFact;
  duration?: DurationFact;
  steadiness?: SteadinessFact;
  dynamics?: VelocityFact;
  pedal?: PedalFact;
}
