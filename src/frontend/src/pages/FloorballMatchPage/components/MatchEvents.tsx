import { useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import type {
  FloorballMatchDto,
  FloorballTeamPlayer,
} from '../../../types/floorball/floorballTypes';
import { formatEventTimeMmSs, formatMatchEventTime } from '../../../utils/matchEventFormat';
import { getFloorballPeriodKind, type FloorballPeriodKind } from '../../../utils/floorballPeriod';
import { getFloorballGoalTypeInfo } from '../../../utils/floorballGoalType';
import { floorballTeamService } from '../../../api/floorball/floorballTeamService';
import { getPlayerPath } from '../../../utils/sportRoutes';
import MatchEventTimeline, {
  type TimelineEvent,
  type TimelinePlayer,
} from '../../../components/match/MatchEventTimeline';

interface MatchEventsProps {
  match: FloorballMatchDto;
}

export default function MatchEvents({ match }: MatchEventsProps) {
  const { t } = useTranslation();
  const numberOfPeriods: number = match.matchRules?.numberOfPeriods ?? 2;

  const periodKind = (period: number): FloorballPeriodKind =>
    getFloorballPeriodKind(period, numberOfPeriods, match.wentToOvertime, match.wentToShootout);

  const periodTitle = (period: number): string => {
    const kind: FloorballPeriodKind = periodKind(period);
    if (kind === 'shootout') return t('matchPage.events.shootoutName');
    if (kind === 'overtime') return t('matchPage.events.overtimeName');
    return t('matchPage.events.periodName', { number: period });
  };

  const eventClock = (period: number, timeInSeconds: number): string => {
    const kind: FloorballPeriodKind = periodKind(period);
    const clock: string = formatEventTimeMmSs(timeInSeconds);
    if (kind === 'shootout') return `${t('matchPage.events.shootoutShort')} - ${clock}`;
    if (kind === 'overtime') return `${t('matchPage.events.overtimeShort')} - ${clock}`;
    return formatMatchEventTime(period, timeInSeconds);
  };
  const [homeRoster, setHomeRoster] = useState<FloorballTeamPlayer[]>([]);
  const [awayRoster, setAwayRoster] = useState<FloorballTeamPlayer[]>([]);

  // Hae molempien joukkueiden rosterit, jotta saamme pelaajan numeron tapahtumariville.
  // Käytetään samaa palvelua kuin MatchLineups-komponentti.
  useEffect(() => {
    let cancelled: boolean = false;
    async function fetchRosters() {
      try {
        // Skip roster lookups for placeholder fixtures — there are no events to enrich either.
        if (!match.homeTeamId || !match.awayTeamId) {
          setHomeRoster([]);
          setAwayRoster([]);
          return;
        }

        const [homeResponse, awayResponse] = await Promise.all([
          floorballTeamService.getById(match.homeTeamId, match.competitionId),
          floorballTeamService.getById(match.awayTeamId, match.competitionId),
        ]);
        if (cancelled) return;
        setHomeRoster(homeResponse.roster ?? []);
        setAwayRoster(awayResponse.roster ?? []);
      } catch (err) {
        console.error('Failed to load team rosters for match events:', err);
      }
    }
    fetchRosters();
    return () => {
      cancelled = true;
    };
  }, [match.homeTeamId, match.awayTeamId, match.competitionId]);

  // Yksittäinen lookup-taulu kaikille pelaajille → paitanumero. Sama playerId voi olla
  // korkeintaan yhdessä rosterissa, joten yhdistäminen on turvallista.
  const jerseyByPlayerId: Map<string, number> = useMemo(() => {
    const map: Map<string, number> = new Map<string, number>();
    for (const player of [...homeRoster, ...awayRoster]) {
      if (player.playerId && typeof player.jerseyNumber === 'number') {
        map.set(player.playerId, player.jerseyNumber);
      }
    }
    return map;
  }, [homeRoster, awayRoster]);

  const toPlayer = (name: string | undefined, playerId: string | undefined): TimelinePlayer | undefined => {
    if (!name || name === 'Unknown Player') return undefined;
    return {
      name,
      jerseyNumber: playerId ? jerseyByPlayerId.get(playerId) : undefined,
      href: playerId ? getPlayerPath('floorball', playerId) : undefined,
    };
  };

  const sideOf = (teamId: string): 'home' | 'away' => (teamId === match.homeTeamId ? 'home' : 'away');

  const events: TimelineEvent[] = [
    ...match.goalEvents.map((goal): TimelineEvent => {
      const goalTypeInfo = getFloorballGoalTypeInfo(goal.goalType);
      return {
        key: `goal-${goal.id}`,
        kind: 'goal',
        periodNumber: goal.periodNumber,
        timeInSeconds: goal.timeInSeconds,
        side: sideOf(goal.teamId),
        player: toPlayer(goal.playerName, goal.playerId),
        assists: [
          toPlayer(goal.assisterName, goal.assisterId),
          toPlayer(goal.secondaryAssisterName, goal.secondaryAssisterId),
        ].filter((assist): assist is TimelinePlayer => assist !== undefined),
        goalBadge: goalTypeInfo ? { abbreviation: goalTypeInfo.abbreviation, label: goalTypeInfo.label } : null,
      };
    }),
    ...match.penaltyEvents.map((penalty): TimelineEvent => ({
      key: `penalty-${penalty.id}`,
      kind: 'penalty',
      periodNumber: penalty.periodNumber,
      timeInSeconds: penalty.timeInSeconds,
      side: sideOf(penalty.teamId),
      player: toPlayer(penalty.playerName, penalty.playerId) ?? { name: penalty.playerName || t('matchPage.events.unknownPlayer') },
      penaltyLabel: penalty.penaltyType ? penalty.penaltyType.toLowerCase() : undefined,
      description: penalty.description,
    })),
  ];

  return (
    <MatchEventTimeline
      events={events}
      home={{ name: match.homeTeamName ?? 'TBD', logo: match.homeTeamLogo }}
      away={{ name: match.awayTeamName ?? 'TBD', logo: match.awayTeamLogo }}
      periodTitle={periodTitle}
      eventClock={eventClock}
    />
  );
}
