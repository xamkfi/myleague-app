import { useState, useEffect, useCallback } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import { footballTeamService } from '../../../../api/football/footballTeamService';
import { footballPlayerService, type FootballPlayerDto } from '../../../../api/football/footballPlayerService';
import { 
  FootballPosition,
  type FootballTeam
} from '../../../../types/football/footballTypes';
import SearchField from '../../../../components/SearchField';
import Button from '../../../../components/Button/Button';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import './AddPlayerToRosterPage.scss';

const AddPlayerToRosterPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { id: teamId } = useParams<{ id: string }>();
  
  const [loading, setLoading] = useState(true);
  const [loadingPlayers, setLoadingPlayers] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [currentTeam, setCurrentTeam] = useState<FootballTeam | null>(null);
  const [allPlayers, setAllPlayers] = useState<FootballPlayerDto[]>([]);
  const [displayedPlayers, setDisplayedPlayers] = useState<FootballPlayerDto[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedPlayers, setSelectedPlayers] = useState<Set<string>>(new Set());
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const pageSize = 10;

  // Load team data
  const loadTeamData = useCallback(async () => {
    if (!teamId) return;
    
    try {
      setLoading(true);
      const team = await footballTeamService.getById(teamId);
      setCurrentTeam(team);
      setError(null);
    } catch (err) {
      console.error('Error loading team data:', err);
      setError(err instanceof Error ? err.message : 'Failed to load team data');
    } finally {
      setLoading(false);
    }
  }, [teamId]);

  // Fetch ALL players (chunked approach like FootballPlayersPage)
  const fetchAllPlayers = useCallback(async (team: FootballTeam) => {
    try {
      setLoadingPlayers(true);
      
      let allPlayersData: FootballPlayerDto[] = [];
      let currentFetchPage = 1;
      let hasMoreData = true;
      
      // First, get the total count
      const firstResponse = await footballPlayerService.getAll({
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
          const response = await footballPlayerService.getAll({
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
  const updateDisplayedPlayers = useCallback((players: FootballPlayerDto[], search: string, page: number) => {
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
      setDisplayedPlayers([]);
      setTotalPages(1);
    }
  }, [allPlayers, searchTerm, currentPage, updateDisplayedPlayers, loadingPlayers, currentTeam]);

  // Reset to page 1 when search changes
  useEffect(() => {
    setCurrentPage(1);
  }, [searchTerm]);

  // Selection management functions
  const togglePlayerSelection = (playerId: string) => {
    setSelectedPlayers(prev => {
      const newSet = new Set(prev);
      if (newSet.has(playerId)) {
        newSet.delete(playerId);
      } else {
        newSet.add(playerId);
      }
      return newSet;
    });
  };

  const selectAllOnPage = () => {
    setSelectedPlayers(prev => {
      const newSet = new Set(prev);
      displayedPlayers.forEach(player => newSet.add(player.id));
      return newSet;
    });
  };

  const clearSelection = () => {
    setSelectedPlayers(new Set());
  };

  const isAllOnPageSelected = displayedPlayers.length > 0 && 
    displayedPlayers.every(player => selectedPlayers.has(player.id));

  // Handle add selected players to team
  const handleAddSelectedPlayers = async () => {
    if (!teamId || selectedPlayers.size === 0) return;
    
    try {
      setSaving(true);
      setError(null);
      
      // Add each selected player with position None and no jersey number
      for (const playerId of selectedPlayers) {
        try {
          await footballTeamService.addPlayerToTeam(
            teamId,
            playerId,
            FootballPosition.None,
            undefined
          );
        } catch (err) {
          console.error(`Failed to add player ${playerId}:`, err);
          // Continue with other players even if one fails
        }
      }
      
      // Navigate back to roster page after successful add
      navigate(`/admin/football/teams/${teamId}/roster`);
    } catch (err) {
      console.error('Error adding players to team:', err);
      setError(err instanceof Error ? err.message : 'Failed to add players to team');
      setSaving(false);
    }
  };

  // Handle page change
  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  // Get position display name
  const getPositionDisplay = (position: FootballPosition | string): string => {
    const positionMap: Record<string, string> = {
      [FootballPosition.Goalkeeper]: t('football.positions.goalkeeper', 'Goalkeeper'),
      [FootballPosition.Defender]: t('football.positions.defender', 'Defender'),
      [FootballPosition.Midfielder]: t('football.positions.midfielder', 'Midfielder'),
      [FootballPosition.Forward]: t('football.positions.forward', 'Forward'),
      [FootballPosition.None]: t('football.positions.none', 'None'),
    };
    return positionMap[position] || position || t('football.positions.none', 'None');
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
      <PageTemplate title={t('football.teams.addPlayer', 'Add Player')}>
        <ErrorPopup message={error || 'Team not found'} />
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={`${t('football.teams.addPlayerToTeam', 'Add Player to Team')} - ${currentTeam.name}`}>
      <div className="add-player-roster-container">
        <h2 className="add-player-roster-title">
          {t('football.teams.addPlayerToTeam', 'ADD PLAYER TO TEAM')}
        </h2>
        
        <div className="team-info-header">
          <span className="team-name">{currentTeam.name}</span>
        </div>

        <ErrorPopup message={error} />

        <div className="add-player-roster-header">
          <div className="search-section">
            <SearchField
              value={searchTerm}
              onChange={setSearchTerm}
              placeholder={t('football.teams.searchAvailablePlayers', 'Search available players...')}
              fullWidth
              rounded="pill"
            />
          </div>
        </div>

        {/* Selection Controls */}
        <div className="selection-controls">
          <div className="selection-info">
            <span className="selected-count">
              {t('football.teams.selectedPlayers', '{{count}} selected', { count: selectedPlayers.size })}
            </span>
            {selectedPlayers.size > 0 && (
              <button
                type="button"
                className="clear-selection-btn"
                onClick={clearSelection}
              >
                {t('common.clearSelection', 'Clear Selection')}
              </button>
            )}
          </div>
          
          <div className="selection-actions">
            <Button
              variant="primary"
              onClick={handleAddSelectedPlayers}
              disabled={selectedPlayers.size === 0 || saving}
            >
              {saving 
                ? t('common.saving', 'Saving...') 
                : t('football.teams.addSelectedToTeam', 'Add Selected to Team ({{count}})', { count: selectedPlayers.size })
              }
            </Button>
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
                  <th className="select-column">
                    <input
                      type="checkbox"
                      checked={isAllOnPageSelected}
                      onChange={(e) => {
                        if (e.target.checked) {
                          selectAllOnPage();
                        } else {
                          // Deselect only players on current page
                          setSelectedPlayers(prev => {
                            const newSet = new Set(prev);
                            displayedPlayers.forEach(player => newSet.delete(player.id));
                            return newSet;
                          });
                        }
                      }}
                      title={t('football.teams.selectAllOnPage', 'Select all on this page')}
                    />
                  </th>
                  <th className="name-column">{t('football.players.name', 'NAME')}</th>
                  <th className="position-column">{t('football.players.position', 'POSITION')}</th>
                  <th className="team-column">{t('football.players.currentTeam', 'CURRENT TEAM')}</th>
                </tr>
              </thead>
              <tbody>
                {displayedPlayers.length === 0 ? (
                  <tr>
                    <td colSpan={4} className="no-players">
                      {searchTerm 
                        ? t('football.teams.noPlayersFoundSearch', 'No players found matching your search')
                        : t('football.teams.noAvailablePlayers', 'No available players found')
                      }
                    </td>
                  </tr>
                ) : (
                  displayedPlayers.map((player) => (
                    <tr 
                      key={player.id}
                      className={`clickable-row${selectedPlayers.has(player.id) ? ' selected' : ''}`}
                      onClick={() => togglePlayerSelection(player.id)}
                    >
                      <td className="select-column">
                        <input
                          type="checkbox"
                          checked={selectedPlayers.has(player.id)}
                          onChange={() => togglePlayerSelection(player.id)}
                          onClick={(e) => e.stopPropagation()}
                        />
                      </td>
                      <td className="name-column">
                        <span className="player-name">{player.person.fullName}</span>
                      </td>
                      <td className="position-column">
                        <span className="position">{getPositionDisplay(player.position)}</span>
                      </td>
                      <td className="team-column">
                        <span className="team-name">
                          {player.team?.name || t('football.players.noTeam', 'No team')}
                        </span>
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
      </div>
    </PageTemplate>
  );
};

export default AddPlayerToRosterPage;
