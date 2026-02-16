import { useTranslation } from 'react-i18next';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import CheckIcon from '../../../../../assets/basicIcons/check.svg';
import CloseIcon from '../../../../../assets/basicIcons/close.svg';
import ActionsDropdown from '../../../../../components/ActionsDropdown/ActionsDropdown';

interface PlayersTableProps {
  players: FloorballPlayerDto[];
  onDelete: (playerId: string) => void;
  onStatusChange: (playerId: string, isActive: boolean) => void;
  onAssignToTeam: (playerId: string) => void;
  selectedPlayers: Set<string>;
  onToggleSelection: (playerId: string) => void;
  onSelectAll: () => void;
  onClearSelection: () => void;
}

const PlayersTable = ({ players, onDelete, onStatusChange, onAssignToTeam, selectedPlayers, onToggleSelection, onSelectAll, onClearSelection }: PlayersTableProps) => {
  const { t } = useTranslation();

  if (players.length === 0) {
    return <div className="no-data-state">{t('floorball.players.noPlayers', 'No players found.')}</div>;
  }

  return (
    <table className="admin-table">
      <thead>
        <tr>
          <th className="admin-table__checkbox-col">
            <input
              type="checkbox"
              checked={players.length > 0 && players.every(player => selectedPlayers.has(player.id))}
              onChange={(e) => {
                if (e.target.checked) {
                  onSelectAll();
                } else {
                  onClearSelection();
                }
              }}
              title={t('floorball.players.selectAll', 'Select all players')}
            />
          </th>
          <th>{t('floorball.players.table.name', 'Name')}</th>
          <th>{t('floorball.players.table.team', 'Team')}</th>
          <th>{t('floorball.players.table.position', 'Position')}</th>
          <th>{t('floorball.players.table.status', 'Status')}</th>
          <th className="admin-table__actions-col">{t('floorball.players.table.actions', 'Actions')}</th>
        </tr>
      </thead>
      <tbody>
        {players.map((player) => (
          <tr
            key={player.id}
            className={`admin-table__row--clickable${selectedPlayers.has(player.id) ? ' admin-table__row--selected' : ''}`}
            onClick={() => onToggleSelection(player.id)}
          >
            <td className="admin-table__checkbox-col">
              <input
                type="checkbox"
                checked={selectedPlayers.has(player.id)}
                onChange={() => onToggleSelection(player.id)}
                onClick={(e) => e.stopPropagation()}
              />
            </td>
            <td className="admin-table__name">
              {player.person.fullName || `${player.person.firstName} ${player.person.lastName}`}
            </td>
            <td>
              {player.team?.name || 'Not assigned'}
            </td>
            <td>
              {player.position ? t(`floorball.positions.${player.position.toLowerCase()}`, player.position) : 'None'}
            </td>
            <td>
              <span
                className={`admin-badge ${player.isActive ? 'admin-badge--active' : 'admin-badge--inactive'}`}
                aria-label={player.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
                title={player.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
              >
                <img
                  src={player.isActive ? CheckIcon : CloseIcon}
                  alt={player.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
                  className="status-icon"
                />
              </span>
            </td>
            <td className="admin-table__actions-col">
              <ActionsDropdown
                actions={[
                  {
                    label: t('floorball.teams.assignPlayerToTeam', 'Assign to Team'),
                    onClick: () => onAssignToTeam(player.id),
                  },
                  {
                    label: player.isActive
                      ? t('floorball.players.actions.deactivate', 'Deactivate Player')
                      : t('floorball.players.actions.activate', 'Activate Player'),
                    onClick: () => onStatusChange(player.id, !player.isActive),
                    variant: 'status',
                  },
                  {
                    label: t('common.delete', 'Delete'),
                    onClick: () => onDelete(player.id),
                    variant: 'danger',
                  },
                ]}
                ariaLabel={t('floorball.players.actions.menu', 'Player actions menu')}
              />
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
};

export default PlayersTable;