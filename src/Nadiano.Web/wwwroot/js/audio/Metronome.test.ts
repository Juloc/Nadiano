import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import type { AudioClock } from "./AudioClock";
import type { ClickSoundPlayer } from "./ClickSoundPlayer";
import { Metronome } from "./Metronome";

class FakeAudioClock implements AudioClock {
  currentTimeSeconds = 0;
  resumeCallCount = 0;

  async resume(): Promise<void> {
    this.resumeCallCount += 1;
  }
}

class RecordingSoundPlayer implements ClickSoundPlayer {
  clicks: { timeSeconds: number; accented: boolean }[] = [];

  playClickAt(timeSeconds: number, accented: boolean): void {
    this.clicks.push({ timeSeconds, accented });
  }
}

async function advance(clock: FakeAudioClock, seconds: number, steps = 4): Promise<void> {
  const stepSeconds = seconds / steps;
  for (let i = 0; i < steps; i += 1) {
    clock.currentTimeSeconds += stepSeconds;
    await vi.advanceTimersByTimeAsync(25);
  }
}

describe("Metronome", () => {
  let clock: FakeAudioClock;
  let player: RecordingSoundPlayer;
  let metronome: Metronome;

  beforeEach(() => {
    vi.useFakeTimers();
    clock = new FakeAudioClock();
    player = new RecordingSoundPlayer();
    metronome = new Metronome(clock, player);
  });

  afterEach(() => {
    metronome.dispose();
    vi.useRealTimers();
  });

  it("resumes the audio clock on start (never auto-plays without it)", async () => {
    await metronome.start({ bpm: 120, beatsPerMeasure: 4 });

    expect(clock.resumeCallCount).toBe(1);
  });

  it("returns a monotonic start time ahead of the current audio clock time", async () => {
    clock.currentTimeSeconds = 5;

    const startTime = await metronome.start({ bpm: 120, beatsPerMeasure: 4 });

    expect(startTime).toBeGreaterThan(5);
  });

  it("plays the first beat of a measure accented", async () => {
    await metronome.start({ bpm: 120, beatsPerMeasure: 4 });

    await advance(clock, 0.2);

    expect(player.clicks[0]?.accented).toBe(true);
  });

  it("stops delivering clicks after stop()", async () => {
    await metronome.start({ bpm: 120, beatsPerMeasure: 4 });
    metronome.stop();

    const clicksBeforeAdvance = player.clicks.length;
    await advance(clock, 2, 20);

    expect(player.clicks.length).toBe(clicksBeforeAdvance);
    expect(metronome.isRunning).toBe(false);
  });

  it("does not leave a duplicate interval running when start() is called again for a tempo change", async () => {
    await metronome.start({ bpm: 60, beatsPerMeasure: 4 });
    await metronome.start({ bpm: 200, beatsPerMeasure: 3 });
    metronome.stop();

    const clicksAfterStop = player.clicks.length;
    await advance(clock, 2, 40);

    expect(player.clicks.length).toBe(clicksAfterStop);
  });

  it("schedules beats at the new tempo's spacing after a tempo change, not the old one", async () => {
    await metronome.start({ bpm: 60, beatsPerMeasure: 4 });
    await advance(clock, 0.15);
    player.clicks = [];

    await metronome.start({ bpm: 240, beatsPerMeasure: 4 }); // 0.25s/beat
    await advance(clock, 0.6, 24);

    expect(player.clicks.length).toBeGreaterThanOrEqual(2);
    const gap = player.clicks[1]!.timeSeconds - player.clicks[0]!.timeSeconds;
    expect(gap).toBeCloseTo(0.25, 5);
  });
});
