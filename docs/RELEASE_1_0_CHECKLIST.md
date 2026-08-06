# Nadiano 1.0 release checklist

## Automated gates

- [ ] German and Indonesian frontend build, lint and unit tests pass.
- [ ] .NET format, build and all test projects pass in Release mode.
- [ ] Bundled content validator passes.
- [ ] Curriculum validation confirms at least 80 lessons, 180 exercises, 12 listening tasks, 8 repertoire tasks, selected E1 content and stage assessments.
- [ ] Chromium first-run, MIDI adapter and practice paths pass.
- [ ] Container builds as non-root and reports ready.
- [ ] Trivy reports no fixed Critical or High vulnerability.
- [ ] Upgrade rehearsal starts the Beta database with the 1.0 image without data loss or migration failure.
- [ ] Immutable `1.0.0` and commit-SHA image tags are pushed before the GitHub release is created.

## Manual gates

- [x] Real MIDI piano recognizes the complete keyboard.
- [x] Sustain, sostenuto and soft pedal are received; the UI displays all three separately.
- [ ] German first-run to first result checked in current Chrome.
- [ ] Indonesian first-run to first result checked in current Edge.
- [ ] PWA installation and reconnect checked on the production HTTPS deployment.
- [ ] Cold backup restored into an empty data directory and verified.

The automated workflow blocks publishing on every automated gate. Manual hardware and deployment checks remain recorded here because GitHub Actions cannot reproduce the household piano, reverse proxy or browser permission state.
