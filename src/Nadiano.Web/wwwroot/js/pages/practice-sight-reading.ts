const modeSelect = document.getElementById("workspace-mode") as HTMLSelectElement | null;
const startButton = document.getElementById("workspace-start-button") as HTMLButtonElement | null;
const modeHint = document.getElementById("workspace-mode-hint") as HTMLElement | null;

if (modeSelect && startButton && modeHint) {
  const indonesian = document.documentElement.lang.startsWith("id");
  let previewTimer: number | undefined;

  function hint(seconds?: number): void {
    if (modeSelect?.value !== "sight-reading") {
      return;
    }
    modeHint!.textContent = seconds === undefined
      ? (indonesian
          ? "Baca partitur terlebih dahulu. Saat mulai, tersedia 15 detik untuk melihat sebelum hitungan masuk."
          : "Lies die Noten zuerst. Nach Start hast du 15 Sekunden Blickzeit vor dem Einzähler.")
      : (indonesian
          ? `Waktu melihat: ${seconds} detik`
          : `Blickzeit: ${seconds} Sekunden`);
  }

  modeSelect.addEventListener("change", () => hint());
  startButton.addEventListener("click", (event) => {
    if (modeSelect.value !== "sight-reading" || startButton.dataset.sightReady === "1") {
      return;
    }

    event.preventDefault();
    event.stopImmediatePropagation();
    if (previewTimer !== undefined) {
      return;
    }

    startButton.disabled = true;
    let remaining = 15;
    hint(remaining);
    previewTimer = window.setInterval(() => {
      remaining -= 1;
      hint(Math.max(0, remaining));
      if (remaining > 0) {
        return;
      }

      window.clearInterval(previewTimer);
      previewTimer = undefined;
      startButton.disabled = false;
      startButton.dataset.sightReady = "1";
      startButton.click();
      delete startButton.dataset.sightReady;
      hint();
    }, 1000);
  }, { capture: true });

  hint();
}
