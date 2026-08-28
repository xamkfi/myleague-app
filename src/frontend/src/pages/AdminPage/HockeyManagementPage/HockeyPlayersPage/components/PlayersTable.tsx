import { useTranslation } from 'react-i18next';
import AdminPlayersTable from '../../../../../components/admin/AdminPlayersTable';

export interface HockeyPlayerListRow {
  playerId: string;
  teamId: string;
  teamName: string;
  name: string;
  position: string;
  isActive: boolean;
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
        rowKey: `${player.playerId}-${player.teamId}`,
        teamId: player.teamId,
        name: player.name,
        teamName: player.teamName,
        positionLabel: t(`hockey.positions.${player.position}`, player.position),
        isActive: player.isActive,
      }))}
      labels={{
        noPlayers: t('hockey.players.noPlayers', 'No players found.'),
        selectAll: t('hockey.players.selectAll', 'Select all players'),
        name: t('hockey.players.table.name', 'Name'),
        team: t('hockey.players.table.team', 'Team'),
        position: t('hockey.players.table.position', 'Position'),
        status: t('hockey.players.table.status', 'Status'),
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
        const source = players.find((row) => `${row.playerId}-${row.teamId}` === player.rowKey);
        if (source) onAssignToTeam(source);
      }}
      onStatusChange={(player, isActive) => {
        const source = players.find((row) => `${row.playerId}-${row.teamId}` === player.rowKey);
        if (source) onStatusChange(source, isActive);
      }}
      onDelete={(player) => {
        if (player.teamId) onDelete(player.id, player.teamId);
      }}
    />
  );
}

export default PlayersTable;
