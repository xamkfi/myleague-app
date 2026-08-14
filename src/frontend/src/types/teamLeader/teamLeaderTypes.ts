export type TeamLeaderSport = 'floorball' | 'football';

/** A team managed by the current team leader, as returned by GET /api/team-leader/my-teams. */
export interface TeamLeaderTeam {
  sport: TeamLeaderSport;
  teamId: string;
  name: string;
  shortName: string;
  logoUrl?: string | null;
}
