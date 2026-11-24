import { RosterJsonImportModal, type RosterImportLock } from '../../../../../components/admin/RosterJsonImportModal/RosterJsonImportModal';
import {
  importFloorballRosters,
  loadFloorballRosterCompetitions,
  loadFloorballRosterTeams,
  revertFloorballRosters,
} from '../../../../../api/floorball/rosterImportService';
import {
  FLOORBALL_ROSTER_IMPORT_AI_PROMPT,
  FLOORBALL_ROSTER_IMPORT_POSITIONS,
  buildFloorballRosterPromptFileName,
} from './rosterImportPrompt';

const SAMPLE_HREF = new URL('../../../../../types/floorball/rosterImport.sample.json', import.meta.url).href;

interface FloorballRosterImportModalProps {
  onClose: () => void;
  onImported: () => void;
  lockedSelection?: RosterImportLock | null;
  preferredTeamId?: string;
  preferredTeamName?: string;
  presetSeasonId?: string;
}

export function FloorballRosterImportModal({
  onClose,
  onImported,
  lockedSelection = null,
  preferredTeamId,
  preferredTeamName,
  presetSeasonId,
}: FloorballRosterImportModalProps) {
  return (
    <RosterJsonImportModal
      onClose={onClose}
      onImported={onImported}
      loadCompetitions={loadFloorballRosterCompetitions}
      loadTeams={loadFloorballRosterTeams}
      importRosters={importFloorballRosters}
      revertRosters={revertFloorballRosters}
      lockedSelection={lockedSelection}
      preferredTeamId={preferredTeamId}
      preferredTeamName={preferredTeamName}
      presetSeasonId={presetSeasonId}
      guide={{
        prompt: FLOORBALL_ROSTER_IMPORT_AI_PROMPT,
        buildPromptFileName: buildFloorballRosterPromptFileName,
        sampleHref: SAMPLE_HREF,
        sampleDownloadName: 'floorball-roster-import-sample.json',
        allowedPositions: FLOORBALL_ROSTER_IMPORT_POSITIONS,
      }}
    />
  );
}
