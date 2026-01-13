import './SummarySection.scss';
import { useTranslation } from 'react-i18next';
import type { FloorballSeasonStatisticsSummaryDto } from '../../../api/floorball/floorballStatistics';

interface SummarySectionProps {
  seasonSummary: FloorballSeasonStatisticsSummaryDto | null;
  loading: boolean;
  error: string | null;
}

export default function SummarySection({ seasonSummary, loading, error }: SummarySectionProps) {
  const { t } = useTranslation();

  if (loading) {
    return (
      <div className="summary-section">
        <div className="loading">{t('leaguePage.summary.loading')}</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="summary-section">
        <div className="error">{t('leaguePage.summary.error', { error })}</div>
      </div>
    );
  }

  if (!seasonSummary) {
    return (
      <div className="summary-section">
        <div className="no-data">{t('leaguePage.summary.noData')}</div>
      </div>
    );
  }

  const teamCount = seasonSummary.teamStandings?.length || 0;

  return (
    <div className="summary-section">
      <h2>{t('leaguePage.summary.title')}</h2>
      <div className="summary-content">
        <div className="league-overview">
          <div className="overview-card">
            <h3>{t('leaguePage.summary.leagueInformation')}</h3>
            <div className="info-grid">
              {seasonSummary.seasonName && (
                <div className="info-item">
                  <span className="label">{t('leaguePage.summary.season')}</span>
                  <span className="value">{seasonSummary.seasonName}</span>
                </div>
              )}
              {teamCount > 0 && (
                <div className="info-item">
                  <span className="label">{t('leaguePage.summary.teams')}</span>
                  <span className="value">{teamCount}</span>
                </div>
              )}
              {seasonSummary.totalGames !== undefined && (
                <div className="info-item">
                  <span className="label">{t('leaguePage.summary.matchesPlayed')}</span>
                  <span className="value">{seasonSummary.totalGames}</span>
                </div>
              )}
              {seasonSummary.totalGoals !== undefined && (
                <div className="info-item">
                  <span className="label">{t('leaguePage.summary.goalsScored')}</span>
                  <span className="value">{seasonSummary.totalGoals}</span>
                </div>
              )}
              {seasonSummary.averageGoalsPerGame !== undefined && (
                <div className="info-item">
                  <span className="label">{t('leaguePage.summary.avgGoalsPerGame')}</span>
                  <span className="value">{seasonSummary.averageGoalsPerGame.toFixed(2)}</span>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
