import type { FloorballMatchDto, FloorballTeam } from '../../../../../types/floorball/floorballTypes';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import type { LiveMatchState } from '../hooks/useLiveMatchState';
import type { FloorballDomainEventDto } from '../../../../../api/floorball/floorballMatchEventService';

export interface LiveMatchModalProps {
  match: FloorballMatchDto;
  isOpen: boolean;
  onClose: () => void;
  onCompleteLive?: (matchId: string, updatedMatch?: FloorballMatchDto) => void;
  onGoLive?: (matchId: string, updatedMatch?: FloorballMatchDto) => void;
  liveState?: LiveMatchState;
  onStateUpdate?: (updates: Partial<LiveMatchState>) => void;
  onMatchUpdated?: (updatedMatch: FloorballMatchDto) => void;
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

export interface ProcessedEvent {
  id: string;
  type: 'goal' | 'penalty' | 'save';
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

export interface LiveMatchContextProps {
  // Match data
  currentMatch: FloorballMatchDto;
  homeTeam: FloorballTeam | null;
  awayTeam: FloorballTeam | null;
  homePlayers: FloorballPlayerDto[];
  awayPlayers: FloorballPlayerDto[];
  
  // State
  loading: boolean;
  error: string | null;
  setError: (error: string | null) => void;
  matchEvents: FloorballDomainEventDto[];
  allEvents: ProcessedEvent[];
  
  // Clock and scores
  localClock: LocalClock;
  currentScore: { home: number; away: number };
  currentTimerElapsedTime: number;
  getCurrentTimeFromTimer: (() => string) | null;
  
  // Period management
  startedPeriods: Set<number>;
  endedPeriods: Set<number>;
  nextPeriodToStart: number;
  periodLoading: Record<number, boolean>;
  
  // Handlers
  handleStartMatch: () => Promise<void>;
  handlePeriodControlClick: () => void;
  handleCompleteLive: () => Promise<void>;
  recordGoal: () => Promise<void>;
  recordPenalty: () => Promise<void>;
  
  // Forms
  goalForm: GoalForm;
  setGoalForm: React.Dispatch<React.SetStateAction<GoalForm>>;
  penaltyForm: PenaltyForm;
  setPenaltyForm: React.Dispatch<React.SetStateAction<PenaltyForm>>;
  
  // Form visibility
  showGoalForm: boolean;
  setShowGoalForm: (show: boolean) => void;
  showPenaltyForm: boolean;
  setShowPenaltyForm: (show: boolean) => void;
  
  // Utility functions
  getPlayersForTeam: (teamId: string) => FloorballPlayerDto[];
  getPlayerNameById: (playerId: string | undefined | null) => string;
  formatTime: (minutes: number, seconds: number) => string;
  formatEventTime: (timeInSeconds: number) => string;
  
  // Period status functions
  canEndPeriod: () => boolean;
  getPeriodStatus: () => string;
  getPeriodControlButtonText: () => string;
  isInOvertime: () => boolean;
  isInShootout: () => boolean;
} 