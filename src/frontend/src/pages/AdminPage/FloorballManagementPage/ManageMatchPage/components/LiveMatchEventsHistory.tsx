import { useEffect, useMemo, useState, type ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import type { EventGroup, ProcessedEvent, ProcessedEventType } from './types';
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
   * Called when the user confirms a multi-select bulk delete. Selection is cleared
   * automatically after this is invoked.
   */
  onBulkDelete?: (groups: EventGroup[]) => void;
  /**
   * When false, the per-row delete affordance and multi-select are hidden. Used once the
   * match is Completed — the backend rejects deletes then anyway.
   */
  canDelete?: boolean;
}

// Derive a smart placeholder short name from a full team name
function getTeamShortName(teamName: string): string {
  const safeName: string = (teamName || '').trim();
  if (safeName.length === 0) return '';

  const words: string[] = safeName.split(/\s+/).filter(Boolean);

  if (words.length === 1) {
    return words[0].substring(0, 3).toUpperCase();
  }

  if (words.length === 2) {
    const first: string = words[0].substring(0, 2);
    const second: string = words[1].substring(0, 1);
    return (first + second).toUpperCase();
  }

  // Three or more words: take first letter of each word
  return words.map(w => w[0]).join('').toUpperCase();
}

const EVENT_TYPE_ICON: Record<ProcessedEventType, string> = {
  goal: 'fas fa-bullseye',
  penalty: 'fas fa-exclamation-triangle',
  save: 'fas fa-shield-alt',
};

/**
 * Collapses bulk-recorded saves into single visual rows. Saves are grouped when
 * they share team, goalie, period and time-in-seconds — exactly the coordinates
 * the bulk-save flow stamps onto every event it produces. Non-save events are
 * passed through 1:1. Input order is preserved.
 */
function groupEvents(events: readonly ProcessedEvent[]): EventGroup[] {
  const groups: EventGroup[] = [];
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

  const bulkSelectionEnabled: boolean = canDelete && !!onBulkDelete;

  const [selectedKeys, setSelectedKeys] = useState<Set<string>>(() => new Set());

  // Prune selections that no longer point at any rendered group.
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

  // Tear down the toolbar immediately if deletion gets disabled mid-flight.
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

  const typeLabel = (type: ProcessedEventType): string =>
    t(`floorball.matches.manage.events.type.${type}`, type);
  const selectAllLabel: string = allSelected
    ? t('floorball.matches.manage.events.clearSelection', 'Clear selection')
    : t('floorball.matches.manage.events.selectAllEvents', 'Select all events');

  return (
    <div className="events-history">
      <div className="events-history__header">
        <h3>
          {t('floorball.matches.manage.events.title', 'Match events')}
          {groups.length > 0 && <span className="events-history__count">{allEvents.length}</span>}
        </h3>
        {bulkSelectionEnabled && groups.length > 0 && (
          <label className="events-history__select-all" title={selectAllLabel}>
            <input
              type="checkbox"
              className="events-history__checkbox"
              checked={allSelected}
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
              aria-label={selectAllLabel}
            />
            <span className="events-history__select-all-label">{t('floorball.matches.manage.events.selectAll', 'Select all')}</span>
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
              // Surface the underlying event count (not the row count) so users know exactly
              // how many backend deletes they are authorising.
              label: t('common.bulk.delete', 'Delete ({{count}})', { count: totalSelectedEvents }),
              onClick: handleBulkDelete,
              variant: 'danger',
              disabled: selectedKeys.size === 0,
            },
          ]}
        />
      )}

      {groups.length === 0 ? (
        <div className="no-events">
          <i className="fas fa-stream" aria-hidden="true"></i>
          <span>{t('floorball.matches.manage.events.empty', 'No events recorded yet')}</span>
        </div>
      ) : (
        <div className="events-list">
          {groups.map(group => {
            const event: ProcessedEvent = group.representative;
            const groupSize: number = group.events.length;
            const label: string = typeLabel(event.type);
            const timeLabel: string = formatMatchEventTime(event.periodNumber, event.timeInSeconds);
            const teamShort: string = event.teamShortName?.trim()
              ? event.teamShortName
              : getTeamShortName(event.teamName);
            const goalTypeInfo = event.type === 'goal'
              ? getFloorballGoalTypeInfo(event.goalType)
              : undefined;
            const penaltyDescription: string = event.type === 'penalty' ? (event.description ?? '').trim() : '';
            const isBulkSave: boolean = event.type === 'save' && groupSize > 1;
            const isSelected: boolean = selectedKeys.has(group.key);
            const deleteLabel: string = isBulkSave
              ? t('floorball.matches.manage.events.deleteGroup', { count: groupSize, defaultValue: 'Delete all {{count}} saves in this group' })
              : t('floorball.matches.manage.events.deleteEvent', 'Delete event');

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
                        ? t('floorball.matches.manage.events.selectGroup', { count: groupSize, time: timeLabel, defaultValue: 'Select {{count}} saves at {{time}}' })
                        : t('floorball.matches.manage.events.selectEvent', { type: label, time: timeLabel, defaultValue: 'Select {{type}} at {{time}}' })
                    }
                  />
                )}

                <div className="event-time">{timeLabel}</div>

                <span className={`event-type-badge ${event.type}`} aria-label={label} title={label}>
                  <i className={`${EVENT_TYPE_ICON[event.type]} badge-icon`} aria-hidden="true"></i>
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
                  {event.type === 'goal' && (
                    <span className="event-text">
                      <span className="player-name">{event.playerName}</span>
                      {event.assisterName && (
                        <span className="event-meta">
                          {t('floorball.matches.manage.events.assist', { name: event.assisterName, defaultValue: 'Assist: {{name}}' })}
                        </span>
                      )}
                      {event.wasInOvertime && <span className="event-meta">OT</span>}
                      {event.wasInShootout && <span className="event-meta">SO</span>}
                    </span>
                  )}
                  {event.type === 'penalty' && (
                    <span className="event-text penalty-text">
                      <span className="penalty-line">
                        {event.playerName || ''}
                        {event.penaltyMinutes ? ` · ${event.penaltyMinutes} min` : ''}
                      </span>
                      {penaltyDescription && (
                        <span className="penalty-description" title={penaltyDescription}>
                          {penaltyDescription}
                        </span>
                      )}
                    </span>
                  )}
                  {event.type === 'save' && (
                    <span className="event-text">
                      <span className="player-name">{event.playerName}</span>
                      {isBulkSave && (
                        <span
                          className="save-count-badge"
                          title={t('floorball.matches.manage.events.savesRecordedTogether', { count: groupSize, defaultValue: '{{count}} saves recorded together' })}
                        >
                          ×{groupSize}
                        </span>
                      )}
                      {event.wasInOvertime && <span className="event-meta">OT</span>}
                      {event.wasInShootout && <span className="event-meta">SO</span>}
                    </span>
                  )}
                </div>

                {canDelete && (
                  <button
                    type="button"
                    className="event-delete"
                    title={deleteLabel}
                    onClick={() => onDeleteEvent && onDeleteEvent(group)}
                    aria-label={deleteLabel}
                  >
                    <i className="fas fa-times" aria-hidden="true"></i>
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
