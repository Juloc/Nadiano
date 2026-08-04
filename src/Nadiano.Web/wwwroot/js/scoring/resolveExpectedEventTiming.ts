import type { ExpectedEventDocument } from "./types";

export interface ResolvedExpectedEvent {
  /** Id of the parent expected event (chord notes share one id — see docs/CONTENT_MODEL.md §7). */
  groupId: string;
  onsetMs: number;
  durationMs: number;
  pitch: number;
}

/**
 * Converts measure-relative beats into absolute milliseconds from the
 * practice session start, expanding chords into one entry per pitch. Kept
 * separate from the matcher itself (docs/JUNIOR_IMPLEMENTATION_PLAN.md
 * WP-014 step 7: "keep matcher independent from ... UI") so timing
 * resolution can change (e.g. tempo changes mid-piece) without touching
 * matching logic.
 */
export function resolveExpectedEventTiming(
  document: ExpectedEventDocument,
  beatsPerMeasure: number,
  sessionStartAtMs: number,
): ResolvedExpectedEvent[] {
  const bpm = document.tempoMap[0]?.bpm ?? 120;
  const msPerBeat = 60000 / bpm;

  const resolved: ResolvedExpectedEvent[] = [];

  for (const event of document.events) {
    const absoluteBeat = (event.measure - 1) * beatsPerMeasure + event.beat;
    const onsetMs = sessionStartAtMs + absoluteBeat * msPerBeat;
    const durationMs = event.durationBeats * msPerBeat;

    for (const pitch of event.pitches) {
      resolved.push({ groupId: event.id, onsetMs, durationMs, pitch });
    }
  }

  return resolved;
}
