interface GeneratedPracticeEvent {
  index: number;
  measure: number;
  onsetUnits: number;
  durationUnits: number;
  midiNote: number;
}

interface GeneratedPracticeCard {
  templateId: string;
  seed: number;
  kind: "reading" | "rhythm";
  skillId: string;
  beatsPerMeasure: number;
  unitsPerBeat: number;
  events: GeneratedPracticeEvent[];
}

interface SessionPlanItem {
  activityId: string;
  skillId: string;
  reasonCode: string;
  seed?: number;
}

interface ReviewItem {
  skillId: string;
  sourceId: string;
  reasonCode: string;
  dueAtUtc: string;
}

function requireElement<T extends HTMLElement>(id: string): T {
  const element = document.getElementById(id);
  if (!element) {
    throw new Error(`Beta practice markup is missing #${id}`);
  }
  return element as T;
}

function randomSeed(): number {
  const values = new Uint32Array(1);
  crypto.getRandomValues(values);
  return (values[0] ?? Date.now()) & 0x7fffffff;
}

function midiName(note: number): string {
  const names = ["C", "C♯", "D", "D♯", "E", "F", "F♯", "G", "G♯", "A", "A♯", "B"];
  return `${names[note % 12] ?? "?"}${Math.floor(note / 12) - 1}`;
}

async function getJson<T>(url: string): Promise<T> {
  const response = await fetch(url, { headers: { Accept: "application/json" } });
  if (!response.ok) {
    throw new Error(`Request failed: ${response.status}`);
  }
  return await response.json() as T;
}

async function recordEvidence(
  activityId: string,
  activityKind: string,
  seed: number | undefined,
  skillId: string,
  expected: unknown,
  response: unknown,
  result: unknown,
  outcome: "Excellent" | "Good" | "NeedsWork" | "Failed",
): Promise<void> {
  await fetch("/api/beta/evidence", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ activityId, activityKind, seed, skillId, expected, response, result, outcome }),
  });
}

function initBetaPractice(): void {
  const loadPlanButton = requireElement<HTMLButtonElement>("beta-load-plan");
  const planList = requireElement<HTMLOListElement>("beta-plan");
  const readingNewButton = requireElement<HTMLButtonElement>("beta-reading-new");
  const readingSeed = requireElement<HTMLElement>("beta-reading-seed");
  const readingEvents = requireElement<HTMLOListElement>("beta-reading-events");
  const readingCorrectButton = requireElement<HTMLButtonElement>("beta-reading-correct");
  const readingRetryButton = requireElement<HTMLButtonElement>("beta-reading-retry");
  const rhythmNewButton = requireElement<HTMLButtonElement>("beta-rhythm-new");
  const rhythmPattern = requireElement<HTMLElement>("beta-rhythm-pattern");
  const rhythmStartButton = requireElement<HTMLButtonElement>("beta-rhythm-start");
  const rhythmTapButton = requireElement<HTMLButtonElement>("beta-rhythm-tap");
  const rhythmResult = requireElement<HTMLElement>("beta-rhythm-result");
  const earPlayButton = requireElement<HTMLButtonElement>("beta-ear-play");
  const earAnswers = requireElement<HTMLElement>("beta-ear-answers");
  const earResult = requireElement<HTMLElement>("beta-ear-result");
  const loadReviewsButton = requireElement<HTMLButtonElement>("beta-load-reviews");
  const reviewsList = requireElement<HTMLUListElement>("beta-reviews");

  let readingCard: GeneratedPracticeCard | undefined;
  let rhythmCard: GeneratedPracticeCard | undefined;
  let rhythmStartedAt = 0;
  let rhythmTaps: number[] = [];
  let rhythmTimer: number | undefined;
  let currentEar: { first: number; second: number; answer: string; seed: number } | undefined;

  async function loadPlan(): Promise<void> {
    planList.replaceChildren();
    const plan = await getJson<SessionPlanItem[]>("/api/beta/session-plan");
    for (const item of plan) {
      const row = document.createElement("li");
      row.textContent = `${item.skillId} — ${item.reasonCode}${item.seed === undefined ? "" : ` (seed ${item.seed})`}`;
      planList.appendChild(row);
    }
  }

  async function newReadingCard(): Promise<void> {
    const seed = randomSeed();
    readingCard = await getJson<GeneratedPracticeCard>(`/api/beta/cards/reading/${seed}`);
    readingEvents.replaceChildren();
    readingSeed.textContent = `Seed ${readingCard.seed}`;
    for (const event of readingCard.events) {
      const item = document.createElement("li");
      item.textContent = `M${event.measure} · ${midiName(event.midiNote)} · ${event.durationUnits}/${readingCard.unitsPerBeat}`;
      readingEvents.appendChild(item);
    }
    readingCorrectButton.disabled = false;
    readingRetryButton.disabled = false;
  }

  async function finishReading(success: boolean): Promise<void> {
    if (!readingCard) {
      return;
    }
    readingCorrectButton.disabled = true;
    readingRetryButton.disabled = true;
    await recordEvidence(
      `reading-${readingCard.seed}`,
      "reading-card",
      readingCard.seed,
      readingCard.skillId,
      readingCard.events,
      { selfReportedSuccess: success },
      { correct: success },
      success ? "Good" : "NeedsWork",
    );
    await loadReviews();
  }

  async function newRhythmCard(): Promise<void> {
    const seed = randomSeed();
    rhythmCard = await getJson<GeneratedPracticeCard>(`/api/beta/cards/rhythm/${seed}`);
    const totalUnits = rhythmCard.beatsPerMeasure * rhythmCard.unitsPerBeat;
    const symbols: string[] = [];
    for (let measure = 1; measure <= 4; measure++) {
      const starts = new Set(rhythmCard.events.filter((event) => event.measure === measure).map((event) => event.onsetUnits));
      symbols.push(Array.from({ length: totalUnits }, (_, unit) => starts.has(unit) ? "●" : "·").join(" "));
    }
    rhythmPattern.textContent = symbols.join("  |  ");
    rhythmStartButton.disabled = false;
    rhythmTapButton.disabled = true;
    rhythmResult.textContent = "";
  }

  async function startRhythm(): Promise<void> {
    if (!rhythmCard) {
      return;
    }
    rhythmTaps = [];
    rhythmStartButton.disabled = true;
    rhythmTapButton.disabled = true;
    rhythmResult.textContent = "4 · 3 · 2 · 1";
    await new Promise((resolve) => setTimeout(resolve, 2000));
    rhythmStartedAt = performance.now();
    rhythmTapButton.disabled = false;
    rhythmResult.textContent = "";

    const unitMs = 60000 / 80 / rhythmCard.unitsPerBeat;
    const finalEvent = rhythmCard.events.at(-1);
    const totalUnits = finalEvent
      ? ((finalEvent.measure - 1) * rhythmCard.beatsPerMeasure * rhythmCard.unitsPerBeat) + finalEvent.onsetUnits + finalEvent.durationUnits
      : 0;
    rhythmTimer = window.setTimeout(() => void finishRhythm(), totalUnits * unitMs + 600);
  }

  function tapRhythm(): void {
    if (rhythmTapButton.disabled || rhythmStartedAt === 0) {
      return;
    }
    rhythmTaps.push(performance.now() - rhythmStartedAt);
    rhythmTapButton.dataset.count = String(rhythmTaps.length);
  }

  async function finishRhythm(): Promise<void> {
    if (!rhythmCard || rhythmStartedAt === 0) {
      return;
    }
    if (rhythmTimer !== undefined) {
      clearTimeout(rhythmTimer);
      rhythmTimer = undefined;
    }
    rhythmTapButton.disabled = true;
    rhythmStartedAt = 0;

    const unitMs = 60000 / 80 / rhythmCard.unitsPerBeat;
    const expected = rhythmCard.events.map((event) =>
      (((event.measure - 1) * rhythmCard.beatsPerMeasure * rhythmCard.unitsPerBeat) + event.onsetUnits) * unitMs);
    const unused = [...rhythmTaps];
    const deviations: number[] = [];
    for (const onset of expected) {
      let bestIndex = -1;
      let bestDeviation = 160;
      unused.forEach((tap, index) => {
        const deviation = Math.abs(tap - onset);
        if (deviation < bestDeviation) {
          bestDeviation = deviation;
          bestIndex = index;
        }
      });
      if (bestIndex >= 0) {
        deviations.push(bestDeviation);
        unused.splice(bestIndex, 1);
      }
    }
    const missed = expected.length - deviations.length;
    const average = deviations.length === 0 ? 999 : deviations.reduce((sum, value) => sum + value, 0) / deviations.length;
    const passed = missed === 0 && unused.length <= 1 && average <= 95;
    rhythmResult.textContent = `Treffer ${deviations.length}/${expected.length} · extra ${unused.length} · Ø ${Math.round(average)} ms`;
    rhythmStartButton.disabled = false;

    await recordEvidence(
      `rhythm-${rhythmCard.seed}`,
      "rhythm-card",
      rhythmCard.seed,
      rhythmCard.skillId,
      expected,
      rhythmTaps,
      { matched: deviations.length, missed, extra: unused.length, averageDeviationMs: average, passed },
      passed ? "Good" : missed > 2 ? "Failed" : "NeedsWork",
    );
    await loadReviews();
  }

  async function playEarPrompt(): Promise<void> {
    const seed = randomSeed();
    const first = 57 + (seed % 10);
    const direction = seed % 3;
    const second = direction === 0 ? first : direction === 1 ? first + 3 : first - 3;
    const answer = direction === 0 ? "same" : direction === 1 ? "higher" : "lower";
    currentEar = { first, second, answer, seed };
    earResult.textContent = "";
    earAnswers.hidden = true;

    const context = new AudioContext();
    const playTone = (note: number, at: number): void => {
      const oscillator = context.createOscillator();
      const gain = context.createGain();
      oscillator.frequency.value = 440 * Math.pow(2, (note - 69) / 12);
      gain.gain.setValueAtTime(0.0001, at);
      gain.gain.exponentialRampToValueAtTime(0.16, at + 0.02);
      gain.gain.exponentialRampToValueAtTime(0.0001, at + 0.55);
      oscillator.connect(gain).connect(context.destination);
      oscillator.start(at);
      oscillator.stop(at + 0.6);
    };
    playTone(first, context.currentTime + 0.05);
    playTone(second, context.currentTime + 0.8);
    window.setTimeout(() => { earAnswers.hidden = false; }, 1500);
  }

  async function answerEar(answer: string): Promise<void> {
    if (!currentEar) {
      return;
    }
    const correct = answer === currentEar.answer;
    earResult.textContent = correct ? "✓" : "✗";
    earAnswers.hidden = true;
    await recordEvidence(
      `ear-direction-${currentEar.seed}`,
      "ear-direction",
      currentEar.seed,
      "ear.direction",
      { notes: [currentEar.first, currentEar.second], answer: currentEar.answer },
      { answer },
      { correct },
      correct ? "Good" : "NeedsWork",
    );
    await loadReviews();
  }

  async function loadReviews(): Promise<void> {
    reviewsList.replaceChildren();
    const reviews = await getJson<ReviewItem[]>("/api/beta/reviews");
    if (reviews.length === 0) {
      const item = document.createElement("li");
      item.textContent = "—";
      reviewsList.appendChild(item);
      return;
    }
    for (const review of reviews) {
      const item = document.createElement("li");
      item.textContent = `${review.skillId} — ${review.reasonCode}`;
      reviewsList.appendChild(item);
    }
  }

  loadPlanButton.addEventListener("click", () => void loadPlan());
  readingNewButton.addEventListener("click", () => void newReadingCard());
  readingCorrectButton.addEventListener("click", () => void finishReading(true));
  readingRetryButton.addEventListener("click", () => void finishReading(false));
  rhythmNewButton.addEventListener("click", () => void newRhythmCard());
  rhythmStartButton.addEventListener("click", () => void startRhythm());
  rhythmTapButton.addEventListener("click", tapRhythm);
  earPlayButton.addEventListener("click", () => void playEarPrompt());
  earAnswers.querySelectorAll<HTMLButtonElement>("button[data-answer]").forEach((button) => {
    button.addEventListener("click", () => void answerEar(button.dataset.answer ?? ""));
  });
  loadReviewsButton.addEventListener("click", () => void loadReviews());
  window.addEventListener("keydown", (event) => {
    if (event.code === "Space" && !rhythmTapButton.disabled) {
      event.preventDefault();
      tapRhythm();
    }
  });

  void loadPlan();
  void loadReviews();
}

if (document.getElementById("beta-load-plan")) {
  initBetaPractice();
}
