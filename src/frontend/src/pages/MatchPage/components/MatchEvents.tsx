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

  const getPeriodScore = (period: number) => {
    const scores = match.periodScores[period];
    return scores ? `${scores.homeScore} - ${scores.awayScore}` : '0 - 0';
  };

  const isHomeTeam = (teamId: string) => teamId === match.homeTeamId;

  const renderGoalEvent = (event: FloorballGoalEventDto, isHome: boolean) => (
    <div className={`event-row ${isHome ? 'home-event' : 'away-event'}`}>
      <div className="event-left">
        {isHome && (
          <>
            <span className="event-time">{Math.floor(event.timeInSeconds / 60)}</span>
            <div className="event-icon goal">⚽</div>
            <span className="event-score">{getPeriodScore(event.periodNumber)}</span>
            <div className="goal-info">
              <span className="event-player">{event.playerName || 'Unknown Player'}</span>
              {event.assisterName && <span className="event-assist">{event.assisterName}</span>}
              {event.secondaryAssisterName && <span className="event-assist">{event.secondaryAssisterName}</span>}
            </div>
          </>
        )}
      </div>
      <div className="event-right">
        {!isHome && (
          <>
            <div className="goal-info">

              <span className="event-player">{event.playerName || 'Unknown Player'}</span>
              {event.assisterName && <span className="event-assist">{event.assisterName}</span>}
              {event.secondaryAssisterName && <span className="event-assist">{event.secondaryAssisterName}</span>}
            </div>
            <span className="event-score">{getPeriodScore(event.periodNumber)}</span>
            <div className="event-icon goal">⚽</div>
            <span className="event-time">{Math.floor(event.timeInSeconds / 60)}</span>
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
            <span className="event-time">{Math.floor(event.timeInSeconds / 60)}</span>
            <div className="event-icon penalty">🟨</div>
            <span className="event-player">{event.playerName || 'Team penalty'}</span>
          </>
        )}
      </div>
      <div className="event-right">
        {!isHome && (
          <>
            <span className="event-player">{event.playerName || 'Team penalty'}</span>
            <div className="event-icon penalty">🟨</div>
            <span className="event-time">{Math.floor(event.timeInSeconds / 60)}</span>
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