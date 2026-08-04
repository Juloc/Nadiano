import type { AudioClock } from "./AudioClock";
import type { ClickSoundPlayer } from "./ClickSoundPlayer";
import { MetronomeScheduler, type MetronomeConfig, type ScheduledBeat } from "./metronomeScheduler";

const LOOKAHEAD_SECONDS = 0.1;
const SCHEDULER_INTERVAL_MS = 25;
const START_DELAY_SECONDS = 0.1;

export type BeatListener = (beat: ScheduledBeat, delaySeconds: number) => void;

/**
 * Owns the look-ahead scheduling loop: a JS timer wakes up roughly every
 * 25ms just to ask "what's due in the next 100ms?", but every actual click
 * is scheduled against precise AudioContext time, not the JS timer itself
 * (docs/TECHNICAL_ARCHITECTURE.md §7).
 */
export class Metronome {
  private intervalId: ReturnType<typeof setInterval> | undefined;
  private scheduler: MetronomeScheduler | undefined;

  constructor(
    private readonly audioClock: AudioClock,
    private readonly soundPlayer: ClickSoundPlayer,
  ) {
    if (typeof document !== "undefined") {
      document.addEventListener("visibilitychange", this.handleVisibilityChange);
    }
  }

  /** Starts (or restarts) the metronome and returns the monotonic AudioContext start time. */
  async start(config: MetronomeConfig, onBeat?: BeatListener): Promise<number> {
    this.stop();
    await this.audioClock.resume();

    const startTimeSeconds = this.audioClock.currentTimeSeconds + START_DELAY_SECONDS;
    this.scheduler = new MetronomeScheduler(config, startTimeSeconds);

    this.intervalId = setInterval(() => {
      this.pollDueBeats(onBeat);
    }, SCHEDULER_INTERVAL_MS);

    return startTimeSeconds;
  }

  stop(): void {
    if (this.intervalId !== undefined) {
      clearInterval(this.intervalId);
      this.intervalId = undefined;
    }
    this.scheduler = undefined;
  }

  dispose(): void {
    this.stop();
    if (typeof document !== "undefined") {
      document.removeEventListener("visibilitychange", this.handleVisibilityChange);
    }
  }

  get isRunning(): boolean {
    return this.intervalId !== undefined;
  }

  private pollDueBeats(onBeat: BeatListener | undefined): void {
    if (!this.scheduler) {
      return;
    }

    const dueBeats = this.scheduler.collectDueBeats(this.audioClock.currentTimeSeconds, LOOKAHEAD_SECONDS);
    for (const beat of dueBeats) {
      this.soundPlayer.playClickAt(beat.timeSeconds, beat.beatIndexInMeasure === 0);

      if (onBeat) {
        const delaySeconds = Math.max(0, beat.timeSeconds - this.audioClock.currentTimeSeconds);
        setTimeout(() => onBeat(beat, delaySeconds), delaySeconds * 1000);
      }
    }
  }

  private readonly handleVisibilityChange = (): void => {
    if (typeof document !== "undefined" && document.visibilityState === "visible") {
      void this.audioClock.resume();
    }
  };
}
