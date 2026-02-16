import { useState, useEffect, useCallback, useMemo } from 'react';
import { floorballMatchService } from '../../../../api/floorball/floorballMatchService';
import { floorballMatchEventService } from '../../../../api/floorball/floorballMatchEventService';
import { floorballSeasonService, type FloorballSeasonDto } from '../../../../api/floorball/floorballSeasonService';
import { signalRService, type MatchEvent } from '../../../../services/signalRService';
import MatchStatsCards from './Components/MatchStatsCards/MatchStatsCards';
import MatchFilters from './Components/MatchFilters/MatchFilters';
import CollapsibleMatchSection from './Components/CollapsibleMatchSection/CollapsibleMatchSection';
import ConfirmationDialog from '../ManageMatchPage/components/ConfirmationDialog';
import type { FloorballMatchDto } from '../../../../types/floorball/floorballTypes';
import { FloorballMatchStatus } from '../../../../types/floorball/floorballTypes';
import './MatchOverviewPage.scss';
import ErrorPopup from '../../../../components/ErrorPopup/ErrorPopup';
import LoadingSpinner from '../../../../components/LoadingSpinner/LoadingSpinner';
import { useTranslation } from 'react-i18next';
import { useNavigate, Link } from 'react-router-dom';
import PageTemplate from '../../../../components/PageTemplate/AdminPageTemplate';

interface MatchStats {
  total: number;
  completed: number;
  scheduled: number;
  inProgress: number;
  cancelled: number;
}

const MatchOverviewPage = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const [matches, setMatches] = useState<FloorballMatchDto[]>([]);
  const [seasons, setSeasons] = useState<FloorballSeasonDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [isFiltering, setIsFiltering] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [selectedSeasonId, setSelectedSeasonId] = useState<string>('');
  const [searchQuery, setSearchQuery] = useState<string>('');

  const [matchStats, setMatchStats] = useState<MatchStats>({
    total: 0, completed: 0, scheduled: 0, inProgress: 0, cancelled: 0,
  });

  const [collapsedSections, setCollapsedSections] = useState({
    ongoing: false,
    scheduled: false,
    completed: false,
    cancelled: false,
  });

  // Compute display-ready match groups (with limits for sections)
  const displayMatches = useMemo(() => {
    const now = new Date();
    const oneWeekFromNow = new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000);

    const ongoing = matches
      .filter(m => m.status === FloorballMatchStatus.InProgress);

    const scheduled = matches
      .filter(m => {
        if (m.status !== FloorballMatchStatus.Scheduled) return false;
        const matchDate = new Date(m.scheduledDateTime);
        return matchDate <= oneWeekFromNow;
      })
      .sort((a, b) => new Date(a.scheduledDateTime).getTime() - new Date(b.scheduledDateTime).getTime());

    const completed = matches
      .filter(m => m.status === FloorballMatchStatus.Completed)
      .sort((a, b) => new Date(b.scheduledDateTime).getTime() - new Date(a.scheduledDateTime).getTime())
      .slice(0, 10);

    const cancelled = matches
      .filter(m => m.status === FloorballMatchStatus.Cancelled)
      .sort((a, b) => new Date(b.scheduledDateTime).getTime() - new Date(a.scheduledDateTime).getTime())
      .slice(0, 10);

    return { ongoing, scheduled, completed, cancelled };
  }, [matches]);

  const fetchData = useCallback(async (isInitialLoad = false) => {
    try {
      if (isInitialLoad) {
        setLoading(true);
      } else {
        setIsFiltering(true);
      }
      setError(null);

      const seasonFilter = selectedSeasonId || undefined;
      const searchFilter = searchQuery.trim() || undefined;

      // Fetch display data (pageSize: 100) + lightweight per-status counts (pageSize: 1)
      // Each status call returns pagination.totalCount for an accurate count
      const [
        seasonsResponse,
        matchesResponse,
        scheduledCountRes,
        inProgressCountRes,
        completedCountRes,
        cancelledCountRes,
      ] = await Promise.all([
        floorballSeasonService.getAll(),
        floorballMatchService.getAll({
          pageSize: 100,
          seasonId: seasonFilter,
          searchQuery: searchFilter,
        }),
        floorballMatchService.getAll({
          pageSize: 1,
          seasonId: seasonFilter,
          searchQuery: searchFilter,
          status: FloorballMatchStatus.Scheduled,
        }),
        floorballMatchService.getAll({
          pageSize: 1,
          seasonId: seasonFilter,
          searchQuery: searchFilter,
          status: FloorballMatchStatus.InProgress,
        }),
        floorballMatchService.getAll({
          pageSize: 1,
          seasonId: seasonFilter,
          searchQuery: searchFilter,
          status: FloorballMatchStatus.Completed,
        }),
        floorballMatchService.getAll({
          pageSize: 1,
          seasonId: seasonFilter,
          searchQuery: searchFilter,
          status: FloorballMatchStatus.Cancelled,
        }),
      ]);

      if (seasonsResponse.success && seasonsResponse.data) {
        setSeasons(seasonsResponse.data);
      }

      if (matchesResponse.success && matchesResponse.data) {
        setMatches(matchesResponse.data);
      }

      // Use pagination.totalCount from each status-filtered call for accurate stats
      setMatchStats({
        total: matchesResponse.pagination?.totalCount ?? matchesResponse.data?.length ?? 0,
        scheduled: scheduledCountRes.pagination?.totalCount ?? 0,
        inProgress: inProgressCountRes.pagination?.totalCount ?? 0,
        completed: completedCountRes.pagination?.totalCount ?? 0,
        cancelled: cancelledCountRes.pagination?.totalCount ?? 0,
      });
    } catch (err) {
      console.error('Error fetching data:', err);
      setError(err instanceof Error ? err.message : 'Failed to fetch data');
    } finally {
      setLoading(false);
      setIsFiltering(false);
    }
  }, [selectedSeasonId, searchQuery]);

  // Initial load
  useEffect(() => {
    fetchData(true);
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  // Debounced fetch for filters
  useEffect(() => {
    if (loading) return;

    const timer = setTimeout(() => {
      fetchData(false);
    }, 500);

    return () => clearTimeout(timer);
  }, [selectedSeasonId, searchQuery]); // eslint-disable-line react-hooks/exhaustive-deps

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
    };

    const handleSignalREvent = (event: MatchEvent) => {
      if (event.eventType === 'FloorballMatchStatusChangedEvent') {
        handleMatchStatusChange(event);
      }
    };

    const setupSignalR = async () => {
      try {
        const isBackendAccessible = await signalRService.testBackendAccessibility();
        if (!isBackendAccessible) return;

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
  }, []);

  const [confirmDialog, setConfirmDialog] = useState<{
    type: 'cancel' | 'reactivate';
    match: FloorballMatchDto;
  } | null>(null);
  const [dialogLoading, setDialogLoading] = useState(false);

  const handleLiveMatch = (match: FloorballMatchDto) => {
    navigate(`/admin/floorball/matches/manage/${match.id}`);
  };

  const handleEditMatch = (match: FloorballMatchDto) => {
    navigate(`/admin/floorball/matches/${match.id}/edit`);
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
      fetchData(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred');
    } finally {
      setDialogLoading(false);
    }
  };

  const toggleSection = (section: keyof typeof collapsedSections) => {
    setCollapsedSections(prev => ({
      ...prev,
      [section]: !prev[section],
    }));
  };

  const hasNoMatches =
    displayMatches.ongoing.length === 0 &&
    displayMatches.scheduled.length === 0 &&
    displayMatches.completed.length === 0 &&
    displayMatches.cancelled.length === 0;

  if (loading) {
    return (
      <PageTemplate title={t('floorball.matches.title', 'Match Management')}>
        <div className="match-overview">
          <LoadingSpinner text={t('floorball.matches.loading', 'Loading matches...')} />
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('floorball.matches.title', 'Match Management')}>
      <div className="match-overview">
        {/* Header */}
        <div className="match-overview__header">
          <div>
            <h2 className="match-overview__title">
              {t('floorball.matches.title', 'Match Management')}
            </h2>
            <p className="match-overview__subtitle">
              {t('floorball.matches.subtitle', 'Manage your floorball matches, track live games, and organize your season')}
            </p>
          </div>
        </div>

        <ErrorPopup message={error} />

        {/* Stats Cards */}
        <MatchStatsCards
          stats={matchStats}
          isSeasonFiltered={!!selectedSeasonId}
          onCompletedClick={() => navigate('/admin/floorball/matches/completed')}
          onScheduledClick={() => navigate('/admin/floorball/matches/scheduled')}
          onInProgressClick={() => navigate('/admin/floorball/matches/in-progress')}
          onCancelledClick={() => navigate('/admin/floorball/matches/cancelled')}
        />

        {/* Filters */}
        <MatchFilters
          seasons={seasons}
          selectedSeasonId={selectedSeasonId}
          onSeasonChange={setSelectedSeasonId}
          searchQuery={searchQuery}
          onSearchChange={setSearchQuery}
          onCreateNew={() => navigate('/admin/floorball/matches/create')}
        />

        {/* Filtering indicator */}
        {isFiltering && (
          <div className="match-overview__filtering">
            {t('common.loading', 'Loading...')}
          </div>
        )}

        {/* Match Sections */}
        <div
          className="match-overview__sections"
          style={{ opacity: isFiltering ? 0.6 : 1, transition: 'opacity 0.2s' }}
        >
          <CollapsibleMatchSection
            title={`${t('floorball.matches.sections.ongoing', 'Ongoing Matches')} (${displayMatches.ongoing.length})`}
            matches={displayMatches.ongoing}
            isCollapsed={collapsedSections.ongoing}
            onToggleCollapse={() => toggleSection('ongoing')}
            onLiveMatch={handleLiveMatch}
            onEditMatch={handleEditMatch}
            onCancelMatch={handleCancelMatch}
            onReactivateMatch={handleReactivateMatch}
            sectionType="ongoing"
          />

          <CollapsibleMatchSection
            title={`${t('floorball.matches.sections.scheduled', 'Scheduled Matches')} (${displayMatches.scheduled.length})`}
            matches={displayMatches.scheduled}
            isCollapsed={collapsedSections.scheduled}
            onToggleCollapse={() => toggleSection('scheduled')}
            onLiveMatch={handleLiveMatch}
            onEditMatch={handleEditMatch}
            onCancelMatch={handleCancelMatch}
            onReactivateMatch={handleReactivateMatch}
            sectionType="scheduled"
          />

          <CollapsibleMatchSection
            title={`${t('floorball.matches.sections.completed', 'Completed Matches')} (${displayMatches.completed.length})`}
            matches={displayMatches.completed}
            isCollapsed={collapsedSections.completed}
            onToggleCollapse={() => toggleSection('completed')}
            onLiveMatch={handleLiveMatch}
            onEditMatch={handleEditMatch}
            onCancelMatch={handleCancelMatch}
            onReactivateMatch={handleReactivateMatch}
            sectionType="completed"
          />

          <CollapsibleMatchSection
            title={`${t('floorball.matches.sections.cancelled', 'Cancelled Matches')} (${displayMatches.cancelled.length})`}
            matches={displayMatches.cancelled}
            isCollapsed={collapsedSections.cancelled}
            onToggleCollapse={() => toggleSection('cancelled')}
            onLiveMatch={handleLiveMatch}
            onEditMatch={handleEditMatch}
            onCancelMatch={handleCancelMatch}
            onReactivateMatch={handleReactivateMatch}
            sectionType="cancelled"
          />

          {hasNoMatches && (
            <div className="match-overview__empty">
              <h3>
                {selectedSeasonId
                  ? t('floorball.matches.noMatchesForSeason', 'No matches found for the selected season')
                  : t('floorball.matches.noMatchesFound', 'No matches found')}
              </h3>
              <p>
                {!selectedSeasonId && t('floorball.matches.createFirstMatch', 'Create your first match to get started')}
              </p>
              <Link to="/admin/floorball/matches/create" className="btn btn--primary btn--pill">
                {t('floorball.matches.createNewMatch', 'Create New Match')}
              </Link>
            </div>
          )}
        </div>

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

export default MatchOverviewPage;
