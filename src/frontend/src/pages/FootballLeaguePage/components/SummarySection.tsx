import './SummarySection.scss';
import { useTranslation } from 'react-i18next';
import { useNavigate, useSearchParams } from 'react-router-dom';
import type { FootballSeasonStatisticsSummaryDto } from '../../../api/football/footballStatistics';
import { TeamLink } from '../../../components/SportLinks';

interface SummarySectionProps {
  seasonSummary: FootballSeasonStatisticsSummaryDto | null;
  loading: boolean;
  error: string | null;
}

const MAX_STANDINGS_PREVIEW = 8;

export default function SummarySection({ seasonSummary, loading, error }: SummarySectionProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  if (loading) {
    return (
      <div className="summary-section">
        <div className="summary-section__loading">{t('leaguePage.summary.loading')}</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="summary-section">
        <div className="summary-section__error">{t('leaguePage.summary.error', { error })}</div>
      </div>
    );
  }

  if (!seasonSummary) {
    return (
      <div className="summary-section">
        <div className="summary-section__empty">{t('leaguePage.summary.noData')}</div>
      </div>
    );
  }

  const teamCount = seasonSummary.teamStandings?.length || 0;
  const standings = seasonSummary.teamStandings?.slice(0, MAX_STANDINGS_PREVIEW) || [];

  const handleViewFullStandings = () => {
    const currentParams = new URLSearchParams(searchParams);
    currentParams.set('tab', 'statistics');
    navigate(`?${currentParams.toString()}`, { replace: true });
  };

  return (
    <div className="summary-section">
      {/* Stat cards row */}
      <div className="summary-section__stats">
        {teamCount > 0 && (
          <div className="summary-section__stat-card">
            <span className="summary-section__stat-value">{teamCount}</span>
            <span className="summary-section__stat-label">{t('leaguePage.summary.teams')}</span>
          </div>
        )}
        {seasonSummary.totalGames !== undefined && (
          <div className="summary-section__stat-card">
            <span className="summary-section__stat-value">{seasonSummary.totalGames}</span>
            <span className="summary-section__stat-label">{t('leaguePage.summary.matchesPlayed')}</span>
          </div>
        )}
        {seasonSummary.totalGoals !== undefined && (
          <div className="summary-section__stat-card">
            <span className="summary-section__stat-value">{seasonSummary.totalGoals}</span>
            <span className="summary-section__stat-label">{t('leaguePage.summary.goalsScored')}</span>
          </div>
        )}
        {seasonSummary.averageGoalsPerGame !== undefined && (
          <div className="summary-section__stat-card">
            <span className="summary-section__stat-value">{seasonSummary.averageGoalsPerGame.toFixed(1)}</span>
            <span className="summary-section__stat-label">{t('leaguePage.summary.avgGoalsPerGame')}</span>
          </div>
        )}
      </div>

      {/* Compact standings table */}
      {standings.length > 0 && (
        <div className="summary-section__standings">
          <div className="summary-section__standings-header">
            <h3 className="summary-section__standings-title">
              {t('leaguePage.summary.standingsPreview')}
            </h3>
          </div>
          <table className="summary-section__standings-table">
            <thead>
              <tr>
                <th className="summary-section__col-rank">#</th>
                <th className="summary-section__col-team">{t('footballPage.teamShort', 'Team')}</th>
                <th className="summary-section__col-gp">{t('footballPage.gamesShort', 'GP')}</th>
                <th className="summary-section__col-w">W</th>
                <th className="summary-section__col-d">D</th>
                <th className="summary-section__col-l">L</th>
                <th className="summary-section__col-pts">{t('footballPage.ptsShort', 'PTS')}</th>
              </tr>
            </thead>
            <tbody>
              {standings.map((team, index) => (
                <tr key={team.teamId}>
                  <td className="summary-section__col-rank">{index + 1}</td>
                  <td className="summary-section__col-team">
                    <div className="summary-section__team-info">
                      {team.teamLogo && team.teamLogo.trim() !== '' ? (
                        <img
                          className="summary-section__team-logo"
                          src={team.teamLogo}
                          alt={team.teamName}
                          onError={(e) => {
                            const target = e.target as HTMLImageElement;
                            target.style.display = 'none';
                          }}
                        />
                      ) : (
                        <div className="summary-section__team-logo-empty" />
                      )}
                      <TeamLink
                        sport="football"
                        teamId={team.teamId}
                        teamName={team.teamName}
                        teams={standings.map((row) => ({ id: row.teamId, name: row.teamName }))}
                        className="summary-section__team-name"
                      />
                    </div>
                  </td>
                  <td className="summary-section__col-gp">{team.gamesPlayed}</td>
                  <td className="summary-section__col-w">{team.wins}</td>
                  <td className="summary-section__col-d">{team.draws}</td>
                  <td className="summary-section__col-l">{team.losses}</td>
                  <td className="summary-section__col-pts">{team.points}</td>
                </tr>
              ))}
            </tbody>
          </table>
          <button
            type="button"
            className="summary-section__view-full"
            onClick={handleViewFullStandings}
          >
            {t('leaguePage.summary.viewFullStandings')} &rarr;
          </button>
        </div>
      )}
    </div>
  );
}
