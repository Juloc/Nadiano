export interface CapabilityResult {
  secureContext: boolean;
  midiAvailable: boolean;
  audioAvailable: boolean;
  indexedDbAvailable: boolean;
}

/**
 * Detects browser capabilities without requesting any permission or
 * creating any resource (no AudioContext, no MIDI access request). A
 * missing capability is reported as `false`, never thrown
 * (docs/JUNIOR_IMPLEMENTATION_PLAN.md WP-006 acceptance criteria).
 */
export function detectCapabilities(): CapabilityResult {
  const hasWindow = typeof window !== "undefined";
  const hasNavigator = typeof navigator !== "undefined";

  return {
    secureContext: hasWindow && window.isSecureContext === true,
    midiAvailable: hasNavigator && "requestMIDIAccess" in navigator,
    audioAvailable: hasWindow && "AudioContext" in window,
    indexedDbAvailable: hasWindow && "indexedDB" in window,
  };
}
