import type { ClickSoundPlayer } from "./ClickSoundPlayer";

const CLICK_DURATION_SECONDS = 0.05;
const ACCENT_FREQUENCY_HZ = 1000;
const REGULAR_FREQUENCY_HZ = 800;
const PEAK_GAIN = 0.5;
const SILENT_GAIN = 0.0001;

/**
 * Production ClickSoundPlayer: one short synthesized tone per beat, no audio
 * assets required. Must be constructed with the same AudioContext instance
 * given to WebAudioClock so scheduled times agree.
 */
export class WebAudioClickSoundPlayer implements ClickSoundPlayer {
  constructor(private readonly audioContext: AudioContext) {}

  playClickAt(timeSeconds: number, accented: boolean): void {
    const oscillator = this.audioContext.createOscillator();
    const gain = this.audioContext.createGain();

    oscillator.frequency.value = accented ? ACCENT_FREQUENCY_HZ : REGULAR_FREQUENCY_HZ;
    oscillator.connect(gain);
    gain.connect(this.audioContext.destination);

    gain.gain.setValueAtTime(SILENT_GAIN, timeSeconds);
    gain.gain.exponentialRampToValueAtTime(PEAK_GAIN, timeSeconds + 0.001);
    gain.gain.exponentialRampToValueAtTime(SILENT_GAIN, timeSeconds + CLICK_DURATION_SECONDS);

    oscillator.start(timeSeconds);
    oscillator.stop(timeSeconds + CLICK_DURATION_SECONDS);
  }
}
