import { useCallback, useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { floorballMatchService } from '../../api/floorball/floorballMatchService';
import type { FloorballMatchDto } from '../../types/floorball/floorballTypes';
import { FloorballMatchStatus } from '../../types/floorball/floorballTypes';
import { useAudience } from '../../context/AudienceContext';
import MatchPanelCard from './MatchPanelCard';
import LoadingSpinner from '../LoadingSpinner/LoadingSpinner';
import './MatchesPanel.scss';

// How many upcoming / completed matches to show initially. Live matches are
// always shown in full because there are typically very few of them and the
// user wants to see every game that is currently being played.
const INITIAL_VISIBLE = 5;
// How many extra matches to fetch every time the user presses "Show more".
const LOAD_MORE_STEP = 10;
// Upper bound to avoid runaway pagination on misconfigured backends.
const MAX_TOTAL = 200;

interface PaginatedSectionState {
  matches: FloorballMatchDto[];
  /** Total number of matches the backend says exist for this filter. */
  totalCount: number;
  /** How many of the loaded matches are currently rendered. */
  visibleCount: number;
  isLoadingMore: boolean;
}

function MatchesPanel() {
  const { t } = useTranslation();
  const { audience } = useAudience();
  const teamCategory = audience.teamCategory;

  const [liveMatches, setLiveMatches] = useState<FloorballMatchDto[]>([]);
  const [upcoming, setUpcoming] = useState<PaginatedSectionState>({
    matches: [],
    totalCount: 0,
    visibleCount: 0,
    isLoadingMore: false,
  });
  const [completed, setCompleted] = useState<PaginatedSectionState>({
    matches: [],
    totalCount: 0,
    visibleCount: 0,
    isLoadingMore: false,
  });

  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchInitial = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);

      // Three independent queries so each section honours its own filters
      // (status + date + sort), rather than slicing a single mixed page that
      // could be dominated by old completed matches.
      const [liveResponse, upcomingResponse, completedResponse] = await Promise.all([
        floorballMatchService.getAll({
          status: FloorballMatchStatus.InProgress,
          sortOrder: 'asc',
          pageSize: 100,
          teamCategory,
        }),
        floorballMatchService.getAll({
          status: FloorballMatchStatus.Scheduled,
          sortOrder: 'asc',
          pageSize: INITIAL_VISIBLE,
          teamCategory,
        }),
        floorballMatchService.getAll({
          status: FloorballMatchStatus.Completed,
          sortOrder: 'desc',
          pageSize: INITIAL_VISIBLE,
          teamCategory,
        }),
      ]);

      const liveList = liveResponse.success && liveResponse.data ? liveResponse.data : [];
      setLiveMatches(liveList);

      const upcomingList =
        upcomingResponse.success && upcomingResponse.data ? upcomingResponse.data : [];
      setUpcoming({
        matches: upcomingList,
        totalCount: upcomingResponse.pagination?.totalCount ?? upcomingList.length,
        visibleCount: upcomingList.length,
        isLoadingMore: false,
      });

      const completedList =
        completedResponse.success && completedResponse.data ? completedResponse.data : [];
      setCompleted({
        matches: completedList,
        totalCount: completedResponse.pagination?.totalCount ?? completedList.length,
        visibleCount: completedList.length,
        isLoadingMore: false,
      });
    } catch (err) {
      console.error('MatchesPanel: fetch failed', err);
      setError(t('sidebar.error', 'Otteluiden lataus epäonnistui'));
    } finally {
      setIsLoading(false);
    }
  }, [t, teamCategory]);

  useEffect(() => {
    fetchInitial();
  }, [fetchInitial]);

  const loadMore = useCallback(
    async (kind: 'upcoming' | 'completed') => {
      const current = kind === 'upcoming' ? upcoming : completed;
      const setSection = kind === 'upcoming' ? setUpcoming : setCompleted;

      const nextSize = Math.min(current.visibleCount + LOAD_MORE_STEP, MAX_TOTAL);
      // If we already have that many in memory we can just reveal them.
      if (current.matches.length >= nextSize) {
        setSection({ ...current, visibleCount: nextSize });
        return;
      }

      setSection({ ...current, isLoadingMore: true });
      try {
        const response = await floorballMatchService.getAll({
          status:
            kind === 'upcoming' ? FloorballMatchStatus.Scheduled : FloorballMatchStatus.Completed,
          sortOrder: kind === 'upcoming' ? 'asc' : 'desc',
          pageSize: nextSize,
          teamCategory,
        });

        if (response.success && response.data) {
          setSection({
            matches: response.data,
            totalCount: response.pagination?.totalCount ?? response.data.length,
            visibleCount: Math.min(response.data.length, nextSize),
            isLoadingMore: false,
          });
        } else {
          setSection({ ...current, isLoadingMore: false });
        }
      } catch (err) {
        console.error(`MatchesPanel: load more (${kind}) failed`, err);
        setSection({ ...current, isLoadingMore: false });
      }
    },
    [upcoming, completed, teamCategory]
  );

  const collapse = useCallback((kind: 'upcoming' | 'completed') => {
    const setSection = kind === 'upcoming' ? setUpcoming : setCompleted;
    setSection((prev) => ({ ...prev, visibleCount: INITIAL_VISIBLE }));
  }, []);

  const upcomingVisible = useMemo(
    () => upcoming.matches.slice(0, upcoming.visibleCount),
    [upcoming]
  );
  const completedVisible = useMemo(
    () => completed.matches.slice(0, completed.visibleCount),
    [completed]
  );

  // --- Loading ---
  if (isLoading) {
    return (
      <div className="matches-panel">
        <div className="matches-panel__state">
          <LoadingSpinner size="sm" text={t('sidebar.loading', 'Ladataan...')} />
        </div>
      </div>
    );
  }

  // --- Error ---
  if (error) {
    return (
      <div className="matches-panel">
        <div className="matches-panel__state">
          <p>{error}</p>
          <button
            type="button"
            className="matches-panel__retry-btn"
            onClick={fetchInitial}
          >
            {t('common.retry', 'Yritä uudelleen')}
          </button>
        </div>
      </div>
    );
  }

  const renderExpandControls = (
    kind: 'upcoming' | 'completed',
    section: PaginatedSectionState
  ) => {
    const remaining = Math.max(section.totalCount - section.visibleCount, 0);
    const canLoadMore = remaining > 0;
    const canCollapse = section.visibleCount > INITIAL_VISIBLE;
    if (!canLoadMore && !canCollapse) return null;

    const nextChunk = Math.min(remaining, LOAD_MORE_STEP);

    return (
      <div className="matches-panel__controls">
        {canLoadMore && (
          <button
            type="button"
            className="matches-panel__more-btn"
            onClick={() => loadMore(kind)}
            disabled={section.isLoadingMore}
          >
            {section.isLoadingMore
              ? t('sidebar.loadingMore', 'Ladataan lisää...')
              : t('sidebar.showMore', 'Näytä lisää ({{count}})', { count: nextChunk })}
          </button>
        )}
        {canCollapse && !section.isLoadingMore && (
          <button
            type="button"
            className="matches-panel__more-btn matches-panel__more-btn--ghost"
            onClick={() => collapse(kind)}
          >
            {t('sidebar.showLess', 'Näytä vähemmän')}
          </button>
        )}
      </div>
    );
  };

  return (
    <div className="matches-panel">
      {/* --- LIVE: always show all --- */}
      <div className="matches-panel__section">
        <div className="matches-panel__section-header matches-panel__section-header--live">
          <span className="pulse-dot" />
          <h3 className="matches-panel__section-title">
            {t('sidebar.liveMatches', 'Käynnissä')}
          </h3>
          {liveMatches.length > 0 && (
            <span className="matches-panel__section-count">({liveMatches.length})</span>
          )}
        </div>

        {liveMatches.length > 0 ? (
          liveMatches.map((match) => <MatchPanelCard key={match.id} match={match} />)
        ) : (
          <p className="matches-panel__empty-text">
            {t('sidebar.noLiveMatches', 'Ei käynnissä olevia otteluita')}
          </p>
        )}
      </div>

      {/* --- UPCOMING: expandable --- */}
      <div className="matches-panel__section">
        <div className="matches-panel__section-header">
          <h3 className="matches-panel__section-title">
            {t('sidebar.upcomingMatches', 'Tulevat')}
          </h3>
          {upcoming.totalCount > 0 && (
            <span className="matches-panel__section-count">
              ({upcoming.visibleCount}/{upcoming.totalCount})
            </span>
          )}
        </div>

        {upcomingVisible.length > 0 ? (
          <>
            {upcomingVisible.map((match) => (
              <MatchPanelCard key={match.id} match={match} />
            ))}
            {renderExpandControls('upcoming', upcoming)}
          </>
        ) : (
          <p className="matches-panel__empty-text">
            {t('sidebar.noUpcomingMatches', 'Ei tulevia otteluita')}
          </p>
        )}
      </div>

      {/* --- COMPLETED: expandable --- */}
      <div className="matches-panel__section">
        <div className="matches-panel__section-header">
          <h3 className="matches-panel__section-title">
            {t('sidebar.completedMatches', 'Päättyneet')}
          </h3>
          {completed.totalCount > 0 && (
            <span className="matches-panel__section-count">
              ({completed.visibleCount}/{completed.totalCount})
            </span>
          )}
        </div>

        {completedVisible.length > 0 ? (
          <>
            {completedVisible.map((match) => (
              <MatchPanelCard key={match.id} match={match} />
            ))}
            {renderExpandControls('completed', completed)}
          </>
        ) : (
          <p className="matches-panel__empty-text">
            {t('sidebar.noCompletedMatches', 'Ei päättyneitä otteluita')}
          </p>
        )}
      </div>
    </div>
  );
}

export default MatchesPanel;
