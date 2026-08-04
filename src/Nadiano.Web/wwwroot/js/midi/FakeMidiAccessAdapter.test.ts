import { describe, expect, it } from "vitest";
import { FakeMidiAccessAdapter } from "./FakeMidiAccessAdapter";
import type { PlayedMidiEvent } from "./types";

describe("FakeMidiAccessAdapter", () => {
  it("reports itself as supported and grants access immediately", async () => {
    const adapter = new FakeMidiAccessAdapter();

    expect(adapter.isSupported()).toBe(true);
    const result = await adapter.requestAccess();

    expect(result.status).toBe("granted");
  });

  it("emits events to listeners in exactly the order emit() was called", () => {
    const adapter = new FakeMidiAccessAdapter();
    const received: PlayedMidiEvent[] = [];
    adapter.onEvent((event) => received.push(event));

    adapter.emit({ kind: "noteOn", channel: 0, note: 60, velocity: 80 });
    adapter.emit({ kind: "noteOn", channel: 0, note: 64, velocity: 80 });
    adapter.emit({ kind: "noteOff", channel: 0, note: 60, velocity: 0 });

    expect(received.map((event) => event.note)).toEqual([60, 64, 60]);
    expect(received.map((event) => event.sequence)).toEqual([0, 1, 2]);
  });

  it("stops delivering events after unsubscribe", () => {
    const adapter = new FakeMidiAccessAdapter();
    const received: PlayedMidiEvent[] = [];
    const unsubscribe = adapter.onEvent((event) => received.push(event));

    adapter.emit({ kind: "noteOn", channel: 0, note: 60, velocity: 80 });
    unsubscribe();
    adapter.emit({ kind: "noteOn", channel: 0, note: 62, velocity: 80 });

    expect(received).toHaveLength(1);
  });

  it("notifies device-change listeners when inputs change", () => {
    const adapter = new FakeMidiAccessAdapter([]);
    const seen: number[] = [];
    adapter.onDeviceChange((inputs) => seen.push(inputs.length));

    adapter.setInputs([{ id: "a", name: "Test Piano", state: "connected" }]);

    expect(seen).toEqual([1]);
  });
});
