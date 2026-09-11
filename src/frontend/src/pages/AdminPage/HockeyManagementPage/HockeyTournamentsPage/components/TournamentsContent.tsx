import { useTranslation } from 'react-i18next';
import { TournamentsTable } from './TournamentsTable';
import type { HockeyTournamentDto } from '../../../../../types/hockey/hockeyTypes';

interface TournamentsContentProps {
  tournaments: HockeyTournamentDto[];
  onEdit: (tournament: HockeyTournamentDto) => void;
}

export function TournamentsContent({ tournaments, onEdit }: TournamentsContentProps) {
  const { t } = useTranslation();

  return (
    <div className="tournaments-table-container">
      {tournaments.length === 0 ? (
        <div className="admin-table__empty">
          <p>{t('hockey.tournaments.noTournaments', 'No tournaments found')}</p>
        </div>
      ) : (
        <TournamentsTable tournaments={tournaments} onEdit={onEdit} />
      )}
    </div>
  );
}
