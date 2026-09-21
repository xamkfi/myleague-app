import type {
  SeasonImportClub,
  SeasonImportDivision,
  SeasonImportMatch,
  SeasonImportPayloadBase,
  SeasonImportSeasonBase,
  SeasonImportTeamBase,
} from '../common/seasonImportTypes';

export interface HockeySeasonImportSeasonSection extends SeasonImportSeasonBase {
  seasonCode?: string;
}

export interface HockeySeasonImportPayload extends SeasonImportPayloadBase {
  season: HockeySeasonImportSeasonSection;
  clubs: SeasonImportClub[];
  divisions: SeasonImportDivision[];
  teams: SeasonImportTeamBase[];
  matches: SeasonImportMatch[];
}
