import type { PlayedMidiEvent } from "../midi/types";
import type { ResolvedExpectedEvent } from "./resolveExpectedEventTiming";
import type { ScoringPolicy } from "./ScoringPolicy";

export interface MatchedOutcome {
  status: "matched";
  expectedGroupId: string;
  pitch: number;
  expectedOnsetMs: number;
  playedSequence: number;
  playedOnsetMs: number;
  /** playedOnsetMs - expectedOnsetMs. Negative = early, positive = late. Raw, unclassified (see WP-015). */
  onsetDeviationMs: number;
}

export interface OmittedOutcome {
  status: "omitted";
  expectedGroupId: string;
  pitch: number;
  expectedOnsetMs: number;
}

export type ExpectedMatchOutcome = MatchedOutcome | OmittedOutcome;

export interface AdditionOutcome {
  status: "addition";
  pitch: number;
  playedSequence: number;
  playedOnsetMs: number;
}

export interface MatchResult {
  expected: ExpectedMatchOutcome[];
  additions: AdditionOutcome[];
}

type NoteOnEvent = PlayedMidiEvent & { note: number };

function isNoteOnWithPitch(event: PlayedMidiEvent): event is NoteOnEvent {
  return event.kind === "noteOn" && event.note !== undefined;
}

/**
 * Matches expected attacks against played MIDI events.
 *
 * Rules (docs/JUNIOR_IMPLEMENTATION_PLAN.md WP-014 acceptance criteria):
 * - a played note only ever matches an expected slot of the SAME pitch —
 *   timing proximity alone never bridges a pitch mismatch;
 * - each played note-on can satisfy at most one expected slot;
 * - expected slots are resolved in onset order (earliest first); each takes
 *   the nearest still-unused same-pitch played note within
 *   `policy.matchWindowMs`. This is a deterministic, documented tie-break —
 *   see matcher.test.ts for the boundary case where two expected slots
 *   compete for one played note.
 */
export function matchEvents(expected: readonly ResolvedExpectedEvent[], played: readonly PlayedMidiEvent[], policy: ScoringPolicy): MatchResult {
  const noteOnEvents = played.filter(isNoteOnWithPitch);
  const usedPlayedSequences = new Set<number>();

  const sortedExpected = [...expected].sort((a, b) => a.onsetMs - b.onsetMs || a.groupId.localeCompare(b.groupId));

  const expectedOutcomes: ExpectedMatchOutcome[] = sortedExpected.map((slot) => {
    const best = findNearestUnusedCandidate(slot, noteOnEvents, usedPlayedSequences, policy.matchWindowMs);

    if (!best) {
      return { status: "omitted", expectedGroupId: slot.groupId, pitch: slot.pitch, expectedOnsetMs: slot.onsetMs };
    }

    usedPlayedSequences.add(best.candidate.sequence);
    return {
      status: "matched",
      expectedGroupId: slot.groupId,
      pitch: slot.pitch,
      expectedOnsetMs: slot.onsetMs,
      playedSequence: best.candidate.sequence,
      playedOnsetMs: best.candidate.receivedAtMs,
      onsetDeviationMs: best.deviationMs,
    };
  });

  const additions: AdditionOutcome[] = noteOnEvents
    .filter((event) => !usedPlayedSequences.has(event.sequence))
    .map((event) => ({ status: "addition", pitch: event.note, playedSequence: event.sequence, playedOnsetMs: event.receivedAtMs }));

  return { expected: expectedOutcomes, additions };
}

function findNearestUnusedCandidate(
  slot: ResolvedExpectedEvent,
  candidates: readonly NoteOnEvent[],
  usedSequences: ReadonlySet<number>,
  matchWindowMs: number,
): { candidate: NoteOnEvent; deviationMs: number } | undefined {
  let best: { candidate: NoteOnEvent; deviationMs: number } | undefined;

  for (const candidate of candidates) {
    if (usedSequences.has(candidate.sequence) || candidate.note !== slot.pitch) {
      continue;
    }

    const deviationMs = candidate.receivedAtMs - slot.onsetMs;
    if (Math.abs(deviationMs) > matchWindowMs) {
      continue;
    }

    if (!best || Math.abs(deviationMs) < Math.abs(best.deviationMs)) {
      best = { candidate, deviationMs };
    }
  }

  return best;
}
