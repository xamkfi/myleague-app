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

const AddPlayerToRosterPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { id: teamId } = useParams<{ id: string }>();
  
  const [loading, setLoading] = useState(true);
  const [loadingPlayers, setLoadingPlayers] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [currentTeam, setCurrentTeam] = useState<FloorballTeam | null>(null);
  const [displayedPlayers, setDisplayedPlayers] = useState<FloorballPlayerDto[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [selectedPlayers, setSelectedPlayers] = useState<Set<string>>(new Set());
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

  useEffect(() => {
    loadTeamData();
  }, [loadTeamData]);

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      const nextSearch = searchTerm.trim().length >= 2 ? searchTerm.trim() : '';
      setDebouncedSearch((previous) => {
        if (previous !== nextSearch) {
          setCurrentPage(1);
        }
        return nextSearch;
      });
    }, 300);

    return () => window.clearTimeout(timeoutId);
  }, [searchTerm]);

  useEffect(() => {
    if (!currentTeam) {
      return;
    }

    let cancelled = false;
    const controller = new AbortController();
    const teamPlayerIds = new Set(currentTeam.roster?.map((player) => player.playerId) ?? []);

    const loadPlayers = async () => {
      try {
        setLoadingPlayers(true);
        const response = await floorballPlayerService.getAll({
          page: currentPage,
          pageSize,
          searchTerm: debouncedSearch || undefined,
          signal: controller.signal,
        });

        if (cancelled) {
          return;
        }

        const availablePlayers = (response.data ?? []).filter((player) => !teamPlayerIds.has(player.id));
        setDisplayedPlayers(availablePlayers);
        setTotalPages(response.pagination.totalPages || 1);
        setError(null);
      } catch (err: unknown) {
        if (cancelled || (err instanceof DOMException && err.name === 'AbortError')) {
          return;
        }
        setError(err instanceof Error ? err.message : 'Failed to load players');
      } finally {
        if (!cancelled) {
          setLoadingPlayers(false);
        }
      }
    };

    void loadPlayers();
    return () => {
      cancelled = true;
      controller.abort();
    };
  }, [currentTeam, currentPage, pageSize, debouncedSearch]);

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
          await floorballTeamService.addPlayerToTeam(
            teamId,
            playerId,
            FloorballPosition.None,
            undefined
          );
        } catch (err) {
          console.error(`Failed to add player ${playerId}:`, err);
          // Continue with other players even if one fails
        }
      }
      
      // Navigate back to roster page after successful add
      navigate(`/admin/floorball/teams/${teamId}/roster`);
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

        {/* Selection Controls */}
        <div className="selection-controls">
          <div className="selection-info">
            <span className="selected-count">
              {t('floorball.teams.selectedPlayers', '{{count}} selected', { count: selectedPlayers.size })}
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
                : t('floorball.teams.addSelectedToTeam', 'Add Selected to Team ({{count}})', { count: selectedPlayers.size })
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
                      title={t('floorball.teams.selectAllOnPage', 'Select all on this page')}
                    />
                  </th>
                  <th className="name-column">{t('floorball.players.name', 'NAME')}</th>
                  <th className="position-column">{t('floorball.players.position', 'POSITION')}</th>
                  <th className="team-column">{t('floorball.players.currentTeam', 'CURRENT TEAM')}</th>
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
                          {player.team?.name || t('floorball.players.noTeam', 'No team')}
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
