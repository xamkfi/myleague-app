import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/PageTemplate';
import { floorballRefereeService, type FloorballRefereeDto } from '../../../../api/floorball/floorballRefereeService';
import RefereesTable from './components/RefereesTable';
import CreateRefereeModal from './components/CreateRefereeModal';
import ConfirmDeleteModal from './components/ConfirmDeleteModal';
import './FloorballRefereesPage.scss';
import './components/CreateRefereeModal.scss';
import BackButton from '../../../../components/BackButton/BackButton';

const FloorballRefereesPage = () => {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [referees, setReferees] = useState<FloorballRefereeDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [refereeToDelete, setRefereeToDelete] = useState<FloorballRefereeDto | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  const [deleteTimeoutId, setDeleteTimeoutId] = useState<number | null>(null);

  useEffect(() => {
    const fetchReferees = async () => {
      try {
        setLoading(true);
        const response = await floorballRefereeService.getAll({ pageSize: 50 }); // Fetch up to 50 for now
        console.log('Referees API response:', response);
        
        // Ensure we have valid response data and it's an array
        if (response?.data && Array.isArray(response.data)) {
          setReferees(response.data);
          setError(null);
        } else {
          // Handle case where response structure is unexpected
          console.warn('Invalid response structure:', response);
          setReferees([]);
          if (response?.success === false) {
            setError(response.errors?.join(', ') || 'Failed to load referees');
          } else {
            setError('Invalid response format from server');
          }
        }
      } catch (err) {
        // Ensure referees is always an array even on error
        setReferees([]);
        setError(t('floorball.referees.errors.loadReferees', 'Failed to load referees. Please try again.'));
        console.error('Error fetching referees:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchReferees();
  }, [t]);

  const handleDelete = (refereeId: string) => {
    const referee = referees.find(r => r.id === refereeId);
    if (!referee) return;

    setRefereeToDelete(referee);
    setIsDeleteModalOpen(true);
  };

  const handleConfirmDelete = async () => {
    if (!refereeToDelete) return;

    try {
      setIsDeleting(true);
      setError(null);
      
      await floorballRefereeService.delete(refereeToDelete.id);
      
      // Remove referee from list
      setReferees(prevReferees => prevReferees.filter(r => r.id !== refereeToDelete.id));
      
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
      setRefereeToDelete(null);
    } catch (err) {
      setError(t('floorball.referees.errors.deleteFailed', 'Failed to delete referee. Please try again.'));
      console.error(err);
    } finally {
      setIsDeleting(false);
    }
  };

  const handleCancelDelete = () => {
    setIsDeleteModalOpen(false);
    setRefereeToDelete(null);
  };

  const handleRefereeCreated = (newReferee: FloorballRefereeDto) => {
    setReferees(prevReferees => [...prevReferees, newReferee]);
    setError(null);
  };

  const handleCreateRefereeClick = () => {
    setIsCreateModalOpen(true);
  };

  const handleCloseCreateModal = () => {
    setIsCreateModalOpen(false);
  };

  if (loading) {
    return (
      <PageTemplate title={t('floorball.referees.title', 'Manage Floorball Referees')}>
        <div className="floorball-referees-loading">
          <p>{t('common.loading', 'Loading...')}</p>
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('floorball.referees.title', 'Manage Floorball Referees')}>      
      <div className="floorball-referees-container">
        {/* Back button */}
        <BackButton 
          to="/admin/floorball" 
          text={t('common.back', 'Back to Floorball Management')} 
        />

        {/* Header with actions */}
        <div className="floorball-referees-header">
          <div className="referees-count">
            <span>{t('floorball.referees.totalCount', `${Array.isArray(referees) ? referees.length : 0} referees`, { count: Array.isArray(referees) ? referees.length : 0 })}</span>
          </div>
          <div className="referees-actions">
            <button className="create-referee-button" onClick={handleCreateRefereeClick}>
              {t('floorball.referees.createNew', 'Create New Referee')}
            </button>
          </div>
        </div>
        
        {/* Error message */}
        {error && (
          <div className="error-message">
            <p>{error}</p>
          </div>
        )}
        
        {/* Referees table */}
        <div className="referees-table-container">
          <RefereesTable 
            referees={Array.isArray(referees) ? referees : []} 
            onDelete={handleDelete} 
          />
        </div>
        
        {/* Create Referee Modal */}
        <CreateRefereeModal
          isOpen={isCreateModalOpen}
          onClose={handleCloseCreateModal}
          onRefereeCreated={handleRefereeCreated}
        />

        {/* Confirm Delete Modal */}
        <ConfirmDeleteModal
          isOpen={isDeleteModalOpen}
          referee={refereeToDelete}
          onConfirm={handleConfirmDelete}
          onCancel={handleCancelDelete}
          isDeleting={isDeleting}
        />
      </div>
    </PageTemplate>
  );
};

export default FloorballRefereesPage; 