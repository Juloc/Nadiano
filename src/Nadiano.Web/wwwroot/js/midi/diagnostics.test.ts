import { describe, expect, it } from "vitest";
import { MidiDiagnosticsCounters } from "./diagnostics";

describe("MidiDiagnosticsCounters", () => {
  it("starts at zero", () => {
    const counters = new MidiDiagnosticsCounters();

    expect(counters.snapshot()).toEqual({ normalizedEventCount: 0, ignoredMessageCount: 0 });
  });

  it("counts normalized and ignored messages independently", () => {
    const counters = new MidiDiagnosticsCounters();

    counters.recordNormalized();
    counters.recordNormalized();
    counters.recordIgnored();

    expect(counters.snapshot()).toEqual({ normalizedEventCount: 2, ignoredMessageCount: 1 });
  });
});
