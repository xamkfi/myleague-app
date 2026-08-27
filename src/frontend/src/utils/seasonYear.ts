/**
 * Builds a season-year label from start/end dates.
 * Same calendar year → "2024"; spanning years → "2024-2025".
 */
export function seasonYearFromDates(startDate: string, endDate: string): string {
  const startYear = new Date(startDate).getUTCFullYear();
  const endYear = new Date(endDate).getUTCFullYear();

  if (Number.isNaN(startYear) || Number.isNaN(endYear)) {
    return '';
  }

  return startYear === endYear ? String(startYear) : `${startYear}-${endYear}`;
}
