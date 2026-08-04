import { describe, expect, it } from "vitest";
import { SustainState } from "./sustainState";
import type { PlayedMidiEvent } from "./types";

function controlChange(channel: number, value: number, sequence = 0): PlayedMidiEvent {
  return { sequence, kind: "controlChange", receivedAtMs: sequence, channel, controller: 64, value };
}

describe("SustainState", () => {
  it("is not sustained before any pedal event", () => {
    const state = new SustainState();

    expect(state.isSustained(0)).toBe(false);
  });

  it("becomes sustained when CC64 reaches the on threshold", () => {
    const state = new SustainState();

    state.update(controlChange(0, 127));

    expect(state.isSustained(0)).toBe(true);
  });

  it("releases when CC64 drops below the on threshold", () => {
    const state = new SustainState();

    state.update(controlChange(0, 127));
    state.update(controlChange(0, 0));

    expect(state.isSustained(0)).toBe(false);
  });

  it("tracks sustain independently per channel", () => {
    const state = new SustainState();

    state.update(controlChange(0, 127));

    expect(state.isSustained(0)).toBe(true);
    expect(state.isSustained(1)).toBe(false);
  });

  it("keeps sustain active while notes change underneath it (pedal overlap)", () => {
    const state = new SustainState();

    state.update(controlChange(0, 127));
    state.update({ sequence: 1, kind: "noteOn", receivedAtMs: 1, channel: 0, note: 60, velocity: 90 });
    state.update({ sequence: 2, kind: "noteOff", receivedAtMs: 2, channel: 0, note: 60, velocity: 0 });

    expect(state.isSustained(0)).toBe(true);
  });

  it("ignores non-sustain control changes", () => {
    const state = new SustainState();

    state.update({ sequence: 0, kind: "controlChange", receivedAtMs: 0, channel: 0, controller: 7, value: 100 });

    expect(state.isSustained(0)).toBe(false);
  });

  it("reports isAnySustained across channels", () => {
    const state = new SustainState();

    expect(state.isAnySustained()).toBe(false);

    state.update(controlChange(3, 127));

    expect(state.isAnySustained()).toBe(true);
  });

  it("clears all sustain state on reset (e.g. device disconnect)", () => {
    const state = new SustainState();

    state.update(controlChange(0, 127));
    state.reset();

    expect(state.isSustained(0)).toBe(false);
    expect(state.isAnySustained()).toBe(false);
  });
});
