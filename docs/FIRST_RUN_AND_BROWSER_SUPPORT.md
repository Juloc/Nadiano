# First run and browser support

Current stable baseline: **1.0.4**.

For product scope and the canonical user journey, see `MASTER_PLAN.md`.

## First run

1. Open Nadiano in a current supported desktop browser.
2. Select German or Indonesian from the secondary language/profile controls.
3. Create/select the learner profile that will own this progress.
4. Open **MIDI setup** from the secondary setup/status control.
5. Complete the progressive MIDI wizard:
   1. browser/security capability check;
   2. explicit MIDI permission action;
   3. select the digital piano input;
   4. play several distinct keys and test available pedals;
   5. confirm setup and continue.
6. Verify Sustain (CC64), Sostenuto (CC66) and Soft/una-corda (CC67) separately when the piano provides them.
7. Return to the learner flow and open **Today** or **Learn**.
8. Start the recommended available lesson/practice task.

Raw MIDI event diagnostics are intentionally secondary and are not required for a normal successful first run.

Each learner must use their own profile before practising. Progress, preferences, sessions, imports and review data are stored per profile.

## Supported target

Nadiano 1.0 supports current stable desktop Chrome and Edge releases where Web MIDI is available. Automated Chromium tests verify the common application path; real-device tests provide separate manual evidence for real notes and the three standard piano pedals.

Nadiano uses capability checks rather than browser-name detection. The setup page is authoritative.

A secure context is required for Web MIDI:

- use HTTPS through the reverse proxy for normal remote/production access;
- `http://localhost` remains suitable for local browser access where the browser permits the required APIs.

## When Web MIDI is unavailable

When Web MIDI is unavailable or permission is denied:

- text/media/listening/dry-task lessons remain usable;
- reading/rhythm activities remain available when they do not require MIDI input;
- MIDI-scored practice cannot start;
- setup shows the missing capability/recovery action without hiding the rest of the product.

Mobile browsers and non-Chromium desktop browsers may use progressively enhanced non-MIDI areas, but MIDI practice is not currently claimed as a supported 1.0 target on them unless the real capability/runtime check and release support matrix say otherwise.

## Connection and permission recovery

- MIDI permission is requested only from an explicit learner action.
- If a device disconnects during setup/practice, preserve the current state where possible and reconnect/reselect without requiring a full product restart.
- Remove/forget the stored device preference when another keyboard should become the default.
- Healthy MIDI state should remain compact; only actionable problems should become prominent.
- Use the secondary diagnostics view/export for troubleshooting capability and sanitized device information.
- Diagnostics must not expose lesson prose, private imported scores or raw private practice history by default.

## Manual release evidence

Automated fake-MIDI/Chromium checks do not replace:

- German real-permission Chrome first-run → result;
- Indonesian real-permission Edge first-run → result;
- production HTTPS PWA install + MIDI reconnect;
- real USB MIDI device verification.

Track those items in `RELEASE_1_0_CHECKLIST.md`.