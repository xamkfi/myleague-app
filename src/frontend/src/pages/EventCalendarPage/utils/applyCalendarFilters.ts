import type { CalendarEvent, CalendarFilters, CalendarSeasonOption } from '../../../types/calendar';

export function applyCalendarFilters(
  events: CalendarEvent[],
  filters: CalendarFilters,
  seasons: CalendarSeasonOption[],
): CalendarEvent[] {
  let result = events;

  if (filters.sports.length > 0) {
    result = result.filter((event) => filters.sports.includes(event.sport));
  }

  if (filters.statuses.length > 0) {
    result = result.filter((event) => event.status !== undefined && filters.statuses.includes(event.status));
  }

  if (filters.competitionId) {
    const seasonName = seasons.find((season) => season.id === filters.competitionId)?.name;
    if (seasonName) {
      result = result.filter((event) => event.subtitle === seasonName);
    }
  }

  if (filters.teamSearch.trim()) {
    const search = filters.teamSearch.trim().toLowerCase();
    result = result.filter((event) => event.title.toLowerCase().includes(search));
  }

  return result;
}

export function seasonsMatchingSportFilter(
  seasons: CalendarSeasonOption[],
  selectedSports: CalendarFilters['sports'],
): CalendarSeasonOption[] {
  if (selectedSports.length === 0) {
    return seasons;
  }
  return seasons.filter((season) => selectedSports.includes(season.sport));
}
