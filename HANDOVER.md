# Handover — Alpha completion (WP-021 → WP-023)

Status as of this commit. Roadmap/plan docs: `docs/ROADMAP.md`, `docs/JUNIOR_IMPLEMENTATION_PLAN.md`, `docs/LEARNING_CURRICULUM.md`, `docs/CONTENT_MODEL.md`.

## Done and verified (build + test + browser check passed)

- WP-001–018 (previous session, commit `4aefa77`).
- **WP-019** course progression: lock/unlock via `LessonManifest.Prerequisites`, `CourseEnrollment`/`LessonProgress` tables, `/Learn` course map.
- **WP-020** technique lesson layout: `/Learn/{lessonId}` page (goal/why/demo/mistake/steps), dry-task self-report completion, `SkillEvidence` table + self-check endpoint, technique media serving (`/api/content/lessons/{id}/files/{path}`, whitelisted to manifest-declared paths only).

## In progress — WP-021 (produce alpha content)

**Content authoring is complete** (all 39 packages written under `content/courses/nadiano-foundations/lessons/`): 7 F0 lessons, 5 F1 core lessons, 20 exercises, 4 ear-training tasks, 3 mini-pieces. Plus `content/skills/skills.json` (24 skills) and `content/courses/nadiano-foundations/course.json`.

**NOT yet done — do this first:**

1. Generate `expected-events.json` for every MIDI-based lesson (the 20 exercises + 3 mini-pieces have `score.musicxml` but no `expected-events.json` yet — it's intentionally omitted, generate it, don't hand-write it):
   ```
   dotnet run --project tools/Nadiano.ContentValidator -- --generate content
   ```
2. Run the validator and fix whatever it reports (most likely failure mode: a hand-authored `score.musicxml` measure whose note/rest durations don't sum exactly to the time signature's beat count — the generator reports "Time signatures... not supported" or an unresolved-tie/unsupported-construct message per lesson):
   ```
   dotnet run --project tools/Nadiano.ContentValidator -- content
   ```
   Iterate until `Content validation passed.`
3. Add a matching Core/ContentTests fixture note if you change the validator itself — otherwise no test changes needed for content-only fixes.
4. Once content validates, start the app (`preview_start` with the `nadiano` launch config) and click through `/Learn` for real — this is the first time the course map and lesson pages render with real content instead of the empty-state placeholder. Check both cultures (`/culture/set`) render sensible German and Indonesian text with no missing `SelfCheck.*`/`course.stage.*` resource keys (search `src/Nadiano.Web/Infrastructure/Localization/SharedResource*.resx` for `SelfCheck.` if anything looks like a raw key instead of translated text).

**Known content-authoring context** (useful if the validator finds problems):
- MusicXML subset actually supported by `MusicXmlExpectedEventGenerator` (`src/Nadiano.Core/Content/MusicXmlExpectedEventGenerator.cs`): single part, `/4` time signatures only, no tuplets/grace notes/multi-staff, chords only via same-position `<chord/>`, ties via `<tie type="start|stop">`. All authored content uses only quarter/half/whole notes and rests, natural white-key pitches, `divisions=1` — deliberately avoided anything unsupported.
- Technique media for F0 lessons is original inline-authored SVG illustration (not filmed video — see the comment in `src/Nadiano.Core/Content/Manifests/TechniqueMediaMetadata.cs`), served as `image/svg+xml`. Ear-training lessons use synthesized sine-tone `.wav` reference audio (`audio/wav`) — see `ContentMediaEndpoints.ContentTypesByExtension` in `src/Nadiano.Web/Features/Content/ContentMediaEndpoints.cs` for the supported extension→content-type map if you add other formats.
- `Lesson.cshtml`/`Lesson.cshtml.cs` pick `<img>`/`<audio>`/`<video>` per media file extension (`MediaTagFor` in `LessonModel`).
- Skill self-check question text lives in `SharedResource*.resx` under keys `SelfCheck.{skillId}` (mirrors the `course.stage.{id}` pattern for stage titles) — every skill referenced in any lesson's `assessment.selfChecks` already has a DE+ID entry across all three resx files.

## Not started

- **WP-022** — progress/session summary page. `Pages/Progress/Index.cshtml` is still the original placeholder. Needs: recent practice list, competency distribution (only shown with enough attempts — don't imply reliability from tiny samples), lesson-completion/review-due indicators, no punitive streaks/global points, text equivalents for anything visual. Data sources already exist: `PracticeAttemptRecord`/`PracticeSessionRecord` (per-attempt facts), `LessonProgressRecord` (completions), `SkillEvidenceRecord` (self-checks) — no new persistence should be needed, this is a read/aggregate page.
- **WP-023** — alpha release hardening: first-run/browser-support docs, `/data` backup instructions, DB/content version diagnostics, run through `docs/ROADMAP.md` "Alpha exit criteria" and fix anything failing, tag/release notes, known limitations doc.
- Final full verification pass: `dotnet build`/`dotnet test` (whole solution), `npm run build`/`test`/`lint` in `src/Nadiano.Web`, then a **complete manual browser walkthrough in both German and Indonesian**: language → profile → MIDI setup (fake adapter is fine) → play through a real F0/F1 lesson end to end → see it reflected on the Progress page.
- Real-hardware verification (5 complete sessions on 2 browsers + 1 real digital piano — the last Alpha exit criterion in `docs/ROADMAP.md`) is explicitly the human user's task, not something either agent should attempt to simulate.

## Working conventions from this session (also saved to persistent memory, but repeating here for a fresh agent)

- **Never add AI attribution to commit messages** in this repo — no "Co-Authored-By: Claude" trailer, no mention of AI assistance anywhere in the message.
- Kill any running `Nadiano.Web` process before `dotnet build`/`dotnet test` (the exe locks the build output).
- SQLite can't `ORDER BY` on `DateTimeOffset` columns — sort client-side after `ToListAsync()`.
- Local dev server: `.claude/launch.json` has a `nadiano` config (`dotnet run --project src/Nadiano.Web --no-build --urls http://localhost:5299`) — use `preview_start` with `name: "nadiano"`.
- Only commit/push when the user explicitly asks.
