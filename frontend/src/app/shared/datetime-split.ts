// Plain date (no time) fields — e.g. Person.birthday — go through mat-datepicker, which binds a
// JS `Date`. Sending that `Date` straight to `HttpClient` serializes via `Date.toJSON()`
// (`toISOString()`, UTC) — for any negative UTC-offset timezone a midnight-local date rolls back
// to the previous day on save. This formats the picked `Date` back to the "yyyy-MM-dd" string the
// backend's `DateTime?` fields actually expect, using local getters (no UTC conversion).
export function toYmd(date: Date | null | undefined): string | null {
  if (!date) return null;

  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}
