import { useTranslation } from 'react-i18next';
import CheckIcon from '../../../../../assets/basicIcons/check.svg';
import CloseIcon from '../../../../../assets/basicIcons/close.svg';
import ActionsDropdown from '../../../../../components/ActionsDropdown/ActionsDropdown';

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

  if (players.length === 0) {
    return <div className="no-data-state">{t('hockey.players.noPlayers', 'No players found.')}</div>;
  }

  return (
    <table className="admin-table">
      <thead>
        <tr>
          <th className="admin-table__checkbox-col">
            <input
              type="checkbox"
              checked={players.length > 0 && players.every((player) => selectedPlayers.has(player.playerId))}
              onChange={(event) => {
                if (event.target.checked) {
                  onSelectAll();
                } else {
                  onClearSelection();
                }
              }}
              title={t('hockey.players.selectAll', 'Select all players')}
            />
          </th>
          <th>{t('hockey.players.table.name', 'Name')}</th>
          <th>{t('hockey.players.table.team', 'Team')}</th>
          <th>{t('hockey.players.table.position', 'Position')}</th>
          <th>{t('hockey.players.table.status', 'Status')}</th>
          <th className="admin-table__actions-col">{t('hockey.players.table.actions', 'Actions')}</th>
        </tr>
      </thead>
      <tbody>
        {players.map((player) => (
          <tr
            key={`${player.playerId}-${player.teamId}`}
            className={`admin-table__row--clickable${selectedPlayers.has(player.playerId) ? ' admin-table__row--selected' : ''}`}
            onClick={() => onToggleSelection(player.playerId)}
          >
            <td className="admin-table__checkbox-col">
              <input
                type="checkbox"
                checked={selectedPlayers.has(player.playerId)}
                onChange={() => onToggleSelection(player.playerId)}
                onClick={(event) => event.stopPropagation()}
              />
            </td>
            <td className="admin-table__name">{player.name}</td>
            <td>{player.teamName}</td>
            <td>{t(`hockey.positions.${player.position}`, player.position)}</td>
            <td>
              <span
                className={`admin-badge ${player.isActive ? 'admin-badge--active' : 'admin-badge--inactive'}`}
                title={player.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
              >
                <img
                  src={player.isActive ? CheckIcon : CloseIcon}
                  alt={player.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
                  className="status-icon"
                />
              </span>
            </td>
            <td className="admin-table__actions-col" onClick={(event) => event.stopPropagation()}>
              <ActionsDropdown
                actions={[
                  {
                    label: t('hockey.teams.assignPlayerToTeam', 'Assign to Team'),
                    onClick: () => onAssignToTeam(player),
                  },
                  {
                    label: player.isActive
                      ? t('hockey.players.actions.deactivate', 'Deactivate Player')
                      : t('hockey.players.actions.activate', 'Activate Player'),
                    onClick: () => onStatusChange(player, !player.isActive),
                    variant: 'status',
                  },
                  {
                    label: t('hockey.teams.removeFromTeam', 'Remove from Team'),
                    onClick: () => onDelete(player.playerId, player.teamId),
                    variant: 'danger',
                  },
                ]}
                ariaLabel={t('hockey.players.actions.menu', 'Player actions menu')}
              />
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

export default PlayersTable;
