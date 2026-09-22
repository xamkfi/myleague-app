import { useTranslation } from 'react-i18next';
import AdminPlayersTable from '../../../../../components/admin/AdminPlayersTable';
import type { ActivePlayerLicence } from '../../../../../types/activePlayerLicence';

export interface HockeyPlayerListRow {
  playerId: string;
  teamId: string;
  teamIds: string[];
  firstName: string;
  lastName: string;
  name: string;
  position: string;
  isActive: boolean;
  licences: ActivePlayerLicence[];
}

interface PlayersTableProps {
  players: HockeyPlayerListRow[];
  onDelete: (playerId: string, teamId: string) => void;
  onStatusChange: (player: HockeyPlayerListRow, isActive: boolean) => void;
  onAssignToTeam: (player: HockeyPlayerListRow) => void;
  selectedPlayers: Set<string>;
  onToggleSelection: (playerId: string) => void;
  onSelectAll: () => void;
  onClearSelection: () => void;
}

function PlayersTable({
  players,
  onDelete,
  onStatusChange,
  onAssignToTeam,
  selectedPlayers,
  onToggleSelection,
  onSelectAll,
  onClearSelection,
}: PlayersTableProps) {
  const { t } = useTranslation();

  return (
    <AdminPlayersTable
      sport="hockey"
      players={players.map((player) => ({
        id: player.playerId,
        rowKey: player.playerId,
        teamId: player.teamId,
        firstName: player.firstName,
        lastName: player.lastName,
        licences: player.licences,
        isActive: player.isActive,
      }))}
      labels={{
        noPlayers: t('hockey.players.noPlayers', 'No players found.'),
        selectAll: t('hockey.players.selectAll', 'Select all players'),
        firstName: t('playerLicence.firstName'),
        lastName: t('playerLicence.lastName'),
        licences: t('playerLicence.activeLicences'),
        noActiveLicences: t('playerLicence.noActiveLicences'),
        actions: t('hockey.players.table.actions', 'Actions'),
        assignToTeam: t('hockey.teams.assignPlayerToTeam', 'Assign to Team'),
        deactivate: t('hockey.players.actions.deactivate', 'Deactivate Player'),
        activate: t('hockey.players.actions.activate', 'Activate Player'),
        delete: t('hockey.teams.removeFromTeam', 'Remove from Team'),
        actionsMenu: t('hockey.players.actions.menu', 'Player actions menu'),
      }}
      selectedPlayers={selectedPlayers}
      onToggleSelection={onToggleSelection}
      onSelectAll={onSelectAll}
      onClearSelection={onClearSelection}
      onAssignToTeam={(player) => {
        const source = players.find((row) => row.playerId === player.id);
        if (source) onAssignToTeam(source);
      }}
      onStatusChange={(player, isActive) => {
        const source = players.find((row) => row.playerId === player.id);
        if (source) onStatusChange(source, isActive);
      }}
      onDelete={(player) => {
        if (player.teamId) onDelete(player.id, player.teamId);
      }}
    />
  );
}

export default PlayersTable;
