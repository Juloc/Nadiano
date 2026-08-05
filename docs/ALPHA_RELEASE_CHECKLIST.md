# Alpha release checklist — 0.1.0-alpha.1

## Automated evidence

- [x] bundled course quantity: 7 F0 lessons, selected F1 lessons, 20 exercises, 4 listening tasks and 3 original mini-pieces;
- [x] expected events generated from MusicXML rather than hand-written;
- [x] bundled package validation runs at startup and in CI;
- [x] German and Indonesian resource parity is tested;
- [x] profile-scoped persistence and cross-profile endpoint isolation are tested;
- [x] deterministic scoring unit tests cover wrong, missing, repeated and chord notes;
- [x] full solution build and tests run in Release configuration;
- [x] frontend build, lint and unit tests run in CI;
- [x] container image builds as a non-root image and is vulnerability-scanned;
- [x] version diagnostics expose application, latest database migration and content versions without private practice data;
- [x] cold backup and restore commands are documented;
- [x] release image is tagged with semantic version and immutable commit SHA.

## Manual browser walkthrough

Perform in both German and Indonesian:

- [ ] select/create the intended profile;
- [ ] verify setup capability messages;
- [ ] connect through the fake adapter or a real piano;
- [ ] complete one F0 dry-task lesson;
- [ ] complete one F1 MIDI lesson;
- [ ] confirm the lesson completion, recent attempt and recommendation on **Progress**;
- [ ] switch profile and confirm that progress is not mixed.

## Real-device exit criterion

The following evidence cannot be produced by CI and must be recorded by the household tester before declaring the Alpha hardware-verified:

- [ ] five complete sessions;
- [ ] at least two supported desktop browsers;
- [ ] at least one real USB-MIDI digital piano;
- [ ] piano model, operating system, browser versions and connection type recorded;
- [ ] no unresolved data-loss, connection-loss or wrong-pitch-scoring blocker.
