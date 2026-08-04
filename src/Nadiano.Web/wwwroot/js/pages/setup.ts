import { getAppVersion } from "../appVersion";
import { detectCapabilities } from "../capabilities";
import { ActiveNoteTracker } from "../diagnostics/ActiveNoteTracker";
import { buildDiagnosticsExport } from "../diagnostics/exportDiagnostics";
import { KeyboardView } from "../diagnostics/KeyboardView";
import { RecentEventBuffer } from "../diagnostics/RecentEventBuffer";
import type { MidiAccessAdapter } from "../midi/MidiAccessAdapter";
import { midiNoteName } from "../midi/noteNames";
import { SustainState } from "../midi/sustainState";
import type { MidiInputDeviceInfo, PlayedMidiEvent } from "../midi/types";
import { WebMidiAccessAdapter } from "../midi/WebMidiAccessAdapter";
import { clearPreferredDevice, getPreferredDevice, setPreferredDevice } from "../setup/devicePreference";

function requireElement<T extends HTMLElement>(id: string): T {
  const element = document.getElementById(id);
  if (!element) {
    throw new Error(`Setup page markup is missing #${id}`);
  }
  return element as T;
}

function setCapabilityBadge(elementId: string, supported: boolean): void {
  const element = requireElement<HTMLElement>(elementId);
  element.textContent = supported ? (element.dataset.yes ?? "") : (element.dataset.no ?? "");
}

export function initSetupPage(adapter: MidiAccessAdapter): void {
  const capabilities = detectCapabilities();

  setCapabilityBadge("capability-secure-context", capabilities.secureContext);
  setCapabilityBadge("capability-midi", capabilities.midiAvailable);
  setCapabilityBadge("capability-audio", capabilities.audioAvailable);
  setCapabilityBadge("capability-indexeddb", capabilities.indexedDbAvailable);

  const guidanceInsecure = requireElement<HTMLElement>("setup-guidance-insecure");
  const guidanceUnsupported = requireElement<HTMLElement>("setup-guidance-unsupported");
  const guidanceDenied = requireElement<HTMLElement>("setup-guidance-denied");
  const connectButton = requireElement<HTMLButtonElement>("setup-connect-button");
  const devicesSection = requireElement<HTMLElement>("setup-devices-section");
  const devicesEmpty = requireElement<HTMLElement>("setup-devices-empty");
  const devicesList = requireElement<HTMLUListElement>("setup-devices-list");
  const preferredHint = requireElement<HTMLElement>("setup-preferred-hint");
  const preferredHintName = requireElement<HTMLElement>("setup-preferred-hint-name");
  const forgetButton = requireElement<HTMLButtonElement>("setup-forget-button");

  const diagnosticsSection = requireElement<HTMLElement>("setup-diagnostics-section");
  const keyboardContainer = requireElement<HTMLElement>("setup-keyboard");
  const activeNotesEmpty = requireElement<HTMLElement>("setup-active-notes-empty");
  const activeNotesList = requireElement<HTMLUListElement>("setup-active-notes-list");
  const sustainStateLabel = requireElement<HTMLElement>("setup-sustain-state");
  const recentEventsEmpty = requireElement<HTMLElement>("setup-recent-events-empty");
  const recentEventsList = requireElement<HTMLUListElement>("setup-recent-events-list");
  const exportButton = requireElement<HTMLButtonElement>("setup-export-button");
  const exportOutput = requireElement<HTMLElement>("setup-export-output");

  const keyboardView = new KeyboardView(keyboardContainer);
  const activeNotes = new ActiveNoteTracker();
  const sustainState = new SustainState();
  const recentEvents = new RecentEventBuffer(20);

  guidanceInsecure.hidden = capabilities.secureContext;

  let selectedDeviceId: string | undefined = getPreferredDevice()?.id;
  let deviceChangeSubscribed = false;

  const existingHint = getPreferredDevice();
  if (existingHint) {
    showPreferredHint(existingHint.name);
  }

  function showPreferredHint(name: string): void {
    preferredHint.hidden = false;
    preferredHintName.textContent = name;
    forgetButton.hidden = false;
  }

  function selectDevice(input: MidiInputDeviceInfo, inputs: MidiInputDeviceInfo[]): void {
    adapter.selectInput(input.id);
    setPreferredDevice({ id: input.id, name: input.name });
    selectedDeviceId = input.id;
    showPreferredHint(input.name);
    renderDevices(inputs);
  }

  function renderDevices(inputs: MidiInputDeviceInfo[]): void {
    devicesList.replaceChildren();
    devicesEmpty.hidden = inputs.length > 0;

    const selected = inputs.find((input) => input.id === selectedDeviceId);
    if (selected && selected.state === "disconnected") {
      resetLiveState();
    }

    for (const input of inputs) {
      const item = document.createElement("li");

      const label = document.createElement("span");
      const stateLabel =
        input.state === "connected" ? devicesSection.dataset.labelConnected : devicesSection.dataset.labelDisconnected;
      label.textContent = `${input.name}${input.manufacturer ? ` (${input.manufacturer})` : ""} — ${stateLabel ?? ""}`;
      item.appendChild(label);

      const isSelected = input.id === selectedDeviceId;
      const button = document.createElement("button");
      button.type = "button";
      button.className = "button-secondary";
      button.textContent = isSelected
        ? (devicesSection.dataset.labelSelected ?? "")
        : (devicesSection.dataset.labelSelect ?? "");
      button.disabled = isSelected;
      button.addEventListener("click", () => selectDevice(input, inputs));
      item.appendChild(button);

      devicesList.appendChild(item);
    }
  }

  function resetLiveState(): void {
    activeNotes.clear();
    keyboardView.clearAll();
    sustainState.reset();
    renderActiveNotes();
    renderSustainState();
  }

  function renderActiveNotes(): void {
    const notes = activeNotes.list();
    activeNotesEmpty.hidden = notes.length > 0;
    activeNotesList.replaceChildren();

    for (const note of notes) {
      const item = document.createElement("li");
      item.textContent = `${midiNoteName(note.note)} (velocity ${note.velocity})`;
      activeNotesList.appendChild(item);
    }
  }

  function renderSustainState(): void {
    sustainStateLabel.textContent = sustainState.isAnySustained()
      ? (diagnosticsSection.dataset.labelSustainOn ?? "")
      : (diagnosticsSection.dataset.labelSustainOff ?? "");
  }

  function renderRecentEvents(): void {
    const events = recentEvents.list();
    recentEventsEmpty.hidden = events.length > 0;
    recentEventsList.replaceChildren();

    for (const event of [...events].reverse()) {
      const item = document.createElement("li");
      item.textContent = describeEvent(event);
      recentEventsList.appendChild(item);
    }
  }

  function describeEvent(event: PlayedMidiEvent): string {
    if (event.kind === "controlChange") {
      return `CC${event.controller} = ${event.value} (ch ${event.channel})`;
    }
    const noteName = event.note !== undefined ? midiNoteName(event.note) : "?";
    return `${event.kind} ${noteName} vel ${event.velocity ?? "-"} (ch ${event.channel})`;
  }

  function handleMidiEvent(event: PlayedMidiEvent): void {
    recentEvents.push(event);
    renderRecentEvents();

    if (event.note !== undefined) {
      if (event.kind === "noteOn") {
        activeNotes.noteOn(event.note, event.velocity ?? 0, event.channel);
        keyboardView.setActive(event.note, true);
      } else if (event.kind === "noteOff") {
        activeNotes.noteOff(event.note);
        keyboardView.setActive(event.note, false);
      }
      renderActiveNotes();
    }

    sustainState.update(event);
    renderSustainState();
  }

  connectButton.addEventListener("click", () => {
    void connect();
  });

  async function connect(): Promise<void> {
    guidanceUnsupported.hidden = true;
    guidanceDenied.hidden = true;

    const result = await adapter.requestAccess();

    if (result.status === "unsupported") {
      guidanceUnsupported.hidden = false;
      return;
    }

    if (result.status === "denied") {
      guidanceDenied.hidden = false;
      return;
    }

    devicesSection.hidden = false;
    diagnosticsSection.hidden = false;
    renderDevices(result.inputs);
    renderActiveNotes();
    renderSustainState();
    renderRecentEvents();

    if (!deviceChangeSubscribed) {
      adapter.onEvent(handleMidiEvent);
      adapter.onDeviceChange((inputs) => renderDevices(inputs));
      deviceChangeSubscribed = true;
    }

    if (selectedDeviceId && result.inputs.some((input) => input.id === selectedDeviceId)) {
      adapter.selectInput(selectedDeviceId);
    }
  }

  forgetButton.addEventListener("click", () => {
    clearPreferredDevice();
    selectedDeviceId = undefined;
    preferredHint.hidden = true;
    forgetButton.hidden = true;
    renderDevices(adapter.listInputs());
  });

  exportButton.addEventListener("click", () => {
    const selectedDevice = adapter.listInputs().find((input) => input.id === selectedDeviceId);
    const diagnostics = buildDiagnosticsExport(getAppVersion(), capabilities, selectedDevice, adapter.getDiagnostics());
    const json = JSON.stringify(diagnostics, null, 2);

    exportOutput.hidden = false;
    exportOutput.textContent = json;

    const blob = new Blob([json], { type: "application/json" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = "nadiano-diagnostics.json";
    link.click();
    URL.revokeObjectURL(url);
  });
}

if (document.getElementById("setup-connect-button")) {
  initSetupPage(new WebMidiAccessAdapter());
}
