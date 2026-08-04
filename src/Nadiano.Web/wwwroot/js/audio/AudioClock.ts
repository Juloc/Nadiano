/** Consumer-facing contract for the Web Audio-backed practice clock. */
export interface AudioClock {
  readonly currentTimeSeconds: number;
  resume(): Promise<void>;
}
