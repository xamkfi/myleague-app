import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import BulkActionsBar from '../../../../components/BulkActionsBar/BulkActionsBar';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import '../../../../styles/AdminTable.scss';
import './FloorballTournamentsPage.scss';
import { useTournamentsManagement } from './hooks/useTournamentsManagement';
import { TournamentsPageHeader } from './components/TournamentsPageHeader';
import { TournamentsFilters } from './components/TournamentsFilters';
import { TournamentsContent } from './components/TournamentsContent';
import { ConfirmDeleteTournamentModal } from './components/ConfirmDeleteTournamentModal';
import { LoadingState } from './components/LoadingState';
import { floorballTournamentService } from '../../../../api/floorball/floorballTournamentService';

const FloorballTournamentsPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const {
    tournaments,
    loading,
    error,
    operationLoading,
    selectedTournament,
    showOngoingOnly,
    statusFilter,
    uniqueStatuses,
    showDeleteModal,
    setShowOngoingOnly,
    setStatusFilter,
    handleDeleteTournament,
    handleLifecycleAction,
    openDeleteModal,
    closeModals,
    loadTournaments,
  } = useTournamentsManagement();

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
    setSelectedIds(new Set(tournaments.map((tournament) => tournament.id)));
  };

  const clearSelection = () => {
    setSelectedIds(new Set());
  };

  const handleBulkDelete = async () => {
    try {
      for (const id of selectedIds) {
        await floorballTournamentService.delete(id);
      }
      setSelectedIds(new Set());
      await loadTournaments();
    } catch (err) {
      console.error('Bulk delete failed:', err);
    }
  };

  if (loading) {
    return (
      <PageTemplate title={t('floorball.tournaments.title', 'Manage Tournaments')}>
        <LoadingState />
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('floorball.tournaments.title', 'Manage Tournaments')}>
      <div className="floorball-tournaments-container">
        <TournamentsPageHeader
          tournamentsCount={tournaments.length}
          onCreateTournament={() => navigate('/admin/floorball/tournaments/create')}
          onManageMatches={() => navigate('/admin/floorball/tournaments/matches')}
        />

        <ErrorPopup message={error} />

        <TournamentsFilters
          showOngoingOnly={showOngoingOnly}
          onShowOngoingOnlyChange={setShowOngoingOnly}
          statusFilter={statusFilter}
          onStatusFilterChange={setStatusFilter}
          uniqueStatuses={uniqueStatuses}
        />

        <BulkActionsBar
          selectedCount={selectedIds.size}
          totalCount={tournaments.length}
          onSelectAll={selectAll}
          onClearSelection={clearSelection}
          actions={[
            {
              label: t('floorball.tournaments.actions.bulkDelete', 'Delete ({{count}})', { count: selectedIds.size }),
              onClick: handleBulkDelete,
              variant: 'danger',
            },
          ]}
        />

        <div className="admin-table__wrapper">
          <TournamentsContent
            tournaments={tournaments}
            onEdit={(tournament) => navigate(`/admin/floorball/tournaments/${tournament.id}/edit`)}
            onDelete={openDeleteModal}
            onLifecycleAction={handleLifecycleAction}
            operationLoading={operationLoading}
            selectedIds={selectedIds}
            onToggleSelect={toggleSelect}
            onSelectAll={selectAll}
            onClearSelection={clearSelection}
          />
        </div>

        {showDeleteModal && selectedTournament && (
          <ConfirmDeleteTournamentModal
            tournament={selectedTournament}
            onConfirm={handleDeleteTournament}
            onCancel={closeModals}
          />
        )}
      </div>
    </PageTemplate>
  );
};

export default FloorballTournamentsPage;
