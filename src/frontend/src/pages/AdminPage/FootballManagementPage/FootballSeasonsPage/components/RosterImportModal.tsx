import { RosterExcelImportModal, type RosterImportLock } from '../../../../../components/admin/RosterExcelImportModal/RosterExcelImportModal';
import {
  importFootballRosters,
  loadFootballRosterSeasonTeams,
  loadFootballRosterSeasons,
  revertFootballRosters,
} from '../../../../../api/football/rosterImportService';

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
    <RosterExcelImportModal
      onClose={onClose}
      onImported={onImported}
      loadSeasons={loadFootballRosterSeasons}
      loadSeasonTeams={loadFootballRosterSeasonTeams}
      importRosters={importFootballRosters}
      revertRosters={revertFootballRosters}
      lockedSelection={lockedSelection}
      preferredTeamId={preferredTeamId}
      preferredTeamName={preferredTeamName}
      presetSeasonId={presetSeasonId}
    />
  );
}
