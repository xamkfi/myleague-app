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
import ConfirmationDialog from '../ManageMatchPage/components/ConfirmationDialog';
import {
  floorballSeasonService,
  type FloorballSeasonDto,
} from '../../../../api/floorball/floorballSeasonService';

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
    openDeleteModal,
    closeModals,
    loadSeasons,
  } = useSeasonsManagement();

  // Stores selected season ids for bulk actions.
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());

  // Controls the confirmation dialog shown before completing a season.
  const [showCompleteSeasonConfirm, setShowCompleteSeasonConfirm] = useState(false);

  // Stores the season that user is trying to complete.
  const [seasonToComplete, setSeasonToComplete] = useState<FloorballSeasonDto | null>(null);

  // Prevents duplicate complete-season requests while API call is running.
  const [completeSeasonLoading, setCompleteSeasonLoading] = useState(false);

  // Stores possible error from completing a season.
  const [completeSeasonError, setCompleteSeasonError] = useState<string | null>(null);

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
    setSelectedIds(new Set(seasons.map((season) => season.id)));
  };

  const clearSelection = () => {
    setSelectedIds(new Set());
  };

  const openCompleteSeasonConfirm = (season: FloorballSeasonDto) => {
    // Clears old complete-season error before opening dialog.
    setCompleteSeasonError(null);
    setSeasonToComplete(season);
    setShowCompleteSeasonConfirm(true);
  };

  const closeCompleteSeasonConfirm = () => {
    // Dialog cannot be closed while complete request is running.
    if (completeSeasonLoading) return;

    setShowCompleteSeasonConfirm(false);
    setSeasonToComplete(null);
  };

  const handleConfirmCompleteSeason = async () => {
    if (!seasonToComplete) return;

    try {
      setCompleteSeasonLoading(true);
      setCompleteSeasonError(null);

      // Completes the selected season through API.
      await floorballSeasonService.complete(seasonToComplete.id);

      setShowCompleteSeasonConfirm(false);
      setSeasonToComplete(null);

      // Reloads seasons so completed status is visible immediately.
      await loadSeasons();
    } catch (err) {
      const message =
        err instanceof Error
          ? err.message
          : t('floorball.seasons.completeError', 'Failed to complete season.');

      setCompleteSeasonError(message);
    } finally {
      setCompleteSeasonLoading(false);
    }
  };

  const handleBulkActivate = async () => {
    try {
      for (const id of selectedIds) {
        const season = seasons.find((s) => s.id === id);

        // Completed seasons cannot be activated again.
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

        // Completed seasons cannot be deactivated.
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

        {/* Shows normal page errors first. If there is none, shows complete-season error. */}
        <ErrorPopup message={error || completeSeasonError} />

        <SeasonsFilters
          showActiveOnly={showActiveOnly}
          onShowActiveOnlyChange={handleShowActiveOnlyChange}
          divisionFilter={divisionFilter}
          onDivisionFilterChange={setDivisionFilter}
          uniqueDivisions={uniqueDivisions}
        />

        <BulkActionsBar
          selectedCount={selectedIds.size}
          totalCount={seasons.length}
          onSelectAll={selectAll}
          onClearSelection={clearSelection}
          actions={[
            {
              label: t('floorball.seasons.actions.bulkActivate', 'Activate ({{count}})', {
                count: selectedIds.size,
              }),
              onClick: handleBulkActivate,
              variant: 'status',
            },
            {
              label: t('floorball.seasons.actions.bulkDeactivate', 'Deactivate ({{count}})', {
                count: selectedIds.size,
              }),
              onClick: handleBulkDeactivate,
              variant: 'status',
            },
            {
              label: t('floorball.seasons.actions.bulkDelete', 'Delete ({{count}})', {
                count: selectedIds.size,
              }),
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
            onComplete={openCompleteSeasonConfirm}
            operationLoading={
              operationLoading ?? (completeSeasonLoading ? (seasonToComplete?.id ?? 'complete') : null)
            }
            selectedIds={selectedIds}
            onToggleSelect={toggleSelect}
            onSelectAll={selectAll}
            onClearSelection={clearSelection}
          />
        </div>

        {showDeleteModal && selectedSeason && (
          <ConfirmDeleteModal
            season={selectedSeason}
            onConfirm={handleDeleteSeason}
            onCancel={closeModals}
          />
        )}

        <ConfirmationDialog
          isOpen={showCompleteSeasonConfirm}
          icon="⚠️"
          title={t('floorball.seasons.confirmComplete.title', 'End season?')}
          message={t(
            'floorball.seasons.confirmComplete.message',
            `Are you sure you want to end the season "${seasonToComplete?.name ?? ''}"?`
          )}
          warningMessage={t(
            'floorball.seasons.confirmComplete.warning',
            'This action cannot be undone. Once the season is ended, it cannot be restored.'
          )}
          confirmText={t('floorball.seasons.confirmComplete.confirm', 'Yes, end season')}
          cancelText={t('common.cancel', 'Cancel')}
          isLoading={completeSeasonLoading}
          onConfirm={handleConfirmCompleteSeason}
          onCancel={closeCompleteSeasonConfirm}
        />
      </div>
    </PageTemplate>
  );
};

export default FloorballSeasonsPage;