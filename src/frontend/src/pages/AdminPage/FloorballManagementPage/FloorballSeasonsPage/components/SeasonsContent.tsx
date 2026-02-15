import { useTranslation } from 'react-i18next';
import { SeasonsTable } from './SeasonsTable';
import type { FloorballSeasonDto } from '../../../../../api/floorball/floorballSeasonService';

interface SeasonsContentProps {
  seasons: FloorballSeasonDto[];
  onEdit: (season: FloorballSeasonDto) => void;
  onDelete: (season: FloorballSeasonDto) => void;
  onActivateToggle: (season: FloorballSeasonDto) => void;
  onComplete: (season: FloorballSeasonDto) => void;
  operationLoading?: string | null;
  selectedIds: Set<string>;
  onToggleSelect: (id: string) => void;
  onSelectAll: () => void;
  onClearSelection: () => void;
}

export const SeasonsContent = ({
  seasons,
  onEdit,
  onDelete,
  onActivateToggle,
  onComplete,
  operationLoading,
  selectedIds,
  onToggleSelect,
  onSelectAll,
  onClearSelection,
}: SeasonsContentProps) => {
  const { t } = useTranslation();

  return (
    <div className="seasons-table-container">
      {seasons.length === 0 ? (
        <div className="admin-table__empty">
          <p>{t('floorball.seasons.noSeasons', 'No seasons found')}</p>
        </div>
      ) : (
        <SeasonsTable
          seasons={seasons}
          onEdit={onEdit}
          onDelete={onDelete}
          onActivateToggle={onActivateToggle}
          onComplete={onComplete}
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
