import { describe, expect, it } from "vitest";
import { ActiveNoteTracker } from "./ActiveNoteTracker";

describe("ActiveNoteTracker", () => {
  it("lists a note after note-on", () => {
    const tracker = new ActiveNoteTracker();

    tracker.noteOn(60, 80, 0);

    expect(tracker.list()).toEqual([{ note: 60, velocity: 80, channel: 0 }]);
  });

  it("removes a note after note-off", () => {
    const tracker = new ActiveNoteTracker();

    tracker.noteOn(60, 80, 0);
    tracker.noteOff(60);

    expect(tracker.list()).toEqual([]);
  });

  it("clears every note on disconnect", () => {
    const tracker = new ActiveNoteTracker();

    tracker.noteOn(60, 80, 0);
    tracker.noteOn(64, 80, 0);
    tracker.clear();

    expect(tracker.list()).toEqual([]);
  });

  it("lists held notes in ascending pitch order regardless of press order", () => {
    const tracker = new ActiveNoteTracker();

    tracker.noteOn(67, 80, 0);
    tracker.noteOn(60, 80, 0);
    tracker.noteOn(64, 80, 0);

    expect(tracker.list().map((n) => n.note)).toEqual([60, 64, 67]);
  });
});
