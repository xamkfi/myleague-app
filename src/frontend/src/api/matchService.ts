import type { Match, StandingsRow, TeamStat } from '../types/league.types';

// Mock data for development
const mockMatch: Match = {
  id: '1',
  date: '12/6/2025',
  homeTeam: { id: '1', name: 'Team 1' },
  awayTeam: { id: '2', name: 'Team 2' },
  status: 'scheduled'
};

const mockStandings: StandingsRow[] = [
  { position: 1, team: 'JOUKKUE 1', points: 37 },
  { position: 2, team: 'JOUKKUE 2', points: 32 },
  { position: 3, team: 'JOUKKUE 3', points: 30 }
];

const mockTeamStats: TeamStat[] = [
  { teamName: 'Joukkue 1', playerName: '', value: 4 },
  { teamName: 'Joukkue 2', playerName: '', value: 3 },
  { teamName: 'Joukkue 1', playerName: '', value: 4 },
  { teamName: 'Joukkue 2', playerName: '', value: 3 },
  { teamName: 'Joukkue 1', playerName: '', value: 4 },
  { teamName: 'Joukkue 2', playerName: '', value: 3 }
];

// API service functions (simulated)
export const getNextMatchApi = async (): Promise<Match> => {
  // Simulating API call with a delay
  return new Promise((resolve) => {
    setTimeout(() => resolve(mockMatch), 500);
  });
};

export const getStandingsApi = async (): Promise<StandingsRow[]> => {
  // Simulating API call with a delay
  return new Promise((resolve) => {
    setTimeout(() => resolve(mockStandings), 500);
  });
};

export const getTeamStatsApi = async (): Promise<TeamStat[]> => {
  // Simulating API call with a delay
  return new Promise((resolve) => {
    setTimeout(() => resolve(mockTeamStats), 500);
  });
}; 