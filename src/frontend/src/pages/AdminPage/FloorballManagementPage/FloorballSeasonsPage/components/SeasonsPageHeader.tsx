import { useTranslation } from 'react-i18next';
import './SeasonsPageHeader.scss';

interface SeasonsPageHeaderProps {
  seasonsCount: number;
  onCreateSeason: () => void;
  onManageMatches: () => void;
  onImportSeason: () => void;
}

export const SeasonsPageHeader = ({
  seasonsCount,
  onCreateSeason,
  onManageMatches,
  onImportSeason,
}: SeasonsPageHeaderProps) => {
  const { t } = useTranslation();

  return (
    <div className="floorball-seasons-header">
      <div className="seasons-count">
        <span>{t('floorball.seasons.totalCount', `${seasonsCount} seasons`, { count: seasonsCount })}</span>
      </div>
      <div className="seasons-actions">
        <button
          type="button"
          className="manage-matches-button"
          onClick={onManageMatches}
        >
          {t('floorball.management.actions.seasonMatches', 'Manage Season Matches')}
        </button>
        <button
          type="button"
          className="import-season-button"
          onClick={onImportSeason}
          title={t('floorball.seasons.import.buttonTooltip', 'Create a season by uploading a JSON file (myleague format).')}
        >
          <i className="fas fa-file-import"></i>
          {' '}
          {t('floorball.seasons.import.button', 'Import from JSON')}
        </button>
        <button
          type="button"
          className="create-season-button"
          onClick={onCreateSeason}
        >
          {t('floorball.seasons.createNew', 'Create New Season')}
        </button>
      </div>
    </div>
  );
}; 