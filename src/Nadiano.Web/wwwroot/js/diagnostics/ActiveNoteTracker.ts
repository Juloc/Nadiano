export interface ActiveNote {
  note: number;
  velocity: number;
  channel: number;
}

/**
 * Tracks currently-held notes so the UI can show a text alternative to the
 * color-only keyboard highlight (docs/JUNIOR_IMPLEMENTATION_PLAN.md WP-009
 * acceptance: "keyboard view uses text/non-color status alternatives").
 */
export class ActiveNoteTracker {
  private readonly active = new Map<number, ActiveNote>();

  noteOn(note: number, velocity: number, channel: number): void {
    this.active.set(note, { note, velocity, channel });
  }

  noteOff(note: number): void {
    this.active.delete(note);
  }

  clear(): void {
    this.active.clear();
  }

  list(): ActiveNote[] {
    return Array.from(this.active.values()).sort((a, b) => a.note - b.note);
  }
}
