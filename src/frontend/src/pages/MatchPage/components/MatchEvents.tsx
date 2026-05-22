import { useNavigate } from 'react-router-dom';
import type {
  FloorballMatchDto,
  FloorballGoalEventDto,
  FloorballPenaltyEventDto,
} from '../../../types/floorball/floorballTypes';
import { getPeriodName, getTeamInitials } from './matchUtils';
import { formatMatchEventTime } from '../../../utils/matchEventFormat';
import { getFloorballGoalTypeInfo } from '../../../utils/floorballGoalType';

type MatchEventItem = {
  type: 'goal' | 'penalty';
  time: number;
  periodNumber: number;
  event: FloorballGoalEventDto | FloorballPenaltyEventDto;
};

interface MatchEventsProps {
  match: FloorballMatchDto;
}

export default function MatchEvents({ match }: MatchEventsProps) {
  const navigate = useNavigate();

  const handlePlayerClick = (playerId: string | undefined, e: React.MouseEvent): void => {
    if (!playerId) return;
    e.stopPropagation();
    navigate(`/floorballplayer/${playerId}`);
  };

  const allEvents: MatchEventItem[] = [
    ...match.goalEvents.map((goal): MatchEventItem => ({
      type: 'goal',
      time: goal.timeInSeconds,
      periodNumber: goal.periodNumber,
      event: goal,
    })),
    ...match.penaltyEvents.map((penalty): MatchEventItem => ({
      type: 'penalty',
      time: penalty.timeInSeconds,
      periodNumber: penalty.periodNumber,
      event: penalty,
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
    eventsByPeriod[parseInt(period)].sort((a, b) => a.time - b.time);
  });

  const getScoreAtEvent = (event: FloorballGoalEventDto): string => {
    let homeScore: number = 0;
    let awayScore: number = 0;

    const sortedGoals: FloorballGoalEventDto[] = [...match.goalEvents].sort((a, b) => {
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
    const teamName: string = isHomeTeam(teamId) ? match.homeTeamName : match.awayTeamName;
    return getTeamInitials(teamName);
  };

  const renderPlayerName = (name: string | undefined, playerId: string | undefined) => {
    if (!name || name === 'Unknown Player') {
      return <span className="event-player-name">{name || 'Unknown Player'}</span>;
    }
    return (
      <span
        className={`event-player-name event-player-link ${playerId ? 'clickable' : ''}`}
        onClick={(e) => handlePlayerClick(playerId, e)}
      >
        {name}
      </span>
    );
  };

  const renderAssistName = (name: string | undefined, assisterId: string | undefined) => {
    if (!name || name === 'Unknown Player') return null;
    return (
      <span
        className={`event-assist ${assisterId ? 'clickable' : ''}`}
        onClick={(e) => handlePlayerClick(assisterId, e)}
      >
        ({name})
      </span>
    );
  };

  const renderGoalRow = (event: FloorballGoalEventDto) => {
    const home: boolean = isHomeTeam(event.teamId);
    const goalTypeInfo = getFloorballGoalTypeInfo(event.goalType);
    return (
      <div className={`event-row ${home ? 'home-event' : 'away-event'} goal`}>
        <span className="event-time" title={`Period ${event.periodNumber}`}>
          {formatMatchEventTime(event.periodNumber, event.timeInSeconds)}
        </span>
        <span className="event-type-badge goal" aria-label="Goal" title="Goal">
          <span className="badge-letter" aria-hidden>G</span>
        </span>
        {goalTypeInfo && goalTypeInfo.abbreviation && (
          <span
            className="goal-type-badge"
            title={goalTypeInfo.label}
            aria-label={goalTypeInfo.label}
          >
            ({goalTypeInfo.abbreviation})
          </span>
        )}
        <span className="event-team-short" title={home ? match.homeTeamName : match.awayTeamName}>
          {getTeamShort(event.teamId)}
        </span>
        <span className="event-score">{getScoreAtEvent(event)}</span>
        <span className="event-details">
          {renderPlayerName(event.playerName, event.playerId)}
          {event.assisterName && event.assisterName !== 'Unknown Player' && (
            <>
              {' '}
              {renderAssistName(event.assisterName, event.assisterId)}
            </>
          )}
          {event.secondaryAssisterName && event.secondaryAssisterName !== 'Unknown Player' && (
            <>
              {' '}
              {renderAssistName(event.secondaryAssisterName, event.secondaryAssisterId)}
            </>
          )}
        </span>
      </div>
    );
  };

  const renderPenaltyRow = (event: FloorballPenaltyEventDto) => {
    const home: boolean = isHomeTeam(event.teamId);
    // Trim defensively because old data may have a non-trimmed empty string. Without trimming
    // we'd render an empty italic line under every legacy penalty.
    const description: string = (event.description ?? '').trim();
    return (
      <div className={`event-row ${home ? 'home-event' : 'away-event'} penalty`}>
        <span className="event-time" title={`Period ${event.periodNumber}`}>
          {formatMatchEventTime(event.periodNumber, event.timeInSeconds)}
        </span>
        <span className="event-type-badge penalty" aria-label="Penalty" title="Penalty">
          <span className="badge-letter" aria-hidden>P</span>
        </span>
        <span className="event-team-short" title={home ? match.homeTeamName : match.awayTeamName}>
          {getTeamShort(event.teamId)}
        </span>
        <span className="event-details penalty-details">
          <span className="penalty-line">
            {renderPlayerName(event.playerName, event.playerId)}
            {event.penaltyType && (
              <span className="penalty-type"> ({event.penaltyType.toLowerCase()})</span>
            )}
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
                  {eventItem.type === 'goal'
                    ? renderGoalRow(eventItem.event as FloorballGoalEventDto)
                    : renderPenaltyRow(eventItem.event as FloorballPenaltyEventDto)}
                </div>
              ))}
            </div>
          </div>
        ))}
    </div>
  );
}
