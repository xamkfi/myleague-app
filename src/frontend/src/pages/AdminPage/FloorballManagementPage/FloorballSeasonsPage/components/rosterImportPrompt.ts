import { FloorballPosition } from '../../../../../types/floorball/floorballTypes';
import { buildRosterImportPrompt, buildRosterPromptFileName } from '../../../../../api/common/rosterImportPrompt';

export const FLOORBALL_ROSTER_IMPORT_POSITIONS = [
  FloorballPosition.Goalkeeper,
  FloorballPosition.Defender,
  FloorballPosition.Forward,
  FloorballPosition.Center,
] as const;

export const FLOORBALL_ROSTER_IMPORT_AI_PROMPT = buildRosterImportPrompt(
  'floorball',
  FLOORBALL_ROSTER_IMPORT_POSITIONS,
);

export function buildFloorballRosterPromptFileName(): string {
  return buildRosterPromptFileName('floorball');
}
