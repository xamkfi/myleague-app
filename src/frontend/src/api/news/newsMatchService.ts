import { floorballMatchService } from '../floorball/floorballMatchService';
import { floorballSeasonService } from '../floorball/floorballSeasonService';
import { floorballTournamentService } from '../floorball/floorballTournamentService';
import { footballMatchService } from '../football/footballMatchService';
import { footballSeasonService } from '../football/footballSeasonService';
import { footballTournamentService } from '../football/footballTournamentService';
import { hockeyMatchService } from '../hockey/hockeyMatchService';
import { hockeySeasonService } from '../hockey/hockeySeasonService';
import { hockeyTeamService } from '../hockey/hockeyTeamService';
import { hockeyTournamentService } from '../hockey/hockeyTournamentService';
import { FloorballMatchStatus, type FloorballMatchDto } from '../../types/floorball/floorballTypes';
import { FootballMatchStatus, type FootballMatchDto } from '../../types/football/footballTypes';
import {
  FINISHED_HOCKEY_STATUSES,
  LIVE_HOCKEY_STATUSES,
  type HockeyMatchListDto,
  type HockeyMatchStatus,
} from '../../types/hockey/hockeyTypes';
import type { SportKind } from '../../utils/sportRoutes';

/** Normalised match status shared by every sport inside a news match embed. */
export type NewsMatchStatus = 'scheduled' | 'in_progress' | 'completed' | 'cancelled' | 'postponed';

/** One match as offered by the match picker of the news editor. */
export interface NewsMatchOption {
  sport: SportKind;
  id: string;
  homeTeamName: string | null;
  awayTeamName: string | null;
  homeTeamLogo: string | null;
  awayTeamLogo: string | null;
  scheduledDateTime: string;
  status: NewsMatchStatus;
  homeScore: number;
  awayScore: number;
  competitionName: string | null;
}

/** A league season or tournament the picker can narrow matches to. */
export interface NewsCompetitionOption {
  id: string;
  name: string;
  kind: 'season' | 'tournament';
  startDate: string;
  endDate: string;
}

export type NewsMatchCategory = 'all' | 'scheduled' | 'results' | 'cancelled';

export interface NewsMatchQuery {
  competitionId?: string;
  category: NewsMatchCategory;
  page: number;
  pageSize: number;
  /** Team name to search for. Matched on the server for floorball and football. */
  searchQuery?: string;
}

export interface NewsMatchPage {
  items: NewsMatchOption[];
  totalCount: number;
  totalPages: number;
}

export interface NewsMatchLogos {
  home?: string | null;
  away?: string | null;
}

function byNewestStart(a: NewsCompetitionOption, b: NewsCompetitionOption): number {
  return new Date(b.startDate).getTime() - new Date(a.startDate).getTime();
}

/** Upcoming matches read best soonest-first; everything else newest-first. */
function sortOrderFor(category: NewsMatchCategory): 'asc' | 'desc' {
  return category === 'scheduled' ? 'asc' : 'desc';
}

function floorballStatus(status: FloorballMatchStatus): NewsMatchStatus {
  switch (status) {
    case FloorballMatchStatus.InProgress:
      return 'in_progress';
    case FloorballMatchStatus.Completed:
      return 'completed';
    case FloorballMatchStatus.Cancelled:
      return 'cancelled';
    case FloorballMatchStatus.Postponed:
      return 'postponed';
    default:
      return 'scheduled';
  }
}

function floorballStatusFilter(category: NewsMatchCategory): FloorballMatchStatus | undefined {
  switch (category) {
    case 'scheduled':
      return FloorballMatchStatus.Scheduled;
    case 'results':
      return FloorballMatchStatus.Completed;
    case 'cancelled':
      return FloorballMatchStatus.Cancelled;
    default:
      return undefined;
  }
}

function footballStatus(status: FootballMatchStatus): NewsMatchStatus {
  switch (status) {
    case FootballMatchStatus.InProgress:
      return 'in_progress';
    case FootballMatchStatus.Completed:
      return 'completed';
    case FootballMatchStatus.Cancelled:
      return 'cancelled';
    case FootballMatchStatus.Postponed:
      return 'postponed';
    default:
      return 'scheduled';
  }
}

function footballStatusFilter(category: NewsMatchCategory): FootballMatchStatus | undefined {
  switch (category) {
    case 'scheduled':
      return FootballMatchStatus.Scheduled;
    case 'results':
      return FootballMatchStatus.Completed;
    case 'cancelled':
      return FootballMatchStatus.Cancelled;
    default:
      return undefined;
  }
}

function hockeyStatus(status: HockeyMatchStatus | string): NewsMatchStatus {
  if (FINISHED_HOCKEY_STATUSES.includes(status as HockeyMatchStatus)) {
    return 'completed';
  }
  if (LIVE_HOCKEY_STATUSES.includes(status as HockeyMatchStatus)) {
    return 'in_progress';
  }
  if (status === 'Cancelled') {
    return 'cancelled';
  }
  if (status === 'Postponed') {
    return 'postponed';
  }
  return 'scheduled';
}

function hockeyStatusFilter(category: NewsMatchCategory): HockeyMatchStatus[] | undefined {
  switch (category) {
    case 'scheduled':
      return ['Scheduled'];
    case 'results':
      return FINISHED_HOCKEY_STATUSES;
    case 'cancelled':
      return ['Cancelled'];
    default:
      return undefined;
  }
}

function fromFloorball(match: FloorballMatchDto): NewsMatchOption {
  return {
    sport: 'floorball',
    id: match.id,
    homeTeamName: match.homeTeamName,
    awayTeamName: match.awayTeamName,
    homeTeamLogo: match.homeTeamLogo,
    awayTeamLogo: match.awayTeamLogo,
    scheduledDateTime: match.scheduledDateTime,
    status: floorballStatus(match.status),
    homeScore: match.homeScore,
    awayScore: match.awayScore,
    competitionName: match.competitionName || null,
  };
}

function fromFootball(match: FootballMatchDto): NewsMatchOption {
  return {
    sport: 'football',
    id: match.id,
    homeTeamName: match.homeTeamName,
    awayTeamName: match.awayTeamName,
    homeTeamLogo: match.homeTeamLogo,
    awayTeamLogo: match.awayTeamLogo,
    scheduledDateTime: match.scheduledDateTime,
    status: footballStatus(match.status),
    homeScore: match.homeScore,
    awayScore: match.awayScore,
    competitionName: match.competitionName || null,
  };
}

function fromHockey(match: HockeyMatchListDto, logos: Map<string, string | null>): NewsMatchOption {
  return {
    sport: 'hockey',
    id: match.id,
    homeTeamName: match.homeTeamName,
    awayTeamName: match.awayTeamName,
    homeTeamLogo: match.homeTeamId ? (logos.get(match.homeTeamId) ?? null) : null,
    awayTeamLogo: match.awayTeamId ? (logos.get(match.awayTeamId) ?? null) : null,
    scheduledDateTime: match.scheduledStartTime,
    status: hockeyStatus(match.status),
    homeScore: match.homeScore,
    awayScore: match.awayScore,
    competitionName: match.competitionName,
  };
}

let hockeyTeamLogos: Promise<Map<string, string | null>> | null = null;

/** Hockey match lists carry no logos, so look them up from the team list once per page load. */
function loadHockeyTeamLogos(): Promise<Map<string, string | null>> {
  if (!hockeyTeamLogos) {
    hockeyTeamLogos = hockeyTeamService
      .getAll()
      .then((teams) => new Map(teams.map((team) => [team.id, team.logoUrl])))
      .catch(() => {
        hockeyTeamLogos = null;
        return new Map<string, string | null>();
      });
  }
  return hockeyTeamLogos;
}

function matchesTeamName(match: HockeyMatchListDto, search: string): boolean {
  return [match.homeTeamName, match.awayTeamName].some((name) => name?.toLowerCase().includes(search) ?? false);
}

/** Sport-agnostic match lookups for the match embed of the news editor. */
export const newsMatchService = {
  /** Seasons and tournaments of one sport, newest first. */
  getCompetitions: async (sport: SportKind): Promise<NewsCompetitionOption[]> => {
    let options: NewsCompetitionOption[] = [];

    if (sport === 'floorball') {
      const [seasons, tournaments] = await Promise.all([
        floorballSeasonService.getAll(),
        floorballTournamentService.getAll(),
      ]);
      options = [
        ...(seasons.data ?? []).map((season) => ({ ...season, kind: 'season' as const })),
        ...(tournaments.data ?? []).map((tournament) => ({ ...tournament, kind: 'tournament' as const })),
      ];
    } else if (sport === 'football') {
      const [seasons, tournaments] = await Promise.all([
        footballSeasonService.getAll(),
        footballTournamentService.getAll(),
      ]);
      options = [
        ...(seasons.data ?? []).map((season) => ({ ...season, kind: 'season' as const })),
        ...(tournaments.data ?? []).map((tournament) => ({ ...tournament, kind: 'tournament' as const })),
      ];
    } else {
      const [seasons, tournaments] = await Promise.all([
        hockeySeasonService.getAll(),
        hockeyTournamentService.getAll(),
      ]);
      options = [
        ...seasons.map((season) => ({ ...season, kind: 'season' as const })),
        ...tournaments.map((tournament) => ({ ...tournament, kind: 'tournament' as const })),
      ];
    }

    return options
      .map(({ id, name, kind, startDate, endDate }) => ({ id, name, kind, startDate, endDate }))
      .sort(byNewestStart);
  },

  /** One page of matches for the picker. */
  getMatches: async (sport: SportKind, query: NewsMatchQuery): Promise<NewsMatchPage> => {
    const sortOrder = sortOrderFor(query.category);
    const searchQuery: string | undefined = query.searchQuery?.trim() || undefined;

    if (sport === 'floorball') {
      const response = await floorballMatchService.getAll({
        page: query.page,
        pageSize: query.pageSize,
        competitionId: query.competitionId,
        status: floorballStatusFilter(query.category),
        searchQuery,
        sortOrder,
      });
      return {
        items: response.data.map(fromFloorball),
        totalCount: response.pagination.totalCount,
        totalPages: response.pagination.totalPages,
      };
    }

    if (sport === 'football') {
      const response = await footballMatchService.getAll({
        page: query.page,
        pageSize: query.pageSize,
        competitionId: query.competitionId,
        status: footballStatusFilter(query.category),
        searchQuery,
        sortOrder,
      });
      return {
        items: response.data.map(fromFootball),
        totalCount: response.pagination.totalCount,
        totalPages: response.pagination.totalPages,
      };
    }

    // The public hockey list has no text search and the admin list only searches venues,
    // so a search loads the filtered matches and pages through the name matches here.
    if (searchQuery) {
      const search: string = searchQuery.toLowerCase();
      const [all, logos] = await Promise.all([
        hockeyMatchService.getAllListPages({
          competitionId: query.competitionId,
          statuses: hockeyStatusFilter(query.category),
          sortOrder,
        }),
        loadHockeyTeamLogos(),
      ]);
      const matching: HockeyMatchListDto[] = all.filter((match) => matchesTeamName(match, search));
      const start: number = (query.page - 1) * query.pageSize;
      return {
        items: matching.slice(start, start + query.pageSize).map((match) => fromHockey(match, logos)),
        totalCount: matching.length,
        totalPages: Math.ceil(matching.length / query.pageSize),
      };
    }

    const [response, logos] = await Promise.all([
      hockeyMatchService.getList({
        page: query.page,
        pageSize: query.pageSize,
        competitionId: query.competitionId,
        statuses: hockeyStatusFilter(query.category),
        sortOrder,
      }),
      loadHockeyTeamLogos(),
    ]);
    return {
      items: response.data.map((match) => fromHockey(match, logos)),
      totalCount: response.pagination.totalCount,
      totalPages: response.pagination.totalPages,
    };
  },

  /** Current team logos of one match, used to refresh a stored embed on display. */
  getMatchLogos: async (sport: SportKind, matchId: string): Promise<NewsMatchLogos | null> => {
    if (sport === 'floorball') {
      const response = await floorballMatchService.getById(matchId);
      return response.data ? { home: response.data.homeTeamLogo, away: response.data.awayTeamLogo } : null;
    }

    if (sport === 'football') {
      const response = await footballMatchService.getById(matchId);
      return response.data ? { home: response.data.homeTeamLogo, away: response.data.awayTeamLogo } : null;
    }

    const match = await hockeyMatchService.getById(matchId);
    const [home, away] = await Promise.all(
      [match.homeTeamId, match.awayTeamId].map(async (teamId) => {
        if (!teamId) {
          return null;
        }
        try {
          const team = await hockeyTeamService.getById(teamId);
          return team.logoUrl;
        } catch {
          return null;
        }
      }),
    );
    return { home, away };
  },
};
