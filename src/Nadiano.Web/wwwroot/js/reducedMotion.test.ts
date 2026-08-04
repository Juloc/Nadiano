import { afterEach, describe, expect, it, vi } from "vitest";
import { prefersReducedMotion } from "./reducedMotion";

describe("prefersReducedMotion", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("returns false when window/matchMedia is unavailable, instead of throwing", () => {
    expect(prefersReducedMotion()).toBe(false);
  });

  it("returns true when the browser reports a reduced-motion preference", () => {
    vi.stubGlobal("window", { matchMedia: () => ({ matches: true }) });

    expect(prefersReducedMotion()).toBe(true);
  });

  it("returns false when the browser reports no reduced-motion preference", () => {
    vi.stubGlobal("window", { matchMedia: () => ({ matches: false }) });

    expect(prefersReducedMotion()).toBe(false);
  });
});
