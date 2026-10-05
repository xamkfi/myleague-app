import { useState, useCallback } from 'react';
import { footballTeamService } from '../../../../../api/football/footballTeamService';
import type { FootballPlayerDto } from '../../../../../api/football/footballPlayerService';
import { footballMatchService } from '../../../../../api/football/footballMatchService';
import type { FootballMatchDto, FootballTeam, FootballTeamPlayer } from '../../../../../types/football/footballTypes';
import { rosterDisplayName } from '../../../../../types/loanGoalkeeper';
import type { StateUpdate } from '../components/types';

/**
 * Every player on the competition roster is selectable for the match, whether or not the
 * licence is paid (`isActive`).
 */
function rosterToPlayers(roster: FootballTeamPlayer[]): FootballPlayerDto[] {
  const seen: Set<string> = new Set<string>();
  return roster
    .filter((row) => {
      if (seen.has(row.playerId)) return false;
      seen.add(row.playerId);
      return true;
    })
    .map((row) => {
      const { firstName, lastName } = rosterDisplayName(row.playerName);
      return {
        id: row.playerId,
        personId: '',
        person: {
          id: '',
          firstName,
          lastName,
          birthDate: '',
          fullName: row.playerName,
          isRegistered: false,
        },
        isActive: true,
        position: row.position,
        careerGoals: row.goals,
        careerAssists: row.assists,
        jerseyNumber: row.jerseyNumber,
      };
    });
}

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
        footballTeamService.getById(match.homeTeamId, match.competitionId),
        footballTeamService.getById(match.awayTeamId, match.competitionId),
      ]);

      setHomeTeam(homeTeamData);
      setAwayTeam(awayTeamData);
      setHomePlayers(rosterToPlayers(homeTeamData.roster ?? []));
      setAwayPlayers(rosterToPlayers(awayTeamData.roster ?? []));
    } catch (loadError) {
      console.error('Error loading team data:', loadError);
      setError('Failed to load team data');
    } finally {
      setLoading(false);
    }
  }, [match.homeTeamId, match.awayTeamId, match.competitionId]);

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
    if (!player) {
      return `Player ${playerId.slice(0, 8)}...`;
    }
    return [player.person.firstName, player.person.lastName].filter((part) => part.trim().length > 0).join(' ');
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
    appendPlayer: (teamId: string, player: FootballPlayerDto): void => {
      const add = (current: FootballPlayerDto[]): FootballPlayerDto[] =>
        current.some((item) => item.id === player.id) ? current : [...current, player];
      if (teamId === match.homeTeamId) {
        setHomePlayers(add);
      } else if (teamId === match.awayTeamId) {
        setAwayPlayers(add);
      }
    },
    loadCurrentMatchStatus,
    getPlayersForTeam,
    getPlayerNameById,
  };
};
