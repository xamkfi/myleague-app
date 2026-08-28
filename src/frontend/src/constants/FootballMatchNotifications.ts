/**
 * SignalR event names for football match notifications.
 * Must match src/backend/Application/Constants/FootballNotificationEvents.cs
 */
export const FOOTBALL_MATCH_NOTIFICATION_EVENTS = {
  GOAL_SCORED: 'FootballGoalScored',
  CARD_ASSIGNED: 'FootballCardAssigned',
  SUBSTITUTION_RECORDED: 'FootballSubstitutionRecorded',
  MATCH_STARTED: 'FootballMatchStarted',
  MATCH_COMPLETED: 'FootballMatchCompleted',
  MATCH_REOPENED: 'FootballMatchReopened',
  MATCH_CREATED: 'FootballMatchCreated',
} as const;
