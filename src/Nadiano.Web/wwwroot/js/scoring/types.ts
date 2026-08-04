/** Mirrors the C# ExpectedEventDocument JSON shape (docs/CONTENT_MODEL.md §7). */
export interface ExpectedEvent {
  id: string;
  measure: number;
  beat: number;
  durationBeats: number;
  pitches: number[];
  hand?: "left" | "right" | "both";
  voice?: string;
  fingering?: number[];
  articulation?: "legato" | "detached" | "staccato";
  velocityTarget?: { minimum: number; maximum: number };
}

export interface TempoMapEntry {
  beat: number;
  bpm: number;
}

export interface ExpectedEventDocument {
  schemaVersion: number;
  timeBase: string;
  tempoMap: TempoMapEntry[];
  events: ExpectedEvent[];
}
