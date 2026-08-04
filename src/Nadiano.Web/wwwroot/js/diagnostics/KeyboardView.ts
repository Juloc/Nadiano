import { isBlackKey } from "../midi/noteNames";

const LOWEST_NOTE = 21; // A0
const HIGHEST_NOTE = 108; // C8

/**
 * Renders a simple 88-key strip and lets callers toggle the active/highlighted
 * state per note. This is a diagnostics aid, not notation — it does not
 * attempt real piano key proportions.
 */
export class KeyboardView {
  private readonly keyElements = new Map<number, HTMLElement>();

  constructor(private readonly container: HTMLElement) {
    this.render();
  }

  setActive(note: number, active: boolean): void {
    this.keyElements.get(note)?.classList.toggle("keyboard-key-active", active);
  }

  clearAll(): void {
    for (const key of this.keyElements.values()) {
      key.classList.remove("keyboard-key-active");
    }
  }

  private render(): void {
    this.container.replaceChildren();
    this.keyElements.clear();

    for (let note = LOWEST_NOTE; note <= HIGHEST_NOTE; note += 1) {
      const key = document.createElement("div");
      key.className = isBlackKey(note) ? "keyboard-key keyboard-key-black" : "keyboard-key keyboard-key-white";
      key.dataset.note = String(note);
      this.keyElements.set(note, key);
      this.container.appendChild(key);
    }
  }
}
