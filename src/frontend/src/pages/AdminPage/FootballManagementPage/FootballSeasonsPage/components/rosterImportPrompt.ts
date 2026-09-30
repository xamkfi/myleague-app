import { FootballPosition } from '../../../../../types/football/footballTypes';
import { buildRosterImportPrompt, buildRosterPromptFileName } from '../../../../../api/common/rosterImportPrompt';

export const FOOTBALL_ROSTER_IMPORT_POSITIONS = [
  FootballPosition.Goalkeeper,
  FootballPosition.Defender,
  FootballPosition.Midfielder,
  FootballPosition.Forward,
] as const;

export const FOOTBALL_ROSTER_IMPORT_AI_PROMPT = buildRosterImportPrompt(
  'football',
  FOOTBALL_ROSTER_IMPORT_POSITIONS,
);

export function buildFootballRosterPromptFileName(): string {
  return buildRosterPromptFileName('football');
}
