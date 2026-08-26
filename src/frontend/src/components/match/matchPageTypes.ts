export type MatchTabType = 'summary' | 'lineups' | 'table';

export type TableTabVariant = 'season' | 'tournamentGroup' | 'tournamentPlayoff';

export interface MatchScoreHeaderTeam {
  name: string | null;
  logo: string | null | undefined;
  href: string | null;
}

export interface MatchScoreHeaderProps {
  home: MatchScoreHeaderTeam;
  away: MatchScoreHeaderTeam;
  homeScore: number;
  awayScore: number;
  scheduledDateTime: string;
  isScheduled: boolean;
  isLive: boolean;
  isFinal: boolean;
}

export function resolveTableTabVariant(
  isTournament: boolean,
  tournamentGroupId: string | null | undefined
): TableTabVariant {
  if (!isTournament) {
    return 'season';
  }

  return tournamentGroupId ? 'tournamentGroup' : 'tournamentPlayoff';
}
