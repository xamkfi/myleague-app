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
          {t('floorball.teams.players', 'Team Players')} ({playerCount})
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
          <div className="roster-section">
            {/* Playing positions */}
            {playerPositions.map((pos, key) => (
              <div key={key} className="roster-container">
                <div className="roster-position-header">
                  {t(`floorball.positions.${pos.toLowerCase()}`, pos)}
                </div>

                <div className="roster-position-container">
                  <div className="table stats-header">
                    <div className="roster-jersey" title={t('roster.tooltips.jerseyNumber', 'Jersey Number')}>
                      {t('roster.jerseyNumber', '#')}
                    </div>
                    <div className="roster-player-name">{t('roster.name', 'Name')}</div>
                    <div className="roster-age" title={t('roster.tooltips.age', 'Age')}>
                      {t('roster.age', 'Age')}
                    </div>
                    <div className="roster-games-played" title={t('roster.tooltips.matchesPlayed', 'Matches Played')}>
                      {t('roster.matchesPlayed', 'Games')}
                    </div>
                    <div className="roster-goals" title={t('roster.tooltips.goals', 'Goals')}>
                      {t('roster.goals', 'Goals')}
                    </div>
                    <div className="roster-assists" title={t('roster.tooltips.assists', 'Assists')}>
                      {t('roster.assists', 'Assists')}
                    </div>
                  </div>
                  {displayPlayers
                    .filter(player => player.position === pos)
                    .map((player, index) => {
                      // Handle both API player data and roster data
                      const playerId = 'id' in player ? player.id : `${teamId}-${index}`;
                      const playerName = 'person' in player ? player.person.fullName : player.playerName;
                      const isActive = 'isActive' in player ? player.isActive : true;
                      
                      // Handle jersey number - roster data has it directly, API data doesn't
                      let jerseyNumber: number | undefined = undefined;
                      if ('jerseyNumber' in player) {
                        jerseyNumber = player.jerseyNumber !== null && player.jerseyNumber !== undefined ? player.jerseyNumber : undefined;
                      }
                      
                      // Handle stats with proper type checking
                      let games = 0, goals = 0, assists = 0;
                      if ('gamesPlayed' in player) {
                        games = player.gamesPlayed || 0;
                        goals = player.goals || 0;
                        assists = player.assists || 0;
                      }

                      // Calculate age if birth date is available
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
                          className={`table roster-player ${!isActive ? 'inactive' : ''}`}
                        >
                          <div className="roster-jersey row">
                            {jerseyNumber || '?'}
                          </div>

                          <div className="roster-player-name">
                            {playerName}
                          </div>

                          <div className="roster-age">
                            {age ?? '-'}
                          </div>

                          <div className="roster-games-played">
                            {games || 0}
                          </div>

                          <div className="roster-goals">
                            {goals || '-'}
                          </div>

                          <div className="roster-assists">
                            {assists || '-'}
                          </div>
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