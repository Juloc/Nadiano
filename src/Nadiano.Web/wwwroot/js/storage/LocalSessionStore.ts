/** Consumer-facing contract for the active-session recovery buffer (docs/TECHNICAL_ARCHITECTURE.md §12). */
export interface LocalSessionStore {
  get<T>(key: string): Promise<T | undefined>;
  set<T>(key: string, value: T): Promise<void>;
  remove(key: string): Promise<void>;
}
