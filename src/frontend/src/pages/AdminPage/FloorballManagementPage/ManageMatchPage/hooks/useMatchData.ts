import { useState, useCallback } from 'react';
import { floorballTeamService } from '../../../../../api/floorball/floorballTeamService';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import { floorballMatchService } from '../../../../../api/floorball/floorballMatchService';
import type { FloorballMatchDto, FloorballTeam, FloorballTeamPlayer } from '../../../../../types/floorball/floorballTypes';
import { rosterDisplayName } from '../../../../../types/loanGoalkeeper';

/**
 * Every player on the competition roster is selectable for the match, whether or not the
 * licence is paid (`isActive`).
 */
function rosterToPlayers(roster: FloorballTeamPlayer[]): FloorballPlayerDto[] {
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
  match: FloorballMatchDto;
  onMatchUpdated?: (updatedMatch: FloorballMatchDto) => void;
}

export const useMatchData = ({ match, onMatchUpdated }: UseMatchDataProps) => {
  // State management
  const [homeTeam, setHomeTeam] = useState<FloorballTeam | null>(null);
  const [awayTeam, setAwayTeam] = useState<FloorballTeam | null>(null);
  const [homePlayers, setHomePlayers] = useState<FloorballPlayerDto[]>([]);
  const [awayPlayers, setAwayPlayers] = useState<FloorballPlayerDto[]>([]);
  const [currentMatch, setCurrentMatch] = useState<FloorballMatchDto>(match);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  /**
   * Loads team and player data for both teams
   */
  const loadTeamData = useCallback(async () => {
    try {
      setLoading(true);

      // ManageMatchPage is only used by admins to operate a live match, which by definition has
      // both teams assigned. Defensively short-circuit here when a slot is still null so the
      // hook fails loudly with a clear error rather than triggering a server roundtrip with
      // undefined ids.
      if (!match.homeTeamId || !match.awayTeamId) {
        setError('Match does not have both teams assigned yet. Assign teams before managing the match.');
        setLoading(false);
        return;
      }

      const [homeTeamData, awayTeamData] = await Promise.all([
        floorballTeamService.getById(match.homeTeamId, match.competitionId),
        floorballTeamService.getById(match.awayTeamId, match.competitionId)
      ]);

      setHomeTeam(homeTeamData);
      setAwayTeam(awayTeamData);
      setHomePlayers(rosterToPlayers(homeTeamData.roster ?? []));
      setAwayPlayers(rosterToPlayers(awayTeamData.roster ?? []));
      
    } catch (error) {
      console.error('Error loading team data:', error);
      setError('Failed to load team data');
    } finally {
      setLoading(false);
    }
  }, [match.homeTeamId, match.awayTeamId, match.competitionId]);

  /**
   * Loads the current match status from the backend
   * This ensures we have the most up-to-date match information
   */
  const loadCurrentMatchStatus = useCallback(async () => {
    try {
      const response = await floorballMatchService.getById(match.id);
      if (response.success && response.data) {
        setCurrentMatch(response.data);
        onMatchUpdated?.(response.data);
      } else {
        console.warn('Failed to load match status:', response.message || 'Unknown error');
      }
    } catch (error) {
      // Not critical: the page keeps the last known match state.
      console.error('Error loading current match status:', error);
    }
  }, [match.id, onMatchUpdated]);

  /**
   * Gets all players for a specific team (home or away)
   */
  const getPlayersForTeam = useCallback((teamId: string) => {
    return teamId === currentMatch.homeTeamId ? homePlayers : awayPlayers;
  }, [currentMatch.homeTeamId, homePlayers, awayPlayers]);

  /**
   * Looks up a player's full name by their ID using the loaded player data
   */
  const getPlayerNameById = useCallback((playerId: string | undefined | null): string => {
    if (!playerId) {
      return 'Unknown Player';
    }
    
    const allPlayers = [...homePlayers, ...awayPlayers];
    const player = allPlayers.find(p => p.id === playerId);
    if (!player) {
      return `Player ${playerId.slice(0, 8)}...`;
    }
    return [player.person.firstName, player.person.lastName].filter((part) => part.trim().length > 0).join(' ');
  }, [homePlayers, awayPlayers]);

  return {
    // State
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
    
    // Actions
    loadTeamData,
    appendPlayer: (teamId: string, player: FloorballPlayerDto): void => {
      const add = (current: FloorballPlayerDto[]): FloorballPlayerDto[] =>
        current.some((item) => item.id === player.id) ? current : [...current, player];
      if (teamId === match.homeTeamId) {
        setHomePlayers(add);
      } else if (teamId === match.awayTeamId) {
        setAwayPlayers(add);
      }
    },
    loadCurrentMatchStatus,
    
    // Utility functions
    getPlayersForTeam,
    getPlayerNameById
  };
}; 