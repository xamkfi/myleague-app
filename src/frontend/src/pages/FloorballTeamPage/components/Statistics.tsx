import './Statistics.scss';
import { useTranslation } from 'react-i18next';
import LoadingSpinner from '../../../components/LoadingSpinner/LoadingSpinner';
import type { FloorballTeamSeasonStatisticsDto } from '../../../api/floorball/floorballStatistics';

interface StatisticsProps {
  teamStatistics?: FloorballTeamSeasonStatisticsDto | null;
  loading?: boolean;
  error?: string | null;
  seasonName?: string;
}

export default function Statistics({ teamStatistics, loading, error, seasonName }: StatisticsProps) {
  const { t } = useTranslation();

  // Show loading state
  if (loading) {
    return (
      <div className="statistics-container">
        <div className="loading-state">
          <LoadingSpinner size="lg" text={t('teamUserPage.stats.loading')} />
        </div>
      </div>
    );
  }

  // Show error state
  if (error) {
    return (
      <div className="statistics-container">
        <div className="error-state">
          <h3>{t('teamUserPage.stats.error')}</h3>
          <p>{error}</p>
        </div>
      </div>
    );
  }

  // If no data available, show message
  if (!teamStatistics) {
    return (
      <div className="statistics-container">
        <div className="no-data-state">
          <h3>{t('teamUserPage.stats.noStats')}</h3>
          <p>{t('teamUserPage.stats.noStatsDesc')}</p>
        </div>
      </div>
    );
  }

  const data = teamStatistics;
  const hasGames = data.gamesPlayed > 0;

  // Helper function to calculate percentage safely
  const calculatePercentage = (value: number, total: number): number => {
    if (total === 0) return 0;
    return (value / total) * 100;
  };

  // Helper function to calculate per game average safely
  const calculatePerGame = (value: number): number => {
    if (!hasGames) return 0;
    return value / data.gamesPlayed;
  };

  const StatCard = ({ 
    title, 
    value, 
    subtitle, 
    icon,
    className = "",
    trend
  }: { 
    title: string; 
    value: string | number; 
    subtitle?: string; 
    icon?: string;
    className?: string;
    trend?: 'up' | 'down' | 'neutral';
  }) => (
    <div className={`stat-card ${className} ${trend ? `trend-${trend}` : ''}`}>
      {icon && <div className="stat-icon">{icon}</div>}
      <div className="stat-content">
        <div className="stat-title">{title}</div>
        <div className="stat-value">{value}</div>
        {subtitle && <div className="stat-subtitle">{subtitle}</div>}
      </div>
    </div>
  );

  const StatRow = ({ label, value, className = "" }: { 
    label: string; 
    value: string | number; 
    className?: string; 
  }) => (
    <div className={`stat-row ${className}`}>
      <span className="stat-label">{label}</span>
      <span className="stat-value">{value}</span>
    </div>
  );

  return (
    <div className="statistics-container">
      <div className="statistics-header">
        <div className="header-content">
          {data.teamLogo && (
            <img src={data.teamLogo} alt={data.teamName} className="team-logo" />
          )}
          <div className="header-text">
            <h2>{data.teamName}</h2>
            <p className="season-info">
              {seasonName || data.seasonName || t('teamUserPage.stats.currentSeason')}
            </p>
          </div>
        </div>
      </div>

      {/* Overall Record */}
      <div className="statistics-section">
        <h3 className="section-title">{t('teamUserPage.stats.overallRecord')}</h3>
        <div className="stat-cards-grid">
          <StatCard 
            title={t('teamUserPage.stats.gamesPlayed')}
            value={data.gamesPlayed} 
            icon="📊"
            className="primary"
          />
          <StatCard 
            title={t('teamUserPage.stats.wins')}
            value={data.wins} 
            subtitle={hasGames ? `${calculatePercentage(data.wins, data.gamesPlayed).toFixed(1)}%` : '0%'}
            icon="✅"
            className="success"
            trend="up"
          />
          <StatCard 
            title={t('teamUserPage.stats.losses')}
            value={data.losses} 
            subtitle={hasGames ? `${calculatePercentage(data.losses, data.gamesPlayed).toFixed(1)}%` : '0%'}
            icon="❌"
            className="danger"
            trend="down"
          />
          <StatCard 
            title={t('teamUserPage.stats.ties')}
            value={data.ties} 
            subtitle={hasGames ? `${calculatePercentage(data.ties, data.gamesPlayed).toFixed(1)}%` : '0%'}
            icon="⚖️"
            className="warning"
            trend="neutral"
          />
          <StatCard 
            title={t('teamUserPage.stats.points')}
            value={data.points} 
            subtitle={hasGames ? `${calculatePerGame(data.points).toFixed(1)} PPG` : '0.0 PPG'}
            icon="💯"
            className="highlight"
            trend="up"
          />
        </div>
      </div>

      {/* Goals */}
      <div className="statistics-section">
        <h3 className="section-title">{t('teamUserPage.stats.goals')}</h3>
        <div className="stat-cards-grid">
          <StatCard 
            title={t('teamUserPage.stats.goalsFor')}
            value={data.goalsFor} 
            subtitle={hasGames ? `${calculatePerGame(data.goalsFor).toFixed(1)} ${t('teamUserPage.stats.perGame')}` : `0.0 ${t('teamUserPage.stats.perGame')}`}
            icon="🎯"
            className="success"
            trend="up"
          />
          <StatCard 
            title={t('teamUserPage.stats.goalsAgainst')}
            value={data.goalsAgainst} 
            subtitle={hasGames ? `${calculatePerGame(data.goalsAgainst).toFixed(1)} ${t('teamUserPage.stats.perGame')}` : `0.0 ${t('teamUserPage.stats.perGame')}`}
            icon="🛡️"
            className="danger"
            trend="down"
          />
          <StatCard 
            title={t('teamUserPage.stats.goalDifference')}
            value={data.goalDifference > 0 ? `+${data.goalDifference}` : data.goalDifference} 
            subtitle={hasGames ? `${calculatePerGame(data.goalDifference).toFixed(1)} ${t('teamUserPage.stats.perGame')}` : `0.0 ${t('teamUserPage.stats.perGame')}`}
            icon={data.goalDifference > 0 ? "📈" : "📉"}
            className={data.goalDifference > 0 ? "success" : "danger"}
            trend={data.goalDifference > 0 ? "up" : "down"}
          />
        </div>
      </div>

      {/* Home/Away Split */}
      <div className="statistics-section">
        <h3 className="section-title">{t('teamUserPage.stats.homeVsAway')}</h3>
        <div className="split-stats-container">
          <div className="split-stat home-stat">
            <div className="split-stat-header">
              <h4>{t('teamUserPage.stats.home')}</h4>
              <span className="home-icon">🏠</span>
            </div>
            <div className="split-stat-content">
              <StatRow label={t('teamUserPage.stats.wins')} value={data.homeWins} />
              <StatRow label={t('teamUserPage.stats.losses')} value={data.homeLosses} />
              {data.homeWins + data.homeLosses > 0 && (
                <StatRow 
                  label={t('teamUserPage.stats.winPercentage')}
                  value={`${calculatePercentage(data.homeWins, data.homeWins + data.homeLosses).toFixed(1)}%`} 
                />
              )}
            </div>
          </div>
          <div className="split-stat away-stat">
            <div className="split-stat-header">
              <h4>{t('teamUserPage.stats.away')}</h4>
              <span className="away-icon">✈️</span>
            </div>
            <div className="split-stat-content">
              <StatRow label={t('teamUserPage.stats.wins')} value={data.awayWins} />
              <StatRow label={t('teamUserPage.stats.losses')} value={data.awayLosses} />
              {data.awayWins + data.awayLosses > 0 && (
                <StatRow 
                  label={t('teamUserPage.stats.winPercentage')}
                  value={`${calculatePercentage(data.awayWins, data.awayWins + data.awayLosses).toFixed(1)}%`} 
                />
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

