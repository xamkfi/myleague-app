import { useState, useEffect, useCallback, useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import { floorballMatchService } from '../../api/floorball/floorballMatchService';
import { floorballSeasonService } from '../../api/floorball/floorballSeasonService';
import { loadAllHockeyMatches } from '../../api/hockey/loadAllHockeyMatches';
import { hockeySeasonService } from '../../api/hockey/hockeySeasonService';
import { loadTeamNameMap } from '../../utils/hockeyLookups';
import { mapHockeyMatchToCalendarEvent } from './utils/mapHockeyToCalendarEvent';
import type { CalendarEvent, CalendarFilters as FiltersType } from '../../types/calendar';
import { DEFAULT_CALENDAR_FILTERS } from '../../types/calendar';
import { mapFloorballMatchToCalendarEvent } from './utils/mapFloorballToCalendarEvent';
import MiniCalendar from './components/MiniCalendar';
import EventAgendaList from './components/EventAgendaList';
import CalendarFilters from './components/CalendarFilters';
import LoadingSpinner from '../../components/LoadingSpinner/LoadingSpinner';
import { useAudience } from '../../context/AudienceContext';
import './EventCalendarPage.scss';

interface SeasonOption {
  id: string;
  name: string;
}

function getMonthBounds(year: number, month: number): { startDate: string; endDate: string } {
  const start = new Date(year, month - 1, 1);
  const end = new Date(year, month, 0);
  return {
    startDate: start.toISOString().slice(0, 10),
    endDate: end.toISOString().slice(0, 10),
  };
}

function EventCalendarPage() {
  const { t } = useTranslation();
  const { audience } = useAudience();
  const now = new Date();
  const [year, setYear] = useState(now.getFullYear());
  const [month, setMonth] = useState(now.getMonth() + 1);
  const [allEvents, setAllEvents] = useState<CalendarEvent[]>([]);
  const [seasons, setSeasons] = useState<SeasonOption[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filters, setFilters] = useState<FiltersType>(DEFAULT_CALENDAR_FILTERS);

  const fetchSeasons = useCallback(async () => {
    try {
      const [floorball, hockey] = await Promise.all([
        floorballSeasonService.getActive(),
        hockeySeasonService.getActive(audience.teamCategory).catch(() => []),
      ]);
      const floorballList = (floorball.data ?? []).filter(
        (season) => !season.teamCategory || season.teamCategory === audience.teamCategory,
      );
      const hockeyList = hockey;
      setSeasons([
        ...floorballList.map((season) => ({ id: season.id, name: season.name })),
        ...hockeyList.map((season) => ({ id: season.id, name: season.name })),
      ]);
    } catch {
      // Non-critical: seasons filter just won't show options
    }
  }, [audience.teamCategory]);

  const fetchEvents = useCallback(async (y: number, m: number) => {
    setIsLoading(true);
    setError(null);
    const { startDate, endDate } = getMonthBounds(y, m);
    try {
      const response = await floorballMatchService.getAll({
        startDate,
        endDate,
        pageSize: 100,
        sortOrder: 'asc',
        teamCategory: audience.teamCategory,
      });
      const list = response.data ?? [];
      const floorballEvents = list.map(mapFloorballMatchToCalendarEvent);
      const [hockeyMatches, teamNames, hockeySeasons] = await Promise.all([
        loadAllHockeyMatches(audience.teamCategory).catch(() => []),
        loadTeamNameMap(undefined, audience.teamCategory).catch(() => new Map<string, string>()),
        hockeySeasonService.getAll(audience.teamCategory).catch(() => []),
      ]);
      const seasonNames = new Map(hockeySeasons.map((season) => [season.id, season.name]));
      const monthStart = new Date(startDate);
      const monthEnd = new Date(endDate);
      monthEnd.setHours(23, 59, 59, 999);
      const hockeyEvents = hockeyMatches
        .filter((match) => {
          const kickoff = new Date(match.scheduledStartTime);
          return kickoff >= monthStart && kickoff <= monthEnd;
        })
        .map((match) => mapHockeyMatchToCalendarEvent(
          match,
          teamNames,
          match.competitionId ? seasonNames.get(match.competitionId) : undefined,
        ));
      setAllEvents([...floorballEvents, ...hockeyEvents]);
    } catch (err) {
      console.error('EventCalendarPage: fetch failed', err);
      setError(t('eventCalendarPage.error'));
    } finally {
      setIsLoading(false);
    }
  }, [t, audience.teamCategory]);

  useEffect(() => {
    fetchSeasons();
  }, [fetchSeasons]);

  useEffect(() => {
    fetchEvents(year, month);
  }, [year, month, fetchEvents]);

  useEffect(() => {
    setFilters((prev) => ({ ...prev, selectedDay: null }));
  }, [year, month]);

  const filteredEvents = useMemo(() => {
    let result = allEvents;

    if (filters.statuses.length > 0) {
      result = result.filter((e) => e.status && filters.statuses.includes(e.status));
    }

    if (filters.competitionId) {
      result = result.filter((e) => e.subtitle === seasons.find((s) => s.id === filters.competitionId)?.name);
    }

    if (filters.teamSearch.trim()) {
      const search = filters.teamSearch.trim().toLowerCase();
      result = result.filter((e) => e.title.toLowerCase().includes(search));
    }

    return result;
  }, [allEvents, filters.statuses, filters.competitionId, filters.teamSearch, seasons]);

  const goPrevMonth = () => {
    if (month === 1) { setMonth(12); setYear((y) => y - 1); }
    else { setMonth((m) => m - 1); }
  };

  const goNextMonth = () => {
    if (month === 12) { setMonth(1); setYear((y) => y + 1); }
    else { setMonth((m) => m + 1); }
  };

  const goToday = () => {
    const today = new Date();
    setYear(today.getFullYear());
    setMonth(today.getMonth() + 1);
    setFilters((prev) => ({ ...prev, selectedDay: today.getDate() }));
  };

  const handleSelectDay = (day: number | null) => {
    setFilters((prev) => ({ ...prev, selectedDay: day }));
  };

  return (
    <PageTemplate title={t('eventCalendarPage.title')}>
      <div className="event-calendar-page">
        <div className="event-calendar-page__header">
          <h1 className="event-calendar-page__title">{t('eventCalendarPage.title')}</h1>
        </div>

        <div className="event-calendar-page__layout">
          <aside className="event-calendar-page__sidebar">
            <MiniCalendar
              year={year}
              month={month}
              events={filteredEvents}
              selectedDay={filters.selectedDay}
              onSelectDay={handleSelectDay}
              onPrevMonth={goPrevMonth}
              onNextMonth={goNextMonth}
              onToday={goToday}
            />
            <CalendarFilters
              filters={filters}
              onFiltersChange={setFilters}
              seasons={seasons}
            />
          </aside>

          <main className="event-calendar-page__content">
            {isLoading && (
              <div className="event-calendar-page__state">
                <LoadingSpinner size="sm" text={t('eventCalendarPage.loading')} />
              </div>
            )}

            {error && (
              <div className="event-calendar-page__state event-calendar-page__state--error">
                <p>{error}</p>
              </div>
            )}

            {!isLoading && !error && (
              <EventAgendaList
                events={filteredEvents}
                selectedDay={filters.selectedDay}
                year={year}
                month={month}
              />
            )}
          </main>
        </div>
      </div>
    </PageTemplate>
  );
}

export default EventCalendarPage;
