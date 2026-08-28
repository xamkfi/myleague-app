import { useTranslation } from 'react-i18next';
import type { HockeyMatchDto, HockeyMatchEventDto } from '../../types/hockey/hockeyTypes';
import { hockeyHomeTeam } from '../../types/hockey/hockeyTypes';
import { formatHockeyClock, hockeyEventPlayerLabel } from '../../utils/hockeyLookups';
import {
  hockeyPublicEventDetail,
  hockeyPublicEventLabel,
  isPublicHockeyEvent,
} from '../../utils/hockeyEventDisplay';

interface HockeyMatchEventsProps {
  match: HockeyMatchDto;
  homeName: string;
  awayName: string;
  playerNames: Map<string, string>;
}

function eventRowClass(typeClass: string): string {
  return typeClass ? `event-row ${typeClass}` : 'event-row';
}

function HockeyMatchEvents({ match, homeName, awayName, playerNames }: HockeyMatchEventsProps) {
  const { t } = useTranslation();
  const home = hockeyHomeTeam(match);
  const events = [...match.events]
    .filter((eventItem) => isPublicHockeyEvent(eventItem))
    .sort((a, b) => {
      if (a.periodNumber !== b.periodNumber) {
        return a.periodNumber - b.periodNumber;
      }
      return a.gameTimeSeconds - b.gameTimeSeconds;
    });

  return (
    <div className="events-section">
      <h3>{t('hockeyPage.eventLog', 'Event log')}</h3>
      <div className="match-events">
        {events.length === 0 ? (
          <p>{t('hockeyPage.noEvents', 'No events recorded yet')}</p>
        ) : (
          events.map((eventItem: HockeyMatchEventDto) => {
            const details = hockeyEventPlayerLabel(match, eventItem, playerNames);
            const extra = hockeyPublicEventDetail(eventItem, t);
            const isHomeEvent = Boolean(home && eventItem.matchTeamId === home.id);
            const meta = hockeyPublicEventLabel(eventItem, t);
            return (
              <div key={eventItem.id} className={eventRowClass(meta.typeClass)}>
                <span className="event-time">
                  P{eventItem.periodNumber} {formatHockeyClock(eventItem.gameTimeSeconds)}
                </span>
                <span className={`event-type-badge ${meta.typeClass}`} title={meta.label}>
                  <span className="badge-letter">{meta.badge}</span>
                </span>
                {eventItem.matchTeamId && (
                  <span className={`event-team-short ${isHomeEvent ? 'home-team' : 'away-team'}`}>
                    {isHomeEvent ? homeName : awayName}
                  </span>
                )}
                <span className="event-details">
                  {meta.label}
                  {details ? ` · ${details}` : ''}
                  {extra ? ` · ${extra}` : ''}
                </span>
              </div>
            );
          })
        )}
      </div>
    </div>
  );
}

export default HockeyMatchEvents;
