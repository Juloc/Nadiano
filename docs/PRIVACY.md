# Privacy

Nadiano is designed for a private self-hosted deployment.

- No analytics, advertising, remote account service or product telemetry is included.
- Learner profiles, progress, MIDI-derived results and imported files remain in the configured local data directory.
- MIDI events are processed in the browser and only normalized practice results are stored.
- Diagnostic exports omit raw imported files and do not transmit data automatically.
- Profile exports are available only for the profile selected by the same-origin profile cookie.
- Private MusicXML/MXL files are not placed in the service-worker cache.

The operator controls network access, backups and retention. Deleting a profile removes its application records; deployment backups remain under the operator's control and must be deleted separately when required.
