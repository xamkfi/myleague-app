import { useEffect, useMemo, useState, type ReactElement } from 'react';
import { useTranslation } from 'react-i18next';
import { FootballCardType } from '../../../../../types/football/footballTypes';
import type { EventGroup, ProcessedEvent } from './types';
import { formatMatchEventTime } from '../../../../../utils/matchEventFormat';
import { getFootballGoalTypeInfo } from '../../../../../utils/footballGoalType';
import BulkActionsBar from '../../../../../components/BulkActionsBar/BulkActionsBar';
import './LiveMatchEventsHistory.scss';

interface LiveMatchEventsHistoryProps {
  allEvents: ProcessedEvent[];
  onDeleteEvent?: (group: EventGroup) => void;
  onBulkDelete?: (groups: EventGroup[]) => void;
  canDelete?: boolean;
}

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

  return words.map((w) => w[0]).join('').toUpperCase();
}

function getEventTypeLabel(type: ProcessedEvent['type']): { label: string; icon: string } {
  switch (type) {
    case 'goal':
      return { label: 'Goal', icon: '⚽' };
    case 'card':
      return { label: 'Card', icon: '🟨' };
    case 'substitution':
      return { label: 'Sub', icon: '🔄' };
    default:
      return { label: '', icon: '' };
  }
}

function getCardTypeLabel(cardType: ProcessedEvent['cardType']): string {
  if (cardType === FootballCardType.Yellow || cardType === 'Yellow' || cardType === 0) {
    return 'Yellow';
  }
  if (cardType === FootballCardType.SecondYellow || cardType === 'SecondYellow' || cardType === 1) {
    return 'Second yellow';
  }
  if (cardType === FootballCardType.DirectRed || cardType === 'DirectRed' || cardType === 2) {
    return 'Direct red';
  }
  return 'Card';
}

function groupEvents(events: readonly ProcessedEvent[]): EventGroup[] {
  return events.map((event) => ({
    key: event.eventId ?? event.id,
    representative: event,
    events: [event],
  }));
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

  useEffect(() => {
    if (selectedKeys.size === 0) return;
    const liveKeys: Set<string> = new Set(groups.map((g) => g.key));
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

  useEffect(() => {
    if (!bulkSelectionEnabled && selectedKeys.size > 0) {
      setSelectedKeys(new Set());
    }
  }, [bulkSelectionEnabled, selectedKeys.size]);

  const toggleSelection = (key: string): void => {
    setSelectedKeys((prev) => {
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
    setSelectedKeys(new Set(groups.map((g) => g.key)));
  };

  const clearSelection = (): void => {
    setSelectedKeys(new Set());
  };

  const handleBulkDelete = (): void => {
    if (!onBulkDelete || selectedKeys.size === 0) return;
    const selectedGroups: EventGroup[] = groups.filter((g) => selectedKeys.has(g.key));
    if (selectedGroups.length === 0) return;
    onBulkDelete(selectedGroups);
    setSelectedKeys(new Set());
  };

  const allSelected: boolean = groups.length > 0 && selectedKeys.size === groups.length;

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
              label: t('common.bulk.delete', 'Delete ({{count}})', { count: selectedKeys.size }),
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
          {groups.map((group) => {
            const event: ProcessedEvent = group.representative;
            const { label, icon } = getEventTypeLabel(event.type);
            const teamShort = event.teamShortName?.trim()
              ? event.teamShortName
              : getTeamShortName(event.teamName);
            const goalTypeInfo = event.type === 'goal'
              ? getFootballGoalTypeInfo(event.goalType)
              : undefined;
            const cardDescription: string = event.type === 'card' ? (event.description ?? '').trim() : '';
            const isSelected: boolean = selectedKeys.has(group.key);

            return (
              <div
                key={group.key}
                className={`event-item ${event.type}${isSelected ? ' selected' : ''}`}
              >
                {bulkSelectionEnabled && (
                  <input
                    type="checkbox"
                    className="event-select events-history__checkbox"
                    checked={isSelected}
                    onChange={() => toggleSelection(group.key)}
                    aria-label={`Select ${event.type} at ${formatMatchEventTime(event.periodNumber, event.timeInSeconds)}`}
                    title="Select event"
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
                    </span>
                  ) : event.type === 'card' ? (
                    <span className="event-text penalty-text">
                      <span className="penalty-line">
                        {event.playerName || ''}
                        {` · ${getCardTypeLabel(event.cardType)}`}
                      </span>
                      {cardDescription && (
                        <span className="penalty-description" title={cardDescription}>
                          {cardDescription}
                        </span>
                      )}
                    </span>
                  ) : event.type === 'substitution' ? (
                    <span className="event-text">
                      <span className="player-name">
                        {event.playerOffName} → {event.playerOnName}
                      </span>
                    </span>
                  ) : null}
                </div>

                {canDelete && (
                  <button
                    className="event-delete"
                    title="Delete event"
                    onClick={() => onDeleteEvent && onDeleteEvent(group)}
                    aria-label="Delete event"
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
