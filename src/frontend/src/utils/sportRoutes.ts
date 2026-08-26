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

function withTab(path: string, tab?: string): string {
  return tab ? `${path}?tab=${tab}` : path;
}

export function getTeamPath(sport: SportKind, slug: string): string {
  if (sport === 'football') {
    return `/football/team/${slug}`;
  }
  if (sport === 'hockey') {
    return `/hockey/team/${slug}`;
  }
  return `/team/${slug}`;
}

export function getPlayerPath(sport: SportKind, playerId: string): string {
  if (sport === 'football') {
    return `/football/player/${playerId}`;
  }
  if (sport === 'hockey') {
    return `/hockeyplayer/${playerId}`;
  }
  return `/floorballplayer/${playerId}`;
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
