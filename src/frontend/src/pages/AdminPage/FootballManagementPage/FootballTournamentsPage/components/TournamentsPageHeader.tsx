import { useTranslation } from 'react-i18next';
import './TournamentsPageHeader.scss';

interface TournamentsPageHeaderProps {
  tournamentsCount: number;
  onCreateTournament: () => void;
  onManageMatches: () => void;
}

export const TournamentsPageHeader = ({
  tournamentsCount,
  onCreateTournament,
  onManageMatches,
}: TournamentsPageHeaderProps) => {
  const { t } = useTranslation();

  return (
    <div className="football-tournaments-header">
      <div className="tournaments-count">
        <span>
          {t('football.tournaments.totalCount', '{{count}} tournaments', { count: tournamentsCount })}
        </span>
      </div>
      <div className="tournaments-actions">
        <button
          type="button"
          className="manage-matches-button"
          onClick={onManageMatches}
        >
          {t('football.management.actions.tournamentMatches', 'Manage Tournament Matches')}
        </button>
        <button
          type="button"
          className="create-tournament-button"
          onClick={onCreateTournament}
        >
          {t('football.tournaments.createNew', 'Create New Tournament')}
        </button>
      </div>
    </div>
  );
};
