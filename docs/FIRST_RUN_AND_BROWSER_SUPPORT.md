# First run and browser support

## First run

1. Open Nadiano in a current desktop browser.
2. Select German or Indonesian in the top navigation.
3. Open **Profiles**, rename the automatically created profile and create a second profile when another learner will use the deployment.
4. Open **MIDI setup** from the start page.
5. Confirm that secure context, MIDI and audio are available.
6. Connect the digital piano by USB, select the input and play several notes in the live diagnostics view.
7. Verify sustain, sostenuto and soft pedal when the piano provides them.
8. Open **Learn** and begin with the first available F0 lesson.

Each learner must select their own profile before practising. Progress, preferences, sessions, imports and review data are stored per profile.

## Supported target

Nadiano 1.0 supports current stable desktop Chrome and Edge releases where Web MIDI is available. Automated Chromium tests verify the common application path; real-device tests verify notes and the three standard piano pedals.

Nadiano uses capability checks rather than browser-name detection. The setup page is authoritative. A secure context is required for Web MIDI. Use HTTPS through the reverse proxy for remote access. `http://localhost` is suitable for local access.

When Web MIDI is unavailable or permission is denied:

- text, media, listening and dry-task lessons remain usable;
- generated reading and rhythm activities remain available where they do not require the piano;
- MIDI-scored practice cannot start;
- the setup page explains the missing capability without hiding the rest of the application.

Mobile browsers and non-Chromium desktop browsers can use progressively enhanced non-MIDI areas, but MIDI practice is not a supported 1.0 target on them.

## Permission and reconnect problems

- Start MIDI permission only from the setup button; browsers may reject background requests.
- Reconnect the USB cable and reopen the setup page if the device disappears.
- Remove the stored preference when a replacement keyboard should become the default.
- Use the diagnostics export for capability and sanitized device information. It does not contain lesson prose, imported scores or raw practice history.
