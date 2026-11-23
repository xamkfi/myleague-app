import { useMemo, type MouseEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import type {
  FootballMatchDto,
  FootballGoalEventDto,
  FootballCardEventDto,
  FootballMissedPenaltyEventDto,
  FootballSubstitutionEventDto,
  FootballTeamPlayer,
} from '../../../../types/football/footballTypes';
import { FootballCardType } from '../../../../types/football/footballTypes';
import { getPeriodName, getTeamInitials } from './matchUtils';
import { formatMatchEventTime } from '../../../../utils/matchEventFormat';
import { getFootballGoalTypeInfo } from '../../../../utils/footballGoalType';
import { getPlayerPath } from '../../../../utils/sportRoutes';
import '../../../../components/match/matchEventKinds.scss';

type MatchEventItem = {
  type: 'goal' | 'card' | 'substitution' | 'missedPenalty';
  time: number;
  periodNumber: number;
  event: FootballGoalEventDto | FootballCardEventDto | FootballSubstitutionEventDto | FootballMissedPenaltyEventDto;
};

interface MatchEventsProps {
  match: FootballMatchDto;
  homeRoster: FootballTeamPlayer[];
  awayRoster: FootballTeamPlayer[];
}

type CardTypeName = 'Yellow' | 'SecondYellow' | 'DirectRed';

const CARD_CLASS: Record<CardTypeName, string> = {
  Yellow: 'yellow-card',
  SecondYellow: 'second-yellow-card',
  DirectRed: 'red-card',
};

function cardTypeName(cardType: FootballCardType | string): CardTypeName {
  const value = String(cardType);
  if (value === 'SecondYellow' || value === String(FootballCardType.SecondYellow)) return 'SecondYellow';
  if (value === 'DirectRed' || value === String(FootballCardType.DirectRed)) return 'DirectRed';
  return 'Yellow';
}

export default function MatchEvents({ match, homeRoster, awayRoster }: MatchEventsProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();

  const jerseyByPlayerId: Map<string, number> = useMemo(() => {
    const map: Map<string, number> = new Map<string, number>();
    for (const player of [...homeRoster, ...awayRoster]) {
      if (player.playerId && typeof player.jerseyNumber === 'number') {
        map.set(player.playerId, player.jerseyNumber);
      }
    }
    return map;
  }, [homeRoster, awayRoster]);

  const handlePlayerClick = (playerId: string | undefined, e: MouseEvent): void => {
    if (!playerId) return;
    e.stopPropagation();
    navigate(getPlayerPath('football', playerId));
  };

  const allEvents: MatchEventItem[] = [
    ...match.goalEvents.map((goal): MatchEventItem => ({
      type: 'goal',
      time: goal.timeInSeconds,
      periodNumber: goal.periodNumber,
      event: goal,
    })),
    ...match.cardEvents.map((card): MatchEventItem => ({
      type: 'card',
      time: card.timeInSeconds,
      periodNumber: card.periodNumber,
      event: card,
    })),
    ...match.substitutionEvents.map((sub): MatchEventItem => ({
      type: 'substitution',
      time: sub.timeInSeconds,
      periodNumber: sub.periodNumber,
      event: sub,
    })),
    ...(match.missedPenaltyEvents ?? []).map((missed): MatchEventItem => ({
      type: 'missedPenalty',
      time: missed.timeInSeconds,
      periodNumber: missed.periodNumber,
      event: missed,
    })),
  ];

  const eventsByPeriod: Record<number, MatchEventItem[]> = allEvents.reduce<Record<number, MatchEventItem[]>>(
    (acc, event) => {
      if (!acc[event.periodNumber]) {
        acc[event.periodNumber] = [];
      }
      acc[event.periodNumber].push(event);
      return acc;
    },
    {},
  );

  Object.keys(eventsByPeriod).forEach((period) => {
    eventsByPeriod[parseInt(period, 10)].sort((a, b) => a.time - b.time);
  });

  const getScoreAtEvent = (event: FootballGoalEventDto): string => {
    let homeScore = 0;
    let awayScore = 0;

    const sortedGoals: FootballGoalEventDto[] = [...match.goalEvents].sort((a, b) => {
      if (a.periodNumber !== b.periodNumber) {
        return a.periodNumber - b.periodNumber;
      }
      if (a.timeInSeconds !== b.timeInSeconds) {
        return a.timeInSeconds - b.timeInSeconds;
      }
      return match.goalEvents.indexOf(a) - match.goalEvents.indexOf(b);
    });

    for (const goal of sortedGoals) {
      if (
        goal.periodNumber > event.periodNumber ||
        (goal.periodNumber === event.periodNumber && goal.timeInSeconds > event.timeInSeconds)
      ) {
        break;
      }

      if (goal.teamId === match.homeTeamId) {
        homeScore++;
      } else {
        awayScore++;
      }

      if (goal === event) {
        break;
      }
    }

    return `${homeScore} - ${awayScore}`;
  };

  const isHomeTeam = (teamId: string): boolean => teamId === match.homeTeamId;

  const getTeamShort = (teamId: string): string => {
    const teamName: string = (isHomeTeam(teamId) ? match.homeTeamName : match.awayTeamName) ?? 'TBD';
    return getTeamInitials(teamName);
  };

  const renderTeamBadge = (teamId: string) => {
    const home = isHomeTeam(teamId);
    const teamName: string = (home ? match.homeTeamName : match.awayTeamName) ?? 'TBD';
    const teamLogo: string | null = home ? match.homeTeamLogo : match.awayTeamLogo;
    const initials: string = getTeamShort(teamId);
    const sideClass: string = home ? 'home-team' : 'away-team';

    if (teamLogo) {
      return (
        <span className={`event-team-short has-logo ${sideClass}`} title={teamName}>
          <img
            src={teamLogo}
            alt={`${teamName} logo`}
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
      <span className={`event-team-short ${sideClass}`} title={teamName}>
        {initials}
      </span>
    );
  };

  const renderPlayerName = (name: string | undefined, playerId: string | undefined) => {
    if (!name || name === 'Unknown Player') {
      return <span className="event-player-name">{t('matchPage.events.unknownPlayer')}</span>;
    }
    const jerseyNumber: number | undefined = playerId ? jerseyByPlayerId.get(playerId) : undefined;
    return (
      <span
        className={`event-player-name event-player-link ${playerId ? 'clickable' : ''}`}
        onClick={(e) => handlePlayerClick(playerId, e)}
      >
        {typeof jerseyNumber === 'number' && (
          <span className="event-player-number" aria-label={`#${jerseyNumber}`}>
            #{jerseyNumber}
          </span>
        )}
        <span className="event-player-name-text">{name}</span>
      </span>
    );
  };

  const renderAssistName = (name: string | undefined, assisterId: string | undefined) => {
    if (!name || name === 'Unknown Player') return null;
    const jerseyNumber: number | undefined = assisterId ? jerseyByPlayerId.get(assisterId) : undefined;
    return (
      <span
        className={`event-assist ${assisterId ? 'clickable' : ''}`}
        onClick={(e) => handlePlayerClick(assisterId, e)}
      >
        ({typeof jerseyNumber === 'number' ? `#${jerseyNumber} ` : ''}{name})
      </span>
    );
  };

  const renderGoalRow = (event: FootballGoalEventDto) => {
    const home = isHomeTeam(event.teamId);
    const goalTypeInfo = getFootballGoalTypeInfo(event.goalType);
    return (
      <div className={`event-row ${home ? 'home-event' : 'away-event'} goal`}>
        <span className="event-time" title={getPeriodName(event.periodNumber, match, t)}>
          {formatMatchEventTime(event.periodNumber, event.timeInSeconds)}
        </span>
        <span className="event-type-badge goal" aria-label={t('matchPage.events.goal')} title={t('matchPage.events.goal')}>
          <span className="badge-letter" aria-hidden>G</span>
        </span>
        {goalTypeInfo && goalTypeInfo.abbreviation && (
          <span className="goal-type-badge" title={t(`football.matches.manage.goalTypes.${goalTypeInfo.name}`, goalTypeInfo.label)} aria-label={t(`football.matches.manage.goalTypes.${goalTypeInfo.name}`, goalTypeInfo.label)}>
            ({goalTypeInfo.abbreviation})
          </span>
        )}
        {renderTeamBadge(event.teamId)}
        <span className="event-score">{getScoreAtEvent(event)}</span>
        <span className="event-details">
          {renderPlayerName(event.playerName, event.scoringPlayerId)}
          {event.assisterName && event.assisterName !== 'Unknown Player' && (
            <>
              {' '}
              {renderAssistName(event.assisterName, event.assistingPlayerId)}
            </>
          )}
        </span>
      </div>
    );
  };

  const renderCardRow = (event: FootballCardEventDto) => {
    const home = isHomeTeam(event.teamId);
    const cardName: CardTypeName = cardTypeName(event.cardType);
    const cardClass: string = CARD_CLASS[cardName];
    const label: string = t(`football.matches.manage.cardTypes.${cardName}`);
    const description = (event.description ?? '').trim();
    return (
      <div className={`event-row ${home ? 'home-event' : 'away-event'} ${cardClass}`}>
        <span className="event-time" title={getPeriodName(event.periodNumber, match, t)}>
          {formatMatchEventTime(event.periodNumber, event.timeInSeconds)}
        </span>
        <span className={`event-type-badge ${cardClass}`} aria-label={label} title={label}>
          <span className="badge-letter" aria-hidden>{t(`football.match.events.cardShort.${cardName}`)}</span>
        </span>
        {renderTeamBadge(event.teamId)}
        <span className="event-details penalty-details">
          <span className="penalty-line">
            {renderPlayerName(event.playerName, event.playerId)}
            <span className="penalty-type"> ({label.toLowerCase()})</span>
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

  const renderSubstitutionRow = (event: FootballSubstitutionEventDto) => {
    const home = isHomeTeam(event.teamId);
    return (
      <div className={`event-row ${home ? 'home-event' : 'away-event'} substitution`}>
        <span className="event-time" title={getPeriodName(event.periodNumber, match, t)}>
          {formatMatchEventTime(event.periodNumber, event.timeInSeconds)}
        </span>
        <span
          className="event-type-badge substitution"
          aria-label={t('football.matches.manage.events.type.substitution')}
          title={t('football.matches.manage.events.type.substitution')}
        >
          <i className="fas fa-exchange-alt" aria-hidden="true"></i>
        </span>
        {renderTeamBadge(event.teamId)}
        <span className="event-details">
          {renderPlayerName(event.playerOnName, event.playerOnId)}
          {' ← '}
          {renderPlayerName(event.playerOffName, event.playerOffId)}
        </span>
      </div>
    );
  };

  const renderMissedPenaltyRow = (event: FootballMissedPenaltyEventDto) => {
    const label: string = t('matchPage.events.missedPenalty');
    return (
      <div className={`event-row ${isHomeTeam(event.teamId) ? 'home-event' : 'away-event'} missed-penalty`}>
        <span className="event-time" title={getPeriodName(event.periodNumber, match, t)}>
          {formatMatchEventTime(event.periodNumber, event.timeInSeconds)}
        </span>
        <span className="event-type-badge missed-penalty" aria-label={label} title={label}>
          <i className="fas fa-times" aria-hidden="true"></i>
        </span>
        {renderTeamBadge(event.teamId)}
        <span className="event-details">
          {renderPlayerName(event.playerName, event.playerId)}
          <span className="penalty-type"> ({label.toLowerCase()})</span>
        </span>
      </div>
    );
  };

  if (allEvents.length === 0) {
    return null;
  }

  return (
    <div className="match-events">
      {Object.keys(eventsByPeriod)
        .map(Number)
        .sort((a, b) => a - b)
        .map((period) => (
          <div key={period} className="period-section">
            <div className="period-header">
              <span className="period-name">{getPeriodName(period, match, t)}</span>
            </div>
            <div className="period-events">
              {eventsByPeriod[period].map((eventItem, index) => (
                <div key={index}>
                  {eventItem.type === 'goal' && renderGoalRow(eventItem.event as FootballGoalEventDto)}
                  {eventItem.type === 'card' && renderCardRow(eventItem.event as FootballCardEventDto)}
                  {eventItem.type === 'substitution' &&
                    renderSubstitutionRow(eventItem.event as FootballSubstitutionEventDto)}
                  {eventItem.type === 'missedPenalty' &&
                    renderMissedPenaltyRow(eventItem.event as FootballMissedPenaltyEventDto)}
                </div>
              ))}
            </div>
          </div>
        ))}
    </div>
  );
}
