# First run and browser support

## First run

1. Open Nadiano in a current desktop browser.
2. Select German or Indonesian in the top navigation.
3. Open **Profiles**, rename the automatically created profile and create a second profile when another learner will use the deployment.
4. Open **MIDI setup** from the start page.
5. Confirm that secure context, MIDI and audio are available.
6. Connect the digital piano by USB, select the input and play several notes in the live diagnostics view.
7. Open **Learn** and begin with the first available F0 lesson.

Each household learner must select their own profile before practising. Progress, preferences, sessions and self-checks are stored per profile.

## Supported target

The Alpha target is the current stable desktop versions of Chrome and Edge with Web MIDI available. Nadiano does not identify browsers by name to decide support; the capability checks on the setup page are authoritative.

A secure context is required for Web MIDI. Use HTTPS through the reverse proxy for remote access. `http://localhost` is suitable for local development.

When Web MIDI is unavailable or permission is denied:

- text, media, listening and dry-task lessons remain usable;
- MIDI-scored practice cannot start;
- the setup page explains the missing capability without hiding the rest of the application.

Mobile browsers and non-Chromium desktop browsers are not part of the Alpha test matrix. They may still open the application, but MIDI practice is unsupported until the capability and real-device matrix is completed for them.

## Permission and reconnect problems

- Start MIDI permission only from the setup button; browsers may reject background requests.
- Reconnect the USB cable and reopen the setup page if the device disappears.
- Remove the stored preference when a replacement keyboard should become the default.
- Use the diagnostics export for capability and sanitized device information. It does not contain lesson prose or raw practice history.
