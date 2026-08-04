export interface MetronomeConfig {
  bpm: number;
  beatsPerMeasure: number;
}

export interface ScheduledBeat {
  timeSeconds: number;
  beatIndexInMeasure: number;
}

/**
 * Pure look-ahead beat calculator (Chris Wilson's "A Tale of Two Clocks"
 * pattern). Deciding *which* beats are due is separated from actually
 * playing/animating them, so the timing math is unit-testable without any
 * Web Audio API.
 */
export class MetronomeScheduler {
  private nextBeatTimeSeconds: number;
  private beatIndexInMeasure = 0;

  constructor(
    private readonly config: MetronomeConfig,
    startTimeSeconds: number,
  ) {
    this.nextBeatTimeSeconds = startTimeSeconds;
  }

  private get secondsPerBeat(): number {
    return 60 / this.config.bpm;
  }

  /** Returns every beat due before `currentTimeSeconds + lookaheadSeconds`, advancing internal state. */
  collectDueBeats(currentTimeSeconds: number, lookaheadSeconds: number): ScheduledBeat[] {
    const due: ScheduledBeat[] = [];

    while (this.nextBeatTimeSeconds < currentTimeSeconds + lookaheadSeconds) {
      due.push({ timeSeconds: this.nextBeatTimeSeconds, beatIndexInMeasure: this.beatIndexInMeasure });
      this.beatIndexInMeasure = (this.beatIndexInMeasure + 1) % this.config.beatsPerMeasure;
      this.nextBeatTimeSeconds += this.secondsPerBeat;
    }

    return due;
  }
}
