import './Statistics.scss';
import type { FloorballTeamSeasonStatisticsDto } from '../../../api/floorball/floorballStatistics';

interface StatisticsProps {
  teamStatistics?: FloorballTeamSeasonStatisticsDto | null;
  loading?: boolean;
  error?: string | null;
  seasonName?: string;
}

export default function Statistics({ teamStatistics, loading, error, seasonName }: StatisticsProps) {
  // Show loading state
  if (loading) {
    return (
      <div className="statistics-container">
        <div className="loading-state">
          <h3>Loading team statistics...</h3>
        </div>
      </div>
    );
  }

  // Show error state
  if (error) {
    return (
      <div className="statistics-container">
        <div className="error-state">
          <h3>Error loading team statistics</h3>
          <p>{error}</p>
        </div>
      </div>
    );
  }

  // Use real data or fallback to null
  const data = teamStatistics;

  // If no data available, show message
  if (!data) {
    return (
      <div className="statistics-container">
        <div className="no-data-state">
          <h3>No statistics available</h3>
          <p>Team statistics are not available for this season.</p>
        </div>
      </div>
    );
  }

  const StatCard = ({ title, value, subtitle, className = "" }: { 
    title: string; 
    value: string | number; 
    subtitle?: string; 
    className?: string; 
  }) => (
    <div className={`stat-card ${className}`}>
      <div className="stat-title">{title}</div>
      <div className="stat-value">{value}</div>
      {subtitle && <div className="stat-subtitle">{subtitle}</div>}
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
      <div className="statistics-header text-center">
        <h2>Team Statistics</h2>
        <p className="season-info text-muted">
          {seasonName || data?.seasonName || 'Current Season'}
        </p>
      </div>

      {/* Overall Record */}
      <div className="statistics-section">
        <h3 className="section-title">Overall Record</h3>
        <div className="stat-cards-grid">
          <StatCard 
            title="Games Played" 
            value={data.gamesPlayed} 
            className="primary"
          />
          <StatCard 
            title="Wins" 
            value={data.wins} 
            subtitle={`${((data.wins / data.gamesPlayed) * 100).toFixed(1)}%`}
            className="success"
          />
          <StatCard 
            title="Losses" 
            value={data.losses} 
            subtitle={`${((data.losses / data.gamesPlayed) * 100).toFixed(1)}%`}
            className="danger"
          />
          <StatCard 
            title="Ties" 
            value={data.ties} 
            subtitle={`${((data.ties / data.gamesPlayed) * 100).toFixed(1)}%`}
            className="warning"
          />
          <StatCard 
            title="Points" 
            value={data.points} 
            subtitle={`${(data.points / data.gamesPlayed).toFixed(1)} PPG`}
            className="highlight"
          />
        </div>
      </div>

      {/* Goals */}
      <div className="statistics-section">
        <h3 className="section-title">Goals</h3>
        <div className="stat-cards-grid">
          <StatCard 
            title="Goals For" 
            value={data.goalsFor} 
            subtitle={`${(data.goalsFor / data.gamesPlayed).toFixed(1)} per game`}
            className="success"
          />
          <StatCard 
            title="Goals Against" 
            value={data.goalsAgainst} 
            subtitle={`${(data.goalsAgainst / data.gamesPlayed).toFixed(1)} per game`}
            className="danger"
          />
          <StatCard 
            title="Goal Difference" 
            value={data.goalDifference > 0 ? `+${data.goalDifference}` : data.goalDifference} 
            subtitle={`${(data.goalDifference / data.gamesPlayed).toFixed(1)} per game`}
            className={data.goalDifference > 0 ? "success" : "danger"}
          />
        </div>
      </div>

      {/* Shooting */}
      <div className="statistics-section">
        <h3 className="section-title">Shooting</h3>
        <div className="stat-cards-grid">
          <StatCard 
            title="Shots For" 
            value={data.shotsFor} 
            subtitle={`${(data.shotsFor / data.gamesPlayed).toFixed(1)} per game`}
            className="primary"
          />
          <StatCard 
            title="Shots Against" 
            value={data.shotsAgainst} 
            subtitle={`${(data.shotsAgainst / data.gamesPlayed).toFixed(1)} per game`}
            className="primary"
          />
          <StatCard 
            title="Shot Percentage" 
            value={`${data.shotPercentage}%`} 
            className="highlight"
          />
        </div>
      </div>

      {/* Special Teams */}
      <div className="statistics-section">
        <h3 className="section-title">Special Teams</h3>
        <div className="stat-cards-grid">
          <StatCard 
            title="Power Play" 
            value={`${data.powerPlayPercentage}%`} 
            subtitle={`${data.powerPlayGoals}/${data.powerPlayOpportunities}`}
            className="success"
          />
          <StatCard 
            title="Penalty Kill" 
            value={`${data.penaltyKillPercentage}%`} 
            subtitle={`${data.penaltyKillOpportunities - Math.round(data.penaltyKillOpportunities * data.penaltyKillPercentage / 100)}/${data.penaltyKillOpportunities}`}
            className="success"
          />
          <StatCard 
            title="Short Handed Goals" 
            value={data.shortHandedGoals} 
            className="highlight"
          />
        </div>
      </div>

      {/* Faceoffs */}
      <div className="statistics-section">
        <h3 className="section-title">Faceoffs</h3>
        <div className="stat-cards-grid">
          <StatCard 
            title="Faceoff Percentage" 
            value={`${data.faceoffPercentage}%`} 
            subtitle={`${data.faceoffWins}/${data.faceoffAttempts}`}
            className="primary"
          />
        </div>
      </div>

      {/* Penalties */}
      <div className="statistics-section">
        <h3 className="section-title">Penalties</h3>
        <div className="stat-cards-grid">
          <StatCard 
            title="Penalty Minutes" 
            value={data.penaltyMinutes} 
            subtitle={`${(data.penaltyMinutes / data.gamesPlayed).toFixed(1)} per game`}
            className="warning"
          />
        </div>
      </div>

      {/* Home/Away Split */}
      <div className="statistics-section">
        <h3 className="section-title">Home vs Away</h3>
        <div className="split-stats-container">
          <div className="split-stat">
            <h4>Home</h4>
            <div className="split-stat-content">
              <StatRow label="Wins" value={data.homeWins} />
              <StatRow label="Losses" value={data.homeLosses} />
              <StatRow 
                label="Win %" 
                value={`${((data.homeWins / (data.homeWins + data.homeLosses)) * 100).toFixed(1)}%`} 
              />
            </div>
          </div>
          <div className="split-stat">
            <h4>Away</h4>
            <div className="split-stat-content">
              <StatRow label="Wins" value={data.awayWins} />
              <StatRow label="Losses" value={data.awayLosses} />
              <StatRow 
                label="Win %" 
                value={`${((data.awayWins / (data.awayWins + data.awayLosses)) * 100).toFixed(1)}%`} 
              />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
