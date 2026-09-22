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

export interface SportYearOption {
  year: string;
  hasActiveSeason: boolean;
}

export function getSeasonStartYear(year: string): number | null {
  const match = year.match(/^(\d{4})/);
  return match ? Number(match[1]) : null;
}

export function isFutureUnpublishedYear(
  year: SportYearOption,
  now: Date = new Date(),
): boolean {
  const startYear = getSeasonStartYear(year.year);
  if (startYear === null) {
    return false;
  }
  return !year.hasActiveSeason && startYear > now.getFullYear();
}

export function filterPublicSportYears(
  years: SportYearOption[],
  now: Date = new Date(),
): SportYearOption[] {
  return years.filter((year) => !isFutureUnpublishedYear(year, now));
}

export function pickDefaultSportYear(
  years: SportYearOption[],
  urlYear: string | null,
): string {
  if (urlYear && years.some((year) => year.year === urlYear)) {
    return urlYear;
  }

  return years.find((year) => year.hasActiveSeason)?.year ?? years[0]?.year ?? '';
}
