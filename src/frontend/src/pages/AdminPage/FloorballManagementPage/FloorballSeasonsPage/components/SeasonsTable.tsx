import { useTranslation } from 'react-i18next';
import type { FloorballSeasonDto } from '../../../../../api/floorball/floorballSeasonService';

interface SeasonsTableProps {
  seasons: FloorballSeasonDto[];
  onEdit: (season: FloorballSeasonDto) => void;
  onDelete: (season: FloorballSeasonDto) => void;
  onActivateToggle: (season: FloorballSeasonDto) => void;
  onComplete: (season: FloorballSeasonDto) => void;
  operationLoading?: string | null;
}

export const SeasonsTable = ({
  seasons,
  onEdit,
  onDelete,
  onActivateToggle,
  onComplete,
  operationLoading
}: SeasonsTableProps) => {
  const { t } = useTranslation();

  const formatDate = (dateString: string) => {
    try {
      return new Date(dateString).toLocaleDateString();
    } catch {
      return dateString;
    }
  };

  const getStatusBadge = (season: FloorballSeasonDto) => {
    if (season.isCompleted) {
      return <span className="status-badge completed">{t('floorball.seasons.status.completed', 'Completed')}</span>;
    }
    if (season.isActive) {
      return <span className="status-badge active">{t('floorball.seasons.status.active', 'Active')}</span>;
    }
    return <span className="status-badge inactive">{t('floorball.seasons.status.inactive', 'Inactive')}</span>;
  };

  return (
      <table className="seasons-table">
        <thead>
          <tr>
            <th>{t('floorball.seasons.fields.name', 'Name')}</th>
            <th>{t('floorball.seasons.fields.division', 'Division')}</th>
            <th>{t('floorball.seasons.fields.startDate', 'Starts')}</th>
            <th>{t('floorball.seasons.fields.endDate', 'Ends')}</th>
            <th>{t('floorball.seasons.fields.teams', 'Teams')}</th>
            <th>{t('floorball.seasons.fields.status', 'Status')}</th>
            <th>{t('common.actions', 'Actions')}</th>
          </tr>
        </thead>
        <tbody>
          {seasons.map((season) => (
            <tr key={season.id}>
              <td>
                <div className="season-name">
                  <strong>{season.name}</strong>
                </div>
              </td>
              <td>
                <span className={`division-badge division-${season.division.toLowerCase()}`}>
                  {season.division}
                </span>
              </td>
              <td>{formatDate(season.startDate)}</td>
              <td>{formatDate(season.endDate)}</td>
              <td>
                <span className="teams-count">
                  {season.teams?.length || 0} {t('floorball.seasons.teamsCount', 'teams')}
                </span>
              </td>
              <td>{getStatusBadge(season)}</td>
              <td>
                <div className="actions-group">
                  <button
                    className="btn btn-sm btn-outline-primary"
                    onClick={() => onEdit(season)}
                    title={t('common.edit', 'Edit')}
                    disabled={operationLoading === season.id}
                  >
                    ✏️
                  </button>
                  
                  {!season.isCompleted && (
                    <button
                      className={`btn btn-sm ${season.isActive ? 'btn-outline-warning' : 'btn-outline-success'}`}
                      onClick={() => onActivateToggle(season)}
                      title={season.isActive ? t('floorball.seasons.deactivate', 'Deactivate') : t('floorball.seasons.activate', 'Activate')}
                      disabled={operationLoading === season.id}
                    >
                      {operationLoading === season.id ? (
                        <i className="fas fa-spinner fa-spin"></i>
                      ) : (
                        season.isActive ? '⏸️' : '▶️'
                      )}
                    </button>
                  )}
                  
                  {season.isActive && !season.isCompleted && (
                    <button
                      className="btn btn-sm btn-outline-info"
                      onClick={() => onComplete(season)}
                      title={t('floorball.seasons.complete', 'Complete')}
                      disabled={operationLoading === season.id}
                    >
                      {operationLoading === season.id ? (
                        <i className="fas fa-spinner fa-spin"></i>
                      ) : (
                        '✅'
                      )}
                    </button>
                  )}
                  
                  <button
                    className="btn btn-sm btn-outline-danger"
                    onClick={() => onDelete(season)}
                    title={t('common.delete', 'Delete')}
                    disabled={operationLoading === season.id}
                  >
                    🗑️
                  </button>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
  );
}; 