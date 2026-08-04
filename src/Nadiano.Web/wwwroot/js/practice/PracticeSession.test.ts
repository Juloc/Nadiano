import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { FakeMidiAccessAdapter } from "../midi/FakeMidiAccessAdapter";
import { NORMAL_MODE_POLICY } from "../scoring/ScoringPolicy";
import { PracticeSession, type PracticeSessionConfig, type PracticeSessionResult } from "./PracticeSession";
import type { ResolvedExpectedEvent } from "../scoring/resolveExpectedEventTiming";

function config(overrides: Partial<PracticeSessionConfig> = {}): PracticeSessionConfig {
  return {
    mode: "wait",
    policy: NORMAL_MODE_POLICY,
    enabledCategories: ["pitch", "onset"],
    onTimeToleranceMs: 50,
    ...overrides,
  };
}

describe("PracticeSession", () => {
  let adapter: FakeMidiAccessAdapter;
  let clockMs: number;
  let now: () => number;

  beforeEach(() => {
    vi.useFakeTimers();
    adapter = new FakeMidiAccessAdapter();
    clockMs = 0;
    now = () => clockMs;
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  describe("wait mode", () => {
    it("completes as soon as the single expected pitch is played, regardless of timing", () => {
      const expected: ResolvedExpectedEvent[] = [{ groupId: "m1-v1-n1", onsetMs: 0, durationMs: 500, pitch: 60 }];
      const session = new PracticeSession(adapter, expected, config({ mode: "wait" }), now);
      let result: PracticeSessionResult | undefined;
      session.onComplete = (r) => (result = r);

      session.start();
      clockMs = 5000; // wait mode has no deadline — arriving very "late" still completes it.
      adapter.emit({ kind: "noteOn", channel: 0, note: 60, velocity: 80 });

      expect(result?.facts.pitch).toMatchObject({ correctCount: 1, totalExpected: 1 });
    });

    it("ignores a wrong pitch and only advances once the correct one arrives", () => {
      const expected: ResolvedExpectedEvent[] = [{ groupId: "m1-v1-n1", onsetMs: 0, durationMs: 500, pitch: 60 }];
      const session = new PracticeSession(adapter, expected, config({ mode: "wait" }), now);
      let completed = false;
      session.onComplete = () => (completed = true);

      session.start();
      adapter.emit({ kind: "noteOn", channel: 0, note: 61, velocity: 80 });
      expect(completed).toBe(false);

      adapter.emit({ kind: "noteOn", channel: 0, note: 60, velocity: 80 });
      expect(completed).toBe(true);
    });

    it("requires every pitch of a chord before advancing to the next group", () => {
      const expected: ResolvedExpectedEvent[] = [
        { groupId: "m1-v1-n1", onsetMs: 0, durationMs: 500, pitch: 60 },
        { groupId: "m1-v1-n1", onsetMs: 0, durationMs: 500, pitch: 64 },
      ];
      const session = new PracticeSession(adapter, expected, config({ mode: "wait" }), now);
      let completed = false;
      session.onComplete = () => (completed = true);

      session.start();
      adapter.emit({ kind: "noteOn", channel: 0, note: 60, velocity: 80 });
      expect(completed).toBe(false);

      adapter.emit({ kind: "noteOn", channel: 0, note: 64, velocity: 80 });
      expect(completed).toBe(true);
    });

    it("accepts chord pitches in any order", () => {
      const expected: ResolvedExpectedEvent[] = [
        { groupId: "m1-v1-n1", onsetMs: 0, durationMs: 500, pitch: 60 },
        { groupId: "m1-v1-n1", onsetMs: 0, durationMs: 500, pitch: 64 },
      ];
      const session = new PracticeSession(adapter, expected, config({ mode: "wait" }), now);
      let completed = false;
      session.onComplete = () => (completed = true);

      session.start();
      adapter.emit({ kind: "noteOn", channel: 0, note: 64, velocity: 80 });
      adapter.emit({ kind: "noteOn", channel: 0, note: 60, velocity: 80 });

      expect(completed).toBe(true);
    });

    it("advances through multiple groups in order", () => {
      const expected: ResolvedExpectedEvent[] = [
        { groupId: "m1-v1-n1", onsetMs: 0, durationMs: 500, pitch: 60 },
        { groupId: "m1-v1-n2", onsetMs: 500, durationMs: 500, pitch: 62 },
      ];
      const session = new PracticeSession(adapter, expected, config({ mode: "wait" }), now);
      let result: PracticeSessionResult | undefined;
      session.onComplete = (r) => (result = r);

      session.start();
      adapter.emit({ kind: "noteOn", channel: 0, note: 60, velocity: 80 });
      adapter.emit({ kind: "noteOn", channel: 0, note: 62, velocity: 80 });

      expect(result?.facts.pitch).toMatchObject({ correctCount: 2, totalExpected: 2 });
    });
  });

  describe("performance/loop mode", () => {
    it("reports a live update on every incoming note", () => {
      const expected: ResolvedExpectedEvent[] = [{ groupId: "m1-v1-n1", onsetMs: 0, durationMs: 500, pitch: 60 }];
      const session = new PracticeSession(adapter, expected, config({ mode: "performance" }), now);
      let updateCount = 0;
      session.onLiveUpdate = () => (updateCount += 1);

      session.start();
      adapter.emit({ kind: "noteOn", channel: 0, note: 60, velocity: 80 });

      expect(updateCount).toBe(1);
    });

    it("automatically completes once the last expected event's window has elapsed", async () => {
      const expected: ResolvedExpectedEvent[] = [{ groupId: "m1-v1-n1", onsetMs: 0, durationMs: 500, pitch: 60 }];
      const session = new PracticeSession(adapter, expected, config({ mode: "performance" }), () => performance.now());
      let result: PracticeSessionResult | undefined;
      session.onComplete = (r) => (result = r);

      session.start();
      await vi.advanceTimersByTimeAsync(500 + NORMAL_MODE_POLICY.matchWindowMs + 10);

      expect(result?.facts.pitch).toMatchObject({ correctCount: 0, omittedCount: 1 });
    });

    it("finishNow() ends the attempt early and still reports a result", () => {
      const expected: ResolvedExpectedEvent[] = [{ groupId: "m1-v1-n1", onsetMs: 0, durationMs: 500, pitch: 60 }];
      const session = new PracticeSession(adapter, expected, config({ mode: "performance" }), now);
      let result: PracticeSessionResult | undefined;
      session.onComplete = (r) => (result = r);

      session.start();
      adapter.emit({ kind: "noteOn", channel: 0, note: 60, velocity: 80 });
      session.finishNow();

      expect(result).toBeDefined();
    });
  });

  describe("cleanup", () => {
    it("stop() unsubscribes so later events are not processed", () => {
      const expected: ResolvedExpectedEvent[] = [{ groupId: "m1-v1-n1", onsetMs: 0, durationMs: 500, pitch: 60 }];
      const session = new PracticeSession(adapter, expected, config({ mode: "performance" }), now);
      let updateCount = 0;
      session.onLiveUpdate = () => (updateCount += 1);

      session.start();
      adapter.emit({ kind: "noteOn", channel: 0, note: 60, velocity: 80 });
      session.stop();
      adapter.emit({ kind: "noteOn", channel: 0, note: 62, velocity: 80 });

      expect(updateCount).toBe(1);
    });

    it("calling start() again does not leave a duplicate subscription (no double updates per event)", () => {
      const expected: ResolvedExpectedEvent[] = [{ groupId: "m1-v1-n1", onsetMs: 0, durationMs: 500, pitch: 60 }];
      const session = new PracticeSession(adapter, expected, config({ mode: "performance" }), now);
      let updateCount = 0;
      session.onLiveUpdate = () => (updateCount += 1);

      session.start();
      session.start();
      adapter.emit({ kind: "noteOn", channel: 0, note: 60, velocity: 80 });

      expect(updateCount).toBe(1);
    });
  });
});
