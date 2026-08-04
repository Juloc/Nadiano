/** Plays one metronome click at a precise AudioContext time. Kept separate from Metronome so scheduling logic is testable without real audio. */
export interface ClickSoundPlayer {
  playClickAt(timeSeconds: number, accented: boolean): void;
}
