export const HOCKEY_POSITIONS = [
  'Goalie',
  'Defenseman',
  'Center',
  'LeftWing',
  'RightWing',
] as const;
export type HockeyPosition = (typeof HOCKEY_POSITIONS)[number];

export const HOCKEY_PLAYOFF_ROUNDS = [
  'Qualification',
  'RoundOf16',
  'QuarterFinal',
  'SemiFinal',
  'BronzeGame',
  'Final',
  'PlacementGame',
] as const;
export type HockeyPlayoffRound = (typeof HOCKEY_PLAYOFF_ROUNDS)[number];

export const HOCKEY_TEAM_SLOTS = ['Home', 'Away', 'Neutral'] as const;
export type HockeyTeamSlot = (typeof HOCKEY_TEAM_SLOTS)[number];

export const HOCKEY_TEAM_CATEGORIES = ['Adult', 'Youth', 'Women'] as const;
export type HockeyTeamCategory = (typeof HOCKEY_TEAM_CATEGORIES)[number];

export const HOCKEY_MATCH_STATUSES = [
  'Scheduled',
  'Warmup',
  'InProgress',
  'Intermission',
  'Overtime',
  'Shootout',
  'Finished',
  'Cancelled',
  'Postponed',
  'Suspended',
  'Forfeit',
] as const;
export type HockeyMatchStatus = (typeof HOCKEY_MATCH_STATUSES)[number];

export const LIVE_HOCKEY_STATUSES: HockeyMatchStatus[] = [
  'Warmup',
  'InProgress',
  'Intermission',
  'Overtime',
  'Shootout',
];

export const FINISHED_HOCKEY_STATUSES: HockeyMatchStatus[] = ['Finished', 'Forfeit'];

export const HOCKEY_MATCH_TYPES = [
  'League',
  'TournamentGroup',
  'TournamentPlayoff',
  'Friendly',
  'Scrimmage',
  'Practice',
  'Exhibition',
] as const;
export type HockeyMatchType = (typeof HOCKEY_MATCH_TYPES)[number];

export const HOCKEY_OFFICIAL_ROLES = [
  'Referee',
  'Linesperson',
  'Scorekeeper',
  'Timekeeper',
  'GoalJudge',
  'GameSupervisor',
] as const;
export type HockeyOfficialRole = (typeof HOCKEY_OFFICIAL_ROLES)[number];

export const HOCKEY_GOAL_STRENGTHS = [
  'EvenStrength',
  'PowerPlayOneMan',
  'PowerPlayTwoMan',
  'ShortHandedOneMan',
  'ShortHandedTwoMan',
  'PenaltyShot',
  'EmptyNet',
  'OwnGoal',
] as const;
export type HockeyGoalStrength = (typeof HOCKEY_GOAL_STRENGTHS)[number];

export const HOCKEY_PENALTY_SEVERITIES = [
  'Minor',
  'DoubleMinor',
  'Major',
  'Misconduct',
  'GameMisconduct',
  'MatchPenalty',
  'PenaltyShot',
  'BenchMinor',
] as const;
export type HockeyPenaltySeverity = (typeof HOCKEY_PENALTY_SEVERITIES)[number];

export const HOCKEY_PENALTY_OFFENCES = [
  'Tripping',
  'Hooking',
  'Holding',
  'Interference',
  'Slashing',
  'HighSticking',
  'CrossChecking',
  'Boarding',
  'Charging',
  'CheckingFromBehind',
  'Elbowing',
  'Roughing',
  'Fighting',
  'DelayOfGame',
  'TooManyMen',
  'UnsportsmanlikeConduct',
  'Other',
] as const;
export type HockeyPenaltyOffence = (typeof HOCKEY_PENALTY_OFFENCES)[number];

export const HOCKEY_SHOT_RESULTS = [
  'Saved',
  'Goal',
  'Missed',
  'Blocked',
  'Post',
] as const;
export type HockeyShotResult = (typeof HOCKEY_SHOT_RESULTS)[number];

export const HOCKEY_PERIOD_TYPES = ['RegularPeriod', 'Overtime', 'Shootout'] as const;
export type HockeyPeriodType = (typeof HOCKEY_PERIOD_TYPES)[number];

export const HOCKEY_PERIOD_ACTIONS = [
  'PeriodStarted',
  'PeriodEnded',
  'IntermissionStarted',
  'OvertimeStarted',
  'ShootoutStarted',
] as const;
export type HockeyPeriodAction = (typeof HOCKEY_PERIOD_ACTIONS)[number];

export const HOCKEY_SHOOTS = ['Unknown', 'Left', 'Right'] as const;
export type HockeyShoots = (typeof HOCKEY_SHOOTS)[number];

export const HOCKEY_CATCHES = ['Unknown', 'Left', 'Right'] as const;
export type HockeyCatches = (typeof HOCKEY_CATCHES)[number];

export const HOCKEY_CAPTAIN_ROLES = ['None', 'Captain', 'AlternateCaptain'] as const;
export type HockeyCaptainRole = (typeof HOCKEY_CAPTAIN_ROLES)[number];

export const HOCKEY_ROSTER_STATUSES = ['Active', 'Injured', 'Suspended', 'Inactive'] as const;
export type HockeyRosterStatus = (typeof HOCKEY_ROSTER_STATUSES)[number];

export const HOCKEY_LINE_TYPES = [
  'ForwardLine',
  'DefensePair',
  'PowerPlayUnit',
  'PenaltyKillUnit',
  'OvertimeUnit',
  'ShootoutOrder',
  'GoaliePair',
  'Custom',
] as const;
export type HockeyLineType = (typeof HOCKEY_LINE_TYPES)[number];

export const HOCKEY_COMPETITION_STATUSES = [
  'Draft',
  'Published',
  'RegistrationOpen',
  'Active',
  'Completed',
  'Cancelled',
] as const;
export type HockeyCompetitionStatus = (typeof HOCKEY_COMPETITION_STATUSES)[number];

export interface HockeyTeamPlayerDto {
  id: string;
  teamId: string;
  playerId: string;
  competitionId: string | null;
  position: HockeyPosition;
  captainRole: HockeyCaptainRole;
  rosterStatus: HockeyRosterStatus;
  jerseyNumber: number | null;
  requestedJerseyNumber: number | null;
  isActive: boolean;
  joinedAt: string;
}

export interface HockeyLinePlayerDto {
  id: string;
  lineId: string;
  teamPlayerId: string;
  slot: string;
  order: number;
}

export interface HockeyLineDto {
  id: string;
  teamId: string;
  competitionId: string | null;
  name: string;
  lineNumber: number;
  lineType: HockeyLineType;
  isActive: boolean;
  players: HockeyLinePlayerDto[];
}

export interface HockeyTeamStaffDto {
  id: string;
  teamId: string;
  personId: string;
  competitionId: string | null;
  role: string;
  isActive: boolean;
  joinedAt: string;
}

export interface HockeyTeamDto {
  id: string;
  name: string;
  shortName: string;
  clubId: string;
  divisionId: string | null;
  teamCategory: HockeyTeamCategory;
  homeArena: string;
  primaryJerseyColor: string;
  secondaryJerseyColor: string;
  logoUrl: string | null;
  isActive: boolean;
  roster: HockeyTeamPlayerDto[];
  lines: HockeyLineDto[];
  staffMembers: HockeyTeamStaffDto[];
}

export interface CreateHockeyTeamRequest {
  name: string;
  clubId: string;
  teamCategory: HockeyTeamCategory;
  divisionId?: string | null;
  homeArena?: string;
  primaryJerseyColor?: string;
  secondaryJerseyColor?: string;
  shortName?: string;
}

export interface UpdateHockeyTeamRequest {
  name: string;
  shortName?: string;
  teamCategory: HockeyTeamCategory;
  divisionId?: string | null;
  homeArena?: string;
  primaryJerseyColor?: string;
  secondaryJerseyColor?: string;
}

export interface HockeyPlayerDto {
  id: string;
  personId: string;
  licenseNumber: string | null;
  isActive: boolean;
  primaryPosition: HockeyPosition;
  shoots: HockeyShoots;
  catches: HockeyCatches | null;
  careerGamesPlayed: number;
  careerGoals: number;
  careerAssists: number;
  careerPenaltyMinutes: number;
}

export interface CreateHockeyPlayerRequest {
  personId: string;
  primaryPosition: HockeyPosition;
  shoots?: HockeyShoots;
  catches?: HockeyCatches | null;
  licenseNumber?: string;
}

export interface HockeyOfficialDto {
  id: string;
  personId: string;
  officialRole: HockeyOfficialRole;
  officialNumber: string | null;
  isActive: boolean;
  licenseIssueDate: string | null;
  licenseExpiryDate: string | null;
  matchesOfficiated: number;
}

export interface CreateHockeyOfficialRequest {
  personId: string;
  officialRole: HockeyOfficialRole;
  officialNumber?: string;
  licenseIssueDate?: string | null;
  licenseExpiryDate?: string | null;
}

export interface UpdateHockeyOfficialRequest {
  officialRole: HockeyOfficialRole;
  officialNumber?: string | null;
  licenseIssueDate?: string | null;
  licenseExpiryDate?: string | null;
  isActive: boolean;
}

export interface HockeyCompetitionTeamDto {
  id: string;
  competitionId: string;
  teamId: string;
  seed: number | null;
  joinedAt: string;
  isActive: boolean;
}

export interface HockeyCompetitionDivisionTeamDto {
  id: string;
  competitionDivisionId: string;
  competitionTeamId: string;
  seed: number | null;
  isActive: boolean;
}

export interface HockeyCompetitionDivisionDto {
  id: string;
  competitionId: string;
  divisionId: string;
  name: string;
  sortOrder: number;
  isActive: boolean;
  championCompetitionTeamId: string | null;
  teams: HockeyCompetitionDivisionTeamDto[];
}

export interface HockeyPlayoffSeriesDto {
  id: string;
  competitionId: string;
  round: string;
  seriesOrder: number;
  bestOf: number;
  homeCompetitionTeamId: string | null;
  awayCompetitionTeamId: string | null;
  homeTeamWins: number;
  awayTeamWins: number;
  winnerCompetitionTeamId: string | null;
  status: string;
}

export interface HockeyPlayoffScheduleSlotDto {
  round: string;
  seriesOrder: number;
  matchOrder: number;
  homeSourceType: string;
  awaySourceType: string;
  homeSourceGroupId?: string | null;
  awaySourceGroupId?: string | null;
  homeSourceSeriesId?: string | null;
  awaySourceSeriesId?: string | null;
  homeSourceRank?: number | null;
  awaySourceRank?: number | null;
  manualHomeCompetitionTeamId?: string | null;
  manualAwayCompetitionTeamId?: string | null;
}

export interface HockeySeasonDto {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  status: HockeyCompetitionStatus;
  isActive: boolean;
  isCompleted: boolean;
  seasonCode: string | null;
  teamCategory: HockeyTeamCategory;
  championCompetitionTeamId: string | null;
  teams: HockeyCompetitionTeamDto[];
  divisions: HockeyCompetitionDivisionDto[];
  playoffSeries: HockeyPlayoffSeriesDto[];
  playoffSchedule: HockeyPlayoffScheduleSlotDto[];
}

export interface CreateHockeySeasonRequest {
  name: string;
  startDate: string;
  endDate: string;
  seasonCode?: string;
  teamCategory?: HockeyTeamCategory;
}

export interface UpdateHockeySeasonRequest {
  name: string;
  startDate: string;
  endDate: string;
  seasonCode?: string | null;
  teamCategory: HockeyTeamCategory;
}

export interface HockeyTournamentRulesDto {
  format: string;
  hasGroupStage: boolean;
  hasPlayoffs: boolean;
  hasBronzeGame: boolean;
  hasPlacementGames: boolean;
  teamsAdvancingPerGroup: number;
}

export interface HockeyTournamentGroupTeamDto {
  id: string;
  tournamentGroupId: string;
  competitionTeamId: string;
  seed: number | null;
  isActive: boolean;
}

export interface HockeyTournamentGroupDto {
  id: string;
  tournamentId: string;
  name: string;
  sortOrder: number;
  teams: HockeyTournamentGroupTeamDto[];
}

export interface HockeyTournamentDto {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  status: HockeyCompetitionStatus;
  isActive: boolean;
  isCompleted: boolean;
  venue: string | null;
  contentHtml: string | null;
  currentStage: string;
  teamCategory: HockeyTeamCategory;
  championCompetitionTeamId: string | null;
  teams: HockeyCompetitionTeamDto[];
  groups: HockeyTournamentGroupDto[];
  playoffSeries: HockeyPlayoffSeriesDto[];
  tournamentRules: HockeyTournamentRulesDto;
  playoffSchedule: HockeyPlayoffScheduleSlotDto[];
}

export interface CreateHockeyTournamentRequest {
  name: string;
  startDate: string;
  endDate: string;
  venue?: string;
  contentHtml?: string;
  teamCategory?: HockeyTeamCategory;
}

export interface HockeyMatchActivePlayerDto {
  id: string;
  teamPlayerId: string;
  jerseyNumber: number;
  position: string;
  isActive: boolean;
  isStartingPlayer: boolean;
  isGoalie: boolean;
}

export interface HockeyMatchTeamDto {
  id: string;
  matchId: string;
  teamId: string;
  competitionTeamId: string | null;
  teamSlot: 'Home' | 'Away' | 'Neutral';
  goals: number;
  isConfirmedRoster: boolean;
  tracksOnIcePlayers: boolean;
  activeGoalieMatchPlayerId: string | null;
  activePlayers: HockeyMatchActivePlayerDto[];
  lines: unknown[];
  onIceState: unknown | null;
}

export interface HockeyMatchEventDto {
  id: string;
  eventType: string;
  periodNumber: number;
  gameTimeSeconds: number;
  matchTeamId: string | null;
  matchActivePlayerId: string | null;
  losingActivePlayerId?: string | null;
  description: string | null;
}

export interface HockeyMatchOfficialDto {
  id: string;
  officialId: string;
  role: string;
  isMainOfficial: boolean;
}

export interface HockeyPeriodScoreDto {
  id: string;
  periodNumber: number;
  periodType: string;
  homeMatchTeamId: string;
  awayMatchTeamId: string;
  homeGoals: number;
  awayGoals: number;
  isCompleted: boolean;
}

export interface HockeyMatchDto {
  id: string;
  competitionId: string | null;
  competitionDivisionId: string | null;
  tournamentGroupId: string | null;
  playoffSeriesId: string | null;
  playoffRound: HockeyPlayoffRound | string | null;
  playoffMatchOrder: number | null;
  nextMatchId: string | null;
  nextMatchSlot: HockeyTeamSlot | string | null;
  scheduledStartTime: string;
  actualStartTime: string | null;
  actualEndTime: string | null;
  venue: string | null;
  matchType: HockeyMatchType | string;
  status: HockeyMatchStatus | string;
  resultType: string | null;
  currentPeriodNumber: number;
  wentToOvertime: boolean;
  wentToShootout: boolean;
  homeTeamId: string | null;
  awayTeamId: string | null;
  homeScore: number;
  awayScore: number;
  matchTeams: HockeyMatchTeamDto[];
  events: HockeyMatchEventDto[];
  officials: HockeyMatchOfficialDto[];
  periodScores: HockeyPeriodScoreDto[];
}

export interface CreateHockeyMatchRequest {
  scheduledStartTime: string;
  matchType: HockeyMatchType;
  competitionId?: string;
  competitionDivisionId?: string;
  tournamentGroupId?: string;
  playoffSeriesId?: string;
  playoffRound?: HockeyPlayoffRound;
  playoffMatchOrder?: number;
  nextMatchId?: string;
  nextMatchSlot?: HockeyTeamSlot;
  venue?: string;
}

export interface RecordHockeyGoalRequest {
  scoringMatchTeamId: string;
  scorerActivePlayerId: string;
  periodNumber: number;
  timeInSeconds: number;
  goalStrength: HockeyGoalStrength;
  primaryAssistActivePlayerId?: string;
  secondaryAssistActivePlayerId?: string;
  goalieActivePlayerId?: string;
  wasEmptyNet?: boolean;
  description?: string;
}

export interface RecordHockeyPenaltyRequest {
  penaltyMatchTeamId: string;
  periodNumber: number;
  timeInSeconds: number;
  severity: HockeyPenaltySeverity;
  offence: HockeyPenaltyOffence;
  penaltyMinutes: number;
  penalizedActivePlayerId?: string;
  servedByActivePlayerId?: string;
  isBenchPenalty?: boolean;
  description?: string;
}

export const HOCKEY_FACEOFF_ZONES = ['NeutralZone', 'DefensiveZone', 'OffensiveZone'] as const;
export type HockeyFaceoffZone = (typeof HOCKEY_FACEOFF_ZONES)[number];

export const HOCKEY_FACEOFF_SPOTS = [
  'CenterIce',
  'NeutralLeft',
  'NeutralRight',
  'HomeDefensiveLeft',
  'HomeDefensiveRight',
  'AwayDefensiveLeft',
  'AwayDefensiveRight',
  'HomeOffensiveLeft',
  'HomeOffensiveRight',
  'AwayOffensiveLeft',
  'AwayOffensiveRight',
] as const;
export type HockeyFaceoffSpot = (typeof HOCKEY_FACEOFF_SPOTS)[number];

export interface RecordHockeyPeriodEventRequest {
  periodNumber: number;
  timeInSeconds: number;
  action: HockeyPeriodAction;
  description?: string;
}

export interface RecordHockeyFaceoffRequest {
  winningMatchTeamId: string;
  losingMatchTeamId: string;
  periodNumber: number;
  timeInSeconds: number;
  zone: HockeyFaceoffZone;
  spot: HockeyFaceoffSpot;
  winningActivePlayerId?: string;
  losingActivePlayerId?: string;
  description?: string;
}

export const HOCKEY_STOPPAGE_REASONS = [
  'Goal',
  'Offside',
  'Icing',
  'PuckOutOfPlay',
  'HandPass',
  'HighStick',
  'GoalieFreeze',
  'NetDislodged',
  'PenaltyCalled',
  'Injury',
  'Timeout',
  'VideoReview',
  'PeriodEnded',
  'RefereeWhistle',
] as const;
export type HockeyStoppageReason = (typeof HOCKEY_STOPPAGE_REASONS)[number];

export const HOCKEY_FACEOFF_SPOTS_BY_ZONE: Record<HockeyFaceoffZone, HockeyFaceoffSpot[]> = {
  NeutralZone: ['CenterIce', 'NeutralLeft', 'NeutralRight'],
  DefensiveZone: ['HomeDefensiveLeft', 'HomeDefensiveRight', 'AwayDefensiveLeft', 'AwayDefensiveRight'],
  OffensiveZone: ['HomeOffensiveLeft', 'HomeOffensiveRight', 'AwayOffensiveLeft', 'AwayOffensiveRight'],
};

export interface RecordHockeyStoppageRequest {
  periodNumber: number;
  timeInSeconds: number;
  reason: HockeyStoppageReason;
  responsibleMatchTeamId?: string;
  responsibleActivePlayerId?: string;
  nextFaceoffZone?: HockeyFaceoffZone;
  nextFaceoffSpot?: HockeyFaceoffSpot;
  ruleReference?: string;
  description?: string;
}

export interface RecordHockeyShotRequest {
  shootingMatchTeamId: string;
  periodNumber: number;
  timeInSeconds: number;
  shotResult: HockeyShotResult;
  countsAsShotOnGoal?: boolean;
  shooterActivePlayerId?: string;
  goalieActivePlayerId?: string;
  description?: string;
}

export interface HockeyTeamCompetitionStatisticsDto {
  id: string;
  teamId: string;
  competitionId: string;
  gamesPlayed: number;
  regulationWins: number;
  overtimeWins: number;
  shootoutWins: number;
  regulationLosses: number;
  overtimeLosses: number;
  shootoutLosses: number;
  ties: number;
  wins: number;
  losses: number;
  points: number;
  goalsFor: number;
  goalsAgainst: number;
  goalDifference: number;
  standingRank: number;
}

export interface HockeyPlayerCompetitionStatisticsDto {
  id: string;
  playerId: string;
  teamId: string;
  teamPlayerId: string;
  competitionId: string;
  gamesPlayed: number;
  goals: number;
  assists: number;
  points: number;
  penaltyMinutes: number;
  plusMinusRating: number;
  faceoffWins: number;
  faceoffAttempts: number;
}

export interface HockeyGoalieCompetitionStatisticsDto {
  id: string;
  playerId: string;
  teamId: string;
  teamPlayerId: string;
  competitionId: string;
  gamesPlayed: number;
  wins: number;
  losses: number;
  overtimeLosses: number;
  shootoutLosses: number;
  saves: number;
  shotsAgainst: number;
  savePercentage: number;
  goalsAgainst: number;
  goalsAgainstAverage: number;
  shutouts: number;
}

export interface HockeyTopScorerDto {
  playerId: string;
  teamId: string;
  gamesPlayed: number;
  goals: number;
  assists: number;
  points: number;
}

export interface HockeyTopGoalieDto {
  playerId: string;
  teamId: string;
  gamesPlayed: number;
  wins: number;
  savePercentage: number;
  goalsAgainstAverage: number;
  shutouts: number;
}

export interface HockeyCompetitionStatisticsSummaryDto {
  competitionId: string;
  teamCount: number;
  playerCount: number;
  goalieCount: number;
  standings: HockeyTeamCompetitionStatisticsDto[];
  topScorers: HockeyTopScorerDto[];
  topGoalies: HockeyTopGoalieDto[];
}

export interface HockeyMatchStatisticsDto {
  matchId: string;
  teams: Array<{
    teamId: string;
    matchTeamId: string;
    goalsFor: number;
    goalsAgainst: number;
    shotsOnGoal: number;
    faceoffWins: number;
    penaltyMinutes: number;
  }>;
  players: Array<{
    playerId: string;
    teamId: string;
    matchActivePlayerId?: string;
    goals: number;
    assists: number;
    points: number;
    penaltyMinutes: number;
    faceoffWins: number;
    faceoffAttempts: number;
  }>;
  goalies: Array<{
    playerId: string;
    teamId: string;
    saves: number;
    shotsAgainst: number;
    savePercentage: number;
    goalsAgainst: number;
  }>;
}

export function isHockeyMatchLive(status: string): boolean {
  return LIVE_HOCKEY_STATUSES.includes(status as HockeyMatchStatus);
}

export function isHockeyMatchFinished(status: string): boolean {
  return FINISHED_HOCKEY_STATUSES.includes(status as HockeyMatchStatus);
}

export function shouldRefreshHockeyMatches(matches: HockeyMatchDto[]): boolean {
  const horizon = Date.now() + 2 * 60 * 60 * 1000;
  return matches.some((match) => {
    if (isHockeyMatchLive(match.status)) {
      return true;
    }
    if (isHockeyMatchFinished(match.status) || match.status === 'Cancelled') {
      return false;
    }
    const start = new Date(match.scheduledStartTime).getTime();
    return Number.isFinite(start) && start <= horizon;
  });
}

export function hockeyHomeTeam(match: HockeyMatchDto): HockeyMatchTeamDto | undefined {
  return match.matchTeams.find((side) => side.teamSlot === 'Home');
}

export function hockeyAwayTeam(match: HockeyMatchDto): HockeyMatchTeamDto | undefined {
  return match.matchTeams.find((side) => side.teamSlot === 'Away');
}

export function hockeyActiveGoalieId(side: HockeyMatchTeamDto | undefined): string {
  if (!side) {
    return '';
  }
  return side.activeGoalieMatchPlayerId
    ?? side.activePlayers.find((player) => player.isGoalie)?.id
    ?? '';
}

export function hockeyOpposingGoalieId(match: HockeyMatchDto, shootingMatchTeamId: string): string {
  const defending = match.matchTeams.find((side) => side.id !== shootingMatchTeamId);
  return hockeyActiveGoalieId(defending);
}

export function hockeyShotIsOnGoal(result: HockeyShotResult): boolean {
  return result === 'Saved' || result === 'Goal';
}

export function hockeyShotCreditsGoalieSave(result: HockeyShotResult): boolean {
  return result === 'Saved';
}
