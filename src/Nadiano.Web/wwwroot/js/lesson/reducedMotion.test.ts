import { describe, expect, it } from "vitest";
import { shouldLoopMedia } from "./reducedMotion";

describe("shouldLoopMedia", () => {
  it("loops when the author requested it and the learner has no motion preference", () => {
    expect(shouldLoopMedia(true, false)).toBe(true);
  });

  it("never loops when the learner prefers reduced motion, even if the author requested it", () => {
    expect(shouldLoopMedia(true, true)).toBe(false);
  });

  it("never loops when the author did not request it", () => {
    expect(shouldLoopMedia(false, false)).toBe(false);
    expect(shouldLoopMedia(false, true)).toBe(false);
  });
});
