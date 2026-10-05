import type { MouseEvent, ReactElement } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { getTeamInitials } from './matchHeaderUtils';

export interface TimelinePlayer {
  name: string;
  jerseyNumber?: number;
  href?: string;
}

export interface TimelineGoalBadge {
  abbreviation: string;
  label: string;
}

export interface TimelineEvent {
  key: string;
  kind: 'goal' | 'penalty';
  periodNumber: number;
  timeInSeconds: number;
  side: 'home' | 'away';
  player?: TimelinePlayer;
  assists?: TimelinePlayer[];
  goalBadge?: TimelineGoalBadge | null;
  penaltyLabel?: string;
  description?: string;
}

export interface TimelineTeam {
  name: string;
  logo: string | null;
}

interface MatchEventTimelineProps {
  events: TimelineEvent[];
  home: TimelineTeam;
  away: TimelineTeam;
  periodTitle: (periodNumber: number) => string;
  eventClock: (periodNumber: number, timeInSeconds: number) => string;
  emptyMessage?: string;
}

function compareEvents(left: TimelineEvent, right: TimelineEvent): number {
  if (left.periodNumber !== right.periodNumber) return left.periodNumber - right.periodNumber;
  return left.timeInSeconds - right.timeInSeconds;
}

function buildScoreByGoalKey(events: TimelineEvent[]): Map<string, string> {
  const scores: Map<string, string> = new Map<string, string>();
  let homeScore: number = 0;
  let awayScore: number = 0;
  for (const event of events) {
    if (event.kind !== 'goal') continue;
    if (event.side === 'home') homeScore++;
    else awayScore++;
    scores.set(event.key, `${homeScore} - ${awayScore}`);
  }
  return scores;
}

function MatchEventTimeline({
  events,
  home,
  away,
  periodTitle,
  eventClock,
  emptyMessage,
}: MatchEventTimelineProps): ReactElement | null {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const sorted: TimelineEvent[] = [...events].sort(compareEvents);
  const scoreByGoalKey: Map<string, string> = buildScoreByGoalKey(sorted);
  const periods: number[] = [...new Set(sorted.map((event) => event.periodNumber))].sort((a, b) => a - b);

  if (sorted.length === 0) {
    return emptyMessage ? <p className="match-events-empty">{emptyMessage}</p> : null;
  }

  const handlePlayerClick = (href: string | undefined, e: MouseEvent): void => {
    if (!href) return;
    e.stopPropagation();
    navigate(href);
  };

  const renderTeamBadge = (side: 'home' | 'away'): ReactElement => {
    const team: TimelineTeam = side === 'home' ? home : away;
    const sideClass: string = side === 'home' ? 'home-team' : 'away-team';
    const initials: string = getTeamInitials(team.name);

    if (team.logo) {
      return (
        <span className={`event-team-short has-logo ${sideClass}`} title={team.name}>
          <img
            src={team.logo}
            alt={`${team.name} logo`}
            className="event-team-logo"
            loading="lazy"
            onError={(e) => {
              const target = e.target as HTMLImageElement;
              target.style.display = 'none';
              const parent = target.parentElement;
              if (parent) {
                parent.classList.remove('has-logo');
                parent.textContent = initials;
              }
            }}
          />
        </span>
      );
    }

    return (
      <span className={`event-team-short ${sideClass}`} title={team.name}>
        {initials}
      </span>
    );
  };

  const renderPlayerName = (player: TimelinePlayer | undefined): ReactElement => {
    if (!player || !player.name) {
      return <span className="event-player-name">{t('matchPage.events.unknownPlayer', 'Unknown player')}</span>;
    }
    return (
      <span
        className={`event-player-name event-player-link ${player.href ? 'clickable' : ''}`}
        onClick={(e) => handlePlayerClick(player.href, e)}
      >
        {typeof player.jerseyNumber === 'number' && (
          <span className="event-player-number" aria-label={`Number ${player.jerseyNumber}`}>
            #{player.jerseyNumber}
          </span>
        )}
        <span className="event-player-name-text">{player.name}</span>
      </span>
    );
  };

  const renderAssist = (assist: TimelinePlayer, index: number): ReactElement => (
    <span key={index}>
      {' '}
      <span
        className={`event-assist ${assist.href ? 'clickable' : ''}`}
        onClick={(e) => handlePlayerClick(assist.href, e)}
      >
        ({typeof assist.jerseyNumber === 'number' ? `#${assist.jerseyNumber} ` : ''}{assist.name})
      </span>
    </span>
  );

  const renderGoalRow = (event: TimelineEvent): ReactElement => (
    <div className={`event-row ${event.side === 'home' ? 'home-event' : 'away-event'} goal`}>
      <span className="event-time" title={periodTitle(event.periodNumber)}>
        {eventClock(event.periodNumber, event.timeInSeconds)}
      </span>
      <span className="event-type-badge goal" aria-label={t('matchPage.events.goal', 'Goal')} title={t('matchPage.events.goal', 'Goal')}>
        <span className="badge-letter" aria-hidden>G</span>
      </span>
      {event.goalBadge && event.goalBadge.abbreviation && (
        <span className="goal-type-badge" title={event.goalBadge.label} aria-label={event.goalBadge.label}>
          ({event.goalBadge.abbreviation})
        </span>
      )}
      {renderTeamBadge(event.side)}
      <span className="event-score">{scoreByGoalKey.get(event.key) ?? ''}</span>
      <span className="event-details">
        {renderPlayerName(event.player)}
        {(event.assists ?? []).filter((assist) => assist.name).map(renderAssist)}
      </span>
    </div>
  );

  const renderPenaltyRow = (event: TimelineEvent): ReactElement => {
    const description: string = (event.description ?? '').trim();
    return (
      <div className={`event-row ${event.side === 'home' ? 'home-event' : 'away-event'} penalty`}>
        <span className="event-time" title={periodTitle(event.periodNumber)}>
          {eventClock(event.periodNumber, event.timeInSeconds)}
        </span>
        <span className="event-type-badge penalty" aria-label={t('matchPage.events.penalty', 'Penalty')} title={t('matchPage.events.penalty', 'Penalty')}>
          <span className="badge-letter" aria-hidden>P</span>
        </span>
        {renderTeamBadge(event.side)}
        <span className="event-details penalty-details">
          <span className="penalty-line">
            {event.player ? renderPlayerName(event.player) : null}
            {event.penaltyLabel && <span className="penalty-type"> ({event.penaltyLabel})</span>}
          </span>
          {description && (
            <span className="penalty-description" title={description}>
              {description}
            </span>
          )}
        </span>
      </div>
    );
  };

  return (
    <div className="match-events">
      {periods.map((period) => (
        <div key={period} className="period-section">
          <div className="period-header">
            <span className="period-name">{periodTitle(period)}</span>
          </div>
          <div className="period-events">
            {sorted
              .filter((event) => event.periodNumber === period)
              .map((event) => (
                <div key={event.key}>
                  {event.kind === 'goal' ? renderGoalRow(event) : renderPenaltyRow(event)}
                </div>
              ))}
          </div>
        </div>
      ))}
    </div>
  );
}

export default MatchEventTimeline;
