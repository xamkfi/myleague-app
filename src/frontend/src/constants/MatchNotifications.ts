/**
 * SignalR event names used by the backend for match notifications
 * These constants should match the event names defined in the backend
 */
export const MATCH_NOTIFICATION_EVENTS = {
  GOAL_SCORED: 'FloorballGoalScored',
  PENALTY_ASSIGNED: 'FloorballPenaltyAssigned',
  SAVE_RECORDED: 'FloorballSaveRecorded',
  MATCH_STARTED: 'FloorballMatchStarted',
  MATCH_COMPLETED: 'FloorballMatchCompleted',
} as const;

