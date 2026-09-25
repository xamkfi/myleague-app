import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import ConfirmDeleteTournamentDialog from '../../../../components/admin/ConfirmDeleteTournamentDialog';
import { footballTournamentService } from '../../../../api/football/footballTournamentService';
import type { FootballTournamentDto } from '../../../../types/football/tournamentTypes';
import '../../../../styles/AdminTable.scss';
import './FootballTournamentsPage.scss';
import { useTournamentsManagement } from './hooks/useTournamentsManagement';
import { TournamentsPageHeader } from './components/TournamentsPageHeader';
import { TournamentsFilters } from './components/TournamentsFilters';
import { TournamentsContent } from './components/TournamentsContent';
import { LoadingState } from './components/LoadingState';

const FootballTournamentsPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const {
    tournaments,
    loading,
    error,
    showOngoingOnly,
    statusFilter,
    categoryFilter,
    uniqueStatuses,
    setShowOngoingOnly,
    setStatusFilter,
    setCategoryFilter,
    loadTournaments,
  } = useTournamentsManagement();

  const [pendingDelete, setPendingDelete] = useState<FootballTournamentDto | null>(null);
  const [deleting, setDeleting] = useState<boolean>(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  const confirmDelete = async (): Promise<void> => {
    if (!pendingDelete) return;
    setDeleting(true);
    setDeleteError(null);
    try {
      await footballTournamentService.delete(pendingDelete.id);
      setPendingDelete(null);
      await loadTournaments({ silent: true });
    } catch (err) {
      const message = err instanceof Error ? err.message : t('football.tournaments.errors.operationFailed', 'Operation failed. Please try again.');
      setDeleteError(message);
    } finally {
      setDeleting(false);
    }
  };

  if (loading) {
    return (
      <PageTemplate title={t('football.tournaments.title', 'Manage Tournaments')}>
        <LoadingState />
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('football.tournaments.title', 'Manage Tournaments')}>
      <div className="football-tournaments-container">
        <TournamentsPageHeader
          tournamentsCount={tournaments.length}
          onCreateTournament={() => navigate('/admin/football/tournaments/create')}
          onManageMatches={() => navigate('/admin/football/tournaments/matches')}
        />

        <ErrorPopup message={error} />

        <TournamentsFilters
          showOngoingOnly={showOngoingOnly}
          onShowOngoingOnlyChange={setShowOngoingOnly}
          statusFilter={statusFilter}
          onStatusFilterChange={setStatusFilter}
          uniqueStatuses={uniqueStatuses}
          categoryFilter={categoryFilter}
          onCategoryFilterChange={setCategoryFilter}
        />

        <div className="admin-table__wrapper">
          <TournamentsContent
            tournaments={tournaments}
            onEdit={(tournament) => navigate(`/admin/football/tournaments/${tournament.id}/edit`)}
            onDelete={(tournament) => {
              setDeleteError(null);
              setPendingDelete(tournament);
            }}
          />
        </div>

        {pendingDelete && (
          <ConfirmDeleteTournamentDialog
            tournament={{
              id: pendingDelete.id,
              name: pendingDelete.name,
              status: pendingDelete.tournamentStatus,
              teamCount: pendingDelete.teamCount,
              matchCount: pendingDelete.matchCount,
              groups: pendingDelete.groups ?? [],
            }}
            deleting={deleting}
            error={deleteError}
            onConfirm={() => { void confirmDelete(); }}
            onCancel={() => {
              if (!deleting) setPendingDelete(null);
            }}
          />
        )}
      </div>
    </PageTemplate>
  );
};

export default FootballTournamentsPage;
