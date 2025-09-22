import { useState, useEffect, useDeferredValue } from 'react';
import { useTranslation } from 'react-i18next';
// import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/PageTemplate';
import { floorballPlayerService, type FloorballPlayerDto } from '../../../../api/floorball/floorballPlayerService';
import PlayersTable from './components/PlayersTable';
import ConfirmDeleteModal from './components/ConfirmDeleteModal';
import './FloorballPlayersPage.scss';
import BackButton from '../../../../components/BackButton/BackButton';
import Button from '../../../../components/Button/Button';
import AddIcon from '../../../../assets/basicIcons/add.svg';
import SearchIcon from '../../../../assets/basicIcons/search.svg';

const FloorballPlayersPage = () => {
  const { t } = useTranslation();
  // const navigate = useNavigate();
  const [players, setPlayers] = useState<FloorballPlayerDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(10);
  const [totalPages, setTotalPages] = useState<number>(1);
  const [totalCount, setTotalCount] = useState<number>(0);
  const [searchTerm, setSearchTerm] = useState<string>('');
  const deferredSearchTerm = useDeferredValue(searchTerm);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [playerToDelete, setPlayerToDelete] = useState<FloorballPlayerDto | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  const [deleteTimeoutId, setDeleteTimeoutId] = useState<number | null>(null);
  
  // Selection state for multiselect
  const [selectedPlayers, setSelectedPlayers] = useState<Set<string>>(new Set());
  const [isBulkDeleteModalOpen, setIsBulkDeleteModalOpen] = useState(false);
  const [isBulkDeleting, setIsBulkDeleting] = useState(false);

  useEffect(() => {
    const fetchPlayers = async () => {
      try {
        setLoading(true);
        const response = await floorballPlayerService.getAll({ page: currentPage, pageSize, searchTerm: deferredSearchTerm });
        setPlayers(response.data || []);
        setTotalPages(response.pagination?.totalPages ?? 1);
        setTotalCount(response.pagination?.totalCount ?? (response.data?.length || 0));
        setError(null);
      } catch (err) {
        setError(t('floorball.players.errors.loadPlayers', 'Failed to load players. Please try again.'));
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchPlayers();
  }, [currentPage, pageSize, deferredSearchTerm, t]);

  // Pagination handlers
  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const handlePageSizeChange = (size: number) => {
    setPageSize(size);
    setCurrentPage(1);
  };

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

  // Keep rendering the page layout; show loading state only in the table area

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
          <div className="players-actions">
            <div className="search-input-wrapper">
              <img src={SearchIcon} alt="Search" className="search-icon" />
              <input
                type="text"
                className="search-input"
                placeholder={t('floorball.players.searchPlayers', 'Search players...')}
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>
            <Button
              className="create-player-button"
              iconLeft={AddIcon}
              to="/admin/floorball/players/create"
            >
              {t('floorball.players.createNew', 'Create New Player')}
            </Button>
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
                  {t('common.selectAll', 'Select All')} ({players.length})
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
        <div className="players-count">
          <span>{t('floorball.players.totalCount', `${totalCount} players`, { count: totalCount })}</span>
        </div>
        {/* Players table */}
        <div className="players-table-container">
          {loading ? (
            <div className="floorball-players-loading">
              <p>{t('common.loading', 'Loading...')}</p>
            </div>
          ) : (
            <PlayersTable 
              players={players} 
              onDelete={handleDelete}
              selectedPlayers={selectedPlayers}
              onToggleSelection={togglePlayerSelection}
              onSelectAll={selectAllPlayers}
              onClearSelection={clearSelection}
              pagination={{ currentPage, totalPages, totalCount, pageSize }}
              onPageChange={handlePageChange}
              onPageSizeChange={handlePageSizeChange}
            />
          )}
        </div>

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
      </div>
    </PageTemplate>
  );
};

export default FloorballPlayersPage; 