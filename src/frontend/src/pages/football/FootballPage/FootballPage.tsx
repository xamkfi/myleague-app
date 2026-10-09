import { useState, useEffect, useMemo, useRef } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  footballSeasonService,
  type FootballSeasonSummaryDto,
} from '../../../api/football/footballSeasonService';
import { footballStatisticsService } from '../../../api/football/footballStatistics';
import { footballMatchService } from '../../../api/football/footballMatchService';
import { FootballMatchStatus } from '../../../types/football/footballTypes';
import { useAudience } from '../../../context/AudienceContext';
import type { SeasonContentBlockDto } from '../../../types/common/seasonContent';
import SportLandingPage, {
  PAGE_SIZE,
  type SportLandingLabels,
  type SportLandingSeasonData,
  type SportLandingUpcomingMatch,
} from '../../SportLanding/SportLandingPage';
import { filterPublicSportYears, formatSeasonYearLabel, pickDefaultSportYear } from '../../../utils/seasonYear';
const MAX_UPCOMING_MATCHES = 6;

function FootballPage() {
  const { t } = useTranslation();
  const { audience } = useAudience();
  const [searchParams, setSearchParams] = useSearchParams();
  const initialQueryRef = useRef({
    year: searchParams.get('year'),
    page: searchParams.get('page'),
  });
  const hasAppliedInitialQuery = useRef(false);

  const [years, setYears] = useState<Array<{ year: string; hasActiveSeason: boolean }>>([]);
  const [selectedYear, setSelectedYear] = useState<string>('');
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [seasonsData, setSeasonsData] = useState<SportLandingSeasonData[]>([]);
  const [upcomingMatches, setUpcomingMatches] = useState<SportLandingUpcomingMatch[]>([]);
  const [isLoadingYears, setIsLoadingYears] = useState(true);
  const [isLoadingSeasons, setIsLoadingSeasons] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [reloadToken, setReloadToken] = useState(0);
  const [contentBlocks, setContentBlocks] = useState<{
    seasonId: string;
    blocks: SeasonContentBlockDto[];
  } | null>(null);

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

  // Intro blocks come from a season that is on screen, so they follow the audience
  // filter and also show for a season that is not active or completed yet.
  const featuredSeasonId =
    (seasonsData.find((item) => item.season.isActive) ?? seasonsData[0])?.season.id ?? null;

  useEffect(() => {
    if (!featuredSeasonId) {
      setContentBlocks(null);
      return;
    }

    let cancelled = false;
    footballSeasonService
      .getContentBlocks(featuredSeasonId)
      .then((result) => {
        if (!cancelled) {
          setContentBlocks({ seasonId: featuredSeasonId, blocks: result.blocks });
        }
      })
      .catch(() => {
        if (!cancelled) {
          setContentBlocks(null);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [featuredSeasonId]);

  useEffect(() => {
    let cancelled = false;

    const loadYears = async () => {
      try {
        setIsLoadingYears(true);
        setError(null);
        const yearList = filterPublicSportYears(
          await footballSeasonService.getYears(audience.teamCategory),
        );
        if (cancelled) {
          return;
        }
        setYears(yearList);

        if (!hasAppliedInitialQuery.current) {
          hasAppliedInitialQuery.current = true;
          const urlYear = initialQueryRef.current.year;
          const urlPage = Number(initialQueryRef.current.page || '1');
          setSelectedYear(pickDefaultSportYear(yearList, urlYear));
          setCurrentPage(Number.isFinite(urlPage) && urlPage > 0 ? urlPage : 1);
          return;
        }

        setSelectedYear((current) =>
          yearList.some((year) => year.year === current)
            ? current
            : pickDefaultSportYear(yearList, null),
        );
        setCurrentPage(1);
      } catch (err) {
        console.error('Failed to fetch season years:', err);
        if (!cancelled) {
          setError(t('footballPage.error'));
        }
      } finally {
        if (!cancelled) {
          setIsLoadingYears(false);
        }
      }
    };

    void loadYears();
    return () => {
      cancelled = true;
    };
    // `t` is omitted so language changes do not refetch season years.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [audience.teamCategory]);

  useEffect(() => {
    if (isLoadingYears) {
      return;
    }

    if (!selectedYear) {
      setSeasonsData([]);
      setUpcomingMatches([]);
      setTotalCount(0);
      setTotalPages(0);
      return;
    }

    const next = new URLSearchParams();
    next.set('year', selectedYear);
    if (currentPage > 1) {
      next.set('page', String(currentPage));
    }
    setSearchParams(next, { replace: true });

    let cancelled = false;
    const loadSeasons = async () => {
      try {
        setIsLoadingSeasons(true);
        setError(null);
        setUpcomingMatches([]);

        const response = await footballSeasonService.getPaged({
          page: currentPage,
          pageSize: PAGE_SIZE,
          seasonYear: selectedYear,
          teamCategory: audience.teamCategory,
        });

        const seasons: FootballSeasonSummaryDto[] = response.data ?? [];
        if (cancelled) {
          return;
        }
        setTotalCount(response.pagination.totalCount);
        setTotalPages(response.pagination.totalPages);

        setSeasonsData(
          seasons.map((season) => ({
            season: { id: season.id, name: season.name, isActive: season.isActive },
            standings: [],
            standingsLoading: true,
          })),
        );
        setIsLoadingSeasons(false);

        const standingsTask = Promise.all(
          seasons.map(async (season) => {
            try {
              const standings = await footballStatisticsService.getTeamStandings(season.id);
              if (cancelled) {
                return;
              }
              setSeasonsData((prev) =>
                prev.map((item) =>
                  item.season.id === season.id
                    ? { ...item, standings, standingsLoading: false, teamsAdvancing: season.teamsAdvancing ?? 0 }
                    : item,
                ),
              );
            } catch (err) {
              console.error(`Failed to fetch standings for season ${season.id}:`, err);
              if (cancelled) {
                return;
              }
              setSeasonsData((prev) =>
                prev.map((item) =>
                  item.season.id === season.id ? { ...item, standingsLoading: false } : item,
                ),
              );
            }
          }),
        );

        const hasActiveSeason = seasons.some((season) => season.isActive);
        const upcomingFilter = {
          page: 1,
          pageSize: MAX_UPCOMING_MATCHES,
          status: FootballMatchStatus.Scheduled,
          startDate: new Date().toISOString(),
          sortOrder: 'asc',
          competitionType: 'Season' as const,
          teamCategory: audience.teamCategory,
        };
        const upcomingRequests = hasActiveSeason
          ? [footballMatchService.getAll({ ...upcomingFilter, activeOnly: true })]
          : seasons.map((season) => footballMatchService.getAll({ ...upcomingFilter, competitionId: season.id }));
        const matchesTask = Promise.all(
          upcomingRequests.map((request) =>
            request
              .then((result) => result.data ?? [])
              .catch((err: unknown) => {
                console.error('Failed to fetch upcoming matches:', err);
                return [];
              })),
        ).then((matchLists) => {
          const upcoming = matchLists
            .flat()
            .sort(
              (left, right) =>
                new Date(left.scheduledDateTime).getTime()
                - new Date(right.scheduledDateTime).getTime(),
            )
            .slice(0, MAX_UPCOMING_MATCHES);
          if (!cancelled) {
            setUpcomingMatches(upcoming);
          }
        });

        await Promise.all([standingsTask, matchesTask]);
      } catch (err) {
        console.error('Failed to fetch seasons:', err);
        if (!cancelled) {
          setError(t('footballPage.error'));
          setIsLoadingSeasons(false);
        }
      }
    };

    void loadSeasons();
    return () => {
      cancelled = true;
    };
    // `t` is omitted so language changes do not refetch standings and matches.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isLoadingYears, selectedYear, currentPage, reloadToken, setSearchParams, audience.teamCategory]);

  const handleYearSelect = (year: string) => {
    if (year === selectedYear) {
      return;
    }
    setSelectedYear(year);
    setCurrentPage(1);
  };

  const labels: SportLandingLabels = {
    loading: t('footballPage.loading'),
    error: t('footballPage.error'),
    retry: t('footballPage.retry'),
    noSeasonsForYear: t('footballPage.noSeasonsForYear'),
    seasonLabel: t('footballPage.seasonLabel'),
    seasonYears: t('footballPage.seasonYears'),
    standingsTitle: t('footballPage.standingsTitle'),
    teamShort: t('footballPage.teamShort'),
    gdShort: t('footballPage.gdShort'),
    ptsShort: t('footballPage.ptsShort'),
    noStandings: t('footballPage.noStandings'),
    viewFullTable: t('footballPage.viewFullTable'),
    upcomingMatches: t('footballPage.upcomingMatches'),
    tbd: t('footballPage.tbd'),
    archiveTitle: t('footballPage.archiveTitle', { year: formatSeasonYearLabel(selectedYear) }),
    archiveText: t('footballPage.archiveText'),
    backToCurrent: t('footballPage.backToCurrent', { year: formatSeasonYearLabel(currentYear) }),
    fixtures: t('leaguePage.tabs.fixtures'),
    results: t('leaguePage.tabs.results'),
    statistics: t('leaguePage.tabs.statistics'),
    summary: t('leaguePage.tabs.summary'),
    rules: t('nav.rules'),
  };

  const fallbackInfo = (
    <>
      {[
        { title: t('footballPage.info.introTitle'), paragraphs: ['intro1', 'intro2', 'intro3'] },
        {
          title: t('footballPage.info.seriesTitle'),
          paragraphs: ['series1', 'series2', 'series3', 'series4'],
        },
        { title: t('footballPage.info.loanTitle'), paragraphs: ['loan1'] },
        { title: t('footballPage.info.feeTitle'), paragraphs: ['fee1', 'fee2', 'fee3'] },
      ].map((section) => (
        <article key={section.title} className="fb-info-card">
          <h2 className="fb-info-card__title">{section.title}</h2>
          {section.paragraphs.map((key) => (
            <p key={key}>{t(`footballPage.info.${key}`)}</p>
          ))}
        </article>
      ))}
      <article className="fb-info-card">
        <h2 className="fb-info-card__title">{t('footballPage.info.contactTitle')}</h2>
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

  const visibleContentBlocks =
    contentBlocks && contentBlocks.seasonId === featuredSeasonId ? contentBlocks.blocks : [];

  return (
    <SportLandingPage
      sport="football"
      title={t('sports.football')}
      labels={labels}
      years={years}
      selectedYear={selectedYear}
      currentYear={currentYear}
      isCurrentSeasonView={isCurrentSeasonView}
      onYearSelect={handleYearSelect}
      seasonsData={seasonsData}
      upcomingMatches={upcomingMatches}
      contentBlocks={visibleContentBlocks}
      fallbackInfo={fallbackInfo}
      isLoadingYears={isLoadingYears}
      isLoadingSeasons={isLoadingSeasons}
      error={error}
      onRetry={() => {
        setError(null);
        setReloadToken((token) => token + 1);
      }}
      currentPage={currentPage}
      totalPages={totalPages}
      totalCount={totalCount}
      onPageChange={(page) => {
        setCurrentPage(page);
        window.scrollTo({ top: 0, behavior: 'smooth' });
      }}
    />
  );
}

export default FootballPage;
