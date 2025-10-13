import type { ProcessedEvent } from './types';

interface LiveMatchEventsHistoryProps {
  allEvents: ProcessedEvent[];
  formatEventTime: (timeInSeconds: number) => string;
  onDeleteEvent?: (event: ProcessedEvent) => void;
}

const LiveMatchEventsHistory = ({
  allEvents,
  formatEventTime,
  onDeleteEvent
}: LiveMatchEventsHistoryProps) => {
  return (
    <div className="events-history">
      <h3>Match Events</h3>
      {allEvents.length === 0 ? (
        <div className="no-events">No events recorded yet</div>
      ) : (
        <div className="events-list">
          {allEvents.map(event => (
            <div key={event.id} className={`event-item ${event.type}`}>
              <div className="event-time">
                P{event.periodNumber} - {formatEventTime(event.timeInSeconds)}
              </div>
              <div className="event-details">
                {event.type === 'goal' ? (
                  <div className="goal-event">
                    <span className="event-icon">⚽</span>
                    <span className="event-text">
                      <strong>{event.teamName}</strong> - Goal by {event.playerName}
                      {event.assisterName && ` (Assist: ${event.assisterName})`}
                      {event.wasInOvertime && ` (OT)`}
                      {event.wasInShootout && ` (SO)`}
                    </span>
                  </div>
                ) : event.type === 'penalty' ? (
                  <div className="penalty-event">
                    <span className="event-icon">🟨</span>
                    <span className="event-text">
                      <strong>{event.teamName}</strong> - {event.penaltyType} ({event.penaltyMinutes}min)
                      {event.playerName && ` - ${event.playerName}`}
                    </span>
                  </div>
                ) : event.type === 'save' ? (
                  <div className="save-event">
                    <span className="event-icon">🛡️</span>
                    <span className="event-text">
                      <strong>{event.teamName}</strong> - Save by {event.playerName}
                      {event.wasInOvertime && ` (OT)`}
                      {event.wasInShootout && ` (SO)`}
                    </span>
                  </div>
                ) : null}
              </div>
              <button
                className="event-delete"
                title="Delete event"
                onClick={() => onDeleteEvent && onDeleteEvent(event)}
                aria-label="Delete event"
              >
                ×
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default LiveMatchEventsHistory; 