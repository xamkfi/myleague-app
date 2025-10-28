import { useEffect, useState, useCallback } from 'react';
import { floorballStatisticsService, type FloorballMatchTeamStatisticsDto } from '../../../api/floorball/floorballStatistics';
import type { FloorballMatchDto } from '../../../types/floorball/floorballTypes';
import { signalRService, type MatchEvent } from '../../../services/signalRService';
import { useTranslation } from 'react-i18next';
import './MatchStats.scss';
import StatRow from './StatRow';

// SignalR event names that should trigger stats refresh
const STATS_UPDATE_EVENTS = [
  'FloorballGoalScored',
  'FloorballPenaltyAssigned',
  'FloorballSaveRecorded',
  'FloorballMatchStarted',
  'FloorballMatchCompleted',
  'FloorballMatchStatsUpdated'  // In case there's a direct stats update event
];

interface MatchStatsProps {
  match: FloorballMatchDto;
}

export default function MatchStats({ match }: MatchStatsProps) {
  const { t } = useTranslation();
  const [isLoading, setIsLoading] = useState(true);
  const [stats, setStats] = useState<FloorballMatchTeamStatisticsDto[]>([]);

  // Helper function to load stats
  const loadStats = useCallback(async () => {
    try {
      const data = await floorballStatisticsService.getMatchStatistics(match.id);
      setStats(data);
      console.log('Updated match statistics');
    } catch (err) {
      console.error('Error loading match statistics:', err);
      // On error, keep existing stats or show empty stats
      setStats([]);
    } finally {
      setIsLoading(false);
    }
  }, [match.id]);

  // Initial stats load
  useEffect(() => {
    setIsLoading(true);
    loadStats();
  }, [loadStats]);

  // Setup SignalR subscription for stats updates
  useEffect(() => {
    const unsubscribe = signalRService.onMatchEvent((evt: MatchEvent) => {
      if (STATS_UPDATE_EVENTS.includes(evt.eventType)) {
        console.log(`Updating stats due to ${evt.eventType} event`);
        loadStats();
      }
    });

    return () => {
      unsubscribe();
    };
  }, [loadStats]);

  if (isLoading) {
    return (
      <div className="match-stats-loading">
        <div className="spinner"></div>
        <p>{t('matchPage.stats.loading')}</p>
        <small>{t('matchPage.stats.realTimeEnabled')}</small>
      </div>
    );
  }

  // Create empty stats if not available
  const homeStats = stats.find(s => s.teamId === match.homeTeamId) || {
    teamId: match.homeTeamId,
    teamName: match.homeTeamName,
    shotsTotal: 0,
    penaltyMinutes: 0
  };
  
  const awayStats = stats.find(s => s.teamId === match.awayTeamId) || {
    teamId: match.awayTeamId,
    teamName: match.awayTeamName,
    shotsTotal: 0,
    penaltyMinutes: 0
  };

  

  return (
    <div className="match-stats">
      <div className="stats-header">
        <div className="team-name home">{homeStats.teamName}</div>
        <div className="header-label">{t('matchPage.stats.title')}</div>
        <div className="team-name away">{awayStats.teamName}</div>
      </div>

      <div className="stats-content">
        <StatRow 
          label={t('matchPage.stats.shotsOnTarget')}
          home={homeStats.shotsTotal}
          away={awayStats.shotsTotal}
          homeValue={homeStats.shotsTotal}
          awayValue={awayStats.shotsTotal}
          total={homeStats.shotsTotal + awayStats.shotsTotal}
        />
        <StatRow 
          label={t('matchPage.stats.goalieSaves')}
          home={homeStats.shotsTotal - (match.awayScore || 0)}
          away={awayStats.shotsTotal - (match.homeScore || 0)}
          homeValue={homeStats.shotsTotal - (match.awayScore || 0)}
          awayValue={awayStats.shotsTotal - (match.homeScore || 0)}
          total={(homeStats.shotsTotal - (match.awayScore || 0)) + (awayStats.shotsTotal - (match.homeScore || 0))}
        />
        <StatRow 
          label={t('matchPage.stats.savePercentage')}
          home={`${(((homeStats.shotsTotal - (match.awayScore || 0)) / (homeStats.shotsTotal || 1)) * 100).toFixed(1)}%`}
          away={`${(((awayStats.shotsTotal - (match.homeScore || 0)) / (awayStats.shotsTotal || 1)) * 100).toFixed(1)}%`}
          homeValue={((homeStats.shotsTotal - (match.awayScore || 0)) / (homeStats.shotsTotal || 1)) * 100}
          awayValue={((awayStats.shotsTotal - (match.homeScore || 0)) / (awayStats.shotsTotal || 1)) * 100}
          total={200} // Using 200 as total for percentage comparison
        />
        <StatRow 
          label={t('matchPage.stats.penaltyMinutes')}
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
