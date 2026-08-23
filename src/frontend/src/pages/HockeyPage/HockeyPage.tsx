import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import LoadingSpinner from '../../components/LoadingSpinner/LoadingSpinner';
import { hockeySeasonService } from '../../api/hockey/hockeySeasonService';
import { hockeyStatisticsService } from '../../api/hockey/hockeyStatisticsService';
import { hockeyTeamService } from '../../api/hockey/hockeyTeamService';
import type { HockeySeasonDto, HockeyTeamCompetitionStatisticsDto } from '../../types/hockey/hockeyTypes';
import { useAudience } from '../../context/AudienceContext';
import StatAbbr from '../../components/StatAbbr/StatAbbr';
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
          const standings = await hockeyStatisticsService.getStandings(season.id);
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
      setError(t('hockeyPage.error', 'Failed to load leagues'));
      setIsLoading(false);
    }
  }, [t, audience.teamCategory]);

  useEffect(() => {
    void fetchSeasons();
  }, [fetchSeasons]);

  if (isLoading) {
    return (
      <PageTemplate title={t('sports.iceHockey', 'Ice hockey')}>
        <div className="hockey-page">
          <div className="hockey-page__loading">
            <LoadingSpinner variant="light" text={t('hockeyPage.loading', 'Loading leagues...')} />
          </div>
        </div>
      </PageTemplate>
    );
  }

  return (
    <PageTemplate title={t('sports.iceHockey', 'Ice hockey')}>
      <div className="hockey-page">
        <div className="hockey-page__header">
          <h1 className="hockey-page__title">{t('sports.iceHockey', 'Ice hockey')}</h1>
          <p className="hockey-page__description">
            {t('hockeyPage.description', 'Browse hockey leagues, standings, and statistics.')}
          </p>
          <nav className="season-card__links">
            <Link to="/hockey/tournaments" className="season-card__link">
              {t('hockeyPage.tournaments', 'Tournaments')}
            </Link>
          </nav>
        </div>
        {error && (
          <div className="hockey-page__error">
            <p>{error}</p>
            <button type="button" className="hockey-page__retry-btn" onClick={() => void fetchSeasons()}>
              {t('hockeyPage.retry', 'Try again')}
            </button>
          </div>
        )}
        {seasonsData.length === 0 ? (
          <div className="hockey-page__empty">
            <p>{t('hockeyPage.noSeasons', 'No leagues available')}</p>
          </div>
        ) : (
          <div className="hockey-page__seasons">
            {seasonsData.map((data) => (
              <div key={data.season.id} className="season-card">
                <div className="season-card__header">
                  <h2 className="season-card__title">{data.season.name}</h2>
                  {data.season.isActive && <span className="season-card__badge season-card__badge--active">{t('hockeyPage.active', 'Active')}</span>}
                </div>
                <nav className="season-card__links">
                  <Link to={`/hockey/league/${data.season.id}?tab=fixtures`} className="season-card__link">{t('hockeyPage.fixtures', 'Fixtures')}</Link>
                  <Link to={`/hockey/league/${data.season.id}?tab=statistics`} className="season-card__link">{t('hockeyPage.standings', 'Standings')}</Link>
                  <Link to={`/hockey/league/${data.season.id}?tab=players`} className="season-card__link">{t('hockeyPage.playerStats', 'Player Statistics')}</Link>
                </nav>
                {data.standingsLoading && (
                  <div className="standings-table standings-table--loading">
                    <div className="standings-table__header">
                      <span className="standings-table__header-title">
                        {t('hockeyPage.standingsTitle', 'STANDINGS')} {data.season.name}
                      </span>
                    </div>
                    <div className="standings-table__loading">
                      {t('hockeyPage.loadingStandings', 'Loading standings...')}
                    </div>
                  </div>
                )}
                {data.standings.length > 0 && (
                  <div className="standings-table">
                    <div className="standings-table__header">
                      <span className="standings-table__header-title">{t('hockeyPage.standingsTitle', 'STANDINGS')} {data.season.name}</span>
                    </div>
                    <table className="standings-table__table">
                      <thead>
                        <tr>
                          <th>#</th>
                          <th>{t('hockeyPage.team', 'TEAM')}</th>
                          <th className="standings-table__games">
                            <StatAbbr abbr={t('hockeyPage.gamesShort', 'GP')} title={t('hockeyPage.gamesShortTitle', 'Games played')} />
                          </th>
                          <th className="standings-table__points">
                            <StatAbbr abbr={t('hockeyPage.pointsShort', 'PTS')} title={t('hockeyPage.pointsShortTitle', 'Points')} />
                          </th>
                        </tr>
                      </thead>
                      <tbody>
                        {data.standings.slice(0, MAX_STANDINGS_PREVIEW).map((row) => (
                          <tr key={row.teamId}>
                            <td>{row.standingRank || ''}</td>
                            <td>{data.teamNames.get(row.teamId) ?? row.teamId.slice(0, 8)}</td>
                            <td>{row.gamesPlayed}</td>
                            <td>{row.points}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                    <Link to={`/hockey/league/${data.season.id}?tab=statistics`} className="standings-table__full-link">
                      {t('hockeyPage.viewFullTable', '>> full table')}
                    </Link>
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </PageTemplate>
  );
}

export default HockeyPage;
