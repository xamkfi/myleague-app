import { useEffect, useState, useCallback, useRef } from 'react';
import { floorballStatisticsService, type FloorballMatchTeamStatisticsDto } from '../../../api/floorball/floorballStatistics';
import type { FloorballMatchDto } from '../../../types/floorball/floorballTypes';
import { useTranslation } from 'react-i18next';
import './MatchStats.scss';
import StatRow from './StatRow';
import { getTeamInitials } from './matchUtils';

interface MatchStatsProps {
  match: FloorballMatchDto;
}

export default function MatchStats({ match }: MatchStatsProps) {
  const { t } = useTranslation();
  const [isLoading, setIsLoading] = useState(true);
  const [stats, setStats] = useState<FloorballMatchTeamStatisticsDto[]>([]);
  const isInitialLoadRef = useRef(true);

  // Helper function to load stats
  const loadStats = useCallback(async (showLoading = false) => {
    if (showLoading) {
      setIsLoading(true);
    }
    try {
      const data = await floorballStatisticsService.getMatchStatistics(match.id);
      setStats(data);
    } catch (err) {
      console.error('Error loading match statistics:', err);
      // On error, keep existing stats or show empty stats
      setStats([]);
    } finally {
      setIsLoading(false);
      isInitialLoadRef.current = false;
    }
  }, [match.id]);

  // Reset initial load flag when match ID changes
  useEffect(() => {
    isInitialLoadRef.current = true;
  }, [match.id]);

  // Initial stats load and reload when match events change (saves, goals, penalties)
  useEffect(() => {
    const isInitialLoad = isInitialLoadRef.current;
    loadStats(isInitialLoad);
  }, [
    loadStats, 
    match.id,
    match.saveEvents?.length,
    match.goalEvents?.length,
    match.penaltyEvents?.length
  ]);

  if (isLoading) {
    return (
      <div className="match-stats-loading">
        <div className="spinner"></div>
        <p>{t('matchPage.stats.loading')}</p>
        <small>{t('matchPage.stats.realTimeEnabled')}</small>
      </div>
    );
  }

  // Create empty stats if not available. Placeholder slots (null IDs) get an empty teamId so the
  // find() above degenerates to the fallback record, which is fine because there cannot be any
  // recorded stats for an unassigned slot anyway.
  const homeStats = stats.find(s => s.teamId === match.homeTeamId) || {
    teamId: match.homeTeamId ?? '',
    teamName: match.homeTeamName ?? 'TBD',
    shotsTotal: 0,
    shotsOnGoal: 0,
    penaltyMinutes: 0
  };

  const awayStats = stats.find(s => s.teamId === match.awayTeamId) || {
    teamId: match.awayTeamId ?? '',
    teamName: match.awayTeamName ?? 'TBD',
    shotsTotal: 0,
    shotsOnGoal: 0,
    penaltyMinutes: 0
  };

  // Calculate saves: opposing team's shotsTotal - shotsOnGoal
  // Home team saves = shots that away team took but didn't score
  const homeSaves = awayStats.shotsTotal - awayStats.shotsOnGoal;
  // Away team saves = shots that home team took but didn't score
  const awaySaves = homeStats.shotsTotal - homeStats.shotsOnGoal;

  

  const homeInitials: string = getTeamInitials(homeStats.teamName);
  const awayInitials: string = getTeamInitials(awayStats.teamName);

  return (
    <div className="match-stats">
      <div className="stats-header">
        <div className="team-identity home">
          <div className="team-crest home-team" title={homeStats.teamName}>
            {match.homeTeamLogo ? (
              <img
                src={match.homeTeamLogo}
                alt={`${homeStats.teamName} logo`}
                loading="lazy"
                onError={(e) => {
                  const target = e.target as HTMLImageElement;
                  target.style.display = 'none';
                }}
              />
            ) : (
              <span className="team-initials">{homeInitials}</span>
            )}
          </div>
          <span className="team-name">{homeStats.teamName}</span>
        </div>
        <div className="header-label">{t('matchPage.stats.title')}</div>
        <div className="team-identity away">
          <span className="team-name">{awayStats.teamName}</span>
          <div className="team-crest away-team" title={awayStats.teamName}>
            {match.awayTeamLogo ? (
              <img
                src={match.awayTeamLogo}
                alt={`${awayStats.teamName} logo`}
                loading="lazy"
                onError={(e) => {
                  const target = e.target as HTMLImageElement;
                  target.style.display = 'none';
                }}
              />
            ) : (
              <span className="team-initials">{awayInitials}</span>
            )}
          </div>
        </div>
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
          home={homeSaves}
          away={awaySaves}
          homeValue={homeSaves}
          awayValue={awaySaves}
          total={homeSaves + awaySaves}
        />
        <div className="stat-row stat-percentage-only">
          <div className="stat-values">
            <div className="home-value">{((homeSaves / (awayStats.shotsTotal || 1)) * 100).toFixed(1)}%</div>
            <div className="stat-label">{t('matchPage.stats.savePercentage')}</div>
            <div className="away-value">{((awaySaves / (homeStats.shotsTotal || 1)) * 100).toFixed(1)}%</div>
          </div>
        </div>
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
