import { describe, expect, it } from "vitest";
import { MetronomeScheduler } from "./metronomeScheduler";

describe("MetronomeScheduler", () => {
  it("schedules the first beat exactly at the start time", () => {
    const scheduler = new MetronomeScheduler({ bpm: 60, beatsPerMeasure: 4 }, 10);

    const due = scheduler.collectDueBeats(10, 0.01);

    expect(due).toEqual([{ timeSeconds: 10, beatIndexInMeasure: 0 }]);
  });

  it("spaces beats by 60/bpm seconds at 60 BPM", () => {
    const scheduler = new MetronomeScheduler({ bpm: 60, beatsPerMeasure: 4 }, 0);

    const due = scheduler.collectDueBeats(0, 3.01);

    expect(due.map((b) => b.timeSeconds)).toEqual([0, 1, 2, 3]);
  });

  it("spaces beats correctly at 120 BPM (0.5s per beat)", () => {
    const scheduler = new MetronomeScheduler({ bpm: 120, beatsPerMeasure: 4 }, 0);

    const due = scheduler.collectDueBeats(0, 1.51);

    expect(due.map((b) => b.timeSeconds)).toEqual([0, 0.5, 1, 1.5]);
  });

  it("wraps the accented beat index around the configured meter", () => {
    const scheduler = new MetronomeScheduler({ bpm: 60, beatsPerMeasure: 3 }, 0);

    const due = scheduler.collectDueBeats(0, 4.01);

    expect(due.map((b) => b.beatIndexInMeasure)).toEqual([0, 1, 2, 0, 1]);
  });

  it("returns no beats when nothing is due yet within the lookahead window", () => {
    const scheduler = new MetronomeScheduler({ bpm: 60, beatsPerMeasure: 4 }, 10);

    expect(scheduler.collectDueBeats(5, 0.1)).toEqual([]);
  });

  it("does not repeat a beat once collected, across successive polls (no accumulating drift)", () => {
    const scheduler = new MetronomeScheduler({ bpm: 120, beatsPerMeasure: 4 }, 0);

    const firstPoll = scheduler.collectDueBeats(0, 0.1);
    const secondPoll = scheduler.collectDueBeats(0.45, 0.1);
    const thirdPoll = scheduler.collectDueBeats(0.6, 0.1);

    expect(firstPoll.map((b) => b.timeSeconds)).toEqual([0]);
    expect(secondPoll.map((b) => b.timeSeconds)).toEqual([0.5]);
    expect(thirdPoll.map((b) => b.timeSeconds)).toEqual([]);
  });

  it("catches up exactly (no drift) even if a poll is delayed past several beats", () => {
    const scheduler = new MetronomeScheduler({ bpm: 120, beatsPerMeasure: 4 }, 0);

    const due = scheduler.collectDueBeats(1.6, 0.1);

    expect(due.map((b) => b.timeSeconds)).toEqual([0, 0.5, 1, 1.5]);
  });
});
