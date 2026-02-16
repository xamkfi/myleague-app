import { useTranslation } from 'react-i18next';
import type { FloorballSeasonDto } from '../../../../../../api/floorball/floorballSeasonService';
import { formatSeasonDisplayName } from '../../../ManageMatchPage/utils/matchFormatters';
import SearchField from '../../../../../../components/SearchField/SearchField';
import Button from '../../../../../../components/Button/Button';
import AddIcon from '../../../../../../assets/basicIcons/add.svg';
import './MatchFilters.scss';

interface MatchFiltersProps {
  seasons: FloorballSeasonDto[];
  selectedSeasonId: string;
  onSeasonChange: (seasonId: string) => void;
  searchQuery: string;
  onSearchChange: (query: string) => void;
  onCreateNew?: () => void;
}

function MatchFilters({
  seasons,
  selectedSeasonId,
  onSeasonChange,
  searchQuery,
  onSearchChange,
  onCreateNew,
}: MatchFiltersProps) {
  const { t } = useTranslation();

  return (
    <div className="match-filters">
      <div className="match-filters__inputs">
        <SearchField
          value={searchQuery}
          onChange={onSearchChange}
          placeholder={t('floorball.matches.filters.searchPlaceholder', 'Search for team names...')}
          rounded="pill"
          fullWidth
        />

        <div className="match-filters__select-group">
          <label htmlFor="season-filter">
            {t('floorball.matches.filters.filterBySeason', 'Filter by Season:')}
          </label>
          <select
            id="season-filter"
            value={selectedSeasonId}
            onChange={(e) => onSeasonChange(e.target.value)}
            className="match-filters__select"
          >
            <option value="">{t('floorball.matches.filters.allSeasons', 'All Seasons')}</option>
            {seasons.map(season => (
              <option key={season.id} value={season.id}>
                {formatSeasonDisplayName(season)}
              </option>
            ))}
          </select>
        </div>
      </div>

      <Button
        iconLeft={AddIcon}
        rounded="pill"
        onClick={onCreateNew}
      >
        {t('floorball.matches.createNewMatch', 'Create New Match')}
      </Button>
    </div>
  );
}

export default MatchFilters;
