import { useTranslation } from 'react-i18next';
import { TournamentsTable } from './TournamentsTable';
import type { FloorballTournamentDto } from '../../../../../types/floorball/tournamentTypes';

interface TournamentsContentProps {
  tournaments: FloorballTournamentDto[];
  onEdit: (tournament: FloorballTournamentDto) => void;
  onDelete: (tournament: FloorballTournamentDto) => void;
  onLifecycleAction: (tournament: FloorballTournamentDto, action: 'startGroupStage' | 'startPlayoffStage' | 'complete' | 'cancel') => void;
  onManageMatches: (tournament: FloorballTournamentDto) => void;
  operationLoading?: string | null;
  selectedIds: Set<string>;
  onToggleSelect: (id: string) => void;
  onSelectAll: () => void;
  onClearSelection: () => void;
}

export const TournamentsContent = ({
  tournaments,
  onEdit,
  onDelete,
  onLifecycleAction,
  onManageMatches,
  operationLoading,
  selectedIds,
  onToggleSelect,
  onSelectAll,
  onClearSelection,
}: TournamentsContentProps) => {
  const { t } = useTranslation();

  return (
    <div className="tournaments-table-container">
      {tournaments.length === 0 ? (
        <div className="admin-table__empty">
          <p>{t('floorball.tournaments.noTournaments', 'No tournaments found')}</p>
        </div>
      ) : (
        <TournamentsTable
          tournaments={tournaments}
          onEdit={onEdit}
          onDelete={onDelete}
          onLifecycleAction={onLifecycleAction}
          onManageMatches={onManageMatches}
          operationLoading={operationLoading}
          selectedIds={selectedIds}
          onToggleSelect={onToggleSelect}
          onSelectAll={onSelectAll}
          onClearSelection={onClearSelection}
        />
      )}
    </div>
  );
};
