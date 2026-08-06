import type { ExpectedEventDocument } from "./types";

export interface ResolvedExpectedEvent {
  groupId: string;
  onsetMs: number;
  durationMs: number;
  pitch: number;
}

export function resolveExpectedEventTiming(
  document: ExpectedEventDocument,
  beatsPerMeasure: number,
  sessionStartAtMs: number,
  tempoOverrideBpm?: number,
): ResolvedExpectedEvent[] {
  const bpm = tempoOverrideBpm ?? document.tempoMap[0]?.bpm ?? 120;
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
