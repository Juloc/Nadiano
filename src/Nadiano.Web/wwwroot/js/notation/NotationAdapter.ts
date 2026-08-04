import { OpenSheetMusicDisplay } from "opensheetmusicdisplay";

export type NotationLoadResult = { status: "loaded" } | { status: "error"; reason: string };

export interface MeasureRange {
  fromMeasure: number;
  toMeasure: number;
}

/**
 * Thin wrapper around OpenSheetMusicDisplay (docs/JUNIOR_IMPLEMENTATION_PLAN.md
 * WP-012). Consumer pages depend on this adapter, not on the OSMD API
 * directly, so lesson/practice logic stays independent of the renderer's
 * internals (docs/TECHNICAL_ARCHITECTURE.md §9).
 */
export class NotationAdapter {
  private readonly osmd: OpenSheetMusicDisplay;
  private loaded = false;

  constructor(container: HTMLElement) {
    this.osmd = new OpenSheetMusicDisplay(container, {
      autoResize: true,
      backend: "svg",
      drawingParameters: "compacttight",
    });
  }

  async loadAndRender(musicXml: string): Promise<NotationLoadResult> {
    try {
      await this.osmd.load(musicXml);
      this.osmd.render();
      this.loaded = true;
      return { status: "loaded" };
    } catch (error) {
      this.loaded = false;
      return { status: "error", reason: error instanceof Error ? error.message : String(error) };
    }
  }

  get isLoaded(): boolean {
    return this.loaded;
  }

  get measureCount(): number {
    return this.loaded ? this.osmd.Sheet.SourceMeasures.length : 0;
  }

  setZoom(zoom: number): void {
    this.osmd.Zoom = zoom;
    if (this.loaded) {
      this.osmd.render();
    }
  }

  showCursor(): void {
    this.osmd.cursor.show();
  }

  hideCursor(): void {
    this.osmd.cursor.hide();
  }

  /** Moves the cursor to the start of the given 1-based measure number. No-op if out of range or nothing is loaded. */
  moveCursorToMeasure(measureNumber: number): void {
    if (!this.loaded) {
      return;
    }

    const targetIndex = measureNumber - 1;
    if (targetIndex < 0 || targetIndex >= this.measureCount) {
      return;
    }

    this.osmd.cursor.reset();
    while (this.osmd.cursor.iterator.CurrentMeasureIndex < targetIndex && !this.osmd.cursor.iterator.EndReached) {
      this.osmd.cursor.nextMeasure();
    }
    this.osmd.cursor.show();
  }

  /** A measure range is valid only when both bounds fall within the loaded sheet and from <= to. */
  isValidMeasureRange(range: MeasureRange): boolean {
    return (
      Number.isInteger(range.fromMeasure) &&
      Number.isInteger(range.toMeasure) &&
      range.fromMeasure >= 1 &&
      range.toMeasure >= range.fromMeasure &&
      range.toMeasure <= this.measureCount
    );
  }

  clear(): void {
    this.osmd.clear();
    this.loaded = false;
  }
}
