import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import '../../../../styles/AdminTable.scss';
import './FloorballTournamentsPage.scss';
import { useTournamentsManagement } from './hooks/useTournamentsManagement';
import { TournamentsPageHeader } from './components/TournamentsPageHeader';
import { TournamentsFilters } from './components/TournamentsFilters';
import { TournamentsContent } from './components/TournamentsContent';
import { TournamentImportModal } from './components/TournamentImportModal';
import { LoadingState } from './components/LoadingState';

const FloorballTournamentsPage = () => {
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

  const [showImportModal, setShowImportModal] = useState<boolean>(false);

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
          onImportTournament={() => setShowImportModal(true)}
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
            onEdit={(tournament) => navigate(`/admin/floorball/tournaments/${tournament.id}/edit`)}
          />
        </div>

        {showImportModal && (
          <TournamentImportModal
            onClose={() => setShowImportModal(false)}
            // Silent refresh — a non-silent refresh would flip the page-level loading flag and
            // unmount the modal mid-import, throwing away the user's progress log.
            onImported={() => loadTournaments({ silent: true })}
          />
        )}
      </div>
    </PageTemplate>
  );
};

export default FloorballTournamentsPage;
