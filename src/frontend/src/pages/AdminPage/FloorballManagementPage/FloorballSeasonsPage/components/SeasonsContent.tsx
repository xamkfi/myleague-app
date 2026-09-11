import { useTranslation } from 'react-i18next';
import { SeasonsTable } from './SeasonsTable';
import type { FloorballSeasonDto } from '../../../../../api/floorball/floorballSeasonService';

interface SeasonsContentProps {
  seasons: FloorballSeasonDto[];

  /**
   * Avaa kauden muokkaussivun.
   * Tätä kutsutaan SeasonsTable-komponentista, kun käyttäjä klikkaa kauden riviä.
   */
  onEdit: (season: FloorballSeasonDto) => void;

  /**
   * Avaa poistovahvistuksen valitulle kaudelle.
   */
  onDelete: (season: FloorballSeasonDto) => void;

  /**
   * Aktivoi tai deaktivoi kauden nykyisen tilan mukaan.
   */
  onActivateToggle: (season: FloorballSeasonDto) => void;

  /**
   * Merkitsee kauden valmiiksi.
   */
  onComplete: (season: FloorballSeasonDto) => void;

  /**
   * Sisältää sen kauden id:n, jolla on parhaillaan operaatio käynnissä.
   * Tätä käytetään esimerkiksi painikkeiden disablointiin.
   */
  operationLoading?: string | null;
}

export const SeasonsContent = ({
  seasons,
  onEdit,
  onDelete,
  onActivateToggle,
  onComplete,
  operationLoading,
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
        />
      )}
    </div>
  );
};