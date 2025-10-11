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

    // Sort goals by period and time
    const sortedGoals = [...match.goalEvents].sort((a, b) => {
      if (a.periodNumber !== b.periodNumber) {
        return a.periodNumber - b.periodNumber;
      }
      if (a.timeInSeconds !== b.timeInSeconds) {
        return a.timeInSeconds - b.timeInSeconds;
      }
      // If same time, use array index to maintain consistent order
      return match.goalEvents.indexOf(a) - match.goalEvents.indexOf(b);
    });

    // Count goals up to and including this one
    for (const goal of sortedGoals) {
      // Stop if we've reached a future goal
      if (goal.periodNumber > event.periodNumber || 
          (goal.periodNumber === event.periodNumber && goal.timeInSeconds > event.timeInSeconds)) {
        break;
      }

      // Count this goal
      if (goal.teamId === match.homeTeamId) {
        homeScore++;
      } else {
        awayScore++;
      }

      // If this is the current goal, stop here
      if (goal === event) {
        break;
      }
    }

    return `${homeScore} - ${awayScore}`;
  };

  const isHomeTeam = (teamId: string) => teamId === match.homeTeamId;

  const renderGoalEvent = (event: FloorballGoalEventDto, isHome: boolean) => (
    <div className={`event-row ${isHome ? 'home-event' : 'away-event'}`}>
      <div className="event-left">
        {isHome && (
          <>
            <span className="event-time">{Math.ceil(event.timeInSeconds / 60)}</span>
            <div className="event-icon goal">⚽</div>
            <span className="event-score">{getScoreAtEvent(event)}</span>
            <div className="goal-info">
              <span className="event-player">{event.playerName || 'Unknown Player'}</span>
              {event.assisterName && event.assisterName !== 'Unknown Player' && <span className="event-assist"> ({event.assisterName})</span>}
              {event.secondaryAssisterName && event.secondaryAssisterName !== 'Unknown Player' && <span className="event-assist"> ({event.secondaryAssisterName})</span>}
            </div>
          </>
        )}
      </div>
      <div className="event-right">
        {!isHome && (
          <>
            <div className="goal-info">

              <span className="event-player">{event.playerName || 'Unknown Player'}</span>
              {event.assisterName && event.assisterName !== 'Unknown Player' && <span className="event-assist"> ({event.assisterName})</span>}
              {event.secondaryAssisterName && event.secondaryAssisterName !== 'Unknown Player' && <span className="event-assist"> ({event.secondaryAssisterName})</span>}
            </div>
            <span className="event-score">{getScoreAtEvent(event)}</span>
            <div className="event-icon goal">⚽</div>
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
            <div className="event-icon penalty">🟨</div>
            <span className="event-player">
              {event.playerName || 'Team penalty'}
              {event.penaltyType && ` (${event.penaltyType.toLowerCase()})`}
            </span>
          </>
        )}
      </div>
      <div className="event-right">
        {!isHome && (
          <>
            <span className="event-player">
              {event.playerName || 'Team penalty'}
              {event.penaltyType && ` (${event.penaltyType.toLowerCase()})`}
            </span>
            <div className="event-icon penalty">🟨</div>
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