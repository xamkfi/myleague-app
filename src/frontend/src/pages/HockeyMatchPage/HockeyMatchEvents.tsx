import { useTranslation } from 'react-i18next';
import type { HockeyMatchDto, HockeyMatchEventDto, HockeyTeamDto } from '../../types/hockey/hockeyTypes';
import { hockeyHomeTeam } from '../../types/hockey/hockeyTypes';
import { resolveHockeyCareerPlayerId } from '../../utils/hockeyLookups';
import {
  hockeyGoalStrengthAbbreviation,
  isHockeyGoalEvent,
  isHockeyPenaltyEvent,
} from '../../utils/hockeyEventDisplay';
import { formatEventTimeMmSs, formatMatchEventTime } from '../../utils/matchEventFormat';
import { getPlayerPath } from '../../utils/sportRoutes';
import MatchEventTimeline, {
  type TimelineEvent,
  type TimelinePlayer,
} from '../../components/match/MatchEventTimeline';

interface HockeyMatchEventsProps {
  match: HockeyMatchDto;
  teams: HockeyTeamDto[];
  homeName: string;
  awayName: string;
  playerNames: Map<string, string>;
}

type HockeyPeriodKind = 'regular' | 'overtime' | 'shootout';

const REGULATION_PERIODS: number = 3;

function HockeyMatchEvents({ match, teams, homeName, awayName, playerNames }: HockeyMatchEventsProps) {
  const { t } = useTranslation();
  const home = hockeyHomeTeam(match);

  const periodKind = (period: number): HockeyPeriodKind => {
    const periodType: string | undefined = match.periodScores.find((row) => row.periodNumber === period)?.periodType;
    if (periodType === 'Shootout') return 'shootout';
    if (periodType === 'Overtime') return 'overtime';
    if (periodType === 'RegularPeriod') return 'regular';
    return period > REGULATION_PERIODS ? 'overtime' : 'regular';
  };

  const periodTitle = (period: number): string => {
    const kind: HockeyPeriodKind = periodKind(period);
    if (kind === 'shootout') return t('matchPage.events.shootoutName');
    if (kind === 'overtime') return t('matchPage.events.overtimeName');
    return t('matchPage.events.periodName', { number: period });
  };

  const eventClock = (period: number, timeInSeconds: number): string => {
    const kind: HockeyPeriodKind = periodKind(period);
    const clock: string = formatEventTimeMmSs(timeInSeconds);
    if (kind === 'shootout') return `${t('matchPage.events.shootoutShort')} - ${clock}`;
    if (kind === 'overtime') return `${t('matchPage.events.overtimeShort')} - ${clock}`;
    return formatMatchEventTime(period, timeInSeconds);
  };

  const toPlayer = (matchActivePlayerId: string | null | undefined): TimelinePlayer | undefined => {
    if (!matchActivePlayerId) return undefined;
    for (const side of match.matchTeams) {
      const active = side.activePlayers.find((row) => row.id === matchActivePlayerId);
      if (!active) continue;
      const careerPlayerId: string | undefined = resolveHockeyCareerPlayerId(match, teams, matchActivePlayerId);
      return {
        name: playerNames.get(active.teamPlayerId) ?? '',
        jerseyNumber: Number.isFinite(active.jerseyNumber) ? active.jerseyNumber : undefined,
        href: careerPlayerId ? getPlayerPath('hockey', careerPlayerId) : undefined,
      };
    }
    return undefined;
  };

  const penaltyLabel = (event: HockeyMatchEventDto): string | undefined => {
    const parts: string[] = [];
    if (event.penaltyOffence) {
      parts.push(t(`hockey.matches.penaltyOffences.${event.penaltyOffence}`, event.penaltyOffence).toLowerCase());
    }
    if (typeof event.penaltyMinutes === 'number' && event.penaltyMinutes > 0) {
      parts.push(t('hockeyPage.penaltyMinutesShort', '{{count}} min', { count: event.penaltyMinutes }));
    }
    return parts.length > 0 ? parts.join(', ') : undefined;
  };

  const events: TimelineEvent[] = match.events
    .filter((event) => isHockeyGoalEvent(event) || isHockeyPenaltyEvent(event))
    .map((event): TimelineEvent => {
      const side: 'home' | 'away' = home && event.matchTeamId === home.id ? 'home' : 'away';
      if (isHockeyGoalEvent(event)) {
        const abbreviation: string = hockeyGoalStrengthAbbreviation(event.goalStrength);
        return {
          key: event.id,
          kind: 'goal',
          periodNumber: event.periodNumber,
          timeInSeconds: event.gameTimeSeconds,
          side,
          player: toPlayer(event.matchActivePlayerId),
          assists: [
            toPlayer(event.primaryAssistActivePlayerId),
            toPlayer(event.secondaryAssistActivePlayerId),
          ].filter((assist): assist is TimelinePlayer => assist !== undefined),
          goalBadge: abbreviation
            ? {
                abbreviation,
                label: t(`hockey.matches.goalStrengths.${event.goalStrength}`, event.goalStrength ?? ''),
              }
            : null,
        };
      }
      return {
        key: event.id,
        kind: 'penalty',
        periodNumber: event.periodNumber,
        timeInSeconds: event.gameTimeSeconds,
        side,
        player: toPlayer(event.matchActivePlayerId),
        penaltyLabel: penaltyLabel(event),
        description: event.description ?? undefined,
      };
    });

  return (
    <div className="summary-events-section">
      <MatchEventTimeline
        events={events}
        home={{ name: homeName, logo: teams.find((team) => team.id === match.homeTeamId)?.logoUrl ?? null }}
        away={{ name: awayName, logo: teams.find((team) => team.id === match.awayTeamId)?.logoUrl ?? null }}
        periodTitle={periodTitle}
        eventClock={eventClock}
        emptyMessage={t('hockeyPage.noEvents', 'No events recorded yet')}
      />
    </div>
  );
}

export default HockeyMatchEvents;
