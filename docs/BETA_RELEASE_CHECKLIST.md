# Beta release checklist

## Automated gate

- [ ] `dotnet format Nadiano.slnx --verify-no-changes`
- [ ] Release build and all .NET tests pass.
- [ ] TypeScript build, lint, tests and high-severity audit pass.
- [ ] Bundled Alpha content and generated Beta catalogue validation pass.
- [ ] Browser test covers first run, daily plan, practice result, private import and profile isolation.
- [ ] Docker image builds and has no known fixed high/critical Trivy findings.
- [ ] Gitleaks scan passes.
- [ ] Upgrade test from the Alpha migration baseline preserves profiles and attempts.

## Functional gate

- [ ] Course map exposes 45 guided lessons and 100 exercises.
- [ ] Stage checks cannot be completed from repertoire-only or failed activity.
- [ ] Reading and rhythm cards repeat exactly for the same seed.
- [ ] Review items become due according to explicit interval rules.
- [ ] The daily plan explains every recommendation.
- [ ] MusicXML and MXL imports use the normal practice workspace.
- [ ] Invalid and unsupported imports produce different messages.
- [ ] Imported parts and voices can be assigned explicitly to left and right hand.
- [ ] Limited fingering overrides appear in the practice cues without changing the original file.
- [ ] Private library files never enter the public service-worker cache.
- [ ] Offline completion and evidence retry do not create duplicate records.
- [ ] German and Indonesian learner-facing Beta paths contain equivalent information.

## Manual browser and hardware gate

- [ ] Current stable Chrome on desktop.
- [ ] Current stable Edge on desktop.
- [ ] Install and launch as PWA.
- [ ] USB-MIDI connect, play all key ranges, disconnect and reconnect.
- [ ] Sustain CC64, sostenuto CC66 and soft/una-corda CC67 appear separately when supplied by the instrument.
- [ ] Wait, loop, hands-separate, rhythm, tempo-ladder, listen-and-copy and performance modes.
- [ ] Two profiles cannot see each other's imported files, review items or progress.
- [ ] Backup Alpha data, upgrade, practise, restore onto a clean deployment.

A checkbox may only be marked when its evidence is linked from the release issue or release notes.
