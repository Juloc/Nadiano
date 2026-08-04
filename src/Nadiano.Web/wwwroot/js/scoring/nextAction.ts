import type { ScoringFacts } from "./facts";

export type NextActionCode = "hands-separate" | "repeat-slower" | "repeat-section" | "well-done";

const PITCH_ACCURACY_THRESHOLD = 0.7;
const OFF_BAND_RATIO_THRESHOLD = 0.4;

/**
 * Rule-based, explicit and testable — no adaptive/statistical model
 * (docs/PRODUCT_CONCEPT.md §2.4, docs/JUNIOR_IMPLEMENTATION_PLAN.md WP-015
 * step 7). Every branch is traceable to a specific fact; this function
 * returns a CODE only — the practice workspace (WP-016) maps it to a
 * localized string via nextActionLocalizationKey, so the recommendation
 * text itself is never hardcoded here.
 */
export function recommendNextAction(facts: ScoringFacts): NextActionCode {
  if (facts.pitch && facts.pitch.totalExpected > 0) {
    const accuracy = facts.pitch.correctCount / facts.pitch.totalExpected;
    if (accuracy < PITCH_ACCURACY_THRESHOLD) {
      return "hands-separate";
    }
  }

  if (facts.onset && facts.onset.deviations.length > 0) {
    const offBandCount = facts.onset.deviations.filter((d) => d.band !== "onTime").length;
    if (offBandCount / facts.onset.deviations.length > OFF_BAND_RATIO_THRESHOLD) {
      return "repeat-slower";
    }
  }

  const pitchFullyCorrect = !facts.pitch || (facts.pitch.totalExpected > 0 && facts.pitch.correctCount === facts.pitch.totalExpected && facts.pitch.additionCount === 0);
  const onsetAllOnTime = !facts.onset || facts.onset.deviations.every((d) => d.band === "onTime");

  if (pitchFullyCorrect && onsetAllOnTime) {
    return "well-done";
  }

  return "repeat-section";
}

const LOCALIZATION_KEYS: Record<NextActionCode, string> = {
  "hands-separate": "Practice.NextAction.HandsSeparate",
  "repeat-slower": "Practice.NextAction.RepeatSlower",
  "repeat-section": "Practice.NextAction.RepeatSection",
  "well-done": "Practice.NextAction.WellDone",
};

/** Maps a code to a resource key — never to display text (docs/JUNIOR_IMPLEMENTATION_PLAN.md WP-015 step 8: "Localize the display separately"). */
export function nextActionLocalizationKey(code: NextActionCode): string {
  return LOCALIZATION_KEYS[code];
}
