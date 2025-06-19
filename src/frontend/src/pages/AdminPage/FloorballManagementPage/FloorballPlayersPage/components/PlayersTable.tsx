import { useTranslation } from 'react-i18next';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';

interface PlayersTableProps {
  players: FloorballPlayerDto[];
  onDelete: (playerId: string) => void;
}

const PlayersTable = ({ players, onDelete }: PlayersTableProps) => {
  const { t } = useTranslation();

  if (players.length === 0) {
    return <div className="no-data-state">{t('floorball.players.noPlayers', 'No players found.')}</div>;
  }

  return (
    <table className="players-table">
      <thead>
        <tr>
          <th>{t('floorball.players.table.name', 'Name')}</th>
          <th>{t('floorball.players.table.position', 'Position')}</th>
          <th>{t('floorball.players.table.status', 'Status')}</th>
          <th>{t('floorball.players.table.actions', 'Actions')}</th>
        </tr>
      </thead>
      <tbody>
        {players.map((player) => (
          <tr key={player.id}>
            <td>{player.person.fullName}</td>
            <td>{player.position ? t(`floorball.positions.${player.position.toLowerCase()}`, player.position) : 'N/A'}</td>
            <td>
              <span className={`status-badge ${player.isActive ? 'active' : 'inactive'}`}>
                {player.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
              </span>
            </td>
            <td>
              <div className="action-buttons">
                <button onClick={() => onDelete(player.id)} className="delete-btn">
                  {t('common.delete', 'Delete')}
                </button>
              </div>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
};

export default PlayersTable; 