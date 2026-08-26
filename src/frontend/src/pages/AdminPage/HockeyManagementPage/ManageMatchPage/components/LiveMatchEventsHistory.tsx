import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import type { HockeyMatchEventDto } from '../../../../../types/hockey/hockeyTypes';
import { HOCKEY_SHOT_RESULTS } from '../../../../../types/hockey/hockeyTypes';
import { formatHockeyClock } from '../../../../../utils/hockeyLookups';
import './LiveMatchEventsHistory.scss';

interface LiveMatchEventsHistoryProps {
  events: HockeyMatchEventDto[];
  teamNamesByMatchTeamId: Map<string, string>;
  playerNames: Map<string, string>;
  onDeleteEvent?: (event: HockeyMatchEventDto) => void;
  canDelete?: boolean;
  busy?: boolean;
}

const SHOT_RESULT_SET: ReadonlySet<string> = new Set(HOCKEY_SHOT_RESULTS);
const PERIOD_PREVIEW_COUNT = 5;

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

function formatEventDetail(
  event: HockeyMatchEventDto,
  t: (key: string, fallback: string) => string,
): string {
  const type = event.eventType.toLowerCase();
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
  event,
  teamName,
  canRemove,
  busy,
  onDeleteEvent,
}: {
  event: HockeyMatchEventDto;
  teamName: string;
  canRemove: boolean;
  busy: boolean;
  onDeleteEvent?: (event: HockeyMatchEventDto) => void;
}) {
  const { t } = useTranslation();
  const meta = eventLabel(event.eventType, event.description, t);
  const detail = formatEventDetail(event, t);

  return (
    <li className="events-history__row">
      <span className="events-history__icon" aria-hidden="true">{meta.icon}</span>
      <span className="events-history__body">
        <strong>{meta.label}</strong>
        {teamName ? ` ${teamName}` : ''}
        {' · '}
        {formatHockeyClock(event.gameTimeSeconds)}
        {detail ? ` · ${detail}` : ''}
      </span>
      {canRemove && onDeleteEvent && (
        <button
          type="button"
          className="events-history__delete"
          disabled={busy}
          onClick={() => onDeleteEvent(event)}
          aria-label={t('common.delete', 'Delete')}
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
  onDeleteEvent,
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
        events: [...periodEvents].reverse(),
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
          const visible = isExpanded ? group.events : group.events.slice(0, PERIOD_PREVIEW_COUNT);
          const hiddenCount = group.events.length - visible.length;
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
                  {t('hockey.matches.eventCount', '{{count}} events', { count: group.events.length })}
                </span>
              </header>
              <ul className="events-history__list">
                {visible.map((event) => {
                  const type = event.eventType.toLowerCase();
                  const canRemove = canDelete && (type.includes('goal') || type.includes('penalty') || type.includes('shot'));
                  const teamName = event.matchTeamId ? teamNamesByMatchTeamId.get(event.matchTeamId) ?? '' : '';
                  return (
                    <EventRow
                      key={event.id}
                      event={event}
                      teamName={teamName}
                      canRemove={canRemove}
                      busy={busy}
                      onDeleteEvent={onDeleteEvent}
                    />
                  );
                })}
              </ul>
              {group.events.length > PERIOD_PREVIEW_COUNT && (
                <button
                  type="button"
                  className="events-history__expand"
                  onClick={() => togglePeriod(group.periodNumber)}
                >
                  {isExpanded
                    ? t('hockey.matches.showLatestEvents', 'Show latest {{count}}', { count: PERIOD_PREVIEW_COUNT })
                    : t('hockey.matches.showAllPeriodEvents', 'Show all {{count}} events', { count: group.events.length })}
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
