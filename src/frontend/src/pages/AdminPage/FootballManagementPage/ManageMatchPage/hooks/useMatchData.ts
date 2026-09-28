import { useState, useCallback } from 'react';
import { footballTeamService } from '../../../../../api/football/footballTeamService';
import type { FootballPlayerDto } from '../../../../../api/football/footballPlayerService';
import { footballMatchService } from '../../../../../api/football/footballMatchService';
import type { FootballMatchDto, FootballTeam, FootballTeamPlayer } from '../../../../../types/football/footballTypes';
import type { StateUpdate } from '../components/types';

function splitPlayerName(fullName: string): { firstName: string; lastName: string } {
  const trimmed = fullName.trim();
  const spaceIndex = trimmed.indexOf(' ');
  if (spaceIndex === -1) {
    return { firstName: trimmed, lastName: '' };
  }
  return {
    firstName: trimmed.slice(0, spaceIndex),
    lastName: trimmed.slice(spaceIndex + 1),
  };
}

/** Players on this competition's active roster, not earlier seasons of the same team. */
function rosterToPlayers(roster: FootballTeamPlayer[]): FootballPlayerDto[] {
  return roster
    .filter((row) => row.isActive)
    .map((row) => {
      const { firstName, lastName } = splitPlayerName(row.playerName);
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
