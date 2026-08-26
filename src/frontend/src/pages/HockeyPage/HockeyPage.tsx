import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import LoadingSpinner from '../../components/LoadingSpinner/LoadingSpinner';
import SeasonStandingsCard from '../../components/SeasonStandingsCard/SeasonStandingsCard';
import { hockeySeasonService } from '../../api/hockey/hockeySeasonService';
import { hockeyStatisticsService } from '../../api/hockey/hockeyStatisticsService';
import { hockeyTeamService } from '../../api/hockey/hockeyTeamService';
import type { HockeySeasonDto, HockeyTeamCompetitionStatisticsDto } from '../../types/hockey/hockeyTypes';
import { useAudience } from '../../context/AudienceContext';
import { uniqueHockeyStandingsByTeamId } from '../../utils/hockeyLookups';
import './HockeyPage.scss';

interface SeasonWithStandings {
  season: HockeySeasonDto;
  standings: HockeyTeamCompetitionStatisticsDto[];
  teamNames: Map<string, string>;
  standingsLoading: boolean;
}

const MAX_STANDINGS_PREVIEW = 9;

function HockeyPage() {
  const { t } = useTranslation();
  const { audience } = useAudience();
  const [seasonsData, setSeasonsData] = useState<SeasonWithStandings[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchSeasons = useCallback(async (): Promise<void> => {
    try {
      setIsLoading(true);
      setError(null);
      const [seasons, teams] = await Promise.all([
        hockeySeasonService.getAll(audience.teamCategory),
        hockeyTeamService.getAll(audience.teamCategory),
      ]);
      const teamNames = new Map(teams.map((team) => [team.id, team.name]));
      const sorted = [...seasons].sort((a, b) => {
        if (a.isActive && !b.isActive) return -1;
        if (!a.isActive && b.isActive) return 1;
        return new Date(b.startDate).getTime() - new Date(a.startDate).getTime();
      });
      setSeasonsData(sorted.map((season) => ({
        season,
        standings: [],
        teamNames,
        standingsLoading: true,
      })));
      setIsLoading(false);
      for (const season of sorted) {
        try {
          const standings = uniqueHockeyStandingsByTeamId(
            await hockeyStatisticsService.getStandings(season.id),
          );
          setSeasonsData((prev) => prev.map((item) => (
            item.season.id === season.id
              ? { ...item, standings, standingsLoading: false }
              : item
          )));
        } catch {
          setSeasonsData((prev) => prev.map((item) => (
            item.season.id === season.id ? { ...item, standingsLoading: false } : item
          )));
        }
      }
    } catch {
      setError(t('hockeyPage.error'));
      setIsLoading(false);
    }
  }, [t, audience.teamCategory]);

  useEffect(() => {
    void fetchSeasons();
  }, [fetchSeasons]);

  if (isLoading) {
    return (
      <PageTemplate title={t('sports.iceHockey')}>
        <div className="hockey-page">
          <div className="hockey-page__loading">
            <LoadingSpinner variant="light" text={t('hockeyPage.loading')} />
          </div>
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('sports.iceHockey')}>
      <div className="hockey-page">
        <div className="hockey-page__header">
          <h1 className="hockey-page__title">{t('sports.iceHockey')}</h1>
          <p className="hockey-page__description">
            {t('hockeyPage.description')}
          </p>
          <nav className="season-card__links">
            <Link to="/hockey/tournaments" className="season-card__link">
              {t('hockeyPage.tournaments')}
            </Link>
          </nav>
        </div>
        {error && (
          <div className="hockey-page__error">
            <p>{error}</p>
            <button type="button" className="hockey-page__retry-btn" onClick={() => void fetchSeasons()}>
              {t('hockeyPage.retry')}
            </button>
          </div>
        )}
        {seasonsData.length === 0 ? (
          <div className="hockey-page__empty">
            <p>{t('hockeyPage.noSeasons')}</p>
          </div>
        ) : (
          <div className="hockey-page__seasons">
            {seasonsData.map((data) => {
              const namedStandings = uniqueHockeyStandingsByTeamId(data.standings).map((row) => ({
                teamId: row.teamId,
                teamName: data.teamNames.get(row.teamId) ?? row.teamId.slice(0, 8),
                goalDifference: row.goalDifference,
                points: row.points,
              }));
              return (
                <SeasonStandingsCard
                  key={data.season.id}
                  sport="hockey"
                  seasonId={data.season.id}
                  seasonName={data.season.name}
                  standings={namedStandings}
                  standingsLoading={data.standingsLoading}
                  isDark={data.season.isActive}
                  maxRows={MAX_STANDINGS_PREVIEW}
                  labels={{
                    standingsTitle: t('hockeyPage.standingsTitle'),
                    teamShort: t('hockeyPage.team'),
                    gdShort: t('hockeyPage.colGd'),
                    ptsShort: t('hockeyPage.pointsShort'),
                    noStandings: t('hockeyPage.noStats'),
                    viewFullTable: t('hockeyPage.viewFullTable'),
                  }}
                  navLinks={[
                    { tab: 'fixtures', label: t('hockeyPage.fixtures') },
                    { tab: 'statistics', label: t('hockeyPage.standings') },
                    { tab: 'players', label: t('hockeyPage.playerStats') },
                  ]}
                />
              );
            })}
          </div>
        )}
      </div>
    </PageTemplate>
  );
}

export default HockeyPage;
