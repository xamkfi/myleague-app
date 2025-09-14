import { useTranslation } from 'react-i18next';
import type { FloorballPlayerDto } from '../../../../../api/floorball/floorballPlayerService';
import Pagination from '../../../../../components/Pagination';

interface PlayersTableProps {
  players: FloorballPlayerDto[];
  onDelete: (playerId: string) => void;
  pagination: {
    currentPage: number;
    totalPages: number;
    totalCount: number;
    pageSize: number;
  };
  onPageChange?: (page: number) => void;
  onPageSizeChange?: (pageSize: number) => void;
  isLoading?: boolean;
}

const PlayersTable = ({ players, onDelete, pagination, onPageChange, onPageSizeChange, isLoading }: PlayersTableProps) => {
  const { t } = useTranslation();

  if (players.length === 0) {
    return <div className="no-data-state">{t('floorball.players.noPlayers', 'No players found.')}</div>;
  }

  return (
    <>
        
        
      
        <table className="players-table">

          <thead>
            <tr>
              <th style={{ textAlign: 'left' }}>{t('floorball.players.table.name', 'Name')}</th>
              <th style={{ textAlign: 'left' }}>{t('floorball.players.table.status', 'Status')}</th>
              <th style={{ textAlign: 'left' }}>{t('floorball.players.table.actions', 'Actions')}</th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              <tr>
                <td colSpan={3} style={{ textAlign: 'center', padding: '1rem' }}>
                  {t('common.loading', 'Loading...')}
                </td>
              </tr>
            ) : players.map((player) => (
              <tr key={player.id}>
                <td style={{ textAlign: 'left' }}>{player.person.fullName}</td>
                <td style={{ textAlign: 'left' }}>
                  <span className={`status-badge ${player.isActive ? 'active' : 'inactive'}`}>
                    {player.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
                  </span>
                </td>
                <td style={{ textAlign: 'left' }}>
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
        <div className="players-pagination sticky-bottom">
          <Pagination
            currentPage={pagination.currentPage}
            totalPages={pagination.totalPages}
            totalCount={pagination.totalCount}
            pageSize={pagination.pageSize}
            onPageChange={(p) => onPageChange ? onPageChange(p) : undefined}
            onPageSizeChange={(s) => onPageSizeChange ? onPageSizeChange(s) : undefined}
            className="no-margin"
          />
        </div>
    </>
  );
};

export default PlayersTable; 