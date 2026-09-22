export type SportKind = 'floorball' | 'football' | 'hockey';

export type CompetitionKind = 'season' | 'tournament';

export interface CompetitionRouteHints {
  competitionType?: 'Season' | 'Tournament' | null;
  tournamentGroupId?: string | null;
  tournamentStage?: string | null;
}

export function isTournamentCompetition(hints: CompetitionRouteHints | null | undefined): boolean {
  if (!hints) {
    return false;
  }
  if (hints.competitionType === 'Tournament') {
    return true;
  }
  if (hints.competitionType === 'Season') {
    return false;
  }
  if (hints.tournamentGroupId) {
    return true;
  }
  const stage = hints.tournamentStage;
  return Boolean(stage && stage !== 'None');
}

const GUID_RE = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

export function isGuid(value: string | null | undefined): value is string {
  return typeof value === 'string' && GUID_RE.test(value);
}

function withTab(path: string, tab?: string): string {
  return tab ? `${path}?tab=${tab}` : path;
}

function withSeason(path: string, seasonId?: string | null): string {
  return isGuid(seasonId) ? `${path}?season=${seasonId}` : path;
}

export function getTeamPath(sport: SportKind, slug: string, seasonId?: string | null): string {
  if (sport === 'football') {
    return withSeason(`/football/team/${slug}`, seasonId);
  }
  if (sport === 'hockey') {
    return withSeason(`/hockey/team/${slug}`, seasonId);
  }
  return withSeason(`/team/${slug}`, seasonId);
}

export function getPlayerPath(sport: SportKind, playerId: string): string {
  return `/player/${playerId}?sport=${sport}`;
}

export function getMatchPath(sport: SportKind, matchId: string): string {
  if (sport === 'football') {
    return `/football/match/${matchId}`;
  }
  if (sport === 'hockey') {
    return `/hockey/match/${matchId}`;
  }
  return `/match/${matchId}`;
}

export function getLeaguePath(sport: SportKind, leagueId: string, tab?: string): string {
  if (sport === 'football') {
    return withTab(`/football/league/${leagueId}`, tab);
  }
  if (sport === 'hockey') {
    return withTab(`/hockey/league/${leagueId}`, tab);
  }
  return withTab(`/league/${leagueId}`, tab);
}

export function getTournamentPath(sport: SportKind, tournamentId: string, tab?: string): string {
  if (sport === 'football') {
    return withTab(`/football/tournaments/${tournamentId}`, tab);
  }
  if (sport === 'hockey') {
    return withTab(`/hockey/tournaments/${tournamentId}`, tab);
  }
  return withTab(`/tournaments/${tournamentId}`, tab);
}

export function getCompetitionPathForSport(
  sport: SportKind,
  competitionId: string,
  kindOrHints?: CompetitionKind | CompetitionRouteHints | null,
  tab?: string,
): string {
  const kind: CompetitionKind =
    typeof kindOrHints === 'string'
      ? kindOrHints
      : isTournamentCompetition(kindOrHints ?? undefined)
        ? 'tournament'
        : 'season';

  return kind === 'tournament'
    ? getTournamentPath(sport, competitionId, tab)
    : getLeaguePath(sport, competitionId, tab);
}
