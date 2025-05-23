export interface Team {
  id: string;
  name: string;
  logo?: string;
}

export interface Match {
  id: string;
  date: string;
  homeTeam: Team;
  awayTeam: Team;
  homeScore?: number;
  awayScore?: number;
  venue?: string;
  status: 'scheduled' | 'live' | 'completed' | 'cancelled';
}

export interface StandingsRow {
  position: number;
  team: string;
  points: number;
  gamesPlayed?: number;
  wins?: number;
  losses?: number;
  ties?: number;
}

export interface TeamStat {
  teamName: string;
  playerName: string;
  value: number;
  statType?: string;
} 