import type { PlayedMidiEvent } from "../midi/types";
import type { PedalFact } from "./facts";

const SUSTAIN_CONTROLLER = 64;
const SUSTAIN_ON_THRESHOLD = 64;

/**
 * Raw sustain pedal on/off observations, no correctness judgment. Real pedal
 * scoring is out of alpha scope (docs/ROADMAP.md Alpha exclusions:
 * "advanced pedal evaluation") — callers only build this fact when the
 * lesson explicitly declares "pedal" in its assessment categories.
 */
export function buildPedalFact(played: readonly PlayedMidiEvent[]): PedalFact {
  const observations = played
    .filter((event) => event.kind === "controlChange" && event.controller === SUSTAIN_CONTROLLER && event.value !== undefined)
    .map((event) => ({
      atMs: event.receivedAtMs,
      state: (event.value! >= SUSTAIN_ON_THRESHOLD ? "on" : "off") as "on" | "off",
    }));

  return { category: "pedal", observations };
}
