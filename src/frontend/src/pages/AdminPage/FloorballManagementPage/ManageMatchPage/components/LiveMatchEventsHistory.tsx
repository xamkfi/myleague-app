import { useMemo } from 'react';
import type { EventGroup, ProcessedEvent } from './types';
import { formatMatchEventTime } from '../../../../../utils/matchEventFormat';
import { getFloorballGoalTypeInfo } from '../../../../../utils/floorballGoalType';
import './LiveMatchEventsHistory.scss';

interface LiveMatchEventsHistoryProps {
  allEvents: ProcessedEvent[];
  /**
   * Called when the user clicks the per-row delete affordance. The group always
   * contains at least one event; for bulk-recorded saves it can contain many,
   * and the consumer is responsible for deleting all of them.
   */
  onDeleteEvent?: (group: EventGroup) => void;
  /**
   * When false, the per-row delete affordance is hidden. Used to make the events list
   * read-only once the match is Completed — at that point the backend rejects deletes
   * anyway and the only sanctioned way to mutate events is to reopen the match first.
   */
  canDelete?: boolean;
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

/**
 * Collapses bulk-recorded saves into single visual rows. Saves are grouped when
 * they share team, goalie, period and time-in-seconds — exactly the coordinates
 * the bulk-save flow stamps onto every event it produces — so genuinely separate
 * saves recorded at different moments remain on their own rows. Non-save events
 * are passed through 1:1.
 *
 * Input order is preserved so the existing "most recent first" sort produced by
 * `useMatchEvents` continues to dictate the rendering order.
 */
function groupEvents(events: readonly ProcessedEvent[]): EventGroup[] {
  const groups: EventGroup[] = [];
  // Maps a save-group key → its index in `groups` so we can append additional
  // saves to an existing visual row without doing an O(n) scan per event.
  const saveKeyToIndex = new Map<string, number>();

  for (const event of events) {
    if (event.type === 'save') {
      const key: string = `save|${event.teamId}|${event.playerId ?? ''}|${event.periodNumber}|${event.timeInSeconds}`;
      const existingIndex: number | undefined = saveKeyToIndex.get(key);
      if (existingIndex !== undefined) {
        groups[existingIndex].events.push(event);
        continue;
      }
      saveKeyToIndex.set(key, groups.length);
      groups.push({
        key: `${key}|${event.eventId ?? event.id}`,
        representative: event,
        events: [event],
      });
      continue;
    }

    groups.push({
      key: event.eventId ?? event.id,
      representative: event,
      events: [event],
    });
  }

  return groups;
}

const LiveMatchEventsHistory = ({
  allEvents,
  onDeleteEvent,
  canDelete = true,
}: LiveMatchEventsHistoryProps) => {
  const groups: EventGroup[] = useMemo(() => groupEvents(allEvents), [allEvents]);

  return (
    <div className="events-history">
      <h3>MATCH EVENTS</h3>
      {groups.length === 0 ? (
        <div className="no-events">No events recorded yet</div>
      ) : (
        <div className="events-list">
          {groups.map(group => {
            const event: ProcessedEvent = group.representative;
            const groupSize: number = group.events.length;
            const { label, icon } = getEventTypeLabel(event.type);
            const teamShort = event.teamShortName?.trim()
              ? event.teamShortName
              : getTeamShortName(event.teamName);
            const goalTypeInfo = event.type === 'goal'
              ? getFloorballGoalTypeInfo(event.goalType)
              : undefined;
            // Trim the description so whitespace-only entries (e.g. left over from a previously
            // typed-then-cleared note) don't render an empty line under the penalty row.
            const penaltyDescription: string = event.type === 'penalty' ? (event.description ?? '').trim() : '';
            const isBulkSave: boolean = event.type === 'save' && groupSize > 1;

            return (
              <div key={group.key} className={`event-item ${event.type}${isBulkSave ? ' bulk-save' : ''}`}>
                <div className="event-time">
                  {formatMatchEventTime(event.periodNumber, event.timeInSeconds)}
                </div>

                <span className={`event-type-badge ${event.type}`} aria-label={label} title={label}>
                  <span className="badge-icon" aria-hidden>
                    {icon}
                  </span>
                  <span className="badge-text">{label}</span>
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
                    <span className="event-text penalty-text">
                      <span className="penalty-line">
                        {event.playerName || ''}
                        {event.penaltyMinutes ? ` · ${event.penaltyMinutes}min` : ''}
                      </span>
                      {penaltyDescription && (
                        <span className="penalty-description" title={penaltyDescription}>
                          {penaltyDescription}
                        </span>
                      )}
                    </span>
                  ) : event.type === 'save' ? (
                    <span className="event-text">
                      <span className="player-name">{event.playerName}</span>
                      {isBulkSave && (
                        <span
                          className="save-count-badge"
                          title={`${groupSize} saves recorded together`}
                          aria-label={`${groupSize} saves`}
                        >
                          {` (×${groupSize})`}
                        </span>
                      )}
                      {event.wasInOvertime && ` (OT)`}
                      {event.wasInShootout && ` (SO)`}
                    </span>
                  ) : null}
                </div>

                {canDelete && (
                  <button
                    className="event-delete"
                    title={isBulkSave ? `Delete all ${groupSize} saves in this group` : 'Delete event'}
                    onClick={() => onDeleteEvent && onDeleteEvent(group)}
                    aria-label={isBulkSave ? `Delete all ${groupSize} saves` : 'Delete event'}
                  >
                    ×
                  </button>
                )}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
};

export default LiveMatchEventsHistory; 