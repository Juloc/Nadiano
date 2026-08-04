import type { PlayedMidiEvent } from "./types";

const SUSTAIN_CONTROLLER = 64;
const SUSTAIN_ON_THRESHOLD = 64;

/**
 * Tracks sustain pedal (CC64) state per channel from normalized events.
 * Values 0-63 are "off", 64-127 are "on" (standard MIDI controller convention).
 */
export class SustainState {
  private readonly sustainedChannels = new Set<number>();

  update(event: PlayedMidiEvent): void {
    if (event.kind !== "controlChange" || event.controller !== SUSTAIN_CONTROLLER || event.value === undefined) {
      return;
    }

    if (event.value >= SUSTAIN_ON_THRESHOLD) {
      this.sustainedChannels.add(event.channel);
    } else {
      this.sustainedChannels.delete(event.channel);
    }
  }

  isSustained(channel: number): boolean {
    return this.sustainedChannels.has(channel);
  }

  isAnySustained(): boolean {
    return this.sustainedChannels.size > 0;
  }

  reset(): void {
    this.sustainedChannels.clear();
  }
}
