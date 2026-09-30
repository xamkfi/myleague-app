import type { FloorballDomainEventDto } from '../../../../../api/floorball/floorballMatchEventService';
import type { ProcessedEvent } from '../components/types';

/**
 * Context needed to turn raw domain events into rows the live desk can render.
 */
export interface FloorballEventMappingContext {
  homeTeamId: string | null | undefined;
  homeTeamName?: string | null;
  awayTeamName?: string | null;
  homeTeamShortName?: string | null;
  awayTeamShortName?: string | null;
  getPlayerNameById: (playerId: string | undefined | null) => string;
  /** Fallbacks for missing team names / team penalties, already translated. */
  labels: {
    home: string;
    away: string;
    teamPenalty: string;
  };
}

type EventData = Record<string, unknown>;

/**
 * Domain event payloads arrive either PascalCase (SignalR / raw domain event) or camelCase
 * (System.Text.Json). Look the property up under both spellings.
 */
function pick<T>(data: EventData, pascal: string, camel: string): T | undefined {
  const value: unknown = data[pascal] ?? data[camel];
  return value === null ? undefined : (value as T | undefined);
}

function pickString(data: EventData, pascal: string, camel: string): string | undefined {
  const value: unknown = pick<unknown>(data, pascal, camel);
  return typeof value === 'string' && value.length > 0 ? value : undefined;
}

function pickNumber(data: EventData, pascal: string, camel: string): number | undefined {
  const value: unknown = pick<unknown>(data, pascal, camel);
  return typeof value === 'number' && Number.isFinite(value) ? value : undefined;
}

function pickBoolean(data: EventData, pascal: string, camel: string): boolean {
  return pick<unknown>(data, pascal, camel) === true;
}

/**
 * Parses `mm:ss` / `hh:mm:ss` into whole seconds. Returns 0 for anything else.
 */
export function parseClockToSeconds(clock: string | undefined): number {
  if (!clock) return 0;
  const parts: number[] = clock.split(':').map((p: string) => parseInt(p, 10) || 0);
  if (parts.length === 3) return parts[0] * 3600 + parts[1] * 60 + parts[2];
  if (parts.length === 2) return parts[0] * 60 + parts[1];
  return 0;
}

function resolveTimeInSeconds(data: EventData): number {
  const explicit: number | undefined = pickNumber(data, 'TimeInSeconds', 'timeInSeconds');
  if (explicit !== undefined && explicit > 0) return explicit;
  return parseClockToSeconds(pickString(data, 'EventTime', 'eventTime'));
}

function teamNames(ctx: FloorballEventMappingContext, teamId: string): { teamName: string; teamShortName?: string } {
  const isHome: boolean = teamId === ctx.homeTeamId;
  return {
    teamName: (isHome ? ctx.homeTeamName : ctx.awayTeamName) || (isHome ? ctx.labels.home : ctx.labels.away),
    teamShortName: (isHome ? ctx.homeTeamShortName : ctx.awayTeamShortName) ?? undefined,
  };
}

function mapGoal(event: FloorballDomainEventDto, ctx: FloorballEventMappingContext): ProcessedEvent {
  const data: EventData = event.data;
  const teamId: string = pickString(data, 'TeamId', 'teamId') ?? '';
  const playerId: string = pickString(data, 'PlayerId', 'playerId') ?? '';
  const periodNumber: number = pickNumber(data, 'PeriodNumber', 'periodNumber') ?? 1;
  const timeInSeconds: number = resolveTimeInSeconds(data);
  const assisterId: string | undefined = pickString(data, 'AssisterId', 'assisterId');
  const goalType: unknown = pick<unknown>(data, 'GoalType', 'goalType');

  return {
    id: `goal-${teamId}-${playerId}-${periodNumber}-${timeInSeconds}`,
    type: 'goal',
    eventId: pickString(data, 'EventId', 'eventId') ?? '',
    teamId,
    ...teamNames(ctx, teamId),
    playerId,
    playerName: ctx.getPlayerNameById(playerId),
    assisterId,
    assisterName: assisterId ? ctx.getPlayerNameById(assisterId) : undefined,
    periodNumber,
    timeInSeconds,
    timestamp: new Date(event.occurredOn),
    wasInOvertime: pickBoolean(data, 'IsOvertime', 'isOvertime'),
    wasInShootout: pickBoolean(data, 'IsShootout', 'isShootout'),
    goalType: typeof goalType === 'number' || typeof goalType === 'string' ? goalType : null,
  };
}

function mapPenalty(event: FloorballDomainEventDto, ctx: FloorballEventMappingContext): ProcessedEvent {
  const data: EventData = event.data;
  const teamId: string = pickString(data, 'TeamId', 'teamId') ?? '';
  const playerId: string | undefined = pickString(data, 'PlayerId', 'playerId');
  const periodNumber: number = pickNumber(data, 'PeriodNumber', 'periodNumber') ?? 1;
  const timeInSeconds: number = resolveTimeInSeconds(data);

  return {
    id: `penalty-${teamId}-${playerId ?? 'team'}-${periodNumber}-${timeInSeconds}`,
    type: 'penalty',
    eventId: pickString(data, 'EventId', 'eventId') ?? '',
    teamId,
    ...teamNames(ctx, teamId),
    playerId,
    playerName: playerId ? ctx.getPlayerNameById(playerId) : ctx.labels.teamPenalty,
    periodNumber,
    timeInSeconds,
    timestamp: new Date(event.occurredOn),
    penaltyType: pickString(data, 'PenaltyType', 'penaltyType') ?? '',
    penaltyMinutes: pickNumber(data, 'Minutes', 'minutes') ?? 0,
    description: pickString(data, 'Description', 'description') ?? '',
  };
}

function mapSave(event: FloorballDomainEventDto, ctx: FloorballEventMappingContext): ProcessedEvent {
  const data: EventData = event.data;
  const teamId: string = pickString(data, 'TeamId', 'teamId') ?? '';
  const goalieId: string = pickString(data, 'GoalieId', 'goalieId') ?? '';
  const periodNumber: number = pickNumber(data, 'PeriodNumber', 'periodNumber') ?? 0;
  const timeInSeconds: number = pickNumber(data, 'TimeInSeconds', 'timeInSeconds') ?? 0;

  return {
    id: `save-${teamId}-${goalieId}-${periodNumber}-${timeInSeconds}`,
    type: 'save',
    eventId: pickString(data, 'EventId', 'eventId') ?? '',
    teamId,
    ...teamNames(ctx, teamId),
    playerId: goalieId,
    playerName: ctx.getPlayerNameById(goalieId),
    periodNumber,
    timeInSeconds,
    timestamp: new Date(event.occurredOn),
    wasInOvertime: pickBoolean(data, 'IsOvertime', 'wasInOvertime'),
    wasInShootout: pickBoolean(data, 'IsShootout', 'wasInShootout'),
  };
}

/**
 * Maps the flat domain-event stream of a match into display rows, most recent first.
 * Unknown event types are dropped.
 */
export function mapFloorballDomainEvents(
  events: readonly FloorballDomainEventDto[] | null | undefined,
  ctx: FloorballEventMappingContext,
): ProcessedEvent[] {
  if (!events) return [];

  const mapped: ProcessedEvent[] = [];
  for (const event of events) {
    if (!event || !event.data) continue;
    switch (event.eventType) {
      case 'FloorballGoalScoredEvent':
        mapped.push(mapGoal(event, ctx));
        break;
      case 'FloorballPenaltyAssignedEvent':
        mapped.push(mapPenalty(event, ctx));
        break;
      case 'FloorballSaveEvent':
        mapped.push(mapSave(event, ctx));
        break;
      default:
        break;
    }
  }

  return mapped.sort((a, b) =>
    a.periodNumber !== b.periodNumber ? b.periodNumber - a.periodNumber : b.timeInSeconds - a.timeInSeconds,
  );
}
