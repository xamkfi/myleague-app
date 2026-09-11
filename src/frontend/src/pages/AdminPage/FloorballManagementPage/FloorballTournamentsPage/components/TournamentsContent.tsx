import { useTranslation } from 'react-i18next';
import { TournamentsTable } from './TournamentsTable';
import type { FloorballTournamentDto } from '../../../../../types/floorball/tournamentTypes';

interface TournamentsContentProps {
  tournaments: FloorballTournamentDto[];
  onEdit: (tournament: FloorballTournamentDto) => void;
}

export const TournamentsContent = ({
  tournaments,
  onEdit,
}: TournamentsContentProps) => {
  const { t } = useTranslation();

  return (
    <div className="tournaments-table-container">
      {tournaments.length === 0 ? (
        <div className="admin-table__empty">
          <p>{t('floorball.tournaments.noTournaments', 'No tournaments found')}</p>
        </div>
      ) : (
        <TournamentsTable tournaments={tournaments} onEdit={onEdit} />
      )}
    </div>
  );
};
