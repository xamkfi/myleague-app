import React from 'react';
import type { FloorballSeasonDto } from '../../../../../../api/floorball/floorballSeasonService';
import { formatSeasonDisplayName } from '../../utils/matchFormatters';
import './MatchFilters.scss';

interface MatchFiltersProps {
  seasons: FloorballSeasonDto[];
  selectedSeasonId: string;
  onSeasonChange: (seasonId: string) => void;
}

const MatchFilters: React.FC<MatchFiltersProps> = ({
  seasons,
  selectedSeasonId,
  onSeasonChange
}) => {
  return (
    <div className="filter-section">
      <label htmlFor="season-filter">Filter by Season:</label>
      <select
        id="season-filter"
        value={selectedSeasonId}
        onChange={(e) => onSeasonChange(e.target.value)}
        className="season-filter"
      >
        <option value="">All Seasons</option>
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