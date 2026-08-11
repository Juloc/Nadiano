# Nadiano 1.0 release checklist

Stable software release: **1.0.4**.

`MASTER_PLAN.md` is the canonical product/roadmap source. This checklist records release evidence and manual gates only.

## Automated gates

- [x] German and Indonesian frontend build, lint and unit tests pass.
- [x] .NET format, build and all test projects pass in Release mode.
- [x] Bundled content validator passes.
- [x] Curriculum validation confirms at least 60 guided lessons, 120 technique/rhythm exercises, 80 reading configurations, 60 ear-training tasks, 24 original mini-pieces, 12 public-domain Nadiano editions, selected E1 content and the final beginner assessment.
- [x] Chromium first-run, fake MIDI adapter and practice paths pass.
- [x] WCAG-focused learner-page baseline passes for landmarks, headings, IDs, labels, image alternatives, named actions, tabindex and keyboard focus entry.
- [x] Container builds as non-root and reports ready.
- [x] Stable container passes the modest-hardware profile at 1 CPU and 512 MiB RAM, including startup and p95 route budgets.
- [x] Trivy reports no fixed Critical or High vulnerability.
- [x] Dependency inventory and third-party license report are generated for the stable release.
- [x] Upgrade rehearsal starts Beta-era data with the stable image and preserves the learner profile.
- [x] Cold restore copies the pre-upgrade backup into an empty volume and verifies the preserved profile.
- [x] Rollback restores the pre-upgrade backup with the matching older image and verifies the preserved profile.
- [x] Immutable `1.0.4` and commit-SHA image tags are pushed before the GitHub release is created.

Release evidence is attached to the `v1.0.4` GitHub release, including `performance-profile.json`, `upgrade-rehearsal.json`, `restore-rehearsal.json`, `rollback-rehearsal.json`, dependency inventories and the third-party license report.

## Manual gates that CI cannot truthfully replace

- [x] Real MIDI piano recognizes the complete keyboard.
- [x] Sustain, sostenuto and soft pedal are received; the UI displays all three separately.
- [ ] German first-run to first result checked in current Chrome with real browser permissions.
- [ ] Indonesian first-run to first result checked in current Edge with real browser permissions.
- [ ] PWA installation and MIDI reconnect checked on the production HTTPS deployment.
- [ ] Manual keyboard-only review of the core workflows in current supported desktop browsers.
- [ ] Human musical, pedagogical, German/Indonesian localization and licensing sign-off for the bundled 1.0 repertoire/content set.
- [ ] At least two weeks of household/invited-user daily use completed without manual database repair.

The stable workflow blocks publishing on every software-enforceable gate above. The remaining unchecked items require real household hardware, browser permission state or human review and must not be represented as automated passes.