import { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import LoadingSpinner from '../../components/LoadingSpinner/LoadingSpinner';
import Pagination from '../../components/Pagination';
import {
  floorballSeasonService,
  type FloorballSeasonSummaryDto,
  type FloorballSeasonYearDto,
} from '../../api/floorball/floorballSeasonService';
import {
  floorballStatisticsService,
  type FloorballTeamSeasonStatisticsDto,
} from '../../api/floorball/floorballStatistics';
import './FloorballPage.scss';

interface SeasonWithStandings {
  season: FloorballSeasonSummaryDto;
  standings: FloorballTeamSeasonStatisticsDto[];
  standingsLoading: boolean;
}

const PAGE_SIZE = 6;
const MAX_STANDINGS_PREVIEW = 9;

function formatSeasonYearLabel(year: string): string {
  return year.replace('-', '–');
}

function formatDateRange(startDate: string, endDate: string, locale: string): string {
  const start = new Date(startDate);
  const end = new Date(endDate);
  const opts: Intl.DateTimeFormatOptions = { day: 'numeric', month: 'numeric', year: 'numeric' };
  return `${start.toLocaleDateString(locale, opts)} – ${end.toLocaleDateString(locale, opts)}`;
}

function FloorballPage() {
  const { t, i18n } = useTranslation();
  const [searchParams, setSearchParams] = useSearchParams();
  const initializedRef = useRef(false);

  const [years, setYears] = useState<FloorballSeasonYearDto[]>([]);
  const [selectedYear, setSelectedYear] = useState<string>('');
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [seasonsData, setSeasonsData] = useState<SeasonWithStandings[]>([]);
  const [isLoadingYears, setIsLoadingYears] = useState(true);
  const [isLoadingSeasons, setIsLoadingSeasons] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [listKey, setListKey] = useState(0);
  const [reloadToken, setReloadToken] = useState(0);

  const locale = i18n.language?.startsWith('fi') ? 'fi-FI' : 'en-GB';

  const selectedYearMeta = useMemo(
    () => years.find((y) => y.year === selectedYear),
    [years, selectedYear]
  );

  const fetchStandingsForSeason = useCallback(
    async (competitionId: string): Promise<FloorballTeamSeasonStatisticsDto[]> => {
      try {
        return await floorballStatisticsService.getTeamStandings(competitionId);
      } catch (err) {
        console.error(`Failed to fetch standings for season ${competitionId}:`, err);
        return [];
      }
    },
    []
  );

  useEffect(() => {
    if (initializedRef.current) return;
    initializedRef.current = true;

    const bootstrap = async () => {
      try {
        setIsLoadingYears(true);
        setError(null);
        const yearList = await floorballSeasonService.getYears();
        setYears(yearList);

        const urlYear = searchParams.get('year');
        const urlPage = Number(searchParams.get('page') || '1');
        const defaultYear =
          yearList.find((y) => y.hasActiveSeason)?.year ?? yearList[0]?.year ?? '';
        const initialYear =
          urlYear && yearList.some((y) => y.year === urlYear) ? urlYear : defaultYear;

        setSelectedYear(initialYear);
        setCurrentPage(Number.isFinite(urlPage) && urlPage > 0 ? urlPage : 1);
      } catch (err) {
        console.error('Failed to fetch season years:', err);
        setError(t('floorballPage.error'));
      } finally {
        setIsLoadingYears(false);
      }
    };

    void bootstrap();
    // Intentionally only on mount — URL is read once for initial state.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (isLoadingYears || !selectedYear) return;

    const next = new URLSearchParams();
    next.set('year', selectedYear);
    if (currentPage > 1) next.set('page', String(currentPage));
    setSearchParams(next, { replace: true });

    const loadSeasons = async () => {
      try {
        setIsLoadingSeasons(true);
        setError(null);

        const response = await floorballSeasonService.getPaged({
          page: currentPage,
          pageSize: PAGE_SIZE,
          seasonYear: selectedYear,
        });

        const seasons = response.data ?? [];
        setTotalCount(response.pagination.totalCount);
        setTotalPages(response.pagination.totalPages);
        setListKey((k) => k + 1);

        const initial: SeasonWithStandings[] = seasons.map((season) => ({
          season,
          standings: [],
          standingsLoading: true,
        }));
        setSeasonsData(initial);
        setIsLoadingSeasons(false);

        await Promise.all(
          seasons.map(async (season) => {
            const standings = await fetchStandingsForSeason(season.id);
            setSeasonsData((prev) =>
              prev.map((item) =>
                item.season.id === season.id
                  ? { ...item, standings, standingsLoading: false }
                  : item
              )
            );
          })
        );
      } catch (err) {
        console.error('Failed to fetch seasons:', err);
        setError(t('floorballPage.error'));
        setIsLoadingSeasons(false);
      }
    };

    void loadSeasons();
  }, [
    isLoadingYears,
    selectedYear,
    currentPage,
    reloadToken,
    fetchStandingsForSeason,
    setSearchParams,
    t,
  ]);

  const handleYearSelect = (year: string) => {
    if (year === selectedYear) return;
    setSelectedYear(year);
    setCurrentPage(1);
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const handleRetry = () => {
    initializedRef.current = false;
    setIsLoadingYears(true);
    window.location.reload();
  };

  const renderStandingsTable = (data: SeasonWithStandings) => {
    const { standings, standingsLoading } = data;

    if (standingsLoading) {
      return (
        <div className="standings-table standings-table--loading">
          <div className="standings-table__header">
            <span className="standings-table__header-title">
              {t('floorballPage.standingsTitle')}
            </span>
          </div>
          <div className="standings-table__skeleton" aria-hidden="true">
            {Array.from({ length: 5 }).map((_, index) => (
              <div key={index} className="standings-table__skeleton-row" />
            ))}
          </div>
        </div>
      );
    }

    if (standings.length === 0) {
      return null;
    }

    const displayStandings = standings.slice(0, MAX_STANDINGS_PREVIEW);

    return (
      <div className="standings-table">
        <div className="standings-table__header">
          <span className="standings-table__header-title">
            {t('floorballPage.standingsTitle')}
          </span>
        </div>
        <table className="standings-table__table">
          <thead>
            <tr>
              <th className="standings-table__rank">#</th>
              <th className="standings-table__team">{t('floorballPage.team')}</th>
              <th className="standings-table__games">{t('floorballPage.gamesShort')}</th>
              <th className="standings-table__points">{t('floorballPage.pointsShort')}</th>
            </tr>
          </thead>
          <tbody>
            {displayStandings.map((team, index) => (
              <tr
                key={team.teamId}
                className={index < 3 ? `standings-table__row--top-${index + 1}` : undefined}
              >
                <td className="standings-table__rank">
                  <span className="standings-table__rank-badge">{index + 1}</span>
                </td>
                <td className="standings-table__team">{team.teamName}</td>
                <td className="standings-table__games">{team.gamesPlayed}</td>
                <td className="standings-table__points">{team.points}</td>
              </tr>
            ))}
          </tbody>
        </table>
        <Link
          to={`/league/${data.season.id}?tab=statistics`}
          className="standings-table__full-link"
        >
          {t('floorballPage.viewFullTable')}
        </Link>
      </div>
    );
  };

  const renderSeasonCard = (data: SeasonWithStandings) => {
    const { season } = data;
    const cardClass = [
      'season-card',
      season.isActive ? 'season-card--active' : '',
      season.isCompleted ? 'season-card--completed' : '',
    ]
      .filter(Boolean)
      .join(' ');

    return (
      <article key={season.id} className={cardClass}>
        <span className="season-card__accent" aria-hidden="true" />
        <div className="season-card__header">
          <div className="season-card__heading">
            <h2 className="season-card__title">{season.name}</h2>
            <span className="season-card__dates">
              {formatDateRange(season.startDate, season.endDate, locale)}
            </span>
          </div>
          {season.isActive && (
            <span className="season-card__badge season-card__badge--active">
              <span className="season-card__badge-dot" aria-hidden="true" />
              {t('floorballPage.active')}
            </span>
          )}
          {season.isCompleted && !season.isActive && (
            <span className="season-card__badge season-card__badge--completed">
              {t('floorballPage.completed')}
            </span>
          )}
        </div>

        <nav className="season-card__links" aria-label={season.name}>
          <Link to={`/league/${season.id}?tab=fixtures`} className="season-card__link">
            {t('floorballPage.fixtures')}
          </Link>
          <Link to={`/league/${season.id}?tab=statistics`} className="season-card__link">
            {t('floorballPage.standings')}
          </Link>
          <Link to={`/league/${season.id}?tab=summary`} className="season-card__link">
            {t('floorballPage.stats')}
          </Link>
        </nav>

        {renderStandingsTable(data)}
      </article>
    );
  };

  if (isLoadingYears) {
    return (
      <PageTemplate title={t('sports.floorball')}>
        <div className="floorball-page">
          <div className="floorball-page__loading">
            <LoadingSpinner variant="light" text={t('floorballPage.loading')} />
          </div>
        </div>
      </PageTemplate>
    );
  }

  if (error && years.length === 0) {
    return (
      <PageTemplate title={t('sports.floorball')}>
        <div className="floorball-page">
          <div className="floorball-page__error">
            <p>{error}</p>
            <button type="button" onClick={handleRetry} className="floorball-page__retry-btn">
              {t('floorballPage.retry')}
            </button>
          </div>
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('sports.floorball')}>
      <div className="floorball-page">
        <header className="floorball-page__header">
          <div className="floorball-page__header-top">
            <div>
              <span className="floorball-page__kicker">{t('floorballPage.kicker')}</span>
              <h1 className="floorball-page__title">{t('sports.floorball')}</h1>
              <p className="floorball-page__description">{t('floorballPage.description')}</p>
            </div>
            {selectedYearMeta && (
              <div className="floorball-page__header-stat">
                <span className="floorball-page__header-stat-value">
                  {selectedYearMeta.seasonCount}
                </span>
                <span className="floorball-page__header-stat-label">
                  {t('floorballPage.seasonsInYear')}
                </span>
              </div>
            )}
          </div>

          {years.length > 0 && (
            <div className="floorball-page__years-wrap">
              <div
                className="floorball-page__years"
                role="tablist"
                aria-label={t('floorballPage.seasonYears')}
              >
                {years.map((year) => (
                  <button
                    key={year.year}
                    type="button"
                    role="tab"
                    aria-selected={year.year === selectedYear}
                    className={`floorball-page__year-chip${
                      year.year === selectedYear ? ' floorball-page__year-chip--active' : ''
                    }`}
                    onClick={() => handleYearSelect(year.year)}
                  >
                    {formatSeasonYearLabel(year.year)}
                    {year.hasActiveSeason && (
                      <span className="floorball-page__year-dot" aria-hidden="true" />
                    )}
                  </button>
                ))}
              </div>
            </div>
          )}
        </header>

        {selectedYear && (
          <div className="floorball-page__section-head">
            <h2 className="floorball-page__section-title">
              {t('floorballPage.seasonYear', { year: formatSeasonYearLabel(selectedYear) })}
            </h2>
            <span className="floorball-page__section-rule" aria-hidden="true" />
          </div>
        )}

        {isLoadingSeasons && seasonsData.length === 0 ? (
          <div className="floorball-page__loading floorball-page__loading--inline">
            <LoadingSpinner variant="light" text={t('floorballPage.loading')} />
          </div>
        ) : error ? (
          <div className="floorball-page__error">
            <p>{error}</p>
            <button
              type="button"
              onClick={() => {
                setError(null);
                setReloadToken((token) => token + 1);
              }}
              className="floorball-page__retry-btn"
            >
              {t('floorballPage.retry')}
            </button>
          </div>
        ) : seasonsData.length === 0 ? (
          <div className="floorball-page__empty">
            <p>{t('floorballPage.noSeasonsForYear')}</p>
          </div>
        ) : (
          <>
            <div key={listKey} className="floorball-page__seasons floorball-page__seasons--enter">
              {seasonsData.map((data) => renderSeasonCard(data))}
            </div>

            {totalPages > 1 && (
              <div className="floorball-page__pagination">
                <Pagination
                  currentPage={currentPage}
                  totalPages={totalPages}
                  totalCount={totalCount}
                  pageSize={PAGE_SIZE}
                  onPageChange={handlePageChange}
                  onPageSizeChange={() => undefined}
                  showPageSizeSelector={false}
                  showSummary
                />
              </div>
            )}
          </>
        )}
      </div>
    </PageTemplate>
  );
}

export default FloorballPage;
