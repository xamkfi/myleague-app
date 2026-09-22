import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import type { HockeyMatchDto, HockeyMatchEventDto, HockeyTeamDto } from '../../types/hockey/hockeyTypes';
import { hockeyHomeTeam } from '../../types/hockey/hockeyTypes';
import {
  formatHockeyClock,
  hockeyActivePlayerLabel,
  resolveHockeyCareerPlayerId,
} from '../../utils/hockeyLookups';
import {
  hockeyPublicEventDetail,
  hockeyPublicEventLabel,
  isPublicHockeyEvent,
} from '../../utils/hockeyEventDisplay';
import { getPlayerPath } from '../../utils/sportRoutes';

interface HockeyMatchEventsProps {
  match: HockeyMatchDto;
  teams: HockeyTeamDto[];
  homeName: string;
  awayName: string;
  playerNames: Map<string, string>;
}

function eventRowClass(typeClass: string): string {
  return typeClass ? `event-row ${typeClass}` : 'event-row';
}

function EventPlayerLink({
  match,
  teams,
  matchActivePlayerId,
  playerNames,
}: {
  match: HockeyMatchDto;
  teams: HockeyTeamDto[];
  matchActivePlayerId: string | null | undefined;
  playerNames: Map<string, string>;
}) {
  if (!matchActivePlayerId) {
    return null;
  }

  const label = hockeyActivePlayerLabel(match, matchActivePlayerId, playerNames);
  if (!label) {
    return null;
  }

  const playerId = resolveHockeyCareerPlayerId(match, teams, matchActivePlayerId);
  if (!playerId) {
    return <span>{label}</span>;
  }

  return (
    <Link className="event-player-link" to={getPlayerPath('hockey', playerId)}>
      {label}
    </Link>
  );
}

function HockeyMatchEvents({ match, teams, homeName, awayName, playerNames }: HockeyMatchEventsProps) {
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
                  {eventItem.matchActivePlayerId && (
                    <>
                      {' · '}
                      <EventPlayerLink
                        match={match}
                        teams={teams}
                        matchActivePlayerId={eventItem.matchActivePlayerId}
                        playerNames={playerNames}
                      />
                    </>
                  )}
                  {eventItem.losingActivePlayerId && (
                    <>
                      {' · '}
                      <EventPlayerLink
                        match={match}
                        teams={teams}
                        matchActivePlayerId={eventItem.losingActivePlayerId}
                        playerNames={playerNames}
                      />
                    </>
                  )}
                  {!eventItem.matchActivePlayerId && eventItem.description?.trim()
                    ? ` · ${eventItem.description.trim()}`
                    : ''}
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
