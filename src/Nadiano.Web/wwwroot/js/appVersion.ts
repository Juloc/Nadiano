/** Reads the app version from the <meta name="nadiano-version"> tag rendered by _Layout.cshtml. */
export function getAppVersion(): string {
  const meta = document.querySelector('meta[name="nadiano-version"]');
  return meta?.getAttribute("content") ?? "0.0.0-unknown";
}
