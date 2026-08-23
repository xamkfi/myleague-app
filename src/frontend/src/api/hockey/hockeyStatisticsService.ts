import type {
  HockeyCompetitionStatisticsSummaryDto,
  HockeyGoalieCompetitionStatisticsDto,
  HockeyMatchStatisticsDto,
  HockeyPlayerCompetitionStatisticsDto,
  HockeyTeamCompetitionStatisticsDto,
  HockeyTopGoalieDto,
  HockeyTopScorerDto,
} from '../../types/hockey/hockeyTypes';
import { hockeyRequest, hockeyRequestVoid } from './hockeyApi';

export const hockeyStatisticsService = {
  getMatchStats: (matchId: string): Promise<HockeyMatchStatisticsDto> =>
    hockeyRequest<HockeyMatchStatisticsDto>(
      `/HockeyStatistics/matches/${matchId}`,
      'Failed to fetch match statistics',
    ),

  getStandings: (competitionId: string): Promise<HockeyTeamCompetitionStatisticsDto[]> =>
    hockeyRequest<HockeyTeamCompetitionStatisticsDto[]>(
      `/HockeyStatistics/standings/${competitionId}`,
      'Failed to fetch standings',
    ),

  getGroupStandings: (
    competitionId: string,
    groupId: string,
  ): Promise<HockeyTeamCompetitionStatisticsDto[]> =>
    hockeyRequest<HockeyTeamCompetitionStatisticsDto[]>(
      `/HockeyStatistics/standings/${competitionId}/groups/${groupId}`,
      'Failed to fetch group standings',
    ),

  getPlayers: (
    competitionId: string,
    playerId?: string,
  ): Promise<HockeyPlayerCompetitionStatisticsDto[]> => {
    const query = playerId ? `?playerId=${encodeURIComponent(playerId)}` : '';
    return hockeyRequest<HockeyPlayerCompetitionStatisticsDto[]>(
      `/HockeyStatistics/players/${competitionId}${query}`,
      'Failed to fetch player statistics',
    );
  },

  getGoalies: (
    competitionId: string,
    playerId?: string,
  ): Promise<HockeyGoalieCompetitionStatisticsDto[]> => {
    const query = playerId ? `?playerId=${encodeURIComponent(playerId)}` : '';
    return hockeyRequest<HockeyGoalieCompetitionStatisticsDto[]>(
      `/HockeyStatistics/goalies/${competitionId}${query}`,
      'Failed to fetch goalie statistics',
    );
  },

  getTopScorers: (competitionId: string, topN = 10): Promise<HockeyTopScorerDto[]> =>
    hockeyRequest<HockeyTopScorerDto[]>(
      `/HockeyStatistics/topscorers/${competitionId}?topN=${topN}`,
      'Failed to fetch top scorers',
    ),

  getTopGoalies: (competitionId: string, topN = 10): Promise<HockeyTopGoalieDto[]> =>
    hockeyRequest<HockeyTopGoalieDto[]>(
      `/HockeyStatistics/topgoalies/${competitionId}?topN=${topN}`,
      'Failed to fetch top goalies',
    ),

  getSummary: (competitionId: string): Promise<HockeyCompetitionStatisticsSummaryDto> =>
    hockeyRequest<HockeyCompetitionStatisticsSummaryDto>(
      `/HockeyStatistics/summary/${competitionId}`,
      'Failed to fetch statistics summary',
    ),

  recalculateMatch: (matchId: string): Promise<void> =>
    hockeyRequestVoid(
      `/HockeyStatistics/matches/${matchId}/recalculate`,
      'Failed to recalculate match statistics',
      { method: 'POST' },
    ),

  recalculateCompetition: (competitionId: string): Promise<void> =>
    hockeyRequestVoid(
      `/HockeyStatistics/competitions/${competitionId}/recalculate`,
      'Failed to recalculate competition statistics',
      { method: 'POST', body: JSON.stringify({}) },
    ),
};
