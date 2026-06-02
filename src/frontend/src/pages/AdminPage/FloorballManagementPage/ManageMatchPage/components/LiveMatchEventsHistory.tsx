import { useEffect, useMemo, useState, type ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import type { EventGroup, ProcessedEvent } from './types';
import { formatMatchEventTime } from '../../../../../utils/matchEventFormat';
import { getFloorballGoalTypeInfo } from '../../../../../utils/floorballGoalType';
import BulkActionsBar from '../../../../../components/BulkActionsBar/BulkActionsBar';
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
   * Called when the user confirms a multi-select bulk delete (checkbox column +
   * bulk actions toolbar). The callback receives every selected group and is
   * expected to drive the deletion + confirmation flow itself, just like
   * {@link onDeleteEvent} does for single rows.
   *
   * Selection is cleared automatically after this is invoked so the toolbar
   * disappears immediately while the parent runs the (typically asynchronous)
   * delete pipeline.
   */
  onBulkDelete?: (groups: EventGroup[]) => void;
  /**
   * When false, the per-row delete affordance is hidden. Used to make the events list
   * read-only once the match is Completed — at that point the backend rejects deletes
   * anyway and the only sanctioned way to mutate events is to reopen the match first.
   *
   * Hiding the per-row "x" also disables multi-select: there is nothing the bulk bar
   * could do that the backend would accept, so we keep the UI honest.
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
  onBulkDelete,
  canDelete = true,
}: LiveMatchEventsHistoryProps): ReactElement => {
  const { t } = useTranslation();
  const groups: EventGroup[] = useMemo(() => groupEvents(allEvents), [allEvents]);

  // Multi-select is only meaningful when both deletion and a bulk handler are wired.
  // Without `onBulkDelete` the parent has opted out of bulk deletion; without `canDelete`
  // the backend rejects deletes anyway and we hide the affordance entirely.
  const bulkSelectionEnabled: boolean = canDelete && !!onBulkDelete;

  const [selectedKeys, setSelectedKeys] = useState<Set<string>>(() => new Set());

  // Prune selections that no longer point at any rendered group. Without this, deleting
  // some-but-not-all selected events would leave dangling ids in the set and the toolbar's
  // "selected count" would lie until the user reset it manually.
  useEffect(() => {
    if (selectedKeys.size === 0) return;
    const liveKeys: Set<string> = new Set(groups.map(g => g.key));
    let changed: boolean = false;
    const pruned: Set<string> = new Set<string>();
    for (const key of selectedKeys) {
      if (liveKeys.has(key)) {
        pruned.add(key);
      } else {
        changed = true;
      }
    }
    if (changed) {
      setSelectedKeys(pruned);
    }
  }, [groups, selectedKeys]);

  // If the parent disables deletion mid-flight (e.g. match transitioned to Completed),
  // tear down the toolbar immediately so the user doesn't see a stale "X selected" badge
  // pointing at rows whose checkboxes have just disappeared.
  useEffect(() => {
    if (!bulkSelectionEnabled && selectedKeys.size > 0) {
      setSelectedKeys(new Set());
    }
  }, [bulkSelectionEnabled, selectedKeys.size]);

  const toggleSelection = (key: string): void => {
    setSelectedKeys(prev => {
      const next: Set<string> = new Set(prev);
      if (next.has(key)) {
        next.delete(key);
      } else {
        next.add(key);
      }
      return next;
    });
  };

  const handleSelectAll = (): void => {
    setSelectedKeys(new Set(groups.map(g => g.key)));
  };

  const clearSelection = (): void => {
    setSelectedKeys(new Set());
  };

  const handleBulkDelete = (): void => {
    if (!onBulkDelete || selectedKeys.size === 0) return;
    const selectedGroups: EventGroup[] = groups.filter(g => selectedKeys.has(g.key));
    if (selectedGroups.length === 0) return;
    onBulkDelete(selectedGroups);
    // Optimistically clear the toolbar; the parent will re-render the list from the
    // backend response so a follow-up prune pass via `useEffect` is unnecessary but
    // harmless.
    setSelectedKeys(new Set());
  };

  const allSelected: boolean = groups.length > 0 && selectedKeys.size === groups.length;
  const totalSelectedEvents: number = useMemo(() => {
    if (selectedKeys.size === 0) return 0;
    let total: number = 0;
    for (const group of groups) {
      if (selectedKeys.has(group.key)) {
        total += group.events.length;
      }
    }
    return total;
  }, [groups, selectedKeys]);

  return (
    <div className="events-history">
      <div className="events-history__header">
        <h3>MATCH EVENTS</h3>
        {bulkSelectionEnabled && groups.length > 0 && (
          <label className="events-history__select-all" title={allSelected ? 'Clear selection' : 'Select all events'}>
            <input
              type="checkbox"
              className="events-history__checkbox"
              checked={allSelected}
              // Render an indeterminate state when only some rows are selected so the
              // header checkbox accurately reflects the partial selection.
              ref={(el) => {
                if (el) el.indeterminate = !allSelected && selectedKeys.size > 0;
              }}
              onChange={(e) => {
                if (e.target.checked) {
                  handleSelectAll();
                } else {
                  clearSelection();
                }
              }}
              aria-label={allSelected ? 'Clear selection' : 'Select all events'}
            />
            <span className="events-history__select-all-label">Select all</span>
          </label>
        )}
      </div>

      {bulkSelectionEnabled && (
        <BulkActionsBar
          selectedCount={selectedKeys.size}
          totalCount={groups.length}
          onSelectAll={handleSelectAll}
          onClearSelection={clearSelection}
          actions={[
            {
              // Surface the underlying event count (not the group count) when the
              // selection mixes single rows with a bulk-save cluster, so users know
              // exactly how many backend deletes they're authorising.
              label: totalSelectedEvents !== selectedKeys.size
                ? t(
                    'common.bulk.delete',
                    'Delete ({{count}})',
                    { count: totalSelectedEvents }
                  )
                : t(
                    'common.bulk.delete',
                    'Delete ({{count}})',
                    { count: selectedKeys.size }
                  ),
              onClick: handleBulkDelete,
              variant: 'danger',
              disabled: selectedKeys.size === 0,
            },
          ]}
        />
      )}

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
            const isSelected: boolean = selectedKeys.has(group.key);

            return (
              <div
                key={group.key}
                className={`event-item ${event.type}${isBulkSave ? ' bulk-save' : ''}${isSelected ? ' selected' : ''}`}
              >
                {bulkSelectionEnabled && (
                  <input
                    type="checkbox"
                    className="event-select events-history__checkbox"
                    checked={isSelected}
                    onChange={() => toggleSelection(group.key)}
                    aria-label={
                      isBulkSave
                        ? `Select ${groupSize} saves at ${formatMatchEventTime(event.periodNumber, event.timeInSeconds)}`
                        : `Select ${event.type} at ${formatMatchEventTime(event.periodNumber, event.timeInSeconds)}`
                    }
                    title={isBulkSave ? `Select all ${groupSize} saves in this group` : 'Select event'}
                  />
                )}

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
