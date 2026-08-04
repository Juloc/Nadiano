import type { PlayedMidiEvent } from "../midi/types";
import type { DurationFact } from "./facts";
import type { MatchResult } from "./matcher";
import type { ResolvedExpectedEvent } from "./resolveExpectedEventTiming";

function findPlayedDurationMs(played: readonly PlayedMidiEvent[], noteOnSequence: number, pitch: number): number | null {
  const noteOnIndex = played.findIndex((e) => e.sequence === noteOnSequence);
  if (noteOnIndex < 0) {
    return null;
  }

  const noteOnEvent = played[noteOnIndex]!;
  for (let i = noteOnIndex + 1; i < played.length; i += 1) {
    const candidate = played[i]!;
    if (candidate.kind === "noteOff" && candidate.note === pitch && candidate.channel === noteOnEvent.channel) {
      return candidate.receivedAtMs - noteOnEvent.receivedAtMs;
    }
  }

  return null;
}

/** Ratio is omitted (not guessed) when the note-off was never observed — e.g. session ended mid-note. */
export function buildDurationFact(
  result: MatchResult,
  resolvedExpected: readonly ResolvedExpectedEvent[],
  played: readonly PlayedMidiEvent[],
): DurationFact {
  const expectedDurationByKey = new Map(resolvedExpected.map((e) => [`${e.groupId}|${e.pitch}`, e.durationMs]));

  const ratios = result.expected
    .filter((outcome) => outcome.status === "matched")
    .flatMap((outcome) => {
      const expectedDurationMs = expectedDurationByKey.get(`${outcome.expectedGroupId}|${outcome.pitch}`);
      const playedDurationMs = findPlayedDurationMs(played, outcome.playedSequence, outcome.pitch);

      if (!expectedDurationMs || playedDurationMs === null) {
        return [];
      }

      return [{ expectedGroupId: outcome.expectedGroupId, pitch: outcome.pitch, ratio: playedDurationMs / expectedDurationMs }];
    });

  return { category: "duration", ratios };
}
