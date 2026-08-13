import { useEffect, useState, useCallback, useRef } from 'react';
import { footballStatisticsService, type FootballMatchTeamStatisticsDto } from '../../../api/football/footballStatistics';
import type { FootballMatchDto } from '../../../types/football/footballTypes';
import { useTranslation } from 'react-i18next';
import './MatchStats.scss';
import StatRow from './StatRow';
import { getTeamInitials } from './matchUtils';

interface MatchStatsProps {
  match: FootballMatchDto;
}

interface TeamStatView {
  teamId: string;
  teamName: string;
  goals: number;
  yellowCards: number;
  redCards: number;
  substitutions: number;
  cleanSheet: boolean;
}

export default function MatchStats({ match }: MatchStatsProps) {
  const { t } = useTranslation();
  const [isLoading, setIsLoading] = useState(true);
  const [stats, setStats] = useState<FootballMatchTeamStatisticsDto[]>([]);
  const isInitialLoadRef = useRef(true);

  const loadStats = useCallback(async (showLoading = false) => {
    if (showLoading) {
      setIsLoading(true);
    }
    try {
      const data = await footballStatisticsService.getMatchStatistics(match.id);
      setStats(data);
    } catch (err) {
      console.error('Error loading match statistics:', err);
      setStats([]);
    } finally {
      setIsLoading(false);
      isInitialLoadRef.current = false;
    }
  }, [match.id]);

  useEffect(() => {
    isInitialLoadRef.current = true;
  }, [match.id]);

  useEffect(() => {
    const isInitialLoad = isInitialLoadRef.current;
    void loadStats(isInitialLoad);
  }, [
    loadStats,
    match.id,
    match.goalEvents?.length,
    match.cardEvents?.length,
    match.substitutionEvents?.length,
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

  const emptyStats = (teamId: string, teamName: string): TeamStatView => ({
    teamId,
    teamName,
    goals: 0,
    yellowCards: 0,
    redCards: 0,
    substitutions: 0,
    cleanSheet: false,
  });

  const homeStats: TeamStatView = stats.find((s) => s.teamId === match.homeTeamId) ?? emptyStats(
    match.homeTeamId ?? '',
    match.homeTeamName ?? 'TBD',
  );
  const awayStats: TeamStatView = stats.find((s) => s.teamId === match.awayTeamId) ?? emptyStats(
    match.awayTeamId ?? '',
    match.awayTeamName ?? 'TBD',
  );

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
          label={t('football.match.stats.goals', 'Goals')}
          home={homeStats.goals}
          away={awayStats.goals}
          homeValue={homeStats.goals}
          awayValue={awayStats.goals}
          total={homeStats.goals + awayStats.goals}
        />
        <StatRow
          label={t('football.match.stats.yellowCards', 'Yellow cards')}
          home={homeStats.yellowCards}
          away={awayStats.yellowCards}
          homeValue={homeStats.yellowCards}
          awayValue={awayStats.yellowCards}
          total={homeStats.yellowCards + awayStats.yellowCards}
        />
        <StatRow
          label={t('football.match.stats.redCards', 'Red cards')}
          home={homeStats.redCards}
          away={awayStats.redCards}
          homeValue={homeStats.redCards}
          awayValue={awayStats.redCards}
          total={homeStats.redCards + awayStats.redCards}
        />
        <StatRow
          label={t('football.match.stats.substitutions', 'Substitutions')}
          home={homeStats.substitutions}
          away={awayStats.substitutions}
          homeValue={homeStats.substitutions}
          awayValue={awayStats.substitutions}
          total={homeStats.substitutions + awayStats.substitutions}
        />
        <div className="stat-row stat-percentage-only">
          <div className="stat-values">
            <div className="home-value">{homeStats.cleanSheet ? t('football.match.stats.yes', 'Yes') : t('football.match.stats.no', 'No')}</div>
            <div className="stat-label">{t('football.match.stats.cleanSheet', 'Clean sheet')}</div>
            <div className="away-value">{awayStats.cleanSheet ? t('football.match.stats.yes', 'Yes') : t('football.match.stats.no', 'No')}</div>
          </div>
        </div>
      </div>
    </div>
  );
}
