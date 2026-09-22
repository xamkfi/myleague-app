import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import type { FloorballMatchDto } from '../../types/floorball/floorballTypes';
import { FloorballMatchStatus } from '../../types/floorball/floorballTypes';
import { resolveLogoUrl } from '../../utils/resolveLogoUrl';

interface MatchPanelCardProps {
  match: FloorballMatchDto;
}

function MatchPanelCard({ match }: MatchPanelCardProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();

  const isLive = match.status === FloorballMatchStatus.InProgress;
  const isCompleted = match.status === FloorballMatchStatus.Completed;
  const showScore = isLive || isCompleted;

  // --- Formatting helpers ---

  const formatTime = (iso: string): string => {
    const d = new Date(iso);
    return d.toLocaleTimeString('fi-FI', { hour: '2-digit', minute: '2-digit' });
  };

  const formatDate = (iso: string): string => {
    const d = new Date(iso);
    const now = new Date();
    const tomorrow = new Date(now);
    tomorrow.setDate(tomorrow.getDate() + 1);

    if (d.toDateString() === now.toDateString()) return t('sidebar.today', 'Tänään');
    if (d.toDateString() === tomorrow.toDateString()) return t('sidebar.tomorrow', 'Huomenna');

    return d.toLocaleDateString('fi-FI', { day: 'numeric', month: 'numeric' });
  };

  const dateLabel = isLive
    ? formatTime(match.scheduledDateTime)
    : `${formatDate(match.scheduledDateTime)} ${formatTime(match.scheduledDateTime)}`;

  // --- Navigation ---

  const handleClick = () => {
    navigate(`/match/${match.id}`);
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault();
      handleClick();
    }
  };

  // --- Render helpers ---

  const renderTeamLogo = (logoUrl: string | null, teamName: string) => {
    const resolved = resolveLogoUrl(logoUrl);
    if (resolved) {
      return (
        <img
          src={resolved}
          alt={teamName}
          className="match-panel-card__team-logo"
          onError={(e) => {
            (e.target as HTMLImageElement).style.display = 'none';
          }}
        />
      );
    }
    const initial = teamName.trim().charAt(0).toUpperCase();
    return (
      <span className="match-panel-card__team-logo-fallback" aria-hidden="true">
        {initial || '·'}
      </span>
    );
  };

  const renderTeamRow = (
    teamName: string,
    teamLogo: string | null,
    score: number,
  ) => (
    <div className="match-panel-card__team-row">
      <div className="match-panel-card__team-info">
        {renderTeamLogo(teamLogo, teamName)}
        <span className="match-panel-card__team-name">{teamName}</span>
      </div>
      {showScore && (
        <span className="match-panel-card__score">{score}</span>
      )}
    </div>
  );

  const statusClass = isLive
    ? 'match-panel-card match-panel-card--live'
    : isCompleted
      ? 'match-panel-card match-panel-card--completed'
      : 'match-panel-card match-panel-card--scheduled';

  return (
    <div
      className={statusClass}
      role="button"
      tabIndex={0}
      onClick={handleClick}
      onKeyDown={handleKeyDown}
    >
      <div className="match-panel-card__top">
        <span className="match-panel-card__date">{dateLabel}</span>
      </div>

      {/* Live badge */}
      {isLive && (
        <span className="match-panel-card__live-badge">
          <span className="pulse-dot pulse-dot--sm pulse-dot--white" />
          {t('sidebar.liveBadge', 'LIVE')}
        </span>
      )}

      {/* Teams. Unassigned slots fall back to "TBD" so future-scheduled fixtures still render. */}
      {renderTeamRow(match.homeTeamName ?? t('sidebar.tbd', 'TBD'), match.homeTeamLogo, match.homeScore)}
      {renderTeamRow(match.awayTeamName ?? t('sidebar.tbd', 'TBD'), match.awayTeamLogo, match.awayScore)}

      <div className="match-panel-card__footer">
        <span className="match-panel-card__season">
          {match.competitionName || t('sidebar.seasonFallback', 'Kausi')}
        </span>
        {match.venue && (
          <span className="match-panel-card__venue">{match.venue}</span>
        )}
      </div>
    </div>
  );
}

export default MatchPanelCard;
