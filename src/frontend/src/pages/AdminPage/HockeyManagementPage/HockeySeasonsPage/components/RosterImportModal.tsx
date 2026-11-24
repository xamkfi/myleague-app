import { RosterJsonImportModal, type RosterImportLock } from '../../../../../components/admin/RosterJsonImportModal/RosterJsonImportModal';
import {
  importHockeyRosters,
  loadHockeyRosterCompetitions,
  loadHockeyRosterTeams,
  revertHockeyRosters,
} from '../../../../../api/hockey/rosterImportService';
import {
  HOCKEY_ROSTER_IMPORT_AI_PROMPT,
  HOCKEY_ROSTER_IMPORT_POSITIONS,
  buildHockeyRosterPromptFileName,
} from './rosterImportPrompt';

const SAMPLE_HREF = new URL('../../../../../types/hockey/rosterImport.sample.json', import.meta.url).href;

interface HockeyRosterImportModalProps {
  onClose: () => void;
  onImported: () => void;
  lockedSelection?: RosterImportLock | null;
  preferredTeamId?: string;
  preferredTeamName?: string;
  presetSeasonId?: string;
}

export function HockeyRosterImportModal({
  onClose,
  onImported,
  lockedSelection = null,
  preferredTeamId,
  preferredTeamName,
  presetSeasonId,
}: HockeyRosterImportModalProps) {
  return (
    <RosterJsonImportModal
      onClose={onClose}
      onImported={onImported}
      loadCompetitions={loadHockeyRosterCompetitions}
      loadTeams={loadHockeyRosterTeams}
      importRosters={importHockeyRosters}
      revertRosters={revertHockeyRosters}
      lockedSelection={lockedSelection}
      preferredTeamId={preferredTeamId}
      preferredTeamName={preferredTeamName}
      presetSeasonId={presetSeasonId}
      guide={{
        prompt: HOCKEY_ROSTER_IMPORT_AI_PROMPT,
        buildPromptFileName: buildHockeyRosterPromptFileName,
        sampleHref: SAMPLE_HREF,
        sampleDownloadName: 'hockey-roster-import-sample.json',
        allowedPositions: HOCKEY_ROSTER_IMPORT_POSITIONS,
      }}
    />
  );
}
