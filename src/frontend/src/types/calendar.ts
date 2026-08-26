/**
 * Shared calendar event type (sport-agnostic) for the event calendar page.
 * Allows multiple sports to be displayed in the same calendar later.
 */
export interface CalendarEvent {
  id: string;
  date: string; // ISO date (YYYY-MM-DD)
  time?: string; // e.g. "20:00"
  title: string;
  subtitle?: string;
  link: string;
  sport: string; // 'floorball' | 'icehockey' | ...
  status?: 'scheduled' | 'live' | 'completed' | 'cancelled';
  venue?: string;
  homeScore?: number;
  awayScore?: number;
}

export interface CalendarFilters {
  sports: string[];
  statuses: string[];
  competitionId: string | null;
  teamSearch: string;
  selectedDay: number | null;
}

export const CALENDAR_SPORTS = ['floorball', 'icehockey'] as const;

export const CALENDAR_STATUSES = ['scheduled', 'live', 'completed'] as const;

export const DEFAULT_CALENDAR_FILTERS: CalendarFilters = {
  sports: [],
  statuses: [],
  competitionId: null,
  teamSearch: '',
  selectedDay: null,
};
