import { useTranslation } from 'react-i18next';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import PlayerActionsDropdown from './PlayerActionsDropdown';

interface PlayersTableProps {
  players: FloorballPlayerDto[];
  onDelete: (playerId: string) => void;
  onStatusChange: (playerId: string, isActive: boolean) => void;
  selectedPlayers: Set<string>;
  onToggleSelection: (playerId: string) => void;
  onSelectAll: () => void;
  onClearSelection: () => void;
}

const PlayersTable = ({ players, onDelete, onStatusChange, selectedPlayers, onToggleSelection, onSelectAll, onClearSelection }: PlayersTableProps) => {
  const { t } = useTranslation();

  if (players.length === 0) {
    return <div className="no-data-state">{t('floorball.players.noPlayers', 'No players found.')}</div>;
  }

  return (
    <table className="players-table">
      <thead>
        <tr>
          <th className="select-column">
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
          <th>{t('floorball.players.table.position', 'Position')}</th>
          <th>{t('floorball.players.table.status', 'Status')}</th>
          <th>{t('floorball.players.table.actions', 'Actions')}</th>
        </tr>
      </thead>
      <tbody>
        {players.map((player) => (
          <tr 
            key={player.id}
            className={selectedPlayers.has(player.id) ? 'selected' : ''}
          >
            <td className="select-column">
              <input
                type="checkbox"
                checked={selectedPlayers.has(player.id)}
                onChange={() => onToggleSelection(player.id)}
                onClick={(e) => e.stopPropagation()}
              />
            </td>
            <td onClick={() => onToggleSelection(player.id)} className="clickable-cell">
              {player.person.fullName}
            </td>
            <td onClick={() => onToggleSelection(player.id)} className="clickable-cell">
              {player.position ? t(`floorball.positions.${player.position.toLowerCase()}`, player.position) : 'N/A'}
            </td>
            <td onClick={() => onToggleSelection(player.id)} className="clickable-cell">
              <span className={`status-badge ${player.isActive ? 'active' : 'inactive'}`}>
                {player.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
              </span>
            </td>
            <td>
              <div className="action-buttons">
                <PlayerActionsDropdown
                  player={player}
                  onDelete={onDelete}
                  onStatusChange={onStatusChange}
                />
              </div>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
};

export default PlayersTable; 