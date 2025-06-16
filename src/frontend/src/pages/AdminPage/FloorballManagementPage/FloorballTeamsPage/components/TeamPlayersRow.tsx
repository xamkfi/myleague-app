import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { floorballPlayerService, type FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import type { FloorballTeam } from '../../../../../types/floorball/floorballTypes';

interface TeamPlayersRowProps {
  teamId: string;
  isExpanded: boolean;
  team?: FloorballTeam; // Add team data as fallback
}

const TeamPlayersRow = ({ teamId, isExpanded, team }: TeamPlayersRowProps) => {
  const { t } = useTranslation();
  const [players, setPlayers] = useState<FloorballPlayerDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [useRosterFallback, setUseRosterFallback] = useState(false);

  useEffect(() => {
    if (isExpanded && teamId) {
      fetchPlayers();
    }
  }, [isExpanded, teamId]);

  const fetchPlayers = async () => {
    try {
      setLoading(true);
      setError(null);
      setUseRosterFallback(false);
      
      console.log('Attempting to fetch players for team:', teamId);
      const playersData = await floorballPlayerService.getByTeamId(teamId);
      console.log('Successfully fetched players:', playersData.length);
      setPlayers(playersData);
    } catch (err) {
      console.error('Player API failed:', err);
      
      // Always try to use team roster as fallback first if available
      if (team?.roster && team.roster.length > 0) {
        console.log('Using team roster as fallback:', team.roster.length, 'players');
        setUseRosterFallback(true);
        setError(null);
        setPlayers([]); // Clear API players since we're using roster
      } else {
        // No roster data available
        if (err instanceof Error && err.message.includes('500')) {
          console.log('Server error and no roster data - likely new team with no players');
          setPlayers([]);
          setError(null);
        } else {
          // Other errors - show error with retry option
          console.log('API error and no roster fallback available');
          setPlayers([]);
          setError(err instanceof Error ? err.message : 'Failed to fetch players');
        }
      }
    } finally {
      setLoading(false);
    }
  };

  if (!isExpanded) return null;

  // Use roster data if API failed and roster is available
  const displayPlayers = useRosterFallback && team?.roster ? team.roster : players;
  const playerCount = displayPlayers.length;

  return (
    <tr className="team-players-row">
      <td colSpan={6} className="team-players-cell">
        <div className="team-players-container">
          <h4 className="players-title">
            {t('floorball.teams.players', 'Team Players')} ({playerCount})
            {useRosterFallback && (
              <span className="fallback-notice">
                {' '}({t('floorball.teams.rosterData', 'Roster Data')})
              </span>
            )}
          </h4>
          
          {loading && (
            <div className="players-loading">
              <p>{t('common.loading', 'Loading...')}</p>
            </div>
          )}

          {error && !useRosterFallback && (
            <div className="players-error">
              <p>{error}</p>
              <button 
                onClick={fetchPlayers}
                className="retry-button"
              >
                {t('common.retry', 'Retry')}
              </button>
            </div>
          )}

          {!loading && !error && playerCount === 0 && (
            <div className="no-players">
              <p>{t('floorball.teams.noPlayersInTeam', 'This team has no players assigned yet.')}</p>
              <p className="help-text">
                {t('floorball.teams.addPlayersHelp', 'Use the edit button to manage team roster and add players.')}
              </p>
            </div>
          )}

          {!loading && playerCount > 0 && (
            <div className="players-grid">
              {displayPlayers.map((player, index) => {
                // Handle both API player data and roster data
                const playerId = 'id' in player ? player.id : `${teamId}-${index}`;
                const playerName = 'fullName' in player ? player.fullName : player.playerName;
                const isActive = 'isActive' in player ? player.isActive : true;
                const position = 'position' in player ? player.position : player.position;
                const jerseyNumber = 'jerseyNumber' in player ? player.jerseyNumber : player.jerseyNumber;
                
                // Handle stats with proper type checking
                let games = 0, goals = 0, assists = 0, penalties = 0;
                if ('gamesPlayed' in player) {
                  games = player.gamesPlayed || 0;
                  goals = player.goals || 0;
                  assists = player.assists || 0;
                  penalties = player.penaltyMinutes || 0;
                }

                return (
                  <div 
                    key={playerId} 
                    className={`player-card ${!isActive ? 'inactive' : ''}`}
                  >
                    <div className="player-header">
                      <div className="player-name">
                        <span className="name">{playerName}</span>
                        {jerseyNumber && (
                          <span className="jersey-number">#{jerseyNumber}</span>
                        )}
                      </div>
                      <div className="player-status">
                        <span className={`status-badge ${isActive ? 'active' : 'inactive'}`}>
                          {isActive 
                            ? t('common.active', 'Active')
                            : t('common.inactive', 'Inactive')
                          }
                        </span>
                      </div>
                    </div>
                    
                    <div className="player-details">
                      {position && (
                        <div className="player-position">
                          <span className="label">{t('floorball.players.position', 'Position')}:</span>
                          <span className="value">{t(`floorball.positions.${position.toLowerCase()}`, position)}</span>
                        </div>
                      )}
                      
                      <div className="player-stats">
                        <div className="stat">
                          <span className="label">{t('floorball.players.games', 'Games')}:</span>
                          <span className="value">{games || 0}</span>
                        </div>
                        <div className="stat">
                          <span className="label">{t('floorball.players.goals', 'Goals')}:</span>
                          <span className="value">{goals || 0}</span>
                        </div>
                        <div className="stat">
                          <span className="label">{t('floorball.players.assists', 'Assists')}:</span>
                          <span className="value">{assists || 0}</span>
                        </div>
                        <div className="stat">
                          <span className="label">{t('floorball.players.penalties', 'PIM')}:</span>
                          <span className="value">{penalties || 0}</span>
                        </div>
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>
      </td>
    </tr>
  );
};

export default TeamPlayersRow; 