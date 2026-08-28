import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
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
  } = useTournamentsManagement();

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
          />
        </div>
      </div>
    </PageTemplate>
  );
};

export default FootballTournamentsPage;
