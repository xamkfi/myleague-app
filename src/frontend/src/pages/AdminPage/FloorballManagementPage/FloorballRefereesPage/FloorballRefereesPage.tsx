import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import { floorballRefereeService, type FloorballRefereeDto } from '../../../../api/floorball/floorballRefereeService';
import RefereesTable from './components/RefereesTable';
import ConfirmDeleteModal from './components/ConfirmDeleteModal';
import SearchField from '../../../../components/SearchField';
import Button from '../../../../components/Button/Button';
import AddIcon from '../../../../assets/basicIcons/add.svg';
import BulkActionsBar from '../../../../components/BulkActionsBar/BulkActionsBar';
import '../../../../styles/AdminTable.scss';
import './FloorballRefereesPage.scss';

import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import { mapDeletionError } from '../../../../utils/mapDeletionError';

const FloorballRefereesPage = () => {
  const { t } = useTranslation();
  const [referees, setReferees] = useState<FloorballRefereeDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [refereeToDelete, setRefereeToDelete] = useState<FloorballRefereeDto | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');

  const [deleteTimeoutId, setDeleteTimeoutId] = useState<ReturnType<typeof setTimeout> | null>(null);

  // Selection state
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  // Filter referees based on search term
  const filteredReferees = referees.filter(referee => {
    if (!searchTerm) return true;
    const searchLower = searchTerm.toLowerCase().trim();
    const fullName = [referee.person.firstName, referee.person.lastName].filter(Boolean).join(' ');
    return fullName.toLowerCase().includes(searchLower);
  });

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

  // Selection handlers
  const toggleSelect = (id: string) => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  };

  const selectAll = () => {
    setSelectedIds(new Set(filteredReferees.map((r) => r.id)));
  };

  const clearSelection = () => {
    setSelectedIds(new Set());
  };

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
      
      // Clear selection if the deleted referee was selected
      setSelectedIds((prev) => {
        const next = new Set(prev);
        next.delete(refereeToDelete.id);
        return next;
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
      setRefereeToDelete(null);
    } catch (err) {
      setError(
        mapDeletionError(err, t) ??
          t('floorball.referees.errors.deleteFailed', 'Failed to delete referee. Please try again.'),
      );
      console.error(err);
    } finally {
      setIsDeleting(false);
    }
  };

  const handleCancelDelete = () => {
    setIsDeleteModalOpen(false);
    setRefereeToDelete(null);
  };

  // Bulk delete handler
  const handleBulkDelete = async () => {
    if (selectedIds.size === 0) return;

    try {
      setError(null);
      for (const id of selectedIds) {
        await floorballRefereeService.delete(id);
      }

      // Remove deleted referees from list
      setReferees((prev) => prev.filter((r) => !selectedIds.has(r.id)));
      setSelectedIds(new Set());
    } catch (err) {
      setError(t('floorball.referees.errors.bulkDeleteFailed', 'Failed to delete selected referees. Please try again.'));
      console.error('Bulk delete failed:', err);
    }
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
    <PageTemplate title={t('floorball.referees.title', 'MANAGE REFEREES')}>      
      <div className="floorball-referees-container">
        <h2 className="floorball-referees-title">{t('floorball.referees.title', 'MANAGE REFEREES')}</h2>

        {/* Header with search and create button */}
        <div className="floorball-referees-header">
          <div className="referees-actions">
            <SearchField
              value={searchTerm}
              onChange={setSearchTerm}
              placeholder={t('floorball.referees.searchReferees', 'Search referees...')}
              fullWidth
              rounded="pill"
            />
            <Button
              className="create-referee-button"
              iconLeft={AddIcon}
              to="/admin/floorball/referees/create"
            >
              {t('floorball.referees.createNew', 'Create new referee')}
            </Button>
          </div>
        </div>
        
        {/* Error message */}
        <ErrorPopup message={error} />

        {/* Bulk Actions Bar */}
        <BulkActionsBar
          selectedCount={selectedIds.size}
          totalCount={filteredReferees.length}
          onSelectAll={selectAll}
          onClearSelection={clearSelection}
          actions={[
            {
              label: t('floorball.referees.actions.bulkDelete', 'Delete ({{count}})', { count: selectedIds.size }),
              onClick: handleBulkDelete,
              variant: 'danger',
            },
          ]}
        />

        {/* Referees table */}
        <div className="admin-table__wrapper">
          <RefereesTable 
            referees={filteredReferees} 
            onDelete={handleDelete}
            selectedIds={selectedIds}
            onToggleSelect={toggleSelect}
            onSelectAll={selectAll}
            onClearSelection={clearSelection}
          />
        </div>

        {/* No data states */}
        {filteredReferees.length === 0 && !loading && (
          <div className="no-data">
            {searchTerm 
              ? t('floorball.referees.noSearchResults', 'No referees found matching "{{searchTerm}}"', { searchTerm })
              : t('floorball.referees.noReferees', 'No referees found.')
            }
          </div>
        )}
        
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
