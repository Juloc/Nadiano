import { postOrQueue } from "../offline/requestQueue";

interface PracticeEvent {
  index: number;
  measure: number;
  onsetUnits: number;
  durationUnits: number;
  midiNote: number;
}

interface PracticeCard {
  seed: number;
  skillId: string;
  beatsPerMeasure: number;
  unitsPerBeat: number;
  events: PracticeEvent[];
}

interface PlanItem {
  skillId: string;
  reasonCode: string;
  seed?: number;
}

interface ReviewItem {
  skillId: string;
  reasonCode: string;
}

type Outcome = "Good" | "NeedsWork" | "Failed";

function element<T extends HTMLElement>(id: string): T {
  const found = document.getElementById(id);
  if (!found) {
    throw new Error(`Missing beta practice element: ${id}`);
  }
  return found as T;
}

function seed(): number {
  const values = new Uint32Array(1);
  crypto.getRandomValues(values);
  return (values[0] ?? Date.now()) & 0x7fffffff;
}

async function json<T>(url: string): Promise<T> {
  const response = await fetch(url, { headers: { Accept: "application/json" } });
  if (!response.ok) {
    throw new Error(`Request failed: ${response.status}`);
  }
  return await response.json() as T;
}

async function saveEvidence(
  activityId: string,
  activityKind: string,
  cardSeed: number,
  skillId: string,
  expected: unknown,
  answer: unknown,
  result: unknown,
  outcome: Outcome,
): Promise<void> {
  const response = await postOrQueue(
    "/api/beta/evidence",
    JSON.stringify({
      activityId,
      activityKind,
      seed: cardSeed,
      skillId,
      expected,
      response: answer,
      result,
      outcome,
    }),
    `beta-evidence:${activityId}`,
  );
  if (response && !response.ok && response.status !== 409) {
    throw new Error(`Evidence failed: ${response.status}`);
  }
}

function noteName(note: number): string {
  const names = ["C", "C♯", "D", "D♯", "E", "F", "F♯", "G", "G♯", "A", "A♯", "B"];
  return `${names[note % 12] ?? "?"}${Math.floor(note / 12) - 1}`;
}

function init(): void {
  const planList = element<HTMLOListElement>("beta-plan");
  const readingSeed = element<HTMLElement>("beta-reading-seed");
  const readingEvents = element<HTMLOListElement>("beta-reading-events");
  const readingCorrect = element<HTMLButtonElement>("beta-reading-correct");
  const readingRetry = element<HTMLButtonElement>("beta-reading-retry");
  const rhythmPattern = element<HTMLElement>("beta-rhythm-pattern");
  const rhythmStart = element<HTMLButtonElement>("beta-rhythm-start");
  const rhythmTap = element<HTMLButtonElement>("beta-rhythm-tap");
  const rhythmResult = element<HTMLElement>("beta-rhythm-result");
  const earAnswers = element<HTMLElement>("beta-ear-answers");
  const earResult = element<HTMLElement>("beta-ear-result");
  const reviewsList = element<HTMLUListElement>("beta-reviews");

  let readingCard: PracticeCard | undefined;
  let rhythmCard: PracticeCard | undefined;
  let rhythmStartedAt = 0;
  let taps: number[] = [];
  let timer: number | undefined;
  let earPrompt: { first: number; second: number; answer: string; seed: number } | undefined;

  async function loadPlan(): Promise<void> {
    const items = await json<PlanItem[]>("/api/beta/session-plan");
    planList.replaceChildren(...items.map((item) => {
      const row = document.createElement("li");
      row.textContent = `${item.skillId} — ${item.reasonCode}${item.seed === undefined ? "" : ` (seed ${item.seed})`}`;
      return row;
    }));
  }

  async function loadReviews(): Promise<void> {
    const items = await json<ReviewItem[]>("/api/beta/reviews");
    const rows = items.map((item) => {
      const row = document.createElement("li");
      row.textContent = `${item.skillId} — ${item.reasonCode}`;
      return row;
    });
    if (rows.length === 0) {
      const empty = document.createElement("li");
      empty.textContent = "—";
      rows.push(empty);
    }
    reviewsList.replaceChildren(...rows);
  }

  async function newReading(): Promise<void> {
    const card = await json<PracticeCard>(`/api/beta/cards/reading/${seed()}`);
    readingCard = card;
    readingSeed.textContent = `Seed ${card.seed}`;
    readingEvents.replaceChildren(...card.events.map((event) => {
      const row = document.createElement("li");
      row.textContent = `M${event.measure} · ${noteName(event.midiNote)} · ${event.durationUnits}/${card.unitsPerBeat}`;
      return row;
    }));
    readingCorrect.disabled = false;
    readingRetry.disabled = false;
  }

  async function finishReading(success: boolean): Promise<void> {
    const card = readingCard;
    if (!card) {
      return;
    }
    readingCorrect.disabled = true;
    readingRetry.disabled = true;
    await saveEvidence(
      `reading-${card.seed}`,
      "reading-card",
      card.seed,
      card.skillId,
      card.events,
      { selfReportedSuccess: success },
      { correct: success },
      success ? "Good" : "NeedsWork",
    );
    await loadReviews();
  }

  async function newRhythm(): Promise<void> {
    const card = await json<PracticeCard>(`/api/beta/cards/rhythm/${seed()}`);
    rhythmCard = card;
    const unitsPerMeasure = card.beatsPerMeasure * card.unitsPerBeat;
    const measures: string[] = [];
    for (let measure = 1; measure <= 4; measure += 1) {
      const onsets = new Set(card.events.filter((event) => event.measure === measure).map((event) => event.onsetUnits));
      measures.push(Array.from({ length: unitsPerMeasure }, (_, unit) => onsets.has(unit) ? "●" : "·").join(" "));
    }
    rhythmPattern.textContent = measures.join("  |  ");
    rhythmStart.disabled = false;
    rhythmTap.disabled = true;
    rhythmResult.textContent = "";
  }

  async function startRhythm(): Promise<void> {
    const card = rhythmCard;
    if (!card) {
      return;
    }
    taps = [];
    rhythmStart.disabled = true;
    rhythmTap.disabled = true;
    rhythmResult.textContent = "4 · 3 · 2 · 1";
    await new Promise((resolve) => setTimeout(resolve, 2000));
    rhythmStartedAt = performance.now();
    rhythmTap.disabled = false;
    rhythmResult.textContent = "";

    const last = card.events.at(-1);
    const unitMs = 60000 / 80 / card.unitsPerBeat;
    const totalUnits = last
      ? (last.measure - 1) * card.beatsPerMeasure * card.unitsPerBeat + last.onsetUnits + last.durationUnits
      : 0;
    timer = window.setTimeout(() => void finishRhythm(card), totalUnits * unitMs + 600);
  }

  function tap(): void {
    if (!rhythmTap.disabled && rhythmStartedAt > 0) {
      taps.push(performance.now() - rhythmStartedAt);
    }
  }

  async function finishRhythm(card: PracticeCard): Promise<void> {
    if (timer !== undefined) {
      window.clearTimeout(timer);
      timer = undefined;
    }
    rhythmTap.disabled = true;
    rhythmStartedAt = 0;

    const unitMs = 60000 / 80 / card.unitsPerBeat;
    const expected = card.events.map((event) =>
      ((event.measure - 1) * card.beatsPerMeasure * card.unitsPerBeat + event.onsetUnits) * unitMs);
    const remaining = [...taps];
    const deviations: number[] = [];
    for (const onset of expected) {
      let bestIndex = -1;
      let bestDeviation = 160;
      remaining.forEach((value, index) => {
        const deviation = Math.abs(value - onset);
        if (deviation < bestDeviation) {
          bestDeviation = deviation;
          bestIndex = index;
        }
      });
      if (bestIndex >= 0) {
        deviations.push(bestDeviation);
        remaining.splice(bestIndex, 1);
      }
    }

    const missed = expected.length - deviations.length;
    const average = deviations.length === 0
      ? 999
      : deviations.reduce((sum, value) => sum + value, 0) / deviations.length;
    const passed = missed === 0 && remaining.length <= 1 && average <= 95;
    rhythmResult.textContent = `Treffer ${deviations.length}/${expected.length} · extra ${remaining.length} · Ø ${Math.round(average)} ms`;
    rhythmStart.disabled = false;
    await saveEvidence(
      `rhythm-${card.seed}`,
      "rhythm-card",
      card.seed,
      card.skillId,
      expected,
      taps,
      { matched: deviations.length, missed, extra: remaining.length, averageDeviationMs: average, passed },
      passed ? "Good" : missed > 2 ? "Failed" : "NeedsWork",
    );
    await loadReviews();
  }

  async function playEar(): Promise<void> {
    const promptSeed = seed();
    const first = 57 + promptSeed % 10;
    const direction = promptSeed % 3;
    const second = direction === 0 ? first : direction === 1 ? first + 3 : first - 3;
    earPrompt = { first, second, answer: direction === 0 ? "same" : direction === 1 ? "higher" : "lower", seed: promptSeed };
    earAnswers.hidden = true;
    earResult.textContent = "";

    const context = new AudioContext();
    [first, second].forEach((note, index) => {
      const oscillator = context.createOscillator();
      const gain = context.createGain();
      const startsAt = context.currentTime + 0.05 + index * 0.75;
      oscillator.frequency.value = 440 * Math.pow(2, (note - 69) / 12);
      gain.gain.setValueAtTime(0.0001, startsAt);
      gain.gain.exponentialRampToValueAtTime(0.16, startsAt + 0.02);
      gain.gain.exponentialRampToValueAtTime(0.0001, startsAt + 0.55);
      oscillator.connect(gain).connect(context.destination);
      oscillator.start(startsAt);
      oscillator.stop(startsAt + 0.6);
    });
    window.setTimeout(() => { earAnswers.hidden = false; }, 1500);
  }

  async function answerEar(answer: string): Promise<void> {
    const prompt = earPrompt;
    if (!prompt) {
      return;
    }
    const correct = answer === prompt.answer;
    earAnswers.hidden = true;
    earResult.textContent = correct ? "✓" : "✗";
    await saveEvidence(
      `ear-direction-${prompt.seed}`,
      "ear-direction",
      prompt.seed,
      "ear.direction",
      { notes: [prompt.first, prompt.second], answer: prompt.answer },
      { answer },
      { correct },
      correct ? "Good" : "NeedsWork",
    );
    await loadReviews();
  }

  element<HTMLButtonElement>("beta-load-plan").addEventListener("click", () => void loadPlan());
  element<HTMLButtonElement>("beta-reading-new").addEventListener("click", () => void newReading());
  readingCorrect.addEventListener("click", () => void finishReading(true));
  readingRetry.addEventListener("click", () => void finishReading(false));
  element<HTMLButtonElement>("beta-rhythm-new").addEventListener("click", () => void newRhythm());
  rhythmStart.addEventListener("click", () => void startRhythm());
  rhythmTap.addEventListener("click", tap);
  element<HTMLButtonElement>("beta-ear-play").addEventListener("click", () => void playEar());
  earAnswers.querySelectorAll<HTMLButtonElement>("button[data-answer]").forEach((button) => {
    button.addEventListener("click", () => void answerEar(button.dataset.answer ?? ""));
  });
  element<HTMLButtonElement>("beta-load-reviews").addEventListener("click", () => void loadReviews());
  window.addEventListener("keydown", (event) => {
    if (event.code === "Space" && !rhythmTap.disabled) {
      event.preventDefault();
      tap();
    }
  });

  void loadPlan();
  void loadReviews();
}

if (document.getElementById("beta-load-plan")) {
  init();
}
