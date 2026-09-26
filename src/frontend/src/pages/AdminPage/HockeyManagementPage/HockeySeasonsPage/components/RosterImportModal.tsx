import { RosterExcelImportModal, type RosterImportLock } from '../../../../../components/admin/RosterExcelImportModal/RosterExcelImportModal';
import {
  importHockeyRosters,
  loadHockeyRosterSeasonTeams,
  loadHockeyRosterSeasons,
  revertHockeyRosters,
} from '../../../../../api/hockey/rosterImportService';

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
    <RosterExcelImportModal
      onClose={onClose}
      onImported={onImported}
      loadSeasons={loadHockeyRosterSeasons}
      loadSeasonTeams={loadHockeyRosterSeasonTeams}
      importRosters={importHockeyRosters}
      revertRosters={revertHockeyRosters}
      lockedSelection={lockedSelection}
      preferredTeamId={preferredTeamId}
      preferredTeamName={preferredTeamName}
      presetSeasonId={presetSeasonId}
    />
  );
}
