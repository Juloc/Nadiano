import type { NextActionCode } from "../scoring/nextAction";

export interface AttemptResponse {
  attemptId: string;
  completedAtUtc: string;
  resultSchemaVersion: number;
  resultJson: string;
  nextActionCode: string;
}

/** Thin wrapper around the WP-017 practice endpoints. Session/attempt ids are generated client-side for idempotent completion. */
export async function createSession(lessonId: string, contentVersion: string, mode: string): Promise<string> {
  const sessionId = crypto.randomUUID();
  const response = await fetch("/api/practice/sessions", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ sessionId, lessonId, contentVersion, mode }),
  });

  if (!response.ok) {
    throw new Error(`Failed to create practice session: ${response.status}`);
  }

  return sessionId;
}

export async function completeSession(
  sessionId: string,
  resultSchemaVersion: number,
  resultJson: string,
  nextActionCode: NextActionCode,
): Promise<AttemptResponse> {
  const attemptId = crypto.randomUUID();
  const response = await fetch(`/api/practice/sessions/${sessionId}/complete`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ attemptId, resultSchemaVersion, resultJson, nextActionCode }),
  });

  if (!response.ok) {
    throw new Error(`Failed to complete practice session: ${response.status}`);
  }

  return (await response.json()) as AttemptResponse;
}
