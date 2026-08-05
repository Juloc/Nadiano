import { Metronome } from "../audio/Metronome";
import { WebAudioClickSoundPlayer } from "../audio/WebAudioClickSoundPlayer";
import { WebAudioClock } from "../audio/WebAudioClock";
import { midiNoteName } from "../midi/noteNames";
import { WebMidiAccessAdapter } from "../midi/WebMidiAccessAdapter";
import { NotationAdapter } from "../notation/NotationAdapter";
import type { MatchResult } from "../scoring/matcher";
import { NORMAL_MODE_POLICY, PERFORMANCE_MODE_POLICY, WAIT_MODE_POLICY } from "../scoring/ScoringPolicy";
import { resolveExpectedEventTiming, type ResolvedExpectedEvent } from "../scoring/resolveExpectedEventTiming";
import type { AssessmentCategory } from "../scoring/computeScoringFacts";
import type { ExpectedEventDocument } from "../scoring/types";
import { PracticeSession, type PracticeMode, type PracticeSessionResult } from "../practice/PracticeSession";
import { completeSession, createSession } from "../practice/practiceApi";

const RESULT_SCHEMA_VERSION = 1;
const BEATS_PER_MEASURE = 4;

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
  const targetTempoBpm = Number(workspace.dataset.targetTempo) || 90;
  const countInMeasures = Number(workspace.dataset.countInMeasures) || 0;
  const enabledCategories = JSON.parse(workspace.dataset.assessmentCategories ?? "[]") as AssessmentCategory[];

  const targetTempoLabel = requireElement<HTMLElement>("workspace-target-tempo");
  const deviceStatus = requireElement<HTMLElement>("workspace-device-status");
  const connectButton = requireElement<HTMLButtonElement>("workspace-connect-button");
  const loadError = requireElement<HTMLElement>("workspace-load-error");
  const notationContainer = requireElement<HTMLElement>("workspace-notation");
  const zoomInput = requireElement<HTMLInputElement>("workspace-zoom");
  const modeSelect = requireElement<HTMLSelectElement>("workspace-mode");
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

  targetTempoLabel.textContent = String(targetTempoBpm);

  const midiAdapter = new WebMidiAccessAdapter();
  const notationAdapter = new NotationAdapter(notationContainer);
  let expectedDocument: ExpectedEventDocument | undefined;
  let audioContext: AudioContext | undefined;
  let metronome: Metronome | undefined;
  let session: PracticeSession | undefined;
  let currentResolvedExpected: ResolvedExpectedEvent[] = [];
  let currentSessionId: string | undefined;

  async function loadLesson(): Promise<void> {
    try {
      const [scoreResponse, eventsResponse] = await Promise.all([
        fetch(scoreUrl),
        fetch(expectedEventsUrl),
      ]);
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

  zoomInput.addEventListener("input", () => {
    notationAdapter.setZoom(Number(zoomInput.value) || 1);
  });

  modeSelect.addEventListener("change", () => {
    loopRangeFields.hidden = modeSelect.value !== "loop";
  });

  connectButton.addEventListener("click", () => {
    void connect();
  });

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
    if (modeSelect.value !== "loop") {
      return expectedDocument;
    }

    const from = Number(fromInput.value) || 1;
    const to = Number(toInput.value) || from;
    return { ...expectedDocument, events: expectedDocument.events.filter((e) => e.measure >= from && e.measure <= to) };
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
      const label =
        status === "correct" ? liveList.dataset.statusCorrect : status === "missed" ? liveList.dataset.statusMissed : liveList.dataset.statusPending;

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
      const onTimeCount = facts.onset.deviations.filter((d) => d.band === "onTime").length;
      addResultItem((resultSection.dataset.onsetTemplate ?? "").replace("{0}", String(onTimeCount)).replace("{1}", String(facts.onset.deviations.length)));
    }

    if (facts.duration && facts.duration.ratios.length > 0) {
      const average = facts.duration.ratios.reduce((sum, r) => sum + r.ratio, 0) / facts.duration.ratios.length;
      addResultItem((resultSection.dataset.durationTemplate ?? "").replace("{0}", average.toFixed(2)));
    }

    if (facts.steadiness && facts.steadiness.intervalVariability !== null) {
      addResultItem((resultSection.dataset.steadinessTemplate ?? "").replace("{0}", facts.steadiness.intervalVariability.toFixed(2)));
    }

    if (facts.dynamics && facts.dynamics.minVelocity !== null) {
      addResultItem(
        (resultSection.dataset.dynamicsTemplate ?? "")
          .replace("{0}", String(facts.dynamics.minVelocity))
          .replace("{1}", String(facts.dynamics.maxVelocity))
          .replace("{2}", String(Math.round(facts.dynamics.averageVelocity ?? 0))),
      );
    }

    const nextActionText: Record<string, string | undefined> = {
      "hands-separate": resultSection.dataset.nextActionHandsSeparate,
      "repeat-slower": resultSection.dataset.nextActionRepeatSlower,
      "repeat-section": resultSection.dataset.nextActionRepeatSection,
      "well-done": resultSection.dataset.nextActionWellDone,
    };
    nextActionLabel.textContent = nextActionText[result.nextAction] ?? "";

    startButton.disabled = false;
    stopButton.disabled = true;
  }

  async function ensureMetronome(): Promise<Metronome> {
    if (!metronome) {
      audioContext = new AudioContext();
      metronome = new Metronome(new WebAudioClock(audioContext), new WebAudioClickSoundPlayer(audioContext));
    }
    return metronome;
  }

  async function playCountIn(): Promise<void> {
    const activeMetronome = await ensureMetronome();
    await activeMetronome.start({ bpm: targetTempoBpm, beatsPerMeasure: BEATS_PER_MEASURE });
    const countInDurationMs = (60000 / targetTempoBpm) * BEATS_PER_MEASURE * countInMeasures;
    await new Promise((resolve) => setTimeout(resolve, countInDurationMs));
    activeMetronome.stop();
  }

  async function start(): Promise<void> {
    const events = eventsForCurrentMode();
    if (!events) {
      return;
    }

    resultSection.hidden = true;
    startButton.disabled = true;
    stopButton.disabled = false;

    await playCountIn();

    const mode = modeSelect.value as PracticeMode;

    try {
      currentSessionId = await createSession(lessonId, contentVersion, mode);
    } catch {
      currentSessionId = undefined; // Persistence is best-effort here — a failed create must not block practising.
    }

    const policy = policyForMode(mode);
    const sessionStartAtMs = performance.now();
    currentResolvedExpected = resolveExpectedEventTiming(events, BEATS_PER_MEASURE, sessionStartAtMs);

    liveSection.hidden = false;
    renderLiveList(currentResolvedExpected, undefined);

    session = new PracticeSession(midiAdapter, currentResolvedExpected, {
      mode,
      policy,
      enabledCategories,
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
      // Best-effort: the learner already sees the result on screen even if saving it failed.
    }
  }

  startButton.addEventListener("click", () => {
    void start();
  });

  stopButton.addEventListener("click", () => {
    session?.finishNow();
  });

  retryButton.addEventListener("click", () => {
    void start();
  });

  void loadLesson();
}

if (document.getElementById("practice-workspace")) {
  initPracticeWorkspace();
}
