import { completeSession, createSession } from "../practice/practiceApi";
import { shouldLoopMedia } from "../lesson/reducedMotion";
import { recordSelfCheck } from "../progress/selfCheckApi";

const RESULT_SCHEMA_VERSION = 1;

function requireElement<T extends HTMLElement>(id: string): T {
  const element = document.getElementById(id);
  if (!element) {
    throw new Error(`Lesson page markup is missing #${id}`);
  }
  return element as T;
}

function applyReducedMotion(): void {
  const mediaList = document.querySelector<HTMLElement>(".technique-media-list");
  if (!mediaList) {
    return;
  }

  const metadataLoop = mediaList.dataset.loop === "true";
  const prefersReducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  const loop = shouldLoopMedia(metadataLoop, prefersReducedMotion);

  for (const media of mediaList.querySelectorAll<HTMLMediaElement>("video.technique-media, audio.technique-media")) {
    media.loop = loop;
  }
}

function revealSelfChecks(): void {
  const section = document.getElementById("lesson-selfcheck-section");
  if (section) {
    section.hidden = false;
  }
}

/** Only present for lessons with no MIDI/notation content — see docs/JUNIOR_IMPLEMENTATION_PLAN.md WP-020 step 4. */
function initDryTask(): void {
  const section = document.getElementById("lesson-dry-task");
  if (!section) {
    return;
  }

  const lessonId = section.dataset.lessonId ?? "";
  const contentVersion = section.dataset.contentVersion ?? "";
  const button = requireElement<HTMLButtonElement>("lesson-dry-task-button");
  const status = requireElement<HTMLElement>("lesson-dry-task-status");

  button.addEventListener("click", () => {
    void completeDryTask(lessonId, contentVersion, button, status);
  });
}

async function completeDryTask(lessonId: string, contentVersion: string, button: HTMLButtonElement, status: HTMLElement): Promise<void> {
  button.disabled = true;
  try {
    const sessionId = await createSession(lessonId, contentVersion, "dry");
    await completeSession(sessionId, RESULT_SCHEMA_VERSION, "{}", "well-done");
    status.hidden = false;
    button.hidden = true;
    revealSelfChecks();
  } catch {
    button.disabled = false;
  }
}

function initSelfChecks(): void {
  const list = document.getElementById("lesson-selfcheck-list");
  if (!list) {
    return;
  }

  const lessonId = list.dataset.lessonId ?? "";

  for (const item of list.querySelectorAll<HTMLElement>(".self-check-item")) {
    const skillId = item.dataset.skillId ?? "";
    const answeredLabel = item.querySelector<HTMLElement>(".self-check-answered");
    const buttons = item.querySelectorAll<HTMLButtonElement>(".self-check-answer");

    for (const button of buttons) {
      button.addEventListener("click", () => {
        void answerSelfCheck(lessonId, skillId, button.dataset.value === "true", buttons, answeredLabel);
      });
    }
  }
}

async function answerSelfCheck(
  lessonId: string,
  skillId: string,
  selfReportedSuccess: boolean,
  buttons: NodeListOf<HTMLButtonElement>,
  answeredLabel: HTMLElement | null,
): Promise<void> {
  try {
    await recordSelfCheck(lessonId, skillId, selfReportedSuccess);
    for (const button of buttons) {
      button.hidden = true;
    }
    if (answeredLabel) {
      answeredLabel.hidden = false;
    }
  } catch {
    // Best-effort — the learner can just try answering again.
  }
}

if (document.getElementById("lesson-cue")) {
  applyReducedMotion();
  initDryTask();
  initSelfChecks();
}
