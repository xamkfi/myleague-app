import { useState, useEffect, useMemo, useRef } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  footballSeasonService,
  type FootballSeasonSummaryDto,
} from '../../api/football/footballSeasonService';
import { footballStatisticsService } from '../../api/football/footballStatistics';
import { footballMatchService } from '../../api/football/footballMatchService';
import { FootballMatchStatus } from '../../types/football/footballTypes';
import { useAudience } from '../../context/AudienceContext';
import type { SeasonContentBlockDto } from '../../types/common/seasonContent';
import SportLandingPage, {
  PAGE_SIZE,
  type SportLandingLabels,
  type SportLandingSeasonData,
  type SportLandingUpcomingMatch,
} from '../SportLanding/SportLandingPage';
import { formatSeasonYearLabel } from '../../utils/seasonYear';
import bannerImage from '../../assets/floorball-banner.png';

const MAX_UPCOMING_MATCHES = 6;

function FootballPage() {
  const { t } = useTranslation();
  const { audience } = useAudience();
  const [searchParams, setSearchParams] = useSearchParams();
  const initializedRef = useRef(false);

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
  const [contentBlocks, setContentBlocks] = useState<SeasonContentBlockDto[]>([]);

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

  useEffect(() => {
    if (!selectedYear) {
      setContentBlocks([]);
      return;
    }

    let cancelled = false;
    footballSeasonService
      .getFeaturedContentBlocks(selectedYear)
      .then((result) => {
        if (!cancelled) {
          setContentBlocks(result.blocks);
        }
      })
      .catch(() => {
        if (!cancelled) {
          setContentBlocks([]);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [selectedYear]);

  useEffect(() => {
    if (initializedRef.current) {
      return;
    }
    initializedRef.current = true;

    const bootstrap = async () => {
      try {
        setIsLoadingYears(true);
        setError(null);
        const yearList = await footballSeasonService.getYears();
        setYears(yearList);

        const urlYear = searchParams.get('year');
        const urlPage = Number(searchParams.get('page') || '1');
        const defaultYear =
          yearList.find((year) => year.hasActiveSeason)?.year ?? yearList[0]?.year ?? '';
        const initialYear =
          urlYear && yearList.some((year) => year.year === urlYear) ? urlYear : defaultYear;

        setSelectedYear(initialYear);
        setCurrentPage(Number.isFinite(urlPage) && urlPage > 0 ? urlPage : 1);
      } catch (err) {
        console.error('Failed to fetch season years:', err);
        setError(t('footballPage.error'));
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
              setSeasonsData((prev) =>
                prev.map((item) =>
                  item.season.id === season.id
                    ? { ...item, standings, standingsLoading: false }
                    : item,
                ),
              );
            } catch (err) {
              console.error(`Failed to fetch standings for season ${season.id}:`, err);
              setSeasonsData((prev) =>
                prev.map((item) =>
                  item.season.id === season.id ? { ...item, standingsLoading: false } : item,
                ),
              );
            }
          }),
        );

        const activeSeasons = seasons.some((season) => season.isActive)
          ? seasons.filter((season) => season.isActive)
          : seasons;
        const matchesTask = Promise.all(
          activeSeasons.map(async (season) => {
            try {
              const result = await footballMatchService.getBySeason(season.id);
              return result.data ?? [];
            } catch (err) {
              console.error(`Failed to fetch matches for season ${season.id}:`, err);
              return [];
            }
          }),
        ).then((matchLists) => {
          const now = Date.now();
          const upcoming = matchLists
            .flat()
            .filter(
              (match) =>
                match.status === FootballMatchStatus.Scheduled &&
                new Date(match.scheduledDateTime).getTime() >= now,
            )
            .sort(
              (left, right) =>
                new Date(left.scheduledDateTime).getTime()
                - new Date(right.scheduledDateTime).getTime(),
            )
            .slice(0, MAX_UPCOMING_MATCHES);
          setUpcomingMatches(upcoming);
        });

        await Promise.all([standingsTask, matchesTask]);
      } catch (err) {
        console.error('Failed to fetch seasons:', err);
        setError(t('footballPage.error'));
        setIsLoadingSeasons(false);
      }
    };

    void loadSeasons();
  }, [isLoadingYears, selectedYear, currentPage, reloadToken, setSearchParams, t, audience.teamCategory]);

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

  return (
    <SportLandingPage
      sport="football"
      title={t('sports.football')}
      bannerImage={bannerImage}
      labels={labels}
      years={years}
      selectedYear={selectedYear}
      currentYear={currentYear}
      isCurrentSeasonView={isCurrentSeasonView}
      onYearSelect={handleYearSelect}
      seasonsData={seasonsData}
      upcomingMatches={upcomingMatches}
      contentBlocks={contentBlocks}
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
