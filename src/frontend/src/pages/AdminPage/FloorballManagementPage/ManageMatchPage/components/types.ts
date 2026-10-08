import type { FloorballGoalType } from '../../../../../types/floorball/floorballTypes';

/** SignalR payload for `FloorballPeriodStartedEvent`. */
export interface PeriodEventData {
  matchId: string;
  periodNumber: number;
  homeTeamScore: number;
  awayTeamScore: number;
  isLastRegularPeriod: boolean;
  occurredOn: string;
}

export interface GoalEventData {
  MatchId: string;
  TeamId: string;
  PlayerId: string;
  PeriodNumber: number;
  EventTime: string;
  HomeTeam: { Id: string; Name: string };
  AwayTeam: { Id: string; Name: string };
}

export interface PenaltyEventData {
  MatchId: string;
  EventTime: string;
  PenaltyType: string;
  TeamId: string;
  PlayerId: string;
  HomeTeam: { Id: string; Name: string };
  AwayTeam: { Id: string; Name: string };
}

/**
 * Save event data from SignalR (PascalCase fields)
 */
export interface SaveEventData {
  MatchId: string;
  TeamId: string;
  GoalieId: string;
  PeriodNumber: number;
  TimeInSeconds: number;
  IsOvertime: boolean;
  IsShootout: boolean;
}

/**
 * Visual grouping wrapper used by {@link LiveMatchEventsHistory}. Bulk-recorded
 * saves at the same (team, goalie, period, time) coordinate are collapsed into
 * a single row so the events list isn't flooded with N identical rows. Non-save
 * events (and single saves) are represented as a group of size 1.
 *
 * The full underlying {@link ProcessedEvent} list is preserved on the group so
 * the delete affordance can remove every save in a bulk batch in one action.
 */
export interface EventGroup {
  /** Stable React key — derived from event ids so reorders are well-behaved. */
  key: string;
  /** Event used to render the row (player name, time, team, etc.). */
  representative: ProcessedEvent;
  /** Underlying events; length === 1 for normal rows, > 1 for grouped saves. */
  events: ProcessedEvent[];
}

export type ProcessedEventType = 'goal' | 'penalty' | 'save';

export interface ProcessedEvent {
  id: string;
  type: ProcessedEventType;
  eventId?: string;
  teamId: string;
  teamName: string;
  teamShortName?: string;
  playerId?: string;
  playerName: string;
  assisterId?: string;
  assisterName?: string;
  periodNumber: number;
  timeInSeconds: number;
  timestamp: Date;
  wasInOvertime?: boolean;
  wasInShootout?: boolean;
  penaltyType?: string;
  penaltyMinutes?: number;
  description?: string;
  /**
   * Goal type for goal events. Null/undefined for non-goal events or for
   * goals recorded without an explicit type (treated as regular). Accepts the
   * string-name form (e.g. `"PenaltyShot"`) because the backend serializes the
   * enum as a string via `JsonStringEnumConverter`.
   */
  goalType?: FloorballGoalType | number | string | null;
}

export interface GoalForm {
  teamId: string;
  playerId: string;
  assisterId: string;
  timeMinutes: number;
  timeSeconds: number;
  /** Period the goal is recorded in; follows the entered time unless chosen by hand. */
  periodNumber: number;
  /**
   * Optional goal type. `null` means no explicit type was chosen by the
   * recorder and the backend will treat it as a regular goal.
   */
  goalType: FloorballGoalType | null;
}

export interface PenaltyForm {
  teamId: string;
  playerId: string;
  penaltyType: string;
  minutes: number;
  description: string;
  periodNumber: number;
  timeMinutes: number;
  timeSeconds: number;
}
