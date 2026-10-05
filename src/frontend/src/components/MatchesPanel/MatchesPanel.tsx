import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useAudience } from '../../context/AudienceContext';
import { SportsCategory } from '../../types/common/sports';
import type { SportKind } from '../../utils/sportRoutes';
import LoadingSpinner from '../LoadingSpinner/LoadingSpinner';
import SportIcon from '../SportIcon/SportIcon';
import MatchPanelCard from './MatchPanelCard';
import {
  loadPanelSection,
  sportsForFilter,
  type PanelMatch,
  type PanelSectionKind,
  type SportFilter,
} from './panelMatch';
import './MatchesPanel.scss';

// How many upcoming / completed matches to show initially. Live matches are
// always shown in full because there are typically very few of them and the
// user wants to see every game that is currently being played.
const INITIAL_VISIBLE = 5;
// On narrow screens the panel sits above the news list, so keep it shorter.
const INITIAL_VISIBLE_COMPACT = 3;
const COMPACT_MEDIA_QUERY = '(max-width: 768px)';

function getInitialVisible(): number {
  if (typeof window === 'undefined' || typeof window.matchMedia !== 'function') {
    return INITIAL_VISIBLE;
  }
  return window.matchMedia(COMPACT_MEDIA_QUERY).matches ? INITIAL_VISIBLE_COMPACT : INITIAL_VISIBLE;
}
// How many extra matches to fetch every time the user presses "Show more".
const LOAD_MORE_STEP = 10;
// Upper bound to avoid runaway pagination on misconfigured backends.
const MAX_TOTAL = 200;
const LIVE_PAGE_SIZE = 100;

const SPORT_FILTERS: { id: SportKind; labelKey: string; icon: SportsCategory }[] = [
  { id: 'floorball', labelKey: 'sports.floorball', icon: SportsCategory.Floorball },
  { id: 'football', labelKey: 'sports.football', icon: SportsCategory.Football },
  { id: 'hockey', labelKey: 'sports.iceHockey', icon: SportsCategory.Icehockey },
];

interface PaginatedSectionState {
  matches: PanelMatch[];
  /** Total number of matches the backends say exist for this filter. */
  totalCount: number;
  /** How many of the loaded matches are currently rendered. */
  visibleCount: number;
  isLoadingMore: boolean;
}

const EMPTY_SECTION: PaginatedSectionState = {
  matches: [],
  totalCount: 0,
  visibleCount: 0,
  isLoadingMore: false,
};

interface MatchFilterChipProps {
  label: string;
  pressed: boolean;
  onSelect: () => void;
  icon?: SportsCategory;
}

function MatchFilterChip({ label, pressed, onSelect, icon }: MatchFilterChipProps) {
  return (
    <button
      type="button"
      className={`matches-panel-filter__chip${pressed ? ' matches-panel-filter__chip--active' : ''}`}
      aria-pressed={pressed}
      onClick={onSelect}
    >
      {icon ? (
        <SportIcon sport={icon} size="sm" className="matches-panel-filter__sport-icon" inverted={pressed} decorative />
      ) : (
        <span className="matches-panel-filter__dot" aria-hidden="true" />
      )}
      {label}
    </button>
  );
}

function MatchesPanel() {
  const { t } = useTranslation();
  const { audience } = useAudience();
  const teamCategory = audience.teamCategory;
  const fetchGeneration = useRef(0);
  const [initialVisible] = useState<number>(getInitialVisible);

  const [sportFilter, setSportFilter] = useState<SportFilter>('all');
  const [filtersOpen, setFiltersOpen] = useState(false);
  const filtersActive = sportFilter !== 'all';

  const [liveMatches, setLiveMatches] = useState<PanelMatch[]>([]);
  const [upcoming, setUpcoming] = useState<PaginatedSectionState>(EMPTY_SECTION);
  const [completed, setCompleted] = useState<PaginatedSectionState>(EMPTY_SECTION);

  const [isLoading, setIsLoading] = useState(true);
  // Keeps the previous lists on screen while a filter change refetches, so the filter
  // toggle in the section header does not disappear mid-interaction.
  const [hasLoaded, setHasLoaded] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const fetchInitial = useCallback(async () => {
    const generation = ++fetchGeneration.current;
    try {
      setIsLoading(true);
      setError(null);

      const sports = sportsForFilter(sportFilter);
      const [liveResult, upcomingResult, completedResult] = await Promise.allSettled([
        loadPanelSection(sports, 'live', LIVE_PAGE_SIZE, teamCategory),
        loadPanelSection(sports, 'upcoming', initialVisible, teamCategory),
        loadPanelSection(sports, 'completed', initialVisible, teamCategory),
      ]);

      if (generation !== fetchGeneration.current) {
        return;
      }

      if (
        liveResult.status === 'rejected'
        && upcomingResult.status === 'rejected'
        && completedResult.status === 'rejected'
      ) {
        console.error('MatchesPanel: fetch failed', upcomingResult.reason);
        setError(t('sidebar.error', 'Otteluiden lataus epäonnistui'));
        setLiveMatches([]);
        setUpcoming(EMPTY_SECTION);
        setCompleted(EMPTY_SECTION);
        return;
      }

      setLiveMatches(liveResult.status === 'fulfilled' ? liveResult.value.matches : []);

      if (upcomingResult.status === 'fulfilled') {
        const page = upcomingResult.value;
        setUpcoming({
          matches: page.matches,
          totalCount: page.totalCount,
          visibleCount: page.matches.length,
          isLoadingMore: false,
        });
      } else {
        console.error('MatchesPanel: upcoming fetch failed', upcomingResult.reason);
        setUpcoming(EMPTY_SECTION);
      }

      if (completedResult.status === 'fulfilled') {
        const page = completedResult.value;
        setCompleted({
          matches: page.matches,
          totalCount: page.totalCount,
          visibleCount: page.matches.length,
          isLoadingMore: false,
        });
      } else {
        console.error('MatchesPanel: completed fetch failed', completedResult.reason);
        setCompleted(EMPTY_SECTION);
      }
    } catch (err) {
      if (generation !== fetchGeneration.current) {
        return;
      }
      console.error('MatchesPanel: fetch failed', err);
      setError(t('sidebar.error', 'Otteluiden lataus epäonnistui'));
    } finally {
      if (generation === fetchGeneration.current) {
        setIsLoading(false);
        setHasLoaded(true);
      }
    }
  }, [t, teamCategory, sportFilter, initialVisible]);

  useEffect(() => {
    void fetchInitial();
  }, [fetchInitial]);

  const loadMore = useCallback(
    async (kind: 'upcoming' | 'completed') => {
      const generation = fetchGeneration.current;
      const current = kind === 'upcoming' ? upcoming : completed;
      const setSection = kind === 'upcoming' ? setUpcoming : setCompleted;
      const nextSize = Math.min(current.visibleCount + LOAD_MORE_STEP, MAX_TOTAL);

      setSection({ ...current, isLoadingMore: true });
      try {
        const page = await loadPanelSection(
          sportsForFilter(sportFilter),
          kind,
          nextSize,
          teamCategory,
        );
        if (generation !== fetchGeneration.current) {
          return;
        }
        setSection({
          matches: page.matches,
          totalCount: page.totalCount,
          visibleCount: page.matches.length,
          isLoadingMore: false,
        });
      } catch (err) {
        console.error(`MatchesPanel: load more (${kind}) failed`, err);
        if (generation !== fetchGeneration.current) {
          return;
        }
        setSection({ ...current, isLoadingMore: false });
      }
    },
    [upcoming, completed, teamCategory, sportFilter],
  );

  const collapse = useCallback((kind: 'upcoming' | 'completed') => {
    const setSection = kind === 'upcoming' ? setUpcoming : setCompleted;
    setSection((prev) => ({ ...prev, visibleCount: Math.min(initialVisible, prev.matches.length) }));
  }, [initialVisible]);

  const upcomingVisible = useMemo(
    () => upcoming.matches.slice(0, upcoming.visibleCount),
    [upcoming],
  );
  const completedVisible = useMemo(
    () => completed.matches.slice(0, completed.visibleCount),
    [completed],
  );

  const filters = filtersOpen ? (
    <div className="matches-panel-filter">
      <div className="matches-panel-filter__group" role="group" aria-label={t('homePage.newsSection.filters.sport', 'Laji')}>
        <MatchFilterChip
          label={t('homePage.newsSection.filters.allSports', 'Kaikki lajit')}
          pressed={sportFilter === 'all'}
          onSelect={() => setSportFilter('all')}
        />
        {SPORT_FILTERS.map((sport) => (
          <MatchFilterChip
            key={sport.id}
            label={t(sport.labelKey)}
            pressed={sportFilter === sport.id}
            icon={sport.icon}
            onSelect={() => setSportFilter(sport.id)}
          />
        ))}
      </div>
    </div>
  ) : null;

  const filterToggleLabel: string = t('homePage.newsSection.filters.toggle', 'Suodata');
  const filterToggle = (
    <button
      type="button"
      className={`matches-panel__filter-toggle${filtersOpen ? ' matches-panel__filter-toggle--open' : ''}${filtersActive ? ' matches-panel__filter-toggle--active' : ''}`}
      aria-expanded={filtersOpen}
      aria-label={filterToggleLabel}
      title={filterToggleLabel}
      onClick={() => setFiltersOpen((open) => !open)}
    >
      <svg viewBox="0 0 24 24" width="14" height="14" aria-hidden="true" focusable="false">
        <path
          d="M3 5h18l-7 8.5V19l-4 2v-7.5L3 5z"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          strokeLinejoin="round"
        />
      </svg>
    </button>
  );

  const showSpinner: boolean = isLoading && !hasLoaded;
  const showSections: boolean = hasLoaded && !(error && !isLoading);
  const liveIsFirst: boolean = liveMatches.length > 0;

  const renderExpandControls = (
    kind: Exclude<PanelSectionKind, 'live'>,
    section: PaginatedSectionState,
  ) => {
    const remaining = Math.max(section.totalCount - section.visibleCount, 0);
    const canLoadMore = remaining > 0;
    const canCollapse = section.visibleCount > initialVisible;
    if (!canLoadMore && !canCollapse) return null;

    const nextChunk = Math.min(remaining, LOAD_MORE_STEP);

    return (
      <div className="matches-panel__controls">
        {canLoadMore && (
          <button
            type="button"
            className="matches-panel__more-btn"
            onClick={() => void loadMore(kind)}
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
    <div className={`matches-panel${isLoading && hasLoaded ? ' matches-panel--refreshing' : ''}`}>
      {showSpinner && (
        <div className="matches-panel__state">
          <LoadingSpinner size="sm" text={t('sidebar.loading', 'Ladataan...')} />
        </div>
      )}

      {!isLoading && error && (
        <div className="matches-panel__state">
          {filtersActive && (
            <div className="matches-panel__filters">
              <div className="matches-panel__toolbar">{filterToggle}</div>
              {filters}
            </div>
          )}
          <p>{error}</p>
          <button
            type="button"
            className="matches-panel__retry-btn"
            onClick={() => void fetchInitial()}
          >
            {t('common.retry', 'Yritä uudelleen')}
          </button>
        </div>
      )}

      {showSections && (
        <>
          {liveMatches.length > 0 && (
            <div className="matches-panel__section">
              <div className="matches-panel__section-header matches-panel__section-header--live">
                <span className="pulse-dot" />
                <h3 className="matches-panel__section-title">
                  {t('sidebar.liveMatches', 'Käynnissä')}
                </h3>
                <span className="matches-panel__section-count">{liveMatches.length}</span>
                {filterToggle}
              </div>
              {filters}
              {liveMatches.map((match) => (
                <MatchPanelCard key={`${match.sport}-${match.id}`} match={match} />
              ))}
            </div>
          )}

          <div className="matches-panel__section">
            <div className="matches-panel__section-header">
              <h3 className="matches-panel__section-title">
                {t('sidebar.upcomingMatches', 'Tulevat')}
              </h3>
              {upcoming.totalCount > 0 && (
                <span className="matches-panel__section-count">
                  {upcoming.visibleCount}/{upcoming.totalCount}
                </span>
              )}
              {!liveIsFirst && filterToggle}
            </div>
            {!liveIsFirst && filters}

            {upcomingVisible.length > 0 ? (
              <>
                {upcomingVisible.map((match) => (
                  <MatchPanelCard key={`${match.sport}-${match.id}`} match={match} />
                ))}
                {renderExpandControls('upcoming', upcoming)}
              </>
            ) : (
              <p className="matches-panel__empty-text">
                {t('sidebar.noUpcomingMatches', 'Ei tulevia otteluita')}
              </p>
            )}
          </div>

          <div className="matches-panel__section">
            <div className="matches-panel__section-header">
              <h3 className="matches-panel__section-title">
                {t('sidebar.completedMatches', 'Päättyneet')}
              </h3>
              {completed.totalCount > 0 && (
                <span className="matches-panel__section-count">
                  {completed.visibleCount}/{completed.totalCount}
                </span>
              )}
            </div>

            {completedVisible.length > 0 ? (
              <>
                {completedVisible.map((match) => (
                  <MatchPanelCard key={`${match.sport}-${match.id}`} match={match} />
                ))}
                {renderExpandControls('completed', completed)}
              </>
            ) : (
              <p className="matches-panel__empty-text">
                {t('sidebar.noCompletedMatches', 'Ei päättyneitä otteluita')}
              </p>
            )}
          </div>
        </>
      )}
    </div>
  );
}

export default MatchesPanel;
