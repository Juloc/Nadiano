// @vitest-environment happy-dom
import { beforeEach, describe, expect, it } from "vitest";
import { KeyboardView } from "./KeyboardView";

describe("KeyboardView", () => {
  let container: HTMLElement;
  let view: KeyboardView;

  beforeEach(() => {
    container = document.createElement("div");
    view = new KeyboardView(container);
  });

  it("renders all 88 keys", () => {
    expect(container.querySelectorAll(".keyboard-key")).toHaveLength(88);
  });

  it("highlights a note as active", () => {
    view.setActive(60, true);

    const key = container.querySelector('[data-note="60"]');
    expect(key?.classList.contains("keyboard-key-active")).toBe(true);
  });

  it("clears the highlight after note-off", () => {
    view.setActive(60, true);
    view.setActive(60, false);

    const key = container.querySelector('[data-note="60"]');
    expect(key?.classList.contains("keyboard-key-active")).toBe(false);
  });

  it("clears every highlighted key on disconnect (clearAll)", () => {
    view.setActive(60, true);
    view.setActive(64, true);
    view.setActive(67, true);

    view.clearAll();

    expect(container.querySelectorAll(".keyboard-key-active")).toHaveLength(0);
  });

  it("marks known black keys distinctly from white keys", () => {
    const cSharp = container.querySelector('[data-note="61"]');
    const c = container.querySelector('[data-note="60"]');

    expect(cSharp?.classList.contains("keyboard-key-black")).toBe(true);
    expect(c?.classList.contains("keyboard-key-white")).toBe(true);
  });
});
