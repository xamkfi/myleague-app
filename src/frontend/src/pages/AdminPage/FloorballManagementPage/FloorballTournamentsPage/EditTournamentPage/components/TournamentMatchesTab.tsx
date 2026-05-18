import { useCallback, useEffect, useMemo, useState, type ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { floorballMatchService } from '../../../../../../api/floorball/floorballMatchService';
import {
  FloorballMatchStatus,
  type FloorballMatchDto,
} from '../../../../../../types/floorball/floorballTypes';
import ErrorPopup from '../../../../../../components/ErrorPopup/ErrorPopup';
import SearchField from '../../../../../../components/SearchField/SearchField';
import StatusTabs, {
  type MatchTab,
  type StatusCounts,
} from '../../../MatchManagementPage/components/StatusTabs/StatusTabs';
import StatsBar from '../../../MatchManagementPage/components/StatsBar/StatsBar';
import MatchTable from '../../../MatchManagementPage/components/MatchTable/MatchTable';
import './TournamentMatchesTab.scss';

interface TournamentMatchesTabProps {
  tournamentId: string;
}

const TAB_TO_STATUS: Record<MatchTab, FloorballMatchStatus | undefined> = {
  all: undefined,
  ongoing: FloorballMatchStatus.InProgress,
  scheduled: FloorballMatchStatus.Scheduled,
  completed: FloorballMatchStatus.Completed,
  cancelled: FloorballMatchStatus.Cancelled,
};

const PAGE_SIZE: number = 50;

const TournamentMatchesTab = ({ tournamentId }: TournamentMatchesTabProps): ReactElement => {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const [searchInput, setSearchInput] = useState<string>('');
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [activeStatusTab, setActiveStatusTab] = useState<MatchTab>('all');

  const [matches, setMatches] = useState<FloorballMatchDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const [statusCounts, setStatusCounts] = useState<StatusCounts>({
    total: 0,
    inProgress: 0,
    scheduled: 0,
    completed: 0,
    cancelled: 0,
  });

  useEffect(() => {
    const timer = setTimeout(() => setSearchQuery(searchInput), 300);
    return () => clearTimeout(timer);
  }, [searchInput]);

  const fetchStatusCounts = useCallback(async (): Promise<void> => {
    const baseFilters = {
      competitionId: tournamentId,
      competitionType: 'Tournament' as const,
      searchQuery: searchQuery.trim() || undefined,
    };
    try {
      const [totalRes, scheduledRes, inProgressRes, completedRes, cancelledRes] = await Promise.all([
        floorballMatchService.getAll({ pageSize: 1, ...baseFilters }),
        floorballMatchService.getAll({ pageSize: 1, ...baseFilters, status: FloorballMatchStatus.Scheduled }),
        floorballMatchService.getAll({ pageSize: 1, ...baseFilters, status: FloorballMatchStatus.InProgress }),
        floorballMatchService.getAll({ pageSize: 1, ...baseFilters, status: FloorballMatchStatus.Completed }),
        floorballMatchService.getAll({ pageSize: 1, ...baseFilters, status: FloorballMatchStatus.Cancelled }),
      ]);

      setStatusCounts({
        total: totalRes.pagination?.totalCount ?? 0,
        scheduled: scheduledRes.pagination?.totalCount ?? 0,
        inProgress: inProgressRes.pagination?.totalCount ?? 0,
        completed: completedRes.pagination?.totalCount ?? 0,
        cancelled: cancelledRes.pagination?.totalCount ?? 0,
      });
    } catch (err) {
      console.error('Failed to fetch tournament match status counts', err);
    }
  }, [tournamentId, searchQuery]);

  const fetchMatches = useCallback(async (): Promise<void> => {
    try {
      setLoading(true);
      setError(null);

      const response = await floorballMatchService.getAll({
        page: 1,
        pageSize: PAGE_SIZE,
        competitionId: tournamentId,
        competitionType: 'Tournament',
        searchQuery: searchQuery.trim() || undefined,
        status: TAB_TO_STATUS[activeStatusTab],
        sortOrder: 'asc',
      });

      if (response.success && response.data) {
        setMatches(response.data);
      } else {
        setMatches([]);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load tournament matches');
      setMatches([]);
    } finally {
      setLoading(false);
    }
  }, [tournamentId, searchQuery, activeStatusTab]);

  useEffect(() => {
    fetchMatches();
    fetchStatusCounts();
  }, [fetchMatches, fetchStatusCounts]);

  const manageAllPath: string = useMemo(
    () => `/admin/floorball/tournaments/matches?competitionId=${tournamentId}`,
    [tournamentId]
  );

  const createMatchPath: string = useMemo(() => {
    const returnTo: string = `/admin/floorball/tournaments/${tournamentId}/edit?tab=matches`;
    return `/admin/floorball/tournaments/matches/create?competitionId=${tournamentId}&returnTo=${encodeURIComponent(returnTo)}`;
  }, [tournamentId]);

  const handleManageAll = useCallback((): void => {
    navigate(manageAllPath);
  }, [navigate, manageAllPath]);

  const handleCreateNew = useCallback((): void => {
    navigate(createMatchPath);
  }, [navigate, createMatchPath]);

  const handleOpenMatch = useCallback(
    (match: FloorballMatchDto): void => {
      navigate(`/admin/floorball/matches/manage/${match.id}`);
    },
    [navigate]
  );

  const handleNavigateToFullManagement = useCallback(
    (): void => {
      navigate(manageAllPath);
    },
    [navigate, manageAllPath]
  );

  const isEmpty: boolean = !loading && statusCounts.total === 0 && !searchQuery;

  return (
    <div className="tmt">
      <div className="tmt__action-row">
        <button type="button" className="btn btn-secondary" onClick={handleManageAll}>
          <i className="fas fa-tasks" aria-hidden="true"></i>{' '}
          {t('floorball.tournaments.actions.manageMatches', 'Manage tournament matches')}
        </button>
        <button type="button" className="btn btn-primary" onClick={handleCreateNew}>
          <i className="fas fa-plus" aria-hidden="true"></i>{' '}
          {t('floorball.tournaments.matchesTab.createNew', 'Create new match')}
        </button>
      </div>

      <ErrorPopup message={error} />

      <StatsBar stats={statusCounts} isSeasonFiltered={false} />

      <div className="tmt__filters">
        <SearchField
          value={searchInput}
          onChange={setSearchInput}
          placeholder={t('floorball.matches.filters.searchPlaceholder', 'Search for team names...')}
          rounded="pill"
          fullWidth
        />
      </div>

      <StatusTabs
        activeTab={activeStatusTab}
        onTabChange={setActiveStatusTab}
        counts={statusCounts}
      />

      {isEmpty ? (
        <div className="tmt__empty">
          <i className="fas fa-calendar-times" aria-hidden="true"></i>
          <p>
            {t(
              'floorball.tournaments.matchesTab.empty',
              'No matches have been added to this tournament yet.'
            )}
          </p>
          <button type="button" className="btn btn-primary" onClick={handleCreateNew}>
            <i className="fas fa-plus" aria-hidden="true"></i>{' '}
            {t('floorball.tournaments.matchesTab.createFirst', 'Create the first match')}
          </button>
        </div>
      ) : (
        <div className="tmt__table-panel">
          <MatchTable
            matches={matches}
            loading={loading && matches.length === 0}
            onOpenMatch={handleOpenMatch}
            onLiveMatch={handleOpenMatch}
            onEditMatch={handleOpenMatch}
            onStartMatch={handleNavigateToFullManagement}
            onCancelMatch={handleNavigateToFullManagement}
            onReactivateMatch={handleNavigateToFullManagement}
          />
        </div>
      )}
    </div>
  );
};

export default TournamentMatchesTab;
