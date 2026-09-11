import { useTranslation } from 'react-i18next';
import { TournamentsTable } from './TournamentsTable';
import type { FootballTournamentDto } from '../../../../../types/football/tournamentTypes';

interface TournamentsContentProps {
  tournaments: FootballTournamentDto[];
  onEdit: (tournament: FootballTournamentDto) => void;
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
          <p>{t('football.tournaments.noTournaments', 'No tournaments found')}</p>
        </div>
      ) : (
        <TournamentsTable tournaments={tournaments} onEdit={onEdit} />
      )}
    </div>
  );
};
