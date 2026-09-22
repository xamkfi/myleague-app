import type {
  SeasonImportClub,
  SeasonImportDivision,
  SeasonImportMatch,
  SeasonImportPayloadBase,
  SeasonImportSeasonBase,
  SeasonImportTeamBase,
} from '../common/seasonImportTypes';

export interface FootballSeasonImportSeasonSection extends SeasonImportSeasonBase {
  numberOfHalves?: number;
  halfDurationMinutes?: number;
  playersOnField?: number;
  requireGoalkeeper?: boolean;
  maxSubstitutions?: number;
  requireOfficialsToStart?: boolean;
  allowExtraTime?: boolean;
  extraTimeHalfCount?: number;
  extraTimeHalfDurationMinutes?: number;
  allowPenaltyShootout?: boolean;
  winPoints?: number;
  drawPoints?: number;
  lossPoints?: number;
}

export interface FootballSeasonImportPayload extends SeasonImportPayloadBase {
  season: FootballSeasonImportSeasonSection;
  clubs: SeasonImportClub[];
  divisions: SeasonImportDivision[];
  teams: SeasonImportTeamBase[];
  matches: SeasonImportMatch[];
}
