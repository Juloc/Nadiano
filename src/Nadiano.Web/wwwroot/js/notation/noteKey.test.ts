import { describe, expect, it } from "vitest";
import { buildNoteKey } from "./noteKey";

describe("buildNoteKey", () => {
  it("matches the docs/CONTENT_MODEL.md convention", () => {
    expect(buildNoteKey(1, "1", 1)).toBe("m1-v1-n1");
  });

  it("produces distinct keys for different measures, voices or note indices", () => {
    const keys = new Set([buildNoteKey(1, "1", 1), buildNoteKey(2, "1", 1), buildNoteKey(1, "2", 1), buildNoteKey(1, "1", 2)]);

    expect(keys.size).toBe(4);
  });
});
