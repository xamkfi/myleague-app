import { useTranslation } from 'react-i18next';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import Pagination from '../../../../../components/Pagination';
import CheckIcon from '../../../../../assets/basicIcons/check.svg';
import CloseIcon from '../../../../../assets/basicIcons/close.svg';

interface PlayersTableProps {
  players: FloorballPlayerDto[];
  onDelete: (playerId: string) => void;
  selectedPlayers: Set<string>;
  onToggleSelection: (playerId: string) => void;
  onSelectAll: () => void;
  onClearSelection: () => void;
  pagination?: {
    currentPage: number;
    totalPages: number;
    totalCount: number;
    pageSize: number;
  };
  onPageChange?: (page: number) => void;
  onPageSizeChange?: (pageSize: number) => void;
}

const PlayersTable = ({ players, onDelete, selectedPlayers, onToggleSelection, onSelectAll, onClearSelection, pagination, onPageChange, onPageSizeChange }: PlayersTableProps) => {
  const { t } = useTranslation();

  if (players.length === 0) {
    return <div className="no-data-state">{t('floorball.players.noPlayers', 'No players found.')}</div>;
  }

  return (
    <>
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
          <th>{t('floorball.players.table.status', 'Status')}</th>
          <th className="actions-column">{t('floorball.players.table.actions', 'Actions')}</th>
        </tr>
      </thead>
      <tbody>
        {players.map((player) => (
          <tr 
            key={player.id}
            className={`clickable-row${selectedPlayers.has(player.id) ? ' selected' : ''}`}
            onClick={() => onToggleSelection(player.id)}
          >
            <td className="select-column">
              <input
                type="checkbox"
                checked={selectedPlayers.has(player.id)}
                onChange={() => onToggleSelection(player.id)}
                onClick={(e) => e.stopPropagation()}
              />
            </td>
            <td>
              <span className="floorball-player-name">{player.person.fullName}</span>
            </td>
            <td>
              <span 
                className={`status-badge ${player.isActive ? 'active' : 'inactive'}`}
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
            <td className="actions-column">
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
    {pagination && (
      <div className="players-pagination">
        <Pagination
          currentPage={pagination.currentPage}
          totalPages={pagination.totalPages}
          totalCount={pagination.totalCount}
          pageSize={pagination.pageSize}
          onPageChange={(p) => (onPageChange ? onPageChange(p) : undefined)}
          onPageSizeChange={(s) => (onPageSizeChange ? onPageSizeChange(s) : undefined)}
          className="no-margin"
        />
      </div>
    )}
    </>
  );
};

export default PlayersTable;