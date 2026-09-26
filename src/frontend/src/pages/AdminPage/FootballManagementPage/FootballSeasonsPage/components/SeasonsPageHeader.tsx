import { useTranslation } from 'react-i18next';
import './SeasonsPageHeader.scss';

interface SeasonsPageHeaderProps {
  seasonsCount: number;
  onCreateSeason: () => void;
  onManageMatches: () => void;
  onImportSeason: () => void;
  onImportRoster: () => void;
}

export const SeasonsPageHeader = ({
  seasonsCount,
  onCreateSeason,
  onManageMatches,
  onImportSeason,
  onImportRoster,
}: SeasonsPageHeaderProps) => {
  const { t } = useTranslation();

  return (
    <div className="football-seasons-header">
      <div className="seasons-count">
        <span>{t('football.seasons.totalCount', `${seasonsCount} seasons`, { count: seasonsCount })}</span>
      </div>
      <div className="seasons-actions">
        <button
          type="button"
          className="manage-matches-button"
          onClick={onManageMatches}
        >
          {t('football.management.actions.seasonMatches', 'Manage Season Matches')}
        </button>
        <button
          type="button"
          className="import-season-button"
          onClick={onImportSeason}
          title={t('football.seasons.import.buttonTooltip', 'Create a season by uploading a JSON file (myleague format).')}
        >
          <i className="fas fa-file-import"></i>
          {' '}
          {t('football.seasons.import.button', 'Import from JSON')}
        </button>
        <button
          type="button"
          className="import-season-button"
          onClick={onImportRoster}
          title={t('common.rosterImport.buttonTooltip', 'Import a team roster from an Excel file into a season.')}
        >
          <i className="fas fa-file-excel"></i>
          {' '}
          {t('common.rosterImport.button', 'Import roster')}
        </button>
        <button
          type="button"
          className="create-season-button"
          onClick={onCreateSeason}
        >
          {t('football.seasons.createNew', 'Create New Season')}
        </button>
      </div>
    </div>
  );
}; 