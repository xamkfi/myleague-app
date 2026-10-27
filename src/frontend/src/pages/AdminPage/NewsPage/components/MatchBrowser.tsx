import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import {
  newsMatchService,
  type NewsCompetitionOption,
  type NewsMatchCategory,
  type NewsMatchOption,
} from '../../../../api/news/newsMatchService';
import Pagination from '../../../../components/Pagination';
import type { SportKind } from '../../../../utils/sportRoutes';
import '../styles/MatchBrowser.scss';

interface MatchBrowserProps {
  onInsertMatches: (matches: NewsMatchOption[]) => void;
  /** Sport the picker opens with (the article's sport category). Without it the user picks the sport first. */
  defaultSport?: SportKind;
}

const SPORTS: SportKind[] = ['floorball', 'football', 'hockey'];
const CATEGORIES: NewsMatchCategory[] = ['results', 'scheduled', 'all', 'cancelled'];
/** Recently finished matches are what a news article usually links to, so the picker opens on them. */
const DEFAULT_CATEGORY: NewsMatchCategory = 'results';
const ALL_COMPETITIONS = '';

function selectionKey(match: Pick<NewsMatchOption, 'sport' | 'id'>): string {
  return `${match.sport}:${match.id}`;
}

export default function MatchBrowser({ onInsertMatches, defaultSport }: MatchBrowserProps) {
  const { t } = useTranslation();
  const [showBrowser, setShowBrowser] = useState(false);
  const [sport, setSport] = useState<SportKind | null>(defaultSport ?? null);
  const [competitions, setCompetitions] = useState<NewsCompetitionOption[]>([]);
  const [competitionsLoading, setCompetitionsLoading] = useState(false);
  const [competitionId, setCompetitionId] = useState<string>(ALL_COMPETITIONS);
  const [selectedMatches, setSelectedMatches] = useState<NewsMatchOption[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [matches, setMatches] = useState<NewsMatchOption[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedCategory, setSelectedCategory] = useState<NewsMatchCategory>(DEFAULT_CATEGORY);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);

  // Follow the article's sport category when it changes while the picker is closed.
  useEffect(() => {
    if (!showBrowser && defaultSport) {
      setSport(defaultSport);
    }
  }, [defaultSport, showBrowser]);

  // Load the seasons and tournaments of the chosen sport for the optional competition filter.
  useEffect(() => {
    if (!showBrowser || !sport) {
      return undefined;
    }

    let cancelled = false;
    setCompetitionsLoading(true);
    newsMatchService
      .getCompetitions(sport)
      .then((options) => {
        if (!cancelled) {
          setCompetitions(options);
        }
      })
      .catch((err: unknown) => {
        if (cancelled) {
          return;
        }
        console.error('Error fetching competitions:', err);
        setCompetitions([]);
      })
      .finally(() => {
        if (!cancelled) {
          setCompetitionsLoading(false);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [showBrowser, sport]);

  const fetchMatches = useCallback(async () => {
    if (!sport) {
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const page = await newsMatchService.getMatches(sport, {
        competitionId: competitionId || undefined,
        category: selectedCategory,
        page: currentPage,
        pageSize,
      });
      setMatches(page.items);
      setTotalCount(page.totalCount);
      setTotalPages(page.totalPages);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch matches');
      console.error('Error fetching matches:', err);
    } finally {
      setLoading(false);
    }
  }, [sport, competitionId, selectedCategory, currentPage, pageSize]);

  // Fetch matches as soon as a sport is chosen and whenever a filter or page changes.
  useEffect(() => {
    if (showBrowser) {
      void fetchMatches();
    }
  }, [showBrowser, fetchMatches]);

  const handleMatchSelect = (match: NewsMatchOption) => {
    setSelectedMatches((prev) => {
      const key = selectionKey(match);
      if (prev.some((item) => selectionKey(item) === key)) {
        return prev.filter((item) => selectionKey(item) !== key);
      }
      return [...prev, match];
    });
  };

  const insertSelectedMatches = () => {
    if (selectedMatches.length > 0) {
      onInsertMatches(selectedMatches);
      setSelectedMatches([]);
      setShowBrowser(false);
    }
  };

  const clearSelection = () => {
    setSelectedMatches([]);
  };

  const handleSportChange = (nextSport: SportKind) => {
    if (nextSport === sport) {
      return;
    }
    setSport(nextSport);
    setCompetitions([]);
    setCompetitionId(ALL_COMPETITIONS);
    setSelectedCategory(DEFAULT_CATEGORY);
    setSearchTerm('');
    setCurrentPage(1);
  };

  const handleCompetitionChange = (nextCompetitionId: string) => {
    setCompetitionId(nextCompetitionId);
    setCurrentPage(1);
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const handlePageSizeChange = (size: number) => {
    setPageSize(size);
    setCurrentPage(1);
  };

  const handleCategoryChange = (category: NewsMatchCategory) => {
    setSelectedCategory(category);
    setCurrentPage(1);
  };

  // Filter matches by search term only (sport, competition and category are applied on the backend)
  const getFilteredMatches = () => {
    if (!searchTerm) {
      return matches;
    }
    // Placeholder fixtures (no team yet) cannot match a text query; treat them as a non-match.
    const term: string = searchTerm.toLowerCase();
    return matches.filter(
      (match) =>
        (match.homeTeamName?.toLowerCase().includes(term) ?? false) ||
        (match.awayTeamName?.toLowerCase().includes(term) ?? false),
    );
  };

  const filteredMatches = getFilteredMatches();
  const seasons = competitions.filter((competition) => competition.kind === 'season');
  const tournaments = competitions.filter((competition) => competition.kind === 'tournament');

  const sportLabel = (kind: SportKind): string => t(`admin.news.matches.sports.${kind}`, kind);

  const categoryLabel = (category: NewsMatchCategory): string => {
    switch (category) {
      case 'scheduled':
        return t('admin.news.matches.upcoming_matches', 'Upcoming matches');
      case 'results':
        return t('admin.news.matches.results', 'Results');
      case 'cancelled':
        return t('admin.news.matches.cancelled', 'Cancelled');
      default:
        return t('admin.news.matches.all_matches', 'All matches');
    }
  };

  return (
    <>
      <button
        type="button"
        className="match-browser__trigger-btn"
        onClick={() => {
          setShowBrowser(true);
          setCurrentPage(1);
          setSearchTerm('');
          setCompetitionId(ALL_COMPETITIONS);
          setSelectedCategory(DEFAULT_CATEGORY);
        }}
      >
        {t('admin.news.matches.add_matches_selected')}
      </button>

      {showBrowser && (
        <div className="match-browser-modal">
          <div className="match-browser-modal__backdrop" onClick={() => setShowBrowser(false)} />
          <div className="match-browser-modal__content">
            <div className="match-browser-modal__header">
              <h2>{t('admin.news.matches.add_matches', 'ADD MATCHES')}</h2>
              <button
                type="button"
                className="match-browser-modal__close-btn"
                onClick={() => setShowBrowser(false)}
              >
                ×
              </button>
            </div>

            <div className="match-browser-modal__body">
              {/* Error message */}
              {error && (
                <div className="match-browser__error">
                  <p>{t('admin.news.matches.error', 'Error')}: {error}</p>
                  <button type="button" onClick={fetchMatches}>{t('admin.news.matches.try_again', 'Try again')}</button>
                </div>
              )}

              {/* Step 1: sport */}
              <div className="match-browser__filter">
                <span className="match-browser__filter-label">{t('admin.news.matches.sport', 'Sport')}</span>
                <div className="match-browser__categories">
                  {SPORTS.map((kind) => (
                    <button
                      key={kind}
                      type="button"
                      className={`match-browser__category-btn ${sport === kind ? 'match-browser__category-btn--active' : ''}`}
                      onClick={() => handleSportChange(kind)}
                    >
                      {sportLabel(kind)}
                    </button>
                  ))}
                </div>
              </div>

              {!sport && (
                <div className="match-browser__empty">
                  <p>{t('admin.news.matches.choose_sport_first', 'Choose a sport to list its matches.')}</p>
                </div>
              )}

              {sport && (
                <>
                {/* Step 2: optional season or tournament filter */}
                <div className="match-browser__filter">
                  <label className="match-browser__filter-label" htmlFor="match-browser-competition">
                    {t('admin.news.matches.competition', 'Season or tournament')}
                  </label>
                  <select
                    id="match-browser-competition"
                    className="match-browser__select"
                    value={competitionId}
                    disabled={competitionsLoading}
                    onChange={(e) => handleCompetitionChange(e.target.value)}
                  >
                    <option value={ALL_COMPETITIONS}>{t('admin.news.matches.all_competitions', 'All seasons and tournaments')}</option>
                    {seasons.length > 0 && (
                      <optgroup label={t('admin.news.matches.seasons', 'Seasons')}>
                        {seasons.map((season) => (
                          <option key={season.id} value={season.id}>{season.name}</option>
                        ))}
                      </optgroup>
                    )}
                    {tournaments.length > 0 && (
                      <optgroup label={t('admin.news.matches.tournaments', 'Tournaments')}>
                        {tournaments.map((tournament) => (
                          <option key={tournament.id} value={tournament.id}>{tournament.name}</option>
                        ))}
                      </optgroup>
                    )}
                  </select>
                </div>
  
                {/* Step 3: status filter, newest matches first */}
                <div className="match-browser__categories">
                  {CATEGORIES.map((category) => (
                    <button
                      key={category}
                      type="button"
                      className={`match-browser__category-btn ${selectedCategory === category ? 'match-browser__category-btn--active' : ''}`}
                      onClick={() => handleCategoryChange(category)}
                    >
                      {categoryLabel(category)}
                    </button>
                  ))}
                </div>
  
                {/* Search bar */}
                <div className="match-browser__search">
                  <div className="match-browser__search-icon">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                      <circle cx="11" cy="11" r="8"/>
                      <path d="m21 21-4.35-4.35"/>
                    </svg>
                  </div>
                  <input
                    type="text"
                    placeholder={t('admin.news.matches.search_matches', 'Search matches...')}
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    className="match-browser__search-input"
                  />
                </div>
  
                {/* Matches list */}
                {!loading && !error && (
                  <div className="match-browser__list-section">
                    <h3 className="match-browser__list-title">{categoryLabel(selectedCategory)}</h3>
                    <div className="match-browser__list">
                      {filteredMatches.length === 0 ? (
                        <div className="match-browser__empty">
                          <p>{t('admin.news.matches.no_matches_available', 'No matches available')}</p>
                        </div>
                      ) : (
                        filteredMatches.map((match) => (
                          <div key={selectionKey(match)} className="match-browser__item">
                            <div className="match-browser__item-content">
                              <div className="match-browser__item-date">
                                {new Date(match.scheduledDateTime).toLocaleDateString('fi-FI')} {new Date(match.scheduledDateTime).toLocaleTimeString('fi-FI', { hour: '2-digit', minute: '2-digit' })}
                              </div>
                              <div className="match-browser__item-sport">
                                {sportLabel(match.sport)}
                              </div>
                              <div className="match-browser__item-teams">
                                {match.homeTeamName ?? 'TBD'} {match.homeScore} - {match.awayScore} {match.awayTeamName ?? 'TBD'}
                              </div>
                            </div>
                            <input
                              type="checkbox"
                              checked={selectedMatches.some((item) => selectionKey(item) === selectionKey(match))}
                              onChange={() => handleMatchSelect(match)}
                              className="match-browser__item-checkbox"
                            />
                          </div>
                        ))
                      )}
                    </div>
  
                    {/* Pagination */}
                    {totalCount > 0 && (
                      <div className="match-browser__pagination">
                        <Pagination
                          currentPage={currentPage}
                          totalPages={totalPages}
                          totalCount={totalCount}
                          pageSize={pageSize}
                          onPageChange={handlePageChange}
                          onPageSizeChange={handlePageSizeChange}
                          pageSizeOptions={[5, 10, 25, 50]}
                        />
                      </div>
                    )}
                  </div>
                )}
  
                {/* Loading state */}
                {loading && (
                  <div className="match-browser__loading">
                    <p>{t('admin.news.matches.loading_matches', 'Loading matches...')}</p>
                  </div>
                )}
                </>
              )}

              {/* Selected matches chips */}
              {selectedMatches.length > 0 && (
                <div className="match-browser__selected">
                  <div className="match-browser__selected-chips">
                    {selectedMatches.map((match) => (
                      <span key={selectionKey(match)} className="match-browser__selected-chip">
                        {match.homeTeamName ?? 'TBD'} vs {match.awayTeamName ?? 'TBD'}
                        <button
                          type="button"
                          onClick={() => handleMatchSelect(match)}
                          className="match-browser__selected-chip-remove"
                        >
                          ×
                        </button>
                      </span>
                    ))}
                  </div>
                  <button
                    type="button"
                    onClick={clearSelection}
                    className="match-browser__clear-all"
                  >
                    {t('admin.news.matches.clear_all', 'Clear all')}
                  </button>
                </div>
              )}

              {/* Action buttons */}
              <div className="match-browser__actions">
                <button
                  type="button"
                  onClick={() => setShowBrowser(false)}
                  className="match-browser__cancel-btn"
                >
                  {t('admin.news.matches.cancel', 'Cancel')}
                </button>
                <button
                  type="button"
                  onClick={insertSelectedMatches}
                  disabled={selectedMatches.length === 0}
                  className="match-browser__add-btn"
                >
                  {t('admin.news.matches.add_selected', 'Add selected')}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
