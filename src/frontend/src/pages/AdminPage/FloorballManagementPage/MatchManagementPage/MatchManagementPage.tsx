import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { floorballMatchService } from '../../../../api/floorball/floorballMatchService';
import { floorballMatchEventService } from '../../../../api/floorball/floorballMatchEventService';
import { floorballSeasonService, type FloorballSeasonDto } from '../../../../api/floorball/floorballSeasonService';
import { signalRService, type MatchEvent } from '../../../../services/signalRService';
import { FloorballMatchStatus } from '../../../../types/floorball/floorballTypes';
import type { FloorballMatchDto } from '../../../../types/floorball/floorballTypes';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import LoadingSpinner from '../../../../components/LoadingSpinner/LoadingSpinner';
import ConfirmationDialog from '../ManageMatchPage/components/ConfirmationDialog';
import SearchField from '../../../../components/SearchField/SearchField';
import Button from '../../../../components/Button/Button';
import AddIcon from '../../../../assets/basicIcons/add.svg';
import { formatSeasonDisplayName } from '../ManageMatchPage/utils/matchFormatters';
import PaginationControls from '../FloorballPlayersPage/components/PaginationControls';
import StatusTabs from './components/StatusTabs/StatusTabs';
import type { MatchTab, StatusCounts } from './components/StatusTabs/StatusTabs';
import StatsBar from './components/StatsBar/StatsBar';
import MatchTable from './components/MatchTable/MatchTable';
import './MatchManagementPage.scss';

const TAB_TO_STATUS: Record<MatchTab, FloorballMatchStatus | undefined> = {
  all: undefined,
  ongoing: FloorballMatchStatus.InProgress,
  scheduled: FloorballMatchStatus.Scheduled,
  completed: FloorballMatchStatus.Completed,
  cancelled: FloorballMatchStatus.Cancelled,
};

const VALID_TABS: MatchTab[] = ['all', 'ongoing', 'scheduled', 'completed', 'cancelled'];

const isValidTab = (value: string | null): value is MatchTab =>
  value !== null && VALID_TABS.includes(value as MatchTab);

const MatchManagementPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();

  // Tab state from URL
  const urlTab = searchParams.get('tab');
  const activeTab: MatchTab = isValidTab(urlTab) ? urlTab : 'all';

  const setActiveTab = (tab: MatchTab) => {
    setSearchParams(tab === 'all' ? {} : { tab }, { replace: true });
    setCurrentPage(1);
  };

  // Data state
  const [matches, setMatches] = useState<FloorballMatchDto[]>([]);
  const [seasons, setSeasons] = useState<FloorballSeasonDto[]>([]);
  const [initialLoading, setInitialLoading] = useState(true);
  const [tableLoading, setTableLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Filter state
  const [selectedSeasonId, setSelectedSeasonId] = useState<string>('');
  const [searchQuery, setSearchQuery] = useState<string>('');

  // Pagination state
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(25);
  const [totalCount, setTotalCount] = useState(0);

  // Stats (counts per status)
  const [statusCounts, setStatusCounts] = useState<StatusCounts>({
    total: 0, inProgress: 0, scheduled: 0, completed: 0, cancelled: 0,
  });

  // Confirmation dialog
  const [confirmDialog, setConfirmDialog] = useState<{
    type: 'cancel' | 'reactivate';
    match: FloorballMatchDto;
  } | null>(null);
  const [dialogLoading, setDialogLoading] = useState(false);

  // Fetch status counts (lightweight queries)
  const fetchStatusCounts = useCallback(async () => {
    const seasonFilter = selectedSeasonId || undefined;
    const searchFilter = searchQuery.trim() || undefined;

    try {
      const [totalRes, scheduledRes, inProgressRes, completedRes, cancelledRes] = await Promise.all([
        floorballMatchService.getAll({ pageSize: 1, seasonId: seasonFilter, searchQuery: searchFilter }),
        floorballMatchService.getAll({ pageSize: 1, seasonId: seasonFilter, searchQuery: searchFilter, status: FloorballMatchStatus.Scheduled }),
        floorballMatchService.getAll({ pageSize: 1, seasonId: seasonFilter, searchQuery: searchFilter, status: FloorballMatchStatus.InProgress }),
        floorballMatchService.getAll({ pageSize: 1, seasonId: seasonFilter, searchQuery: searchFilter, status: FloorballMatchStatus.Completed }),
        floorballMatchService.getAll({ pageSize: 1, seasonId: seasonFilter, searchQuery: searchFilter, status: FloorballMatchStatus.Cancelled }),
      ]);

      setStatusCounts({
        total: totalRes.pagination?.totalCount ?? 0,
        scheduled: scheduledRes.pagination?.totalCount ?? 0,
        inProgress: inProgressRes.pagination?.totalCount ?? 0,
        completed: completedRes.pagination?.totalCount ?? 0,
        cancelled: cancelledRes.pagination?.totalCount ?? 0,
      });
    } catch (err) {
      console.error('Error fetching status counts:', err);
    }
  }, [selectedSeasonId, searchQuery]);

  // Fetch matches for the current tab
  const fetchMatches = useCallback(async (isInitial = false) => {
    try {
      if (isInitial) {
        setInitialLoading(true);
      } else {
        setTableLoading(true);
      }
      setError(null);

      const seasonFilter = selectedSeasonId || undefined;
      const searchFilter = searchQuery.trim() || undefined;
      const statusFilter = TAB_TO_STATUS[activeTab];

      const [seasonsResponse, matchesResponse] = await Promise.all([
        isInitial ? floorballSeasonService.getAll() : Promise.resolve(null),
        floorballMatchService.getAll({
          page: currentPage,
          pageSize,
          seasonId: seasonFilter,
          searchQuery: searchFilter,
          status: statusFilter,
        }),
      ]);

      if (seasonsResponse?.success && seasonsResponse.data) {
        setSeasons(seasonsResponse.data);
      }

      if (matchesResponse.success && matchesResponse.data) {
        setMatches(matchesResponse.data);
        setTotalCount(matchesResponse.pagination?.totalCount ?? matchesResponse.data.length);
      }
    } catch (err) {
      console.error('Error fetching matches:', err);
      setError(err instanceof Error ? err.message : 'Failed to fetch data');
    } finally {
      setInitialLoading(false);
      setTableLoading(false);
    }
  }, [activeTab, currentPage, pageSize, selectedSeasonId, searchQuery]);

  // Initial load: fetch matches + seasons + counts
  useEffect(() => {
    fetchMatches(true);
    fetchStatusCounts();
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  // Debounced re-fetch when filters/tab/pagination change (skip initial)
  useEffect(() => {
    if (initialLoading) return;

    const timer = setTimeout(() => {
      fetchMatches(false);
      fetchStatusCounts();
    }, 300);

    return () => clearTimeout(timer);
  }, [activeTab, currentPage, pageSize, selectedSeasonId, searchQuery]); // eslint-disable-line react-hooks/exhaustive-deps

  // SignalR real-time updates
  useEffect(() => {
    let unsubscribe: (() => void) | undefined;

    const handleMatchStatusChange = (eventData: MatchEvent) => {
      const { MatchId, NewStatus } = eventData.data as { MatchId: string; NewStatus: string };

      setMatches(prev => prev.map(match => {
        if (match.id === MatchId) {
          return { ...match, status: NewStatus as FloorballMatchDto['status'] };
        }
        return match;
      }));

      // Refresh counts on status change
      fetchStatusCounts();
    };

    const handleSignalREvent = (event: MatchEvent) => {
      if (event.eventType === 'FloorballMatchStatusChangedEvent') {
        handleMatchStatusChange(event);
      }
    };

    const setupSignalR = async () => {
      try {
        await signalRService.connect();
        if (!signalRService.isConnected) return;

        await signalRService.subscribeToEventType('FloorballMatchStatusChangedEvent');
        unsubscribe = signalRService.onMatchEvent(handleSignalREvent);
      } catch (err) {
        console.error('Error setting up SignalR:', err);
      }
    };

    setupSignalR();

    return () => {
      if (unsubscribe) unsubscribe();
      signalRService.unsubscribeFromEventType('FloorballMatchStatusChangedEvent');
    };
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  // Action handlers
  const handleLiveMatch = (match: FloorballMatchDto) => {
    navigate(`/admin/floorball/matches/manage/${match.id}`);
  };

  const handleEditMatch = (match: FloorballMatchDto) => {
    navigate(`/admin/floorball/matches/${match.id}/edit`);
  };

  const handleOpenMatch = (match: FloorballMatchDto) => {
    navigate(`/admin/floorball/matches/manage/${match.id}`);
  };

  const handleStartMatch = (match: FloorballMatchDto) => {
    navigate(`/admin/floorball/matches/manage/${match.id}`);
  };

  const handleCancelMatch = (match: FloorballMatchDto) => {
    setConfirmDialog({ type: 'cancel', match });
  };

  const handleReactivateMatch = (match: FloorballMatchDto) => {
    setConfirmDialog({ type: 'reactivate', match });
  };

  const handleConfirmAction = async () => {
    if (!confirmDialog) return;
    try {
      setDialogLoading(true);
      if (confirmDialog.type === 'cancel') {
        await floorballMatchEventService.cancelMatch(confirmDialog.match.id);
      } else {
        await floorballMatchEventService.reactivateMatch(confirmDialog.match.id);
      }
      setConfirmDialog(null);
      fetchMatches(false);
      fetchStatusCounts();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred');
    } finally {
      setDialogLoading(false);
    }
  };

  const handlePageSizeChange = (newSize: number) => {
    setPageSize(newSize);
    setCurrentPage(1);
  };

  const totalPages = Math.ceil(totalCount / pageSize) || 1;

  if (initialLoading) {
    return (
      <PageTemplate title={t('floorball.matches.title', 'Match Management')}>
        <div className="match-mgmt">
          <LoadingSpinner text={t('floorball.matches.loading', 'Loading matches...')} />
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('floorball.matches.title', 'Match Management')}>
      <div className="match-mgmt">
        {/* Header */}
        <div className="match-mgmt__header">
          <div>
            <h2 className="match-mgmt__title">
              {t('floorball.matches.title', 'Match Management')}
            </h2>
            <p className="match-mgmt__subtitle">
              {t('floorball.matches.subtitle', 'Manage your floorball matches, track live games, and organize your season')}
            </p>
          </div>
          <Button
            iconLeft={AddIcon}
            rounded="pill"
            onClick={() => navigate('/admin/floorball/matches/create')}
          >
            {t('floorball.matches.createNewMatch', 'Create New Match')}
          </Button>
        </div>

        <ErrorPopup message={error} />

        {/* Stats bar */}
        <StatsBar
          stats={statusCounts}
          isSeasonFiltered={!!selectedSeasonId}
        />

        {/* Filter toolbar */}
        <div className="match-mgmt__filters">
          <SearchField
            value={searchQuery}
            onChange={setSearchQuery}
            placeholder={t('floorball.matches.filters.searchPlaceholder', 'Search for team names...')}
            rounded="pill"
            fullWidth
          />
          <div className="match-mgmt__season-filter">
            <label htmlFor="season-filter">
              {t('floorball.matches.filters.filterBySeason', 'Filter by Season:')}
            </label>
            <select
              id="season-filter"
              value={selectedSeasonId}
              onChange={(e) => { setSelectedSeasonId(e.target.value); setCurrentPage(1); }}
              className="match-mgmt__select"
            >
              <option value="">{t('floorball.matches.filters.allSeasons', 'All Seasons')}</option>
              {seasons.map(season => (
                <option key={season.id} value={season.id}>
                  {formatSeasonDisplayName(season)}
                </option>
              ))}
            </select>
          </div>
        </div>

        {/* Status tabs */}
        <StatusTabs
          activeTab={activeTab}
          onTabChange={setActiveTab}
          counts={statusCounts}
        />

        {/* Match table */}
        <div
          id="match-table-panel"
          role="tabpanel"
          aria-labelledby={`tab-${activeTab}`}
          className={tableLoading ? 'match-mgmt__table--dimmed' : ''}
        >
          <MatchTable
            matches={matches}
            loading={tableLoading && matches.length === 0}
            onLiveMatch={handleLiveMatch}
            onEditMatch={handleEditMatch}
            onOpenMatch={handleOpenMatch}
            onStartMatch={handleStartMatch}
            onCancelMatch={handleCancelMatch}
            onReactivateMatch={handleReactivateMatch}
          />
        </div>

        {/* Pagination */}
        {totalCount > 0 && (
          <PaginationControls
            currentPage={currentPage}
            totalPages={totalPages}
            totalCount={totalCount}
            pageSize={pageSize}
            onPageChange={setCurrentPage}
            onPageSizeChange={handlePageSizeChange}
          />
        )}

        {/* Confirmation Dialog */}
        <ConfirmationDialog
          isOpen={confirmDialog !== null}
          icon={confirmDialog?.type === 'cancel' ? '⚠️' : '✅'}
          title={
            confirmDialog?.type === 'cancel'
              ? t('floorball.matches.confirmCancel.title', 'Cancel Match')
              : t('floorball.matches.confirmReactivate.title', 'Reactivate Match')
          }
          message={
            confirmDialog?.type === 'cancel'
              ? t('floorball.matches.confirmCancel.message', 'Are you sure you want to cancel this match? This will mark the match as cancelled.')
              : t('floorball.matches.confirmReactivate.message', 'Are you sure you want to reactivate this match? This will set the match back to Scheduled status.')
          }
          confirmText={
            confirmDialog?.type === 'cancel'
              ? t('floorball.matches.confirmCancel.confirm', 'Yes, Cancel Match')
              : t('floorball.matches.confirmReactivate.confirm', 'Yes, Reactivate Match')
          }
          cancelText={t('common.cancel', 'Cancel')}
          isLoading={dialogLoading}
          onConfirm={handleConfirmAction}
          onCancel={() => setConfirmDialog(null)}
        />
      </div>
    </PageTemplate>
  );
};

export default MatchManagementPage;
