import { floorballMatchService } from '../../api/floorball/floorballMatchService';
import { footballMatchService } from '../../api/football/footballMatchService';
import { hockeyMatchService } from '../../api/hockey/hockeyMatchService';
import {
  FloorballMatchStatus,
  type FloorballMatchDto,
  type TeamCategory as FloorballTeamCategory,
} from '../../types/floorball/floorballTypes';
import {
  FootballMatchStatus,
  type FootballMatchDto,
  type TeamCategory as FootballTeamCategory,
} from '../../types/football/footballTypes';
import {
  FINISHED_HOCKEY_STATUSES,
  LIVE_HOCKEY_STATUSES,
  type HockeyMatchListDto,
  type HockeyMatchStatus,
} from '../../types/hockey/hockeyTypes';
import type { SportKind } from '../../utils/sportRoutes';

export type PanelSectionKind = 'live' | 'upcoming' | 'completed';

export type PanelMatchPhase = 'live' | 'scheduled' | 'completed';

export type SportFilter = 'all' | SportKind;

export interface PanelMatch {
  id: string;
  sport: SportKind;
  scheduledDateTime: string;
  phase: PanelMatchPhase;
  homeTeamName: string | null;
  awayTeamName: string | null;
  homeTeamLogo: string | null;
  awayTeamLogo: string | null;
  homeScore: number;
  awayScore: number;
  competitionName: string | null;
  venue: string | null;
}

export interface PanelSectionPage {
  matches: PanelMatch[];
  totalCount: number;
}

export const PANEL_SPORTS: readonly SportKind[] = ['floorball', 'football', 'hockey'];

const HOCKEY_UPCOMING_STATUSES: HockeyMatchStatus[] = ['Scheduled'];

export function sportsForFilter(filter: SportFilter): readonly SportKind[] {
  return filter === 'all' ? PANEL_SPORTS : [filter];
}

export function upcomingStartDate(): string {
  return new Date().toISOString();
}

function phaseFor(kind: PanelSectionKind): PanelMatchPhase {
  if (kind === 'live') {
    return 'live';
  }
  if (kind === 'upcoming') {
    return 'scheduled';
  }
  return 'completed';
}

function compareMatches(left: PanelMatch, right: PanelMatch, descending: boolean): number {
  const diff = new Date(left.scheduledDateTime).getTime() - new Date(right.scheduledDateTime).getTime();
  if (diff !== 0) {
    return descending ? -diff : diff;
  }
  return left.id.localeCompare(right.id);
}

function fromFloorball(match: FloorballMatchDto, phase: PanelMatchPhase): PanelMatch {
  return {
    id: match.id,
    sport: 'floorball',
    scheduledDateTime: match.scheduledDateTime,
    phase,
    homeTeamName: match.homeTeamName,
    awayTeamName: match.awayTeamName,
    homeTeamLogo: match.homeTeamLogo,
    awayTeamLogo: match.awayTeamLogo,
    homeScore: match.homeScore,
    awayScore: match.awayScore,
    competitionName: match.competitionName,
    venue: match.venue ?? null,
  };
}

function fromFootball(match: FootballMatchDto, phase: PanelMatchPhase): PanelMatch {
  return {
    id: match.id,
    sport: 'football',
    scheduledDateTime: match.scheduledDateTime,
    phase,
    homeTeamName: match.homeTeamName,
    awayTeamName: match.awayTeamName,
    homeTeamLogo: match.homeTeamLogo,
    awayTeamLogo: match.awayTeamLogo,
    homeScore: match.homeScore,
    awayScore: match.awayScore,
    competitionName: match.competitionName,
    venue: match.venue ?? null,
  };
}

function fromHockey(match: HockeyMatchListDto, phase: PanelMatchPhase): PanelMatch {
  return {
    id: match.id,
    sport: 'hockey',
    scheduledDateTime: match.scheduledStartTime,
    phase,
    homeTeamName: match.homeTeamName,
    awayTeamName: match.awayTeamName,
    homeTeamLogo: null,
    awayTeamLogo: null,
    homeScore: match.homeScore,
    awayScore: match.awayScore,
    competitionName: match.competitionName,
    venue: match.venue,
  };
}

function hasStatus(status: string, allowed: readonly string[]): boolean {
  const normalized = status.toLowerCase();
  return allowed.some((value) => value.toLowerCase() === normalized);
}

/**
 * Drops rows whose status is outside this section. The public hockey list only
 * recently learned to filter by status, and a finished game must not render as live.
 * When every row matches, the server total is kept. When some do not, that total
 * counts other statuses, so the section total becomes the rows we actually kept.
 */
function pageFromRows<T>(
  items: T[],
  reportedTotal: number,
  allowed: readonly string[],
  statusOf: (item: T) => string,
  map: (item: T) => PanelMatch,
): PanelSectionPage {
  const matching = items.filter((item) => hasStatus(statusOf(item), allowed));
  const serverHonoredFilter = matching.length === items.length;
  return {
    matches: matching.map(map),
    totalCount: serverHonoredFilter ? reportedTotal : matching.length,
  };
}

function floorballStatus(kind: PanelSectionKind): FloorballMatchStatus {
  if (kind === 'live') {
    return FloorballMatchStatus.InProgress;
  }
  if (kind === 'upcoming') {
    return FloorballMatchStatus.Scheduled;
  }
  return FloorballMatchStatus.Completed;
}

function footballStatus(kind: PanelSectionKind): FootballMatchStatus {
  if (kind === 'live') {
    return FootballMatchStatus.InProgress;
  }
  if (kind === 'upcoming') {
    return FootballMatchStatus.Scheduled;
  }
  return FootballMatchStatus.Completed;
}

async function fetchFloorball(
  kind: PanelSectionKind,
  pageSize: number,
  teamCategory: FloorballTeamCategory,
): Promise<PanelSectionPage> {
  const status = floorballStatus(kind);
  const response = await floorballMatchService.getAll({
    status,
    startDate: kind === 'upcoming' ? upcomingStartDate() : undefined,
    sortOrder: kind === 'completed' ? 'desc' : 'asc',
    pageSize,
    teamCategory,
  });
  const list = response.data ?? [];
  const phase = phaseFor(kind);
  return pageFromRows(
    list,
    response.pagination?.totalCount ?? list.length,
    [status],
    (match) => match.status,
    (match) => fromFloorball(match, phase),
  );
}

async function fetchFootball(
  kind: PanelSectionKind,
  pageSize: number,
  teamCategory: FloorballTeamCategory,
): Promise<PanelSectionPage> {
  const status = footballStatus(kind);
  const response = await footballMatchService.getAll({
    status,
    startDate: kind === 'upcoming' ? upcomingStartDate() : undefined,
    sortOrder: kind === 'completed' ? 'desc' : 'asc',
    pageSize,
    teamCategory: teamCategory as FootballTeamCategory,
  });
  const list = response.data ?? [];
  const phase = phaseFor(kind);
  return pageFromRows(
    list,
    response.pagination?.totalCount ?? list.length,
    [status],
    (match) => match.status,
    (match) => fromFootball(match, phase),
  );
}

function hockeyStatuses(kind: PanelSectionKind): HockeyMatchStatus[] {
  if (kind === 'live') {
    return LIVE_HOCKEY_STATUSES;
  }
  if (kind === 'upcoming') {
    return HOCKEY_UPCOMING_STATUSES;
  }
  return FINISHED_HOCKEY_STATUSES;
}

async function fetchHockey(
  kind: PanelSectionKind,
  pageSize: number,
  teamCategory: FloorballTeamCategory,
): Promise<PanelSectionPage> {
  const statuses = hockeyStatuses(kind);
  const response = await hockeyMatchService.getList({
    statuses,
    startDate: kind === 'upcoming' ? upcomingStartDate() : undefined,
    sortOrder: kind === 'completed' ? 'desc' : 'asc',
    pageSize,
    teamCategory,
  });
  const list = response.data ?? [];
  const phase = phaseFor(kind);
  return pageFromRows(
    list,
    response.pagination?.totalCount ?? list.length,
    statuses,
    (match) => String(match.status),
    (match) => fromHockey(match, phase),
  );
}

async function fetchSport(
  sport: SportKind,
  kind: PanelSectionKind,
  pageSize: number,
  teamCategory: FloorballTeamCategory,
): Promise<PanelSectionPage> {
  if (sport === 'floorball') {
    return fetchFloorball(kind, pageSize, teamCategory);
  }
  if (sport === 'football') {
    return fetchFootball(kind, pageSize, teamCategory);
  }
  return fetchHockey(kind, pageSize, teamCategory);
}

/**
 * Loads one sidebar section across the selected sports.
 * Each sport is asked for `pageSize` rows so the merged window is the true next matches
 * even when a single sport fills it. Live matches are returned in full (up to that page).
 * A sport that fails is skipped; the call rejects only when every sport fails.
 */
export async function loadPanelSection(
  sports: readonly SportKind[],
  kind: PanelSectionKind,
  pageSize: number,
  teamCategory: FloorballTeamCategory,
): Promise<PanelSectionPage> {
  const settled = await Promise.allSettled(
    sports.map((sport) => fetchSport(sport, kind, pageSize, teamCategory)),
  );

  const pages: PanelSectionPage[] = [];
  settled.forEach((result, index) => {
    if (result.status === 'fulfilled') {
      pages.push(result.value);
      return;
    }
    console.error(`MatchesPanel: ${sports[index]} ${kind} failed`, result.reason);
  });

  if (pages.length === 0) {
    throw new Error(`Failed to load ${kind} matches`);
  }

  const merged = pages
    .flatMap((page) => page.matches)
    .sort((left, right) => compareMatches(left, right, kind === 'completed'));
  const matches = kind === 'live' ? merged : merged.slice(0, pageSize);

  return {
    matches,
    totalCount: pages.reduce((sum, page) => sum + page.totalCount, 0),
  };
}
