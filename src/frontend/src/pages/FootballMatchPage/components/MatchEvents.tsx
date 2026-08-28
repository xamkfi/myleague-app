import { useEffect, useMemo, useState, type MouseEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import type {
  FootballMatchDto,
  FootballGoalEventDto,
  FootballCardEventDto,
  FootballSubstitutionEventDto,
  FootballTeamPlayer,
} from '../../../types/football/footballTypes';
import { FootballCardType } from '../../../types/football/footballTypes';
import { getPeriodName, getTeamInitials } from './matchUtils';
import { formatMatchEventTime } from '../../../utils/matchEventFormat';
import { getFootballGoalTypeInfo } from '../../../utils/footballGoalType';
import { footballTeamService } from '../../../api/football/footballTeamService';

type MatchEventItem = {
  type: 'goal' | 'card' | 'substitution';
  time: number;
  periodNumber: number;
  event: FootballGoalEventDto | FootballCardEventDto | FootballSubstitutionEventDto;
};

interface MatchEventsProps {
  match: FootballMatchDto;
}

function cardLabel(cardType: FootballCardType | string): string {
  const value = String(cardType);
  if (value === 'Yellow' || value === String(FootballCardType.Yellow)) return 'YC';
  if (value === 'SecondYellow' || value === String(FootballCardType.SecondYellow)) return '2YC';
  if (value === 'DirectRed' || value === String(FootballCardType.DirectRed)) return 'RC';
  return 'C';
}

export default function MatchEvents({ match }: MatchEventsProps) {
  const navigate = useNavigate();
  const [homeRoster, setHomeRoster] = useState<FootballTeamPlayer[]>([]);
  const [awayRoster, setAwayRoster] = useState<FootballTeamPlayer[]>([]);

  useEffect(() => {
    let cancelled = false;
    async function fetchRosters() {
      try {
        if (!match.homeTeamId || !match.awayTeamId) {
          setHomeRoster([]);
          setAwayRoster([]);
          return;
        }

        const [homeResponse, awayResponse] = await Promise.all([
          footballTeamService.getById(match.homeTeamId),
          footballTeamService.getById(match.awayTeamId),
        ]);
        if (cancelled) return;
        setHomeRoster(homeResponse.roster ?? []);
        setAwayRoster(awayResponse.roster ?? []);
      } catch (err) {
        console.error('Failed to load team rosters for match events:', err);
      }
    }
    void fetchRosters();
    return () => {
      cancelled = true;
    };
  }, [match.homeTeamId, match.awayTeamId]);

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
    navigate(`/football/player/${playerId}`);
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
      return <span className="event-player-name">{name || 'Unknown Player'}</span>;
    }
    const jerseyNumber: number | undefined = playerId ? jerseyByPlayerId.get(playerId) : undefined;
    return (
      <span
        className={`event-player-name event-player-link ${playerId ? 'clickable' : ''}`}
        onClick={(e) => handlePlayerClick(playerId, e)}
      >
        {typeof jerseyNumber === 'number' && (
          <span className="event-player-number" aria-label={`Number ${jerseyNumber}`}>
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
        <span className="event-time" title={`Period ${event.periodNumber}`}>
          {formatMatchEventTime(event.periodNumber, event.timeInSeconds)}
        </span>
        <span className="event-type-badge goal" aria-label="Goal" title="Goal">
          <span className="badge-letter" aria-hidden>G</span>
        </span>
        {goalTypeInfo && goalTypeInfo.abbreviation && (
          <span className="goal-type-badge" title={goalTypeInfo.label} aria-label={goalTypeInfo.label}>
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
    const label = cardLabel(event.cardType);
    const description = (event.description ?? '').trim();
    return (
      <div className={`event-row ${home ? 'home-event' : 'away-event'} penalty`}>
        <span className="event-time" title={`Period ${event.periodNumber}`}>
          {formatMatchEventTime(event.periodNumber, event.timeInSeconds)}
        </span>
        <span className="event-type-badge penalty" aria-label="Card" title={label}>
          <span className="badge-letter" aria-hidden>{label}</span>
        </span>
        {renderTeamBadge(event.teamId)}
        <span className="event-details penalty-details">
          <span className="penalty-line">
            {renderPlayerName(event.playerName, event.playerId)}
            <span className="penalty-type"> ({label})</span>
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
      <div className={`event-row ${home ? 'home-event' : 'away-event'} penalty`}>
        <span className="event-time" title={`Period ${event.periodNumber}`}>
          {formatMatchEventTime(event.periodNumber, event.timeInSeconds)}
        </span>
        <span className="event-type-badge penalty" aria-label="Substitution" title="Substitution">
          <span className="badge-letter" aria-hidden>SUB</span>
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

  return (
    <div className="match-events">
      {Object.keys(eventsByPeriod)
        .map(Number)
        .sort((a, b) => a - b)
        .map((period) => (
          <div key={period} className="period-section">
            <div className="period-header">
              <span className="period-name">{getPeriodName(period)}</span>
            </div>
            <div className="period-events">
              {eventsByPeriod[period].map((eventItem, index) => (
                <div key={index}>
                  {eventItem.type === 'goal' && renderGoalRow(eventItem.event as FootballGoalEventDto)}
                  {eventItem.type === 'card' && renderCardRow(eventItem.event as FootballCardEventDto)}
                  {eventItem.type === 'substitution' &&
                    renderSubstitutionRow(eventItem.event as FootballSubstitutionEventDto)}
                </div>
              ))}
            </div>
          </div>
        ))}
    </div>
  );
}
