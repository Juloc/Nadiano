import { MidiDiagnosticsCounters, type MidiDiagnosticsSnapshot } from "./diagnostics";
import type { MidiAccessAdapter, Unsubscribe } from "./MidiAccessAdapter";
import { MidiSequenceCounter, normalizeMidiMessage } from "./normalize";
import type { MidiAccessResult, MidiInputDeviceInfo, PlayedMidiEvent } from "./types";

/** Production adapter backed by the real Web MIDI API. */
export class WebMidiAccessAdapter implements MidiAccessAdapter {
  private midiAccess: MIDIAccess | undefined;
  private readonly sequence = new MidiSequenceCounter();
  private readonly diagnostics = new MidiDiagnosticsCounters();
  private readonly eventListeners = new Set<(event: PlayedMidiEvent) => void>();
  private readonly deviceChangeListeners = new Set<(inputs: MidiInputDeviceInfo[]) => void>();

  isSupported(): boolean {
    return typeof navigator !== "undefined" && "requestMIDIAccess" in navigator;
  }

  async requestAccess(): Promise<MidiAccessResult> {
    if (!this.isSupported()) {
      return { status: "unsupported" };
    }

    try {
      this.midiAccess = await navigator.requestMIDIAccess({ sysex: false });
    } catch {
      return { status: "denied" };
    }

    this.midiAccess.onstatechange = () => this.notifyDeviceChange();

    return { status: "granted", inputs: this.listInputs() };
  }

  listInputs(): MidiInputDeviceInfo[] {
    if (!this.midiAccess) {
      return [];
    }

    return Array.from(this.midiAccess.inputs.values()).map((input) => ({
      id: input.id,
      name: input.name ?? "Unknown device",
      manufacturer: input.manufacturer ?? undefined,
      state: input.state,
    }));
  }

  selectInput(deviceId: string): void {
    if (!this.midiAccess) {
      return;
    }

    for (const input of this.midiAccess.inputs.values()) {
      input.onmidimessage = null;
    }

    const selected = this.midiAccess.inputs.get(deviceId);
    if (!selected) {
      return;
    }

    selected.onmidimessage = (message) => this.handleMessage(message);
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

  private handleMessage(message: MIDIMessageEvent): void {
    if (!message.data) {
      this.diagnostics.recordIgnored();
      return;
    }

    const normalized = normalizeMidiMessage(message.data, performance.now(), this.sequence, message.timeStamp);
    if (!normalized) {
      this.diagnostics.recordIgnored();
      return;
    }

    this.diagnostics.recordNormalized();
    for (const listener of this.eventListeners) {
      listener(normalized);
    }
  }

  private notifyDeviceChange(): void {
    const inputs = this.listInputs();
    for (const listener of this.deviceChangeListeners) {
      listener(inputs);
    }
  }
}
