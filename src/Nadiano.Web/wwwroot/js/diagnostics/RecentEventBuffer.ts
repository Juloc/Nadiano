import type { PlayedMidiEvent } from "../midi/types";

/** Bounded ring buffer so the compact event list cannot grow into a full session history. */
export class RecentEventBuffer {
  private readonly events: PlayedMidiEvent[] = [];

  constructor(private readonly capacity: number = 20) {}

  push(event: PlayedMidiEvent): void {
    this.events.push(event);
    if (this.events.length > this.capacity) {
      this.events.shift();
    }
  }

  list(): readonly PlayedMidiEvent[] {
    return this.events;
  }

  clear(): void {
    this.events.length = 0;
  }
}
