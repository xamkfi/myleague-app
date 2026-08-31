/**
 * Builds a season-year label from start/end dates.
 * Same calendar year → "2024"; spanning years → "2024-2025".
 */
export function seasonYearFromDates(startDate: string, endDate: string): string {
  const startYear = new Date(startDate).getFullYear();
  const endYear = new Date(endDate).getFullYear();
  if (!Number.isFinite(startYear) || !Number.isFinite(endYear)) {
    return '';
  }
  return startYear === endYear ? String(startYear) : `${startYear}-${endYear}`;
}

export function formatSeasonYearLabel(year: string): string {
  return year.replace('-', '–');
}
