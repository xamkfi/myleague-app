import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { floorballMatchService } from '../../api/floorball/floorballMatchService';
import { footballMatchService } from '../../api/football/footballMatchService';
import { hockeyMatchService } from '../../api/hockey/hockeyMatchService';
import LoadingSpinner from '../../components/LoadingSpinner/LoadingSpinner';
import UnderlineTabs, { type UnderlineTab } from '../../components/UnderlineTabs/UnderlineTabs';
import SportIcon from '../../components/SportIcon/SportIcon';
import { FloorballMatchStatus, type FloorballMatchDto } from '../../types/floorball/floorballTypes';
import { FootballMatchStatus, type FootballMatchDto } from '../../types/football/footballTypes';
import {
  isHockeyMatchFinished,
  isHockeyMatchLive,
  type HockeyMatchListDto,
} from '../../types/hockey/hockeyTypes';
import type { SportKind } from '../../utils/sportRoutes';
import OfficialPageTemplate from './components/OfficialPageTemplate';
import './OfficialMatchesPage.scss';

type OfficialMatchState = 'scheduled' | 'live' | 'completed';

interface OfficialMatchRow {
  id: string;
  homeTeamName: string | null;
  awayTeamName: string | null;
  scheduledAt: string;
  venue: string | null;
  competitionName: string | null;
  state: OfficialMatchState;
  homeScore: number;
  awayScore: number;
}

const SPORTS: SportKind[] = ['floorball', 'football', 'hockey'];
const SPORT_STORAGE_KEY: string = 'official-matches-sport';
const PAGE_SIZE: number = 100;
/** Safety cap so a misbehaving filter can never page forever. */
const MAX_PAGES: number = 10;

function readStoredSport(): SportKind {
  try {
    const stored: string | null = localStorage.getItem(SPORT_STORAGE_KEY);
    return SPORTS.find((sport) => sport === stored) ?? 'floorball';
  } catch {
    return 'floorball';
  }
}

function storeSport(sport: SportKind): void {
  try {
    localStorage.setItem(SPORT_STORAGE_KEY, sport);
  } catch {
    /* noop – localStorage may be unavailable */
  }
}

/** Today's date in the browser's time zone, formatted for `<input type="date">`. */
function todayLocalIsoDate(): string {
  const now: Date = new Date();
  const month: string = String(now.getMonth() + 1).padStart(2, '0');
  const day: string = String(now.getDate()).padStart(2, '0');
  return `${now.getFullYear()}-${month}-${day}`;
}

/** Shifts a `YYYY-MM-DD` date by whole days, staying in local time. */
function shiftIsoDate(isoDate: string, days: number): string {
  const [year, month, day] = isoDate.split('-').map(Number);
  const shifted: Date = new Date(year, month - 1, day + days);
  const shiftedMonth: string = String(shifted.getMonth() + 1).padStart(2, '0');
  const shiftedDay: string = String(shifted.getDate()).padStart(2, '0');
  return `${shifted.getFullYear()}-${shiftedMonth}-${shiftedDay}`;
}

/** Start and end of a local calendar day as UTC ISO timestamps for the API filters. */
function localDayRange(isoDate: string): { start: Date; end: Date } {
  const [year, month, day] = isoDate.split('-').map(Number);
  const start: Date = new Date(year, month - 1, day, 0, 0, 0, 0);
  const end: Date = new Date(year, month - 1, day, 23, 59, 59, 999);
  return { start, end };
}

/** The local calendar day of an ISO timestamp, formatted for `<input type="date">`. */
function localIsoDateOf(dateTime: string): string | null {
  const value: Date = new Date(dateTime);
  if (Number.isNaN(value.getTime())) return null;
  const month: string = String(value.getMonth() + 1).padStart(2, '0');
  const day: string = String(value.getDate()).padStart(2, '0');
  return `${value.getFullYear()}-${month}-${day}`;
}

/** Day of the first scheduled match of the sport after the given day, or null when none is left. */
async function findNextMatchDay(sport: SportKind, afterIsoDate: string): Promise<string | null> {
  const startDate: string = localDayRange(shiftIsoDate(afterIsoDate, 1)).start.toISOString();
  let scheduledAt: string | undefined;
  if (sport === 'floorball') {
    const response = await floorballMatchService.getAll({
      startDate, status: FloorballMatchStatus.Scheduled, sortOrder: 'asc', page: 1, pageSize: 1,
    });
    scheduledAt = response.data?.[0]?.scheduledDateTime;
  } else if (sport === 'football') {
    const response = await footballMatchService.getAll({
      startDate, status: FootballMatchStatus.Scheduled, sortOrder: 'asc', page: 1, pageSize: 1,
    });
    scheduledAt = response.data?.[0]?.scheduledDateTime;
  } else {
    const response = await hockeyMatchService.getList({
      startDate, statuses: ['Scheduled'], sortOrder: 'asc', page: 1, pageSize: 1,
    });
    scheduledAt = response.data?.[0]?.scheduledStartTime;
  }
  return scheduledAt ? localIsoDateOf(scheduledAt) : null;
}

function isWithin(dateTime: string, start: Date, end: Date): boolean {
  const time: number = new Date(dateTime).getTime();
  return !Number.isNaN(time) && time >= start.getTime() && time <= end.getTime();
}

function floorballState(status: FloorballMatchStatus): OfficialMatchState | null {
  if (status === FloorballMatchStatus.Scheduled) return 'scheduled';
  if (status === FloorballMatchStatus.InProgress) return 'live';
  if (status === FloorballMatchStatus.Completed) return 'completed';
  return null;
}

function footballState(status: FootballMatchStatus): OfficialMatchState | null {
  if (status === FootballMatchStatus.Scheduled) return 'scheduled';
  if (status === FootballMatchStatus.InProgress) return 'live';
  if (status === FootballMatchStatus.Completed) return 'completed';
  return null;
}

function hockeyState(status: string): OfficialMatchState | null {
  if (status === 'Scheduled') return 'scheduled';
  if (isHockeyMatchLive(status)) return 'live';
  if (isHockeyMatchFinished(status)) return 'completed';
  return null;
}

function fromFloorball(match: FloorballMatchDto): OfficialMatchRow | null {
  const state: OfficialMatchState | null = floorballState(match.status);
  if (!state) return null;
  return {
    id: match.id,
    homeTeamName: match.homeTeamName,
    awayTeamName: match.awayTeamName,
    scheduledAt: match.scheduledDateTime,
    venue: match.venue ?? null,
    competitionName: match.competitionName ?? null,
    state,
    homeScore: match.homeScore ?? 0,
    awayScore: match.awayScore ?? 0,
  };
}

function fromFootball(match: FootballMatchDto): OfficialMatchRow | null {
  const state: OfficialMatchState | null = footballState(match.status);
  if (!state) return null;
  return {
    id: match.id,
    homeTeamName: match.homeTeamName,
    awayTeamName: match.awayTeamName,
    scheduledAt: match.scheduledDateTime,
    venue: match.venue ?? null,
    competitionName: match.competitionName ?? null,
    state,
    homeScore: match.homeScore ?? 0,
    awayScore: match.awayScore ?? 0,
  };
}

function fromHockey(match: HockeyMatchListDto): OfficialMatchRow | null {
  const state: OfficialMatchState | null = hockeyState(String(match.status));
  if (!state) return null;
  return {
    id: match.id,
    homeTeamName: match.homeTeamName,
    awayTeamName: match.awayTeamName,
    scheduledAt: match.scheduledStartTime,
    venue: match.venue,
    competitionName: match.competitionName,
    state,
    homeScore: match.homeScore ?? 0,
    awayScore: match.awayScore ?? 0,
  };
}

async function loadFloorballMatches(startDate: string, endDate: string): Promise<FloorballMatchDto[]> {
  const items: FloorballMatchDto[] = [];
  for (let page = 1; page <= MAX_PAGES; page += 1) {
    const response = await floorballMatchService.getAll({ startDate, endDate, sortOrder: 'asc', page, pageSize: PAGE_SIZE });
    items.push(...(response.data ?? []));
    if (page >= (response.pagination?.totalPages ?? 1)) break;
  }
  return items;
}

async function loadFootballMatches(startDate: string, endDate: string): Promise<FootballMatchDto[]> {
  const items: FootballMatchDto[] = [];
  for (let page = 1; page <= MAX_PAGES; page += 1) {
    const response = await footballMatchService.getAll({ startDate, endDate, sortOrder: 'asc', page, pageSize: PAGE_SIZE });
    items.push(...(response.data ?? []));
    if (page >= (response.pagination?.totalPages ?? 1)) break;
  }
  return items;
}

async function loadMatches(sport: SportKind, isoDate: string): Promise<OfficialMatchRow[]> {
  const { start, end } = localDayRange(isoDate);
  const startDate: string = start.toISOString();
  const endDate: string = end.toISOString();

  let rows: Array<OfficialMatchRow | null>;
  if (sport === 'floorball') {
    rows = (await loadFloorballMatches(startDate, endDate)).map(fromFloorball);
  } else if (sport === 'football') {
    rows = (await loadFootballMatches(startDate, endDate)).map(fromFootball);
  } else {
    rows = (await hockeyMatchService.getAllListPages({ startDate, endDate, sortOrder: 'asc' })).map(fromHockey);
  }

  // The list endpoints differ slightly in how they treat the range bounds, so filter again
  // on the local calendar day.
  return rows
    .filter((row): row is OfficialMatchRow => row !== null && isWithin(row.scheduledAt, start, end))
    .sort((a, b) => new Date(a.scheduledAt).getTime() - new Date(b.scheduledAt).getTime());
}

function ChevronIcon({ direction }: { direction: 'left' | 'right' }) {
  return (
    <svg
      className="official-matches__chevron"
      width="16"
      height="16"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2.5"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      {direction === 'left' ? <path d="m15 6-6 6 6 6" /> : <path d="m9 6 6 6-6 6" />}
    </svg>
  );
}

/** Landing page for match officials: one day's matches per sport with a link to the live desk. */
function OfficialMatchesPage() {
  const { t, i18n } = useTranslation();
  const [sport, setSport] = useState<SportKind>(readStoredSport);
  const [date, setDate] = useState<string>(todayLocalIsoDate);
  const [matches, setMatches] = useState<OfficialMatchRow[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [reloadToken, setReloadToken] = useState<number>(0);
  const [jumping, setJumping] = useState<boolean>(false);
  const [notice, setNotice] = useState<string | null>(null);

  useEffect(() => {
    setNotice(null);
  }, [sport, date]);

  useEffect(() => {
    let cancelled: boolean = false;
    setLoading(true);
    setError(null);
    loadMatches(sport, date)
      .then((rows) => {
        if (!cancelled) setMatches(rows);
      })
      .catch((err: unknown) => {
        if (cancelled) return;
        console.error('Failed to load matches for official view:', err);
        setMatches([]);
        setError(t('official.matches.loadError', 'Failed to load matches.'));
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => { cancelled = true; };
  }, [sport, date, reloadToken, t]);

  const handleSportChange = useCallback((id: string): void => {
    const next: SportKind | undefined = SPORTS.find((item) => item === id);
    if (!next) return;
    setSport(next);
    storeSport(next);
  }, []);

  const jumpToNextMatchDay = useCallback(async (): Promise<void> => {
    setJumping(true);
    setNotice(null);
    try {
      const nextDay: string | null = await findNextMatchDay(sport, date);
      if (nextDay) {
        setDate(nextDay);
      } else {
        setNotice(t('official.matches.noUpcoming', 'No upcoming matches after this day.'));
      }
    } catch (err: unknown) {
      console.error('Failed to find the next match day:', err);
      setNotice(t('official.matches.loadError', 'Failed to load matches.'));
    } finally {
      setJumping(false);
    }
  }, [sport, date, t]);

  const tabs: UnderlineTab[] = SPORTS.map((item) => ({
    id: item,
    label: t(`official.matches.sports.${item}`),
    icon: <SportIcon sport={item} size="sm" decorative />,
  }));

  const formatTime = (iso: string): string => {
    const value: Date = new Date(iso);
    if (Number.isNaN(value.getTime())) return '';
    return value.toLocaleTimeString(i18n.language, { hour: '2-digit', minute: '2-digit' });
  };

  const stateLabel = (state: OfficialMatchState): string => t(`official.matches.status.${state}`);
  const tbd: string = t('official.matches.tbd', 'TBD');
  const isToday: boolean = date === todayLocalIsoDate();

  return (
    <OfficialPageTemplate title={t('official.matches.title', 'Matches')}>
      <div className="official-matches">
        <UnderlineTabs
          tabs={tabs}
          activeId={sport}
          onChange={handleSportChange}
          ariaLabel={t('official.matches.sportTabs', 'Sport')}
        />

        <div className="official-matches__toolbar">
          <div className="official-matches__day-nav">
            <button
              type="button"
              className="official-matches__day-button"
              onClick={() => setDate((prev) => shiftIsoDate(prev, -1))}
              aria-label={t('official.matches.previousDay', 'Previous day')}
            >
              <ChevronIcon direction="left" />
            </button>
            <label className="official-matches__date">
              <span className="official-matches__date-label">{t('official.matches.date', 'Date')}</span>
              <input
                type="date"
                className="official-matches__date-input"
                value={date}
                onChange={(event) => { if (event.target.value) setDate(event.target.value); }}
              />
            </label>
            <button
              type="button"
              className="official-matches__day-button"
              onClick={() => setDate((prev) => shiftIsoDate(prev, 1))}
              aria-label={t('official.matches.nextDay', 'Next day')}
            >
              <ChevronIcon direction="right" />
            </button>
          </div>
          <div className="official-matches__quick-actions">
            {!isToday && (
              <button
                type="button"
                className="official-matches__today-button"
                onClick={() => setDate(todayLocalIsoDate())}
              >
                {t('official.matches.today', 'Today')}
              </button>
            )}
            <button
              type="button"
              className="official-matches__today-button official-matches__today-button--primary"
              onClick={() => { void jumpToNextMatchDay(); }}
              disabled={jumping}
            >
              {t('official.matches.nextMatchDay', 'Next match day')}
              <ChevronIcon direction="right" />
            </button>
            <button
              type="button"
              className="official-matches__today-button"
              onClick={() => setReloadToken((prev) => prev + 1)}
              disabled={loading}
            >
              {t('official.matches.refresh', 'Refresh')}
            </button>
          </div>
        </div>

        {notice && (
          <div className="official-matches__notice" role="status">{notice}</div>
        )}

        <div id={`tabpanel-${sport}`} role="tabpanel" aria-labelledby={`tab-${sport}`}>
          {loading ? (
            <div className="official-matches__state">
              <LoadingSpinner text={t('official.matches.loading', 'Loading matches...')} />
            </div>
          ) : error ? (
            <div className="official-matches__error" role="alert">{error}</div>
          ) : matches.length === 0 ? (
            <div className="official-matches__state">{t('official.matches.empty', 'No matches on this day.')}</div>
          ) : (
            <ul className="official-matches__list">
              {matches.map((match) => (
                <li key={match.id} className={`official-match official-match--${match.state}`}>
                  <div className="official-match__time">{formatTime(match.scheduledAt)}</div>
                  <div className="official-match__main">
                    <div className="official-match__teams">
                      <span className="official-match__team">{match.homeTeamName ?? tbd}</span>
                      <span className="official-match__score">
                        {match.state === 'scheduled' ? '–' : `${match.homeScore} – ${match.awayScore}`}
                      </span>
                      <span className="official-match__team">{match.awayTeamName ?? tbd}</span>
                    </div>
                    <div className="official-match__meta">
                      {match.competitionName && <span>{match.competitionName}</span>}
                      {match.venue && <span>{match.venue}</span>}
                    </div>
                  </div>
                  <span className={`official-match__status official-match__status--${match.state}`}>
                    {stateLabel(match.state)}
                  </span>
                  <Link className="official-match__manage" to={`/official/${sport}/matches/${match.id}`}>
                    {t('official.matches.manage', 'Manage')}
                  </Link>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </OfficialPageTemplate>
  );
}

export default OfficialMatchesPage;
