import { useNavigate } from 'react-router-dom';
import type { FloorballMatchDto, FloorballGoalEventDto, FloorballPenaltyEventDto } from '../../../types/floorball/floorballTypes';
import { getPeriodName } from './matchUtils';

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

  const handlePlayerClick = (playerId: string | undefined, e: React.MouseEvent) => {
    if (!playerId) return;
    e.stopPropagation();
    navigate(`/floorballplayer/${playerId}`);
  };

  // Combine all events and sort by time within each period
  const allEvents: MatchEventItem[] = [
    ...match.goalEvents.map(goal => ({
      type: 'goal' as const,
      time: goal.timeInSeconds,
      periodNumber: goal.periodNumber,
      event: goal
    })),
    ...match.penaltyEvents.map(penalty => ({
      type: 'penalty' as const,
      time: penalty.timeInSeconds,
      periodNumber: penalty.periodNumber,
      event: penalty
    }))
  ];

  // Group events by period
  const eventsByPeriod = allEvents.reduce((acc, event) => {
    if (!acc[event.periodNumber]) {
      acc[event.periodNumber] = [];
    }
    acc[event.periodNumber].push(event);
    return acc;
  }, {} as Record<number, MatchEventItem[]>);

  // Sort events within each period by time
  Object.keys(eventsByPeriod).forEach(period => {
    eventsByPeriod[parseInt(period)].sort((a, b) => a.time - b.time);
  });

  const getScoreAtEvent = (event: FloorballGoalEventDto) => {
    let homeScore = 0;
    let awayScore = 0;

    const sortedGoals = [...match.goalEvents].sort((a, b) => {
      if (a.periodNumber !== b.periodNumber) {
        return a.periodNumber - b.periodNumber;
      }
      if (a.timeInSeconds !== b.timeInSeconds) {
        return a.timeInSeconds - b.timeInSeconds;
      }
      return match.goalEvents.indexOf(a) - match.goalEvents.indexOf(b);
    });

    for (const goal of sortedGoals) {
      if (goal.periodNumber > event.periodNumber || 
          (goal.periodNumber === event.periodNumber && goal.timeInSeconds > event.timeInSeconds)) {
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

  const isHomeTeam = (teamId: string) => teamId === match.homeTeamId;

  const renderPlayerName = (name: string | undefined, playerId: string | undefined) => {
    if (!name || name === 'Unknown Player') return <span>{name || 'Unknown Player'}</span>;
    return (
      <span 
        className={`event-player-link ${playerId ? 'clickable' : ''}`}
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

  const renderGoalEvent = (event: FloorballGoalEventDto, isHome: boolean) => (
    <div className={`event-row ${isHome ? 'home-event' : 'away-event'}`}>
      <div className="event-left">
        {isHome && (
          <>
            <span className="event-time">{Math.ceil(event.timeInSeconds / 60)}</span>
            <div className="event-icon goal">G</div>
            <span className="event-score">{getScoreAtEvent(event)}</span>
            <div className="goal-info">
              <span className="event-player">{renderPlayerName(event.playerName, event.playerId)}</span>
              {event.assisterName && event.assisterName !== 'Unknown Player' && (
                <span className="event-assists-inline">
                  {' '}{renderAssistName(event.assisterName, event.assisterId)}
                </span>
              )}
              {event.secondaryAssisterName && event.secondaryAssisterName !== 'Unknown Player' && (
                <span className="event-assists-inline">
                  {' '}{renderAssistName(event.secondaryAssisterName, event.secondaryAssisterId)}
                </span>
              )}
            </div>
          </>
        )}
      </div>
      <div className="event-right">
        {!isHome && (
          <>
            <div className="goal-info">
              <span className="event-player">{renderPlayerName(event.playerName, event.playerId)}</span>
              {event.assisterName && event.assisterName !== 'Unknown Player' && (
                <span className="event-assists-inline">
                  {' '}{renderAssistName(event.assisterName, event.assisterId)}
                </span>
              )}
              {event.secondaryAssisterName && event.secondaryAssisterName !== 'Unknown Player' && (
                <span className="event-assists-inline">
                  {' '}{renderAssistName(event.secondaryAssisterName, event.secondaryAssisterId)}
                </span>
              )}
            </div>
            <span className="event-score">{getScoreAtEvent(event)}</span>
            <div className="event-icon goal">G</div>
            <span className="event-time">{Math.ceil(event.timeInSeconds / 60)}</span>
          </>
        )}
      </div>
    </div>
  );

  const renderPenaltyEvent = (event: FloorballPenaltyEventDto, isHome: boolean) => (
    <div className={`event-row ${isHome ? 'home-event' : 'away-event'}`}>
      <div className="event-left">
        {isHome && (
          <>
            <span className="event-time">{Math.ceil(event.timeInSeconds / 60)}</span>
            <div className="event-icon penalty">P</div>
            <span className="event-player">
              {renderPlayerName(event.playerName, event.playerId)}
              {event.penaltyType && <span className="penalty-type"> ({event.penaltyType.toLowerCase()})</span>}
            </span>
          </>
        )}
      </div>
      <div className="event-right">
        {!isHome && (
          <>
            <span className="event-player">
              {renderPlayerName(event.playerName, event.playerId)}
              {event.penaltyType && <span className="penalty-type"> ({event.penaltyType.toLowerCase()})</span>}
            </span>
            <div className="event-icon penalty">P</div>
            <span className="event-time">{Math.ceil(event.timeInSeconds / 60)}</span>
          </>
        )}
      </div>
    </div>
  );

  return (
    <div className="match-events">
      {Object.keys(eventsByPeriod)
        .map(Number)
        .sort((a, b) => a - b)
        .map(period => (
          <div key={period} className="period-section">
            <div className="period-header">
              <span className="period-name">{getPeriodName(period)}</span>
            </div>
            <div className="period-events">
              {eventsByPeriod[period].map((eventItem, index) => {
                const isHome = isHomeTeam(eventItem.event.teamId);
                return (
                  <div key={index}>
                    {eventItem.type === 'goal' 
                      ? renderGoalEvent(eventItem.event as FloorballGoalEventDto, isHome)
                      : renderPenaltyEvent(eventItem.event as FloorballPenaltyEventDto, isHome)
                    }
                  </div>
                );
              })}
            </div>
          </div>
        ))}
    </div>
  );
}
