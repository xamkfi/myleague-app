import type {
  SeasonImportClub,
  SeasonImportDivision,
  SeasonImportMatch,
  SeasonImportPayloadBase,
  SeasonImportSeasonBase,
  SeasonImportTeamBase,
} from '../common/seasonImportTypes';

export const FLOORBALL_SEASON_IMPORT_SCHEMA_VERSION = 'myleague-season-import/v1' as const;

export interface FloorballSeasonImportSeasonSection extends SeasonImportSeasonBase {
  numberOfPeriods?: number;
  periodDurationMinutes?: number;
  allowOvertime?: boolean;
  overtimeDurationMinutes?: number;
  allowShootout?: boolean;
}

export interface FloorballSeasonImportPayload extends SeasonImportPayloadBase {
  season: FloorballSeasonImportSeasonSection;
  clubs: SeasonImportClub[];
  divisions: SeasonImportDivision[];
  teams: SeasonImportTeamBase[];
  matches: SeasonImportMatch[];
}
