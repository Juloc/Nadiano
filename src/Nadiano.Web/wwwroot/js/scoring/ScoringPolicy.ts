/**
 * Timing windows driving the matcher. onTimeToleranceMs classifies a match
 * as "on time" vs "early"/"late" (used by feedback formatting, WP-015);
 * matchWindowMs is the outer bound beyond which a played note can no
 * longer satisfy an expected attack at all (docs/JUNIOR_IMPLEMENTATION_PLAN.md WP-014).
 */
export interface ScoringPolicy {
  readonly onTimeToleranceMs: number;
  readonly matchWindowMs: number;
  readonly chordRollToleranceMs: number;
}

export const WAIT_MODE_POLICY: ScoringPolicy = {
  onTimeToleranceMs: 150,
  matchWindowMs: 2000,
  chordRollToleranceMs: 80,
};

export const RHYTHM_MODE_POLICY: ScoringPolicy = {
  onTimeToleranceMs: 100,
  matchWindowMs: 300,
  chordRollToleranceMs: 80,
};

export const NORMAL_MODE_POLICY: ScoringPolicy = {
  onTimeToleranceMs: 80,
  matchWindowMs: 250,
  chordRollToleranceMs: 50,
};

export const PERFORMANCE_MODE_POLICY: ScoringPolicy = {
  onTimeToleranceMs: 60,
  matchWindowMs: 180,
  chordRollToleranceMs: 40,
};
