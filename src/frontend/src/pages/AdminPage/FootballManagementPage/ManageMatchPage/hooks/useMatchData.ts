import { useState, useCallback } from 'react';
import { footballTeamService } from '../../../../../api/football/footballTeamService';
import { footballPlayerService, type FootballPlayerDto } from '../../../../../api/football/footballPlayerService';
import { footballMatchService } from '../../../../../api/football/footballMatchService';
import type { FootballMatchDto, FootballTeam } from '../../../../../types/football/footballTypes';
import type { StateUpdate } from '../components/types';

interface UseMatchDataProps {
  match: FootballMatchDto;
  onMatchUpdated?: (updatedMatch: FootballMatchDto) => void;
  onStateUpdate?: (updates: StateUpdate) => void;
}

export const useMatchData = ({
  match,
  onMatchUpdated,
  onStateUpdate,
}: UseMatchDataProps) => {
  const [homeTeam, setHomeTeam] = useState<FootballTeam | null>(null);
  const [awayTeam, setAwayTeam] = useState<FootballTeam | null>(null);
  const [homePlayers, setHomePlayers] = useState<FootballPlayerDto[]>([]);
  const [awayPlayers, setAwayPlayers] = useState<FootballPlayerDto[]>([]);
  const [currentMatch, setCurrentMatch] = useState<FootballMatchDto>(match);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadTeamData = useCallback(async () => {
    try {
      setLoading(true);

      if (!match.homeTeamId || !match.awayTeamId) {
        setError('Match does not have both teams assigned yet. Assign teams before managing the match.');
        setLoading(false);
        return;
      }

      const [homeTeamData, awayTeamData] = await Promise.all([
        footballTeamService.getById(match.homeTeamId),
        footballTeamService.getById(match.awayTeamId),
      ]);

      setHomeTeam(homeTeamData);
      setAwayTeam(awayTeamData);

      const [homePlayersDataRaw, awayPlayersDataRaw] = await Promise.all([
        footballPlayerService.getByTeamId(match.homeTeamId),
        footballPlayerService.getByTeamId(match.awayTeamId),
      ]);

      const homeRosterMap = new Map(homeTeamData?.roster?.map((tp) => [tp.playerId, tp.jerseyNumber]) || []);
      const awayRosterMap = new Map(awayTeamData?.roster?.map((tp) => [tp.playerId, tp.jerseyNumber]) || []);
      const homePlayersData = homePlayersDataRaw.map((p) => ({ ...p, jerseyNumber: homeRosterMap.get(p.id) }));
      const awayPlayersData = awayPlayersDataRaw.map((p) => ({ ...p, jerseyNumber: awayRosterMap.get(p.id) }));

      setHomePlayers(homePlayersData);
      setAwayPlayers(awayPlayersData);
    } catch (loadError) {
      console.error('Error loading team data:', loadError);
      setError('Failed to load team data');
    } finally {
      setLoading(false);
    }
  }, [match.homeTeamId, match.awayTeamId]);

  const loadCurrentMatchStatus = useCallback(async () => {
    try {
      const response = await footballMatchService.getById(match.id);

      if (response.success && response.data) {
        const updatedMatch = response.data;
        setCurrentMatch(updatedMatch);

        if (onStateUpdate) {
          onStateUpdate({
            currentScore: {
              home: updatedMatch.homeScore,
              away: updatedMatch.awayScore,
            },
          });
        }

        if (onMatchUpdated) {
          onMatchUpdated(updatedMatch);
        }
      }
    } catch (statusError) {
      console.error('Error loading current match status:', statusError);
    }
  }, [match.id, onStateUpdate, onMatchUpdated]);

  const getPlayersForTeam = useCallback((teamId: string) => {
    return teamId === currentMatch.homeTeamId ? homePlayers : awayPlayers;
  }, [currentMatch.homeTeamId, homePlayers, awayPlayers]);

  const getPlayerNameById = useCallback((playerId: string | undefined | null): string => {
    if (!playerId) {
      return 'Unknown Player';
    }

    const allPlayers = [...homePlayers, ...awayPlayers];
    const player = allPlayers.find((p) => p.id === playerId);
    return player
      ? `${player.person.firstName} ${player.person.lastName}`
      : `Player ${playerId.slice(0, 8)}...`;
  }, [homePlayers, awayPlayers]);

  return {
    homeTeam,
    awayTeam,
    homePlayers,
    awayPlayers,
    currentMatch,
    setCurrentMatch,
    loading,
    setLoading,
    error,
    setError,
    loadTeamData,
    loadCurrentMatchStatus,
    getPlayersForTeam,
    getPlayerNameById,
  };
};
