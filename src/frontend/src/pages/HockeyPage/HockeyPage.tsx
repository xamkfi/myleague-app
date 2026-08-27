import { useEffect, useMemo, useRef, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import LoadingSpinner from '../../components/LoadingSpinner/LoadingSpinner';
import Pagination from '../../components/Pagination';
import { hockeySeasonService } from '../../api/hockey/hockeySeasonService';
import { hockeyStatisticsService } from '../../api/hockey/hockeyStatisticsService';
import { hockeyTeamService } from '../../api/hockey/hockeyTeamService';
import { hockeyMatchService } from '../../api/hockey/hockeyMatchService';
import { seasonContentBlockService } from '../../api/common/seasonContentBlockService';
import { SportsCategory } from '../../types/common/sports';
import type { SeasonContentBlockDto } from '../../types/admin/seasonContentBlockTypes';
import type {
  HockeyMatchDto,
  HockeySeasonDto,
  HockeyTeamCompetitionStatisticsDto,
  HockeyTeamDto,
} from '../../types/hockey/hockeyTypes';
import { uniqueHockeyStandingsByTeamId } from '../../utils/hockeyLookups';
import { formatMatchDateTime } from '../../utils/helpers';
import { seasonYearFromDates } from '../../utils/seasonYear';
import { useAudience } from '../../context/AudienceContext';
import bannerImage from '../../assets/floorball-banner.png';
import '../FloorballPage/FloorballPage.scss';
import './HockeyPage.scss';

interface SeasonWithStandings {
  season: HockeySeasonDto;
  standings: HockeyTeamCompetitionStatisticsDto[];
  standingsLoading: boolean;
}

interface YearMeta {
  year: string;
  hasActiveSeason: boolean;
}

const PAGE_SIZE = 6;
const MAX_STANDINGS_PREVIEW = 10;
const MAX_UPCOMING_MATCHES = 6;

function formatSeasonYearLabel(year: string): string {
  return year.replace('-', '–');
}

function teamNameFromMatch(
  match: HockeyMatchDto,
  slot: 'Home' | 'Away',
  teamsById: Map<string, HockeyTeamDto>,
): string | null {
  const matchTeam = match.matchTeams.find((item) => item.teamSlot === slot);
  if (!matchTeam) {
    return null;
  }
  return teamsById.get(matchTeam.teamId)?.name ?? null;
}

function HockeyPage() {
  const { t } = useTranslation();
  const { audience } = useAudience();
  const [searchParams, setSearchParams] = useSearchParams();
  const initializedRef = useRef(false);

  const [allSeasons, setAllSeasons] = useState<HockeySeasonDto[]>([]);
  const [teamsById, setTeamsById] = useState<Map<string, HockeyTeamDto>>(new Map());
  const [selectedYear, setSelectedYear] = useState<string>('');
  const [currentPage, setCurrentPage] = useState(1);
  const [seasonsData, setSeasonsData] = useState<SeasonWithStandings[]>([]);
  const [upcomingMatches, setUpcomingMatches] = useState<HockeyMatchDto[]>([]);
  const [contentBlocks, setContentBlocks] = useState<SeasonContentBlockDto[]>([]);
  const [isLoadingYears, setIsLoadingYears] = useState(true);
  const [isLoadingSeasons, setIsLoadingSeasons] = useState(false);
  const [isLoadingBlocks, setIsLoadingBlocks] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [reloadToken, setReloadToken] = useState(0);

  const years = useMemo((): YearMeta[] => {
    const byYear = new Map<string, YearMeta>();
    allSeasons.forEach((season) => {
      const year = seasonYearFromDates(season.startDate, season.endDate);
      if (!year) {
        return;
      }
      const existing = byYear.get(year);
      byYear.set(year, {
        year,
        hasActiveSeason: Boolean(existing?.hasActiveSeason || season.isActive),
      });
    });
    return [...byYear.values()].sort((a, b) => b.year.localeCompare(a.year));
  }, [allSeasons]);

  const selectedYearMeta = useMemo(
    () => years.find((year) => year.year === selectedYear),
    [years, selectedYear],
  );
  const currentYear = useMemo(
    () => years.find((year) => year.hasActiveSeason)?.year ?? years[0]?.year ?? '',
    [years],
  );
  const isCurrentSeasonView =
    (selectedYearMeta?.hasActiveSeason ?? false) || selectedYear === currentYear;

  const yearSeasons = useMemo(
    () =>
      allSeasons.filter(
        (season) => seasonYearFromDates(season.startDate, season.endDate) === selectedYear,
      ),
    [allSeasons, selectedYear],
  );
  const totalCount = yearSeasons.length;
  const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE));

  useEffect(() => {
    if (initializedRef.current) {
      return;
    }
    initializedRef.current = true;

    const bootstrap = async (): Promise<void> => {
      try {
        setIsLoadingYears(true);
        setError(null);
        const [seasons, teams] = await Promise.all([
          hockeySeasonService.getAll(audience.teamCategory),
          hockeyTeamService.getAll(audience.teamCategory),
        ]);
        setAllSeasons(seasons);
        setTeamsById(new Map(teams.map((team) => [team.id, team])));

        const yearList = [...new Set(seasons.map((season) => seasonYearFromDates(season.startDate, season.endDate)))]
          .filter(Boolean)
          .sort((a, b) => b.localeCompare(a));
        const activeSeason = seasons.find((season) => season.isActive);
        const defaultYear = activeSeason
          ? seasonYearFromDates(activeSeason.startDate, activeSeason.endDate)
          : yearList[0] ?? '';
        const urlYear = searchParams.get('year');
        const urlPage = Number(searchParams.get('page') || '1');
        const initialYear = urlYear && yearList.includes(urlYear) ? urlYear : defaultYear;
        setSelectedYear(initialYear);
        setCurrentPage(Number.isFinite(urlPage) && urlPage > 0 ? urlPage : 1);
      } catch {
        setError(t('hockeyPage.error'));
      } finally {
        setIsLoadingYears(false);
      }
    };

    void bootstrap();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (isLoadingYears || !selectedYear) {
      return;
    }

    const next = new URLSearchParams();
    next.set('year', selectedYear);
    if (currentPage > 1) {
      next.set('page', String(currentPage));
    }
    setSearchParams(next, { replace: true });

    const seasonsForYear = allSeasons.filter(
      (season) => seasonYearFromDates(season.startDate, season.endDate) === selectedYear,
    );
    const pageSeasons = seasonsForYear.slice((currentPage - 1) * PAGE_SIZE, currentPage * PAGE_SIZE);

    const loadSeasonDetails = async (): Promise<void> => {
      try {
        setIsLoadingSeasons(true);
        setUpcomingMatches([]);
        const initial: SeasonWithStandings[] = pageSeasons.map((season) => ({
          season,
          standings: [],
          standingsLoading: true,
        }));
        setSeasonsData(initial);
        setIsLoadingSeasons(false);

        const standingsTask = Promise.all(
          pageSeasons.map(async (season) => {
            try {
              const standings = uniqueHockeyStandingsByTeamId(
                await hockeyStatisticsService.getStandings(season.id),
              );
              setSeasonsData((prev) =>
                prev.map((item) =>
                  item.season.id === season.id
                    ? { ...item, standings, standingsLoading: false }
                    : item,
                ),
              );
            } catch {
              setSeasonsData((prev) =>
                prev.map((item) =>
                  item.season.id === season.id ? { ...item, standingsLoading: false } : item,
                ),
              );
            }
          }),
        );

        const activeSeasons = pageSeasons.some((season) => season.isActive)
          ? pageSeasons.filter((season) => season.isActive)
          : pageSeasons;
        const matchesTask = Promise.all(
          activeSeasons.map(async (season) => {
            try {
              return await hockeyMatchService.getByCompetition(season.id);
            } catch {
              return [];
            }
          }),
        ).then((matchLists) => {
          const now = Date.now();
          const upcoming = matchLists
            .flat()
            .filter(
              (match) =>
                match.status === 'Scheduled' &&
                new Date(match.scheduledStartTime).getTime() >= now,
            )
            .sort(
              (a, b) =>
                new Date(a.scheduledStartTime).getTime() - new Date(b.scheduledStartTime).getTime(),
            )
            .slice(0, MAX_UPCOMING_MATCHES);
          setUpcomingMatches(upcoming);
        });

        await Promise.all([standingsTask, matchesTask]);
      } catch {
        setError(t('hockeyPage.error'));
        setIsLoadingSeasons(false);
      }
    };

    void loadSeasonDetails();
  }, [isLoadingYears, selectedYear, currentPage, reloadToken, setSearchParams, t, allSeasons]);

  useEffect(() => {
    if (!selectedYear) {
      setContentBlocks([]);
      return;
    }

    const loadBlocks = async (): Promise<void> => {
      try {
        setIsLoadingBlocks(true);
        const blocks = await seasonContentBlockService.getBySportAndYear(
          SportsCategory.Icehockey,
          selectedYear,
        );
        setContentBlocks(blocks);
      } catch {
        setContentBlocks([]);
      } finally {
        setIsLoadingBlocks(false);
      }
    };

    void loadBlocks();
  }, [selectedYear, reloadToken]);

  const handleYearSelect = (year: string): void => {
    if (year === selectedYear) {
      return;
    }
    setSelectedYear(year);
    setCurrentPage(1);
  };

  const handlePageChange = (page: number): void => {
    setCurrentPage(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const renderTeamLogo = (logo: string | null | undefined) =>
    logo && logo.trim() !== '' ? (
      <img
        className="fb-team-logo"
        src={logo}
        alt=""
        onError={(event) => {
          (event.target as HTMLImageElement).style.visibility = 'hidden';
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
            <Link to={`/hockey/league/${season.id}?tab=fixtures`}>{t('leaguePage.tabs.fixtures')}</Link>
            <Link to={`/hockey/league/${season.id}?tab=results`}>{t('leaguePage.tabs.results')}</Link>
            <Link to={`/hockey/league/${season.id}?tab=statistics`}>
              {t('leaguePage.tabs.statistics')}
            </Link>
            <Link to={`/hockey/league/${season.id}?tab=summary`}>{t('leaguePage.tabs.summary')}</Link>
          </nav>
        )}
        <span className="fb-standings-card__label">{t('hockeyPage.standingsTitle')}</span>
        {standingsLoading ? (
          <div className="fb-standings-card__skeleton" aria-hidden="true">
            {Array.from({ length: 5 }).map((_, index) => (
              <div key={index} className="fb-standings-card__skeleton-row" />
            ))}
          </div>
        ) : displayStandings.length === 0 ? (
          <p className="fb-standings-card__empty">{t('hockeyPage.noStandings')}</p>
        ) : (
          <div className="fb-standings-table">
            <div className="fb-standings-table__head">
              <span className="fb-standings-table__rank">#</span>
              <span className="fb-standings-table__team">{t('hockeyPage.teamShort')}</span>
              <span className="fb-standings-table__num">{t('hockeyPage.gdShort')}</span>
              <span className="fb-standings-table__num fb-standings-table__num--pts">
                {t('hockeyPage.ptsShort')}
              </span>
            </div>
            {displayStandings.map((row, index) => {
              const team = teamsById.get(row.teamId);
              return (
                <div key={row.teamId} className="fb-standings-table__row">
                  <span className="fb-standings-table__rank">{index + 1}.</span>
                  <span className="fb-standings-table__team">
                    {renderTeamLogo(team?.logoUrl)}
                    <span className="fb-standings-table__team-name">
                      {team?.name ?? row.teamId.slice(0, 8)}
                    </span>
                  </span>
                  <span className="fb-standings-table__num">{row.goalDifference}</span>
                  <span className="fb-standings-table__num fb-standings-table__num--pts">
                    {row.points}
                  </span>
                </div>
              );
            })}
          </div>
        )}
        <Link to={`/hockey/league/${season.id}?tab=statistics`} className="fb-standings-card__full-link">
          {t('hockeyPage.viewFullTable')}
        </Link>
      </section>
    );
  };

  const renderUpcomingMatchesCard = () => {
    if (upcomingMatches.length === 0) {
      return null;
    }

    return (
      <section className="fb-upcoming-card">
        <h2 className="fb-upcoming-card__title">{t('hockeyPage.upcomingMatches')}</h2>
        <div className="fb-upcoming-card__list">
          {upcomingMatches.map((match) => {
            const [date, time] = formatMatchDateTime(match.scheduledStartTime);
            return (
              <Link key={match.id} to={`/hockey/match/${match.id}`} className="fb-upcoming-card__row">
                <span className="fb-upcoming-card__datetime">
                  <span>{date}</span>
                  <span>{time}</span>
                </span>
                <span className="fb-upcoming-card__teams">
                  <span className="fb-upcoming-card__team">
                    {renderTeamLogo(
                      teamsById.get(
                        match.matchTeams.find((item) => item.teamSlot === 'Home')?.teamId ?? '',
                      )?.logoUrl,
                    )}
                    <span>{teamNameFromMatch(match, 'Home', teamsById) ?? t('hockeyPage.tbd')}</span>
                  </span>
                  <span className="fb-upcoming-card__team">
                    {renderTeamLogo(
                      teamsById.get(
                        match.matchTeams.find((item) => item.teamSlot === 'Away')?.teamId ?? '',
                      )?.logoUrl,
                    )}
                    <span>{teamNameFromMatch(match, 'Away', teamsById) ?? t('hockeyPage.tbd')}</span>
                  </span>
                </span>
              </Link>
            );
          })}
        </div>
      </section>
    );
  };

  const renderInfoCards = () => (
    <>
      {!isCurrentSeasonView && currentYear && (
        <article className="fb-info-card">
          <h2 className="fb-info-card__title">
            {t('hockeyPage.archiveTitle', { year: formatSeasonYearLabel(selectedYear) })}
          </h2>
          <p>{t('hockeyPage.archiveText')}</p>
          <p>
            <button
              type="button"
              className="fb-info-card__link-button"
              onClick={() => handleYearSelect(currentYear)}
            >
              {t('hockeyPage.backToCurrent', { year: formatSeasonYearLabel(currentYear) })}
            </button>
          </p>
        </article>
      )}
      {isLoadingBlocks ? (
        <div className="floorball-page__state floorball-page__state--inline">
          <LoadingSpinner variant="dark" text={t('hockeyPage.loading')} />
        </div>
      ) : contentBlocks.length === 0 ? (
        <article className="fb-info-card">
          <p>{t('hockeyPage.noContentBlocks')}</p>
        </article>
      ) : (
        contentBlocks.map((block) => (
          <article key={block.id} className="fb-info-card">
            <h2 className="fb-info-card__title">{block.title}</h2>
            <div
              className="fb-info-card__body"
              dangerouslySetInnerHTML={{ __html: block.contentHtml }}
            />
          </article>
        ))
      )}
    </>
  );

  if (isLoadingYears) {
    return (
      <PageTemplate title={t('sports.iceHockey')} fullBleed>
        <div className="floorball-page">
          <div className="floorball-page__state">
            <LoadingSpinner variant="light" text={t('hockeyPage.loading')} />
          </div>
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('sports.iceHockey')} fullBleed>
      <div className="floorball-page">
        <header className="fb-banner">
          <img className="fb-banner__image" src={bannerImage} alt="" aria-hidden="true" />
          <div className="fb-banner__content">
            <h1 className="fb-banner__title">{t('sports.iceHockey')}</h1>
            {years.length > 0 && (
              <div className="fb-banner__nav">
                <label className="fb-banner__select-wrap">
                  <span className="fb-banner__select-label">{t('hockeyPage.seasonLabel')}</span>
                  <select
                    className="fb-banner__select"
                    value={selectedYear}
                    onChange={(event) => handleYearSelect(event.target.value)}
                    aria-label={t('hockeyPage.seasonYears')}
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
                    initializedRef.current = false;
                    setReloadToken((token) => token + 1);
                  }}
                >
                  {t('hockeyPage.retry')}
                </button>
              </div>
            ) : (
              <div className="fb-columns">
                <div className="fb-columns__main">{renderInfoCards()}</div>
                <aside className="fb-columns__side">
                  {isLoadingSeasons && seasonsData.length === 0 ? (
                    <div className="floorball-page__state floorball-page__state--inline">
                      <LoadingSpinner variant="dark" text={t('hockeyPage.loading')} />
                    </div>
                  ) : seasonsData.length === 0 ? (
                    <div className="fb-info-card">
                      <p>{t('hockeyPage.noSeasonsForYear')}</p>
                    </div>
                  ) : (
                    (() => {
                      const ordered = [
                        ...seasonsData.filter((item) => item.season.isActive),
                        ...seasonsData.filter((item) => item.season.isActive === false),
                      ];
                      const darkCount =
                        ordered.filter((item) => item.season.isActive).length ||
                        (isCurrentSeasonView ? 1 : 0);
                      return (
                        <>
                          {ordered.slice(0, darkCount).map((item) => renderStandingsCard(item, true))}
                          {renderUpcomingMatchesCard()}
                          {ordered.slice(darkCount).map((item) => renderStandingsCard(item, false))}
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

export default HockeyPage;
