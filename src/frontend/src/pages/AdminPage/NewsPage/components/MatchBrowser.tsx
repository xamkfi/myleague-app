import { useEffect, useId, useMemo, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import { useTranslation } from 'react-i18next';
import {
  newsMatchService,
  type NewsCompetitionOption,
  type NewsMatchCategory,
  type NewsMatchOption,
  type NewsMatchPage,
} from '../../../../api/news/newsMatchService';
import Button from '../../../../components/Button/Button';
import Pagination from '../../../../components/Pagination';
import SegmentedControl, { type SegmentedOption } from '../../../../components/SegmentedControl/SegmentedControl';
import SportIcon from '../../../../components/SportIcon/SportIcon';
import { useDebouncedValue } from '../../../../hooks/useDebouncedValue';
import { useDialogBehavior } from '../../../../hooks/useDialogBehavior';
import type { SportKind } from '../../../../utils/sportRoutes';
import MatchPickerRow from './MatchPickerRow';
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
const PAGE_SIZE_OPTIONS: number[] = [10, 25, 50];
const SKELETON_ROWS = 6;
const EMPTY_PAGE: NewsMatchPage = { items: [], totalCount: 0, totalPages: 0 };

function selectionKey(match: Pick<NewsMatchOption, 'sport' | 'id'>): string {
  return `${match.sport}:${match.id}`;
}

function PlusIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" aria-hidden="true">
      <path d="M12 5v14M5 12h14" />
    </svg>
  );
}

function CloseIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" aria-hidden="true">
      <path d="M6 6l12 12M18 6 6 18" />
    </svg>
  );
}

function SearchIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" aria-hidden="true">
      <circle cx="11" cy="11" r="7" />
      <path d="m20 20-3.5-3.5" />
    </svg>
  );
}

interface MatchPickerDialogProps {
  defaultSport?: SportKind;
  onClose: () => void;
  onInsert: (matches: NewsMatchOption[]) => void;
}

/** The picker itself. Mounted only while open, so every opening starts from a clean state. */
function MatchPickerDialog({ defaultSport, onClose, onInsert }: MatchPickerDialogProps) {
  const { t } = useTranslation();
  const titleId: string = useId();
  const dialogRef = useRef<HTMLDivElement>(null);
  const searchRef = useRef<HTMLInputElement>(null);
  const bodyRef = useRef<HTMLDivElement>(null);

  const [sport, setSport] = useState<SportKind | null>(defaultSport ?? null);
  const [competitions, setCompetitions] = useState<NewsCompetitionOption[]>([]);
  const [competitionsLoading, setCompetitionsLoading] = useState(false);
  const [competitionId, setCompetitionId] = useState<string>(ALL_COMPETITIONS);
  const [category, setCategory] = useState<NewsMatchCategory>(DEFAULT_CATEGORY);
  const [searchTerm, setSearchTerm] = useState('');
  const debouncedSearch: string = useDebouncedValue(searchTerm, 300);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(PAGE_SIZE_OPTIONS[0]);
  const [result, setResult] = useState<NewsMatchPage>(EMPTY_PAGE);
  const [loading, setLoading] = useState(false);
  const [loadFailed, setLoadFailed] = useState(false);
  const [reloadToken, setReloadToken] = useState(0);
  const [selected, setSelected] = useState<NewsMatchOption[]>([]);

  useDialogBehavior(true, dialogRef, onClose, defaultSport ? searchRef : undefined);

  // Seasons and tournaments of the chosen sport feed the optional competition filter.
  useEffect(() => {
    if (!sport) {
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
        if (!cancelled) {
          console.error('Error fetching competitions:', err);
          setCompetitions([]);
        }
      })
      .finally(() => {
        if (!cancelled) {
          setCompetitionsLoading(false);
        }
      });
    return () => {
      cancelled = true;
    };
  }, [sport]);

  useEffect(() => {
    if (!sport) {
      return undefined;
    }
    let cancelled = false;
    setLoading(true);
    setLoadFailed(false);
    newsMatchService
      .getMatches(sport, {
        competitionId: competitionId || undefined,
        category,
        page,
        pageSize,
        searchQuery: debouncedSearch,
      })
      .then((nextPage) => {
        if (!cancelled) {
          setResult(nextPage);
        }
      })
      .catch((err: unknown) => {
        if (!cancelled) {
          console.error('Error fetching matches:', err);
          setResult(EMPTY_PAGE);
          setLoadFailed(true);
        }
      })
      .finally(() => {
        if (!cancelled) {
          setLoading(false);
        }
      });
    return () => {
      cancelled = true;
    };
  }, [sport, competitionId, category, page, pageSize, debouncedSearch, reloadToken]);

  useEffect(() => {
    bodyRef.current?.scrollTo({ top: 0 });
  }, [page]);

  const selectedKeys: Set<string> = useMemo(() => new Set(selected.map(selectionKey)), [selected]);

  const toggleMatch = (match: NewsMatchOption): void => {
    const key: string = selectionKey(match);
    setSelected((previous) =>
      previous.some((item) => selectionKey(item) === key)
        ? previous.filter((item) => selectionKey(item) !== key)
        : [...previous, match],
    );
  };

  const handleSportChange = (nextSport: SportKind): void => {
    if (nextSport === sport) {
      return;
    }
    setSport(nextSport);
    setCompetitions([]);
    setCompetitionId(ALL_COMPETITIONS);
    setCategory(DEFAULT_CATEGORY);
    setSearchTerm('');
    setPage(1);
    setResult(EMPTY_PAGE);
  };

  const sportOptions: SegmentedOption<SportKind>[] = SPORTS.map((kind) => ({
    value: kind,
    label: t(`admin.news.matches.sports.${kind}`),
    icon: (active: boolean) => <SportIcon sport={kind} size="sm" inverted={active} decorative />,
  }));

  const categoryLabel = (value: NewsMatchCategory): string => {
    switch (value) {
      case 'scheduled':
        return t('admin.news.matches.upcoming_matches');
      case 'results':
        return t('admin.news.matches.results');
      case 'cancelled':
        return t('admin.news.matches.cancelled');
      default:
        return t('admin.news.matches.all_matches');
    }
  };

  const categoryOptions: SegmentedOption<NewsMatchCategory>[] = CATEGORIES.map((value) => ({
    value,
    label: categoryLabel(value),
  }));

  const seasons: NewsCompetitionOption[] = competitions.filter((item) => item.kind === 'season');
  const tournaments: NewsCompetitionOption[] = competitions.filter((item) => item.kind === 'tournament');
  const tbd: string = t('admin.news.matches.tbd');

  const renderBody = () => {
    if (!sport) {
      return (
        <div className="match-picker__state">
          <div className="match-picker__state-icons" aria-hidden="true">
            {SPORTS.map((kind) => (
              <SportIcon key={kind} sport={kind} size="md" decorative />
            ))}
          </div>
          <p className="match-picker__state-title">{t('admin.news.matches.choose_sport_first')}</p>
        </div>
      );
    }

    if (loading) {
      return (
        <ul className="match-picker__list" aria-busy="true" aria-label={t('admin.news.matches.loading_matches')}>
          {Array.from({ length: SKELETON_ROWS }, (_, index) => (
            <li key={index} className="match-picker__skeleton" />
          ))}
        </ul>
      );
    }

    if (loadFailed) {
      return (
        <div className="match-picker__state match-picker__state--error" role="alert">
          <p className="match-picker__state-title">{t('admin.news.matches.load_error')}</p>
          <Button variant="secondary" size="sm" onClick={() => setReloadToken((value) => value + 1)}>
            {t('admin.news.matches.try_again')}
          </Button>
        </div>
      );
    }

    if (result.items.length === 0) {
      return (
        <div className="match-picker__state">
          <p className="match-picker__state-title">{t('admin.news.matches.no_matches_available')}</p>
          <p className="match-picker__state-hint">{t('admin.news.matches.empty_hint')}</p>
        </div>
      );
    }

    return (
      <>
        <ul className="match-picker__list">
          {result.items.map((match) => (
            <MatchPickerRow
              key={selectionKey(match)}
              match={match}
              selected={selectedKeys.has(selectionKey(match))}
              showCompetition={!competitionId}
              onToggle={toggleMatch}
            />
          ))}
        </ul>
        <Pagination
          className="match-picker__pagination"
          currentPage={page}
          totalPages={result.totalPages}
          totalCount={result.totalCount}
          pageSize={pageSize}
          onPageChange={setPage}
          onPageSizeChange={(size) => {
            setPageSize(size);
            setPage(1);
          }}
          pageSizeOptions={PAGE_SIZE_OPTIONS}
        />
      </>
    );
  };

  return (
    <div className="match-picker" role="presentation">
      <div className="match-picker__backdrop" onClick={onClose} aria-hidden="true" />
      <div
        ref={dialogRef}
        className="match-picker__dialog"
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        tabIndex={-1}
      >
        <header className="match-picker__header">
          <div>
            <h2 id={titleId} className="match-picker__title">{t('admin.news.matches.add_matches')}</h2>
            <p className="match-picker__subtitle">{t('admin.news.matches.subtitle')}</p>
          </div>
          <button
            type="button"
            className="match-picker__close"
            onClick={onClose}
            aria-label={t('admin.news.matches.close')}
          >
            <CloseIcon />
          </button>
        </header>

        <div className="match-picker__filters">
          <SegmentedControl
            options={sportOptions}
            value={sport}
            onChange={handleSportChange}
            ariaLabel={t('admin.news.matches.sport')}
            tone="sport"
            layout="stretch"
          />

          {sport && (
            <>
              <div className="match-picker__filter-row">
                <label className="match-picker__field match-picker__field--competition">
                  <span className="match-picker__field-label">{t('admin.news.matches.competition')}</span>
                  <select
                    className="match-picker__select"
                    value={competitionId}
                    disabled={competitionsLoading}
                    onChange={(event) => {
                      setCompetitionId(event.target.value);
                      setPage(1);
                    }}
                  >
                    <option value={ALL_COMPETITIONS}>{t('admin.news.matches.all_competitions')}</option>
                    {seasons.length > 0 && (
                      <optgroup label={t('admin.news.matches.seasons')}>
                        {seasons.map((season) => (
                          <option key={season.id} value={season.id}>{season.name}</option>
                        ))}
                      </optgroup>
                    )}
                    {tournaments.length > 0 && (
                      <optgroup label={t('admin.news.matches.tournaments')}>
                        {tournaments.map((tournament) => (
                          <option key={tournament.id} value={tournament.id}>{tournament.name}</option>
                        ))}
                      </optgroup>
                    )}
                  </select>
                </label>
                <div className="match-picker__field">
                  <span className="match-picker__field-label" aria-hidden="true">{t('admin.news.matches.status')}</span>
                  <SegmentedControl
                    options={categoryOptions}
                    value={category}
                    onChange={(value) => {
                      setCategory(value);
                      setPage(1);
                    }}
                    ariaLabel={t('admin.news.matches.status')}
                    size="sm"
                    className="match-picker__status"
                  />
                </div>
              </div>

              <label className="match-picker__field">
                <span className="match-picker__field-label">{t('admin.news.matches.search_label')}</span>
                <span className="match-picker__search">
                  <SearchIcon />
                  <input
                    ref={searchRef}
                    type="search"
                    className="match-picker__search-input"
                    value={searchTerm}
                    placeholder={t('admin.news.matches.search_matches')}
                    onChange={(event) => {
                      setSearchTerm(event.target.value);
                      setPage(1);
                    }}
                  />
                </span>
              </label>
            </>
          )}
        </div>

        <div ref={bodyRef} className="match-picker__body">
          {renderBody()}
        </div>

        <footer className="match-picker__footer">
          {selected.length > 0 && (
            <ul className="match-picker__chips" aria-label={t('admin.news.matches.selected_matches')}>
              {selected.map((match) => {
                const label: string = `${match.homeTeamName ?? tbd} – ${match.awayTeamName ?? tbd}`;
                return (
                  <li key={selectionKey(match)} className="match-picker__chip">
                    <span className="match-picker__chip-label">{label}</span>
                    <button
                      type="button"
                      className="match-picker__chip-remove"
                      onClick={() => toggleMatch(match)}
                      aria-label={t('admin.news.matches.remove_selected', { match: label })}
                    >
                      <CloseIcon />
                    </button>
                  </li>
                );
              })}
            </ul>
          )}
          <div className="match-picker__actions">
            <div className="match-picker__selection" aria-live="polite">
              <span className="match-picker__count">
                {t('admin.news.matches.selected_count', { count: selected.length })}
              </span>
              {selected.length > 0 && (
                <button type="button" className="match-picker__clear" onClick={() => setSelected([])}>
                  {t('admin.news.matches.clear_all')}
                </button>
              )}
            </div>
            <div className="match-picker__buttons">
              <Button variant="secondary" onClick={onClose}>
                {t('admin.news.matches.cancel')}
              </Button>
              <Button variant="primary" onClick={() => onInsert(selected)} disabled={selected.length === 0}>
                {selected.length > 0
                  ? t('admin.news.matches.add_selected_count', { count: selected.length })
                  : t('admin.news.matches.add_selected')}
              </Button>
            </div>
          </div>
        </footer>
      </div>
    </div>
  );
}

/** "Add matches" button and the match picker dialog of the news editor. */
export default function MatchBrowser({ onInsertMatches, defaultSport }: MatchBrowserProps) {
  const { t } = useTranslation();
  const [open, setOpen] = useState(false);

  const handleInsert = (matches: NewsMatchOption[]): void => {
    if (matches.length > 0) {
      onInsertMatches(matches);
    }
    setOpen(false);
  };

  return (
    <>
      <button type="button" className="match-picker-trigger" onClick={() => setOpen(true)}>
        <PlusIcon />
        {t('admin.news.matches.add_matches')}
      </button>
      {open &&
        createPortal(
          <MatchPickerDialog defaultSport={defaultSport} onClose={() => setOpen(false)} onInsert={handleInsert} />,
          document.body,
        )}
    </>
  );
}
