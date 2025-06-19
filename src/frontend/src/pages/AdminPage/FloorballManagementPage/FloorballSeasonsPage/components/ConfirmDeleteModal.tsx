import { useTranslation } from 'react-i18next';
import type { FloorballSeasonDto } from '../../../../../api/floorball/floorballSeasonService';

interface ConfirmDeleteModalProps {
  season: FloorballSeasonDto;
  onConfirm: () => void;
  onCancel: () => void;
}

export const ConfirmDeleteModal = ({
  season,
  onConfirm,
  onCancel
}: ConfirmDeleteModalProps) => {
  const { t } = useTranslation();

  const handleConfirm = () => {
    onConfirm();
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <div className="modal-header">
          <h3>{t('floorball.seasons.deleteConfirm.title', 'Delete Season')}</h3>
          <button 
            className="modal-close-btn"
            onClick={onCancel}
            aria-label={t('common.close', 'Close')}
          >
            ×
          </button>
        </div>
        
        <div className="modal-body">
          <div className="warning-icon">
            <i className="fas fa-exclamation-triangle"></i>
          </div>
          
          <p>{t('floorball.seasons.deleteConfirm.message', 'Are you sure you want to delete this season?')}</p>
          
          <div className="season-details">
            <strong>{season.name}</strong>
            <div className="season-meta">
              <span className="division">{season.division}</span>
              {season.teams && season.teams.length > 0 && (
                <span className="teams-warning">
                  {t('floorball.seasons.deleteConfirm.teamsWarning', 'This season has {{count}} teams', { count: season.teams.length })}
                </span>
              )}
            </div>
          </div>
          
          <p className="warning-text">
            {t('floorball.seasons.deleteConfirm.warning', 'This action cannot be undone.')}
          </p>
        </div>
        
        <div className="modal-footer">
          <button 
            className="btn btn-secondary"
            onClick={onCancel}
          >
            {t('common.cancel', 'Cancel')}
          </button>
          <button 
            className="btn btn-danger"
            onClick={handleConfirm}
          >
            {t('common.delete', 'Delete')}
          </button>
        </div>
      </div>
    </div>
  );
}; 