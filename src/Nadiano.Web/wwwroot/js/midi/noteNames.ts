const NOTE_NAMES = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"];

/** Converts a MIDI note number to a name using the common C4 = middle C (note 60) convention. */
export function midiNoteName(note: number): string {
  const octave = Math.floor(note / 12) - 1;
  const name = NOTE_NAMES[note % 12];
  return `${name}${octave}`;
}

export function isBlackKey(note: number): boolean {
  return NOTE_NAMES[note % 12]!.includes("#");
}
