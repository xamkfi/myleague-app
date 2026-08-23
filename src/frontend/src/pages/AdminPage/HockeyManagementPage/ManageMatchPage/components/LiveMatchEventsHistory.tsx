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

function LiveMatchEventsHistory({
  events,
  teamNamesByMatchTeamId,
  onDeleteEvent,
  canDelete = true,
  busy = false,
}: LiveMatchEventsHistoryProps) {
  const { t } = useTranslation();
  const ordered = [...events].reverse();

  if (ordered.length === 0) {
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
      <ul className="events-history__list">
        {ordered.map((event) => {
          const meta = eventLabel(event.eventType, event.description, t);
          const detail = formatEventDetail(event, t);
          const type = event.eventType.toLowerCase();
          const canRemove = canDelete && (type.includes('goal') || type.includes('penalty') || type.includes('shot'));
          const teamName = event.matchTeamId ? teamNamesByMatchTeamId.get(event.matchTeamId) ?? '' : '';
          return (
            <li key={event.id} className="events-history__row">
              <span className="events-history__icon" aria-hidden="true">{meta.icon}</span>
              <span className="events-history__body">
                <strong>{meta.label}</strong>
                {teamName ? ` ${teamName}` : ''}
                {' · '}
                P{event.periodNumber} {formatHockeyClock(event.gameTimeSeconds)}
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
        })}
      </ul>
    </div>
  );
}

export default LiveMatchEventsHistory;
