import { HOCKEY_POSITIONS } from '../../../../../types/hockey/hockeyTypes';
import { buildRosterImportPrompt, buildRosterPromptFileName } from '../../../../../api/common/rosterImportPrompt';

export const HOCKEY_ROSTER_IMPORT_POSITIONS = HOCKEY_POSITIONS;

export const HOCKEY_ROSTER_IMPORT_AI_PROMPT = buildRosterImportPrompt('ice hockey', HOCKEY_POSITIONS);

export function buildHockeyRosterPromptFileName(): string {
  return buildRosterPromptFileName('hockey');
}
