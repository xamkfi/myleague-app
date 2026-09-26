import { RosterExcelImportModal, type RosterImportLock } from '../../../../../components/admin/RosterExcelImportModal/RosterExcelImportModal';
import {
  importFloorballRosters,
  loadFloorballRosterSeasonTeams,
  loadFloorballRosterSeasons,
  revertFloorballRosters,
} from '../../../../../api/floorball/rosterImportService';

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
    <RosterExcelImportModal
      onClose={onClose}
      onImported={onImported}
      loadSeasons={loadFloorballRosterSeasons}
      loadSeasonTeams={loadFloorballRosterSeasonTeams}
      importRosters={importFloorballRosters}
      revertRosters={revertFloorballRosters}
      lockedSelection={lockedSelection}
      preferredTeamId={preferredTeamId}
      preferredTeamName={preferredTeamName}
      presetSeasonId={presetSeasonId}
    />
  );
}
