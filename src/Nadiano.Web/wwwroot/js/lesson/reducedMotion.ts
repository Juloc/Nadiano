/**
 * Whether technique demonstration media should loop. Media never autoplays
 * (the markup omits the autoplay attribute), so this only controls repeat
 * behavior once a learner presses play — reduced motion always wins over
 * the content author's loop preference (docs/JUNIOR_IMPLEMENTATION_PLAN.md
 * WP-020 step 3).
 */
export function shouldLoopMedia(metadataLoop: boolean, prefersReducedMotion: boolean): boolean {
  return metadataLoop && !prefersReducedMotion;
}
