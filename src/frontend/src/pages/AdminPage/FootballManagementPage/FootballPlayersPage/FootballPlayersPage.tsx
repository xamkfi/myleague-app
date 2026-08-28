import { useState, useEffect, useCallback, useRef } from 'react';
import { useTranslation } from 'react-i18next';
// import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import { footballPlayerService, type FootballPlayerDto } from '../../../../api/football/footballPlayerService';
import { footballTeamService } from '../../../../api/football/footballTeamService';
import { FootballPosition } from '../../../../types/football/footballTypes';
import PlayersTable from './components/PlayersTable';
import ConfirmDeleteModal from './components/ConfirmDeleteModal';
import BulkStatusUpdateModal from './components/BulkStatusUpdateModal';
import AssignToTeamModal from './components/AssignToTeamModal';
import Pagination from '../../../../components/Pagination';
import SearchField from '../../../../components/SearchField';
import BulkActionsBar from '../../../../components/BulkActionsBar/BulkActionsBar';
import '../../../../styles/AdminTable.scss';
import './FootballPlayersPage.scss';
import Button from '../../../../components/Button/Button';
import AddIcon from '../../../../assets/basicIcons/add.svg';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import { mapDeletionError } from '../../../../utils/mapDeletionError';

const FootballPlayersPage = () => {
  const { t } = useTranslation();
  // const navigate = useNavigate();
  const [players, setPlayers] = useState<FootballPlayerDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [paginationLoading, setPaginationLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [playerToDelete, setPlayerToDelete] = useState<FootballPlayerDto | null>(null);
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
  const [playerToAssign, setPlayerToAssign] = useState<FootballPlayerDto | null>(null);
  const [isAssigning, setIsAssigning] = useState(false);

  // Bulk assign to team state
  const [isBulkAssignToTeamModalOpen, setIsBulkAssignToTeamModalOpen] = useState(false);
  const [isBulkAssigning, setIsBulkAssigning] = useState(false);

  // Pagination state
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  
  // Search state
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [reloadToken, setReloadToken] = useState(0);
  const hasLoadedOnce = useRef(false);

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

  const fetchPlayers = useCallback(() => {
    setReloadToken((token) => token + 1);
  }, []);

  useEffect(() => {
    let cancelled = false;
    const controller = new AbortController();
    const isFirstLoad = !hasLoadedOnce.current;

    const loadPlayers = async () => {
      if (isFirstLoad) {
        setLoading(true);
      } else {
        setPaginationLoading(true);
      }

      try {
        const response = await footballPlayerService.getAll({
          page: currentPage || 1,
          pageSize: pageSize || 10,
          searchTerm: debouncedSearch || undefined,
          signal: controller.signal,
        });

        if (cancelled) {
          return;
        }

        if (response.data) {
          setPlayers(response.data);
          setTotalCount(response.pagination.totalCount || 0);
          setTotalPages(response.pagination.totalPages || 0);
        }
        setError(null);
        hasLoadedOnce.current = true;
      } catch (err: unknown) {
        if (cancelled || (err instanceof DOMException && err.name === 'AbortError')) {
          return;
        }
        setError(t('football.players.errors.loadPlayers', 'Failed to load players. Please try again.'));
      } finally {
        if (!cancelled) {
          setLoading(false);
          setPaginationLoading(false);
        }
      }
    };

    void loadPlayers();

    return () => {
      cancelled = true;
      controller.abort();
    };
  }, [currentPage, pageSize, debouncedSearch, reloadToken, t]);

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
      
      await footballPlayerService.delete(playerToDelete.id);
      
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
      setError(
        mapDeletionError(err, t) ??
          t('football.players.errors.deleteFailed', 'Failed to delete player. Please try again.'),
      );
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
  //   navigate('/admin/football/players/create');
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
        await footballPlayerService.delete(playerId);
      }
      
      // Remove deleted players from list
      setPlayers(prevPlayers => prevPlayers.filter(p => !selectedPlayers.has(p.id)));
      
      // Clear selection and close modal
      setSelectedPlayers(new Set());
      setIsBulkDeleteModalOpen(false);
      
    } catch (err) {
      setError(t('football.players.errors.bulkDeleteFailed', 'Failed to delete selected players. Please try again.'));
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

      for (const playerId of selectedPlayers) {
        try {
          await footballPlayerService.update(playerId, {
            isActive: newStatus
          });
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
      
    } catch (err) {
      const actionText = bulkStatusUpdateAction === 'activate' ? 'activate' : 'deactivate';
      setError(t('football.players.errors.bulkStatusUpdateFailed', `Failed to ${actionText} selected players. Please try again.`));
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
      
      await footballPlayerService.update(playerId, {
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
      setError(t('football.players.errors.statusUpdateFailed', 'Failed to update player status. Please try again.'));
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

  const handleConfirmAssignToTeam = async (teamId: string, position: FootballPosition, jerseyNumber?: number) => {
    if (!playerToAssign) return;

    try {
      setIsAssigning(true);
      setError(null);

      await footballTeamService.addPlayerToTeam(teamId, playerToAssign.id, position, jerseyNumber);

      // Refresh the players list to show updated team assignment
      await fetchPlayers();

      // Close modal and clear state
      setIsAssignToTeamModalOpen(false);
      setPlayerToAssign(null);
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

  const handleConfirmBulkAssignToTeam = async (teamId: string, position: FootballPosition, jerseyNumber?: number) => {
    if (selectedPlayers.size === 0) return;

    try {
      setIsBulkAssigning(true);
      setError(null);

      // Assign each selected player to the team
      for (const playerId of selectedPlayers) {
        try {
          await footballTeamService.addPlayerToTeam(teamId, playerId, position, jerseyNumber);
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

  // Get counts for bulk actions (based on current page data)
  const selectedPlayersData = players.filter(p => selectedPlayers.has(p.id));
  const selectedActiveCount = selectedPlayersData.filter(p => p.isActive).length;
  const selectedInactiveCount = selectedPlayersData.filter(p => !p.isActive).length;

  if (loading) {
    return (
      <PageTemplate title={t('football.players.title', 'Manage Football Players')}>
        <div className="football-players-loading">
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('football.players.title', 'MANAGE PLAYERS')}>
      <div className="football-players-container">

        <h2 className="football-players-title">{t('football.players.title', 'MANAGE PLAYERS')}</h2>
        {/* Header with actions */}

        <div className="football-players-header">
          <div className="players-actions">
            <SearchField
              value={searchTerm}
              onChange={setSearchTerm}
              placeholder={t('football.players.searchPlayers', 'Search players...')}
              fullWidth
              rounded="pill"
            />
            <Button
              className="create-player-button"
              iconLeft={AddIcon}
              to="/admin/football/players/create"
            >
              {t('football.players.createNew', 'Create New Player')}
            </Button>
            
          </div>
        </div>
        
        {/* Error message */}
        <ErrorPopup message={error} />

        {/* Bulk Actions Bar */}
        <BulkActionsBar
          selectedCount={selectedPlayers.size}
          totalCount={players.length}
          onSelectAll={selectAllPlayers}
          onClearSelection={clearSelection}
          actions={[
            {
              label: t('football.players.actions.bulkAssignToTeam', 'Assign to Team ({{count}})', { count: selectedPlayers.size }),
              onClick: handleBulkAssignToTeam,
              variant: 'default',
              disabled: isBulkAssigning,
            },
            {
              label: t('football.players.actions.bulkActivate', 'Activate ({{count}})', { count: selectedInactiveCount }),
              onClick: () => handleBulkStatusUpdate('activate'),
              variant: 'status',
              disabled: isBulkStatusUpdating || selectedInactiveCount === 0,
            },
            {
              label: t('football.players.actions.bulkDeactivate', 'Deactivate ({{count}})', { count: selectedActiveCount }),
              onClick: () => handleBulkStatusUpdate('deactivate'),
              variant: 'status',
              disabled: isBulkStatusUpdating || selectedActiveCount === 0,
            },
            {
              label: t('football.players.actions.bulkDelete', 'Delete ({{count}})', { count: selectedPlayers.size }),
              onClick: handleBulkDelete,
              variant: 'danger',
              disabled: isBulkDeleting,
            },
          ]}
        />
        {/* Players table */}
        <div className={`admin-table__wrapper ${paginationLoading ? 'pagination-loading' : ''}`}>
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
              ? t('football.players.noSearchResults', 'No players found matching "{{searchTerm}}"', { searchTerm })
              : t('football.players.noPlayers', 'No players found.')
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

export default FootballPlayersPage;