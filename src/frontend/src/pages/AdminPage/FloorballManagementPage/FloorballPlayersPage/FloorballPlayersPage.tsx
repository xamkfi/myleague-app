import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/PageTemplate';
import { floorballPlayerService, type FloorballPlayerDto } from '../../../../api/floorball/floorballPlayerService';
import PlayersTable from './components/PlayersTable';
import ConfirmDeleteModal from './components/ConfirmDeleteModal';
import BulkStatusUpdateModal from './components/BulkStatusUpdateModal';
import PaginationControls from './components/PaginationControls';
import './FloorballPlayersPage.scss';
import BackButton from '../../../../components/BackButton/BackButton';

const FloorballPlayersPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [players, setPlayers] = useState<FloorballPlayerDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [paginationLoading, setPaginationLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [playerToDelete, setPlayerToDelete] = useState<FloorballPlayerDto | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  const [deleteTimeoutId, setDeleteTimeoutId] = useState<number | null>(null);
  
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

  // Pagination state
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  


  // Centralized function to fetch players with server-side pagination
  const fetchPlayers = useCallback(async (isInitialLoad = false) => {
    try {
      if (!isInitialLoad) {
        setPaginationLoading(true);
      }
      
      const response = await floorballPlayerService.getAll({
        page: currentPage || 1,
        pageSize: pageSize || 10,
      });
      
      if (response.data) {
        setPlayers(response.data);
        setTotalCount(response.pagination.totalCount || 0);
        setTotalPages(response.pagination.totalPages || 0);
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
  }, [currentPage, pageSize, t]);

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

  const handleCreatePlayerClick = () => {
    navigate('/admin/floorball/players/create');
  };

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

  // Handle page change
  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  // Handle page size change
  const handlePageSizeChange = (newPageSize: number) => {
    setPageSize(newPageSize);
    setCurrentPage(1); // Reset to first page when changing page size
  };


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
    <PageTemplate title={t('floorball.players.title', 'Manage Floorball Players')}>      
      <div className="floorball-players-container">

        {/* Back button */}
        <BackButton 
          to="/admin/floorball" 
          text={t('common.back', 'Back to Floorball Management')} 
        />
        
        {/* Header with actions */}
        <div className="floorball-players-header">
          <div className="players-count">
            <span>{t('floorball.players.totalCount', `${totalCount} players`, { count: totalCount })}</span>
          </div>
          <div className="players-actions">
            <button className="create-player-button" onClick={handleCreatePlayerClick}>
              {t('floorball.players.createNew', 'Create New Player')}
            </button>
          </div>
        </div>

        
        {/* Selection Controls */}
        <div className="selection-controls">
          <div className="selection-info">
            <span className="selected-count">
              {t('floorball.players.selected', '{{count}} selected', { count: selectedPlayers.size })}
            </span>
            {players.length > 0 && (
              <div className="selection-buttons">
                <button
                  type="button"
                  className="control-btn"
                  onClick={selectAllPlayers}
                  disabled={selectedPlayers.size === players.length}
                >
                  {t('common.selectAll', 'Select All on Page')} ({players.length})
                </button>
                <button
                  type="button"
                  className="control-btn"
                  onClick={clearSelection}
                  disabled={selectedPlayers.size === 0}
                >
                  {t('common.clear', 'Clear')}
                </button>
              </div>
            )}
          </div>
          
          {/* Bulk Actions */}
          {selectedPlayers.size > 0 && (
            <div className="bulk-actions">
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
              <button
                type="button"
                className="bulk-delete-btn"
                onClick={handleBulkDelete}
                disabled={isBulkDeleting}
              >
                {t('floorball.players.actions.bulkDelete', 'Delete Selected ({{count}})', { count: selectedPlayers.size })}
              </button>
            </div>
          )}
        </div>
        
        {/* Error message */}
        {error && (
          <div className="error-message">
            <p>{error}</p>
          </div>
        )}
        
        {/* Players table */}
        <div className={`players-table-container ${paginationLoading ? 'pagination-loading' : ''}`}>
          <PlayersTable 
            players={players} 
            onDelete={handleDelete}
            onStatusChange={handleStatusChange}
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

        {/* Pagination Controls - Bottom */}
        <PaginationControls
          currentPage={currentPage}
          totalPages={totalPages}
          totalCount={totalCount}
          pageSize={pageSize}
          onPageChange={handlePageChange}
          onPageSizeChange={handlePageSizeChange}
        />

        {/* No data states */}
        {totalCount === 0 && !loading && (
          <div className="no-data">
            {t('floorball.players.noPlayers', 'No players found.')}
          </div>
        )}

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
      </div>
    </PageTemplate>
  );
};

export default FloorballPlayersPage;