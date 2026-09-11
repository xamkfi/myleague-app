import { useState, useCallback } from 'react';
import { floorballTeamService } from '../../../../../api/floorball/floorballTeamService';
import { floorballPlayerService, type FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import { floorballMatchService } from '../../../../../api/floorball/floorballMatchService';
import type { FloorballMatchDto, FloorballTeam } from '../../../../../types/floorball/floorballTypes';
import type { StateUpdate } from '../components/types';

interface UseMatchDataProps {
  match: FloorballMatchDto;
  onMatchUpdated?: (updatedMatch: FloorballMatchDto) => void;
  onStateUpdate?: (updates: StateUpdate) => void;
}

export const useMatchData = ({ 
  match, 
  onMatchUpdated, 
  onStateUpdate 
}: UseMatchDataProps) => {
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
        floorballTeamService.getById(match.homeTeamId),
        floorballTeamService.getById(match.awayTeamId)
      ]);

      setHomeTeam(homeTeamData);
      setAwayTeam(awayTeamData);

      // Load players for both teams
      const [homePlayersDataRaw, awayPlayersDataRaw] = await Promise.all([
        floorballPlayerService.getByTeamId(match.homeTeamId),
        floorballPlayerService.getByTeamId(match.awayTeamId)
      ]);

      const homeRosterMap = new Map(homeTeamData?.roster?.map(tp => [tp.playerId, tp.jerseyNumber]) || []);
      const awayRosterMap = new Map(awayTeamData?.roster?.map(tp => [tp.playerId, tp.jerseyNumber]) || []);
      const homePlayersData = homePlayersDataRaw.map(p => ({ ...p, jerseyNumber: homeRosterMap.get(p.id) }));
      const awayPlayersData = awayPlayersDataRaw.map(p => ({ ...p, jerseyNumber: awayRosterMap.get(p.id) }));

      setHomePlayers(homePlayersData);
      setAwayPlayers(awayPlayersData);
      
    } catch (error) {
      console.error('Error loading team data:', error);
      setError('Failed to load team data');
    } finally {
      setLoading(false);
    }
  }, [match.homeTeamId, match.awayTeamId]);

  /**
   * Loads the current match status from the backend
   * This ensures we have the most up-to-date match information
   */
  const loadCurrentMatchStatus = useCallback(async () => {
    try {
      console.log('Loading current match status for match:', match.id);
      const response = await floorballMatchService.getById(match.id);
      
      console.log('Match status response:', response);
      
      if (response.success && response.data) {
        const updatedMatch = response.data;
        console.log('Updated match data:', updatedMatch);
        console.log('Current scores - Home:', updatedMatch.homeScore, 'Away:', updatedMatch.awayScore);
        
        setCurrentMatch(updatedMatch);
        
        // Update the liveState with the new score from backend
        if (onStateUpdate) {
          const newScore = {
            home: updatedMatch.homeScore,
            away: updatedMatch.awayScore
          };
          console.log('Updating liveState with new score:', newScore);
          onStateUpdate({
            currentScore: newScore
          });
        }
        
        // Notify parent component about the updated match data
        if (onMatchUpdated) {
          console.log('Notifying parent component about updated match data');
          onMatchUpdated(updatedMatch);
        }
      } else {
        console.warn('Failed to load match status:', response.message || 'Unknown error');
      }
    } catch (error) {
      console.error('Error loading current match status:', error);
      // Don't set error for status loading - it's not critical
    }
  }, [match.id, onStateUpdate, onMatchUpdated]);

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
    return player ? `${player.person.firstName} ${player.person.lastName}` : `Player ${playerId.slice(0, 8)}...`;
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
    loadCurrentMatchStatus,
    
    // Utility functions
    getPlayersForTeam,
    getPlayerNameById
  };
}; 