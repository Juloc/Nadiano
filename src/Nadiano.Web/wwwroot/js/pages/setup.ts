import { getAppVersion } from "../appVersion";
import { detectCapabilities } from "../capabilities";
import { ActiveNoteTracker } from "../diagnostics/ActiveNoteTracker";
import { buildDiagnosticsExport } from "../diagnostics/exportDiagnostics";
import { KeyboardView } from "../diagnostics/KeyboardView";
import { RecentEventBuffer } from "../diagnostics/RecentEventBuffer";
import type { MidiAccessAdapter } from "../midi/MidiAccessAdapter";
import { midiNoteName } from "../midi/noteNames";
import type { MidiInputDeviceInfo, PlayedMidiEvent } from "../midi/types";
import { WebMidiAccessAdapter } from "../midi/WebMidiAccessAdapter";
import { clearPreferredDevice, getPreferredDevice, setPreferredDevice } from "../setup/devicePreference";

const PEDAL_CONTROLLERS = [64, 66, 67] as const;
const REQUIRED_TEST_NOTES = 3;

type PedalController = typeof PEDAL_CONTROLLERS[number];
type StepState = "pending" | "current" | "complete";

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
  element.dataset.supported = String(supported);
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
  const connectStep = requireElement<HTMLElement>("setup-step-2");
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
  const recentEventsEmpty = requireElement<HTMLElement>("setup-recent-events-empty");
  const recentEventsList = requireElement<HTMLUListElement>("setup-recent-events-list");
  const exportButton = requireElement<HTMLButtonElement>("setup-export-button");
  const exportOutput = requireElement<HTMLElement>("setup-export-output");
  const noteTestState = requireElement<HTMLElement>("setup-note-test-state");
  const pedalTestState = requireElement<HTMLElement>("setup-pedal-test-state");
  const testCompleteButton = requireElement<HTMLButtonElement>("setup-test-complete-button");
  const completeStep = requireElement<HTMLElement>("setup-step-5");
  const completeHeading = requireElement<HTMLElement>("setup-complete-heading");

  const pedalElements = new Map<PedalController, HTMLElement>([
    [64, requireElement<HTMLElement>("setup-pedal-sustain")],
    [66, requireElement<HTMLElement>("setup-pedal-sostenuto")],
    [67, requireElement<HTMLElement>("setup-pedal-soft")],
  ]);
  const pedalValues = new Map<PedalController, number>(PEDAL_CONTROLLERS.map((controller) => [controller, 0]));
  const testedNotes = new Set<number>();
  const detectedPedals = new Set<PedalController>();
  const keyboardView = new KeyboardView(keyboardContainer);
  const activeNotes = new ActiveNoteTracker();
  const recentEvents = new RecentEventBuffer(20);
  const indonesian = document.documentElement.lang.startsWith("id");

  let selectedDeviceId: string | undefined = getPreferredDevice()?.id;
  let subscribed = false;

  function setStepState(step: number, state: StepState): void {
    const item = requireElement<HTMLElement>(`setup-progress-${step}`);
    const stateChip = item.querySelector<HTMLElement>(".setup-progress-state");
    item.dataset.state = state;
    if (state === "current") {
      item.setAttribute("aria-current", "step");
    } else {
      item.removeAttribute("aria-current");
    }
    if (stateChip) {
      stateChip.textContent = stateChip.dataset[state] ?? "";
    }
  }

  function showPreferredHint(name: string): void {
    preferredHint.hidden = false;
    preferredHintName.textContent = name;
    forgetButton.hidden = false;
  }

  function showStep(element: HTMLElement): void {
    element.hidden = false;
  }

  function renderTestProgress(): void {
    const noteCount = Math.min(REQUIRED_TEST_NOTES, testedNotes.size);
    const noteTemplate = indonesian ? noteTestState.dataset.templateId : noteTestState.dataset.templateDe;
    const pedalTemplate = indonesian ? pedalTestState.dataset.templateId : pedalTestState.dataset.templateDe;
    noteTestState.textContent = (noteTemplate ?? "{0}/3").replace("{0}", String(noteCount));
    pedalTestState.textContent = (pedalTemplate ?? "{0}").replace("{0}", String(detectedPedals.size));
    noteTestState.dataset.complete = String(testedNotes.size >= REQUIRED_TEST_NOTES);
    pedalTestState.dataset.complete = String(detectedPedals.size > 0);
    testCompleteButton.disabled = testedNotes.size < REQUIRED_TEST_NOTES;
  }

  function resetLiveState(): void {
    activeNotes.clear();
    keyboardView.clearAll();
    testedNotes.clear();
    detectedPedals.clear();
    completeStep.hidden = true;
    setStepState(5, "pending");
    for (const controller of PEDAL_CONTROLLERS) {
      pedalValues.set(controller, 0);
    }
    renderActiveNotes();
    renderPedals();
    renderTestProgress();
  }

  function enterDeviceTest(): void {
    devicesSection.hidden = false;
    diagnosticsSection.hidden = false;
    setStepState(3, "complete");
    setStepState(4, "current");
    resetLiveState();
  }

  function selectDevice(input: MidiInputDeviceInfo, inputs: MidiInputDeviceInfo[]): void {
    adapter.selectInput(input.id);
    setPreferredDevice({ id: input.id, name: input.name });
    selectedDeviceId = input.id;
    showPreferredHint(input.name);
    renderDevices(inputs);
    enterDeviceTest();
  }

  function renderDevices(inputs: MidiInputDeviceInfo[]): void {
    devicesList.replaceChildren();
    devicesEmpty.hidden = inputs.length > 0;

    const selected = inputs.find((input) => input.id === selectedDeviceId);
    if (selected && selected.state === "disconnected") {
      resetLiveState();
      diagnosticsSection.hidden = true;
      setStepState(3, "current");
      setStepState(4, "pending");
    }

    for (const input of inputs) {
      const item = document.createElement("li");
      item.className = "setup-device-item";

      const copy = document.createElement("div");
      const name = document.createElement("strong");
      const state = document.createElement("span");
      const stateLabel = input.state === "connected"
        ? devicesSection.dataset.labelConnected
        : devicesSection.dataset.labelDisconnected;
      name.textContent = input.name;
      state.textContent = `${input.manufacturer ? `${input.manufacturer} · ` : ""}${stateLabel ?? ""}`;
      state.className = "help-text";
      copy.append(name, state);
      item.appendChild(copy);

      const isSelected = input.id === selectedDeviceId;
      const button = document.createElement("button");
      button.type = "button";
      button.className = isSelected ? "button" : "button-secondary";
      button.textContent = isSelected
        ? (devicesSection.dataset.labelSelected ?? "")
        : (devicesSection.dataset.labelSelect ?? "");
      button.disabled = isSelected;
      button.addEventListener("click", () => selectDevice(input, inputs));
      item.appendChild(button);
      devicesList.appendChild(item);
    }
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

  function renderPedals(): void {
    for (const controller of PEDAL_CONTROLLERS) {
      const element = pedalElements.get(controller);
      const value = pedalValues.get(controller) ?? 0;
      if (element) {
        const active = value >= 64;
        const state = active
          ? (diagnosticsSection.dataset.labelPedalOn ?? "on")
          : (diagnosticsSection.dataset.labelPedalOff ?? "off");
        element.textContent = `${state} · ${value}`;
        element.dataset.active = String(active);
      }
    }
  }

  function describeEvent(event: PlayedMidiEvent): string {
    if (event.kind === "controlChange") {
      return `CC${event.controller} = ${event.value} (ch ${event.channel})`;
    }
    const noteName = event.note !== undefined ? midiNoteName(event.note) : "?";
    return `${event.kind} ${noteName} vel ${event.velocity ?? "-"} (ch ${event.channel})`;
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

  function handleMidiEvent(event: PlayedMidiEvent): void {
    recentEvents.push(event);
    renderRecentEvents();

    if (event.note !== undefined) {
      if (event.kind === "noteOn") {
        activeNotes.noteOn(event.note, event.velocity ?? 0, event.channel);
        keyboardView.setActive(event.note, true);
        testedNotes.add(event.note);
      } else if (event.kind === "noteOff") {
        activeNotes.noteOff(event.note);
        keyboardView.setActive(event.note, false);
      }
      renderActiveNotes();
    }

    if (event.kind === "controlChange" && PEDAL_CONTROLLERS.includes(event.controller as PedalController)) {
      const controller = event.controller as PedalController;
      const value = event.value ?? 0;
      pedalValues.set(controller, value);
      if (value >= 64) {
        detectedPedals.add(controller);
      }
      renderPedals();
    }

    renderTestProgress();
  }

  async function connect(): Promise<void> {
    guidanceUnsupported.hidden = true;
    guidanceDenied.hidden = true;
    connectButton.disabled = true;
    try {
      const result = await adapter.requestAccess();
      if (result.status === "unsupported") {
        guidanceUnsupported.hidden = false;
        return;
      }
      if (result.status === "denied") {
        guidanceDenied.hidden = false;
        return;
      }

      setStepState(2, "complete");
      setStepState(3, "current");
      showStep(devicesSection);
      renderDevices(result.inputs);

      if (!subscribed) {
        adapter.onEvent(handleMidiEvent);
        adapter.onDeviceChange(renderDevices);
        subscribed = true;
      }

      const preferred = selectedDeviceId
        ? result.inputs.find((input) => input.id === selectedDeviceId && input.state === "connected")
        : undefined;
      const connectedInputs = result.inputs.filter((input) => input.state === "connected");
      const automatic = preferred ?? (connectedInputs.length === 1 ? connectedInputs[0] : undefined);
      if (automatic) {
        selectDevice(automatic, result.inputs);
      }
    } finally {
      connectButton.disabled = false;
    }
  }

  const existingHint = getPreferredDevice();
  if (existingHint) {
    showPreferredHint(existingHint.name);
  }

  guidanceInsecure.hidden = capabilities.secureContext;
  guidanceUnsupported.hidden = capabilities.midiAvailable;
  const browserReady = capabilities.secureContext && capabilities.midiAvailable;
  if (browserReady) {
    setStepState(1, "complete");
    setStepState(2, "current");
    showStep(connectStep);
  }

  renderTestProgress();

  connectButton.addEventListener("click", () => void connect());
  forgetButton.addEventListener("click", () => {
    clearPreferredDevice();
    selectedDeviceId = undefined;
    preferredHint.hidden = true;
    forgetButton.hidden = true;
    diagnosticsSection.hidden = true;
    completeStep.hidden = true;
    resetLiveState();
    setStepState(3, "current");
    setStepState(4, "pending");
    renderDevices(adapter.listInputs());
  });

  testCompleteButton.addEventListener("click", () => {
    if (testedNotes.size < REQUIRED_TEST_NOTES) {
      return;
    }
    setStepState(4, "complete");
    setStepState(5, "complete");
    showStep(completeStep);
    completeHeading.tabIndex = -1;
    completeHeading.focus();
  });

  exportButton.addEventListener("click", () => {
    const selectedDevice = adapter.listInputs().find((input) => input.id === selectedDeviceId);
    const diagnostics = buildDiagnosticsExport(getAppVersion(), capabilities, selectedDevice, adapter.getDiagnostics());
    const json = JSON.stringify({ ...diagnostics, pedalControllers: Object.fromEntries(pedalValues) }, null, 2);
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
