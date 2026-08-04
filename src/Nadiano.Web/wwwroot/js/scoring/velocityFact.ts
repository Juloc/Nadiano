import type { PlayedMidiEvent } from "../midi/types";
import type { VelocityFact } from "./facts";
import type { MatchResult } from "./matcher";

/**
 * Raw MIDI velocity range of matched notes only — a dynamics PROXY, not a
 * claim about acoustic tone quality (docs/RESEARCH_BASIS.md §5: velocity is
 * "a dynamic proxy, not acoustic tone quality").
 */
export function buildVelocityFact(result: MatchResult, played: readonly PlayedMidiEvent[]): VelocityFact {
  const playedBySequence = new Map(played.map((event) => [event.sequence, event]));

  const velocities = result.expected
    .filter((outcome) => outcome.status === "matched")
    .map((outcome) => playedBySequence.get(outcome.playedSequence)?.velocity)
    .filter((velocity): velocity is number => velocity !== undefined);

  if (velocities.length === 0) {
    return { category: "dynamics", minVelocity: null, maxVelocity: null, averageVelocity: null };
  }

  return {
    category: "dynamics",
    minVelocity: Math.min(...velocities),
    maxVelocity: Math.max(...velocities),
    averageVelocity: velocities.reduce((sum, v) => sum + v, 0) / velocities.length,
  };
}
