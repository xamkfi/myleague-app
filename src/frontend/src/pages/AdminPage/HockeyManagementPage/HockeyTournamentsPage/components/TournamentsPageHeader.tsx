import { useTranslation } from 'react-i18next';
import './TournamentsPageHeader.scss';

interface TournamentsPageHeaderProps {
  tournamentsCount: number;
  onCreateTournament: () => void;
  onManageMatches: () => void;
}

export function TournamentsPageHeader({
  tournamentsCount,
  onCreateTournament,
  onManageMatches,
}: TournamentsPageHeaderProps) {
  const { t } = useTranslation();

  return (
    <div className="floorball-tournaments-header">
      <div className="tournaments-count">
        <span>{t('hockey.tournaments.totalCount', '{{count}} tournaments', { count: tournamentsCount })}</span>
      </div>
      <div className="tournaments-actions">
        <button type="button" className="manage-matches-button" onClick={onManageMatches}>
          {t('hockey.management.actions.tournamentMatches', 'Manage Tournament Matches')}
        </button>
        <button type="button" className="create-tournament-button" onClick={onCreateTournament}>
          {t('hockey.tournaments.create', 'Create New Tournament')}
        </button>
      </div>
    </div>
  );
}
