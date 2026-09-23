/**
 * Shared calendar event type (sport-agnostic) for the event calendar page.
 * Allows multiple sports to be displayed in the same calendar later.
 */
export const CALENDAR_SPORTS = ['floorball', 'football', 'icehockey'] as const;

export type CalendarSport = (typeof CALENDAR_SPORTS)[number];

export interface CalendarEvent {
  id: string;
  date: string; // ISO date (YYYY-MM-DD)
  time?: string; // e.g. "20:00"
  title: string;
  subtitle?: string;
  link: string;
  sport: CalendarSport;
  status?: 'scheduled' | 'live' | 'completed' | 'cancelled';
  venue?: string;
  homeScore?: number;
  awayScore?: number;
}

export interface CalendarSeasonOption {
  id: string;
  name: string;
  sport: CalendarSport;
}

export interface CalendarFilters {
  sports: CalendarSport[];
  competitionId: string | null;
  teamSearch: string;
  selectedDay: number | null;
}

export function calendarSportLabelKey(sport: string): string {
  if (sport === 'icehockey') {
    return 'sports.iceHockey';
  }
  return `sports.${sport}`;
}

export const DEFAULT_CALENDAR_FILTERS: CalendarFilters = {
  sports: [],
  competitionId: null,
  teamSearch: '',
  selectedDay: null,
};
