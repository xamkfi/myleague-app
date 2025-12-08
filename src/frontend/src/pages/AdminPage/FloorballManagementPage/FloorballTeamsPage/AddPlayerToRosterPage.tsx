import { useState, useEffect, useCallback } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import { floorballTeamService } from '../../../../api/floorball/floorballTeamService';
import { floorballPlayerService, type FloorballPlayerDto } from '../../../../api/floorball/floorballPlayerService';
import { 
  FloorballPosition,
  type FloorballTeam
} from '../../../../types/floorball/floorballTypes';
import SearchField from '../../../../components/SearchField';
import Button from '../../../../components/Button/Button';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import './AddPlayerToRosterPage.scss';

interface SelectedPlayer {
  player: FloorballPlayerDto;
  position: FloorballPosition;
  jerseyNumber: string;
}

const AddPlayerToRosterPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { id: teamId } = useParams<{ id: string }>();
  
  const [loading, setLoading] = useState(true);
  const [loadingPlayers, setLoadingPlayers] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [currentTeam, setCurrentTeam] = useState<FloorballTeam | null>(null);
  const [allPlayers, setAllPlayers] = useState<FloorballPlayerDto[]>([]); // Cache for all players
  const [displayedPlayers, setDisplayedPlayers] = useState<FloorballPlayerDto[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedPlayer, setSelectedPlayer] = useState<SelectedPlayer | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const pageSize = 10;

  // Load team data
  const loadTeamData = useCallback(async () => {
    if (!teamId) return;
    
    try {
      setLoading(true);
      const team = await floorballTeamService.getById(teamId);
      setCurrentTeam(team);
      setError(null);
    } catch (err) {
      console.error('Error loading team data:', err);
      setError(err instanceof Error ? err.message : 'Failed to load team data');
    } finally {
      setLoading(false);
    }
  }, [teamId]);

  // Fetch ALL players (chunked approach like FloorballPlayersPage)
  const fetchAllPlayers = useCallback(async (team: FloorballTeam) => {
    try {
      setLoadingPlayers(true);
      
      let allPlayersData: FloorballPlayerDto[] = [];
      let currentFetchPage = 1;
      let hasMoreData = true;
      
      // First, get the total count
      const firstResponse = await floorballPlayerService.getAll({
        page: 1,
        pageSize: 50,
      });
      
      if (!firstResponse.data) {
        setAllPlayers([]);
        return [];
      }
      
      allPlayersData = [...firstResponse.data];
      const fetchTotalPages = firstResponse.pagination.totalPages || 1;
      
      // Fetch remaining pages
      currentFetchPage = 2;
      while (currentFetchPage <= fetchTotalPages && hasMoreData) {
        try {
          const response = await floorballPlayerService.getAll({
            page: currentFetchPage,
            pageSize: 50,
          });
          
          if (response.data && response.data.length > 0) {
            allPlayersData = [...allPlayersData, ...response.data];
            currentFetchPage++;
          } else {
            hasMoreData = false;
          }
        } catch (pageErr) {
          console.error(`Error fetching page ${currentFetchPage}:`, pageErr);
          hasMoreData = false;
        }
      }
      
      // Filter out players already in the team
      const teamPlayerIds = new Set(team.roster?.map(p => p.playerId) || []);
      const availablePlayers = allPlayersData.filter(player => !teamPlayerIds.has(player.id));
      
      setAllPlayers(availablePlayers);
      setError(null);
      return availablePlayers;
      
    } catch (err) {
      console.error('Error loading players:', err);
      setError(err instanceof Error ? err.message : 'Failed to load players');
      return [];
    } finally {
      setLoadingPlayers(false);
    }
  }, []);

  // Apply search and pagination to display players
  const updateDisplayedPlayers = useCallback((players: FloorballPlayerDto[], search: string, page: number) => {
    let filtered = players;
    
    // Apply search filter
    if (search) {
      const searchLower = search.toLowerCase().trim();
      filtered = players.filter(player => {
        if (!player || !player.person) return false;
        
        const firstName = player.person.firstName || '';
        const lastName = player.person.lastName || '';
        const fullName = player.person.fullName || `${firstName} ${lastName}`.trim();
        
        return fullName.toLowerCase().includes(searchLower) ||
               firstName.toLowerCase().includes(searchLower) ||
               lastName.toLowerCase().includes(searchLower);
      });
    }
    
    // Calculate pagination
    const totalCount = filtered.length;
    const calculatedTotalPages = Math.ceil(totalCount / pageSize) || 1;
    const startIndex = (page - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const paginatedPlayers = filtered.slice(startIndex, endIndex);
    
    setDisplayedPlayers(paginatedPlayers);
    setTotalPages(calculatedTotalPages);
  }, [pageSize]);

  useEffect(() => {
    loadTeamData();
  }, [loadTeamData]);

  // Load all players when team is loaded
  useEffect(() => {
    if (currentTeam) {
      fetchAllPlayers(currentTeam);
    }
  }, [currentTeam, fetchAllPlayers]);

  // Update displayed players when search or pagination changes
  useEffect(() => {
    if (allPlayers.length > 0) {
      updateDisplayedPlayers(allPlayers, searchTerm, currentPage);
    } else if (!loadingPlayers && currentTeam) {
      // No players available
      setDisplayedPlayers([]);
      setTotalPages(1);
    }
  }, [allPlayers, searchTerm, currentPage, updateDisplayedPlayers, loadingPlayers, currentTeam]);

  // Reset to page 1 when search changes
  useEffect(() => {
    setCurrentPage(1);
  }, [searchTerm]);

  // Handle player selection
  const handleSelectPlayer = (player: FloorballPlayerDto) => {
    setSelectedPlayer({
      player,
      position: player.position || FloorballPosition.None,
      jerseyNumber: ''
    });
  };

  // Handle position change
  const handlePositionChange = (position: FloorballPosition) => {
    if (selectedPlayer) {
      setSelectedPlayer({
        ...selectedPlayer,
        position
      });
    }
  };

  // Handle jersey number change
  const handleJerseyNumberChange = (value: string) => {
    if (selectedPlayer) {
      // Only allow numbers
      const numericValue = value.replace(/\D/g, '');
      setSelectedPlayer({
        ...selectedPlayer,
        jerseyNumber: numericValue
      });
    }
  };

  // Handle add player to team
  const handleAddPlayer = async () => {
    if (!teamId || !selectedPlayer) return;
    
    try {
      setSaving(true);
      setError(null);
      
      const jerseyNumber = selectedPlayer.jerseyNumber 
        ? parseInt(selectedPlayer.jerseyNumber, 10) 
        : undefined;
      
      await floorballTeamService.addPlayerToTeam(
        teamId,
        selectedPlayer.player.id,
        selectedPlayer.position,
        jerseyNumber
      );
      
      // Navigate back to roster page after successful add
      navigate(`/admin/floorball/teams/${teamId}/roster`);
    } catch (err) {
      console.error('Error adding player to team:', err);
      setError(err instanceof Error ? err.message : 'Failed to add player to team');
      setSaving(false);
    }
  };

  // Handle cancel selection
  const handleCancelSelection = () => {
    setSelectedPlayer(null);
  };

  // Handle page change
  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  // Get position display name
  const getPositionDisplay = (position: FloorballPosition | string): string => {
    const positionMap: Record<string, string> = {
      [FloorballPosition.Goalkeeper]: t('floorball.positions.goalkeeper', 'Goalkeeper'),
      [FloorballPosition.Defender]: t('floorball.positions.defender', 'Defender'),
      [FloorballPosition.Forward]: t('floorball.positions.forward', 'Forward'),
      [FloorballPosition.None]: t('floorball.positions.none', 'None'),
    };
    return positionMap[position] || position || t('floorball.positions.none', 'None');
  };

  if (loading) {
    return (
      <PageTemplate title={t('common.loading', 'Loading...')}>
        <div className="add-player-roster-loading">
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      </PageTemplate>
    );
  }

  if (!teamId || !currentTeam) {
    return (
      <PageTemplate title={t('floorball.teams.addPlayer', 'Add Player')}>
        <ErrorPopup message={error || 'Team not found'} />
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={`${t('floorball.teams.addPlayerToTeam', 'Add Player to Team')} - ${currentTeam.name}`}>
      <div className="add-player-roster-container">
        <h2 className="add-player-roster-title">
          {t('floorball.teams.addPlayerToTeam', 'ADD PLAYER TO TEAM')}
        </h2>
        
        <div className="team-info-header">
          <span className="team-name">{currentTeam.name}</span>
        </div>

        <ErrorPopup message={error} />

        {selectedPlayer ? (
          // Player selection form
          <div className="player-selection-form">
            <h3 className="selection-title">
              {t('floorball.teams.configurePlayer', 'Configure Player')}
            </h3>
            
            <div className="selected-player-info">
              <span className="player-name-label">{t('floorball.players.name', 'Name')}:</span>
              <span className="player-name-value">{selectedPlayer.player.person.fullName}</span>
            </div>

            <div className="form-group">
              <label htmlFor="position">{t('floorball.players.position', 'Position')}</label>
              <select
                id="position"
                value={selectedPlayer.position}
                onChange={(e) => handlePositionChange(e.target.value as FloorballPosition)}
              >
                <option value={FloorballPosition.None}>{getPositionDisplay(FloorballPosition.None)}</option>
                <option value={FloorballPosition.Goalkeeper}>{getPositionDisplay(FloorballPosition.Goalkeeper)}</option>
                <option value={FloorballPosition.Defender}>{getPositionDisplay(FloorballPosition.Defender)}</option>
                <option value={FloorballPosition.Forward}>{getPositionDisplay(FloorballPosition.Forward)}</option>
              </select>
            </div>

            <div className="form-group">
              <label htmlFor="jerseyNumber">{t('floorball.players.jerseyNumber', 'Jersey Number')}</label>
              <input
                type="text"
                id="jerseyNumber"
                value={selectedPlayer.jerseyNumber}
                onChange={(e) => handleJerseyNumberChange(e.target.value)}
                placeholder={t('floorball.players.jerseyNumberPlaceholder', 'Enter jersey number (optional)')}
                maxLength={3}
              />
            </div>

            <div className="form-actions">
              <Button
                variant="secondary"
                onClick={handleCancelSelection}
                disabled={saving}
              >
                {t('common.cancel', 'Cancel')}
              </Button>
              <Button
                variant="primary"
                onClick={handleAddPlayer}
                disabled={saving}
              >
                {saving 
                  ? t('common.saving', 'Saving...') 
                  : t('floorball.teams.addToTeam', 'Add to Team')
                }
              </Button>
            </div>
          </div>
        ) : (
          // Player list
          <>
            <div className="add-player-roster-header">
              <div className="search-section">
                <SearchField
                  value={searchTerm}
                  onChange={setSearchTerm}
                  placeholder={t('floorball.teams.searchAvailablePlayers', 'Search available players...')}
                  fullWidth
                  rounded="pill"
                />
              </div>
            </div>

            <div className="players-table-wrapper">
              {loadingPlayers ? (
                <div className="loading-players">
                  <p>{t('common.loading', 'Loading...')}</p>
                </div>
              ) : (
                <table className="players-table">
                  <thead>
                    <tr>
                      <th className="name-column">{t('floorball.players.name', 'NAME')}</th>
                      <th className="position-column">{t('floorball.players.position', 'POSITION')}</th>
                      <th className="team-column">{t('floorball.players.currentTeam', 'CURRENT TEAM')}</th>
                      <th className="actions-column">{t('common.actions', 'ACTIONS')}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {displayedPlayers.length === 0 ? (
                      <tr>
                        <td colSpan={4} className="no-players">
                          {searchTerm 
                            ? t('floorball.teams.noPlayersFoundSearch', 'No players found matching your search')
                            : t('floorball.teams.noAvailablePlayers', 'No available players found')
                          }
                        </td>
                      </tr>
                    ) : (
                      displayedPlayers.map((player) => (
                        <tr key={player.id}>
                          <td className="name-column">
                            <span className="player-name">{player.person.fullName}</span>
                          </td>
                          <td className="position-column">
                            <span className="position">{getPositionDisplay(player.position)}</span>
                          </td>
                          <td className="team-column">
                            <span className="team-name">
                              {player.team?.name || t('floorball.players.noTeam', 'No team')}
                            </span>
                          </td>
                          <td className="actions-column">
                            <Button
                              variant="primary"
                              size="sm"
                              onClick={() => handleSelectPlayer(player)}
                            >
                              {t('common.select', 'Select')}
                            </Button>
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </table>
              )}
            </div>

            {/* Pagination */}
            {totalPages > 1 && (
              <div className="pagination">
                <Button
                  variant="secondary"
                  size="sm"
                  onClick={() => handlePageChange(currentPage - 1)}
                  disabled={currentPage === 1}
                >
                  {t('common.previous', 'Previous')}
                </Button>
                <span className="page-info">
                  {t('common.pageOf', 'Page {{current}} of {{total}}', { 
                    current: currentPage, 
                    total: totalPages 
                  })}
                </span>
                <Button
                  variant="secondary"
                  size="sm"
                  onClick={() => handlePageChange(currentPage + 1)}
                  disabled={currentPage === totalPages}
                >
                  {t('common.next', 'Next')}
                </Button>
              </div>
            )}
          </>
        )}
      </div>
    </PageTemplate>
  );
};

export default AddPlayerToRosterPage;
