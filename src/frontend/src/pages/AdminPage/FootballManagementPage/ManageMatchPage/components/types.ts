import type {
  FootballCardType,
  FootballGoalType,
  FootballMatchDto,
  FootballTeam,
} from '../../../../../types/football/footballTypes';
import type { FootballPlayerDto } from '../../../../../api/football/footballPlayerService';
import type { LiveMatchState } from '../hooks/useLiveMatchState';
import type { FootballDomainEventDto } from '../../../../../api/football/footballMatchEventService';

export interface LiveMatchModalProps {
  match: FootballMatchDto;
  isOpen: boolean;
  onClose: () => void;
  onCompleteLive?: (matchId: string, updatedMatch?: FootballMatchDto) => void;
  onGoLive?: (matchId: string, updatedMatch?: FootballMatchDto) => void;
  liveState?: LiveMatchState;
  onStateUpdate?: (updates: Partial<LiveMatchState>) => void;
  onMatchUpdated?: (updatedMatch: FootballMatchDto) => void;
}

export interface StateUpdate {
  currentScore?: { home: number; away: number };
  clock?: LocalClock;
  [key: string]: unknown;
}

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

export interface CardEventData {
  MatchId: string;
  TeamId: string;
  PlayerId: string;
  CardType: FootballCardType | string;
  PeriodNumber: number;
  EventTime: string;
}

export interface SubstitutionEventData {
  MatchId: string;
  TeamId: string;
  PlayerOffId: string;
  PlayerOnId: string;
  PeriodNumber: number;
  EventTime: string;
}

export interface MatchLifecycleEventData {
  MatchId: string;
}

export interface EventGroup {
  /** Stable React key — derived from event ids so reorders are well-behaved. */
  key: string;
  /** Event used to render the row (player name, time, team, etc.). */
  representative: ProcessedEvent;
  /** Underlying events; length === 1 for normal rows. */
  events: ProcessedEvent[];
}

export interface ProcessedEvent {
  id: string;
  type: 'goal' | 'card' | 'substitution';
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
  description?: string;
  goalType?: FootballGoalType | number | string | null;
  cardType?: FootballCardType | number | string | null;
  playerOffId?: string;
  playerOffName?: string;
  playerOnId?: string;
  playerOnName?: string;
}

export interface LocalClock {
  period: number;
  minutes: number;
  seconds: number;
  isRunning: boolean;
}

export interface GoalForm {
  teamId: string;
  playerId: string;
  assisterId: string;
  timeMinutes: number;
  timeSeconds: number;
  goalType: FootballGoalType | null;
}

export interface CardForm {
  teamId: string;
  playerId: string;
  cardType: FootballCardType | null;
  description: string;
  timeMinutes: number;
  timeSeconds: number;
}

export interface SubstitutionForm {
  teamId: string;
  playerOffId: string;
  playerOnId: string;
  description: string;
  timeMinutes: number;
  timeSeconds: number;
}

export interface LiveMatchContextProps {
  currentMatch: FootballMatchDto;
  homeTeam: FootballTeam | null;
  awayTeam: FootballTeam | null;
  homePlayers: FootballPlayerDto[];
  awayPlayers: FootballPlayerDto[];
  loading: boolean;
  error: string | null;
  setError: (error: string | null) => void;
  matchEvents: FootballDomainEventDto[];
  allEvents: ProcessedEvent[];
  localClock: LocalClock;
  currentScore: { home: number; away: number };
  currentTimerElapsedTime: number;
  getCurrentTimeFromTimer: (() => string) | null;
  startedPeriods: Set<number>;
  endedPeriods: Set<number>;
  nextPeriodToStart: number;
  periodLoading: Record<number, boolean>;
  handleStartMatch: () => Promise<void>;
  handlePeriodControlClick: () => void;
  handleCompleteLive: () => Promise<void>;
  recordGoal: () => Promise<void>;
  recordCard: () => Promise<void>;
  recordSubstitution: () => Promise<void>;
  goalForm: GoalForm;
  setGoalForm: React.Dispatch<React.SetStateAction<GoalForm>>;
  cardForm: CardForm;
  setCardForm: React.Dispatch<React.SetStateAction<CardForm>>;
  substitutionForm: SubstitutionForm;
  setSubstitutionForm: React.Dispatch<React.SetStateAction<SubstitutionForm>>;
  showGoalForm: boolean;
  setShowGoalForm: (show: boolean) => void;
  showCardForm: boolean;
  setShowCardForm: (show: boolean) => void;
  showSubstitutionForm: boolean;
  setShowSubstitutionForm: (show: boolean) => void;
  getPlayersForTeam: (teamId: string) => FootballPlayerDto[];
  getPlayerNameById: (playerId: string | undefined | null) => string;
  formatTime: (minutes: number, seconds: number) => string;
  formatEventTime: (timeInSeconds: number) => string;
  canEndPeriod: () => boolean;
  getPeriodStatus: () => string;
  getPeriodControlButtonText: () => string;
  isInExtraTime: () => boolean;
  isInPenaltyShootout: () => boolean;
}
