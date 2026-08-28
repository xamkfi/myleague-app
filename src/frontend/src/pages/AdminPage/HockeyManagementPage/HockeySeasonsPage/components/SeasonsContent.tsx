import { useTranslation } from 'react-i18next';
import { SeasonsTable } from './SeasonsTable';
import type { HockeySeasonDto } from '../../../../../types/hockey/hockeyTypes';

interface SeasonsContentProps {
  seasons: HockeySeasonDto[];
  onEdit: (season: HockeySeasonDto) => void;
  onActivateToggle: (season: HockeySeasonDto) => void;
  onComplete: (season: HockeySeasonDto) => void;
  operationLoading?: string | null;
}

export function SeasonsContent({
  seasons,
  onEdit,
  onActivateToggle,
  onComplete,
  operationLoading,
}: SeasonsContentProps) {
  const { t } = useTranslation();

  return (
    <div className="seasons-table-container">
      {seasons.length === 0 ? (
        <div className="admin-table__empty">
          <p>{t('hockey.seasons.noSeasons', 'No seasons found')}</p>
        </div>
      ) : (
        <SeasonsTable
          seasons={seasons}
          onEdit={onEdit}
          onActivateToggle={onActivateToggle}
          onComplete={onComplete}
          operationLoading={operationLoading}
        />
      )}
    </div>
  );
}
