import type { AssessmentCategory } from "../scoring/computeScoringFacts";
import { computeScoringFacts } from "../scoring/computeScoringFacts";
import type { ScoringFacts } from "../scoring/facts";
import { matchEvents } from "../scoring/matcher";
import type { MatchedOutcome, MatchResult } from "../scoring/matcher";
import { nextActionLocalizationKey, recommendNextAction } from "../scoring/nextAction";
import type { NextActionCode } from "../scoring/nextAction";
import type { ResolvedExpectedEvent } from "../scoring/resolveExpectedEventTiming";
import type { ScoringPolicy } from "../scoring/ScoringPolicy";
import type { MidiAccessAdapter, Unsubscribe } from "../midi/MidiAccessAdapter";
import type { PlayedMidiEvent } from "../midi/types";

export type PracticeMode = "wait" | "loop" | "performance";

export interface PracticeSessionConfig {
  mode: PracticeMode;
  policy: ScoringPolicy;
  enabledCategories: readonly AssessmentCategory[];
  onTimeToleranceMs: number;
}

export interface PracticeSessionResult {
  facts: ScoringFacts;
  nextAction: NextActionCode;
  nextActionKey: string;
}

/**
 * Ties the matcher (WP-014) and scoring facts (WP-015) to a live MIDI stream
 * for one practice attempt. "wait" mode never times out — it waits
 * indefinitely for the correct pitch(es), matching
 * docs/PRODUCT_CONCEPT.md §4 ("Wait: progression pauses until the required
 * pitch or chord is played"). "loop" and "performance" share the same
 * timed-matching path; loop only differs in which expected events the
 * caller passes in (a measure range).
 */
export class PracticeSession {
  private unsubscribe: Unsubscribe | undefined;
  private completionTimer: ReturnType<typeof setTimeout> | undefined;
  private playedEvents: PlayedMidiEvent[] = [];
  private sessionStartAtMs = 0;
  private completed = false;

  // Wait-mode-only state.
  private waitGroupIndex = 0;
  private readonly waitSatisfiedEvents = new Map<number, PlayedMidiEvent>();

  onLiveUpdate?: (result: MatchResult) => void;
  onComplete?: (result: PracticeSessionResult) => void;

  constructor(
    private readonly midiAdapter: MidiAccessAdapter,
    private readonly resolvedExpected: readonly ResolvedExpectedEvent[],
    private readonly config: PracticeSessionConfig,
    private readonly now: () => number = () => performance.now(),
  ) {}

  start(): void {
    this.stop();
    this.playedEvents = [];
    this.sessionStartAtMs = this.now();
    this.waitGroupIndex = 0;
    this.waitSatisfiedEvents.clear();
    this.completed = false;

    this.unsubscribe = this.midiAdapter.onEvent((event) => this.handleEvent(event));

    if (this.config.mode !== "wait") {
      this.scheduleAutoCompletion();
    }
  }

  /** Unsubscribes from MIDI events and cancels any pending timer. Safe to call multiple times. */
  stop(): void {
    this.unsubscribe?.();
    this.unsubscribe = undefined;

    if (this.completionTimer !== undefined) {
      clearTimeout(this.completionTimer);
      this.completionTimer = undefined;
    }
  }

  /** Ends the attempt now and reports a result, even if not everything was played. */
  finishNow(): void {
    this.finish();
  }

  private scheduleAutoCompletion(): void {
    const last = this.resolvedExpected.at(-1);
    if (!last) {
      return;
    }

    const finishAtMs = last.onsetMs - this.sessionStartAtMs + last.durationMs + this.config.policy.matchWindowMs;
    this.completionTimer = setTimeout(() => this.finish(), Math.max(0, finishAtMs));
  }

  private handleEvent(event: PlayedMidiEvent): void {
    if (this.completed) {
      return;
    }

    this.playedEvents.push(event);

    if (this.config.mode === "wait") {
      this.handleWaitModeEvent(event);
    } else {
      this.onLiveUpdate?.(this.computeTimedMatchResult());
    }
  }

  private currentWaitGroup(): ResolvedExpectedEvent[] {
    const groupId = this.groupIdsInOrder()[this.waitGroupIndex];
    return groupId === undefined ? [] : this.resolvedExpected.filter((e) => e.groupId === groupId);
  }

  private groupIdsInOrder(): string[] {
    const seen = new Set<string>();
    const order: string[] = [];
    for (const event of this.resolvedExpected) {
      if (!seen.has(event.groupId)) {
        seen.add(event.groupId);
        order.push(event.groupId);
      }
    }
    return order;
  }

  private handleWaitModeEvent(event: PlayedMidiEvent): void {
    if (event.kind !== "noteOn" || event.note === undefined) {
      return;
    }

    const group = this.currentWaitGroup();
    const stillNeeded = group.some((slot) => slot.pitch === event.note && !this.waitSatisfiedEvents.has(slot.pitch));
    if (!stillNeeded) {
      return;
    }

    this.waitSatisfiedEvents.set(event.note, event);

    const groupSatisfied = group.every((slot) => this.waitSatisfiedEvents.has(slot.pitch));
    if (groupSatisfied) {
      this.waitGroupIndex += 1;
      this.waitSatisfiedEvents.clear();
      this.onLiveUpdate?.(this.computeFinalWaitMatchResult());

      if (this.waitGroupIndex >= this.groupIdsInOrder().length) {
        this.finish();
      }
    }
  }

  private computeTimedMatchResult(): MatchResult {
    return matchEvents(this.resolvedExpected, this.playedEvents, this.config.policy);
  }

  private finish(): void {
    if (this.completed) {
      return;
    }
    this.completed = true;
    this.stop();

    const matchResult = this.config.mode === "wait" ? this.computeFinalWaitMatchResult() : this.computeTimedMatchResult();
    const facts = computeScoringFacts(this.config.enabledCategories, matchResult, this.resolvedExpected, this.playedEvents, this.config.onTimeToleranceMs);
    const nextAction = recommendNextAction(facts);

    this.onComplete?.({ facts, nextAction, nextActionKey: nextActionLocalizationKey(nextAction) });
  }

  private computeFinalWaitMatchResult(): MatchResult {
    // Reconstructing from scratch here (rather than accumulating across handleWaitModeEvent
    // calls) keeps the "what counts as the final result" logic in one place.
    const matched: MatchedOutcome[] = [];
    const usedSequences = new Set<number>();

    for (const groupId of this.groupIdsInOrder()) {
      for (const slot of this.resolvedExpected.filter((e) => e.groupId === groupId)) {
        const satisfiedBy = this.playedEvents.find(
          (event) => event.kind === "noteOn" && event.note === slot.pitch && !usedSequences.has(event.sequence),
        );
        if (satisfiedBy) {
          usedSequences.add(satisfiedBy.sequence);
          matched.push({
            status: "matched",
            expectedGroupId: groupId,
            pitch: slot.pitch,
            expectedOnsetMs: slot.onsetMs,
            playedSequence: satisfiedBy.sequence,
            playedOnsetMs: satisfiedBy.receivedAtMs,
            onsetDeviationMs: satisfiedBy.receivedAtMs - slot.onsetMs,
          });
        }
      }
    }

    return { expected: matched, additions: [] };
  }
}
