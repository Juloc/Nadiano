import { describe, expect, it } from "vitest";
import { isBlackKey, midiNoteName } from "./noteNames";

describe("midiNoteName", () => {
  it("names middle C (60) as C4", () => {
    expect(midiNoteName(60)).toBe("C4");
  });

  it("names the lowest 88-key note (21) as A0", () => {
    expect(midiNoteName(21)).toBe("A0");
  });

  it("names the highest 88-key note (108) as C8", () => {
    expect(midiNoteName(108)).toBe("C8");
  });
});

describe("isBlackKey", () => {
  it("identifies C as a white key", () => {
    expect(isBlackKey(60)).toBe(false);
  });

  it("identifies C# as a black key", () => {
    expect(isBlackKey(61)).toBe(true);
  });
});
