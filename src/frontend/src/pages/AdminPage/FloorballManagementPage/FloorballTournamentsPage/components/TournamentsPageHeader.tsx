import { useTranslation } from 'react-i18next';
import './TournamentsPageHeader.scss';

interface TournamentsPageHeaderProps {
  tournamentsCount: number;
  onCreateTournament: () => void;
  onManageMatches: () => void;
  onImportTournament: () => void;
}

export const TournamentsPageHeader = ({
  tournamentsCount,
  onCreateTournament,
  onManageMatches,
  onImportTournament,
}: TournamentsPageHeaderProps) => {
  const { t } = useTranslation();

  return (
    <div className="floorball-tournaments-header">
      <div className="tournaments-count">
        <span>
          {t('floorball.tournaments.totalCount', '{{count}} tournaments', { count: tournamentsCount })}
        </span>
      </div>
      <div className="tournaments-actions">
        <button
          type="button"
          className="manage-matches-button"
          onClick={onManageMatches}
        >
          {t('floorball.management.actions.tournamentMatches', 'Manage Tournament Matches')}
        </button>
        <button
          type="button"
          className="import-tournament-button"
          onClick={onImportTournament}
          title={t('floorball.tournaments.import.buttonTooltip', 'Create a tournament by uploading a JSON file (myleague format).')}
        >
          <i className="fas fa-file-import"></i>
          {' '}
          {t('floorball.tournaments.import.button', 'Import from JSON')}
        </button>
        <button
          type="button"
          className="create-tournament-button"
          onClick={onCreateTournament}
        >
          {t('floorball.tournaments.createNew', 'Create New Tournament')}
        </button>
      </div>
    </div>
  );
};
