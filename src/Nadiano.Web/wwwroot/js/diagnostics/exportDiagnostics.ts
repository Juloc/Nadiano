import type { CapabilityResult } from "../capabilities";
import type { MidiDiagnosticsSnapshot } from "../midi/diagnostics";
import type { MidiInputDeviceInfo } from "../midi/types";

export interface DiagnosticsExport {
  generatedAtIso: string;
  appVersion: string;
  capabilities: CapabilityResult;
  selectedDevice?: { name: string; manufacturer?: string; state: string };
  midi: MidiDiagnosticsSnapshot;
}

/**
 * Builds the diagnostics export. Deliberately excludes the raw device id,
 * profile prose and any note-by-note practice history — only what is
 * needed to debug a connection problem
 * (docs/JUNIOR_IMPLEMENTATION_PLAN.md WP-009, docs/TECHNICAL_ARCHITECTURE.md §18).
 */
export function buildDiagnosticsExport(
  appVersion: string,
  capabilities: CapabilityResult,
  selectedDevice: MidiInputDeviceInfo | undefined,
  midi: MidiDiagnosticsSnapshot,
  now: Date = new Date(),
): DiagnosticsExport {
  return {
    generatedAtIso: now.toISOString(),
    appVersion,
    capabilities,
    selectedDevice: selectedDevice
      ? { name: selectedDevice.name, manufacturer: selectedDevice.manufacturer, state: selectedDevice.state }
      : undefined,
    midi,
  };
}
