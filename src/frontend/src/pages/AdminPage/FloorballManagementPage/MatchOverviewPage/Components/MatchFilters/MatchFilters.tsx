import { useTranslation } from 'react-i18next';
import type { FloorballSeasonDto } from '../../../../../../api/floorball/floorballSeasonService';
import { formatSeasonDisplayName } from '../../../ManageMatchPage/utils/matchFormatters';
import './MatchFilters.scss';

interface MatchFiltersProps {
  seasons: FloorballSeasonDto[];
  selectedSeasonId: string;
  onSeasonChange: (seasonId: string) => void;
}

const MatchFilters = ({
  seasons,
  selectedSeasonId,
  onSeasonChange
}: MatchFiltersProps) => {
  const { t } = useTranslation();
  return (
    <div className="filter-section">
      <label htmlFor="season-filter">{t('floorball.matches.filters.filterBySeason', 'Filter by Season:')}</label>
      <select
        id="season-filter"
        value={selectedSeasonId}
        onChange={(e) => onSeasonChange(e.target.value)}
        className="season-filter"
      >
        <option value="">{t('floorball.matches.filters.allSeasons', 'All Seasons')}</option>
        {seasons.map(season => (
          <option key={season.id} value={season.id}>
            {formatSeasonDisplayName(season)}
          </option>
        ))}
      </select>
    </div>
  );
};

export default MatchFilters; 