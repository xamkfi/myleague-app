import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import BulkActionsBar from '../../../../components/BulkActionsBar/BulkActionsBar';
import '../../../../styles/AdminTable.scss';
import './FloorballSeasonsPage.scss';
import { useSeasonsManagement } from './hooks/useSeasonsManagement';
import { SeasonsPageHeader } from './components/SeasonsPageHeader';
import { SeasonsFilters } from './components/SeasonsFilters';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import { LoadingState } from './components/LoadingState';
import { SeasonsContent } from './components/SeasonsContent';
import { ConfirmDeleteModal } from './components/ConfirmDeleteModal';
import { floorballSeasonService } from '../../../../api/floorball/floorballSeasonService';

const FloorballSeasonsPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  
  const {
    // Data
    seasons,
    loading,
    error,
    operationLoading,
    selectedSeason,
    uniqueDivisions,
    
    // Filter states
    showActiveOnly,
    divisionFilter,
    
    // Modal states
    showDeleteModal,
    
    // Actions
    setDivisionFilter,
    handleShowActiveOnlyChange,
    handleDeleteSeason,
    handleActivateToggle,
    handleCompleteSeason,
    openDeleteModal,
    closeModals,
    loadSeasons
  } = useSeasonsManagement();

  // Selection state
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

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
    setSelectedIds(new Set(seasons.map((s) => s.id)));
  };

  const clearSelection = () => {
    setSelectedIds(new Set());
  };

  // Bulk actions
  const handleBulkActivate = async () => {
    try {
      for (const id of selectedIds) {
        const season = seasons.find((s) => s.id === id);
        if (season && !season.isActive && !season.isCompleted) {
          await floorballSeasonService.activate(id);
        }
      }
      setSelectedIds(new Set());
      await loadSeasons();
    } catch (err) {
      console.error('Bulk activate failed:', err);
    }
  };

  const handleBulkDeactivate = async () => {
    try {
      for (const id of selectedIds) {
        const season = seasons.find((s) => s.id === id);
        if (season && season.isActive && !season.isCompleted) {
          await floorballSeasonService.deactivate(id);
        }
      }
      setSelectedIds(new Set());
      await loadSeasons();
    } catch (err) {
      console.error('Bulk deactivate failed:', err);
    }
  };

  const handleBulkDelete = async () => {
    try {
      for (const id of selectedIds) {
        await floorballSeasonService.delete(id);
      }
      setSelectedIds(new Set());
      await loadSeasons();
    } catch (err) {
      console.error('Bulk delete failed:', err);
    }
  };

  if (loading) {
    return (
      <PageTemplate title={t('floorball.seasons.title', 'Manage Seasons')}>
        <LoadingState />
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('floorball.seasons.title', 'Manage Seasons')}>
      <div className="floorball-seasons-container">

        <SeasonsPageHeader
          seasonsCount={seasons.length}
          onCreateSeason={() => navigate('/admin/floorball/seasons/create')}
          onManageMatches={() => navigate('/admin/floorball/seasons/matches')}
        />

        <ErrorPopup message={error} />

        <SeasonsFilters
          showActiveOnly={showActiveOnly}
          onShowActiveOnlyChange={handleShowActiveOnlyChange}
          divisionFilter={divisionFilter}
          onDivisionFilterChange={setDivisionFilter}
          uniqueDivisions={uniqueDivisions}
        />

        {/* Bulk Actions Bar */}
        <BulkActionsBar
          selectedCount={selectedIds.size}
          totalCount={seasons.length}
          onSelectAll={selectAll}
          onClearSelection={clearSelection}
          actions={[
            {
              label: t('floorball.seasons.actions.bulkActivate', 'Activate ({{count}})', { count: selectedIds.size }),
              onClick: handleBulkActivate,
              variant: 'status',
            },
            {
              label: t('floorball.seasons.actions.bulkDeactivate', 'Deactivate ({{count}})', { count: selectedIds.size }),
              onClick: handleBulkDeactivate,
              variant: 'status',
            },
            {
              label: t('floorball.seasons.actions.bulkDelete', 'Delete ({{count}})', { count: selectedIds.size }),
              onClick: handleBulkDelete,
              variant: 'danger',
            },
          ]}
        />

        <div className="admin-table__wrapper">
          <SeasonsContent
            seasons={seasons}
            onEdit={(season) => navigate(`/admin/floorball/seasons/${season.id}/edit`)}
            onDelete={openDeleteModal}
            onActivateToggle={handleActivateToggle}
            onComplete={handleCompleteSeason}
            operationLoading={operationLoading}
            selectedIds={selectedIds}
            onToggleSelect={toggleSelect}
            onSelectAll={selectAll}
            onClearSelection={clearSelection}
          />
        </div>

        {/* Delete Modal */}
        {showDeleteModal && selectedSeason && (
          <ConfirmDeleteModal
            season={selectedSeason}
            onConfirm={handleDeleteSeason}
            onCancel={closeModals}
          />
        )}
      </div>
    </PageTemplate>
  );
};

export default FloorballSeasonsPage;
