import { useState, useEffect, useMemo, useRef } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { hockeySeasonService } from '../../api/hockey/hockeySeasonService';
import { hockeyStatisticsService } from '../../api/hockey/hockeyStatisticsService';
import { hockeyMatchService } from '../../api/hockey/hockeyMatchService';
import { hockeyTeamService } from '../../api/hockey/hockeyTeamService';
import type { HockeySeasonDto, HockeyTeamDto } from '../../types/hockey/hockeyTypes';
import { useAudience } from '../../context/AudienceContext';
import type { SeasonContentBlockDto } from '../../types/common/seasonContent';
import { uniqueHockeyStandingsByTeamId } from '../../utils/hockeyLookups';
import { formatSeasonYearLabel, seasonYearFromDates } from '../../utils/seasonYear';
import SportLandingPage, {
  PAGE_SIZE,
  type SportLandingLabels,
  type SportLandingSeasonData,
  type SportLandingUpcomingMatch,
  type SportLandingYear,
} from '../SportLanding/SportLandingPage';
import bannerImage from '../../assets/floorball-banner.png';

const MAX_UPCOMING_MATCHES = 6;

function toYearList(seasons: HockeySeasonDto[]): SportLandingYear[] {
  const byYear = new Map<string, SportLandingYear>();
  for (const season of seasons) {
    const year = seasonYearFromDates(season.startDate, season.endDate);
    if (!year) {
      continue;
    }
    const existing = byYear.get(year);
    if (existing) {
      existing.hasActiveSeason = existing.hasActiveSeason || season.isActive;
    } else {
      byYear.set(year, { year, hasActiveSeason: season.isActive });
    }
  }
  return [...byYear.values()].sort((left, right) => right.year.localeCompare(left.year));
}

function HockeyPage() {
  const { t } = useTranslation();
  const { audience } = useAudience();
  const [searchParams, setSearchParams] = useSearchParams();
  const initialQueryRef = useRef({
    year: searchParams.get('year'),
    page: searchParams.get('page'),
  });

  const [allSeasons, setAllSeasons] = useState<HockeySeasonDto[]>([]);
  const [teams, setTeams] = useState<HockeyTeamDto[]>([]);
  const [years, setYears] = useState<SportLandingYear[]>([]);
  const [selectedYear, setSelectedYear] = useState<string>('');
  const [currentPage, setCurrentPage] = useState(1);
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

  const yearSeasons = useMemo(
    () =>
      allSeasons.filter(
        (season) => seasonYearFromDates(season.startDate, season.endDate) === selectedYear,
      ),
    [allSeasons, selectedYear],
  );
  const totalCount = yearSeasons.length;
  const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE));
  const pagedSeasons = useMemo(() => {
    const start = (currentPage - 1) * PAGE_SIZE;
    return yearSeasons.slice(start, start + PAGE_SIZE);
  }, [yearSeasons, currentPage]);

  const teamNames = useMemo(
    () => new Map(teams.map((team) => [team.id, team.name])),
    [teams],
  );
  const teamLogos = useMemo(
    () => new Map(teams.map((team) => [team.id, team.logoUrl])),
    [teams],
  );

  useEffect(() => {
    const bootstrap = async () => {
      try {
        setIsLoadingYears(true);
        setError(null);
        const [seasons, teamList] = await Promise.all([
          hockeySeasonService.getAll(audience.teamCategory),
          hockeyTeamService.getAll(audience.teamCategory),
        ]);
        const sorted = [...seasons].sort((left, right) => {
          if (left.isActive && !right.isActive) {
            return -1;
          }
          if (!left.isActive && right.isActive) {
            return 1;
          }
          return new Date(right.startDate).getTime() - new Date(left.startDate).getTime();
        });
        setAllSeasons(sorted);
        setTeams(teamList);
        const yearList = toYearList(sorted);
        setYears(yearList);

        const urlYear = initialQueryRef.current.year;
        const urlPage = Number(initialQueryRef.current.page || '1');
        const defaultYear =
          yearList.find((year) => year.hasActiveSeason)?.year ?? yearList[0]?.year ?? '';
        const initialYear =
          urlYear && yearList.some((year) => year.year === urlYear) ? urlYear : defaultYear;

        setSelectedYear(initialYear);
        setCurrentPage(Number.isFinite(urlPage) && urlPage > 0 ? urlPage : 1);
      } catch (err) {
        console.error('Failed to fetch hockey seasons:', err);
        setError(t('hockeyPage.error'));
      } finally {
        setIsLoadingYears(false);
      }
    };

    void bootstrap();
  }, [audience.teamCategory, reloadToken, t]);

  useEffect(() => {
    if (!selectedYear) {
      setContentBlocks([]);
      return;
    }

    let cancelled = false;
    const seasonsInYear = allSeasons.filter(
      (season) => seasonYearFromDates(season.startDate, season.endDate) === selectedYear,
    );
    const featuredSeason =
      seasonsInYear.find((season) => season.isActive) ?? seasonsInYear[0];

    const load = featuredSeason
      ? hockeySeasonService.getContentBlocks(featuredSeason.id)
      : hockeySeasonService.getFeaturedContentBlocks();

    load
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
  }, [selectedYear, allSeasons]);

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

    const loadPage = async () => {
      try {
        setIsLoadingSeasons(true);
        setError(null);
        setUpcomingMatches([]);
        setSeasonsData(
          pagedSeasons.map((season) => ({
            season: { id: season.id, name: season.name, isActive: season.isActive },
            standings: [],
            standingsLoading: true,
          })),
        );
        setIsLoadingSeasons(false);

        const standingsTask = Promise.all(
          pagedSeasons.map(async (season) => {
            try {
              const standings = uniqueHockeyStandingsByTeamId(
                await hockeyStatisticsService.getStandings(season.id),
              ).map((row) => ({
                teamId: row.teamId,
                teamName: teamNames.get(row.teamId) ?? row.teamId.slice(0, 8),
                teamLogo: teamLogos.get(row.teamId),
                goalDifference: row.goalDifference,
                points: row.points,
              }));
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

        const activeSeasons = pagedSeasons.some((season) => season.isActive)
          ? pagedSeasons.filter((season) => season.isActive)
          : pagedSeasons;
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
              (left, right) =>
                new Date(left.scheduledStartTime).getTime()
                - new Date(right.scheduledStartTime).getTime(),
            )
            .slice(0, MAX_UPCOMING_MATCHES)
            .map((match) => ({
              id: match.id,
              scheduledDateTime: match.scheduledStartTime,
              homeTeamId: match.homeTeamId,
              awayTeamId: match.awayTeamId,
              homeTeamName: match.homeTeamId ? teamNames.get(match.homeTeamId) ?? null : null,
              awayTeamName: match.awayTeamId ? teamNames.get(match.awayTeamId) ?? null : null,
              homeTeamLogo: match.homeTeamId ? teamLogos.get(match.homeTeamId) ?? null : null,
              awayTeamLogo: match.awayTeamId ? teamLogos.get(match.awayTeamId) ?? null : null,
            }));
          setUpcomingMatches(upcoming);
        });

        await Promise.all([standingsTask, matchesTask]);
      } catch (err) {
        console.error('Failed to fetch hockey seasons:', err);
        setError(t('hockeyPage.error'));
        setIsLoadingSeasons(false);
      }
    };

    void loadPage();
  }, [
    isLoadingYears,
    selectedYear,
    currentPage,
    pagedSeasons,
    setSearchParams,
    t,
    teamNames,
    teamLogos,
  ]);

  const handleYearSelect = (year: string) => {
    if (year === selectedYear) {
      return;
    }
    setSelectedYear(year);
    setCurrentPage(1);
  };

  const labels: SportLandingLabels = {
    loading: t('hockeyPage.loading'),
    error: t('hockeyPage.error'),
    retry: t('hockeyPage.retry'),
    noSeasonsForYear: t('hockeyPage.noSeasonsForYear'),
    seasonLabel: t('hockeyPage.seasonLabel'),
    seasonYears: t('hockeyPage.seasonYears'),
    standingsTitle: t('hockeyPage.standingsTitle'),
    teamShort: t('hockeyPage.teamShort'),
    gdShort: t('hockeyPage.gdShort'),
    ptsShort: t('hockeyPage.ptsShort'),
    noStandings: t('hockeyPage.noStandings'),
    viewFullTable: t('hockeyPage.viewFullTable'),
    upcomingMatches: t('hockeyPage.upcomingMatches'),
    tbd: t('hockeyPage.tbd'),
    archiveTitle: t('hockeyPage.archiveTitle', { year: formatSeasonYearLabel(selectedYear) }),
    archiveText: t('hockeyPage.archiveText'),
    backToCurrent: t('hockeyPage.backToCurrent', { year: formatSeasonYearLabel(currentYear) }),
    fixtures: t('leaguePage.tabs.fixtures'),
    results: t('leaguePage.tabs.results'),
    statistics: t('leaguePage.tabs.statistics'),
    summary: t('leaguePage.tabs.summary'),
  };

  const fallbackInfo = (
    <>
      <article className="fb-info-card">
        <h2 className="fb-info-card__title">{t('sports.iceHockey')}</h2>
        <p>{t('hockeyPage.description')}</p>
        <p>
          <Link to="/hockey/tournaments" className="fb-info-card__link-button">
            {t('hockeyPage.tournaments')}
          </Link>
        </p>
      </article>
    </>
  );

  return (
    <SportLandingPage
      sport="hockey"
      title={t('sports.iceHockey')}
      bannerImage={bannerImage}
      labels={labels}
      extraNavLinks={[{ tab: 'players', label: t('hockeyPage.playerStats') }]}
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
      totalPages={totalCount === 0 ? 0 : totalPages}
      totalCount={totalCount}
      onPageChange={(page) => {
        setCurrentPage(page);
        window.scrollTo({ top: 0, behavior: 'smooth' });
      }}
    />
  );
}

export default HockeyPage;
