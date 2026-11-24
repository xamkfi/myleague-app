import { RosterJsonImportModal, type RosterImportLock } from '../../../../../components/admin/RosterJsonImportModal/RosterJsonImportModal';
import {
  importFootballRosters,
  loadFootballRosterCompetitions,
  loadFootballRosterTeams,
  revertFootballRosters,
} from '../../../../../api/football/rosterImportService';
import {
  FOOTBALL_ROSTER_IMPORT_AI_PROMPT,
  FOOTBALL_ROSTER_IMPORT_POSITIONS,
  buildFootballRosterPromptFileName,
} from './rosterImportPrompt';

const SAMPLE_HREF = new URL('../../../../../types/football/rosterImport.sample.json', import.meta.url).href;

interface FootballRosterImportModalProps {
  onClose: () => void;
  onImported: () => void;
  lockedSelection?: RosterImportLock | null;
  preferredTeamId?: string;
  preferredTeamName?: string;
  presetSeasonId?: string;
}

export function FootballRosterImportModal({
  onClose,
  onImported,
  lockedSelection = null,
  preferredTeamId,
  preferredTeamName,
  presetSeasonId,
}: FootballRosterImportModalProps) {
  return (
    <RosterJsonImportModal
      onClose={onClose}
      onImported={onImported}
      loadCompetitions={loadFootballRosterCompetitions}
      loadTeams={loadFootballRosterTeams}
      importRosters={importFootballRosters}
      revertRosters={revertFootballRosters}
      lockedSelection={lockedSelection}
      preferredTeamId={preferredTeamId}
      preferredTeamName={preferredTeamName}
      presetSeasonId={presetSeasonId}
      guide={{
        prompt: FOOTBALL_ROSTER_IMPORT_AI_PROMPT,
        buildPromptFileName: buildFootballRosterPromptFileName,
        sampleHref: SAMPLE_HREF,
        sampleDownloadName: 'football-roster-import-sample.json',
        allowedPositions: FOOTBALL_ROSTER_IMPORT_POSITIONS,
      }}
    />
  );
}
