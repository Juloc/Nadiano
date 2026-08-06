# Known limitations — Beta

- Web MIDI requires a secure context and a supported Chromium-based desktop browser. Safari and Firefox do not provide the required Web MIDI workflow.
- PWA offline support covers the prepared public application shell and durable result delivery. It does not promise that every lesson or imported score can first be opened while offline.
- Private imported MusicXML/MXL files are intentionally excluded from shared browser caches.
- MusicXML import accepts `score-partwise` files. Unsupported notation is reported instead of guessed. Complex tuplets, grace notes, some multi-staff encodings and advanced articulations may remain warnings.
- Fingering contained in supported MusicXML is displayed where the renderer supports it. Beta does not provide unrestricted notation rewriting.
- Pedal diagnostics show sustain CC64, sostenuto CC66 and soft/una-corda CC67 separately. Advanced musical pedal scoring remains outside Beta.
- MIDI measures pitch, timing, duration, velocity and controller data. It cannot verify posture, tension, hand shape or movement quality; those remain explained self-checks or teacher-review items.
- Generated cards use reviewed deterministic templates. They do not generate arbitrary compositions or replace the authored course path.
- Local learner profiles are intended for a trusted household deployment. Public multi-user accounts and remote identity management are not included.
