import { describe, expect, it } from "vitest";
import { MidiSequenceCounter, normalizeMidiMessage } from "./normalize";

describe("normalizeMidiMessage", () => {
  it("parses a note-on message", () => {
    const sequence = new MidiSequenceCounter();
    const event = normalizeMidiMessage(new Uint8Array([0x90, 60, 80]), 1000, sequence);

    expect(event).toEqual({
      sequence: 0,
      kind: "noteOn",
      receivedAtMs: 1000,
      deviceTimestampMs: undefined,
      channel: 0,
      note: 60,
      velocity: 80,
    });
  });

  it("parses an explicit note-off message", () => {
    const sequence = new MidiSequenceCounter();
    const event = normalizeMidiMessage(new Uint8Array([0x80, 60, 64]), 1000, sequence);

    expect(event).toMatchObject({ kind: "noteOff", note: 60, velocity: 64 });
  });

  it("treats note-on with velocity zero as note-off", () => {
    const sequence = new MidiSequenceCounter();
    const event = normalizeMidiMessage(new Uint8Array([0x90, 60, 0]), 1000, sequence);

    expect(event?.kind).toBe("noteOff");
  });

  it("parses a control-change message", () => {
    const sequence = new MidiSequenceCounter();
    const event = normalizeMidiMessage(new Uint8Array([0xb0, 64, 127]), 1000, sequence);

    expect(event).toMatchObject({ kind: "controlChange", controller: 64, value: 127 });
  });

  it.each([0, 15])("extracts channel %i from the low nibble of the status byte", (channel) => {
    const sequence = new MidiSequenceCounter();
    const event = normalizeMidiMessage(new Uint8Array([0x90 | channel, 60, 80]), 1000, sequence);

    expect(event?.channel).toBe(channel);
  });

  it.each([0, 127])("preserves note %i at the boundary of the MIDI note range", (note) => {
    const sequence = new MidiSequenceCounter();
    const event = normalizeMidiMessage(new Uint8Array([0x90, note, 80]), 1000, sequence);

    expect(event?.note).toBe(note);
  });

  it("returns undefined for an empty message instead of throwing", () => {
    const sequence = new MidiSequenceCounter();

    expect(normalizeMidiMessage(new Uint8Array([]), 1000, sequence)).toBeUndefined();
  });

  it("returns undefined for a truncated message instead of throwing", () => {
    const sequence = new MidiSequenceCounter();

    expect(normalizeMidiMessage(new Uint8Array([0x90]), 1000, sequence)).toBeUndefined();
    expect(normalizeMidiMessage(new Uint8Array([0x90, 60]), 1000, sequence)).toBeUndefined();
  });

  it("returns undefined for system/sysex messages instead of guessing", () => {
    const sequence = new MidiSequenceCounter();

    expect(normalizeMidiMessage(new Uint8Array([0xf0, 0x7e, 0x00]), 1000, sequence)).toBeUndefined();
  });

  it("returns undefined for 2-byte channel messages such as program change", () => {
    const sequence = new MidiSequenceCounter();

    expect(normalizeMidiMessage(new Uint8Array([0xc0, 5]), 1000, sequence)).toBeUndefined();
  });

  it("does not consume a sequence number for an ignored message", () => {
    const sequence = new MidiSequenceCounter();

    normalizeMidiMessage(new Uint8Array([0xf0, 0x7e, 0x00]), 1000, sequence);
    const event = normalizeMidiMessage(new Uint8Array([0x90, 60, 80]), 1000, sequence);

    expect(event?.sequence).toBe(0);
  });

  it("assigns strictly increasing sequence numbers across calls sharing a counter", () => {
    const sequence = new MidiSequenceCounter();
    const first = normalizeMidiMessage(new Uint8Array([0x90, 60, 80]), 1000, sequence);
    const second = normalizeMidiMessage(new Uint8Array([0x80, 60, 0]), 1010, sequence);

    expect(first?.sequence).toBe(0);
    expect(second?.sequence).toBe(1);
  });

  it("does not round or otherwise alter the received timestamp", () => {
    const sequence = new MidiSequenceCounter();
    const event = normalizeMidiMessage(new Uint8Array([0x90, 60, 80]), 1234.5678, sequence);

    expect(event?.receivedAtMs).toBe(1234.5678);
  });

  it("preserves duplicate identical messages as two distinct events rather than deduplicating", () => {
    const sequence = new MidiSequenceCounter();
    const first = normalizeMidiMessage(new Uint8Array([0x90, 60, 80]), 1000, sequence);
    const second = normalizeMidiMessage(new Uint8Array([0x90, 60, 80]), 1000, sequence);

    expect(first).not.toBe(second);
    expect(first?.sequence).toBe(0);
    expect(second?.sequence).toBe(1);
  });

  it("parses a clean ascending scale as independent note-on/note-off pairs", () => {
    const sequence = new MidiSequenceCounter();
    const scale = [60, 62, 64, 65, 67, 69, 71, 72];
    const events = scale.flatMap((note, index) => [
      normalizeMidiMessage(new Uint8Array([0x90, note, 90]), index * 200, sequence),
      normalizeMidiMessage(new Uint8Array([0x80, note, 0]), index * 200 + 150, sequence),
    ]);

    expect(events.every((event) => event !== undefined)).toBe(true);
    expect(events.map((event) => event!.note)).toEqual(scale.flatMap((note) => [note, note]));
    expect(events.map((event) => event!.kind)).toEqual(scale.flatMap(() => ["noteOn", "noteOff"]));
  });

  it("parses a simultaneous chord as one event per note, preserving arrival order", () => {
    const sequence = new MidiSequenceCounter();
    const chordNotes = [60, 64, 67];
    const events = chordNotes.map((note) => normalizeMidiMessage(new Uint8Array([0x90, note, 90]), 500, sequence));

    expect(events.map((event) => event?.note)).toEqual(chordNotes);
    expect(events.map((event) => event?.sequence)).toEqual([0, 1, 2]);
  });
});
