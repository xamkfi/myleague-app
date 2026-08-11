import { useState, useEffect, useMemo, useRef } from 'react';
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
import { floorballMatchService } from '../../api/floorball/floorballMatchService';
import { FloorballMatchStatus, type FloorballMatchDto } from '../../types/floorball/floorballTypes';
import { formatMatchDateTime } from '../../utils/helpers';
import bannerImage from '../../assets/floorball-banner.png';
import './FloorballPage.scss';

interface SeasonWithStandings {
  season: FloorballSeasonSummaryDto;
  standings: FloorballTeamSeasonStatisticsDto[];
  standingsLoading: boolean;
}

const PAGE_SIZE = 6;
const MAX_STANDINGS_PREVIEW = 10;
const MAX_UPCOMING_MATCHES = 6;

function formatSeasonYearLabel(year: string): string {
  return year.replace('-', '–');
}

function FloorballPage() {
  const { t } = useTranslation();
  const [searchParams, setSearchParams] = useSearchParams();
  const initializedRef = useRef(false);

  const [years, setYears] = useState<FloorballSeasonYearDto[]>([]);
  const [selectedYear, setSelectedYear] = useState<string>('');
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [seasonsData, setSeasonsData] = useState<SeasonWithStandings[]>([]);
  const [upcomingMatches, setUpcomingMatches] = useState<FloorballMatchDto[]>([]);
  const [isLoadingYears, setIsLoadingYears] = useState(true);
  const [isLoadingSeasons, setIsLoadingSeasons] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [reloadToken, setReloadToken] = useState(0);

  const selectedYearMeta = useMemo(
    () => years.find((y) => y.year === selectedYear),
    [years, selectedYear]
  );
  // The "current season" view is the year with an active season, or the newest
  // year when nothing is flagged active (e.g. between seasons).
  const currentYear = useMemo(
    () => years.find((y) => y.hasActiveSeason)?.year ?? years[0]?.year ?? '',
    [years]
  );
  const isCurrentSeasonView =
    (selectedYearMeta?.hasActiveSeason ?? false) || selectedYear === currentYear;

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
        setUpcomingMatches([]);

        const response = await floorballSeasonService.getPaged({
          page: currentPage,
          pageSize: PAGE_SIZE,
          seasonYear: selectedYear,
        });

        const seasons = response.data ?? [];
        setTotalCount(response.pagination.totalCount);
        setTotalPages(response.pagination.totalPages);

        const initial: SeasonWithStandings[] = seasons.map((season) => ({
          season,
          standings: [],
          standingsLoading: true,
        }));
        setSeasonsData(initial);
        setIsLoadingSeasons(false);

        const standingsTask = Promise.all(
          seasons.map(async (season) => {
            try {
              const standings = await floorballStatisticsService.getTeamStandings(season.id);
              setSeasonsData((prev) =>
                prev.map((item) =>
                  item.season.id === season.id
                    ? { ...item, standings, standingsLoading: false }
                    : item
                )
              );
            } catch (err) {
              console.error(`Failed to fetch standings for season ${season.id}:`, err);
              setSeasonsData((prev) =>
                prev.map((item) =>
                  item.season.id === season.id ? { ...item, standingsLoading: false } : item
                )
              );
            }
          })
        );

        const activeSeasons = seasons.some((s) => s.isActive)
          ? seasons.filter((s) => s.isActive)
          : seasons;
        const matchesTask = Promise.all(
          activeSeasons.map(async (season) => {
            try {
              const result = await floorballMatchService.getBySeason(season.id);
              return result.data ?? [];
            } catch (err) {
              console.error(`Failed to fetch matches for season ${season.id}:`, err);
              return [];
            }
          })
        ).then((matchLists) => {
          const now = Date.now();
          const upcoming = matchLists
            .flat()
            .filter(
              (m) =>
                m.status === FloorballMatchStatus.Scheduled &&
                new Date(m.scheduledDateTime).getTime() >= now
            )
            .sort(
              (a, b) =>
                new Date(a.scheduledDateTime).getTime() - new Date(b.scheduledDateTime).getTime()
            )
            .slice(0, MAX_UPCOMING_MATCHES);
          setUpcomingMatches(upcoming);
        });

        await Promise.all([standingsTask, matchesTask]);
      } catch (err) {
        console.error('Failed to fetch seasons:', err);
        setError(t('floorballPage.error'));
        setIsLoadingSeasons(false);
      }
    };

    void loadSeasons();
  }, [isLoadingYears, selectedYear, currentPage, reloadToken, setSearchParams, t]);

  const handleYearSelect = (year: string) => {
    if (year === selectedYear) return;
    setSelectedYear(year);
    setCurrentPage(1);
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const renderTeamLogo = (logo: string | null | undefined) =>
    logo && logo.trim() !== '' ? (
      <img
        className="fb-team-logo"
        src={logo}
        alt=""
        onError={(e) => {
          (e.target as HTMLImageElement).style.visibility = 'hidden';
        }}
      />
    ) : (
      <span className="fb-team-logo fb-team-logo--empty" aria-hidden="true" />
    );

  const renderStandingsCard = (data: SeasonWithStandings, isDark: boolean) => {
    const { season, standings, standingsLoading } = data;
    const displayStandings = standings.slice(0, MAX_STANDINGS_PREVIEW);

    return (
      <section
        key={season.id}
        className={`fb-standings-card${isDark ? ' fb-standings-card--dark' : ''}`}
      >
        <h2 className="fb-standings-card__title">{season.name}</h2>

        {isDark && (
          <nav className="fb-standings-card__links" aria-label={season.name}>
            <Link to={`/league/${season.id}?tab=fixtures`}>{t('leaguePage.tabs.fixtures')}</Link>
            <Link to={`/league/${season.id}?tab=results`}>{t('leaguePage.tabs.results')}</Link>
            <Link to={`/league/${season.id}?tab=statistics`}>
              {t('leaguePage.tabs.statistics')}
            </Link>
            <Link to={`/league/${season.id}?tab=summary`}>{t('leaguePage.tabs.summary')}</Link>
          </nav>
        )}

        <span className="fb-standings-card__label">{t('floorballPage.standingsTitle')}</span>

        {standingsLoading ? (
          <div className="fb-standings-card__skeleton" aria-hidden="true">
            {Array.from({ length: 5 }).map((_, index) => (
              <div key={index} className="fb-standings-card__skeleton-row" />
            ))}
          </div>
        ) : displayStandings.length === 0 ? (
          <p className="fb-standings-card__empty">{t('floorballPage.noStandings')}</p>
        ) : (
          <div className="fb-standings-table">
            <div className="fb-standings-table__head">
              <span className="fb-standings-table__rank">#</span>
              <span className="fb-standings-table__team">{t('floorballPage.teamShort')}</span>
              <span className="fb-standings-table__num">{t('floorballPage.gdShort')}</span>
              <span className="fb-standings-table__num fb-standings-table__num--pts">
                {t('floorballPage.ptsShort')}
              </span>
            </div>
            {displayStandings.map((team, index) => (
              <div key={team.teamId} className="fb-standings-table__row">
                <span className="fb-standings-table__rank">{index + 1}.</span>
                <span className="fb-standings-table__team">
                  {renderTeamLogo(team.teamLogo)}
                  <span className="fb-standings-table__team-name">{team.teamName}</span>
                </span>
                <span className="fb-standings-table__num">{team.goalDifference}</span>
                <span className="fb-standings-table__num fb-standings-table__num--pts">
                  {team.points}
                </span>
              </div>
            ))}
          </div>
        )}

        <Link to={`/league/${season.id}?tab=statistics`} className="fb-standings-card__full-link">
          {t('floorballPage.viewFullTable')}
        </Link>
      </section>
    );
  };

  const renderUpcomingMatchesCard = () => {
    if (upcomingMatches.length === 0) return null;

    return (
      <section className="fb-upcoming-card">
        <h2 className="fb-upcoming-card__title">{t('floorballPage.upcomingMatches')}</h2>
        <div className="fb-upcoming-card__list">
          {upcomingMatches.map((match) => {
            const [date, time] = formatMatchDateTime(match.scheduledDateTime);
            return (
              <Link
                key={match.id}
                to={`/match/${match.id}`}
                className="fb-upcoming-card__row"
              >
                <span className="fb-upcoming-card__datetime">
                  <span>{date}</span>
                  <span>{time}</span>
                </span>
                <span className="fb-upcoming-card__teams">
                  <span className="fb-upcoming-card__team">
                    {renderTeamLogo(match.homeTeamLogo)}
                    <span>{match.homeTeamName ?? t('floorballPage.tbd')}</span>
                  </span>
                  <span className="fb-upcoming-card__team">
                    {renderTeamLogo(match.awayTeamLogo)}
                    <span>{match.awayTeamName ?? t('floorballPage.tbd')}</span>
                  </span>
                </span>
              </Link>
            );
          })}
        </div>
      </section>
    );
  };

  const renderInfoCards = () => {
    if (!isCurrentSeasonView) {
      return (
        <article className="fb-info-card">
          <h2 className="fb-info-card__title">
            {t('floorballPage.archiveTitle', { year: formatSeasonYearLabel(selectedYear) })}
          </h2>
          <p>{t('floorballPage.archiveText')}</p>
          {currentYear && (
            <p>
              <button
                type="button"
                className="fb-info-card__link-button"
                onClick={() => handleYearSelect(currentYear)}
              >
                {t('floorballPage.backToCurrent', { year: formatSeasonYearLabel(currentYear) })}
              </button>
            </p>
          )}
        </article>
      );
    }

    const infoSections = [
      { title: t('floorballPage.info.introTitle'), paragraphs: ['intro1', 'intro2', 'intro3'] },
      {
        title: t('floorballPage.info.seriesTitle'),
        paragraphs: ['series1', 'series2', 'series3', 'series4'],
      },
      { title: t('floorballPage.info.loanTitle'), paragraphs: ['loan1'] },
      { title: t('floorballPage.info.feeTitle'), paragraphs: ['fee1', 'fee2', 'fee3'] },
    ];

    return (
      <>
        {infoSections.map((section) => (
          <article key={section.title} className="fb-info-card">
            <h2 className="fb-info-card__title">{section.title}</h2>
            {section.paragraphs.map((key) => (
              <p key={key}>{t(`floorballPage.info.${key}`)}</p>
            ))}
          </article>
        ))}
        <article className="fb-info-card">
          <h2 className="fb-info-card__title">{t('floorballPage.info.contactTitle')}</h2>
          <p className="fb-info-card__contact">
            Mikko Luukkonen
            <br />
            mikko(at)mahl.fi
            <br />
            044 209 9199
          </p>
        </article>
      </>
    );
  };

  if (isLoadingYears) {
    return (
      <PageTemplate title={t('sports.floorball')} fullBleed>
        <div className="floorball-page">
          <div className="floorball-page__state">
            <LoadingSpinner variant="light" text={t('floorballPage.loading')} />
          </div>
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('sports.floorball')} fullBleed>
      <div className="floorball-page">
        <header className="fb-banner">
          <img className="fb-banner__image" src={bannerImage} alt="" aria-hidden="true" />
          <div className="fb-banner__content">
            <h1 className="fb-banner__title">{t('sports.floorball')}</h1>
            {years.length > 0 && (
              <div className="fb-banner__nav">
                <label className="fb-banner__select-wrap">
                  <span className="fb-banner__select-label">
                    {t('floorballPage.seasonLabel')}
                  </span>
                  <select
                    className="fb-banner__select"
                    value={selectedYear}
                    onChange={(e) => handleYearSelect(e.target.value)}
                    aria-label={t('floorballPage.seasonYears')}
                  >
                    {years.map((year) => (
                      <option key={year.year} value={year.year}>
                        {formatSeasonYearLabel(year.year)}
                      </option>
                    ))}
                  </select>
                  <span className="fb-banner__select-chevron" aria-hidden="true" />
                </label>
              </div>
            )}
          </div>
        </header>

        <div className="fb-content">
          <div className="fb-container">
            {error ? (
              <div className="floorball-page__state">
                <p>{error}</p>
                <button
                  type="button"
                  className="fb-retry-btn"
                  onClick={() => {
                    setError(null);
                    setReloadToken((token) => token + 1);
                  }}
                >
                  {t('floorballPage.retry')}
                </button>
              </div>
            ) : (
              <div className="fb-columns">
                <div className="fb-columns__main">{renderInfoCards()}</div>
                <aside className="fb-columns__side">
                  {isLoadingSeasons && seasonsData.length === 0 ? (
                    <div className="floorball-page__state floorball-page__state--inline">
                      <LoadingSpinner variant="dark" text={t('floorballPage.loading')} />
                    </div>
                  ) : seasonsData.length === 0 ? (
                    <div className="fb-info-card">
                      <p>{t('floorballPage.noSeasonsForYear')}</p>
                    </div>
                  ) : (
                    (() => {
                      const ordered = [
                        ...seasonsData.filter((d) => d.season.isActive),
                        ...seasonsData.filter((d) => !d.season.isActive),
                      ];
                      const darkCount = ordered.filter((d) => d.season.isActive).length
                        || (isCurrentSeasonView ? 1 : 0);
                      return (
                        <>
                          {ordered
                            .slice(0, darkCount)
                            .map((d) => renderStandingsCard(d, true))}
                          {renderUpcomingMatchesCard()}
                          {ordered
                            .slice(darkCount)
                            .map((d) => renderStandingsCard(d, false))}
                        </>
                      );
                    })()
                  )}

                  {totalPages > 1 && (
                    <div className="fb-pagination">
                      <Pagination
                        currentPage={currentPage}
                        totalPages={totalPages}
                        totalCount={totalCount}
                        pageSize={PAGE_SIZE}
                        onPageChange={handlePageChange}
                        onPageSizeChange={() => undefined}
                        showPageSizeSelector={false}
                        showSummary={false}
                      />
                    </div>
                  )}
                </aside>
              </div>
            )}
          </div>
        </div>
      </div>
    </PageTemplate>
  );
}

export default FloorballPage;
