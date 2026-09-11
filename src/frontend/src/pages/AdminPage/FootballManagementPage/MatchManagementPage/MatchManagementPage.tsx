import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { footballMatchService } from '../../../../api/football/footballMatchService';
import { footballMatchEventService } from '../../../../api/football/footballMatchEventService';
import { footballSeasonService, type FootballSeasonDto } from '../../../../api/football/footballSeasonService';
import { footballTournamentService } from '../../../../api/football/footballTournamentService';
import type { FootballTournamentDto } from '../../../../types/football/tournamentTypes';
import { signalRService, type MatchEvent } from '../../../../services/signalRService';
import { FootballMatchStatus, type FootballCompetitionType } from '../../../../types/football/footballTypes';
import type { FootballMatchDto } from '../../../../types/football/footballTypes';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import LoadingSpinner from '../../../../components/LoadingSpinner/LoadingSpinner';
import ConfirmationDialog from '../ManageMatchPage/components/ConfirmationDialog';
import SearchField from '../../../../components/SearchField/SearchField';
import Button from '../../../../components/Button/Button';
import AddIcon from '../../../../assets/basicIcons/add.svg';
import { formatSeasonDisplayName } from '../ManageMatchPage/utils/matchFormatters';
import PaginationControls from '../FootballPlayersPage/components/PaginationControls';
import StatusTabs from './components/StatusTabs/StatusTabs';
import type { MatchTab, StatusCounts } from './components/StatusTabs/StatusTabs';
import StatsBar from './components/StatsBar/StatsBar';
import MatchTable from './components/MatchTable/MatchTable';
import './MatchManagementPage.scss';

const TAB_TO_STATUS: Record<MatchTab, FootballMatchStatus | undefined> = {
  all: undefined,
  ongoing: FootballMatchStatus.InProgress,
  scheduled: FootballMatchStatus.Scheduled,
  completed: FootballMatchStatus.Completed,
  cancelled: FootballMatchStatus.Cancelled,
};

const VALID_TABS: MatchTab[] = ['all', 'ongoing', 'scheduled', 'completed', 'cancelled'];

const isValidTab = (value: string | null): value is MatchTab =>
  value !== null && VALID_TABS.includes(value as MatchTab);

export type MatchManagementMode = 'all' | 'season' | 'tournament';

interface MatchManagementPageProps {
  mode?: MatchManagementMode;
}

const MatchManagementPage = ({ mode = 'all' }: MatchManagementPageProps) => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();

  const competitionType: FootballCompetitionType | undefined =
    mode === 'season' ? 'Season' : mode === 'tournament' ? 'Tournament' : undefined;

  // Tab state from URL
  const urlTab = searchParams.get('tab');
  const activeTab: MatchTab = isValidTab(urlTab) ? urlTab : 'all';

  // Competition filter is mirrored in the URL (?competitionId=...) so callers like
  // EditTournamentPage and the tournament list's kebab menu can deep-link straight to a
  // pre-filtered match list. Empty string means "All Competitions / Tournaments / Seasons".
  const urlCompetitionId: string = searchParams.get('competitionId') ?? '';

  const setActiveTab = (tab: MatchTab) => {
    setSearchParams(
      (prev) => {
        const next = new URLSearchParams(prev);
        if (tab === 'all') {
          next.delete('tab');
        } else {
          next.set('tab', tab);
        }
        return next;
      },
      { replace: true },
    );
    setCurrentPage(1);
  };

  // Data state
  const [matches, setMatches] = useState<FootballMatchDto[]>([]);
  const [seasons, setSeasons] = useState<FootballSeasonDto[]>([]);
  const [tournaments, setTournaments] = useState<FootballTournamentDto[]>([]);
  const [initialLoading, setInitialLoading] = useState(true);
  const [tableLoading, setTableLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Filter state — `selectedCompetitionId` is the source of truth for the dropdown; we mirror
  // it back to the URL so refreshes / deep links keep the filter applied.
  const [selectedCompetitionId, setSelectedCompetitionId] = useState<string>(urlCompetitionId);
  const [searchQuery, setSearchQuery] = useState<string>('');

  // Sync external URL changes (e.g. user navigates from another page with `?competitionId=...`)
  // into the local select state. We compare against the current local value so we don't fight
  // with the user typing into the dropdown.
  useEffect(() => {
    if (urlCompetitionId !== selectedCompetitionId) {
      setSelectedCompetitionId(urlCompetitionId);
      setCurrentPage(1);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [urlCompetitionId]);

  const handleCompetitionFilterChange = (value: string): void => {
    setSelectedCompetitionId(value);
    setCurrentPage(1);
    setSearchParams(
      (prev) => {
        const next = new URLSearchParams(prev);
        if (value) {
          next.set('competitionId', value);
        } else {
          next.delete('competitionId');
        }
        return next;
      },
      { replace: true },
    );
  };

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
    match: FootballMatchDto;
  } | null>(null);
  const [dialogLoading, setDialogLoading] = useState(false);

  // Fetch status counts (lightweight queries)
  const fetchStatusCounts = useCallback(async () => {
    const seasonFilter = selectedCompetitionId || undefined;
    const searchFilter = searchQuery.trim() || undefined;
    const baseFilters = { competitionId: seasonFilter, searchQuery: searchFilter, competitionType };

    try {
      const [totalRes, scheduledRes, inProgressRes, completedRes, cancelledRes] = await Promise.all([
        footballMatchService.getAll({ pageSize: 1, ...baseFilters }),
        footballMatchService.getAll({ pageSize: 1, ...baseFilters, status: FootballMatchStatus.Scheduled }),
        footballMatchService.getAll({ pageSize: 1, ...baseFilters, status: FootballMatchStatus.InProgress }),
        footballMatchService.getAll({ pageSize: 1, ...baseFilters, status: FootballMatchStatus.Completed }),
        footballMatchService.getAll({ pageSize: 1, ...baseFilters, status: FootballMatchStatus.Cancelled }),
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
  }, [selectedCompetitionId, searchQuery, competitionType]);

  // Fetch matches for the current tab
  const fetchMatches = useCallback(async (isInitial = false) => {
    try {
      if (isInitial) {
        setInitialLoading(true);
      } else {
        setTableLoading(true);
      }
      setError(null);

      const seasonFilter = selectedCompetitionId || undefined;
      const searchFilter = searchQuery.trim() || undefined;
      const statusFilter = TAB_TO_STATUS[activeTab];

      const [seasonsResponse, tournamentsResponse, matchesResponse] = await Promise.all([
        isInitial && (mode === 'season' || mode === 'all') ? footballSeasonService.getAll() : Promise.resolve(null),
        isInitial && (mode === 'tournament' || mode === 'all') ? footballTournamentService.getAll() : Promise.resolve(null),
        footballMatchService.getAll({
          page: currentPage,
          pageSize,
          competitionId: seasonFilter,
          searchQuery: searchFilter,
          status: statusFilter,
          competitionType,
        }),
      ]);

      if (seasonsResponse?.success && seasonsResponse.data) {
        setSeasons(seasonsResponse.data);
      }

      if (tournamentsResponse?.success && tournamentsResponse.data) {
        setTournaments(tournamentsResponse.data);
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
  }, [activeTab, currentPage, pageSize, selectedCompetitionId, searchQuery, mode, competitionType]);

  // Reset filters/state when switching modes (e.g. /seasons/matches -> /tournaments/matches).
  // Preserve any `competitionId` from the URL on the first render after a mode switch so deep
  // links continue to work — only clear it if it doesn't match the current mode (e.g. coming
  // from /all into /tournaments with a season id wouldn't make sense, but we don't validate
  // that here; the dropdown will just not have it as an option and show "All ...").
  useEffect(() => {
    setSelectedCompetitionId(urlCompetitionId);
    setSearchQuery('');
    setCurrentPage(1);
    setInitialLoading(true);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [mode]);

  // Initial load: fetch matches + seasons + counts
  useEffect(() => {
    fetchMatches(true);
    fetchStatusCounts();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [mode]);

  // Debounced re-fetch when filters/tab/pagination change (skip initial)
  useEffect(() => {
    if (initialLoading) return;

    const timer = setTimeout(() => {
      fetchMatches(false);
      fetchStatusCounts();
    }, 300);

    return () => clearTimeout(timer);
  }, [activeTab, currentPage, pageSize, selectedCompetitionId, searchQuery]); // eslint-disable-line react-hooks/exhaustive-deps

  // SignalR real-time updates
  useEffect(() => {
    let unsubscribe: (() => void) | undefined;

    const handleMatchStatusChange = (eventData: MatchEvent) => {
      const { MatchId, NewStatus } = eventData.data as { MatchId: string; NewStatus: string };

      setMatches(prev => prev.map(match => {
        if (match.id === MatchId) {
          return { ...match, status: NewStatus as FootballMatchDto['status'] };
        }
        return match;
      }));

      // Refresh counts on status change
      fetchStatusCounts();
    };

    const handleSignalREvent = (event: MatchEvent) => {
      if (event.eventType === 'FootballMatchStatusChangedEvent') {
        handleMatchStatusChange(event);
      }
    };

    const setupSignalR = async () => {
      try {
        await signalRService.connect();
        if (!signalRService.isConnected) return;

        await signalRService.subscribeToEventType('FootballMatchStatusChangedEvent');
        unsubscribe = signalRService.onMatchEvent(handleSignalREvent);
      } catch (err) {
        console.error('Error setting up SignalR:', err);
      }
    };

    setupSignalR();

    return () => {
      if (unsubscribe) unsubscribe();
      signalRService.unsubscribeFromEventType('FootballMatchStatusChangedEvent');
    };
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  // Action handlers
  const handleLiveMatch = (match: FootballMatchDto) => {
    navigate(`/admin/football/matches/manage/${match.id}`);
  };

  const handleEditMatch = (match: FootballMatchDto) => {
    navigate(`/admin/football/matches/${match.id}/edit`);
  };

  const handleOpenMatch = (match: FootballMatchDto) => {
    navigate(`/admin/football/matches/manage/${match.id}`);
  };

  const handleStartMatch = (match: FootballMatchDto) => {
    navigate(`/admin/football/matches/manage/${match.id}`);
  };

  const handleCancelMatch = (match: FootballMatchDto) => {
    setConfirmDialog({ type: 'cancel', match });
  };

  const handleReactivateMatch = (match: FootballMatchDto) => {
    setConfirmDialog({ type: 'reactivate', match });
  };

  const handleConfirmAction = async () => {
    if (!confirmDialog) return;
    try {
      setDialogLoading(true);
      if (confirmDialog.type === 'cancel') {
        await footballMatchEventService.cancelMatch(confirmDialog.match.id);
      } else {
        await footballMatchEventService.reactivateMatch(confirmDialog.match.id);
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

  const pageTitle =
    mode === 'season'
      ? t('football.matches.seasonTitle', 'Season Match Management')
      : mode === 'tournament'
      ? t('football.matches.tournamentTitle', 'Tournament Match Management')
      : t('football.matches.title', 'Match Management');

  const pageSubtitle =
    mode === 'season'
      ? t('football.matches.seasonSubtitle', 'Manage matches scheduled within league seasons')
      : mode === 'tournament'
      ? t('football.matches.tournamentSubtitle', 'Manage matches scheduled within tournaments')
      : t('football.matches.subtitle', 'Manage your football matches, track live games, and organize your season');

  if (initialLoading) {
    return (
      <PageTemplate title={pageTitle}>
        <div className="match-mgmt">
          <LoadingSpinner text={t('football.matches.loading', 'Loading matches...')} />
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={pageTitle}>
      <div className="match-mgmt">
        {/* Header */}
        <div className="match-mgmt__header">
          <div>
            <h2 className="match-mgmt__title">
              {pageTitle}
            </h2>
            <p className="match-mgmt__subtitle">
              {pageSubtitle}
            </p>
          </div>
          <Button
            iconLeft={AddIcon}
            rounded="pill"
            onClick={() => {
              // Route to the mode-specific create page so the form shows the
              // right competition picker (Tournament+Group vs Season+Division).
              const createPath =
                mode === 'tournament'
                  ? '/admin/football/tournaments/matches/create'
                  : mode === 'season'
                    ? '/admin/football/seasons/matches/create'
                    : '/admin/football/matches/create';
              navigate(createPath);
            }}
          >
            {t('football.matches.createNewMatch', 'Create New Match')}
          </Button>
        </div>

        <ErrorPopup message={error} />

        {/* Stats bar */}
        <StatsBar
          stats={statusCounts}
          isSeasonFiltered={!!selectedCompetitionId}
        />

        {/* Filter toolbar */}
        <div className="match-mgmt__filters">
          <SearchField
            value={searchQuery}
            onChange={setSearchQuery}
            placeholder={t('football.matches.filters.searchPlaceholder', 'Search for team names...')}
            rounded="pill"
            fullWidth
          />
          <div className="match-mgmt__season-filter">
            <label htmlFor="competition-filter">
              {mode === 'tournament'
                ? t('football.matches.filters.filterByTournament', 'Filter by Tournament:')
                : t('football.matches.filters.filterBySeason', 'Filter by Season:')}
            </label>
            <select
              id="competition-filter"
              value={selectedCompetitionId}
              onChange={(e) => handleCompetitionFilterChange(e.target.value)}
              className="match-mgmt__select"
            >
              <option value="">
                {mode === 'tournament'
                  ? t('football.matches.filters.allTournaments', 'All Tournaments')
                  : mode === 'season'
                  ? t('football.matches.filters.allSeasons', 'All Seasons')
                  : t('football.matches.filters.allCompetitions', 'All Competitions')}
              </option>
              {(mode === 'season' || mode === 'all') && seasons.map(season => (
                <option key={season.id} value={season.id}>
                  {formatSeasonDisplayName(season)}
                </option>
              ))}
              {(mode === 'tournament' || mode === 'all') && tournaments.map(tournament => (
                <option key={tournament.id} value={tournament.id}>
                  {tournament.name}
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
              ? t('football.matches.confirmCancel.title', 'Cancel Match')
              : t('football.matches.confirmReactivate.title', 'Reactivate Match')
          }
          message={
            confirmDialog?.type === 'cancel'
              ? t('football.matches.confirmCancel.message', 'Are you sure you want to cancel this match? This will mark the match as cancelled.')
              : t('football.matches.confirmReactivate.message', 'Are you sure you want to reactivate this match? This will set the match back to Scheduled status.')
          }
          confirmText={
            confirmDialog?.type === 'cancel'
              ? t('football.matches.confirmCancel.confirm', 'Yes, Cancel Match')
              : t('football.matches.confirmReactivate.confirm', 'Yes, Reactivate Match')
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
