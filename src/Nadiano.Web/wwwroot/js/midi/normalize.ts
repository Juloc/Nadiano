import type { MidiEventKind, PlayedMidiEvent } from "./types";

/** Monotonic per-session sequence numbers so events can be ordered even if timestamps collide. */
export class MidiSequenceCounter {
  private next = 0;

  nextValue(): number {
    const value = this.next;
    this.next += 1;
    return value;
  }
}

const STATUS_NOTE_OFF = 0x8;
const STATUS_NOTE_ON = 0x9;
const STATUS_CONTROL_CHANGE = 0xb;

/**
 * Parses one raw MIDI message into a normalized event. Returns undefined for
 * messages this layer does not score against (system messages, malformed
 * data) rather than throwing, so a single bad message cannot crash a session.
 *
 * Note-on with velocity zero is treated as note-off, per the MIDI spec and
 * docs/TECHNICAL_ARCHITECTURE.md §6.
 */
export function normalizeMidiMessage(
  data: Uint8Array,
  receivedAtMs: number,
  sequence: MidiSequenceCounter,
  deviceTimestampMs?: number,
): PlayedMidiEvent | undefined {
  if (data.length < 3) {
    return undefined;
  }

  const statusByte = data[0]!;
  const messageType = (statusByte >> 4) & 0x0f;
  const channel = statusByte & 0x0f;
  const data1 = data[1]!;
  const data2 = data[2]!;

  let kind: MidiEventKind;
  if (messageType === STATUS_NOTE_ON) {
    kind = data2 === 0 ? "noteOff" : "noteOn";
  } else if (messageType === STATUS_NOTE_OFF) {
    kind = "noteOff";
  } else if (messageType === STATUS_CONTROL_CHANGE) {
    kind = "controlChange";
  } else {
    return undefined;
  }

  const base = {
    sequence: sequence.nextValue(),
    kind,
    receivedAtMs,
    deviceTimestampMs,
    channel,
  };

  return kind === "controlChange"
    ? { ...base, controller: data1, value: data2 }
    : { ...base, note: data1, velocity: data2 };
}
