import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
// import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import { floorballPlayerService, type FloorballPlayerDto } from '../../../../api/floorball/floorballPlayerService';
import { floorballTeamService } from '../../../../api/floorball/floorballTeamService';
import { FloorballPosition } from '../../../../types/floorball/floorballTypes';
import PlayersTable from './components/PlayersTable';
import ConfirmDeleteModal from './components/ConfirmDeleteModal';
import BulkStatusUpdateModal from './components/BulkStatusUpdateModal';
import AssignToTeamModal from './components/AssignToTeamModal';
import Pagination from '../../../../components/Pagination';
import SearchField from '../../../../components/SearchField';
import './FloorballPlayersPage.scss';
import BackButton from '../../../../components/BackButton/BackButton';
import Button from '../../../../components/Button/Button';
import AddIcon from '../../../../assets/basicIcons/add.svg';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';

const FloorballPlayersPage = () => {
  const { t } = useTranslation();
  // const navigate = useNavigate();
  const [players, setPlayers] = useState<FloorballPlayerDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [paginationLoading, setPaginationLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [playerToDelete, setPlayerToDelete] = useState<FloorballPlayerDto | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  const [deleteTimeoutId, setDeleteTimeoutId] = useState<ReturnType<typeof setTimeout> | null>(null);
  
  // Server pagination state
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  
  // Selection state for multiselect
  const [selectedPlayers, setSelectedPlayers] = useState<Set<string>>(new Set());
  const [isBulkDeleteModalOpen, setIsBulkDeleteModalOpen] = useState(false);
  const [isBulkDeleting, setIsBulkDeleting] = useState(false);
  
  // Bulk status update state
  const [isBulkStatusUpdateModalOpen, setIsBulkStatusUpdateModalOpen] = useState(false);
  const [bulkStatusUpdateAction, setBulkStatusUpdateAction] = useState<'activate' | 'deactivate'>('activate');
  const [isBulkStatusUpdating, setIsBulkStatusUpdating] = useState(false);

  // Assign to team state
  const [isAssignToTeamModalOpen, setIsAssignToTeamModalOpen] = useState(false);
  const [playerToAssign, setPlayerToAssign] = useState<FloorballPlayerDto | null>(null);
  const [isAssigning, setIsAssigning] = useState(false);

  // Bulk assign to team state
  const [isBulkAssignToTeamModalOpen, setIsBulkAssignToTeamModalOpen] = useState(false);
  const [isBulkAssigning, setIsBulkAssigning] = useState(false);

  // Pagination state
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  
  // Search state
  const [searchTerm, setSearchTerm] = useState('');
  const [allPlayers, setAllPlayers] = useState<FloorballPlayerDto[]>([]); // Cache for search
  


  // Function to fetch all players for search (proper chunked approach)
  const fetchAllPlayers = useCallback(async () => {
    try {
      console.log('Fetching all players for search...');
      
      let allPlayersData: FloorballPlayerDto[] = [];
      let currentPage = 1;
      let hasMoreData = true;
      
      // First, get the total count to know how many players exist
      const firstResponse = await floorballPlayerService.getAll({
        page: 1,
        pageSize: 50, // Safe page size
      });
      
      if (!firstResponse.data) {
        console.log('No data returned from first API call');
        return [];
      }
      
      // Add first batch
      allPlayersData = [...firstResponse.data];
      const totalCount = firstResponse.pagination.totalCount || 0;
      const totalPages = firstResponse.pagination.totalPages || 1;
      
      console.log(`First batch: ${firstResponse.data.length} players`);
      console.log(`Total players: ${totalCount}, Total pages: ${totalPages}`);
      
      // Fetch remaining pages if there are more
      currentPage = 2;
      while (currentPage <= totalPages && hasMoreData) {
        try {
          console.log(`Fetching page ${currentPage}/${totalPages}...`);
          
          const response = await floorballPlayerService.getAll({
            page: currentPage,
            pageSize: 50,
          });
          
          if (response.data && response.data.length > 0) {
            allPlayersData = [...allPlayersData, ...response.data];
            console.log(`Page ${currentPage}: ${response.data.length} players (total so far: ${allPlayersData.length})`);
            currentPage++;
          } else {
            console.log(`Page ${currentPage}: No more data`);
            hasMoreData = false;
          }
        } catch (pageErr) {
          console.error(`Error fetching page ${currentPage}:`, pageErr);
          hasMoreData = false;
        }
      }
      
      console.log(`Finished fetching all players: ${allPlayersData.length} total`);
      setAllPlayers(allPlayersData);
      return allPlayersData;
      
    } catch (err) {
      console.error('Failed to fetch all players for search:', err);
      
      // Fallback: try with just the first page
      try {
        console.log('Trying fallback with single page...');
        const fallbackResponse = await floorballPlayerService.getAll({
          page: 1,
          pageSize: 50,
        });
        
        if (fallbackResponse.data) {
          console.log(`Fallback fetched ${fallbackResponse.data.length} players`);
          setAllPlayers(fallbackResponse.data);
          return fallbackResponse.data;
        }
      } catch (fallbackErr) {
        console.error('Fallback also failed:', fallbackErr);
      }
      
      return [];
    }
  }, []);

  // Centralized function to fetch players with server-side pagination or client-side search
  const fetchPlayers = useCallback(async (isInitialLoad = false) => {
    try {
      if (!isInitialLoad) {
        setPaginationLoading(true);
      }
      
      if (searchTerm) {
        // Search mode: use cached players or fetch all if not cached
        let playersToSearch = allPlayers;
        if (allPlayers.length === 0) {
          playersToSearch = await fetchAllPlayers();
        }

        // Apply client-side search filtering
        console.log('=== SEARCH DEBUG INFO ===');
        console.log('Search term:', searchTerm);
        console.log('Players to search:', playersToSearch.length);
        console.log('Sample player:', playersToSearch[0]);
        console.log('All cached players count:', allPlayers.length);
        console.log('First 5 player names:', playersToSearch.slice(0, 5).map(p => 
          p.person.fullName || `${p.person.firstName} ${p.person.lastName}`
        ));
        console.log('Last 5 player names:', playersToSearch.slice(-5).map(p => 
          p.person.fullName || `${p.person.firstName} ${p.person.lastName}`
        ));
        
        const filteredPlayers = playersToSearch.filter(player => {
          // Make sure we have valid data
          if (!player || !player.person) {
            console.log('Invalid player data:', player);
            return false;
          }
          
          const searchLower = searchTerm.toLowerCase().trim();
          
          // Build full name safely
          const firstName = player.person.firstName || '';
          const lastName = player.person.lastName || '';
          const fullName = player.person.fullName || `${firstName} ${lastName}`.trim();
          
          // Check all possible matches
          const nameMatch = fullName.toLowerCase().includes(searchLower);
          const firstNameMatch = firstName.toLowerCase().includes(searchLower);
          const lastNameMatch = lastName.toLowerCase().includes(searchLower);
          
          // Position match - handle enum values safely
          const position = player.position || '';
          const positionMatch = position.toLowerCase().includes(searchLower);
          
          const matches = nameMatch || firstNameMatch || lastNameMatch || positionMatch;
          
          // Debug first few players
          if (playersToSearch.indexOf(player) < 3) {
            console.log(`Player ${player.id}:`, {
              fullName,
              firstName,
              lastName,
              position,
              searchTerm: searchLower,
              nameMatch,
              firstNameMatch,
              lastNameMatch,
              positionMatch,
              matches
            });
          }
          
          return matches;
        });
        
        console.log('Filtered players:', filteredPlayers.length);
        
        // Debug: Let's also check if a specific player exists in our cache
        // (You can modify this to search for the player you're looking for)
        const debugPlayerName = searchTerm; // Use the actual search term
        const foundInCache = playersToSearch.find(p => {
          const fullName = p.person.fullName || `${p.person.firstName} ${p.person.lastName}`;
          return fullName.toLowerCase().includes(debugPlayerName.toLowerCase());
        });
        if (debugPlayerName && debugPlayerName.length > 1) {
          console.log(`Debug: Looking for "${debugPlayerName}" in cache:`, foundInCache ? 'FOUND' : 'NOT FOUND');
          if (foundInCache) {
            console.log('Found player:', foundInCache.person.fullName || `${foundInCache.person.firstName} ${foundInCache.person.lastName}`);
          }
        }
        
        // Apply client-side pagination to filtered results
        const totalCount = filteredPlayers.length;
        const totalPages = Math.ceil(totalCount / pageSize);
        const startIndex = (currentPage - 1) * pageSize;
        const endIndex = startIndex + pageSize;
        const paginatedPlayers = filteredPlayers.slice(startIndex, endIndex);

        setPlayers(paginatedPlayers);
        setTotalCount(totalCount);
        setTotalPages(totalPages);
      } else {
        // Normal pagination mode: use server-side pagination
        const response = await floorballPlayerService.getAll({
          page: currentPage || 1,
          pageSize: pageSize || 10,
        });
        
        if (response.data) {
          setPlayers(response.data);
          setTotalCount(response.pagination.totalCount || 0);
          setTotalPages(response.pagination.totalPages || 0);
          
          // Don't fetch all players immediately - only when search is actually used
          // This prevents unnecessary API calls and validation errors
        }
      }
      setError(null);
    } catch (err) {
      console.error('fetchPlayers error:', err);
      setError(t('floorball.players.errors.loadPlayers', 'Failed to load players. Please try again.'));
    } finally {
      if (!isInitialLoad) {
        setPaginationLoading(false);
      }
    }
  }, [currentPage, pageSize, searchTerm, allPlayers, fetchAllPlayers, t]);

  // Track if this is the initial load
  const [isInitialLoad, setIsInitialLoad] = useState(true);

  // Load players when component mounts or when pagination parameters change
  useEffect(() => {
    const loadPlayers = async () => {
      try {
        if (isInitialLoad) {
          setLoading(true);
          await fetchPlayers(true);
          setIsInitialLoad(false);
        } else {
          // For pagination changes, don't show the main loading spinner
          await fetchPlayers(false);
        }
      } finally {
        if (isInitialLoad) {
          setLoading(false);
        }
      }
    };

    loadPlayers();
  }, [fetchPlayers, isInitialLoad]); // fetchPlayers includes currentPage, pageSize dependencies

  const handleDelete = (playerId: string) => {
    const player = players.find(p => p.id === playerId);
    if (!player) return;

    setPlayerToDelete(player);
    setIsDeleteModalOpen(true);
  };

  const handleConfirmDelete = async () => {
    if (!playerToDelete) return;

    try {
      setIsDeleting(true);
      setError(null);
      
      await floorballPlayerService.delete(playerToDelete.id);
      
      // Remove player from list
      setPlayers(prevPlayers => prevPlayers.filter(p => p.id !== playerToDelete.id));
      
      // Clear selection if the deleted player was selected
      setSelectedPlayers(prev => {
        const newSet = new Set(prev);
        newSet.delete(playerToDelete.id);
        return newSet;
      });
      
      // Clear any existing timeout to prevent flickering
      if (deleteTimeoutId) {
        clearTimeout(deleteTimeoutId);
      }
      
      // Clear any timeout
      if (deleteTimeoutId) {
        clearTimeout(deleteTimeoutId);
        setDeleteTimeoutId(null);
      }
      
      // Close modal
      setIsDeleteModalOpen(false);
      setPlayerToDelete(null);
    } catch (err) {
      setError(t('floorball.players.errors.deleteFailed', 'Failed to delete player. Please try again.'));
      console.error(err);
    } finally {
      setIsDeleting(false);
    }
  };

  const handleCancelDelete = () => {
    setIsDeleteModalOpen(false);
    setPlayerToDelete(null);
  };

  // const handleCreatePlayerClick = () => {
  //   navigate('/admin/floorball/players/create');
  // };

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

  const selectAllPlayers = () => {
    setSelectedPlayers(new Set(players.map(p => p.id)));
  };

  const clearSelection = () => {
    setSelectedPlayers(new Set());
  };

  const handleBulkDelete = () => {
    if (selectedPlayers.size === 0) return;
    setIsBulkDeleteModalOpen(true);
  };

  const handleConfirmBulkDelete = async () => {
    if (selectedPlayers.size === 0) return;

    try {
      setIsBulkDeleting(true);
      setError(null);
      
      // Delete each selected player
      for (const playerId of selectedPlayers) {
        await floorballPlayerService.delete(playerId);
      }
      
      // Remove deleted players from list
      setPlayers(prevPlayers => prevPlayers.filter(p => !selectedPlayers.has(p.id)));
      
      // Clear selection and close modal
      setSelectedPlayers(new Set());
      setIsBulkDeleteModalOpen(false);
      
    } catch (err) {
      setError(t('floorball.players.errors.bulkDeleteFailed', 'Failed to delete selected players. Please try again.'));
      console.error(err);
    } finally {
      setIsBulkDeleting(false);
    }
  };

  const handleCancelBulkDelete = () => {
    setIsBulkDeleteModalOpen(false);
  };

  // Bulk status update functions
  const handleBulkStatusUpdate = (action: 'activate' | 'deactivate') => {
    if (selectedPlayers.size === 0) return;
    setBulkStatusUpdateAction(action);
    setIsBulkStatusUpdateModalOpen(true);
  };

  const handleConfirmBulkStatusUpdate = async () => {
    if (selectedPlayers.size === 0) return;

    try {
      setIsBulkStatusUpdating(true);
      setError(null);
      
      const newStatus = bulkStatusUpdateAction === 'activate';
      let successfulUpdates = 0;
      
      // Update each selected player
      for (const playerId of selectedPlayers) {
        try {
          await floorballPlayerService.update(playerId, {
            isActive: newStatus
          });
          successfulUpdates++;
        } catch (err) {
          console.error(`Failed to update player ${playerId}:`, err);
          // Continue with other players even if one fails
        }
      }
      
      // Refetch all players to ensure we have the most up-to-date data
      // This guarantees that player names and all other data are correct
      await fetchPlayers();
      
      // Clear selection and close modal
      setSelectedPlayers(new Set());
      setIsBulkStatusUpdateModalOpen(false);
      
      // Show success message
      const actionText = bulkStatusUpdateAction === 'activate' ? 'activated' : 'deactivated';
      console.log(`Successfully ${actionText} ${successfulUpdates} players`);
      
    } catch (err) {
      const actionText = bulkStatusUpdateAction === 'activate' ? 'activate' : 'deactivate';
      setError(t('floorball.players.errors.bulkStatusUpdateFailed', `Failed to ${actionText} selected players. Please try again.`));
      console.error(err);
    } finally {
      setIsBulkStatusUpdating(false);
    }
  };

  const handleCancelBulkStatusUpdate = () => {
    setIsBulkStatusUpdateModalOpen(false);
  };

  // Individual status change handler
  const handleStatusChange = async (playerId: string, isActive: boolean) => {
    try {
      setError(null);
      
      await floorballPlayerService.update(playerId, {
        isActive: isActive
      });
      
      // Update the player in the local state
      setPlayers(prevPlayers => 
        prevPlayers.map(player => 
          player.id === playerId 
            ? { ...player, isActive: isActive }
            : player
        )
      );
      
    } catch (err) {
      setError(t('floorball.players.errors.statusUpdateFailed', 'Failed to update player status. Please try again.'));
      console.error(err);
    }
  };

  // Assign to team handlers
  const handleAssignToTeam = (playerId: string) => {
    const player = players.find(p => p.id === playerId);
    if (!player) return;

    setPlayerToAssign(player);
    setIsAssignToTeamModalOpen(true);
  };

  const handleConfirmAssignToTeam = async (teamId: string, position: FloorballPosition, jerseyNumber?: number) => {
    if (!playerToAssign) return;

    try {
      setIsAssigning(true);
      setError(null);

      await floorballTeamService.addPlayerToTeam(teamId, playerToAssign.id, position, jerseyNumber);

      // Refresh the players list to show updated team assignment
      await fetchPlayers();

      // Close modal and clear state
      setIsAssignToTeamModalOpen(false);
      setPlayerToAssign(null);

      console.log(`Successfully assigned player ${playerToAssign.id} to team ${teamId}`);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to assign player to team';
      setError(errorMessage);
      console.error(err);
      throw err; // Re-throw to let modal handle it
    } finally {
      setIsAssigning(false);
    }
  };

  const handleCancelAssignToTeam = () => {
    setIsAssignToTeamModalOpen(false);
    setPlayerToAssign(null);
  };

  // Bulk assign to team handlers
  const handleBulkAssignToTeam = () => {
    if (selectedPlayers.size === 0) return;
    setIsBulkAssignToTeamModalOpen(true);
  };

  const handleConfirmBulkAssignToTeam = async (teamId: string, position: FloorballPosition, jerseyNumber?: number) => {
    if (selectedPlayers.size === 0) return;

    try {
      setIsBulkAssigning(true);
      setError(null);

      // Assign each selected player to the team
      for (const playerId of selectedPlayers) {
        try {
          await floorballTeamService.addPlayerToTeam(teamId, playerId, position, jerseyNumber);
        } catch (err) {
          console.error(`Failed to assign player ${playerId} to team:`, err);
          // Continue with other players even if one fails
        }
      }

      // Refresh the players list to show updated team assignments
      await fetchPlayers();

      // Clear selection and close modal
      setSelectedPlayers(new Set());
      setIsBulkAssignToTeamModalOpen(false);

      console.log(`Successfully assigned ${selectedPlayers.size} players to team ${teamId}`);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to assign players to team';
      setError(errorMessage);
      console.error(err);
      throw err; // Re-throw to let modal handle it
    } finally {
      setIsBulkAssigning(false);
    }
  };

  const handleCancelBulkAssignToTeam = () => {
    setIsBulkAssignToTeamModalOpen(false);
  };

  // Handle page change
  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  // Handle page size change
  const handlePageSizeChange = (newPageSize: number) => {
    setPageSize(newPageSize);
    setCurrentPage(1); // Reset to first page when changing page size
  };

  // Reset to first page when search term changes
  useEffect(() => {
    setCurrentPage(1);
  }, [searchTerm]);


  // Get counts for bulk actions (based on current page data)
  const selectedPlayersData = players.filter(p => selectedPlayers.has(p.id));
  const selectedActiveCount = selectedPlayersData.filter(p => p.isActive).length;
  const selectedInactiveCount = selectedPlayersData.filter(p => !p.isActive).length;

  if (loading) {
    return (
      <PageTemplate title={t('floorball.players.title', 'Manage Floorball Players')}>
        <div className="floorball-players-loading">
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('floorball.players.title', 'MANAGE PLAYERS')}>
      <div className="floorball-players-container">

        {/* Back button */}
        <BackButton 
          to="/admin/floorball" 
          text={t('common.back', 'Back to Floorball Management')} 
        />
        <h2 className="floorball-players-title">{t('floorball.players.title', 'MANAGE PLAYERS')}</h2>
        {/* Header with actions */}

        <div className="floorball-players-header">
          <div className="players-actions">
            <SearchField
              value={searchTerm}
              onChange={setSearchTerm}
              placeholder={t('floorball.players.searchPlayers', 'Search players...')}
              fullWidth
              rounded="pill"
            />
            <Button
              className="create-player-button"
              iconLeft={AddIcon}
              to="/admin/floorball/players/create"
            >
              {t('floorball.players.createNew', 'Create New Player')}
            </Button>
            
          </div>
        </div>
        
        {/* Error message */}
        <ErrorPopup message={error} />

        {/* Selection Controls */}
          <div className="selection-controls">
            <div className="selection-info">
              <span className="selected-count">
                {t('floorball.players.selected', '{{count}} selected', { count: selectedPlayers.size })}
              </span>
              <div className="selection-buttons">
                <button
                  type="button"
                  className="control-btn"
                  onClick={clearSelection}
                >
                  {t('common.clear', 'Clear')}
                </button>
              </div>
            </div>
            
            {/* Bulk Actions */}
            <div className="bulk-actions">
              {selectedPlayers.size > 0 && (
                <button
                  type="button"
                  className="bulk-team-assign-btn"
                  onClick={handleBulkAssignToTeam}
                  disabled={isBulkAssigning}
                >
                  {t('floorball.players.actions.bulkAssignToTeam', 'Assign to Team ({{count}})', { count: selectedPlayers.size })}
                </button>
              )}
              {selectedInactiveCount > 0 && (
                <button
                  type="button"
                  className="bulk-activate-btn"
                  onClick={() => handleBulkStatusUpdate('activate')}
                  disabled={isBulkStatusUpdating}
                >
                  {t('floorball.players.actions.bulkActivate', 'Activate Selected ({{count}})', { count: selectedInactiveCount })}
                </button>
              )}
              {selectedActiveCount > 0 && (
                <button
                  type="button"
                  className="bulk-deactivate-btn"
                  onClick={() => handleBulkStatusUpdate('deactivate')}
                  disabled={isBulkStatusUpdating}
                >
                  {t('floorball.players.actions.bulkDeactivate', 'Deactivate Selected ({{count}})', { count: selectedActiveCount })}
                </button>
              )}
              {selectedPlayers.size > 0 && (
                <button
                type="button"
                className="bulk-delete-btn"
                onClick={handleBulkDelete}
                disabled={isBulkDeleting}
              >
                {t('floorball.players.actions.bulkDelete', 'Delete Selected ({{count}})', { count: selectedPlayers.size })}
              </button>
              )}
              {selectedPlayers.size == 0 && (
                <button
                type="button"
                className="dead-deletebtn"
                disabled={isBulkDeleting}
              >
                {t('floorball.players.actions.bulkDelete', 'Delete Selected ({{count}})', { count: selectedPlayers.size })}
              </button>
              )}              
            </div>
          </div>
        {/* Players table */}
        <div className={`players-table-wrapper ${paginationLoading ? 'pagination-loading' : ''}`}>
          <PlayersTable 
            players={players} 
            onDelete={handleDelete}
            onStatusChange={handleStatusChange}
            onAssignToTeam={handleAssignToTeam}
            selectedPlayers={selectedPlayers}
            onToggleSelection={togglePlayerSelection}
            onSelectAll={selectAllPlayers}
            onClearSelection={clearSelection}
          />
          {paginationLoading && (
            <div className="pagination-loading-overlay">
              <div className="loading-spinner-small"></div>
            </div>
          )}
        </div>

        {/* No data states */}
        {totalCount === 0 && !loading && (
          <div className="no-data">
            {searchTerm 
              ? t('floorball.players.noSearchResults', 'No players found matching "{{searchTerm}}"', { searchTerm })
              : t('floorball.players.noPlayers', 'No players found.')
            }
          </div>
        )}

        {/* Pagination - Bottom */}
        <Pagination
          currentPage={currentPage}
          totalPages={totalPages}
          totalCount={totalCount}
          pageSize={pageSize}
          onPageChange={handlePageChange}
          onPageSizeChange={handlePageSizeChange}
        />

        {/* Confirm Delete Modal */}
        <ConfirmDeleteModal
          isOpen={isDeleteModalOpen}
          player={playerToDelete}
          onConfirm={handleConfirmDelete}
          onCancel={handleCancelDelete}
          isDeleting={isDeleting}
        />
        
        {/* Bulk Delete Modal */}
        <ConfirmDeleteModal
          isOpen={isBulkDeleteModalOpen}
          player={null}
          onConfirm={handleConfirmBulkDelete}
          onCancel={handleCancelBulkDelete}
          isDeleting={isBulkDeleting}
          bulkCount={selectedPlayers.size}
        />

        {/* Bulk Status Update Modal */}
        <BulkStatusUpdateModal
          isOpen={isBulkStatusUpdateModalOpen}
          action={bulkStatusUpdateAction}
          selectedCount={selectedPlayers.size}
          activeCount={selectedActiveCount}
          inactiveCount={selectedInactiveCount}
          onConfirm={handleConfirmBulkStatusUpdate}
          onCancel={handleCancelBulkStatusUpdate}
          isUpdating={isBulkStatusUpdating}
        />

        {/* Assign to Team Modal */}
        <AssignToTeamModal
          isOpen={isAssignToTeamModalOpen}
          player={playerToAssign}
          onConfirm={handleConfirmAssignToTeam}
          onCancel={handleCancelAssignToTeam}
          isAssigning={isAssigning}
        />

        {/* Bulk Assign to Team Modal */}
        <AssignToTeamModal
          isOpen={isBulkAssignToTeamModalOpen}
          player={null}
          onConfirm={handleConfirmBulkAssignToTeam}
          onCancel={handleCancelBulkAssignToTeam}
          isAssigning={isBulkAssigning}
          bulkCount={selectedPlayers.size}
        />
      </div>
    </PageTemplate>
  );
};

export default FloorballPlayersPage;