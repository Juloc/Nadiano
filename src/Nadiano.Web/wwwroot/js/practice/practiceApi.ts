import { postOrQueue } from "../offline/requestQueue";
import type { NextActionCode } from "../scoring/nextAction";

export interface AttemptResponse {
  attemptId: string;
  completedAtUtc: string;
  resultSchemaVersion: number;
  resultJson: string;
  nextActionCode: string;
}

/** Thin wrapper around the practice endpoints. IDs are generated client-side so queued retries remain idempotent. */
export async function createSession(lessonId: string, contentVersion: string, mode: string): Promise<string> {
  const sessionId = crypto.randomUUID();
  const body = JSON.stringify({ sessionId, lessonId, contentVersion, mode });
  const response = await postOrQueue("/api/practice/sessions", body, `session:${sessionId}`);

  if (response && !response.ok && response.status !== 409) {
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
  const body = JSON.stringify({ attemptId, resultSchemaVersion, resultJson, nextActionCode });
  const response = await postOrQueue(
    `/api/practice/sessions/${sessionId}/complete`,
    body,
    `attempt:${attemptId}`,
  );

  if (!response) {
    return {
      attemptId,
      completedAtUtc: new Date().toISOString(),
      resultSchemaVersion,
      resultJson,
      nextActionCode,
    };
  }
  if (!response.ok && response.status !== 409) {
    throw new Error(`Failed to complete practice session: ${response.status}`);
  }
  if (response.status === 409) {
    return {
      attemptId,
      completedAtUtc: new Date().toISOString(),
      resultSchemaVersion,
      resultJson,
      nextActionCode,
    };
  }

  return (await response.json()) as AttemptResponse;
}
