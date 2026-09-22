import { useTranslation } from 'react-i18next';
import type { FootballPlayerDto } from '../../../../../api/football/footballPlayerService';
import AdminPlayersTable from '../../../../../components/admin/AdminPlayersTable';

interface PlayersTableProps {
  players: FootballPlayerDto[];
  onDelete: (playerId: string) => void;
  onStatusChange: (playerId: string, isActive: boolean) => void;
  onAssignToTeam: (playerId: string) => void;
  selectedPlayers: Set<string>;
  onToggleSelection: (playerId: string) => void;
  onSelectAll: () => void;
  onClearSelection: () => void;
}

const PlayersTable = ({
  players,
  onDelete,
  onStatusChange,
  onAssignToTeam,
  selectedPlayers,
  onToggleSelection,
  onSelectAll,
  onClearSelection,
}: PlayersTableProps) => {
  const { t } = useTranslation();

  return (
    <AdminPlayersTable
      sport="football"
      players={players.map((player) => ({
        id: player.id,
        firstName: player.person.firstName,
        lastName: player.person.lastName,
        licences: player.activeLicences ?? [],
        isActive: player.isActive,
      }))}
      labels={{
        noPlayers: t('football.players.noPlayers', 'No players found.'),
        selectAll: t('football.players.selectAll', 'Select all players'),
        firstName: t('playerLicence.firstName'),
        lastName: t('playerLicence.lastName'),
        licences: t('playerLicence.activeLicences'),
        noActiveLicences: t('playerLicence.noActiveLicences'),
        actions: t('football.players.table.actions', 'Actions'),
        assignToTeam: t('football.teams.assignPlayerToTeam', 'Assign to Team'),
        deactivate: t('football.players.actions.deactivate', 'Deactivate Player'),
        activate: t('football.players.actions.activate', 'Activate Player'),
        delete: t('common.delete'),
        actionsMenu: t('football.players.actions.menu', 'Player actions menu'),
      }}
      selectedPlayers={selectedPlayers}
      onToggleSelection={onToggleSelection}
      onSelectAll={onSelectAll}
      onClearSelection={onClearSelection}
      onAssignToTeam={(player) => onAssignToTeam(player.id)}
      onStatusChange={(player, isActive) => onStatusChange(player.id, isActive)}
      onDelete={(player) => onDelete(player.id)}
    />
  );
};

export default PlayersTable;
