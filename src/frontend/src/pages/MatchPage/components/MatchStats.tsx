import { useEffect, useState } from 'react';
import { floorballStatisticsService, type FloorballMatchTeamStatisticsDto } from '../../../api/floorball/floorballStatistics';
import type { FloorballMatchDto } from '../../../types/floorball/floorballTypes';
import './MatchStats.scss';

interface MatchStatsProps {
  match: FloorballMatchDto;
}

export default function MatchStats({ match }: MatchStatsProps) {
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [stats, setStats] = useState<FloorballMatchTeamStatisticsDto[]>([]);

  useEffect(() => {
    const fetchStats = async () => {
      try {
        setIsLoading(true);
        setError(null);
        const data = await floorballStatisticsService.getMatchStatistics(match.id);
        setStats(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load match statistics');
      } finally {
        setIsLoading(false);
      }
    };

    fetchStats();
  }, [match.id]);

  if (isLoading) {
    return (
      <div className="match-stats-loading">
        <div className="spinner"></div>
        <p>Loading match statistics...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="match-stats-error">
        <p>Error: {error}</p>
        <button onClick={() => setIsLoading(true)}>Retry</button>
      </div>
    );
  }

  const homeStats = stats.find(s => s.teamId === match.homeTeamId);
  const awayStats = stats.find(s => s.teamId === match.awayTeamId);

  if (!homeStats || !awayStats) {
    return (
      <div className="match-stats-error">
        <p>Statistics not available for this match</p>
      </div>
    );
  }

  const StatRow = ({ 
    label, 
    home, 
    away, 
    homeValue, 
    awayValue,
    total
  }: { 
    label: string; 
    home: number | string; 
    away: number | string;
    homeValue: number;
    awayValue: number;
    total: number;
  }) => {
    const homeWidth = (homeValue / total) * 100;
    const awayWidth = (awayValue / total) * 100;
    
    return (
      <div className="stat-row">
        <div className="stat-values">
          <div className="home-value">{home}</div>
          <div className="stat-label">{label}</div>
          <div className="away-value">{away}</div>
        </div>
        <div className="stat-bars">
          <div className="bar-container">
            <div 
              className="home-bar" 
              style={{ width: `${homeWidth}%` }}
            />
            <div 
              className="away-bar" 
              style={{ width: `${awayWidth}%` }}
            />
          </div>
          {homeValue > 0 && awayValue > 0 && (
            <div className="percentage-values">
              <span className="home-percentage">{(homeValue / total * 100).toFixed(1)}%</span>
              <span className="away-percentage">{(awayValue / total * 100).toFixed(1)}%</span>
            </div>
          )}
        </div>
      </div>
    );
  };

  return (
    <div className="match-stats">
      <div className="stats-header">
        <div className="team-name home">{homeStats.teamName}</div>
        <div className="header-label">Team Statistics</div>
        <div className="team-name away">{awayStats.teamName}</div>
      </div>

      <div className="stats-content">
        <StatRow 
          label="Total Shots" 
          home={homeStats.shotsTotal}
          away={awayStats.shotsTotal}
          homeValue={homeStats.shotsTotal}
          awayValue={awayStats.shotsTotal}
          total={homeStats.shotsTotal + awayStats.shotsTotal}
        />
        <StatRow 
          label="Shots on Goal" 
          home={`${homeStats.shotsOnGoal}/${homeStats.shotsTotal}`}
          away={`${awayStats.shotsOnGoal}/${awayStats.shotsTotal}`}
          homeValue={homeStats.shotsOnGoal}
          awayValue={awayStats.shotsOnGoal}
          total={homeStats.shotsTotal + awayStats.shotsTotal}
        />
        <StatRow 
          label="Shot Accuracy" 
          home={`${homeStats.shotPercentage.toFixed(1)}%`}
          away={`${awayStats.shotPercentage.toFixed(1)}%`}
          homeValue={homeStats.shotPercentage}
          awayValue={awayStats.shotPercentage}
          total={200} // Using 200 as total for percentage comparison (100% + 100%)
        />
        <StatRow 
          label="Penalty Minutes" 
          home={homeStats.penaltyMinutes}
          away={awayStats.penaltyMinutes}
          homeValue={homeStats.penaltyMinutes}
          awayValue={awayStats.penaltyMinutes}
          total={homeStats.penaltyMinutes + awayStats.penaltyMinutes}
        />
      </div>
    </div>
  );
}
