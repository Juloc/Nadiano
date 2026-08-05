/** Thin wrapper around the WP-020 self-check endpoint. The answer is stored as learner evidence, never scored. */
export async function recordSelfCheck(lessonId: string, skillId: string, selfReportedSuccess: boolean): Promise<void> {
  const response = await fetch("/api/progress/self-checks", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ lessonId, skillId, selfReportedSuccess }),
  });

  if (!response.ok) {
    throw new Error(`Failed to record self-check: ${response.status}`);
  }
}
