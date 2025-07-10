import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../../components/PageTemplate/PageTemplate';
import { floorballPlayerService, type FloorballPlayerDto } from '../../../../api/floorball/floorballPlayerService';
import PlayersTable from './components/PlayersTable';
import CreatePlayerModal from './components/CreatePlayerModal';
import ConfirmDeleteModal from './components/ConfirmDeleteModal';
import './FloorballPlayersPage.scss';
import './components/CreatePlayerModal.scss';
import BackButton from '../../../../components/BackButton/BackButton';

const FloorballPlayersPage = () => {
  const { t } = useTranslation();
  const [players, setPlayers] = useState<FloorballPlayerDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [playerToDelete, setPlayerToDelete] = useState<FloorballPlayerDto | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  const [deleteTimeoutId, setDeleteTimeoutId] = useState<number | null>(null);

  useEffect(() => {
    const fetchPlayers = async () => {
      try {
        setLoading(true);
        const response = await floorballPlayerService.getAll({ pageSize: 50 }); // Fetch up to 50 for now
        if (response.data) {
          setPlayers(response.data);
        }
        setError(null);
      } catch (err) {
        setError(t('floorball.players.errors.loadPlayers', 'Failed to load players. Please try again.'));
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchPlayers();
  }, [t]);

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

  const handlePlayerCreated = (newPlayer: FloorballPlayerDto) => {
    setPlayers(prevPlayers => [...prevPlayers, newPlayer]);
    setError(null);
  };

  const handleCreatePlayerClick = () => {
    setIsCreateModalOpen(true);
  };

  const handleCloseCreateModal = () => {
    setIsCreateModalOpen(false);
  };

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
            <span>{t('floorball.players.totalCount', `${players.length} players`, { count: players.length })}</span>
          </div>
          <div className="players-actions">
            <button className="create-player-button" onClick={handleCreatePlayerClick}>
              {t('floorball.players.createNew', 'Create New Player')}
            </button>
          </div>
        </div>
        
        {/* Error message */}
        {error && (
          <div className="error-message">
            <p>{error}</p>
          </div>
        )}
        
        {/* Players table */}
        <div className="players-table-container">
          <PlayersTable 
            players={players} 
            onDelete={handleDelete} 
          />
        </div>

        {/* Create Player Modal */}
        <CreatePlayerModal
          isOpen={isCreateModalOpen}
          onClose={handleCloseCreateModal}
          onPlayerCreated={handlePlayerCreated}
        />

        {/* Confirm Delete Modal */}
        <ConfirmDeleteModal
          isOpen={isDeleteModalOpen}
          player={playerToDelete}
          onConfirm={handleConfirmDelete}
          onCancel={handleCancelDelete}
          isDeleting={isDeleting}
        />
      </div>
    </PageTemplate>
  );
};

export default FloorballPlayersPage; 