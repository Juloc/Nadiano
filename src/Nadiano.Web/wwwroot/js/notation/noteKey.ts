/**
 * Deterministic key scheme matching the expected-event id convention in
 * docs/CONTENT_MODEL.md §7 (e.g. "m1-v1-n1"). WP-013 generates
 * expected-events.json using this same scheme in C#; this is the browser-side
 * half so a rendered note can later be looked up by an expected event's id
 * (wired up fully once WP-014/WP-016 consume real expected-events.json).
 */
export function buildNoteKey(measureNumber: number, voiceId: string, noteIndexInVoice: number): string {
  return `m${measureNumber}-v${voiceId}-n${noteIndexInVoice}`;
}
