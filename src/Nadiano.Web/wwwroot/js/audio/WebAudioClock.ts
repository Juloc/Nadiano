import type { AudioClock } from "./AudioClock";

/**
 * Production AudioClock, wrapping an AudioContext supplied by the caller.
 * The context itself must be created inside a user-gesture handler (e.g. a
 * "Start" button click), never at page load
 * (docs/JUNIOR_IMPLEMENTATION_PLAN.md WP-010 step 1) — and the same context
 * instance must be shared with the ClickSoundPlayer so scheduled click
 * times line up with `currentTimeSeconds`.
 */
export class WebAudioClock implements AudioClock {
  constructor(private readonly context: AudioContext) {}

  get currentTimeSeconds(): number {
    return this.context.currentTime;
  }

  async resume(): Promise<void> {
    if (this.context.state === "suspended") {
      await this.context.resume();
    }
  }
}
