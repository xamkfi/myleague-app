import type { ProcessedEvent } from './types';
import './LiveMatchEventsHistory.scss';

interface LiveMatchEventsHistoryProps {
  allEvents: ProcessedEvent[];
  formatEventTime: (timeInSeconds: number) => string;
  onDeleteEvent?: (event: ProcessedEvent) => void;
}

// Derive a smart placeholder short name from a full team name
function getTeamShortName(teamName: string): string {
  const safeName = (teamName || '').trim();
  if (safeName.length === 0) return '';

  const words = safeName.split(/\s+/).filter(Boolean);

  if (words.length === 1) {
    return words[0].substring(0, 3).toUpperCase();
  }

  if (words.length === 2) {
    const first = words[0].substring(0, 2);
    const second = words[1].substring(0, 1);
    return (first + second).toUpperCase();
  }

  // Three or more words: take first letter of each word (can be 3-4 letters typically)
  return words.map(w => w[0]).join('').toUpperCase();
}

function getEventTypeLabel(type: ProcessedEvent['type']): { label: string; icon: string } {
  switch (type) {
    case 'goal':
      return { label: 'Goal', icon: '⚽' };
    case 'penalty':
      return { label: 'Penalty', icon: '🟨' };
    case 'save':
      return { label: 'Save', icon: '🛡️' };
    default:
      return { label: '', icon: '' };
  }
}

const LiveMatchEventsHistory = ({
  allEvents,
  formatEventTime,
  onDeleteEvent
}: LiveMatchEventsHistoryProps) => {
  return (
    <div className="events-history">
      <h3>MATCH EVENTS</h3>
      {allEvents.length === 0 ? (
        <div className="no-events">No events recorded yet</div>
      ) : (
        <div className="events-list">
          {allEvents.map(event => {
            const { label, icon } = getEventTypeLabel(event.type);
            const teamShort = getTeamShortName(event.teamName);

            return (
              <div key={event.id} className={`event-item ${event.type}`}>
                <div className="event-time">
                  P{event.periodNumber} - {formatEventTime(event.timeInSeconds)}
                </div>

                <span className={`event-type-badge ${event.type}`} aria-label={label} title={label}>
                  <span className="badge-icon" aria-hidden>
                    {icon}
                  </span>
                  <span className="badge-text">{label}</span>
                </span>

                <span className="team-short" title={event.teamName}>{teamShort}</span>

                <div className="event-details">
                  {event.type === 'goal' ? (
                    <span className="event-text">
                      <span className="player-name">{event.playerName}</span>
                      {event.assisterName && ` (Assist: ${event.assisterName})`}
                      {event.wasInOvertime && ` (OT)`}
                      {event.wasInShootout && ` (SO)`}
                    </span>
                  ) : event.type === 'penalty' ? (
                    <span className="event-text">
                      {event.playerName || ''}
                      {event.penaltyMinutes ? ` · ${event.penaltyMinutes}min` : ''}
                    </span>
                  ) : event.type === 'save' ? (
                    <span className="event-text">
                      <span className="player-name">{event.playerName}</span>
                      {event.wasInOvertime && ` (OT)`}
                      {event.wasInShootout && ` (SO)`}
                    </span>
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
            );
          })}
        </div>
      )}
    </div>
  );
};

export default LiveMatchEventsHistory; 