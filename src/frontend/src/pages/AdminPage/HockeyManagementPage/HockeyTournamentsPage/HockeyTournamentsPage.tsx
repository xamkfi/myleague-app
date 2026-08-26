import { useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import '../../../../styles/AdminTable.scss';
import './HockeyTournamentsPage.scss';
import { TournamentsPageHeader } from './components/TournamentsPageHeader';
import { TournamentsFilters } from './components/TournamentsFilters';
import { TournamentsContent } from './components/TournamentsContent';
import { LoadingState } from '../HockeySeasonsPage/components/LoadingState';
import { hockeyTournamentService } from '../../../../api/hockey/hockeyTournamentService';
import type { HockeyTournamentDto } from '../../../../types/hockey/hockeyTypes';

function HockeyTournamentsPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [tournaments, setTournaments] = useState<HockeyTournamentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showOngoingOnly, setShowOngoingOnly] = useState(false);
  const [statusFilter, setStatusFilter] = useState('all');
  const [categoryFilter, setCategoryFilter] = useState<string[]>([]);

  useEffect(() => {
    hockeyTournamentService.getAll()
      .then(setTournaments)
      .catch((err) => setError(err instanceof Error ? err.message : t('hockey.tournaments.errors.loadFailed', 'Failed to load tournaments')))
      .finally(() => setLoading(false));
  }, [t]);

  const uniqueStatuses = useMemo(() => {
    const values = new Set<string>();
    for (const tournament of tournaments) {
      values.add(tournament.status);
      if (tournament.currentStage) {
        values.add(tournament.currentStage);
      }
    }
    return [...values];
  }, [tournaments]);

  const filtered = useMemo(() => {
    return tournaments.filter((tournament) => {
      if (showOngoingOnly && (!tournament.isActive || tournament.isCompleted)) {
        return false;
      }
      if (statusFilter !== 'all' && tournament.status !== statusFilter && tournament.currentStage !== statusFilter) {
        return false;
      }
      if (categoryFilter.length > 0 && !categoryFilter.includes(tournament.teamCategory ?? '')) {
        return false;
      }
      return true;
    });
  }, [tournaments, showOngoingOnly, statusFilter, categoryFilter]);

  if (loading) {
    return (
      <PageTemplate title={t('hockey.tournaments.title', 'Manage Tournaments')}>
        <LoadingState />
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('hockey.tournaments.title', 'Manage Tournaments')}>
      <div className="floorball-tournaments-container">
        <TournamentsPageHeader
          tournamentsCount={tournaments.length}
          onCreateTournament={() => navigate('/admin/hockey/tournaments/create')}
          onManageMatches={() => navigate('/admin/hockey/tournaments/matches')}
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
            tournaments={filtered}
            onEdit={(tournament) => navigate(`/admin/hockey/tournaments/${tournament.id}/edit`)}
          />
        </div>
      </div>
    </PageTemplate>
  );
}

export default HockeyTournamentsPage;
