import type { MidiDiagnosticsSnapshot } from "./diagnostics";
import type { MidiAccessResult, MidiInputDeviceInfo, PlayedMidiEvent } from "./types";

export type Unsubscribe = () => void;

/**
 * Consumer-facing contract for MIDI input. Browser modules must depend on
 * this interface only, never on `navigator.requestMIDIAccess` directly, so
 * that a FakeMidiAccessAdapter can stand in for browser tests (see
 * docs/JUNIOR_IMPLEMENTATION_PLAN.md WP-006).
 */
export interface MidiAccessAdapter {
  isSupported(): boolean;
  requestAccess(): Promise<MidiAccessResult>;
  listInputs(): MidiInputDeviceInfo[];
  selectInput(deviceId: string): void;
  onEvent(listener: (event: PlayedMidiEvent) => void): Unsubscribe;
  onDeviceChange(listener: (inputs: MidiInputDeviceInfo[]) => void): Unsubscribe;
  getDiagnostics(): MidiDiagnosticsSnapshot;
}
