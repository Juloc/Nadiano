import { describe, expect, it } from "vitest";
import type { PlayedMidiEvent } from "../midi/types";
import { RecentEventBuffer } from "./RecentEventBuffer";

function fakeEvent(sequence: number): PlayedMidiEvent {
  return { sequence, kind: "noteOn", receivedAtMs: sequence, channel: 0, note: 60, velocity: 80 };
}

describe("RecentEventBuffer", () => {
  it("lists pushed events in order", () => {
    const buffer = new RecentEventBuffer(5);

    buffer.push(fakeEvent(0));
    buffer.push(fakeEvent(1));

    expect(buffer.list().map((e) => e.sequence)).toEqual([0, 1]);
  });

  it("drops the oldest event once capacity is exceeded", () => {
    const buffer = new RecentEventBuffer(3);

    buffer.push(fakeEvent(0));
    buffer.push(fakeEvent(1));
    buffer.push(fakeEvent(2));
    buffer.push(fakeEvent(3));

    expect(buffer.list().map((e) => e.sequence)).toEqual([1, 2, 3]);
  });

  it("clears all events", () => {
    const buffer = new RecentEventBuffer(3);
    buffer.push(fakeEvent(0));

    buffer.clear();

    expect(buffer.list()).toEqual([]);
  });
});
