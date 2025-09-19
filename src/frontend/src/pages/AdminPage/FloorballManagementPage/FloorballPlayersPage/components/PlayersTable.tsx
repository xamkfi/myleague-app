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
    <div className="players-table-container">
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
            <th className="name-column">{t('floorball.players.table.name', 'Name')}</th>
            <th className="team-column">{t('floorball.players.table.team', 'Team')}</th>
            <th className="position-column">{t('floorball.players.table.position', 'Position')}</th>
            <th className="status-column">{t('floorball.players.table.status', 'Status')}</th>
            <th className="actions-column">{t('floorball.players.table.actions', 'Action')}</th>
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
              <td className="name-column">
                {player.person.fullName || `${player.person.firstName} ${player.person.lastName}`}
              </td>
              <td className="team-column">
                {player.team?.name || 'Not assigned'}
              </td>
              <td className="position-column">
                {player.position ? t(`floorball.positions.${player.position.toLowerCase()}`, player.position) : 'None'}
              </td>
              <td className="status-column">
                <span className={`status-badge ${player.isActive ? 'active' : 'inactive'}`}>
                  {player.isActive ? '✓' : '✗'}
                </span>
              </td>
              <td className="actions-column">
                <PlayerActionsDropdown
                  player={player}
                  onDelete={onDelete}
                  onStatusChange={onStatusChange}
                />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default PlayersTable; 