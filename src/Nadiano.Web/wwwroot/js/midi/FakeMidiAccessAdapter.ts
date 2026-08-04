import { MidiDiagnosticsCounters, type MidiDiagnosticsSnapshot } from "./diagnostics";
import type { MidiAccessAdapter, Unsubscribe } from "./MidiAccessAdapter";
import type { MidiAccessResult, MidiInputDeviceInfo, PlayedMidiEvent } from "./types";

export type FakeEventInput = Omit<PlayedMidiEvent, "sequence" | "receivedAtMs"> & { receivedAtMs?: number };

/**
 * Deterministic test double behind the same MidiAccessAdapter interface the
 * real Web MIDI adapter implements. Used by browser tests and manual
 * diagnostics so no test depends on real MIDI hardware
 * (docs/AGENTS.md "Testing").
 */
export class FakeMidiAccessAdapter implements MidiAccessAdapter {
  private inputs: MidiInputDeviceInfo[];
  private readonly eventListeners = new Set<(event: PlayedMidiEvent) => void>();
  private readonly deviceChangeListeners = new Set<(inputs: MidiInputDeviceInfo[]) => void>();
  private readonly diagnostics = new MidiDiagnosticsCounters();
  private nextSequence = 0;

  constructor(inputs: MidiInputDeviceInfo[] = [{ id: "fake-1", name: "Fake Test Piano", state: "connected" }]) {
    this.inputs = inputs;
  }

  isSupported(): boolean {
    return true;
  }

  async requestAccess(): Promise<MidiAccessResult> {
    return { status: "granted", inputs: this.inputs };
  }

  listInputs(): MidiInputDeviceInfo[] {
    return this.inputs;
  }

  selectInput(_deviceId: string): void {
    // No-op: every configured fake input is already "selected" and emits through emit().
  }

  onEvent(listener: (event: PlayedMidiEvent) => void): Unsubscribe {
    this.eventListeners.add(listener);
    return () => this.eventListeners.delete(listener);
  }

  onDeviceChange(listener: (inputs: MidiInputDeviceInfo[]) => void): Unsubscribe {
    this.deviceChangeListeners.add(listener);
    return () => this.deviceChangeListeners.delete(listener);
  }

  getDiagnostics(): MidiDiagnosticsSnapshot {
    return this.diagnostics.snapshot();
  }

  /** Test-only: emits events to listeners in exactly the order this is called. */
  emit(event: FakeEventInput): PlayedMidiEvent {
    const fullEvent: PlayedMidiEvent = {
      ...event,
      sequence: this.nextSequence,
      receivedAtMs: event.receivedAtMs ?? this.nextSequence,
    };
    this.nextSequence += 1;
    this.diagnostics.recordNormalized();

    for (const listener of this.eventListeners) {
      listener(fullEvent);
    }

    return fullEvent;
  }

  /** Test-only: simulates a device connecting/disconnecting. */
  setInputs(inputs: MidiInputDeviceInfo[]): void {
    this.inputs = inputs;
    for (const listener of this.deviceChangeListeners) {
      listener(inputs);
    }
  }
}
