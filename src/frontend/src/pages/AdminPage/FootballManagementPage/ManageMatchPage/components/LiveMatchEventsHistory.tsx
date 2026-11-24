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

const EVENT_ICON: Record<ProcessedEvent['type'], string> = {
  goal: '⚽',
  card: '🟨',
  substitution: '🔄',
  missedPenalty: '❌',
};

type CardTypeName = 'Yellow' | 'SecondYellow' | 'DirectRed';

function cardTypeName(cardType: ProcessedEvent['cardType']): CardTypeName | null {
  if (cardType === FootballCardType.Yellow || cardType === 'Yellow') return 'Yellow';
  if (cardType === FootballCardType.SecondYellow || cardType === 'SecondYellow') return 'SecondYellow';
  if (cardType === FootballCardType.DirectRed || cardType === 'DirectRed') return 'DirectRed';
  return null;
}

function eventIcon(event: ProcessedEvent): string {
  if (event.type !== 'card') return EVENT_ICON[event.type];
  return cardTypeName(event.cardType) === 'Yellow' ? '🟨' : '🟥';
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
  const selectAllLabel: string = allSelected
    ? t('floorball.matches.manage.events.clearSelection')
    : t('floorball.matches.manage.events.selectAllEvents');

  return (
    <div className="events-history">
      <div className="events-history__header">
        <h3>{t('floorball.matches.manage.events.title')}</h3>
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
            <span className="events-history__select-all-label">{t('floorball.matches.manage.events.selectAll')}</span>
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
        <div className="no-events">{t('floorball.matches.manage.events.empty')}</div>
      ) : (
        <div className="events-list">
          {groups.map((group) => {
            const event: ProcessedEvent = group.representative;
            const label: string = t(`football.matches.manage.events.type.${event.type}`);
            const cardName: CardTypeName | null = event.type === 'card' ? cardTypeName(event.cardType) : null;
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
                    aria-label={t('floorball.matches.manage.events.selectEvent', {
                      type: label,
                      time: formatMatchEventTime(event.periodNumber, event.timeInSeconds),
                    })}
                  />
                )}

                <div className="event-time">
                  {formatMatchEventTime(event.periodNumber, event.timeInSeconds)}
                </div>

                <span className={`event-type-badge ${event.type}`} aria-label={label} title={label}>
                  <span className="badge-icon" aria-hidden>
                    {eventIcon(event)}
                  </span>
                  <span className="badge-text">{label}</span>
                </span>

                {goalTypeInfo && goalTypeInfo.abbreviation && (
                  <span
                    className="goal-type-badge"
                    title={t(`football.matches.manage.goalTypes.${goalTypeInfo.name}`, goalTypeInfo.label)}
                    aria-label={t(`football.matches.manage.goalTypes.${goalTypeInfo.name}`, goalTypeInfo.label)}
                  >
                    ({goalTypeInfo.abbreviation})
                  </span>
                )}

                <span className="team-short" title={event.teamName}>{teamShort}</span>

                <div className="event-details">
                  {event.type === 'goal' ? (
                    <span className="event-text">
                      <span className="player-name">{event.playerName}</span>
                      {event.assisterName && ` (${t('floorball.matches.manage.events.assist', { name: event.assisterName })})`}
                    </span>
                  ) : event.type === 'card' ? (
                    <span className="event-text penalty-text">
                      <span className="penalty-line">
                        {event.playerName || ''}
                        {cardName && ` · ${t(`football.matches.manage.cardTypes.${cardName}`)}`}
                      </span>
                      {cardDescription && (
                        <span className="penalty-description" title={cardDescription}>
                          {cardDescription}
                        </span>
                      )}
                    </span>
                  ) : event.type === 'missedPenalty' ? (
                    <span className="event-text">
                      <span className="player-name">{event.playerName}</span>
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
                    title={t('floorball.matches.manage.events.deleteEvent')}
                    onClick={() => onDeleteEvent && onDeleteEvent(group)}
                    aria-label={t('floorball.matches.manage.events.deleteEvent')}
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
