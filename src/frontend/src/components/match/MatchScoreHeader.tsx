import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { formatMatchHeaderDate, getTeamInitials } from './matchHeaderUtils';
import type { MatchScoreHeaderProps, MatchScoreHeaderTeam } from './matchPageTypes';
import './MatchScoreHeader.scss';

interface TeamBlockProps {
  team: MatchScoreHeaderTeam;
  side: 'home' | 'away';
  fallbackName: string;
}

function TeamBlock({ team, side, fallbackName }: TeamBlockProps) {
  const displayName = team.name ?? fallbackName;
  const isClickable = Boolean(team.name && team.href);
  const className = `team-section ${side}${isClickable ? ' clickable' : ''}`;

  const body = (
    <>
      <div className="team-crest">
        {getTeamInitials(displayName)}
        {team.logo && (
          <img
            src={team.logo}
            alt={`${displayName} logo`}
            className="team-logo"
            loading="lazy"
            onError={(event) => {
              const target = event.target as HTMLImageElement;
              target.style.display = 'none';
            }}
          />
        )}
      </div>
      <div className="team-name">{displayName}</div>
    </>
  );

  if (isClickable && team.href) {
    return (
      <Link className={className} to={team.href}>
        {body}
      </Link>
    );
  }

  return <div className={className}>{body}</div>;
}

export default function MatchScoreHeader({
  home,
  away,
  homeScore,
  awayScore,
  scheduledDateTime,
  isScheduled,
  isLive,
  isFinal,
}: MatchScoreHeaderProps) {
  const { t } = useTranslation();
  const scheduled = formatMatchHeaderDate(scheduledDateTime);
  const tbd = t('matchPage.tbd');

  return (
    <div className="match-header">
      <div className="teams-container">
        <TeamBlock team={home} side="home" fallbackName={tbd} />

        <div className="score-container">
          {isScheduled ? (
            <div className="vs-separator">VS</div>
          ) : (
            <div className="match-score">
              <span className="home-score">{homeScore}</span>
              <span className="score-separator">—</span>
              <span className="away-score">{awayScore}</span>
            </div>
          )}
        </div>

        <TeamBlock team={away} side="away" fallbackName={tbd} />
      </div>

      <div className="match-date-time">
        <span className="weekday">{scheduled.weekday}</span>
        <span className="separator">·</span>
        <span className="date">{scheduled.date}</span>
        <span className="separator">·</span>
        <span className="time">{scheduled.time}</span>
      </div>

      {isLive && (
        <div className="match-status live">
          <span className="status-dot" aria-label={t('matchPage.live')} />
          <span>{t('matchPage.live')}</span>
        </div>
      )}

      {isFinal && (
        <div className="match-status final">
          <span>{t('matchPage.final')}</span>
        </div>
      )}
    </div>
  );
}
