export type MidiEventKind = "noteOn" | "noteOff" | "controlChange";

/**
 * Normalized MIDI event shape shared across the browser adapters, the
 * scoring matcher and diagnostics (see docs/TECHNICAL_ARCHITECTURE.md §6).
 */
export interface PlayedMidiEvent {
  sequence: number;
  kind: MidiEventKind;
  receivedAtMs: number;
  deviceTimestampMs?: number;
  channel: number;
  note?: number;
  velocity?: number;
  controller?: number;
  value?: number;
}

export interface MidiInputDeviceInfo {
  id: string;
  name: string;
  manufacturer?: string;
  state: "connected" | "disconnected";
}

export type MidiAccessResult =
  | { status: "granted"; inputs: MidiInputDeviceInfo[] }
  | { status: "denied" }
  | { status: "unsupported" };
