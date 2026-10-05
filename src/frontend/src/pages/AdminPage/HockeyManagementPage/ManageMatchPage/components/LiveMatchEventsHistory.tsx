import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { HockeyMatchEventDto } from '../../../../../types/hockey/hockeyTypes';
import { HOCKEY_SHOT_RESULTS } from '../../../../../types/hockey/hockeyTypes';
import { formatHockeyClock } from '../../../../../utils/hockeyLookups';
import './LiveMatchEventsHistory.scss';

interface LiveMatchEventsHistoryProps {
  events: HockeyMatchEventDto[];
  teamNamesByMatchTeamId: Map<string, string>;
  /** Display labels ("#12 Name") keyed by match active player id. */
  activePlayerLabels: Map<string, string>;
  onDeleteEvent?: (event: HockeyMatchEventDto) => void;
  onDeleteEvents?: (events: HockeyMatchEventDto[]) => void;
  canDelete?: boolean;
  busy?: boolean;
}

interface HistoryRow {
  event: HockeyMatchEventDto;
  members: HockeyMatchEventDto[];
}

const SHOT_RESULT_SET: ReadonlySet<string> = new Set(HOCKEY_SHOT_RESULTS);
const PERIOD_PREVIEW_COUNT = 5;

function isSave(event: HockeyMatchEventDto): boolean {
  return event.eventType.toLowerCase().includes('shot') && event.description === 'Saved';
}

/** Collapses consecutive saves with the same team and clock (bulk-recorded) into one row. */
function groupSaves(events: HockeyMatchEventDto[]): HistoryRow[] {
  const rows: HistoryRow[] = [];
  for (const event of events) {
    const previous: HistoryRow | undefined = rows[rows.length - 1];
    if (
      previous
      && isSave(event)
      && isSave(previous.event)
      && previous.event.matchTeamId === event.matchTeamId
      && previous.event.gameTimeSeconds === event.gameTimeSeconds
    ) {
      previous.members.push(event);
      continue;
    }
    rows.push({ event, members: [event] });
  }
  return rows;
}

function eventLabel(
  eventType: string,
  description: string | null,
  t: (key: string, fallback: string) => string,
): { label: string; icon: string } {
  const type = eventType.toLowerCase();
  if (type.includes('goal')) {
    return { label: t('hockey.matches.eventGoal', 'Goal'), icon: '🥅' };
  }
  if (type.includes('penalty')) {
    return { label: t('hockey.matches.eventPenalty', 'Penalty'), icon: '🟧' };
  }
  if (type.includes('shot')) {
    if (description === 'Saved') {
      return { label: t('hockey.matches.eventSave', 'Save'), icon: '🛡️' };
    }
    return { label: t('hockey.matches.eventShot', 'Shot'), icon: '🏒' };
  }
  if (type.includes('faceoff')) {
    return { label: t('hockey.matches.eventFaceoff', 'Face-off'), icon: '🏒' };
  }
  if (type.includes('stoppage')) {
    if (description === 'Offside') {
      return { label: t('hockey.matches.eventOffside', 'Offside'), icon: '🛑' };
    }
    return { label: t('hockey.matches.eventStoppage', 'Stoppage'), icon: '⏸️' };
  }
  if (type.includes('period')) {
    return { label: t('hockey.matches.eventPeriod', 'Period'), icon: '⏱️' };
  }
  return { label: eventType, icon: '•' };
}

function formatGoalDetail(event: HockeyMatchEventDto, activePlayerLabels: Map<string, string>): string {
  const scorer: string = event.matchActivePlayerId ? activePlayerLabels.get(event.matchActivePlayerId) ?? '' : '';
  const assists: string[] = [event.primaryAssistActivePlayerId, event.secondaryAssistActivePlayerId]
    .map((id) => (id ? activePlayerLabels.get(id) ?? '' : ''))
    .filter(Boolean);
  if (!scorer) {
    return '';
  }
  return assists.length > 0 ? `${scorer} (${assists.join(', ')})` : scorer;
}

function formatPenaltyDetail(
  event: HockeyMatchEventDto,
  activePlayerLabels: Map<string, string>,
  t: (key: string, fallback: string, options?: Record<string, unknown>) => string,
): string {
  const parts: string[] = [];
  const player: string = event.matchActivePlayerId ? activePlayerLabels.get(event.matchActivePlayerId) ?? '' : '';
  if (player) {
    parts.push(player);
  }
  if (event.penaltyOffence) {
    parts.push(t(`hockey.matches.penaltyOffences.${event.penaltyOffence}`, event.penaltyOffence));
  }
  if (typeof event.penaltyMinutes === 'number' && event.penaltyMinutes > 0) {
    parts.push(t('hockeyPage.penaltyMinutesShort', '{{count}} min', { count: event.penaltyMinutes }));
  }
  return parts.join(' · ');
}

function formatEventDetail(
  event: HockeyMatchEventDto,
  activePlayerLabels: Map<string, string>,
  t: (key: string, fallback: string, options?: Record<string, unknown>) => string,
): string {
  const type = event.eventType.toLowerCase();
  if (type === 'goal') {
    return formatGoalDetail(event, activePlayerLabels);
  }
  if (type === 'penalty') {
    return formatPenaltyDetail(event, activePlayerLabels, t);
  }
  const description = event.description;
  if (!description) {
    return '';
  }

  if (type.includes('shot')) {
    if (description === 'Saved' || !SHOT_RESULT_SET.has(description)) {
      return '';
    }
    return t(`hockey.matches.shotResults.${description}`, description);
  }

  if (type.includes('faceoff')) {
    const [zone, spot] = description.split(' ');
    const parts: string[] = [];
    if (zone) {
      parts.push(t(`hockey.matches.faceoffZones.${zone}`, zone));
    }
    if (spot) {
      parts.push(t(`hockey.matches.faceoffSpots.${spot}`, spot));
    }
    return parts.join(' · ');
  }

  if (type.includes('stoppage')) {
    if (description === 'Offside') {
      return '';
    }
    return t(`hockey.matches.stoppageReasons.${description}`, description);
  }

  if (type.includes('period')) {
    return t(`hockey.matches.periodActions.${description}`, description);
  }

  return '';
}

function periodTitle(periodNumber: number, overtimeLabel: string, shootoutLabel: string, regularLabel: string): string {
  if (periodNumber === 4) {
    return overtimeLabel;
  }
  if (periodNumber === 5) {
    return shootoutLabel;
  }
  return regularLabel;
}

function EventRow({
  row,
  teamName,
  activePlayerLabels,
  canRemove,
  busy,
  onDeleteEvent,
  onDeleteEvents,
}: {
  row: HistoryRow;
  teamName: string;
  activePlayerLabels: Map<string, string>;
  canRemove: boolean;
  busy: boolean;
  onDeleteEvent?: (event: HockeyMatchEventDto) => void;
  onDeleteEvents?: (events: HockeyMatchEventDto[]) => void;
}) {
  const { t } = useTranslation();
  const { event, members } = row;
  const meta = eventLabel(event.eventType, event.description, t);
  const detail = formatEventDetail(event, activePlayerLabels, t);
  const isBulk: boolean = members.length > 1;

  const handleDelete = (): void => {
    if (isBulk && onDeleteEvents) {
      onDeleteEvents(members);
      return;
    }
    onDeleteEvent?.(event);
  };

  return (
    <li className={`events-history__row${isBulk ? ' events-history__row--bulk' : ''}`}>
      <span className="events-history__icon" aria-hidden="true">{meta.icon}</span>
      <span className="events-history__body">
        <strong>{meta.label}</strong>
        {isBulk && <span className="events-history__count"> ×{members.length}</span>}
        {teamName ? ` ${teamName}` : ''}
        {' · '}
        {formatHockeyClock(event.gameTimeSeconds)}
        {detail ? ` · ${detail}` : ''}
      </span>
      {canRemove && (onDeleteEvent || onDeleteEvents) && (
        <button
          type="button"
          className="events-history__delete"
          disabled={busy}
          onClick={handleDelete}
          aria-label={isBulk
            ? t('hockey.matches.manage.deleteBulkSaves', 'Delete {{count}} saves', { count: members.length })
            : t('common.delete', 'Delete')}
        >
          ×
        </button>
      )}
    </li>
  );
}

function LiveMatchEventsHistory({
  events,
  teamNamesByMatchTeamId,
  activePlayerLabels,
  onDeleteEvent,
  onDeleteEvents,
  canDelete = true,
  busy = false,
}: LiveMatchEventsHistoryProps) {
  const { t } = useTranslation();
  const [expandedPeriods, setExpandedPeriods] = useState<Set<number>>(new Set());

  const periodGroups = useMemo(() => {
    const grouped = new Map<number, HockeyMatchEventDto[]>();
    for (const event of events) {
      const list = grouped.get(event.periodNumber) ?? [];
      list.push(event);
      grouped.set(event.periodNumber, list);
    }
    return [...grouped.entries()]
      .sort((left, right) => right[0] - left[0])
      .map(([periodNumber, periodEvents]) => ({
        periodNumber,
        eventCount: periodEvents.length,
        rows: groupSaves([...periodEvents].reverse()),
      }));
  }, [events]);

  const togglePeriod = (periodNumber: number): void => {
    setExpandedPeriods((current) => {
      const next = new Set(current);
      if (next.has(periodNumber)) {
        next.delete(periodNumber);
      } else {
        next.add(periodNumber);
      }
      return next;
    });
  };

  if (periodGroups.length === 0) {
    return (
      <div className="events-history">
        <h3 className="events-history__title">{t('hockey.matches.eventHistory', 'EVENT HISTORY')}</h3>
        <div className="events-history__empty">{t('hockey.matches.noEvents', 'No events yet')}</div>
      </div>
    );
  }

  return (
    <div className="events-history">
      <h3 className="events-history__title">{t('hockey.matches.eventHistory', 'EVENT HISTORY')}</h3>
      <div className="events-history__periods">
        {periodGroups.map((group) => {
          const isExpanded = expandedPeriods.has(group.periodNumber);
          const visible = isExpanded ? group.rows : group.rows.slice(0, PERIOD_PREVIEW_COUNT);
          const hiddenCount = group.rows.length - visible.length;
          return (
            <section key={group.periodNumber} className="events-history__period">
              <header className="events-history__period-header">
                <h4 className="events-history__period-title">
                  {periodTitle(
                    group.periodNumber,
                    t('hockey.matches.overtime', 'Overtime'),
                    t('hockey.matches.shootout', 'Shootout'),
                    t('hockey.matches.periodN', 'Period {{number}}', { number: group.periodNumber }),
                  )}
                </h4>
                <span className="events-history__period-count">
                  {t('hockey.matches.eventCount', '{{count}} events', { count: group.eventCount })}
                </span>
              </header>
              <ul className="events-history__list">
                {visible.map((row) => {
                  const { event } = row;
                  const type = event.eventType.toLowerCase();
                  const canRemove = canDelete && (type.includes('goal') || type.includes('penalty') || type.includes('shot'));
                  const teamName = event.matchTeamId ? teamNamesByMatchTeamId.get(event.matchTeamId) ?? '' : '';
                  return (
                    <EventRow
                      key={event.id}
                      row={row}
                      teamName={teamName}
                      activePlayerLabels={activePlayerLabels}
                      canRemove={canRemove}
                      busy={busy}
                      onDeleteEvent={onDeleteEvent}
                      onDeleteEvents={onDeleteEvents}
                    />
                  );
                })}
              </ul>
              {group.rows.length > PERIOD_PREVIEW_COUNT && (
                <button
                  type="button"
                  className="events-history__expand"
                  onClick={() => togglePeriod(group.periodNumber)}
                >
                  {isExpanded
                    ? t('hockey.matches.showLatestEvents', 'Show latest {{count}}', { count: PERIOD_PREVIEW_COUNT })
                    : t('hockey.matches.showAllPeriodEvents', 'Show all {{count}} events', { count: group.rows.length })}
                </button>
              )}
              {!isExpanded && hiddenCount > 0 && (
                <p className="events-history__more-hint">
                  {t('hockey.matches.morePeriodEvents', '{{count}} earlier events in this period', { count: hiddenCount })}
                </p>
              )}
            </section>
          );
        })}
      </div>
    </div>
  );
}

export default LiveMatchEventsHistory;
