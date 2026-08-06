import { Metronome } from "../audio/Metronome";
import { WebAudioClickSoundPlayer } from "../audio/WebAudioClickSoundPlayer";
import { WebAudioClock } from "../audio/WebAudioClock";
import type { MidiAccessAdapter, Unsubscribe } from "../midi/MidiAccessAdapter";
import { midiNoteName } from "../midi/noteNames";
import type { MidiAccessResult, MidiInputDeviceInfo, PlayedMidiEvent } from "../midi/types";
import { WebMidiAccessAdapter } from "../midi/WebMidiAccessAdapter";
import { NotationAdapter } from "../notation/NotationAdapter";
import type { PracticeMode, PracticeSessionResult } from "../practice/PracticeSession";
import { PracticeSession } from "../practice/PracticeSession";
import { completeSession, createSession } from "../practice/practiceApi";
import type { AssessmentCategory } from "../scoring/computeScoringFacts";
import type { MatchResult } from "../scoring/matcher";
import { resolveExpectedEventTiming, type ResolvedExpectedEvent } from "../scoring/resolveExpectedEventTiming";
import { NORMAL_MODE_POLICY, PERFORMANCE_MODE_POLICY, WAIT_MODE_POLICY } from "../scoring/ScoringPolicy";
import type { ExpectedEventDocument } from "../scoring/types";

const RESULT_SCHEMA_VERSION = 1;
const BEATS_PER_MEASURE = 4;
const RHYTHM_PITCH = 60;

class RhythmMidiAdapter implements MidiAccessAdapter {
  constructor(private readonly inner: MidiAccessAdapter) {}

  isSupported(): boolean {
    return this.inner.isSupported();
  }

  requestAccess(): Promise<MidiAccessResult> {
    return this.inner.requestAccess();
  }

  listInputs(): MidiInputDeviceInfo[] {
    return this.inner.listInputs();
  }

  selectInput(deviceId: string): void {
    this.inner.selectInput(deviceId);
  }

  onEvent(listener: (event: PlayedMidiEvent) => void): Unsubscribe {
    return this.inner.onEvent((event) => {
      if ((event.kind === "noteOn" || event.kind === "noteOff") && event.note !== undefined) {
        listener({ ...event, note: RHYTHM_PITCH });
        return;
      }
      listener(event);
    });
  }

  onDeviceChange(listener: (inputs: MidiInputDeviceInfo[]) => void): Unsubscribe {
    return this.inner.onDeviceChange(listener);
  }

  getDiagnostics() {
    return this.inner.getDiagnostics();
  }
}

function requireElement<T extends HTMLElement>(id: string): T {
  const element = document.getElementById(id);
  if (!element) {
    throw new Error(`Practice page markup is missing #${id}`);
  }
  return element as T;
}

function policyForMode(mode: PracticeMode) {
  if (mode === "wait") {
    return WAIT_MODE_POLICY;
  }
  return mode === "performance" ? PERFORMANCE_MODE_POLICY : NORMAL_MODE_POLICY;
}

function initPracticeWorkspace(): void {
  const workspace = requireElement<HTMLElement>("practice-workspace");
  const lessonId = workspace.dataset.lessonId ?? "";
  const contentVersion = workspace.dataset.contentVersion ?? "";
  const scoreUrl = workspace.dataset.scoreUrl ?? "";
  const expectedEventsUrl = workspace.dataset.expectedEventsUrl ?? "";
  const initialTempoBpm = Number(workspace.dataset.targetTempo) || 90;
  const countInMeasures = Number(workspace.dataset.countInMeasures) || 0;
  const enabledCategories = JSON.parse(workspace.dataset.assessmentCategories ?? "[]") as AssessmentCategory[];

  const targetTempoLabel = requireElement<HTMLElement>("workspace-target-tempo");
  const deviceStatus = requireElement<HTMLElement>("workspace-device-status");
  const connectButton = requireElement<HTMLButtonElement>("workspace-connect-button");
  const loadError = requireElement<HTMLElement>("workspace-load-error");
  const notationContainer = requireElement<HTMLElement>("workspace-notation");
  const zoomInput = requireElement<HTMLInputElement>("workspace-zoom");
  const modeSelect = requireElement<HTMLSelectElement>("workspace-mode");
  const modeHint = requireElement<HTMLElement>("workspace-mode-hint");
  const handFields = requireElement<HTMLElement>("workspace-hand-range");
  const handSelect = requireElement<HTMLSelectElement>("workspace-hand");
  const loopRangeFields = requireElement<HTMLElement>("workspace-loop-range");
  const fromInput = requireElement<HTMLInputElement>("workspace-from-measure");
  const toInput = requireElement<HTMLInputElement>("workspace-to-measure");
  const startButton = requireElement<HTMLButtonElement>("workspace-start-button");
  const stopButton = requireElement<HTMLButtonElement>("workspace-stop-button");
  const liveSection = requireElement<HTMLElement>("workspace-live-section");
  const liveList = requireElement<HTMLUListElement>("workspace-live-list");
  const resultSection = requireElement<HTMLElement>("workspace-result-section");
  const resultList = requireElement<HTMLUListElement>("workspace-result-list");
  const nextActionLabel = requireElement<HTMLElement>("workspace-next-action");
  const retryButton = requireElement<HTMLButtonElement>("workspace-retry-button");

  const indonesian = document.documentElement.lang.startsWith("id");
  const midiAdapter = new WebMidiAccessAdapter();
  const rhythmAdapter = new RhythmMidiAdapter(midiAdapter);
  const notationAdapter = new NotationAdapter(notationContainer);
  let expectedDocument: ExpectedEventDocument | undefined;
  let audioContext: AudioContext | undefined;
  let metronome: Metronome | undefined;
  let session: PracticeSession | undefined;
  let currentResolvedExpected: ResolvedExpectedEvent[] = [];
  let currentSessionId: string | undefined;
  let currentTempoBpm = initialTempoBpm;

  function updateTempoLabel(): void {
    targetTempoLabel.textContent = String(currentTempoBpm);
  }

  function updateModeControls(): void {
    const mode = modeSelect.value as PracticeMode;
    loopRangeFields.hidden = mode !== "loop";
    handFields.hidden = mode !== "hands-separate";
    const hints: Record<PracticeMode, string> = {
      wait: indonesian ? "Nada berikutnya menunggu sampai benar." : "Die nächste Note wartet, bis sie richtig gespielt wurde.",
      loop: indonesian ? "Hanya bagian birama yang dipilih diulang." : "Nur der gewählte Taktbereich wird wiederholt.",
      "hands-separate": indonesian ? "Hanya not untuk tangan yang dipilih dinilai." : "Nur die Noten der gewählten Hand werden bewertet.",
      rhythm: indonesian ? "Setiap tuts boleh dipakai; hanya waktu dinilai." : "Jede Taste ist erlaubt; bewertet wird nur der Rhythmus.",
      "tempo-ladder": indonesian ? "Tempo naik 5 BPM setelah hasil baik dan turun setelah kesalahan." : "Das Tempo steigt nach einem guten Lauf um 5 BPM und sinkt nach Fehlern.",
      "listen-and-copy": indonesian ? "Nadiano memainkan contoh singkat sebelum hitungan masuk." : "Nadiano spielt vor dem Einzähler ein kurzes Klangbeispiel.",
      performance: indonesian ? "Mainkan tanpa berhenti; hasil muncul setelah selesai." : "Ohne Unterbrechung durchspielen; Feedback folgt am Ende.",
    };
    modeHint.textContent = hints[mode];
  }

  async function loadLesson(): Promise<void> {
    try {
      const [scoreResponse, eventsResponse] = await Promise.all([fetch(scoreUrl), fetch(expectedEventsUrl)]);
      if (!scoreResponse.ok || !eventsResponse.ok) {
        throw new Error("Lesson assets could not be loaded.");
      }
      const musicXml = await scoreResponse.text();
      const renderResult = await notationAdapter.loadAndRender(musicXml);
      if (renderResult.status === "error") {
        throw new Error(renderResult.reason);
      }
      expectedDocument = (await eventsResponse.json()) as ExpectedEventDocument;
      fromInput.max = String(notationAdapter.measureCount);
      toInput.max = String(notationAdapter.measureCount);
      toInput.value = String(notationAdapter.measureCount);
    } catch {
      loadError.hidden = false;
      startButton.disabled = true;
    }
  }

  zoomInput.addEventListener("input", () => notationAdapter.setZoom(Number(zoomInput.value) || 1));
  modeSelect.addEventListener("change", updateModeControls);
  handSelect.addEventListener("change", () => {
    liveSection.hidden = true;
    resultSection.hidden = true;
  });
  connectButton.addEventListener("click", () => void connect());

  async function connect(): Promise<void> {
    const result = await midiAdapter.requestAccess();
    deviceStatus.hidden = false;
    if (result.status === "granted") {
      deviceStatus.textContent = deviceStatus.dataset.connected ?? "";
      const firstInput = result.inputs[0];
      if (firstInput) {
        midiAdapter.selectInput(firstInput.id);
      }
    } else {
      deviceStatus.textContent = deviceStatus.dataset.notConnected ?? "";
    }
  }

  function eventsForCurrentMode(): ExpectedEventDocument | undefined {
    if (!expectedDocument) {
      return undefined;
    }
    const mode = modeSelect.value as PracticeMode;
    let events = expectedDocument.events;
    if (mode === "loop") {
      const from = Number(fromInput.value) || 1;
      const to = Number(toInput.value) || from;
      events = events.filter((event) => event.measure >= from && event.measure <= to);
    }
    if (mode === "hands-separate") {
      const selectedHand = handSelect.value;
      events = events.filter((event) => event.hand === selectedHand || event.hand === "both" || event.hand === undefined);
    }
    if (mode === "rhythm") {
      events = events.map((event) => ({ ...event, pitches: [RHYTHM_PITCH] }));
    }
    return { ...expectedDocument, events };
  }

  function renderLiveList(resolvedExpected: readonly ResolvedExpectedEvent[], matchResult: MatchResult | undefined): void {
    const statusByKey = new Map<string, "correct" | "missed">();
    if (matchResult) {
      for (const outcome of matchResult.expected) {
        statusByKey.set(`${outcome.expectedGroupId}|${outcome.pitch}`, outcome.status === "matched" ? "correct" : "missed");
      }
    }
    liveList.replaceChildren();
    for (const slot of resolvedExpected) {
      const status = statusByKey.get(`${slot.groupId}|${slot.pitch}`) ?? "pending";
      const label = status === "correct"
        ? liveList.dataset.statusCorrect
        : status === "missed"
          ? liveList.dataset.statusMissed
          : liveList.dataset.statusPending;
      const item = document.createElement("li");
      item.textContent = `${midiNoteName(slot.pitch)}: ${label ?? ""}`;
      item.className = `practice-status-${status}`;
      liveList.appendChild(item);
    }
  }

  function addResultItem(text: string): void {
    if (!text) {
      return;
    }
    const item = document.createElement("li");
    item.textContent = text;
    resultList.appendChild(item);
  }

  function renderResult(result: PracticeSessionResult): void {
    liveSection.hidden = true;
    resultSection.hidden = false;
    resultList.replaceChildren();
    const facts = result.facts;
    if (facts.pitch) {
      addResultItem((resultSection.dataset.pitchTemplate ?? "").replace("{0}", String(facts.pitch.correctCount)).replace("{1}", String(facts.pitch.totalExpected)));
      if (facts.pitch.additionCount > 0) {
        addResultItem((resultSection.dataset.additionsTemplate ?? "").replace("{0}", String(facts.pitch.additionCount)));
      }
    }
    if (facts.onset && facts.onset.deviations.length > 0) {
      const onTimeCount = facts.onset.deviations.filter((deviation) => deviation.band === "onTime").length;
      addResultItem((resultSection.dataset.onsetTemplate ?? "").replace("{0}", String(onTimeCount)).replace("{1}", String(facts.onset.deviations.length)));
    }
    if (facts.duration && facts.duration.ratios.length > 0) {
      const average = facts.duration.ratios.reduce((sum, ratio) => sum + ratio.ratio, 0) / facts.duration.ratios.length;
      addResultItem((resultSection.dataset.durationTemplate ?? "").replace("{0}", average.toFixed(2)));
    }
    if (facts.steadiness && facts.steadiness.intervalVariability !== null) {
      addResultItem((resultSection.dataset.steadinessTemplate ?? "").replace("{0}", facts.steadiness.intervalVariability.toFixed(2)));
    }
    if (facts.dynamics && facts.dynamics.minVelocity !== null) {
      addResultItem((resultSection.dataset.dynamicsTemplate ?? "")
        .replace("{0}", String(facts.dynamics.minVelocity))
        .replace("{1}", String(facts.dynamics.maxVelocity))
        .replace("{2}", String(Math.round(facts.dynamics.averageVelocity ?? 0))));
    }

    const nextActionText: Record<string, string | undefined> = {
      "hands-separate": resultSection.dataset.nextActionHandsSeparate,
      "repeat-slower": resultSection.dataset.nextActionRepeatSlower,
      "repeat-section": resultSection.dataset.nextActionRepeatSection,
      "well-done": resultSection.dataset.nextActionWellDone,
    };
    nextActionLabel.textContent = nextActionText[result.nextAction] ?? "";

    if (modeSelect.value === "tempo-ladder") {
      currentTempoBpm = result.nextAction === "well-done"
        ? Math.min(240, currentTempoBpm + 5)
        : Math.max(30, currentTempoBpm - 5);
      updateTempoLabel();
      addResultItem(indonesian ? `Tempo berikutnya: ${currentTempoBpm} BPM` : `Nächstes Tempo: ${currentTempoBpm} BPM`);
    }
    startButton.disabled = false;
    stopButton.disabled = true;
  }

  async function ensureAudioContext(): Promise<AudioContext> {
    audioContext ??= new AudioContext();
    if (audioContext.state === "suspended") {
      await audioContext.resume();
    }
    return audioContext;
  }

  async function ensureMetronome(): Promise<Metronome> {
    if (!metronome) {
      const context = await ensureAudioContext();
      metronome = new Metronome(new WebAudioClock(context), new WebAudioClickSoundPlayer(context));
    }
    return metronome;
  }

  async function playCountIn(): Promise<void> {
    if (countInMeasures <= 0) {
      return;
    }
    const activeMetronome = await ensureMetronome();
    await activeMetronome.start({ bpm: currentTempoBpm, beatsPerMeasure: BEATS_PER_MEASURE });
    const countInDurationMs = (60000 / currentTempoBpm) * BEATS_PER_MEASURE * countInMeasures;
    await new Promise((resolve) => setTimeout(resolve, countInDurationMs));
    activeMetronome.stop();
  }

  async function playReference(document: ExpectedEventDocument): Promise<void> {
    const context = await ensureAudioContext();
    const msPerBeat = 60000 / currentTempoBpm;
    const previewEvents = document.events.slice(0, 12);
    if (previewEvents.length === 0) {
      return;
    }
    const firstBeat = (previewEvents[0]?.measure ?? 1) * BEATS_PER_MEASURE - BEATS_PER_MEASURE + (previewEvents[0]?.beat ?? 0);
    let previewDurationMs = 0;
    for (const event of previewEvents) {
      const absoluteBeat = (event.measure - 1) * BEATS_PER_MEASURE + event.beat;
      const offsetSeconds = Math.max(0, (absoluteBeat - firstBeat) * msPerBeat / 1000);
      const durationSeconds = Math.min(1.2, Math.max(0.12, event.durationBeats * msPerBeat / 1000));
      previewDurationMs = Math.max(previewDurationMs, (offsetSeconds + durationSeconds) * 1000);
      for (const pitch of event.pitches) {
        const oscillator = context.createOscillator();
        const gain = context.createGain();
        const startsAt = context.currentTime + 0.1 + offsetSeconds;
        oscillator.frequency.value = 440 * Math.pow(2, (pitch - 69) / 12);
        gain.gain.setValueAtTime(0.0001, startsAt);
        gain.gain.exponentialRampToValueAtTime(0.13, startsAt + 0.02);
        gain.gain.exponentialRampToValueAtTime(0.0001, startsAt + durationSeconds);
        oscillator.connect(gain).connect(context.destination);
        oscillator.start(startsAt);
        oscillator.stop(startsAt + durationSeconds + 0.02);
      }
    }
    await new Promise((resolve) => setTimeout(resolve, Math.min(10_000, previewDurationMs + 250)));
  }

  async function start(): Promise<void> {
    const events = eventsForCurrentMode();
    if (!events || events.events.length === 0) {
      return;
    }
    resultSection.hidden = true;
    startButton.disabled = true;
    stopButton.disabled = false;
    const mode = modeSelect.value as PracticeMode;
    if (mode === "listen-and-copy") {
      await playReference(events);
    }
    await playCountIn();

    try {
      currentSessionId = await createSession(lessonId, contentVersion, mode);
    } catch {
      currentSessionId = undefined;
    }

    const policy = policyForMode(mode);
    const sessionStartAtMs = performance.now();
    currentResolvedExpected = resolveExpectedEventTiming(events, BEATS_PER_MEASURE, sessionStartAtMs, currentTempoBpm);
    liveSection.hidden = false;
    renderLiveList(currentResolvedExpected, undefined);

    const categories = mode === "rhythm"
      ? enabledCategories.filter((category) => category === "onset" || category === "steadiness")
      : enabledCategories;
    session = new PracticeSession(mode === "rhythm" ? rhythmAdapter : midiAdapter, currentResolvedExpected, {
      mode,
      policy,
      enabledCategories: categories,
      onTimeToleranceMs: policy.onTimeToleranceMs,
    });
    session.onLiveUpdate = (result) => renderLiveList(currentResolvedExpected, result);
    session.onComplete = (result) => {
      renderResult(result);
      void persistResult(result);
    };
    session.start();
  }

  async function persistResult(result: PracticeSessionResult): Promise<void> {
    if (!currentSessionId) {
      return;
    }
    try {
      await completeSession(currentSessionId, RESULT_SCHEMA_VERSION, JSON.stringify(result.facts), result.nextAction);
    } catch {
      // The visible result remains usable even if storage is temporarily unavailable.
    }
  }

  startButton.addEventListener("click", () => void start());
  stopButton.addEventListener("click", () => session?.finishNow());
  retryButton.addEventListener("click", () => void start());

  updateTempoLabel();
  updateModeControls();
  void loadLesson();
}

if (document.getElementById("practice-workspace")) {
  initPracticeWorkspace();
}
