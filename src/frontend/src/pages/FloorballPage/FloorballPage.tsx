import { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import LoadingSpinner from '../../components/LoadingSpinner/LoadingSpinner';
import { floorballSeasonService, type FloorballSeasonDto } from '../../api/floorball/floorballSeasonService';
import { floorballStatisticsService, type FloorballTeamSeasonStatisticsDto } from '../../api/floorball/floorballStatistics';
import './FloorballPage.scss';

interface SeasonWithStandings {
  season: FloorballSeasonDto;
  standings: FloorballTeamSeasonStatisticsDto[];
  standingsLoading: boolean;
  standingsError: string | null;
}

const MAX_STANDINGS_PREVIEW = 9;

function FloorballPage() {
  const { t } = useTranslation();
  const [seasonsData, setSeasonsData] = useState<SeasonWithStandings[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchStandingsForSeason = useCallback(async (seasonId: string): Promise<FloorballTeamSeasonStatisticsDto[]> => {
    try {
      const standings = await floorballStatisticsService.getTeamStandings(seasonId);
      return standings;
    } catch (err) {
      console.error(`Failed to fetch standings for season ${seasonId}:`, err);
      return [];
    }
  }, []);

  const fetchSeasons = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      
      const response = await floorballSeasonService.getAll();
      const seasons = response.data || [];
      
      // Sort seasons: active first, then by start date descending
      const sortedSeasons = [...seasons].sort((a, b) => {
        if (a.isActive && !b.isActive) return -1;
        if (!a.isActive && b.isActive) return 1;
        return new Date(b.startDate).getTime() - new Date(a.startDate).getTime();
      });

      // Initialize seasons with loading states
      const initialData: SeasonWithStandings[] = sortedSeasons.map(season => ({
        season,
        standings: [],
        standingsLoading: true,
        standingsError: null
      }));
      
      setSeasonsData(initialData);
      setIsLoading(false);

      // Fetch standings for each season
      for (const season of sortedSeasons) {
        const standings = await fetchStandingsForSeason(season.id);
        setSeasonsData(prev => prev.map(item => 
          item.season.id === season.id 
            ? { ...item, standings, standingsLoading: false }
            : item
        ));
      }
    } catch (err) {
      console.error('Failed to fetch seasons:', err);
      setError(t('floorballPage.error'));
      setIsLoading(false);
    }
  }, [t, fetchStandingsForSeason]);

  useEffect(() => {
    fetchSeasons();
  }, [fetchSeasons]);

  const renderStandingsTable = (data: SeasonWithStandings) => {
    const { standings, standingsLoading, season } = data;

    if (standingsLoading) {
      return (
        <div className="standings-table standings-table--loading">
          <div className="standings-table__header">
            <span className="standings-table__header-title">
              {t('floorballPage.standingsTitle')} {season.name}
            </span>
          </div>
          <div className="standings-table__loading">
            {t('floorballPage.loadingStandings')}
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
            {t('floorballPage.standingsTitle')} {season.name}
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
              <tr key={team.teamId}>
                <td className="standings-table__rank">{index + 1}</td>
                <td className="standings-table__team">{team.teamName}</td>
                <td className="standings-table__games">{team.gamesPlayed}</td>
                <td className="standings-table__points">{team.points}</td>
              </tr>
            ))}
          </tbody>
        </table>
        <Link 
          to={`/league/${season.id}?tab=statistics`}
          className="standings-table__full-link"
        >
          {t('floorballPage.viewFullTable')}
        </Link>
      </div>
    );
  };

  const renderSeasonCard = (data: SeasonWithStandings) => {
    const { season } = data;

    return (
      <div key={season.id} className="season-card">
        <div className="season-card__header">
          <h2 className="season-card__title">{season.name}</h2>
          {season.isActive && (
            <span className="season-card__badge season-card__badge--active">
              {t('floorballPage.active')}
            </span>
          )}
          {season.isCompleted && (
            <span className="season-card__badge season-card__badge--completed">
              {t('floorballPage.completed')}
            </span>
          )}
        </div>

        <nav className="season-card__links">
          <Link to={`/league/${season.id}?tab=fixtures`} className="season-card__link">
            {t('floorballPage.fixtures')}
          </Link>
          <Link to={`/league/${season.id}?tab=statistics`} className="season-card__link">
            {t('floorballPage.standings')}
          </Link>
          <Link to={`/league/${season.id}?tab=summary`} className="season-card__link">
            {t('floorballPage.playerStats')}
          </Link>
          <Link to={`/league/${season.id}?tab=summary`} className="season-card__link">
            {t('floorballPage.goalieStats')}
          </Link>
          <Link to={`/league/${season.id}?tab=summary`} className="season-card__link">
            {t('floorballPage.teamStats')}
          </Link>
        </nav>

        {renderStandingsTable(data)}
      </div>
    );
  };

  if (isLoading) {
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

  if (error) {
    return (
      <PageTemplate title={t('sports.floorball')}>
        <div className="floorball-page">
          <div className="floorball-page__error">
            <p>{error}</p>
            <button onClick={fetchSeasons} className="floorball-page__retry-btn">
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
        <div className="floorball-page__header">
          <h1 className="floorball-page__title">{t('sports.floorball')}</h1>
          <p className="floorball-page__description">
            {t('floorballPage.description')}
          </p>
        </div>

        {seasonsData.length === 0 ? (
          <div className="floorball-page__empty">
            <p>{t('floorballPage.noSeasons')}</p>
          </div>
        ) : (
          <div className="floorball-page__seasons">
            {seasonsData.map(data => renderSeasonCard(data))}
          </div>
        )}
      </div>
    </PageTemplate>
  );
}

export default FloorballPage;
