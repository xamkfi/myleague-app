import { useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import type { FootballSeasonDto } from '../../../../../api/football/footballSeasonService';
import { useDivisions } from '../../../../../hooks/useDivisions';
import { SportsCategory } from '../../../../../types/common/sports';

interface ConfirmDeleteModalProps {
  season: FootballSeasonDto;
  onConfirm: () => void;
  onCancel: () => void;
}

export const ConfirmDeleteModal = ({
  season,
  onConfirm,
  onCancel
}: ConfirmDeleteModalProps) => {
  const { t } = useTranslation();
  const { divisions } = useDivisions();
  const footballDivisions = useMemo(
    () => divisions.filter((division) => division.sportType === SportsCategory.Football),
    [divisions]
  );
  
  const getDivisionName = (divisionId: string) => {
    const division = footballDivisions.find(d => d.id === divisionId);
    return division?.name || 'Unknown Division';
  };

  const handleConfirm = () => {
    onConfirm();
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <div className="modal-header">
          <h3>{t('football.seasons.deleteConfirm.title', 'Delete Season')}</h3>
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
          
          <p>{t('football.seasons.deleteConfirm.message', 'Are you sure you want to delete this season?')}</p>
          
          <div className="season-details">
            <strong>{season.name}</strong>
            <div className="season-meta">
              <div className="divisions">
                {season.seasonDivisions && season.seasonDivisions.length > 0 ? (
                  season.seasonDivisions.map((seasonDivision) => (
                    <span key={seasonDivision.divisionId} className="division">
                      {getDivisionName(seasonDivision.divisionId)}
                    </span>
                  ))
                ) : (
                  <span className="division">{t('football.seasons.noDivisions', 'No divisions')}</span>
                )}
              </div>
              {season.teams && season.teams.length > 0 && (
                <span className="teams-warning">
                  {t('football.seasons.deleteConfirm.teamsWarning', 'This season has {{count}} teams', { count: season.teams.length })}
                </span>
              )}
            </div>
          </div>
          
          <p className="warning-text">
            {t('football.seasons.deleteConfirm.warning', 'This action cannot be undone.')}
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