import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { floorballPlayerService, type FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import type { FloorballTeam } from '../../../../../types/floorball/floorballTypes';
import './TeamPlayersRow.scss';

interface TeamPlayersRowProps {
  teamId: string;
  isExpanded: boolean;
  isClosing: boolean;
  team?: FloorballTeam; // Add team data as fallback
}

const TeamPlayersRow = ({ teamId, isExpanded, isClosing, team }: TeamPlayersRowProps) => {
  const { t } = useTranslation();
  const [players, setPlayers] = useState<FloorballPlayerDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [useRosterFallback, setUseRosterFallback] = useState(false);

  useEffect(() => {
    if (isExpanded && teamId) {
      fetchPlayers();
    }
  }, [isExpanded, teamId]); // eslint-disable-line react-hooks/exhaustive-deps

  const fetchPlayers = async () => {
    try {
      setLoading(true);
      setError(null);
      setUseRosterFallback(false);
      
      const playersData = await floorballPlayerService.getByTeamId(teamId);
      setPlayers(playersData);
    } catch (err) {
      console.error('Player API failed:', err);
      
      // Always try to use team roster as fallback first if available
      if (team?.roster && team.roster.length > 0) {
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

  const displayPlayers = team?.roster && team.roster.length > 0 ? team.roster : players;
  const playerCount = displayPlayers.length;

  // Get unique positions and sort them in the correct order
  const positionOrder = ['Goalkeeper', 'Defender', 'Center', 'Forward'];
  const playerPositions = [...new Set(displayPlayers.map(p => p.position))].sort((a, b) => 
    positionOrder.indexOf(a) - positionOrder.indexOf(b)
  );

  return (
    <div className={`team-players-row ${isClosing ? 'is-closing' : ''}`}>
      <div className="team-players-container">
        <h4 className="players-title">
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
          <div className="admin-roster-section">
            {/* Playing positions */}
            {playerPositions.map((pos, key) => (
              <div key={key} className="admin-roster-group">
                <div className="admin-roster-position-header">
                  {t(`floorball.positions.${pos.toLowerCase()}`, pos)}
                </div>
                <div className="admin-roster-table-header">
                  <span className="col-jersey">#</span>
                  <span className="col-name">{t('roster.name', 'Name')}</span>
                  <span className="col-stat">{t('roster.age', 'Age')}</span>
                  <span className="col-stat">{t('roster.matchesPlayedShort', 'GP')}</span>
                  <span className="col-stat">{t('roster.goalsShort', 'G')}</span>
                  <span className="col-stat">{t('roster.assistsShort', 'A')}</span>
                </div>
                <div className="admin-roster-players">
                  {displayPlayers
                    .filter(player => player.position === pos)
                    .map((player, index) => {
                      const playerId = 'id' in player ? player.id : `${teamId}-${index}`;
                      const playerName = 'person' in player ? player.person.fullName : player.playerName;
                      const isActive = 'isActive' in player ? player.isActive : true;
                      
                      let jerseyNumber: number | undefined = undefined;
                      if ('jerseyNumber' in player) {
                        jerseyNumber = player.jerseyNumber !== null && player.jerseyNumber !== undefined ? player.jerseyNumber : undefined;
                      }
                      
                      let games = 0, goals = 0, assists = 0;
                      if ('gamesPlayed' in player) {
                        games = player.gamesPlayed || 0;
                        goals = player.goals || 0;
                        assists = player.assists || 0;
                      }

                      let age: number | undefined = undefined;
                      if ('person' in player && player.person.birthDate) {
                        const birthDate = new Date(player.person.birthDate);
                        const today = new Date();
                        age = today.getFullYear() - birthDate.getFullYear();
                        const monthDiff = today.getMonth() - birthDate.getMonth();
                        if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
                          age--;
                        }
                      }

                      return (
                        <div
                          key={playerId}
                          className={`admin-roster-player ${!isActive ? 'inactive' : ''}`}
                        >
                          <span className="col-jersey">{jerseyNumber ?? '?'}</span>
                          <span className="col-name">{playerName}</span>
                          <span className="col-stat">{age ?? '-'}</span>
                          <span className="col-stat">{games}</span>
                          <span className="col-stat">{goals || '-'}</span>
                          <span className="col-stat">{assists || '-'}</span>
                        </div>
                      );
                    })}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default TeamPlayersRow; 