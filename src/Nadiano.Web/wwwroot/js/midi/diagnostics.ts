export interface MidiDiagnosticsSnapshot {
  normalizedEventCount: number;
  ignoredMessageCount: number;
}

/**
 * Counts messages the normalizer accepted vs. safely ignored (system
 * messages, malformed data), so connection problems are observable without
 * exposing raw MIDI payloads (docs/JUNIOR_IMPLEMENTATION_PLAN.md WP-008/WP-009).
 */
export class MidiDiagnosticsCounters {
  private normalizedEventCount = 0;
  private ignoredMessageCount = 0;

  recordNormalized(): void {
    this.normalizedEventCount += 1;
  }

  recordIgnored(): void {
    this.ignoredMessageCount += 1;
  }

  snapshot(): MidiDiagnosticsSnapshot {
    return {
      normalizedEventCount: this.normalizedEventCount,
      ignoredMessageCount: this.ignoredMessageCount,
    };
  }
}
